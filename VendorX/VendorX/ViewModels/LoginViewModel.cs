using CommunityToolkit.Mvvm.Messaging;
using Rg.Plugins.Popup.Services;
using VendorX;
using VendorX.Views;
using VendorX.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using VendorX.Resources;

using Xamarin.CommunityToolkit.Helpers;

using Realms;
using MongoDB.Bson.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using P42.Utils;
using ZXing;
using Xamarin.CommunityToolkit.Behaviors;
using System;
using System.Net.Http;
using VendorX.Models;
using System.Text.RegularExpressions;


namespace Vendor.ViewModels;



public partial class LoginViewModel : BaseViewModel
{
    private string userName;

    public static MaskedBehavior Mask = new MaskedBehavior
    {
        Mask = "XXX XXX XX XX",
        UnMaskedCharacter = 'X',
    };



    [ObservableProperty]
    private string  password,phone;
    [ObservableProperty]
    bool isEmail=true, isPhone;

    [ObservableProperty]
    CurrentRegistration nextLogin = CurrentRegistration.Phone;

    [ObservableProperty]
    CurrentRegistration currentLoginReg = CurrentRegistration.Email;

    [ObservableProperty]
    private CurrentRegistration isEmailLogin = CurrentRegistration.Email;
    [ObservableProperty]
    private CurrentRegistration isPhoneLogin = CurrentRegistration.Phone;

    private string currentCodeRegion = "7";
    private Realms.Realm realm;
    [ObservableProperty]
    Profile profile;

    [ObservableProperty]
    Entry loginEntry;
 


    public string UserName { get => userName; set { userName = IsPhoneOrMail(value)?value:userName=value.RemoveWhitespace(); OnPropertyChanged(nameof(UserName)); } }


    private bool IsPhoneOrMail(string st)
    {
        //Regex validatePhoneNumberRegex = new Regex("^\\+?[Aa-Zz][Aa-Zz]{10,14}$");
  
       
        IsEmail = HasNonDigits(st);
        CurrentLoginReg = IsEmail ? CurrentRegistration.Email : CurrentRegistration.Phone;
        IsPhone = !IsEmail;
        Phone = UserName;
        if (IsEmail && LoginEntry != null && LoginEntry.Behaviors.Count > 0)
        {
             LoginEntry.Behaviors.Clear();
            Phone = UserName;
            LoginEntry.Text = UserName.RemoveWhitespace();

        }
        else if(IsPhone && LoginEntry!=null  && LoginEntry.Behaviors.Count==0)
        {
            
                LoginEntry.Behaviors.Add(Mask);
        }
        if (IsPhone)
            return true;
        
        return false;

    }

    public static bool HasNonDigits(string input)
    {
        if (String.IsNullOrEmpty(input))// Если пустая, возвращаем true
            return true;

        foreach (char c in input.Replace(" ",""))
        {
            if (!Char.IsDigit(c))
            {

              
                return true; // Если найден нецифровой символ, возвращаем true
            }
        }


        return false; // Если все символы являются цифрами, возвращаем false
    }

    public LoginViewModel()
    {

       string  temp= Preferences.Get(nameof(CurrentLoginReg),CurrentRegistration.Email.ToString());
        CurrentRegistration cs;
        Enum.TryParse(temp, out cs );
        CurrentLoginReg = cs;
        

        if (!string.IsNullOrWhiteSpace(SaveCulture))
        {
            switch (SaveCulture)
            {
                case "kk-KZ":
                    LocalizationResourceManager.Current.CurrentCulture=new System.Globalization.CultureInfo("kk-KZ");
                    break;

                case "en-US":
                    LocalizationResourceManager.Current.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                    break;
            }
        }

        UserName = Preferences.Get(nameof(UserName), "");
        Password = Preferences.Get(nameof(Password), "");
      
   

            Password = Preferences.Get(nameof(Password), "");
            UserName = Preferences.Get(nameof(UserName), "");
        WeakReferenceMessenger.Default.Register<ChangeCurrentUser>(this, async (sender, message) =>
        {
            string temp = Preferences.Get(nameof(CurrentLoginReg), CurrentRegistration.Email.ToString());
            CurrentRegistration cs;
            Enum.TryParse(temp, out cs);
            CurrentLoginReg = cs;
            switch (CurrentLoginReg)
            {
                case CurrentRegistration.Email:
                    UserName = Preferences.Get(nameof(UserName), "");
                    IsEmail = true;
                    IsPhone = false;
                    //CurrentLoginTitle = "Войти по номеру телефону";
                    break;
                case CurrentRegistration.Phone:
                    Phone = Preferences.Get(nameof(Phone), "");
                    IsPhone = true;
                    IsEmail = false;
                    //CurrentLoginTitle = "Войти по Электронной почте";
                    break;
            }
            Password = Preferences.Get(nameof(Password), "");
            UserName = Preferences.Get(nameof(UserName), "");
            if (RealmService.CurrentUser != null && await VeryifyEmailAndPassword() && !string.IsNullOrWhiteSpace(CurrentShopID) && !string.IsNullOrWhiteSpace(Token) && !string.IsNullOrWhiteSpace(DateLogin))
            {
                IsBusy = true;


                if ((DateTime.UtcNow.Subtract(DateTime.Now).TotalSeconds) > double.Parse(DateLogin))
                {
                    DoLogin();
                    return;
                }

                IsBusy = true;
                await Task.Delay(100);
                IsBusy = false;
                await GoToMainPage();
            }
            else
                IsBusy = false;
            await Task.CompletedTask;
        });

    }

/*    [RelayCommand]
    public async Task ChangeLogin(CurrentRegistration current)
    {
        switch(current)
        {
            case CurrentRegistration.Email:
                NextLogin = CurrentRegistration.Phone;
                CurrentLoginTitle = "Войти по номеру телефону";
                IsEmail = true;
                IsPhone = false;
                CurrentLoginReg = CurrentRegistration.Email;
                break;
            case CurrentRegistration.Phone:
                NextLogin = CurrentRegistration.Email;
                CurrentLoginTitle = "Войти по Электронной почте";
                IsPhone=true;
                IsEmail = false;
                CurrentLoginReg = CurrentRegistration.Phone;
                break;
        }
    }*/


    [RelayCommand]
    public async Task OnAppearing()
    {


        await RealmService.Init();


        if (RealmService.CurrentUser != null && await VeryifyEmailAndPassword() && !string.IsNullOrWhiteSpace(CurrentShopID) && !string.IsNullOrWhiteSpace(Token) && !string.IsNullOrWhiteSpace(DateLogin))
        {


            IsBusy = true;

            if ((DateTime.UtcNow.Subtract(DateTime.Now).TotalSeconds) >double.Parse(DateLogin) )

            {
                IsBusy = true;
                DoLogin();
                return;
            }

            await Task.Delay(100);

            await GoToMainPage();
        }
        else
            IsBusy = false;
    }

    // Обработчик команды вход
    [RelayCommand]
    public async Task Login()
    {
        if (!await VeryifyEmailAndPassword())
        {
            return;
        }
        await DoLogin();
    }

    // Обработчик команды регистрации сброса пароля
    [RelayCommand]
    public async Task SignUp(string param)
    {
        await DoSignup(param);
    }

    // Вход
    private async Task DoLogin()
    {
        IsBusy = true;

     

            try
            {
                switch (CurrentLoginReg)
                {
                    case CurrentRegistration.Email:
                      
                        Token = await GosyncServise.Login((UserName).RemoveWhitespace(), Password, AuthType.Email);
                        break;
                    case CurrentRegistration.Phone:
                        Token = await GosyncServise.Login(UserName.Replace(" ", ""), Password, AuthType.Phone);
                        break;
                }
            }catch(Exception ex)
            {
                if (ex.Message.ToLower().Contains("404"))
                {
                    await DialogService.ShowAlertAsync(AppResources.ErrorLabel, AppResources.DataLoginErrorLabel, "OK");
                    IsBusy = false;
                    return;
                }
            await DialogService.ShowAlertAsync($"{AppResources.EnterError}", $"{AppResources.LoginError}", "Ok");
            IsBusy = false;
            return;
        }
            


          
            DateLogin = (DateTime.UtcNow.Subtract(DateTime.Now).TotalSeconds + 3600 * 12).ToString();
            await RealmService.LoginAsync(Token);
        
     
        await GoToMainPage();
    }

  
    // Регистрация сброс пароля
    private async Task DoSignup(string param)
    {
        // нужно перенести регистрацию в отдельный попап
        try
        {
            if (param == "0")// регистрация
                Application.Current.MainPage = new RegistView();
            else // Сброс пароля
                 Application.Current.MainPage = new ChangePasswordView();

        }
        catch (Exception ex)
        {
            if (param == "0")
                await DialogService.ShowAlertAsync("Sign up failed", ex.Message, "Ok");
            else // вывод ошибка прии сбросе пароля
            {
                await DialogService.ShowToast(AppResources.TryLaterLabel);
            }
            return;
        }

     
    }

    // Проверка заполнены ли данные пользователя
    private async Task<bool> VeryifyEmailAndPassword()
    {
        if (string.IsNullOrEmpty(ReturnCurrentLogin()) || string.IsNullOrEmpty(Password))
        {
            //await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $" {AppResources.FieldsEmptyError}", "Ok");
            return false;
        }
      

        return true;
    }

    private string ReturnCurrentLogin()
    {
        var result = "";
        switch (CurrentLoginReg)
        {
            case CurrentRegistration.Email:
                result = UserName.ToLower();
                break;
            case CurrentRegistration.Phone:
                result=UserName.Replace(" ","");
                break;
        }
        return result;
    }




    // Перехож к основному меню
    private async Task GoToMainPage()
    {

        Preferences.Set(nameof(Password), Password.RemoveWhitespace());
       
        Preferences.Set(nameof(UserName), UserName.RemoveWhitespace());
        realm = RealmService.GetMainThreadRealm();

        CurrentShopID = realm.All<Shop>().ToList().First()?.Id;
        Application.Current.MainPage = new AppShell();
        IsBusy = false;
        await Task.CompletedTask;
    }
}

