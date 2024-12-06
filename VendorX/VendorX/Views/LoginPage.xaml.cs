
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VendorX.Views;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class LoginPage : ContentPage
{
    private LoginViewModel vm;
    public LoginPage()
    {
        InitializeComponent();
        vm = new LoginViewModel();
        BindingContext = vm;
        //vm.IsBusy = true;
        vm.LoginEntry = LoginEntry;
     
    }

    void SwichPassVisible(System.Object sender, System.EventArgs e)
    {
        PassEntry.IsPassword = !PassEntry.IsPassword;
    }


    protected override async void OnAppearing()
    {
        
        base.OnAppearing();
        await vm.OnAppearing();
        Shell.SetTabBarIsVisible(this, false);
    }

  
 
}
