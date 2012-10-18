using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Type
{
    sealed class Command
    {
        public string CommandText { get; set; }
        public string Text { get; set; }

        public Command(string CommandText, string Text)
        {
            this.CommandText = CommandText;
            this.Text = Text;
        }

        /// <summary>
        /// Parses input by splitting it into a token containing the command's text, and a token containing the rest of the input.
        /// </summary>
        /// <param name="input">Input to parse. Commands should start with the symbol defined in COMMAND_TOKEN.</param>
        /// <returns>A Tuple containing the command text and remaining input.</returns>
        public static Command Parse(string input)
        {
            string cmd;
            if (input.StartsWith(Command.Token))
            {
                int spIndex = input.IndexOf(' ');
                if (spIndex < 0)
                {
                    cmd = Command.Invalid;
                    input = "";
                }
                else
                {
                    cmd = input.Substring(1, spIndex - 1);
                    input = input.Substring(spIndex + 1);
                }
            }
            else if (input.StartsWith(Command.SearchToken))
            {
                cmd = Command.Search;
                input = input.Substring(Command.SearchToken.Length);
            }
            else
            {
                cmd = Command.Add;
            }

            return new Command(cmd, input.Trim());
        }

        //Enumeration of accepted commands.
        public const string Token = ":";
        public const string Invalid = "invalid";
        public const string Add = "add";
        public const string Edit = "edit";
        public const string Done = "done";
        public const string Archive = "archive";
        public const string Sort = "sort";

        public const string SearchToken = "/";
        public const string Search = "search";
    }
}
