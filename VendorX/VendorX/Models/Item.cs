using System;
using System.Collections.Generic;
using MongoDB.Bson;
using Realms;

namespace Vendor.Models;

public class Item : RealmObject
{
  
    [PrimaryKey,MapTo("_id"),Required]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    //  id магазина владельца
    [MapTo("owner_id"), Required]
    public string OwnerId { get; set; }
    //[MapTo("creator_id")]
    //[Required]
    //public string CreatorId { get; set; } = string.Empty;
    //[MapTo("shop_id")]
    //[Required]
    //public string ShopId { get; set; } = string.Empty;
    [MapTo("name"),Required]
    public string Name { get; set; } = string.Empty;

    [MapTo("description")]
    public string Description { get; set; } = string.Empty;

    [MapTo("barcode")]
    public string BarCode { get; set; } = string.Empty;

    [MapTo("price")]
    public double Price { get; set; }

    [MapTo("cost")]
    public double Cost { get; internal set; }

    [MapTo("photo_url")]
    public string PhotoUrl { get; set; }

    [MapTo("photo_urls"),Required]
    public IList<string> PhotoUrls { get; }

    [MapTo("tags"),Required]
    public IList<string> Tags { get; }

    [MapTo("raiting")]
    public double Raiting { get; set; }

    [MapTo("is_favorit")]
    public bool IsFavorite { get; set; }

    [MapTo("is_arhive")]
    public bool IsArchive { get; set; }

    [MapTo("create_date")]
    public DateTimeOffset CreateDate { get; set; } = DateTimeOffset.Now.AddMinutes(DateTimeOffset.Now.Offset.TotalMinutes);

    // количество на складе
    [MapTo("on_stock")]
    public int OnStock { get; set; }

    public Item() { }


}

