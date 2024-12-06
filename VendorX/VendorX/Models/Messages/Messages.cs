
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Vendor.Models.Messages;

// Сообшение о смене магазина
/// <summary>
/// подписаны - MainPageViewModel => очищает корзину
/// отправляет - ProfileViewModel
/// </summary>
public class CurrentShopChengetMessage : ValueChangedMessage<bool>
{
    public CurrentShopChengetMessage(bool value) : base(value)
    {
    }
}

// Сообшение для сканирования ШК
/// <summary>
/// подписаны - MainPageViewModel
/// отправляет -
/// </summary>
public class ItemBarcodeScanMessage : ValueChangedMessage<string>
{
    public ItemBarcodeScanMessage(string value) : base(value)
    {
    }
}
public class ShowNavBarMessage : ValueChangedMessage<bool>
{
    public ShowNavBarMessage(bool value) : base(value)
    {
    }
}
public class ChangeCurrentUser : ValueChangedMessage<bool>
{
    public ChangeCurrentUser(bool value) : base(value)
    {
    }
}
public class ItemChangeState : ValueChangedMessage<string>
{
    public ItemChangeState(string value) : base(value)
    {
    }
}
// Сообшение для изменения товаров в корзине
/// <summary>
/// подписаны - MainPageViewModel
/// отправляет - EditItemViewModel
/// </summary>
public class ItemInCartChangeMessage : ValueChangedMessage<double>
{
    public ItemInCartChangeMessage(double value) : base(value)
    {
    }
}