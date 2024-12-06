using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace VendorX.Views
{	
	public partial class ChangePasswordView : ContentPage
	{
        private TaskCompletionSource<bool> _taskCompletionSource;
        public Task<bool> PopupClosedTask => _taskCompletionSource.Task;

        ChengePasswordViewModel vm;
        public ChangePasswordView()
        {
            InitializeComponent();
            vm = (ChengePasswordViewModel)BindingContext;
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
            _taskCompletionSource.SetResult(vm.Result);
            base.OnDisappearing();
        }

        void SwichPassVisible(System.Object sender, System.EventArgs e)
        {
            PassEntry.IsPassword = !PassEntry.IsPassword;
        }
      
    }
}

