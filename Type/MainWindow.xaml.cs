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
    #region Delegates
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

    /// <summary>
    /// Forces the UI to redraw its contents based on the current input box state.
    /// </summary>
    public delegate void ForceRedrawCallback();
    #endregion

    public partial class MainWindow : Window
    {
        #region Constants
        private const string TEXT_WELCOME = "start typing...";
        private const string TEXT_NOTASKS = "no tasks.";
        private const int NUMBER_OF_TASKS_LOADED = 50;
        private const int NUMBER_OF_TASKS_DISPLAYED = 5;
        #endregion

        #region Fields
        private List<String> helpDescription = new List<String>();

        private ExecuteCommandCallback ExecuteCommand;
        private FilterSuggestionsCallback GetFilterSuggestions;
        private GetTasksCallback GetTasks;
        private GetTasksByHashTagCallback GetTasksByHashTag;

        private IList<Task> renderedTasks;
        private List<TextBlock> taskTextBlockList;

        private bool isHighlighting;
        private int highlightListIndex;
        private int listStartIndex;
        private int listEndIndex;
        private bool isOriginalTasks;

        private Ellipse[] pageButtonArray;
        private int highlightPageIndex = 1;

        private Task selectedTask;
        private Command parseResult;
        private StackPanel taskView = new StackPanel();
        #endregion

        public MainWindow(FilterSuggestionsCallback GetFilterSuggestions, ExecuteCommandCallback ExecuteCommand, GetTasksCallback GetTasks, GetTasksByHashTagCallback GetTasksByHashTag)
        {
            InitializeComponent();

            this.GetFilterSuggestions = GetFilterSuggestions;
            this.ExecuteCommand = ExecuteCommand;
            this.GetTasks = GetTasks;
            this.GetTasksByHashTag = GetTasksByHashTag;

            // Focus cursor in input box
            inputBox.Focus();

            // Display input label
            DisplayInputLabel();

            // Populate help lists
            PopulateHelpList();

            // Bootstrap tasks
            isOriginalTasks = true;
            renderedTasks = GetTasks(NUMBER_OF_TASKS_LOADED);
            taskTextBlockList = new List<TextBlock>();

            pageButtonArray = new Ellipse[NUMBER_OF_TASKS_LOADED + 1];

            parseResult = Command.Parse(inputBox.Text);

            InitializeListBounderIndex();
            StopHighlighting();

            RenderTasks();
        }

        //@author A0083834Y
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

        // Render List of Tasks.
        private void RefreshViewList()
        {
            taskView.Children.Clear();
            tasksGrid.Children.Clear();

            if (renderedTasks.Count == 0)
            {
                DisplayEmptyViewList();
            }
            else
            {
                DisplayNonEmptyViewList();
            }

            // Append task view to grid view
            tasksGrid.Children.Add(taskView);

            // Display page buttons
            int pages = GetPageNumber();
            DisplayPageButton(tasksGrid, pages);

            DisplayDashedBorder(tasksGrid);    
        }

        //@author A0083834Y
        // Get number of pages in current task list
        private int GetPageNumber()
        {
            int pages;

            if (renderedTasks.Count < NUMBER_OF_TASKS_DISPLAYED)
            {
                pages = 1;
            }
            else if (renderedTasks.Count % NUMBER_OF_TASKS_DISPLAYED != 0)
            {
                pages = renderedTasks.Count / NUMBER_OF_TASKS_DISPLAYED + 1;
            }
            else
            {
                pages = renderedTasks.Count / NUMBER_OF_TASKS_DISPLAYED;
            }

            return pages;

        }

        // If task list is not empty
        private void DisplayNonEmptyViewList()
        {
            TextBlock text = new TextBlock();

            // Display blue border above text
            DisplayBlueBorder(taskView);

            // Loop over each task and create task view
            // Append each to tasks grid
            for (int i = listStartIndex; i < listEndIndex; i++)
            {
                text = taskTextBlockList[i];

                // Highlight target textbox                    
                if ((i == highlightListIndex + listStartIndex) && isHighlighting)
                {
                    text.Background = new SolidColorBrush(Color.FromArgb(255, 230, 243, 244));

                    selectedTask = renderedTasks[i];

                    text.TextWrapping = TextWrapping.Wrap;
                }
                else
                {
                    text.Background = Brushes.White;

                    text.TextWrapping = TextWrapping.NoWrap;
                }

                taskView.Children.Add(text);

                // Display blue border below text
                DisplayBlueBorder(taskView);
            }
        }

        // If task list is empty
        private void DisplayEmptyViewList()
        {
            TextBlock text = new TextBlock();

            DisplayBlueBorder(tasksGrid);

            text.Text = TEXT_NOTASKS;

            StyleNoTasks(text);

            taskView.Children.Add(text);
            DisplayBlueBorder(taskView);
        }

        // Generate list of text block
        private void RenderTasks()
        {
            StopHighlighting();

            taskTextBlockList.Clear();
            InitializeListBounderIndex();

            RenderTasksDecorations();

            RefreshViewList();
        }

        private void RenderTasksDecorations()
        {
            for (int i = 0; i < renderedTasks.Count; i++)
            {
                TextBlock text = new TextBlock();

                // Style tokens within the textblock
                foreach (Tuple<string, Task.ParsedType> tuple in renderedTasks[i].Tokens)
                {
                    Run run = new Run(tuple.Item1);
                    // Style Runs
                    if (renderedTasks[i].Done)
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

                StyleTasks(text);
                taskTextBlockList.Add(text);
            }
        }

        private void InitializeListBounderIndex()
        {
            highlightListIndex = 0;
            listStartIndex = 0;

            if (renderedTasks.Count > NUMBER_OF_TASKS_DISPLAYED)
            {
                listEndIndex = NUMBER_OF_TASKS_DISPLAYED;
            }

            else
            {
                listEndIndex = renderedTasks.Count;
            }            
        }

        // Event Listener when Input Box text changes.
        private void inputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DisplayInputLabel();

            parseResult = Command.Parse(inputBox.Text);

            if (inputBox.Text == string.Empty)
            {
                isOriginalTasks = true;
                renderedTasks = GetTasks(NUMBER_OF_TASKS_LOADED);
                RenderTasks();
            }

            else
            {
                if (parseResult.CommandText == Command.Search && parseResult.Text != string.Empty)
                {
                    isOriginalTasks = false;
                    renderedTasks = GetTasksByHashTag(parseResult.Text);
                    RenderTasks();
                }

                else if (parseResult.CommandText != Command.Add && parseResult.Text!=string.Empty)
                {
                    isOriginalTasks = false;
                    renderedTasks = GetFilterSuggestions(parseResult.Text);
                    RenderTasks();
                }

                else if (!isOriginalTasks && parseResult.Text == string.Empty)
                {
                    isOriginalTasks = true;
                    renderedTasks = GetTasks(NUMBER_OF_TASKS_LOADED);
                    RenderTasks();
                }
            }
        
            invalidCmdPopup.IsOpen = false;
            helpDescriptionPopup.IsOpen = false;
        }

        // Event Listener, onKeyUp Input Box
        private void inputBox_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    HandleSendCommand();
                    isOriginalTasks = true;
                    break;

                case Key.Tab:
                    HandleAutoComplete();
                    break;

                case Key.Escape:
                    HandleEscapeKey();
                    break;

                case Key.Up:
                    HandleUpArrow();
                    break;

                case Key.Down:
                    HandleDownArrow();
                    break;

                case Key.Left:
                    HandleLeftArrow();
                    break;

                case Key.Right:
                    HandleRightArrow();
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

        //@author A0092104U
        /// <summary>
        /// Forces the UI to update its task list and redraw.
        /// </summary>
        public void ForceRedraw()
        {
            if (inputBox.Dispatcher.CheckAccess())
            {
                // Force a UI Redraw by changing the text of the input box.
                var originalState = inputBox.Text;
                inputBox.Clear();
                inputBox.Text = originalState;
            }
            else
            {
                inputBox.Dispatcher.Invoke(new ForceRedrawCallback(ForceRedraw));
            }
        }
    }
}
