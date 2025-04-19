using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RecipeTest.Models;
using RecipeTest.Enums;
using System.Web.Razor.Text;
using RecipeTest.Security;
using System.Web;
using System.Data.Entity;
using System.Collections;

namespace RecipeTest.Controllers
{

    public class AdController : ApiController
    {
        private UserEncryption userhash = new UserEncryption();
        private RecipeModel db = new RecipeModel();
        [HttpGet]
        [Route("api/ad")]
        public IHttpActionResult GetAd(string pos = "home", int? recipeId =null)
        {
            int DisplayLocation;
            string place = string.Empty;
            int takeNumber = 5;
            bool isRandom = false;
            switch (pos)
            {
                case "search":
                    DisplayLocation = (int)AdDisplayPageType.SearchList;
                    place = "搜尋列表廣告";
                    takeNumber = 1;
                    isRandom = true;
                    break;
                case "recipe":
                    DisplayLocation = (int)AdDisplayPageType.RecipeDetail;
                    place = "食譜內頁";
                    isRandom = true;
                    break;
                default:
                    place = "首頁橫幅";
                    DisplayLocation = (int)AdDisplayPageType.Home;
                    break;
            }
            var adData = db.Advertisements.Include(ad => ad.AdImgs)
            .Where(ad => ad.AdDisplayPage == DisplayLocation && ad.StartDate < DateTime.Now && ad.EndDate > DateTime.Now && ad.IsEnabled); 
                if (isRandom)
                {
                    if(pos =="recipe" && recipeId.HasValue)
                    {
                    var recipeTagsId = db.RecipeTags.Where(rt => rt.RecipeId == recipeId).Select(rt=>rt.TagId).ToList();
                    var AdTags = db.AdTags.Where(at => recipeTagsId.Contains(at.TagId)).Select(at => at.AdId).Distinct().ToList();
                    adData = adData.Where(ad => AdTags.Contains(ad.Id)).OrderBy(ad => Guid.NewGuid());
                    }
                    else
                    {
                        adData = adData.OrderBy(ad => Guid.NewGuid()).Take(takeNumber);
                    }
                }
                else 
                {
                    adData = adData.OrderBy(ad => ad.Priority).Take(takeNumber);
            }
            var uniqueAds = adData
                .ToList()  // ✅ 拉進記憶體中（EF 到這裡為止）
                .GroupBy(ad => ad.Id)
                .Select(g => g.First())  // ✅ 現在這裡是在記憶體中做 LINQ，不會報錯
                .ToList();
            var result = uniqueAds.Select(ad => new


            {
                id = ad.Id,
                adTitle = ad.AdName,
                adPrice = ad.AdPrice,
                adImages = ad.AdImgs.Select(img => img.ImgUrl).ToList(),
                adLink = ad.LinkUrl,
                adDescription = ad.AdIntro,

            }).ToList();

            var res = new
            {
                StatusCode = 200,
                msg = $"獲取了{place}廣告",
                data = result
            };
            return Ok(res);
        }
        //[HttpPost]
        //[Route("api/adlogs")]
        //public IHttpActionResult AdLogRecord([FromBody] int adId)
        //{
        //    string token = Request.Headers.Authorization?.Parameter;
        //    string sessionId = Request.Headers.GetValues("X-Session-Id")?.FirstOrDefault();

        //    if (string.IsNullOrEmpty(sessionId))
        //        return BadRequest("缺少 sessionId");

        //    int? userId = null;
        //    if (!string.IsNullOrEmpty(token))
        //    {
        //        var user = userhash.GetUserFromJWT();
        //        userId = user.Id;
        //    }

        //    DateTime today = DateTime.Today;

        //    // 🔍 檢查有沒有今天看過這則廣告
        //    var existingLog = db.AdViewLogs.FirstOrDefault(l =>
        //        l.AdId == adId &&
        //        DbFunctions.TruncateTime(l.ViewedAt) == today &&
        //        l.SessionId == sessionId);

        //    if (existingLog != null)
        //    {
        //        // 🔄 如果是後來登入的，把 userId 補上去
        //        if (userId != null && existingLog.UserId == null)
        //        {
        //            existingLog.UserId = userId;
        //            db.SaveChanges();
        //        }

        //        return Ok(new { msg = "已紀錄，今日不重複" });
        //    }

        //    // 🆕 沒有紀錄就新增
        //    db.AdViewLogs.Add(new AdViewLog
        //    {
        //        AdId = adId,
        //        SessionId = sessionId,
        //        UserId = userId,
        //        ViewedAt = DateTime.Now
        //    });

        //    db.SaveChanges();

        //    return Ok(new { msg = "曝光紀錄成功" });
        //}

    }
}
