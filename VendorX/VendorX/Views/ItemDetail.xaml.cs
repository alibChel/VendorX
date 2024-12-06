using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging;
using Rg.Plugins.Popup.Services;
using VendorX.Resources;
using VendorX.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using ZXing.QrCode.Internal;
using static ZXing.Mobile.MobileBarcodeScanningOptions;

namespace VendorX.Views
{	
	public partial class ItemDetail : ContentPage
	{
		ItemDetailViewModel vm;
        private static readonly ZXing.Mobile.MobileBarcodeScanningOptions opts = new ZXing.Mobile.MobileBarcodeScanningOptions
        {
            CameraResolutionSelector = new CameraResolutionSelectorDelegate(ZxingResolution.SelectLowestResolutionMatchingDisplayAspectRatio)
        };
        public ItemDetail()
		{
			InitializeComponent ();
			vm = (ItemDetailViewModel)BindingContext;

            vm.GridScanner = gridScanner;
            vm.BarcodeScanner = barcodeScanner;
            vm.CostEntry = costEntry;
            vm.PriceEntry = priceEntry;
            vm.NameEntry = nameEntry;
          //  barcodeScanner.Options = opts;
            gridScanner?.Children.Remove(barcodeScanner);
        }

        
        protected override bool OnBackButtonPressed()
        {
			if (PopupNavigation.PopupStack.Count() > 0)
				return true;
			else
				return false;

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
                Device.BeginInvokeOnMainThread(async () =>
                {
                    vm.Barcode = barcode;
                });
                await vm.HideShowScan();
            }

        }

        // Изменение размера 
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);


            

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

        private async void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            int count = 0;
            if (string.IsNullOrWhiteSpace(e.OldTextValue))
                return;
            try
            {
                count = Int32.Parse(e.NewTextValue);
            }
            catch (Exception ex)
            {
                if (string.IsNullOrWhiteSpace(e.NewTextValue))
                   vm.InCart = 0;
                else
                    vm.InCart = vm.InStockCount;
                return;
            }
            if(count==vm.InStockCount)
            {
                return;
            }
            await Task.Delay(50);
            if (vm.InCart < 0)
            {
                vm.InCart = Math.Abs(vm.InCart);
            }

            if (vm.isStrict && count >= vm.InStockCount && !vm.IsAvrialPage)
            {
                vm.InCart = vm.InStockCount;
            }
            await Task.Delay(50);
            if (count > 0)
                WeakReferenceMessenger.Default.Send(new ItemInCartChangeMessage(vm.InCart));
            else
                vm.InCart= vm.InStockCount;

        }

        private async void Entry_Unfocused(object sender, FocusEventArgs e)
        {
            if (vm.InCart < 0)
            {
                vm.InCart = Math.Abs(vm.InCart);
            }

            if (vm.isStrict && vm.InCart >= vm.InStockCount && !vm.IsAvrialPage)
            {
                vm.InCart = vm.InStockCount;
                MainThread.BeginInvokeOnMainThread( async() =>
                {
                    await DialogService.ShowToast(AppResources.NotInStockLabel);
                });
                
            }
            await Task.Delay(100);
            Console.WriteLine(vm.InCart);
            WeakReferenceMessenger.Default.Send(new ItemInCartChangeMessage(vm.InCart));
        }
    }

   
}

