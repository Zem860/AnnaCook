using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RecipeTest.Models;
using RecipeTest.Pages;
using static RecipeTest.Controllers.HomeController;

namespace RecipeTest.Controllers
{
    public class RecipeController : ApiController
    {
        private Model1 db = new Model1();
        [HttpGet]
        [Route("api/recipes")]
        public IHttpActionResult getAllRecipe()
        {
            var recipesWithPhotos = db.Recipe
             .GroupJoin(
                 db.RecipePhotos,
                 recipe => recipe.Id,         // 連接主鍵 (Recipe.Id)
                 photo => photo.RecipeId,     // 連接外鍵 (RecipePhotos.RecipeId)
                 (recipe, photos) => new RecipeCard
                 {
                     RecipeName = recipe.RecipeName,
                     CoverPhoto = photos.FirstOrDefault(p => p.IsCover == true).ImgUrl, // ✅ 正確過濾封面圖片
                     RecipeIntro = recipe.RecipeIntro,
                     CookingTime = recipe.CookingTime.ToString(),
                     Portion = recipe.Portion,
                     Rating = recipe.Rating,
                 });
            return Ok(recipesWithPhotos);
        }

        [HttpPost]
        [Route("api/recipe/add")]
        public IHttpActionResult addRecipe(Recipe recipe)
        {
            db.Recipe.Add(recipe);
            db.SaveChanges();
            return Ok();
        }

    }
}
