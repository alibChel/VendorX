using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DevExpress.XamarinForms.CollectionView;
using DevExpress.XamarinForms.Core.Filtering;
//using VendorX.Resources; 
using Realms;
using Rg.Plugins.Popup.Services;
using VendorX.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

using Transaction = Vendor.Models.Transaction;

namespace Vendor.ViewModels;

public partial class TransactionsViewModel : BaseViewModel
{
    // Все товары 
    [ObservableProperty]
    public IQueryable<Transaction> transactions;

    // Аниматор фильтров
    public AnimationStateMachine FilterbarState { get; set; }

    /// <summary>
    /// searchtext - Текст поиска
    /// walletTag - значек валюты
    /// </summary>
    [ObservableProperty]
    private string searchtext, walletTag = "₸";

    [ObservableProperty]
    CriteriaOperator searchFilter;

    // Параметры сортировки и фильтрации
    /// <summary>
    /// sortNew -сначала новые
    /// sortLowPrice -сначала дешевые
    /// sortHipricce -сначала дорогие
    /// filterAll -все
    /// filterActual -актуальные
    /// filterArchive -архивные
    /// CreatedTicks -по дате
    /// groupName - по имени добавленного контакта
    /// groupPhone - по телефону добавленного контакта
    /// </summary>
    [ObservableProperty]
    private bool sortAsc, sortDis, sortNone, filterAll, filterSale, filterAdmission, filterReturn,filterEdit, filterUnpaid, filterPaid,filterPeriod, createdTicks, groupName, groupPhone;

    [ObservableProperty]
    private DateTime startDate = DateTime.Today,endDate= DateTime.Now,maxDateEnd=DateTime.Now, maxDateStart = DateTime.Today;

    // фильтр товара
    private TransactionFilter filter
    {
        get
        {
            if (FilterSale)
                return TransactionFilter.Sale;
            if (FilterAdmission)
                return TransactionFilter.Admission;
            if (FilterReturn)
                return TransactionFilter.Return;
            if (FilterEdit)
                return TransactionFilter.Edit;
            if (FilterUnpaid)
                return TransactionFilter.Unpaid;
            if (FilterPaid)
                return TransactionFilter.Paid;

            return TransactionFilter.All;
        }
    }



    // Профиль
    [ObservableProperty]
    Profile profile;

    // Текущий магазин
    [ObservableProperty]
    private Shop shop;
    // склад текущего магазина
    //[ObservableProperty]
    //private Stock stock;
    // Текущая роль
    private UserRole role;

    // экземпляр реалма
    private Realm realm;
    // id текущего пользователя mongo 

    // Группировка
    [ObservableProperty]
    public TransactionGroup transactionGroup;
    // сортировка товара
    //[ObservableProperty]
    //private SortDescription dataSortOrder;

    private TransactionFilter transactionFilter;

    // для отслеживания первой загрузки
    private bool firstload;

    public DXCollectionView CollectionView {  get; set; }

    public TransactionsViewModel()
    {
    
        WeakReferenceMessenger.Default.Register<CurrentShopChengetMessage>(this, async (sender, message) =>
        {
            realm = RealmService.GetMainThreadRealm();
            Shop = realm.Find<Shop>(CurrentShopID);
            WalletTag = Shop.WalletTag;
            role = realm.All<Member>().Where(x => x.OwnerId == RealmService.CurrentUser.Identities[0].Id)?.FirstOrDefault(x => x.Shop == shop)?.Role ?? UserRole.User;
            await GetItems();
        });
       _ = GetItems();
    }

    internal async Task OnAppearing()
    {
        if (!firstload) {

            await Task.Delay(150);
            IsBusy = true;
            realm = RealmService.GetMainThreadRealm();
            Profile = realm.All<Profile>().FirstOrDefault();
            await GetItems();
            IsBusy = false;
        }
      
        firstload = true;
        Shop = realm.Find<Shop>(CurrentShopID);
        WalletTag = Shop.WalletTag;
        Transactions = realm.All<Transaction>().Where(x => x.OwnerId == CurrentShopID);
        if (FilterPeriod)
            await FiltredTransactionPeriod();
        OnPropertyChanged(nameof(Profile));
        ChekNotification();
    }

    // Переход к деталям транзакции
    [RelayCommand]
    private async void GoToDetails(Transaction item)
    {
        //await Shell.Current.GoToAsync(nameof(TransactionsDetailPage), true, new Dictionary<string, object>
        //{
        //    { "Transaction", item }
        //});

        //var itemParameter = new Dictionary<string, string>() { { "item", item.Id }, { "count", incart.ToString()} };
        await Shell.Current.GoToAsync($"{nameof(TransactionsDetailPage)}?Transaction={item.Id}");
    }

    // Редактирование текущего магазина
 

    //
    private async Task GetItems()
    {


        // var _sort = "";
        // var _filter = "";
        FilterPeriod = Preferences.Get($"{nameof(FilterPeriod)}Transaction{CurrentShopID}", false);
      /*  StartDate = Preferences.Get($"{nameof(StartDate)}Transaction{CurrentShopID}", DateTime.Now.AddDays(-1)) ;
        EndDate = Preferences.Get($"{nameof(EndDate)}Transaction{CurrentShopID}", DateTime.Now) ;*/
       
        if (FilterPeriod)
            FiltredTransactionPeriod();

        CreatedTicks = Preferences.Get($"{nameof(CreatedTicks)}Transaction{CurrentShopID}", true);
        GroupName = Preferences.Get($"{nameof(GroupName)}Transaction{CurrentShopID}", false);
        GroupPhone = Preferences.Get($"{nameof(GroupPhone)}Transaction{CurrentShopID}", false);


        if (GroupPhone)
            TransactionGroup = TransactionGroup.ContactPhone;
        else if (GroupName)
            TransactionGroup = TransactionGroup.ContactName;
        else
            TransactionGroup = TransactionGroup.CreatedTicks;

        SortAsc = Preferences.Get($"{nameof(SortAsc)}Transaction{CurrentShopID}", false);
        SortDis = Preferences.Get($"{nameof(SortDis)}Transaction{CurrentShopID}", true);
        SortNone = Preferences.Get($"{nameof(SortNone)}Transaction{CurrentShopID}", false);

        if (SortAsc)
            CollectionView.GroupDescription.SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
        //DataSortOrder =new SortDescription {  SortOrder = DevExpress.Data.ColumnSortOrder.Ascending};
        else if (SortDis)
            CollectionView.GroupDescription.SortOrder = DevExpress.Data.ColumnSortOrder.Descending;
        //DataSortOrder = new SortDescription { SortOrder = DevExpress.Data.ColumnSortOrder.Descending };
        else
            CollectionView.GroupDescription.SortOrder = DevExpress.Data.ColumnSortOrder.None;
   
        //DataSortOrder = new SortDescription { SortOrder = DevExpress.Data.ColumnSortOrder.None };

        FilterAll = Preferences.Get($"{nameof(FilterAll)}Transaction{CurrentShopID}", true);
        FilterSale = Preferences.Get($"{nameof(FilterSale)}Transaction{CurrentShopID}", false);
        FilterAdmission = Preferences.Get($"{nameof(FilterAdmission)}Transaction{CurrentShopID}", false);
        FilterReturn = Preferences.Get($"{nameof(FilterReturn)}Transaction{CurrentShopID}", false);
        FilterEdit = Preferences.Get($"{nameof(FilterEdit)}Transaction{CurrentShopID}", false);
        FilterPaid = Preferences.Get($"{nameof(FilterPaid)}Transaction{CurrentShopID}", false);
        FilterUnpaid = Preferences.Get($"{nameof(FilterUnpaid)}Transaction{CurrentShopID}", false);



        if (FilterSale)
            transactionFilter = TransactionFilter.Sale;
        else if (FilterAdmission)
            transactionFilter = TransactionFilter.Admission;
        else if (FilterReturn)
            transactionFilter = TransactionFilter.Return;
        else if (FilterEdit)
            transactionFilter = TransactionFilter.Edit;
        else if(FilterUnpaid)
            transactionFilter= TransactionFilter.Unpaid;
        else if(FilterPaid)
            transactionFilter= TransactionFilter.Paid;
        else
            transactionFilter = TransactionFilter.All;


      //  Transactions = realm.All<Transaction>().Where(x=> x.OwnerId == CurrentShopID);
        await Task.CompletedTask;
    }

    // Изменение текста поиска
    partial void OnSearchtextChanged(string value)
    {
        MakeFilterWhitText();
    }


    private void MakeFilterWhitText()
    {
        

        switch (filter)
        {
            case TransactionFilter.All:
                SearchFilter = CriteriaOperator.Parse("Contains([ContactName], ?)" +
                    " or Contains([ContactPhone], ?) " +
                    "or Contains([Number.ToString()], ?) " +
                    "or Contains([Description], ?) " +
                    "or Contains([AutorName],?)", 
                    Searchtext,Searchtext, Searchtext, Searchtext,Searchtext);
               
                break;
            case TransactionFilter.Sale:
                SearchFilter = CriteriaOperator.Parse("[OperationCode] ==? and (Contains([ContactName], ?)" +
                    " or Contains([ContactPhone], ?) " +
                    "or Contains([Number.ToString()], ?) " +
                    "or Contains([Description], ?)"
                    + "or Contains([AutorName],?)", 
                    Operation.Sale, Searchtext, Searchtext, Searchtext, Searchtext,Searchtext);
       
                break;
            case TransactionFilter.Admission:
                SearchFilter = CriteriaOperator.Parse("[OperationCode] ==? and (Contains([ContactName], ?)" +
              " or Contains([ContactPhone], ?) " +
              "or Contains([Number.ToString()], ?) " +
              "or Contains([Description], ?)"
               + "or Contains([AutorName],?)",
              Operation.Admission, Searchtext, Searchtext, Searchtext, Searchtext, Searchtext);
      
                break;
            case TransactionFilter.Return:
                SearchFilter = CriteriaOperator.Parse("[OperationCode] ==? and (Contains([ContactName], ?)" +
              " or Contains([ContactPhone], ?) " +
              "or Contains([Number.ToString()], ?) " +
              "or Contains([Description], ?)"
               + "or Contains([AutorName],?)",
              Operation.Return, Searchtext, Searchtext, Searchtext, Searchtext, Searchtext);
     
                break;
            case TransactionFilter.Edit:
                SearchFilter = CriteriaOperator.Parse("[OperationCode] ==? and (Contains([ContactName], ?)" +
              " or Contains([ContactPhone], ?) " +
              "or Contains([Number.ToString()], ?) " +
              "or Contains([Description], ?)"
               + "or Contains([AutorName],?)",
              Operation.Edit, Searchtext, Searchtext, Searchtext, Searchtext,Searchtext);
    
                break;
            case TransactionFilter.Unpaid:
                SearchFilter = CriteriaOperator.Parse("[StateCode] !=? and [StateCode] !=? and (Contains([ContactName], ?)" +
              " or Contains([ContactPhone], ?) " +
              "or Contains([Number.ToString()], ?) " +
              "or Contains([Description], ?)"
               + "or Contains([AutorName],?)",
              Models.TransactionState.Paid,Models.TransactionState.PartiallyReturn, Searchtext, Searchtext, Searchtext, Searchtext,Searchtext);

                break;
            case TransactionFilter.Paid:
                SearchFilter = CriteriaOperator.Parse("[StateCode] ==? or [StateCode]==? and (Contains([ContactName], ?)" +
              " or Contains([ContactPhone], ?) " +
              "or Contains([Number.ToString()], ?) " +
              "or Contains([Description], ?)"
               + "or Contains([AutorName],?)",
               Models.TransactionState.Paid, Models.TransactionState.PartiallyReturn, Searchtext, Searchtext, Searchtext, Searchtext, Searchtext);

                break;
        }
    }

    // Изменение параметров сортировки
    [RelayCommand]
    private async Task GroupChanged(TransactionGroup group)
    {
        switch (group)
        {
            case TransactionGroup.CreatedTicks:
                CreatedTicks = true;
                GroupName = false;
                GroupPhone = false;
                break;
            case TransactionGroup.ContactName:
                CreatedTicks = false;
                GroupName = true;
                GroupPhone = false;
                break;
            case TransactionGroup.ContactPhone:
                CreatedTicks = false;
                GroupName = false;
                GroupPhone = true;
                break;
        }
        TransactionGroup = group;
        Preferences.Set($"{nameof(CreatedTicks)}Transaction{CurrentShopID}", CreatedTicks);
        Preferences.Set($"{nameof(GroupName)}Transaction{CurrentShopID}", GroupName);
        Preferences.Set($"{nameof(GroupPhone)}Transaction{CurrentShopID}", GroupPhone);
        await Task.CompletedTask;
    }

    // Изменение параметров сортировки
    [RelayCommand]
    private async Task SortChanged(TransactionSort _sort)
    {
        switch (_sort)
        {
            case TransactionSort.None:
                SortAsc = false;
                SortDis = false;
                SortNone = true; 
                //DataSortOrder  = new SortDescription { SortOrder = DevExpress.Data.ColumnSortOrder.None };
                CollectionView.GroupDescription.SortOrder = DevExpress.Data.ColumnSortOrder.None; 
                break;
            case TransactionSort.Descending:
                SortAsc = false;
                SortDis = true;
                SortNone = false;
                //DataSortOrder = new SortDescription { SortOrder = DevExpress.Data.ColumnSortOrder.Descending };
                CollectionView.GroupDescription.SortOrder = DevExpress.Data.ColumnSortOrder.Descending;
                break;
            case TransactionSort.Ascending:
                SortAsc = true;
                SortDis = false;
                SortNone = false;
                //DataSortOrder = new SortDescription { SortOrder = DevExpress.Data.ColumnSortOrder.Ascending };
                CollectionView.GroupDescription.SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
                break;
        }

        Preferences.Set($"{nameof(SortAsc)}Transaction{CurrentShopID}", SortAsc);
        Preferences.Set($"{nameof(SortDis)}Transaction{CurrentShopID}", SortDis);
        Preferences.Set($"{nameof(SortNone)}Transaction{CurrentShopID}", SortNone);
        await Task.CompletedTask;
    }

    // Изменение параметров фильтрации
    [RelayCommand]
    private async Task FilterChanged(TransactionFilter filter)
    {
        transactionFilter = filter;
        var _filter = "";
        switch (filter)
        {
            case TransactionFilter.All:                
                FilterAll = true;
                FilterSale = false;
                FilterAdmission = false;
                FilterReturn = false;
                FilterEdit = false;
                FilterUnpaid = false;
                FilterPaid = false;


                if (string.IsNullOrWhiteSpace(Searchtext))
                    SearchFilter = CriteriaOperator.Parse("[Id] !=null");
              
               
                else
                    MakeFilterWhitText();
                break;
            case TransactionFilter.Sale:
                FilterAll = false;
                FilterSale = true;
                FilterAdmission = false;
                FilterReturn = false;
                FilterEdit = false;
                FilterUnpaid = false;
                FilterPaid = false;
                if (string.IsNullOrWhiteSpace(Searchtext))
                    SearchFilter = CriteriaOperator.Parse("[OperationCode] ==?  ", Operation.Sale);
        
                else
                    MakeFilterWhitText();
                break;
            case TransactionFilter.Admission:
                FilterAll = false;
                FilterSale = false;
                FilterAdmission = true;
                FilterReturn = false;
                FilterEdit = false;
                FilterUnpaid = false;
                FilterPaid = false;
                if (string.IsNullOrWhiteSpace(Searchtext))
                    SearchFilter = CriteriaOperator.Parse("[OperationCode] ==?", Operation.Admission);
      
                else
                    MakeFilterWhitText();
                break;
            case TransactionFilter.Return:
                FilterAll = false;
                FilterSale = false;
                FilterAdmission = false;
                FilterReturn = true;
                FilterEdit = false;
                FilterUnpaid = false;
                FilterPaid = false;
                if (string.IsNullOrWhiteSpace(Searchtext))
                    SearchFilter = CriteriaOperator.Parse("[OperationCode] ==? ", Operation.Return);
         
                else
                    MakeFilterWhitText();
                break;
            case TransactionFilter.Edit:
                FilterAll = false;
                FilterSale = false;
                FilterAdmission = false;
                FilterReturn = false;
                FilterEdit = true;
                FilterUnpaid = false;
                FilterPaid = false;
                if (string.IsNullOrWhiteSpace(Searchtext))
                    SearchFilter = CriteriaOperator.Parse("[OperationCode] ==?", Operation.Edit);
        
                else
                    MakeFilterWhitText();
                break;
            case TransactionFilter.Unpaid:
                FilterAll = false;
                FilterSale = false;
                FilterAdmission = false;
                FilterReturn = false;
                FilterEdit = false;
                FilterUnpaid = true;
                FilterPaid = false;
                if (string.IsNullOrWhiteSpace(Searchtext))
                   
                    SearchFilter = CriteriaOperator.Parse("[StateCode] !=? and [StateCode] !=? ", Models.TransactionState.Paid, Models.TransactionState.PartiallyReturn);

                else
                    MakeFilterWhitText();
                break;
            case TransactionFilter.Paid:
                FilterAll = false;
                FilterSale = false;
                FilterAdmission = false;
                FilterReturn = false;
                FilterEdit = false;
                FilterUnpaid = false;
                FilterPaid = true;
                if (string.IsNullOrWhiteSpace(Searchtext))
                   SearchFilter = CriteriaOperator.Parse("[StateCode] ==? or [StateCode] ==?", Models.TransactionState.Paid, Models.TransactionState.PartiallyReturn);

                else
                    MakeFilterWhitText();
                break;

            case TransactionFilter.Period:
                FilterPeriod = FilterPeriod ? false : true;
                if (FilterPeriod)
                    FiltredTransactionPeriod();
                else
                    Transactions = realm.All<Transaction>().Where(x => x.OwnerId == CurrentShopID);
                break;

        }

        Preferences.Set($"{nameof(FilterPeriod)}Transaction{CurrentShopID}", FilterPeriod);
        Preferences.Set($"{nameof(FilterAll)}Transaction{CurrentShopID}", FilterAll);
        Preferences.Set($"{nameof(FilterSale)}Transaction{CurrentShopID}", FilterSale);
        Preferences.Set($"{nameof(FilterAdmission)}Transaction{CurrentShopID}", FilterAdmission);
        Preferences.Set($"{nameof(FilterReturn)}Transaction{CurrentShopID}", FilterReturn);
        Preferences.Set($"{nameof(FilterEdit)}Transaction{CurrentShopID}", FilterEdit);
        Preferences.Set($"{nameof(FilterPaid)}Transaction{CurrentShopID}", FilterPaid);
        Preferences.Set($"{nameof(FilterUnpaid)}Transaction{CurrentShopID}", FilterUnpaid);
        await Task.CompletedTask;
        //await GetItems();
    }

    // Вызов профиля
   

    // Показать скрыть фильтры
    [RelayCommand]
    private async Task HideShowFilters()
    {
        if ((FilterStates)FilterbarState.CurrentState == FilterStates.Hide)
            FilterbarState.Go(FilterStates.Show);
        else
            FilterbarState.Go(FilterStates.Hide);
        await Task.CompletedTask;
    }

    //фильтрация по периоду
    internal async Task FiltredTransactionPeriod()
    {
        var ts = new TimeSpan(0, 0, 0);
        var start = StartDate.Date + ts;
        ts=new TimeSpan(23, 59, 59);
        var end = EndDate.Date + ts;

        var startDate = DateTimeOffset.Parse(start.ToString());
        var endDate = DateTimeOffset.Parse(end.ToString());

        Transactions = realm.All<Transaction>().Where(x => x.OwnerId == CurrentShopID && ( x.CreateDate >=startDate  && x.CreateDate <=endDate )); 
        await Task.CompletedTask;
        // await GetItems();
/*        Preferences.Set($"{nameof(StartDate)}Transaction{CurrentShopID}", StartDate);
        Preferences.Set($"{nameof(EndDate)}Transaction{CurrentShopID}", EndDate);*/

    }
}
