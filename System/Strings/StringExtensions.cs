namespace System;

/// <summary>
/// Commonly used string utilities. 
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Returns a string reduced to maxLength if longer than maxLength, optionally trimmed.
    /// If aString is null then returns null (as opposed to throwing a null reference exception).
    /// </summary>
    /// <param name="aString"></param>
    /// <param name="maxLength">A non-negative integer representing the maximum length of the returned string.</param>
    /// <param name="trimSpaces">Whether to trim leading and trailing whitespace from the string before truncation.</param>
    /// <returns></returns>
    public static string? Truncate(this string? aString, int maxLength, bool trimSpaces = true)
    {
        if (maxLength<0) throw new ArgumentException("maxLength must be non-negative.");
        if (aString == null) return null;
        if (trimSpaces)
        {
            aString = aString.Trim();
        }
        if (aString.Length > maxLength)
        {
            return aString.Substring(0, maxLength);
        }
        return aString;
    }
    
    /// <summary>
    /// Splits a string using the passed separator characters (, and ; by default),
    /// removing any whitespace lines, and trimming all lines.
    /// </summary>
    /// <param name="stringWithSeparators"></param>
    /// <param name="separatorChars">defaults to ";,"</param>
    /// <returns></returns>
    public static IReadOnlyList<string> SplitAndClean(this string? stringWithSeparators, string separatorChars = ",;")
    {
        if (string.IsNullOrWhiteSpace(stringWithSeparators))
        {
            return [];
        }

        return stringWithSeparators.Split(separatorChars.ToCharArray())
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
    }
    
}