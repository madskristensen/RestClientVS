using System;

namespace RestClient
{
    public static class StringExtensions
    {
        /// <summary>
        /// Performs culture-invariant, case insensitive comparison to see if a string
        /// is a match for the supplied token string. The test string is trimmed of
        /// spaces first.
        /// </summary>
        public static bool IsTokenMatch(this string input, string token) =>
            string.Compare(input.Trim(), token, StringComparison.InvariantCultureIgnoreCase) == 0;

        /// <summary>
        /// Returns the first token from a list of tokens with the specified separator.
        /// This is used mainly to fetch the MIME type from content-type headers.
        /// </summary>
        public static string GetFirstToken(this string input, char separator = ';') =>
            input.Split(separator)[0];
    }
}
