using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Type
{
    interface IAutoComplete
    {
        /// <summary>
        /// Completes a partial query to the longest common prefix of the result set.
        /// </summary>
        /// <param name="query">Query to run.</param>
        /// <returns>Remaining string to complete query.</returns>
        string CompleteToCommonPrefix(string str);

        /// <summary>
        /// Determines if a suggestion exists in the dictionary.
        /// </summary>
        /// <param name="suggestion">Suggestion to test.</param>
        /// <returns>A boolean</returns>
        bool ContainsSuggestion(string str);

        /// <summary>
        /// Gets an array of suggestions matching a query.
        /// </summary>
        /// <param name="query">Query to run.</param>
        /// <returns>Array of suggestions with prefix of query.</returns>
        string[] GetSuggestions(string str);
    }
}
