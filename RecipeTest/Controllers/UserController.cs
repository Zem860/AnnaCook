using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RecipeTest.Models;
using RecipeTest.Pages;
using RecipeTest.Security;

namespace RecipeTest.Controllers
{
    public class UserController : ApiController
    {
        private JwtAuthUtil jwt = new JwtAuthUtil();

        [HttpPost]
        [Route("api/testjwt")]
        public IHttpActionResult getAllUserData([FromBody] UserRelated.UserTokenData request)
        {
            string token = jwt.GenerateToken(request);
            return Ok(token);
        }

        [HttpPost]
        [Route("api/users")]
        public IHttpActionResult createUser()
        {
            return Ok();
        }
    }
}
