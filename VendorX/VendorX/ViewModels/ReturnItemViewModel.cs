using System;
using Rg.Plugins.Popup.Services;
using VendorX.Resources;
using Xamarin.Forms;
using Transaction = Vendor.Models.Transaction;

namespace Vendor.ViewModels;

public partial class ReturnItemViewModel : BaseViewModel, IQueryAttributable
{
    // Транзакция
    [ObservableProperty]
    Transaction transaction;

    // Возвращаемые товары
    public ObservableCollection<TransactionItem> ReturnedItems { get; set; } = new ObservableCollection<TransactionItem>();

    // все возвратные транзакции
    private List<Transaction> returnedTransactions = new List<Transaction>();

    // экземпляр реалма
    private Realm realm;


    /// <summary>
    /// walletTag - значок валюты
    /// </summary>
    [ObservableProperty]
    private string walletTag = "₸";


    // Профиль
    [ObservableProperty]
    Profile profile;

    // Текущий магазин
    private Shop shop;

    // Текущая роль
    private UserRole role;

    public ReturnItemViewModel()
    {
        realm = RealmService.GetMainThreadRealm();
        shop = realm.Find<Shop>(CurrentShopID);
        WalletTag = shop.WalletTag;
        Profile = realm.All<Profile>().FirstOrDefault();
    }

    // Применение переданных параметров
    public void ApplyQueryAttributes(IDictionary<string, string> query)
    {
        if (query.Count > 0 && query["Transaction"] != null)
        {
            Transaction = realm.Find<Transaction>(query["Transaction"]);
            //Transaction = query["Transaction"] as Transaction;
            Title = $"{AppResources.ReturnLabel} {Transaction.Number}";
        }
        else
        {

        }
    }

    internal async Task OnAppearing()
    {
        Title = $"{AppResources.ReturnLabel} {Transaction.Number}";

        returnedTransactions = realm.All<Transaction>().Where(x => x.ParentTransactionId == Transaction.Id).ToList();

        if (returnedTransactions.Count > 0)
        {
            var tritems = returnedTransactions.SelectMany(x => x.TransactionItems);
            foreach (var item in Transaction.TransactionItems)
            {
                var returnedcount = tritems.Where(x => x.ItemId == item.ItemId).ToList()?.Sum(x => x.Count) ?? 0;
                var addcount = item.Count - returnedcount;
                if (addcount != 0)
                {
                    var newremoveitem = new TransactionItem(item);
                    newremoveitem.Count = addcount;
                    ReturnedItems.Add(newremoveitem);
                }
            }
        }
        else
        {
            foreach (var item in Transaction.TransactionItems)
            {
                ReturnedItems.Add(new TransactionItem(item));
            }
        }
        await Task.CompletedTask;
    }

    // Нажатие кнопки отмена
    [RelayCommand]
    private async Task Cencel()
    {
        //await Shell.Current.GoToAsync("..");
        await PopupNavigation.Instance.PopAsync();

    }

    // Нажатие кнопки вернуть
    [RelayCommand]
    private async Task ReturnItems()
    {
        var returntransaction = new Transaction();
        returntransaction.ParentTransactionId = Transaction.Id;
        returntransaction.ContactName = Transaction.ContactName;
        returntransaction.ContactPhone = Transaction.ContactPhone;
        returntransaction.OwnerId = CurrentShopID;
        returntransaction.AutorId = Profile.Id;
        returntransaction.AutorName = Profile.FullName;
        returntransaction.ShopName = shop.Name;
        returntransaction.OperationCode = Operation.Return;

        if (Transaction.OperationCode == Operation.Sale)
        {
            returntransaction.Price = ReturnedItems.Sum(x => x.FinalPrice);
            returntransaction.Payments.Add(new Payment { Summ = returntransaction.Price });
        }
        else if (Transaction.OperationCode == Operation.Admission)
        {
            returntransaction.Price = ReturnedItems.Sum(x => x.FinalCost);
            returntransaction.Payments.Add(new Payment { Summ = returntransaction.Price });
        }
        foreach (var item in ReturnedItems)
        {
            returntransaction.TransactionItems.Add(item);
        }

        var countnewitems = returntransaction.TransactionItems.Sum(x => x.Count);
        var countreturneditems = returnedTransactions.SelectMany(x => x.TransactionItems).Sum(x => x.Count);
        if (countnewitems + countreturneditems < Transaction.TransactionItems.Sum(x => x.Count))
        {
            returntransaction.StateCode = Models.TransactionState.PartiallyReturn;
        }
        else
        {
            returntransaction.StateCode = Models.TransactionState.Return;
        }

        var tritems = returnedTransactions.SelectMany(x => x.TransactionItems);

        await realm.WriteAsync(() =>
        {
            realm.Add<Transaction>(returntransaction);
            Transaction.StateCode = returntransaction.StateCode;
            foreach (var item in Transaction.TransactionItems)
            {
                var returnedcount = tritems.Where(x => x.ItemId == item.ItemId).ToList()?.Sum(x => x.Count) ?? 0;
                var newreturncount = ReturnedItems.Where(x => x.ItemId == item.ItemId).ToList()?.Sum(x => x.Count) ?? 0;

                item.ReturnedCount = returnedcount + newreturncount;
            }

        });

        //await Shell.Current.GoToAsync("..");
        await PopupNavigation.Instance.PopAsync();

    }


    // Добавление товара для возврата, кнопкой + 
    [RelayCommand]
    private async Task AddTransactionItemToCart(TransactionItem item)
    {
        try
        {
            //var tritems = returnedTransactions.SelectMany(x => x.TransactionItems);
            //var returneditemcount = tritems.FirstOrDefault(x => x.ItemId == item.ItemId).Count;
            var transaction_item = Transaction.TransactionItems.FirstOrDefault(x => x.ItemId?.Id == item.ItemId.Id);
            if (transaction_item.Count - transaction_item.ReturnedCount > item.Count)
                item.Count++;
            await Task.CompletedTask;
        }
        catch { }
    }

    // Удаление товара из списка возврата, кнопкой -
    [RelayCommand]
    private async Task RemoveItemFromCart(TransactionItem transactionItem)
    {
        try
        {
            if (transactionItem.Count == 1)
                ReturnedItems.Remove(transactionItem);
            else
            {
                transactionItem.Count--;
            }

            if (ReturnedItems.Count == 0)
                await Cencel();
            await Task.CompletedTask;
        }
        catch { }
    }

    // Удаление товара из списка возврата, свайпом
    [RelayCommand]
    private async Task RemoveItemsFromCart(TransactionItem transactionItem)
    {
        try
        {
            ReturnedItems.Remove(transactionItem);
            await Task.CompletedTask;

            if (ReturnedItems.Count == 0)
                await Cencel();
        }
        catch { }
    }



    internal async Task CheckItemChangeAsync(TransactionItem item)
    {
        //return;
        if (item == null)
        {
            return;
        }

        var transaction_item = Transaction.TransactionItems.FirstOrDefault(x => x.ItemId == item.ItemId);
        if (transaction_item.Count - transaction_item.ReturnedCount < item.Count)
        {

            item.Count = transaction_item.Count;
            await DialogService.ShowAlertAsync("", $"{item.ItemName} нет в достаточном количестве.", "OK");
            return;

        }

    }
}

