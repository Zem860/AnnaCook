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

        public class RecipeViewModel
        {
            public string RecipeName { get; set; }
            public List<string> Photos { get; set; }
            public string RecipeIntro { get; set; }
            public string CookingTime { get; set; }
            public decimal Portion {  get; set; }
            public string Rating { get; set; }
        }

        public ActionResult Index()
        {
            var recipesWithPhotos = db.Recipes
            .GroupJoin(
                db.RecipePhotos,
                recipe => recipe.Id,         // 連接主鍵 (Recipe.Id)
                photo => photo.RecipeId,     // 連接外鍵 (RecipePhotos.RecipeId)
                (recipe, photos) => new RecipeViewModel
                {
                    RecipeName = recipe.RecipeName,
                    Photos = photos.Where(p => p.IsCover).Select(p => p.ImgUrl).ToList(), // ✅ 正確過濾封面圖片
                    RecipeIntro = recipe.RecipeIntro,
                    CookingTime = recipe.CookingTime.ToString(),
                    Portion = recipe.Portion,
                    Rating = recipe.Rating.ToString(),
                    })
                    .ToList();
                ViewBag.Recipes = recipesWithPhotos;
                return View(recipesWithPhotos); 
        }
    }
}
