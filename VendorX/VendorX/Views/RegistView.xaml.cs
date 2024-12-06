using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace VendorX.Views
{

    public partial class RegistView : ContentPage
    {
        private TaskCompletionSource<bool> _taskCompletionSource;
        public Task<bool> PopupClosedTask => _taskCompletionSource.Task;

        RegistViewModel vm;
        public RegistView()
        {
            InitializeComponent();
            vm = (RegistViewModel)BindingContext;
            vm.LoginEntry = LoginEntry;

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            vm.OnAppearing();
            _taskCompletionSource = new TaskCompletionSource<bool>();
        }

        protected override void OnDisappearing()
        {
            _taskCompletionSource.SetResult(true);
            base.OnDisappearing();
        }

        void SwichPassVisible(System.Object sender, System.EventArgs e)
        {
            PassEntry.IsPassword = !PassEntry.IsPassword;
        }
    /*    void SwichRePassVisible(System.Object sender, System.EventArgs e)
        {
            RePassEntry.IsPassword = !RePassEntry.IsPassword;
        }*/

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vm.Code.Length >= 6)
            {
                vm.ConfirmCode();
            }
        }
    }

 //   public partial class RegistView : 
	//{	
	//	public RegistView ()
	//	{
	//		InitializeComponent ();
	//	}
	//}
}

