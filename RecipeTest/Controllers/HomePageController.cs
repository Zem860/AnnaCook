using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Ajax.Utilities;
using RecipeTest.Models;

namespace RecipeTest.Controllers
{
    public class HomePageController : ApiController
    {
        RecipeModel db = new RecipeModel();
        //首頁get下方以排序

        [HttpGet]
        [Route("api/home/recipes")]
        public IHttpActionResult GetRecipes(string type = "latest", int page = 1)
        {
            const int pageSize = 5;
            var query = db.Recipes.AsQueryable();
            int skip = ((page - 1) * pageSize);
            query = query.Where(r => r.IsPublished == true); // 只選擇已發布的食譜

            switch (type.ToLower())
            {
                case "latest":
                    query = query.OrderByDescending(r => r.CreatedAt);
                    break;
                case "popular":
                    query = query.OrderByDescending(r => r.Rating);
                    break;
                case "classic":
                    query = query.OrderBy(r => r.CreatedAt).ThenByDescending(r => r.Rating);
                    break;
                default:
                    return BadRequest("不支援type參數");

            }

            int totalCount = query.Count();
            bool hasMore = page * pageSize < totalCount; // 判斷是否還有下一頁
            var result = query.Skip(skip).Take(pageSize).Select(r => new
            {
                id = r.Id,
                recipeName = r.RecipeName,
                coverPhoto = r.RecipesPhotos.FirstOrDefault(p=>p.IsCover).ImgUrl,
                description = r.RecipeIntro,
                portion = r.Portion,
                cookingTime = r.CookingTime,
                rating = r.Rating,
            }).ToList();

            var res = new
            {
                StatusCode = 200,
                msg = "獲取推薦動態食譜卡片",
                totalCount = totalCount,
                hasMore = hasMore,
                data = result,
            };
            return Ok(res);
        }


     
    }
}


