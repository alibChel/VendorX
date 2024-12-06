

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Transaction = Vendor.Models.Transaction;

namespace VendorX.Widgets;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ReturnItemPage 
{
    private TaskCompletionSource<bool> _taskCompletionSource;
    public Task<bool> PopupClosedTask => _taskCompletionSource.Task;

    ReturnItemViewModel vm;

    public ReturnItemPage(Transaction transaction)
    {
        InitializeComponent();
        vm = (ReturnItemViewModel)BindingContext;
    
        
        vm.Transaction = transaction;
        _taskCompletionSource = new TaskCompletionSource<bool>();

    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await vm.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _taskCompletionSource.SetResult(true);

    }

    private async void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {

            if (e.NewTextValue != null && e.NewTextValue != e.OldTextValue)
            {
                var tri = ((Entry)sender).BindingContext;
                if (tri != null && tri.GetType() == typeof(TransactionItem))
                {
                    await vm.CheckItemChangeAsync((TransactionItem)tri);
                }
            }
        }
        catch
        {

        }
    }
}