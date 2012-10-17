using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Type
{
    /// <summary>
    /// Retrieves a list of suggestions that begin with a specified prefix.
    /// </summary>
    /// <param name="partialText">Prefix to match.</param>
    /// <returns>Read-only list of suggestions as strings.</returns>
    public delegate IList<Task> FilterSuggestionsCallback(string partialText);

    /// <summary>
    /// Parses a raw string and executes its command, if valid.
    /// If no valid command is found, this method does nothing.
    /// </summary>
    /// <param name="cmd">Command.</param>
    /// <param name="content">Text of the Command.</param>
    /// <param name="selected">Selected task. Throws an exception if no reference is specified, but the command requires one.</param>
    public delegate void ExecuteCommandCallback(string cmd, string content, Task selectedTask = null);

    /// <summary>
    /// Retrieves a list of tasks to be displayed.
    /// </summary>
    /// <param name="num">Number of tasks to retrieve.</param>
    /// <returns>Read-only list of tasks.</returns>
    public delegate IList<Task> GetTasksCallback(int num);

    /// <summary>
    /// Retrieves a list of tasks tagged with at least one hash tag.
    /// </summary>
    /// <param name="content">String containing hash tags separated by ' '.</param>
    /// <returns>Read-only list of tasks.</returns>
    public delegate IList<Task> GetTasksByHashTagCallback(string content);

    public partial class MainWindow : Window
    {
        private const string TEXT_WELCOME = "start typing...";
        private const string TEXT_NOTASKS = "no tasks.";

        private ExecuteCommandCallback ExecuteCommand;
        private FilterSuggestionsCallback GetFilterSuggestions;
        private GetTasksCallback GetTasks;
        private GetTasksByHashTagCallback GetTasksByHashTag;

        private IList<Task> renderedTasks;

        public MainWindow(FilterSuggestionsCallback GetFilterSuggestions, ExecuteCommandCallback ExecuteCommand, GetTasksCallback GetTasks, GetTasksByHashTagCallback GetTasksByHashTag)
        {
            InitializeComponent();

            this.GetFilterSuggestions = GetFilterSuggestions;
            this.ExecuteCommand = ExecuteCommand;
            this.GetTasks = GetTasks;
            this.GetTasksByHashTag = GetTasksByHashTag;

            // focus cursor in input box
            inputBox.Focus();

            // display input label
            DisplayInputLabel();

            // bootstrap tasks
            // TODO. abstract this number.
            renderedTasks = GetTasks(8);
            RenderTasks();
        }

        // Input Label
        private void DisplayInputLabel()
        {
            if (inputBox.Text.Trim() == "")
            {
                inputBoxLabel.Content = TEXT_WELCOME;
            }
            else
            {
                inputBoxLabel.Content = "";
            }
        }

        // Render List of Tasks
        private void RenderTasks()
        {
            tasksGrid.Children.Clear();
            if (renderedTasks.Count == 0)
            {
                // display no tasks text.
                StackPanel noTasksText = new StackPanel();

                TextBlock text = new TextBlock();
                text.Text = TEXT_NOTASKS;

                StyleNoTasks(text);

                noTasksText.Children.Add(text);

                // Append to tasksgrid.
                tasksGrid.Children.Add(noTasksText);

                DisplayBlueBorder(noTasksText);
            }
            else
            {
                // loop over each task and create task view
                // append each to tasks grid
                foreach (Task task in renderedTasks)
                {
                    // create single stacked panel w/ info
                    StackPanel taskView = new StackPanel();
                    TextBlock text = new TextBlock();

                    // style tokens within the textblock
                    for (int i = 0; i< task.Tokens.Count; i++)
                    {
                        Tuple<string, Task.ParsedType> tuple = task.Tokens[i];

                        Run run = new Run(tuple.Item1);
                        // Style Runs
                        if (tuple.Item2 == Task.ParsedType.HashTag)
                        {
                            // blue
                            run.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x18, 0x23, 0x7f));
                            run.FontWeight =  FontWeights.DemiBold;
                        }

                        text.Inlines.Add(run);
                    }

                    // style accordingly
                    if (task.Done)
                    {
                        StyleDone(text);
                    }

                    StyleTasks(text);

                    taskView.Children.Add(text);

                    // append task view to grid view
                    tasksGrid.Children.Add(taskView);

                    DisplayBlueBorder(taskView);
                }
            }
            
            DisplayDashedBorder(tasksGrid);
        }

        // Event Listener when Input Box text changes.
        private void InputBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            DisplayInputLabel();

            if (inputBox.Text == string.Empty)
            {
                renderedTasks = GetTasks(8);
            }
            else
            {
                var result = Command.Parse(inputBox.Text);
                if (result.CommandText == Command.Search)
                {
                    renderedTasks = GetTasksByHashTag(result.Text);
                }
                else if (result.CommandText != Command.Add)
                {
                    renderedTasks = GetFilterSuggestions(result.Text);
                }
            }

            RenderTasks();
        }

        // Used for auto complete.
        private void MoveCursorToEndOfWord()
        {
            inputBox.Select(inputBox.Text.Length, 0);
        }

        // Event Listener, onKeyUp Input Box
        private void InputBoxKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    HandleSendCommand();
                    break;

                case Key.Tab:
                    HandleAutoComplete();
                    break;

                case Key.Escape:
                    HandleHideWindow();
                    break;
            }
        }

        private void HandleSendCommand()
        {
            //Parse input.
            var result = Command.Parse(inputBox.Text);

            if (result.CommandText == Command.Invalid)
            {
                //Alert the user somehow that the command was invalid.
                //TODO
            }
            else if (result.CommandText == Command.Search)
            {
                DoSearch(result);
            }
            else if (result.CommandText != Command.Add)
            {
                DoOther(result);
            }
            else
            {
                DoAdd(result);
            }

            //Retrieve a list of tasks, unless the list has already been retrieved by Search.
            if (result.CommandText != Command.Search)
            {
                renderedTasks = GetTasks(8);
            }

            RenderTasks();
        }

        private void DoAdd(Command result)
        {
            //The default command is "add".
            if (result.Text.Trim() != string.Empty)
            {
                ExecuteCommand(result.CommandText, result.Text);
                inputBox.Clear();
            }
        }

        private void DoOther(Command result)
        {
            if (renderedTasks.Count != 0)
            {
                Task selectedTask = renderedTasks[0];
                ExecuteCommand(result.CommandText, result.Text, selectedTask);

                if (result.CommandText == Command.Edit)
                {
                    DoEdit(selectedTask);
                }
                else
                {
                    inputBox.Clear();
                }
            }
        }

        private void DoEdit(Task selectedTask)
        {
            //Populate inputBox with edit text.
            inputBox.Text = selectedTask.RawText;
            MoveCursorToEndOfWord();
        }

        private void DoSearch(Command result)
        {
            if (result.Text.Trim() != string.Empty)
            {
                renderedTasks = GetTasksByHashTag(result.Text.Trim());
            }
        }

        private void HandleAutoComplete()
        {
            //AutoComplete is only defined if there are rendered tasks on screen.
            if (renderedTasks != null && renderedTasks.Count > 0)
            {
                var result = Command.Parse(inputBox.Text);
                if (result.CommandText != Command.Invalid)
                {
                    int completeBegin = LCPIndex(result.Text, renderedTasks[0].RawText);
                    
                    if (inputBox.Text.EndsWith(result.CommandText))
                    {
                        inputBox.Text += " ";
                    }

                    if (completeBegin >= 0)
                    {
                        inputBox.Text += renderedTasks[0].RawText.Substring(completeBegin + 1);
                        MoveCursorToEndOfWord();
                    }
                }
            }
        }

        private void HandleHideWindow()
        {
            //If the input box is not empty, we clear the input box, but do not hide the window.
            //Otherwise, we hide the window.
            //If the input box contains only whitespace (which will not be caught by the first condition),
            //we clear it before hiding the window.
            if (inputBox.Text.Trim() != string.Empty)
            {
                inputBox.Clear();
            }
            else
            {
                if (inputBox.Text.Trim() == string.Empty)
                {
                    inputBox.Clear();
                }
                this.Hide();
            }
        }

        //Finds the longest common prefix of a and b.
        private int LCPIndex(string a, string b)
        {
            int found = -1;
            for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
            {
                if (a[i] == b[i])
                {
                    found = i;
                }
                else
                {
                    break;
                }
            }

            return found;
        }
    }
}
