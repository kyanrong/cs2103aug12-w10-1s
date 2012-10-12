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
    internal delegate void ExecuteHandler(string command, string content, UIRedrawHandler redrawHandler);
    internal delegate IAutoComplete AutocompleteAccessor();

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string COMMAND_PREFIX = ":";

        private const string INPUT_WELCOME_TEXT = "start typing...";
        private const string INPUT_NOTASKS_TEXT = "no tasks.";

        private ExecuteHandler ExecuteCommand;
        private IAutoComplete tasksAutoComplete;

        public MainWindow()
        {
            InitializeComponent();

            textBox1.Focus();
        }

        internal MainWindow setCallbacks(ExecuteHandler cp, AutocompleteAccessor getAutoCompleteReference)
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

        //@yanrong You can decide what to do with msg based on msgCode
        private void ExecuteResultCallback(IList<Task> tasks, UIRedrawMsgCode msgCode = UIRedrawMsgCode.EMPTY, string msg = null)
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
            listBox1.ItemsSource = suggestions;
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    //@yanrong Should parse and process the command here.
                    var tokenizeResult = TokenizeInput(textBox1.Text);
                    ExecuteCommand(tokenizeResult.Item1, tokenizeResult.Item2, ExecuteResultCallback);
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

        //@yanrong The functions below were moved from the controller.
        private Tuple<string, string> TokenizeInput(string userInput)
        {
            string command;

            if (!userInput.StartsWith(COMMAND_PREFIX))
            {
                command = "add";
            }
            else
            {
                var spIndex = userInput.IndexOf(' ');
                command = userInput.Substring(1, spIndex - 1);
                userInput = userInput.Substring(spIndex + 1);
            }

            return new Tuple<string, string>(command, userInput);
        }
    }
}
