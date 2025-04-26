using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Ajax.Utilities;
using MyWebApiProject.Security;
using Org.BouncyCastle.Bcpg.Sig;
using RecipeTest.Enums;
using RecipeTest.Models;
using RecipeTest.Pages;
using RecipeTest.Security;
using static RecipeTest.Pages.UserRelated;

namespace RecipeTest.Controllers
{
    public class HomePageController : ApiController
    {
        RecipeModel db = new RecipeModel();

        [HttpGet]
        [Route("api/home/features")] //ok
        public IHttpActionResult GetFeatures() {

            // 1. 撈出所有主題區塊
            var featuredList = db.FeatureSections
                                 .Where(f => f.IsActive)
                                 .OrderBy(f => f.SectionPos)
                                 .ToList();
            // 2. 將每個主題區塊的 tags 轉換成 List<string>
            var result = new List<object>();
            foreach (var f in featuredList)
            {
                var tags = f.FeaturedSectionTags.Split(',').Select(t => t.Trim().ToLower()).ToList();
                // 查符合的食譜（最多6道）
                var recipes = db.Recipes.Where(r => r.IsPublished && !r.IsDeleted && !r.IsArchived && r.RecipeTags.Any(rt => tags.Contains(rt.Tags.TagName.ToLower()))).OrderByDescending(r => r.CreatedAt)
                                .Take(6)
                                .Select(r => new
                                {
                                    recipeName = r.RecipeName,
                                    rating = r.Rating,
                                    coverPhoto = r.RecipesPhotos.FirstOrDefault(p => p.IsCover).ImgUrl,
                                    author = r.User.AccountName,
                                })
                             .ToList();
                result.Add(new
                {
                    sectionPos = f.SectionPos,
                    sectionName = f.FeaturedSectionName,
                    tags = tags,
                    recipes = recipes
                });
            }
            return Ok(new
            {
                StatusCode = 200,
                msg = "獲取推特色區態食譜卡片",
                data = result
            });


        }      

        [HttpGet]
        [Route("api/home/recipes")] //ok
        public IHttpActionResult GetRecipes(string type = "latest", int number = 1)
        {
            const int pageSize = 5;
            var query = db.Recipes.AsQueryable();
            int skip = ((number - 1) * pageSize);
            query = query.Where(r => r.IsPublished == true && !r.IsArchived && !r.IsDeleted); // 只選擇已發布的食譜

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
            bool hasMore = number * pageSize < totalCount; // 判斷是否還有下一頁
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


