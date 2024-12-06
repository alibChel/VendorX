using Xamarin.Forms;

namespace VendorX.Views.Template
{
    [ContentProperty("Child")]
    public partial class HeaderViewProfile : ContentView
    {


        public static readonly BindableProperty ProfileUrlProperty =
            BindableProperty.Create(nameof(ProfileUrl), typeof(string), typeof(HeaderViewProfile), default(string), BindingMode.TwoWay, propertyChanged: OnProfileUrlChanged);


        public static readonly BindableProperty ShopNameProperty =
            BindableProperty.Create(nameof(ShopName), typeof(string), typeof(HeaderViewProfile), default(string), BindingMode.TwoWay, propertyChanged: OnShopNameChanged);


        private static void OnProfileUrlChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (bindable is HeaderViewProfile headerViewProfile)
                {
                    headerViewProfile.OnPropertyChanged((string)oldValue, (string)newValue, true);
                }
            });
        }
        private static void OnShopNameChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (bindable is HeaderViewProfile headerViewProfile)
                {
                    headerViewProfile.OnPropertyChanged((string)oldValue, (string)newValue, false);
                }
            });
        }

        private void OnPropertyChanged(string oldValue, string newValue, bool isProfile)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (isProfile)
                {
                    ProfileImage.Source = newValue;
                    if (string.IsNullOrWhiteSpace(newValue))
                    {
                        IconUser.IsVisible = false;
                        ProfileImage.IsVisible = false;

                    }
                    else
                    {
                        IconUser.IsVisible = true;
                        ProfileImage.IsVisible = true;

                    }

                }
                else
                {
                    ShopNameLabel.Text = newValue;
                }
            });
        }

        public string ProfileUrl
        {
            get
            {

                return (string)GetValue(ProfileUrlProperty);

            }
            set
            {
                SetValue(ProfileUrlProperty, value);
            }
        }
        public string ShopName
        {
            get => (string)GetValue(ShopNameProperty);
            set => SetValue(ShopNameProperty, value);
        }


        public HeaderViewProfile()
        {
            InitializeComponent();

        }
    }

}