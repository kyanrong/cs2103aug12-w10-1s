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

        private List<Task> tasks;
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
            tasks = new List<Task>();
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
                tasks.Add(new Task(content));
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
                            selectedTask.ToggleDone();
                            break;

                        case "archive":
                            selectedTask.ToggleArchive();
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

        private Tuple<FindTaskResult, Task> FindTaskByText(string rawText)
        {
            int querySize;
            FindTaskResult queryStatus;
            Task result = null;

            if ((querySize = tasks.Count(task => task.RawText == rawText)) == 0)
            {
                queryStatus = FindTaskResult.NOT_FOUND;
            }
            else if (querySize > 1)
            {
                queryStatus = FindTaskResult.AMBIGUOUS;
            }
            else
            {
                queryStatus = FindTaskResult.FOUND;
                result = tasks.First(task => task.RawText == rawText);
            }

            return new Tuple<FindTaskResult, Task>(queryStatus, result);
        }

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
