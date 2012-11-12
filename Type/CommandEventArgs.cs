using System;

namespace Type
{
    //@author A0092104U
    public class CommandEventArgs : EventArgs
    {
        public string Command { get; set; }
        public string Content { get; set; }
        public Task SelectedTask { get; set; }

        public CommandEventArgs(string cmd, string content, Task selectedTask = null)
        {
            Command = cmd;
            Content = content;
            SelectedTask = selectedTask;
        }
    }
}
