using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace VendorX.Models
{
    public class TokenResponse
    {
        public string access_token { get; set; }
    }
    public class UserData
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("verified_email")]
        public bool VerifedEmail { get; set; }

        [JsonProperty("given_name")]
        public string FirstName { get; set; }

        [JsonProperty("family_name")]
        public string LastName { get; set; }

        [JsonProperty("picture")]
        public string Image { get; set; }




    }
}

