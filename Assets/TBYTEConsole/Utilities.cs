using System;

namespace TBYTEConsole.Utilities
{
    public static class TBYTEConsoleExtensions
    {
        internal static void ThrowIfNull<T>(this T any, string parameterName) where T : class
        {
            if (any == null)
                throw new ArgumentNullException(parameterName);

            // Modified from ...
            // http://stackoverflow.com/questions/11522104/what-is-the-best-way-to-extend-null-check
        }
        internal static void ThrowIfNullOrEmpty(this string str, string parameterName)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentException(parameterName);
        }
    }
}