using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using P42.Utils;
using Rg.Plugins.Popup.Services;
using VendorX;
using VendorX.Services;
using VendorX.Views;
using Xamarin.CommunityToolkit.Behaviors;
using Xamarin.Essentials;
using Xamarin.Forms;

using static Xamarin.Essentials.Permissions;
using VendorX.Resources;

namespace Vendor.ViewModels;

public partial class ChengePasswordViewModel : BaseViewModel
{

    [ObservableProperty]
    private Realms.Realm realm;

    public static MaskedBehavior Mask = new MaskedBehavior
    {
        Mask = "XXX XXX XX XX",
        UnMaskedCharacter = 'X',
    };


    [ObservableProperty]
    Profile profile;

    [ObservableProperty]
    private bool result, isCodeVisibl, isPassVis = true;

    //TODO использование Email, для сброса пароля
    [ObservableProperty]
    string email, password, sendCodeText, btnText = $"{AppResources.ResetPwdLabel}", code, currentTitle= $"{AppResources.ResetPwdByPhoneLabel}", phone;


    [ObservableProperty]
    private CurrentRegistration isEmailLogin = CurrentRegistration.Email;
    [ObservableProperty]
    private CurrentRegistration isPhoneLogin = CurrentRegistration.Phone;

    [ObservableProperty]
    CurrentRegistration nextRegister = CurrentRegistration.Phone;

    [ObservableProperty]
    bool isEmail = true, isPhone;
    [ObservableProperty]
    private CurrentRegistration currentChangeReg = CurrentRegistration.Email;

    private string currentCodeRegion = "7";


    private int SmsRequestTimeOut = 181;
    private bool TimerAlive = true;

    private string userName;

    [ObservableProperty]
    Entry loginEntry;



    public string UserName { get => userName; set { userName = IsPhoneOrMail(value) ? value :  value.RemoveWhitespace(); OnPropertyChanged(nameof(UserName)); } }


    private bool IsPhoneOrMail(string st)
    {
        //Regex validatePhoneNumberRegex = new Regex("^\\+?[Aa-Zz][Aa-Zz]{10,14}$");
        if (Phone == UserName)
            return false;


        IsEmail = HasNonDigits(st);
        IsPhone = !HasNonDigits(st);
        CurrentChangeReg = HasNonDigits(st) ? CurrentRegistration.Email : CurrentRegistration.Phone;
        Email = UserName;
        Phone = UserName;
        if (IsEmail && LoginEntry != null && LoginEntry.Behaviors.Count > 0)
        {
            LoginEntry.Behaviors.Clear();
            Phone = UserName;
            LoginEntry.Text = UserName.RemoveWhitespace();

        }
        else if (IsPhone && LoginEntry != null && LoginEntry.Behaviors.Count == 0)
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

        foreach (char c in input.Replace(" ", ""))
        {
            if (!Char.IsDigit(c))
            {
                return true; // Если найден нецифровой символ, возвращаем true
            }
        }

        return false; // Если все символы являются цифрами, возвращаем false
    }


    public ChengePasswordViewModel()
    {
        SendCodeText = $"{AppResources.SendCodeConfirmLabel}"; 
            //"Отправить код подтверждения";
        IsCodeVisibl = false;
        IsPassVis = false;

    }

    // Закрыть
    [RelayCommand]
    private async Task CloseTapped()
    {
        //Result = false;
        //await PopupNavigation.Instance.PopAsync();
        Application.Current.MainPage = new LoginPage();
    }

    //Пере отправка кода
    [RelayCommand]
    public async void ReSendCode()
    {
        if (!IsCodeVisibl)
        {


            
             IsBusy = true;
                switch (CurrentChangeReg)
                {
                    case CurrentRegistration.Email:
                        if (string.IsNullOrWhiteSpace(UserName.RemoveWhitespace()) || !IsValidEmail(UserName.RemoveWhitespace()))
                        {
                            await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.WrongEmail}", "ОК");
                            IsBusy = false;
                            return;
                        }
                        IsCodeVisibl = await SendEmailCode(UserName.RemoveWhitespace(), UserName.RemoveWhitespace());
                        break;

                    case CurrentRegistration.Phone:
                        if (string.IsNullOrWhiteSpace(UserName.Replace(" ", "")) || !IsValidPhone(UserName.Replace(" ", "")))
                        {
                            await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.WrongNumber}", "ОК");
                            IsBusy = false;
                            return;
                        }
                        IsCodeVisibl = await SendSmsCode(UserName.Replace(" ", ""));
                        break;
                }
               

                IsBusy = false;
                if (!IsCodeVisibl)
                {
                    //IsBusy = false;

                    await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.NotSuccesRestorePwdError}", "ОК");

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
                IsPassVis = true;
            
        }
        else
        {
            if (TimerAlive)
            {

                await DialogService.ShowAlertAsync("",$"{AppResources.RepeatSendCodeError} " + SmsRequestTimeOut + $" {AppResources.SecondLabel}","OK");
                //$"Повторная отправка кода будет доступна через {SmsRequestTimeOut} секунд. Пожалуйста, подождите", "OK");

            }
            else
            {
                switch (CurrentChangeReg)
                {
                    case CurrentRegistration.Email:
                        IsCodeVisibl = await SendEmailCode(UserName.RemoveWhitespace(), UserName.RemoveWhitespace());
                        break;

                    case CurrentRegistration.Phone:
                        IsCodeVisibl = await SendSmsCode(UserName.Replace(" ", ""));
                        break;
                }
                if (!IsCodeVisibl)
                {
                    //IsBusy = false;

                    await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.NotSuccesRestorePwdError}", "ОК");
                    await Task.Delay(250);
                    /*  await CloseTapped();*/
                }
                switch (CurrentChangeReg)
                {
                    case CurrentRegistration.Email:
                        await DialogService.ShowAlertAsync("", $"{AppResources.NewCodeSendError}", "ОК");
                        break;
                    case CurrentRegistration.Phone:
                        await DialogService.ShowAlertAsync("", $"{AppResources.NewPhoneCodeSend}", "ОК");
                        break;
                }
             

                SmsRequestTimeOut = 181;
                TimerAlive = true;
                Device.StartTimer(TimeSpan.FromSeconds(1), timer1_Tick);
            }


        }




    }
    ////Смена типа сброса пароля
    //[RelayCommand]
    //async void ChangeRegistration(CurrentRegistration current)
    //{

    //    switch (NextRegister)
    //    {
    //        case CurrentRegistration.Email:
    //            currentChangeReg = CurrentRegistration.Email;
    //            NextRegister = CurrentRegistration.Phone;
    //            CurrentTitle = "Cбросить пароль по номеру телефону";
 
    //            IsEmail = true;
    //            IsPhone = false;
    //            Phone = "";
    //            break;
    //        case CurrentRegistration.Phone:
    //            currentChangeReg = CurrentRegistration.Phone;
    //            NextRegister = CurrentRegistration.Email;
    //            CurrentTitle = "Cбросить пароль по электронной почте";
    //            IsPhone = true;
    //            IsEmail = false;
    //            Email = "";
    //            break;
    //    }

    //}


    private async Task<bool> SendEmailCode(string email, string login)
     {
        bool result;
        try
        {
            result = await GosyncServise.SendEmailCode(email, login, false);
        }
        catch (Exception ex)
        {
            result = false;

        }

        return result;
    }
    private async Task<bool> SendSmsCode(string number)
    {
        bool result;
        try
        {
            result = await GosyncServise.SendSmsCode(number: number, message: $"{AppResources.ConfirmCodeVendorLabel} %CODE%");
        }
        catch (Exception ex)
        {
            result = false;

        }

        return result;
    }



    private async Task<bool> ConfirmCode(string code)
    {
        bool result;
        try
        {
            result = await GosyncServise.ConfirmEmailCode(code, UserName.RemoveWhitespace());
        }
        catch (Exception ex)
        {
            result = false;


        }

        return result;
    }

    private async Task<bool> ConfirmCode(string code, string number)
    {
        bool result;
        try
        {
            result = await GosyncServise.ConfirmSmsCode(code: code, number: number);
        }
        catch (Exception ex)
        {
            result = false;

        }

        return result;
    }


    private async Task <bool> ResetPassword(string handle,string password,AuthType auth)
    {
        bool result;
        try
        {
            result = await GosyncServise.ResetPswd(handle:handle,newpswd:password,auth);
        }
        catch (Exception ex)
        {
            result = false;

        }

        return result;

    }

    // Смена пороля и проверка кода
    [RelayCommand]
    private async Task SaveChange()
    {
        var auth = AuthType.Email;
        try
        {

            switch (CurrentChangeReg)
            {

                case CurrentRegistration.Email:
                    if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Code))
                    {
                        await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.ChangePasswordError} {AppResources.FieldsEmptyError}", "ОК");
                        IsBusy = false;
                        return;
                    }


                    if (!IsValidEmail(UserName.RemoveWhitespace()))
                    {
                        await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.ChangePasswordError} {AppResources.WrongEmail}", "ОК");
                        IsBusy = false;
                        return;
                    }

                    break;
                case CurrentRegistration.Phone:
                    if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password)  || string.IsNullOrWhiteSpace(Code))
                    {
                        await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.ChangePasswordError} {AppResources.FieldsEmptyError}", "ОК");
                        IsBusy = false;
                        return;
                    }


                    if (!IsValidPhone(UserName.Replace(" ", "")))
                    {
                        await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.ChangePasswordError} {AppResources.WrongNumber}", "ОК");
                        IsBusy = false;
                        return;
                    }

                    break;

            }

           
            if (Password.Length < 8 || !ValidatePassword(Password))
            {

                IsBusy = false;

                await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.PwdLess8Error}", "OK");

                    //"Ошибка", "Ошибка смены пароля: Слишком короткий пароль. Пароль должен содержать не менее 8 символов и хотя бы одну заглавную букву и цифру.", "ОК");

                return;
            }

           

           
            IsBusy = true;
           
            switch (CurrentChangeReg)
            {

                case CurrentRegistration.Email:
                    Result = await ConfirmCode(Code.RemoveWhitespace());
                    break;
                case CurrentRegistration.Phone:
                    Result= await ConfirmCode(Code.RemoveWhitespace(), UserName.Replace(" ",""));   
                    auth= AuthType.Phone;
                    break;
            }
            if (!Result)
            {
                IsBusy = false;


                await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.ChangePasswordError} {AppResources.NotCorrectCodeError}", "ОК");

                return;
            }
            Result = await ResetPassword(UserName.Replace(" ","").RemoveWhitespace(), Password,auth);


            if (!Result)
                {
                    IsBusy = false;


                    await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.NotSuccesRestorePwdError}", "OK");
                        //"Ошибка", "Ошибка смены пароля: Извините, не удалось сменить пароль. Пожалуйста, проверьте правильность введенных данных и убедитесь, что вы подключены к интернету.", "ОК");


                    return;
                }
                //IsBusy = false;

             
                //await RealmService.ChangePasswordAsync(Email);
                // await PopupNavigation.Instance.PopAsync();


            
        }
        catch (Exception)
        {
            IsBusy = false;

            await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.NotSuccesRestorePwdError}", "OK");
            //"Ошибка", "Ошибка смены пароля: Извините, не удалось сменить пароль. Пожалуйста, проверьте правильность введенных данных и убедитесь, что вы подключены к интернету.", "ОК");

            return;
        }


        //IsBusy = true;
        
        try
           
        {

            Token = await GosyncServise.Login(UserName.Replace(" ","").RemoveWhitespace().ToLower(), Password,auth:auth);

            DateLogin = (DateTime.UtcNow.Subtract(DateTime.Now).TotalSeconds + 3600 * 12).ToString();
           await RealmService.LoginAsync(Token);
        }
        catch (Exception ex)
        {
            IsBusy = false;


            await DialogService.ShowAlertAsync($"{AppResources.EnterError}", $"{AppResources.LoginError}", "OK");
            return;
                //"Ошибка входа", "К сожалению, мы не можем войти в систему с данными учетными данными. Пожалуйста, убедитесь, что вы вводите правильный логин и пароль, и повторите попытку", "Ok");



        }
       
    
      
        
      
        //await PopupNavigation.Instance.PopAsync();
       // WeakReferenceMessenger.Default.Send(new ChangeCurrentUser(true));
       
       

       
        switch (CurrentChangeReg)
        {
            case CurrentRegistration.Email:
                Preferences.Set("UserName", UserName.ToLower());
                break;
            case CurrentRegistration.Phone:
                Preferences.Set(nameof(Phone), UserName.Replace(" ",""));
                break;
        }
        Preferences.Set(nameof(Password), Password);

        Preferences.Set("CurrentLoginReg", CurrentChangeReg.ToString());
        await DialogService.ShowAlertAsync("", $"{AppResources.SuccesRestorePwdLabel}", "OK");

        Application.Current.MainPage = new AppShell();
        
        //await DialogService.ShowToast("Пароль успешно изменен. Теперь вы можете использовать новый пароль для входа в свой аккаунт.");
        IsBusy = false;
        await Task.CompletedTask;
       
        //  

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

    internal void OnAppearing()
    {
        //User = RealmService.CurrentUser;

        //Photo = Profile?.PhotoUrl;
        //Fname = Profile?.FirstName;
        //Lname = Profile?.LastName;
    }
}