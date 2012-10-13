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
    public delegate IList<Task> FilterSuggestionsCallback(string partialText);
    public delegate void ExecuteCommandCallback(string rawText, Task selectedTask);
    public delegate IList<Task> GetTasksCallback(int num);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string COMMAND_PREFIX = ":";

        private const string INPUT_WELCOME_TEXT = "start typing...";
        private const string INPUT_NOTASKS_TEXT = "no tasks.";

        private ExecuteCommandCallback ExecuteCommand;
        private FilterSuggestionsCallback GetFilterSuggestions;
        private GetTasksCallback GetTasks;

        private IList<Task> renderedTasks;
        private bool editState;
        private int editIndex;

        public MainWindow(FilterSuggestionsCallback GetFilterSuggestions, ExecuteCommandCallback ExecuteCommand, GetTasksCallback GetTasks)
        {
            InitializeComponent();

            this.GetFilterSuggestions = GetFilterSuggestions;
            this.ExecuteCommand = ExecuteCommand;
            this.GetTasks = GetTasks;

            // focus cursor in input box
            inputBox.Focus();

            // display input label
            DisplayInputLabel();

            // bootstrap tasks
            // TODO. abstract this number.
            renderedTasks = GetTasks(8);
            RenderTasks();
        }

        // Input Label
        private void DisplayInputLabel()
        {
            if (inputBox.Text.Trim() == "")
            {
                inputBoxLabel.Content = INPUT_WELCOME_TEXT;
            }
            else
            {
                inputBoxLabel.Content = "";
            }
        }

        // Render List of Tasks
        private void RenderTasks()
        {
            tasksGrid.Children.Clear();
            if (renderedTasks.Count == 0)
            {
                // display no tasks text.
                StackPanel noTasksText = new StackPanel();

                TextBlock text = new TextBlock();
                text.Text = INPUT_NOTASKS_TEXT;

                text.TextAlignment = TextAlignment.Center;
                text.FontSize = 20;

                noTasksText.Children.Add(text);

                // Append to tasksgrid.
                tasksGrid.Children.Add(noTasksText);
            }
            else
            {
                // loop over each task and create task view
                // append each to tasks grid
                foreach (Task task in renderedTasks)
                {
                    // create single stacked panel w/ info
                    StackPanel taskView = new StackPanel();
                    TextBlock text = new TextBlock();
                    text.Text = task.RawText;

                    // style accordingly
                    if (task.Done)
                    {
                        text.TextDecorations = TextDecorations.Strikethrough;
                    }

                    text.FontSize = 20;
                    taskView.Children.Add(text);

                    // append task view to grid view
                    tasksGrid.Children.Add(taskView);
                }
            }
        }

        // Event Listener when Input Box text changes.
        private void InputBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            DisplayInputLabel();

            // TODO.

            // display filtered tasks
            IList<Task> filtered = GetFilterSuggestions(inputBox.Text);
            if (filtered != null)
            {
                renderedTasks = filtered;
                RenderTasks();
            }
        }

        // Used for auto complete.
        private void MoveCursorToEndOfWord()
        {
            inputBox.Select(inputBox.Text.Length, 0);
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

        // Event Listener, onKeyUp Input Box
        private void InputBoxKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    // get input text
                    string inputText = inputBox.Text;

                    if (inputText.StartsWith(COMMAND_PREFIX))
                    {
                        if (renderedTasks.Count != 0)
                        {
                            Task selectedTask = renderedTasks[0];
                            ExecuteCommand(inputBox.Text, selectedTask);
                        }
                    }
                    else
                    {
                        // add command
                        ExecuteCommand(inputBox.Text, null);
                    }
                    // render tasks
                    renderedTasks = GetTasks(8);
                    RenderTasks();

                    // clear input box
                    inputBox.Clear();

                    break;

                case Key.Tab:
                    // TODO
                    // autocomplete
                    break;

                case Key.Escape:
                    // Hide window
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
