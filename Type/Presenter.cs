using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Type
{
    public class Presenter
    {
        //Key combination to catch.
        private const uint COMBINATION_MOD = GlobalKeyCombinationHook.MOD_SHIFT;
        private const uint COMBINATION_TRIGGER = 0x20;

        private GlobalKeyCombinationHook globalHook;
        private MainWindow ui;
        private TaskCollection tasks;
        private bool editMode;
        private Task selected;
        private Comparison<Task> comparator;
        
        public Presenter()
        {
            //Sequence is important here. Messing up the sequence may result in race conditions.
            comparator = Task.DefaultComparison;
            tasks = new TaskCollection();
            ui = new MainWindow(GetTasksWithPartialText, HandleCommand, GetTasksNoFilter, GetTasksByHashTags);
            globalHook = (new GlobalKeyCombinationHook(ui, ShowUi, COMBINATION_MOD, COMBINATION_TRIGGER)).StartListening();
        }

        ~Presenter()
        {
            //We need to unregister the hotkey when the application closes to be a good Windows citizen.
            globalHook.StopListening();
        }

        /// <summary>
        /// Displays the UI window. Called when a defined key combination is pressed.
        /// </summary>
        public void ShowUi()
        {
            ui.Show();
        }

        /// <summary>
        /// Retrieves a list of suggestions that begin with a specified prefix.
        /// </summary>
        /// <param name="partialText">Prefix to match.</param>
        /// <returns>Read-only list of suggestions as strings.</returns>
        private IList<Task> GetTasksWithPartialText(string partialText)
        {
            var resultSet = tasks.FilterAll(partialText);
            resultSet.Sort(comparator);
            return resultSet;
        }

        /// <summary>
        /// Retrieves a list of tasks to be displayed.
        /// </summary>
        /// <param name="num">Number of tasks to retrieve.</param>
        /// <returns>Read-only list of tasks.</returns>
        private IList<Task> GetTasksNoFilter(int num)
        {
            var resultSet = tasks.Get(num);
            resultSet.Sort(comparator);
            return resultSet;
        }

        /// <summary>
        /// Retrieves a list of tasks tagged with at least one hash tag.
        /// </summary>
        /// <param name="content">String containing hash tags separated by ' '.</param>
        /// <returns>Read-only list of tasks.</returns>
        private IList<Task> GetTasksByHashTags(string content)
        {
            var tags = content.Split(' ').ToList();
            
            //Prepend a hash to the tag name if it doesn't already have one.
            for (int i = 0; i < tags.Count; i++)
            {
                if (!tags[i].StartsWith("#"))
                {
                    tags[i] = "#" + tags[i];
                }
            }

            var resultSet = tasks.ByHashTags(tags);
            resultSet.Sort(comparator);
            return resultSet;
        }
        
        /// <summary>
        /// Parses a raw string and executes its command, if valid.
        /// If no valid command is found, this method does nothing.
        /// </summary>
        /// <param name="cmd">Command.</param>
        /// <param name="content">Text of the Command.</param>
        /// <param name="selected">Selected task. Throws an exception if no reference is specified, but the command requires one.</param>
        private void HandleCommand(string cmd, string content, Task selected = null)
        {
            //In edit mode, the only valid command is 'add'.
            //Otherwise, accept all commands.
            if (editMode)
            {
                EditModeSelectedTask(cmd, content);
            }
            else
            {
                Execute(cmd, content, selected);
            }
        }

        private void Execute(string cmd, string content, Task selected)
        {
            //Store a reference to the selected task in case we need to use it again in edit mode.
            this.selected = selected;

            switch (cmd)
            {
                case Command.Add:
                    tasks.Create(content);
                    break;

                case Command.Edit:
                    //The selected task is already stored. We set the editMode flag and return. The next command
                    //should be an 'add' containing the edited raw text of the selected task.
                    if (selected != null)
                    {
                        editMode = true;
                    }
                    break;

                case Command.Done:
                    if (selected != null)
                    {
                        tasks.UpdateDone(selected.Id, true);
                    }
                    break;

                case Command.Archive:
                    if (selected != null)
                    {
                        //Archive selected.
                        tasks.UpdateArchive(selected.Id, true);
                    }
                    else
                    {
                        //Archive all done.
                        tasks.ArchiveAll();
                    }
                    break;

                case Command.Undo:
                    // Call undo method of TaskCollection Obj.
                    tasks.Undo();
                    break;

                case Command.Clear:
                    tasks.Clear();
                    tasks = new TaskCollection();
                    break;
    
                default:
                    //Do nothing.
                    break;
            }
        }

        private void EditModeSelectedTask(string cmd, string content)
        {
            if (cmd == Command.Add)
            {
                //The selected task should have been previously stored on the preceeding command.
                tasks.UpdateRawText(selected.Id, content);
            }

            //If the command was not "Add", we assume the user wants to exit edit mode.

            //Escape from edit mode after this function call.
            editMode = false;
        }
    }
}
