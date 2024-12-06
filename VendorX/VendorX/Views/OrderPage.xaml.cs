using Autofac;
using DevExpress.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VendorX.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OrderPage : ContentPage
    {
        private readonly OrderPageViewModel _vm;
        // ускорение для отслеживания позиций при ручном перемещении корзины
        private double lastVelocity;
        private bool firstloadStartDate = true, firstloadEndDate = true;
        // Анимитор корзины
        AnimationStateMachine filterbarState;

        public OrderPage()
        {
            InitializeComponent();
            BindingContext = _vm = DependencyConfig.Container.Resolve<OrderPageViewModel>();
        }



        protected override async void OnAppearing()
        {
            base.OnAppearing();
         
            _vm.OrderCollectionView = dxcollectionOrder;
            await _vm.OnAppearing();

        }

        // Нажатие кнопки или жест назад если открыты фильтры или корзина закрывает их
        protected override bool OnBackButtonPressed()
        {

            if ((FilterStates)filterbarState.CurrentState == FilterStates.Show)
            {
                filterbarState.Go(FilterStates.Hide);
                return true;
            }

            return base.OnBackButtonPressed();
        }

        // Изменение размера 
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (filterbarState == null)
            {
                SetupFilterStates();
            }
            if (filterbarState.CurrentState == null)
                filterbarState.Go(FilterStates.Hide, false);
        }

        // Настройка состояния меню фильтрации
        private void SetupFilterStates()
        {
            filterbarState = new AnimationStateMachine();
            filterbarState.Add(FilterStates.Show, new ViewTransition[]
            {
            new ViewTransition(FilterBG, AnimationType.Opacity, 1, length:250),
            new ViewTransition(FilterBar, AnimationType.Opacity, 1, length:250),
            new ViewTransition(FilterBar, AnimationType.TranslationY, 0, length: 250)

            });
            filterbarState.Add(FilterStates.Hide, new ViewTransition[]
            {
            new ViewTransition(FilterBG, AnimationType.Opacity, 0, length:250),
            new ViewTransition(FilterBar, AnimationType.Opacity, 0, length:250),
            new ViewTransition(FilterBar, AnimationType.TranslationY, 300, length: 250)

            });
            _vm.FilterbarState = filterbarState;
        }

        void PanGestureRecognizer_PanUpdated(System.Object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    lastVelocity = e.TotalY;
                    if (lastVelocity < 0)
                    {
                        if (FilterBar.TranslationY >= 0)
                            FilterBar.TranslationY += e.TotalY;

                    }
                    else if (lastVelocity > 0)
                    {

                        FilterBar.TranslationY += e.TotalY;
                    }
                    break;
                case GestureStatus.Completed:

                    if (lastVelocity < -10)
                        filterbarState.Go(FilterStates.Show);
                    else
                    {
                        filterbarState.Go(FilterStates.Hide);
                    }
                    break;
            }
        }

       

    }
}