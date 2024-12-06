using Forms9Patch;
using System;
using System.Collections.Generic;
using VendorX.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Mobile;
using ZXing.Net.Mobile.Forms;
using static ZXing.Mobile.MobileBarcodeScanningOptions;

namespace VendorX.Views;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MainPage : ContentPage
{
    // Высота страницы
    private double conteinerHeigh;
    // Ширина страницы
    private double conteinerWidth;
    // Высота мини корзины 
    private double openThreshold;
    // Размер смещения каталога при открытии мини корзины
    private double catalogIndentHeight = 300;
    // ускорение для отслеживания позиций при ручном перемещении корзины
    private double lastVelocity;
    private static readonly ZXing.Mobile.MobileBarcodeScanningOptions opts = new ZXing.Mobile.MobileBarcodeScanningOptions
    {
        CameraResolutionSelector = new CameraResolutionSelectorDelegate(ZxingResolution.SelectLowestResolutionMatchingDisplayAspectRatio)
    };
    //// Анимитор корзины
    //AnimationStateMachine cartState;
    //// Анимитор корзины
    //AnimationStateMachine filterbarState;
    // 
    MainPageViewModel vm;

    public MainPage()
    {
        InitializeComponent();
    
        BindingContext = vm = new MainPageViewModel();
       vm.GridScanner = gridScanner;
        vm.BarcodeScanner = barcodeScanner;
       // barcodeScanner.Options= opts;
        gridScanner?.Children.Remove(barcodeScanner);
    }

    // Нажатие кнопки или жест назад если открыты фильтры или корзина закрывает их
    protected override bool OnBackButtonPressed()
    {

        if ((CartStates)vm.CartState.CurrentState == CartStates.ShowCart)
        {
            vm.CartState.Go(CartStates.ShowCartThreshold);
            return true;
        }

        if ((FilterStates)vm.FilterbarState.CurrentState == FilterStates.Show)
        {
            vm.FilterbarState.Go(FilterStates.Hide);
            return true;
        }

        return base.OnBackButtonPressed();
    }

    // Выполняется при обращении к странице
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await vm.OnAppearing();
        vm.BarcodeScanner = barcodeScanner;
    }




    
    // Вызывается при завершении взаимодействия с текущей страницей(при переходе на другую страницу, кроме попапов)
    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await vm.OnDisappearing();
    }

    // Изменение размера 
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);


        vm.OnSizeAllocated(width, height);
        conteinerWidth = width;
        conteinerHeigh = height;
        if (vm.CartState == null)
        {
            // Высотакнопки +80(высота миникорзины) + 20(высота декоротивного элемента) +15(отступы между 3 элементами по 5)
            openThreshold = CartCheckBtn.Height + 80 + 20 + 15;
            catalogIndentHeight = CartCheckBtn.Height + 80 + 20 + 15;
            if (Device.RuntimePlatform == Device.iOS)
            {
                openThreshold += App.SaveAreaHeight +App.TabHeight;
                //catalogIndentHeight += App.SaveAreaHeight + App.TabHeight;
            }
            SetupCartStates();
        }
        if (vm.CartState.CurrentState == null)
            vm.CartState.Go(CartStates.HideCart, false);
        if (vm.FilterbarState == null)
        {
            SetupFilterStates();
        }
        if (vm.FilterbarState.CurrentState == null)
            vm.FilterbarState.Go(FilterStates.Hide, false);

        if (vm.CameraScanbarState == null)
        {
            SetupCameraScanStates();
        }
        if (vm.CameraScanbarState.CurrentState == null)
            vm.CameraScanbarState.Go(CameraScanStates.Hide, false);

        vm.CheckAddButon();
    }

    // Настройка состояния Окна сканирования
    private void SetupCameraScanStates()
    {
        vm.CameraScanbarState = new AnimationStateMachine();
        vm.CameraScanbarState.Add(CameraScanStates.Show, new ViewTransition[]
        {
            new ViewTransition(CatalogCameraScan, AnimationType.Opacity, 1, length:250),
            new ViewTransition(CatalogCameraScan, AnimationType.TranslationY, 0, length: 250),
            

        });
        vm.CameraScanbarState.Add(CameraScanStates.Hide, new ViewTransition[]
        {
            new ViewTransition(CatalogCameraScan, AnimationType.Opacity, 0, length:250),
            new ViewTransition(CatalogCameraScan, AnimationType.TranslationY, 300, length: 250),
            

        });

    }

    // Настройка состояния меню фильтрации
    private void SetupFilterStates()
    {
        vm.FilterbarState = new AnimationStateMachine();
        vm.FilterbarState.Add(FilterStates.Show, new ViewTransition[]
        {
            new ViewTransition(FilterBG, AnimationType.Opacity, 1, length:250),
            new ViewTransition(FilterBar, AnimationType.Opacity, 1, length:250),
            new ViewTransition(FilterBar, AnimationType.TranslationY, 0, length: 250)

        });
        vm.FilterbarState.Add(FilterStates.Hide, new ViewTransition[]
        {
            new ViewTransition(FilterBG, AnimationType.Opacity, 0, length:250),
            new ViewTransition(FilterBar, AnimationType.Opacity, 0, length:250),
            new ViewTransition(FilterBar, AnimationType.TranslationY, 300, length: 250)

        });
        
    }
    // Настройка  состояний корзины
    private void SetupCartStates()
    {
        vm.CartState = new AnimationStateMachine();

        vm.CartState.Add(CartStates.ShowCart, new ViewTransition[]
        {
            new ViewTransition(Cart, AnimationType.TranslationY, 0, length:250),
            new ViewTransition(Cart, AnimationType.Opacity, 1, length:0),
            new ViewTransition(CartCheckBtn, AnimationType.Opacity, 0, length:0),
            new ViewTransition(MiniCart, AnimationType.Opacity, 0, length:0),
            new ViewTransition(DescriptionBlock, AnimationType.Opacity, 1, length:0),
            new ViewTransition(CatalogIndent, AnimationType.HeightRequest, 0, length:250),

            //new ViewTransition(PrimaryCatalog, AnimationType.Opacity, 0, length:0),
            //new ViewTransition(CartCollection, AnimationType.Opacity, 1, length:250),

        });

        vm.CartState.Add(CartStates.HideCart, new ViewTransition[]
        {
            new ViewTransition(Cart, AnimationType.TranslationY, conteinerHeigh+60 , length:250),
            new ViewTransition(Cart, AnimationType.Opacity, 0, length:0),
            new ViewTransition(CartCheckBtn, AnimationType.Opacity, 1, length:0),
            new ViewTransition(AddButton, AnimationType.TranslationY, 0, length:250),
            new ViewTransition(CatalogIndent, AnimationType.HeightRequest, 0, length:250),
            new ViewTransition(MiniCart, AnimationType.Opacity, 0, length:0),
            new ViewTransition(DescriptionBlock, AnimationType.Opacity, 0, length:0),
            //new ViewTransition(PrimaryCatalog, AnimationType.Opacity, 1, length:250),
            //new ViewTransition(CartCollection, AnimationType.Opacity, 0, length:0)

        });

        vm.CartState.Add(CartStates.ShowCartThreshold, new ViewTransition[]
        {
            new ViewTransition(Cart, AnimationType.TranslationY, conteinerHeigh - (openThreshold ) , length:250),
            new ViewTransition(Cart, AnimationType.Opacity, 1, length:0),
            new ViewTransition(CartCheckBtn, AnimationType.Opacity, 1, length:0),
            new ViewTransition(CatalogIndent, AnimationType.HeightRequest, catalogIndentHeight, length:250),
            new ViewTransition(AddButton, AnimationType.TranslationY, -(catalogIndentHeight) +100, length:250),
            new ViewTransition(MiniCart, AnimationType.Opacity, 1, length:0),
            new ViewTransition(DescriptionBlock, AnimationType.Opacity, 0, length:250),
            //new ViewTransition(PrimaryCatalog, AnimationType.Opacity, 1, length:250),
            //new ViewTransition(CartCollection, AnimationType.Opacity, 0, length:0)

        });


       

    }

    // ивент перемещения корзины
    void CartPanGestureRecognizer_PanUpdated(System.Object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                lastVelocity = e.TotalY;
                if (lastVelocity < 0)
                {
                    if (Cart.TranslationY >= 0)
                        Cart.TranslationY += e.TotalY;
                    //vm.IsAddProductVisible = false;
                }
                else if (lastVelocity > 0)
                {
                    if (Cart.TranslationY <= conteinerHeigh - 30)
                        Cart.TranslationY += e.TotalY;
                }
                break;
            case GestureStatus.Completed:

                if (lastVelocity < -10)
                    vm.CartState.Go(CartStates.ShowCart);
                else if (lastVelocity > 10)
                {
                    vm.CartState.Go(CartStates.ShowCartThreshold, false);
                }
                else
                {
                    if (lastVelocity > 0)
                    {
                        if (Cart.TranslationY < openThreshold)
                            vm.CartState.Go(CartStates.ShowCart);
                        else
                        {
                            vm.CartState.Go(CartStates.ShowCartThreshold, false);
                        }
                    }
                    else if (lastVelocity < 0)
                    {
                        if (conteinerHeigh - Cart.TranslationY < openThreshold)
                        {
                            vm.CartState.Go(CartStates.ShowCartThreshold, false);
                        }
                        else
                            vm.CartState.Go(CartStates.ShowCart);
                    }
                }
                break;
        }

        vm.CheckAddButon();
    }

    // ивент движения фильтров
    void PanGestureRecognizer_PanUpdated(System.Object sender,PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                lastVelocity = e.TotalY;
                if (lastVelocity < 0)
                {
                    if (FilterBar.TranslationY >= 0)
                        FilterBar.TranslationY += e.TotalY;

                }
                else if (lastVelocity > 0)
                {

                    FilterBar.TranslationY += e.TotalY;
                }
                break;
            case GestureStatus.Completed:

                if (lastVelocity < -10)
                    vm.FilterbarState.Go(FilterStates.Show);
                else
                {
                    vm.FilterbarState.Go(FilterStates.Hide);
                }
                break;
        }

    }

    private async void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if ( (CartStates)vm.CartState.CurrentState != CartStates.ShowCart)
            return;
       
           
        if (e.NewTextValue != null && e.NewTextValue != e.OldTextValue)
        {
          
            var tri = ((Entry)sender).BindingContext;
            if (tri != null && tri.GetType() == typeof(TransactionItem))
            {
                if (string.IsNullOrWhiteSpace(e.OldTextValue) || e.NewTextValue[0]=='0')
                    await vm.CheckItemChangeAsync((TransactionItem)tri, "0");
                else
                    await vm.CheckItemChangeAsync((TransactionItem)tri, e.OldTextValue);
            }
        }
    }

    private void Entry_Focused(object sender, FocusEventArgs e)
    {
        var entry = (Entry)sender;
        entry.RemoveBinding(Entry.TextProperty);
        entry.TextChanged -= Entry_TextChanged;
    }

    private async void Entry_Unfocused(object sender, FocusEventArgs e)
    {
        var context = ((Entry)sender).BindingContext;
        var item = (TransactionItem)context;
        var entry = (Entry)sender;
        await CheckEntrySetCount(entry, item);
        //var source = vm.CheckOutItems.FirstOrDefault(x=> x.ItemId == item.ItemId);
        //Binding binding = new Binding { Source = source, Path = "Count", Mode = BindingMode.TwoWay };
        entry.SetBinding(Entry.TextProperty, "Count", BindingMode.TwoWay);
        entry.TextChanged += Entry_TextChanged;
        OnPropertyChanged(nameof(item.Count));
    }

    private async Task CheckEntrySetCount(Entry entry, TransactionItem transactionItem)
    {
        var val = decimal.Parse(string.IsNullOrWhiteSpace(entry.Text)? "0": entry.Text);
        var res = (int)val;
        transactionItem.Count = res;
        await vm.CheckItemChangeAsync(transactionItem, "");
        
        await Task.CompletedTask;
    }

    private async void Entry_Completed(object sender, EventArgs e)
    {
        var tri = ((Entry)sender).BindingContext;
        if (tri != null && tri.GetType() == typeof(TransactionItem))
        {
            if (string.IsNullOrWhiteSpace(e.ToString()))
                await vm.CheckItemChangeAsync((TransactionItem)tri, "0");
            else
                await vm.CheckItemChangeAsync((TransactionItem)tri, e.ToString()); ;
        }
    }

    private async void ZXingScannerView_OnScanResult(ZXing.Result result)
    { 
        var barcode = result.Text;
        if (!string.IsNullOrWhiteSpace(barcode))
        {
            if (vm.VibrationScanner)
            {
                var duration = TimeSpan.FromSeconds(0.5);
                Vibration.Vibrate(duration);
            }
            if (vm.SoundScanner)
                DependencyService.Get<IBeep>().PlayAudioFile("norm.mp3");


            vm.AddItemInBasketByBarcode(barcode);
        }

    }

   
}

