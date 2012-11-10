using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Type
{
    public partial class MainWindow : Window
    {
        #region Page Navigation Handlers
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

            if ((highlightListIndex > NUMBER_OF_TASKS_DISPLAYED - 1) && (listEndIndex != renderedTasks.Count))
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
        #endregion

        #region Page Navigation Helper Methods
        //@author A0083834Y
        // Get number of pages in current task list
        private int GetPageNumber()
        {
            int pages;

            if (renderedTasks.Count < NUMBER_OF_TASKS_DISPLAYED)
            {
                pages = 1;
            }
            else if (renderedTasks.Count % NUMBER_OF_TASKS_DISPLAYED != 0)
            {
                pages = renderedTasks.Count / NUMBER_OF_TASKS_DISPLAYED + 1;
            }
            else
            {
                pages = renderedTasks.Count / NUMBER_OF_TASKS_DISPLAYED;
            }

            return pages;

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
        #endregion
    }
}
