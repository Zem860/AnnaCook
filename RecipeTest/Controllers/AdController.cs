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
using RecipeTest.Pages;
namespace RecipeTest.Controllers
{

    public class AdController : ApiController
    {
        private UserEncryption userhash = new UserEncryption();
        private RecipeModel db = new RecipeModel();
        [HttpGet]
        [Route("api/ad")]
        public IHttpActionResult GetAd(string pos = "home", int? recipeId = null)
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
                if (pos == "recipe" && recipeId.HasValue)
                {
                    var recipeTagsId = db.RecipeTags.Where(rt => rt.RecipeId == recipeId).Select(rt => rt.TagId).ToList();
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
        //再與前端討論他要傳哪種ID進來
        [HttpPost]
        [Route("api/adlogs")]
        public IHttpActionResult recordAdRecord(AdRelated.AdLogDto userRecords)
        {
            var token = Request.Headers.Authorization?.Parameter;
            int? currentUserId = 0;
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var payload = JwtAuthUtil.GetPayload(token);
                    currentUserId = ((int?)payload["Id"]).Value;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Token 無法解析：" + ex.Message);
                }
            }
            if (userRecords.IsClick)
            {

                bool hasClickedBySession = db.AdViewLogs.Any(av =>
                av.AdId == userRecords.AdId &&
                av.SessionId == userRecords.SessionId.ToString() &&
                av.IsClick &&
                DbFunctions.TruncateTime(av.ViewedAt) == DateTime.Today
                    );

                bool hasClickedByUser = db.AdViewLogs.Any(av => av.AdId == userRecords.AdId && av.UserId == currentUserId && av.IsClick && DbFunctions.TruncateTime(av.ViewedAt) == DateTime.Today);

                if (hasClickedBySession||hasClickedByUser)
                {
                    return Ok(new { StatusCode = 401, msg = "今日已點擊" });
                }
            }

            int displayLocation = 0;
            switch (userRecords.Pos.ToLower())
            {
                case "home":
                    displayLocation = (int)AdDisplayPageType.Home;
                    break;
                case "search":
                    displayLocation = (int)AdDisplayPageType.SearchList;
                    break;
                case "recipe":
                    displayLocation = (int)AdDisplayPageType.RecipeDetail;
                    break;
                default:
                    return Ok(new { StatusCode = 401, msg = "位置參數錯誤" });
            }

            var adlog = new AdViewLog();
            adlog.AdId = userRecords.AdId;
            adlog.AdDisplayPage = displayLocation;
            adlog.UserId = currentUserId.HasValue && currentUserId > 0 ? currentUserId : null;
            adlog.SessionId = userRecords.SessionId.ToString();
            adlog.IsClick = userRecords.IsClick;
            adlog.ViewedAt = DateTime.Now;

            db.AdViewLogs.Add(adlog);
            db.SaveChanges();
            var type = userRecords.IsClick ? "點擊" : "觀看";
            var res = new
            {
                StatusCode = 200,
                msg = $"廣告{type}已記錄",
                AdId = userRecords.AdId,
                logId = adlog.Id,
            };
            return Ok(res);
        }
    }
}
