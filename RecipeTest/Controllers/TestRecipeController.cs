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

namespace RecipeTest.Controllers
{
    public static class HttpMethodExtensions
    {
        public static readonly HttpMethod Patch = new HttpMethod("PATCH");
    }
    public class TestRecipeController : ApiController
    {
        private RecipeModel db = new RecipeModel();
        private string localStorragePath = HttpContext.Current.Server.MapPath("~/TestPhoto");
        private string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
        private string vimeoAccessToken = ConfigurationManager.AppSettings["VimeoAccessToken"];
        private string vimeoUploadUrl = "https://api.vimeo.com/me/videos";

        //上傳食譜
        [HttpPost]
        [Route("api/recipes")]
        public async Task<IHttpActionResult> PostRecipe()
        {
            bool isFormData = Request.Content.IsMimeMultipartContent();
            bool localStorageExist =Directory.Exists(localStorragePath);
            string recipeId = "";
            if (!isFormData)
            {
                return BadRequest("請使用multipart/form-data");
            }
            if (!localStorageExist)
            {
                Directory.CreateDirectory(localStorragePath);
            }

            var formData = new MultipartFormDataStreamProvider(localStorragePath);
            await Request.Content.ReadAsMultipartAsync(formData);

            string recipeName = formData.FormData["recipeName"];
            if (string.IsNullOrWhiteSpace(recipeName))
            {
                return BadRequest("食譜名稱為必填欄位");
            }
            using (var transaction = db.Database.BeginTransaction()) //原子操作避免併發
            {
                try
                {
                    var recipe = new Recipes
                    {
                        RecipeName = recipeName,
                        IsPublished = false,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        UserId = 1
                    };
                    db.Recipes.Add(recipe);
                    db.SaveChanges();
                    var photo = formData.FileData.FirstOrDefault();
                    if (photo != null)
                    {
                        recipeId = recipe.Id.ToString();
                        string filename = photo.Headers.ContentDisposition.FileName.Trim('\"');
                        string extension = Path.GetExtension(filename).ToLower();
                        if (!allowedExtensions.Contains(extension))
                        {
                            return BadRequest("檔案格式錯誤");
                        }
                        string newFileName = Guid.NewGuid().ToString("N") + extension;
                        string relativePath = "/TestPhoto/" + newFileName;
                        string fullPath = Path.Combine(localStorragePath, newFileName);
                        File.Move(photo.LocalFileName, fullPath);

                        var recipePhoto = new RecipePhotos
                        {
                            RecipeId = recipe.Id,
                            ImgUrl = relativePath,
                            IsCover = true,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                        };
                        db.RecipePhotos.Add(recipePhoto);
                        db.SaveChanges();
                        transaction.Commit(); //  一定要有這行，不然整段會回滾
                    }
                    var res = new
                    {
                        StatusCode = 200,
                        msg = "食譜新增成功",
                        Id = recipeId,
                    };
                    return Ok(res);
                    //圖片處理
                } catch (Exception ex)
                {
                    transaction.Rollback();//有錯不會新增這筆資料
                    var res = new
                    {
                        StatusCode = 500,
                        msg = "食譜新增失敗",
                        error = ex,
                    };
                    return Ok(res);
                }
            }       
        }
        //step2 上傳食譜細項(修改)
        [HttpPut]
        [Route("api/recipes/step2/{id}")]
        public IHttpActionResult UpdateRecipeDetail(int id, UserRecipeDetail recipeDetail)
        {
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == id);
            if ( recipe ==null)
            {
                return NotFound();
            }
            recipe.RecipeIntro = recipeDetail.RecipeIntro;
            recipe.CookingTime = recipeDetail.CookingTime;
            recipe.Portion = recipeDetail.Portion;
            recipe.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            var oldIngredients = db.Ingredients.Where(i => i.RecipeId == id).ToList();
            db.Ingredients.RemoveRange(oldIngredients);
            foreach(var ing in recipeDetail.Ingredients)
            {
                Console.WriteLine(ing.IngredientName);
                db.Ingredients.Add(new Ingredients
                {

                    RecipeId = id,
                    IngredientName = ing.IngredientName,
                    IsFlavoring = ing.IsFlavoring,
                    Amount = ing.IngredientAmount,
                    Unit = ing.IngredientUnit,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                });
            }
            db.SaveChanges();

            var oldTags = db.RecipeTags.Where(rt => rt.RecipeId == id).ToList();
            db.RecipeTags.RemoveRange(oldTags);

            foreach (var tag in recipeDetail.Tags)
            {
                // 嘗試找到既有 Tag
                var existingTag = db.Tags.FirstOrDefault(t => t.TagName == tag);
                // 若沒有則新增
                if (existingTag == null)
                {
                    existingTag = new Tags
                    {
                        TagName = tag,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    db.Tags.Add(existingTag);
                    db.SaveChanges(); 
                }
                // 建立中間關聯
                db.RecipeTags.Add(new RecipeTags
                {
                    RecipeId = id,
                    TagId = existingTag.Id,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }
            db.SaveChanges();
                var res = new
                {
                    StatusCode = 200,
                    msg = "食譜更新成功",
                    Id = id,
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
        [HttpPut]
        [Route("api/recipes/{id}/video")]
        public async Task<IHttpActionResult> UploadToVimeo(int id)
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                    return BadRequest("請使用 Multipart 表單上傳影片");

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);
                var file = provider.Contents.FirstOrDefault();
                if (file == null)
                    return BadRequest("未收到影片檔案");

                var fileStream = await file.ReadAsStreamAsync();
                var fileSize = fileStream.Length;
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
                    var createVideoResponse = await client.PostAsync("https://api.vimeo.com/me/videos", content);
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

                                var recipe = db.Recipes.FirstOrDefault(r => r.Id == id);
                                if (recipe == null) return NotFound();

                                recipe.RecipeVideoLink = videoUri;
                                //recipe.RecipeVideoDuration = (status == "complete") ? duration : null;
                                db.SaveChanges();

                                return Ok(new
                                {
                                    message = "影片上傳成功",
                                    videoUri,
                                    //duration = recipe.RecipeVideoDuration,
                                    status
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
        [HttpDelete]
        [Route("api/recipes/{id}")]
        public IHttpActionResult DeleteRecipe(int id)
        {
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == id);
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

            // 3-1️⃣ 刪除 SubSteps
            var subSteps = db.SubSteps.Where(ss => stepIds.Contains(ss.StepId)).ToList();
            db.SubSteps.RemoveRange(subSteps);

            // 3-2️⃣ 刪除 StepPhotos
            var stepPhotos = db.StepPhotos.Where(sp => stepIds.Contains(sp.StepId)).ToList();
            db.StepPhotos.RemoveRange(stepPhotos);

            // 3-3️⃣ 刪除 Steps
            db.Steps.RemoveRange(steps);

            // 4️⃣ 刪除 Recipe 本身
            db.Recipes.Remove(recipe);

            db.SaveChanges();

            var res = new
            {
                StatusCode = 200,
                msg = "食譜及其相關資料刪除成功",
                Id = id
            };

            return Ok(res);
        }



        //刪除食材
        [HttpDelete]
        [Route("api/recipes/{recipeId}/ingredient/{ingredientId}")]
        public IHttpActionResult DeleteIngredient(int recipeId, int ingredientId)
        {
            var ingredient = db.Ingredients.FirstOrDefault(i => i.Id == ingredientId && i.RecipeId == recipeId);
            if (ingredient == null)
            {
                return NotFound();
            }
            db.Ingredients.Remove(ingredient);
            db.SaveChanges();
            var res = new
            {
                StatusCode = 200,
                msg = "食材刪除成功",
                Id = ingredientId,
            };
            return Ok(res);
        }

        //首頁get

        [HttpGet]
        [Route("api/recipes")]
        public IHttpActionResult GetRecipes(string type="latest", int page =1)
        {
            const int pageSize = 5;
            IQueryable<Recipes> query = db.Recipes;
            switch (type.ToLower())
            {
                case "latest":
                    query = query.OrderByDescending(r=>r.CreatedAt);
                    break;
                case "popular":
                    query = query.OrderByDescending(r => r.Rating);
                    break;
                case "classic": //目前還不知道classic的定義所以先用id代替
                    query = query.OrderBy(r => r.Id);
                    break;
                default:
                    return BadRequest("不支援type參數");

            }
            var paged = query.Skip((page - 1) * pageSize).Take(pageSize).Select(r => new
            {
                r.Id,
                r.RecipeName,
                r.RecipeIntro,
                r.Portion,
                r.CookingTime,
                r.Rating,
            }).ToList();

            var res = new
            {
                StatusCode = 200,
                msg = "獲取首頁顯示食譜",
                data = paged,
            };

            return Ok(res);         
        }

        //食譜內頁get//食譜細項
        [HttpGet]
        [Route("api/recipes/{id}")]
        public IHttpActionResult GetRecipe(int id)
        {
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == id);
            if (recipe == null)
            {
                return NotFound();
            }
            var recipeAndCover = db.Recipes
                .Where(r => r.Id == id)
                .Select(r => new
                {
                    id = r.Id,
                    isPublished= r.IsPublished,
                    title = r.RecipeName,
                    coverPhoto = r.RecipesPhotos.FirstOrDefault(p => p.IsCover == true).ImgUrl,
                    description = r.RecipeIntro,
                    cookingTime = r.CookingTime,
                    portion = r.Portion,
                    rating = r.Rating,
                    videoId = r.RecipeVideoLink,
                })
                .FirstOrDefault();
            if (recipeAndCover == null)
            {
                return NotFound();
            }
            var RecipeWithIngredient = new
            {
                recipe = recipeAndCover,
                Ingredients = db.Ingredients
                    .Where(i => i.RecipeId == id)
                    .Select(i => new
                    {
                        ingredientId = i.Id,
                        ingredientName = i.IngredientName,
                        ingredientAmount = i.Amount,
                        ingredientUnit = i.Unit,
                        ingredientFlavoring = i.IsFlavoring
                    })
            };

            var RecipeWithTags = new
            {
                recipe = RecipeWithIngredient.recipe,
                ingredients = RecipeWithIngredient.Ingredients,
                tags = db.RecipeTags
                    .Where(rt => rt.RecipeId == id)
                    .Select(rt => rt.Tags.TagName)
            };

            var RecipeWithSteps = new
            {
                recipe = RecipeWithTags.recipe,
                ingredients = RecipeWithTags.ingredients,
                tags = RecipeWithTags.tags,
                steps = db.Steps.Where(s => s.RecipeId == id).Select(s => new
                {
                    id = s.Id,
                    description = s.StepDescription,
                    stepOrder = s.StepOrder,
                    startTime = s.VideoStart,
                    endTime = s.VideoEnd,
                })


            };

            var res = new
            {
                StatusCode = 200,
                data = RecipeWithSteps
            };
            return Ok(res);
        }


        [HttpPost]
        [Route("api/recipes/{id}/steps")]
        public IHttpActionResult CreateStep(int id)
        {
            var recipeExists = db.Recipes.Any(r => r.Id == id);
            if (!recipeExists)
            {
                return NotFound(); // 或 BadRequest("該食譜不存在")
            }

            int latestOrder = db.Steps.Where(s => s.RecipeId == id)
                .Select(s => s.StepOrder)
                .DefaultIfEmpty(0)
                .Max();
            decimal startTime = db.Steps.Where(s => s.RecipeId == id)
            .Select(s => s.StepOrder)
            .DefaultIfEmpty(0)
            .Max();
            int defaultDuration = 10; // 預設 10 秒

            var step = new Steps
            {
                RecipeId = id,
                StepOrder = latestOrder + 1,
                VideoStart = Convert.ToInt32(startTime),
                VideoEnd = Convert.ToInt32(startTime + defaultDuration),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            db.Steps.Add(step);
            db.SaveChanges();

            var res = new
            {
                StatusCode = 200,
                msg = $"步驟{step.Id}新增成功",
                stepId = step.Id,
            };


            return Ok(res);
        }

        [HttpPut]
        [Route("api/recipes/{id}/steps/bulk")]
        public IHttpActionResult UpdateStepsBulk(int id, List<StepDto> steps)
        {
            // 確認該食譜存在
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == id);
            if (recipe == null)
                return NotFound();

            // 移除原本的所有步驟
            var originalSteps = db.Steps.Where(s => s.RecipeId == id).ToList();
            db.Steps.RemoveRange(originalSteps);

            // 新增新的步驟
            int order = 1;
            foreach (var dto in steps)
            {
                db.Steps.Add(new Steps
                {
                    RecipeId = id,
                    StepOrder = order++,
                    StepDescription = dto.Description,
                    VideoStart = dto.StartTime,
                    VideoEnd = dto.EndTime,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }

            db.SaveChanges();

            return Ok(new
            {
                StatusCode = 200,
                msg = "步驟已成功更新（舊的已清除）",
                stepCount = steps.Count
            });
        }


        //[HttpPost]
        //[Route("api/recipes/{id}/steps/bulk")]
        //public IHttpActionResult CreateStepsBulk(int id, List<StepDto> steps)
        //{
        //    if (!db.Recipes.Any(r => r.Id == id)) return NotFound();

        //    int order = 1;
        //    foreach (var dto in steps)
        //    {
        //        db.Steps.Add(new Steps
        //        {
        //            RecipeId = id,
        //            StepOrder = order++,
        //            StepDescription = dto.Description,
        //            VideoStart = dto.StartTime,
        //            VideoEnd = dto.EndTime,
        //            CreatedAt = DateTime.Now,
        //            UpdatedAt = DateTime.Now
        //        });
        //    }

        //    db.SaveChanges();

        //    return Ok(new
        //    {
        //        StatusCode = 200,
        //        msg = "所有步驟新增完成",
        //        stepCount = steps.Count
        //    });
        //}

        [HttpPatch]
        [Route("api/recipes/{id}/publish")]
        public IHttpActionResult UpdateIsPublished(int id, [FromBody] JObject data)
        {
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == id);
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

            return Ok(new
            {
                StatusCode = 200,
                msg = $"食譜 {(isPublished ? "已發布" : "已取消發布")}",
                id = id,
                isPublished = isPublished
            });
        }


        [HttpPut]
        [Route("api/recipes/{id}/steps/{stepId}")]
        public IHttpActionResult UpdateStep(int id, int stepId, StepDto stepData)
        {
            var step =  db.Steps.FirstOrDefault(s => s.Id == stepId && s.RecipeId == id);
            if (step == null)
            {
                return NotFound();
            }

            
            step.StepDescription = stepData.Description;
            step.VideoStart = stepData.StartTime;
            step.VideoEnd = stepData.EndTime;
            step.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            var res = new
            {
                StatusCode = 200,
                msg = $"步驟更新成功",
                stepId = stepId,
            };
            return Ok(res);
        }

        [HttpDelete]
        [Route("api/recipes/{id}/steps/{stepId}")]
        public IHttpActionResult DeleteStep(int id, int stepId)
        {
            var step = db.Steps.FirstOrDefault(s => s.Id == stepId && s.RecipeId == id);
            bool hasData = step != null;
            if (hasData)
            {
                int deletedOrder = step.StepOrder;
                var followingSteps = db.Steps.Where(s => s.RecipeId == id && s.StepOrder > deletedOrder).ToList();

                foreach(var s in followingSteps)
                {
                    s.StepOrder -= 1;
                    s.UpdatedAt = DateTime.Now;
                }

                db.Steps.Remove(step);
                db.SaveChanges();
            }

            var res = new
            {
                StatusCode = hasData ? 200 : 404,
                msg = hasData ? $"刪除成功" : $"步驟不存在",
                Id=stepId,
            };

            return Ok(res);
        }
    }
}
