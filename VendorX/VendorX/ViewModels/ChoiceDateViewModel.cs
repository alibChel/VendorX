

using Rg.Plugins.Popup.Services;

namespace Vendor.ViewModels;

public partial class ChoiceDateViewModel : BaseViewModel, INotifyPropertyChanged
{
    [ObservableProperty]
    DateTime endDate = new DateTime(DateTime.Now.Ticks, DateTimeKind.Unspecified);
    

    [ObservableProperty]
    DateTime startDate = new DateTime(DateTime.Today.Ticks, DateTimeKind.Unspecified);

    public Tuple<DateTime, DateTime> Result ;

    public ChoiceDateViewModel() 
    {
        
    }

    internal void OnAppearing()
    {
        Result = new Tuple<DateTime, DateTime>(DateTime.MaxValue, DateTime.Now);
    }

    [RelayCommand]
    private async Task Close()
    {
        Result = new Tuple<DateTime,DateTime>(DateTime.MaxValue, DateTime.Now);
        await PopupNavigation.Instance.PopAsync();
    }

    [RelayCommand]
    private async Task Ok()
    {
        Result = new Tuple<DateTime, DateTime>(StartDate, EndDate);
        await PopupNavigation.Instance.PopAsync();
    }


}
