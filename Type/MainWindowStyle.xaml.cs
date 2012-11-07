using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Documents;

namespace Type
{
    public partial class MainWindow : Window
    {
        // default style for text 
        private void DefaultStyle(TextBlock textBlock)
        {
            textBlock.FontSize = 20;
            textBlock.FontFamily = new FontFamily("GillSans");
            textBlock.Padding = new Thickness(10);
        }
        
        // style for active tasks
        private void StyleTasks(TextBlock textBlock)
        {
            DefaultStyle(textBlock);
            textBlock.Margin = new Thickness(15, 0, 0, 0);
        }


        // style for "no tasks" text
        private void StyleNoTasks(TextBlock textBlock)
        {
            DefaultStyle(textBlock);
            textBlock.TextAlignment = TextAlignment.Center;
        }

        // style for completed parsed types (hash tags, datetime, priority)
        private void StyleDoneParsedTypes(Run run)
        {
            run.TextDecorations = TextDecorations.Strikethrough;
            run.FontStyle = FontStyles.Italic;
            run.Foreground = Brushes.SlateGray;
        }

        // style for hashtags (blue)
        private void StyleHashTags(Run run)
        {
            run.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x18, 0x23, 0x7f));
            run.FontWeight = FontWeights.DemiBold;
        }

        // style for datetime (red)
        private void StyleDateTime(Run run)
        {
            run.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xd7, 0x00, 0x00));
            run.FontWeight = FontWeights.DemiBold;
        }

        // style for priorityhigh (orange)
        private void StylePriorityHigh(Run run)
        {
            run.Foreground = Brushes.Gold;
            run.FontWeight = FontWeights.DemiBold;
        }

        // style for prioritylow (green)
        private void StylePriorityLow(Run run)
        {
            run.Foreground = Brushes.OliveDrab;
            run.FontWeight = FontWeights.DemiBold;
        }

        // display blue border
        private void DisplayBlueBorder(StackPanel parentStackPanel)
        {
            Line line = DrawBlueLine();
            AddStackPanel(parentStackPanel, line);
        }

        // display dashed border
        private void DisplayDashedBorder(StackPanel parentStackPanel)
        {
            Rectangle dashedLine = DrawDashedLine();
            AddStackPanel(parentStackPanel, dashedLine);
        }

        private void DisplayPageButton(StackPanel parentStackPanel, int pageNumber)
        {
            StackPanel pageButtons = new StackPanel();
            pageButtons.Orientation = Orientation.Horizontal;
            pageButtons.HorizontalAlignment = HorizontalAlignment.Center;

            for (int i = 0; i < pageNumber; i++)
            {
                Ellipse ellipse = DrawEllipse();
                pageButtons.Children.Add(ellipse);
            }

            AddStackPanel(parentStackPanel, pageButtons);
        }

        private void AddStackPanel(StackPanel parentStackPanel, Shape shape)
        {
            StackPanel border = new StackPanel();
            border.Children.Add(shape);
            parentStackPanel.Children.Add(border);
        }

        private void AddStackPanel(StackPanel parentStackPanel, StackPanel childStackPanel)
        {
            parentStackPanel.Children.Add(childStackPanel);
        }

        private Line DrawBlueLine()
        {
            Line blueLine = new Line();

            blueLine.Stroke = Brushes.SkyBlue;
            blueLine.StrokeThickness = 0.5;
            blueLine.X1 = 0;
            blueLine.Y1 = 0;
            blueLine.X2 = 484;
            blueLine.Y2 = 0;

            return blueLine;
        }

        private Rectangle DrawDashedLine()
        {
            Rectangle dashedLine = new Rectangle();

            dashedLine.Stroke = Brushes.SandyBrown;
            dashedLine.StrokeThickness = 0.5;
            dashedLine.StrokeDashArray = new DoubleCollection() { 4, 3 };
            dashedLine.Margin = new Thickness(0, 10, 0, 0);

            return dashedLine;
        }

        private Ellipse DrawEllipse()
        {
            Ellipse ellipse = new Ellipse();

            ellipse.Stroke = Brushes.Gainsboro;
            ellipse.Fill = Brushes.Gainsboro;
            ellipse.Margin = new Thickness(3, 10, 0, 0);
            ellipse.Width = 7;
            ellipse.Height = 7;
            
            return ellipse;
        }
    }
}
