using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RecipeTest.Models;

namespace RecipeTest.Controllers
{
    public class AdController : ApiController
    {
        private RecipeModel db = new RecipeModel();
        [HttpGet]
        [Route("api/ad")]
        public IHttpActionResult GetAd(string location = "home")
        {
            return Ok();
        }

    }
}
