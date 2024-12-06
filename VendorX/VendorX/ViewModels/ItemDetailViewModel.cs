using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using VendorX.Resources;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

using VendorX.Resources;

using ZXing.Net.Mobile.Forms;
using ZXing.QrCode.Internal;
using Microsoft.AppCenter.Crashes;

namespace Vendor.ViewModels;


public partial class ItemDetailViewModel : BaseViewModel, Xamarin.Forms.IQueryAttributable
{
    // Переданный Item
    [ObservableProperty]
    private Item initialItem;

    [ObservableProperty]
    Entry costEntry, nameEntry, priceEntry;
    /// <summary>
    /// isReedonly - только чтение 
    /// isOwner - для владельца доступно удаление
    /// isNewItem - новый Item
    /// isArchive - Item в архиве
    /// isDopCommandVisible - видимость доп команд удалить и архив
    /// isDopExpandet - для отслеживания состояния доп команд
    /// isMarkToDelete - помечен на удаление
    /// </summary>
    [ObservableProperty]
    private bool isReedonly, isNewItem, isArchive, isDopCommandVisible, isDopExpandet, isMarkToDelete, isAvrialPage, isScannerVisible;

    private bool isOwner;

    public bool IsOwner
    {
        get { return isOwner; }
        set
        {
            isOwner = value;
            OnPropertyChanged(nameof(IsOwner));
            PriceEntry.SetAppThemeColor(Entry.TextColorProperty, (Color)Application.Current.Resources["TextColorLight"], (Color)Application.Current.Resources["TextColorDark"]);
            CostEntry.SetAppThemeColor(Entry.TextColorProperty, (Color)Application.Current.Resources["TextColorLight"], (Color)Application.Current.Resources["TextColorDark"]);
            NameEntry.SetAppThemeColor(Entry.TextColorProperty, (Color)Application.Current.Resources["TextColorLight"], (Color)Application.Current.Resources["TextColorDark"]);

        }
    }

    /// <summary>
    /// обязательные параметры *
    /// name - Наименование *
    /// description - описание
    /// barcode -штрихкод
    /// walletTag - значек валюты
    /// </summary>
    [ObservableProperty]
    private string name, description, barcode, walletTag = "";

    /// <summary>
    /// обязательные параметры *
    /// price - цена *
    /// cost - стоимость
    /// inCart - кол-во в корзине
    /// </summary>
    [ObservableProperty]
    private double price, cost, inCart;


    /// <summary>
    /// добавление количества на складе
    /// </summary>
    [ObservableProperty]
    private int inStockCount;

    [ObservableProperty]
    Grid gridScanner;

    [ObservableProperty]
    ZXingScannerView barcodeScanner;

    // состояние окна сканирования
    public AnimationStateMachine CameraScanbarState { get; set; }



    public bool isStrict;
    // Фотки
    public ObservableCollection<string> Photos { get; set; } = new();

    // Теги
    public ObservableCollection<Tags> Tags { get; set; } = new();



    Realm realm;

    public ItemDetailViewModel()
    {
        realm = RealmService.GetMainThreadRealm();
        WalletTag = realm.Find<Shop>(CurrentShopID).WalletTag;
        isStrict = realm.Find<Shop>(CurrentShopID).IsStrict;
    }



    // Применение переданных параметров
    public void ApplyQueryAttributes(IDictionary<string, string> query)
    {
        if (query.Count > 0 && query.ContainsKey("item") && query["item"] != null)
        {
            InitialItem = realm.Find<Item>(query["item"]);
            Title = $"{InitialItem.Name}";
            Name = InitialItem.Name;
            Description = InitialItem.Description;
            Barcode = InitialItem.BarCode;
            Photos = new(InitialItem.PhotoUrls);
            Price = InitialItem.Price;
            Cost = InitialItem.Cost;
            IsArchive = InitialItem.IsArchive;
            InStockCount = InitialItem.OnStock;
            foreach (var tag in InitialItem.Tags)
            {
                Tags.Add(new Tags { NameTag = tag });
            }
            // Tags = new(InitialItem.Tags);
            OnPropertyChanged(nameof(Photos));
            OnPropertyChanged(nameof(Tags));

            if (query["count"] != null)
            {
                InCart = double.Parse(query["count"]);
            }
            else
                InCart = 0;
            try
            {
                if (query["avrial"] != null)
                    IsAvrialPage = bool.Parse(query["avrial"]);
            }
            catch (Exception ex)
            {

            }
            try
            {
                var targetEmail = realm.All<Member>().Where(x => x.Id == UserId || x.Id == RealmService.CurrentUser.Id).FirstOrDefault()?.UserEmail;
                var mymems = realm.All<Member>().Where(x => x.UserEmail == targetEmail).ToList();
                var employer = mymems.First(x => x.UserEmail == targetEmail && x.Shop.Id == CurrentShopID);
                //var m = realm.All<Member>().Where(x => x.Shop.Id == CurrentShopID).ToList();
                //var mymems = realm.All<Member>().Where(x => x.OwnerId == userid).ToList();

                //var currole = mymems.Where(x => x.Shop.Id == CurrentShopID).First().Role;
                if (employer.Role == UserRole.User)
                {
                    IsReedonly = true;
                    IsDopCommandVisible = false;

                }
                else if (employer.Role != UserRole.User)
                {
                    IsOwner = true;
                    IsDopCommandVisible = true;
                }
            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.WriteLine(ex.Message.ToString());
            }
        }
        else
        {

            Title = AppResources.CreateLabel;
            // "Создание";            

            if (query["avrial"] != null)
                IsAvrialPage = bool.Parse(query["avrial"]);

            IsNewItem = true;
            IsOwner = true;
            IsDopCommandVisible = false;
            InitialItem = new Item();
        }

    }

    // Помечает на удаление или отменяет отметку на удаление
    [RelayCommand]
    private async Task ChangeIsMarkToDelete()
    {
        //if (IsMarkToDelete)
        //{
        //    IsMarkToDelete = false;
        //    await DialogService.ShowToast($"{InitialItem.Name} отмена пометки на удаление");
        //}
        //else
        //{
        //var msg = $" {InitialItem.Name} будет удалён. Продолжить?";

        //if (await DialogService.ShowWarningAsync($"Внимание", msg, true))
        //{
        IsMarkToDelete = true;
        if (!string.IsNullOrWhiteSpace(InitialItem.Id))
            WeakReferenceMessenger.Default.Send(new ItemChangeState(InitialItem.Id));
        await SaveItem();

        //}
        //}
        IsDopExpandet = false;
    }

    // Изменяет параметр архива
    [RelayCommand]
    private async Task ChangeIsArchive()
    {
        var msg = IsArchive ? $"{AppResources.ChangeLocationLabel} {InitialItem.Name} {AppResources.FromArchiveLabel}?" : $"{AppResources.ChangeLocationLabel} {InitialItem.Name} {AppResources.InArchiveLabel}?";

        //$"Переместить {InitialItem.Name} из архива?" : $"Переместить {InitialItem.Name} в архив?"; 

        if (await DialogService.ShowWarningAsync(AppResources.AttentionLabel, msg, true))
        //($"Внимание", msg, true))
        {
            IsArchive = !IsArchive;
            if (IsArchive)

            {
                InCart = 0;
                await DialogService.ShowToast($"{InitialItem.Name} {AppResources.ChangeLocationLabel} {AppResources.InArchiveLabel}");
            }
            else
                await DialogService.ShowToast($"{InitialItem.Name} {AppResources.ChangeLocationLabel} {AppResources.FromArchiveLabel}");
            //$"{InitialItem.Name} перемещен из архива");
            if (!string.IsNullOrWhiteSpace(InitialItem.Id))
                WeakReferenceMessenger.Default.Send(new ItemChangeState(InitialItem.Id));
            await SaveItem();
        }
        IsDopExpandet = false;

    }

    // Генерирует штрихкод
    [RelayCommand]
    private async void GenerateBarcode()
    {
        if (IsReedonly)
        {
            await DialogService.ShowToast($"{InitialItem.Name} {AppResources.OnlyReaderError}");
            //$"{InitialItem.Name} доступен только для чтения");
            return;
        }
        GenerateUniqueBarcode();

    }

    // добавить тэг
    [RelayCommand]
    public void AddTag()
    {

        //if (String.IsNullOrEmpty(Tags.LastOrDefault()))
        //    return;
        Tags.Add(new Tags { NameTag = "              " });
        // Tags.Add("");
    }


    // переключает видимость доп команд
    [RelayCommand]
    private void ToggleIsDopExpandet()
    {
        IsDopExpandet = !IsDopExpandet;
    }

    // Добавляет в корзину
    [RelayCommand]
    private void AddToCart()
    {
        if (InCart < 0)
        {
            InCart = Math.Abs(InCart);
        }

        if (isStrict && InCart >= InStockCount && !IsAvrialPage)
        {

            DialogService.ShowToast(AppResources.NotInStockLabel);
            InCart = InStockCount;
            //"У вас не достаточно товара на складе");

        }
        else
        {
            InCart++;
            WeakReferenceMessenger.Default.Send(new ItemInCartChangeMessage(InCart));
        }

    }

    // Добавляет в корзину
    [RelayCommand]
    private async Task RemoveFromCart()
    {

        if (InCart < 0)
        {
            InCart = Math.Abs(InCart);
        }

        if (isStrict && InCart >= InStockCount && !IsAvrialPage)
        {
            InCart = InStockCount;
        }
        await Task.Delay(100);
        if (InCart > 0)
        {

            --InCart;

            WeakReferenceMessenger.Default.Send(new ItemInCartChangeMessage(InCart));
        }
    }

    // Удаляет фото из списка
    [RelayCommand]
    private void RemoveImage(string img)
    {
        Photos.Remove(img);
    }
    // Удаляет тег из списка
    [RelayCommand]
    private void DelTag(Tags tag)
    {
        Tags.Remove(tag);

    }

    //добавление количества товара на склад
    [RelayCommand]
    private void AddToStockCount()
    {

        InStockCount++;
    }
    //отнять количество товара на складе
    [RelayCommand]
    private void RemoveFromStockCount()
    {
        if (InStockCount <= 0)
        {
            InStockCount = 0;
            return;
        }
        InStockCount--;

    }

    // Заменяет фото
    [RelayCommand]
    private async Task ReplaceImage(string img)
    {

        var res = await PickPhoto(1);
        if (res.Count == 0)
            return;
        Photos[Photos.IndexOf(img)] = res.FirstOrDefault();
    }

    // Добавляет фото
    [RelayCommand]
    private async Task AddImage()
    {

        if (Photos.Count >= 5)
        {
            await DialogService.ShowToast(AppResources.AddMaxCountPhotoError);
            //"Добавлено максимальное количество фотографий");
            return;
        }

        var res = await PickPhoto(5 - Photos.Count);
        foreach (var p in res)
        {
            Photos.Add(p);
        }
    }

    // Сохранить изменения
    [RelayCommand]
    public async Task SaveItem()
    {
        if (string.IsNullOrWhiteSpace(Name))

        {
            await DialogService.ShowToast(AppResources.NameError);
            //"Наименование не может быть пустым");
            return;
        }

        if (Price <= 0)
        {
            await DialogService.ShowToast(AppResources.PriceError);
            //"Цена не может быть меньше или равна нулю");
            return;
        }
        if (Price < Cost)
        {
            await DialogService.ShowToast(AppResources.CostError);
            //"Себестоимость не может быть больше цены");
            return;
        }

        if (InCart < 0)
        {
            InCart = Math.Abs(InCart);
        }

        //if (isStrict && (InCart < InStockCount) && !IsAvrialPage)
        //{
        //    DialogService.ShowToast("У вас не достаточно товара на складе");
        //    return;
        //}

        var realm = RealmService.GetMainThreadRealm();
        if (IsMarkToDelete)
        {
            var msg = $"{InitialItem.Name} {AppResources.ItemDeleteAttention}";
            //$" {InitialItem.Name} будет удален, это действие необратимо. Продолжить?";

            if (await DialogService.ShowWarningAsync(AppResources.AttentionLabel, msg, true))
            //$"Внимание", msg, true))
            {


                await realm.WriteAsync(() =>
                {
                    realm.Remove(InitialItem);
                });
                //await DialogService.ShowToast($"{InitialItem.Name} удалён");
            }

        }
        else
        {
            var resultphoto = new List<string>();
            try
            {
                foreach (var img in Photos)
                {
                    if (!img.Contains("lis.4dev.kz") && !img.Contains("http", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (System.IO.File.Exists(img))
                        {
                            var imgurl = await RestService.PostImage(img);
                            resultphoto.Add(imgurl);
                        }
                    }
                    else
                    {
                        resultphoto.Add(img);
                    }
                }

            }
            catch (Exception)
            {

            }

            var resulttag = new List<string>();
            foreach (var tag in Tags)
            {
                if (!string.IsNullOrWhiteSpace(tag.NameTag))
                    resulttag.Add(tag.NameTag);
            }
            if (InStockCount < 0)
                InStockCount = 0;
            var i = 0;


            await realm.WriteAsync(() =>
            {
                if (!IsNewItem)
                {
                    InitialItem.Name = Name;
                    InitialItem.Description = Description;
                    InitialItem.BarCode = Barcode;
                    InitialItem.Price = Price;
                    InitialItem.Cost = Cost;
                    InitialItem.IsArchive = IsArchive;
                    InitialItem.OnStock = InStockCount;
                    InitialItem.PhotoUrls.Clear();
                    foreach (var photo in resultphoto)
                    {
                        InitialItem.PhotoUrls.Add(photo);
                    }
                    foreach (var tag in resulttag)
                    {
                        InitialItem.Tags.Add(tag);
                    }
                    InitialItem.PhotoUrl = resultphoto.FirstOrDefault();
                }
                else
                {
                    var item = new Item()
                    {
                        OwnerId = CurrentShopID,
                        Name = Name,
                        Description = Description,
                        BarCode = Barcode,
                        Price = Price,
                        OnStock = InStockCount,
                        Cost = Cost
                    };

                    foreach (var photo in resultphoto)
                    {
                        item.PhotoUrls.Add(photo);
                    }
                    item.PhotoUrl = resultphoto.FirstOrDefault();
                    foreach (var tag in resulttag)
                    {
                        item.Tags.Add(tag);
                    }

                    realm.Add(item);
                }
            });
        }
        //WeakReferenceMessenger.Default.Send(new ItemViewUpdateMessage(InitialItem));
        await Shell.Current.GoToAsync("..");
    }

    // Отмена, возврат без сохранения
    [RelayCommand]
    public async Task Cancel()
    {
        await Shell.Current.GoToAsync("..");
    }

    //Рекурсивная функция для генерации баркода
    private void GenerateUniqueBarcode()
    {
        var random = new Random();
        var chars = "0123456789";
        var code = new string(Enumerable.Repeat(chars, 12).Select(s => s[random.Next(s.Length)]).ToArray());
        if (!realm.All<Item>().Any(x => x.BarCode == code))
            Barcode = code;
        else
            GenerateUniqueBarcode();
    }

    // Показать скрыть скан
    [RelayCommand]
    internal async Task HideShowScan()
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
            Device.BeginInvokeOnMainThread(async () =>
            {
                GridScanner.Children.Add(BarcodeScanner);
                CameraScanbarState.Go(CameraScanStates.Show);
            });

        }


        else
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                GridScanner.Children.Remove(BarcodeScanner);
                CameraScanbarState.Go(CameraScanStates.Hide);
            });


        }


        await Task.CompletedTask;
    }




}

