using CommunityToolkit.Mvvm.Messaging;
using DevExpress.XamarinForms.CollectionView;
using DevExpress.XamarinForms.Core.Filtering;
using VendorX.Models;
using VendorX.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Vendor.ViewModels;

public partial class OrderPageViewModel : BaseViewModel
{


    // Аниматор фильтров
    public AnimationStateMachine FilterbarState { get; set; }

    /// <summary>
    /// searchtext - Текст поиска
    /// walletTag - значек валюты
    /// </summary>
    [ObservableProperty]
    private string searchtext, walletTag = "₸";



    [ObservableProperty]
    CriteriaOperator searchFilter, searchOrderFilter;

    [ObservableProperty]
    bool filterAllOrders = true, filterProcessOrder, filterDeliveredOrder, filterCancelOrder, filterShippedOrder, isSortDesc;



    [ObservableProperty]
    IQueryable<Order> orders;

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


    private OrderFilter orderFilter;

    // для отслеживания первой загрузки
    private bool firstload;

    public DXCollectionView OrderCollectionView { get; set; }



    internal async Task OnAppearing()
    {
        if (!firstload)
        {


            WeakReferenceMessenger.Default.Register<CurrentShopChengetMessage>(this, async (sender, message) =>
            {
                realm = RealmService.GetMainThreadRealm();
                Shop = realm.Find<Shop>(CurrentShopID);
                WalletTag = Shop.WalletTag;
                role = realm.All<Member>().Where(x => x.OwnerId == RealmService.CurrentUser.Identities[0].Id)?.FirstOrDefault(x => x.Shop == shop)?.Role ?? UserRole.User;
                MakeFilterWithSearchOrder();
            });
            IsBusy = true;
            realm = RealmService.GetMainThreadRealm();
            Profile = realm.All<Profile>().FirstOrDefault();

            MakeFilterWithSearchOrder();
            IsBusy = false;
        }

        firstload = true;
        Shop = realm.Find<Shop>(CurrentShopID);
        WalletTag = Shop.WalletTag;
        Orders = realm.All<Order>();


        OnPropertyChanged(nameof(Profile));
        ChekNotification();

    }

    [RelayCommand]
    private async Task SortChanged(TransactionSort _sort)
    {

        OrderCollectionView.GroupDescription.SortOrder = _sort == TransactionSort.Descending ? DevExpress.Data.ColumnSortOrder.Descending : DevExpress.Data.ColumnSortOrder.Ascending;
        IsSortDesc = _sort == TransactionSort.Descending;

    }





    [RelayCommand]
    private async void GoToOrderDetail(Order order)
    {
        if (IsBusy) return;
        IsBusy = true;

        await Shell.Current.GoToAsync($"{nameof(OrderDetailPage)}?Order={order?.Id}");
        IsBusy = false;
    }







    private void MakeFilterWithSearchOrder()
    {
        try
        {
            switch (orderFilter)
            {
                case OrderFilter.All:
                    SearchOrderFilter = CriteriaOperator.Parse(
                        "Contains([ShopName], ?)" +
                        "or [OrdersItems][].Any(ItemName In (?))" +
                        "or Contains([Number.ToString()], ?) " +
                        "or Contains([Price.ToString()], ?) " +
                        "or Contains([Description], ?) ",

                        Searchtext, Searchtext, Searchtext, Searchtext, Searchtext);

                    break;
                case OrderFilter.InProcess:

                    SearchOrderFilter = CriteriaOperator.Parse(
                        "[OrderStatus] ==? " +
                        "and Contains([ShopName], ?)" +
                        "or [OrdersItems][].Any(ItemName In (?))" +
                        "or Contains([Number.ToString()], ?) " +
                        "or Contains([Price.ToString()], ?) " +
                        "or Contains([Description], ?) ",
                       OrderStatus.Processing, Searchtext, Searchtext, Searchtext, Searchtext, Searchtext);

                    break;
                case OrderFilter.Delivered:
                    SearchOrderFilter = CriteriaOperator.Parse(
                         "[OrderStatus] ==? " +
                         "and Contains([ShopName], ?)" +
                         "or [OrdersItems][].Any(ItemName In (?))" +
                         "or Contains([Number.ToString()], ?) " +
                         "or Contains([Price.ToString()], ?) " +
                         "or Contains([Description], ?) ",
                        OrderStatus.Delivered, Searchtext, Searchtext, Searchtext, Searchtext, Searchtext);
                    break;
                case OrderFilter.Canceled:
                    SearchOrderFilter = CriteriaOperator.Parse(
                "[OrderStatus] ==? " +
                "and Contains([ShopName], ?)" +
                "or [OrdersItems][].Any(ItemName In (?))" +
                "or Contains([Number.ToString()], ?) " +
                "or Contains([Price.ToString()], ?) " +
                "or Contains([Description], ?) ",
               OrderStatus.Cancelled, Searchtext, Searchtext, Searchtext, Searchtext, Searchtext);
                    break;


                case OrderFilter.Shipped:
                    SearchOrderFilter = CriteriaOperator.Parse(
                "[OrderStatus] ==? " +
                "and Contains([ShopName], ?)" +
                "or [OrdersItems][].Any(ItemName In (?))" +
                "or Contains([Number.ToString()], ?) " +
                "or Contains([Price.ToString()], ?) " +
                "or Contains([Description], ?) ",
               OrderStatus.Shipped, Searchtext, Searchtext, Searchtext, Searchtext, Searchtext);
                    break;
            }
        }
        catch { }
    }








    // Изменение параметров заказа
    [RelayCommand]
    private async Task FilterChangedOrder(OrderFilter filter)
    {
        orderFilter = filter;

        switch (filter)
        {
            case OrderFilter.All:
                FilterAllOrders = true;
                FilterCancelOrder = false;
                FilterProcessOrder = false;
                FilterShippedOrder = false;
                FilterDeliveredOrder = false;
                if (string.IsNullOrWhiteSpace(Searchtext))
                    SearchOrderFilter = CriteriaOperator.Parse("[Id] !=null");
                else
                    MakeFilterWithSearchOrder();
                break;
            case OrderFilter.InProcess:
                FilterAllOrders = false;
                FilterCancelOrder = false;
                FilterShippedOrder = false;
                FilterProcessOrder = true;
                FilterDeliveredOrder = false;
                if (string.IsNullOrWhiteSpace(Searchtext))
                    SearchOrderFilter = CriteriaOperator.Parse("[OrderStatus] In (?, ?, ?)", OrderStatus.Processing, OrderStatus.Pending, OrderStatus.AcceptedByMerchant);

                else
                    MakeFilterWithSearchOrder();
                break;
            case OrderFilter.Delivered:
                FilterAllOrders = false;
                FilterCancelOrder = false;
                FilterProcessOrder = false;
                FilterShippedOrder = false;
                FilterDeliveredOrder = true;
                if (string.IsNullOrWhiteSpace(Searchtext))
                    SearchOrderFilter = CriteriaOperator.Parse("[OrderStatus] ==?", OrderStatus.Delivered);

                else
                    MakeFilterWithSearchOrder();
                break;
            case OrderFilter.Canceled:
                FilterAllOrders = false;
                FilterCancelOrder = true;
                FilterProcessOrder = false;
                FilterDeliveredOrder = false;
                FilterShippedOrder = false;
                if (string.IsNullOrWhiteSpace(Searchtext))
                    SearchOrderFilter = CriteriaOperator.Parse("[OrderStatus] ==? ", OrderStatus.Cancelled);

                else
                    MakeFilterWithSearchOrder();
                break;


            case OrderFilter.Shipped:
                FilterAllOrders = false;
                FilterCancelOrder = false;
                FilterProcessOrder = false;
                FilterDeliveredOrder = false;
                FilterShippedOrder = true;
                if (string.IsNullOrWhiteSpace(Searchtext))
                    SearchOrderFilter = CriteriaOperator.Parse("[OrderStatus] ==? ", OrderStatus.Shipped);

                else
                    MakeFilterWithSearchOrder();
                break;


        }

        Preferences.Set($"{nameof(FilterAllOrders)}Order{CurrentShopID}", FilterAllOrders);
        Preferences.Set($"{nameof(FilterCancelOrder)}Order{CurrentShopID}", FilterCancelOrder);
        Preferences.Set($"{nameof(FilterProcessOrder)}Order{CurrentShopID}", FilterProcessOrder);
        Preferences.Set($"{nameof(FilterDeliveredOrder)}Order{CurrentShopID}", FilterDeliveredOrder);

        await Task.CompletedTask;

        await Task.CompletedTask;
        //await GetItems();
    }





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



}
