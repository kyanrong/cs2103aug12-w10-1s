using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Type
{
    internal delegate void UIRedrawHandler(IList<Task> updateData, UIRedrawMsgCode msgCode = UIRedrawMsgCode.EMPTY, string msg = null);
    internal enum UIRedrawMsgCode
    {
        EMPTY,
        EDITED_TEXT,
        ERROR,
        WARNING
    }

    public class Controller
    {
        private const uint COMBINATION_MOD = GlobalKeyCombinationHook.MOD_SHIFT;
        private const uint COMBINATION_TRIGGER = 0x20;
        private const string FIND_NOT_FOUND = "no matches found";
        private const string FIND_AMBIGIOUS = "more than one match found";
        private string[] COMMANDS_ACCEPTED = { "done", "archive", "edit" };

        private GlobalKeyCombinationHook globalHook;

        private MainWindow ui;

        private TaskCollection tasks;
        //private List<Task> tasks;
        private AutoComplete tasksAutoComplete;

        private AutoComplete acceptedCommands;

        private enum FindTaskResult
        {
            NOT_FOUND,
            AMBIGUOUS,
            FOUND
        }

        public Controller()
        {
            //Sequence is important here. We need to initialize backend storage first,
            //followed by instantiating the UI, and finally, listening on the keyboard
            //hook. Messing up the sequence may result in race conditions.

            tasks = new TaskCollection();
            //tasks = new List<Task>();
            tasksAutoComplete = new AutoComplete();
            acceptedCommands = new AutoComplete(COMMANDS_ACCEPTED);

            ui = (new MainWindow()).setCallbacks(ExecuteCommand, GetAutoCompleteReference, GetAcceptedCommandsReference);

            globalHook = (new GlobalKeyCombinationHook(ui, ShowUi, COMBINATION_MOD, COMBINATION_TRIGGER)).StartListening();
        }

        ~Controller()
        {
            //We need to unregister the hotkey when the application closes to be a good Windows citizen.
            globalHook.StopListening();
        }

        private void ShowUi()
        {
            ui.Show();
        }

        internal void ExecuteCommand(string command, string content, UIRedrawHandler redrawHandler)
        {
            UIRedrawMsgCode msgCode = 0;
            string msg = null;

            if (command == "add")
            {
                tasks.Create(content);
                tasksAutoComplete.AddSuggestion(content);
            }
            else
            {
                var idResult = FindTaskByText(content);
                if (idResult.Item1 == FindTaskResult.NOT_FOUND)
                {
                    msgCode = UIRedrawMsgCode.ERROR;
                    msg = FIND_NOT_FOUND;
                }
                else
                {
                    if (idResult.Item1 == FindTaskResult.AMBIGUOUS)
                    {
                        msgCode = UIRedrawMsgCode.WARNING;
                        msg = FIND_AMBIGIOUS;
                    }

                    var selectedTask = idResult.Item2;
                    switch (command)
                    {
                        case "done":
                            tasks.UpdateDone(selectedTask, true);
                            break;

                        case "archive":
                            tasks.UpdateArchive(selectedTask, true);
                            break;

                        case "edit":
                            //Remove the original task from the model.
                            tasks.Remove(selectedTask);
                            tasksAutoComplete.RemoveSuggestion(selectedTask.RawText);

                            //Return the text to the UI for editing.
                            msgCode = UIRedrawMsgCode.EDITED_TEXT;
                            msg = selectedTask.RawText;
                            break;
                    }
                }
            }

            redrawHandler(tasks.AsReadOnly(), msgCode, msg);
        }

        /// <summary>
        /// Gets the handle of a task based on its raw text.
        /// </summary>
        /// <param name="rawText">Raw text search key.</param>
        /// <returns>A Tuple containing the find result, and the handle of the task if it is found.
        /// If there is an ambiguous match, the handle of the first task matched is returned.
        /// If there are no matches, a null value is returned.</returns>
        private Tuple<FindTaskResult, int> FindTaskByText(string rawText)
        {
            FindTaskResult queryStatus;
            Task result = null;

            var resultSet = tasks.filterAll(rawText);
            if (resultSet.Count == 0)
            {
                queryStatus = FindTaskResult.NOT_FOUND;
            }
            else 
            {
                queryStatus = FindTaskResult.FOUND;

                if (resultSet.Count > 1)
                {
                    queryStatus = FindTaskResult.AMBIGUOUS;
                }

                result = resultSet[0].Id;
            }

            return new Tuple<FindTaskResult, int>(queryStatus, result);
        }

        /// <summary>
        /// Builds a list of tasks to display based on the search criteria.
        /// If a list of hash tags is specified in tags, the return value will contain only tasks that match at least one of the specified hash tags.
        /// </summary>
        /// <param name="count">Number of tasks to get.</param>
        /// <param name="tags">List of hash tags.</param>
        /// <returns>Read-only list of tasks to display.</returns>
        private IList<Task> GetTasksToDisplay(int count = 5, List<string> tags = null)
        {
            if (tags == null)
            {
                return (tasks.Take(count).ToList().AsReadOnly());
            }
            else
            {
                var resultSet = new HashSet<Task>();
                foreach (var tag in tags)
                {
                    resultSet.UnionWith(tasks.Where(t => t.Tags.Contains(tag)));
                }
                return (resultSet.Take(count).ToList().AsReadOnly());
            }
        }

        private IAutoComplete GetAutoCompleteReference()
        {
            return tasksAutoComplete;
        }

        private IAutoComplete GetAcceptedCommandsReference()
        {
            return acceptedCommands;
        }
    }
}
