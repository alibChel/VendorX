using Realms;

namespace Vendor.Models;

public class StockItem : EmbeddedObject
{

    // предмет
    [MapTo("item")]
    public Item Item { get; set; }

    // количество на складе
    [MapTo("on_stock")]
    public int OnStock { get; set; }

}

