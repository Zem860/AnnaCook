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
                { "DisplayId", user.DisplayId },
                { "AccountEmail", user.AccountEmail },
                { "AccountName", user.AccountName },
                { "Role", user.Role },
                { "LoginProvider", user.LoginProvider },
                { "Exp", DateTime.UtcNow.AddMinutes(30).ToString("o") }, // 建議設定長一點，例如 30 分
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

        /// <summary>
        /// 生成只刷新效期的 JwtToken
        /// </summary>
        /// <returns>JwtToken</returns>
        public string ExpRefreshToken(Dictionary<string, object> tokenData)
        {
            // payload 從原本 token 傳遞的資料沿用，並刷新效期，再api直接加入
            var payload = new Dictionary<string, object>
            {
                { "Id", (int)tokenData["Id"] },
                { "DisplayId", tokenData["DisplayId"] },
                { "AccountEmail", tokenData["AccountEmail"] },
                { "AccountName", tokenData["AccountName"] },
                { "Role", tokenData["Role"] },
                { "LoginProvider", tokenData["LoginProvider"] },
                { "Exp", DateTime.UtcNow.AddMinutes(30).ToString("o") }
            };

            //產生刷新時效的 JwtToken
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }

        public static Dictionary<string, object> GetPayload(string token)
        {
            return JWT.Decode<Dictionary<string, object>>(token, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
        }
    }
}