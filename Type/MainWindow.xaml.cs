﻿using System;
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
    internal delegate IAutoComplete AutoCompleteAccessor();

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string COMMAND_PREFIX = ":";

        private const string INPUT_WELCOME_TEXT = "start typing...";
        private const string INPUT_NOTASKS_TEXT = "no tasks.";

        private string content;

        private ExecuteHandler ExecuteCommand;
        private IAutoComplete tasksAutoComplete;

        public MainWindow()
        {
            InitializeComponent();

            textBox1.Focus();
        }

        internal MainWindow setCallbacks(ExecuteHandler cp, AutoCompleteAccessor getAutoCompleteReference, AutoCompleteAccessor getAcceptedCommands)
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

            bool continueCheck;

            continueCheck = (isCommand(textBox1.Text));
            int spIndex = getSpIndex(textBox1.Text);

            if (continueCheck)
            {               
                content = getMessage(spIndex, textBox1.Text);
                string[] suggestions = GetSuggestions(content);
                RedrawContents(suggestions);
            }
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

        private bool isCommand(string input)
        {
            if (input.StartsWith(COMMAND_PREFIX))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private int getSpIndex(string input)
        {
            return input.IndexOf(" ");
        }

        private string getMessage(int spIndex, string input)
        {
            return input.Substring(spIndex + 1);
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
                    string completedQuery = tasksAutoComplete.CompleteToCommonPrefix(content);
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
