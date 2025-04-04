using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using RecipeTest.Models;
using RecipeTest.Pages;
using RecipeTest.Security;
using static RecipeTest.Pages.UserRelated;
using MyWebApiProject.Security;
using System.Web;
using System.Threading.Tasks;
using System.IO;
using MailKit.Search;
using Org.BouncyCastle.Bcpg;
using System.Web.Http.Results;


namespace RecipeTest.Controllers
{
    public class UserController : ApiController
    {
        private RecipeModel db = new RecipeModel();
        private UserEncryption userhash = new UserEncryption();
        private string localStorragePath = HttpContext.Current.Server.MapPath("~/UserPhotos");
        private string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
        //-------------------------------------------------------------------------------------------
        //這隻是會判斷是否為作者根據它顯示不同的profile
        [HttpGet]
        [Route("api/user/{displayId}")]
        public IHttpActionResult getUserProfileData(string displayId)
        {
            bool isMe = false;
            var user = db.Users.FirstOrDefault(u => u.DisplayId == displayId);
            if (user == null)
            {
                return NotFound();
            }
            //若有帶token(代表有使用者登入，若是一般瀏覽者則不需要)
            var token = Request.Headers.Authorization?.Parameter;
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    isMe = userhash.IsSelf(Request.Headers.Authorization?.Parameter, displayId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Token 無法解析：" + ex.Message);
                    // 保持 isMe 為 false
                }
            }

            //不是我
            var userProfileData = new
            {
                userId = user.Id,
                displayId = user.DisplayId,
                accountName = user.AccountName,
                profilePhoto = user.AccountProfilePhoto,
                userIntro = user.UserIntro,
                recipeCount = db.Recipes.Count(r => r.UserId == user.Id),
                followerCount = db.Follows.Count(f => f.FollowedUserId == user.Id),
            };
            //是我
            var recipesByUser = db.Recipes.Where(r => r.UserId == user.Id).ToList();
            var publishedWithRating = recipesByUser.Where(r => r.Rating > 0);

            var authorProfileData = new
            {
                userId = user.Id,
                displayId = user.DisplayId,
                accountName = user.AccountName,

                followingCount = db.Follows.Count(f => f.UserId == user.Id),
                followerCount = db.Follows.Count(f => f.FollowedUserId == user.Id),

                favoritedTotal = recipesByUser.Sum(r => r.Favorites.Count),
                myFavoriteCount = db.Favorites.Count(f => f.UserId == user.Id),

                averageRating = publishedWithRating.Any()
                    ? Math.Round(publishedWithRating.Average(r => r.Rating), 1)
                    : 0,

                totalViewCount = recipesByUser.Sum(r => r.ViewCount)
            };
            return Ok(new
            {
                StatusCode = 200,
                isMe = isMe, // ✅ 回傳是否是本人
                userData = isMe ? null : userProfileData,
                authorData = isMe ? authorProfileData : null,
            });
        }
        //-------------------------------------------------------------------------------------
        //這個是給一般瀏覽者可以看得

        [HttpGet]
        [Route("api/user/{displayId}/recipes")]
        public IHttpActionResult GetUserRecipes(string displayId, int page = 1)
        {
            var user = db.Users.FirstOrDefault(u => u.DisplayId == displayId);
            bool hasUser = user != null;
            if (!hasUser)
            {
                return NotFound();
            }
            int pageSize = 3;
            var totalCount = db.Recipes.Count(r => r.UserId == user.Id && r.IsPublished);
            int skip = (page - 1) * pageSize;
            bool hasMore = page * pageSize < totalCount; // 判斷是否還有下一頁

            // ✅ 取得封面圖片的食譜列表（略）
            var recipes = db.Recipes
                .Where(r => r.UserId == user.Id && r.IsPublished)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(r => new
                {
                    recipeId = r.Id,
                    title = r.RecipeName,
                    description = r.RecipeIntro,
                    portion = r.Portion,
                    cookTime = r.CookingTime,
                    rating = r.Rating,
                    coverPhoto = db.RecipePhotos
                        .Where(p => p.RecipeId == r.Id && p.IsCover)
                        .Select(p => p.ImgUrl)
                        .FirstOrDefault()
                })
                .ToList();
            var res = new
            {
                statusCode = 200,
                hasMore = hasMore,
                recipeCount = totalCount,
                recipes,
            };

            return Ok(res);
        }
        //-------------------------------------------------------------------------------------------------
        //這隻是讓作者看自己收藏的食譜(和前端討論是否要整合到上面)
        [HttpGet]
        [Route("api/user/{displayId}/author-recipes")]
        [JwtAuthFilter]
        public IHttpActionResult GetAuthorRecipes(string displayId, bool isPublished = true, int skip = 0, int take = 3) //預設是看有發布的要看未發布的要另外加
        {
            var user = userhash.GetUserFromJWT();
            if (user.DisplayId != displayId)
            {
                return Unauthorized(); // 防止別人偷打
            }

            var recipes = db.Recipes.Where(r => r.UserId == user.Id && r.IsPublished == isPublished).OrderByDescending(r => r.CreatedAt);
            int totalCount = recipes.Count();
            bool hasMore = (skip + take) < totalCount;
            var result = recipes.Skip(skip).Take(take).ToList();
            var data = result.Select(r => new
            {
                recipeId = r.Id,
                title = r.RecipeName,
                description = r.RecipeIntro,
                isPublished = r.IsPublished,
                sharedCount = r.SharedCount,
                rating = r.Rating,
                viewCount = r.ViewCount,
                averageRating = Math.Round
                (
                    db.Ratings
               .Where(rt => rt.RecipeId == r.Id)
               .Select(rt => rt.Rating)
               .DefaultIfEmpty(0)
               .Average(), 1),
                commentCount = db.Comments.Count(c => c.RecipeId == r.Id),
                favoritedCount = db.Favorites.Count(f => f.RecipeId == r.Id),
                image = db.RecipePhotos
             .Where(p => p.RecipeId == r.Id && p.IsCover)
             .Select(p => p.ImgUrl)
             .FirstOrDefault(),
            }).ToList();


            return Ok(new
            {
                statusCode = 200,
                totalCount = totalCount,
                data = data
            });
        }
        //-----------------------------獲取以收藏食譜(作者)----------------------
        [HttpGet]
        [Route("api/user/{displayId}/author-favorite-follow")]
        [JwtAuthFilter]
        public IHttpActionResult GetUserFavoritesAndFollows(
           string displayId,
           string table = "favorite",
           int skip = 0,
           int take = 3)
        {
            bool hasMore = false;
            var user = userhash.GetUserFromJWT();
            if (user.DisplayId != displayId)
            {
                return Unauthorized(); // 防止別人偷打
            }

            int totalCount = 0;
            object result = null;
            string msg = "";

            if (table == "follow")
            {
                var data = db.Follows.Where(f => f.UserId == user.Id);
                totalCount = data.Count();
                hasMore = (skip + take) < totalCount;
                result = data.OrderByDescending(f => f.CreatedAt).Skip(skip).Take(take).Select(f => new
                {
                    id = f.FollowedUser.Id,
                    displayId = f.FollowedUser.DisplayId,
                    name = f.FollowedUser.AccountName,
                    profilePhoto = f.FollowedUser.AccountProfilePhoto,
                    description = f.FollowedUser.UserIntro,
                    followedUserRecipeCount = f.FollowedUser.Recipes.Count(),
                    followedUserFollowerCount = f.FollowedUser.Followers.Count(),
                }).ToList();
                msg = $"追蹤人數共 {totalCount} 人";
            }
            else if (table == "favorite")
            {
                var data = db.Favorites.Where(f => f.UserId == user.Id);
                totalCount = data.Count();
                hasMore = (skip + take) < totalCount;
                result = data.OrderByDescending(f=>f.CreatedAt).Skip(skip).Take(take).Select(f => new
                {
                    id = f.Recipe.Id,
                    displayId = f.Recipe.DisplayId,
                    recipeName = f.Recipe.RecipeName,
                    description = f.Recipe.RecipeIntro,
                    portion = f.Recipe.Portion,
                    cookingTime = f.Recipe.CookingTime,
                    rating = f.Recipe.Rating,
                    coverPhoto = f.Recipe.RecipesPhotos
                        .Where(p => p.IsCover)
                        .OrderBy(p => p.CreatedAt)
                        .Select(p => p.ImgUrl)
                        .FirstOrDefault()
                }).ToList();
                msg = $"已收藏 {totalCount} 筆";
            }
            else
            {
                return BadRequest("無效的表格類型");
            }

            return Ok(new
            {
                StatusCode = 200,
                hasMore,
                msg,
                totalCount,
                data = result
            });
        }



        //-------------------------------------------------------------------------------------------------------
        //這隻是讓使用者修改自己的資料的
        [HttpPut]
        [Route("api/user/profile")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> EditUser()
        {
            var user = userhash.GetUserFromJWT();
            var existingUser = db.Users.FirstOrDefault(u => u.Id == user.Id);
            bool hasUser = existingUser != null;
            if (existingUser == null)
            {
                return NotFound();
            }
            var provider = await Request.Content.ReadAsMultipartAsync();
            var formData = provider.Contents;
            string accountName = formData.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "accountName")?.ReadAsStringAsync().Result;
            string accountIntro = formData.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "userIntro")?.ReadAsStringAsync().Result;
            var photoContent = formData.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "profilePhoto");
            if (!string.IsNullOrEmpty(accountName))
            {
                existingUser.AccountName = accountName;
            }
            if (!string.IsNullOrEmpty(accountIntro))
            {
                existingUser.UserIntro = accountIntro;
            }
            if (photoContent != null && photoContent.Headers.ContentDisposition.FileName != null)
            {
                var file = photoContent.Headers.ContentDisposition.FileName.Trim('"');
                string extension = Path.GetExtension(file).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("只允許上傳 .jpg, .jpeg, .png檔案");
                }

                string newFileName = Guid.NewGuid().ToString("N") + extension;
                string relativePath = "/UserPhotos/" + newFileName;
                string fullPath = Path.Combine(localStorragePath, newFileName);
                var fileBytes = await photoContent.ReadAsByteArrayAsync();
                existingUser.AccountProfilePhoto = relativePath;
                File.WriteAllBytes(fullPath, fileBytes); // ⬅️ 真正把圖片寫進磁碟

            }
            existingUser.UpdatedAt = DateTime.Now;
            db.SaveChanges();
            return Ok(new
            {
                StatusCode = 200,
                msg = "使用者資料已經成功更新",
                data = new
                {
                    accountName = existingUser.AccountName,
                    userIntro = existingUser.UserIntro,
                    profilePhoto = existingUser.AccountProfilePhoto,
                }
            });
        }

        [HttpDelete]
        [Route("api/deleteUser/{id}")]
        public IHttpActionResult deleteUser(int id)
        {
            var user = db.Users.FirstOrDefault(u => u.Id == id);
            bool hasUser = user != null;

            var res = new
            {
                StatusCode = hasUser ? 200 : 400,
                msg = hasUser ? "已刪除使用者" : "查無使用者",
            };
            if (hasUser)
            {
                db.Users.Remove(user);
                db.SaveChanges();
            }

            return Ok(res);
        }

        [HttpGet]
        [Route("api/user/profile")]
        public IHttpActionResult BrowseUserFile(string displayId)
        {
            if (string.IsNullOrEmpty(displayId))
            {
                return BadRequest("DisplayId是必填");
            }
            var user = db.Users.FirstOrDefault(u => u.DisplayId == displayId);
            return Ok();
        }
        [HttpGet]
        [Route("api/users")]
        public IHttpActionResult showUser()
        {
            var users = db.Users
                                .Select(u => new { u.Id, u.AccountName, u.AccountEmail , u.DisplayId})
                                .ToList();
            var res = new
            {
                StatusCode = 200,
                msg = "使用者資料獲取成功",
                data = users
            };
            return Ok(res);
        }

    }
}
