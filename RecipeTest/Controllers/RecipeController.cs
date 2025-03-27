using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using RecipeTest.Models;
using static RecipeTest.Pages.RecipeRelated;

namespace RecipeTest.Controllers
{
    public class RecipeController : ApiController
    {
        private RecipeModel db = new RecipeModel();
        //[HttpGet]
        //[Route("api/recipes")]
        //public IHttpActionResult getAllRecipeData()
        //{
        //    var RecipesWithPhotos = db.Recipes
        //        .GroupJoin(
        //            db.RecipePhotos,              // 內表：RecipePhotos
        //            recipe => recipe.Id,          // 外鍵 (Recipe.Id)
        //            photo => photo.RecipeId,      // 內鍵 (RecipePhotos.RecipeId)
        //            (recipe, photos) => new RecipeCard      // 先處理 Recipe 與 RecipePhotos
        //            {
        //                RecipeId = recipe.Id,
        //                RecipeName = recipe.RecipeName,
        //                CoverPhoto = photos.FirstOrDefault(p => p.IsCover == true).ImgUrl,
        //                RecipeIntro = recipe.RecipeIntro,
        //                CookingTime = recipe.CookingTime.ToString(),
        //                Portion = recipe.Portion,
        //                Rating = recipe.Rating,
        //            }
        //        );

        //    var RecipesWithDetails = RecipesWithPhotos
        //        .GroupJoin(
        //            db.Ingredients,               // 內表：Ingredients
        //            r => r.RecipeId,             // 外鍵 (Recipe.Id)
        //            ingredient => ingredient.RecipeId, // 內鍵 (Ingredients.RecipeId)
        //            (recipeWithPhoto, ingredients) => new
        //            {
        //                Recipe = recipeWithPhoto,
        //                Ingredients = ingredients.ToList() // 轉換為 List
        //            }
        //        );
              

        //    return Ok(RecipesWithDetails);
        //}

        //[HttpGet]
        //[Route("api/recipes/{id}")]
        //public IHttpActionResult getOneRecipe()
        //{

        //    var RecipeWithCover = db.Recipes
        //        .GroupJoin(
        //        db.RecipePhotos,
        //        recipe => recipe.Id,
        //        photo => photo.RecipeId,
        //        (recipe, photo) => new RecipeCard
        //        {
        //            RecipeId = recipe.Id,
        //            RecipeName = recipe.RecipeName,
        //            CoverPhoto = photo.FirstOrDefault(p => p.IsCover == true).ImgUrl,
        //            RecipeIntro = recipe.RecipeIntro,
        //            CookingTime = recipe.CookingTime.ToString(),
        //            Portion = recipe.Portion,
        //            Rating = recipe.Rating
        //        }
        //        );
        //    var RecipeWithIngredient = RecipeWithCover
        //    .GroupJoin(
        //        db.Ingredients,
        //        r => r.RecipeId,
        //        ingredient => ingredient.RecipeId,
        //        (recipeAndCover, Ingredient) => new
        //        {
        //            recipe = recipeAndCover,

        //            Ingredients = Ingredient.Select(i=>new { 
        //               IngredientName = i.IngredientName,
        //               IngredientAmount = i.Amount,
        //               IngredientUnit = i.Unit,
        //               IngredientFlavoring = i.IsFlavoring

        //            })
        //        }

        //        );



        //    return Ok(RecipeWithIngredient);
        //}


        [HttpGet]
        [Route("api/recipeCards")]
        public IHttpActionResult getAllRecipe()
        {
            var recipesWithPhotos = db.Recipes
             .GroupJoin(
                 db.RecipePhotos,
                 recipe => recipe.Id,         // 連接主鍵 (Recipe.Id)
                 photo => photo.RecipeId,     // 連接外鍵 (RecipePhotos.RecipeId)
                 (recipe, photos) => new RecipeCard
                 {
                     RecipeId = recipe.Id,
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

//        [HttpPost]
//        [Route("api/recipe/add")]
//        public IHttpActionResult addRecipe(RecipePlaceholder recipe)
//        {
//            if (recipe == null || string.IsNullOrEmpty(recipe.RecipeName))
//            {
//                return BadRequest("食譜名稱為必填欄位");
//            }

//            using (var transaction = db.Database.BeginTransaction()) // ✅ 開啟 Transaction
//            {
//                try
//                {
//                    // 1️⃣ 新增 `Recipe`
//                    Recipes newRecipe = new Recipes
//                    {
//                        RecipeName = recipe.RecipeName,
//                        CreatedAt = DateTime.UtcNow,
//                        UpdatedAt = DateTime.UtcNow
//                    };
//                    Recipes recipes = new Recipes();
//                    recipes.RecipeName = recipe.RecipeName;
//                    recipes.CreatedAt = DateTime.Now;
//                    //-----------------------------------
//                    var a = new RecipePhotos
//                    {
//                        ImgUrl = recipe.CoverPhoto,
//                        IsCover = true,
//                        CreatedAt = DateTime.UtcNow,
//                        UpdatedAt = DateTime.UtcNow
//                    };
//                    List<RecipePhotos> photos = new List<RecipePhotos>();//空集合確保不會為null
//                    photos.Add(a);
//                    newRecipe.RecipesPhotos = photos;
//                    newRecipe.RecipesPhotos.Add(a)
//.                    //newRecipe.RecipesPhotos.Add(new RecipePhotos
//                    //{
//                    //    RecipeId = newRecipe.Id, // ✅ 確保 `RecipeId` 正確
//                    //    ImgUrl = recipe.CoverPhoto,
//                    //    IsCover = true,
//                    //    CreatedAt = DateTime.UtcNow,
//                    //    UpdatedAt = DateTime.UtcNow
//                    //});
//                    db.Recipes.Add(newRecipe);
//                    db.SaveChanges(); // ✅ 先確保 `Recipe` 成功存入，獲得 `Id`

//                    // **檢查是否成功取得 `Recipe.Id`**
//                    if (newRecipe.Id == 0)
//                    {
//                        throw new Exception("無法取得新食譜的 ID，請檢查資料庫");
//                    }

//                    // 2️⃣ 新增 `RecipePhotos` (若有封面圖片)
//                    if (!string.IsNullOrEmpty(recipe.CoverPhoto))
//                    {
//                        RecipePhotos newRecipeCoverPhoto = new RecipePhotos
//                        {
//                            RecipeId = newRecipe.Id, // ✅ 確保 `RecipeId` 正確
//                            ImgUrl = recipe.CoverPhoto,
//                            IsCover = true,
//                            CreatedAt = DateTime.UtcNow,
//                            UpdatedAt = DateTime.UtcNow
//                        };

//                        db.RecipePhotos.Add(newRecipeCoverPhoto);
//                        db.SaveChanges();
//                    }


//                    // 3️⃣ **所有操作成功，提交 Transaction**
//                    transaction.Commit();
//                    return Ok(new { message = "食譜新增成功", RecipeId = newRecipe.Id });
//                }
//                catch (Exception ex)
//                {
//                    // ❌ 若有錯誤，則回滾 (Rollback) 變更
//                    transaction.Rollback();
//                    return InternalServerError(new Exception($"資料存入失敗: {ex.Message}"));
//                }
//            }
//        }

    }
}
