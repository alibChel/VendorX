using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace VendorX.Widgets;

public partial class UserEditView 
{
    UserEditViewModel vm;
    public UserEditView(Profile profile)
    {
        InitializeComponent();
        vm = (UserEditViewModel)BindingContext;
        vm.Profile = profile;
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
}