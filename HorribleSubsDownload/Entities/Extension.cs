using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HorribleSubsDownload.Entities
{
    public static class Extension
    {
        public static string ReplaceSpecialCharacters(this string input)
        {
            string result = "";
            Regex namePattern = new Regex(@"\w+");
            MatchCollection nameMatch = namePattern.Matches(input);
            foreach (var match in nameMatch)
            {
                result += match.ToString().ToUpper();
            }
            return result.Trim();
        }
    }
}
