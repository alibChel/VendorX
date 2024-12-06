using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MongoDB.Bson;
using Realms;

namespace Vendor.Models;

public class TransactionItem : EmbeddedObject
{
    // id товара
    [MapTo("item_id")]
    public Item ItemId { get; set; }

    // название товара
    [MapTo("item_name"), Required]
    public string ItemName { get; set; } = string.Empty;

    // ссылка на картинку товара
    [MapTo("photo_url")]
    public string PhotoUrl { get; set; } = string.Empty;

    // цена предмета
    [MapTo("item_price")]
    public double ItemPrice { get; set; }

    // себес предмета
    [MapTo("item_cost")]
    public double ItemCost { get; set; }

    // количество
    [MapTo("count")]
    public int Count { get; set; }

    // цена продажи/приобретения
    [MapTo("price")]
    public double Price { get; set; }

    // скидка
    [MapTo("discount")]
    public int Discount { get; set; }

    // наценка
    [MapTo("markup")]
    public int Markup { get; set; }

    // стоимость
    [MapTo("cost")]
    public double Cost { get; set; }

    // количество возвратов
    [MapTo("returned_count")]
    public int ReturnedCount { get; set; }


    [Ignored]
    public double FinalPrice { get => Price * Count; }
    [Ignored]
    public double FinalCost { get => Cost * Count; }

    [Ignored]
    public bool IsItemPriceNotEqualsPrice { get => ItemPrice !=Price; }

    [Ignored]
    public bool IsItemCostNotEqualsPrice { get => ItemCost != Cost; }

    

    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);
        if( propertyName == nameof(Count) )
        {
            RaisePropertyChanged(nameof(FinalPrice));
            RaisePropertyChanged(nameof(FinalCost));
        }
        if (propertyName == nameof(ItemPrice) || propertyName == nameof(Price))
        {
            RaisePropertyChanged(nameof(FinalPrice));            
            RaisePropertyChanged(nameof(IsItemPriceNotEqualsPrice)); 
        }
        if (propertyName == nameof(ItemCost) || propertyName == nameof(Cost))
        {            
            RaisePropertyChanged(nameof(FinalCost));
            RaisePropertyChanged(nameof(IsItemCostNotEqualsPrice));
        }

    }

    public void ReturnedCountChanged()
    {
        RaisePropertyChanged(nameof(ReturnedCount));

    }

    public TransactionItem()
    {

    }

    public TransactionItem(TransactionItem item)
    {
        ItemId = item.ItemId;
        ItemName = item.ItemName;
        PhotoUrl = item.PhotoUrl;
        ItemPrice = item.ItemPrice;
        ItemCost = item.ItemCost;
        Count = item.Count;
        ReturnedCount = item.ReturnedCount;
        Price = item.Price;
        Discount = item.Discount;
        Markup = item.Markup;
        Cost = item.Cost;
    }
}

