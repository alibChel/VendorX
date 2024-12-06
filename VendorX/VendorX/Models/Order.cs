using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace VendorX.Models
{
    public class OrderItem : EmbeddedObject
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

        // количество
        [MapTo("count")]
        public int Count { get; set; }

        // цена продажи/приобретения
        [MapTo("price")]
        public double Price { get; set; }


    }


    [MapTo("Orders")]
    public class Order : RealmObject
    {
        [MapTo("_id"), PrimaryKey]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [MapTo("description")]
        public string Description { get; set; } = string.Empty;


        [MapTo("shop_name")]
        public string ShopName { get; set; }

        [MapTo("shop_id")]
        public string ShopId { get; set; }





        [MapTo("order_status")]
        public int OrderStatusRaw { get; set; }

        [JsonIgnore]
        [MapTo("create_date")]
        public DateTimeOffset CreateDate { get; set; } = DateTimeOffset.Now;
        [JsonIgnore]
        [MapTo("complite_date")]
        public DateTimeOffset ComplitDate { get; set; } = DateTimeOffset.MinValue;

        [MapTo("order_items")]
        public IList<OrderItem> OrdersItems { get; }

        [MapTo("price")]
        public double Price { get; set; }

        [MapTo("number")]
        public int Number { get; set; }

        [MapTo("customer")]
        public string CustomerFullName { get; set; }


        [MapTo("phone")]
        public string Phone { get; set; }


        [MapTo("address")]
        public string Address { get; set; }

        [MapTo("service_type")]
        public int RawValue { get; set; }

        [MapTo("owner_id")]
        public string OwnerId { get; set; } = string.Empty;

        [JsonIgnore]
        [Ignored]
        public string CreatedDateToString { get => CreateDate.ToString("dd MMM yyyy HH:mm"); }
        [JsonIgnore]
        [Ignored]
        public string GroupDate { get => (new DateTime(CreatedTicks)).ToString("dd.MM.yyyy"); }


        [Ignored]
        public long CreatedTicks { get { return this.CreateDate.Date.Ticks; } }


        [JsonIgnore]
        [Ignored]
        public ServiceType ServiceType { get => (ServiceType)RawValue; set => RawValue = (int)value; }
        [JsonIgnore]
        [Ignored]
        public string ServiceTypeToString
        {
            get
            {
                return ServiceType switch
                {
                    ServiceType.Halyk => "Халык",
                    ServiceType.Kaspi => "Каспи",
                    ServiceType.Jusan => "Jusan",
                    ServiceType.WebMarket => "Интернет магазин",
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
        [JsonIgnore]
        [Ignored]
        public string OrderStatusToString
        {
            get
            {
                return OrderStatus switch
                {
                    OrderStatus.Pending => "Ожидает обработки",
                    OrderStatus.AcceptedByMerchant => "Ожидает обработки",
                    OrderStatus.Processing => "Обрабатывается",
                    OrderStatus.Shipped => "Отправлен",
                    OrderStatus.Delivered => "Доставлен",
                    OrderStatus.Cancelled => "Отменен",
                    OrderStatus.Success => "Выдан",
                    OrderStatus.Return => "Возврат",
                    OrderStatus.DeliveryReturned => "Возврат доставки",


                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
        [JsonIgnore]
        [Ignored]
        public string OrderStatusColor
        {
            get
            {
                return OrderStatus switch
                {
                    OrderStatus.Pending => "#3E8EED",
                    OrderStatus.AcceptedByMerchant => "#3E8EED",
                    OrderStatus.Processing => "#66B2F0",
                    OrderStatus.Shipped => "#479649",
                    OrderStatus.Delivered => "#479649",
                    OrderStatus.Cancelled => "#964747",
                    OrderStatus.Success => "#479649",


                    _ => "#964747"
                };
            }
        }



        [JsonIgnore]
        [Ignored]
        public OrderStatus OrderStatus{ get => (OrderStatus)OrderStatusRaw; set => OrderStatusRaw = (int)value; }

    }
}
