using Xamarin.Forms;
using VendorX.Models;
namespace Vendor.ViewModels;


public partial class OrderDetailViewModel : BaseViewModel, IQueryAttributable
{

    [ObservableProperty]
    Order order;

    [ObservableProperty]
    private string walletTag;

    private Realm realm;

    [ObservableProperty]
    string nextStatusOrderBtnText = "Собрать";


    [ObservableProperty]
    bool isNextStatusVisible = true;


    public void ApplyQueryAttributes(IDictionary<string, string> query)
    {
        try
        {
            if (query.Count > 0 && query.ContainsKey("Order"))
            {
                string _id = query["Order"];
                Order = realm.Find<Order>(_id);
                Title = $"{Order.Number}";
            }
        }
        catch { }
    }






    public OrderDetailViewModel()
    {
        realm = RealmService.GetMainThreadRealm();

    }
    internal async Task OnAppearing()
    {
        if (realm == null || realm.IsClosed)
            realm = RealmService.GetRealm();

        var shop = realm.Find<Shop>(CurrentShopID);
        WalletTag = shop?.WalletTag;
        ChangeTextBtn();
        await Task.CompletedTask;
    }

    private void ChangeTextBtn()
    {
        try
        {
            (NextStatusOrderBtnText, IsNextStatusVisible) = Order.OrderStatus switch
            {
                OrderStatus.Pending => ("Собрать", true),
                OrderStatus.Processing => ("Отправлен", true),
                OrderStatus.AcceptedByMerchant => ("Передать в доставку", true),
                _ => ("", false)
            };
        }
        catch { }
    }
    [RelayCommand]
    private async Task OrderChangeStatus()
    {
        try
        {
            await realm.WriteAsync(() =>
            {
                switch (Order.OrderStatus)
                {
                    case OrderStatus.Pending:
                        Order.OrderStatus = OrderStatus.AcceptedByMerchant;
                        break;

                    case OrderStatus.AcceptedByMerchant:
                        Order.OrderStatus = OrderStatus.Processing;
                        break;
                    case OrderStatus.Processing:
                        Order.OrderStatus = OrderStatus.Shipped;
                        break;

                }
            });


            ChangeTextBtn();
        }
        catch (Exception ex)
        {
            _ = ex;
        }
    }


}
