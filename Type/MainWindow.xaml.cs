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
                        text.TextDecorations = TextDecorations.Strikethrough;
                        text.FontStyle = FontStyles.Italic;
                        text.Foreground = Brushes.SlateGray;
                    }

                    text.FontSize = 20;
                    text.FontFamily = new FontFamily("GillSans");
                    text.Padding = new Thickness(10);
                    text.Margin = new Thickness(15, 0, 0, 0);

                    taskView.Children.Add(text);

                    // display horizontal blue line below input box
                    StackPanel topHorizontalLine = new StackPanel();
                    Line topBlueLine = DrawBlueLine();
                    topHorizontalLine.Margin = new Thickness(12, 47, 0, 0);
                    topHorizontalLine.Children.Add(topBlueLine);
                    mainGrid.Children.Add(topHorizontalLine);

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

            // display vertical redline
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
            var parseResult = Parse(inputBox.Text);
            string cmd = parseResult.Item1;
            string content = parseResult.Item2;
            if (cmd != Commands.Add && content != string.Empty)
            {
                IList<Task> filtered = GetFilterSuggestions(content);
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
            redLine.Y2 = this.Height;
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

        // Event Listener, onKeyUp Input Box
        private void InputBoxKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    // parse input
                    var parseResult = Parse(inputBox.Text);
                    string cmd = parseResult.Item1;
                    string content = parseResult.Item2;

                    // execute command
                    if (cmd == Commands.Invalid)
                    {
                        // show an alert message?
                    }
                    else if (cmd != Commands.Add)
                    {
                        if (renderedTasks.Count != 0)
                        {
                            Task selectedTask = renderedTasks[0];
                            ExecuteCommand(cmd, content, selectedTask);

                            if (cmd == Commands.Edit)
                            {
                                // populate input box with edit text
                                inputBox.Text = selectedTask.RawText;
                            }
                            else
                            {
                                // clear input box
                                inputBox.Clear();
                            }

                        }
                    }
                    else
                    {
                        if (content.Trim() != string.Empty)
                        {
                            // add command
                            ExecuteCommand(cmd, content);
                            // clear input box
                            inputBox.Clear();
                        }
                    }



                    // render tasks
                    renderedTasks = GetTasks(8);
                    RenderTasks();
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

        /// <summary>
        /// Parses input by splitting it into a token containing the command's text, and a token containing the rest of the input.
        /// </summary>
        /// <param name="input">Input to parse. Commands should start with the symbol defined in COMMAND_TOKEN.</param>
        /// <returns>A Tuple containing the command text and remaining input.</returns>
        private Tuple<string, string> Parse(string input)
        {
            string cmd;
            if (input.StartsWith(Commands.Token))
            {
                int spIndex = input.IndexOf(' ');
                if (spIndex < 0)
                {
                    cmd = Commands.Invalid;
                    input = "";
                }
                else
                {
                    cmd = input.Substring(1, spIndex - 1);
                    input = input.Substring(spIndex + 1);
                }
            }
            else
            {
                cmd = Commands.Add;
            }

            return new Tuple<string, string>(cmd, input);
        }

        private void tasksGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void mainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }
}
