using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using Plugin.FacebookClient;
using UIKit;
using Xamarin.Essentials;

namespace VendorX.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Rg.Plugins.Popup.Popup.Init();
            //NativeMedia.Platform.Init(GetTopViewController);
            global::Xamarin.Forms.Forms.Init();
            global::Xamarin.Forms.Forms.Init();
            Forms9Patch.iOS.Settings.Initialize(this);
            LoadApplication(new App());
            Xamarin.Forms.Nuke.FormsHandler.Init(debug: false);
            //Xamarin.Forms.Nuke.FormsHandler.Init(debug: false, disableFileImageSourceHandling: true);
            DevExpress.XamarinForms.CollectionView.iOS.Initializer.Init();

            ZXing.Net.Mobile.Forms.iOS.Platform.Init();

            //Firebase.Core.App.Configure();
            //Preferences.Set("Token", Firebase.CloudMessaging.Messaging.SharedInstance.FcmToken);

            LoadApplication(new App());

            FacebookClientManager.Initialize(app, options);
            
            return base.FinishedLaunching(app, options);
            
        }

        public override void OnActivated(UIApplication uiApplication)
        {
            FacebookClientManager.OnActivated();
            base.OnActivated(uiApplication);
        }

        

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            return FacebookClientManager.OpenUrl(app, url, options);
        }

        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            return FacebookClientManager.OpenUrl(application, url, sourceApplication, annotation);
        }

        public UIViewController GetTopViewController() {
            var vc = UIApplication.SharedApplication.KeyWindow.RootViewController;

            if (vc is UINavigationController navController)
                vc = navController.ViewControllers.Last();

            return vc;
        }
    }
}

