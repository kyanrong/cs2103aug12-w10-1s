using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Type
{
    public class Controller
    {
        private string COMMAND_PREFIX = ":";
        private Key[] START_KEY_COMBINATION = { Key.LeftShift, Key.Space };

        private ShortcutKeyHook globalHook;
        private MainWindow ui;

        private List<Task> tasks;
        private List<Task> displaySet;

        public Controller()
        {
            ui = new MainWindow(this);
            globalHook = new ShortcutKeyHook(this, START_KEY_COMBINATION);

            tasks = new List<Task>();
        }

        ~Controller()
        {
            globalHook.StopListening();
        }

        internal void ShowUi()
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

        internal void ExecuteCommand(string userInput)
        {
            // the default command is 'add'
            if (IsDefaultCommand(userInput))
            {
                Task newTask = new Task(userInput);
                tasks.Add(newTask);
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
                        break;
                }
            }
            ui.UpdateDisplay();
        }

        private Task FindTaskByText(string rawText)
        {
            return (tasks.First(task => task.RawText == rawText));
        }

        internal IList<Task> GetTasksToDisplay(int count = 5, List<string>? tags = null)
        {
            if (tags == null)
            {
                var resultSet = tasks.Where(t => t.Tags.Intersect(tags.Value).Equals(tags.Value));
                return (resultSet.Take(count).ToList().AsReadOnly());
            }
            else
            {
                return (tasks.Take(count).ToList().AsReadOnly());
            }
        }
    }
}
