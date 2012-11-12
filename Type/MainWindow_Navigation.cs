using System.Windows;

namespace Type
{
    public partial class MainWindow : Window
    {        
        //@author A0088574M
        #region Page Navigation Handlers
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

        //@author A0088574M
        #region Page Navigation Helper Methods
        //go to next page, by modifying listStartIndex and listEndIndex
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

        //go to previous page, by modifying listStartIndex and listEndIndex
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

        #region Page Number Methods
        //@author A0083834Y
        private int GetTotalPageNumber()
        {
            return (renderedTasks.Count - 1) / NUMBER_OF_TASKS_DISPLAYED + 1;
        }

        //@author A0088574M
        private int GetCurrentPageNumber()
        {
            return (listEndIndex - 1) / NUMBER_OF_TASKS_DISPLAYED + 1;
        }
        #endregion

        #region Highlight Methods
        //@author A0092104U
        private void StartHighlighting()
        {
            highlightListIndex = 0;
            isHighlighting = true;
            ResetSelection();
        }

        //@author A0092104U
        private void StopHighlighting()
        {
            isHighlighting = false;
            ResetSelection();
        }

        //@author A0092104U
        private void ResetSelection()
        {
            // We have a non-ambiguous match iff there is exactly one task rendered.
            // Otherwise, set the selectedTask to null to represent no task selected.
            selectedTask = renderedTasks.Count == 1 ? renderedTasks[0] : null;
        }

        //@author A0088574M
        //if the highLightIndex out of bound the set it back to the correct bound
        private void CheckHighlightIndexBound()
        {
            if (highlightListIndex < 0)
            {
                highlightListIndex = 0;
            }

            if (highlightListIndex > (listEndIndex - 1) % NUMBER_OF_TASKS_DISPLAYED)
            {
                highlightListIndex = (listEndIndex - 1) % NUMBER_OF_TASKS_DISPLAYED;
            }
        }
        #endregion

        //@author A0088574M
        #region Display List Index Methods
        private void InitializeListBounderIndex()
        {
            listStartIndex = 0;

            if (renderedTasks.Count > NUMBER_OF_TASKS_DISPLAYED)
            {
                listEndIndex = NUMBER_OF_TASKS_DISPLAYED;
            }
            else
            {
                listEndIndex = renderedTasks.Count;
            }
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
        #endregion
    }
}
