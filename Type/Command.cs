using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Type
{
    sealed class Command
    {
        private static HashSet<string> acceptedCommands;

        public string CommandText { get; set; }
        public string Text { get; set; }

        public Command(string CommandText, string Text)
        {
            this.CommandText = CommandText;
            this.Text = Text;
        }

        static Command()
        {
            acceptedCommands = new HashSet<string>();
            acceptedCommands.Add(Command.Archive);
            acceptedCommands.Add(Command.Done);
            acceptedCommands.Add(Command.Edit);
            acceptedCommands.Add(Command.Undo);
            acceptedCommands.Add(Command.Help);
        }

        public static string Complete(string partial)
        {
            var result = acceptedCommands.FirstOrDefault(cmdText => cmdText.StartsWith(partial));
            return result.Substring(partial.Length);
        }

        /// <summary>
        /// Parses input by splitting it into a token containing the command's text, and a token containing the rest of the input.
        /// </summary>
        /// <param name="input">Input to parse. Commands should start with the symbol defined in COMMAND_TOKEN.</param>
        /// <returns>A Command object containing the command text and remaining input.</returns>
        public static Command Parse(string input)
        {
            string cmd;
            if (input.StartsWith(Command.Token))
            {
                input = input.Substring(Command.Token.Length);
                int spIndex = input.IndexOf(' ');
                if (spIndex > 0)
                {
                    cmd = input.Substring(0, spIndex);
                    input = input.Substring(spIndex + 1);
                }
                else
                {
                    cmd = input.Trim();
                    input = string.Empty;
                }

                if (!acceptedCommands.Contains(cmd))
                {
                    cmd = Command.Invalid;
                    input = string.Empty;
                }
            }
            else if (input.StartsWith(Command.SearchToken))
            {
                cmd = Command.Search;
                input = input.Substring(Command.SearchToken.Length);
            }
            else if (input.StartsWith(Command.Help))
            {
                cmd = Command.Help;
                input = "";
            }
            else
            {
                cmd = Command.Add;
            }

            return new Command(cmd, input.Trim());
        }

        //Tokens.
        public const string Token = ":";
        public const string SearchToken = "/";

        //Enumeration of accepted explicit commands.
        public const string Edit = "edit";
        public const string Done = "done";
        public const string Undo = "undo";
        public const string Archive = "archive";
        public const string Help = "?";

        //Enumeration of implicit commands.
        public const string Add = "add";
        public const string Invalid = "invalid";
        public const string Search = "search";
    }
}
