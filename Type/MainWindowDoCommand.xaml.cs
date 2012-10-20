using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

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

        private void DoOther(Command result)
        {
            if (renderedTasks.Count != 0)
            {
                Task selectedTask = renderedTasks[0];
                ExecuteCommand(result.CommandText, result.Text, selectedTask);

                if (result.CommandText == Command.Edit)
                {
                    DoEdit(selectedTask);
                }
                else
                {
                    inputBox.Clear();
                }
            }
        }

        private void DoEdit(Task selectedTask)
        {
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
    }
}
