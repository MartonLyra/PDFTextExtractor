using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PDFTextExtractor.Util
{
    static class StringExtensions
    {
        /// <summary>
        /// Replace string with ignore case flag
        /// </summary>
        /// <param name="str">input string</param>
        /// <param name="from">search for</param>
        /// <param name="to">replace to</param>
        /// <returns>Returns replaced input string</returns>
        public static string ReplaceInsensitive(this string str, string from, string to)
        {
            str = Regex.Replace(str, Regex.Escape(from), to.Replace("$", "$$"), RegexOptions.IgnoreCase);
            return str;
        }
    }
}
