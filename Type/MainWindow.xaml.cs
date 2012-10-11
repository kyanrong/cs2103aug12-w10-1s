using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Type
{
    internal delegate void ExecuteCommandHandler(string userInput, UIRedrawHandler redrawHandler);
    internal delegate IAutoComplete AutoCompleteAccessor();

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string INPUT_WELCOME_TEXT = "Start typing...";
        private const string INPUT_NOTASKS_TEXT = "no tasks.";

        private Boolean showingWelcomeText;

        private ExecuteCommandHandler ExecuteCommand;
        private IAutoComplete tasksAutoComplete;

        public MainWindow()
        {
            InitializeComponent();

            textBox1.Focus();
        }

        internal MainWindow setCallbacks(ExecuteCommandHandler cp, AutoCompleteAccessor getAutoCompleteReference)
        {
            ExecuteCommand = cp;
            tasksAutoComplete = getAutoCompleteReference();
            return this;
        }

        private void ShowWelcomeText()
        {
            if (!showingWelcomeText)
            {
                textBox1.Text = INPUT_WELCOME_TEXT;
                textBox1.Foreground = Brushes.LightGray;
            }
            showingWelcomeText = true;
        }

        private void HideWelcomeText(string input)
        {
            if (showingWelcomeText)
            {
                textBox1.Text = input;
                textBox1.Foreground = Brushes.Black;
                MoveCursorToBack();
            }
            showingWelcomeText = false;
        }

        private void MoveCursorToBack()
        {
            textBox1.Select(textBox1.Text.Length, 0);
        }

        private void RedrawContents(IList<Task> tasks)
        {
            DisplayNoTasksText(tasks);
            listBox1.ItemsSource = tasks;
        } 

        private void DisplayNoTasksText(IList<Task> tasks)
        {
            if (tasks.Count == 0)
            {
                label1.Content = INPUT_NOTASKS_TEXT;
            }
            else
            {
                label1.Content = "";
            }
        }


        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBox1.Text.Trim() == "")
            {
                ShowWelcomeText();
            }
            else
            {
                if (showingWelcomeText)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var change in e.Changes)
                    {
                        int offset = change.Offset;
                        int addedLength = change.AddedLength;
                        if (addedLength > 0)
                        {
                            sb.Append(textBox1.Text.Substring(offset, addedLength));
                        }
                    }
                    HideWelcomeText(sb.ToString());
                }
            }
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    // @yanrong Should parse and process the command here.
                    ExecuteCommand(textBox1.Text, RedrawContents);
                    textBox1.Clear();
                    break;

                case Key.Tab:
                    string completedQuery = tasksAutoComplete.CompleteToCommonPrefix(textBox1.Text);
                    textBox1.Text = completedQuery;
                    break;

                case Key.Escape:
                    this.Hide();
                    break;
            }
        }
    }
}
