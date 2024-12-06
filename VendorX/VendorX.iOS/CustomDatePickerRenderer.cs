using System;
using UIKit;
using VendorX.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(DatePicker), typeof(CustomDatePickerRenderer))]
namespace VendorX.iOS
{
    public class CustomDatePickerRenderer: DatePickerRenderer
    {


        protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
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

