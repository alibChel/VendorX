using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Vendor.Models;
using Vendor.Services;
using System.Linq;
using Realms.Sync;
using Rg.Plugins.Popup.Services;
//using VendorX.Resources;
namespace Vendor.ViewModels;

public partial class UserEditViewModel : BaseViewModel
{

    //[ObservableProperty]
    //Realms.Sync.User user;

    [ObservableProperty]
    Profile profile;

    [ObservableProperty]
    string photo,fname,lname;

 

    public UserEditViewModel()
	{

        //Profile = realm.Find<Profile>(User.Id);
    }

    // Закрыть
    [RelayCommand]
    private async Task CloseTapped()
    {        
        await PopupNavigation.Instance.PopAsync();
    }

    // Выбор фото
    [RelayCommand]
    private async Task TakePhoto()
    {
        var res = await PickPhoto();
        if(res != null && res.Count >0)
            Photo = res.FirstOrDefault();
        
    }

    [RelayCommand]
    private async Task SaveChange()
    {
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

        await realm.WriteAsync(() =>
        {
            Profile.PhotoUrl = photourl;
            Profile.FirstName = Fname;
            Profile.LastName = Lname;
            //TODO получение емайла для записи профиля
            var email= RealmService.CurrentUser.Profile.Email;
            var phone = RealmService.CurrentUser.Profile.Name;
            if (string.IsNullOrEmpty(email))
            {
                Profile.Phone = phone;
                
            }
            else
                Profile.Login = email;
          
        });

        await CloseTapped();
    }

    internal void OnAppearing()
    {
        //User = RealmService.CurrentUser;
        
        Photo = Profile?.PhotoUrl;
        Fname = Profile?.FirstName;
        Lname = Profile?.LastName;
    }
}

