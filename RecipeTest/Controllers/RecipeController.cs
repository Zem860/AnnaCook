using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using RecipeTest.Models;
using System.Threading.Tasks;
using RecipeTest.Pages;
using static RecipeTest.Pages.RecipeRelated;
using System.Configuration;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;
using RecipeTest.Security;
using MyWebApiProject.Security;
using RecipeTest.Enums;

namespace RecipeTest.Controllers
{
    public static class HttpMethodExtensions
    {
        public static readonly HttpMethod Patch = new HttpMethod("PATCH");
    }
    public class RecipeController : ApiController
    {
        private RecipeModel db = new RecipeModel();
        private string localStorragePath = HttpContext.Current.Server.MapPath("~/TestPhoto");
        private string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
        private string vimeoAccessToken = ConfigurationManager.AppSettings["VimeoAccessToken"];
        private string vimeoUploadUrl = "https://api.vimeo.com/me/videos";
        private JwtAuthUtil jwt = new JwtAuthUtil();
        private UserEncryption userhash = new UserEncryption();

        //-------------------------------搜尋食譜--------------------------------

        [HttpGet]
        [Route("api/recipes/search")]
        public IHttpActionResult SearchRecipe(string searchData = "", string type = "createdAt", int page = 1)
        {
            int pageSize = 10;
            var query = db.Recipes.AsQueryable();
            query = query.Where(r => r.IsPublished == true); // 只選擇已發布的食譜

            if (!string.IsNullOrWhiteSpace(searchData))
            {

                query = query.Where(r => r.RecipeName.Contains(searchData));
            }


            switch (type.ToLower())
            {
                case "popular":
                    query = query.OrderByDescending(r => r.Rating);
                    break;
                case "createdat":
                default:
                    query = query.OrderByDescending(r => r.CreatedAt);
                    break;
            }

            var totalCount = query.Count();

            var pagedQuery = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var result = pagedQuery.Select(r => new
            {
                id = r.Id,
                authorId = r.UserId,
                displayId = r.DisplayId,
                isPublished = r.IsPublished,
                recipeName = r.RecipeName,
                description = r.RecipeIntro,
                cookingTime = r.CookingTime,
                portion = r.Portion,
                rating = r.Rating,
                createdAt = r.CreatedAt,
                coverPhoto = r.RecipesPhotos
                    .Where(p => p.IsCover)
                    .OrderBy(p => p.CreatedAt)
                    .Select(p => p.ImgUrl)
                    .FirstOrDefault()
            }).ToList();

            bool hasMore = page * pageSize < totalCount; 

            var res = new
            {
                StatusCode = 200,
                msg = totalCount > 0 ? $"獲取資料{result.Count}筆" : "查無資料",
                page = $"第{page}頁",
                hasMore = hasMore,
                totalCount = totalCount,
                data = result
            };

            return Ok(res); // ✅ 正確回傳整包資料
        }

        //--------------------瀏覽單一食譜-----------------------------------------
        //食譜內頁get//食譜細項
        [HttpGet]
        [Route("api/recipes/{id}")]
        public IHttpActionResult GetRecipe(int id)
        {
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == id);
            bool hasRecipe = recipe != null;
            if (hasRecipe)
            {
                recipe.ViewCount += 1;
                db.SaveChanges();
                var recipeData = new
                {
                    id = recipe.Id,
                    displayId = recipe.DisplayId,
                    isPublished = recipe.IsPublished,
                    viewCount = recipe.ViewCount,
                    recipeName = recipe.RecipeName,
                    coverPhoto = recipe.RecipesPhotos.FirstOrDefault(p => p.IsCover)?.ImgUrl,
                    description = recipe.RecipeIntro,
                    cookingTime = recipe.CookingTime,
                    portion = recipe.Portion,
                    rating = recipe.Rating,
                    videoId = recipe.RecipeVideoLink,

                };
                var followers = db.Follows.Where(f => f.FollowedUserId == recipe.UserId).Count();
                var authorData = new
                {
                    id = recipe.UserId,
                    displayId = recipe.User.DisplayId,
                    name = recipe.User.AccountName,
                    followersCount = followers,
                };
                var ingredients = recipe.Ingredients.Select(i => new
                {
                    ingredientId = i.Id,
                    ingredientName = i.IngredientName,
                    amount = i.Amount,
                    unit = i.Unit,
                    isFlavoring = i.IsFlavoring,
                });

                var tagData = recipe.RecipeTags.Select(rt => new
                {
                    id = rt.Tags.Id,
                    tag = rt.Tags.TagName,
                });

                //var stepsData = recipe.Steps.Select(s => new
                //{
                //    id = s.Id,
                //    description = s.StepDescription,
                //    stepOrder = s.StepOrder,
                //    startTime = s.VideoStart,
                //    endTime = s.VideoEnd,
                //}
                //);
                var data = new
                {
                    author = authorData,
                    recipe = recipeData,
                    ingredients = ingredients,
                    tags = tagData,
                    //steps = stepsData,
                };

                var res = new
                {
                    StatusCode = 200,
                    msg = "成功獲取單筆食譜資料",
                    data = data,
                };

                return Ok(res);

            } else
            {
                return NotFound();
            }

        }
        //--------------------------------點選教學開始(暫時先用資料庫id做)------------------------------------------

        [HttpGet]
        [Route("api/recipes/{id}/teaching")]
        public IHttpActionResult GetTeaching(int id)
        {
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == id);
            if (recipe == null)
            {
                return NotFound();
            }
            var stepsData = recipe.Steps.Select(s => new
            {
                id = s.Id,
                description = s.StepDescription,
                stepOrder = s.StepOrder,
                startTime = s.VideoStart,
                endTime = s.VideoEnd,
            }).ToList();

            var data = new
            {
                recipeId = recipe.Id,
                recipeName = recipe.RecipeName,
                video = recipe.RecipeVideoLink,
                steps = stepsData
            };

            var res = new
            {
                StatusCode = 200,
                msg = "成功獲取食譜步驟資料",
                data = data,
            };
            return Ok(res);
        }


        //-----------------------------------上傳食譜-----------------------------------
        [HttpPost]
        [Route("api/recipes")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> AddNewRecipe()
        {
            var user = userhash.GetUserFromJWT();
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("請使用multipart/form-data");
            }
            if (!Directory.Exists(localStorragePath))
            {
                Directory.CreateDirectory(localStorragePath);

            }

            var provider = await Request.Content.ReadAsMultipartAsync();
            var contents = provider.Contents;
            //取得食譜名稱
            string recipeName = contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "recipeName")?
            .ReadAsStringAsync().Result;
            if (string.IsNullOrWhiteSpace(recipeName))
            {
                return BadRequest("食譜名稱為必填欄位");
            }
            try
            {
                //建立唯一顯示ID
                int count = db.Recipes.Count();
                string displayId = "R" + (count + 1).ToString("D6");
                var recipe = new Recipes();
                recipe.RecipeName = recipeName;
                recipe.DisplayId = displayId;
                recipe.IsPublished = false;
                recipe.CreatedAt = DateTime.Now;
                recipe.UpdatedAt = DateTime.Now;
                recipe.UserId = user.Id;
                db.Recipes.Add(recipe);
                db.SaveChanges();
                //-----------------------開始處理圖片-------------------(與前端討論是否開放null目前預設是不可以)
                var photo = contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "photo");
                if (photo == null)
                {
                    return BadRequest("請上傳圖片");
                }
                //檢查副檔名
                string fileName = photo.Headers.ContentDisposition.FileName.Trim('"');
                string extension = Path.GetExtension(fileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("檔案格式錯誤");
                }
                //重新命名檔案
                string newFileName = Guid.NewGuid().ToString("N") + extension;
                string relativePath = "/TestPhoto/" + newFileName;
                string fullPath = Path.Combine(localStorragePath, newFileName);
                byte[] fileBytes = await photo.ReadAsByteArrayAsync();
                File.WriteAllBytes(fullPath, fileBytes);
                var recipePhoto = new RecipePhotos();
                recipePhoto.RecipeId = recipe.Id;
                recipePhoto.ImgUrl = relativePath;
                recipePhoto.IsCover = true;
                recipePhoto.CreatedAt = DateTime.Now;
                recipePhoto.UpdatedAt = DateTime.Now;
                db.RecipePhotos.Add(recipePhoto);
                db.SaveChanges();
                //--------------------試做refreshToken-----------------------------------
                string token = userhash.GetRawTokenFromHeader();
                var payload = JwtAuthUtil.GetPayload(token);
                var newToken = jwt.ExpRefreshToken(payload);

                var res = new
                {
                    StatusCode = 200,
                    msg = "食譜新增成功",
                    Id = recipe.Id,
                    newToken = newToken,
                };
                return Ok(res);

            }
            catch (Exception ex)
            {
                var res = new
                {
                    StatusCode = 500,
                    msg = "食譜新增失敗",
                    error = ex,
                };
                return Ok(res);
            }
        }

        //--------------------食譜細項上傳(步驟2)--------------------------------
        //step2 上傳食譜細項(修改)
        [HttpPut]
        [Route("api/recipes/step2/{id}")]
        [JwtAuthFilter]
        public IHttpActionResult UpdateRecipeDetail(int id, UserRecipeDetail recipeDetail)
        {
            var user = userhash.GetUserFromJWT();
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == id && r.UserId == user.Id);
            //理論上這個食譜，的published應該是都可以但是必須對上userId不然是不能修改的
            bool hasRecipe = recipe != null;
            if (!hasRecipe)
            {
                return NotFound();
            }
            recipe.RecipeIntro = recipeDetail.RecipeIntro;
            recipe.CookingTime = recipeDetail.CookingTime;
            recipe.Portion = recipeDetail.Portion;
            recipe.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            // ToList() 表示先將查詢結果拉到記憶體中，雖然變成 List，但裡面的實體仍然被 DbContext 追蹤。
            // 這樣做是為了避免之後查詢還沒執行就先操作，或在操作過程中資料被改變（如同一時間的新增、刪除、更新等切片問題）。
            // 也能確保 RemoveRange 時資料是穩定一致的。
            var oldIngredients = db.Ingredients.Where(i => i.RecipeId == id).ToList();
            db.Ingredients.RemoveRange(oldIngredients);
            foreach(var ing in recipeDetail.Ingredients)
            {
                var ingredients = new Ingredients();
                ingredients.RecipeId = id;
                ingredients.IngredientName = ing.IngredientName;
                ingredients.IsFlavoring = ing.IsFlavoring;
                ingredients.Amount = ing.IngredientAmount;
                ingredients.Unit = ing.IngredientUnit;
                ingredients.CreatedAt = DateTime.Now;
                ingredients.UpdatedAt = DateTime.Now;
                db.Ingredients.Add(ingredients);
            }
            db.SaveChanges();

            var oldTags = db.RecipeTags.Where(rt => rt.RecipeId == id).ToList();
            db.RecipeTags.RemoveRange(oldTags);

            foreach (var tag in recipeDetail.Tags)
            {
                var normalizedTag = tag.Trim().ToLower();
                // 嘗試找到既有 Tag
                var existingTag = db.Tags.FirstOrDefault(t => t.TagName.ToLower() == normalizedTag);
                bool alreadyExist = existingTag != null;
                // 若沒有則新增
                if (!alreadyExist)
                {
                    existingTag = new Tags();
                    existingTag.TagName = tag.Trim();
                    existingTag.CreatedAt = DateTime.Now;
                    existingTag.UpdatedAt = DateTime.Now;
                    db.Tags.Add(existingTag);
                    db.SaveChanges(); 
                }
                // 建立中間關聯
                var recipeTag = new RecipeTags();
                recipeTag.RecipeId = id;
                recipeTag.TagId = existingTag.Id;
                recipeTag.CreatedAt = DateTime.Now;
                recipeTag.UpdatedAt = DateTime.Now;
                db.RecipeTags.Add(recipeTag);


            }
            db.SaveChanges();
            //--------------------試做refreshToken-----------------------------------
            string token = userhash.GetRawTokenFromHeader();
            var payload = JwtAuthUtil.GetPayload(token);
            var newToken = jwt.ExpRefreshToken(payload);

            var res = new
                {
                    StatusCode = 200,
                    msg = "食譜更新成功",
                    Id = id,
                newToken = newToken,
            };
                return Ok(res);
            }
        //[HttpPut]
        //[Route("api/recipes/{id}/video")]
        //public async Task<IHttpActionResult> UploadToVimeo(int id)
        //{
        //    try
        //    {
        //        if (!Request.Content.IsMimeMultipartContent())
        //            return BadRequest("請使用 Multipart 表單上傳影片");
        //        var provider = new MultipartMemoryStreamProvider();
        //        await Request.Content.ReadAsMultipartAsync(provider);
        //        var file = provider.Contents.FirstOrDefault();
        //        if (file == null)
        //            return BadRequest("未收到影片檔案");
        //        var fileStream = await file.ReadAsStreamAsync();
        //        var fileSize = fileStream.Length;
        //        Console.WriteLine($"影片大小: {fileSize} bytes");
        //        using (var client = new HttpClient())
        //        {
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", vimeoAccessToken);
        //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //            string videoTitle = $"AC_{Guid.NewGuid()}";

        //            // 1️⃣ `POST` 取得 `upload.upload_link`
        //            var requestBody = new
        //            {
        //                name = videoTitle,
        //                upload = new
        //                {
        //                    approach = "tus",
        //                    size = fileSize
        //                }
        //            };

        //            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
        //            var createVideoResponse = await client.PostAsync("https://api.vimeo.com/me/videos", content);
        //            string errorResponse = await createVideoResponse.Content.ReadAsStringAsync();

        //            if (!createVideoResponse.IsSuccessStatusCode)
        //            {
        //                Console.WriteLine($"Vimeo API 錯誤 (POST): {errorResponse}");
        //                return BadRequest($"無法創建 Vimeo 上傳: {errorResponse}");
        //            }

        //            var createVideoContent = await createVideoResponse.Content.ReadAsStringAsync();
        //            JObject createVideoJson = JObject.Parse(createVideoContent);
        //            string uploadUrl = createVideoJson["upload"]["upload_link"]?.ToString();
        //            string videoUri = createVideoJson["uri"]?.ToString();
        //            if (string.IsNullOrEmpty(uploadUrl))
        //                return BadRequest("未能取得 Vimeo 上傳 URL");
        //            Console.WriteLine($"上傳 URL: {uploadUrl}");
        //            // 進行 `PATCH` 上傳影片檔案的同時，加入進度檢查
        //            using (var uploadClient = new HttpClient())
        //            {
        //                var uploadRequest = new HttpRequestMessage(HttpMethodExtensions.Patch, uploadUrl)
        //                {
        //                    Content = new StreamContent(fileStream)
        //                };
        //                uploadRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/offset+octet-stream");
        //                uploadRequest.Headers.Add("Tus-Resumable", "1.0.0");
        //                uploadRequest.Headers.Add("Upload-Offset", "0");

        //                var uploadResponse = await uploadClient.SendAsync(uploadRequest);
        //                if (!uploadResponse.IsSuccessStatusCode)
        //                {
        //                    string uploadError = await uploadResponse.Content.ReadAsStringAsync();
        //                    Console.WriteLine($"影片上傳失敗: {uploadError}");
        //                    return BadRequest($"影片上傳失敗: {uploadError}");
        //                }

        //                // 檢查影片是否已完成上傳
        //                var headRequest = new HttpRequestMessage(HttpMethod.Head, uploadUrl);
        //                headRequest.Headers.Add("Tus-Resumable", "1.0.0");

        //                var headResponse = await uploadClient.SendAsync(headRequest);
        //                if (headResponse.IsSuccessStatusCode)
        //                {
        //                    var uploadOffset = headResponse.Headers.GetValues("Upload-Offset").FirstOrDefault();
        //                    var uploadLength = headResponse.Headers.GetValues("Upload-Length").FirstOrDefault();
        //                    var videoInfoResponse = await client.GetAsync($"https://api.vimeo.com{videoUri}");
        //                    var videoInfoJson = await videoInfoResponse.Content.ReadAsStringAsync();
        //                    JObject videoInfo = JObject.Parse(videoInfoJson);

        //                    var duration = videoInfo["duration"]?.Value<decimal>();
        //                    var status = videoInfo["transcode"]?["status"]?.ToString();
        //                    if (uploadOffset == uploadLength && status == "complete")
        //                    {
        //                        //將id存入資料庫
        //                        var recipe = db.Recipes.FirstOrDefault(r=>r.Id == id);
        //                        recipe.RecipeVideoLink = videoUri;
        //                        recipe.RecipeVideoDuration = duration;
        //                        db.SaveChanges();
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine("影片仍在上傳中，繼續上傳...");
        //                        return BadRequest("影片尚未上傳完成，請稍後再試");
        //                    }
        //                }
        //            }


        //            return Ok(new { message = "影片上傳成功", videoUri });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"內部錯誤: {ex.Message}");
        //        return InternalServerError(ex);
        //    }
        //}
        //step3上傳影片
        //--------------------------------上傳影片-----------------------------------


        [HttpPut]
        [Route("api/recipes/{id}/video")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> UploadToVimeo(int id)
        {
            var user = userhash.GetUserFromJWT();
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == id && r.UserId == user.Id);
            bool hasRecipe = recipe != null;
            if (!hasRecipe)
            {
                return NotFound();
            }
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                    return BadRequest("請使用 Multipart 表單上傳影片");
                // 🎯 圖片上傳的情境：使用內建的 Request.Content.ReadAsMultipartAsync()
                // 系統會自動使用預設 Provider（可能是記憶體或磁碟），接收整個表單資料
                // 通常用於小型檔案（圖片、文字欄位），資料讀入後常轉為 byte[] 再寫入本地

                // 🎯 影片上傳的情境：為了支援 Vimeo 的 TUS 串流協定
                // 我們主動提供一個 MemoryStream 的 Provider，讓系統將上傳資料寫入記憶體中

                var provider = new MultipartMemoryStreamProvider();
                // ⛳️ 將 multipart/form-data 的內容讀進 provider（由你準備）
                // 系統會將每個欄位（例如 video）分開儲存在 provider.Contents 裡
                await Request.Content.ReadAsMultipartAsync(provider);
                // 📌 尋找欄位名稱為 "video" 的檔案內容

                var file = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "video");
                if (file == null)
                    return BadRequest("未收到影片檔案");

                // 🎥 取得影片的串流（不一次吃進記憶體，而是邊讀邊傳）
                // Vimeo 的 tus 協定要求使用串流上傳
                var fileStream = await file.ReadAsStreamAsync();
                var fileSize = fileStream.Length;
                // 取得影片總大小（Vimeo 建立 tus session 時需要知道 size）

                Console.WriteLine($"影片大小: {fileSize} bytes");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", vimeoAccessToken);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    string videoTitle = $"AC_{Guid.NewGuid()}";

                    // 1️⃣ 建立 Vimeo 影片上傳請求

                    var requestBody = new
                    {
                        name = videoTitle,
                        upload = new
                        {
                            approach = "tus",
                            size = fileSize
                        }
                    };

                    var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                    var createVideoResponse = await client.PostAsync(vimeoUploadUrl, content);
                    if (!createVideoResponse.IsSuccessStatusCode)
                    {
                        string errorResponse = await createVideoResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"Vimeo API 錯誤 (POST): {errorResponse}");
                        return BadRequest($"無法創建 Vimeo 上傳: {errorResponse}");
                    }

                    var createVideoContent = await createVideoResponse.Content.ReadAsStringAsync();
                    JObject createVideoJson = JObject.Parse(createVideoContent);
                    string uploadUrl = createVideoJson["upload"]["upload_link"]?.ToString();
                    string videoUri = createVideoJson["uri"]?.ToString();
                    if (string.IsNullOrEmpty(uploadUrl))
                        return BadRequest("未能取得 Vimeo 上傳 URL");

                    // 2️⃣ 使用 PATCH 上傳影片檔案
                    using (var uploadClient = new HttpClient())
                    {
                        var uploadRequest = new HttpRequestMessage(HttpMethodExtensions.Patch, uploadUrl)
                        {
                            Content = new StreamContent(fileStream)
                        };
                        uploadRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/offset+octet-stream");
                        uploadRequest.Headers.Add("Tus-Resumable", "1.0.0");
                        uploadRequest.Headers.Add("Upload-Offset", "0");

                        var uploadResponse = await uploadClient.SendAsync(uploadRequest);
                        if (!uploadResponse.IsSuccessStatusCode)
                        {
                            string uploadError = await uploadResponse.Content.ReadAsStringAsync();
                            Console.WriteLine($"影片上傳失敗: {uploadError}");
                            return BadRequest($"影片上傳失敗: {uploadError}");
                        }

                        // 3️⃣ 確認影片已上傳完
                        var headRequest = new HttpRequestMessage(HttpMethod.Head, uploadUrl);
                        headRequest.Headers.Add("Tus-Resumable", "1.0.0");

                        var headResponse = await uploadClient.SendAsync(headRequest);
                        if (headResponse.IsSuccessStatusCode)
                        {
                            var uploadOffset = headResponse.Headers.GetValues("Upload-Offset").FirstOrDefault();
                            var uploadLength = headResponse.Headers.GetValues("Upload-Length").FirstOrDefault();

                            if (uploadOffset == uploadLength)
                            {
                                // 4️⃣ 查詢影片資訊（抓 duration）
                                var videoInfoResponse = await client.GetAsync($"https://api.vimeo.com{videoUri}");
                                var videoInfoJson = await videoInfoResponse.Content.ReadAsStringAsync();
                                JObject videoInfo = JObject.Parse(videoInfoJson);

                                //var duration = videoInfo["duration"]?.Value<decimal>();
                                var status = videoInfo["transcode"]?["status"]?.ToString();
                                recipe.RecipeVideoLink = videoUri;
                                //recipe.RecipeVideoDuration = (status == "complete") ? duration : null;
                                db.SaveChanges();
                                //--------------------試做refreshToken-----------------------------------
                                string token = userhash.GetRawTokenFromHeader();
                                var payload = JwtAuthUtil.GetPayload(token);
                                var newToken = jwt.ExpRefreshToken(payload);
                                return Ok(new
                                {
                                    message = "影片上傳成功",
                                    videoUri,
                                    //duration = recipe.RecipeVideoDuration,
                                    status,
                                    newToken = newToken,
                                });
                            }
                            else
                            {
                                Console.WriteLine("影片仍在上傳中...");
                                return BadRequest("影片尚未上傳完成，請稍後再試");
                            }
                        }
                    }

                    return BadRequest("影片 HEAD 檢查失敗");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"內部錯誤: {ex.Message}");
                return InternalServerError(ex);
            }
        }
        //---------------------------------------刪除食譜-----------------------------------------------
        [HttpDelete]
        [Route("api/recipes/{id}")]
        [JwtAuthFilter]
        public IHttpActionResult DeleteRecipe(int id)
        {
            var user = userhash.GetUserFromJWT();
            //這個寫法代表說食譜因為使用者沒對上所以找不到
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == id && r.UserId == user.Id);
            if (recipe == null)
            {
                return NotFound();
            }

            // 0️⃣ 刪除 RecipeTags（中介表）
            var recipeTags = db.RecipeTags.Where(rt => rt.RecipeId == id).ToList();
            db.RecipeTags.RemoveRange(recipeTags);

            // 1️⃣ 刪除 Ingredients
            var ingredients = db.Ingredients.Where(i => i.RecipeId == id).ToList();
            db.Ingredients.RemoveRange(ingredients);

            // 2️⃣ 刪除 RecipePhotos
            var recipePhotos = db.RecipePhotos.Where(rp => rp.RecipeId == id).ToList();
            db.RecipePhotos.RemoveRange(recipePhotos);

            // 3️⃣ 找出 Steps
            var steps = db.Steps.Where(s => s.RecipeId == id).ToList();
            var stepIds = steps.Select(s => s.Id).ToList();

            //// 3-1️⃣ 刪除 SubSteps
            //var subSteps = db.SubSteps.Where(ss => stepIds.Contains(ss.StepId)).ToList();
            //db.SubSteps.RemoveRange(subSteps);

            //// 3-2️⃣ 刪除 StepPhotos
            //var stepPhotos = db.StepPhotos.Where(sp => stepIds.Contains(sp.StepId)).ToList();
            //db.StepPhotos.RemoveRange(stepPhotos);

            // 3-3️⃣ 刪除 Steps
            db.Steps.RemoveRange(steps);
            //----------------------------------------------------------------------
            var ratings = db.Ratings.Where(r => r.RecipeId == id).ToList();
            db.Ratings.RemoveRange(ratings);
            var comments = db.Comments.Where(c => c.RecipeId == id).ToList();
            db.Comments.RemoveRange(comments);
            var favorites = db.Favorites.Where(f => f.RecipeId == id).ToList();
            db.Favorites.RemoveRange(favorites);
            // 4️⃣ 刪除 Recipe 本身
            db.Recipes.Remove(recipe);

            db.SaveChanges();
            //--------------------試做refreshToken-----------------------------------
            string token = userhash.GetRawTokenFromHeader();
            var payload = JwtAuthUtil.GetPayload(token);
            var newToken = jwt.ExpRefreshToken(payload);
            var res = new
            {
                StatusCode = 200,
                msg = "食譜及其相關資料刪除成功",
                Id = id,
                newToken = newToken
            };
            return Ok(res);
        }

        //---------------------------------------更新食譜步驟-----------------------------------------------
        [HttpPut]
        [Route("api/recipes/{id}/steps/bulk")]
        [JwtAuthFilter]
        public IHttpActionResult UpdateStepsBulk(int id, List<StepDto> steps)
        {
            var user = userhash.GetUserFromJWT();
            // 確認該食譜存在
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == id && r.UserId == user.Id);
            if (recipe == null)
                return NotFound();

            // 移除原本的所有步驟
            var originalSteps = db.Steps.Where(s => s.RecipeId == id).ToList();
            db.Steps.RemoveRange(originalSteps);

            // 新增新的步驟
            int order = 1;
            foreach (var dto in steps)
            {

                var step = new Steps();
                step.RecipeId = id;
                step.StepOrder = order++;
                step.StepDescription = dto.Description;
                step.VideoStart = dto.StartTime;
                step.VideoEnd = dto.EndTime;
                step.CreatedAt = DateTime.Now;
                step.UpdatedAt = DateTime.Now;
                db.Steps.Add(step);
            }

            db.SaveChanges();
            //--------------------試做refreshToken-----------------------------------
            string token = userhash.GetRawTokenFromHeader();
            var payload = JwtAuthUtil.GetPayload(token);
            var newToken = jwt.ExpRefreshToken(payload);

            return Ok(new
            {
                StatusCode = 200,
                msg = "步驟已成功更新（舊的已清除）",
                stepCount = steps.Count,
                newToken = newToken,
            });
        }
        //------------------------------------食譜發布狀態更新-----------------------------------
        [HttpPatch]
        [Route("api/recipes/{id}/publish")]
        [JwtAuthFilter]
        public IHttpActionResult UpdateIsPublished(int id, [FromBody] JObject data)
        {
            var user = userhash.GetUserFromJWT();
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == id && r.UserId == user.Id);
            if (recipe == null)
            {
                return NotFound();
            }

            // 取得 isPublished 值（從 JSON body 中）
            if (data["isPublished"] == null)
            {
                return BadRequest("缺少 isPublished 欄位");
            }

            bool isPublished = data["isPublished"].Value<bool>();
            recipe.IsPublished = isPublished;
            recipe.UpdatedAt = DateTime.Now;
            db.SaveChanges();


            string authHeader = HttpContext.Current.Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(); // 或你要的處理方式
            }
            //--------------------試做refreshToken-----------------------------------
            string token = userhash.GetRawTokenFromHeader();
            var payload = JwtAuthUtil.GetPayload(token);
            var newToken = jwt.ExpRefreshToken(payload);


            return Ok(new
            {
                StatusCode = 200,
                msg = $"食譜 {(isPublished ? "已發布" : "已取消發布")}",
                id = id,
                isPublished = isPublished,
                token = newToken // 新的 token 放這裡
            });
        }


        //----------多餘的和前端對完之後可以刪------------------------------------------------------------------------------------------
        [HttpPut]
        [Route("api/recipes/{id}/steps/{stepId}")]
        [JwtAuthFilter]
        public IHttpActionResult UpdateStep(int id, int stepId, StepDto stepData)
        {
            var user = userhash.GetUserFromJWT();
            //檢查源頭的主人是不是同一個人
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == id && r.UserId == user.Id);
            if (recipe == null)
            {
                return NotFound();
            }
            var step = db.Steps.FirstOrDefault(s => s.Id == stepId && s.RecipeId == id);
            if (step == null)
            {
                return NotFound();
            }


            step.StepDescription = stepData.Description;
            step.VideoStart = stepData.StartTime;
            step.VideoEnd = stepData.EndTime;
            step.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            //--------------------試做refreshToken-----------------------------------
            string token = userhash.GetRawTokenFromHeader();
            var payload = JwtAuthUtil.GetPayload(token);
            var newToken = jwt.ExpRefreshToken(payload);

            var res = new
            {
                StatusCode = 200,
                msg = $"步驟更新成功",
                stepId = stepId,
                newToken = newToken,
            };
            return Ok(res);
        }


        [HttpDelete]
        [Route("api/recipes/{id}/steps/{stepId}")]
        [JwtAuthFilter]
        public IHttpActionResult DeleteStep(int id, int stepId)
        {
            var user = userhash.GetUserFromJWT();
            //檢查源頭的主人是不是同一個人
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == id && r.UserId == user.Id);
            if (recipe == null)
            {
                return NotFound();
            }
            var step = db.Steps.FirstOrDefault(s => s.Id == stepId && s.RecipeId == id);
            bool hasData = step != null;
            if (hasData)
            {
                int deletedOrder = step.StepOrder;
                var followingSteps = db.Steps.Where(s => s.RecipeId == id && s.StepOrder > deletedOrder).ToList();

                foreach (var s in followingSteps)
                {
                    s.StepOrder -= 1;
                    s.UpdatedAt = DateTime.Now;
                }

                db.Steps.Remove(step);
                db.SaveChanges();
            }

            //--------------------試做refreshToken-----------------------------------
            string token = userhash.GetRawTokenFromHeader();
            var payload = JwtAuthUtil.GetPayload(token);
            var newToken = jwt.ExpRefreshToken(payload);

            var res = new
            {
                StatusCode = hasData ? 200 : 404,
                msg = hasData ? $"刪除成功" : $"步驟不存在",
                Id = stepId,
                newToken = newToken,
            };

            return Ok(res);
        }

        [HttpDelete]
        [Route("api/recipes/{recipeId}/ingredient/{ingredientId}")]
        [JwtAuthFilter]
        public IHttpActionResult DeleteIngredient(int recipeId, int ingredientId)
        {
            var user = userhash.GetUserFromJWT();
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == recipeId && r.UserId == user.Id);
            if (recipe == null)
            {
                return NotFound();
            }
            string ingName = "";
            //不想寫兩次所以用外鍵關聯的寫法
            var ingredient = db.Ingredients.FirstOrDefault(i => i.Id == ingredientId && i.RecipeId == recipeId);
            if (ingredient == null || recipe.UserId != user.Id)
            {
                return NotFound();
            }
            ingName = ingredient.IngredientName;
            db.Ingredients.Remove(ingredient);
            db.SaveChanges();
            //--------------------試做refreshToken-----------------------------------
            string token = userhash.GetRawTokenFromHeader();
            var payload = JwtAuthUtil.GetPayload(token);
            var newToken = jwt.ExpRefreshToken(payload);
            var res = new
            {
                StatusCode = 200,
                msg = $"{ingName}食材刪除成功",
                Id = ingredientId,
            };
            return Ok(res);
        }

    }
}
