

using Vendor.Models;
using VendorX.Resources;
namespace Vendor.ViewModels; 
public partial class NotificationViewModel:BaseViewModel {

    public ObservableCollection<Notification> Items { get; set; } = new ObservableCollection<Notification>();

    [ObservableProperty]
    private IQueryable<Invites> invites;

    // экземпляр реалма
    private Realm realm;    

    public NotificationViewModel() {
        Title = AppResources.NotificationsLabels;
            //"Уведомления";
        realm = RealmService.GetMainThreadRealm();
    }

    partial void OnInvitesChanged(IQueryable<Invites> value) {
        Items?.Clear();
        CreateNotification();
    }

    private void CreateNotification() {
        foreach(var item in Invites) 
        {
            if (item.TargetId != UserId || item.TargetId!= RealmService.CurrentUser.Id)
                continue;
            var notif = new Notification { Title =AppResources.InvitationLabel, ItemId = item.Id };
            // "Приглашение"
            notif.Description = $"{item.OwnerName} {AppResources.HasInviteInLabel} {item.ShopName} {AppResources.OnRoleLabel} {getRoleText(item.Role)}.  \r\n {item.CreateDate.ToString("f")}";
            //$"{item.OwnerName} пригласил вас в {item.ShopName} на должность : {getRoleText(item.Role)}.  \r\n {item.CreateDate.ToString("f")}";
            Items.Add(notif);
        }
    }

    private string getRoleText(UserRole role)
    {
        var result = string.Empty;
        switch (role)
        {
            case UserRole.Owner:
                result = AppResources.OwnerLabel;
                    //"Владелец";
                break;
            case UserRole.Manager:
                result = AppResources.ManagerLabel; 
                    //"Управляющий";
                break;
            case UserRole.User:
                result = AppResources.SalesmanLabel; 
                    //"Продавец";
                break;  
        }
        return result;
    }

    // Принять приглашение
    [RelayCommand]
    private async Task AcceptInvites(Notification _invites) 
    {
        if (realm.IsClosed || realm == null)
        {
            await RealmService.Init();
            realm = RealmService.GetMainThreadRealm();
        }
        await realm.WriteAsync(() =>
        {
            Invites.FirstOrDefault(x => x.Id == _invites.ItemId).State = InviteState.Accept;
        });
        Items.Remove(_invites);
        IsBusy = true;
        await Task.Delay(4000);
        await realm.SyncSession.WaitForDownloadAsync();
        await RealmService.SetSubscription(realm, SubscriptionType.Mine);
        IsBusy = false;
        await Task.CompletedTask;
    }

    // Отклонить приглашение
    [RelayCommand]
    private void DenyInvites(Notification _invites) {

        realm.WriteAsync(() => {
            Invites.FirstOrDefault(x => x.Id == _invites.ItemId).State = InviteState.Decline;
        });
        Items.Remove(_invites);
    }

    internal async Task OnAppearing() {
        Invites = realm.All<Invites>();
     
        await Task.CompletedTask;
    }
}
