using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualCelesteCutscene;

public sealed class CelesteMapMod
{
    private readonly List<string> mapsFiles;
    private readonly List<string> dialogFiles;

    public string FolderPath { get; private set; }

    public string ModName { get; private set; }

    public IReadOnlyList<string> MapsFiles => mapsFiles;

    public IReadOnlyList<string> DialogFiles => dialogFiles;

    private CelesteMapMod(string folderPath, string modName, List<string> mapsFiles, List<string> dialogFiles)
    {
        FolderPath = folderPath;
        ModName = modName;
        this.mapsFiles = mapsFiles;
        this.dialogFiles = dialogFiles;
    }

    public static CelesteMapMod? ReadFrom(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            return null;
        string yamlPath = Path.Combine(folderPath, "everest.yaml");
        if (!File.Exists(yamlPath))
            return null;
        string? modName = null;
        using (FileStream fs = new(yamlPath, FileMode.Open, FileAccess.Read))
        using (StreamReader sr = new(fs))
        {
            string? line;
            while ((line = sr.ReadLine()) is not null)
            {
                int index = line.IndexOf("Name:");
                if (index != -1)
                {
                    modName = line[(index + "Name:".Length)..].Trim();
                    break;
                }
            }
        }
        if (modName is null)
            return null;

        string dialogFolder = Path.Combine(folderPath, "Dialog");
        string mapsFolder = Path.Combine(folderPath, "Maps");
        string vccMetaPath = Path.Combine(folderPath, ".vcc_meta");

        List<string> dialogFiles =
            Directory.EnumerateFiles(dialogFolder)
            .Select(p => Path.GetRelativePath(dialogFolder, p))
            .ToList();
        List<string> mapsFiles =
            Directory.EnumerateFiles(mapsFolder, "*.bin", SearchOption.AllDirectories)
            .Select(p => Path.GetRelativePath(mapsFolder, p))
            .ToList();

        return new CelesteMapMod(folderPath, modName, mapsFiles, dialogFiles);
    }

    public string GetDialogFile(string dialogFileName)
        => Path.Combine(FolderPath, "Dialog", dialogFileName);
}
