using System.Text.RegularExpressions;

namespace VisualCelesteCutscene;

// written by gemini 2.5 flash
// (and refactored by saplonily)
// (not that easy to refactor tough)

public static class SurroundingHelper
{
    /// <summary>
    /// Determines if a string contains a matching "{L param}" pattern to the left of the specified index
    /// and a "{R}" pattern to the right.
    /// </summary>
    /// <param name="input">The string to search within.</param>
    /// <param name="index">The index in the string to check around. The "{L param}" pattern is sought to the left,
    /// and the "{R}" pattern to the right of this index.</param>
    /// <param name="left">The string representing the 'L' part of the left-side pattern (e.g., in "{L param}").</param>
    /// <param name="right">The string representing the 'R' part of the right-side pattern (e.g., in "{R}").</param>
    /// <returns>
    /// <see langword="true"/> if both a matching "{L param}" (or "{L}") to the left and "{R}" to the right are found;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool HasMatchingLR(string input, int index, string left, string right)
    {
        if (index < 0 || index >= input.Length)
            return false;

        string escapedL = Regex.Escape(left);
        string lPattern = @"\{" + escapedL + @"\s*(?:[^}]*)?\}";

        string escapedR = Regex.Escape(right);
        string rPattern = @"\{" + escapedR + @"\}";

        Match lMatch = Regex.Match(input[..index], lPattern, RegexOptions.RightToLeft);
        Match rMatch = Regex.Match(input[index..], rPattern);

        return lMatch.Success && rMatch.Success;
    }

    /// <summary>
    /// Removes a matching "{L param}" pattern to the left of the specified index
    /// and a "{R}" pattern to the right from the input string.
    /// </summary>
    /// <param name="input">The string from which to remove the patterns.</param>
    /// <param name="index">The index in the string used as a reference point.
    /// The "{L param}" pattern is sought to the left, and the "{R}" pattern to the right of this index.</param>
    /// <param name="left">The string representing the 'L' part of the left-side pattern (e.g., in "{L param}").</param>
    /// <param name="right">The string representing the 'R' part of the right-side pattern (e.g., in "{R}").</param>
    /// <returns>
    /// The modified string with the "{L param}" and "{R}" patterns removed if found;
    /// otherwise, the original string is returned.
    /// </returns>
    public static string RemoveMatchingLR(string input, int index, string left, string right, out int leftLength)
    {
        leftLength = 0;
        if (index < 0 || index >= input.Length)
            return input;

        string escapedL = Regex.Escape(left);
        string lPattern = @"\{" + escapedL + @"\s*(?:[^}]*)?\}";

        string escapedR = Regex.Escape(right);
        string rPattern = @"\{" + escapedR + @"\}";

        Match lMatch = Regex.Match(input[..index], lPattern, RegexOptions.RightToLeft);
        Match rMatch = Regex.Match(input[index..], rPattern);

        if (lMatch.Success && rMatch.Success)
        {
            int lStartIndex = lMatch.Index;
            int lEndIndex = lMatch.Index + lMatch.Length;

            int rStartIndex = index + rMatch.Index;
            int rEndIndex = index + rMatch.Index + rMatch.Length;

            string result = string.Concat(
                input.AsSpan(0, lStartIndex),
                input.AsSpan(lEndIndex, rStartIndex - lEndIndex),
                input.AsSpan(rEndIndex)
                );
            leftLength = lMatch.Length;
            return result;
        }

        return input;
    }
}