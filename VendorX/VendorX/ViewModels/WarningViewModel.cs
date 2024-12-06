using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Rg.Plugins.Popup.Services;

namespace Vendor.ViewModels;

public partial class WarningViewModel:BaseViewModel
{

    [ObservableProperty]
    private string message;

    [ObservableProperty]
    private bool isNeedConfirm;

    public bool Result { get; set; }

	public WarningViewModel()
	{
	}

    [RelayCommand]
    private async Task CloseTapped(bool res)
    {
        Result = res;
        await PopupNavigation.Instance.PopAsync();
    }
}

