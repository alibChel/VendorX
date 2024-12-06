using System;
using SpriteKit;
using UIKit;
using VendorX.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Page), typeof(CustomPageRenderer))]
namespace VendorX.iOS
{
    public class CustomPageRenderer : PageRenderer
    {
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            var page = Element as Xamarin.Forms.Page;

            page.On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);

            App.SaveAreaHeight = (int)(UIApplication.SharedApplication.Delegate.GetWindow().SafeAreaInsets.Bottom + UIApplication.SharedApplication.Delegate.GetWindow().SafeAreaInsets.Top);
            App.TabHeight = 50;
        }
    }
}

