using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace VendorX.Widgets;

public partial class PhotoPickerView 
{
    private TaskCompletionSource<List<string>> _taskCompletionSource;
    public Task<List<string>> PopupClosedTask => _taskCompletionSource.Task;

    PhotoPickerViewModel vm;
    public PhotoPickerView(int count)
    {
        InitializeComponent();
        _taskCompletionSource = new TaskCompletionSource<List<string>>();

        vm = (PhotoPickerViewModel)BindingContext;
        vm.Count = count;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
    }

    protected override void OnDisappearing()
    {
        if (!vm.IsCameraActive)
            _taskCompletionSource.SetResult(vm.Result);
        base.OnDisappearing();
    }

    void PopupPage_BackgroundClicked(System.Object sender, System.EventArgs e)
    {
        //await MopupService.Instance.PopAsync();
    }
}