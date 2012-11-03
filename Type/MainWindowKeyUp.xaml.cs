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
                case Command.Clear:
                    DoGenericCommand(result);
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

        // @author A0092104
        private void HandleAutoComplete()
        {
            //AutoComplete is only defined if there are rendered tasks on screen.
            if (renderedTasks != null && renderedTasks.Count > 0)
            {
                var result = Command.Parse(inputBox.Text);

                // If the command is Invalid, we try to autocomplete the command.
                // Otherwise, we complete the task.
                if (result.CommandText != Command.Invalid && !result.IsAlias)
                {
                    // If the input text is just the command, we append a space so that 
                    // the user can continue typing.
                    // Otherwise, we complete the partially written task.
                    int completeBegin;
                    if (inputBox.Text.EndsWith(result.CommandText))
                    {
                        inputBox.Text += " ";
                        MoveCursorToEndOfWord();
                    }
                    else if ((completeBegin =  LCPIndex(result.Text, renderedTasks[0].RawText)) >= 0)
                    {
                        inputBox.Text += renderedTasks[0].RawText.Substring(completeBegin + 1);
                        MoveCursorToEndOfWord();
                    }
                }
                else
                {
                    inputBox.Text += Command.TryComplete(inputBox.Text.Substring(1));
                    MoveCursorToEndOfWord();
                }
            }
        }

        // @author A0092104U
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

        //modify the highlight index and may go to the previous page.
        private void HandleUpArrow()
        {
            if (!isHighlighting)
            {
                StartHighlighting();
            }
            else
            {
                highlightIndex--;
            }

            //when highlighIndex out of bound and current page is not the first page
            if (highlightIndex < 0 && listStartIndex > 0)
            {
                HandleLeftArrow();//move to previous page
                highlightIndex = (listEndIndex-1) % NUMBER_OF_TASKS_DISPLAYED;
            }

            CheckHighlightIndexBound();

            RefreshViewList();
            //inputBox.Text = selectedTaskText;
        }

        // @author A0092104
        private void StartHighlighting()
        {
            isHighlighting = true;
            ResetSelection();
        }

        // @author A0092104
        private void StopHighlighting()
        {
            isHighlighting = false;
            ResetSelection();
        }

        // @author A0092104
        private void ResetSelection()
        {
            highlightIndex = 0;

            // We have a non-ambiguous match iff there is exactly one task rendered.
            // Otherwise, set the selectedTask to null to represent no task selected.
            selectedTask = renderedTasks.Count == 1 ? renderedTasks[0] : null;
        }

        //modify the highlightIndex and may go to next page
        private void HandleDownArrow()
        {
            if (!isHighlighting)
            {
                StartHighlighting();
            }
            else
            {
                highlightIndex++;
            }

            if ((highlightIndex > NUMBER_OF_TASKS_DISPLAYED-1) && (listEndIndex != renderedTasks.Count))
            {
                HandleRightArrow();//move to next page
                highlightIndex = 0;
            }

            CheckHighlightIndexBound();

            RefreshViewList();
            //inputBox.Text = selectedTaskText;
        }

        //go to previous page, will modify listStartIndex and listEndIndex
        //may modify highlightIndex
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
            CheckHighlightIndexBound();

            ResetSelection();

            RefreshViewList();
        }

        //go to next page, will modify listStartIndex and listEndIndex
        //may modify highlightIndex
        private void HandleRightArrow()
        {
            //already at the last page, no need changes
            if (listEndIndex == renderedTasks.Count)
            {
                return;
            }

            listEndIndex += NUMBER_OF_TASKS_DISPLAYED;
            listStartIndex = listEndIndex - NUMBER_OF_TASKS_DISPLAYED;

            CheckListIndexBound();
            CheckHighlightIndexBound();

            ResetSelection();

            RefreshViewList();
        }

        //if the list index is out of bound then set it back to the correct bound
        private void CheckListIndexBound()
        {
            if (listStartIndex < 0)
            {
                listStartIndex = 0;
            }

            if (listEndIndex > renderedTasks.Count)
            {
                listEndIndex = renderedTasks.Count;
            }
        }

        //if the highLightIndex out of bound the set it back to the correct bound
        private void CheckHighlightIndexBound()
        {
            if (highlightIndex < 0)
            {
                highlightIndex = 0;
            }
            if (highlightIndex > (listEndIndex-1) % NUMBER_OF_TASKS_DISPLAYED)
            {
                highlightIndex = (listEndIndex-1) % NUMBER_OF_TASKS_DISPLAYED;
            }
        }
        
        // Used for auto complete.
        private void MoveCursorToEndOfWord()
        {
            inputBox.Select(inputBox.Text.Length, 0);
        }

        // @author A0092104
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
    }
}
