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
        #region Drawing
        //@author A0083834Y
        // Used for auto complete.
        private void MoveCursorToEndOfWord()
        {
            inputBox.Select(inputBox.Text.Length, 0);
        }

        //@author A0092104U
        /// <summary>
        /// Forces the UI to update its task list and redraw.
        /// </summary>
        public void ForceRedraw()
        {
            if (inputBox.Dispatcher.CheckAccess())
            {
                // Force a UI Redraw by changing the text of the input box.
                var originalState = inputBox.Text;
                inputBox.Clear();
                inputBox.Text = originalState;
            }
            else
            {
                inputBox.Dispatcher.Invoke(new ForceRedrawCallback(ForceRedraw));
            }
        }

        //@author A0083834Y
        // If task list is not empty
        private void DisplayNonEmptyViewList()
        {
            TextBlock text = new TextBlock();

            // Display blue border above text
            DisplayBlueBorder(taskView);

            // Loop over each task and create task view
            // Append each to tasks grid
            for (int i = listStartIndex; i < listEndIndex; i++)
            {
                text = taskTextBlockList[i];

                // Highlight target textbox                    
                if ((i == highlightListIndex + listStartIndex) && isHighlighting)
                {
                    text.Background = new SolidColorBrush(Color.FromArgb(255, 230, 243, 244));

                    selectedTask = renderedTasks[i];

                    text.TextWrapping = TextWrapping.Wrap;
                }
                else
                {
                    text.Background = Brushes.White;

                    text.TextWrapping = TextWrapping.NoWrap;
                }

                taskView.Children.Add(text);

                // Display blue border below text
                DisplayBlueBorder(taskView);
            }
        }

        // If task list is empty
        private void DisplayEmptyViewList()
        {
            TextBlock text = new TextBlock();

            DisplayBlueBorder(tasksGrid);

            text.Text = TEXT_NOTASKS;

            StyleNoTasks(text);

            taskView.Children.Add(text);
            DisplayBlueBorder(taskView);
        }

        // Generate list of text block
        private void RenderTasks()
        {
            StopHighlighting();

            taskTextBlockList.Clear();
            InitializeListBounderIndex();

            RenderTasksDecorations();

            RefreshViewList();
        }

        //@author A0083834Y
        // Input Label
        private void DisplayInputLabel()
        {
            if (inputBox.Text.Trim() == "")
            {
                inputBoxLabel.Content = TEXT_WELCOME;
            }
            else
            {
                inputBoxLabel.Content = "";
            }
        }

        // Render List of Tasks.
        private void RefreshViewList()
        {
            taskView.Children.Clear();
            tasksGrid.Children.Clear();

            if (renderedTasks.Count == 0)
            {
                DisplayEmptyViewList();
            }
            else
            {
                DisplayNonEmptyViewList();
            }

            // Append task view to grid view
            tasksGrid.Children.Add(taskView);

            // Display page buttons
            int pages = GetPageNumber();
            DisplayPageButton(tasksGrid, pages);
            highlightPageButton();

            DisplayDashedBorder(tasksGrid);
        }

        //@author A0083834Y
        // Default style for text 
        private void DefaultStyle(TextBlock textBlock)
        {
            textBlock.FontSize = 20;
            textBlock.FontFamily = new FontFamily("GillSans");
            textBlock.Padding = new Thickness(10);
        }

        // Style for active tasks
        private void StyleTasks(TextBlock textBlock)
        {
            DefaultStyle(textBlock);
            textBlock.Margin = new Thickness(15, 0, 0, 0);
        }

        // Style for "no tasks" text
        private void StyleNoTasks(TextBlock textBlock)
        {
            DefaultStyle(textBlock);
            textBlock.TextAlignment = TextAlignment.Center;
        }

        // Style for completed parsed types (hash tags, datetime, priority)
        private void StyleDoneParsedTypes(Run run)
        {
            run.TextDecorations = TextDecorations.Strikethrough;
            run.FontStyle = FontStyles.Italic;
            run.Foreground = new SolidColorBrush(Color.FromArgb(255, 155, 157, 164));
        }

        // Style for hashtags (blue)
        private void StyleHashTags(Run run)
        {
            run.Foreground = new SolidColorBrush(Color.FromArgb(255, 68, 0, 150));
            run.FontWeight = FontWeights.DemiBold;
        }

        // Style for datetime (red)
        private void StyleDateTime(Run run)
        {
            run.Foreground = new SolidColorBrush(Color.FromArgb(255, 244, 0, 0));
            run.FontWeight = FontWeights.DemiBold;
        }

        // Style for priorityhigh (orange)
        private void StylePriorityHigh(Run run)
        {
            run.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 118, 20));
            run.FontWeight = FontWeights.DemiBold;
        }

        // Style for prioritylow (green)
        private void StylePriorityLow(Run run)
        {
            run.Foreground = new SolidColorBrush(Color.FromArgb(255, 152, 163, 62));
            run.FontWeight = FontWeights.DemiBold;
        }

        // Style for highlighted page button 
        private void StyleHighlightedPageButton(int index)
        {
            Ellipse ellipse = pageButtonArray[index];
            ellipse.Fill = new SolidColorBrush(Color.FromArgb(255, 89, 81, 70));
        }

        // Display blue border after each task
        private void DisplayBlueBorder(StackPanel parentStackPanel)
        {
            Line line = DrawBlueLine();
            AddStackPanel(parentStackPanel, line);
        }

        // Display dashed border at the end of current page
        private void DisplayDashedBorder(StackPanel parentStackPanel)
        {
            Rectangle dashedLine = DrawDashedLine();
            AddStackPanel(parentStackPanel, dashedLine);
        }

        // Display page button (gray)
        private void DisplayPageButton(StackPanel parentStackPanel, int pageNumber)
        {
            StackPanel pageButtons = new StackPanel();
            pageButtons.Orientation = Orientation.Horizontal;
            pageButtons.HorizontalAlignment = HorizontalAlignment.Center;

            for (int i = 1; i < pageNumber + 1; i++)
            {
                pageButtonArray[i] = DrawEllipse();
                pageButtons.Children.Add(pageButtonArray[i]);
            }

            AddStackPanel(parentStackPanel, pageButtons);
        }

        // Append shape to parent stackpanel
        private void AddStackPanel(StackPanel parentStackPanel, Shape shape)
        {
            StackPanel border = new StackPanel();
            border.Children.Add(shape);
            parentStackPanel.Children.Add(border);
        }

        // Append child stackpanel to parent stackpanel (for page buttons)
        private void AddStackPanel(StackPanel parentStackPanel, StackPanel childStackPanel)
        {
            parentStackPanel.Children.Add(childStackPanel);
        }

        private Line DrawBlueLine()
        {
            Line blueLine = new Line();

            blueLine.Stroke = new SolidColorBrush(Color.FromArgb(255, 128, 182, 248));
            blueLine.StrokeThickness = 0.7;
            blueLine.X1 = 0;
            blueLine.Y1 = 0;
            blueLine.X2 = 484;
            blueLine.Y2 = 0;

            return blueLine;
        }

        private Rectangle DrawDashedLine()
        {
            Rectangle dashedLine = new Rectangle();

            dashedLine.Stroke = new SolidColorBrush(Color.FromArgb(255, 197, 143, 57));
            dashedLine.StrokeThickness = 0.5;
            dashedLine.StrokeDashArray = new DoubleCollection() { 4, 3 };
            dashedLine.Margin = new Thickness(0, 10, 0, 0);

            return dashedLine;
        }

        // For page buttons
        private Ellipse DrawEllipse()
        {
            Ellipse ellipse = new Ellipse();

            ellipse.Stroke = new SolidColorBrush(Color.FromArgb(255, 212, 202, 190));
            ellipse.Fill = new SolidColorBrush(Color.FromArgb(255, 212, 202, 190));
            ellipse.Margin = new Thickness(3, 10, 0, 0);
            ellipse.Width = 7;
            ellipse.Height = 7;

            return ellipse;
        }

        private void RenderTasksDecorations()
        {
            for (int i = 0; i < renderedTasks.Count; i++)
            {
                TextBlock text = new TextBlock();

                // Style tokens within the textblock
                foreach (Tuple<string, Task.ParsedType> tuple in renderedTasks[i].Tokens)
                {
                    Run run = new Run(tuple.Item1);
                    // Style Runs
                    if (renderedTasks[i].Done)
                    {
                        StyleDoneParsedTypes(run);
                    }
                    else
                    {
                        if (tuple.Item2 == Task.ParsedType.HashTag)
                        {
                            StyleHashTags(run);
                        }

                        // Style Dates
                        if (tuple.Item2 == Task.ParsedType.DateTime)
                        {
                            StyleDateTime(run);
                        }

                        // Style PriorityHigh
                        if (tuple.Item2 == Task.ParsedType.PriorityHigh)
                        {
                            StylePriorityHigh(run);
                        }

                        // Style PriorityLow
                        if (tuple.Item2 == Task.ParsedType.PriorityLow)
                        {
                            StylePriorityLow(run);
                        }
                    }

                    text.Inlines.Add(run);
                }

                StyleTasks(text);
                taskTextBlockList.Add(text);
            }
        }
        #endregion
    }
}
