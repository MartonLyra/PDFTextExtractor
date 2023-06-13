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

        /// <summary>
        /// String equals with ignore case flag
        /// </summary>
        /// <param name="source"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        public static bool EqualsIgnoreCase(this string source, string comp)
        {
            return String.Equals(source, comp, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// String contains with ignore case flag
        /// </summary>
        /// <param name="source"></param>
        /// <param name="cont"></param>
        /// <returns></returns>
        public static bool ContainsIgnoreCase(this string source, string cont)
        {
            return source.IndexOf(cont, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }
    }
}
