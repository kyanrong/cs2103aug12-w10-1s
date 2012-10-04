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

        public Controller()
        {
            ui = new MainWindow(this);
            globalHook = new ShortcutKeyHook(this, START_KEY_COMBINATION);


        }

        ~Controller()
        {
            globalHook.StopListening();
        }

        public void ShowUi()
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
            return (userInput.StartsWith(COMMAND_PREFIX));
        }

        public void ExecuteCommand(string userInput, int? taskId)
        {
            // the default command is 'add'
            if (IsDefaultCommand(userInput))
            {
                Task newTask = new Task(userInput);
                tasks.Add(newTask);
            }
            else
            {
                if (taskId == null)
                {
                    throw new ArgumentNullException();
                }
                string command = ExtractCommandToken(ref userInput);
                Task selectedTask = tasks.Single((Task t) => (t.Id == taskId));
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
            ui.UpdateDisplay(tasks);
        }
    }
}
