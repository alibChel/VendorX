using System;
using MongoDB.Bson;
using Realms;

namespace Vendor.Models
{
    public class Invites : RealmObject
    {
        [PrimaryKey, MapTo("_id"), Required]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        //  id магазина владельца
        [MapTo("owner_id"), Required]
        public string OwnerId { get; set; }

        // login владельца магазина
        [MapTo("owner_login"), Required]
        public string OwnerName { get; set; }

        [MapTo("shop_name"), Required]
        public string ShopName { get; set; }

        // Логин приглащенного
        [MapTo("target_login"), Required, Indexed]
        public string TargetLogin { get; set; }
        [MapTo("target_id"), Required, Indexed]
        public string TargetId { get; set; }

        // Роль приглашения
        [MapTo("role"), Required]
        public string RoleRaw { get; set; }

        // Дата создания
        [MapTo("create_date")]
        public DateTimeOffset CreateDate { get; set; } = DateTimeOffset.Now.AddMinutes(DateTimeOffset.Now.Offset.TotalMinutes);

        [MapTo("state")]
        public string StateRaw { get; set; }

        [Ignored]
        public UserRole Role
        {
            get { return Enum.TryParse(RoleRaw, out UserRole result) ? result : UserRole.User; }
            set { RoleRaw = value.ToString(); }
        }
        [Ignored]
        public InviteState State
        {
            get { return Enum.TryParse(StateRaw, out InviteState result) ? result : InviteState.Nothing; }
            set { StateRaw = value.ToString(); }
        }
    }
}

