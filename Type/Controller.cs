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

        public void ParseCommand(string userInput)
        {
            if (IsDefaultCommand(userInput))
            {
                // Add

            }
            else
            {
                string command = ExtractCommandToken(ref userInput);
                switch (command)
                {
                    case "done":

                        break;

                    case "archive":

                        break;

                    case "edit":

                        break;
                }
            }

        }
    }
}
