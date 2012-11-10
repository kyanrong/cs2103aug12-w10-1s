using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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

        #region Constructors
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
        #endregion

        #region WPF Events
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
        #endregion

        #region Command Handlers
        private void HandleSendCommand()
        {
            switch (parseResult.CommandText)
            {
                case Command.Invalid:
                    invalidCmdPopup.IsOpen = true;
                    break;

                case Command.Search:
                    DoSearch(parseResult);
                    break;

                case Command.Add:
                    DoAdd(parseResult);
                    break;

                case Command.Help:
                    DoHelp();
                    break;

                case Command.Archive:
                case Command.Done:
                case Command.Undo:
                case Command.Edit:
                case Command.Clear:
                    DoGenericCommand(parseResult);
                    break;

                default:
                    break;
            }

            // Retrieve a list of tasks, unless the list has already been retrieved by Search.
            if (parseResult.CommandText != Command.Search)
            {
                isOriginalTasks = true;
                renderedTasks = GetTasks(NUMBER_OF_TASKS_LOADED);
            }

            RenderTasks();
        }

        //@author A0092104U
        private void DoEdit(Task selectedTask)
        {
            //Populate inputBox with edit text.
            if (selectedTask != null)
            {
                inputBox.Text = selectedTask.RawText;
            }
            MoveCursorToEndOfWord();
        }

        //@author A0092104U
        private void DoSearch(Command result)
        {
            if (result.Text.Trim() != string.Empty)
            {
                isOriginalTasks = false;
                renderedTasks = GetTasksByHashTag(result.Text.Trim());
            }
        }

        //@author A0083834Y
        private void DoHelp()
        {
            helpDescriptionListBox.DataContext = helpDescription;

            helpDescriptionPopup.IsOpen = true;
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

        private void DoGenericCommand(Command result)
        {
            Task target = null;

            if (renderedTasks.Count > 0)
            {
                target = selectedTask;

                if (result.CommandText == Command.Edit)
                {
                    DoEdit(target);
                }
            }

            ExecuteCommand(result.CommandText, result.Text, target);

            // Clear the input box if the command was not edit.
            // If the command was edit, and a null task was sent to the Presenter,
            // then it was an invalid edit command. Clear the input box.
            if (result.CommandText != Command.Edit)
            {
                inputBox.Clear();
            }
            else if (selectedTask == null)
            {
                inputBox.Clear();
            }
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
        #endregion

        #region AutoComplete
        //@author A0092104U
        private void HandleAutoComplete()
        {
            const string SPACE = " ";

            // AutoComplete is only defined if there are rendered tasks on screen.
            if (renderedTasks != null && renderedTasks.Count > 0)
            {
                // If the command is Invalid, we try to autocomplete the command.
                // Otherwise, we complete the task.
                if ((parseResult.CommandText == Command.Invalid || parseResult.IsAlias) && parseResult.Text == string.Empty)
                {
                    inputBox.Text += (Command.TryComplete(inputBox.Text.Substring(1)) + SPACE);
                    MoveCursorToEndOfWord();
                }
                else
                {
                    // If the input text is just the command, we append a space so that 
                    // the user can continue typing.
                    // Otherwise, we complete the partially written task.
                    int completeBegin;
                    if (inputBox.Text.EndsWith(parseResult.CommandText))
                    {
                        inputBox.Text += SPACE;
                        MoveCursorToEndOfWord();
                    }
                    else if ((completeBegin = LCPIndex(parseResult.Text, renderedTasks[0].RawText)) >= 0)
                    {
                        inputBox.Text = inputBox.Text.Trim() + renderedTasks[0].RawText.Substring(completeBegin + 1);
                        MoveCursorToEndOfWord();
                    }
                }
            }
        }

        //@author A0092104U
        // Finds the longest common prefix of a and b.
        private int LCPIndex(string a, string b)
        {
            var found = -1;
            var commonLength = Math.Min(a.Length, b.Length);
            for (int i = 0; i < commonLength; i++)
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
        #endregion

        #region Context Escape
        private void HandleEscapeKey()
        {
            // If we are highlighting something, we stop highlighting, but do not hide the window;
            // We also refresh the view so that the highlight no longer shows.
            // If the input box is not empty, we clear the input box, but do not hide the window.
            // Otherwise, we hide the window.
            // If the input box contains only whitespace (which will not be caught by the first condition),
            // we clear it before hiding the window.
            if (isHighlighting)
            {
                StopHighlighting();
                RefreshViewList();
            }
            else if (inputBox.Text.Trim() != string.Empty)
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
        #endregion

        #region Selection Methods
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

        private void StartHighlighting()
        {
            highlightListIndex = 0;
            isHighlighting = true;
            ResetSelection();
        }

        private void StopHighlighting()
        {
            isHighlighting = false;
            ResetSelection();
        }

        private void ResetSelection()
        {
            // We have a non-ambiguous match iff there is exactly one task rendered.
            // Otherwise, set the selectedTask to null to represent no task selected.
            selectedTask = renderedTasks.Count == 1 ? renderedTasks[0] : null;
        }

        //if the list index is out of bound then set it back to the correct bound
        private void CheckListIndexBound()
        {
            if (listStartIndex < 0)
            {
                listStartIndex = 0;
            }

            if (listEndIndex > renderedTasks.Count)
            {
                listEndIndex = renderedTasks.Count;
            }
        }

        //if the highLightIndex out of bound the set it back to the correct bound
        private void CheckHighlightIndexBound()
        {
            if (highlightListIndex < 0)
            {
                highlightListIndex = 0;
            }
            if (highlightListIndex > (listEndIndex - 1) % NUMBER_OF_TASKS_DISPLAYED)
            {
                highlightListIndex = (listEndIndex - 1) % NUMBER_OF_TASKS_DISPLAYED;
            }
        }
        #endregion

        #region Page Navigation Handlers
        //@author A0088574M
        //modify the highlight index and may go to the previous page.
        private void HandleUpArrow()
        {
            if (!isHighlighting && listStartIndex != 0)
            {
                StartHighlighting();
            }

            highlightListIndex--;

            if (highlightListIndex < 0 && listStartIndex == 0)
            {
                StopHighlighting();
            }

            //when highlighIndex out of bound and current page is not the first page
            if (highlightListIndex < 0 && listStartIndex > 0)
            {
                MoveToPreviousPage();
                highlightListIndex = (listEndIndex - 1) % NUMBER_OF_TASKS_DISPLAYED;
            }

            CheckHighlightIndexBound();

            RefreshViewList();
        }

        //modify the highlightIndex and may go to next page
        private void HandleDownArrow()
        {
            if (!isHighlighting)
            {
                StartHighlighting();
            }
            else
            {
                highlightListIndex++;
            }

            if ((highlightListIndex > NUMBER_OF_TASKS_DISPLAYED - 1) && (listEndIndex != renderedTasks.Count))
            {
                MoveToNextPage();
                highlightListIndex = 0;
            }

            CheckHighlightIndexBound();

            RefreshViewList();
        }

        private void HandleLeftArrow()
        {
            //in case user only want to move the cursor in the text box, not the page
            //but user can still use the left key when searching for task in filter list
            if (parseResult.CommandText == Command.Search || inputBox.Text == string.Empty)
            {
                MoveToPreviousPage();

                int tempIndex = highlightPageIndex - 1;
                highlightPageButton(tempIndex);
            }
        }

        private void HandleRightArrow()
        {
            //in case user only want to move the cursor in the text box, not the page
            //but user can still use the right key when searching for task in filter list
            if (parseResult.CommandText == Command.Search || inputBox.Text == string.Empty)
            {
                MoveToNextPage();

                int tempIndex = highlightPageIndex + 1;
                highlightPageButton(tempIndex);
            }
        }
        #endregion

        #region Page Navigation Helper Methods
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

        //@author A0083834Y
        //Check if highlightPageIndex will be within the min and max page number
        private bool isWithinPageRange(int tempIndex)
        {
            int pages = GetPageNumber();

            if (tempIndex > 0 && tempIndex < pages + 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //@author A0083834Y
        //Style page button if index is within range
        private void highlightPageButton(int tempIndex)
        {
            if (isWithinPageRange(tempIndex))
            {
                highlightPageIndex = tempIndex;
                StyleHighlightedPageButton(highlightPageIndex);
            }
        }


        //go to next page, will modify listStartIndex and listEndIndex
        //may modify highlightIndex
        private void MoveToNextPage()
        {
            //already at the last page
            if (listEndIndex == renderedTasks.Count)
            {
                return;
            }

            listEndIndex += NUMBER_OF_TASKS_DISPLAYED;
            listStartIndex = listEndIndex - NUMBER_OF_TASKS_DISPLAYED;

            CheckListIndexBound();
            CheckHighlightIndexBound();

            ResetSelection();

            RefreshViewList();
        }

        //go to previous page, will modify listStartIndex and listEndIndex
        //may modify highlightIndex
        private void MoveToPreviousPage()
        {
            //already at the first page
            if (listStartIndex == 0)
            {
                return;
            }

            listStartIndex -= NUMBER_OF_TASKS_DISPLAYED;
            listEndIndex = listStartIndex + NUMBER_OF_TASKS_DISPLAYED;

            CheckListIndexBound();
            CheckHighlightIndexBound();

            ResetSelection();

            RefreshViewList();
        }
        #endregion

        #region Drawing
        //@author A0083834Y
        // Used for auto complete.
        private void MoveCursorToEndOfWord()
        {
            inputBox.Select(inputBox.Text.Length, 0);
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
        // Default style for text 
        private void DefaultStyle(TextBlock textBlock)
        {
            textBlock.FontSize = 20;
            textBlock.FontFamily = new FontFamily("GillSans");
            textBlock.Padding = new Thickness(10);
        }

        // Style for active tasks
        private void StyleTasks(TextBlock textBlock)
        {
            DefaultStyle(textBlock);
            textBlock.Margin = new Thickness(15, 0, 0, 0);
        }

        // Style for "no tasks" text
        private void StyleNoTasks(TextBlock textBlock)
        {
            DefaultStyle(textBlock);
            textBlock.TextAlignment = TextAlignment.Center;
        }

        // Style for completed parsed types (hash tags, datetime, priority)
        private void StyleDoneParsedTypes(Run run)
        {
            run.TextDecorations = TextDecorations.Strikethrough;
            run.FontStyle = FontStyles.Italic;
            run.Foreground = new SolidColorBrush(Color.FromArgb(255, 155, 157, 164));
        }

        // Style for hashtags (blue)
        private void StyleHashTags(Run run)
        {
            run.Foreground = new SolidColorBrush(Color.FromArgb(255, 68, 0, 150));
            run.FontWeight = FontWeights.DemiBold;
        }

        // Style for datetime (red)
        private void StyleDateTime(Run run)
        {
            run.Foreground = new SolidColorBrush(Color.FromArgb(255, 244, 0, 0));
            run.FontWeight = FontWeights.DemiBold;
        }

        // Style for priorityhigh (orange)
        private void StylePriorityHigh(Run run)
        {
            run.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 118, 20));
            run.FontWeight = FontWeights.DemiBold;
        }

        // Style for prioritylow (green)
        private void StylePriorityLow(Run run)
        {
            run.Foreground = new SolidColorBrush(Color.FromArgb(255, 152, 163, 62));
            run.FontWeight = FontWeights.DemiBold;
        }

        // Style for highlighted page button 
        private void StyleHighlightedPageButton(int index)
        {
            Ellipse ellipse = pageButtonArray[index];
            ellipse.Fill = new SolidColorBrush(Color.FromArgb(255, 89, 81, 70));
        }

        // Display blue border after each task
        private void DisplayBlueBorder(StackPanel parentStackPanel)
        {
            Line line = DrawBlueLine();
            AddStackPanel(parentStackPanel, line);
        }

        // Display dashed border at the end of current page
        private void DisplayDashedBorder(StackPanel parentStackPanel)
        {
            Rectangle dashedLine = DrawDashedLine();
            AddStackPanel(parentStackPanel, dashedLine);
        }

        // Display page button (gray)
        private void DisplayPageButton(StackPanel parentStackPanel, int pageNumber)
        {
            StackPanel pageButtons = new StackPanel();
            pageButtons.Orientation = Orientation.Horizontal;
            pageButtons.HorizontalAlignment = HorizontalAlignment.Center;

            for (int i = 1; i < pageNumber + 1; i++)
            {
                pageButtonArray[i] = DrawEllipse();
                pageButtons.Children.Add(pageButtonArray[i]);
            }

            AddStackPanel(parentStackPanel, pageButtons);
        }

        // Append shape to parent stackpanel
        private void AddStackPanel(StackPanel parentStackPanel, Shape shape)
        {
            StackPanel border = new StackPanel();
            border.Children.Add(shape);
            parentStackPanel.Children.Add(border);
        }

        // Append child stackpanel to parent stackpanel (for page buttons)
        private void AddStackPanel(StackPanel parentStackPanel, StackPanel childStackPanel)
        {
            parentStackPanel.Children.Add(childStackPanel);
        }

        private Line DrawBlueLine()
        {
            Line blueLine = new Line();

            blueLine.Stroke = new SolidColorBrush(Color.FromArgb(255, 128, 182, 248));
            blueLine.StrokeThickness = 0.7;
            blueLine.X1 = 0;
            blueLine.Y1 = 0;
            blueLine.X2 = 484;
            blueLine.Y2 = 0;

            return blueLine;
        }

        private Rectangle DrawDashedLine()
        {
            Rectangle dashedLine = new Rectangle();

            dashedLine.Stroke = new SolidColorBrush(Color.FromArgb(255, 197, 143, 57));
            dashedLine.StrokeThickness = 0.5;
            dashedLine.StrokeDashArray = new DoubleCollection() { 4, 3 };
            dashedLine.Margin = new Thickness(0, 10, 0, 0);

            return dashedLine;
        }

        // For page buttons
        private Ellipse DrawEllipse()
        {
            Ellipse ellipse = new Ellipse();

            ellipse.Stroke = new SolidColorBrush(Color.FromArgb(255, 212, 202, 190));
            ellipse.Fill = new SolidColorBrush(Color.FromArgb(255, 212, 202, 190));
            ellipse.Margin = new Thickness(3, 10, 0, 0);
            ellipse.Width = 7;
            ellipse.Height = 7;

            return ellipse;
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
        #endregion
    }
}
