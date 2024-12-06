using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Vendor.ViewModels;
using VendorX.Resources;
using VendorX.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace VendorX;

public partial class AppShell : Xamarin.Forms.Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(TransactionsDetailPage), typeof(TransactionsDetailPage));
        Routing.RegisterRoute(nameof(ItemDetail), typeof(ItemDetail));
        Routing.RegisterRoute(nameof(ArrivalPage), typeof(ArrivalPage));
        Routing.RegisterRoute(nameof(NotificationPage), typeof(NotificationPage));
        Routing.RegisterRoute(nameof(RegistView), typeof(RegistView));
        Routing.RegisterRoute(nameof(SettingPage), typeof(SettingPage));
        Routing.RegisterRoute(nameof(OrderDetailPage), typeof(OrderDetailPage));
        Routing.RegisterRoute($"//{nameof(LoginPage)}", typeof(LoginPage));
        ChangeTabColor();
     
    }

    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        if (args.Source == ShellNavigationSource.ShellSectionChanged)
        {
            ChangeTabColor();
        }

        base.OnNavigated(args);
    }

    // Замена цвета таббара
    private void ChangeTabColor()
    {
        if (DeviceInfo.Platform != DevicePlatform.iOS)
            return;

        Color selcolor = Color.FromHex("#66B2F0");
        Color unselcolor = Color.FromHex("#919191");
        

        for (int i = 0; i < MyTabbar.Items.Count; i++)
        {
            var img = (FontImageSource)MyTabbar.Items[i].Icon;
            bool isCurrentPage = MyTabbar.CurrentItem == MyTabbar.Items[i] ? true : false;
            MyTabbar.Items[i].Icon = new FontImageSource
            {
                Glyph = img.Glyph,
                FontFamily = img.FontFamily,
                Size = isCurrentPage ? 30 : 25,
                Color = isCurrentPage ? selcolor : unselcolor
            };
        }
    }

}

