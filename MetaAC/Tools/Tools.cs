using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MetaAC
{
    public static class Tools
    {
        /// <summary>
        /// Met en majuscule la première lettre de chaque mot
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string UpperFirstLetters(string text)
        {
            string upperedText = null;
            if (text != null)
            {
                upperedText = Regex.Replace(text, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
            }
            return upperedText;
        }
    }
}
