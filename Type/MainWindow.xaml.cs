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
    internal delegate void CommandProcessor(string userInput, UIRedrawHandler redrawHandler);
    internal delegate IAutoComplete AutocompleteAccessor();

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string INPUT_WELCOME_TEXT = "start typing...";
        private const string INPUT_NOTASKS_TEXT = "no tasks.";

        private CommandProcessor ExecuteCommand;
        private IAutoComplete tasksAutoComplete;

        public MainWindow()
        {
            InitializeComponent();

            textBox1.Focus();
        }

        internal MainWindow setCallbacks(CommandProcessor cp, AutocompleteAccessor getAutoCompleteReference)
        {
            ExecuteCommand = cp;
            tasksAutoComplete = getAutoCompleteReference();
            return this;
        }

        private void DisplayWelcomeText()
        {
            if (textBox1.Text.Trim() == "")
            {
                label2.Content = INPUT_WELCOME_TEXT;
            }
            else
            {
                label2.Content = "";
            }
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
            DisplayWelcomeText();
            string[] suggestions = GetSuggestions(textBox1.Text);
            RedrawContents(suggestions);
            DisplayPopUp(suggestions);
        }

        private void MoveCursorToEndOfWord()
        {
            textBox1.Select(textBox1.Text.Length, 0);
        }

        private string[] GetSuggestions(string input)
        {
            string[] suggestions;
            suggestions = tasksAutoComplete.GetSuggestions(input);
            return suggestions;
        }

        private void RedrawContents(string[] suggestions)
        {
            listBox2.ItemsSource = suggestions;
        }

        private void DisplayPopUp(string[] suggestions)
        {
            if (suggestions.Length == 0 || textBox1.Text == "")
            {
                popUp.IsOpen = false;
            }
            else
            {
                popUp.IsOpen = true;
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
                    textBox1.Text += completedQuery;
                    MoveCursorToEndOfWord();
                    break;

                case Key.Escape:
                    this.Hide();
                    break;
            }
        }
    }
}
