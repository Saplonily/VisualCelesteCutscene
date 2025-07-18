namespace VisualCelesteCutscene;

public static class StringExtensions
{
    public static string RemovePrefix(this string str, string prefix)
        => str.StartsWith(prefix) ? str[prefix.Length..] : str;

    public static string RemoveOneOfPrefixes(this string str, params ReadOnlySpan<string> prefixes)
    {
        foreach (string prefix in prefixes)
        {
            if (str.StartsWith(prefix))
                return str[prefix.Length..];
        }
        return str;
    }

    public static string RemoveSuffix(this string str, string suffix)
        => str.EndsWith(suffix) ? str[..^suffix.Length] : str;

    public static string RemoveOneOfSuffixes(this string str, params ReadOnlySpan<string> suffixes)
    {
        foreach (string suffix in suffixes)
        {
            if (str.EndsWith(suffix))
                return str[..^suffix.Length];
        }
        return str;
    }
}
