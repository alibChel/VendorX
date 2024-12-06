using System;
using MongoDB.Bson;
using Realms;

namespace Vendor.Models;

public class Payment: EmbeddedObject
{
    [MapTo("date")]
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now.AddMinutes(DateTimeOffset.Now.Offset.TotalMinutes);

    [MapTo("summ")]
    public double Summ { get; set; }
}

