using System;
using Realms;
using Realms.Sync;
using System.Text;
using Transaction = Vendor.Models.Transaction;
using Xamarin.Essentials;
using System.IO;
using Newtonsoft.Json;
using Xamarin.CommunityToolkit.Converters;
using MongoDB.Bson.Serialization.Conventions;
using System.Runtime.CompilerServices;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using VendorX.Models;

namespace Vendor.Services;

public static class RealmService
{
    private static bool serviceInitialised;

    private static Realms.Sync.App app;

    private static Realm mainThreadRealm;

    public static User CurrentUser => app.CurrentUser;

    public static string CurrentShopID { get => Preferences.Get(nameof(CurrentShopID), ""); set => Preferences.Set(nameof(CurrentShopID), value); }
    public static string CurrentLogin { get => Preferences.Get(nameof(CurrentLogin), ""); set => Preferences.Set(nameof(CurrentLogin), value); }
    public static string CurrentPhone { get => Preferences.Get(nameof(CurrentPhone), ""); set => Preferences.Set(nameof(CurrentPhone), value); }

    public static async Task Init()
    {
        if (serviceInitialised)
        {
            return;
        }

        //using Stream fileStream = await FileSystem.AppDataDirectory.OpenAppPackageFileAsync("atlasConfig.json");
        //using StreamReader reader = new StreamReader(fileStream);
        //var fileContent = await reader.ReadToEndAsync();

        //var config = JsonSerializer.Deserialize<RealmAppConfig>(fileContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var config = new RealmAppConfig
        {
            //AppId = "vendorm-rxksc", prod
            AppId = "vendor_gba_test-wntprnc",
            BaseUrl = "https://realm.mongodb.com"
        };

        var appConfiguration = new Realms.Sync.AppConfiguration(config.AppId)
        {
            BaseUri = new Uri(config.BaseUrl)
        };

        app = Realms.Sync.App.Create(appConfiguration);

        serviceInitialised = true;
        await Task.CompletedTask;
    }

    public static Realm GetMainThreadRealm()
    {
        return mainThreadRealm ??= GetRealm();
    }

    private static string GetShopSubsFilter()
    {
        Realm realm = GetMainThreadRealm();
        var mems = realm.All<Member>().ToList();
        var owner_id = CurrentUser.Identities[0].Id;
        string query = $"owner_id == '{owner_id}' OR owner_id=='{CurrentUser.Id}'";
        foreach (var mem in mems)
        {
            if (mem.OwnerId != owner_id)
            {
                query += $"OR _id == '{mem.OwnerId}' ";
            }
        }
        return query;
    }

    private static string GetTransactionSubsFilter()
    {
        Realm realm = GetMainThreadRealm();
        var owner_id = CurrentUser.Identities[0].Id;
        string query = $"autor_id == '{owner_id}' OR autor_id=='{CurrentUser.Id}'";
        var mems = realm.All<Member>().ToList();
        if (mems.Count == 0)
            return query;
        var mem = mems.Where(x => x.Shop.Id == CurrentShopID).First();
        if (mem.Role != UserRole.User && !string.IsNullOrEmpty(CurrentShopID))
            query += $"OR owner_id == '{CurrentShopID}' ";
        if (!string.IsNullOrEmpty(CurrentShopID))
            query += $"AND owner_id == '{CurrentShopID}'";
        return query;
    }





    public static Realm GetRealm()
    {
        var config = new FlexibleSyncConfiguration(app.CurrentUser)
        {
            PopulateInitialSubscriptions = (realm) =>
            {
                var (queryProfile, queryNameProfile) = GetQueryForSubscriptionProfile(realm, SubscriptionType.Mine);
                var (queryItem, queryNameItem) = GetQueryForSubscriptionItem(realm, SubscriptionType.Mine);
                var (queryMember, queryNameMember) = GetQueryForSubscriptionMember(realm, SubscriptionType.Mine);
                var (queryShop, queryNameShop) = GetQueryForSubscriptionShop(realm, SubscriptionType.Mine);
                var (queryOrder, queryNameOrder) = GetQueryForSubscriptionOrder(realm, SubscriptionType.Mine);
                //var (queryStock, queryNameStock) = GetQueryForSubscriptionStock(realm, SubscriptionType.Mine);
                var (queryTransaction, queryNameTransaction) = GetQueryForSubscriptionTransaction(realm, SubscriptionType.Mine);
                var (queryInvites, queryNameInvites) = GetQueryForSubscriptionInvites(realm, SubscriptionType.Mine);
                var (queryInvitesForlogin, queryNameInvitesForlogin) = GetQueryForSubscriptionInvitesForLogin(realm, SubscriptionType.Mine);


                realm.Subscriptions.Add(queryProfile, new SubscriptionOptions { Name = queryNameProfile });
                realm.Subscriptions.Add(queryItem, new SubscriptionOptions { Name = queryNameItem });
                realm.Subscriptions.Add(queryMember, new SubscriptionOptions { Name = queryNameMember });
                realm.Subscriptions.Add(queryShop, new SubscriptionOptions { Name = queryNameShop });
                realm.Subscriptions.Add(queryOrder, new SubscriptionOptions { Name = queryNameOrder });
                //realm.Subscriptions.Add(queryStock, new SubscriptionOptions { Name = queryNameStock });
                realm.Subscriptions.Add(queryTransaction, new SubscriptionOptions { Name = queryNameTransaction });
                realm.Subscriptions.Add(queryInvites, new SubscriptionOptions { Name = queryNameInvites });
                realm.Subscriptions.Add(queryInvitesForlogin, new SubscriptionOptions { Name = queryNameInvitesForlogin });

            }
        };

        return Realm.GetInstance(config);
    }

    public static async Task RegisterAsync(string token)
    {
        if (app == null)
            await Init();
        var user = await app.LogInAsync(Credentials.JWT(token));
        //CurrentShopID = user.Id;
        using var realm = GetRealm();
        await realm.Subscriptions.WaitForSynchronizationAsync();
    }

    public static async Task LoginAsync(string token)
    {
        if (app == null)
            await Init();
        var user = await app.LogInAsync(Credentials.JWT(token));
        //CurrentShopID = user.Id;
        //This will populate the initial set of subscriptions the first time the realm is opened

        //TODO получение Email
        var email = user.App.CurrentUser.Profile.Email;
        var phone = user.App.CurrentUser.Profile.Name;
        CurrentLogin = email;
        CurrentPhone = phone;
        using var realm = GetRealm();
        await realm.Subscriptions.WaitForSynchronizationAsync();

        var shops = realm.All<Shop>();
        bool awaitloadshop = true;
        while (awaitloadshop)
        {
            if (shops?.Count() > 0)
            {
                awaitloadshop = false;
                CurrentShopID = shops.FirstOrDefault().Id;

            }
            else
            {
                if (realm.Subscriptions.Count <= 0)
                    await SetSubscription(realm, SubscriptionType.Mine);
                shops = realm.All<Shop>();
                await Task.Delay(1000);
            }

        }
    }

    public static async Task ChangePasswordAsync(string target_login)
    {
        await app.EmailPasswordAuth.SendResetPasswordEmailAsync(target_login);
    }

    public static async Task LogoutAsync()
    {
        mainThreadRealm.Subscriptions.Update(() =>
        {
            mainThreadRealm.Subscriptions.RemoveAll(true);
        });
        CurrentShopID = string.Empty;
        CurrentLogin = string.Empty;
        CurrentPhone = string.Empty;
        //serviceInitialised = false;
        await app.CurrentUser.LogOutAsync();
        mainThreadRealm?.Dispose();
        mainThreadRealm = null;
    }

    public static async Task SetSubscription(Realm realm, SubscriptionType subType)
    {
        //if (GetCurrentSubscriptionType(realm) == subType)
        //{
        //    return;
        //}

        realm.Subscriptions.Update(() =>
        {
            //realm.Subscriptions.RemoveAll(true);

            var (queryProfile, queryNameProfile) = GetQueryForSubscriptionProfile(realm, SubscriptionType.Mine);
            var (queryItem, queryNameItem) = GetQueryForSubscriptionItem(realm, SubscriptionType.Mine);
            var (queryMember, queryNameMember) = GetQueryForSubscriptionMember(realm, SubscriptionType.Mine);
            var (queryShop, queryNameShop) = GetQueryForSubscriptionShop(realm, SubscriptionType.Mine);
            var (queryOrder, queryNameOrder) = GetQueryForSubscriptionOrder(realm, SubscriptionType.Mine);
            //var (queryStock, queryNameStock) = GetQueryForSubscriptionStock(realm, SubscriptionType.Mine);
            var (queryTransaction, queryNameTransaction) = GetQueryForSubscriptionTransaction(realm, SubscriptionType.Mine);
            var (queryInvites, queryNameInvites) = GetQueryForSubscriptionInvites(realm, SubscriptionType.Mine);
            var (queryInvitesForlogin, queryNameInvitesForlogin) = GetQueryForSubscriptionInvitesForLogin(realm, SubscriptionType.Mine);



            realm.Subscriptions.Add(queryProfile, new SubscriptionOptions { Name = queryNameProfile });
            realm.Subscriptions.Add(queryItem, new SubscriptionOptions { Name = queryNameItem });
            realm.Subscriptions.Add(queryMember, new SubscriptionOptions { Name = queryNameMember });
            realm.Subscriptions.Add(queryShop, new SubscriptionOptions { Name = queryNameShop });
            realm.Subscriptions.Add(queryOrder, new SubscriptionOptions { Name = queryNameOrder });
            //realm.Subscriptions.Add(queryStock, new SubscriptionOptions { Name = queryNameStock });
            realm.Subscriptions.Add(queryTransaction, new SubscriptionOptions { Name = queryNameTransaction });
            realm.Subscriptions.Add(queryInvites, new SubscriptionOptions { Name = queryNameInvites });
            realm.Subscriptions.Add(queryInvitesForlogin, new SubscriptionOptions { Name = queryNameInvitesForlogin });

        });

        //There is no need to wait for synchronization if we are disconnected
        if (realm.SyncSession.ConnectionState != ConnectionState.Disconnected)
        {
            await realm.Subscriptions.WaitForSynchronizationAsync();
        }
    }

    public static SubscriptionType GetCurrentSubscriptionType(Realm realm)
    {
        var activeSubscription = realm.Subscriptions.FirstOrDefault();

        return activeSubscription.Name switch
        {
            "all" => SubscriptionType.All,
            "mine" => SubscriptionType.Mine,
            _ => throw new InvalidOperationException("Unknown subscription type")
        };
    }

    private static (IQueryable<Profile> Query, string Name) GetQueryForSubscriptionProfile(Realm realm, SubscriptionType subType)
    {
        IQueryable<Profile> query = null;
        string queryName = null;
        if (subType == SubscriptionType.Mine)
        {
            var owner_id = CurrentUser.Identities[0].Id;
            query = realm.All<Profile>().Where(i => i.OwnerId == owner_id || i.OwnerId == CurrentUser.Id);
            queryName = "mineProfile";
        }
        else if (subType == SubscriptionType.All)
        {
            query = realm.All<Profile>();
            queryName = "allProfile";
        }
        else
        {
            throw new ArgumentException("Unknown subscription type");
        }

        return (query, queryName);



    }




    private static (IQueryable<Order> Query, string Name) GetQueryForSubscriptionOrder(Realm realm, SubscriptionType subType)
    {
        IQueryable<Order> query = null;
        string queryName = null;
        if (subType == SubscriptionType.Mine)
        {
            query = realm.All<Order>().Where(i => i.ShopId == CurrentShopID);
            queryName = "mineOrder";
        }
        else if (subType == SubscriptionType.All)
        {
            query = realm.All<Order>();
            queryName = "allOrders";
        }
        else
        {
            throw new ArgumentException("Unknown subscription type");
        }

        return (query, queryName);
    }

    private static (IQueryable<Member> Query, string Name) GetQueryForSubscriptionMember(Realm realm, SubscriptionType subType)
    {
        IQueryable<Member> query = null;
        string queryName = null;
        if (subType == SubscriptionType.Mine)
        {
            //TODO подписка по Email
            var owner_id = CurrentUser.Identities[0].Id;
            query = realm.All<Member>().Where(i => i.OwnerId == owner_id || i.OwnerId == CurrentUser.Id || i.OwnerId == CurrentShopID || i.UserEmail == CurrentLogin || i.UserEmail == CurrentPhone);
            queryName = "mineMember";
            //Task.Delay(250);
            Task.Delay(250);

        }
        else if (subType == SubscriptionType.All)
        {
            query = realm.All<Member>();
            queryName = "allMember";
        }
        else
        {
            throw new ArgumentException("Unknown subscription type");
        }

        return (query, queryName);
    }
    private static (IQueryable<Shop> Query, string Name) GetQueryForSubscriptionShop(Realm realm, SubscriptionType subType)
    {
        IQueryable<Shop> query = null;
        string queryName = null;

        if (subType == SubscriptionType.Mine)
        {
            query = realm.All<Shop>().Filter(GetShopSubsFilter());
            Task.Delay(250);
            queryName = "mineShop";
        }
        else if (subType == SubscriptionType.All)
        {
            query = realm.All<Shop>();
            queryName = "allShop";
        }
        else
        {
            throw new ArgumentException("Unknown subscription type");
        }

        return (query, queryName);
    }

    private static (IQueryable<Item> Query, string Name) GetQueryForSubscriptionItem(Realm realm, SubscriptionType subType)
    {
        IQueryable<Item> query = null;
        string queryName = null;

        if (subType == SubscriptionType.Mine)
        {
            query = realm.All<Item>().Where(i => i.OwnerId == CurrentShopID);
            queryName = "mineItem";
        }
        else if (subType == SubscriptionType.All)
        {
            query = realm.All<Item>();
            queryName = "allItem";
        }
        else
        {
            throw new ArgumentException("Unknown subscription type");
        }

        return (query, queryName);
    }

    private static (IQueryable<Transaction> Query, string Name) GetQueryForSubscriptionTransaction(Realm realm, SubscriptionType subType)
    {
        IQueryable<Transaction> query = null;
        string queryName = null;

        if (subType == SubscriptionType.Mine)
        {
            query = realm.All<Transaction>().Filter(GetTransactionSubsFilter());
            queryName = "mineTransaction";
        }
        else if (subType == SubscriptionType.All)
        {
            query = realm.All<Transaction>();
            queryName = "allTransaction";
        }
        else
        {
            throw new ArgumentException("Unknown subscription type");
        }

        return (query, queryName);
    }

    private static (IQueryable<Invites> Query, string Name) GetQueryForSubscriptionInvites(Realm realm, SubscriptionType subType)
    {
        IQueryable<Invites> query = null;
        string queryName = null;

        if (subType == SubscriptionType.Mine)
        {

            //var ownid = string.IsNullOrWhiteSpace(CurrentShopID) ? "" : $"owner_id == {CurrentShopID}  OR";
            //var _filter = $"{ownid} target_login == '{CurrentLogin}'";
            //query = realm.All<Invites>().Filter(_filter);

            query = realm.All<Invites>().Where(i => i.OwnerId == CurrentShopID);
            queryName = "mineInvites";
        }
        else if (subType == SubscriptionType.All)
        {
            query = realm.All<Invites>();
            queryName = "allInvites";
        }
        else
        {
            throw new ArgumentException("Unknown subscription type");
        }

        return (query, queryName);
    }

    private static (IQueryable<Invites> Query, string Name) GetQueryForSubscriptionInvitesForLogin(Realm realm, SubscriptionType subType)
    {
        IQueryable<Invites> query = null;
        string queryName = null;

        if (subType == SubscriptionType.Mine)
        {

            //var ownid = string.IsNullOrWhiteSpace(CurrentShopID) ? "" : $"owner_id == {CurrentShopID}  OR";
            //var _filter = $"{ownid} target_login == '{CurrentLogin}'";
            //query = realm.All<Invites>().Filter(_filter);

            var owner_id = CurrentUser.Identities[0].Id;
            query = realm.All<Invites>().Where(i => i.TargetId == owner_id || i.TargetId == CurrentUser.Id);
            queryName = "mineInvitesForlogin";
        }
        else if (subType == SubscriptionType.All)
        {
            query = realm.All<Invites>();
            queryName = "allInvites";
        }
        else
        {
            throw new ArgumentException("Unknown subscription type");
        }

        return (query, queryName);
    }

    public static async Task DeletUser(string UserId)
    {

        mainThreadRealm.Subscriptions.Update(() =>
        {
            mainThreadRealm.Subscriptions.RemoveAll(true);
        });
        CurrentShopID = string.Empty;
        CurrentLogin = string.Empty;
        CurrentPhone = string.Empty;
        //serviceInitialised = false;
        // await app.CurrentUser.LogOutAsync();
        mainThreadRealm?.Dispose();
        mainThreadRealm = null;
        await CurrentUser.Functions.CallAsync<bool>("UserExit", UserId);


    }

}


public enum SubscriptionType
{
    Mine,
    All,
}

public class RealmAppConfig
{
    public string AppId { get; set; }

    public string BaseUrl { get; set; }
}
