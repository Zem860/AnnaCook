using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
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
        [Route("api/recipeCards")]
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
            bool gotData = recipesWithPhotos != null;

            var res = new
            {
                StatusCode = gotData ? "200" : "404",
                msg = gotData ? "success" : "not found",
                data = recipesWithPhotos,
            };
            return Ok(res);
        }

        [HttpPost]
        [Route("api/recipe/add")]
        public IHttpActionResult addRecipe(RecipePlaceholder recipe)
        {
            if (recipe == null || string.IsNullOrEmpty(recipe.RecipeName))
            {
                return BadRequest("食譜名稱為必填欄位");
            }

            using (var transaction = db.Database.BeginTransaction()) // ✅ 開啟 Transaction
            {
                try
                {
                    // 1️⃣ 新增 `Recipe`
                    Recipe newRecipe = new Recipe
                    {
                        RecipeName = recipe.RecipeName,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    db.Recipe.Add(newRecipe);
                    db.SaveChanges(); // ✅ 先確保 `Recipe` 成功存入，獲得 `Id`

                    // **檢查是否成功取得 `Recipe.Id`**
                    if (newRecipe.Id == 0)
                    {
                        throw new Exception("無法取得新食譜的 ID，請檢查資料庫");
                    }

                    // 2️⃣ 新增 `RecipePhotos` (若有封面圖片)
                    if (!string.IsNullOrEmpty(recipe.CoverPhoto))
                    {
                        RecipePhotos newRecipeCoverPhoto = new RecipePhotos
                        {
                            RecipeId = newRecipe.Id, // ✅ 確保 `RecipeId` 正確
                            ImgUrl = recipe.CoverPhoto,
                            IsCover = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        db.RecipePhotos.Add(newRecipeCoverPhoto);
                        db.SaveChanges();
                    }

                    // 3️⃣ **所有操作成功，提交 Transaction**
                    transaction.Commit();
                    return Ok(new { message = "食譜新增成功", RecipeId = newRecipe.Id });
                }
                catch (Exception ex)
                {
                    // ❌ 若有錯誤，則回滾 (Rollback) 變更
                    transaction.Rollback();
                    return InternalServerError(new Exception($"資料存入失敗: {ex.Message}"));
                }
            }
        }

    }
}
