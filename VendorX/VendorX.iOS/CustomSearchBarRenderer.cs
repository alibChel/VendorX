using System;
using UIKit;
using VendorX.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(SearchBar), typeof(CustomSearchBarRenderer))]
namespace VendorX.iOS
{
	public class CustomSearchBarRenderer : SearchBarRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
        {
            base.OnElementChanged(e);

            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0) && Control != null)
            {
                Control.SearchTextField.BackgroundColor = UIColor.FromRGBA(255, 255, 255, 0);
            }
        }
    }
}

