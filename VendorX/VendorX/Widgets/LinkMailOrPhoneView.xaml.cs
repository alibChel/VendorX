using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace VendorX.Widgets;

public partial class LinkMailOrPhoneView 
{
    LinkMailOrPhoneViewModel vm;
    public LinkMailOrPhoneView(AuthType mod)
    {
        InitializeComponent();
        vm = (LinkMailOrPhoneViewModel)BindingContext;
        vm.IsEmail = mod == AuthType.Email ;
        vm.CurrentAuthType = mod;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        vm.OnAppearing();
        //_taskCompletionSource = new TaskCompletionSource<bool>();
    }

    protected override void OnDisappearing()
    {
        //_taskCompletionSource.SetResult(vm.Result);
        base.OnDisappearing();
    }
    void SwichPassVisible(System.Object sender, System.EventArgs e)
    {
        PassEntry.IsPassword = !PassEntry.IsPassword;
    }
}