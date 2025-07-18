using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace VisualCelesteCutscene;

public sealed class FileEditViewModel
{
    public string FileName { get; set; }

    public DialogEditViewModel Edit { get; set; }

    public FileEditViewModel(string fileName, DialogEditViewModel edit)
    {
        FileName = fileName;
        Edit = edit;
    }
}

// Goce => Goto Or Create Entry
public sealed class GoceMenuAction
{
    public string Content { get; set; }

    public IRelayCommand<string> Command { get; set; }

    public GoceMenuAction(string content, IRelayCommand<string> command)
    {
        Content = content;
        Command = command;
    }
}

public sealed partial class DialogEditorWindowViewModel : ObservableObject
{
    private readonly CelesteMapMod mapMod;

    [ObservableProperty]
    public partial ObservableCollection<FileEditViewModel> Edits { get; private set; }

    [ObservableProperty]
    public partial FileEditViewModel? SelectedEdit { get; set; }

    public string ModPath => mapMod.ModName;

    public IReadOnlyList<GoceMenuAction> GoceMapNameActions { get; private set; }
    public IReadOnlyList<GoceMenuAction> GoceMapPoemActions { get; private set; }
    public IReadOnlyList<GoceMenuAction> GoceMapPostCardActions { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial bool IsDirty { get; set; }

    public string Title => $"Visual Celeste Cutscene Editor {App.Current.Version}{(IsDirty ? " *" : "")}";

    public DialogEditorWindowViewModel(CelesteMapMod mapMod, IEditorDialogHost editorDialogHost)
    {
        this.mapMod = mapMod;
        var models =
            from f in mapMod.DialogFiles
            let path = mapMod.GetDialogFile(f)
            let doc = App.Current.DialogFileService.ReadFrom(path)
            select new FileEditViewModel(f, new(doc, editorDialogHost));

        Edits = new(models);
        foreach (var edit in Edits)
        {
            edit.Edit.Dirty += () =>
            {
                IsDirty = true;
                SaveSelectedCommand.NotifyCanExecuteChanged();
                SaveAllCommand.NotifyCanExecuteChanged();
            };
        }

        SelectedEdit = Edits.FirstOrDefault();

        var mapNamesList = mapMod.MapsFiles.Select(f => f.RemoveSuffix(".bin").Replace('\\', '/')).ToList();
        mapNamesList.Sort();

        var mapNamesListWithoutBC = mapNamesList.Select(n => n.RemoveOneOfSuffixes("-B", "-C")).Distinct().ToList();

        GoceMapNameActions = mapNamesListWithoutBC.Select(s => new GoceMenuAction(s, GoceMapNameCommand)).ToList();
        GoceMapPoemActions = mapNamesList.Select(s => new GoceMenuAction(s, GoceMapPoemCommand)).ToList();
        GoceMapPostCardActions = mapNamesList.Select(s => new GoceMenuAction(s, GoceMapPostcardCommand)).ToList();
    }

    [RelayCommand(CanExecute = nameof(CanSaveSelectedExecute))]
    public void SaveSelected()
    {
        ApplyAndSave(SelectedEdit!);
        if (Edits.All(fe => !fe.Edit.IsDirty))
            IsDirty = false;
    }

    [RelayCommand(CanExecute = nameof(CanSaveAllExecute))]
    public void SaveAll()
    {
        foreach (var fileEdit in Edits)
            ApplyAndSave(fileEdit);
        IsDirty = false;
    }

    [RelayCommand(CanExecute = nameof(CanGoceExecute))]
    public void GoceMapName(string mapFileName)
    {
        string key = DialogHelper.DialogKeyify(mapFileName);
        SelectedEdit!.Edit.GotoOrCreateEntryCommand.Execute(key);
    }

    [RelayCommand(CanExecute = nameof(CanGoceExecute))]
    public void GoceMapPoem(string mapFileName)
    {
        string key = DialogHelper.DialogKeyify(mapFileName);
        if (!key.EndsWith("_B") && !key.EndsWith("_C"))
            key += "_A";
        key = "poem_" + key;
        SelectedEdit!.Edit.GotoOrCreateEntryCommand.Execute(key);
    }

    [RelayCommand(CanExecute = nameof(CanGoceExecute))]
    public void GoceMapPostcard(string mapFileName)
    {
        string key = DialogHelper.DialogKeyify(mapFileName);
        key += "_postcard";
        SelectedEdit!.Edit.GotoOrCreateEntryCommand.Execute(key);
    }

    public bool CanSaveSelectedExecute()
        => SelectedEdit is not null && SelectedEdit?.Edit.IsDirty is true;

    public bool CanSaveAllExecute()
        => IsDirty;

    public bool CanGoceExecute(string mapFileName)
        => SelectedEdit is not null;

    private void ApplyAndSave(FileEditViewModel fileEdit)
    {
        var document = fileEdit.Edit.ApplyChanges();
        App.Current.DialogFileService.SaveTo(document, mapMod.GetDialogFile(fileEdit.FileName));
    }
}