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
            // Parse input.
            //var parseResult = Command.Parse(inputBox.Text);

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
                renderedTasks = GetTasks(NUMBER_OF_TASKS_LOADED);
            }

            RenderTasks();
        }

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

        private void StartHighlighting()
        {
            highlightListIndex = 0;
            isHighlighting = true;
            ResetSelection();
        }

        private void StopHighlighting()
        {
            isHighlighting = false;
            ResetSelection();
        }

        private void ResetSelection()
        {
            // We have a non-ambiguous match iff there is exactly one task rendered.
            // Otherwise, set the selectedTask to null to represent no task selected.
            selectedTask = renderedTasks.Count == 1 ? renderedTasks[0] : null;
        }

        //@author A0088574M
        //modify the highlight index and may go to the previous page.
        private void HandleUpArrow()
        {
            if (!isHighlighting && listStartIndex != 0)
            {
                StartHighlighting();
            }
            
            highlightListIndex--;

            if (highlightListIndex < 0 && listStartIndex == 0)
            {
                StopHighlighting();
            }

            //when highlighIndex out of bound and current page is not the first page
            if (highlightListIndex < 0 && listStartIndex > 0)
            {
                MoveToPreviousPage();
                highlightListIndex = (listEndIndex - 1) % NUMBER_OF_TASKS_DISPLAYED;
            }

            CheckHighlightIndexBound();

            RefreshViewList();
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
                highlightListIndex++;
            }

            if ((highlightListIndex > NUMBER_OF_TASKS_DISPLAYED-1) && (listEndIndex != renderedTasks.Count))
            {
                MoveToNextPage();
                highlightListIndex = 0;
            }

            CheckHighlightIndexBound();

            RefreshViewList();
        }

        private void HandleLeftArrow()
        {
            //in case user only want to move the cursor in the text box, not the page
            //but user can still use the left key when searching for task in filter list
            if (parseResult.CommandText == Command.Search || inputBox.Text == string.Empty)
            {
                MoveToPreviousPage();

                int tempIndex = highlightPageIndex - 1;
                highlightPageButton(tempIndex);
            }
        }

        private void HandleRightArrow()
        {
            //in case user only want to move the cursor in the text box, not the page
            //but user can still use the right key when searching for task in filter list
            if (parseResult.CommandText == Command.Search || inputBox.Text == string.Empty)
            {
                MoveToNextPage();
                
                int tempIndex = highlightPageIndex + 1;
                highlightPageButton(tempIndex);
            }
        }

        //@author A0083834Y
        //Check if highlightPageIndex will be within the min and max page number
        private bool isWithinPageRange(int tempIndex)
        {
            int pages = GetPageNumber();

            if (tempIndex > 0 && tempIndex < pages + 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //@author A0083834Y
        //Style page button if index is within range
        private void highlightPageButton(int tempIndex)
        {
            if (isWithinPageRange(tempIndex))
            {
                highlightPageIndex = tempIndex;
                StyleHighlightedPageButton(highlightPageIndex);
            }
        }

        
        //go to next page, will modify listStartIndex and listEndIndex
        //may modify highlightIndex
        private void MoveToNextPage()
        {
            //already at the last page
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

        //go to previous page, will modify listStartIndex and listEndIndex
        //may modify highlightIndex
        private void MoveToPreviousPage()
        {
            //already at the first page
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
            if (highlightListIndex < 0)
            {
                highlightListIndex = 0;
            }
            if (highlightListIndex > (listEndIndex-1) % NUMBER_OF_TASKS_DISPLAYED)
            {
                highlightListIndex = (listEndIndex-1) % NUMBER_OF_TASKS_DISPLAYED;
            }
        }

        //@author A0083834Y
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
