using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VendorX.Widgets
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CodeComplit 
    {
        private TaskCompletionSource<int> _taskCompletionSource;
        public Task<int> PopupClosedTask => _taskCompletionSource.Task;

        RegistViewModel vm;
        public CodeComplit()
        {
            InitializeComponent();
            vm = (RegistViewModel)BindingContext;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            vm.OnAppearing();
            _taskCompletionSource = new TaskCompletionSource<int>();
        }

        protected override void OnDisappearing()
        {
            _taskCompletionSource.SetResult(vm.Result);
            base.OnDisappearing();
        }

      

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

