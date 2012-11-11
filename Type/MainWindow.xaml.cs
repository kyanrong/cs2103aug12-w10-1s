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

    public delegate void RequestExecuteEventHandler(object sender, CommandEventArgs e);
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

        private List<Ellipse> pageButtonList;

        private Task selectedTask;
        private Command parseResult;
        private StackPanel taskView;
        private StackPanel pageButtons;

        #endregion

        #region Events
        public event RequestExecuteEventHandler RequestExecute;
        #endregion

        #region Constructors
        public MainWindow(FilterSuggestionsCallback GetFilterSuggestions, GetTasksCallback GetTasks, GetTasksByHashTagCallback GetTasksByHashTag)
        {
            InitializeComponent();

            this.GetFilterSuggestions = GetFilterSuggestions;
            this.GetTasks = GetTasks;
            this.GetTasksByHashTag = GetTasksByHashTag;

            //create stack panel here
            taskView = new StackPanel();
            pageButtons = new StackPanel();

            // Focus cursor in input box
            inputBox.Focus();

            // Display input label
            DisplayInputLabel();

            // Bootstrap tasks
            isOriginalTasks = true;
            renderedTasks = GetTasks(NUMBER_OF_TASKS_LOADED);
            taskTextBlockList = new List<TextBlock>();

            parseResult = Command.Parse(inputBox.Text);

            InitializeListBounderIndex();
            StopHighlighting();

            pageButtonList = new List<Ellipse>();

            RenderTasks();
        }
        #endregion

        #region WPF Events
        //@author A0083834Y
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
                    if (parseResult.Text.StartsWith("#"))
                    {
                        isOriginalTasks = false;
                        renderedTasks = GetTasksByHashTag(parseResult.Text);
                        RenderTasks();
                    }
                    else
                    {
                        isOriginalTasks = false;
                        renderedTasks = GetFilterSuggestions(parseResult.Text);
                        RenderTasks();
                    }
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
        private void InputBox_KeyUp(object sender, KeyEventArgs e)
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

        #region Event Methods
        //@author A0092104U
        protected virtual void OnRequestExecute(CommandEventArgs e)
        {
            if (RequestExecute != null) RequestExecute(this, e);
        }
        #endregion
    }
}
