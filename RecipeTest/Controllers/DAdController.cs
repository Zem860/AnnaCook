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
        public IHttpActionResult getAds(int number=1)
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
            var ads = db.Advertisements.OrderByDescending(a => a.CreatedAt).Skip(skipNumber).Take(10).ToList(); ;
            DateTime now = DateTime.Now;
            var adData = ads.Select(a => new
            {
                id = a.Id,
                adName = a.AdName,
                adType = a.AdDisplayPage == 1 ? "首頁" : a.AdDisplayPage == 2 ? "食譜列表頁" : a.AdDisplayPage == 3 ? "食譜內頁" : "未知",
                startDate = a.StartDate.ToString("yyyy-MM-dd"),
                endDatae = a.EndDate.ToString("yyyy-MM-dd"),
                status = !a.IsEnabled ? "草稿" : (now < a.StartDate) ? "已排成" : (now >= a.StartDate && now <= a.EndDate) ? "進行中" : "已結束",
                clicks = db.AdViewLogs.Where(al => al.AdId == a.Id).Count(),
                tags = a.AdTags.Select(at=>at.Tags.TagName).ToList()
            });

            return Ok(new {StatusCode = 200, msg="獲取廣告", data = adData });
        }
    }
}
