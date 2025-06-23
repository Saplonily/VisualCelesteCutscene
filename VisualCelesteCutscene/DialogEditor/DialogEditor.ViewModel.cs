using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using CelesteDialog;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace VisualCelesteCutscene;

// TODO use dependency injection

// maybe we should split this file...?

public sealed partial class DialogEditorViewModel : ObservableObject, IRecipient<EntryChangedMessage>
{
    private CelesteMapMod mapMod;
    private List<string> dialogFiles;
    [AllowNull] private DialogDocument document;

    [AllowNull] private List<EntryViewModel> entries;

    // value -> ObservableCollection<DialogPageViewModel> / TranslationEntryViewModel
    [AllowNull] private Dictionary<EntryViewModel, object> modifiedEntries;

    [ObservableProperty]
    public partial string? DialogFile { get; set; }

    public IReadOnlyList<string> DialogFiles => dialogFiles;

    [AllowNull]
    [ObservableProperty]
    public partial ObservableCollection<EntryViewModel> EntriesToShow { get; private set; }

    [ObservableProperty]
    public partial string EntrySearchText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviewPlotEntryCommand))]
    public partial ObservableCollection<DialogPageViewModel>? CurrentPageViewModels { get; private set; }

    [ObservableProperty]
    public partial TranslationEntryViewModel? CurrentTranslation { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddNewPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddNewEntryCommand))]
    [NotifyCanExecuteChangedFor(nameof(DuplicateEntryCommand))]
    [NotifyCanExecuteChangedFor(nameof(RenameEntryCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteEntryCommand))]
    public partial EntryViewModel? SelectedEntry { get; set; }

    public DialogPlotPageViewModel? SelectedPlotPage => SelectedPage as DialogPlotPageViewModel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Character))]
    [NotifyPropertyChangedFor(nameof(SelectedPlotPage))]
    [NotifyCanExecuteChangedFor(nameof(DuplicatePageCommand))]
    public partial DialogPageViewModel? SelectedPage { get; set; }

    [DisallowNull]
    public string? Character
    {
        get => SelectedPlotPage?.Character;
        set
        {
            if (SelectedPlotPage is null) return;
            if (SelectedPlotPage!.Character != value)
            {
                SelectedPlotPage.Character = value;
                UpdateSubCharacters(SelectedPlotPage);
            }
        }
    }

    [ObservableProperty]
    public partial ObservableCollection<string> AvailableCharacters { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<string> AvailableSubCharacters { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveAllCommand))]
    public partial bool EntriesDirty { get; set; }

    public RelayCommand<DialogPageViewModel> MoveUpPageCommand { get; }
    public RelayCommand<DialogPageViewModel> MoveDownPageCommand { get; }
    public RelayCommand<DialogPageViewModel> DeletePageCommand { get; }
    public RelayCommand<DialogPageViewModel> PreviewPageCommand { get; }

    public RelayCommand<DialogPageViewModel> DuplicatePageCommand { get; }

    public RelayCommand AddNewPageCommand { get; }
    public RelayCommand PreviewPlotEntryCommand { get; }

    public RelayCommand AddNewEntryCommand { get; }
    public RelayCommand DuplicateEntryCommand { get; }
    public RelayCommand RenameEntryCommand { get; }
    public RelayCommand DeleteEntryCommand { get; }

    public RelayCommand SaveAllCommand { get; }

    public RelayCommand CloseCommand { get; }
    public RelayCommand ExitCommand { get; }

    public event Action? RequestClose;
    public event Action? RequestExit;

    public DialogEditorViewModel(CelesteMapMod mapMod)
    {
        entries = null;
        modifiedEntries = null;
        EntriesToShow = null;

        dialogFiles = mapMod.DialogFiles.ToList();
        this.mapMod = mapMod;
        DialogFile = dialogFiles.FirstOrDefault();

        MoveUpPageCommand = new(OnMoveUpPage!, CanMoveUpPageExecute);
        MoveDownPageCommand = new(OnMoveDownPage!, CanMoveDownPageExecute);
        PreviewPageCommand = new(OnPreviewPage, CanPreviewPageExecute);
        DeletePageCommand = new(OnDeletePage!, CanDeletePageExecute);
        DuplicatePageCommand = new(OnDuplicatePageCommand, CanDuplicatePageExecute);

        AddNewPageCommand = new(OnAddNewPage, CanAddNewPageExecute);
        PreviewPlotEntryCommand = new(OnPreviewPlotEntry, CanPreviewPlotEntryExecute);

        AddNewEntryCommand = new(OnAddNewEntry, CanActionOnEntryExecute);
        DuplicateEntryCommand = new(OnDuplicateEntry, CanActionOnEntryExecute);
        RenameEntryCommand = new(OnRenameEntry, CanActionOnEntryExecute);
        DeleteEntryCommand = new(OnDeleteEntry, CanActionOnEntryExecute);

        SaveAllCommand = new(OnSaveAll, CanSaveAllExecute);
        CloseCommand = new(() => RequestClose?.Invoke());
        ExitCommand = new(() => RequestExit?.Invoke());

        AvailableSubCharacters = null!;
        AvailableCharacters = new(App.Current.PortraitsInfoService.GetCharacters());

        App.Current.Messenger.Register(this);
    }

    private void UpdateSubCharacters(DialogPlotPageViewModel page)
        => AvailableSubCharacters = new(App.Current.PortraitsInfoService.GetSubCharacters(page.Character));

    partial void OnDialogFileChanged(string? oldValue, string? newValue)
    {
        if (newValue is null)
        {
            document = null;
            entries = null;
            modifiedEntries = null;
            return;
        }
        document = App.Current.DialogFileService.ReadFrom(Path.Combine(mapMod.FolderPath, "Dialog", newValue));
        entries = new(document.Entries.Select(p => new EntryViewModel(p.name, p.entry)));
        modifiedEntries = new();
        UpdateEntriesToShow();
    }

    partial void OnSelectedPageChanged(DialogPageViewModel? value)
    {
        DialogPlotPageViewModel? newPlotPage = value as DialogPlotPageViewModel;
        if (newPlotPage is not null)
            UpdateSubCharacters(newPlotPage);
    }

    partial void OnEntrySearchTextChanged(string value)
        => UpdateEntriesToShow();

    private void UpdateEntriesToShow()
    {
        if (entries is null)
        {
            EntriesToShow = null;
            return;
        }
        string valueTrimmed = EntrySearchText.Trim();
        if (string.IsNullOrWhiteSpace(EntrySearchText))
            EntriesToShow = new(entries);
        else
            EntriesToShow = new(entries.Where(e => e.EntryName.Contains(valueTrimmed, StringComparison.OrdinalIgnoreCase)));
    }

    partial void OnSelectedEntryChanged(EntryViewModel? value)
    {
        if (value is null)
        {
            CurrentTranslation = null;
            CurrentPageViewModels = null;
            return;
        }

        if (modifiedEntries.TryGetValue(value, out object? obj))
        {
            if (obj is ObservableCollection<DialogPageViewModel> pagesVM)
            {
                CurrentTranslation = null;
                CurrentPageViewModels = pagesVM;
                return;
            }
            else if (obj is TranslationEntryViewModel tevm)
            {
                CurrentTranslation = tevm;
                CurrentPageViewModels = null;
                return;
            }
            else
            {
                throw new UnreachableException();
            }
        }

        DialogEntry entry = value.Entry;
        if (entry is DialogPlotEntry plotEntry)
        {
            var pages = CreatePlotPageViewModels(plotEntry);
            CurrentTranslation = null;
            CurrentPageViewModels = new(pages);
            modifiedEntries[value] = CurrentPageViewModels;
            return;
        }
        DialogTranslationEntry transEntry = (DialogTranslationEntry)entry;
        CurrentPageViewModels = null;
        var newTrans = new TranslationEntryViewModel(transEntry.Translation);
        CurrentTranslation = newTrans;
        modifiedEntries[value] = newTrans;
    }

    private void CurrentPageViewModels_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ReevaluateIndex(CurrentPageViewModels!);
        MoveUpPageCommand.NotifyCanExecuteChanged();
        MoveDownPageCommand.NotifyCanExecuteChanged();
        ((IRecipient<EntryChangedMessage>)this).Receive(new());
    }

    partial void OnCurrentPageViewModelsChanged(
        ObservableCollection<DialogPageViewModel>? oldValue,
        ObservableCollection<DialogPageViewModel>? newValue
        )
    {
        if (oldValue is not null)
            oldValue.CollectionChanged -= CurrentPageViewModels_CollectionChanged;
        if (newValue is not null)
            newValue.CollectionChanged += CurrentPageViewModels_CollectionChanged;
    }

    #region page action

    private void OnMoveUpPage(DialogPageViewModel page)
    {
        int index = page.Index;
        CurrentPageViewModels!.Move(index, index - 1);
    }

    private void OnMoveDownPage(DialogPageViewModel page)
    {
        int index = page.Index;
        CurrentPageViewModels!.Move(index, index + 1);
    }

    private void OnPreviewPage(DialogPageViewModel? page)
    {
        DialogPlotPageViewModel plotPage = (DialogPlotPageViewModel)page!;
        List<DialogPlotPageViewModel> pagesToPreview = new();
        while (true)
        {
            pagesToPreview.Add(plotPage);
            if (plotPage.Index + 1 < CurrentPageViewModels!.Count)
            {
                DialogPlotPageViewModel? next = CurrentPageViewModels[plotPage.Index + 1] as DialogPlotPageViewModel;
                if (next?.InlinedToPrevious is true)
                    plotPage = next;
                else
                    break;
            }
            else
            {
                break;
            }
        }
        App.Current.PreviewService.Request(pagesToPreview);
    }

    private void OnDeletePage(DialogPageViewModel page)
    {
        var message = new RequestConfirmMessage("警告", "确定要删除此页吗？");
        bool delete = App.Current.Messenger.Send(message);
        if (delete) CurrentPageViewModels!.Remove(page);
    }

    private void OnAddNewPage()
    {
        (PageNewPosition, DialogPageType)? r = App.Current.Messenger.Send(new RequestNewPageMessage(SelectedPlotPage?.Index is not null));
        if (r is not null)
        {
            (PageNewPosition pos, DialogPageType type) = r.Value;
            if (type is not DialogPageType.Plot and not DialogPageType.InlinedPlot)
                throw new NotSupportedException();
            var page = new DialogPlotPage(new DialogPortraitState("madeline", "normal", false), string.Empty)
            {
                InlinedToPrevious = type is DialogPageType.InlinedPlot
            };
            var model = new DialogPlotPageViewModel(page);
            CurrentPageViewModels!.Insert(pos switch
            {
                PageNewPosition.Top => 0,
                PageNewPosition.Above => SelectedPlotPage!.Index,
                PageNewPosition.Below => SelectedPlotPage!.Index + 1,
                PageNewPosition.Bottom => CurrentPageViewModels.Count,
                _ => throw new ArgumentException()
            }, model);
            SelectedPage = model;
        }
    }

    private void OnDuplicatePageCommand(DialogPageViewModel? model)
    {
        var dup = model!.Clone();
        CurrentPageViewModels!.Insert(CurrentPageViewModels.IndexOf(model) + 1, dup);
    }

    private bool CanAddNewPageExecute()
        => SelectedEntry != null && CurrentPageViewModels != null;

    private bool CanMoveUpPageExecute(DialogPageViewModel? page)
        => page is not null && page.Index != 0;

    private bool CanMoveDownPageExecute(DialogPageViewModel? page)
        => page is not null && page.Index != CurrentPageViewModels!.Count - 1;

    private bool CanPreviewPageExecute(DialogPageViewModel? page)
        => page is DialogPlotPageViewModel plotPage && !plotPage.InlinedToPrevious;

    private bool CanDeletePageExecute(DialogPageViewModel? page)
        => page is not null;

    private bool CanDuplicatePageExecute(DialogPageViewModel? page)
        => page is not null;

    #endregion

    #region entry action

    private void OnPreviewPlotEntry()
        => App.Current.PreviewService.Request(CurrentPageViewModels!.OfType<DialogPlotPageViewModel>());

    private bool CanPreviewPlotEntryExecute()
        => CurrentPageViewModels is not null;

    private void OnAddNewEntry()
    {
        (bool isPlot, string entryName)? result = App.Current.Messenger.Send<RequestNewEntryMessage>();
        if (result is not null)
        {
            string entryName = result.Value.entryName;
            if (entries.Any(e => e.EntryName.Equals(entryName, StringComparison.OrdinalIgnoreCase)))
            {
                // TODO
                MessageBox.Show($"已存在项 \"{entryName}\"，项名不区分大小写。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            EntryViewModel viewModel = new EntryViewModel(
                entryName,
                result.Value.isPlot ? new DialogTranslationEntry() : new DialogPlotEntry()
                );
            viewModel.IsDirty = true;
            entries.Insert(entries.IndexOf(SelectedEntry!) + 1, viewModel);
            EntriesToShow.Insert(EntriesToShow.IndexOf(SelectedEntry!) + 1, viewModel);
            EntriesDirty = true;
        }
    }

    private void OnRenameEntry()
    {
        string originalName = SelectedEntry!.EntryName;
        string? result = App.Current.Messenger.Send(new RequestRenameMessage(originalName));
        if (result is not null)
        {
            bool collided = false;
            if (!result.Equals(result, StringComparison.OrdinalIgnoreCase))
                collided =
                    entries.Any(e => e.EntryName.Equals(result, StringComparison.OrdinalIgnoreCase)) ||
                    modifiedEntries.Keys.Any(k => k.EntryName.Equals(result, StringComparison.OrdinalIgnoreCase));

            if (collided)
            {
                MessageBox.Show($"已存在同名项 \"{result}\"。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            SelectedEntry.EntryName = result;
            SelectedEntry.IsDirty = true;
        }
    }

    private void OnDuplicateEntry()
    {
        string entryName = SelectedEntry!.EntryName;
        string duplicatedName;
        int ind = 0;
        do
        {
            ind++;
            duplicatedName = $"{entryName}_{ind}";
        }
        while (entries.Find(e => e.EntryName.Equals(duplicatedName, StringComparison.OrdinalIgnoreCase)) is not null);

        EntryViewModel newEntry = new(duplicatedName, SelectedEntry.Entry.Clone());
        newEntry.IsDirty = true;
        entries.Insert(entries.IndexOf(SelectedEntry) + 1, newEntry);
        EntriesToShow.Insert(EntriesToShow.IndexOf(SelectedEntry) + 1, newEntry);
        EntriesDirty = true;
    }

    private void OnDeleteEntry()
    {
        bool result = App.Current.Messenger.Send(new RequestConfirmMessage("警告", "确认要删除该项吗？"));
        if (!result) return;
        entries.Remove(SelectedEntry!);
        EntriesToShow.Remove(SelectedEntry!);
    }

    private bool CanActionOnEntryExecute()
        => SelectedEntry is not null;

    #endregion

    #region save action

    private void OnSaveAll()
    {
        // any better way to keep selected entry?
        var preEntry = SelectedEntry;
        object preValue = null!;
        if (preEntry is not null)
            modifiedEntries.TryGetValue(preEntry, out preValue!);

        foreach (var pair in modifiedEntries)
        {
            EntryViewModel entryViewModel = pair.Key;
            if (!entryViewModel.IsDirty) continue;
            string entryName = entryViewModel.EntryName;
            object modelCache = pair.Value;
            DialogEntry newEntry;
            if (modelCache is ObservableCollection<DialogPageViewModel> pagesVM)
            {
                newEntry = new DialogPlotEntry(pagesVM.Select(v => v.ToModel()));
            }
            else if (modelCache is TranslationEntryViewModel tevm)
            {
                newEntry = new DialogTranslationEntry(tevm.Translation);
            }
            else
            {
                throw new UnreachableException();
            }

            var model = entries[entries.IndexOf(entryViewModel)];
            model.Entry = newEntry;
            model.IsDirty = false;
        }
        EntriesDirty = false;
        modifiedEntries.Clear();
        // any better way to keep selected entry?
        if (preEntry is not null)
            modifiedEntries.Add(preEntry, preValue);
        document.Entries = entries.Select(vm => (vm.EntryName, vm.Entry)).ToList();
        using (FileStream fs = new FileStream(mapMod.GetDialogFile(DialogFile!), FileMode.Create, FileAccess.Write))
        {
            document.SaveTo(fs);
        }
    }

    private bool CanSaveAllExecute()
        => DialogFile is not null && EntriesDirty;

    #endregion

    private static void ReevaluateIndex(IList<DialogPageViewModel> pages)
    {
        for (int i = 0; i < pages.Count; i++)
            pages[i].Index = i;
    }

    private static List<DialogPageViewModel> CreatePlotPageViewModels(DialogPlotEntry plotEntry)
    {
        List<DialogPageViewModel> vms = new();
        foreach (CelesteDialog.DialogPage page in plotEntry.Pages)
        {
            DialogPageViewModel vm = page switch
            {
                DialogPlotPage plotPage => new DialogPlotPageViewModel(plotPage),
                DialogTriggerPage triggerPage => new DialogTriggerPageViewModel(triggerPage),
                _ => throw new ArgumentException("Invalid type of page.", nameof(plotEntry)),
            };
            vms.Add(vm);
        }
        ReevaluateIndex(vms);
        foreach (var vm in vms)
            vm.ListenOnChanged();
        return vms;
    }

    void IRecipient<EntryChangedMessage>.Receive(EntryChangedMessage message)
    {
        SelectedEntry!.IsDirty = true;
        EntriesDirty = true;
    }
}