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
        public class ForgotPasswordRequest
        {
            public string AccountEmail { get; set; }
        }
        public class ResetPasswordData
        {
            public string Token { get; set; }
            public string NewPassword { get; set; }
        }
        public class UpdateUserDto
        {
            public string AccountName { get; set; }
            public string UserIntro { get; set; }
            public string ProfilePhoto { get; set; }  // 如果是圖片網址，這樣就可以；如果是檔案上傳要另外處理
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
        public class GoogleRegisterData
        {
            public string AccountEmail { get; set; }
            public string AccountName { get; set; }
            public string AccountProfilePhoto { get; set; }
        }

        public class UserTokenData
        {

            // JWT Token 資料拿去產token時的資料(JWTAuthUtil.cs)
            public int Id { get; set; }
            public string DisplayId { get; set; }
            public string AccountEmail { get; set; }
            public string AccountName { get; set; }
            public int Role { get; set; }
            public string ProfilePhoto { get; set; }  // ✅ 加這一行

            public int LoginProvider { get; set; }
            public string Exp { get; set; } 

        }



        public class UserDto
        {

            //解出前端token的資料用在(UserEncryption.cs)
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("displayId")]
            public string DisplayId { get; set; }

            [JsonProperty("accountEmail")]
            public string AccountEmail { get; set; }

            [JsonProperty("accountName")]
            public string AccountName { get; set; }

            [JsonProperty("profilePhoto")]
            public string ProfilePhoto { get; set; }

            [JsonProperty("role")]
            public int Role { get; set; } // 可選：傳 int，前端轉顯示（如 0=User, 1=Admin）

            [JsonProperty("loginProvider")]
            public int LoginProvider { get; set; } // 0=Local, 1=Google 等
        }

    }

}