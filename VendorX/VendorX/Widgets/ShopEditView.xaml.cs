using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace VendorX.Widgets;

public partial class ShopEditView 
{
    private TaskCompletionSource<Shop> _taskCompletionSource;
    public Task<Shop> PopupClosedTask => _taskCompletionSource.Task;

    ShopEditViewModel vm;
    public ShopEditView(Shop shop, bool isnew = true)
    {
        InitializeComponent();
        vm = (ShopEditViewModel)BindingContext;
        vm.Shop = shop;
        vm.IsNew = isnew;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        vm.OnAppearing();
        _taskCompletionSource = new TaskCompletionSource<Shop>();
    }

    protected override void OnDisappearing()
    {
        _taskCompletionSource.SetResult(vm.Shop);
        base.OnDisappearing();
    }

    void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
    {
        WalletPicker.Focus();
    }
}

