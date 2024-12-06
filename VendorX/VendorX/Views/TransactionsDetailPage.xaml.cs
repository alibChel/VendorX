using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace VendorX.Views;

public partial class TransactionsDetailPage : ContentPage
{
    TransactionsDetailViewModel vm;
    public TransactionsDetailPage( )
    {
        InitializeComponent();
        BindingContext = vm = new TransactionsDetailViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await vm.OnAppearing();
        ReturnCountVisual();
    }

    private void ReturnCountVisual()
    {
       // var elements = ItemsCollection.GetVisualElementWindow();
    }
    protected override bool OnBackButtonPressed()
    {
        if (PopupNavigation.PopupStack.Count() > 0)
            return true;
        else
            return false;

    }
}

