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
    public class CommentsController : ApiController
    {
        private JwtAuthUtil jwt = new JwtAuthUtil();
        private RecipeModel db = new RecipeModel();
        private UserEncryption userhash = new UserEncryption();

        [HttpPost]
        [Route("api/recipes/{recipeId}/rating-comment")]
        [JwtAuthFilter]
        public IHttpActionResult AddRatingAndComment(int recipeId, ComplexUserRecipe.RatingCommentDto dto)
        {
            var user = userhash.GetUserFromJWT();

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
                    var recipe = db.Recipes.FirstOrDefault(r => r.Id == recipeId);
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
