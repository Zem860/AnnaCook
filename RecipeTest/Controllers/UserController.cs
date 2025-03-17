using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RecipeTest.Controllers
{
    public class UserController : ApiController
    {
        [HttpGet]
        [Route("api/users")]
        public IHttpActionResult getAllUserData()
        {
            return Ok();
        }

        [HttpPost]
        [Route("api/users")]
        public IHttpActionResult createUser()
        {
            return Ok();
        }
    }
}
