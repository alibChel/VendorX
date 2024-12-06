using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AppCenter;
using Rg.Plugins.Popup.Services;
//using DevExpress.Data.Filtering;
//using DevExpress.Maui.Core;
using Vendor.Models;
using VendorX.Resources;
using VendorX.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;
using ZXing.QrCode.Internal;

namespace Vendor.ViewModels;

public partial class StockViewModel : BaseViewModel
{
    // Все товары
    [ObservableProperty]
    public IQueryable<Item> items;

    // Аниматор фильтров
    public AnimationStateMachine FilterbarState { get; set; }

    /// <summary>
    /// searchtext - Текст поиска
    /// walletTag - значек валюты
    /// </summary>
    [ObservableProperty]
    private string searchtext, walletTag = "₸";


    // Аниматор сканерКамеры
    public AnimationStateMachine CameraScanbarState { get; set; }

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
    private string currentUserId;

    [ObservableProperty]
    ZXingScannerView barcodeScanner;

    [ObservableProperty]
    Grid gridScanner;
    // Параметры сортировки и фильтрации
    /// <summary>
    /// sortNew -сначала новые
    /// sortLowPrice -сначала дешевые
    /// sortHipricce -сначала дорогие
    /// filterAll -все
    /// filterActual -актуальные
    /// filterArchive -архивные
    /// </summary>
    [ObservableProperty]
    private bool sortNew, sortLowPrice, sortHiStock, sortLowStock, sortHiPrice, filterAll, filterActual, filterArchive;
    [ObservableProperty]
    bool isScannerVisible;

    // фильтр товара
    private ItemFilter filter
    {
        get
        {
            if (FilterAll)
                return ItemFilter.All;
            if (FilterActual)
                return ItemFilter.Actual;

            return ItemFilter.Archive;
        }
    }

    // сортировка товара
    private ItemSort sort
    {
        get
        {
            if (SortHiPrice)
                return ItemSort.Max;
            if (SortLowPrice)
                return ItemSort.Min;
            if (SortHiStock)
                return ItemSort.Hi;
            if (SortLowStock)
                return ItemSort.Low;

            return ItemSort.New;
        }
    }

    // Корректировка
    [ObservableProperty]
    private bool isEdit;

    // Список команд открыт/закрыт
    [ObservableProperty]
    private bool isDopExpandet, isDopCommandVisible = true;

    //[ObservableProperty]
    //CriteriaOperator collectionFilter;

    //[ObservableProperty]
    //private DataSortOrder dataSortOrder;

    [ObservableProperty]
    StockSortingNames stockSortingNames;

    // для отслеживания первой загрузки
    private bool firstload;


    public StockViewModel()
    {

        WeakReferenceMessenger.Default.Register<CurrentShopChengetMessage>(this, async (sender, message) =>
        {
            realm = RealmService.GetMainThreadRealm();
            Shop = realm.Find<Shop>(CurrentShopID);
            WalletTag = Shop.WalletTag;
            //stock = realm.Find<Stock>(CurrentShopID);
            role = realm.All<Member>().Where(x => x.OwnerId == RealmService.CurrentUser.Identities[0].Id)?.FirstOrDefault(x => x.Shop == shop)?.Role ?? UserRole.User;
            if (role == UserRole.User)
                await Shell.Current.GoToAsync($"//MainPage");
            //Stock = realm.All<Stock>().FirstOrDefault();
            else
                await GetItems();
            //await Task.CompletedTask;
        });


    }

    internal async Task OnAppearing()
    {

        //Items = realm.All<Item>();
        if (!firstload)
        {
            await Task.Delay(50);
            IsBusy = true;
            realm = RealmService.GetMainThreadRealm();
            currentUserId = RealmService.CurrentUser.Identities[0].Id;
            Profile = realm.All<Profile>().FirstOrDefault();
            Shop = realm.Find<Shop>(CurrentShopID);

            await GetItems();
            IsBusy = false;
        }
        WalletTag = Shop?.WalletTag;
        //await Task.CompletedTask;
        OnPropertyChanged(nameof(Profile));
        firstload = true;
        ChekNotification();
    }

    // Редактирование текущего магазина
   

    // Переход к деталям 
    [RelayCommand]
    private async void GoToDetails(Item item)
    {
        IsDopExpandet = false;
        if (IsEdit)
            return;

        //var itemParameter = new Dictionary<string, string>() { { "item", item.Id }, { "count", incart.ToString()} };
        await Shell.Current.GoToAsync($"{nameof(ItemDetail)}?item={item.Id}&count=0");


        //await Shell.Current.GoToAsync(nameof(ItemDetail), true, new Dictionary<string, object>
        //{
        //    { "item", item }
        //});
    }

    // Создание товара
    [RelayCommand]
    public async Task CreateNewItem()
    {
        await Shell.Current.GoToAsync(nameof(ItemDetail));
    }

    // Взвимодействие с доп кнопками(Отобразись скрыть)
    [RelayCommand]
    private void ChangeDopExpanded()
    {
        IsDopExpandet = !IsDopExpandet;
    }

    //
    private async Task GetItems()
    {
        // var _sort = "";
        // var _filter = "";
        if (IsEdit)
            return;
        SortNew = Preferences.Get($"{nameof(SortNew)}stock{CurrentShopID}", true);
        SortLowPrice = Preferences.Get($"{nameof(SortLowPrice)}stock{CurrentShopID}", false);
        SortHiPrice = Preferences.Get($"{nameof(SortHiPrice)}stock{CurrentShopID}", false);
        SortHiStock = Preferences.Get($"{nameof(SortHiStock)}stock{CurrentShopID}", false);
        SortLowStock = Preferences.Get($"{nameof(SortLowStock)}stock{CurrentShopID}", false);

        FilterAll = Preferences.Get($"{nameof(FilterAll)}stock{CurrentShopID}", false);
        FilterActual = Preferences.Get($"{nameof(FilterActual)}stock{CurrentShopID}", true);
        FilterArchive = Preferences.Get($"{nameof(FilterArchive)}stock{CurrentShopID}", false);

        var _sort = "";
        var _filter = "";

        switch (sort)
        {
            case ItemSort.Min:

                _sort = "SORT(price ASC)";
                break;
            case ItemSort.Max:

                _sort = "SORT(price DESC)";
                break;
            case ItemSort.New:

                _sort = "SORT(create_date DESC)";
                break;

            case ItemSort.Hi:

                _sort = "SORT(on_stock DESC)";
                break;

            case ItemSort.Low:
                _sort = "SORT(on_stock ASC)";
                break;
        }

        if (string.IsNullOrWhiteSpace(Searchtext))
        {
            switch (filter)
            {
                case ItemFilter.Actual:

                    _filter = "is_arhive == false";
                    break;
                case ItemFilter.Archive:

                    _filter = "is_arhive == true";
                    break;
                case ItemFilter.All:
                    _filter = "is_arhive != nil";

                    break;
            }

        }
        else
        {
            switch (filter)
            {
                case ItemFilter.Actual:

                    _filter = $"is_arhive == false AND (name CONTAINS[c] '{Searchtext}' OR description CONTAINS[c] '{Searchtext}' OR barcode CONTAINS[c] '{Searchtext}')";
                    break;
                case ItemFilter.Archive:

                    _filter = $"is_arhive == true AND (name CONTAINS[c] '{Searchtext}' OR description CONTAINS[c] '{Searchtext}' OR barcode CONTAINS[c] '{Searchtext}')";
                    break;
                case ItemFilter.All:
                    _filter = $"(name CONTAINS[c] '{Searchtext}' OR description CONTAINS[c] '{Searchtext}' OR barcode CONTAINS[c] '{Searchtext}')";

                    break;
            }
        }

        Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
        {
            Items = realm.All<Item>().Filter($"{_filter} {_sort}");

        });

        OnPropertyChanged(nameof(Items));

        //Items = realm.All<Item>();

        await Task.CompletedTask;
    }

    // Изменение текста поиска
    partial void OnSearchtextChanged(string value)
    {
        MakeFilter();
    }

    private async void MakeFilter()
    {
        //if (string.IsNullOrWhiteSpace(Searchtext))
        //{
        //    switch (filter)
        //    {
        //        case ItemFilter.All:
        //            CollectionFilter = CriteriaOperator.FromLambda<Item>(x => x.Id != null);
        //            break;
        //        case ItemFilter.Actual:
        //            CollectionFilter = CriteriaOperator.FromLambda<Item>(x => !x.IsArchive);
        //            break;
        //        case ItemFilter.Archive:
        //            CollectionFilter = CriteriaOperator.FromLambda<Item>(x => x.IsArchive);
        //            break;
        //    }
        //}
        //else
        //{
        //    switch (filter)
        //    {
        //        case ItemFilter.All:
        //            CollectionFilter = CriteriaOperator.FromLambda<Item>(x =>
        //            x.Name.Contains(Searchtext)
        //            || x.Description.Contains(Searchtext)
        //            || x.BarCode.ToString().Contains(Searchtext));
        //            break;
        //        case ItemFilter.Actual:
        //            CollectionFilter = CriteriaOperator.FromLambda<Item>(x => !x.IsArchive &&
        //            (x.Name.Contains(Searchtext)
        //            || x.Description.Contains(Searchtext)
        //            || x.BarCode.ToString().Contains(Searchtext)));
        //            break;
        //        case ItemFilter.Archive:
        //            CollectionFilter = CriteriaOperator.FromLambda<Item>(x => x.IsArchive &&
        //            (x.Name.Contains(Searchtext)
        //            || x.Description.Contains(Searchtext)
        //            || x.BarCode.ToString().Contains(Searchtext)));
        //            break;
        //    }
        //}
        await GetItems();
    }

    // Изменение параметров сортировки
    [RelayCommand]
    private async Task SortChanged(ItemSort sort)
    {

        switch (sort)
        {
            case ItemSort.Min:
                SortNew = false;
                SortHiPrice = false;
                SortLowPrice = true;
                SortHiStock = false;
                SortLowStock = false;
                //StockSortingNames = StockSortingNames.Price;
                //DataSortOrder = DataSortOrder.Ascending;
                break;
            case ItemSort.Max:
                SortNew = false;
                SortHiPrice = true;
                SortLowPrice = false;
                SortHiStock = false;
                SortLowStock = false;
                //StockSortingNames = StockSortingNames.Price;
                //DataSortOrder = DataSortOrder.Descending;
                break;
            case ItemSort.New:
                SortNew = true;
                SortHiPrice = false;
                SortLowPrice = false;
                SortHiStock = false;
                SortLowStock = false;
                //StockSortingNames = StockSortingNames.CreateDate;
                //DataSortOrder = DataSortOrder.Descending;
                break;
            case ItemSort.Hi:
                SortNew = false;
                SortHiPrice = false;
                SortLowPrice = false;
                SortHiStock = true;
                SortLowStock = false;
                break;
            case ItemSort.Low:
                SortNew = false;
                SortHiPrice = false;
                SortLowPrice = false;
                SortHiStock = false;
                SortLowStock = true;
                break;
        }

        Preferences.Set($"{nameof(SortNew)}stock{CurrentShopID}", SortNew);
        Preferences.Set($"{nameof(SortLowPrice)}stock{CurrentShopID}", SortLowPrice);
        Preferences.Set($"{nameof(SortHiPrice)}stock{CurrentShopID}", SortHiPrice);
        Preferences.Set($"{nameof(SortHiStock)}stock{CurrentShopID}", SortHiStock);
        Preferences.Set($"{nameof(SortLowStock)}stock{CurrentShopID}", SortLowStock);
        MakeFilter();

    }

    // Изменение параметров фильтрации
    [RelayCommand]
    private async Task FilterChanged(ItemFilter _filter)
    {
        switch (_filter)
        {
            case ItemFilter.Actual:
                FilterActual = true;
                FilterAll = false;
                FilterArchive = false;
                break;
            case ItemFilter.Archive:
                FilterActual = false;
                FilterAll = false;
                FilterArchive = true;
                break;
            case ItemFilter.All:
                FilterActual = false;
                FilterAll = true;
                FilterArchive = false;
                break;
        }


        Preferences.Set($"{nameof(FilterAll)}stock{CurrentShopID}", FilterAll);
        Preferences.Set($"{nameof(FilterActual)}stock{CurrentShopID}", FilterActual);
        Preferences.Set($"{nameof(FilterArchive)}stock{CurrentShopID}", FilterArchive);
        MakeFilter();
        await Task.CompletedTask;
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

    // Показать скрыть скан
    [RelayCommand]
    private async Task HideShowScan()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await DialogService.ShowToast($"{AppResources.CameraExtError}");
                return;
            }

        }
        IsScannerVisible = ((CameraScanStates)CameraScanbarState.CurrentState == CameraScanStates.Hide);

        if ((CameraScanStates)CameraScanbarState.CurrentState == CameraScanStates.Hide)
        {
            GridScanner.Children?.Remove(BarcodeScanner);
            GridScanner.Children.Add(BarcodeScanner);
            await Task.Delay(200);
            CameraScanbarState.Go(CameraScanStates.Show);
        }


        else
        {
            GridScanner.Children.Remove(BarcodeScanner);
            await Task.Delay(200);
            CameraScanbarState.Go(CameraScanStates.Hide);
        }

        await Task.CompletedTask;
    }


    // Редактирование склада
    [RelayCommand]
    private async Task CangeEditStock()
    {
        IsEdit = !IsEdit;
        IsDopCommandVisible = !IsDopCommandVisible;
        IsDopExpandet = false;

    }

    // Прием на склав
    [RelayCommand]
    private async void AdmistionStock()
    {
        IsDopExpandet = false;
        await Shell.Current.GoToAsync(nameof(ArrivalPage));
    }


    // Прибавить товар на складе
    [RelayCommand]
    private async void AddItemOnStock(Item item)
    {
        try
        {
            await realm.WriteAsync(() =>
            {
                item.OnStock++;

            });
            await Task.Delay(250);
        }
        catch (Exception) { }
    }

    // отнять товар со склада
    [RelayCommand]
    private async void MinustemOnStock(Item item)
    {
        try
        {
            if (item.OnStock > 0)
                await realm.WriteAsync(() =>
                {
                    item.OnStock--;

                });
            await Task.Delay(250);
        }
        catch (Exception) { }
    }

    // searchBarcode
    public async Task SerchForBarCode(string searchtext)
    {

        //await Task.Delay(500);

        Searchtext = searchtext;
        IsScannerVisible = ((CameraScanStates)CameraScanbarState.CurrentState == CameraScanStates.Hide);

        if ((CameraScanStates)CameraScanbarState.CurrentState == CameraScanStates.Hide)
        {
            GridScanner.Children.Add(BarcodeScanner);
            CameraScanbarState.Go(CameraScanStates.Show);
        }


        else
        {
            CameraScanbarState.Go(CameraScanStates.Hide);
            GridScanner.Children.Remove(BarcodeScanner);
        }
        await Task.CompletedTask;

    }

    internal async void OnDisappearing()
    {

        await Task.Delay(200);
        Searchtext = null;

        //WeakReferenceMessenger.Default.Unregister<CurrentShopChengetMessage>(this);
        if (IsScannerVisible)
            await HideShowScan();
    }
}
