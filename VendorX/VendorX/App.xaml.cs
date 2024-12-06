using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Vendor.Services;
using VendorX.Views;
using Forms9Patch;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using VendorX.Helpers;
using Xamarin.Essentials;
using VendorX.Resources;
using System.Globalization;
using Xamarin.CommunityToolkit.Helpers;
using Autofac;
using DevExpress.XamarinForms.Core;
using VendorX.Services;
using IContainer = Autofac.IContainer;
using Autofac.Extras.CommonServiceLocator;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.Messaging;
using Vendor.Models.Messages;
using Rg.Plugins.Popup.Pages;
using Realms.Sync;
using VendorX.Views.Template;

namespace VendorX
{
    public partial class App : Application
    {
        public static int TabHeight { get; set; }
        public static int SaveAreaHeight { get; set; }

        public App()
        {
            InitializeComponent();
            LocalizationResourceManager.Current.Init(AppResources.ResourceManager);
            LocalizationResourceManager.Current.PropertyChanged += (sender, e) => AppResources.Culture = LocalizationResourceManager.Current.CurrentCulture;
            //  DependencyService.Register<IRabitPlatform>();
            DependencyConfig.RegisterDependencies();



            MainPage = new LoginPage();



        }



        protected override void OnStart()
        {
            try
            {
                OnResume();
                AppCenter.Start("android=2af9c43d-7c0e-40c1-9810-0c57eb6cb4cd;" + "ios=783dce06-6d97-4942-b1a2-7db5c4cde84d;",
                      typeof(Analytics), typeof(Crashes));
            }
            catch { }

        }



        protected override void OnSleep()
        {
            try
            {
                TheTheme.SetTheme();
                RequestedThemeChanged -= App_RequestedThemeChanged;
            }
            catch { }
        }

        protected override void OnResume()
        {
            try
            {
                TheTheme.SetTheme();
                RequestedThemeChanged += App_RequestedThemeChanged;
            }
            catch { }
        }
        private void App_RequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                TheTheme.SetTheme();
            });
        }
    }



    public static class DependencyConfig
    {
        public static IContainer Container { get; private set; }

        public static void RegisterDependencies()
        {
            var builder = new ContainerBuilder();

            builder.RegisterAssemblyTypes(typeof(BaseViewModel).Assembly)
                   .Where(t => t.IsSubclassOf(typeof(BaseViewModel)))
                   .AsSelf()
                   .InstancePerDependency();

            // Регистрация всех ContentPage
            builder.RegisterAssemblyTypes(typeof(ContentPage).Assembly)
                   .Where(t => t.IsSubclassOf(typeof(ContentPage)))
                   .AsSelf()
                   .InstancePerDependency();

            // Регистрация всех PopupPage
            builder.RegisterAssemblyTypes(typeof(PopupPage).Assembly)
                   .Where(t => t.IsSubclassOf(typeof(PopupPage)))
                   .AsSelf()
                   .InstancePerDependency();


          

            builder.RegisterAssemblyTypes(typeof(Xamarin.Forms.ContentView).Assembly)
                 .Where(t => t.IsSubclassOf(typeof(Xamarin.Forms.ContentView)))
    .AsSelf()
    .InstancePerDependency();





            Container = builder.Build();

            var csl = new AutofacServiceLocator(Container);
            ServiceLocator.SetLocatorProvider(() => csl);
        }
    }



}

