using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VendorX.Widgets;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ChoiceDateView 
{
    private TaskCompletionSource<Tuple<DateTime, DateTime>> _taskCompletionSource;
    public Task<Tuple<DateTime, DateTime>> PopupClosedTask => _taskCompletionSource.Task;

    ChoiceDateViewModel vm; 

    public ChoiceDateView ()
	{
		InitializeComponent ();
        vm = (ChoiceDateViewModel)BindingContext;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        vm.OnAppearing();
        _taskCompletionSource = new TaskCompletionSource<Tuple<DateTime, DateTime>>();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _taskCompletionSource.SetResult(vm.Result);
    }
}