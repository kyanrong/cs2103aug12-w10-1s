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
                case Command.Undo:
                case Command.Edit:
                    DoOther(result);
                    break;
                    
                default:
                    break;
            }

            //Retrieve a list of tasks, unless the list has already been retrieved by Search.
            if (result.CommandText != Command.Search)
            {
                renderedTasks = GetTasks(NUMBER_OF_TASKS_LOADED);
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
                else
                {
                    inputBox.Text += Command.Complete(inputBox.Text.Substring(1));
                    MoveCursorToEndOfWord();
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

        //update highlight index and may navigate to previous page
        private void HandleUpArrow()
        {
            highlightIndex--;
            if (highlightIndex < 0 && listStartIndex > 0)
            {
                highlightIndex = NUMBER_OF_TASKS_DISPLAYED-1;
                listStartIndex -= NUMBER_OF_TASKS_DISPLAYED ;
                if (listStartIndex < 0)
                {
                    listStartIndex = 0;
                    highlightIndex = listStartIndex;                 
                }
                listEndIndex = listStartIndex + NUMBER_OF_TASKS_DISPLAYED;
            }
            if (highlightIndex < 0)
            {
                highlightIndex = 0;
            }
            RefreshViewList();
        }

        //update highlight index and may navigate to next page
        private void HandleDownArrow()
        {
            highlightIndex++;
            if ((highlightIndex > NUMBER_OF_TASKS_DISPLAYED-1) && (listEndIndex != renderedTasks.Count))
            {
                listStartIndex += NUMBER_OF_TASKS_DISPLAYED;
                listEndIndex = listStartIndex + NUMBER_OF_TASKS_DISPLAYED;
                highlightIndex = 0;
                if (listEndIndex >= renderedTasks.Count)
                {
                    highlightIndex += (listEndIndex - renderedTasks.Count);
                    listEndIndex = renderedTasks.Count;
                }
                listStartIndex = listEndIndex - NUMBER_OF_TASKS_DISPLAYED;
            }
            if (highlightIndex > NUMBER_OF_TASKS_DISPLAYED-1)
            {
                highlightIndex = NUMBER_OF_TASKS_DISPLAYED-1;
            }
            RefreshViewList();
        }

        //go to previous page
        private void HandleLeftArrow()
        {
            //already at the first page, no need changes
            if (listStartIndex == 0)
            {
                return;
            }
            listStartIndex -= NUMBER_OF_TASKS_DISPLAYED;            
            listEndIndex = listStartIndex + NUMBER_OF_TASKS_DISPLAYED;
            CheckListIndexBound();
            RefreshViewList();
        }
        //go to next page

        private void HandleRightArrow()
        {
            //already at the last page, no need changes
            if (listEndIndex == renderedTasks.Count)
            {
                return;
            }
            listEndIndex += NUMBER_OF_TASKS_DISPLAYED;
            listStartIndex = listEndIndex - NUMBER_OF_TASKS_DISPLAYED;

            if (CheckListIndexBound() && (highlightIndex > listEndIndex % NUMBER_OF_TASKS_DISPLAYED - 1))
            {
                highlightIndex = listEndIndex % NUMBER_OF_TASKS_DISPLAYED - 1;
            }
            RefreshViewList();
        }

        //if the list index is out of bound then set to the correct bound
        private bool CheckListIndexBound()
        {
            bool isChanged = false;
            if (listStartIndex < 0)
            {
                listStartIndex = 0;
                isChanged = true;
            }
            if (listEndIndex > renderedTasks.Count)
            {
                listEndIndex = renderedTasks.Count;
                isChanged = true;
            }
            return isChanged;
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
