using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Type
{
    public partial class MainWindow : Window
    {
        private void HandleSendCommand()
        {
            //Parse input.
            var result = Command.Parse(inputBox.Text);

            switch (result.CommandText)
            {
                case Command.Invalid:
                    invalidCmdPopup.IsOpen = true;
                    break;

                case Command.Search:
                    DoSearch(result);
                    break;

                case Command.Add:
                    DoAdd(result);
                    break;

                case Command.Help:
                    DoHelp();
                    break;

                case Command.Archive:
                case Command.Done:
                case Command.Edit:
                    DoOther(result);
                    break;
                    
                default:
                    break;
            }

            //Retrieve a list of tasks, unless the list has already been retrieved by Search.
            if (result.CommandText != Command.Search)
            {
                renderedTasks = GetTasks(8);
            }

            RenderTasks();
        }

        private void HandleAutoComplete()
        {
            //AutoComplete is only defined if there are rendered tasks on screen.
            if (renderedTasks != null && renderedTasks.Count > 0)
            {
                var result = Command.Parse(inputBox.Text);
                if (result.CommandText != Command.Invalid)
                {
                    int completeBegin = LCPIndex(result.Text, renderedTasks[0].RawText);

                    if (inputBox.Text.EndsWith(result.CommandText))
                    {
                        inputBox.Text += " ";
                    }

                    if (completeBegin >= 0)
                    {
                        inputBox.Text += renderedTasks[0].RawText.Substring(completeBegin + 1);
                        MoveCursorToEndOfWord();
                    }
                }
            }
        }

        private void HandleHideWindow()
        {
            //If the input box is not empty, we clear the input box, but do not hide the window.
            //Otherwise, we hide the window.
            //If the input box contains only whitespace (which will not be caught by the first condition),
            //we clear it before hiding the window.
            if (inputBox.Text.Trim() != string.Empty)
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

        // Used for auto complete.
        private void MoveCursorToEndOfWord()
        {
            inputBox.Select(inputBox.Text.Length, 0);
        }

        //Finds the longest common prefix of a and b.
        private int LCPIndex(string a, string b)
        {
            int found = -1;
            for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
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
    }
}
