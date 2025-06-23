namespace VisualCelesteCutscene;

public static class StringExtensions
{
    public static string RemovePrefix(this string str, string prefix)
        => str.StartsWith(prefix) ? str[prefix.Length..] : str;

    public static string RemovePrefix(this string str, params ReadOnlySpan<string> prefixes)
    {
        foreach (string prefix in prefixes)
        {
            if (str.StartsWith(prefix))
                return str[prefix.Length..];
        }
        return str;
    }
}
