using System.IO;

namespace VisualCelesteCutscene;

public static class PathValidator
{
    public static string ValidateCelesteGamePath(string value)
    {
        if (!Directory.Exists(value))
            return "指定的目录不存在";
        else if (!Directory.Exists(Path.Combine(value, "Content")))
            return "指定的目录不是有效的蔚蓝游戏根目录";
        else
            return string.Empty;
    }

    public static string ValidateCelesteGraphicsDumpPath(string value)
    {
        if (!Directory.Exists(value))
            return "指定的目录不存在";
        else if (!Directory.Exists(Path.Combine(value, "Gameplay")))
            return "指定的目录不是有效的 Dump 目录";
        else
            return string.Empty;
    }
}