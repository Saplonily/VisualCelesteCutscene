using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace VisualCelesteCutscene;

public sealed class FileEditViewModel : ObservableObject
{
    public string FileName { get; set; }
    public DialogEditViewModel Edit { get; set; }

    public FileEditViewModel(string fileName, DialogEditViewModel edit)
    {
        FileName = fileName;
        Edit = edit;
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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial bool IsDirty { get; set; }

    public string Title => $"Visual Celeste Cutscene Editor {App.Current.Version}{(IsDirty ? " *" : "")}";

    public DialogEditorWindowViewModel(CelesteMapMod mapMod, IEditorDialogHost editorDialogHost)
    {
        this.mapMod = mapMod;
        var pairs =
            from f in mapMod.DialogFiles
            let path = mapMod.GetDialogFile(f)
            let doc = App.Current.DialogFileService.ReadFrom(path)
            select new FileEditViewModel(f, new(doc, editorDialogHost));

        Edits = new(pairs);
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

    public bool CanSaveSelectedExecute()
        => SelectedEdit is not null && SelectedEdit?.Edit.IsDirty is true;

    public bool CanSaveAllExecute()
        => IsDirty;

    private void ApplyAndSave(FileEditViewModel fileEdit)
    {
        var document = fileEdit.Edit.ApplyChanges();
        App.Current.DialogFileService.SaveTo(document, mapMod.GetDialogFile(fileEdit.FileName));
    }
}