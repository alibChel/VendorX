using DevExpress.Utils;
using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace VendorX.Views;

public partial class TransactionsPage : ContentPage
{
    TransactionsViewModel vm;
    // ускорение для отслеживания позиций при ручном перемещении корзины
    private double lastVelocity;
    private bool firstloadStartDate = true, firstloadEndDate = true;
    // Анимитор корзины
    AnimationStateMachine filterbarState;
     
    public TransactionsPage()
    {
        InitializeComponent();
        vm = (TransactionsViewModel)BindingContext;
    }

    //protected override async void OnNavigatedTo(NavigatedToEventArgs args) {
    //    base.OnNavigatedTo(args);

    //    await vm.OnAppearing();
    //}

    protected override async void OnAppearing() {
        base.OnAppearing();
        vm.CollectionView = dxcollection;
        await vm.OnAppearing();
        
    }

    // Нажатие кнопки или жест назад если открыты фильтры или корзина закрывает их
    protected override bool OnBackButtonPressed() {

        if ((FilterStates)filterbarState.CurrentState == FilterStates.Show) {
            filterbarState.Go(FilterStates.Hide);
            return true;
        }

        return base.OnBackButtonPressed();
    }

    // Изменение размера 
    protected override void OnSizeAllocated(double width, double height) {
        base.OnSizeAllocated(width, height);

        if (filterbarState == null) {
            SetupFilterStates();
        }
        if (filterbarState.CurrentState == null)
            filterbarState.Go(FilterStates.Hide, false);
    }

    // Настройка состояния меню фильтрации
    private void SetupFilterStates() {
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
        vm.FilterbarState = filterbarState;
    }

    void PanGestureRecognizer_PanUpdated(System.Object sender, PanUpdatedEventArgs e) {
        switch (e.StatusType) {
            case GestureStatus.Running:
                lastVelocity = e.TotalY;
                if (lastVelocity < 0) {
                    if (FilterBar.TranslationY >= 0)
                        FilterBar.TranslationY += e.TotalY;

                }
                else if (lastVelocity > 0) {

                    FilterBar.TranslationY += e.TotalY;
                }
                break;
            case GestureStatus.Completed:

                if (lastVelocity < -10)
                    filterbarState.Go(FilterStates.Show);
                else {
                    filterbarState.Go(FilterStates.Hide);
                }
                break;
        }
    }

    private void DatePicker_DateSelectedStartDate(object sender, DateChangedEventArgs e)
    {
      
        if (firstloadStartDate)
        {
            firstloadStartDate = false;
            return;
        }

        if (vm.FilterPeriod)
            vm.FiltredTransactionPeriod();
    }
    private void DatePicker_DateSelectedEndDate(object sender, DateChangedEventArgs e)
    {
/*
        if (firstloadEndDate)
        {
            firstloadEndDate = false;
            return;
        }*/

        if (vm.FilterPeriod)
            vm.FiltredTransactionPeriod();
    }
    
}

