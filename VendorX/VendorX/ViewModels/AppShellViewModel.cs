using System;
using CommunityToolkit.Mvvm.Messaging;

namespace VendorX.ViewModels;

public partial class AppShellViewModel : ObservableObject
{

    [ObservableProperty]
    bool isOwner;
    public AppShellViewModel()
    {
        WeakReferenceMessenger.Default.Register<ShowNavBarMessage>(this, async (sender, message) =>
        {
            IsOwner = message.Value;

            await Task.CompletedTask;
        });
    }
}

