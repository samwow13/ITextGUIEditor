using System;
using System.Text.RegularExpressions;

namespace iTextDesignerWithGUI.Extensions
{
    /// <summary>
    /// Extension methods for string operations
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Splits a camel case string into separate words
        /// </summary>
        /// <param name="input">The camel case string to split</param>
        /// <returns>A string with spaces between words</returns>
        public static string SplitCamelCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Insert a space before each uppercase letter that follows a lowercase letter
            return Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
        }
    }
}
