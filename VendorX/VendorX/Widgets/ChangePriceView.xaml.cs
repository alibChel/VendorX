using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace VendorX.Widgets;

public partial class ChangePriceView
{
    private TaskCompletionSource<double> _taskCompletionSource;
    public Task<double> PopupClosedTask => _taskCompletionSource.Task;
    ChangePriceViewModel vm;

    public ChangePriceView(double _price)
    {
        InitializeComponent();
        _taskCompletionSource = new TaskCompletionSource<double>();
        vm = (ChangePriceViewModel)BindingContext;

        vm.Fnum = _price.ToString();
        vm.Startprice = _price;


    }

    protected override void OnDisappearing()
    {
        double result = string.IsNullOrWhiteSpace(vm.Fnum) ? 0 : double.Parse(vm.Fnum);
        _taskCompletionSource.SetResult(result);
        base.OnDisappearing();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        vm.NumGreedW = width;
        vm.NumGreedH = vm.NumGreedW + (vm.NumGreedW / 6);
        //vm.NumButtonRadius = (vm.NumGreedH - 15*4/ 5 )  / 2; PercentButton
        vm.NumButtonRadius = (PercentButton.Height)  / 2; 
    }
}

