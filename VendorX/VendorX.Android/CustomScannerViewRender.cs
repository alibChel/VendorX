using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScannerView = ZXing.Mobile.ZXingSurfaceView; 
using VendorX.Droid;
using Vendor.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using ZXing.Net.Mobile.Forms;
using static Android.Net.Wifi.WifiManager;
using ZXing.Mobile.CameraAccess;
using ZXing.Mobile;

[assembly: ExportRenderer(typeof(ScannerView), typeof(CustomScannerViewRender))]
namespace VendorX.Droid
{
 
    public class CustomScannerViewRender: ScannerView
    {
        private Action<ZXing.Result> _scanResultCallback;
        private CameraAnalyzer _cameraAnalyzer;


        public CustomScannerViewRender(Context context, MobileBarcodeScanningOptions options) : base(context, options)
        {
        }
       
        public void StartScanning(Action<ZXing.Result> scanResultCallback, MobileBarcodeScanningOptions options = null)
        {
            _scanResultCallback = scanResultCallback;
             ScanningOptions = options ?? MobileBarcodeScanningOptions.Default;
            _cameraAnalyzer.BarcodeFound += _cameraAnalyzer_BarcodeFound;
            _cameraAnalyzer.SetupCamera();
            _cameraAnalyzer.ResumeAnalysis();
        }

        private void _cameraAnalyzer_BarcodeFound(object sender, ZXing.Result e)
        {
            _scanResultCallback?.Invoke(e);
        }

      
        public void StopScanning()
        {
            _cameraAnalyzer.BarcodeFound -= _cameraAnalyzer_BarcodeFound;
            _cameraAnalyzer.ShutdownCamera();
        }
    }
}