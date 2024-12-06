using Android.OS;
using AndroidX.Core.View;
using System.Threading.Tasks;
using VendorX.Helpers;
using Xamarin.Essentials;

namespace VendorX.Droid
{
    public class Environment : IEnvironment
    {
        public async void SetStatusBarColor(System.Drawing.Color color, bool darkStatusBarTint)
        {
            if (Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Lollipop)
                return;

            var activity = Platform.CurrentActivity;
            var window = activity.Window;

            //this may not be necessary(but may be fore older than M)
            window.AddFlags(Android.Views.WindowManagerFlags.DrawsSystemBarBackgrounds);
            window.ClearFlags(Android.Views.WindowManagerFlags.TranslucentStatus);
            window.SetStatusBarColor(color.ToPlatformColor());


            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
            {
                await Task.Delay(50);
                WindowCompat.GetInsetsController(window, window.DecorView).AppearanceLightStatusBars = darkStatusBarTint;
            }

        }
    }
}




