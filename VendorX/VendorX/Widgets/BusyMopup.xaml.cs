using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace VendorX.Widgets;

public partial class BusyMopup 
{	
	public BusyMopup ()
	{
		InitializeComponent ();
	}

    public async void Close_()
    {
        await PopupNavigation.Instance.PopAsync();
    }
}

