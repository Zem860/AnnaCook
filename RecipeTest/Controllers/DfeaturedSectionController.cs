using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MyWebApiProject.Security;
using RecipeTest.Enums;
using RecipeTest.Models;
using RecipeTest.Pages;
using RecipeTest.Security;

namespace RecipeTest.Controllers
{
    public class DfeaturedSectionController : ApiController
    {
        private RecipeModel db = new RecipeModel();
        private UserEncryption userhash = new UserEncryption();

        //首頁get下方以排序
        //Emma改設定了要改變
        [HttpPost]
        [Route("api/admin/features")]
        [JwtAuthFilter]
        public IHttpActionResult AddFeatures(RecipeRelated.FeatureCustomTags FeatureCustomTags)
        {
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


            return Ok(new { StatusCode = 200, msg = "新增主題成功", Id = features.Id });
        }

        [HttpGet]
        [Route("api/admin/feature/{id}")]
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
        [Route("api/admin/featuredList")]
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
            return Ok(new { StatusCode = 200, msg = "找到此特色分類", data = featureList });
        }

        [HttpPut]
        [Route("api/admin/features/{id}")]
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
            if (!hasFeature)
            {

                return Ok(new { StatusCode = 400, msg = "找不到此特色分類" });

            }
            feature.FeaturedSectionName = FeatureCustomTags.SectionName;
            feature.FeaturedSectionTags = string.Join(",", FeatureCustomTags.CustomTags);
            feature.IsActive = FeatureCustomTags.isActive;
            feature.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            return Ok(new { StatusCode = 200, msg = "修改主題狀態成功", Id = feature.Id });
        }

    }
}
