using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace VendorX.Helpers;

public static class TheTheme
{
    public static void SetTheme()
    {
        switch (Settings.CastTheme)
        {
            //default
            case 0:
                App.Current.UserAppTheme = OSAppTheme.Unspecified;
                break;
            //light
            case 1:
                App.Current.UserAppTheme = OSAppTheme.Light;
                break;
            //dark
            case 2:
                App.Current.UserAppTheme = OSAppTheme.Dark;
                break;
        }

        var nav = App.Current.MainPage as Xamarin.Forms.NavigationPage;

        var e = DependencyService.Get<IEnvironment>();
        var darkcol = Color.FromHex("#202125");
        var whitecol = Color.FromHex("#ebeaef");
        if (App.Current.UserAppTheme == OSAppTheme.Unspecified)
        {
            if (App.Current.RequestedTheme == OSAppTheme.Dark)
            {
                e?.SetStatusBarColor(darkcol, false);
                if (nav != null)
                {
                    nav.BarBackgroundColor = Color.Black;
                    nav.BarTextColor = Color.White;
                }
            }
            else
            {
                e?.SetStatusBarColor(whitecol, true);
                if (nav != null)
                {
                    nav.BarBackgroundColor = Color.White;
                    nav.BarTextColor = Color.Black;
                }
            }
        }
        else
        {
            if (App.Current.UserAppTheme == OSAppTheme.Dark)
            {
                e?.SetStatusBarColor(darkcol, false);
                if (nav != null)
                {
                    nav.BarBackgroundColor = Color.Black;
                    nav.BarTextColor = Color.White;
                }
            }
            else
            {
                e?.SetStatusBarColor(whitecol, true);
                if (nav != null)
                {
                    nav.BarBackgroundColor = Color.White;
                    nav.BarTextColor = Color.Black;
                }
            }
        }

    }
}

public static class Settings
{
    static int castTheme;
    public static int CastTheme
    {
        get => Preferences.Get(nameof(CastTheme), castTheme);
    }
    // 0 = default, 1 = light, 2 = dark
    const int theme = 0;
    public static int Theme
    {
        get => Preferences.Get(nameof(Theme), theme);
        set => Preferences.Set(nameof(Theme), value);
    }
}
