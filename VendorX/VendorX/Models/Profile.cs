using MongoDB.Bson;
using Realms;

namespace Vendor.Models;

public class Profile : RealmObject
{
    [PrimaryKey, Required, MapTo("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    // UserId => OwnerId
    [MapTo("owner_id"), Required]
    public string OwnerId { get; set; }

    [MapTo("login")]
    public string Login { get; set; }

    [MapTo("phone")]
    public string Phone { get; set; }

    [MapTo("last_name")]
    public string LastName { get; set; }

    [MapTo("first_name")]
    public string FirstName { get; set; }

    [MapTo("photo_url")]
    public string PhotoUrl { get; set; }

    //[MapTo("device_token")]
    //public string DeviceToken { get; set; }

    [Ignored]
    public string FullName { get => $"{FirstName} {LastName}"; }

}

