using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using NativeMedia;
using Vendor.Helpers;

using VendorX.Resources;
using Realms.Sync;
using CommunityToolkit.Mvvm.Messaging;
using Vendor.Models.Messages;
using System.Globalization;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using VendorX.Services;
using Xamarin.Essentials;
using static System.Net.WebRequestMethods;
using ZXing.QrCode.Internal;
using System.Data;

namespace Vendor.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    // id пользователя
    string userId;

    // 
    [ObservableProperty]
    string userEmail, userLogin;

    // видемость кнопки создания шопа
    [ObservableProperty]
    bool isAddShopBtnVisibl;

    // профиль
    [ObservableProperty]
    Profile profile;

    // список записей участия в магазине
    [ObservableProperty]
    private ObservableCollection<Member> myMembers;
    // Список магазинов
    [ObservableProperty]
    private IQueryable<Shop> myShops;

    // Текущий магазин
    [ObservableProperty]
    private Shop currentShop;

    // Инвайты магазина
    [ObservableProperty]
    private IQueryable<Invites> shopInvites;

    // Инвайты магазина
    [ObservableProperty]
    private IQueryable<Member> shopEmploys;

    // Активная запись
    [ObservableProperty]
    private Member selectetMember;

    private Realms.Realm realm;

    [ObservableProperty]
    private bool isOwner = true;
    [ObservableProperty]
    private bool isOwnerOrManager = true;

    [ObservableProperty]
    private bool isManger = true;

    public ProfileViewModel()
    {

    }


    // переключение активного магазина
    [RelayCommand]
    public async Task SelectMember(Member member)
    {
        try
        {

            await Task.Delay(200);
            if (member == null)


                if (Connectivity.NetworkAccess == NetworkAccess.None)
                {
                    await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.SwitchShopError}", "OK");

                    return;
                }
            if (CurrentShopID != member.Shop.Id)
            {

                if (Connectivity.NetworkAccess == NetworkAccess.None)
                {
                    await DialogService.ShowAlertAsync($"{AppResources.AttentionLabel}", $"{AppResources.SwitchShopError}", "OK");
                    return;
                }
                //f
                IsBusy = true;
                SelectetMember = member;
                IsOwner = SelectetMember.Role == UserRole.Owner ? true : false;
                IsOwnerOrManager = SelectetMember.Role == UserRole.Owner || SelectetMember.Role == UserRole.Manager ? true : false;
                IsManger = SelectetMember.Role == UserRole.Manager ? true : false;
                CurrentShopID = member.Shop.Id;
                CurrentShop = realm.Find<Shop>(CurrentShopID);
                ShopInvites = realm.All<Invites>().Where(i => i.OwnerId == CurrentShopID);
                MyMembers.Move(MyMembers.IndexOf(SelectetMember), 0);

                //stock = realm.Find<Stock>(CurrentShopID);
                var role = realm.All<Member>().Where(x => x.OwnerId == UserId || x.OwnerId == RealmService.CurrentUser.Id)?.FirstOrDefault(x => x.Shop == CurrentShop)?.Role ?? UserRole.User;
                WeakReferenceMessenger.Default.Send(new CurrentShopChengetMessage(true));
                //await Task.Delay(700);

                //realm.Dispose();
                /////realm = Realm.GetInstance();
                //realm =  RealmService.GetMainThreadRealm();
                await RealmService.SetSubscription(realm, SubscriptionType.Mine);
                IsBusy = false;
            }
        }
        catch (Exception)
        {
            await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.ProfileEnterError}", $"ОK");
            await PopupNavigation.Instance.PopAsync();
        }
    }

    // Создание нового магазина
    [RelayCommand]
    private async Task CreateShop()
    {
        var member = new Member() { OwnerId = userId, IsDefault = false, Role = UserRole.Owner, UserEmail = string.IsNullOrWhiteSpace(Profile.Login) ? Profile.Phone : Profile.Login };
        var shop = new Shop() { OwnerId = userId, IsActive = true, IsDefault = false };
        var popup = new ShopEditView(shop);
        await PopupNavigation.Instance.PushAsync(popup);
        shop = await popup.PopupClosedTask;

        if (shop == null)
            return;

        if(realm is null|| realm.IsClosed )
        {
            realm = RealmService.GetMainThreadRealm();
           
        }
        Device.BeginInvokeOnMainThread(async () =>
        {
            await realm.WriteAsync(() =>
            {
                realm.Add(shop);
                member.Shop = shop;
                realm.Add(member);
            });
            OnAppearing();
        });

      
    }

    // Закрытие профиля
    [RelayCommand]
    private async Task Close()
    {
        await PopupNavigation.Instance.PopAsync();
    }

    // Выход
    [RelayCommand]
    private async Task Logout()
    {
        IsBusy = true;
        await RealmService.LogoutAsync();

        Token = "";

        DateLogin = DateTime.MinValue.ToString();

        await Shell.Current.GoToAsync($"//LoginPage");

        await PopupNavigation.Instance.PopAsync();
        IsBusy = false;
    }

    // Редактирование информации пользователя
    [RelayCommand]
    private async Task EditUserData()
    {
        var popup = new UserEditView(Profile);
        await PopupNavigation.Instance.PushAsync(popup);
        OnAppearing();
    }

    // Вызывает попап создание приглашения в магазин
    [RelayCommand]
    private async Task CreateInvite()
    {
        var popup = new InviteView(IsManger);
        await PopupNavigation.Instance.PushAsync(popup);
    }

    // Удаление приглашения 
    [RelayCommand]
    public async Task DeleteInvite(Invites invites)
    {
        if (IsManger)
        {
            if (invites.Role == UserRole.User)
            {


                await realm.WriteAsync(() =>
                {
                    invites.State = InviteState.Nothing;
                    realm.Add(invites);

                });

            }
            else
                await DialogService.ShowToast(AppResources.YouHaveNotAccesError);

            return;
        }

        await realm.WriteAsync(() =>
        {
            invites.State = InviteState.Nothing;
            realm.Add(invites);

        });

        /* await realm.WriteAsync(() => {

             realm.Remove(invites);
         });*/
    }

    // Удаление сотрудника 
    [RelayCommand]
    private async Task DeleteShopEmploy(Member member)
    {

        if (IsManger)
        {
            if (member.Role == UserRole.User)
            {
                await realm.WriteAsync(() =>
                {
                    realm.Remove(member);
                });

            }
            else
                await DialogService.ShowToast(AppResources.YouHaveNotAccesError);

            return;
        }
        await realm.WriteAsync(() =>
        {
            realm.Remove(member);
        });
    }

    internal async void OnAppearing()
    {

        //TODO получение емайла для отображения
        try
        {
            realm = RealmService.GetMainThreadRealm();
            Profile = realm.All<Profile>().FirstOrDefault();

            userId = RealmService.CurrentUser.Identities[0].Id;


            //var mems = realm.All<Member>().ToList();

            var members = realm.All<Member>();
            MyMembers = new ObservableCollection<Member>(members?.Where(x => x.UserEmail == Profile.Login || x.UserEmail == Profile.Phone));

            ShopEmploys = realm.All<Member>().Where(i => i.OwnerId == CurrentShopID && i.UserEmail != Profile.Login && i.UserEmail != Profile.Phone);

            UserEmail = (Profile.Login != Profile.Phone && !string.IsNullOrEmpty(Profile.Login)) ? Profile.Login : "";
            UserLogin = UserEmail;
            MyShops = realm.All<Shop>();
            ShopInvites = realm.All<Invites>().Where(i => i.OwnerId == CurrentShopID);
            CurrentShop = realm.Find<Shop>(CurrentShopID);
   
            Member curmem = MyMembers.FirstOrDefault(x => x.Shop.CompareId(CurrentShop.Id));
            SelectetMember = curmem;
            IsOwner = SelectetMember.Role == UserRole.Owner ? true : false;
            IsOwnerOrManager = SelectetMember.Role == UserRole.Owner || SelectetMember.Role == UserRole.Manager ? true : false;
            IsManger = SelectetMember.Role == UserRole.Manager ? true : false;
            MyMembers.Move(MyMembers.IndexOf(SelectetMember), 0);
            OnPropertyChanged(nameof(MyMembers));

            if (realm.All<Member>().Where(x => x.OwnerId == userId).ToList().Count >= 3)
                IsAddShopBtnVisibl = false;
            else
                IsAddShopBtnVisibl = true;
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync($"{AppResources.ErrorLabel}", $"{AppResources.ProfileEnterError}", $"ОK");
            await PopupNavigation.Instance.PopAsync();
        }


    }
}

