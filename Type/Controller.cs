using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Type
{
    internal delegate void UIRedrawHandler(IList<Task> updateData);

    public class Controller
    {
        private string COMMAND_PREFIX = ":";
        private Key[] START_KEY_COMBINATION = { Key.LeftShift, Key.Space };

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

            globalHook = new GlobalKeyCombinationHook(ShowUi, START_KEY_COMBINATION);
        }

        ~Controller()
        {
            globalHook.StopListening();
        }

        private void ShowUi()
        {
            ui.Show();
        }

        private string ExtractCommandToken(ref string userInput)
        {
            int spIndex = userInput.IndexOf(' ');
            string commandToken = userInput.Substring(0, spIndex);
            userInput = userInput.Substring(spIndex + 1);
            return commandToken;
        }

        private bool IsDefaultCommand(string userInput)
        {
            return (!userInput.StartsWith(COMMAND_PREFIX));
        }

        internal void ExecuteCommand(string userInput, UIRedrawHandler redrawHandler)
        {
            //The default command is 'add'
            if (IsDefaultCommand(userInput))
            {
                tasks.Add(new Task(userInput));
                tasksAutoComplete.AddSuggestion(userInput);
            }
            else
            {
                string command = ExtractCommandToken(ref userInput);
                Task selectedTask = FindTaskByText(userInput);
                switch (command)
                {
                    case "done":
                        selectedTask.Done = true;
                        break;

                    case "archive":
                        selectedTask.Archive = true;
                        break;

                    case "edit":
                        tasks.Remove(selectedTask);
                        tasksAutoComplete.RemoveSuggestion(selectedTask.RawText);
                        break;
                }
            }
            redrawHandler(tasks.AsReadOnly());
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
                foreach (string tag in tags)
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
