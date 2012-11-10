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
            //when not at the first page and not yet start highlighting
            if (!isHighlighting && listStartIndex != 0)
            {
                StartHighlighting();
            }

            highlightListIndex--;

            //when current highlight is at the first task
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

            //when hightlight index out of bound, and current page is not the last page
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
            }
        }

        private void HandleRightArrow()
        {
            //in case user only want to move the cursor in the text box, not the page
            //but user can still use the right key when searching for task in filter list
            if (parseResult.CommandText == Command.Search || inputBox.Text == string.Empty)
            {
                MoveToNextPage();
            }
        }
        #endregion

        #region Page Navigation Helper Methods
        //@author A0088574M
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

        //@author A0088574M
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

        #region Page Button
        //@author A0083834Y
        //Style page button if index is within range
        private void highlightPageButton()
        {
            StyleHighlightedPageButton(GetCurrentPageNumber());
        }
        #endregion

        #region Page Number
        private int GetTotalPageNumber()
        {
            return (renderedTasks.Count - 1) / NUMBER_OF_TASKS_DISPLAYED + 1;
        }

        private int GetCurrentPageNumber()
        {
            return (listEndIndex - 1) / NUMBER_OF_TASKS_DISPLAYED + 1;
        }
        #endregion
    }
}
