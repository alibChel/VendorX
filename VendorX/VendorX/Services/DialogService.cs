using System;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Services;
using VendorX;
using VendorX.Widgets;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.UI.Views.Options;
using Xamarin.Forms;

namespace Vendor.Services;

public static class DialogService
{
    public static Task ShowAlertAsync(string title, string message, string accept)
    {
        return Application.Current.MainPage.DisplayAlert(title, message, accept);
    }

    public static async Task<bool> ShowWarningAsync(string title, string message, bool confirm)
    {
        var popup = new WarningView(title, message, confirm);
        await PopupNavigation.Instance.PushAsync(popup);
        return await popup.PopupClosedTask;


    }
    public static async Task<int> CodeConfirm()
    {
        var popup = new CodeComplit();
        await PopupNavigation.Instance.PushAsync(popup);
        return await popup.PopupClosedTask;


    }

    public static async Task ShowHtmlAsync(string html)
    {
        var popup = new CheckView(html);
        await PopupNavigation.Instance.PushAsync(popup);
    }

    public static Action ShowActivityIndicator()
    {

        var popup = new BusyMopup();
        PopupNavigation.Instance.PushAsync(popup);
        return () => popup.Close_();
    }

    public static Task ShowToast(string text)
    {
        return Application.Current.MainPage.DisplayToastAsync(text, 3000);
    }


}
