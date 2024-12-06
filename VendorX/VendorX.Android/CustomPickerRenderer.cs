using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VendorX.Droid;
using Vendor.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Picker = Xamarin.Forms.Picker;

[assembly: ExportRenderer(typeof(Picker), typeof(CustomPickerRenderer))]
namespace VendorX.Droid
{
    public class CustomPickerRenderer : PickerRenderer
    {
        public CustomPickerRenderer(Context context) : base(context)
        {

        }
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null)
            {
                Control.Background = null;
                Control.Gravity = GravityFlags.CenterHorizontal;
            }
        }
    }
}