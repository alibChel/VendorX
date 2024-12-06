using DevExpress.Utils.StructuredStorage.Internal.Writer;
using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace VendorX.Views;

public partial class AnaliticsPage : ContentPage
{
    // Высота страницы
    private double conteinerHeigh;
    // Ширина страницы
    private double conteinerWidth;

    AnaliticsViewModel vm;
    public AnaliticsPage ()
	{
		InitializeComponent ();
        BindingContext = vm = new AnaliticsViewModel();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);


        //vm.OnSizeAllocated(width, height);
        conteinerWidth = width;
        conteinerHeigh = height;
        vm.ConteinerWidth = width-40;
        vm.ConteinerHeigh = height - Width*1.5;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        OnSizeAllocated(1,1);
        vm.OnApearing();

    }

}

