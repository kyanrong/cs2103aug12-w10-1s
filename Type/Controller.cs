using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Type
{
    internal delegate void UIRedrawHandler(IList<Task> updateData, int msgCode = 0, string msg = null);

    public class Controller
    {
        private const uint COMBINATION_MOD = GlobalKeyCombinationHook.MOD_SHIFT;
        private const uint COMBINATION_TRIGGER = 0x20;

        private GlobalKeyCombinationHook globalHook;

        private MainWindow ui;

        private List<Task> tasks;
        private AutoComplete tasksAutoComplete;

        public Controller()
        {
            //Sequence is important here. We need to initialize backend storage first,
            //followed by instantiating the UI, and finally, listening on the keyboard
            //hook. Messing up the sequence may result in race conditions.
            tasks = new List<Task>();
            tasksAutoComplete = new AutoComplete();

            ui = (new MainWindow()).setCallbacks(ExecuteCommand, GetTaskSuggestions);

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
            int msgCode = 0;
            string msg = null;

            if (command == "add")
            {
                tasks.Add(new Task(content));
                tasksAutoComplete.AddSuggestion(content);
            }
            else
            {
                Task selectedTask = FindTaskByText(content);
                switch (command)
                {
                    case "done":
                        selectedTask.Done = true;
                        break;

                    case "archive":
                        selectedTask.Archive = true;
                        break;
                        
                    case "edit":
                        //Remove the original task from the model.
                        tasks.Remove(selectedTask);
                        tasksAutoComplete.RemoveSuggestion(selectedTask.RawText);

                        //Return the text to the UI for editing.
                        msgCode = 2;
                        msg = selectedTask.RawText;
                        break;
                }
            }

            redrawHandler(tasks.AsReadOnly(), msgCode, msg);
        }

        private Task FindTaskByText(string rawText)
        {
            return (tasks.First(task => task.RawText == rawText));
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

        private IAutoComplete GetTaskSuggestions()
        {
            return tasksAutoComplete;
        }
    }
}
