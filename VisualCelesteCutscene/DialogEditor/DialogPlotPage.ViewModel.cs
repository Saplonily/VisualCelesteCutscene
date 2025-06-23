using System.Windows.Media;
using CelesteDialog;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace VisualCelesteCutscene;

public sealed partial class DialogPlotPageViewModel : DialogPageViewModel
{
    [ObservableProperty] private string character;

    [ObservableProperty] private string subCharacter;

    [ObservableProperty] private string dialogText;

    [ObservableProperty] private bool anchorBottom;

    [ObservableProperty]
    private bool inlinedToPrevious;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FlipResult))]
    private bool atRight;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FlipResult))]
    private bool flip;

    [ObservableProperty]
    private ImageSource? portraitImage;

    public bool FlipResult => AtRight ^ Flip;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SurroundNoParamCommand))]
    [NotifyCanExecuteChangedFor(nameof(SurroundWithNumCommand))]
    [NotifyCanExecuteChangedFor(nameof(SurroundColorCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelSurroundCommand))]
    [NotifyCanExecuteChangedFor(nameof(InsertCommand))]
    public partial int SelectionStart { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SurroundNoParamCommand))]
    [NotifyCanExecuteChangedFor(nameof(SurroundWithNumCommand))]
    [NotifyCanExecuteChangedFor(nameof(SurroundColorCommand))]
    [NotifyCanExecuteChangedFor(nameof(InsertCommand))]
    public partial int SelectionLength { get; set; }

    public RelayCommand<SurroundingType> SurroundNoParamCommand { get; }

    public RelayCommand<SurroundingType> SurroundWithNumCommand { get; }

    public RelayCommand<SurroundingType> CancelSurroundCommand { get; }

    public RelayCommand<InsertType> InsertCommand { get; }

    public RelayCommand SurroundColorCommand { get; }

    // make designer happy
#if DEBUG
    public DialogPlotPageViewModel()
    {
        character = subCharacter = dialogText = null!;
        SurroundNoParamCommand = null!;
        SurroundWithNumCommand = null!;
        SurroundColorCommand = null!;
        CancelSurroundCommand = null!;
        InsertCommand = null!;
    }
#endif

    public DialogPlotPageViewModel(DialogPlotPage page)
    {
        dialogText = page.Text;
        character = page.Portrait.Character;
        subCharacter = page.Portrait.SubCharacter;
        atRight = page.Portrait.AtRight;
        flip = page.Portrait.Flip;
        anchorBottom = page.Portrait.AnchorBottom;
        inlinedToPrevious = page.InlinedToPrevious;
        UpdatePortrait();
        SurroundNoParamCommand = new RelayCommand<SurroundingType>(OnSurroundNoParam, _ => CanSurroundExecute());
        SurroundWithNumCommand = new RelayCommand<SurroundingType>(OnSurroundWithNumParam, _ => CanSurroundExecute());
        SurroundColorCommand = new RelayCommand(OnSurroundColor, CanSurroundExecute);
        CancelSurroundCommand = new RelayCommand<SurroundingType>(OnCancelSurround, CanCancelSurroundExecute);
        InsertCommand = new RelayCommand<InsertType>(OnInsert, CanInsertExecute);
    }

    #region actions

    private void OnSurroundNoParam(SurroundingType type)
    {
        string toInsert = type switch
        {
            SurroundingType.Wavy => "~",
            SurroundingType.Impact => "!",
            SurroundingType.Messy => "%",
            SurroundingType.Bigger => "big",
            _ => throw new ArgumentException("Invalid no-param surrounding type.", nameof(type))
        };
        int start = SelectionStart;
        int length = SelectionLength;
        string left = $"{{{toInsert}}}";
        string right = $"{{/{toInsert}}}";
        DialogText = Surround(DialogText, start, length, left, right);
        SelectionStart = start + left.Length;
        SelectionLength = length;
    }

    private void OnSurroundWithNumParam(SurroundingType type)
    {
        string toInsert = type switch
        {
            SurroundingType.ChangeSpeed => ">>",
            _ => throw new ArgumentException("Invalid with-param surrounding type.", nameof(type))
        };
        int start = SelectionStart;
        int length = SelectionLength;
        string left = $"{{{toInsert} 2}}";
        string right = $"{{{toInsert}}}";
        DialogText = Surround(DialogText, start, length, left, right);
        SelectionStart = start + left.Length - 2;
        SelectionLength = 1;
    }

    private void OnSurroundColor()
    {
        Color? reply = App.Current.Messenger.Send<RequestColorPickMessage>();
        if (reply is null) return;
        Color color = reply.Value;
        int start = SelectionStart;
        int length = SelectionLength;
        string left = $"{{# {color.R:X2}{color.G:X2}{color.B:X2}}}";
        string right = $"{{#}}";
        DialogText = Surround(DialogText, start, length, left, right);
        SelectionStart = start + left.Length;
        SelectionLength = length;
    }

    private void OnCancelSurround(SurroundingType type)
    {
        int start = SelectionStart;
        (string left, string right) = GetSurroundingText(type);
        DialogText = SurroundingUtil.RemoveMatchingLR(DialogText, SelectionStart, left, right, out int leftLength);
        SelectionStart = start - leftLength;
    }

    private void OnInsert(InsertType type)
    {
        if (type is InsertType.Reference)
        {

        }
        else if (type is InsertType.Pause)
        {

        }
    }

    private bool CanInsertExecute(InsertType obj)
        => SelectionLength == 0;

    private bool CanCancelSurroundExecute(SurroundingType type)
    {
        (string left, string right) = GetSurroundingText(type);
        return SurroundingUtil.HasMatchingLR(DialogText, SelectionStart, left, right);
    }

    private static (string left, string right) GetSurroundingText(SurroundingType type) => type switch
    {
        SurroundingType.Wavy => ("~", "/~"),
        SurroundingType.Color => ("#", "#"),
        SurroundingType.Impact => ("!", "/!"),
        SurroundingType.Bigger => ("big", "/big"),
        SurroundingType.ChangeSpeed => (">>", ">>"),
        SurroundingType.Messy => ("%", "/%"),
        _ => throw new ArgumentException("Invalid surrounding type.", nameof(type)),
    };

    private static string Surround(string text, int start, int length, string left, string right)
        => text.Insert(start + length, right).Insert(start, left);

    private bool CanSurroundExecute()
        => SelectionLength > 0;

    #endregion

    private void UpdatePortrait()
        => PortraitImage = App.Current.PortraitsInfoService.GetImageSourceFor(Character, SubCharacter);

    partial void OnCharacterChanged(string value) => UpdatePortrait();

    partial void OnSubCharacterChanged(string value) => UpdatePortrait();

    public override DialogPlotPageViewModel Clone()
        => new DialogPlotPageViewModel(
            new DialogPlotPage(
                new DialogPortraitState(Character, SubCharacter, AtRight, AnchorBottom, Flip),
                DialogText,
                InlinedToPrevious
                )
            );

    public override DialogPlotPage ToModel()
        => new DialogPlotPage(
            new DialogPortraitState(Character, SubCharacter, AtRight, AnchorBottom, Flip, false, false),
            DialogText,
            InlinedToPrevious
            );
}