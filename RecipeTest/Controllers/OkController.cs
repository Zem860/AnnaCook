using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using MyWebApiProject.Security;
using Newtonsoft.Json;
using RecipeTest.Enums;
using RecipeTest.Models;
using RecipeTest.Pages;
using RecipeTest.Security;

namespace RecipeTest.Controllers
{
    public class OkController : ApiController
    {
        private UserEncryption userhash = new UserEncryption();
        private string vimeoAccessToken = ConfigurationManager.AppSettings["VimeoAccessToken"];
        private RecipeModel db = new RecipeModel();

        public class VimeoStatusResult
        {
            public UploadInfo upload { get; set; }
            public TranscodeInfo transcode { get; set; }

            public class UploadInfo
            {
                public string status { get; set; }
            }

            public class TranscodeInfo
            {
                public string status { get; set; }
            }
        }


        [HttpGet]
        [Route("api/checkvimeo")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> CheckAPI(string videoId)
        {
            var userhash = new UserEncryption();
            var user = userhash.GetUserFromJWT();
            var userData = db.Users.FirstOrDefault(u => u.Id == user.Id && !u.IsBanned && !u.IsDeleted && u.IsVerified && u.UserRole == UserRoles.User);
            bool hasUser = userData != null;
            if (!hasUser)
            {
                return Ok(new { StatusCode = 403, msg = "你沒有權限" });
            }
            string requestUrl = $"https://api.vimeo.com/videos/{videoId}?fields=uri,upload.status,transcode.status";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", vimeoAccessToken);

                try
                {
                    var response = await client.GetAsync(requestUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        return Ok(new { });
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<VimeoStatusResult>(content);

                    bool isWatchable = result.transcode?.status == "in_progress" ? false : true;
                    return Ok(new
                    {   StatusCode = 200,
                        msg = "獲取影片進度",
                        isWatchable = isWatchable
                    });
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }
            }
        }


        [HttpGet]
        [Route("api/Ok")]
        public IHttpActionResult SayOk()
        {
            string ip = userhash.getClientIp();
            return Ok(new { message = "Ok", ip = ip });
        }

        [HttpGet]
        [Route("api/check")]
        [JwtAuthFilter]
        public IHttpActionResult Check()
        {
            var payload = userhash.GetUserFromJWT();
            var userTokenData = new UserRelated.UserTokenData
            {
                Id = (int)payload.Id,
                DisplayId = payload.DisplayId,
                AccountEmail = payload.AccountEmail,
                AccountName =payload.AccountName,
                ProfilePhoto = payload.ProfilePhoto,
                Role = Convert.ToInt32(payload.Role),
                LoginProvider = (int)(LoginProvider)Convert.ToInt32(payload.LoginProvider)
            };

            var user = db.Users.FirstOrDefault(u => u.Id == payload.Id);
            var userData = new
            {
                id = user.Id,
                displayId = user.DisplayId,
                accountEmail = user.AccountEmail,
                accountName = user.AccountName,
                profilePhoto = user.AccountProfilePhoto,
                role = Convert.ToInt32(user.UserRole),
                loginProvider = (int)(LoginProvider)Convert.ToInt32(user.LoginProvider)
            };

            // ✅ 重新產生新 token
            var jwt = new JwtAuthUtil();
            var newToken = jwt.GenerateToken(userTokenData);
            return Ok(new
            {
                message = "Token refreshed",
                token = newToken,
                userData = userData,
            });
        }
    }
}
