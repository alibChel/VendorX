
using DevExpress.XamarinForms.Core.Filtering.Helpers;
using Rg.Plugins.Popup.Services;
using Xamarin.CommunityToolkit.Converters;
using VendorX.Resources;
using Xamarin.Forms.Internals;
using Microcharts;
using SkiaSharp;
using System.Diagnostics;
using Vendor.Models;
using P42.Utils;

namespace Vendor.ViewModels;

public partial class AnaliticsViewModel : BaseViewModel
{
    private Realm realm;

    DateTime endDate = new DateTime(DateTime.Now.Ticks, DateTimeKind.Unspecified);
    DateTime startDate = new DateTime(DateTime.Now.Date.Ticks, DateTimeKind.Unspecified);

    // Высота страницы
    [ObservableProperty]
    private double conteinerHeigh;
    // Ширина страницы
    [ObservableProperty]
    private double conteinerWidth;

    [ObservableProperty]
    private string walletTag = "₸";

    [ObservableProperty]
    private double saleCount, returnCount, returnSumm, stockCount, priceSumm, profitability, checkCount, midlChek, midlPrice, midlCount, revenue, costSumm, profit, fullReturnCount;

    [ObservableProperty]
    bool isProfit;

    [ObservableProperty]
    private ChartEntry[] chartEntryArr, donatChartEntres, barChartEntres;

    [ObservableProperty]
    RadialGaugeChart incomeChart;
    [ObservableProperty]
    DonutChart donatChart;
    [ObservableProperty]
    BarChart coolChart;


    private Shop CurrentShop;
    private bool firstload;

    public DateTime EndDate { get => endDate; set { endDate = value; GetfullReport(); } }
    public DateTime StartDate { get => startDate; set { startDate = value; GetfullReport(); } }

    public AnaliticsViewModel()
    {

    }

    internal async Task OnApearing()
    {
        if (!firstload)
        {
            realm = RealmService.GetMainThreadRealm();
            CurrentShop = realm.Find<Shop>(CurrentShopID);
        }
        WalletTag = CurrentShop.WalletTag;
        firstload = true;
        GetfullReport();
        await Task.CompletedTask;
    }

    [RelayCommand]
    public async Task SalesReport()
    {
        IsBusy = true;

        DateTimeOffset _start = new DateTimeOffset(StartDate.Date, TimeSpan.Zero);
        DateTimeOffset _end = new DateTimeOffset(EndDate.Date.AddDays(1), TimeSpan.Zero);

        var items = realm.All<Models.Transaction>().Where(x => x.CreateDate >= _start
                                && x.CreateDate <= _end
                                && x.OwnerId == CurrentShopID);
        var list = items.ToList();
        var html = GetSalesReport(items, _start, _end);
        IsBusy = false;
        await DialogService.ShowHtmlAsync(html);
    }
    [RelayCommand]
    async Task ReturnReport()
    {
        IsBusy = true;

        DateTimeOffset _start = new DateTimeOffset(StartDate.Date, TimeSpan.Zero);
        DateTimeOffset _end = new DateTimeOffset(EndDate.Date.AddDays(1), TimeSpan.Zero);
        var items = realm.All<Models.Transaction>().Where(x => x.CreateDate >= _start
                                && x.CreateDate <= _end
                                && x.OwnerId == CurrentShopID);
        var list = items.ToList();
        var html = GetReturnReport(items, _start, _end);
        IsBusy = false;

        await DialogService.ShowHtmlAsync(html);
    }

    [RelayCommand]
    public async Task CommercialReport()
    {
        IsBusy = true;
        DateTimeOffset _start = new DateTimeOffset(StartDate.Date, TimeSpan.Zero);
        DateTimeOffset _end = new DateTimeOffset(EndDate.Date.AddDays(1), TimeSpan.Zero);
        var items = realm.All<Models.Transaction>().Where(x => x.CreateDate >= _start
                                && x.CreateDate <= _end
                                && x.OwnerId == CurrentShopID);

        var html = GetCommerctialReport(items, _start, _end);

        IsBusy = false;
        await DialogService.ShowHtmlAsync(html);
    }


    //public static async Task<Tuple<DateTime,DateTime>> ShowChoiceDate()
    //{
    //    var popup = new ChoiceDateView();
    //    await PopupNavigation.Instance.PushAsync(popup);
    //    return await popup.PopupClosedTask;
    //}

    [RelayCommand]
    private async Task StockReport()
    {
        var filter = "is_arhive != nil";
        var sort = "SORT(on_stock DESC)";

        IsBusy = true;
        var items = realm.All<Item>().Filter($"{filter} {sort}");
        var html = GenHtmlStockReport(items);
        IsBusy = false;
        await DialogService.ShowHtmlAsync(html);

    }


    [RelayCommand]
    private async Task PriceReport()
    {
        var filter = "is_arhive != nil";
        var sort = "SORT(on_stock DESC)";

        IsBusy = true;
        var items = realm.All<Item>().Filter($"{filter} {sort}");
        var html = GenHtmlPriceReport(items);
        IsBusy = false;
        await DialogService.ShowHtmlAsync(html);
    }

    private string GenHtmlStockReport(IQueryable<Item> items)
    {
        var count = 1;
        double all = 0;
        double allcost = 0;
        double allprice = 0;
        foreach (var item in items)
        {
            allcost += item.Cost * item.OnStock;
            allprice += item.Price * item.OnStock;
            all += item.OnStock;
        }

        var html = $"<!DOCTYPE html>\n<html lang=\"en\">\n<head>\n    <meta charset=\"UTF-8\">\n    <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n    <title>Vendor Report</title>\n    <style>\n   \ntable {{\n    display: table;\n    border-collapse: separate;\n    box-sizing: border-box;\n    text-indent: initial;\n    border-spacing: 2px;\n    border-color: gray;\n}}\n\n.table-bordered>:not(caption)>*>* {{\n    border-width:  1px;\n}}\n\n\n\n.table{{--bs-table-color:var(--bs-body-color);--bs-table-bg:transparent;--bs-table-border-color:var(--bs-border-color);--bs-table-accent-bg:transparent;--bs-table-striped-color:var(--bs-body-color);--bs-table-striped-bg:rgba(0, 0, 0, 0.05);--bs-table-active-color:var(--bs-body-color);--bs-table-active-bg:rgba(0, 0, 0, 0.1);--bs-table-hover-color:var(--bs-body-color);--bs-table-hover-bg:rgba(0, 0, 0, 0.075);width:100%;margin-bottom:1rem;color:var(--bs-table-color);vertical-align:top;border-color:var(--bs-table-border-color)}}.table>:not(caption)>*>*{{padding:.5rem .5rem;background-color:var(--bs-table-bg);border-bottom-width:1px;box-shadow:inset 0 0 0 9999px var(--bs-table-accent-bg)}}.table>tbody{{vertical-align:inherit}}.table>thead{{vertical-align:bottom}}\n\n\ntable {{\n    caption-side: bottom;\n    border-collapse: collapse;\n}}\n\ntbody, td, tfoot, th, thead, tr {{\n    border-color: inherit;\n    border-style: solid;\n    border-width: 1px;\n}} \n\n\n\nth {{\n    display: table-cell;\n    vertical-align: inherit;\n    font-weight: bold;\n    text-align: -internal-center;\n    text-align: inherit;\n    text-align: -webkit-match-parent;\n}}\n     @font-face {{\n          font-family: \"Gosu\";\n          src: url(\"'Radomir Tinkov - Gilroy-Bold.otf'\") format(\"opentype\");\n        }}\n      \n        .container-sm-sm {{\n          margin-right: auto;\n          margin-left: auto;\n        }}\n        \n        .container-sm-sm {{\n          margin-right: auto;\n          margin-left: auto;\n        }}\n        \n        .d-sm-flex {{\n          display: flex !important;\n        }}\n        \n        .d-sm-flex {{\n          justify-content: space-between;\n        }}\n        \n        .border-colors {{\n          border-color: #66b2f0;\n        }}\n        \n        .borderr {{\n          padding-bottom: 10px;\n          border-bottom: 2px dashed #66b2f0;\n          font-size: 25px;\n          color: #66b2f0;\n        }}\n        \n        .borderrd {{\n          border-bottom: 2px dashed #66b2f0;\n          font-size: 18px;\n        }}\n        \n        .size {{\n          margin-left: 72px;\n          width: 150px;\n        }}\n                 #mark {{            display: flex;            justify-content: center;            font-size: 25px;            color: #66B2F0        }}       \n      </style>\n      </head> <body>    <div class=\"container-sm-sm\">      <p class=\"border-bottom-2 borderr\" style=\"text-align: center\">{CurrentShop.Name}</p> <div class=\"d-sm-flex\" id=\"borderr\">      <p>{AppResources.TotalLabel}</p>      <p>{all} {AppResources.ShtLabel}</p>    </div>    <div class=\"d-sm-flex\" id=\"borderr\">      <p>{AppResources.AllSebesLabel}</p>      <p>{string.Format("{0:#,##0}", allcost)} {CurrentShop.WalletTag}</p>    </div>    <div class=\"d-sm-flex\" id=\"borderr\">      <p>{AppResources.AllPriceLabel}</p>      <p>{string.Format("{0:#,##0}", allprice)} {CurrentShop.WalletTag}</p>   </div>  <table class=\"table table-bordered border-colors\" id=\"paddingg\">        <thead>          <tr>            <th scope=\"col\">№</th>            <th scope=\"col\"></th>            <th scope=\"col\">{AppResources.Item}</th>  <th scope=\"col\">{AppResources.CountLabel}</th>          <th scope=\"col\">{AppResources.CostPriceLabel}</th>  <th scope=\"col\">{AppResources.PriceLabel}</th>  <th scope=\"col\">{AppResources.AllSebesLabel}</th> <th scope=\"col\">{AppResources.AllPriceLabel.RemoveLast()}</th>   </tr>        </thead>";



        if (items == null)
        {
            html = $"";
            return html;
        }
        foreach (var item in items)
        {
            if (item.IsArchive) continue;
            html += $"<tr>            <th scope=\"row\">{count}</th>  <td><img src=\"{item.PhotoUrl}\"width=\"100\" height=\"100\"></td>           <td>{item.Name}</td>                       <td>{item.OnStock}</td>  <td>{item.Cost}</td>   <td>{item.Price}</td>  <td>{item.Cost * item.OnStock}</td>  <td>{item.Price * item.OnStock}</td>  </tr>";
            count++;
        }
        html += $"        </tbody>      </table>         </div>            <div class=\"d-sm-flex\" id=\"mark\">            <p class=\"mark\">www.vendor.kz</p>          </div>     </body>\n</html>";
        return html;
    }
    private string GenHtmlPriceReport(IQueryable<Item> items)
    {
        var count = 1;

        var html = $"<!DOCTYPE html>\n<html lang=\"en\">\n<head>\n    <meta charset=\"UTF-8\">\n    <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n    <title>Vendor Report</title>\n    <style>\n    \ntable {{\n    display: table;\n    border-collapse: separate;\n    box-sizing: border-box;\n    text-indent: initial;\n    border-spacing: 2px;\n    border-color: gray;\n}}\n\n.table-bordered>:not(caption)>*>* {{\n    border-width:  1px;\n}}\n\n\n\n.table{{--bs-table-color:var(--bs-body-color);--bs-table-bg:transparent;--bs-table-border-color:var(--bs-border-color);--bs-table-accent-bg:transparent;--bs-table-striped-color:var(--bs-body-color);--bs-table-striped-bg:rgba(0, 0, 0, 0.05);--bs-table-active-color:var(--bs-body-color);--bs-table-active-bg:rgba(0, 0, 0, 0.1);--bs-table-hover-color:var(--bs-body-color);--bs-table-hover-bg:rgba(0, 0, 0, 0.075);width:100%;margin-bottom:1rem;color:var(--bs-table-color);vertical-align:top;border-color:var(--bs-table-border-color)}}.table>:not(caption)>*>*{{padding:.5rem .5rem;background-color:var(--bs-table-bg);border-bottom-width:1px;box-shadow:inset 0 0 0 9999px var(--bs-table-accent-bg)}}.table>tbody{{vertical-align:inherit}}.table>thead{{vertical-align:bottom}}\n\n\ntable {{\n    caption-side: bottom;\n    border-collapse: collapse;\n}}\n\ntbody, td, tfoot, th, thead, tr {{\n    border-color: inherit;\n    border-style: solid;\n    border-width: 1px;\n}} \n\n\n\nth {{\n    display: table-cell;\n    vertical-align: inherit;\n    font-weight: bold;\n    text-align: -internal-center;\n    text-align: inherit;\n    text-align: -webkit-match-parent;\n}}\n    @font-face {{\n          font-family: \"Gosu\";\n          src: url(\"'Radomir Tinkov - Gilroy-Bold.otf'\") format(\"opentype\");\n        }}\n      \n        .container-sm-sm {{\n          margin-right: auto;\n          margin-left: auto;\n        }}\n        \n        .container-sm-sm {{\n          margin-right: auto;\n          margin-left: auto;\n        }}\n        \n        .d-sm-flex {{\n          display: flex !important;\n        }}\n        \n        .d-sm-flex {{\n          justify-content: space-between;\n        }}\n        \n        .border-colors {{\n          border-color: #91dcdf;\n        }}\n        \n        .borderr {{\n          padding-bottom: 10px;\n          border-bottom: 2px dashed #91dcdf;\n          font-size: 25px;\n          color: #91dcdf;\n        }}\n        \n        .borderrd {{\n          border-bottom: 2px dashed #91dcdf;\n          font-size: 18px;\n        }}\n        \n        .size {{\n          margin-left: 72px;\n          width: 150px;\n        }}\n        \n      </style> </head> <body>    <div class=\"container-sm-sm\">      <p class=\"border-bottom-2 borderr\" style=\"text-align: center\">{CurrentShop.Name}</p>  " +
            $" <table class=\"table table-bordered border-colors\" id=\"paddingg\">        <thead>          <tr>            <th scope=\"col\">№</th>            <th scope=\"col\"></th>       " +
            $"     <th scope=\"col\">{AppResources.NamESLabel}</th>            <th scope=\"col\">{AppResources.PriceLabel}</th>       </tr>        </thead>";

        //var html = $"<!DOCTYPE html>\n<html lang=\"en\">\n<head>\n    <meta charset=\"UTF-8\">\n    <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n    <title>Vendor Report</title>\n    <style>\n    \ntable {{\n    display: table;\n    border-collapse: separate;\n    box-sizing: border-box;\n    text-indent: initial;\n    border-spacing: 2px;\n    border-color: gray;\n}}\n\n.table-bordered>:not(caption)>*>* {{\n    border-width:  1px;\n}}\n\n\n\n.table{{--bs-table-color:var(--bs-body-color);--bs-table-bg:transparent;--bs-table-border-color:var(--bs-border-color);--bs-table-accent-bg:transparent;--bs-table-striped-color:var(--bs-body-color);--bs-table-striped-bg:rgba(0, 0, 0, 0.05);--bs-table-active-color:var(--bs-body-color);--bs-table-active-bg:rgba(0, 0, 0, 0.1);--bs-table-hover-color:var(--bs-body-color);--bs-table-hover-bg:rgba(0, 0, 0, 0.075);width:100%;margin-bottom:1rem;color:var(--bs-table-color);vertical-align:top;border-color:var(--bs-table-border-color)}}.table>:not(caption)>*>*{{padding:.5rem .5rem;background-color:var(--bs-table-bg);border-bottom-width:1px;box-shadow:inset 0 0 0 9999px var(--bs-table-accent-bg)}}.table>tbody{{vertical-align:inherit}}.table>thead{{vertical-align:bottom}}\n\n\ntable {{\n    caption-side: bottom;\n    border-collapse: collapse;\n}}\n\ntbody, td, tfoot, th, thead, tr {{\n    border-color: inherit;\n    border-style: solid;\n    border-width: 1px;\n}} \n\n\n\nth {{\n    display: table-cell;\n    vertical-align: inherit;\n    font-weight: bold;\n    text-align: -internal-center;\n    text-align: inherit;\n    text-align: -webkit-match-parent;\n}}\n    @font-face {{\n          font-family: \"Gosu\";\n          src: url(\"'Radomir Tinkov - Gilroy-Bold.otf'\") format(\"opentype\");\n        }}\n      \n        .container-sm-sm {{\n          margin-right: auto;\n          margin-left: auto;\n        }}\n        \n        .container-sm-sm {{\n          margin-right: auto;\n          margin-left: auto;\n        }}\n        \n        .d-sm-flex {{\n          display: flex !important;\n        }}\n        \n        .d-sm-flex {{\n          justify-content: space-between;\n        }}\n        \n        .border-colors {{\n          border-color: #66b2f0;\n        }}\n        \n        .borderr {{\n          padding-bottom: 10px;\n          border-bottom: 2px dashed #66b2f0;\n          font-size: 25px;\n          color: #66b2f0;\n        }}\n        \n        .borderrd {{\n          border-bottom: 2px dashed #66b2f0;\n          font-size: 18px;\n        }}\n        \n        .size {{\n          margin-left: 72px;\n          width: 150px;\n        }}\n                   #mark {{            display: flex;            justify-content: center;            font-size: 25px;            color: #66B2F0        }}                   #mark {{            display: flex;            justify-content: center;            font-size: 25px;            color: #66B2F0        }}                  #mark {{            display: flex;            justify-content: center;            font-size: 25px;            color: #66B2F0        }} \n                  #mark {{            display: flex;            justify-content: center;            font-size: 25px;            color: #66B2F0        }}    </style> </head> <body>    <div class=\"container-sm-sm\">      <p class=\"border-bottom-2 borderr\" style=\"text-align: center\">{CurrentShop.Name}</p>   <table class=\"table table-bordered border-colors\" id=\"paddingg\">        <thead>          <tr>            <th scope=\"col\">№</th>            <th scope=\"col\"></th>            <th scope=\"col\">Наименование</th>            <th scope=\"col\">Цена</th>       </tr>        </thead>";


        if (items == null)
        {
            html = $"AppResources.NoItemsInStore";
            return html;
        }
        foreach (var item in items)
        {
            if (item.IsArchive) continue;
            html += $"<tr>            <th scope=\"row\">{count}</th>  <td><img src=\"{item.PhotoUrl}\"width=\"100\" height=\"100\"></td>           <td>{item.Name}</td>                       <td>{item.Price}</td>         </tr>";
            count++;
        }
        html += $" <div class=\"d-sm-flex\" id=\"mark\">            <p class=\"mark\">www.vendor.kz</p>          </div>";
        return html;
    }


    private string GetSalesReport(IQueryable<Models.Transaction> items, DateTimeOffset _start, DateTimeOffset _end)
    {
        double price = 0;
        var htmlitems = new List<Models.TransactionItem>();

        var html = $"<!DOCTYPE html>\n<html lang=\"en\">\n<head>\n    <meta charset=\"UTF-8\">\n    <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n    <title>Vendor Report</title>\n    <style>\n    \ntable {{\n    display: table;\n    border-collapse: separate;\n    box-sizing: border-box;\n    text-indent: initial;\n    border-spacing: 2px;\n    border-color: gray;\n}}\n\n.table-bordered>:not(caption)>*>* {{\n    border-width:  1px;\n}}\n\n\n\n.table{{--bs-table-color:var(--bs-body-color);--bs-table-bg:transparent;--bs-table-border-color:var(--bs-border-color);--bs-table-accent-bg:transparent;--bs-table-striped-color:var(--bs-body-color);--bs-table-striped-bg:rgba(0, 0, 0, 0.05);--bs-table-active-color:var(--bs-body-color);--bs-table-active-bg:rgba(0, 0, 0, 0.1);--bs-table-hover-color:var(--bs-body-color);--bs-table-hover-bg:rgba(0, 0, 0, 0.075);width:100%;margin-bottom:1rem;color:var(--bs-table-color);vertical-align:top;border-color:var(--bs-table-border-color)}}.table>:not(caption)>*>*{{padding:.5rem .5rem;background-color:var(--bs-table-bg);border-bottom-width:1px;box-shadow:inset 0 0 0 9999px var(--bs-table-accent-bg)}}.table>tbody{{vertical-align:inherit}}.table>thead{{vertical-align:bottom}}\n\n\ntable {{\n    caption-side: bottom;\n    border-collapse: collapse;\n}}\n\ntbody, td, tfoot, th, thead, tr {{\n    border-color: inherit;\n    border-style: solid;\n    border-width: 1px;\n}} \n\n\n\nth {{\n    display: table-cell;\n    vertical-align: inherit;\n    font-weight: bold;\n    text-align: -internal-center;\n    text-align: inherit;\n    text-align: -webkit-match-parent;\n}}\n    @font-face {{\n          font-family: \"Gosu\";\n          src: url(\"'Radomir Tinkov - Gilroy-Bold.otf'\") format(\"opentype\");\n        }}\n      \n        .container-sm-sm {{\n          margin-right: auto;\n          margin-left: auto;\n        }}\n        \n        .container-sm-sm {{\n          margin-right: auto;\n          margin-left: auto;\n        }}\n        \n        .d-sm-flex {{\n          display: flex !important;\n        }}\n        \n        .d-sm-flex {{\n          justify-content: space-between;\n        }}\n        \n        .border-colors {{\n          border-color: #66b2f0;\n        }}\n        \n        .borderr {{\n          padding-bottom: 10px;\n          border-bottom: 2px dashed #66b2f0;\n          font-size: 25px;\n          color: #66b2f0;\n        }}\n        \n        .borderrd {{\n          border-bottom: 2px dashed #66b2f0;\n          font-size: 18px;\n        }}\n        \n        .size {{\n          margin-left: 72px;\n          width: 150px;\n        }}\n          #mark {{            display: flex;            justify-content: center;            font-size: 25px;            color: #66B2F0        }}      \n      </style>\n      </head>  <body>  <div class=\"container-sm-sm\">    <p style=\"text-align: center; font-size: 20px\"><b>{AppResources.ReportSalesLabel}</p>    <p class=\"border-bottom-2 borderr\" style=\"text-align: center\">{CurrentShop.Name}</p>    <div class=\"d-sm-flex border-bottom-2 borderrd\">      <p>{AppResources.PeriodLabel}</p>      <p>        {_start.ToString("dd.MM.yyyy")} <br />        {_end.AddDays(-1).ToString("dd.MM.yyyy")}      </p>    </div>";




        html += $"    <div class=\"d-sm-flex border-bottom-2 borderrd\" id=\"borderr\">      <p>{AppResources.Sales}</p>      <p>{string.Format("{0:#,##0}", PriceSumm)} {CurrentShop.WalletTag}</p>    </div>  </div>   ";


        foreach (var item in items)
        {

            if (item.OperationCode == Models.Operation.Sale)
            {
                foreach (var trItem in item.TransactionItems)
                {
                    htmlitems.Add(trItem);
                }
            }
        }

        //    var htmlitems = items
        //.Where(item => item.OperationCode == Models.Operation.Sale)
        //.SelectMany(item => item.TransactionItems)
        //.ToList();

        foreach (var group in htmlitems.GroupBy(x => x.ItemId))
        {
            var item = group.First();
            var _count =
            html += $"     <div class=\"d-sm-flex\" id=\"borderr\">       <p>{item.ItemName}:</p>      <p> {group.Sum(x => x.Count)} {AppResources.ShtLabel} / {string.Format("{0:#,##0}", group.Sum(x => x.Price))} {CurrentShop.WalletTag} </p>    </div>  </div>  ";
        }

        html += PriceSumm == 0 ? $"   <div class=\"d-sm-flex\" id=\"mark\">        <p class=\"mark\">www.vendor.kz</p>      </div>     </body>\n</html>" : $" <div class=\"d-sm-flex border-bottom-2 borderrd\" id=\"borderr\">      <p> </p>      <p></p>    </div>  </div>     <div class=\"d-sm-flex\" id=\"mark\">        <p class=\"mark\">www.vendor.kz</p>      </div>     </body>\n</html>";

        return html;
    }


    private string GetReturnReport(IQueryable<Models.Transaction> items, DateTimeOffset _start, DateTimeOffset _end)
    {
        var htmlitems = new List<Models.TransactionItem>();


        var html = $"<!DOCTYPE html>\n<html lang=\"en\">\n<head>\n    <meta charset=\"UTF-8\">\n    <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n    <title>Vendor Report</title>\n    <style>\n    \ntable {{\n    display: table;\n    border-collapse: separate;\n    box-sizing: border-box;\n    text-indent: initial;\n    border-spacing: 2px;\n    border-color: gray;\n}}\n\n.table-bordered>:not(caption)>*>* {{\n    border-width:  1px;\n}}\n\n\n\n.table{{--bs-table-color:var(--bs-body-color);--bs-table-bg:transparent;--bs-table-border-color:var(--bs-border-color);--bs-table-accent-bg:transparent;--bs-table-striped-color:var(--bs-body-color);--bs-table-striped-bg:rgba(0, 0, 0, 0.05);--bs-table-active-color:var(--bs-body-color);--bs-table-active-bg:rgba(0, 0, 0, 0.1);--bs-table-hover-color:var(--bs-body-color);--bs-table-hover-bg:rgba(0, 0, 0, 0.075);width:100%;margin-bottom:1rem;color:var(--bs-table-color);vertical-align:top;border-color:var(--bs-table-border-color)}}.table>:not(caption)>*>*{{padding:.5rem .5rem;background-color:var(--bs-table-bg);border-bottom-width:1px;box-shadow:inset 0 0 0 9999px var(--bs-table-accent-bg)}}.table>tbody{{vertical-align:inherit}}.table>thead{{vertical-align:bottom}}\n\n\ntable {{\n    caption-side: bottom;\n    border-collapse: collapse;\n}}\n\ntbody, td, tfoot, th, thead, tr {{\n    border-color: inherit;\n    border-style: solid;\n    border-width: 1px;\n}} \n\n\n\nth {{\n    display: table-cell;\n    vertical-align: inherit;\n    font-weight: bold;\n    text-align: -internal-center;\n    text-align: inherit;\n    text-align: -webkit-match-parent;\n}}\n    @font-face {{\n          font-family: \"Gosu\";\n          src: url(\"'Radomir Tinkov - Gilroy-Bold.otf'\") format(\"opentype\");\n        }}\n      \n        .container-sm-sm {{\n          margin-right: auto;\n          margin-left: auto;\n        }}\n        \n        .container-sm-sm {{\n          margin-right: auto;\n          margin-left: auto;\n        }}\n        \n        .d-sm-flex {{\n          display: flex !important;\n        }}\n        \n        .d-sm-flex {{\n          justify-content: space-between;\n        }}\n        \n        .border-colors {{\n          border-color: #66b2f0;\n        }}\n        \n        .borderr {{\n          padding-bottom: 10px;\n          border-bottom: 2px dashed #66b2f0;\n          font-size: 25px;\n          color: #66b2f0;\n        }}\n        \n        .borderrd {{\n          border-bottom: 2px dashed #66b2f0;\n          font-size: 18px;\n        }}\n        \n        .size {{\n          margin-left: 72px;\n          width: 150px;\n        }}\n          #mark {{            display: flex;            justify-content: center;            font-size: 25px;            color: #66B2F0        }}      \n      </style>\n      </head>  <body>  <div class=\"container-sm-sm\">    <p style=\"text-align: center; font-size: 20px\"><b>{AppResources.ReportRefundsLabel}</p>    <p class=\"border-bottom-2 borderr\" style=\"text-align: center\">{CurrentShop.Name}</p>    <div class=\"d-sm-flex border-bottom-2 borderrd\">      <p>{AppResources.PeriodLabel}</p>      <p>         {_start.ToString("dd.MM.yyyy")}  <br />        {_end.AddDays(-1).ToString("dd.MM.yyyy")}      </p>    </div>";


        html += $"   <div class=\"d-sm-flex border-bottom-2 borderrd\" id=\"borderr\">     </div>   <div class=\"d-sm-flex border-bottom-2 borderrd\" id=\"borderr\">      <p>{AppResources.Refund}</p>      <p>{string.Format("{0:#,##0}", ReturnSumm)} {CurrentShop.WalletTag}</p>    </div>  </div>    ";

        foreach (var item in items)

        {

            if (item.OperationCode == Models.Operation.Return)
            {
                foreach (var trItem in item.TransactionItems)
                {
                    htmlitems.Add(trItem);
                }
            }
        }


        //    var htmlitems = items
        //.Where(item => item.OperationCode == Models.Operation.Sale)
        //.SelectMany(item => item.TransactionItems)
        //.ToList();

        foreach (var group in htmlitems.GroupBy(x => x.ItemId))
        {
            var item = group.First();

            html += $"     <div class=\"d-sm-flex\" id=\"borderr\">       <p>{item.ItemName}:</p>      <p> {group.Sum(x => x.Count)} {AppResources.ShtLabel} / {string.Format("{0:#,##0}", group.Sum(x => x.Price))} {CurrentShop.WalletTag}</p>    </div>  </div>  ";
        }


        html += ReturnSumm == 0 ? $"   <div class=\"d-sm-flex\" id=\"mark\">        <p class=\"mark\">www.vendor.kz</p>      </div>     </body>\n</html>" : $" <div class=\"d-sm-flex border-bottom-2 borderrd\" id=\"borderr\">      <p> </p>      <p></p>    </div>  </div>     <div class=\"d-sm-flex\" id=\"mark\">        <p class=\"mark\">www.vendor.kz</p>      </div>     </body>\n</html>";



        return html;
    }

    private string GetCommerctialReport(IQueryable<Models.Transaction> items, DateTimeOffset _start, DateTimeOffset _end)
    {


        var html = $"<!DOCTYPE html>\n<html lang=\"en\">\n<head>\n    <meta charset=\"UTF-8\">\n    <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n    <title>Vendor Report</title>\n    <style>\n    \ntable {{\n    display: table;\n    border-collapse: separate;\n    box-sizing: border-box;\n    text-indent: initial;\n    border-spacing: 2px;\n    border-color: gray;\n}}\n\n.table-bordered>:not(caption)>*>* {{\n    border-width:  1px;\n}}\n\n\n\n.table{{--bs-table-color:var(--bs-body-color);--bs-table-bg:transparent;--bs-table-border-color:var(--bs-border-color);--bs-table-accent-bg:transparent;--bs-table-striped-color:var(--bs-body-color);--bs-table-striped-bg:rgba(0, 0, 0, 0.05);--bs-table-active-color:var(--bs-body-color);--bs-table-active-bg:rgba(0, 0, 0, 0.1);--bs-table-hover-color:var(--bs-body-color);--bs-table-hover-bg:rgba(0, 0, 0, 0.075);width:100%;margin-bottom:1rem;color:var(--bs-table-color);vertical-align:top;border-color:var(--bs-table-border-color)}}.table>:not(caption)>*>*{{padding:.5rem .5rem;background-color:var(--bs-table-bg);border-bottom-width:1px;box-shadow:inset 0 0 0 9999px var(--bs-table-accent-bg)}}.table>tbody{{vertical-align:inherit}}.table>thead{{vertical-align:bottom}}\n\n\ntable {{\n    caption-side: bottom;\n    border-collapse: collapse;\n}}\n\ntbody, td, tfoot, th, thead, tr {{\n    border-color: inherit;\n    border-style: solid;\n    border-width: 1px;\n}} \n\n\n\nth {{\n    display: table-cell;\n    vertical-align: inherit;\n    font-weight: bold;\n    text-align: -internal-center;\n    text-align: inherit;\n    text-align: -webkit-match-parent;\n}}\n    @font-face {{\n          font-family: \"Gosu\";\n          src: url(\"'Radomir Tinkov - Gilroy-Bold.otf'\") format(\"opentype\");\n        }}\n      \n        .container-sm-sm {{\n          margin-right: auto;\n          margin-left: auto;\n        }}\n        \n        .container-sm-sm {{\n          margin-right: auto;\n          margin-left: auto;\n        }}\n        \n        .d-sm-flex {{\n          display: flex !important;\n        }}\n        \n        .d-sm-flex {{\n          justify-content: space-between;\n        }}\n        \n        .border-colors {{\n          border-color: #66b2f0;\n        }}\n        \n        .borderr {{\n          padding-bottom: 10px;\n          border-bottom: 2px dashed #66b2f0;\n          font-size: 25px;\n          color: #66b2f0;\n        }}\n        \n        .borderrd {{\n          border-bottom: 2px dashed #66b2f0;\n          font-size: 18px;\n        }}\n        \n        .size {{\n          margin-left: 72px;\n          width: 150px;\n        }}\n             #mark {{            display: flex;            justify-content: center;            font-size: 25px;            color: #66B2F0        }}   \n      </style>\n      </head>     <div class=\"container-sm-sm\">    <p style=\"text-align: center; font-size: 20px\"><b>{AppResources.ReportComercialLabel}</p>    <p class=\"border-bottom-2 borderr\" style=\"text-align: center\">{CurrentShop.Name}</p>    <div class=\"d-sm-flex border-bottom-2 borderrd\">      <p>{AppResources.PeriodLabel}</p>      <p>        {_start.ToString("dd.MM.yyyy")} <br />        {_end.AddDays(-1).ToString("dd.MM.yyyy")}      </p>    </div>";

        if (IsProfit)
        {
            html += $"<div class=\"d-sm-flex\" id=\"borderr\">      <p>{AppResources.Sales}</p>      <p>{string.Format("{0:#,##0}", PriceSumm)} {CurrentShop.WalletTag}</p>    </div> <div class=\"d-sm-flex\" id=\"borderr\">      <p>{AppResources.Refund}</p>      <p>{string.Format("{0:#,##0}", ReturnSumm)} {CurrentShop.WalletTag}</p>    </div> <div class=\"d-sm-flex\" id=\"borderr\">      <p>{AppResources.Revenue}</p>      <p>{string.Format("{0:#,##0}", Revenue)} {CurrentShop.WalletTag}</p>    </div> <div class=\"d-sm-flex\" id=\"borderr\">      <p>{AppResources.CostPriceLabel}:</p>      <p>{string.Format("{0:#,##0}", CostSumm)} {CurrentShop.WalletTag}</p>    </div>   <div class=\"d-sm-flex\" id=\"borderr\">      <p>{AppResources.Profit}</p>      <p>{string.Format("{0:#,##0}", Profit)} {CurrentShop.WalletTag}</p>    </div>         " +
                $"<div class=\"d-sm-flex\" id=\"borderr\">      <p>{AppResources.ProfitabilityLabel}</p>      <p>{string.Format("{0:#,##0}", Profitability)} %</p>    </div>    <div class=\"d-sm-flex\" id=\"borderr\">      <p>{AppResources.CheckCount}</p>      <p>{CheckCount} {AppResources.ShtLabel}</p>    </div>    <div class=\"d-sm-flex\" id=\"borderr\">  " +
                $"  <p>{AppResources.AverageCheck}</p>      <p>{string.Format("{0:#,##0}", CheckCount == 0 ? 0 : MidlChek)} {CurrentShop.WalletTag}</p>    </div>    <div class=\"d-sm-flex\" id=\"borderr\">   " +
                $"  <p>{AppResources.AverageCountOfItems}</p>       <p>{string.Format("{0:#,##0}", SaleCount == 0 ? 0 : MidlCount)} {AppResources.ShtLabel}</p>    </div>    <div class=\"d-sm-flex border-bottom-2 borderrd\" id=\"borderr\">    " +
                $"  <p>{AppResources.AveragePriceOfItem}</p>        <p>{string.Format("{0:#,##0}", SaleCount == 0 ? 0 : MidlPrice)} {CurrentShop.WalletTag}</p>    </div>     <div class=\"d-sm-flex\" id=\"mark\">        <p class=\"mark\">www.vendor.kz</p>      </div>  </div>";
        }
        else
        {
            html += $"<div class=\"d-sm-flex\" id=\"borderr\">      <p>{AppResources.Sales}</p>      <p>{string.Format("{0:#,##0}", PriceSumm)} {CurrentShop.WalletTag}</p>    </div> <div class=\"d-sm-flex\" id=\"borderr\">      <p>{AppResources.Refund}</p>      <p>{string.Format("{0:#,##0}", ReturnSumm)} {CurrentShop.WalletTag}</p>    </div> <div class=\"d-sm-flex\" id=\"borderr\">      <p>{AppResources.Revenue}</p>      <p>{string.Format("{0:#,##0}", Revenue)} {CurrentShop.WalletTag}</p>    </div> <div class=\"d-sm-flex\" id=\"borderr\">      <p>{AppResources.CostPriceLabel}:</p>      <p>{string.Format("{0:#,##0}", CostSumm)} {CurrentShop.WalletTag}</p>    </div>   <div class=\"d-sm-flex\" id=\"borderr\">      <p>{AppResources.Loss}</p>      <p>{string.Format("{0:#,##0}", Profit)} {CurrentShop.WalletTag}</p>    </div>         " +
                $"<div class=\"d-sm-flex\" id=\"borderr\">      <p>{AppResources.ProfitabilityLabel}</p>      <p>{string.Format("{0:#,##0}", Profitability)} %</p>    </div>    <div class=\"d-sm-flex\" id=\"borderr\">      <p>{AppResources.CheckCount}</p>      <p>{CheckCount} {AppResources.ShtLabel}</p>    </div>    <div class=\"d-sm-flex\" id=\"borderr\">  " +
                $"  <p>{AppResources.AverageCheck}</p>              <p>{string.Format("{0:#,##0}", MidlChek)} {CurrentShop.WalletTag}</p>    </div>    <div class=\"d-sm-flex\" id=\"borderr\">   " +
                $"  <p>{AppResources.AverageCountOfItems}</p>       <p>{string.Format("{0:#,##0}", MidlCount)} {AppResources.ShtLabel}</p>    </div>    <div class=\"d-sm-flex border-bottom-2 borderrd\" id=\"borderr\">    " +
                $"  <p>{AppResources.AveragePriceOfItem}</p>        <p>{string.Format("{0:#,##0}", MidlPrice)} {CurrentShop.WalletTag}</p>    </div>     <div class=\"d-sm-flex\" id=\"mark\">        <p class=\"mark\">www.vendor.kz</p>      </div> </div>";
        }
        return html;
    }


    public void GetfullReport()
    {

        PriceSumm = 0;          // Сумма всех транзакций
        SaleCount = 0;          // Кол-во всех проданых айтэмов
        CostSumm = 0;           // Себестоимость всех проданых айтэмов
        ReturnCount = 0;        // Кол-во всех возвращённых айтэмов
        ReturnSumm = 0;         // Сумма всех проданых атэмов
        StockCount = 0;         // кол-во всех
        Revenue = 0;            // Выручка
        Profit = 0;             // Прибыль
        Profitability = 0;      // Рентабельность
        CheckCount = 0;         // кол-во всех чеков
        MidlChek = 0;           // Среднее Арифметическое всех чеков
        MidlPrice = 0;          // Среднее Арифметическое стоимости всех проданых товаров
        FullReturnCount = 0;    // Сумма операций возвратов

        //IsBusy = true;

        /* Получение всех данных */
        DateTimeOffset _start = new DateTimeOffset(StartDate.Date, TimeSpan.Zero);
        DateTimeOffset _end = new DateTimeOffset(EndDate.Date.AddDays(1), TimeSpan.Zero);

        var items = realm.All<Models.Transaction>().Where(x => x.CreateDate >= _start
                               && x.CreateDate <= _end
                               && x.OwnerId == CurrentShopID);
        /* Получение всех данных */



        foreach (var item in items)
        {

            if (item.OperationCode == Models.Operation.Sale)
            {
                CheckCount++;
                PriceSumm += item.Price;
                SaleCount += item.TransactionItems.Sum(x => x.Count);
                CostSumm += item.TransactionItems.Sum(x => x.FinalCost);
            }
            if (item.OperationCode == Operation.Return)
            {
                ReturnCount += item.TransactionItems.Sum(x => x.Count);
                ReturnSumm += item.Price;
                CostSumm -= item.TransactionItems.Sum(x => x.FinalCost);

                FullReturnCount += (item.StateCode == Models.TransactionState.PartiallyReturn) ? 0 : 1;

            }
        }

        Revenue = PriceSumm - ReturnSumm;
        Profit = PriceSumm - ReturnSumm - CostSumm;

        if (Revenue == 0)
            Profitability = 0;
        else
            Profitability = Math.Round((Profit / Revenue) * 100, 2);

        if (CheckCount - FullReturnCount == 0)
            MidlChek = 0;
        else
            MidlChek = Math.Round(Revenue / (CheckCount - FullReturnCount), 2);

        if (SaleCount == 0)
            MidlPrice = 0;

        else
            MidlPrice = Math.Round(PriceSumm / SaleCount, 2);

        if (CheckCount == 0)
            MidlCount = 0;
        else
            MidlCount = Math.Round(SaleCount / CheckCount, 2);



        IsProfit = (Profit >= 0);  // для Отоброжение прибыли/убытка 

        // = Profit >= 0 ? Profit : Profit * -1;


        var filter = "is_arhive != nil";
        var sort = "SORT(on_stock DESC)";

        var _stock_items = realm.All<Item>().Filter($"{filter} {sort}");
        foreach (var item in _stock_items)
        {
            StockCount += item.OnStock;
        }

        UpdateCharts();

        //IsBusy = false;
    }

    private void UpdateCharts()
    {
        ChartEntryArr = new[]
        {

            new ChartEntry((float)CostSumm)
            {
                Label = $"{AppResources.CostPriceLabel}",
                ValueLabel = CostSumm.ToString(),
                ValueLabelColor = SKColor.Parse("#F7B548"),
                Color = SKColor.Parse("#F7B548"),
            },
             new ChartEntry((float)ReturnSumm)
            {
                Label = $"{AppResources.RefundLabel}",
                ValueLabel = ReturnSumm.ToString(),
                ValueLabelColor = SKColor.Parse("#964747"),
                Color = SKColor.Parse("#964747"),
            },

            new ChartEntry((float)Profit)
            {
                Label = IsProfit? $"{AppResources.ProfitLabel}":$"{AppResources.LossLabel}",
                ValueLabel = Profit.ToString(),
                ValueLabelColor = IsProfit? SKColor.Parse("#479649"):SKColor.Parse("#ff0000"),
                Color = IsProfit? SKColor.Parse("#479649"):SKColor.Parse("#ff0000"),



            },
            new ChartEntry((float)PriceSumm)
            {
                Label = $"{AppResources.SalesLabel}",
                ValueLabel = PriceSumm.ToString(),
                ValueLabelColor = SKColor.Parse("#66B2F0"),
                Color = SKColor.Parse("#66B2F0"),

            }
        };

        BarChartEntres = ReturnSumm == 0 ?
            new[]
            {
                 new ChartEntry((float)PriceSumm)
                {
                    Label = $"{AppResources.SalesLabel}",
                    ValueLabel = PriceSumm.ToString(),
                    ValueLabelColor = SKColor.Parse("#66B2F0"),
                    Color = SKColor.Parse("#66B2F0"),

                },
                new ChartEntry((float)CostSumm)
                {
                    Label = $"{AppResources.CostPriceLabel}",
                    ValueLabel = CostSumm.ToString(),
                    ValueLabelColor = SKColor.Parse("#F7B548"),
                    Color = SKColor.Parse("#F7B548"),
                },
                new ChartEntry((float)Profit)
                {
                    Label = IsProfit?  $"{AppResources.ProfitLabel}":$"{AppResources.LossLabel}",
                    ValueLabel = Profit.ToString(),
                    ValueLabelColor = IsProfit? SKColor.Parse("#479649"):SKColor.Parse("#ff0000"),
                    Color = IsProfit? SKColor.Parse("#479649"):SKColor.Parse("#ff0000"),}

            }
        :/* это либо очень плохой код либо очень гениальный (Тернарная функция для отоброжения графика)*/
            new[]
            {
                 new ChartEntry((float)PriceSumm)
                {
                    Label = $"{AppResources.SalesLabel}",
                    ValueLabel = PriceSumm.ToString(),
                    ValueLabelColor = SKColor.Parse("#66B2F0"),
                    Color = SKColor.Parse("#66B2F0"),

                },
                 new ChartEntry((float)ReturnSumm)
                {
                    Label = $"{AppResources.RefundLabel}",
                    ValueLabel = ReturnSumm.ToString() ,
                    ValueLabelColor = SKColor.Parse("#964747"),
                    Color = SKColor.Parse("#964747"),
                },
                new ChartEntry((float)CostSumm)
                {
                    Label = $"{AppResources.CostPriceLabel}",
                    ValueLabel = CostSumm.ToString(),
                    ValueLabelColor = SKColor.Parse("#F7B548"),
                    Color = SKColor.Parse("#F7B548"),
                },
                new ChartEntry((float)Profit)
                {
                    Label = IsProfit?  $"{AppResources.ProfitLabel}" : $"{AppResources.LossLabel}",
                    ValueLabel = Profit.ToString(),
                    ValueLabelColor = IsProfit? SKColor.Parse("#479649"):SKColor.Parse("#ff0000"),
                    Color = IsProfit? SKColor.Parse("#479649"):SKColor.Parse("#ff0000"),}

            };

        DonatChartEntres = new[]
        {

            new ChartEntry((float)CostSumm)
            {
                Label = $"{AppResources.CostPriceLabel}",
                ValueLabel = CostSumm.ToString(),
                ValueLabelColor = SKColor.Parse("#F7B548"),
                Color = SKColor.Parse("#F7B548"),
            },
             new ChartEntry((float)ReturnSumm)
            {
                Label = $"{AppResources.RefundLabel}",
                ValueLabel = ReturnSumm.ToString(),
                ValueLabelColor = SKColor.Parse("#964747"),
                Color = SKColor.Parse("#964747"),
            },

            new ChartEntry((float)Profit)
            {
                Label = IsProfit?  $"{AppResources.ProfitLabel}" : $"{AppResources.LossLabel}",
                ValueLabel = Profit.ToString(),
                ValueLabelColor = IsProfit? SKColor.Parse("#479649"):SKColor.Parse("#ff0000"),
                Color = IsProfit? SKColor.Parse("#479649"):SKColor.Parse("#ff0000"),



            },

        };

        IncomeChart = new()
        {
            Entries = ChartEntryArr,
            LabelTextSize = 35,
            BackgroundColor = SKColor.Empty,
            LineSize = 19,
            //PointSize= 25,
            //LabelOrientation = Orientation.Horizontal

        };
        DonatChart = new()
        {
            Entries = DonatChartEntres,
            LabelTextSize = 29,
            BackgroundColor = SKColor.Empty,
            LabelMode = LabelMode.RightOnly,
            HoleRadius = 0,
            //LineSize = 23,
            //PointSize= 25,
            //LabelOrientation = Orientation.Horizontal

        };
        CoolChart = new()
        {
            Entries = BarChartEntres,
            LabelTextSize = 29,
            BackgroundColor = SKColor.Empty,
            ValueLabelOrientation = Orientation.Horizontal,
            //LineSize = 23,
            //PointSize= 25,
            LabelOrientation = Orientation.Horizontal

        };
    }


}
