using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CelesteDialog;

/// <summary>
/// A document of Celeste Dialog that contains a list of <see cref="DialogEntry"/>.
/// </summary>
public sealed class DialogDocument
{
    private readonly static Regex commandRegex = new Regex(@"[\[\{](.*?)[\}\]]", RegexOptions.RightToLeft | RegexOptions.Compiled);
    private readonly static Regex portraitRegex = new Regex(@"\[(?<content>[^\[\\]*(?:\\.[^\]\\]*)*)\]", RegexOptions.IgnoreCase);

    public List<(string name, DialogEntry entry)> Entries { get; set; }

    public DialogDocument()
    {
        Entries = new();
    }

    public DialogDocument(Stream stream)
        => Parse(stream);

    public void SaveTo(Stream stream)
    {
        using StreamWriter sw = new(stream, Encoding.UTF8, leaveOpen: true);
        sw.Write("# Generated with Visual Celeste Dialog Editor\n# VisualCelesteCutscene v");
        sw.WriteLine(Assembly.GetEntryAssembly()!.GetName().Version);
        foreach (var (name, entry) in Entries)
        {
            sw.WriteLine();
            sw.WriteLine();
            sw.Write(name);
            sw.Write('=');
            if (entry is DialogTranslationEntry tse)
            {
                if (tse.Translation.Contains('\n'))
                    sw.WriteLine();
                sw.Write(tse.Translation);
            }
            else if (entry is DialogPlotEntry pe)
            {
                sw.WriteLine();
                bool firstPage = true;
                DialogPortraitState? ps = null;
                foreach (var page in pe.Pages)
                {
                    if (page is DialogTriggerPage tp)
                    {
                        if (!firstPage)
                        {
                            sw.WriteLine();
                            sw.WriteLine();
                        }
                        sw.Write('{');
                        sw.Write(tp.TriggerName);
                        sw.Write(' ');
                        sw.Write(string.Join(' ', tp.Arguments));
                        sw.Write('}');
                        ps = null;
                    }
                    else if (page is DialogPlotPage pp)
                    {
                        if (!pp.InlinedToPrevious)
                        {
                            if (pp.Portrait != ps)
                            {
                                if (!firstPage)
                                {
                                    sw.WriteLine();
                                    sw.WriteLine();
                                }
                                WritePortrait(sw, pp.Portrait);
                            }
                            sw.WriteLine();
                            sw.Write(pp.Text);
                        }
                        else
                        {
                            WritePortrait(sw, pp.Portrait);
                            sw.Write(pp.Text);
                        }
                        ps = pp.Portrait;
                    }
                    else
                    {
                        throw new UnreachableException();
                    }
                    firstPage = false;
                }
            }
            else
            {
                throw new UnreachableException();
            }
        }

        static void WritePortrait(StreamWriter sw, DialogPortraitState ps)
        {
            sw.Write('[');
            sw.Write(ps.Character);

            sw.Write(' ');
            sw.Write(ps.AtRight ? "right" : "left");

            sw.Write(' ');
            sw.Write(ps.SubCharacter);

            if (ps.Flip)
            {
                sw.Write(' ');
                sw.Write("flip");
            }

            if (ps.UpsideDown)
            {
                sw.Write(' ');
                sw.Write("upsidedown");
            }

            if (ps.Pop)
            {
                sw.Write(' ');
                sw.Write("pop");
            }

            sw.Write(']');
        }
    }

    // is there anyone wanna optimize this method?
    [MemberNotNull(nameof(Entries))]
    private void Parse(Stream stream)
    {
        Entries = new();
        using StreamReader sr = new(stream, Encoding.UTF8);
        int lineIndex = -1;
        string? line;
        string? currentKey = null;
        bool lastLineNeedBreak = false;
        StringBuilder valueBuilder = new(64);
        while (true)
        {
            lineIndex++;
            line = sr.ReadLine();
            if (line is null) break;

            var lineSpan = line.AsSpan();

            lineSpan = lineSpan.Trim();

            if (lineSpan.Length == 0) continue;
            if (lineSpan.StartsWith(['#'])) continue;

            int equalsCharPos = lineSpan.IndexOf('=');
            bool asInitPart;
            string key = string.Empty;
            if (equalsCharPos is not -1)
            {
                asInitPart = true;
                key = lineSpan[..equalsCharPos].ToString();
                if (!key.All(c => char.IsLetterOrDigit(c) || c is '_'))
                    asInitPart = false;
            }
            else
            {
                asInitPart = false;
            }

            if (asInitPart)
            {
                if (currentKey is not null)
                    AddEntry(Entries, currentKey, valueBuilder.ToString());
                
                currentKey = key;
                valueBuilder.Clear();
                lastLineNeedBreak = false;

                if (equalsCharPos + 1 < lineSpan.Length)
                {
                    valueBuilder.Append((lineSpan[(equalsCharPos + 1)..]).Trim());
                }
            }
            else
            {
                if (currentKey is null)
                    throw new FormatException($"Missing initial '=' in line {lineIndex + 1} of dialog file.");
                if (lastLineNeedBreak)
                {
                    lastLineNeedBreak = false;
                    valueBuilder.Append("{break}");
                }
                valueBuilder.Append(lineSpan.Trim());
                if (lineSpan.Length > 0 &&
                    !lineSpan.EndsWith("{break}") &&
                    !lineSpan.EndsWith("{n}") &&
                    commandRegex.Replace(lineSpan.ToString(), string.Empty).Length > 0)
                {
                    lastLineNeedBreak = true;
                }
            }
        }

        if (valueBuilder.Length > 0)
            AddEntry(Entries, currentKey!, valueBuilder.ToString());

        static void AddEntry(List<(string, DialogEntry)> list, string key, string text)
        {
            if (TryParsePlot(text, out var plotEntry))
                list.Add((key, plotEntry));
            else
                list.Add((key, new DialogTranslationEntry(text.Replace("{n}", "\n").Replace("{break}", "\n"))));
        }
    }

    // is there anyone wanna optimize this method?
    private static bool TryParsePlot(string rawText, [NotNullWhen(true)] out DialogPlotEntry? entry)
    {
        entry = null;

        if (!rawText.Contains('['))
            return false;

        List<DialogPage> pages = new();

        string text = portraitRegex.Replace(rawText, "{portrait ${content}}");

        DialogPortraitState ps = new();
        int currentIndex = 0;
        StringBuilder pageTextSB = new(32);
        bool lastPlotPageNoBreak = false;
        while (currentIndex < text.Length)
        {
            int openBraceIndex = text.IndexOf('{', currentIndex);
            if (openBraceIndex == -1)
            {
                if (currentIndex < text.Length)
                {
                    pageTextSB.Append(text[currentIndex..]);
                    pages.Add(new DialogPlotPage(ps.Clone(), pageTextSB.ToString(), lastPlotPageNoBreak));
                    pageTextSB.Clear();
                    lastPlotPageNoBreak = false;
                }
                break;
            }

            if (openBraceIndex > currentIndex)
            {
                pageTextSB.Append(text[currentIndex..openBraceIndex]);
            }

            int closeBraceIndex = text.IndexOf('}', openBraceIndex);

            if (closeBraceIndex == -1)
            {
                pages.Add(new DialogPlotPage(ps.Clone(), text[openBraceIndex..]));
                break;
            }

            string triggerContent = text[(openBraceIndex + 1)..closeBraceIndex];
            var triggerPage = ParseTrigger(triggerContent);
            if (triggerPage is not null)
            {
                if (triggerPage.TriggerName is "portrait")
                {
                    if (pageTextSB.Length != 0)
                    {
                        pages.Add(new DialogPlotPage(ps.Clone(), pageTextSB.ToString(), lastPlotPageNoBreak));
                        pageTextSB.Clear();
                        lastPlotPageNoBreak = true;
                    }
                    bool anchorBottom = ps.AnchorBottom;
                    ps.Reset();
                    ps.AnchorBottom = anchorBottom;
                    string character = triggerPage.Arguments.Count > 0 ? triggerPage.Arguments[0] : "";
                    ps.Character = character;
                    if (triggerPage.Arguments.Count >= 3)
                    {
                        string arg1 = triggerPage.Arguments[1];
                        string arg2 = triggerPage.Arguments[2];
                        if (arg1 is "left" or "right")
                            ps.SubCharacter = arg2;
                        else
                            ps.SubCharacter = arg1;

                        foreach (var arg in triggerPage.Arguments)
                        {
                            switch (arg)
                            {
                            case "right": ps.AtRight = true; break;
                            case "flip": ps.Flip = true; break;
                            case "upsidedown": ps.UpsideDown = true; break;
                            case "pop": ps.Pop = true; break;
                            }
                        }
                    }
                }
                else if (triggerPage.TriggerName is "anchor")
                {
                    ps.AnchorBottom = triggerPage.Arguments[0] is "bottom";
                }
                else if (triggerPage.TriggerName is "break")
                {
                    pages.Add(new DialogPlotPage(ps.Clone(), pageTextSB.ToString(), lastPlotPageNoBreak));
                    pageTextSB.Clear();
                    lastPlotPageNoBreak = false;
                }
                else if (triggerPage.TriggerName is "trigger" or "silent_trigger")
                {
                    pages.Add(triggerPage);
                }
                else
                {
                    pageTextSB.Append(text[openBraceIndex..(closeBraceIndex + 1)]);
                }
            }
            else
            {
                pages.Add(new DialogPlotPage(ps.Clone(), text[currentIndex..openBraceIndex]));
            }

            currentIndex = closeBraceIndex + 1;
        }

        if (pageTextSB.Length != 0)
        {
            pages.Add(new DialogPlotPage(ps, pageTextSB.ToString(), lastPlotPageNoBreak));
            pageTextSB.Clear();
        }

        entry = new(pages);
        return true;

        static DialogTriggerPage? ParseTrigger(string triggerContent)
        {
            string[] parts = triggerContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 0)
            {
                string triggerName = parts[0];
                List<string> parameters = [];
                for (int i = 1; i < parts.Length; i++)
                    parameters.Add(parts[i]);
                return new DialogTriggerPage(triggerName, parameters);
            }
            return null;
        }
    }
}