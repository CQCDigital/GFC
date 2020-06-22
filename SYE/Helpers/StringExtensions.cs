using System.Text.RegularExpressions;

namespace SYE.Helpers
{
    public static class StringExtensions
    {
        public static string StripHtml(this string input)
        {
            return Regex.Replace(input, "<.*?>", " ").Trim();
        }

        public static string RemoveLineBreaks(this string input)
        {
            return input.Replace("\r\n", string.Empty);
        }
        public static string Truncate(this string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }
    }
}
