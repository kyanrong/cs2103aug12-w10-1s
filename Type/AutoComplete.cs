using System;
using System.Collections.Generic;
using System.Linq;

namespace Type
{
    /// <summary>
    /// Provides functionality to suggest strings based on a query, and complete a partial query up to its common prefix.
    /// </summary>
    public class AutoComplete
    {
        //Initialized once and used in hash-matching LCP algorithm.
        private static HashSet<String> lcpHs;

        //Stores list of valid strings.
        private SortedSet<String> dictionary;

        /// <summary>
        /// Builds an AutoComplete object with a given dictionary of strings.
        /// </summary>
        /// <param name="dictionary">Array of valid strings to build into a dictionary.</param>
        public AutoComplete(string[] dictionary)
        {
            lcpHs = new HashSet<String>();

            this.dictionary = new SortedSet<String>();
            Load(dictionary);
        }

        private void Load(string[] dictionary)
        {
            foreach (string s in dictionary)
            {
                this.dictionary.Add(s);
            }
        }

        /// <summary>
        /// Gets the number of suggestions in the dictionary.
        /// </summary>
        public int DictionarySize
        {
            get
            {
                return dictionary.Count;
            }
        }

        /// <summary>
        /// Adds a new suggestion to the dictionary.
        /// </summary>
        /// <param name="suggestion">Suggestion to add.</param>
        public void AddSuggestion(string suggestion)
        {
            dictionary.Add(suggestion);
        }

        /// <summary>
        /// Determines if a suggestion exists in the dictionary.
        /// </summary>
        /// <param name="suggestion">Suggestion to test.</param>
        /// <returns>A boolean</returns>
        public bool ContainsSuggestion(string suggestion)
        {
            return (dictionary.Contains(suggestion));
        }

        /// <summary>
        /// Removes an existing suggestion from the dictionary.
        /// </summary>
        /// <param name="suggestion">Suggestion to remove.</param>
        public void RemoveSuggestion(string suggestion)
        {
            dictionary.Remove(suggestion);
        }

        /// <summary>
        /// Gets an array of suggestions matching a query.
        /// </summary>
        /// <param name="query">Query to run.</param>
        /// <returns>Array of suggestions with prefix of query.</returns>
        public string[] GetSuggestions(string query)
        {
            SortedSet<String> results = QueryResultSet(query);
            string[] suggestions = new string[results.Count];
            results.CopyTo(suggestions);
            return suggestions;
        }

        private SortedSet<string> QueryResultSet(string query)
        {
            var results = new SortedSet<String>();
            foreach (string str in dictionary.Where(elem => elem.StartsWith(query)))
            {
                results.Add(str);
            }
            return results;
        }

        /// <summary>
        /// Completes a partial query to the longest common prefix of the result set.
        /// </summary>
        /// <param name="query">Query to run.</param>
        /// <returns>Remaining string to complete query.</returns>
        public string CompleteToCommonPrefix(string query)
        {
            SortedSet<String> results = QueryResultSet(query);
            if (results.Count == 0)
            {
                return "";
            }
            else if (results.Count == 1)
            {
                return (results.ElementAt(0).Substring(query.Length));
            }
            else
            {
                return (FindCommonPrefix(results).Substring(query.Length));
            }
        }

        //Finds the common prefix of a set of strings.
        private string FindCommonPrefix(SortedSet<String> results)
        {
            int lcpIndex = LCPIndex(results.ElementAt(0), results.ElementAt(1));
            string substr = results.ElementAt(0).Substring(0, lcpIndex);
            for (int i = 2; i < results.Count; i++)
            {
                int newLcpIndex = LCPIndex(substr, results.ElementAt(i));
                if (newLcpIndex != lcpIndex)
                {
                    lcpIndex = newLcpIndex;
                    substr = results.ElementAt(i).Substring(0, lcpIndex);
                }
            }
            return (results.ElementAt(0).Substring(0, lcpIndex));
        }

        //Uses hashing and binary search to determine the longest common prefix of a and b.
        private int LCPIndex(string a, string b)
        {
            lcpHs.Clear();

            int low = 0;
            int high = Math.Min(a.Length, b.Length);
            int mid = 0;
            int found = -1;

            while (low <= high)
            {
                mid = (low + high) / 2;
                string subA = a.Substring(0, mid);
                string subB = b.Substring(0, mid);

                lcpHs.Add(subA);

                if (lcpHs.Contains(subB))
                {
                    found = mid;
                    low = (mid + 1);
                }
                else
                {
                    high = (mid - 1);
                }

                lcpHs.Clear();
            }

            return found;
        }
    }
}
