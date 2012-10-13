using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Type
{
    interface IAutoComplete
    {
        string CompleteToCommonPrefix(string str);
        bool ContainsSuggestion(string str);
        string[] GetSuggestions(string str);
    }
}
