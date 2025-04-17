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
        private UserEncryption userhash = new UserEncryption();

        //首頁get下方以排序
        //Emma改設定了要改變
        [HttpPost]
        [Route("api/home/features")] 
        [JwtAuthFilter]
        public IHttpActionResult AddFeatures(RecipeRelated.FeatureCustomTags FeatureCustomTags)
        {
            var user = userhash.GetUserFromJWT();

            var userData = db.Users.FirstOrDefault(u => u.Id == user.Id && user.Role==(int)UserRoles.Admin);
            //檢查本使用者是否有權限問題
            if (userData == null)
            {
                return Ok(new { StatusCode = 401, msg = "使用者不是管理員" });
            }
            var statusCheck = userhash.GetUserStatusErrorMessage(userData);
            if (statusCheck != null)
            {
                return Ok(new { statusCode = 403, msg = statusCheck });
            }


            if (String.IsNullOrEmpty(FeatureCustomTags.SectionName))
            {
                return Ok(new { StatusCode = 401, msg = "需要特色名稱" });
            }
            if (FeatureCustomTags.CustomTags == null || !FeatureCustomTags.CustomTags.Any())
            {
                return Ok(new { StatusCode = 401, msg = "需要至少一個tag" });
            }

            var featureCount = db.FeatureSections.FirstOrDefault(f => f.IsActive && f.SectionPos == FeatureCustomTags.SectionPos);
            bool repeat = featureCount != null;
            if (repeat)
            {
                return Ok(new { StatusCode = 401, msg = "特色已存在" });
            }

            var features = new FeaturedSection();
            features.FeaturedSectionName = FeatureCustomTags.SectionName;
            features.SectionPos = FeatureCustomTags.SectionPos;
            features.FeaturedSectionTags = string.Join(",", FeatureCustomTags.CustomTags);
            features.IsActive = FeatureCustomTags.isActive;
            features.CreatedAt = DateTime.Now;
            features.UpdatedAt = DateTime.Now;
            db.FeatureSections.Add(features);
            db.SaveChanges();


            return Ok(new {StatusCode = 200, msg="新增主題成功", Id = features.Id });
        }

        [HttpGet]
        [Route("api/home/feature/{id}")]
        [JwtAuthFilter]
        public IHttpActionResult getAllFeature(int id)
        {
            //目前還未限制數量為了之後或許可擴充
            var user = userhash.GetUserFromJWT();

            var userData = db.Users.FirstOrDefault(u => u.Id == user.Id && user.Role == (int)UserRoles.Admin);
            //檢查本使用者是否有權限問題
            if (userData == null)
            {
                return Ok(new { StatusCode = 401, msg = "使用者不是管理員" });
            }
            var statusCheck = userhash.GetUserStatusErrorMessage(userData);
            if (statusCheck != null)
            {
                return Ok(new { statusCode = 403, msg = statusCheck });
            }

            // ✅ 這裡先撈出所有資料，不做任何 Split() 與字串處理
            var feature = db.FeatureSections.FirstOrDefault(f => f.Id == id);

            bool hasFeature = feature != null;
            if (!hasFeature)
            {

                return Ok(new { StatusCode = 400, msg = "找不到此特色分類" });

            }
            var result = new
            {
                id = feature.Id,
                sectionPos = feature.SectionPos,
                featuredSectionName = feature.FeaturedSectionName,
                featuredSectionTags = string.IsNullOrEmpty(feature.FeaturedSectionTags)
                    ? new List<string>()
                    : feature.FeaturedSectionTags.Split(',').Select(t => t.Trim()).ToList(),
                isActive = feature.IsActive,
                createdAt = feature.CreatedAt.ToString("yyyy/MM/dd"),
                updatedAt = feature.UpdatedAt.ToString("yyyy/MM/dd")
            };


            return Ok(new { StatusCode = 200, msg = "找到此特色分類", data = result });
        }


        [HttpGet]
        [Route("api/home/featuredList")]
        [JwtAuthFilter]
        public IHttpActionResult getAllFeature()
        {
            //目前還未限制數量為了之後或許可擴充
            var user = userhash.GetUserFromJWT();

            var userData = db.Users.FirstOrDefault(u => u.Id == user.Id && user.Role == (int)UserRoles.Admin);
            //檢查本使用者是否有權限問題
            if (userData == null)
            {
                return Ok(new { StatusCode = 401, msg = "使用者不是管理員" });
            }
            var statusCheck = userhash.GetUserStatusErrorMessage(userData);
            if (statusCheck != null)
            {
                return Ok(new { statusCode = 403, msg = statusCheck });
            }

            // ✅ 這裡先撈出所有資料，不做任何 Split() 與字串處理
            var rawData = db.FeatureSections.OrderBy(f => f.CreatedAt).ToList();

            // ✅ 再用 C# 處理格式轉換（此時已經不在 EF 查詢階段）
            var featureList = rawData.Select(f => new
            {
                id = f.Id,
                sectionPos = f.SectionPos,
                featuredSectionName = f.FeaturedSectionName,
                featuredSectionTags = string.IsNullOrEmpty(f.FeaturedSectionTags)
                    ? new List<string>()
                    : f.FeaturedSectionTags.Split(',').Select(t => t.Trim()).ToList(),
                isActive = f.IsActive,
                createdAt = f.CreatedAt.ToString("yyyy/MM/dd"),
                updatedAt = f.UpdatedAt.ToString("yyyy/MM/dd")
            }).ToList();
            bool hasFeature = featureList != null;
            if (!hasFeature)
            {

                return Ok(new { StatusCode = 400, msg = "找不到此特色分類" });

            }
            return Ok(new {StatusCode = 200, msg="找到此特色分類", data= featureList });
        }

        [HttpPatch]
        [Route("api/home/features/{id}")]
        [JwtAuthFilter]
        public IHttpActionResult SwitchActive(int id, RecipeRelated.FeatureCustomTags FeatureCustomTags)
        {
            //目前還未限制數量為了之後或許可擴充
            var user = userhash.GetUserFromJWT();

            var userData = db.Users.FirstOrDefault(u => u.Id == user.Id && user.Role == (int)UserRoles.Admin);
            //檢查本使用者是否有權限問題
            if (userData == null)
            {
                return Ok(new { StatusCode = 401, msg = "使用者不是管理員" });
            }
            var statusCheck = userhash.GetUserStatusErrorMessage(userData);
            if (statusCheck != null)
            {
                return Ok(new { statusCode = 403, msg = statusCheck });
            }

            var feature = db.FeatureSections.FirstOrDefault(f => f.Id == id);
            bool hasFeature = feature != null;
            if (!hasFeature) {

                return Ok(new { StatusCode = 400, msg = "找不到此特色分類" });
            
            }
            feature.FeaturedSectionName = FeatureCustomTags.SectionName;
            feature.FeaturedSectionTags = string.Join(",", FeatureCustomTags.CustomTags);
            feature.IsActive = FeatureCustomTags.isActive;
            feature.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            return Ok(new { StatusCode = 200, msg = "修改主題狀態成功", Id = feature.Id });
        }


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
        [Route("api/home/recipes/by-tag/{tag}")] //not ok
        public IHttpActionResult GetTagRecipes(string tag, int number=1)
        {
            const int pageSize = 6;
            var recipes = db.Recipes.Where(r => r.IsPublished && !r.IsDeleted && !r.IsArchived&& r.RecipeTags.Any(rt => rt.Tags.TagName.Contains(tag))).OrderByDescending(r=>r.CreatedAt).Skip(0).Take(pageSize);
            int count = recipes.Count();
            var data = recipes.Select(r => new
            {
                recipeName = r.RecipeName,
                coverPhoto = r.RecipesPhotos.FirstOrDefault(p => p.IsCover).ImgUrl,
                author = r.User.AccountName,
            }).ToList();

            var res = new
            {
                StatusCode = 200,
                msg = "獲取推薦動態食譜卡片",
                count = count,
                data = data
            };


            return Ok(res);
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


