using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using MyWebApiProject.Security;
using RecipeTest.Enums;
using RecipeTest.Pages;
using RecipeTest.Security;

namespace RecipeTest.Controllers
{
    public class OkController : ApiController
    {
        private UserEncryption userhash = new UserEncryption();

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
            var user = new UserRelated.UserTokenData
            {
                Id = (int)payload.Id,
                DisplayId = payload.DisplayId,
                AccountEmail = payload.AccountEmail,
                AccountName =payload.AccountName,
                ProfilePhoto = payload.ProfilePhoto,
                Role = Convert.ToInt32(payload.Role),
                LoginProvider = (int)(LoginProvider)Convert.ToInt32(payload.LoginProvider)
            };

            // ✅ 重新產生新 token
            var jwt = new JwtAuthUtil();
            var newToken = jwt.GenerateToken(user);

            return Ok(new
            {
                message = "Token refreshed",
                token = newToken
            });
        }
    }
}
