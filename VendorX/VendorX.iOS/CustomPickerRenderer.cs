using System;
using UIKit;
using VendorX.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Picker), typeof(CustomPickerRenderer))]
namespace VendorX.iOS
{
    public class CustomPickerRenderer: PickerRenderer
    {


        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);
            //var view = e.NewElement as DatePicker;
            if(this.Control != null)
            {
                this.Control.BorderStyle = UITextBorderStyle.None;
                this.Control.TextAlignment = UITextAlignment.Center;
            }
            
        }
    }
}

