using System;
using Xamarin.Forms;
using Transaction = Vendor.Models.Transaction;

namespace Vendor.Helpers;

public class TransactionDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate SaleTemplate { get; set; }
    public DataTemplate AdmissionTemplate { get; set; }
    public DataTemplate ReturnTemplate { get; set; }
    public DataTemplate EditTemplate { get; set; }
    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        var result = SaleTemplate;

        var transaction = (Transaction)item;

        switch (transaction.OperationCode)
        {
            case Operation.Sale:
                result = SaleTemplate;
                break;
            case Operation.Admission:
                result = AdmissionTemplate;
                break;
            case Operation.Return:
                result = ReturnTemplate;
                break;
            case Operation.Edit:
                result = EditTemplate;
                break;  
            
            case Operation.Reserve:
                result = SaleTemplate;
                break;
        }

        return result;
    }
}

