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
    internal delegate IList<Task> FilterSuggestionsCallback(string partialText);
    internal delegate void ExecuteCommandCallback(string rawText, Task selectedTask);
    internal delegate IList<Task> GetTasksCallback(int num);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string COMMAND_PREFIX = ":";

        private const string INPUT_WELCOME_TEXT = "start typing...";
        private const string INPUT_NOTASKS_TEXT = "no tasks.";

        private ExecuteCommandCallback ExecuteCommand;
        private FilterSuggestionsCallback GetFilterSuggestions;
        private GetTasksCallback GetTasks;

        public MainWindow()
        {
            InitializeComponent();

            // focus cursor in input box
            inputBox.Focus();

            // display input label
            DisplayInputLabel();

            // bootstrap tasks
            // TODO. abstract this number.
            IList<Task> tasks = GetTasks(8);
            RenderTasks(tasks);
        }

        internal MainWindow setCallbacks(FilterSuggestionsCallback GetFilterSuggestions, ExecuteCommandCallback ExecuteCommand, GetTasksCallback GetTasks)
        {
            this.GetFilterSuggestions = GetFilterSuggestions;
            this.ExecuteCommand = ExecuteCommand;
            this.GetTasks = GetTasks;
            return this;
        }

        // Input Label
        private void DisplayInputLabel()
        {
            if (inputBox.Text.Trim() == "")
            {
                inputBoxLabel.Content = INPUT_WELCOME_TEXT;
            }
            else
            {
                inputBoxLabel.Content = "";
            }
        }

        // Render List of Tasks
        private void RenderTasks(IList<Task> Tasks)
        {
            tasksGrid.Children.Clear();
            if (Tasks.Count == 0)
            {
                // display no tasks text.
                StackPanel noTasksText = new StackPanel();

                TextBlock text = new TextBlock();
                text.Text = INPUT_NOTASKS_TEXT;
                noTasksText.Children.Add(text);

                // Append to tasksgrid.
                tasksGrid.Children.Add(noTasksText);
            }
            else
            {
                // loop over each task and create task view
                // append each to tasks grid
                foreach (Task task in Tasks)
                {
                    //TODO
                    // create single stacked panel w/ info
                    StackPanel taskView = new StackPanel();
                    TextBlock text = new TextBlock();
                    text.Text = task.RawText;
                    taskView.Children.Add(text);

                    // append task view to grid view
                    tasksGrid.Children.Add(taskView);
                }
            }
        }

        // Event Listener when Input Box text changes.
        private void InputBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            DisplayInputLabel();
            
            // TODO.
        }

        // Used for auto complete.
        private void MoveCursorToEndOfWord()
        {
            inputBox.Select(inputBox.Text.Length, 0);
        }


        // Checks if a command is typed. 
        //private bool isCommand(string input)
        //{
        //    if (input.StartsWith(COMMAND_PREFIX))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        // Gets the index of the first whitespace. 
        //private int getSpIndex(string input)
        //{
        //    return input.IndexOf(" ");
        //}

        //private string getMessage(int spIndex, string input)
        //{
        //    return input.Substring(spIndex + 1);
        //}

        //private void DecideWhatToDo(UIRedrawMsgCode msgCode, string msg)
        //{
        //    if (msgCode == UIRedrawMsgCode.EDITED_TEXT)
        //    {
        //        textBox1.Text = msg;
        //        MoveCursorToEndOfWord();
        //    }
        //    else if(msgCode == UIRedrawMsgCode.WARNING)
        //    {
        //        popUp.IsOpen = true;
        //        textBlock1.Text = msg;
        //    }
        //    else if (msgCode == UIRedrawMsgCode.ERROR)
        //    {
        //        popUp.IsOpen = true;
        //        textBlock1.Text = msg;
        //    }
        //    else
        //    {
        //        textBox1.Clear();
        //    }
        //}

        // Event Listener, onKeyUp Input Box
        private void InputBoxKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    ExecuteCommand(inputBox.Text, null);
                    var tasks = GetTasks(8);

                    // render tasks
                    RenderTasks(tasks);

                    break;

                case Key.Tab:
                    // TODO
                    // autocomplete
                    
                    break;

                case Key.Escape:
                    // Hide window
                    this.Hide();
                    break;
            }
        }

        //private Tuple<string, string> TokenizeInput(string userInput)
        //{
        //    string command;

        //    if (!userInput.StartsWith(COMMAND_PREFIX))
        //    {
        //        command = "add";
        //    }
        //    else
        //    {
        //        var spIndex = userInput.IndexOf(' ');
        //        command = userInput.Substring(1, spIndex - 1);
        //        userInput = userInput.Substring(spIndex + 1);
        //    }

        //    return new Tuple<string, string>(command, userInput);
        //}
    }
}
