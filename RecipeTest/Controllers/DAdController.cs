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
using RecipeTest.Pages;
using System.Web;
using System.Threading.Tasks;
using System.IO;
using static RecipeTest.Pages.AdRelated;

namespace RecipeTest.Controllers
{
    public class DAdController : ApiController
    {
        private RecipeModel db = new RecipeModel();
        private UserEncryption userhash = new UserEncryption();
        private string localStorragePath = HttpContext.Current.Server.MapPath("~/AdImages");
        private string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private string[] adStatus = { "draft", "scheduled", "active", "expired", "all" };
        [HttpGet]
        [Route("api/admin/ads")]
        [JwtAuthFilter]
        public IHttpActionResult getAds(int number = 1, string pos = "all", string status = "all")
        {
            var user = userhash.GetUserFromJWT();
            var admin = db.Users.FirstOrDefault(u => u.Id == u.Id && u.IsVerified && !u.IsBanned && !u.IsDeleted && u.UserRole == UserRoles.Admin);
            bool hasAdmin = admin != null;
            if (!hasAdmin)
            {
                return Ok(new { StatusCode = 401, msg = "此人無權限" });
            }
            int pageSize = 10;
            int skipNumber = (number - 1) * pageSize;
            DateTime now = DateTime.Now;

            var adsQuery = db.Advertisements.AsQueryable();

            if (pos != "all")
            {
                int adPos = pos == "home" ? 1 : pos == "search" ? 2 : pos == "recipe" ? 3 : 0;
                adsQuery = adsQuery.Where(a => a.AdDisplayPage == adPos);
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
            var logCount = db.AdViewLogs.Where(av => adIds.Contains(av.AdvertisementId) && av.IsClick).GroupBy(av => av.AdvertisementId).ToDictionary(g => g.Key, g => g.Count());
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
                clicks = logCount.TryGetValue(a.Id, out int count) ? count : 0,
                tags = a.AdTags.Select(at => at.Tags.TagName),
            }).ToList();

            return Ok(new
            {
                StatusCode = 200,
                msg = "獲取廣告",
                totalCount = adsNumber,
                totalPages = totalPages,
                data = adData
            });
        }

        [HttpPost]
        [Route("api/admin/ad")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> createAd()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Ok(new { StatusCode = 401, msg = "請求格式錯誤" });
            }

            DateTime now = DateTime.Now;
            var user = userhash.GetUserFromJWT();
            var admin = db.Users.FirstOrDefault(u => u.Id == user.Id && u.IsVerified && !u.IsBanned && !u.IsDeleted && u.UserRole == UserRoles.Admin);
            bool isAdmin = admin != null;
            if (!isAdmin)
            {
                return Ok(new { StatusCode = 401, msg = "你權限不夠" });
            }
            var provider = await Request.Content.ReadAsMultipartAsync();
            var contents = provider.Contents;

            string GetFormValue(string key) =>
                contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == key)?.ReadAsStringAsync().Result;

            List<HttpContent> GetFileList(string inputName) =>
                contents.Where(c => c.Headers.ContentDisposition.Name.Trim('"') == inputName && c.Headers.ContentDisposition.FileName != null).ToList();

            var imageFiles = GetFileList("adImages");
            if (!imageFiles.Any())
            {
                return Ok(new { StatusCode = 401, msg = "請上傳圖片" });
            }

            string adName = GetFormValue("adName");
            string dashboardAdName = GetFormValue("dashboardAdName");
            string adPos = GetFormValue("pos");
            string adIntro = GetFormValue("adIntro");
            string adPrice = GetFormValue("adPrice");
            string adCurrency = GetFormValue("currency");
            string advertiserName = GetFormValue("advertiserName");
            string priority = GetFormValue("priority");
            string linkUrl = GetFormValue("linkUrl");
            string status = GetFormValue("status");
            string startDateStr = GetFormValue("startDate");
            string endDateStr = GetFormValue("endDate");

            if (!DateTime.TryParse(startDateStr, out DateTime startDate) || !DateTime.TryParse(endDateStr, out DateTime endDate))
            {
                return Ok(new { StatusCode = 400, msg = "上架/下架日期格式錯誤" });
            }

            if (startDate > endDate)
            {
                return Ok(new { StatusCode = 400, msg = "開始日期不能晚於結束日期" });
            }

            switch (status)
            {
                case "draft":
                    // 不上架
                    break;
                case "scheduled":
                    if (now >= startDate)
                        return Ok(new { StatusCode = 400, msg = "排程中的廣告開始時間必須在未來" });
                    break;
                case "active":
                    if (now < startDate)
                        return Ok(new { StatusCode = 400, msg = "狀態為進行中時，開始時間不可是未來" });
                    if (now > endDate)
                        return Ok(new { StatusCode = 400, msg = "狀態為進行中，但已超過結束時間" });
                    break;
                case "expired":
                    if (now <= endDate)
                        return Ok(new { StatusCode = 400, msg = "已結束的廣告，結束時間必須早於現在" });
                    break;
                default:
                    return Ok(new { StatusCode = 400, msg = "狀態參數不合法" });
            }

            decimal? calcuPrice = decimal.TryParse(adPrice, out decimal price) ? price : (decimal?)null;

            string tagNames = GetFormValue("tagNames");
            List<string> tags = tagNames?
                .Split(',')
                .Select(t => t.Trim().ToLower())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList() ?? new List<string>();

            var matchedTags = db.Tags
                .Where(t => tags.Contains(t.TagName.ToLower()))
                .ToList();

            var ad = new Advertisement
            {
                AdName = adName,
                DashboardAdName = dashboardAdName,
                AdDisplayPage = adPos == "home" ? 1 : adPos == "search" ? 2 : adPos == "recipe" ? 3 : 0,
                AdIntro = adIntro,
                LinkUrl = linkUrl,
                AdPrice = calcuPrice,
                AdvertiserName = advertiserName,
                Currency = adCurrency,
                Priority= Convert.ToInt32(priority),
                StartDate = startDate,
                EndDate = endDate,
                IsEnabled = status == "active" || status == "scheduled" || status == "expired",
                CreatedAt = now,
                UpdateTime = now,
                AdImgs = new List<AdImgs>(),
                AdTags = matchedTags.Select(tag => new AdTags { TagId = tag.Id }).ToList()
            };

            foreach (var file in imageFiles)
            {
                string extension = Path.GetExtension(file.Headers.ContentDisposition.FileName.Trim('"'));
                if (!allowedExtensions.Contains(extension))
                {
                    return Ok(new { StatusCode = 400, msg = "檔案格式錯誤" });
                }

                string newFileName = Guid.NewGuid().ToString("N") + extension;
                string relativePath = "/AdImages/" + newFileName;
                string fullPath = Path.Combine(localStorragePath, newFileName);

                byte[] fileBytes = await file.ReadAsByteArrayAsync();
                File.WriteAllBytes(fullPath, fileBytes);

                ad.AdImgs.Add(new AdImgs
                {
                    ImgUrl = relativePath,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            ad.AdTags = matchedTags.Select(tag => new AdTags
            {
                TagId = tag.Id,
                CreatedAt = now,
                UpdatedAt = now
            }).ToList();


            db.Advertisements.Add(ad);
            db.SaveChanges();


            return Ok(new { StatusCode = 200, msg = "廣告建立成功", adId = ad.Id });
        }

        //[HttpGet]
        //[Route("api/admin/ads/overview")]
        //public IHttpActionResult GetAdsOverview()
        //{
        //    DateTime startOfThisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        //    DateTime startOfNextMonth = startOfThisMonth.AddMonths(1);
        //    DateTime endOfThisMonth = startOfNextMonth.AddDays(-1);
        //    DateTime startOfLastMonth = startOfThisMonth.AddMonths(-1);
        //    DateTime endOfLastMonth = startOfThisMonth.AddDays(-1); // 改成這一行！

        //    int viewLogsOfThisMonth = db.AdViewLogs.Count(av => !av.IsClick && av.ViewedAt >= startOfThisMonth && av.ViewedAt <= endOfThisMonth);
        //    int viewLogsOfLastMonth = db.AdViewLogs.Count(av => !av.IsClick && av.ViewedAt >= startOfLastMonth && av.ViewedAt <= endOfLastMonth);
        //    int clickLogOfThisMonth = db.AdViewLogs.Count(av => av.IsClick && av.ViewedAt >= startOfThisMonth && av.ViewedAt <= endOfThisMonth);
        //    int clickLogsOfLastMonth = db.AdViewLogs.Count(av => av.IsClick && av.ViewedAt >= startOfLastMonth && av.ViewedAt <= endOfLastMonth);

        //    double interactionRate = ((double)clickLogOfThisMonth / (double)viewLogsOfThisMonth)*100;

        //    var thisMonthQuery = db.AdViewLogs.Where(av => av.ViewedAt >= startOfThisMonth && av.ViewedAt <= endOfLastMonth);
        //    var lastMonthQuery = db.AdViewLogs.Where(av => av.ViewedAt >= startOfLastMonth && av.ViewedAt <= endOfLastMonth);

        //    return Ok(new
        //    {
        //        thismonthview = viewLogsOfThisMonth,
        //        lastmonthview = viewLogsOfLastMonth,
        //        thismonthclick = clickLogOfThisMonth,
        //        lastmonthclick = clickLogsOfLastMonth,
        //        interactionRate = interactionRate,
        //    });
        //}

        [HttpGet]
        [Route("api/admin/top3/ads")]
        public IHttpActionResult getTop3()
        {
            DateTime startOfThisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime startOfNextMonth = startOfThisMonth.AddMonths(1);
            DateTime endOfThisMonth = startOfNextMonth.AddDays(-1);
            var query = db.AdViewLogs.Where(av => av.ViewedAt >= startOfThisMonth && av.ViewedAt <= endOfThisMonth);


            var groupData = query.GroupBy(av => av.AdvertisementId);


            var groupDataDetail = groupData.Select(g => new
            { 
                adName = g.FirstOrDefault().Advertisement.AdName,
                adDisplayPage = g.FirstOrDefault().Advertisement.AdDisplayPage == 1 ? "首頁" : g.FirstOrDefault().Advertisement.AdDisplayPage == 2 ? "食譜列表頁" : g.FirstOrDefault().Advertisement.AdDisplayPage == 3 ? "食譜內頁" : "未知",
                viewCount = g.Count(av => !av.IsClick),
                clickCount = g.Count(av => av.IsClick),
                interactionRate = g.Count(av => !av.IsClick) == 0 || g.Count(av => av.IsClick) == 0 ? 0 : ((double)g.Count(av => av.IsClick) / g.Count(av => !av.IsClick))*100
            }).OrderByDescending(gd => gd.interactionRate).Take(3).ToList();

            var res = new
            {
                StatusCode = 200,
                msg = "獲取廣告總表數據",
                data = groupDataDetail,
            };

            return Ok(res);
        }

        [HttpGet]
        [Route("api/admin/ads/chart")]
        public IHttpActionResult getAdsOverview(DateTime startDate, DateTime endDate, string userType = "all")
        {
            var query = db.AdViewLogs.Where(av => av.ViewedAt >= startDate && av.ViewedAt <= endDate);
            if (userType == "guest")
            {
                query = query.Where(av => av.UserId == null);
            } else if (userType == "user")
            {
                query = query.Where(av => av.UserId != null);
            }
            var groupData = query.GroupBy(av => av.ViewedAt.Date);
            var groupDataDetail = groupData.Select(g => new
            {
                date = g.Key.ToString("yyyy-MM-dd"),
                viewCount = g.Count(av => !av.IsClick),
                clickCount = g.Count(av => av.IsClick),
                interactionRate = g.Count(av => !av.IsClick) == 0 || g.Count(av => av.IsClick) == 0 ? 0 : ((double)g.Count(av => av.IsClick) / g.Count(av => !av.IsClick)) * 100,
            }).OrderBy(gd => gd.date).ToList();

            var res = new
            {
                StatusCode = 200,
                msg = "獲取廣告總表數據",
                data = groupDataDetail,
            };


            return Ok(res);
        }


    }
}
