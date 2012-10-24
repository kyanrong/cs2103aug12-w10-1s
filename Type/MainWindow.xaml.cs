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

        private List<String> helpDescription = new List<String>();

        private ExecuteCommandCallback ExecuteCommand;
        private FilterSuggestionsCallback GetFilterSuggestions;
        private GetTasksCallback GetTasks;
        private GetTasksByHashTagCallback GetTasksByHashTag;

        private IList<Task> renderedTasks;
        private int highlightIndex, currentPositionIndex, listStartIndex, listEndIndex;
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

            // populate help lists
            PopulateHelpList();

            // bootstrap tasks
            // TODO. abstract this number.
            renderedTasks = GetTasks(30);
            if (renderedTasks.Count > 6)
            {
                listEndIndex = 6;
            }
            else
            {
                listEndIndex = renderedTasks.Count;
            }
            listStartIndex = 0;
            highlightIndex = 0;
            currentPositionIndex = 0;
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
                for (int j=0 ; j<renderedTasks.Count ; j++)
                {
                    // create single stacked panel w/ info
                    StackPanel taskView = new StackPanel();
                    TextBlock text = new TextBlock();

                    // style tokens within the textblock
                    for (int i = 0; i< renderedTasks[j].Tokens.Count; i++)
                    {
                        Tuple<string, Task.ParsedType> tuple = renderedTasks[j].Tokens[i];

                        Run run = new Run(tuple.Item1);
                        // Style Runs
                        if (renderedTasks[j].Done)
                        {
                            StyleDoneParsedTypes(run);
                        }

                        else
                        {
                            if (tuple.Item2 == Task.ParsedType.HashTag)
                            {
                                 StyleHashTags(run);
                            }

                            // Style Dates
                            if (tuple.Item2 == Task.ParsedType.DateTime)
                            {
                                StyleDateTime(run);
                            }

                            // Style PriorityHigh
                            if (tuple.Item2 == Task.ParsedType.PriorityHigh)
                            {
                                StylePriorityHigh(run);
                            }

                            // Style PriorityLow
                            if (tuple.Item2 == Task.ParsedType.PriorityLow)
                            {
                                StylePriorityLow(run);
                            }
                        }

                        text.Inlines.Add(run);
                    }

                    // style accordingly
                    if (renderedTasks[j].Done)
                    {
                        StyleDone(text);
                    }

                    StyleTasks(text);

                    if (j == highlightIndex + listStartIndex)
                    {
                        text.Background = Brushes.Beige;
                    }
                    if (j <= listEndIndex && j >= listStartIndex)
                    {
                        taskView.Children.Add(text);
                    }
                    
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
            invalidCmdPopup.IsOpen = false;
            helpDescriptionPopup.IsOpen = false;
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
                    case Key.Up:
                    highlightIndex--;
                    if (highlightIndex < 0 && listStartIndex > 0)
                    {
                        highlightIndex = 3;
                        listStartIndex -= 4;
                        if (listStartIndex < 0)
                        {
                            highlightIndex = 3 + listStartIndex;
                            listStartIndex = 0;
                        }
                        listEndIndex = listStartIndex + 6;
                    }
                    if (highlightIndex < 0)
                    {
                        highlightIndex = 0;
                    }
                    
                    RenderTasks();
                    break;
                    
                case Key.Down:
                    highlightIndex++;

                    if (highlightIndex > 5 && listEndIndex != renderedTasks.Count)
                    {
                        listStartIndex += 4;
                        listEndIndex = listStartIndex + 6;
                        highlightIndex = 2;
                        if (listEndIndex >= renderedTasks.Count)
                        {
                            highlightIndex = 6- (listEndIndex - renderedTasks.Count);
                            listEndIndex = renderedTasks.Count;
                            listStartIndex = listEndIndex - 6;
                        }
                    }
                    if (highlightIndex > 5)
                    {
                        highlightIndex = 5;
                    }
                    
                    RenderTasks();
                    break;
            
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void PopulateHelpList()
        {
            // Populate helpDescription List
            helpDescription.Add("Create new task");
            helpDescription.Add("<task>");

            helpDescription.Add("Complete a task");
            helpDescription.Add(":done <task>");

            helpDescription.Add("Complete all tasks with a tag");
            helpDescription.Add(":done #<tag name>");

            helpDescription.Add("Archive all completed tasks");
            helpDescription.Add(":archive");

            helpDescription.Add("Archive a single task");
            helpDescription.Add(":archive <task>");

            helpDescription.Add("Archive all tasks with a tag");
            helpDescription.Add(":archive #<tag name>");

            helpDescription.Add("Edit a task");
            helpDescription.Add(":edit <task>");

            helpDescription.Add("Undo last action");
            helpDescription.Add(":undo");

            helpDescription.Add("Filter by hash tags");
            helpDescription.Add("/<tag name> [<tag name>] ...");

            helpDescription.Add("Show archived tasks");
            helpDescription.Add("/archive <tag name> [<tag name>] ...");

            helpDescription.Add("Sort the display");
            helpDescription.Add(":sort <field>");
        }
    }
}
