using System;
namespace Vendor.Models;

public enum PaidStatus
{
    Paid,
    Partially,
    Credit
}

public enum Operation
{
    Sale,
    Admission,
    Return,
    Edit,
    Reserve
}

public enum TransactionState
{
    New,
    Paid,
    PartiallyPaid,
    Credit,
    PartiallyReturn,
    Return,
    Reserve
}
public enum AuthType
{
    Google,
    Facebook,
    Apple,
    Phone,
    Email
}
public enum OrderFilter
{
    All,
    InProcess,
    Delivered,
    Canceled,
    Shipped

}
public enum OrderStatus
{
    Pending,        // Ожидает обработки
    AcceptedByMerchant,//
    Processing,     // Обрабатывается
    Shipped,        // Отправлен
    Delivered,      // Доставлен
    Cancelled,       // Отменен
    Success, // Выдан
    Return,// возврат
    DeliveryReturned,// возврат  во время доставки
    Paid//оплачен

}

public enum ServiceType
{
    Vendor,
    Kaspi,
    Halyk,
    Jusan,
    WebMarket
}

public enum UserRole
{
    User,
    Manager,
    Owner
}

public enum ItemSort
{
    New,
    Min,
    Max,
    Hi,
    Low
}

public enum ItemFilter
{
    Actual,
    All,
    Archive
}

// Состаяния корзины используется на основной странице
/// <summary>
/// ShowCart - показывает все корзину
/// ShowCartThreshold - показывает мини корзину для добавления товаров
/// HideCart - корзина скрыта
/// </summary>
public enum CartStates
{
    ShowCart,
    ShowCartThreshold,
    HideCart
}

// Состаяния корзины используется на основной странице
/// <summary>
/// Show - показывает все фильтры и сортировку
/// Hide - скрывает все фильтры и сортировку
/// </summary>
public enum FilterStates 
{
    Show,
    Hide
}

// Состаяния корзины используется на основной странице
/// <summary>
/// Show - показывает камера скан
/// Hide - скрывает камера скан
/// </summary>
public enum CameraScanStates
{
    Show,
    Hide
}

public enum TransactionGroup
{
    GroupData,
    ContactName,
    ContactPhone,
    AutorName,
    CreatedTicks
}

public enum TransactionSort
{
    Ascending,
    Descending,
    None
}

public enum TransactionFilter
{
    All,
    Sale,
    Admission,
    Return,
    Edit,
    Paid,
    Unpaid,
    Period

}

public enum StockSortingNames
{
    Price,
    CreateDate,
}

public enum InviteState
{
    Nothing,
    Accept,
    Decline
}

public enum CurrentRegistration
{
    Email,
    Phone
}