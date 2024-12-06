
using Realms.Sync;
using CommunityToolkit.Mvvm.Messaging;

using Rg.Plugins.Popup.Services;

using static Xamarin.Essentials.Permissions;
using Xamarin.Essentials;
using P42.Utils;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.CommunityToolkit.Behaviors;


using VendorX.Resources;
using System.Net.Http;

namespace Vendor.ViewModels;

public partial class InviteViewModel : BaseViewModel, INotifyPropertyChanged
{
    private Realms.Realm realm;

    public static MaskedBehavior Mask = new MaskedBehavior
    {
        Mask = "XXX XXX XX XXXXXXXX",
        UnMaskedCharacter = 'X',
    };


    private UserRole roleraw;
    //private static readonly HttpClient client = new HttpClient();


    [ObservableProperty]
    Invites invite;
    [ObservableProperty]
    bool isManger;

    [ObservableProperty]
    string currentTitle = $"{AppResources.InviteByPhoneNumberLabel}";

    [ObservableProperty]
    private CurrentRegistration isEmailLogin = CurrentRegistration.Email;
    [ObservableProperty]
    private CurrentRegistration isPhoneLogin = CurrentRegistration.Phone;


    [ObservableProperty]
    bool ismanager;
    [ObservableProperty]
    CurrentRegistration nextRegister = CurrentRegistration.Phone;

    [ObservableProperty]
    bool isEmail = true, isPhone;
    [ObservableProperty]
    private CurrentRegistration currentChangeReg = CurrentRegistration.Email;

    private string currentCodeRegion = "7";
    string targetlogin;
    public string Targetlogin { get => targetlogin; set { targetlogin = value; OnPropertyChanged(nameof(targetlogin)); IsPhoneOrMail(value); } }
    [ObservableProperty]
    Entry loginEntry;






    private bool IsPhoneOrMail(string st)
    {
        //Regex validatePhoneNumberRegex = new Regex("^\\+?[Aa-Zz][Aa-Zz]{10,14}$");

        IsEmail = HasNonDigits(st);
        IsPhone = !HasNonDigits(st);
        CurrentChangeReg = HasNonDigits(st) ? CurrentRegistration.Email : CurrentRegistration.Phone;

        if (IsEmail && LoginEntry != null && LoginEntry.Behaviors.Count > 0)
        {
            LoginEntry.Behaviors.Clear();
            LoginEntry.Text = Targetlogin.RemoveWhitespace();
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

    public InviteViewModel()
    {

    }

    internal void OnAppearing()
    {

    }

    [RelayCommand]
    private async Task Send()
    {

        var targetLogin = "";
        switch (CurrentChangeReg)
        {

            case CurrentRegistration.Email:
                if (string.IsNullOrWhiteSpace(Targetlogin) || !IsValidEmail(Targetlogin))
                {
                    await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.WrongEmail}", "Ok");
                    IsBusy = false;
                    return;

                }
                targetLogin = Targetlogin.RemoveWhitespace().ToLower();
                break;
            case CurrentRegistration.Phone:
                if (string.IsNullOrWhiteSpace(Targetlogin) || !IsValidPhone(Targetlogin.Replace(" ", "")))//phone
                {
                    await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.WrongNumber}", "Ok");
                    IsBusy = false;
                    return;


                }
                targetLogin = Targetlogin.Replace(" ", "").ToLower();
                break;
        }





        var realm = RealmService.GetMainThreadRealm();

        var curinv = realm.All<Invites>().FirstOrDefault(x => x.TargetLogin == targetLogin);

        var curshop = realm.Find<Shop>(CurrentShopID);

        //var users = realm.All<Profile>().ToList();

        //var receiver = users.Where(x => x.Login == Targetlogin).FirstOrDefault();

        var invite = new Invites()
        {
            OwnerId = CurrentShopID,
            TargetLogin = targetLogin,
            TargetId = "",
            //TODO использование емайла для инвайта
            OwnerName = CurrentLogin,
            ShopName = curshop.Name,
            Role = roleraw
        };

        //await realm.WriteAsync(() =>
        //{
        //    curinv.Match = true;
        //    //realm.Add(invite);
        //});

        if (curinv != null || CheckInMem(targetLogin))
        {
            await DialogService.ShowAlertAsync(AppResources.AttentionLabel, AppResources.UserInvitedInStoreError, "OK");

            return;
            //"Внимание", "Пользователь уже был приглашен или числиться в магазине", "Ok");
        }
        else
        {
            await realm.WriteAsync(() =>
            {
                realm.Add(invite);
            });
            await PopupNavigation.Instance.PopAsync();
        }


    }

    private bool CheckInMem(string targetlogin)
    {
        
        var realm = RealmService.GetMainThreadRealm();
        var mymem = realm.All<Member>().ToList();
        var proffile = realm.All<Profile>().FirstOrDefault();
        return mymem.Any(x => (x.UserEmail == targetlogin && x.Shop.Id == CurrentShopID) || (proffile.Phone == targetlogin||proffile.Login==targetlogin && x.Shop.Id == CurrentShopID));
    }


    [RelayCommand]
    private void SelectRole(UserRole param)
    {
        Ismanager = (param == UserRole.Manager); //для визуального отображения выбраной роли
        roleraw = param;
    }

    [RelayCommand]
    private async Task Close()
    {
        await PopupNavigation.Instance.PopAsync();
    }


}
