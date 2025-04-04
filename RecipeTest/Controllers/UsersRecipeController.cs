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
namespace RecipeTest.Controllers
{
    public class UsersRecipeController : ApiController
    {
        private RecipeModel db = new RecipeModel();
        private UserEncryption userhash = new UserEncryption();
        [HttpGet]
        [Route("api/recipes/{recipeId}/rating-comment")]
        public IHttpActionResult getRatingComment(int recipeId, int page = 1)
        {
            var recipeComment = db.Comments.Where(c => c.RecipeId == recipeId);

            if (!recipeComment.Any())
            {
                return NotFound();
            }
            int pageSize = 3;
            int skip = (page - 1) * pageSize;
            var totalCount = recipeComment.Count();
            var data = recipeComment
                .OrderByDescending(rc => rc.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(c => new
                {
                    id = c.Id,
                    authorName = c.Users.AccountName,
                    authorPhoto = c.Users.AccountProfilePhoto,
                    comment = c.CommentContent,
                    rating = db.Ratings
                        .Where(r => r.UserId == c.Users.Id && r.RecipeId == recipeId)
                        .Select(r => r.Rating)
                        .FirstOrDefault()
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

        [HttpPost]
        [Route("api/recipes/{recipeId}/rating-comment")]
        [JwtAuthFilter]
        public IHttpActionResult AddRatingAndComment(int recipeId, ComplexUserRecipe.RatingCommentDto dto)
        {
            var user = userhash.GetUserFromJWT();

            var recipe = db.Recipes.FirstOrDefault(r => r.Id == recipeId);
            if (recipe.UserId == user.Id)
            {
                return BadRequest("自己不要給自己評分");
            }

            // 檢查是否已評分
            var rating = db.Ratings.FirstOrDefault(r => r.UserId == user.Id && r.RecipeId == recipeId);
            if (rating != null)
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
                        db.SaveChanges();

                    }
                    var ratings = db.Ratings.Where(r => r.RecipeId == recipeId);
                    var avg = Math.Round(ratings.Average(r => r.Rating), 1);
                    bool hasRecipe = (recipe != null);
                    if (hasRecipe)
                    {
                        recipe.Rating = (decimal)avg;
                        db.SaveChanges();
                    }

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
            var res = new
            {
                msg = "評分與留言成功",
                data = new
                {
                    recipeId = recipeId,
                    rating = dto.Rating,
                    comment = dto.CommentContent
                }
            };

            return Ok(res);         
        }


        [HttpDelete]
        [Route("api/users/{id}/follow")]
        [JwtAuthFilter]
        public IHttpActionResult unfollowUser(int id)
        {
            var user = userhash.GetUserFromJWT();
            var follow = db.Follows.FirstOrDefault(f => f.UserId == user.Id && f.FollowedUserId == id);
            bool hasData = follow != null;
            if (hasData)
            {
                db.Follows.Remove(follow);
                db.SaveChanges();
            }
            var res = new
            {
                StatusCode = hasData ? 200 : 400,
                msg = hasData ? "取消追蹤成功" : "找不到這筆追蹤",
            };
            return Ok(res);
        }

        [HttpPost]
        [Route("api/users/{id}/follow")]
        [JwtAuthFilter]
        public IHttpActionResult followUser(int id)
        {
            var user = userhash.GetUserFromJWT();
            if (user.Id == id)
            {
                return BadRequest("使用者不能追蹤自己");
            }
            var follow = db.Follows.FirstOrDefault(f => f.UserId == user.Id && f.FollowedUserId == id);
            bool hasData = follow != null;
            if (hasData)
            {
                return BadRequest("已經追蹤過了");
            }
            var follows = new Follows();
            follows.UserId = user.Id;
            follows.FollowedUserId = id;
            follows.CreatedAt = DateTime.Now;
            follows.UpdatedAt = DateTime.Now;
            db.Follows.Add(follows);
            db.SaveChanges();

            var res = new
            {
                StatusCode = 200,
                msg = "追蹤成功",

            };

            return Ok(res);
        }
        [HttpDelete]
        [Route("api/recipes/{id}/favorite")]
        [JwtAuthFilter]
        public IHttpActionResult RemoveFavorite(int id)
        {
            var user = userhash.GetUserFromJWT();
            var favorite = db.Favorites.FirstOrDefault(f => f.UserId == user.Id && f.RecipeId == id);
            bool hasData = favorite != null;
            if (hasData)
            {
                db.Favorites.Remove(favorite);
                db.SaveChanges();
            }

            var res = new
            {
                StatusCode = hasData ? 200 : 400,
                msg = hasData ? "移除收藏成功" : "找不到這筆收藏",
            };
            return Ok(res);
        }


        [HttpPost]
        [Route("api/recipes/{id}/favorite")]
        [JwtAuthFilter]
        public IHttpActionResult AddFavorite(int id)
        {
            var user = userhash.GetUserFromJWT();
            var favorite = db.Favorites.FirstOrDefault(f => f.UserId == user.Id && f.RecipeId == id);
            if (favorite != null)
            {
                return BadRequest("已收藏");
            }
            favorite = new Favorites();
            favorite.UserId = user.Id;
            favorite.RecipeId = id;
            db.Favorites.Add(favorite);
            db.SaveChanges();
            return Ok(new { msg = "收藏成功" });
        }

        [HttpDelete]
        [Route("api/userRating/{id}")]
        public IHttpActionResult DeleteRating(int id)
        {
            var rating = db.Ratings.FirstOrDefault(r => r.RecipeId == id);
            if (rating == null)
            {
                return Content(HttpStatusCode.NotFound, new { msg = "找不到這筆評分" });
            }

            db.Ratings.Remove(rating);
            db.SaveChanges();

            return Ok(new { msg = "刪除成功" });
        }


        [HttpPost]
        [Route("api/recipes/{recipeId}/ratings")]
        [JwtAuthFilter]
        public IHttpActionResult UserRating(int recipeId, ComplexUserRecipe.RatingDto ratingDto)
        {
            var user = userhash.GetUserFromJWT();
            var rating = db.Ratings.FirstOrDefault(r => r.UserId == user.Id && r.Recipes.Id == recipeId);
            bool hasData = rating != null;
            if (hasData)
            {
                return BadRequest("你已經評分過了");
            }
            else
            {
                try
                {
                    rating = new Ratings();
                    rating.UserId = user.Id;
                    rating.RecipeId = recipeId;
                    rating.Rating = ratingDto.Rating;
                    db.Ratings.Add(rating);
                    db.SaveChanges(); // <== 必須先存入資料庫
                    var RecipeRatings = db.Ratings.Where(r => r.RecipeId == recipeId);
                    decimal AvgRating = (decimal)Math.Round(RecipeRatings.Average(r => r.Rating), 1);
                    var recipe = db.Recipes.FirstOrDefault(r => r.Id == recipeId);
                    bool hasRecipe = recipe != null;
                    if (hasRecipe)
                    {
                        recipe.Rating = AvgRating;
                        db.SaveChanges();
                    }
                    var res = new
                    {
                        StatusCode = 200,
                        msg = "評分成功",
                        data = new
                        {
                            recipeId = recipeId,
                            rating = ratingDto.Rating
                        }
                    };

                    return Ok(res);
                }
                catch (Exception ex)
                {

                    var res = new
                    {
                        StatusCode = 500,
                        msg = "評分失敗",

                    };

                    return Ok(ex);
                }

            }
        }
        //---------------------以下暫時保留------------------------------
        [HttpDelete]
        [Route("api/recipes/{recipeId}/rating-comment")]
        [JwtAuthFilter]
        public IHttpActionResult DeleteRatingAndComment(int recipeId)
        {
            var user = userhash.GetUserFromJWT();
            var rating = db.Ratings.FirstOrDefault(r => r.UserId == user.Id && r.RecipeId == recipeId);
            if (rating == null)
            {
                return BadRequest("你尚未評分");
            }
            else
            {
                try
                {
                    db.Ratings.Remove(rating);
                    var comments = db.Comments.FirstOrDefault(c => c.UserId == user.Id && c.RecipeId == recipeId);
                    db.Comments.Remove(comments);
                    var ratings = db.Ratings.Where(r => r.RecipeId == recipeId);
                    var avg = Math.Round(ratings.Average(r => r.Rating), 1);
                    var recipe = db.Recipes.FirstOrDefault(r => r.Id == recipeId);
                    bool hasRecipe = (recipe != null);
                    if (hasRecipe)
                    {
                        recipe.Rating = (decimal)avg;
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    return Ok(new
                    {
                        msg = "刪除評分與留言失敗",
                        error = ex.Message
                    });
                }
            }
            var res = new
            {
                msg = "刪除評分與留言成功",
                data = new
                {
                    recipeId = recipeId
                }
            };
            return Ok(res);
        }

    }
}
