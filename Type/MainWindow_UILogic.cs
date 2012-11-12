using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

namespace Type
{
    public partial class MainWindow : Window
    {
        #region Command Handlers
        //@author A0083834Y
        private void HandleSendCommand()
        {
            switch (parseResult.CommandText)
            {
                case Command.Invalid:
                    invalidCmdPopup.IsOpen = true;
                    break;

                case Command.Search:
                    DoSearch(parseResult);
                    break;

                case Command.Add:
                    DoAdd(parseResult);
                    break;

                case Command.Help:
                    DoHelp();
                    break;

                case Command.Archive:
                case Command.Done:
                case Command.Undo:
                case Command.Edit:
                case Command.Clear:
                    DoGenericCommand(parseResult);
                    break;

                default:
                    break;
            }

            // Retrieve a list of tasks, unless the list has already been retrieved by Search.
            if (parseResult.CommandText != Command.Search)
            {
                isOriginalTasks = true;
                renderedTasks = GetTasks();
            }

            RenderTasks();
        }

        //@author A0083834Y
        private void DoEdit(Task selectedTask)
        {
            //Populate inputBox with edit text.
            if (selectedTask != null)
            {
                inputBox.Text = selectedTask.RawText;
            }
            MoveCursorToEndOfWord();
        }

        //@author A0083834Y
        private void DoSearch(Command result)
        {
            if (result.Text.Trim() != string.Empty)
            {
                isOriginalTasks = false;
                renderedTasks = GetTasksByHashTag(result.Text.Trim());
            }
        }

        //@author A0083834Y
        private void DoHelp()
        {
            List<String> helpDescription = PopulateHelpList();        

            helpDescriptionListBox.DataContext = helpDescription;

            helpDescriptionPopup.IsOpen = true;
        }

        //@author A0092104U
        private void DoAdd(Command result)
        {
            //The default command is "add".
            if (result.Text.Trim() != string.Empty)
            {
                OnRequestExecute(new CommandEventArgs(result.CommandText, result.Text));
                inputBox.Clear();
            }
        }

        //@author A0088574M
        private void DoGenericCommand(Command result)
        {
            Task target = null;

            if (renderedTasks.Count > 0)
            {
                target = selectedTask;

                if (result.CommandText == Command.Edit)
                {
                    DoEdit(target);
                }
            }

            OnRequestExecute(new CommandEventArgs(result.CommandText, result.Text, target));

            // Clear the input box if the command was not edit.
            // If the command was edit, and a null task was sent to the Presenter,
            // then it was an invalid edit command. Clear the input box.
            if (result.CommandText != Command.Edit)
            {
                inputBox.Clear();
            }
            else if (selectedTask == null)
            {
                inputBox.Clear();
            }
        }
        #endregion

        #region AutoComplete
        //@author A0092104U
        private void HandleAutoComplete()
        {
            const string SPACE = " ";

            // AutoComplete is only defined if there are rendered tasks on screen.
            if (renderedTasks != null && renderedTasks.Count > 0)
            {
                // If the command is Invalid, we try to autocomplete the command.
                // Otherwise, we complete the task.
                if ((parseResult.CommandText == Command.Invalid || parseResult.IsAlias) && parseResult.Text == string.Empty)
                {
                    inputBox.Text += (Command.TryComplete(inputBox.Text.Substring(1)) + SPACE);
                    MoveCursorToEndOfWord();
                }
                else
                {
                    // If the input text is just the command, we append a space so that 
                    // the user can continue typing.
                    // Otherwise, we complete the partially written task.
                    int completeBegin;
                    if (inputBox.Text.EndsWith(parseResult.CommandText))
                    {
                        inputBox.Text += SPACE;
                        MoveCursorToEndOfWord();
                    }
                    else if ((completeBegin = LCPIndex(parseResult.Text, renderedTasks[0].RawText)) >= 0)
                    {
                        inputBox.Text = inputBox.Text.Trim() + renderedTasks[0].RawText.Substring(completeBegin + 1);
                        MoveCursorToEndOfWord();
                    }
                }
            }
        }
        #endregion

        #region Context Escape
        //@author A0092104U
        private void HandleEscapeKey()
        {
            // If we are highlighting something, we stop highlighting, but do not hide the window;
            // We also refresh the view so that the highlight no longer shows.
            // If the input box is not empty, we clear the input box, but do not hide the window.
            // Otherwise, we hide the window.
            // If the input box contains only whitespace (which will not be caught by the first condition),
            // we clear it before hiding the window.
            if (isHighlighting)
            {
                StopHighlighting();
                RefreshViewList();
            }
            else if (inputBox.Text.Trim() != string.Empty)
            {
                inputBox.Clear();
            }
            else
            {
                if (inputBox.Text.Trim() == string.Empty)
                {
                    inputBox.Clear();
                }
                this.Hide();
            }
        }
        #endregion

        #region Helper Methods
        //@author A0083834Y
        private List<string> PopulateHelpList()
        {
            List<String> helpDescription = new List<String>();

            // Populate helpDescription List
            helpDescription.Add("Create new task");
            helpDescription.Add("<task>");

            helpDescription.Add("Complete a task");
            helpDescription.Add(":done <task>");

            helpDescription.Add("Complete all tasks with the hash tag");
            helpDescription.Add(":done #<tag name>");

            helpDescription.Add("Archive all completed tasks");
            helpDescription.Add(":archive");

            helpDescription.Add("Archive a single task");
            helpDescription.Add(":archive <task>");

            helpDescription.Add("Archive all tasks with the hash tag");
            helpDescription.Add(":archive #<tag name>");

            helpDescription.Add("Edit a task");
            helpDescription.Add(":edit <task>");

            helpDescription.Add("Undo last action");
            helpDescription.Add(":undo");

            helpDescription.Add("Filter by hash tags");
            helpDescription.Add("/<tag name> [<tag name>] ...");

            helpDescription.Add("Show archived tasks");
            helpDescription.Add("/archive <tag name> [<tag name>] ...");

            helpDescription.Add("Help(Displays this list)");
            helpDescription.Add(":help or ?");

            return helpDescription;
        }

        //@author A0092104U
        // Finds the longest common prefix of a and b.
        private int LCPIndex(string a, string b)
        {
            var found = -1;
            var commonLength = Math.Min(a.Length, b.Length);
            for (int i = 0; i < commonLength; i++)
            {
                if (a[i] == b[i])
                {
                    found = i;
                }
                else
                {
                    break;
                }
            }
            return found;
        }

        //@author A0083834Y
        private void MoveCursorToEndOfWord()
        {
            inputBox.Select(inputBox.Text.Length, 0);
        }
        #endregion
    }
}
