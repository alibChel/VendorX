using System;
using System.Globalization;
using System.Linq;
using Vendor.Models;
using VendorX.Resources;
using Xamarin.Forms;

namespace Vendor.Helpers;

public class ImageSourceConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return "noimage";
        var imageUri = value?.ToString();
        if (imageUri.Contains("http"))
        {
            UriImageSource uriImageSource = new UriImageSource();
            uriImageSource.Uri = new Uri(imageUri);
            uriImageSource.CachingEnabled = true;
            return uriImageSource;
        }
        else
        {
            return imageUri;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ProfileImageSourceConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || string.IsNullOrWhiteSpace(value?.ToString()))
        {
            FontImageSource uriImageSource = new ();
            uriImageSource.FontFamily = "FAS";
            uriImageSource.Glyph = "\uf007";
            uriImageSource.Color = Color.FromHex("#66B2F0");
            return uriImageSource;
        }
            
        var imageUri = value?.ToString();
        if (imageUri.Contains("http"))
        {
            UriImageSource uriImageSource = new UriImageSource();
            uriImageSource.Uri = new Uri(imageUri);
            uriImageSource.CachingEnabled = true;
            return uriImageSource;
        }
        else
        {
            return imageUri;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ListCurrentBindConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {

            return ((List<CurrentRegistration>)value).Where(x => x.Equals(parameter)).FirstOrDefault() == null ? false : true ;




        }
        catch
        {
            return false;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DateTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var result = DateTime.MinValue;
        try
        {
            
            return new DateTime((long)value).ToString("dd.MM.yyyy"); 
           
        }
        catch
        {
            return value;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ShopImageSourceConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return "noimage";
        }
        else if (string.IsNullOrWhiteSpace(value.ToString()))
        {
            return "noimage";
        }

        var imageUri = value?.ToString();
        if (imageUri.Contains("http"))
        {
            UriImageSource uriImageSource = new UriImageSource();
            uriImageSource.Uri = new Uri(imageUri);
            uriImageSource.CachingEnabled = true;
            return uriImageSource;
        }
        else
        {
            return imageUri;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class UserRoleToTextConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return AppResources.SalesmanLabel;
            //"Продавец";//продавец
        }
        else if (string.IsNullOrWhiteSpace(value.ToString()))
        {
            return AppResources.SalesmanLabel;
            //"Продавец";//продавец

        }

        var role = (UserRole)value;
        if (role == UserRole.Owner)
        {
            return AppResources.OwnerLabel;
            //"Владелец";//владелец
        }
        else if (role == UserRole.Manager)
        {
            return AppResources.ManagerLabel;
            //"Менеджер";// менендежер
        }
        else
            return AppResources.SalesmanLabel;
        //"Продавец";// продавец
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class UsedPriceNotEqualPriceConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return false;
        }

        var item = (TransactionItem)value;
        if (item.Price != item.ItemPrice)
        {
            return true;
        }
        else
            return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class TransactionStatusColorConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var status = (Models.TransactionState)value;
        var result = Color.FromHex("#66B2F0");
        switch (status)
        {
            case Models.TransactionState.Paid:
                // Зеленый
                result = Color.FromHex("#479649");
                break;
            case Models.TransactionState.PartiallyPaid:
                // Синий
                result = Color.FromHex("#2c82ec");
                break;
            case Models.TransactionState.Credit:
                // Оранжевый
                result = Color.FromHex("#F7B548");
                break;
            case Models.TransactionState.PartiallyReturn:
                // темнооранжевый
                result = result = Color.FromHex("#F94A29");
                break;
            case Models.TransactionState.Return:
                // Красный
                result = result = Color.FromHex("#964747");
                break; 
            
            case Models.TransactionState.Reserve:
                // светло фиолетовый
                result = result = Color.FromHex("#7B68EE");
                break;


        }
        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ReturnTransactionCountConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null|| value is not List<TransactionItem> || parameter is not TransactionItem)
            return 0;

        var returnlist = (List<TransactionItem>)value;
        var result = 0;

        var transactionitem = (TransactionItem)parameter;
        result = returnlist.Where(x => x.ItemId == transactionitem.ItemId).Sum(x => x.Count);

        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
