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
using Antlr.Runtime.Tree;
using Jose;
using Org.BouncyCastle.Asn1.Crmf;
using RecipeTest.Enums;
namespace RecipeTest.Controllers
{
    public class UsersRecipeController : ApiController
    {
        private RecipeModel db = new RecipeModel();
        private UserEncryption userhash = new UserEncryption();
        private JwtAuthUtil jwt = new JwtAuthUtil();
        //--------------分享食譜----------------
        [HttpPost]
        [Route("api/recipes/{id}/share")]
        [JwtAuthFilter]
        public IHttpActionResult ShareRecipe(int id)
        {
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == id && !r.IsDeleted && r.IsPublished && !r.IsArchived);
            if (recipe == null)
            {
                return Ok(new { statusCode = 404, msg = "找不到該食譜或無法進行此操作" });
            }

            // 限制只能分享公開食譜
            if (!recipe.IsPublished)
            {
                return Ok(new { StatusCode = 400, msg = "尚未公開的食譜無法分享" });
            }

            recipe.SharedCount += 1;
            db.SaveChanges();

            return Ok(new
            {
                StatusCode = 200,
                msg = "分享成功"
            });
        }

        //---------------------獲取食譜評分評論------------------------------

        [HttpGet]
        [Route("api/recipes/{recipeId}/rating-comment")]
        public IHttpActionResult getRatingComment(int recipeId, int number = 1)
        {
            var recipeComment = db.Comments.Where(c => c.RecipeId == recipeId &&!c.Users.IsDeleted&&!c.Users.IsBanned);

            if (!recipeComment.Any())
            {
                return Ok(new { StatusCode=400, msg="未找到任何留言"});
            }

            var recipe = db.Recipes.FirstOrDefault(r => r.Id == recipeId && !r.IsDeleted && r.IsPublished && !r.IsArchived);
            if (recipe == null)
            {
                return Ok(new { statusCode = 404, msg = "找不到該食譜或無法進行此操作" });
            }

            int pageSize = 3;
            int skip = (number - 1) * pageSize;
            var totalCount = recipeComment.Count();
            var data = recipeComment
                .OrderByDescending(rc => rc.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(c => new
                {
                    commentId = c.Id,
                    displayId =c.Users.DisplayId,
                    authorName = c.Users.AccountName,
                    authorPhoto = c.Users.AccountProfilePhoto,
                    comment = c.CommentContent,
                    rating = (int)Math.Round(db.Ratings
                        .Where(r => r.UserId == c.Users.Id && r.RecipeId == recipeId)
                        .Select(r => r.Rating)
                        .FirstOrDefault(),0),
                })
                .ToList(); // 記得轉成 List 才能正確 return JSON

            var res = new
            {
                StatusCode = 200,
                msg = "成功獲取食譜留言",
                totalCount = totalCount,
                hasMore = skip + pageSize < totalCount,
                data = data,
            };

            return Ok(res);
        }

        //---------------------編輯評分評論------------------------------
        [HttpPut]
        [Route("api/recipes/{recipeId}/rating-comment")] //ok
        [JwtAuthFilter]
        public IHttpActionResult EditRatingAndComment(int recipeId, ComplexUserRecipe.RatingCommentDto dto)
        {
            var user = userhash.GetUserFromJWT();
            var userData = db.Users.FirstOrDefault(u => u.Id == user.Id);
            //檢查本使用者是否有權限問題
            var checkUser = new UserEncryption();
            var statusCheck = checkUser.GetUserStatusErrorMessage(userData);
            if (statusCheck != null) return Ok(new { statusCode = 403, msg = statusCheck });

            var recipe = db.Recipes.FirstOrDefault(r => r.Id == recipeId && !r.IsDeleted && r.IsPublished && !r.IsArchived);
            if (recipe == null)
            {
                return Ok(new { statusCode = 404, msg = "找不到該食譜或無法進行此操作" });
            }

            if (dto.Rating == 0)
            {
                return Ok(new { StatusCode = 401, msg = "你尚未對食譜平分" });
            }
            if (String.IsNullOrEmpty(dto.CommentContent))
            {
                return Ok(new { StatusCode = 401, msg = "您尚未對食譜有任何留言" });
            }
            var rating = db.Ratings.FirstOrDefault(r => r.RecipeId == recipeId && r.UserId == user.Id);
            if (rating == null)
            {
                return Ok(new {StatusCode=400,msg="找不到您的評分紀錄"});
            }

            rating.Rating = dto.Rating;
            rating.UpdatedAt = DateTime.Now;

            var comment = db.Comments.FirstOrDefault(c => c.RecipeId == recipeId && c.UserId == user.Id);
            if (comment == null)
            {
                return BadRequest("找不到您的留言紀錄");
            }

            comment.CommentContent = dto.CommentContent;
            comment.UpdatedAt = DateTime.Now;

            db.SaveChanges();


            return Ok(new {StatusCode = 400, msg="修改留言成功", Id=recipeId});
        }
        //---------------------新增食譜評分與評論------------------------------

        [HttpPost]
        [Route("api/recipes/{recipeId}/rating-comment")]
        [JwtAuthFilter]
        public IHttpActionResult AddRatingAndComment(int recipeId, ComplexUserRecipe.RatingCommentDto dto)
        {
            var user = userhash.GetUserFromJWT();
            var userData = db.Users.FirstOrDefault(u => u.Id == user.Id);
            //檢查本使用者是否有權限問題
            var checkUser = new UserEncryption();
            var statusCheck = checkUser.GetUserStatusErrorMessage(userData);
            if (statusCheck != null) return Ok(new { statusCode = 403, msg = statusCheck });

            var recipe = db.Recipes.FirstOrDefault(r => r.Id == recipeId && !r.IsDeleted && r.IsPublished && !r.IsArchived);
            if (recipe == null)
            {
                return Ok(new { statusCode = 404, msg = "找不到該食譜或無法進行此操作" });
            }

            if (recipe.UserId == user.Id)
            {
                return Ok(new { StatusCode = 400, msg= "自己不能給自己評分" });
            }

            // 檢查是否已評分
            var rating = db.Ratings.FirstOrDefault(r => r.UserId == user.Id && r.RecipeId == recipeId);
            bool hasRating = (rating != null);
            if (hasRating)
            {
                return BadRequest("你已經評分過了");
            } else
            {
                try
                {
                    rating = new Ratings();
                    rating.UserId = user.Id;
                    rating.RecipeId = recipeId;
                    rating.Rating = dto.Rating;
                    rating.CreatedAt = DateTime.Now;
                    rating.UpdatedAt = DateTime.Now;
                    db.Ratings.Add(rating);

                    bool hasContent = (!string.IsNullOrEmpty(dto.CommentContent));
                    if (hasContent)
                    {
                        var comment = new Comments();
                        comment.UserId = user.Id;
                        comment.RecipeId = recipeId;
                        comment.CommentContent = dto.CommentContent;
                        comment.CreatedAt = DateTime.Now;
                        comment.UpdatedAt = DateTime.Now;
                        db.Comments.Add(comment);
                    }

                    db.SaveChanges();
                    //計算平均評分
                    var ratings = db.Ratings.Where(r => r.RecipeId == recipeId);
                    var avg = Math.Round(ratings.Average(r => r.Rating), 1);
                    recipe.Rating = (decimal)avg;
                    db.SaveChanges(); // 儲存所有變更
                   //--------------------試做refreshToken-----------------------------------
                    string token = userhash.GetRawTokenFromHeader();
                    var payload = JwtAuthUtil.GetPayload(token);
                    var newToken = jwt.ExpRefreshToken(payload);
                    var res = new
                    {
                        msg = "評分與留言成功",
                        data = new
                        {
                            recipeId = recipeId,
                            rating = dto.Rating,
                            comment = dto.CommentContent,
                            newToken = newToken,
                        }
                    };

                    return Ok(res);
                }
                catch(Exception ex)
                {
                    return Ok(new
                    {
                        msg = "評分失敗",
                        error = ex.Message
                    });
                }
            }       
        }

        //---------------------使用者退追-------------------------------
        [HttpDelete]
        [Route("api/users/{id}/follow")]
        [JwtAuthFilter]
        public IHttpActionResult unfollowUser(int id)
        {

            var user = userhash.GetUserFromJWT();
            var userData = db.Users.FirstOrDefault(u => u.Id == user.Id);
            //檢查本使用者是否有權限問題
            var checkUser = new UserEncryption();
            var statusCheck = checkUser.GetUserStatusErrorMessage(userData);
            if (statusCheck != null) return Ok(new { statusCode = 403, msg = statusCheck });
            var follow = db.Follows.FirstOrDefault(f => f.UserId == user.Id && f.FollowedUserId == id);
            var targetUser = db.Users.FirstOrDefault(u => u.Id == id);
            if (targetUser == null || targetUser.IsDeleted || targetUser.IsBanned ||targetUser.UserRole==UserRoles.Admin)
                return Ok(new { statusCode = 404, msg = "找不到要取消追蹤的使用者或該帳號已停權" });
            bool hasData = follow != null;
            var newToken = "";
            if (hasData)
            {
                db.Follows.Remove(follow);
                db.SaveChanges();
                //--------------------試做refreshToken-----------------------------------
                string token = userhash.GetRawTokenFromHeader();
                var payload = JwtAuthUtil.GetPayload(token);
                newToken = jwt.ExpRefreshToken(payload);
            }

            var res = new
            {
                StatusCode = hasData ? 200 : 400,
                msg = hasData ? "取消追蹤成功" : "找不到這筆追蹤",
                Id = hasData?id:-1,
                newToken = hasData ? newToken : null,
            };
            return Ok(res);
        }
        //---------------------使用者追隨-------------------------------
        [HttpPost]
        [Route("api/users/{id}/follow")]
        [JwtAuthFilter]
        public IHttpActionResult followUser(int id)
        {
            var user = userhash.GetUserFromJWT();
            var userData = db.Users.FirstOrDefault(u => u.Id == user.Id);
            //檢查本使用者是否有權限問題
            var checkUser = new UserEncryption();
            var statusCheck = checkUser.GetUserStatusErrorMessage(userData);
            if (statusCheck != null) return Ok(new { statusCode = 403, msg = statusCheck });

            if (user.Id == id)
            {
                return Ok(new { StatusCode=400, msg="使用者不能追隨自己"});
            }
            var target = db.Users.FirstOrDefault(u => u.Id == id && !u.IsBanned && !u.IsDeleted&&u.UserRole!=UserRoles.Admin);

            bool hasTarget = target != null;
            if (!hasTarget)
            {
                return Ok(new { StatusCode = 400, msg="找不到這個使用者，使用者或許以停權或已被刪除"});
            }
            var follow = db.Follows.FirstOrDefault(f => f.UserId == user.Id && f.FollowedUserId == id);

            bool hasData = follow != null;
            if (hasData)
            {
                return Ok(new { StatusCode = 400, msg = "已經追蹤過了" });
            }
            var follows = new Follows();
            follows.UserId = user.Id;
            follows.FollowedUserId = id;
            follows.CreatedAt = DateTime.Now;
            follows.UpdatedAt = DateTime.Now;
            db.Follows.Add(follows);
            db.SaveChanges();
            //--------------------試做refreshToken-----------------------------------
            string token = userhash.GetRawTokenFromHeader();
            var payload = JwtAuthUtil.GetPayload(token);
            var newToken = jwt.ExpRefreshToken(payload);
            var res = new
            {
                StatusCode = 200,
                msg = "追蹤成功",
                Id = id,
                newToken = newToken,

            };

            return Ok(res);
        }

        //---------------------使用者取消收藏食譜-------------------------------
        [HttpDelete]
        [Route("api/recipes/{id}/favorite")]
        [JwtAuthFilter]
        public IHttpActionResult RemoveFavorite(int id)
        {
            var user = userhash.GetUserFromJWT();
            var userData = db.Users.FirstOrDefault(u => u.Id == user.Id);
            //檢查本使用者是否有權限問題
            var checkUser = new UserEncryption();
            var statusCheck = checkUser.GetUserStatusErrorMessage(userData);
            if (statusCheck != null) return Ok(new { statusCode = 403, msg = statusCheck });
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == id && !r.IsDeleted && r.IsPublished);
            if (recipe == null)
            {
                return Ok(new { statusCode = 404, msg = "找不到該食譜或已被刪除" });
            }

            var favorite = db.Favorites.FirstOrDefault(f => f.UserId == user.Id && f.RecipeId == id);
            bool hasData = favorite != null;
            var newToken = "";
            if (hasData)
            {
                db.Favorites.Remove(favorite);
                db.SaveChanges();
                //--------------------試做refreshToken-----------------------------------
                string token = userhash.GetRawTokenFromHeader();
                var payload = JwtAuthUtil.GetPayload(token);
                newToken = jwt.ExpRefreshToken(payload);
            }

            var res = new
            {
                StatusCode = hasData ? 200 : 400,
                msg = hasData ? "移除收藏成功" : "找不到這筆收藏",
                id = hasData ? id:-1,
                newToken = hasData?newToken:null,
            };
            return Ok(res);
        }

        //---------------------使用者收藏食譜-------------------------------
        [HttpPost]
        [Route("api/recipes/{id}/favorite")]
        [JwtAuthFilter]
        public IHttpActionResult AddFavorite(int id)
        {
            var user = userhash.GetUserFromJWT();
            var userData = db.Users.FirstOrDefault(u => u.Id == user.Id);
            //檢查本使用者是否有權限問題
            var checkUser = new UserEncryption();
            var statusCheck = checkUser.GetUserStatusErrorMessage(userData);
            if (statusCheck != null) return Ok(new { statusCode = 403, msg = statusCheck });
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == id && !r.IsDeleted && r.IsPublished);
            if (recipe == null)
            {
                return Ok(new { statusCode = 404, msg = "找不到該食譜或已被刪除" });
            }

            var favorite = db.Favorites.FirstOrDefault(f => f.UserId == user.Id && f.RecipeId == id);
            bool hasData = favorite != null;
            if (hasData)
            {
                return BadRequest("已收藏");
            }
            favorite = new Favorites();
            favorite.UserId = user.Id;
            favorite.RecipeId = id;
            favorite.CreatedAt = DateTime.Now;
            favorite.UpdatedAt = DateTime.Now;
            db.Favorites.Add(favorite);
            db.SaveChanges();
            //--------------------試做refreshToken-----------------------------------
            string token = userhash.GetRawTokenFromHeader();
            var payload = JwtAuthUtil.GetPayload(token);
            var newToken = jwt.ExpRefreshToken(payload);

            var res = new
            {
                StatusCode = 200,
                msg = "收藏成功",
                Id = id,
                newToken = newToken,
            };
            return Ok(res);
        }
        //---------------------以下暫時保留------------------------------
        //---------------------使用者刪除評分評論------------------------
        //[HttpDelete]
        //[Route("api/recipes/{recipeId}/rating-comment")]
        //[JwtAuthFilter]
        //public IHttpActionResult DeleteRatingAndComment(int recipeId)
        //{
        //    var user = userhash.GetUserFromJWT();
        //    var rating = db.Ratings.FirstOrDefault(r => r.UserId == user.Id && r.RecipeId == recipeId);
        //    if (rating == null)
        //    {
        //        return Ok(new { StatusCode=400, msg="你尚未評分"});
        //    }
        //    else
        //    {
        //        try
        //        {
        //            db.Ratings.Remove(rating);
        //            var comments = db.Comments.FirstOrDefault(c => c.UserId == user.Id && c.RecipeId == recipeId);
        //            db.Comments.Remove(comments);
        //            db.SaveChanges();
        //            var ratings = db.Ratings.Where(r => r.RecipeId == recipeId);
        //            var avg = Math.Round(ratings.Average(r => r.Rating), 1);
        //            var recipe = db.Recipes.FirstOrDefault(r => r.Id == recipeId);
        //            bool hasRecipe = (recipe != null);
        //            if (hasRecipe)
        //            {
        //                recipe.Rating = (decimal)avg;
        //                db.SaveChanges();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            return Ok(new
        //            {
        //                msg = "刪除評分與留言失敗",
        //                error = ex.Message
        //            });
        //        }
        //    }

        //    //--------------------試做refreshToken-----------------------------------
        //    string token = userhash.GetRawTokenFromHeader();
        //    var payload = JwtAuthUtil.GetPayload(token);
        //    var newToken = jwt.ExpRefreshToken(payload);
        //    var res = new
        //    {
        //        StatusCode = 200,
        //        msg = "刪除評分與留言成功",
        //        data = new
        //        {
        //            recipeId = recipeId
        //        },
        //        newToken = newToken,
        //    };
        //    return Ok(res);
        //}

    }
}
