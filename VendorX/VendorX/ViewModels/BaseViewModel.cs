using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Xamarin.Forms;

using Vendor.Models;
using Vendor.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Rg.Plugins.Popup.Services;
using VendorX.Views;

using VendorX.Services;

using System.Globalization;
using Xamarin.CommunityToolkit.Helpers;


namespace Vendor.ViewModels;

public partial class BaseViewModel : ObservableObject
{


    public string UserId { get => RealmService.CurrentUser.Identities.FirstOrDefault()?.Id; }
    // текуший логин
    public static string CurrentLogin { get => Preferences.Get(nameof(CurrentLogin), ""); set => Preferences.Set(nameof(CurrentLogin), value); }

    // Id текущего магазина
    public string CurrentShopID { get => Preferences.Get(nameof(CurrentShopID), ""); set => Preferences.Set(nameof(CurrentShopID), value); }
    public string Token { get => Preferences.Get(nameof(Token), ""); set => Preferences.Set(nameof(Token), value); }

    // Проверка доступности на складе для текущего магазина
    public bool IsCurrentShopCheckStock { get => Preferences.Get(nameof(IsCurrentShopCheckStock), false); set => Preferences.Set(nameof(IsCurrentShopCheckStock), value); }
    public bool VibrationScanner { get => Preferences.Get(nameof(VibrationScanner), true); set => Preferences.Set(nameof(VibrationScanner), value); }
    public bool SoundScanner { get => Preferences.Get(nameof(SoundScanner), true); set => Preferences.Set(nameof(SoundScanner), value); }
    public string DateLogin { get => Preferences.Get(nameof(DateLogin), ""); set => Preferences.Set(nameof(DateLogin), value); }
    public string SaveCulture { get => Preferences.Get(nameof(SaveCulture), ""); set => Preferences.Set(nameof(SaveCulture), value); }
    public string AppVersion { get => $"v:{VersionTracking.CurrentVersion}({VersionTracking.CurrentBuild})"; }
    // 
    [ObservableProperty]
    protected bool isBusy;

    // есть ли уведомления
    [ObservableProperty]
    protected bool isNotification;

    [ObservableProperty]
    private string title;

    protected Action currentDismissAction;

    // экземпляр реалма
    private Realm realm;
    private IQueryable<Invites> invites;

    public BaseViewModel()
    {

        //if (_connectivity == null)
        //    _connectivity = ServiceHelper.GetService<IConnectivity>();
    }

    // Запуск индикатора активности
    partial void OnIsBusyChanged(bool value)
    {
        try
        {
            if (value)
            {
                currentDismissAction = Services.DialogService.ShowActivityIndicator();
            }
            else
            {
                currentDismissAction?.Invoke();
                currentDismissAction = null;
            }
        }
        catch (Exception)
        {

        }

    }

    //Регулярное выражение на проверку номера телефона
    public bool IsValidPhone(string phone) =>
       Regex.IsMatch(phone, @"^7\d{9}$");

    //Регулярное выражение на проверку почты
    public bool IsValidEmail(string email) =>
        Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    ////Регулярное выражение на проверку пароля(буквы и цифры, и хотя бы одна буква в верхнем регистре)
    /*public Func<string, bool> ValidatePassword =
       (string password) => new Regex("^(?=.*[A-Z])(?=.*\\d)[A-Za-z\\d]{8,}$").IsMatch(password);*/

    // Регулярное выражение на проверку пароля (буквы, цифры, и необязательные спец. символы, и хотя бы одна буква в верхнем регистре)
    public Func<string, bool> ValidatePassword =
        (string password) => new Regex("^(?=.*[A-Z])(?=.*\\d)[A-Za-z\\d_!@#$%^&*.,?]{8,}$").IsMatch(password);

    // Выход    
    public async Task LogoutTapped()
    {
        IsBusy = true;
        await RealmService.LogoutAsync();

        IsBusy = false;
        Application.Current.MainPage = new LoginPage();
        //await Shell.Current.GoToAsync($"//LoginPage");
    }

    // Вызов меню выбора фото    
    public async Task<List<string>> PickPhoto(int count = 1)
    {
        var popup = new PhotoPickerView(count);
        await PopupNavigation.Instance.PushAsync(popup);
        var result = await popup.PopupClosedTask;
        return result;
    }

    // Изменение значения числа (калькулятор)
    public async Task<double> ChangeNummValue(double price)
    {
        double result = price;
        try
        {
            var popup = new ChangePriceView(price);
            await PopupNavigation.Instance.PushAsync(popup);
            result = await popup.PopupClosedTask;
        }
        catch (Exception)
        {

        }
        return result;
    }

    // Изменение значения числа (калькулятор)
    [RelayCommand]
    public async Task ShowNotifications()
    {
        await Shell.Current.GoToAsync(nameof(NotificationPage));

    }

    [RelayCommand]
    public async Task OpenSetting()
    {
        await Shell.Current.GoToAsync(nameof(SettingPage));
    }

    [RelayCommand]
    private async Task EditShop()
    {
        try
        {
            var realm = RealmService.GetMainThreadRealm();
            var curmem = realm.All<Member>().ToList().FirstOrDefault(x => x.Shop.Id == CurrentShopID);
            if (curmem?.Role != UserRole.Owner)
                return;
            var popup = new ShopEditView(curmem.Shop, false);
            await PopupNavigation.Instance.PushAsync(popup);
        }
        catch (Exception e)
        {
            var i = e;
        }
    }

    [RelayCommand]
    public async Task ShowProfile()
    {
        try
        {
            var popup = new ProfileView();
            await PopupNavigation.Instance.PushAsync(popup);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
    public void ChekNotification()
    {
        realm = RealmService.GetMainThreadRealm();
        invites = realm.All<Invites>().Where(x => x.TargetId == UserId || x.TargetId == RealmService.CurrentUser.Id);
        IsNotification = (invites.Count() != 0);

    }
}

