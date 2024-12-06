using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Vendor.Models;
using Vendor.Services;
using System.Linq;
using Realms.Sync;
using CommunityToolkit.Mvvm.Messaging;
using Rg.Plugins.Popup.Services;
using Realms;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;

using VendorX.Resources;

namespace Vendor.ViewModels;

public partial class ShopEditViewModel : BaseViewModel
{

    //[ObservableProperty]
    //Realms.Sync.User user;

    [ObservableProperty]
    Shop shop;

    [ObservableProperty]
    private bool isStrict;

    [ObservableProperty]
    string photo, name;
    public ObservableCollection<Currency> Currencys { get; set; } = new ObservableCollection<Currency>(Currency.GetCurrencies());
    [ObservableProperty]
    private Currency selectedCurrency;

    // Новый или редактирование
    public bool IsNew { get; set; }

    public ShopEditViewModel()
    {

        //Profile = realm.Find<Profile>(User.Id);
    }

    // Закрыть
    [RelayCommand]
    private async Task CloseTapped()
    {
        if (IsNew)
            Shop = null;
        await PopupNavigation.Instance.PopAsync();
    }

    // Выбор фото
    [RelayCommand]
    private async Task TakePhoto()
    {
        var res = await PickPhoto();
        if (res.Count > 0)
            Photo = res.FirstOrDefault();
    }

    // Сохранение
    [RelayCommand]
    private async Task SaveChange()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await DialogService.ShowToast(AppResources.NameError);
                //"Название обязательно для заполнения");
            return;
        }

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

        var realm = RealmService.GetMainThreadRealm();
        if (!IsNew)
        {

            await realm.WriteAsync(() =>
            {
                Shop.PhotoUrl = photourl;
                Shop.Name = Name;
                Shop.IsStrict = IsStrict;
                Shop.WalletTag = SelectedCurrency.Value;
            });
        }
        else
        {
            Shop.PhotoUrl = photourl;
            Shop.Name = Name;
            Shop.IsStrict = IsStrict;
            Shop.WalletTag = SelectedCurrency.Value;
        }

        WeakReferenceMessenger.Default.Send(new CurrentShopChengetMessage(true));
        await PopupNavigation.Instance.PopAsync();
    }

    internal void OnAppearing()
    {
        Photo = Shop.PhotoUrl;
        Name = Shop.Name;
        if (IsNew)
            IsStrict = true;
        else
            IsStrict = Shop.IsStrict;
        SelectedCurrency = Currencys.FirstOrDefault(x => x.Value == Shop.WalletTag);
        if (SelectedCurrency == null)
            SelectedCurrency = Currencys.FirstOrDefault();
    }

    [RelayCommand]
    private void StockControl()
    {
        IsStrict = !IsStrict;
    }
}

