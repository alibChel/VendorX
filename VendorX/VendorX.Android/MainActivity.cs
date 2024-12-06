using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Xamarin.Forms;
using Android.Content;
using Java.Security;
using Plugin.FacebookClient;
using CrossGeeks.Facebook;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Specialized;
using static Android.App.VoiceInteractor;
using System.Runtime.Remoting.Contexts;
using Android.Graphics;
using Xamarin.Essentials;
//using Xamarin.Auth;
using Firebase.Iid;
using Firebase;
using Android.Util;

[assembly: Dependency(typeof(VendorX.Droid.Environment))]

namespace VendorX.Droid
{
    [Activity(Label = "Vendor", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        const string CALLBACK_SCHEME = "kz.gosu.vendor";
        protected override void OnCreate(Bundle savedInstanceState)
        {

            /*      try
                 {
                      PackageInfo info = Android.App.Application.Context.PackageManager.GetPackageInfo(Android.App.Application.Context.PackageName, PackageInfoFlags.Signatures);
                     foreach (var signature in info.Signatures)
                   {
                       MessageDigest md = MessageDigest.GetInstance("SHA");
                       md.Update(signature.ToByteArray());

                       System.Diagnostics.Debug.WriteLine(Convert.ToBase64String(md.Digest()));
                    }
                  }
                 catch (NoSuchAlgorithmException e)
                 {
                      System.Diagnostics.Debug.WriteLine(e);
                 }
                 catch (Exception e)
                 {
                      System.Diagnostics.Debug.WriteLine(e);
              }*/

            RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            base.OnCreate(savedInstanceState);
            //if (Intent.Extras != null)
            //{
            //    foreach (var key in Intent.Extras.KeySet())
            //    {
            //        var value = Intent.Extras.GetString(key);
            //        Log.Debug("TAG", "Key: {0} Value: {1}", key, value);
            //    }
            //}
            //Log.Debug("TAG", "InstanceID token: " + FirebaseInstanceId.Instance.Token);

            //Preferences.Set("DeviceToken",FirebaseInstanceId.Instance.Token );

            try
            {
                //FacebookSdk.SdkInitialize(this);
                FacebookClientManager.Initialize(this);

            }
            catch (Exception x)
            {
                Console.WriteLine(x.Message);
            }
            NativeMedia.Platform.Init(this, savedInstanceState);
            Rg.Plugins.Popup.Popup.Init(this);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            global::ZXing.Net.Mobile.Forms.Android.Platform.Init();
            //global::Xamarin.Auth.Presenters.XamarinAndroid.AuthenticationConfiguration.Init(this, savedInstanceState);

            ZXing.Mobile.MobileBarcodeScanner.Initialize(Application);
            //FacebookSdk.SdkInitialize(ApplicationContext);
            Android.Glide.Forms.Init(this);

            DevExpress.XamarinForms.CollectionView.Initializer.Init();


            FirebaseApp.InitializeApp(this);

            LoadApplication(new App());
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            if (NativeMedia.Platform.CheckCanProcessResult(requestCode, resultCode, intent))
                NativeMedia.Platform.OnActivityResult(requestCode, resultCode, intent);

            base.OnActivityResult(requestCode, resultCode, intent);
            FacebookClientManager.OnActivityResult(requestCode, resultCode, intent);
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            //ZXing.Net.Mobile.Forms.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
      

       
        [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
        [IntentFilter(new[] { Android.Content.Intent.ActionView }, Categories = new[] { Android.Content.Intent.CategoryDefault, Intent.CategoryBrowsable }, DataScheme = CALLBACK_SCHEME)]

        public class WebAuthenticationCallbackActivity : WebAuthenticatorCallbackActivity
        {
          



        }
    }
}
