using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VisualCelesteCutscene;

public sealed class SettingsWindowViewModel : ObservableObject, IDataErrorInfo, IDisposable
{
    private readonly UserData userData;

    public string CelesteGamePath
    {
        get => userData.CelesteGamePath;
        set
        {
            userData.CelesteGamePath = value;
            OnPropertyChanged();
        }
    }

    public string CelesteGraphicsDumpPath
    {
        get => userData.CelesteGraphicsDumpPath;
        set
        {
            userData.CelesteGraphicsDumpPath = value;
            OnPropertyChanged();
        }
    }

    public SettingsWindowViewModel(UserData userData)
    {
        this.userData = userData;
        App.Current.PortraitsInfoService.Clear();
    }

    string IDataErrorInfo.Error => string.Empty;

    string IDataErrorInfo.this[string columnName] => columnName switch
    {
        nameof(CelesteGamePath)
            => PathValidator.ValidateCelesteGamePath(CelesteGamePath),
        nameof(CelesteGraphicsDumpPath)
            => PathValidator.ValidateCelesteGraphicsDumpPath(CelesteGraphicsDumpPath),
        _ => string.Empty,
    };

    public void Dispose()
    {
        if (userData.CheckPathValid())
            App.Current.PortraitsInfoService.InitBy(userData);
    }
}