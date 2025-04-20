using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MyWebApiProject.Security;
using RecipeTest.Models;
using RecipeTest.Security;
using RecipeTest.Enums;

namespace RecipeTest.Controllers
{
    public class DAdController : ApiController
    {
        private RecipeModel db = new RecipeModel();
        private UserEncryption userhash = new UserEncryption();
        [HttpGet]
        [Route("api/admin/ads")]
        [JwtAuthFilter]
        public IHttpActionResult getAds(int number=1, string pos ="all", string status ="all")
        {
            var user = userhash.GetUserFromJWT();
            var admin = db.Users.FirstOrDefault(u => u.Id == u.Id && u.IsVerified && !u.IsBanned && !u.IsDeleted && u.UserRole == UserRoles.Admin);
            bool hasAdmin = admin != null;
            if (!hasAdmin)
            {
                return Ok(new { StatusCode = 401, msg = "此人無權限" });
            }
            int pageSize = 10;
            int skipNumber = (number -1 )*pageSize;
            DateTime now = DateTime.Now;

            var adsQuery = db.Advertisements.AsQueryable();

            if (pos != "all")
            {
                int adPos = pos=="home"?1:pos=="search"?2:pos =="recipe"?3:0;
                adsQuery = adsQuery.Where(a=>a.AdDisplayPage == adPos);
            }

            switch (status)
            {
                case "draft":
                    // 草稿 → 只要沒上架，都算草稿（不論時間）
                    adsQuery = adsQuery.Where(a => !a.IsEnabled);
                    break;
                case "scheduled":
                    // 排程 → 上架了，但還沒到開始時間
                    adsQuery = adsQuery.Where(a => a.IsEnabled && a.StartDate > now);
                    break;
                case "active":
                    // 進行中 → 上架了，而且在開始與結束時間內
                    adsQuery = adsQuery.Where(a => a.IsEnabled && a.StartDate <= now && a.EndDate >= now);
                    break;
                case "expired":
                    // 已結束 → 時間已過，不論是否上架
                    adsQuery = adsQuery.Where(a => a.EndDate < now);
                    break;
                default:
                    break;
            }

            var ads = adsQuery.
                OrderByDescending(a => a.CreatedAt).Skip(skipNumber).Take(pageSize).ToList();
            var adsNumber = ads.Count();
            int totalPages = (int)Math.Ceiling((double)adsNumber / pageSize);
            var adIds = ads.Select(a => a.Id).ToList();
            //groupby adlogs 以adid為主
            var logCount = db.AdViewLogs.Where(av => adIds.Contains(av.AdId) && av.IsClick).GroupBy(av => av.AdId).ToDictionary(g=>g.Key, g=>g.Count());
            //1: 5
            //2: 3
            //3: 0
            var adData = ads.Select(a => new
            {
                id = a.Id,
                adName = a.AdName,
                adType = a.AdDisplayPage == 1 ? "首頁" : a.AdDisplayPage == 2 ? "食譜列表頁" : a.AdDisplayPage == 3 ? "食譜內頁" : "未知",
                startDate = a.StartDate.ToString("yyyy-MM-dd"),
                endDate = a.EndDate.ToString("yyyy-MM-dd"),
                status = !a.IsEnabled ? "草稿" : (now < a.StartDate) ? "已排成" : (now >= a.StartDate && now <= a.EndDate) ? "進行中" : "已結束",
                clicks = logCount.TryGetValue(a.Id, out int count)? count: 0,
                tags = a.AdTags.Select(at => at.Tags.TagName),
            }).ToList() ;

            return Ok(new {StatusCode = 200,
                msg = "獲取廣告",
                totalCount = adsNumber,
                totalPages = totalPages,
                data = adData });
        }
    }
}
