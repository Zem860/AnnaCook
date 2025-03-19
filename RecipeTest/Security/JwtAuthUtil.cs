using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Web;
using System.Text;
using Jose;
using Microsoft.Ajax.Utilities;
using RecipeTest.Models;
using RecipeTest.Pages;
namespace RecipeTest.Security
{
    public class JwtAuthUtil
    {
        private static readonly string secretKey = "kiwifruit";
        public string GenerateToken(UserRelated.UserTokenData user)
        {
            var payload = new Dictionary<string, object>
            {
                { "Id", user.Id },
                { "Account", user.Account },
                { "AccountName", user.AccountName },
                { "Exp", DateTime.UtcNow.AddMinutes(3).ToString() } // 設定 Unix 時間戳
            };
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }

        public string RevokeToken()
        {
            string secretKey = "RevokeToken";
            var payload = new Dictionary<string, object>
            {
                { "Id", 0},
                { "Account", "None"},
                { "AccountName", "None"},
                {"Exp", DateTime.Now.AddDays(-15).ToString()  }
            };
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }

        public static Dictionary<string, object> GetPayload(string token)
        {
            return JWT.Decode<Dictionary<string, object>>(token, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
        }
    }
}