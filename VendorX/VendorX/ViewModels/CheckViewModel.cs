using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace Vendor.ViewModels;

public partial class CheckViewModel: BaseViewModel
{
    [ObservableProperty]
    HtmlWebViewSource htmlWebViewSource;

    [ObservableProperty]
    private string httmlPath;


    internal void OnAppearing()
    {
        HtmlWebViewSource = new HtmlWebViewSource();
        if (!string.IsNullOrWhiteSpace(HttmlPath))
        {
            HtmlWebViewSource.Html = HttmlPath;
        }
    }

    [RelayCommand]
    private async Task Close()
    {
        await PopupNavigation.Instance.PopAsync();

    }
}
