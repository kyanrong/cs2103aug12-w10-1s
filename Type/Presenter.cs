using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Type
{
    //@author A0092104U
    class Presenter
    {
        #region Constants
        //Key combination to catch.
        private const uint COMBINATION_MOD = GlobalKeyCombinationHook.MOD_SHIFT;
        private const uint COMBINATION_TRIGGER = 0x20;

        //Fatal error codes.
        private const int ERR_HOTKEY_CODE = -1;
        #endregion

        #region Logging Messages
        //Log file path.
        private const string LOG_PATH = "type.log";
        private const string LOG_APP_FULL_INIT = "Application Fully Initialized.";
        private const string LOG_SHORTCUT = "Shortcut Combination detected. Showing UI...";
        private const string LOG_DATE_CHANGE = "DateChange event detected. Reparsing Tasks, Forcing UI Redraw.";
        private const string LOG_DEL_PARTIALTEXT = "GetTasksWithPartialText() called.";
        private const string LOG_DEL_NOFILTER = "GetTasksNoFilter() called.";
        private const string LOG_DEL_HASHTAGS = "GetTasksByHashTags() called.";
        private const string LOG_EDIT_RECEIVE = "Received Edit Command.";
        private const string LOG_EDIT_ENTER = "Entering Edit Mode...";
        private const string LOG_EDIT_DO = "Performing Edit...";
        private const string LOG_EDIT_LEAVE = "Leaving Edit Mode...";
        private const string LOG_ERR_HOTKEY = "Another instance of Type already running. Shutting down.";
        private const string LOG_ADD = "Received Add Command.";
        private const string LOG_DONE_RECIEVE = "Received Done Command.";
        private const string LOG_DONE_SELECTED = "Received Done Command.";
        private const string LOG_DONE_HASHATGS = "Received Done Command.";
        private const string LOG_ARCHIVE_RECEIVE = "Received Archive Command.";
        private const string LOG_ARCHIVE_SELECTED = "Archiving selected Task.";
        private const string LOG_ARCHIVE_ALL = "Archiving all 'Done' Tasks.";
        private const string LOG_ARCHIVE_HASHTAGS = "Archiving Tasks by Hash Tags.";
        private const string LOG_UNDO = "Received Undo Command.";
        private const string LOG_CLEAR = "Received Clear Command.";
        private const string LOG_INVALID = "Received Invalid Command.";
        #endregion

        #region Fields
        private GlobalKeyCombinationHook globalHook;
        private MainWindow ui;
        private TaskCollection tasks;
        private bool editMode;
        private Task selected;
        private Comparison<Task> comparator;
        private DateChangeNotifier dateNotifier;
        private Logger typeLog;
        #endregion

        #region Constructors
        public Presenter()
        {
            //Sequence is important here. Messing up the sequence may result in race conditions.

            //Create a Logger.
            typeLog = new Logger(LOG_PATH);

            //Set task comparator to the default comparator.
            comparator = Task.DefaultComparison;

            //Create Task Collection.
            tasks = new TaskCollection();

            CreateUI();
            SetGlobalHook();
            SetDateChangeNotifier();
#if !DEBUG
            ShowUIOnFirstLaunch();
#endif

            typeLog.Log(LOG_APP_FULL_INIT);
        }

        ~Presenter()
        {
            //We need to unregister the hotkey when the application closes to be a good Windows citizen.
            globalHook.StopListening();
        }
        #endregion

        #region UI Handler
        /// <summary>
        /// Displays the UI window. Called when a defined key combination is pressed.
        /// </summary>
        void globalHook_ShortcutPressed(object sender, EventArgs e)
        {
            typeLog.Log(LOG_SHORTCUT);

            ui.Show();
        }
        #endregion

        #region Date Change Handler
        private void dateNotifier_DateChange(object sender, EventArgs e)
        {
            typeLog.Log(LOG_DATE_CHANGE);

            tasks.MidnightReParse();
            ui.ForceRedraw();
        }
        #endregion

        #region Delegate Targets
        /// <summary>
        /// Retrieves a list of suggestions that begin with a specified prefix.
        /// </summary>
        /// <param name="partialText">Prefix to match.</param>
        /// <returns>Read-only list of suggestions as strings.</returns>
        private IList<Task> GetTasksWithPartialText(string partialText)
        {
            typeLog.Log(LOG_DEL_PARTIALTEXT);

            var resultSet = tasks.FilterAll(partialText);
            resultSet.Sort(comparator);
            return resultSet;
        }

        /// <summary>
        /// Retrieves a list of tasks to be displayed.
        /// </summary>
        /// <param name="num">Number of tasks to retrieve.</param>
        /// <returns>Read-only list of tasks.</returns>
        private IList<Task> GetTasksNoFilter()
        {
            typeLog.Log(LOG_DEL_NOFILTER);

            var resultSet = tasks.GetNotArchiveTasks();
            resultSet.Sort(comparator);
            return resultSet;
        }

        /// <summary>
        /// Retrieves a list of tasks tagged with at least one hash tag.
        /// </summary>
        /// <param name="content">String containing hash tags separated by ' '.</param>
        /// <returns>Read-only list of tasks.</returns>
        private IList<Task> GetTasksByHashTags(string content)
        {
            typeLog.Log(LOG_DEL_HASHTAGS);

            var tags = content.Split(' ').ToList();
            
            //Prepend a hash to the tag name if it doesn't already have one.
            for (int i = 0; i < tags.Count; i++)
            {
                if (!tags[i].StartsWith(Command.HashToken))
                {
                    tags[i] = Command.HashToken + tags[i];
                }
            }

            var resultSet = tasks.ByHashTags(tags);
            resultSet.Sort(comparator);
            return resultSet;
        }
        #endregion

        #region Decision Making
        private void Execute(string cmd, string content, Task selected)
        {
            //Store a reference to the selected task in case we need to use it again in edit mode.
            this.selected = selected;

            switch (cmd)
            {
                case Command.Add:
                    typeLog.Log(LOG_ADD);
                    tasks.Create(content);
                    break;

                case Command.Edit:
                    typeLog.Log(LOG_EDIT_RECEIVE);
                    //The selected task is already stored. We set the editMode flag and return. The next command
                    //should be an 'add' containing the edited raw text of the selected task.
                    if (selected != null)
                    {
                        typeLog.Log(LOG_EDIT_ENTER);

                        editMode = true;
                    }
                    break;

                case Command.Done:
                    typeLog.Log(LOG_DONE_RECIEVE);
                    HandleCommand_Done(content, selected);
                    break;

                case Command.Archive:
                    typeLog.Log(LOG_ARCHIVE_RECEIVE);
                    HandleCommand_Archive(content, selected);
                    break;

                case Command.Undo:
                    typeLog.Log(LOG_UNDO);
                    // Call undo method of TaskCollection Obj.
                    tasks.Undo();
                    break;

                case Command.Clear:
                    typeLog.Log(LOG_CLEAR);
                    tasks.Clear();
                    tasks = new TaskCollection();
                    break;
    
                default:
                    typeLog.LogException(LOG_INVALID);
                    //Do nothing.
                    break;
            }
        }
        #endregion

        #region UI Event Handlers
        void ui_RequestExecute(object sender, CommandEventArgs e)
        {
            //In edit mode, the only valid command is 'add'.
            //Otherwise, accept all commands.
            if (editMode)
            {
                typeLog.Log(LOG_EDIT_DO);
                EditModeSelectedTask(e.Command, e.Content);
            }
            else
            {
                Execute(e.Command, e.Content, e.SelectedTask);
            }
        }
        #endregion

        #region Helper Methods
        private void CreateUI()
        {
            ui = new MainWindow(GetTasksWithPartialText, GetTasksNoFilter, GetTasksByHashTags);
            ui.RequestExecute += new RequestExecuteEventHandler(ui_RequestExecute);
        }

        private void SetDateChangeNotifier()
        {
            dateNotifier = new DateChangeNotifier();
            dateNotifier.DateChange += new DateChangeEventHandler(dateNotifier_DateChange);
        }

        private void SetGlobalHook()
        {
            try
            {
                globalHook = new GlobalKeyCombinationHook(ui, COMBINATION_MOD, COMBINATION_TRIGGER);
                globalHook.ShortcutPressed += new ShortcutPressedEventHandler(globalHook_ShortcutPressed);
                globalHook.StartListening();
            }
            catch (Exception)
            {
                typeLog.LogException(LOG_ERR_HOTKEY);
                Application.Current.Shutdown(ERR_HOTKEY_CODE);
            }
        }

        private void HandleCommand_Archive(string content, Task selected)
        {
            var tags = GetHashTagList(content);

            if (tags == null && selected != null)
            {
                //Archive selected.
                typeLog.Log(LOG_ARCHIVE_SELECTED);
                tasks.UpdateArchive(selected.Id, true);
            }
            else
            {
                // If content is a list of hash tags, then archive each task that contains any of the
                // supplied hash tags.
                // Otherwise, the intention is to archive all 'done' rendered tasks.
                if (tags == null)
                {
                    //Archive all done.
                    typeLog.Log(LOG_ARCHIVE_ALL);
                    tasks.ArchiveAll();
                }
                else
                {
                    typeLog.Log(LOG_ARCHIVE_HASHTAGS);
                    tasks.ArchiveAllByHashTags(tags);
                }
            }
        }

        private void HandleCommand_Done(string content, Task selected)
        {
            var tags = GetHashTagList(content);

            if (tags == null)
            {
                typeLog.Log(LOG_DONE_SELECTED);
                tasks.UpdateDone(selected.Id, true);
            }
            else
            {
                // If content is a list of hash tags, then mark each task that contains any of the
                // supplied hash tags as 'done'.
                // Otherwise, do nothing.
                if (tags != null)
                {
                    typeLog.Log(LOG_DONE_HASHATGS);
                    tasks.UpdateDoneByHashTags(tags);
                }
            }
        }

        private List<string> GetHashTagList(string input)
        {
            List<string> result = new List<string>();

            var tokens = input.Split(' ');
            bool isList = true;
            for (int i = 0; i < tokens.Length && isList; i++)
            {
                var trimmed = tokens[i].Trim();
                if (!trimmed.StartsWith(Command.HashToken))
                {
                    isList = false;
                }
                else
                {
                    result.Add(trimmed);
                }
            }

            return (isList ? result : null);
        }

        private void EditModeSelectedTask(string cmd, string content)
        {
            if (cmd == Command.Add)
            {
                //The selected task should have been previously stored on the preceeding command.
                tasks.UpdateRawText(selected.Id, content);
            }

            //If the command was not "Add", we assume the user wants to exit edit mode.

            //Escape from edit mode after this function call.
            typeLog.Log(LOG_EDIT_LEAVE);
            editMode = false;
        }

#if !DEBUG
        private void ShowUIOnFirstLaunch()
        {
            if (!Installer.IsInstalled())
            {
                ui.Show();
                Installer.EmbedOnFirstRun();
            }
        }
#endif
        #endregion
    }
}
