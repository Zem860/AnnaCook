using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            return Ok(new { message = "Ok" });
        }

        [HttpGet]
        [Route("api/check")]
        [JwtAuthFilter]
        public IHttpActionResult Check()
        {
            var payload = (Dictionary<string, object>)Request.Properties["JwtPayload"];
            var user = new UserRelated.UserTokenData
            {
                Id = (int)payload["Id"],
                DisplayId = payload["DisplayId"].ToString(),
                AccountEmail = payload["AccountEmail"].ToString(),
                AccountName = payload["AccountName"].ToString(),
                ProfilePhoto = payload.ContainsKey("ProfilePhoto") ? payload["ProfilePhoto"].ToString() : null,
                Role = Convert.ToInt32(payload["Role"]),
                LoginProvider = (int)(LoginProvider)Convert.ToInt32(payload["LoginProvider"])
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
