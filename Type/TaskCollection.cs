using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Type
{
    public class TaskCollection
    {
        #region Constants
        private const string TAG_ARCHIVE = "#archive";
        #endregion

        #region Fields
        // Task Collection
        private List<Task> tasks;
        private DataStore dataStore;

        // Undo Stack
        private DataStore undoDataStore;
        private Stack<KeyValuePair<int, List<string>>> undoStack;
        #endregion

        #region Constructors
        //@author A0082877M
        // Constructor
        public TaskCollection()
        {
            // instantiate task models
            tasks = new List<Task>();
            
            // create data store for tasks
            dataStore = new DataStore("taskcollection.csv");
            
            // create data store for undo stack
            undoDataStore = new DataStore("undostack.csv");

            // create stack to hold undos
            undoStack = new Stack<KeyValuePair<int, List<string>>>();

            // load flat files into memory.
            // basically bootstrap task collection with data from the respective flatfiles.
            this.Fetch();
        }

        //@author A0082877M
        // Fetch Tasks from Flatfile
        private void Fetch()
        {
            // tasks
            Dictionary<int, List<string>> allRows = dataStore.Get();
            foreach (KeyValuePair<int, List<string>> entry in allRows)
            {
                int index = entry.Key;
                List<string> row = entry.Value;
                
                // create task object
                var t = new Task(row);
                t.Id = index; // set Id of task
                
                // add task object to task collection.
                tasks.Add(t);
            }

            // undo stack
            Dictionary<int, List<string>> stackitems = undoDataStore.Get();
            foreach (KeyValuePair<int, List<string>> entry in stackitems)
            {
                // push every row into the undo stack.
                undoStack.Push(entry);
            }
        }
        #endregion

        #region Undo Related Methods
        //Enumeration of undo types
        public const string UndoAdd = "add";
        public const string UndoEdit = "edit";
        public const string UndoDone = "done";
        public const string UndoDoneAll = "doneall";
        public const string UndoArchive = "archive";
        public const string UndoArchiveAll = "archiveall";

        //@author A0082877M
        public void Undo()
        {
            // check if stack is empty
            if (undoStack.Count == 0)
            {
                // noop.
                return;
            }

            // pop last item off the stack
            var undoItem = undoStack.Pop();

            // undo state
            List<string> undoItemData = undoItem.Value;
            string cmd = undoItemData[0];

            int index;
            switch (cmd)
            {
                case UndoAdd:
                    index = int.Parse(undoItemData[1]);
                    
                    // delete task
                    this.Delete(index);
                    
                    break;

                case UndoEdit:
                    index = int.Parse(undoItemData[1]);
                    string rawText = undoItemData[2];
                    
                    // edit task back
                    this.UpdateRawText(index, rawText, false);

                    break;

                case UndoDone:
                    index = int.Parse(undoItemData[1]);
                    bool done = bool.Parse(undoItemData[2]);
                    
                    // update done state of task
                    this.UpdateDone(index, !done, false);

                    break;
                case UndoDoneAll:
                    for (int i = 1; i < undoItemData.Count; i++)
                    {
                        index = int.Parse(undoItemData[i]);

                        // update archived state of task
                        this.UpdateDone(index, false, false);
                    }
                    break;

                case UndoArchive:
                    index = int.Parse(undoItemData[1]);
                    bool archive = bool.Parse(undoItemData[2]);

                    // update archived state of task
                    this.UpdateArchive(index, !archive, false);

                    break;

                case UndoArchiveAll:
                    for (int i = 1; i < undoItemData.Count; i++)
                    {
                        index = int.Parse(undoItemData[i]);

                        // update archived state of task
                        this.UpdateArchive(index, false, false);
                    }
                    break;
            }

            // remove item from flatfile.
            undoDataStore.DeleteRow(undoItem.Key);
        }
    
        //@author A0082877M
        // push action and its relevent data to the undo stack.
        private void PushUndo(string cmd, Task task, List<Task> tasks = null)
        {
            List<string> item = new List<string>();
            // first string in each stack item is the cmd type.
            item.Add(cmd);
            
            switch (cmd)
            {
                case UndoAdd:
                    item.Add(task.Id.ToString());
                    break;

                case UndoEdit:
                    item.Add(task.Id.ToString());
                    item.Add(task.RawText);
                    break;

                case UndoDone:
                    item.Add(task.Id.ToString());
                    item.Add(task.Done.ToString());
                    break;

                case UndoDoneAll:
                    foreach (var t in tasks)
                    {
                        item.Add(t.Id.ToString());
                    }
                    break;

                case UndoArchive:
                    item.Add(task.Id.ToString());
                    item.Add(task.Archive.ToString());
                    break;

                case UndoArchiveAll:
                    foreach (var t in tasks)
                    {
                        item.Add(t.Id.ToString());
                    }
                    break;
            }

            this.PersistUndo(item);
        }

        //@author A0082877M
        // saves stack item to file and updates stack of task collection.
        private void PersistUndo(List<string> item)
        {
            // add row to flatfile
            var id = undoDataStore.InsertRow(item);

            // add to undo stack
            undoStack.Push(new KeyValuePair<int, List<string>>(id, item));
        }
        #endregion

        #region User Actions
        //@author A0082877M
        // Create Task
        public Task Create(string input)
        {
            Task t = new Task(input);
            tasks.Add(t);

            // returns list of strings for storage 
            var row = t.ToRow();

            // save to datastore
            int taskId = dataStore.InsertRow(row);

            // assign id to task
            t.Id = taskId;

            // add action to undo stack
            this.PushUndo(UndoAdd, t);
            
            // return task
            return t;
        }
        
        //@author A0082877M
        // Update rawText
        public Task UpdateRawText(int id, string str, bool addToUndoStack = true)
        {
            Task t = this.GetTask(id);

            if (addToUndoStack)
            {
                // add action to undo stack
                Task clone = t.Clone();
                clone.Id = t.Id;
                this.PushUndo(UndoEdit, clone);
            }

            // update lastMod
            t.lastMod = DateTime.Today;
            t.RawText = str;

            // change row in datastore
            List<string> row = t.ToRow();
            dataStore.ChangeRow(t.Id, row);

            return t;
        }

        //@author A0082877M
        // Update done
        public Task UpdateDone(int id, bool done, bool addToUndoStack = true)
        {
            Task t = this.GetTask(id);
            t.Done = done;

            // change row in datastore
            List<string> row = t.ToRow();
            dataStore.ChangeRow(t.Id, row);

            if (addToUndoStack)
            {
                // add action to undo stack
                this.PushUndo(UndoDone, t);
            }

            return t;
        }

        //@author A0082877M
        // Update archive
        public Task UpdateArchive(int id, bool archiveStatus, bool addToUndoStack = true)
        {
            Task t = this.GetTask(id);
            InternalMarkArchive(archiveStatus, t);

            // change row in datastore
            List<string> row = t.ToRow();
            dataStore.ChangeRow(t.Id, row);

            if (addToUndoStack)
            {
                // add action to undo stack
                this.PushUndo(UndoArchive, t);
            }

            return t;
        }

        //@author A0082877M
        // Marks all done tasks as archived
        public void ArchiveAll()
        {
            var changed = new List<Task>();
            foreach (var t in tasks)
            {
                if (t.Done && !t.Archive)
                {
                    InternalMarkArchive(true, t);

                    // change row in datastore
                    List<string> row = t.ToRow();
                    dataStore.ChangeRow(t.Id, row);

                    changed.Add(t);
                }
            }

            // add action to undo stack
            this.PushUndo(UndoArchiveAll, null, changed);
        }

        //@author A0092104U
        // Archives all Tasks that contain all listed Hash Tags.
        public void ArchiveAllByHashTags(IList<string> hashTags)
        {
            Debug.Assert(hashTags != null);

            var affected = new List<Task>();
            foreach (var task in tasks)
            {
                if (HashTagsMatch(task, hashTags))
                {
                    affected.Add(task);
                    this.UpdateArchive(task.Id, true, false);
                }
            }

            this.PushUndo(UndoArchiveAll, null, affected);
        }

        //@author A0092104U
        // Marks all Tasks that contain all listed Hash Tags as Done.
        public void UpdateDoneByHashTags(IList<string> hashTags)
        {
            Debug.Assert(hashTags != null);

            var affected = new List<Task>();
            foreach (var task in tasks)
            {
                if (HashTagsMatch(task, hashTags))
                {
                    affected.Add(task);
                    this.UpdateDone(task.Id, true, false);
                }
            }

            this.PushUndo(UndoDoneAll, null, affected);
        }

        //@author A0083834Y
        public void Clear()
        {
            dataStore.ClearFile("taskcollection.csv");
            undoDataStore.ClearFile("undostack.csv");
        }

        //@author A0082877M
        // Get number of Tasks starting from skip
        public List<Task> GetNotArchiveTasks()
        {
            // only return pending tasks
            List<Task> pending = tasks.FindAll(
                task => task.Archive == false
            );

            return pending;
        }

        //@author A0082877M
        // Filter All
        public List<Task> FilterAll(string input)
        {
            return tasks.FindAll(
                task =>
                    task.Archive == false &&
                    task.RawText.StartsWith(input)
            );
        }

        //@author A0092104U
        /// <summary>
        /// Filters the current TaskCollection by hash tags. Specifying more than one hash tag takes the intersection of results. (logical AND)
        /// </summary>
        /// <param name="hashTags">List of hash tags to filter by.</param>
        /// <returns>List of Tasks that match criteria.</returns>
        public List<Task> GetTasksByHashTags(IList<string> hashTags)
        {
            var resultSet = new HashSet<Task>();

            // If the list is empty, return an empty result set.
            // Otherwise, run an initial query. If there is exactly one item in the list, return the result of the initial query.
            // Otherwise, continue running additional queries and taking their intersections. Finally, return the result set.
            if (hashTags.Count == 0)
            {
                return resultSet.ToList();
            }
            else
            {
                var initialResult = tasks.FindAll(task => task.Tags.Contains(hashTags[0]));
                resultSet.UnionWith(initialResult);

                if (hashTags.Count == 1)
                {
                    return resultSet.ToList();
                }
                else
                {
                    for (int i = 1; i < hashTags.Count; i++)
                    {
                        var tag = hashTags[i];
                        var queryResults = tasks.FindAll(task => task.Tags.Contains(tag));
                        resultSet.IntersectWith(queryResults);
                    }

                    return resultSet.ToList();
                }
            }
        }

        //@author A0082877M
        public void ReparseAll()
        {
            foreach (var t in tasks)
            {
                var clone = t.Clone();
                t.Parse();

                // check if differ
                if (t.RawText != clone.RawText)
                {
                    // change row in datastore
                    List<string> row = t.ToRow();
                    dataStore.ChangeRow(t.Id, row);
                }
            }
        }
        #endregion

        #region Helper Methods
        //@author A0082877M
        // Delete Task (used in undo.)
        private void Delete(int index)
        {
            // delete in collection
            tasks.Remove(this.GetTask(index));

            // delete in data store.
            dataStore.DeleteRow(index);
        }

        //@author A0082877M
        // Get Task
        private Task GetTask(int id)
        {
            return tasks.Find(task => task.Id == id);
        }

        //@author A0092104U
        // Changes the archive status of a task.
        // If the task is archived, we append the hashtag #archive so that it shows up in searches.
        // If the task is unarchived, we remove all hashtags #archive to return it to its original state.
        private void InternalMarkArchive(bool archiveStatus, Task t)
        {
            string pattern = " " + TAG_ARCHIVE;
            if (!t.RawText.Contains(TAG_ARCHIVE))
            {
                if (archiveStatus)
                {
                    this.UpdateRawText(t.Id, (t.RawText + pattern), false);
                }
            }
            else
            {
                if (!archiveStatus)
                {
                    while (t.RawText.Contains(TAG_ARCHIVE))
                    {
                        this.UpdateRawText(t.Id, t.RawText.Replace(pattern, ""), false);
                    }
                }
            }

            t.Archive = archiveStatus;
        }

        //@author A0092104U
        // Checks if 'task' contains all the hash tags in 'hashTags'.
        private static bool HashTagsMatch(Task task, IList<string> hashTags)
        {
            bool matchCriteria = true;
            for (int i = 0; i < hashTags.Count && matchCriteria; i++)
            {
                if (!task.Tags.Contains(hashTags[i]))
                {
                    matchCriteria = false;
                }
            }
            return matchCriteria;
        }
        #endregion
    }
}