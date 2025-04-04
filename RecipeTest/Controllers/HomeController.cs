using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RecipeTest.Models;

namespace RecipeTest.Controllers
{
    public class HomeController : Controller
    {
        private RecipeModel db = new RecipeModel();

      
        public ActionResult Index()
        {
      
                return View(); 
        }
    }
}
