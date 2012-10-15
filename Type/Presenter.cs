using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Type
{
    public class Presenter
    {
        //Key combination to catch.
        private const uint COMBINATION_MOD = GlobalKeyCombinationHook.MOD_SHIFT;
        private const uint COMBINATION_TRIGGER = 0x20;

        //Command strings.
        private const string CMD_TOKEN = ":";
        private const string CMD_INVALID = "invalid";
        private const string CMD_ADD = "add";
        private const string CMD_EDIT = "edit";
        private const string CMD_DONE = "done";
        private const string CMD_ARCHIVE = "archive";


        private GlobalKeyCombinationHook globalHook;
        private MainWindow ui;
        private TaskCollection tasks;
        private bool editMode;
        private Task selected;

        public Presenter()
        {
            //Sequence is important here. Messing up the sequence may result in race conditions.
            tasks = new TaskCollection();
            ui = new MainWindow(FilterSuggestions, HandleCommand, GetTasks);
            globalHook = (new GlobalKeyCombinationHook(ui, ShowUi, COMBINATION_MOD, COMBINATION_TRIGGER)).StartListening();
        }

        ~Presenter()
        {
            //We need to unregister the hotkey when the application closes to be a good Windows citizen.
            globalHook.StopListening();
        }

        /// <summary>
        /// Displays the UI window. Called when a defined key combination is pressed.
        /// </summary>
        public void ShowUi()
        {
            ui.Show();
        }

        /// <summary>
        /// Retrieves a list of suggestions that begin with a specified prefix.
        /// </summary>
        /// <param name="partialText">Prefix to match.</param>
        /// <returns>Read-only list of suggestions as strings.</returns>
        private IList<Task> FilterSuggestions(string partialText)
        {
            var parseResult = Parse(partialText);
            string cmd = parseResult.Item1;
            string content = parseResult.Item2;

            if (cmd == CMD_ADD)
            {
                return null;
            }
            else
            {
                return tasks.FilterAll(content);
            }
        }

        /// <summary>
        /// Retrieves a list of tasks to be displayed.
        /// </summary>
        /// <param name="num">Number of tasks to retrieve.</param>
        /// <returns>Read-only list of tasks.</returns>
        private IList<Task> GetTasks(int num)
        {
            return tasks.Get(num);
        }

        /// <summary>
        /// Parses a raw string and executes its command, if valid.
        /// If no valid command is found, this method does nothing.
        /// </summary>
        /// <param name="rawText">Text to parse.</param>
        /// <param name="selected">Selected task. Throws an exception if no reference is specified, but the command requires one.</param>
        private void HandleCommand(string rawText, Task selected = null)
        {
            var parseResult = Parse(rawText);
            string cmd = parseResult.Item1;
            string content = parseResult.Item2;

            //In edit mode, the only valid command is 'add'.
            //Otherwise, accept all commands.
            if (editMode)
            {
                EditModeSelectedTask(cmd, content);
            }
            else
            {
                Execute(cmd, content, selected);
            }
        }

        private void Execute(string cmd, string content, Task selected)
        {
            //Store a reference to the selected task in case we need to use it again in edit mode.
            this.selected = selected;

            switch (cmd)
            {
                case CMD_ADD:
                    tasks.Create(content);
                    break;

                case CMD_EDIT:
                    //The selected task is already stored. We set the editMode flag and return. The next command
                    //should be an 'add' containing the edited raw text of the selected task.
                    editMode = true;
                    break;

                case CMD_DONE:
                    tasks.UpdateDone(selected.Id, true);
                    break;

                case CMD_ARCHIVE:
                    tasks.UpdateArchive(selected.Id, true);
                    break;

                default:
                    //Do nothing.
                    break;
            }
        }

        private void EditModeSelectedTask(string cmd, string content)
        {
            if (cmd == CMD_ADD)
            {
                //The selected task should have been previously stored on the preceeding command.
                tasks.UpdateRawText(selected.Id, content);
            }
            else
            {
                //This should not happen. Handle it somehow.
                //TODO
            }

            //Escape from edit mode after this function call.
            editMode = false;
        }

        /// <summary>
        /// Parses input by splitting it into a token containing the command's text, and a token containing the rest of the input.
        /// </summary>
        /// <param name="input">Input to parse. Commands should start with the symbol defined in COMMAND_TOKEN.</param>
        /// <returns>A Tuple containing the command text and remaining input.</returns>
        private Tuple<string, string> Parse(string input)
        {
            string cmd;
            if (input.StartsWith(CMD_TOKEN))
            {
                int spIndex = input.IndexOf(' ');
                if (spIndex < 0)
                {
                    cmd = CMD_INVALID;
                    input = "";
                }
                else
                {
                    cmd = input.Substring(1, spIndex - 1);
                    input = input.Substring(spIndex + 1);
                }
            }
            else
            {
                cmd = CMD_ADD;
            }

            return new Tuple<string, string>(cmd, input);
        }
    }
}
