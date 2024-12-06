using System;
using UIKit;
using VendorX.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Entry), typeof(CustomEntryRenderer))]
namespace VendorX.iOS
{
    public class CustomEntryRenderer: EntryRenderer
	{
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                Control.BorderStyle = UITextBorderStyle.None;
                Control.Layer.CornerRadius = 5;
                Control.Layer.MasksToBounds = true;

            }
        }
    }
}

