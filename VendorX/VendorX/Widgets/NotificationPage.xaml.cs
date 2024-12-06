using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace VendorX.Widgets;

public partial class NotificationPage : ContentPage
{
    NotificationViewModel vm;
    public NotificationPage()
    {
        InitializeComponent();
        vm = (NotificationViewModel)BindingContext;

    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await vm.OnAppearing();
    }
}

