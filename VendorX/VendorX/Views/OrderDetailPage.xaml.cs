using System;
using System.Collections.Generic;
using Autofac;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace VendorX.Views;

public partial class OrderDetailPage : ContentPage
{
    private readonly OrderDetailViewModel _vm;
    public OrderDetailPage()
    {
        InitializeComponent();
        BindingContext = _vm = DependencyConfig.Container.Resolve<OrderDetailViewModel>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.OnAppearing();
   
    }

 
    protected override bool OnBackButtonPressed()
    {
        if (PopupNavigation.PopupStack.Count() > 0)
            return true;
        else
            return false;

    }
}

