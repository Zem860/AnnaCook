using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using RecipeTest.Models;

namespace RecipeTest.Pages
{

    public class UserRelated
    {
        public class UserDto
        {
            [JsonProperty("id")]
            public int Id { get; set; }
            [JsonProperty("accountEmail")]
            public string AccountEmail { get; set; }

            [JsonProperty("accountName")]
            public string AccountName { get; set; }
            [JsonProperty("profilePhoto")]
            public string ProfilePhoto { get; set; } // 加這個欄位

        }
        public class UserUpdateData
        {
            public int Id { get; set; }
            public string AccountName { get; set; }
            public string AccountEmail { get; set; }
        }

        public class UserLoginData
        {
            public string AccountEmail { get; set; }
            public string Password { get; set; }
        }

        public class ClientRegisterData
        {
            public int Id { get; set; }
            [JsonProperty("accountEmail")]
            public string AccountEmail { get; set; }
            [JsonProperty("accountName")]
            public string AccountName { get; set; }
            [JsonProperty("password")]
            public string Password { get; set; }   
        }

        public class UserRegisterData
        {
            public int Id { get; set; }
            public string AccountEmail { get; set; }
            public string AccountName { get; set; }
            public byte[] Password { get; set; }
            public byte[] Salt { get; set; }


        }


        public class UserTokenData
        {

            public int Id { get; set; }
            public string Account { get; set; }
            public string AccountName { get; set; }
            public string Exp { get; set; } 

        }
    }

}