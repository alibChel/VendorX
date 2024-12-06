using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Vendor.Models;
using Vendor.Services;
using System.Linq;
using Realms.Sync;
using Rg.Plugins.Popup.Services;
using VendorX.Resources;
using VendorX.Services;
using Xamarin.Essentials;
using P42.Utils;
using static Xamarin.Essentials.Permissions;
using ZXing;
using Realms.Logging;
using Xamarin.Forms;
//using VendorX.Resources;
namespace Vendor.ViewModels;

public partial class LinkMailOrPhoneViewModel : BaseViewModel
{

    [ObservableProperty]
    bool isCodeSend;

    [ObservableProperty]
    Profile profile;

    [ObservableProperty]
    string login, code, password;

    [ObservableProperty]
    bool isEmail, isPhone, isNeedPass;

    [ObservableProperty]
    string sendCodeText, btnText = $"{AppResources.ResetPwdLabel}";

    [ObservableProperty]
    AuthType currentAuthType;

    private Realms.Realm realm;

    [ObservableProperty]
    private int smsRequestTimeOut = 181;
    private bool TimerAlive = true;

    private string LoginWithoutSpace { get { if (CurrentAuthType == AuthType.Email) return Login.RemoveWhitespace().ToLower(); return Login.Replace(" ", "").ToLower(); } } 


    public LinkMailOrPhoneViewModel()
    {

        realm = RealmService.GetMainThreadRealm();
        Profile = realm.All<Profile>().FirstOrDefault();

        IsEmail = CurrentAuthType == AuthType.Email;
        IsPhone = CurrentAuthType == AuthType.Phone;

        Password = Preferences.Get("Password", "");


        if (string.IsNullOrEmpty(Password))
        {
            //сделать поле для ввода пароля
            IsNeedPass = true;
        }
    }

    // Закрыть
    [RelayCommand]
    private async Task CloseTapped()
    {

        await PopupNavigation.Instance.PopAsync();
    }



    [RelayCommand]
    private async Task SaveChange()
    {

        if (string.IsNullOrWhiteSpace(Code))
        {
            await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.EnterСonfirmationСode}", "ОК");
            return;
        }


        IsBusy = true;



        var Result = false;
        switch (CurrentAuthType)
        {

            case AuthType.Email:
                Result = await GosyncServise.ConfirmEmailCode(Code.RemoveWhitespace(), LoginWithoutSpace);
                break;
            case AuthType.Phone:
                Result = await GosyncServise.ConfirmSmsCode(Code.RemoveWhitespace(), LoginWithoutSpace);

                break;
        }
        if (!Result)
        {
            IsBusy = false;


            await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.BindingError} {AppResources.NotCorrectCodeError}", "ОК");

            return;
        }



        string currentLogin = "";
        switch (CurrentAuthType)
        {
            case AuthType.Email:
                currentLogin = Profile.Phone;
                break;
            case AuthType.Phone:
                currentLogin = Profile.Login;
                break;
        }



        Result = await GosyncServise.BindHandle(currentLogin, LoginWithoutSpace, Password, CurrentAuthType);


        if (!Result)
        {
            IsBusy = false;


            await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.BindingError} {AppResources.ValidityError}", "OK");
            //"Ошибка", "Ошибка смены пароля: Извините, не удалось сменить пароль. Пожалуйста, проверьте правильность введенных данных и убедитесь, что вы подключены к интернету.", "ОК");
            await Task.Delay(300);
            await CloseTapped();

            return;
        }
     
        await realm.WriteAsync(() =>
        {
            switch (CurrentAuthType)
            {
                case AuthType.Email:
                    Profile.Login = LoginWithoutSpace;
                    break;
                case AuthType.Phone:
                    Profile.Phone = LoginWithoutSpace;
                    break;
            }



        });
        await Task.Delay(250);
        await CloseTapped();
        IsBusy = false;
    }

    internal void OnAppearing()
    {
        //User = RealmService.CurrentUser;

    }

    [RelayCommand]
    private async void SendCode()
    {
        switch (CurrentAuthType)
        {
            case AuthType.Email:
                if (!IsValidEmail(LoginWithoutSpace))
                {
                    await DialogService.ShowToast($"{AppResources.BindingError} {AppResources.WrongEmail}");

                    //IsBusy = false; 
                    return;
                }
                break;
            case AuthType.Phone:
                if (!IsValidPhone(LoginWithoutSpace))
                {
                    await DialogService.ShowToast($"{AppResources.BindingError} {AppResources.WrongNumber}");

                    //IsBusy = false; 
                    return;
                }
                break;
        }


        if (string.IsNullOrWhiteSpace(LoginWithoutSpace))
        {
            await DialogService.ShowToast($"{AppResources.ErrorLabel}  {AppResources.FieldsEmptyError}");
            return;
        }

        if (Password.Length < 8 || !ValidatePassword(Password))
        {
            await DialogService.ShowToast($"{AppResources.BindingError} {AppResources.PwdLess8Error}");
            //IsBusy = false;
            return;
        }

      
        IsBusy = true;


        try
        {
            switch (CurrentAuthType)
            {
                case AuthType.Email:
                    IsCodeSend = await GosyncServise.SendEmailCode(LoginWithoutSpace, $"{Profile?.FullName}");
                    break;
                case AuthType.Phone:
                    IsCodeSend = await GosyncServise.SendSmsCode(LoginWithoutSpace, $"{AppResources.ConfirmCodeVendorLabel} %CODE%", true);
                    break;
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("exist"))
                await DialogService.ShowAlertAsync(AppResources.ErrorLabel, AppResources.UserAlreadyExist, "ОК");
            return;
        }

        IsBusy = false;
        if (!IsCodeSend)
        {
            //IsBusy = false;

            await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.BindingError} {AppResources.ValidityError}", "ОК");

            await Task.Delay(250);
            await CloseTapped();
            return;
        }

        SmsRequestTimeOut = 181;
        TimerAlive = true;
        Device.StartTimer(TimeSpan.FromSeconds(1), timer1_Tick);


    }



    [RelayCommand]
    public async void ReSendCode()
    {
        if (!IsCodeSend)
        {


            IsBusy = true;
            try
            {
                switch (CurrentAuthType)
                {
                    case AuthType.Email:
                        if (string.IsNullOrWhiteSpace(LoginWithoutSpace) || !IsValidEmail(LoginWithoutSpace))
                        {
                            await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.BindingError} {AppResources.WrongEmail}", "ОК");
                            IsBusy = false;
                            return;
                        }

                        IsCodeSend = await GosyncServise.SendEmailCode(Login, $"{Profile?.FullName}");
                        break;

                    case AuthType.Phone:
                        if (string.IsNullOrWhiteSpace(LoginWithoutSpace) || !IsValidPhone(LoginWithoutSpace))
                        {
                            await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.BindingError} {AppResources.WrongNumber}", "ОК");
                            IsBusy = false;
                            return;
                        }
                        IsCodeSend = await GosyncServise.SendSmsCode(LoginWithoutSpace, $"{AppResources.ConfirmCodeVendorLabel} %CODE%", true); ;
                        break;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("exist"))
                    await DialogService.ShowAlertAsync(AppResources.ErrorLabel, AppResources.UserAlreadyExist, "ОК");
                return;
            }


            IsBusy = false;
            if (!IsCodeSend)
            {
                //IsBusy = false;

                await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.BindingError} {AppResources.ValidityError}", "ОК");

                await Task.Delay(250);
                await CloseTapped();
            }
            SendCodeText = $"{AppResources.RepeatCodeLabel}";
            //"Отправить код повторно";
            BtnText = $"{AppResources.ResetPwdLabel}";
            //"Сбросить пароль";
            SmsRequestTimeOut = 181;
            TimerAlive = true;
            Device.StartTimer(TimeSpan.FromSeconds(1), timer1_Tick);


        }
        else
        {
            if (TimerAlive)
            {

                await DialogService.ShowAlertAsync("", $"{AppResources.RepeatSendCodeError} " + SmsRequestTimeOut + $" {AppResources.SecondLabel}", "OK");
                return;
                //$"Повторная отправка кода будет доступна через {SmsRequestTimeOut} секунд. Пожалуйста, подождите", "OK");

            }
            try
            {
                switch (CurrentAuthType)
                {
                    case AuthType.Email:
                        if (string.IsNullOrWhiteSpace(LoginWithoutSpace) || !IsValidEmail(LoginWithoutSpace))
                        {
                            await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.WrongEmail}", "ОК");
                            IsBusy = false;
                            return;
                        }

                        IsCodeSend = await GosyncServise.SendEmailCode(LoginWithoutSpace, $"{Profile?.FullName}");



                        break;


                    case AuthType.Phone:
                        if (string.IsNullOrWhiteSpace(LoginWithoutSpace) || !IsValidPhone(LoginWithoutSpace))
                        {
                            await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.WrongNumber}", "ОК");
                            IsBusy = false;
                            return;
                        }
                        IsCodeSend = await GosyncServise.SendSmsCode(LoginWithoutSpace, $"{AppResources.ConfirmCodeVendorLabel} %CODE%", true); ;
                        break;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("exist"))
                    await DialogService.ShowAlertAsync(AppResources.ErrorLabel,  AppResources.UserAlreadyExist, "ОК");
                return;
            }

            if (!IsCodeSend)
            {
                //IsBusy = false;

                await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.ValidityError}", "ОК");
                await Task.Delay(250);
                /*  await CloseTapped();*/
            }
            switch (CurrentAuthType)
            {
                case AuthType.Email:
                    await DialogService.ShowAlertAsync("", $"{AppResources.NewCodeSendError}", "ОК");
                    break;
                case AuthType.Phone:
                    await DialogService.ShowAlertAsync("", $"{AppResources.NewPhoneCodeSend}", "ОК");
                    break;
            }


            SmsRequestTimeOut = 181;
            TimerAlive = true;
            Device.StartTimer(TimeSpan.FromSeconds(1), timer1_Tick);
        }
    }
    private bool timer1_Tick()
    {

        Device.BeginInvokeOnMainThread(async () =>
        {
            if (SmsRequestTimeOut > 0)
            {
                TimerAlive = true;
                /*  ButtonSMS.IsVisible = false;
                  SmsTimeCounter.IsVisible = true;
                  TimerVisible = true;*/
                /*                SmsTimeCounter.Text = $"{SmsRequestTimeOut} c";*/

                SmsRequestTimeOut--;
                OnPropertyChanged(nameof(TimerAlive));

            }
            else
            {
                /*  TimerVisible = false;
                  ButtonSMS.IsVisible = true;

                  SmsTimeCounter.IsVisible = false;*/


                TimerAlive = false;

            }
        });
        return TimerAlive;


    }


   
}

