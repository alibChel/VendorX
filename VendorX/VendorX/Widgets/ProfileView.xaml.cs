using System;
using System.Collections.Generic;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;

namespace VendorX.Widgets;

public partial class ProfileView 
{
    ProfileViewModel vm;
    public ProfileView()
    {
        try
        {
            InitializeComponent();
           vm = (ProfileViewModel)BindingContext;
        }catch (Exception ex)
        {
           Debug.WriteLine(ex);
        }
        //vm.MemsColl = MemColl;
        //vm.Profile = profile; 
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

    void PopupPage_BackgroundClicked(System.Object sender, System.EventArgs e)
    {
        //await MopupService.Instance.PopAsync();
    }

    void MembersView_SelectionChanged(System.Object sender, SelectionChangedEventArgs e)
    {
        //var obj = e.CurrentSelection.FirstOrDefault();
        //if (obj == null)
        //    return;
        //await vm.SelectMember((Member)obj);



    }

    private void Expander_Tapped(object sender, EventArgs e)
    {
        var exp = (Expander)sender;
        if (exp.IsExpanded)
            exp.ForceUpdateSize();

        //
    }

    private async void SwipeItem_Invoked(object sender, EventArgs e)
    {
        var item = (SwipeItem)sender;

        await vm.DeleteInvite((Invites)item.CommandParameter);
    }
}

