using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using Realms;
using Rg.Plugins.Popup.Services;
using Vendor.Models;
using VendorX.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using VendorX.Services;
using VendorX.Resources;

using ZXing.Net.Mobile.Forms;
using System.Text;

namespace Vendor.ViewModels;

public partial class MainPageViewModel : BaseViewModel
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
    // Аниматор сканерКамеры
    public AnimationStateMachine CameraScanbarState { get; set; }


    // высота карточки товара
    [ObservableProperty]
    private int itemCellHeight = 100, itemCellWidht = 100;

    /// <summary>
    /// searchtext - Текст поиска
    /// walletTag - значек валюты
    /// Description - описание транзакции
    /// </summary>
    [ObservableProperty]
    private string searchtext, walletTag = "₸", description, totalDicsountOrMarkup = AppResources.OverDiscountLabel;




    /// <summary>
    /// ContactName - Имя добавленного контакта
    /// ContactPhone - Телефон добавленного контакта
    /// IsContactVisible - Отображение добавленного контакта 
    /// </summary>
    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsContactVisible))]
    private string contactName, contactPhone;
    public bool IsContactVisible { get => (!string.IsNullOrWhiteSpace(ContactName) || !string.IsNullOrWhiteSpace(ContactPhone)); }

    // Профиль
    [ObservableProperty]
    Profile profile;

    // Текущий магазин
    [ObservableProperty]
    private Shop shop;

    // Текущая роль
    private UserRole role;

    // экземпляр реалма
    private Realm realm;
    // id текущего пользователя mongo 

    // Видимость кнопки создания товара
    [ObservableProperty]
    private bool isAddButoonVisible;



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

    [ObservableProperty]
    bool isScannerVisible;

    [ObservableProperty]
    Grid gridScanner;

    [ObservableProperty]
    ZXingScannerView barcodeScanner;



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
    private bool sortNew, sortLowPrice, sortHiPrice, filterAll, filterActual, filterArchive;

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

    // для отслеживания первой загрузки
    private bool firstload;

    public MainPageViewModel()
    {
        //очищения одного айтема если его удалили, добавили в архив
        WeakReferenceMessenger.Default.Register<ItemChangeState>(this, async (sender, message) =>
        {
            try
            {
                CheckOutItems.Remove(CheckOutItems.Where(x => x.ItemId?.Id == message.Value).FirstOrDefault());

                if (CheckOutItems.Count == 0 && CartState != null)
                {
                    _ = ClearCart();
                    if ((CartStates)CartState.CurrentState != CartStates.HideCart)
                        CartState.Go(CartStates.HideCart);
                }
                if (realm == null || realm.IsClosed)
                    realm = RealmService.GetRealm();
                await GetItems();
            }
            catch (Exception ex)
            {

            }


        });


        // Событие Смены магазина если корзина и т.п. заполнена ножно очищать
        WeakReferenceMessenger.Default.Register<CurrentShopChengetMessage>(this, async (sender, message) =>
        {
            await ClearCart();
            if (realm.IsClosed || realm == null)
            {
                await RealmService.Init();
                realm = RealmService.GetMainThreadRealm();
            }
            Shop = realm.Find<Shop>(CurrentShopID);
            role = realm.All<Member>().FirstOrDefault(x => x.Shop == Shop).Role;
            WalletTag = Shop.WalletTag;
            //stock = realm.Find<Stock>(CurrentShopID);

            //MessagingCenter.Send((App)Application.Current, "ShowNavBar", role is UserRole.Owner || role is UserRole.Manager ? true : false);

            WeakReferenceMessenger.Default.Send(new ShowNavBarMessage(role is UserRole.Owner || role is UserRole.Manager ? true : false));
            CheckAddButon();

            await GetItems();
        });
    }

    internal async Task OnAppearing()
    {

        if (!firstload)
        {



            //await Task.Delay(50);
            IsBusy = true;
            realm = RealmService.GetMainThreadRealm();
            await RealmService.SetSubscription(realm, SubscriptionType.Mine);

            await Task.Delay(200);
            var mems = realm.All<Member>().ToList();
            var curmem = mems.First(x => x.Shop.Id == CurrentShopID);
            if (curmem == null)
            {
                CurrentShopID = mems.First().Shop.Id;
                await RealmService.SetSubscription(realm, SubscriptionType.Mine);
                await Task.Delay(200);
            }
            //string h = Preferences.Get("DeviceToken", "");


            Profile = realm.All<Profile>().FirstOrDefault();


            all_items = realm.All<Item>();
            await GetItems();
        }

        firstload = true;

        WeakReferenceMessenger.Default.Unregister<ItemInCartChangeMessage>(this);
        if (CheckOutItems.Count == 0 && CartState != null)
        {
            _ = ClearCart();
            if ((CartStates)CartState.CurrentState != CartStates.HideCart)
                CartState.Go(CartStates.HideCart);
        }
        if (realm == null || realm.IsClosed)
            realm = RealmService.GetRealm();

        //Подписка на получение события сканерование ШК
        WeakReferenceMessenger.Default.Register<ItemBarcodeScanMessage>(this, async (sender, message) =>
    {

        var item = all_items.FirstOrDefault(x => x.BarCode == message.Value);
        if (item != null)
            await AddItemToCart(item);
        else
            await DialogService.ShowToast($"{AppResources.NoBarcodeInListError} " + message.Value);
        //$"в списке нет товара с штрихкодом {message.Value}");
    });
        Shop = realm.Find<Shop>(CurrentShopID);
        WalletTag = Shop?.WalletTag;
        role = realm.All<Member>().FirstOrDefault(x => x.Shop == Shop).Role;
        WeakReferenceMessenger.Default.Send(new ShowNavBarMessage(role is UserRole.Owner || role is UserRole.Manager ? true : false));
        CheckAddButon();

        IsBusy = false;

        OnPropertyChanged(nameof(Profile));

        ChekNotification();
        IsBusy = false;

        await Task.CompletedTask;
    }

    internal async Task OnDisappearing()
    {
        WeakReferenceMessenger.Default.Unregister<ItemBarcodeScanMessage>(this);
        if (IsScannerVisible)
            await HideShowScan();
        await Task.CompletedTask;
    }

    // Расчет размеров
    internal void OnSizeAllocated(double width, double height)
    {
        ItemCellHeight = (int)((width / 3 - 10) * 1.35);
        ItemCellWidht = (int)((width / 3) - 5);
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
        try
        {


            WeakReferenceMessenger.Default.Register<ItemInCartChangeMessage>(this, async (sender, message) =>
            {


                var _item = CheckOutItems.FirstOrDefault(x => ReferenceEquals(x.ItemId, item));

                if (message.Value <= 0 && _item != null)
                {
                    Console.WriteLine("if" + message.Value);
                    _item.Count = 1;
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
                        if (item.Price != _item.ItemPrice)
                        {
                            _item.ItemPrice = item.Price;
                            if (_item.Price > _item.ItemPrice)
                            {
                                _item.Markup = Math.Abs((int)(100 - _item.Price / (_item.ItemPrice / 100)));
                                _item.Discount = 0;
                            }
                            if (_item.Price < _item.ItemPrice)
                            {
                                _item.Discount = Math.Abs((int)(100 - _item.Price / (_item.ItemPrice / 100)));
                                _item.Markup = 0;
                            }
                            if (_item.Price == _item.ItemPrice)
                            {
                                _item.Markup = 0;
                                _item.Discount = 0;
                            }

                        }
                        _item.Count = (int)message.Value;
                        Console.WriteLine(message.Value);
                    }
                }

                Total = CheckOutItems.Sum(x => x.FinalPrice);
                MinusMarkupOrDiscount();

                DepSumm = TotalPrice;
                if ((CartStates)CartState.CurrentState == CartStates.HideCart)
                    CartState.Go(CartStates.ShowCartThreshold);
                CheckAddButon();
            });

            var incart = CheckOutItems.FirstOrDefault(x => x.ItemId?.Id == item.Id)?.Count ?? 0;
            //var itemParameter = new Dictionary<string, string>() { { "item", item.Id }, { "count", incart.ToString()} };
            await Shell.Current.GoToAsync($"{nameof(ItemDetail)}?item={item.Id}&count={incart}");
        }
        catch (Exception ex)
        {

        }
    }

    // Создание товара
    [RelayCommand]
    public async Task CreateNewItem()
    {
        await Shell.Current.GoToAsync($"{nameof(ItemDetail)}?count={0}&avrial=false");
        //await Shell.Current.GoToAsync(nameof(ItemDetail));
    }

    // Добавление товара в корзину а
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

        if (Shop.IsStrict && await CheckItemInStock(item))
        {
            await DialogService.ShowToast(item.Name + $" {AppResources.NoCountError}");
            //$"{item.Name} нет в достаточном количестве.");
            return;
        }

        var _item = CheckOutItems.FirstOrDefault(x => x.ItemId?.Id == item.Id);
        if (_item == null)
        {
            _item = new TransactionItem
            {
                ItemId = item,
                ItemName = item.Name,
                ItemPrice = item.Price,
                PhotoUrl = item.PhotoUrl,
                Count = 1,
                Price = item.Price,
                Cost = item.Cost
            };
            CheckOutItems.Add(_item);
        }
        else
        {
            _item.ItemPrice = item.Price;

            if (_item.Price != _item.ItemPrice)
            {


                if (_item.Price > _item.ItemPrice)
                {
                    _item.Markup = Math.Abs((int)(100 - _item.Price / (_item.ItemPrice / 100)));
                    _item.Discount = 0;
                    OnPropertyChanged(nameof(_item.Discount));
                }
                if (_item.Price < _item.ItemPrice)
                {
                    _item.Discount = Math.Abs((int)(100 - _item.Price / (_item.ItemPrice / 100)));
                    _item.Markup = 0;
                    OnPropertyChanged(nameof(_item.Markup));
                }
                if (_item.Price == _item.ItemPrice)
                {
                    _item.Markup = 0;
                    _item.Discount = 0;
                    OnPropertyChanged(nameof(_item.Discount));
                    OnPropertyChanged(nameof(_item.Markup));
                }
            }
            Device.BeginInvokeOnMainThread(() =>
            {
                _item.Count++;
                Total = CheckOutItems.Sum(x => x.FinalPrice);
                MinusMarkupOrDiscount();

                DepSumm = TotalPrice;
            });

        }

        Total = CheckOutItems.Sum(x => x.FinalPrice);
        MinusMarkupOrDiscount();

        DepSumm = TotalPrice;

        if ((CartStates)CartState.CurrentState == CartStates.HideCart)
            CartState.Go(CartStates.ShowCartThreshold);
        CheckAddButon();
        if (CheckOutItems.Count <= 0)
        {
            CartState.Go(CartStates.HideCart);
        }
    }

    // Добавление товара в корзину 
    [RelayCommand]
    private async Task AddTransactionItemToCart(TransactionItem _item)
    {
        if (_item.ItemId == null) return;

        var item = all_items.FirstOrDefault(x => x.Id == _item.ItemId.Id);
        await AddItemToCart(item);
    }

    // Удаление товара из корзины
    [RelayCommand]
    private async Task RemoveItemFromCart(TransactionItem transactionItem)
    {
        Console.WriteLine($"remove {transactionItem.Count}");
        if (transactionItem.Count <= 1)
            CheckOutItems.Remove(transactionItem);

        else
        {
            if (transactionItem.ItemId == null) return;

            var item = all_items.FirstOrDefault(x => x.Id == transactionItem.ItemId.Id);
            if (item.Price != transactionItem.ItemPrice)
            {
                transactionItem.ItemPrice = item.Price;
                if (transactionItem.Price > transactionItem.ItemPrice)
                {
                    transactionItem.Markup = Math.Abs((int)(100 - transactionItem.Price / (transactionItem.ItemPrice / 100)));
                    transactionItem.Discount = 0;
                }
                if (transactionItem.Price < transactionItem.ItemPrice)
                {
                    transactionItem.Discount = Math.Abs((int)(100 - transactionItem.Price / (transactionItem.ItemPrice / 100)));
                    transactionItem.Markup = 0;
                }
                if (transactionItem.Price == transactionItem.ItemPrice)
                {
                    transactionItem.Markup = 0;
                    transactionItem.Discount = 0;
                }

            }
            transactionItem.Count--;
        }


        if (CheckOutItems.Count == 0)
        {
            _ = ClearCart();
            if ((CartStates)CartState.CurrentState != CartStates.HideCart)
                CartState.Go(CartStates.HideCart);
        }
        Total = CheckOutItems.Sum(x => x.FinalPrice);
        MinusMarkupOrDiscount();

        DepSumm = TotalPrice;
        CheckAddButon();
        await Task.CompletedTask;
    }

    // Проверка доступности на складе
    private async Task<bool> CheckItemInStock(Item item)
    {
        var addcount = CheckOutItems.FirstOrDefault(x => x.ItemId?.Id == item.Id)?.Count ?? 0;
        return await Task.FromResult<bool>(item.OnStock <= addcount);
    }
    // Проверка доступности на складе перегрузка
    private async Task<bool> CheckItemInStock(TransactionItem item)
    {
        if (item.ItemId == null) return false;

        var _item = all_items.FirstOrDefault(x => x.Id == item.ItemId.Id);
        return await CheckItemInStock(_item);
    }

    // Изменение цены продаваемого товара с учетом количества
    [RelayCommand]
    private async Task SetItemsCustomPrice(TransactionItem item)
    {
        bool isMarkup = false;
        var newprice = await ChangeNummValue(item.FinalPrice);
        if (newprice == -1)
            return;
        if (newprice < 0)
        {
            item.Price = 0;
            item.Markup = 0;
            item.Discount = 100;
            return;
        }

        item.Price = (double)newprice / item.Count;
        if (newprice > ((double)item.ItemPrice * item.Count))
        {
            item.Markup = (int)(100 - item.Price / (item.ItemPrice / 100)) * -1;
            isMarkup = true;
            item.Discount = 0;
        }

        if (!isMarkup)
        {
            item.Discount = (int)(100 - item.Price / (item.ItemPrice / 100));
            item.Markup = 0;
        }
        Total = CheckOutItems.Sum(x => x.FinalPrice);
        MinusMarkupOrDiscount();

        DepSumm = TotalPrice;
    }

    // Изменение цены продаваемого товара
    [RelayCommand]
    private async Task SetItemCustomPrice(TransactionItem item)
    {
        bool isMarkup = false;
        var newprice = await ChangeNummValue(item.Price);
        if (newprice == -1)
            return;

        if (newprice > item.ItemPrice)
        {
            item.Price = newprice;
            item.Markup = (int)(100 - item.Price / (item.ItemPrice / 100)) * -1;
            isMarkup = true;
            item.Discount = 0;
        }

        if (!isMarkup)
        {
            if ((newprice * item.Count) < 0)
            {
                item.Price = 0;
                item.Markup = 0;
                item.Discount = 100;
                return;
            }

            item.Price = newprice;
            item.Discount = (int)(100 - item.Price / (item.ItemPrice / 100));
            item.Markup = 0;
        }
        Total = CheckOutItems.Sum(x => x.FinalPrice);
        MinusMarkupOrDiscount();

        DepSumm = TotalPrice;
    }

    // Изменение общей скидки
    [RelayCommand]
    private async Task ChangeTransactionTotalDiscount()
    {
        var newvalue = await ChangeNummValue(TotalDicount);
        if (newvalue == -1)
            return;

        if (newvalue <= 0)
        {
            TotalDicount = 0;
        }
        else
            TotalDicount = newvalue;
        MinusMarkupOrDiscount();

        DepSumm = TotalPrice;
    }

    // Изменение внесенной суммы
    [RelayCommand]
    private async Task ChangeTransactionDepSumm()
    {
        var newvalue = await ChangeNummValue(DepSumm);
        if (newvalue == -1)
            return;

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

        if (newvalue == 0)
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
            return;



        if (newvalue > Total)
        {
            TotalDicount = Total - newvalue;
        }
        else if (newvalue <= 0)
        {
            TotalDicount = TotalPrice;
        }
        else
            TotalDicount = Total - newvalue;



        TotalPrice = Total - TotalDicount;
        DepSumm = TotalPrice;

        if (TotalDicount < 0)
        {
            TotalDicount *= -1;
            TotalDicsountOrMarkup = AppResources.MarkupLabel;
        }
        else
        {
            TotalDicsountOrMarkup = AppResources.OverDiscountLabel;
        }
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
        foreach (var item in CheckOutItems)
        {
            if (item.ItemId == null) return;

            var stockcount = all_items.FirstOrDefault(x => x.Id == item.ItemId.Id).OnStock;



            if (Shop.IsStrict && stockcount < item.Count)
            {
                await DialogService.ShowToast($"{item.ItemName} нет в достаточном количестве.");
                return;
            }
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
        transaction.ShopName = Shop.Name;
        transaction.AutorId = Profile.Id;
        transaction.AutorName = Profile.FullName;
        transaction.OperationCode = Operation.Sale;
        transaction.Price = TotalPrice;
        transaction.Discount =
            TotalDicsountOrMarkup.Equals(AppResources.MarkupLabel) ? 0 : (int)TotalDicount;
        transaction.Markup =
            TotalDicsountOrMarkup.Equals(AppResources.MarkupLabel) ? (int)TotalDicount : 0;
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
    private async Task ClearCart()
    {
        TotalPrice = 0;
        TotalDicount = 0;
        Description = string.Empty;
        ClearContact();
        CheckOutItems.Clear();
        //if (CheckOutItems.Count == 0)
        //{
        //    if ((CartStates)CartState.CurrentState != CartStates.HideCart)
        //        CartState.Go(CartStates.HideCart);
        //}
        CartState.Go(CartStates.HideCart);
        CheckAddButon();
        await Task.CompletedTask;
    }

    // Открывает полную корзину
    [RelayCommand]
    private async Task ShowCart()
    {
        if ((CartStates)CartState.CurrentState != CartStates.ShowCart)
        {
            CartState.Go(CartStates.ShowCart);
            CheckAddButon();
        }
        await Task.CompletedTask;
    }
    //
    private async Task GetItems()

    {
        SortNew = Preferences.Get($"{nameof(SortNew)}{CurrentShopID}", true);
        SortLowPrice = Preferences.Get($"{nameof(SortLowPrice)}{CurrentShopID}", false);
        SortHiPrice = Preferences.Get($"{nameof(SortHiPrice)}{CurrentShopID}", false);

        FilterAll = Preferences.Get($"{nameof(FilterAll)}{CurrentShopID}", false);
        FilterActual = Preferences.Get($"{nameof(FilterActual)}{CurrentShopID}", true);
        FilterArchive = Preferences.Get($"{nameof(FilterArchive)}{CurrentShopID}", false);
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
        _ = GetItems();
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

        Preferences.Set($"{nameof(SortNew)}{CurrentShopID}", SortNew);
        Preferences.Set($"{nameof(SortLowPrice)}{CurrentShopID}", SortLowPrice);
        Preferences.Set($"{nameof(SortHiPrice)}{CurrentShopID}", SortHiPrice);
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

        Preferences.Set($"{nameof(FilterAll)}{CurrentShopID}", FilterAll);
        Preferences.Set($"{nameof(FilterActual)}{CurrentShopID}", FilterActual);
        Preferences.Set($"{nameof(FilterArchive)}{CurrentShopID}", FilterArchive);
        await GetItems();
    }



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

        OnPropertyChanged(nameof(IsContactVisible));
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
            if (item.ItemId == null) return;


            var _item = all_items.FirstOrDefault(x => x.Id == item.ItemId.Id);
            try
            {

                if (Shop.IsStrict && item.Count > _item.OnStock)
                {
                    item.Count = _item.OnStock;
                    await DialogService.ShowToast($"{item.ItemName} {AppResources.NoCountError}");
                }
            }
            catch (Exception ex)
            {
                item.Count = int.Parse(oldTextValue);

            }
            if (_item == null)
            {
                return;
            }
            else
            {


            }

            Total = CheckOutItems.Sum(x => x.FinalPrice);

            MinusMarkupOrDiscount();
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

    private void MinusMarkupOrDiscount()
    {

        if (!TotalDicsountOrMarkup.Equals(AppResources.MarkupLabel))
            TotalPrice = Total - TotalDicount;
        else
            TotalPrice = Total + TotalDicount;
    }


    internal async Task AddItemInBasketByBarcode(string barcode)
    {
        try
        {

            Device.BeginInvokeOnMainThread(async () =>
            {
                var item = realm.All<Item>().FirstOrDefault(x => x.BarCode == barcode);
                if (item != null)
                    await AddItemToCart(item);
                else
                    DialogService.ShowToast($"{AppResources.NoBarcodeInListError}");


            });


        }
        catch (Exception ex)
        {

        }


    }
}
