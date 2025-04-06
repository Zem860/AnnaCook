using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Jose;
using MyWebApiProject.Security;
using Newtonsoft.Json;
using RecipeTest.Models;
using RecipeTest.Security;
using static RecipeTest.Pages.RecipeRelated;

namespace RecipeTest.Controllers
{
    public class UnsureController : ApiController
    {

        RecipeModel db = new RecipeModel();
        UserEncryption userhash = new UserEncryption();
        private string localStorragePath = HttpContext.Current.Server.MapPath("~/TestPhoto");
        private string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
        JwtAuthUtil jwt = new JwtAuthUtil();

        //--------------送出草稿----------------


        [HttpPost]
        [Route("api/recipes/{id}/submit-draft")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> SubmitRecipeDraft(int id)
        {
            var user = userhash.GetUserFromJWT();
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == id && r.UserId == user.Id);
            if (recipe == null)
            {
                return NotFound();
            }

            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                    return BadRequest("請使用 multipart/form-data");

                if (!Directory.Exists(localStorragePath))
                {
                    Directory.CreateDirectory(localStorragePath);
                }

                var provider = await Request.Content.ReadAsMultipartAsync();
                var contents = provider.Contents;

                // 更新名稱與封面
                string recipeName = contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "recipeName")?.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(recipeName))
                    return BadRequest("缺少食譜名稱");

                recipe.RecipeName = recipeName;
                recipe.UpdatedAt = DateTime.Now;

                var photo = contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "photo");
                if (photo != null)
                {
                    string fileName = photo.Headers.ContentDisposition.FileName.Trim('"');
                    string extension = Path.GetExtension(fileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                        return BadRequest("檔案格式錯誤");

                    string newFileName = Guid.NewGuid().ToString("N") + extension;
                    string relativePath = "/TestPhoto/" + newFileName;
                    string fullPath = Path.Combine(localStorragePath, newFileName);
                    byte[] fileBytes = await photo.ReadAsByteArrayAsync();
                    File.WriteAllBytes(fullPath, fileBytes);

                    var recipePhotos = db.RecipePhotos.Where(rp => rp.RecipeId == id).ToList();
                    foreach (var rp in recipePhotos)
                    {
                        rp.IsCover = false;
                        rp.UpdatedAt = DateTime.Now;
                    }

                    db.RecipePhotos.Add(new RecipePhotos
                    {
                        RecipeId = id,
                        ImgUrl = relativePath,
                        IsCover = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }

                // 更新詳細資訊
                var detailContent = contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "detail")?.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(detailContent))
                {
                    var recipeDetail = JsonConvert.DeserializeObject<UserRecipeDetail>(detailContent);
                    recipe.RecipeIntro = recipeDetail.RecipeIntro;
                    recipe.CookingTime = recipeDetail.CookingTime;
                    recipe.Portion = recipeDetail.Portion;

                    db.Ingredients.RemoveRange(db.Ingredients.Where(i => i.RecipeId == id));
                    foreach (var ing in recipeDetail.Ingredients)
                    {
                        db.Ingredients.Add(new Ingredients
                        {
                            RecipeId = id,
                            IngredientName = ing.IngredientName,
                            IsFlavoring = ing.IsFlavoring,
                            Amount = ing.IngredientAmount,
                            Unit = ing.IngredientUnit,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    db.RecipeTags.RemoveRange(db.RecipeTags.Where(rt => rt.RecipeId == id));
                    foreach (var tag in recipeDetail.Tags)
                    {
                        var normalizedTag = tag.Trim().ToLower();
                        var existingTag = db.Tags.FirstOrDefault(t => t.TagName.ToLower() == normalizedTag);
                        if (existingTag == null)
                        {
                            existingTag = new Tags
                            {
                                TagName = tag.Trim(),
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };
                            db.Tags.Add(existingTag);
                            db.SaveChanges();
                        }

                        db.RecipeTags.Add(new RecipeTags
                        {
                            RecipeId = id,
                            TagId = existingTag.Id,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }
                }

                // 更新步驟
                var stepsContent = contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "steps")?.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(stepsContent))
                {
                    var steps = JsonConvert.DeserializeObject<List<StepDto>>(stepsContent);
                    db.Steps.RemoveRange(db.Steps.Where(s => s.RecipeId == id));
                    int order = 1;
                    foreach (var step in steps)
                    {
                        db.Steps.Add(new Steps
                        {
                            RecipeId = id,
                            StepOrder = order++,
                            StepDescription = step.Description,
                            VideoStart = step.StartTime,
                            VideoEnd = step.EndTime,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }
                }

                db.SaveChanges();

                string token = userhash.GetRawTokenFromHeader();
                var payload = JwtAuthUtil.GetPayload(token);
                var newToken = jwt.ExpRefreshToken(payload);

                return Ok(new
                {
                    StatusCode = 200,
                    msg = "草稿已送出",
                    recipeId = recipe.Id,
                    newToken = newToken
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    StatusCode = 500,
                    msg = "草稿送出失敗",
                    error = ex.Message
                });
            }
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

    }
}
