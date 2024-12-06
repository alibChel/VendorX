using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using Rg.Plugins.Popup.Services;
using Vendor.Models;
using VendorX.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

using VendorX.Resources;

using ZXing.Net.Mobile.Forms;


namespace Vendor.ViewModels;

public partial class ArrivalPageViewModel : BaseViewModel
{
    // Все товары
    [ObservableProperty]
    private IQueryable<Item> items;

    private IQueryable<Item> all_items;

    // Корзина
    public ObservableCollection<TransactionItem> CheckOutItems { get; set; } = new ObservableCollection<TransactionItem>();

    // Аниматор корзины
    public AnimationStateMachine CartState { get; set; }
    // Аниматор фильтров
    public AnimationStateMachine FilterbarState { get; set; }
    //Аниматор камеры
    public AnimationStateMachine CameraScanbarState { get; set; }

    // высота карточки товара
    [ObservableProperty]
    private int itemCellHeight;

    /// <summary>
    /// searchtext - Текст поиска
    /// walletTag - значек валюты
    /// Description - описание транзакции
    /// </summary>
    [ObservableProperty]
    private string searchtext, walletTag = "₸", description;

    /// <summary>
    /// ContactName - Имя добавленного контакта
    /// ContactPhone - Телефон добавленного контакта
    /// IsContactVisible - Отображение добавленного контакта 
    /// </summary>
    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsContactVisible))]
    private string contactName, contactPhone;

    [ObservableProperty]
    bool isScannerVisible;

    [ObservableProperty]
    Grid gridScanner;

    [ObservableProperty]
    ZXingScannerView barcodeScanner;


    public bool IsContactVisible { get => (!string.IsNullOrWhiteSpace(ContactName) || !string.IsNullOrWhiteSpace(ContactPhone)); }

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
    // Видемость кнопки создания товара
    [ObservableProperty]
    private bool isAddButoonVisible = true;

    /// <summary>
    /// total - общая стоимость транзакции
    /// totalPrice - общая стоимость транзакции к оплате
    /// totalDicount - скидка на транзакцию
    /// </summary>
    [ObservableProperty, NotifyPropertyChangedFor(nameof(TotalPriceText))]
    private double total, totalPrice, totalDicount;

    // внесенная сумма
    [ObservableProperty, NotifyPropertyChangedFor(nameof(SummDiffirence))]
    private double depSumm;

    // Разница внесенной суммы и финальной стоимости 
    public double SummDiffirence { get => Math.Abs(TotalPrice - DepSumm); }

    // Текст разницы сумм Долг или Сдача
    [ObservableProperty]
    private string summDiffirenceText;
    // Текст разницы сумм Долг или Сдача
    [ObservableProperty]
    private Color summDiffirenceColor = Color.FromHex("#66B2F0");

    // Общая стоимость + значек валюты для отображения
    public string TotalPriceText { get => $"{TotalPrice} {WalletTag}"; }

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
    private bool sortNew,sortLowPrice,sortHiPrice,filterAll,filterActual,filterArchive;

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

            return ItemSort.New;
        }
    }

    public ArrivalPageViewModel()
    {
        Title = AppResources.AdmissionItemLabel;
            //"Поступление товара";
        realm = RealmService.GetMainThreadRealm();
        Profile = realm.All<Profile>().FirstOrDefault();
        _ = GetItems();
    }

    internal async Task OnAppearing()
    {  
        IsBusy = false;

        // Событие Смены магазина если корзина и т.п. заполнена ножно очищать
        WeakReferenceMessenger.Default.Register<CurrentShopChengetMessage>(this, async (sender, message) =>
        {
            await ClearCart();
            shop = realm.Find<Shop>(CurrentShopID);
            WalletTag = shop.WalletTag;
            role = realm.All<Member>().Where(x => x.OwnerId == RealmService.CurrentUser.Identities[0].Id)?.FirstOrDefault(x => x.Shop == shop)?.Role ?? UserRole.User;
            if (role == UserRole.User)
                await Shell.Current.GoToAsync($"//MainPage");
            else
            {
                CheckAddButon();
                
                await GetItems();
            }
        });

        //Подписка на получение события сканерование ШК
        WeakReferenceMessenger.Default.Register<ItemBarcodeScanMessage>(this, async (sender, message) =>
        {
            var item = all_items.FirstOrDefault(x => x.BarCode == message.Value);
            if (item != null)
                await AddItemToCart(item);
            else
                await DialogService.ShowToast($"{AppResources.NoBarcodeInListError} {message.Value}");
        });
        WeakReferenceMessenger.Default.Unregister<ItemInCartChangeMessage>(this);
        shop = realm.Find<Shop>(CurrentShopID);
        WalletTag = shop.WalletTag;
        //stock = realm.Find<Stock>(CurrentShopID);
      
            role = realm.All<Member>().FirstOrDefault(x => x.Shop == shop).Role;



        CheckAddButon();
        all_items = realm.All<Item>();
        await Task.CompletedTask;
    }

    internal async Task OnDisappearing()
    {
        WeakReferenceMessenger.Default.Unregister<CurrentShopChengetMessage>(this);
        WeakReferenceMessenger.Default.Unregister<ItemBarcodeScanMessage>(this);
        if(IsScannerVisible)
            await HideShowScan();
        await Task.CompletedTask;
    }

    // Расчет размеров
    internal void OnSizeAllocated(double width, double height)
    {
        ItemCellHeight = (int)((width / 3 - 10) * 1.35);
    }

    // Видимость кнопки создать
    public void CheckAddButon()
    {
        if (role == UserRole.User)
        {
            IsAddButoonVisible = false;
            return;
        }

        if (CartState == null)
            return;

        if ((CartStates)CartState.CurrentState != CartStates.ShowCart && (CameraScanStates)CameraScanbarState.CurrentState == CameraScanStates.Hide)
            IsAddButoonVisible = true;
        else
            IsAddButoonVisible = false;
    }

    // Редактирование товара
    [RelayCommand]
    public async Task EditItem(Item item)
    {
        //var itemParameter = new Dictionary<string, object>() { { "item", item } };
        //await Shell.Current.GoToAsync(nameof(ItemDetail), itemParameter);
        try
        {
            WeakReferenceMessenger.Default.Register<ItemInCartChangeMessage>(this, async (sender, message) =>
            {

                var _item = CheckOutItems.FirstOrDefault(x => ReferenceEquals(x.ItemId, item));

                if (message.Value == 0 && _item != null)
                {
                    await RemoveItemFromCart(_item);
                }
                else
                {
                    if (_item == null)
                    {
                        _item = new TransactionItem
                        {
                            ItemId = item,
                            ItemName = item.Name,
                            ItemPrice = item.Price,
                            PhotoUrl = item.PhotoUrl,
                            Count = (int)message.Value,
                            Price = item.Price,
                            Cost = item.Cost
                        };
                        CheckOutItems.Insert(0, _item);
                    }
                    else
                    {
                        if (item.Cost != _item.ItemCost)
                        {
                            _item.ItemCost = item.Cost;
                            if (_item.Price > _item.ItemPrice)
                            {
                                _item.Markup = Math.Abs((int)(100 - _item.Cost / (_item.ItemCost / 100)));
                                _item.Discount = 0;
                            }
                            if (_item.Cost < _item.ItemCost)
                            {
                                _item.Discount = Math.Abs((int)(100 - _item.Cost / (_item.ItemCost / 100)));
                                _item.Markup = 0;
                            }
                            if (_item.Cost == _item.ItemCost)
                            {
                                _item.Markup = 0;
                                _item.Discount = 0;
                            }

                        }
                        _item.Count = (int)message.Value;
                    }
                }

                Total = CheckOutItems.Sum(x => x.FinalCost);
                TotalPrice = Total - TotalDicount;
                DepSumm = TotalPrice;
                if ((CartStates)CartState.CurrentState == CartStates.HideCart)
                    CartState.Go(CartStates.ShowCartThreshold);
                CheckAddButon();
            });



            var incart = CheckOutItems.FirstOrDefault(x => x.ItemId?.Id == item.Id)?.Count ?? 0;
            await Shell.Current.GoToAsync($"{nameof(ItemDetail)}?item={item.Id}&count={incart}&avrial=true");
        }
        catch(Exception ex) { }
        }

    // Создание товара
    [RelayCommand]
    public async Task CreateNewItem()
    {

        await Shell.Current.GoToAsync($"{nameof(ItemDetail)}?count={0}&avrial=true");

    }

    // Добавление товара в корзину 
    [RelayCommand]
    private async Task AddItemToCart(Item item)
    {
        if (item == null || item.IsArchive)
        {
            if (item.IsArchive)
                await DialogService.ShowToast($"{AppResources.CantAddError} " + item.Name + $" {AppResources.InArchiveError}");
            //$"невозможно добавить {item.Name} находится в архиве");
            return;
        }

        var _item = CheckOutItems.FirstOrDefault(x => x.ItemId?.Id == item.Id);
        if (_item == null)
        {
            _item = new TransactionItem
            {
                ItemId = item,
                ItemName = item.Name,
                ItemCost = item.Cost,
                PhotoUrl = item.PhotoUrl,
                Count = 1,                
                Cost = item.Cost
            };
            CheckOutItems.Add(_item);
        }
        else
        {
            if (item.Cost != _item.ItemCost)
            {
                _item.ItemCost = item.Cost;

                if (_item.Cost < _item.ItemCost)
                {
                    _item.Markup = Math.Abs((int)(100 - _item.Cost / (_item.ItemCost / 100)));
                    _item.Discount = 0;
                }
                if (_item.Cost > _item.ItemCost)
                {
                    _item.Discount = Math.Abs((int)(100 - _item.Cost / (_item.ItemCost / 100)));
                    _item.Markup = 0;
                }
                if (_item.Cost == _item.ItemCost)
                {
                    _item.Markup = 0;
                    _item.Discount = 0;
                }
                
            }
                
            _item.Count++;            
        }

        Total = CheckOutItems.Sum(x => x.FinalCost);
        TotalPrice = Total - TotalDicount;
        DepSumm = TotalPrice;
        if ((CartStates)CartState.CurrentState == CartStates.HideCart)
            CartState.Go(CartStates.ShowCartThreshold);
        CheckAddButon();
    }

    // Добавление товара в корзину 
    [RelayCommand]
    private async Task AddTransactionItemToCart(TransactionItem item)
    {
        if (item.ItemId == null) return;

        var _item = all_items.FirstOrDefault(x => x.Id == item.ItemId.Id);
        await AddItemToCart(_item);
    }

    // Удаление товара из корзины
    [RelayCommand]
    private async Task RemoveItemFromCart(TransactionItem transactionItem)
    {
        if (transactionItem.Count == 1)
            CheckOutItems.Remove(transactionItem);
        else
        {
            if (transactionItem.ItemId == null) return;

            var item = all_items.FirstOrDefault(x => x.Id==transactionItem.ItemId.Id);

            if (item.Cost != transactionItem.ItemCost)
            {
                transactionItem.ItemCost = item.Cost;
                if (transactionItem.Cost < transactionItem.ItemCost)
                {
                    transactionItem.Markup = Math.Abs((int)(100 - transactionItem.Cost / (transactionItem.ItemCost / 100)));
                    transactionItem.Discount = 0;
                }
                if (transactionItem.Cost > transactionItem.ItemCost)
                {
                    transactionItem.Discount = Math.Abs((int)(100 - transactionItem.Cost / (transactionItem.ItemCost / 100)));
                    transactionItem.Markup = 0;
                }
                if (transactionItem.Cost == transactionItem.ItemCost)
                {
                    transactionItem.Markup = 0;
                    transactionItem.Discount = 0;
                }
                transactionItem.ItemCost = item.Cost;
            }

            transactionItem.Count--;
        }
            

        if (CheckOutItems.Count == 0)
        {
            if ((CartStates)CartState.CurrentState != CartStates.HideCart)
                CartState.Go(CartStates.HideCart);
        }
        Total = CheckOutItems.Sum(x => x.FinalCost);
        TotalPrice = Total - TotalDicount;
        DepSumm = TotalPrice;
        CheckAddButon();
        await Task.CompletedTask;
    }



    // Изменение цены продаваемого товара с учетом количества
    [RelayCommand]
    private async Task SetItemsCustomPrice(TransactionItem item)
    {
        bool isMarkup = false;
        var newprice = await ChangeNummValue(item.FinalCost);

        if (newprice == -1)
            newprice = item.FinalCost;

        if (newprice < 0)
        {
            item.Cost = 0;
            item.Markup = 0;
            item.Discount = 100;
            return;
        }

        item.Cost = (double)newprice / item.Count;
        if (newprice > ((double)item.ItemCost * item.Count))
        {

            item.Markup = Math.Abs((int)(100 - item.Cost / (item.ItemCost / 100)));
            isMarkup = true;
            item.Discount = 0;
        }

        if (!isMarkup)
        {

            item.Discount = Math.Abs((int)(100 - item.Cost / (item.ItemCost / 100)));
            item.Markup = 0;
        }
        Total = CheckOutItems.Sum(x => x.FinalCost);
        TotalPrice = Total - TotalDicount;
        DepSumm = TotalPrice;
    }

    // Изменение цены продаваемого товара
    [RelayCommand]
    private async Task SetItemCustomPrice(TransactionItem item)
    {
        bool isMarkup = false;
        var newprice = await ChangeNummValue(item.Cost);

        if (newprice == -1)
            newprice = item.ItemCost;

        if (newprice > item.ItemCost)
        {
            item.Cost = newprice;            
            item.Markup = (int)(100 - item.Cost / (item.ItemCost / 100)) * -1;
            isMarkup = true;
            item.Discount = 0;
        }

        if (!isMarkup)
        {
            if ((newprice * item.Count) < 0)
            {
                item.Cost = 0;
                item.Markup = 0;
                item.Discount = 100;
                return;
            }

            item.Cost = newprice;
            item.Discount = (int)(100 - item.Cost / (item.ItemCost / 100));
            item.Markup = 0;
        }
        Total = CheckOutItems.Sum(x => x.FinalCost);
        TotalPrice = Total - TotalDicount;
        DepSumm = TotalPrice;
    }

    // Изменение общей скидки
    [RelayCommand]
    private async Task ChangeTransactionTotalDiscount()
    {
        var newvalue = await ChangeNummValue(TotalDicount);

        if (newvalue == -1)
            newvalue = TotalDicount;

        if (newvalue <= 0)
        {
            TotalDicount = 0;
        }
        else
            TotalDicount = newvalue;
        
        TotalPrice = Total - TotalDicount;
        DepSumm = TotalPrice;
    }

    // Изменение внесенной суммы
    [RelayCommand]
    private async Task ChangeTransactionDepSumm()
    {
        var newvalue = await ChangeNummValue(DepSumm);

        if (newvalue == -1)
            newvalue = DepSumm;

        if (newvalue > TotalPrice)
        {
            SummDiffirenceColor = Color.FromHex("#479649");
            SummDiffirenceText = AppResources.ChangeLabel;//"Сдача :";
        }
        else
        {
            SummDiffirenceColor = Color.FromHex("#964747");
            SummDiffirenceText = AppResources.DutyLabel;
            //"Долг :";
        }

        if (newvalue <= 0)
        {
            DepSumm = 0;
        }
        else
            DepSumm = newvalue;

    }

    // Изменение суммы к оплате меняется скидка
    [RelayCommand]
    private async void ChangeTransactionTotalPrice()
    {
        var newvalue = await ChangeNummValue(TotalPrice);
        if (newvalue == -1)
            newvalue = TotalPrice;

        if (newvalue > Total)
        {
            TotalDicount = 0;
        }
        else if (newvalue <= 0)
        {
            TotalDicount = TotalPrice;
        }
        else
            TotalDicount = Total - newvalue;

        
        TotalPrice = Total - TotalDicount;
        DepSumm = TotalPrice;
    }

    // Создание транзакции
    [RelayCommand]
    private async Task SaveTransaction()
    {
        if (DepSumm < TotalPrice)
            if (String.IsNullOrEmpty(ContactPhone) || String.IsNullOrEmpty(ContactName))
            {
                await DialogService.ShowToast(AppResources.AddCreditError);
                //"Вы не можете совершить долговую транзакцию, пока не добавите контакт.");
                return;
            }

        IsBusy = true;

        var transaction = new Models.Transaction();
        transaction.ParentTransactionId = string.Empty;
        transaction.Description = Description;
        if (string.IsNullOrEmpty(transaction.ContactName) || string.IsNullOrEmpty(transaction.ContactPhone))
        {
            transaction.ContactName = "";
            transaction.ContactPhone = "";
        }
        transaction.ContactName = ContactName;
        transaction.ContactPhone = ContactPhone;        
        transaction.OwnerId = CurrentShopID;
        transaction.ShopName = shop.Name;
        transaction.AutorId = Profile.Id;
        transaction.AutorName = Profile.FullName;
        transaction.OperationCode = Operation.Admission;
        transaction.Price = TotalPrice;
        transaction.Discount = (int)TotalDicount;
        foreach (var tr in CheckOutItems)
        {
            transaction.TransactionItems.Add(tr);
        }
        if (DepSumm <= 0)
        {
            transaction.StateCode = Models.TransactionState.Credit;
        }
        else if (DepSumm < TotalPrice)
        {
            transaction.Payments.Add(new Payment { Summ = DepSumm });
            transaction.StateCode = Models.TransactionState.PartiallyPaid;
        }
        else
        {
            transaction.Payments.Add(new Payment { Summ = DepSumm });
            transaction.StateCode = Models.TransactionState.Paid;
        }

        await realm.WriteAsync(() =>
        {
            realm.Add(transaction);
        });
        await ClearCart();
        await Task.Delay(250);
        IsBusy = false;
    }

    // Очистить корзину
    [RelayCommand]
    public async Task ClearCart()
    {
        TotalPrice = 0;
        TotalDicount = 0;
        Description = string.Empty;
        ClearContact();
        CheckOutItems.Clear();
        if (CheckOutItems.Count == 0)
        {
            if ((CartStates)CartState.CurrentState != CartStates.HideCart)
                CartState.Go(CartStates.HideCart);
        }
        CheckAddButon();
        await Task.CompletedTask;
    }

    // Открывает полную корзину
    [RelayCommand]
    private void ShowCart()
    {
        if ((CartStates)CartState.CurrentState != CartStates.ShowCart)
        {
            CartState.Go(CartStates.ShowCart);
            CheckAddButon();
        }
            
    }

    //
    private async Task GetItems()
    {
        SortNew = Preferences.Get($"{nameof(SortNew)}arrival{CurrentShopID}", true);
        SortLowPrice = Preferences.Get($"{nameof(SortLowPrice)}arrival{CurrentShopID}", false);
        SortHiPrice = Preferences.Get($"{nameof(SortHiPrice)}arrival{CurrentShopID}", false);

        FilterAll = Preferences.Get($"{nameof(FilterAll)}arrival{CurrentShopID}", false);
        FilterActual = Preferences.Get($"{nameof(FilterActual)}arrival{CurrentShopID}", true);
        FilterArchive = Preferences.Get($"{nameof(FilterArchive)}arrival{CurrentShopID}", false);
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

                    _filter = $"is_arhive == false AND (name CONTAINS[c] '{Searchtext}' OR description CONTAINS[c] '{Searchtext}')";
                    break;
                case ItemFilter.Archive:

                    _filter = $"is_arhive == true AND (name CONTAINS[c] '{Searchtext}' OR description CONTAINS[c] '{Searchtext}')";
                    break;
                case ItemFilter.All:
                    _filter = $"is_arhive != nil AND (name CONTAINS[c] '{Searchtext}' OR description CONTAINS[c] '{Searchtext}')";

                    break;
            }
        }

        Items = realm.All<Item>().Filter($"{_filter} {_sort}");

        await Task.CompletedTask;
    }

    // Изменение текста поиска
    partial void OnSearchtextChanged(string value)
    {
        _= GetItems();
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
                break;
            case ItemSort.Max:
                SortNew = false;
                SortHiPrice = true;
                SortLowPrice = false;
                break;
            case ItemSort.New:
                SortNew = true;
                SortHiPrice = false;
                SortLowPrice = false;
                break;
        }           

        Preferences.Set($"{nameof(SortNew)}arrival{CurrentShopID}", SortNew);
        Preferences.Set($"{nameof(SortLowPrice)}arrival{CurrentShopID}", SortLowPrice);
        Preferences.Set($"{nameof(SortHiPrice)}arrival{CurrentShopID}", SortHiPrice);
        await GetItems();
    }

    // Изменение параметров фильтрации
    [RelayCommand]
    private async Task FilterChanged(ItemFilter filter)
    {
        switch (filter)
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

        Preferences.Set($"{nameof(FilterAll)}arrival{CurrentShopID}", FilterAll);
        Preferences.Set($"{nameof(FilterActual)}arrival{CurrentShopID}", FilterActual);
        Preferences.Set($"{nameof(FilterArchive)}arrival{CurrentShopID}", FilterArchive);
        await GetItems();
    }

    // Вызов профиля
 

    // Выбор контакта для добавление к транзакции
    [RelayCommand]
    private async Task BrowseContact()
    {
        try
        {
            PermissionStatus status = await Permissions.RequestAsync<Permissions.ContactsRead>();
            if (status != PermissionStatus.Granted)
            {
                await DialogService.ShowToast(AppResources.AccesContactError);
                //"доступ к контактам запрещен");
                return;
            }

            var contact = await Contacts.PickContactAsync();
            if (contact == null)
                return;
            ContactName = contact.DisplayName;
            ContactPhone = contact.Phones.FirstOrDefault()?.PhoneNumber;
        }
        catch (Exception)
        {
            ContactName = string.Empty;
            ContactPhone = string.Empty;
        }
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
        CheckAddButon();
        await Task.CompletedTask;
    }

    // Очищает добавленный контакт
    [RelayCommand]
    private void ClearContact()
    {
        ContactName = string.Empty;
        ContactPhone = string.Empty;
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


    internal async Task CheckItemChangeAsync(TransactionItem item, string oldTextValue)
    {

        //return;
        if (item == null)
        {
            return;
        }


        if (item.Count <= 0)
        {
            CheckOutItems.Remove(item);
        }
        else
        {



            Total = CheckOutItems.Sum(x => x.FinalCost);
            TotalPrice = Total - TotalDicount;
            DepSumm = TotalPrice;
        }
        if ((CartStates)CartState.CurrentState == CartStates.HideCart)
            CartState.Go(CartStates.ShowCartThreshold);
        CheckAddButon();
        if (CheckOutItems.Count <= 0)
        {
            CartState.Go(CartStates.HideCart);
        }


    }
    internal async Task AddItemInBasketByBarcode(string barcode)
    {
        try
        {

            Device.BeginInvokeOnMainThread(async () =>
            {
                var item = realm.All<Item>().FirstOrDefault(x => x.BarCode == barcode);
                if (item != null)
                    AddItemToCart(item);
                else 
                     DialogService.ShowToast($"{AppResources.NoBarcodeInListError}");
                

            });


        }
        catch (Exception ex)
        {

        }


    }

}
