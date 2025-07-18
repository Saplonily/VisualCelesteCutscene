using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace VisualCelesteCutscene;

public record struct PortraitImageInfo(string Character, string SubCharacter);

public class PortraitImageInfoEqualityComparer : IEqualityComparer<PortraitImageInfo>
{
    public bool Equals(PortraitImageInfo x, PortraitImageInfo y)
        => x.Character.Equals(y.Character, StringComparison.OrdinalIgnoreCase) &&
           x.SubCharacter.Equals(y.SubCharacter, StringComparison.OrdinalIgnoreCase);

    public int GetHashCode([DisallowNull] PortraitImageInfo obj)
        => obj.Character.GetHashCode(StringComparison.OrdinalIgnoreCase) ^
           obj.SubCharacter.GetHashCode(StringComparison.OrdinalIgnoreCase);
}

public sealed class PortraitsImageService
{
    private readonly List<string> imageSearchPaths;
    private readonly Dictionary<PortraitImageInfo, string> imagePaths;
    private readonly Dictionary<PortraitImageInfo, BitmapImage> cachedImages;
    private readonly Dictionary<string, HashSet<string>> charToSubChars;

    public PortraitsImageService()
    {
        var comparer = new PortraitImageInfoEqualityComparer();
        imageSearchPaths = new();
        cachedImages = new(comparer);
        imagePaths = new(comparer);
        charToSubChars = new(StringComparer.OrdinalIgnoreCase);
    }

    public void AddPortraitsSource(string portraitsXmlFile)
    {
        XmlDocument xdoc = new();
        xdoc.Load(portraitsXmlFile);
        var nodes = xdoc.LastChild!.ChildNodes;
        foreach (XmlNode node in nodes)
        {
            string characterName = node.Name.RemovePrefix("portrait_");
            string? copySource = node.Attributes!["copy"]?.InnerText?.RemovePrefix("portrait_");
            if (copySource is not null)
            {
                charToSubChars[characterName] = charToSubChars[copySource].ToHashSet();
                List<KeyValuePair<PortraitImageInfo, string>> toAdd = new();
                foreach (var info in imagePaths)
                {
                    if (info.Key.Character.Equals(copySource, StringComparison.OrdinalIgnoreCase))
                        toAdd.Add(new(new PortraitImageInfo(characterName, info.Key.SubCharacter), info.Value));
                }
                foreach (var info in toAdd)
                    imagePaths.Add(info.Key, info.Value);
            }

            string basePath = node.Attributes!["path"]!.InnerText;
            foreach (XmlNode subNode in node)
            {
                string animType = subNode.Name;
                if (animType is not "Loop")
                    continue;
                string subCharacterName = subNode.Attributes!["id"]!.InnerText
                    .RemoveOneOfPrefixes("idle_", "talk_", "begin_");
                string subPath = subNode.Attributes!["path"]!.InnerText;
                imagePaths[new(characterName, subCharacterName)] = basePath + subPath + "00.png";
                if (charToSubChars.TryGetValue(characterName, out var list))
                    list.Add(subCharacterName);
                else
                    charToSubChars[characterName] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { subCharacterName };
            }
        }
    }

    public void AddImageSearchPath(string path)
    {
        imageSearchPaths.Add(path);
    }

    public ImageSource? GetImageSourceFor(string character, string subcharacter)
    {
        var info = new PortraitImageInfo(character, subcharacter);
        if (cachedImages.TryGetValue(info, out BitmapImage? bitmapImage))
            return bitmapImage;

        if (!imagePaths.TryGetValue(info, out string? subPath))
            return null;

        foreach (var path in imageSearchPaths)
        {
            string fullPath = Path.Combine(path, subPath);
            if (File.Exists(fullPath))
            {
                var image = new BitmapImage(new Uri(Path.GetFullPath(fullPath)));
                cachedImages[info] = image;
                return image;
            }
        }

        return null;
    }

    public IEnumerable<string> GetCharacters()
        => charToSubChars.Keys;

    public IEnumerable<string> GetSubCharacters(string character)
    {
        if (charToSubChars.TryGetValue(character, out var subs))
            return subs;
        else
            return [];
    }
}
