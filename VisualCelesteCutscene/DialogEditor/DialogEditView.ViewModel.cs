using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CelesteDialog;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace VisualCelesteCutscene;

// TODO should we use dependency injection?

// TODO maybe we could split this file...?

public sealed partial class DialogEditViewModel : ObservableObject
{
    private readonly IEditorDialogHost editorDialogHost;

    private readonly DialogDocument document;
    private readonly List<EntryViewModel> entries;

    private Dictionary<EntryViewModel, EntryEditViewModel> entryEdits;

    [ObservableProperty]
    public partial string EntrySearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<EntryViewModel> EntriesToShow { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedTranslationEntryEdit))]
    [NotifyPropertyChangedFor(nameof(SelectedPlotEntryEdit))]
    [NotifyCanExecuteChangedFor(nameof(PreviewPlotEntryCommand))]
    public partial EntryEditViewModel? SelectedEntryEdit { get; set; }

    public TranslationEntryEditViewModel? SelectedTranslationEntryEdit
        => SelectedEntryEdit as TranslationEntryEditViewModel;

    public PlotEntryEditViewModel? SelectedPlotEntryEdit
        => SelectedEntryEdit as PlotEntryEditViewModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddNewPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddNewEntryCommand))]
    [NotifyCanExecuteChangedFor(nameof(DuplicateEntryCommand))]
    [NotifyCanExecuteChangedFor(nameof(RenameEntryCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteEntryCommand))]
    [NotifyCanExecuteChangedFor(nameof(ChangeEntryTypeCommand))]
    public partial EntryViewModel? SelectedEntry { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Character))]
    [NotifyPropertyChangedFor(nameof(SelectedPlotPage))]
    [NotifyCanExecuteChangedFor(nameof(DuplicatePageCommand))]
    public partial DialogPageViewModel? SelectedPage { get; set; }

    public DialogPlotPageViewModel? SelectedPlotPage => SelectedPage as DialogPlotPageViewModel;

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
    public partial bool IsDirty { get; set; }

    public event Action? Dirty;

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

    public RelayCommand<string> GotoOrCreateEntryCommand { get; }

    public RelayCommand ChangeEntryTypeCommand { get; }

    public DialogEditViewModel(DialogDocument dialogDocument, IEditorDialogHost editorDialogHost)
    {
        this.editorDialogHost = editorDialogHost;

        document = dialogDocument;
        entries = new(document.Entries.Select(p => new EntryViewModel(p.name, p.entry)));
        entryEdits = new();
        UpdateEntriesToShow();

        MoveUpPageCommand = new(OnMoveUpPage!, CanMoveUpPageExecute);
        MoveDownPageCommand = new(OnMoveDownPage!, CanMoveDownPageExecute);
        PreviewPageCommand = new(OnPreviewPage, CanPreviewPageExecute);
        DeletePageCommand = new(OnDeletePage!, CanActionOnPageExecute);
        DuplicatePageCommand = new(OnDuplicatePageCommand, CanActionOnPageExecute);

        AddNewPageCommand = new(OnAddNewPage, CanAddNewPageExecute);
        PreviewPlotEntryCommand = new(OnPreviewPlotEntry, CanPreviewPlotEntryExecute);

        AddNewEntryCommand = new(OnAddNewEntry, CanActionOnEntryExecute);
        DuplicateEntryCommand = new(OnDuplicateEntry, CanActionOnEntryExecute);
        RenameEntryCommand = new(OnRenameEntry, CanActionOnEntryExecute);
        DeleteEntryCommand = new(OnDeleteEntry, CanActionOnEntryExecute);

        ChangeEntryTypeCommand = new(OnChangeEntryType, CanActionOnEntryExecute);

        GotoOrCreateEntryCommand = new(OnGotoOrCreateEntry!);

        AvailableSubCharacters = null!;
        AvailableCharacters = new(App.Current.PortraitsInfoService.GetCharacters());
    }

    private void UpdateSubCharacters(DialogPlotPageViewModel page)
        => AvailableSubCharacters = new(App.Current.PortraitsInfoService.GetSubCharacters(page.Character));

    partial void OnSelectedPageChanged(DialogPageViewModel? value)
    {
        if (value is DialogPlotPageViewModel newPlotPage)
            UpdateSubCharacters(newPlotPage);
    }

    partial void OnEntrySearchTextChanged(string value)
        => UpdateEntriesToShow();

    partial void OnIsDirtyChanged(bool value)
        => Dirty?.Invoke();

    [MemberNotNull(nameof(EntriesToShow))]
    private void UpdateEntriesToShow()
    {
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
            SelectedEntryEdit = null;
            return;
        }

        if (entryEdits.TryGetValue(value, out EntryEditViewModel? edit))
        {
            SelectedEntryEdit = edit;
            return;
        }

        SetEditTo(value);
    }

    private void SetEditTo(EntryViewModel entryViewModel)
    {
        DialogEntry entry = entryViewModel.Entry;
        EntryEditViewModel? newEdit;
        if (entry is DialogPlotEntry plotEntry)
        {
            var pages = CreatePlotPageViewModels(plotEntry, editorDialogHost);
            newEdit = new PlotEntryEditViewModel(new(pages));
        }
        else if (entry is DialogTranslationEntry transEntry)
        {
            newEdit = new TranslationEntryEditViewModel(transEntry.Translation);
        }
        else
        {
            throw new UnreachableException();
        }
        SelectedEntryEdit = entryEdits[entryViewModel] = newEdit;
    }

    private void CurrentPageViewModels_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ReevaluateIndex(SelectedPlotEntryEdit!.PagesViewModels);
        MoveUpPageCommand.NotifyCanExecuteChanged();
        MoveDownPageCommand.NotifyCanExecuteChanged();
        IsDirty = true;

        if (e.OldItems is not null)
            foreach (DialogPageViewModel item in e.OldItems)
                item.Dirty -= SetDirty;
        if (e.NewItems is not null)
            foreach (DialogPageViewModel item in e.NewItems)
                item.Dirty += SetDirty;
    }

    partial void OnSelectedEntryEditChanged(EntryEditViewModel? oldValue, EntryEditViewModel? newValue)
    {
        var oldPlotEdit = oldValue as PlotEntryEditViewModel;
        var newPlotEdit = newValue as PlotEntryEditViewModel;
        if (oldPlotEdit is not null)
            oldPlotEdit.PagesViewModels.CollectionChanged -= CurrentPageViewModels_CollectionChanged;
        if (newPlotEdit is not null)
            newPlotEdit.PagesViewModels.CollectionChanged += CurrentPageViewModels_CollectionChanged;

        var oldTransEdit = oldValue as TranslationEntryEditViewModel;
        var newTransEdit = newValue as TranslationEntryEditViewModel;
        if (oldTransEdit is not null)
            oldTransEdit.PropertyChanged -= SetDirtyHandler;
        if (newTransEdit is not null)
            newTransEdit.PropertyChanged += SetDirtyHandler;

        void SetDirtyHandler(object? sender, PropertyChangedEventArgs args)
            => SetDirty();
    }

    void SetDirty()
    {
        SelectedEntry!.IsDirty = true;
        IsDirty = true;
    }

    #region page action

    private void OnMoveUpPage(DialogPageViewModel page)
    {
        int index = page.Index;
        SelectedPlotEntryEdit!.PagesViewModels.Move(index, index - 1);
    }

    private void OnMoveDownPage(DialogPageViewModel page)
    {
        int index = page.Index;
        SelectedPlotEntryEdit!.PagesViewModels.Move(index, index + 1);
    }

    private void OnPreviewPage(DialogPageViewModel? page)
    {
        DialogPlotPageViewModel plotPage = (DialogPlotPageViewModel)page!;
        List<DialogPlotPageViewModel> pagesToPreview = new();
        while (true)
        {
            pagesToPreview.Add(plotPage);
            if (plotPage.Index + 1 < SelectedPlotEntryEdit!.PagesViewModels.Count)
            {
                DialogPlotPageViewModel? next = SelectedPlotEntryEdit.PagesViewModels[plotPage.Index + 1] as DialogPlotPageViewModel;
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
        if (editorDialogHost.RequestConfirm("确认要删除此页吗？", "警告"))
            SelectedPlotEntryEdit!.PagesViewModels.Remove(page);
    }

    private void OnAddNewPage()
    {
        (PageNewPosition, DialogPageType)? r = App.Current.Messenger.Send(new RequestNewPageMessage(SelectedPlotPage?.Index is not null));
        if (r is not null)
        {
            (PageNewPosition pos, DialogPageType type) = r.Value;
            if (type is not DialogPageType.Plot and not DialogPageType.InlinedPlot)
                throw new NotSupportedException();
            var page = new DialogPlotPage(new DialogPortraitState(), string.Empty)
            {
                InlinedToPrevious = type is DialogPageType.InlinedPlot
            };
            var model = new DialogPlotPageViewModel(page, editorDialogHost);
            SelectedPlotEntryEdit!.PagesViewModels.Insert(pos switch
            {
                PageNewPosition.Top => 0,
                PageNewPosition.Above => SelectedPlotPage!.Index,
                PageNewPosition.Below => SelectedPlotPage!.Index + 1,
                PageNewPosition.Bottom => SelectedPlotEntryEdit.PagesViewModels.Count,
                _ => throw new ArgumentException()
            }, model);
            SelectedPage = model;
        }
    }

    private void OnDuplicatePageCommand(DialogPageViewModel? model)
    {
        var dup = model!.Clone();
        SelectedPlotEntryEdit!.PagesViewModels.Insert(SelectedPlotEntryEdit.PagesViewModels.IndexOf(model) + 1, dup);
    }

    private bool CanAddNewPageExecute()
        => SelectedEntry != null && SelectedPlotEntryEdit != null;

    private bool CanMoveUpPageExecute(DialogPageViewModel? page)
        => page is not null && page.Index != 0;

    private bool CanMoveDownPageExecute(DialogPageViewModel? page)
        => page is not null && SelectedPlotEntryEdit is not null && page.Index != SelectedPlotEntryEdit.PagesViewModels.Count - 1;

    private bool CanPreviewPageExecute(DialogPageViewModel? page)
        => page is DialogPlotPageViewModel plotPage && !plotPage.InlinedToPrevious;

    private bool CanActionOnPageExecute(DialogPageViewModel? page)
        => page is not null;

    #endregion

    #region entry action

    private void OnPreviewPlotEntry()
        => App.Current.PreviewService.Request(SelectedPlotEntryEdit!.PagesViewModels.OfType<DialogPlotPageViewModel>());

    private bool CanPreviewPlotEntryExecute()
        => SelectedPlotEntryEdit is not null;

    private void OnAddNewEntry()
    {
        (bool isPlot, string entryName)? result = App.Current.Messenger.Send<RequestNewEntryMessage>();
        if (result is not null)
        {
            string entryName = result.Value.entryName;
            if (entries.Any(e => e.EntryName.Equals(entryName, StringComparison.OrdinalIgnoreCase)))
            {
                editorDialogHost.ShowErrorDialog($"已存在项 \"{entryName}\"，项名不区分大小写。", "错误");
                return;
            }
            AddNewEntry(entryName, result.Value.isPlot);
        }
    }

    private EntryViewModel AddNewEntry(string entryName, bool isPlotEntry)
    {
        EntryViewModel viewModel = new EntryViewModel(
            entryName,
            isPlotEntry ? new DialogPlotEntry() : new DialogTranslationEntry()
            );
        viewModel.IsDirty = true;
        entries.Insert(entries.IndexOf(SelectedEntry!) + 1, viewModel);
        EntriesToShow.Insert(EntriesToShow.IndexOf(SelectedEntry!) + 1, viewModel);
        IsDirty = true;
        return viewModel;
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
                    entryEdits.Keys.Any(k => k.EntryName.Equals(result, StringComparison.OrdinalIgnoreCase));

            if (collided)
            {
                editorDialogHost.ShowErrorDialog($"已存在同名项 \"{result}\"。", "错误");
                return;
            }
            SelectedEntry.EntryName = result;
            SelectedEntry.IsDirty = true;
            IsDirty = true;
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
        IsDirty = true;
    }

    private void OnDeleteEntry()
    {
        bool result = editorDialogHost.RequestConfirm($"确认要删除项 \"{SelectedEntry!.EntryName}\" 吗？", "警告");
        if (!result) return;
        entries.Remove(SelectedEntry);
        EntriesToShow.Remove(SelectedEntry);
        IsDirty = true;
    }

    private void OnChangeEntryType()
    {
        var entry = SelectedEntry!.Entry;
        if (entry is DialogTranslationEntry transEntry)
        {
            DialogPlotEntry plotEntry = new DialogPlotEntry([new DialogPlotPage(new(), transEntry.Translation)]);
            SelectedEntry.Entry = plotEntry;
        }
        else if (entry is DialogPlotEntry plotEntry)
        {
            bool confirm = editorDialogHost.RequestConfirm("将剧情项转换为翻译项会取第一页的内容，且该操作不可逆，确认执行该操作吗？", "警告");
            if (!confirm) return;
            var content = plotEntry.Pages.OfType<DialogPlotPage>().FirstOrDefault()?.Text ?? string.Empty;
            DialogTranslationEntry translationEntry = new DialogTranslationEntry(content);
            SelectedEntry.Entry = translationEntry;
        }
        else
        {
            throw new UnreachableException();
        }

        SelectedEntry.IsDirty = true;
        IsDirty = true;
        SetEditTo(SelectedEntry);
        // TODO hmmm any better way?
        AddNewPageCommand.NotifyCanExecuteChanged();
    }

    private void OnGotoOrCreateEntry(string entryName)
    {
        var entry = entries.SingleOrDefault(m => m.EntryName.Equals(entryName, StringComparison.OrdinalIgnoreCase));
        if (entry is null)
        {
            var newEntry = AddNewEntry(entryName, false);
            SelectedEntry = newEntry;
        }
        else
        {
            SelectedEntry = entry;
        }
    }

    private bool CanActionOnEntryExecute()
        => SelectedEntry is not null;

    #endregion

    #region save action

    public DialogDocument ApplyChanges()
    {
        var sEntry = SelectedEntry;
        EntryEditViewModel sEdit = null!;
        if (sEntry is not null)
            entryEdits.TryGetValue(sEntry, out sEdit!);

        foreach (var pair in entryEdits)
        {
            EntryViewModel entryViewModel = pair.Key;
            if (!entryViewModel.IsDirty) continue;
            string entryName = entryViewModel.EntryName;
            EntryEditViewModel edit = pair.Value;
            DialogEntry newEntry = edit switch
            {
                PlotEntryEditViewModel p => new DialogPlotEntry(p.PagesViewModels.Select(v => v.ToModel())),
                TranslationEntryEditViewModel t => new DialogTranslationEntry(t.Translation),
                _ => throw new UnreachableException()
            };

            entryViewModel.Entry = newEntry;
        }

        foreach (var entry in entries)
            entry.IsDirty = false;

        IsDirty = false;
        entryEdits.Clear();

        if (sEntry is not null)
            entryEdits.Add(sEntry, sEdit);

        document.Entries = entries.Select(vm => (vm.EntryName, vm.Entry)).ToList();
        return document;
    }

    #endregion

    private static void ReevaluateIndex(IList<DialogPageViewModel> pages)
    {
        for (int i = 0; i < pages.Count; i++)
            pages[i].Index = i;
    }

    private List<DialogPageViewModel> CreatePlotPageViewModels(DialogPlotEntry plotEntry, IEditorDialogHost editorDialogHostt)
    {
        List<DialogPageViewModel> vms = new();
        foreach (CelesteDialog.DialogPage page in plotEntry.Pages)
        {
            DialogPageViewModel vm = page switch
            {
                DialogPlotPage plotPage => new DialogPlotPageViewModel(plotPage, editorDialogHostt),
                DialogTriggerPage triggerPage => new DialogTriggerPageViewModel(triggerPage),
                _ => throw new ArgumentException("Invalid type of page.", nameof(plotEntry)),
            };
            vms.Add(vm);
        }
        ReevaluateIndex(vms);
        foreach (var vm in vms)
        {
            vm.ListenOnChanged();
            vm.Dirty += SetDirty;
        }
        return vms;
    }
}