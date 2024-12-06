using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Transaction = Vendor.Models.Transaction;
using VendorX.Resources;
namespace Vendor.ViewModels;


public partial class TransactionsDetailViewModel : BaseViewModel, IQueryAttributable
{
    // Транзакция
    [ObservableProperty, NotifyPropertyChangedFor(nameof(TotalDicsountOrMarkup))]
    Transaction transaction;

    // Возвратные транзакции
    [ObservableProperty]
    private IQueryable<Transaction> returnedTransactions;

    // Возвратные транзакйии
    [ObservableProperty]
    private List<TransactionItem> returnedTransactionItem;

    // Значек валюты магазина
    [ObservableProperty]
    private string walletTag;

    /// <summary>
    ///  isAddPaymentVisible - видемость кнопки добавить платеж
    ///  isReturnButtonVisible - видемость кнопки возврат
    /// </summary>
    [ObservableProperty]
    private bool isAddPaymentButtonVisible, isReturnButtonVisible, isReturnTransactionListVisible;

    // Профиль
    [ObservableProperty]
    Profile profile;

    // Текущий магазин
    private Shop shop;
    // склад текущего магазина
    //private Stock stock;
    // Текущая роль
    private UserRole role;

    // экземпляр реалма
    private Realm realm;
    // id текущего пользователя mongo 


    public string TotalDicsountOrMarkup
    {
        get { return Transaction.Markup>0 ? AppResources.MarkupLabel : AppResources.OverDiscountLabel; }
    }



 



    /// <summary>
    /// returnprice - сумма возвратов
    /// total - итого
    /// totalPrice - К оплате 
    /// </summary>
    [ObservableProperty]
    private double totalPrice, total, returnprice;


   

 // внесенная сумма
 [ObservableProperty, NotifyPropertyChangedFor(nameof(SummDiffirence)), NotifyPropertyChangedFor(nameof(TotalDicsountOrMarkup))]
    private double depSumm;
  
    // Разница внесенной суммы и финальной стоимости 
    public double SummDiffirence { get => Math.Abs(TotalPrice - DepSumm); }

    // Текст разницы сумм Долг или Сдача
    [ObservableProperty]
    private string summDiffirenceText;
    // Текст разницы сумм Долг или Сдача
    [ObservableProperty]
    private Color summDiffirenceColor = Color.FromHex("#66B2F0");

    // Применение переданных параметров
    public void ApplyQueryAttributes(IDictionary<string, string> query)
    {
        if (query.Count > 0 && query["Transaction"] != null)
        {
            Transaction = realm.Find<Transaction>(query["Transaction"]);
            Title = $"{Transaction.Number}";
        }
        else
        {

        }
         CheckReturns();
        CheckPayment();
    }

    public TransactionsDetailViewModel()
    {
        realm = RealmService.GetMainThreadRealm();
    }
    internal async Task OnAppearing()
    {
        if (realm == null || realm.IsClosed)
            realm = RealmService.GetRealm();

        shop = realm.Find<Shop>(CurrentShopID);
        WalletTag = shop?.WalletTag;

        Member curmem = realm.All<Member>().Where(x => x.OwnerId == RealmService.CurrentUser.Identities[0].Id).FirstOrDefault(x => x.Shop == shop);
        if (curmem != null)
        {
            role = curmem.Role;
        }
        else
        {
            role = realm.All<Member>().FirstOrDefault(x => x.Shop == shop)?.Role ?? UserRole.User;
        }
        //stock = realm.Find<Stock>(CurrentShopID);
          
        IsBusy = false;
        CheckReturns();
        CheckPayment();
        await Task.CompletedTask;
    }

    // Сравнивает сумму платежей и сумму транзакции и проставляет видимость кнопки добавить платеж
    private void CheckPayment()
    {
        var returnsumm = ReturnedTransactions.ToList().Sum(x => x.Price);

        if (Transaction.OperationCode == Operation.Admission)
            Total = Transaction.TransactionItems.Sum(x => x.FinalCost) ;
        else
            Total = Transaction.TransactionItems.Sum(x => x.FinalPrice) ;

        DepSumm = Transaction.Payments.Sum(x => x.Summ) ;
        TotalPrice = Total - Transaction.Discount - returnsumm;
        TotalPrice += Transaction.Markup;
        if (TotalPrice < 0)
            TotalPrice = 0;

        if (DepSumm > TotalPrice)
        {
            SummDiffirenceColor = Color.FromHex("#479649");
            SummDiffirenceText = AppResources.ChangeLabel;
                //"Сдача :";
        }
        else
        {
            SummDiffirenceColor = Color.FromHex("#964747");
            SummDiffirenceText = AppResources.DutyLabel;
                //"Долг :";
        }

        if ( TotalPrice > DepSumm )
        {
            IsAddPaymentButtonVisible = true;
        }
        else
            IsAddPaymentButtonVisible = false;

        OnPropertyChanged(nameof(SummDiffirence));
    }

    //


    [RelayCommand]
    private async Task CheckTransaction()
    {
        IsBusy = true;
        var html=Transaction.TransactionToHtml(shop.WalletTag);
        IsBusy = false;

        await DialogService.ShowHtmlAsync(html);
    }


    //Добавление платежа
    [RelayCommand]
    private async Task AddPayments()
    {
        var newvalue = await ChangeNummValue(Transaction.Price - Transaction.Payments.Sum(x => x.Summ) - Returnprice);
        if (newvalue <= 0)
        {
            //await DialogService.ShowToast("Платёж не может быть меньше нуля");
            return;
        }

        await realm.WriteAsync(() =>
        {
            Transaction.Payments.Add(new Payment { Summ = newvalue});
            if (Transaction.Price <= Transaction.Payments.Sum(x => x.Summ))
                Transaction.StateCode = Models.TransactionState.Paid;
        });

        CheckPayment();
    }

    // Проверка возвратов
    private void CheckReturns()
    {
        ReturnedTransactions = realm.All<Transaction>().Where(x => x.ParentTransactionId == Transaction.Id);
        ReturnedTransactionItem = ReturnedTransactions.ToList().SelectMany(x=> x.TransactionItems).ToList();
        var returnList = ReturnedTransactions.ToList();
        if (returnList.Count > 0 )
            IsReturnTransactionListVisible = true;
        else
            IsReturnTransactionListVisible = false;

        IsReturnButtonVisible = false;
        foreach (var item in Transaction.TransactionItems)
        {
            var tritem = ReturnedTransactionItem.Where(x => x.ItemId == item.ItemId).Sum(x => x.Count);
            if (item.Count > tritem && Transaction.OperationCode == Operation.Sale)
            {
                IsReturnButtonVisible = true;
                break;
            }

            item.ReturnedCountChanged();
        }
        Returnprice = ReturnedTransactionItem.Sum(x=> x.FinalPrice);
        OnPropertyChanged(nameof(IsReturnButtonVisible));
    }

    // Изменение общей скидки
    [RelayCommand]
    private async Task ChangeTransactionTotalDiscount()
    {
        var newvalue = await ChangeNummValue(Transaction.Discount);
        if (newvalue == -1)
            return;
        if (newvalue < 0)
            newvalue = 0;
        await realm.WriteAsync(() =>
        {
            Transaction.Price = Transaction.TransactionItems.Sum(x => x.FinalPrice) - newvalue;
            Transaction.Discount = (int)newvalue;
        });
        CheckPayment();
    }

    // Создать возврат
    [RelayCommand]
    private async Task MakeReturn()
    {
        //var itemParameter = new Dictionary<string, object>() { { "Transaction", Transaction } };
        //await Shell.Current.GoToAsync(nameof(ReturnItemPage), itemParameter);

        var popup = new ReturnItemPage(Transaction);
        await PopupNavigation.Instance.PushAsync(popup);
        var result = await popup.PopupClosedTask;
        await OnAppearing();
    }
}
