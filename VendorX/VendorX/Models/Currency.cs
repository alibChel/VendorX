using System;
namespace Vendor.Models;
using VendorX.Resources;

public class Currency
{
    public string Text { get; set; }
    public string Value { get; set; }



    public static List<Currency> GetCurrencies()
    {
        var result = new List<Currency>();

        result.Add(new Currency { Text = "KZT", Value = "₸" });
        result.Add(new Currency { Text = "RUR", Value = "₽" });
        result.Add(new Currency { Text = "USD", Value = "$" });
        result.Add(new Currency { Text = "KGS", Value = "⊆" });
        result.Add(new Currency { Text = "UZS", Value = " " });
        return result;
    }

    public override string ToString()
    {
        return Text;
    }
}

