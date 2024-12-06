using System;
using MongoDB.Bson;
using Realms;

namespace Vendor.Models;

public class Shop : RealmObject
{
  
    [PrimaryKey,MapTo("_id"), Required]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [MapTo("owner_id"), Required]
    public string OwnerId { get; set; }

    [MapTo("photo_url")]
    public string PhotoUrl { get; set; } = string.Empty;

    [MapTo("name"),Required]
    public string Name { get; set; } = string.Empty;

    [MapTo("is_default")]
    public bool IsDefault { get; set; }

    [MapTo("is_active")]
    public bool IsActive { get; internal set; }

    // Возможность продавать в минус
    [MapTo("is_strict")]
    public bool IsStrict { get; set; }

    [MapTo("create_date")]
    public DateTimeOffset CreateDate { get; set; } = DateTimeOffset.Now.AddMinutes(DateTimeOffset.Now.Offset.TotalMinutes);

    [MapTo("access_end")]
    public DateTimeOffset AccessEndDate { get; set; }

    [MapTo("emlpoys")]
    public IList<Member> Employs { get; }

    [MapTo("wallet_tag")]
    public string WalletTag { get; set; } = "₸";


    // 
    //[MapTo("emlpoys_id")]
    //public IList<string> EmploysId { get; }


    public bool CompareId(string shopid)
    {
        return shopid == Id;
        
    }

}

