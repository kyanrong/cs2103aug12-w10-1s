using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Type
{
    sealed class Commands
    {
        public const string Token = ":";
        public const string Invalid = "invalid";
        public const string Add = "add";
        public const string Edit = "edit";
        public const string Done = "done";
        public const string Archive = "archive";

        public const string SearchToken = "/";
        public const string Search = "search";

        /// <summary>
        /// Parses input by splitting it into a token containing the command's text, and a token containing the rest of the input.
        /// </summary>
        /// <param name="input">Input to parse. Commands should start with the symbol defined in COMMAND_TOKEN.</param>
        /// <returns>A Tuple containing the command text and remaining input.</returns>
        public static Tuple<string, string> Parse(string input)
        {
            string cmd;
            if (input.StartsWith(Commands.Token))
            {
                int spIndex = input.IndexOf(' ');
                if (spIndex < 0)
                {
                    cmd = Commands.Invalid;
                    input = "";
                }
                else
                {
                    cmd = input.Substring(1, spIndex - 1);
                    input = input.Substring(spIndex + 1);
                }
            }
            else if (input.StartsWith(Commands.SearchToken))
            {
                cmd = Commands.Search;
                input = input.Substring(Commands.SearchToken.Length);
            }
            else
            {
                cmd = Commands.Add;
            }

            return new Tuple<string, string>(cmd, input);
        }
    }
}
