using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Vendor.Services;
using System.Linq;
using Realms.Sync;
using System.ComponentModel;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using VendorX.Services;
using System.Text;
using Xamarin.Forms;
using P42.Utils;
using CommunityToolkit.Mvvm.Messaging;
using System.Data;
using VendorX.Views;
using VendorX;

using System.Collections.Specialized;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Behaviors;


using VendorX.Resources;

namespace Vendor.ViewModels;

public partial class RegistViewModel : BaseViewModel
{
    public static MaskedBehavior Mask = new MaskedBehavior
    {
        Mask = "XXX XXX XX XXXXXXX",
        UnMaskedCharacter = 'X',
    };


    private Realms.Realm realm;
  


    [ObservableProperty]
    Profile profile;
    
    [ObservableProperty]
    private int result;
    
    [ObservableProperty]
    string fname, lname, password,code, btnText,phone;//, rePassword
    
    [ObservableProperty]
    string photo;
    
    [ObservableProperty]
    bool codeSend, emailSend;

    //[ObservableProperty]
    //string currentRegisTitle = "Зарегистрироваться по номеру телефону";

    [ObservableProperty]
    CurrentRegistration nextRegister = CurrentRegistration.Phone;


    [ObservableProperty]
    private CurrentRegistration isEmailLogin = CurrentRegistration.Email;
    [ObservableProperty]
    private CurrentRegistration isPhoneLogin = CurrentRegistration.Phone;

    //Для отображения entry xaml

    /*    private bool isEmail { get { return CurrentRegistration == CurrentRegistration.Email; } }
        private bool isPhone { get { return CurrentRegistration == CurrentRegistration.Phone; } }
        public bool IsPhone { get { return isPhone; } set { OnPropertyChanged(nameof(IsPhone)); } }
        public bool IsEmail { get { return isEmail; } set { OnPropertyChanged(nameof(IsEmail)); } }*/

    private int SmsRequestTimeOut = 181;
    private bool TimerAlive = true;
    private string userName;

    [ObservableProperty]
    bool isEmail=true   , isPhone;
    [ObservableProperty]
    private CurrentRegistration currentLoginReg = CurrentRegistration.Email;

    private string currentCodeRegion = "7";

    [ObservableProperty]
    Entry loginEntry;



    public string UserName { get => userName; set { userName = IsPhoneOrMail(value) ? value : userName = value.RemoveWhitespace(); OnPropertyChanged(nameof(UserName)); } }


    private bool IsPhoneOrMail(string st)
    {
        //Regex validatePhoneNumberRegex = new Regex("^\\+?[Aa-Zz][Aa-Zz]{10,14}$");
        if (Phone == UserName)
            return false;


        CurrentLoginReg = HasNonDigits(st) ? CurrentRegistration.Email : CurrentRegistration.Phone;
        IsEmail = HasNonDigits(st);
        IsPhone = !HasNonDigits(st);
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



    public RegistViewModel()
	{

        //Profile = realm.Find<Profile>(User.Id);
        BtnText = $"{AppResources.RegistrationLabel}";
        Title = $"{AppResources.RegistrationLabel}";
    }

    // Закрыть
    [RelayCommand]
    private async Task CloseTapped()
    {
        Application.Current.MainPage = new LoginPage();
        //await PopupNavigation.Instance.PopAsync();
    }

    // Выбор фото
    [RelayCommand]
    private async Task TakePhoto()
    {
        var res = await PickPhoto();
        if(res != null && res.Count >0)
            Photo = res.FirstOrDefault();
        
    }

    private async Task<Profile> GetProfile()
    {
        
        Profile = realm.All<Profile>().FirstOrDefault();
        if (Profile == null)
             await GetProfile();
        return Profile;
    }


  
    


    // подверждения кода 
    internal async void ConfirmCode()
    {
        /*try
        {
            IsBusy = true;
            Token = await CosyncService.ConfirmSignUp(UserName, Code.RemoveWhitespace());
            DateLogin= (DateTime.UtcNow.Subtract(DateTime.Now).TotalSeconds + 3600 * 24).ToString();
            //await RealmService.LoginAsync(Token);
            SuccesRegister();

        }
        catch (Exception ex)
        {
            Code = "";
            IsBusy = false;
            await DialogService.ShowToast($"{AppResources.RegisterError} "+$"{AppResources.NotCorrectCodeError}");
                
                
              //  "Ошибка регистрации: Введенный вами код подтверждения неверный или устарел. Пожалуйста, убедитесь, что вы вводите последний выданный вам код подтверждения, и повторите попытку.");
            return;
        }*/
        bool res = false;
        var authType = AuthType.Email;
        switch (CurrentLoginReg)
        {
            case CurrentRegistration.Phone:
                res = await ConfirmCode(code: Code.RemoveWhitespace(), number: $"{currentCodeRegion}{Phone.Replace(" ", "")}");
                authType=AuthType.Phone;
                break;
            case CurrentRegistration.Email:
                res = await ConfirmCode(Code.RemoveWhitespace());
                break;
        }

        if (res)
        {
            IsBusy = true;
            try
            {
                Token = await GosyncServise.Register(string.IsNullOrWhiteSpace(UserName) ? Phone : UserName, Password, authType);

            }
            catch (Exception ex)
            {
                await DialogService.ShowToast($"{AppResources.UserAlreadyExist}");
                return;
            }
          
            DateLogin = (DateTime.UtcNow.Subtract(DateTime.Now).TotalSeconds + 3600 * 12).ToString();
            //await RealmService.LoginAsync(Token);
            SuccesRegister();
        }
        else
        {
            Code = "";
            IsBusy = false;
            await DialogService.ShowToast($"{AppResources.RegisterError} {AppResources.NotCorrectCodeError}");
            return;
        }


    }



    //Смена типа регистрации
    [RelayCommand]
    async void ChangeRegistration(CurrentRegistration current)
    {
        
        switch(NextRegister)
        {
            case CurrentRegistration.Email:
                CurrentLoginReg = CurrentRegistration.Email;
                NextRegister = CurrentRegistration.Phone;
                //CurrentRegisTitle = "Зарегистрироваться по электронной почте";
                IsEmail = true;
                IsPhone = false;
                Phone = "";
                break;
            case CurrentRegistration.Phone:
                CurrentLoginReg = CurrentRegistration.Phone;
                NextRegister = CurrentRegistration.Email;
                //CurrentRegisTitle = "Зарегистрироваться по номеру телефону";
                IsPhone = true;
                IsEmail=false;
                UserName="";
                break;
        }

    }


    [RelayCommand]
    private async void ConfirmCodee()
    {
        Result = int.Parse(Code.RemoveWhitespace());
        await PopupNavigation.Instance.PopAsync();
        //  return 2;
    }
    [RelayCommand]
    private async void CancelCode()
    {
        Result = -200;
        await PopupNavigation.Instance.PopAsync();
        //return -200;
    }
    //возврат к регистрации
    [RelayCommand]
    private void Back()
    {

        EmailSend = !EmailSend;
        BtnText = $"{AppResources.RegistrationLabel}";
        Title = $"{AppResources.RegistrationLabel}";

    }

    private async Task<string> RegisterUser()
    {
        return "";
    }
    private async Task<bool> SendEmailCode(string email,string login)
    {
        bool result;
        try 
        {
            result = await GosyncServise.SendEmailCode(email,login);
        }
        catch (Exception ex)
        {
            result= false;
            
        }

        return result;
    }
    private async Task<bool> SendSmsCode(string number, string message)
    {
        bool result;
        try
        {
            result = await GosyncServise.SendSmsCode(number:number, message:message,true);
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
            result= false;
            
        }

        return result;
    }

    private async Task<bool> ConfirmCode(string code,string number)
    {
        bool result;
        try
        {
            result = await GosyncServise.ConfirmSmsCode(code: code, number:number);
        }
        catch (Exception ex)
        {
            result = false;

        }

        return result;
    }


    // Регистрация
    [RelayCommand]
    private async Task SaveChange()
    {
        Phone = UserName;
        if(IsEmail)
        {
            try
            {
                if (CodeSend)
                {


                    ConfirmCode();
                    return;
                }

                if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password) /*|| string.IsNullOrWhiteSpace(RePassword)*/ || string.IsNullOrWhiteSpace(Fname) || string.IsNullOrWhiteSpace(Lname))
                {
                    //IsBusy = false;
                    await DialogService.ShowToast($"{AppResources.RegisterError} {AppResources.FieldsEmptyError}");
                    return;
                }

                if (!IsValidEmail(UserName))
                {
                    await DialogService.ShowToast($"{AppResources.RegisterError} {AppResources.WrongEmail}");

                    //IsBusy = false; 
                    return;
                }
                if (Password.Length < 8 || !ValidatePassword(Password))
                {
                    await DialogService.ShowToast($"{AppResources.RegisterError} {AppResources.PwdLess8Error}");
                    //IsBusy = false;
                    return;
                }
                /*if (!Password.Equals(RePassword))
                {
                    await DialogService.ShowToast("Ошибка регистрации: Введенные пароли не совпадают. Пожалуйста, убедитесь, что вы правильно ввели пароль и повторите попытку.");
                    RePassword = "";
                    //IsBusy = false;
                    return;

                }*/
                //IsBusy = true;

                UserName = UserName.ToLower();

                //await RealmService.RegisterAsync(UserName, Password);

                // await RealmService.LoginAsync(UserName, Password);

               CodeSend = await SendEmailCode(email: UserName, login: $"{Fname} {Lname}");
                     
               


                //IsBusy = false;
                if (!CodeSend)
                    await DialogService.ShowToast($"{AppResources.UserAlreadyExist}");
                else
                {
                    BtnText = $"{AppResources.CONFIRMLABEL}";
                    Title = $"{AppResources.ConfirmEmailLabel}";
                    SmsRequestTimeOut = 181;
                    TimerAlive = true;
                Device.StartTimer(TimeSpan.FromSeconds(1), timer1_Tick);
                }

            }
            catch (Exception ex)
            {

                //IsBusy = false; 

                await DialogService.ShowToast($"{ex.Message}");


            }
        }
        else if (IsPhone)
        {
            try
            {
                if (CodeSend)
                {

                    ConfirmCode();//phone
                    return;
                }

                if (string.IsNullOrWhiteSpace(Phone) || string.IsNullOrWhiteSpace(Password)  || string.IsNullOrWhiteSpace(Fname) || string.IsNullOrWhiteSpace(Lname))
                {
                    //IsBusy = false;
                    await DialogService.ShowToast($"{AppResources.RegisterError} {AppResources.FieldsEmptyError}");
                    return;
                }

                if (!IsValidPhone(Phone.Replace(" ","")))
                {
                    await DialogService.ShowToast($"{AppResources.RegisterError} {AppResources.WrongNumber}");
                    //IsBusy = false; 
                    return;
                }
                if (Password.Length < 8 || !ValidatePassword(Password))
                {
                    await DialogService.ShowToast($"{AppResources.RegisterError} {AppResources.PwdLess8Error}");
                    //IsBusy = false;
                    return;
                }
               
                CodeSend = await SendSmsCode(number: $"{currentCodeRegion}{Phone.Replace(" ", "")}", message: $"{AppResources.ConfirmCodeVendorLabel} %CODE%");//phone
                    
                

               
                if (!CodeSend)
                    await DialogService.ShowToast($"{AppResources.RegisterError} {AppResources.ValidityError}");
                else
                {
                    BtnText = $"{AppResources.CONFIRMLABEL}";
                    Title = $"{AppResources.ConfirmPhoneLabel}";
                    SmsRequestTimeOut = 181;
                    TimerAlive = true;
                    Device.StartTimer(TimeSpan.FromSeconds(1), timer1_Tick);
                }


            }
            catch (Exception ex)
            {
                //IsBusy = false; 

                await DialogService.ShowToast($"{ex.Message}");


            }
        }
        
        //await CloseTapped();

    }

    //Пере отправка кода
    [RelayCommand]
    public async void ReSendCode()
    {
        if (TimerAlive)
        {

            await DialogService.ShowAlertAsync("", $"{AppResources.RepeatSendCodeError} " + SmsRequestTimeOut + $" {AppResources.SecondLabel}", "OK");


            //$"Повторная отправка кода будет доступна через {SmsRequestTimeOut} секунд. Пожалуйста, подождите");

            IsBusy = false;
            return;
        }

        /*  var json_metadata = "";

          CodeSend = await CosyncService.SignUp(UserName, Password, json_metadata);*/

        switch (CurrentLoginReg)
        {
            case CurrentRegistration.Email:
                CodeSend = await SendEmailCode(email: UserName, login: $"{Fname} {Lname}");
                break;
            case CurrentRegistration.Phone:
                CodeSend = await SendSmsCode(number: $"{currentCodeRegion}{UserName.Replace(" ", "")}", message:$"{AppResources.ConfirmCodeVendorLabel} %CODE%");//phone
                break;
        }
       

        //IsBusy = false;

        if (!CodeSend) { 
            await DialogService.ShowToast($"{AppResources.RegisterError} {AppResources.SuccessRegisterError}");

            await Task.Delay(250);
            await PopupNavigation.Instance.PopAsync();
            IsBusy = false;
            return;     
        }
        else
        {

            BtnText = $"{AppResources.CONFIRMLABEL}";
            switch (CurrentLoginReg)
            {
                case CurrentRegistration.Email:
                    Title = $"{AppResources.ConfirmEmailLabel}";
                    break;
                case CurrentRegistration.Phone:
                    Title = $"{AppResources.ConfirmPhoneLabel}";//phone
                    break;
            }
           

            SmsRequestTimeOut = 181;
            TimerAlive = true;
            Device.StartTimer(TimeSpan.FromSeconds(1), timer1_Tick);
        }

        await DialogService.ShowToast(IsEmail? $"{AppResources.NewCodeSendError}" : $"{AppResources.NewPhoneCodeSend}");

    
    }

    private async void SuccesRegister()
    {
 
        try
        {

            await RealmService.Init();
            await RealmService.LoginAsync(Token);
            realm = RealmService.GetMainThreadRealm();
            await RealmService.SetSubscription(realm, SubscriptionType.Mine);
            

            var photourl = "";

            if (!string.IsNullOrWhiteSpace(Photo))
            {
                if (!Photo.Contains("lis.4dev.kz"))
                {
                    if (System.IO.File.Exists(Photo))
                    {
                        photourl = await RestService.PostImage(Photo);
                    }
                }
                else
                {
                    photourl = Photo;
                }
            }

            await Task.Delay(300);

            var userId = RealmService.CurrentUser.Identities[0].Id;
            await Task.Delay(300);

            Profile = realm.All<Profile>().FirstOrDefault();
            switch (CurrentLoginReg)
            {
                case CurrentRegistration.Email:
                    await realm.WriteAsync(() =>
                    {
                        Profile.Id = userId;
                        Profile.FirstName = Fname;
                        Profile.LastName = Lname;
                        Profile.Login = UserName;
                        Profile.OwnerId = userId;
                        Profile.PhotoUrl = photourl;
                    });
                    break;
                case CurrentRegistration.Phone:
                    await realm.WriteAsync(() =>
                    {
                        Profile.Id = userId;
                        Profile.FirstName = Fname;
                        Profile.LastName = Lname;
                        Profile.Phone = Phone.Replace(" ", "");
                        Profile.OwnerId = userId;
                        Profile.PhotoUrl = photourl;
                    });
                    break;
            }
        
         
           
         
            
           
        }
        catch (Exception ex)
        {
        
        
        
       

        }
        
        await Task.Delay(450);
        IsBusy = false;
        Preferences.Set(nameof(Password), Password);
        switch (CurrentLoginReg)
        {
            case CurrentRegistration.Phone:
                Preferences.Set(nameof(Phone), Phone.Replace(" ", ""));
                break;
            case CurrentRegistration.Email:
                Preferences.Set(nameof(UserName), UserName);
                break;
        }

        Preferences.Set(nameof(CurrentLoginReg), CurrentLoginReg.ToString());
      
        Application.Current.MainPage = new AppShell();
        await DialogService.ShowToast($"{AppResources.SuccesRegisterLabel}");
            //"Регистрация завершина");
       // WeakReferenceMessenger.Default.Send(new ChangeCurrentUser(true));
    }



    //timer
    private bool timer1_Tick()
    {

        Device.BeginInvokeOnMainThread(async () =>
        {
            if (SmsRequestTimeOut > 0)
            {
                TimerAlive = true;

              
                SmsRequestTimeOut--;
                OnPropertyChanged(nameof(TimerAlive));

            }
            else
            {
  


                TimerAlive = false;

            }
        });
        return TimerAlive;


    }

    internal async void OnAppearing()
    {
     

    }
}

