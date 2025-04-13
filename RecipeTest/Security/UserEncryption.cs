using System.Collections.Generic;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using static RecipeTest.Pages.UserRelated;
using System;
using MailKit.Net.Smtp; // ✅ 這個才是 MailKit 的 SmtpClient
using MimeKit;
using System.Net.Http;



namespace RecipeTest.Security
{
    public class UserEncryption
    {

        public string getClientIp()
        {
            var request = HttpContext.Current?.Request;
            var ip = request?.ServerVariables["X-Forwarded-For"];
            if (string.IsNullOrEmpty(ip))
            {
                ip = request?.UserHostAddress;
            }
            if (!string.IsNullOrEmpty(ip) && ip.Contains(","))
            {
                ip=ip.Split(',')[0].Trim();
            }
            return ip ?? "unkenown";
        } 
        public bool IsSelf(string token, string displayId)
        {
            try
            {
                var payload = JwtAuthUtil.GetPayload(token);
                return payload != null && payload.ContainsKey("DisplayId") && payload["DisplayId"].ToString() == displayId;
            }
            catch
            {
                return false;
            }
        }



        public void sendResetPasswordEmail(string toEmail, string name, string resetLink)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("AnnaCook", "onlyforwork12345678910@gmail.com"));
            message.To.Add(new MailboxAddress(name, toEmail));
            message.Subject = "重設密碼連結";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $@"
        <h2>Hi {name}，請點擊下方連結重設密碼：</h2>
        <p><a href='{resetLink}'>點我重設密碼</a></p>
        <p>此連結將於 5 分鐘後失效，請儘速完成操作。</p>
    ";
            message.Body = bodyBuilder.ToMessageBody();

            try
            {
                using (var client = new SmtpClient())
                {
                    client.CheckCertificateRevocation = false;
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("onlyforwork12345678910@gmail.com", "zrzduqmtncilnplz");
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("寄信失敗：" + ex.Message);
            }
        }

        public void sendVerifyEmail(string toEmail, string name, string verifyLink)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("AnnaCook", "onlyforwork12345678910@gmail.com"));
            message.To.Add(new MailboxAddress(name, toEmail));
            message.Subject = "請驗證您的帳號";
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $@"
        <h2>Hi {name},</h2>
        <p>請點擊下方連結完成您的帳號驗證：</p>
        <a href='{verifyLink}'>{verifyLink}</a>
        <p>此連結 24 小時內有效。</p>
    ";
            message.Body = bodyBuilder.ToMessageBody();
            using (var client = new SmtpClient())
            {
                //有開防毒時需設定 false 關閉檢查
               
                client.CheckCertificateRevocation = false;
                //設定連線 gmail ("smtp Server", Port, SSL加密) 
                client.Connect("smtp.gmail.com", 587, false); // localhost 測試使用加密需先關閉 

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate("onlyforwork12345678910@gmail.com", "zrzduqmtncilnplz");
                //發信
                client.Send(message);
                //結束連線
                client.Disconnect(true);
            }
        }
        public string GetRawTokenFromHeader()
        {
            var authHeader = HttpContext.Current.Request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }
            return authHeader.Replace("Bearer ", "").Trim();
        }

        public UserDto GetUserFromJWT()
        //後端從前端的bearer取出token資料做驗證以及取出使用者資料
        {
            var payload = HttpContext.Current.Items["jwtUser"] as Dictionary<string, object>;

            return new UserDto
            {
                Id = Convert.ToInt32(payload["Id"]),
                AccountEmail = payload["AccountEmail"].ToString(),
                DisplayId = payload["DisplayId"].ToString(), // ✅ 加這行
                AccountName = payload["AccountName"].ToString(),
                ProfilePhoto = payload.ContainsKey("ProfilePhoto") ? payload["ProfilePhoto"]?.ToString() : null,
                Role = Convert.ToInt32(payload["Role"]),
                LoginProvider = Convert.ToInt32(payload["LoginProvider"])
            };

        }
        public byte[] createSalt()
        {
            var buffer = new byte[16];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(buffer);
            return buffer;
        }


        public byte[] HashPassword(string password, byte[] salt)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
            argon2.Salt = salt;
            argon2.DegreeOfParallelism = 8;
            argon2.Iterations = 4;
            argon2.MemorySize = 1024 * 1024;
            return argon2.GetBytes(16);
        }

        public bool loginSuccess(string account, string password, byte[] salt)
        {


            return false;
        }

    }
}