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

            if (result.CommandText != Command.Edit)
            {
                inputBox.Clear();
            }
        }

        // @author A0092104
        private void DoEdit(Task selectedTask)
        {
            Debug.Assert(selectedTask != null);

            //Populate inputBox with edit text.
            inputBox.Text = selectedTask.RawText;
            MoveCursorToEndOfWord();
        }

        private void DoSearch(Command result)
        {
            if (result.Text.Trim() != string.Empty)
            {
                renderedTasks = GetTasksByHashTag(result.Text.Trim());
            }
        }

        private void DoHelp()
        {
            helpDescriptionListBox.DataContext = helpDescription;

            helpDescriptionPopup.IsOpen = true;
        }
    }
}
