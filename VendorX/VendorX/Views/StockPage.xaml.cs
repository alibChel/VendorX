using System;
using System.Collections.Generic;
using VendorX.Resources;
using VendorX.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using static ZXing.Mobile.MobileBarcodeScanningOptions;

namespace VendorX.Views;

public partial class StockPage : ContentPage
{
    StockViewModel vm;
    // ускорение для отслеживания позиций при ручном перемещении корзины
    private double lastVelocity;
    private static readonly ZXing.Mobile.MobileBarcodeScanningOptions opts = new ZXing.Mobile.MobileBarcodeScanningOptions
    {
        CameraResolutionSelector = new CameraResolutionSelectorDelegate(ZxingResolution.SelectLowestResolutionMatchingDisplayAspectRatio)
    };
    // Анимитор корзины
    AnimationStateMachine filterbarState;

    public StockPage()
    {
        InitializeComponent();
        BindingContext = vm = new StockViewModel();
        vm.GridScanner = gridScanner;
        vm.BarcodeScanner = barcodeScanner;
       // barcodeScanner.Options = opts;
        gridScanner?.Children.Remove(barcodeScanner);
    }

    //protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    //{
    //    base.OnNavigatedTo(args);
    //    await vm.OnAppearing();
    //}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await vm.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        vm.OnDisappearing();
    }

    // Нажатие кнопки или жест назад если открыты фильтры или корзина закрывает их
    protected override bool OnBackButtonPressed()
    {

        if ((FilterStates)filterbarState.CurrentState == FilterStates.Show)
        {
            filterbarState.Go(FilterStates.Hide);
            return true;
        }

        return base.OnBackButtonPressed();
    }

    // Изменение размера 
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (filterbarState == null)
        {
            SetupFilterStates();
        }
        if (filterbarState.CurrentState == null)
            filterbarState.Go(FilterStates.Hide, false);

        if (vm.CameraScanbarState == null)
        {
            SetupCameraScanStates();
        }
        if (vm.CameraScanbarState.CurrentState == null)
            vm.CameraScanbarState.Go(CameraScanStates.Hide, false);
    }

    // Настройка состояния Окна сканирования
    private void SetupCameraScanStates()
    {
        vm.CameraScanbarState = new AnimationStateMachine();
        vm.CameraScanbarState.Add(CameraScanStates.Show, new ViewTransition[]
        {
            new ViewTransition(CatalogCameraScan, AnimationType.Opacity, 1, length:250),
            new ViewTransition(CatalogCameraScan, AnimationType.TranslationY, 0, length: 250)

        });
        vm.CameraScanbarState.Add(CameraScanStates.Hide, new ViewTransition[]
        {
            new ViewTransition(CatalogCameraScan, AnimationType.Opacity, 0, length:250),
            new ViewTransition(CatalogCameraScan, AnimationType.TranslationY, 300, length: 250)

        });

    }

    // Настройка состояния меню фильтрации
    private void SetupFilterStates()
    {
        filterbarState = new AnimationStateMachine();
        filterbarState.Add(FilterStates.Show, new ViewTransition[]
        {
            new ViewTransition(FilterBG, AnimationType.Opacity, 1, length:250),
            new ViewTransition(FilterBar, AnimationType.Opacity, 1, length:250),
            new ViewTransition(FilterBar, AnimationType.TranslationY, 0, length: 250)

        });
        filterbarState.Add(FilterStates.Hide, new ViewTransition[]
        {
            new ViewTransition(FilterBG, AnimationType.Opacity, 0, length:250),
            new ViewTransition(FilterBar, AnimationType.Opacity, 0, length:250),
            new ViewTransition(FilterBar, AnimationType.TranslationY, 300, length: 250)

        });
        vm.FilterbarState = filterbarState;
    }

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
                    filterbarState.Go(FilterStates.Show);
                else
                {
                    filterbarState.Go(FilterStates.Hide);
                }
                break;
        }
    }

    private async void ZXingScannerView_OnScanResult(ZXing.Result result)
    {
        try
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

                vm.SerchForBarCode(barcode);
            }
        }
        catch(Exception ex){ await DialogService.ShowToast($"{AppResources.ErrorLabel}"); }

    }

}

