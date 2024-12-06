using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace VendorX.Views;

public partial class SettingPage : ContentPage
{
    private SettingViewModel vm;
    public SettingPage()
    {
        InitializeComponent();
        vm = new SettingViewModel();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        await vm.OnAppearing();
    }

    private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}

