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
    internal delegate IList<Task> FilterSuggestionsCallback(string partialText);
    internal delegate IList<Task> ExecuteCommandCallback(string rawText, Task selectedTask);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string COMMAND_PREFIX = ":";

        private const string INPUT_WELCOME_TEXT = "start typing...";
        private const string INPUT_NOTASKS_TEXT = "no tasks.";

        private string content;

        private ExecuteCommandCallback ExecuteCommand;
        private FilterSuggestionsCallback GetFilterSuggestions;

        public MainWindow()
        {
            InitializeComponent();

            textBox1.Focus();
        }

        internal MainWindow setCallbacks(FilterSuggestionsCallback GetFilterSuggestions, ExecuteCommandCallback ExecuteCommand)
        {
            this.GetFilterSuggestions = GetFilterSuggestions;
            this.ExecuteCommand = ExecuteCommand;
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

        ////@yanrong You can decide what to do with msg based on msgCode
        //// Refreshes listbox1 to display the list of tasks 
        //private void ExecuteResultCallback(IList<Task> tasks, UIRedrawMsgCode msgCode = UIRedrawMsgCode.EMPTY, string msg = null)
        //{
        //    DisplayNoTasksText(tasks);

        //    DecideWhatToDo(msgCode, msg);

        //    listBox1.ItemsSource = tasks;
        //} 

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

        // For auto-suggesting tasks as each char is typed
        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            DisplayWelcomeText();

            //bool continueCheck = (isCommand(textBox1.Text));
           
            //if (continueCheck)
            //{   
            //    int spIndex = getSpIndex(textBox1.Text);            
            //    content = getMessage(spIndex, textBox1.Text);
            //    string[] suggestions = GetSuggestions(content);
            //    RedrawContents(suggestions);
            //}
        }

        private void MoveCursorToEndOfWord()
        {
            textBox1.Select(textBox1.Text.Length, 0);
        }

        //private string[] GetSuggestions(string input)
        //{
        //    string[] suggestions;
        //    suggestions = tasksAutoComplete.GetSuggestions(input);
        //    return suggestions;
        //}

        // Refreshes listbox1 to show the list of suggestions
        private void RedrawContents(string[] suggestions)
        {
            listBox1.ItemsSource = suggestions;
        }

        // Checks if a command is typed. 
        //private bool isCommand(string input)
        //{
        //    if (input.StartsWith(COMMAND_PREFIX))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        // Gets the index of the first whitespace. 
        //private int getSpIndex(string input)
        //{
        //    return input.IndexOf(" ");
        //}

        //private string getMessage(int spIndex, string input)
        //{
        //    return input.Substring(spIndex + 1);
        //}

        //private void DecideWhatToDo(UIRedrawMsgCode msgCode, string msg)
        //{
        //    if (msgCode == UIRedrawMsgCode.EDITED_TEXT)
        //    {
        //        textBox1.Text = msg;
        //        MoveCursorToEndOfWord();
        //    }
        //    else if(msgCode == UIRedrawMsgCode.WARNING)
        //    {
        //        popUp.IsOpen = true;
        //        textBlock1.Text = msg;
        //    }
        //    else if (msgCode == UIRedrawMsgCode.ERROR)
        //    {
        //        popUp.IsOpen = true;
        //        textBlock1.Text = msg;
        //    }
        //    else
        //    {
        //        textBox1.Clear();
        //    }
        //}

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    
                    break;

                case Key.Tab:
                    
                    break;

                case Key.Escape:
                    this.Hide();
                    break;
            }
        }

        //private Tuple<string, string> TokenizeInput(string userInput)
        //{
        //    string command;

        //    if (!userInput.StartsWith(COMMAND_PREFIX))
        //    {
        //        command = "add";
        //    }
        //    else
        //    {
        //        var spIndex = userInput.IndexOf(' ');
        //        command = userInput.Substring(1, spIndex - 1);
        //        userInput = userInput.Substring(spIndex + 1);
        //    }

        //    return new Tuple<string, string>(command, userInput);
        //}
    }
}
