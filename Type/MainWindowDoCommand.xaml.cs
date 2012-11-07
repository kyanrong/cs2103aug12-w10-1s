using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;

namespace Type
{
    public partial class MainWindow : Window
    {
        private void DoAdd(Command result)
        {
            //The default command is "add".
            if (result.Text.Trim() != string.Empty)
            {
                ExecuteCommand(result.CommandText, result.Text);
                inputBox.Clear();
            }
        }

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

            ExecuteCommand(result.CommandText, result.Text, target);

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

        //@author A0092104U
        private void DoEdit(Task selectedTask)
        {
            //Populate inputBox with edit text.
            if (selectedTask != null)
            {
                inputBox.Text = selectedTask.RawText;
            }
            MoveCursorToEndOfWord();
        }

        //@author A0092104U
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
            helpDescriptionListBox.DataContext = helpDescription;

            helpDescriptionPopup.IsOpen = true;
        }
    }
}
