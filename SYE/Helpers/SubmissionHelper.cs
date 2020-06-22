using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SYE.Helpers
{
    public static class SubmissionHelper
    {
        /// <summary>
        /// converts a list of strings to a list of SelectListItems
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<SelectItem> ConvertList(IEnumerable<string> source)
        {
            var returnList = source?.Select(x => new SelectItem {Text = x, Selected = false}).ToList();

            return returnList;
        }        
        public static string GenerateUserRef(string string1, string string2, int numChars)
        {
            var s1 = string.Empty;
            var s2 = string.Empty;
            var separator = "-";

            if (string.IsNullOrWhiteSpace(string1))
            {
                return string.Empty;
            }
            else
            {
                if (string1.Length <= numChars)
                {
                    s1 = string1;
                }
                else
                {
                    s1 = string1.Substring(string1.Length - (numChars), numChars);
                }
            }

            if (string.IsNullOrWhiteSpace(string2))
            {
                return string.Empty;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(string2))
                {
                    if (string2.Length <= numChars)
                    {
                        s2 = string2;
                    }
                    else
                    {
                        s2 = string2.Substring(string2.Length - (numChars), numChars);
                    }
                }

            }

            var returnString = s1 + separator + s2;

            return returnString.ToUpper();
        }

    }

}
