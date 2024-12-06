using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MongoDB.Bson;
using Realms;
using Realms.Sync;
using VendorX.Resources;

namespace Vendor.Models;

public class Member : RealmObject
{
    [PrimaryKey, Required, MapTo("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    // UserId => OwnerId или ShopId
    [MapTo("owner_id"), Required]
    public string OwnerId { get; set; }

    // почта пользователя для отображения
    [MapTo("user_email")]
    public string UserEmail { get; set; } = string.Empty;

    [MapTo("shop")]
    public Shop Shop { get; set; }

    [MapTo("role"), Required]
    private string RoleRaw { get; set; }

    [MapTo("is_default")]
    public bool IsDefault { get; set; }

    [Ignored]
    public UserRole Role
    {
        get { return Enum.TryParse(RoleRaw, out UserRole result) ? result : UserRole.User; }
        set { RoleRaw = value.ToString(); }
    }


    [Ignored]
    public string RoleName {
        get {
            if (Role == UserRole.User)
                return $"{AppResources.SalesmanLabel}";
            else if (Role == UserRole.Manager)
                return $"{AppResources.ManagerLabel}";
            else
                return $"{AppResources.OwnerLabel}";
        }
    
    }

}

