using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows;

namespace VisualCelesteCutscene;

public sealed class PreviewService
{
    private readonly HttpClient http;

    public PreviewService()
    {
        http = new()
        {
            BaseAddress = new("http://localhost:32270"),
            Timeout = TimeSpan.FromMilliseconds(300)
        };
        http.DefaultRequestHeaders.Add("User-Agent", $"vcc/v{typeof(PreviewService).Assembly.GetName().Version?.ToString()}");
    }

    // TODO don't show ui here
    public void Request(IEnumerable<DialogPlotPageViewModel> pages)
    {
        StringContent content = new(GetDialogString(pages));
        try
        {
            var res = http.PostAsync("vcc/preview", content).GetAwaiter().GetResult();
            if (res.StatusCode is HttpStatusCode.NotFound)
                MessageBox.Show(
                    "预览失败，请确保游戏已安装 VisualCelesteCutscene.Helper。",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
            else if(res.StatusCode is HttpStatusCode.ServiceUnavailable)
                MessageBox.Show(
                    "预览失败，请确保游戏已在关卡内。",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
            else if (!res.IsSuccessStatusCode)
                MessageBox.Show(
                    $"未知错误。 (Code: {(int)res.StatusCode})",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
        }
        catch (TaskCanceledException)
        {
            MessageBox.Show(
                "预览失败，请检查游戏是否已打开且调试模式已开启。",
                "错误",
                MessageBoxButton.OK,
                MessageBoxImage.Error
                );
        }
    }

    private static string GetDialogString(IEnumerable<DialogPlotPageViewModel> pages)
    {
        StringBuilder sb = new();
        DialogPlotPageViewModel? previousPage = null;
        foreach (var page in pages)
        {
            if (previousPage is not null && !page.InlinedToPrevious)
                sb.Append("{break}");
            if (page.AnchorBottom)
                sb.Append("{anchor bottom}");
            sb.Append("{portrait ");
            sb.Append($"{page.Character} {(page.AtRight ? "right" : "left")} {page.SubCharacter}");
            if (page.Flip)
                sb.Append(" flip");
            sb.Append('}');
            sb.Append(page.DialogText.ReplaceLineEndings("{n}"));
            previousPage = page;
        }
        return sb.ToString();
    }
}
