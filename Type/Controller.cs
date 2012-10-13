using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Type
{
    public class Controller
    {
        private const uint COMBINATION_MOD = GlobalKeyCombinationHook.MOD_SHIFT;
        private const uint COMBINATION_TRIGGER = 0x20;
        private const string FIND_NOT_FOUND = "no matches found";
        private const string FIND_AMBIGIOUS = "more than one match found";
        private const int UI_NUM_DISPLAY = 5;
        private const string COMMAND_TOKEN = ":";

        private GlobalKeyCombinationHook globalHook;

        private MainWindow ui;

        private TaskCollection tasks;

        private bool editMode;
        private Task editTask;

        public Controller()
        {
            //Sequence is important here. We need to initialize backend storage first,
            //followed by instantiating the UI, and finally, listening on the keyboard
            //hook. Messing up the sequence may result in race conditions.

            tasks = new TaskCollection();
            //tasks = new List<Task>();

            ui = new MainWindow(FilterSuggestions, ProcessCommandHandler, GetTasks);

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

        private IList<Task> FilterSuggestions(string partialText)
        {
            return tasks.FilterAll(partialText);
        }

        private IList<Task> GetTasks(int num)
        {
            return tasks.Get(num);
        }

        private void ProcessCommandHandler(string rawText, Task selected)
        {
            var parseResult = ParseCommand(rawText);
            string cmd = parseResult.Item1;
            string content = parseResult.Item2;

            if (editMode)
            {
                EditModeSelectedTask(cmd, content);
            }
            else
            {
                ExecuteCommand(cmd, content, selected);
            }
        }

        private void ExecuteCommand(string cmd, string content, Task selected)
        {
            switch (cmd)
            {
                case "add":
                    tasks.Create(content);
                    break;

                case "edit":
                    editMode = true;
                    editTask = selected;
                    break;

                case "done":
                    tasks.UpdateDone(selected.Id, true);
                    break;

                case "archive":
                    tasks.UpdateArchive(selected.Id, true);
                    break;

                default:
                    //Do nothing.
                    break;
            }
        }

        private void EditModeSelectedTask(string cmd, string content)
        {
            if (cmd == "add")
            {
                tasks.UpdateRawText(editTask.Id, content);
            }
            else
            {
                //This should not happen. Handle it somehow.
            }
            editMode = false;
        }

        private Tuple<string, string> ParseCommand(string input)
        {
            string cmd;
            if (input.StartsWith(COMMAND_TOKEN))
            {
                int spIndex = input.IndexOf(' ');
                cmd = input.Substring(1, spIndex);
                input = input.Substring(spIndex + 1);
            }
            else
            {
                cmd = "add";
            }

            return new Tuple<string, string>(cmd, input);
        }
    }
}
