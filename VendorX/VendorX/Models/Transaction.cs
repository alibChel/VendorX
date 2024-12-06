using System;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Realms;
using VendorX.Models;
using VendorX.Resources;
namespace Vendor.Models;

public class Transaction : RealmObject
{

    [MapTo("_id"), PrimaryKey, Required]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();


    [MapTo("order_id")]
    public Order Order { get; set; }

    [MapTo("parent_transaction_id")]
    public string ParentTransactionId { get; set; } = string.Empty;

    [MapTo("description")]
    public string Description { get; set; } = string.Empty;

    [MapTo("contact_name")]
    public string ContactName { get; set; } = string.Empty;

    [MapTo("contact_phone")]
    public string ContactPhone { get; set; } = string.Empty;

    [MapTo("autor_id"), Required]
    public string AutorId { get; set; } = string.Empty;

    [MapTo("autor_name"), Required]
    public string AutorName { get; set; }

    [MapTo("owner_id"), Required]
    public string OwnerId { get; set; } = string.Empty;

    [MapTo("shop_name"), Required]
    public string ShopName { get; set; }

    // Как временное решение со смещением добавил смещение в дату, почему-то в монго данные без смещения 
    [MapTo("create_date")]
    public DateTimeOffset CreateDate { get; set; } = DateTimeOffset.Now.AddMinutes(DateTimeOffset.Now.Offset.TotalMinutes);

    [MapTo("complite_date")]
    public DateTimeOffset ComplitDate { get; set; } = DateTimeOffset.MinValue;

    [MapTo("operation_code"), Required]
    public string OperationCodeRaw { get; set; }

    [MapTo("state_code")]
    public string StateCodeRaw { get; set; }

    [MapTo("transaction_items")]
    public IList<TransactionItem> TransactionItems { get; }

    [MapTo("price")]
    public double Price { get; set; }

    [MapTo("discount")]
    public int Discount { get; set; }
    [MapTo("markup")]
    public int Markup { get; set; }

    [MapTo("number")]
    public int Number { get; set; }

    [MapTo("payments")]
    public IList<Payment> Payments { get; }

    [Ignored]
    public long CreatedTicks { get { return this.CreateDate.Date.Ticks; } }



    [Ignored]
    public Operation OperationCode
    {
        get { return Enum.TryParse(OperationCodeRaw, out Operation result) ? result : Operation.Sale; }
        set { OperationCodeRaw = value.ToString(); }
    }

    [Ignored]
    public TransactionState StateCode
    {
        get { return Enum.TryParse(StateCodeRaw, out TransactionState result) ? result : TransactionState.New; }
        set { StateCodeRaw = value.ToString(); }
    }


    [Ignored]
    public string GroupData { get => (new DateTime(CreatedTicks)).ToString("dd.MM.yyyy"); }
    [Ignored]
    public string OperationName
    {
        get
        {
            if (OperationCode == Operation.Admission)
            {
                //Discount = 0;
                return AppResources.AdmissionLabel;
                //"Прием";
            }
            if (OperationCode == Operation.Sale) return AppResources.SaleLabel;
            //"Продажа";
            if (OperationCode == Operation.Edit) return AppResources.EditLabel;

            if (OperationCode == Operation.Reserve) return "Резерв";
            //"Коректировка";
            return AppResources.ReturnLabel;//"Возврат";
        }
    }
    public string TransactionToHtml(string WalletTag)
    {
        var CreditPrice = AppResources.DutyLabel;//"Долг: ";
        var client = $"{AppResources.ClientLabel}: ";//"Клиент: ";
        var description = AppResources.NoteLabel;//"Примечание: ";
        if (string.IsNullOrWhiteSpace(ContactName) || string.IsNullOrWhiteSpace(ContactPhone)) client = "";
        else client += ContactName + "  " + ContactPhone;
        if (string.IsNullOrWhiteSpace(Description)) description = "";
        else description += Description;
        string autor = "";
        if (!string.IsNullOrWhiteSpace(AutorName)) autor = $"{AppResources.AuthorLabel} {AutorName}";

        var a = autor;
        var html = $"<!DOCTYPE html>\n<html lang=\"en\">\n<head>\n    <meta charset=\"UTF-8\">\n    <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n    <title>Document</title>\n</head>\n\n<style>\n\ntable {{\n    display: table;\n    border-collapse: separate;\n    box-sizing: border-box;\n    text-indent: initial;\n    border-spacing: 2px;\n    border-color: gray;\n}}\n\n.table-bordered>:not(caption)>*>* {{\n    border-width: 1px;\n}}\n\n\n\n.table{{--bs-table-color:var(--bs-body-color);--bs-table-bg:transparent;--bs-table-border-color:var(--bs-border-color);--bs-table-accent-bg:transparent;--bs-table-striped-color:var(--bs-body-color);--bs-table-striped-bg:rgba(0, 0, 0, 0.05);--bs-table-active-color:var(--bs-body-color);--bs-table-active-bg:rgba(0, 0, 0, 0.1);--bs-table-hover-color:var(--bs-body-color);--bs-table-hover-bg:rgba(0, 0, 0, 0.075);width:100%;margin-bottom:1rem;color:var(--bs-table-color);vertical-align:top;border-color:var(--bs-table-border-color)}}.table>:not(caption)>*>*{{padding:.5rem .5rem;background-color:var(--bs-table-bg);border-bottom-width:1px;box-shadow:inset 0 0 0 9999px var(--bs-table-accent-bg)}}.table>tbody{{vertical-align:inherit}}.table>thead{{vertical-align:bottom}}\n\n\ntable {{\n    caption-side: bottom;\n    border-collapse: collapse;\n}}\n\ntbody, td, tfoot, th, thead, tr {{\n    border-color: inherit;\n    border-style: solid;\n    border-width: 1px;\n}} \n\n\n\nth {{\n    display: table-cell;\n    vertical-align: inherit;\n    font-weight: bold;\n    text-align: -internal-center;\n    text-align: inherit;\n    text-align: -webkit-match-parent;\n}}\n\n\n\n.container-sm-sm {{\n   margin-right: auto;\nmargin-left: auto;\n}}\ntd,tbody,tfoot,th,thead,tr {{\nfont-size: 13px;\n}}\n.d-sm-flex {{\njustify-content: space-between;\n}}\n.border-colors {{\nborder-color: #80b1ee;\n}}\n.borderr {{\npadding: 10px;\nborder-bottom: 2px dashed #80b1ee;\nfont-size: 20px;\ncolor: #80b1ee;\n}}\n.borderrd {{\nborder-top: 2px dashed #80b1ee;\n}}\n.size {{\nmargin-left: 120px;\nwidth: 150px;\n}}\n#borderr {{\nmargin-bottom: 20px;\nborder-bottom: 2px dashed #80b1ee;\n}}\n        #mark {{\r\n            display: flex;\r\n            justify-content: center;\r\n            font-size: 25px;\r\n            color: #66B2F0\r\n        }} </style>\n\n<body>\r\n\r\n    <div class=\"container-sm-sm\">\r\n      " +
            $"<p class=\"border-bottom-2 borderr\" style=\"text-align: center\">{ShopName}</p>\r\n      <div class=\"d-sm-flex \">\r\n       " +
            $" <p>{AppResources.DateLabel} : {(CreateDate.ToString("MM/dd/yyyy HH:mm"))}</p>\r\n         </div>      <div class=\"d-sm-flex \">\r\n      " +
            $"  <p>{AppResources.TransactionNumberLabel} {Number}</p>\r\n           </div>  <div class=\"d-sm-flex \">\r\n    " +
            $"<p>{AppResources.OperationLabel} : {OperationName}</p>\r\n </div> <div class=\"d-sm-flex \">\r\n    <p>{autor}</p>\r\n </div>" +
            $" <div class=\"d-sm-flex \">\r\n    <p>{client}</p>\r\n </div><div class=\"d-sm-flex\" id=\"borderr\">\r\n        " +
            $"<p></p>\r\n        <p>{description}</p>\r\n      </div>\r\n      <div ></div>\r\n\r\n      <table class=\"table table-bordered border-colors\" id=\"paddingg\">\r\n     " +
            $"   <thead>\r\n          <tr>\r\n            <th scope=\"col\">№</th>\r\n            <th scope=\"col\">{AppResources.NamESLabel}</th>\r\n          " +
            $"  <th scope=\"col\">{AppResources.PriceLabel}</th>\r\n            <th scope=\"col\">{AppResources.CountLabel_}</th>\r\n            <th scope=\"col\">{AppResources.SummLabel}</th>\r\n          </tr>\r\n        </thead>\r\n        <tbody>";
        int count = 1;
        double income = 0;
        var paynow = 0;
        var discount = $"{AppResources.DiscountLabel} ";
        var valuta = WalletTag;

        var price = 0.0;

        foreach (var i in TransactionItems)
        {

            if (OperationCode is Operation.Admission) price = i.FinalCost;
            else price = i.FinalPrice;

            double re = 0;

            if (i.Price > 0) re = i.Price;
            else re = i.ItemPrice;
            if (OperationCode is Operation.Admission) re = i.Cost;


            var builder = new StringBuilder();
            if (i.ItemId?.Description != null)
            {
                builder.Append("<br/>");
                builder.Append("(");
                builder.Append(i.ItemId.Description);
                builder.Append(")");

            }


            html += $"<tr class=\"zalupa\">\r\n            <th scope=\"row\">{count}</th>\r\n            <td>{i.ItemName}{builder.ToString()}</td>\r\n            <td>{re} {valuta}</td>\r\n            <td>{i.Count}</td>\r\n            <td>{price} {valuta}</td>\r\n          </tr>";
            count++;
            income += price;
        }
        foreach (var i in Payments)
        {
            paynow += ((int)i.Summ);
        }
        CreditPrice = "";

        string payNowStr = "";
        if (paynow > 0 && OperationCode is Operation.Sale) payNowStr = $"{AppResources.ContributedLabel}  {getNumber((int)paynow)} {valuta}";
        if (Price != paynow) CreditPrice = $"{AppResources.DutyLabel} {getNumber((int)(Price - paynow))}  {valuta} ";
        string totalStr = "";
        //-1
        if (OperationCode is Operation.Return) { CreditPrice = ""; totalStr = $"{AppResources.TotalLabel} {getNumber((int)income)} {valuta}"; }
        else totalStr = $"{AppResources.TotalLabel} {getNumber((int)income)} {valuta}";

        discount = "";


        if (Discount > 0 || Markup > 0)
        {
            discount = Markup > 0 ? $"{AppResources.MarkupLabel} {getNumber((int)Markup)} {valuta}" : $"{AppResources.OverDiscountLabel} {getNumber((int)Discount)} {valuta}";

        }

        string PriceStr = "";
        if (Price != 0 && OperationCode is not Operation.Admission && OperationCode is not Operation.Edit)
            PriceStr = $"{AppResources.InPayLabel} {getNumber((int)Price)} {valuta}";




        html += $"       </tbody>\r\n      </table>\r\n      <div class=\"d-sm-flex borderrd\">\r\n        <p>{totalStr}</p>\r\n           </div>\r\n        <div class=\"d-sm-flex \">\r\n        <p> {discount}</p>\r\n           </div>\r\n   <div class=\"d-sm-flex \">\r\n        <p>{PriceStr}</p>\r\n             </div>\r\n    <div class=\"d-sm-flex \">\r\n        <p>  {payNowStr}</p>\r\n             </div>\r\n  <div class=\"d-sm-flex \">\r\n        <p>{CreditPrice}</p>\r\n             </div>\r\n  <div>\r\n        <p style=\"text-align: center\">{AppResources.ThanksPayLabel}</p>\r\n      </div>\r\n         </div>  <div class=\"d-sm-flex\" id=\"mark\">\r\n            <p class=\"mark\">www.vendor.kz</p>\r\n          </div> </body></html>";

        return html;
    }
    private string getNumber(int number)
    {
        var temp = number + "";


        var tresult = "";
        var result = "";

        var tempCount = 0;
        for (int i = temp.Length - 1; i >= 0; i--)
        {

            if (tempCount % 3 == 0 && tempCount != 0)
            {
                tresult += " ";
                tresult += temp[i];


                tempCount++;
                continue;

            }
            tresult += temp[i];
            tempCount++;
        }

        for (int i = tresult.Length - 1; i >= 0; i--)
        {
            result += tresult[i];
        }

        return result;
    }
    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == nameof(StateCodeRaw))
        {
            RaisePropertyChanged(nameof(StateCode));
        }
    }
}

