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
    /// <param name="rawText">Text to parse.</param>
    /// <param name="selected">Selected task. Throws an exception if no reference is specified, but the command requires one.</param>
    public delegate void ExecuteCommandCallback(string rawText, Task selectedTask = null);

    /// <summary>
    /// Retrieves a list of tasks to be displayed.
    /// </summary>
    /// <param name="num">Number of tasks to retrieve.</param>
    /// <returns>Read-only list of tasks.</returns>
    public delegate IList<Task> GetTasksCallback(int num);

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

        private IList<Task> renderedTasks;
        private bool editState;
        private int editIndex;

        public MainWindow(FilterSuggestionsCallback GetFilterSuggestions, ExecuteCommandCallback ExecuteCommand, GetTasksCallback GetTasks)
        {
            InitializeComponent();

            this.GetFilterSuggestions = GetFilterSuggestions;
            this.ExecuteCommand = ExecuteCommand;
            this.GetTasks = GetTasks;

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
                inputBoxLabel.Content = INPUT_WELCOME_TEXT;
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
                text.Text = INPUT_NOTASKS_TEXT;

                text.TextAlignment = TextAlignment.Center;
                text.FontSize = 20;
                text.FontFamily = new FontFamily("GillSans");
                text.Padding = new Thickness(10);

                noTasksText.Children.Add(text);

                // Append to tasksgrid.
                tasksGrid.Children.Add(noTasksText);

                //display horizontal blue line below "no tasks" text
                StackPanel horizontalLine = new StackPanel();
                Line blueLine = DrawBlueLine();
                horizontalLine.Children.Add(blueLine);
                noTasksText.Children.Add(horizontalLine);

                // display horizontal blue line below input box
                StackPanel topHorizontalLine = new StackPanel();
                Line topBlueLine = DrawBlueLine();
                topHorizontalLine.Margin = new Thickness(12, 47, 0, 0);
                topHorizontalLine.Children.Add(topBlueLine);
                mainGrid.Children.Add(topHorizontalLine);
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
                    text.Text = task.RawText;

                    // style accordingly
                    if (task.Done)
                    {
                        text.TextDecorations = TextDecorations.Strikethrough;
                        text.FontStyle = FontStyles.Italic;
                        text.Foreground = Brushes.SlateGray;
                    }

                    text.FontSize = 20;
                    text.FontFamily = new FontFamily("GillSans");
                    text.Padding = new Thickness(10);
                    text.Margin = new Thickness(15, 0, 0, 0);

                    taskView.Children.Add(text);

                    // append task view to grid view
                    tasksGrid.Children.Add(taskView);

                    // display horizontal blue line after each new line
                    StackPanel horizontalLine = new StackPanel();
                    Line blueLine = DrawBlueLine();
                    horizontalLine.Children.Add(blueLine);
                    taskView.Children.Add(horizontalLine);
                }
            }

            // display bottom border
            StackPanel bottomBorder = new StackPanel();
            Rectangle dashedLine = DrawDashedLine();
            bottomBorder.Children.Add(dashedLine);
            tasksGrid.Children.Add(bottomBorder);

            StackPanel verticalLine = new StackPanel();
            verticalLine.Orientation = Orientation.Vertical;
            verticalLine.Margin = new Thickness(25, 12, 0, 0);
            Line redLine = DrawRedLine();
            verticalLine.Children.Add(redLine);
            mainGrid.Children.Add(verticalLine);
        }

        // Event Listener when Input Box text changes.
        private void InputBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            DisplayInputLabel();

            // TODO.

            // display filtered tasks
            IList<Task> filtered = GetFilterSuggestions(inputBox.Text);
            if (filtered != null)
            {
                renderedTasks = filtered;
                RenderTasks();
            }
        }

        private Line DrawBlueLine()
        {
            Line blueLine = new Line();

            blueLine.Stroke = Brushes.SkyBlue;
            blueLine.StrokeThickness = 0.5;
            blueLine.X1 = 0;
            blueLine.Y1 = 0;
            blueLine.X2 = 479;
            blueLine.Y2 = 0;

            return blueLine;
        }

        private Line DrawRedLine()
        {
            Line redLine = new Line();

            redLine.Stroke = Brushes.Salmon;
            redLine.StrokeThickness = 0.5;
            redLine.X1 = 0;
            redLine.Y1 = 0;
            redLine.X2 = 0;
            redLine.Y2 = 244;

            return redLine;
        }

        private Rectangle DrawDashedLine()
        {
            Rectangle dashedLine = new Rectangle();

            dashedLine.Stroke = Brushes.SandyBrown;
            dashedLine.StrokeThickness = 1;
            dashedLine.StrokeDashArray = new DoubleCollection() { 4, 3 };
            dashedLine.Margin = new Thickness(0, 20, 0, 0);

            return dashedLine;
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
                    // get input text
                    string inputText = inputBox.Text;

                    if (inputText.StartsWith(COMMAND_PREFIX))
                    {
                        if (renderedTasks.Count != 0)
                        {
                            Task selectedTask = renderedTasks[0];
                            ExecuteCommand(inputBox.Text, selectedTask);
                        }
                    }
                    else
                    {
                        // add command
                        ExecuteCommand(inputBox.Text, null);
                    }
                    // render tasks
                    renderedTasks = GetTasks(8);
                    RenderTasks();

                    // clear input box
                    inputBox.Clear();

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
