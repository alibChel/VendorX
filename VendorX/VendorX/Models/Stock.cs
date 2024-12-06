using System;
using MongoDB.Bson;
using Realms;

namespace Vendor.Models;

public class Stock: RealmObject
{
    [PrimaryKey, MapTo("_id"), Required]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    //  id магазина владельца
    [MapTo("owner_id"), Required]
    public string OwnerId { get; set; }

    // список предметов на складе
    [MapTo("stock_items")]
    public IList<StockItem> StockItems { get; }
}

