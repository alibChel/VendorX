
using DevExpress.XamarinForms.Core.Internal;
using P42.Utils;
using Realms;
using Rg.Plugins.Popup.Services;
using System;
using System.Drawing;

using System.Globalization;
using System.Text.RegularExpressions;
using VendorX.Resources;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.Essentials;
using Xamarin.Forms;
using VendorX.Views;
using VendorX;

using System.Threading;
using VendorX.Services;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;
using static Xamarin.Essentials.Permissions;
using CommunityToolkit.Mvvm.Input;

namespace Vendor.ViewModels;

public partial class SettingViewModel : BaseViewModel
{
    [ObservableProperty]
    Profile profile;

    //выбранная тема
    // 0 -система , 1 - темная , 2 -светлая
    [ObservableProperty]
    bool systemThemeToogled, lightThemeToogled, darkThemeToogled, ruLangTogled, kzLangTogled, ozLangTogled, kgLangTogled,engLangTogled;
    //тякущая название темы которое отоброжается в пользовательском интерфейсе
    [ObservableProperty]
    string userThemeExpander;
    [ObservableProperty]
    string userEmail, userPhone, userGoogle, userAple, userFaceBook;
    /*    private BluetoothLE.Core.IAdapter BluetoothAdapter;*/


    CultureInfo curentCulture { get { return LocalizationResourceManager.Current.CurrentCulture; } set { LocalizationResourceManager.Current.CurrentCulture = value; } }

    private string cultureToogled { get { return string.IsNullOrWhiteSpace(SaveCulture) ? curentCulture.ToString() : SaveCulture; } }

  

    #region Theme
    [RelayCommand]
    public void SystemThemeChanged()
    {
        LightThemeToogled = false;
        DarkThemeToogled = false;
        SystemThemeToogled = true;
        userTheme = 0;
        ThemeSelectionChanged(0);
    }
    [RelayCommand]
    public void DarkThemeChanged()
    {
        DarkThemeToogled = true;
        LightThemeToogled = false;
        SystemThemeToogled = false;
        userTheme = 2;
        ThemeSelectionChanged(2);
    }
    [RelayCommand]
    public void LightThemeChanged()
    {
        SystemThemeToogled = false;
        DarkThemeToogled = false;
        LightThemeToogled = true;
        userTheme = 1;
        ThemeSelectionChanged(1);
    }
    #endregion


    // 0- rus, 1 - kz, 2-oz, 3-kg, 4-en
    [RelayCommand]
    private void LanguageChanged(string parametr)
    {
  
        switch (parametr)
        {
            case "0":
                KzLangTogled = false;
                OzLangTogled = false;
                KgLangTogled = false;
                EngLangTogled = false;
                RuLangTogled = true;
                curentCulture= new CultureInfo("ru");
                SaveCulture = curentCulture.ToString();
               
                break;
            case "1":
                RuLangTogled = false;
                OzLangTogled = false;
                KgLangTogled = false;
                EngLangTogled = false;
                KzLangTogled = true;
                curentCulture = new CultureInfo("kk-KZ");
                SaveCulture = curentCulture.ToString();
                break;
            case "2":
                KzLangTogled = false;
                OzLangTogled = false;
                KgLangTogled = false;
                EngLangTogled = false;
                DialogService.ShowAlertAsync("Oшибка", "Язык в разработке", "OK");
                break;
            case "3":
                KzLangTogled = false;
                OzLangTogled = false;
                KgLangTogled = false;
                EngLangTogled = false;
                DialogService.ShowAlertAsync("Oшибка", "Язык в разработке", "OK");
                break;
            case "4":
                KzLangTogled = false;
                OzLangTogled = false;
                KgLangTogled = false;
                EngLangTogled = true;
                RuLangTogled = false;
                curentCulture = new CultureInfo("en-US");
                SaveCulture = curentCulture.ToString();
                break;
            default:
                RuLangTogled = true;
                OzLangTogled = false;
                KgLangTogled = false;
                KzLangTogled = false;
                EngLangTogled = false;
                break;
        }
        Application.Current.MainPage = new AppShell();
    }



    


    public SettingViewModel()
    {
      /*  BluetoothAdapter.StartScanningForDevices();

        BluetoothAdapter.DeviceDiscovered += DeviceDiscovered;*/

    }
    //private void DeviceDiscovered(object sender, DeviceDiscoveredEventArgs e)
    //{
    //    if (deviceList.All(x => x.Id != e.Device.Id))
    //    {
    //        deviceList.Add(e.Device);
    //    }
    //}
    public int userTheme
    {
        get => Preferences.Get("CastTheme", 0);

        set
        {
            Preferences.Set("CastTheme", value);
            OnPropertyChanged(nameof(userTheme));
        }
    }

    public int userLanguage
    {
        get => Preferences.Get("CastLanguage", 0);
        set
        {
            Preferences.Set("CastLanguage", value);
            OnPropertyChanged(nameof(userLanguage));
        }
    }

    public async Task OnAppearing()
    {
        //deviceList?.Clear();
        //bluettothServise = new();
        //if(bluettothServise.ble.IsOn)
        //{
        //    await DialogService.ShowToast("YYYES");
        //    IsBusy = true;
        //    deviceList.AddRange(await bluettothServise.ScaningAllDevices());
        //    IsBusy = false;
        //
        //}
        //TODO получение емайла для отлбражения
        //UserEmail = string.IsNullOrWhiteSpace(RealmService.CurrentUser.Profile.Email) ? RealmService.CurrentUser.Profile.Name : RealmService.CurrentUser.Profile.Email;
        //UserEmail = RealmService.CurrentUser.Profile.Email;
      
        var realm = RealmService.GetMainThreadRealm();
        Profile = realm.All<Profile>().FirstOrDefault();
        UserPhone = (Profile.Phone ?? "");
        
        UserPhone=UserPhone.Length > 7?  $"+7 {UserPhone.Substring(0, 3)} {UserPhone.Substring(3, 3)} {UserPhone.Substring(6, 2)} {UserPhone.Substring(8, 2)}":UserPhone;
        UserEmail = (Profile.Login != Profile.Phone && !string.IsNullOrEmpty(Profile.Login)) ? Profile.Login : "" ;




        //select language

        switch (cultureToogled)
        {
            case "kk-KZ":
                KzLangTogled=true;
                RuLangTogled = false;
                OzLangTogled = false;
                KgLangTogled=false;
                EngLangTogled = false;
                break;
            case "en-US":
                KzLangTogled = false;
                RuLangTogled = false;
                OzLangTogled = false;
                KgLangTogled = false;
                EngLangTogled = true;
                break;
            default:
                KzLangTogled = false;
                RuLangTogled = true;
                OzLangTogled = false;
                KgLangTogled = false;
                EngLangTogled = false;
                break;
        }


        // Select theme view
        if (userTheme == 1)
            LightThemeToogled = true;
        else if (userTheme == 2)
            DarkThemeToogled = true;
        else
            SystemThemeToogled = true;
        // Select theme view

        // Select language view
    
        // Select language view
     
        //bluettothServise.ble.StateChanged += async (s, e) =>
        //{
        //    if (e.NewState == BluetoothState.On)
        //    {
        //        await DialogService.ShowToast("YYYES");
        //        IsBusy = true;
        //        deviceList.AddRange(await bluettothServise.ScaningAllDevices());
        //        IsBusy = false;
        //     
        //    }
        //    else
        //    {
        //     await   DialogService.ShowToast("NOOOO");
        //    }
        //    Debug.WriteLine($"The bluetooth state changed to {e.NewState}");
        //};



        await Task.CompletedTask;
    }



    //выбор темы
    [RelayCommand]
    public void ThemeSelectionChanged(int param)
    {
        switch (param)
        {
            case 0:
                Application.Current.UserAppTheme = OSAppTheme.Unspecified;

                break;
            case 1:
                Application.Current.UserAppTheme = OSAppTheme.Light;

                break;
            case 2:
                Application.Current.UserAppTheme = OSAppTheme.Dark;

                break;
        }
    }

    //[RelayCommand]
    //private async void AboutUs()
    //{
    //    try
    //    {
    //        await Browser.OpenAsync($"https://www.gosu.kz/", BrowserLaunchMode.SystemPreferred);
    //    }
    //    catch (Exception ex)
    //    {
    //        await DialogService.ShowAlertAsync("Ошибка", ex.Message, "Ok");
    //    }
    //}


    //звук и вибрация
    [RelayCommand]
    private void ChangeVibrationSound(string parametr)
    {
        if (parametr.Equals("0"))
        {
            VibrationScanner = !VibrationScanner;
            OnPropertyChanged(nameof(VibrationScanner));
        }

        else
        {

            SoundScanner = !SoundScanner;
            OnPropertyChanged(nameof(SoundScanner));
        }
    
    }

    [RelayCommand]
    private async Task EditUserData()
    {
        var popup = new UserEditView(Profile);
        await PopupNavigation.Instance.PushAsync(popup);
    }


  

    [RelayCommand]
    private async void DeletUser()
    {

 
      var confirm=  await DialogService.ShowWarningAsync($"{AppResources.AttentionLabel}", $"{AppResources.DeleteUserAttention}",  true);

        if (!confirm)
            return;

        IsBusy = true;

        var deleteUser = await GosyncServise.DeleteAccount(string.IsNullOrWhiteSpace(Profile.Login) ? Profile.Phone : Profile.Login);
        var userId = Profile.Id;
        if (!deleteUser)
        {
            //alert
            IsBusy = false;
            return;
        }
        Preferences.Set("Username", "");
        Preferences.Set("Pssword", "");
        Token = "";
        DateLogin = DateTime.MinValue.ToString();
        await RealmService.DeletUser(userId);
    
        await Shell.Current.GoToAsync($"//LoginPage");
        IsBusy = false;

   

    }

    [RelayCommand]
    private async void LinkMailOrPhone(string mod)
    {
        var currentBinding = AuthType.Email;
        switch (mod)
        {
            case "0":
                //email
                currentBinding = AuthType.Email;
                if (Profile.Login != Profile.Phone && !String.IsNullOrEmpty(Profile.Login))
                    return;
                break;
            case "1":
                if (!String.IsNullOrEmpty(Profile.Phone))
                    return;
                currentBinding = AuthType.Phone;
                break;
        }

        var popup = new LinkMailOrPhoneView(mod:currentBinding);
        await PopupNavigation.Instance.PushAsync(popup);
    }

}
