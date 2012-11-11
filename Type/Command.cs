﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Type
{
    //@author A0092104U
    public sealed class Command
    {
        #region Fields
        private static HashSet<string> acceptedCommands;

        public string CommandText { get; set; }
        public string Text { get; set; }
        public bool IsAlias { get; set; }
        #endregion

        #region Methods
        public Command(string CommandText, string Text, bool IsAlias)
        {
            Debug.Assert(!CommandText.StartsWith(Command.Token));

            this.CommandText = CommandText;
            this.Text = Text;
            this.IsAlias = IsAlias;
        }
        #endregion

        #region Static Methods
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

            // The completion of the empty string is invalid.
            if (partial == string.Empty)
            {
                return partial;
            }

            var result = acceptedCommands.FirstOrDefault(cmdText => cmdText.StartsWith(partial));

            // If there are no results, we return an empty completion.
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

            bool isAlias = false;
            bool checkedAlias = false;
            string cmd;
            // Handle Type's command prefix.
            if (input.StartsWith(Command.Token))
            {
                input = RemoveCommandToken(input);

                // Split given command into a pair of <Command, Input>
                cmd = SplitCommand(ref input);

                // If the command is initially invalid, try to complete it.
                if (!acceptedCommands.Contains(cmd))
                {
                    cmd += Command.TryComplete(cmd);
                    checkedAlias = true;
                }

                // Check if still invalid.
                if (!acceptedCommands.Contains(cmd))
                {
                    cmd = Command.Invalid;
                    input = string.Empty;
                }
                else
                {
                    isAlias = checkedAlias;

                    // If valid, handle special cases:
                    if (cmd == Command.Help)
                    {
                        input = string.Empty;
                    }
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

            return new Command(cmd, input.Trim(), isAlias);
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
        #endregion

        #region Static Enumerations
        //Tokens.
        public const string Token = ":";
        public const string SearchToken = "/";
        public const string HashToken = "#";

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
        #endregion
    }
}
