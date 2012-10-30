using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Type
{
    sealed class Command
    {
        private static HashSet<string> acceptedCommands;

        public string CommandText { get; set; }
        public string Text { get; set; }

        public Command(string CommandText, string Text)
        {
            Debug.Assert(!CommandText.StartsWith(Command.Token));

            this.CommandText = CommandText;
            this.Text = Text;
        }

        // Initialize acceptedCommands map.
        static Command()
        {
            acceptedCommands = new HashSet<string>();
            acceptedCommands.Add(Command.Archive);
            acceptedCommands.Add(Command.Done);
            acceptedCommands.Add(Command.Edit);
            acceptedCommands.Add(Command.Undo);
            acceptedCommands.Add(Command.Help);
            acceptedCommands.Add(Command.Clear);

            // Ensure the hash table contains exactly 6 commands.
            Debug.Assert(acceptedCommands.Count == 6);
        }

        /// <summary>
        /// Tries to auto complete a command based on the supplied prefix.
        /// </summary>
        /// <param name="partial">Prefix to match.</param>
        /// <returns>Full text of matched command, excluding token, if matched. Otherwise, empty string.</returns>
        public static string TryComplete(string partial)
        {
            Debug.Assert(partial != null);

            var result = acceptedCommands.FirstOrDefault(cmdText => cmdText.StartsWith(partial));

            if (result == null)
            {
                return string.Empty;
            }
            else
            {
                return result.Substring(partial.Length);
            }
        }

        /// <summary>
        /// Parses input by splitting it into a token containing the command's text, and a token containing the rest of the input.
        /// </summary>
        /// <param name="input">Input to parse. Commands should start with the symbol defined in COMMAND_TOKEN.</param>
        /// <returns>A Command object containing the command text and remaining input.</returns>
        public static Command Parse(string input)
        {
            Debug.Assert(input != null);

            string cmd;
            // Handle Type's command prefix.
            if (input.StartsWith(Command.Token))
            {
                input = RemoveCommandToken(input);

                // Split given command into a pair of <Command, Input>
                cmd = SplitCommand(ref input);

                // First check if invalid.
                if (!acceptedCommands.Contains(cmd))
                {
                    cmd = Command.Invalid;
                    input = string.Empty;
                }
                // If valid, handle special cases:
                else if (cmd == Command.Help)
                {
                    input = string.Empty;
                }
            }
            // Handle special command prefixes.
            else if (input.StartsWith(Command.SearchToken))
            {
                cmd = Command.Search;
                input = input.Substring(Command.SearchToken.Length);
            }
            else if (input.StartsWith(Command.HelpToken))
            {
                cmd = Command.Help;
                input = string.Empty;
            }
            // The default command is 'Add'.
            else
            {
                cmd = Command.Add;
            }

            return new Command(cmd, input.Trim());
        }

        private static string SplitCommand(ref string input)
        {
            string cmd;
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
            return cmd;
        }

        private static string RemoveCommandToken(string input)
        {
            input = input.Substring(Command.Token.Length);
            return input;
        }

        //Tokens.
        public const string Token = ":";
        public const string SearchToken = "/";

        //Enumeration of accepted explicit commands.
        public const string Edit = "edit";
        public const string Done = "done";
        public const string Undo = "undo";
        public const string Archive = "archive";
        public const string HelpToken = "?";
        public const string Help = "help";
        public const string Clear = "clear";

        //Enumeration of implicit commands.
        public const string Add = "add";
        public const string Invalid = "invalid";
        public const string Search = "search";
    }
}
