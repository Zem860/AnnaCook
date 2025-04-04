using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RecipeTest.Models;
using RecipeTest.Pages;
using RecipeTest.Security;
using RecipeTest.Enums;
using System.Configuration;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web;
using System.Text;
using MailKit.Net.Smtp;
using MailKit;
using Jose;
using System.Web.Configuration;
using MyWebApiProject.Security;
using Org.BouncyCastle.Asn1.Ocsp;
using static RecipeTest.Pages.UserRelated;


namespace RecipeTest.Controllers
{
    public class RegisterLoginController : ApiController
    {
        private RecipeModel db = new RecipeModel();
        private JwtAuthUtil jwt = new JwtAuthUtil();
        private UserEncryption userhash = new UserEncryption();
        private string clientId = ConfigurationManager.AppSettings["GoogleClient"];
        private string clientSecret = ConfigurationManager.AppSettings["GoogleClientSecret"];
        private string redirectUri = ConfigurationManager.AppSettings["GoogleRedirectUrl"];
        private string verifyUrl = ConfigurationManager.AppSettings["VerifyAPI"];
        
        [HttpGet]
        [Route("api/auth/ok")]
        public IHttpActionResult OkDesu()
        {
            return Ok("ok");
        }
        //傳統註冊
        [HttpPost]
        [Route("api/auth/register")]
        public IHttpActionResult LocalRegister(UserRelated.ClientRegisterData request)
        {
        var userExist = db.Users.FirstOrDefault(u => u.AccountEmail == request.AccountEmail);
            bool alreadyRegistered = userExist != null;
            if (!alreadyRegistered)
            {
                string password = request.Password;
                byte[] salt = userhash.createSalt();
                string stringSalt = Convert.ToBase64String(salt);
                byte[] hashedPassword = userhash.HashPassword(password, salt);
                string stringHashedPassword = Convert.ToBase64String(hashedPassword);
                int count = db.Users.Count(u => u.UserRole == UserRoles.User);
                string displayId = "M" + (count + 1).ToString("D6");//最小寬度 6 位數，不足補 0 在前面U000001
                var payload = new Dictionary<string, object>
                    {
                        { "email", request.AccountEmail },
                        { "exp", DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds() }
                    };

                string verifyToken = JWT.Encode(payload, Encoding.UTF8.GetBytes("kiwifruit"), JwsAlgorithm.HS256);

                var user = new Users
                {
                    DisplayId = displayId,
                    AccountEmail = request.AccountEmail,
                    AccountName = request.AccountName,
                    PasswordHash = stringHashedPassword,
                    Salt = stringSalt,
                    UserRole = UserRoles.User,
                    LoginProvider = LoginProvider.Local,
                    IsVerified = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };
                db.Users.Add(user);
                db.SaveChanges();

                string verifyLink = $"{verifyUrl}?token={verifyToken}";
                userhash.sendVerifyEmail(user.AccountEmail, user.AccountName, verifyLink);
            }

            var res = alreadyRegistered
                    ? new { StatusCode = 400, msg = "此帳號已註冊" }
                    : new { StatusCode = 200, msg = "註冊成功，請至信箱點擊驗證連結完成啟用" };

            return Ok(res);
        }

        //使用者點擊email認證連結可以讓註冊成功
        [HttpGet]
        [Route("api/auth/verifyEmail")]
        public IHttpActionResult VerifyEmail(string token)
        {
            try
            {// 解密 token
                var payload = JWT.Decode<Dictionary<string, object>>(
                    token,
                    Encoding.UTF8.GetBytes("kiwifruit"), // 你 JWT 的金鑰要一致
                    JwsAlgorithm.HS256
                );

                // 解析 email 和過期時間
                string email = payload["email"].ToString();
                long exp = Convert.ToInt64(payload["exp"]);
                var expireTime = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                if (DateTime.UtcNow > expireTime)
                {
                    return Ok(new { StatusCode = 400, msg = "驗證連結已過期請重新註冊" });
                }
                var user = db.Users.FirstOrDefault(u => u.AccountEmail == email);
                if (user == null)
                {
                    return Ok(new { StatusCode = 404, msg = "使用者不存在。" });
                }
                if (user.IsVerified)
                {
                    return Ok(new { StatusCode = 200, msg = "帳號已經被驗證過了!" });
                }

                user.IsVerified = true;
                db.SaveChanges();

                return Ok(new { StatusCode = 200, msg = "驗證成功你現在可以登入了" });
            } catch (Exception ex) {
                return Ok(new { StatusCode = 400, msg = ex });

            }

        }

        //==============google register================
        [HttpGet]
        [Route("api/auth/google/auth")]
        public IHttpActionResult googleAuth()
        {
            string googleAuthUrl = $"https://accounts.google.com/o/oauth2/v2/auth" +
                            $"?client_id={clientId}" +
                            $"&redirect_uri={redirectUri}" +
                            $"&response_type=code" +
                            $"&scope=openid%20email%20profile" +
                            $"&access_type=offline" +
                            $"&prompt=consent";

            return Redirect(googleAuthUrl);
        }

        [HttpGet]
        [Route("api/auth/google/callback")]
        public async Task<IHttpActionResult> googleRegister(string code)
        {
            Users user = new Users(); // ✅ 把 user 拉到外面
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("未收到授權碼");
            }
            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "code", code},
                { "client_id", clientId},
                { "client_secret", clientSecret},
                { "redirect_uri", redirectUri},
                { "grant_type", "authorization_code"},
            });
                var response = await client.PostAsync("https://oauth2.googleapis.com/token", content);
                var result = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("google token error");
                }

                dynamic tokenData = JsonConvert.DeserializeObject(result);
                string accessToken = tokenData.access_token;
                string refreshToken = tokenData.refresh_token;
                if (string.IsNullOrEmpty(accessToken))
                    return BadRequest("access_token 為空");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                var userInfoResponse = await client.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
                var userInfoJson = await userInfoResponse.Content.ReadAsStringAsync();
                if (!userInfoResponse.IsSuccessStatusCode)
                {
                    return BadRequest("google user info error");
                }

                dynamic userInfo = JsonConvert.DeserializeObject(userInfoJson);
                string email = userInfo.email;
                string name = userInfo.name;
                string picture = userInfo.picture;
                var dbuser = db.Users.FirstOrDefault(u => u.AccountEmail == email && u.LoginProvider == LoginProvider.Google);
                if (dbuser == null)
                {
                    int count = db.Users.Count(u => u.UserRole == UserRoles.User);
                    string displayId = "M" + (count + 1).ToString("D6");

                    // 建立一組亂數密碼（雖然不會使用）
                    var salt = userhash.createSalt();
                    var stringSalt = Convert.ToBase64String(salt);
                    var dummyPassword = "GoogleUser123!";
                    var hash = userhash.HashPassword(dummyPassword, salt);
                    var stringHash = Convert.ToBase64String(hash);

                    user.DisplayId = displayId;
                    user.AccountEmail = email;
                    user.AccountName = name;
                    user.AccountProfilePhoto = picture;
                    user.PasswordHash = stringHash;
                    user.Salt = stringSalt;
                    user.LoginProvider = LoginProvider.Google;
                    user.UserRole = UserRoles.User;
                    user.IsVerified = true;
                    user.CreatedAt = DateTime.Now;
                    user.UpdatedAt = DateTime.Now;
                    db.Users.Add(user);
                    db.SaveChanges();
                    if (user.Id == 0)
                    {
                        user = db.Users.FirstOrDefault(u => u.AccountEmail == email && u.LoginProvider == LoginProvider.Google);
                    }
                }
                else
                {
                    user = dbuser;
                }
            }

            var jwt = new JwtAuthUtil();
            var userToken = new UserRelated.UserTokenData();
            userToken.Id = user.Id;
            userToken.DisplayId = user.DisplayId;
            userToken.AccountEmail = user.AccountEmail;
            userToken.AccountName = user.AccountName;
            userToken.ProfilePhoto = user.AccountProfilePhoto;
            userToken.Role = (int)user.UserRole;

            string token = jwt.GenerateToken(userToken);

            string frontendCallback = ConfigurationManager.AppSettings["FrontendGoogleLogin"];
            string userJson = JsonConvert.SerializeObject(userToken);

            // ✅ 將 user JSON 做 Base64（避免中文亂碼）
            // 若有 emoji、中文會亂掉，因此建議轉成 base64，對安全也更好
            string base64User = Convert.ToBase64String(Encoding.UTF8.GetBytes(userJson));

            // ✅ 把 token 和 base64User 安全傳給前端
            return Redirect($"{frontendCallback}?token={token}&user={base64User}");

            //return Ok(new
            //{
            //    StatusCode = 200,
            //    msg = "Google 登入成功",
            //    token = token
            //});

        }

        [HttpPost]
        [Route("api/auth/login")]
        public IHttpActionResult login([FromBody] UserRelated.UserLoginData request)
        {
            var user = db.Users.FirstOrDefault(u => u.AccountEmail == request.AccountEmail && u.IsVerified == true);
            Console.Write(user.IsVerified);
            if (user == null)
            {
                var resError = new
                {
                    StatusCode = 400,
                    msg = "使用者不存在"
                };
                return Ok(resError);
            }
            var salt = Convert.FromBase64String(user.Salt);
            var hash = userhash.HashPassword(request.Password, salt);
            var stringHash = Convert.ToBase64String(hash);
            if (user.PasswordHash != stringHash)
            {
                var resError = new
                {
                    StatusCode = 400,
                    msg = "密碼錯誤"
                };
                return Ok(resError);
            }
            var userToken = new UserTokenData
            {
                Id = user.Id,
                AccountEmail = user.AccountEmail,
                AccountName = user.AccountName,
                DisplayId = user.DisplayId,
            };
            var token = jwt.GenerateToken(userToken);
            var userData = new
            {
                userId = user.Id,
                userDisplayId = user.DisplayId,
                accountEmail = user.AccountEmail,
                accountName = user.AccountName,
                profilePhoto = user.AccountProfilePhoto,
                Role = (int)user.UserRole,
            };
            var res = new
            {
                StatusCode = 200,
                msg = "登入成功",
                token = token,
                userData = userData,
            };
            return Ok(res);
        }
        //---------------------------------------------------------------------------
        [HttpPost]
        [Route("api/auth/forgot-password")]
        public IHttpActionResult SendForgetPasswordEmail(ForgotPasswordRequest request)
        {
            var user = db.Users.FirstOrDefault(u => u.AccountEmail == request.AccountEmail && u.IsVerified && u.LoginProvider== (int)LoginProvider.Local);
            if (user ==null)
            {
                return Ok(new { StatusCode = 404, msg = "找不到使用者" });
            }
            var payload = new Dictionary<string, object>
            {
                {"email",request.AccountEmail },
                { "exp",DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeMilliseconds()}
            };
            string token = JWT.Encode(payload, Encoding.UTF8.GetBytes("kiwifruit"), JwsAlgorithm.HS256);
            string frontedLink = ConfigurationManager.AppSettings["FrontendResetPassword"];
            string resetLink = $"{frontedLink}?token={token}";
            userhash.sendResetPasswordEmail(user.AccountEmail, user.AccountName, resetLink);
            return Ok(new { StatusCode = 200, msg = "已寄出重設密碼連結，請至信箱查收" });
                }
        [HttpPost]
        [Route("api/auth/reset-password")]
        public IHttpActionResult ResetPassword(ResetPasswordData data)
        {
            try {
                var payload = JWT.Decode<Dictionary<string, object>>(data.Token, Encoding.UTF8.GetBytes("kiwifruit"), JwsAlgorithm.HS256);
                string email = payload["email"].ToString();
                long exp = Convert.ToInt64(payload["exp"]);
                if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > exp)
                {
                    return Ok(new { StatusCode = 400, msg = "連結已過期，請重新申請" });
                }
                var user = db.Users.FirstOrDefault(u => u.AccountEmail == email);
                bool hasUser = user != null;
                if (!hasUser)
                {
                    return Ok(new { StatusCode = 404, msg = "找不到使用者" });

                }
                byte[] newSalt = userhash.createSalt();
                byte[] newHash = userhash.HashPassword(data.NewPassword, newSalt);

                user.Salt = Convert.ToBase64String(newSalt);
                user.PasswordHash = Convert.ToBase64String(newHash);
                user.UpdatedAt = DateTime.Now;

                db.SaveChanges();

                return Ok(new { StatusCode = 200, msg = "密碼重設成功，請重新登入" });

            }
            catch (Exception ex) {
                return Ok(new { StatusCode = 400, msg = "連結無效或過期" });
            };
        }

    }
}
