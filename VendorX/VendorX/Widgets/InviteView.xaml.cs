using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace VendorX.Widgets;

public partial class InviteView 
{
    InviteViewModel vm;
    public InviteView()
    {
        InitializeComponent();
        vm = (InviteViewModel)BindingContext;
        //vm.MemsColl = MemColl;
        //vm.Profile = profile;
        vm.LoginEntry = LoginEntry;

    }
    public InviteView(bool IsManger)
    {
        InitializeComponent();
        vm = (InviteViewModel)BindingContext;
        vm.IsManger = IsManger;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        vm.OnAppearing();
        //_taskCompletionSource = new TaskCompletionSource<List<string>>();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

    }
}

