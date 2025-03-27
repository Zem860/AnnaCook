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


namespace RecipeTest.Controllers
{
    public class UserController : ApiController
    {
        private JwtAuthUtil jwt = new JwtAuthUtil();
        private RecipeModel db = new RecipeModel();
        private UserEncryption userhash = new UserEncryption();


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
            bool hasData = follow !=null;
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
                try {
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
                } catch (Exception ex) {

                    var res = new
                    {
                        StatusCode = 500,
                        msg = "評分失敗",
                       
                    };

                    return Ok(ex);
                }
              
            }
            }

       
        [HttpPost]
        [Route("api/user/login")]
        public IHttpActionResult login([FromBody] UserRelated.UserLoginData request)
        {
            var user = db.User.FirstOrDefault(u => u.AccountEmail == request.AccountEmail);
            if (user == null)
            {
                var resError = new
                {
                    StatusCode = 400,
                    msg = "使用者不存在"
                };
                return Ok(resError);
            }
            var salt = Convert.FromBase64String(user.Salt);
            var hash = userhash.HashPassword(request.Password, salt);
            var stringHash = Convert.ToBase64String(hash);
            if (user.PasswordHash != stringHash)
            {
                var resError = new
                {
                    StatusCode = 400,
                    msg = "密碼錯誤"
                };
                return Ok(resError);
            }
            var userToken = new UserTokenData
            {
                Id = user.Id,
                Account = user.AccountEmail,
                AccountName = user.AccountName
            };
            var token = jwt.GenerateToken(userToken);
            var res = new
            {
                StatusCode = 200,
                msg = "登入成功",
                token = token
            };
            return Ok(res);
        }

        //-------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("api/user/register")]
        public IHttpActionResult sendUserData([FromBody] UserRelated.ClientRegisterData request)
        {
            var userExist = db.User.FirstOrDefault(u => u.AccountEmail == request.AccountEmail);
            if (userExist != null) {

                var resError = new
                {
                    StatusCode = 400,
                    msg = "使用者已存在"
                };
                return Ok(resError);
            }
            string password = request.Password;
            var salt = userhash.createSalt() ;
            var stringSalt = Convert.ToBase64String(salt);
            var hash =userhash.HashPassword(password, salt);
            var stringHash = Convert.ToBase64String(hash);
            var user = new Users
            {
                AccountEmail = request.AccountEmail,
                AccountName = request.AccountName,
                PasswordHash = stringHash,
                Salt = stringSalt,
                AccountProfilePhoto = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.User.Add(user);
            db.SaveChanges();
            var res = new
            {
                StatusCode = 200,
                msg = "使用者新增成功",
            };

            return Ok(res);
        }


        [HttpPut]
        [Route("api/editUser/{id}")]
        public IHttpActionResult editUser(int id, UserRelated.UserUpdateData request)
        {

            var user = db.User.FirstOrDefault(y => y.Id == id);
            bool hasUser = user != null;
            user.AccountEmail = request.AccountEmail;
            user.AccountName = request.AccountName;

            db.SaveChanges();
            var res = new
            {
                StatusCode = hasUser ? 200 : 400,
                msg = hasUser ? "使用者名稱email已更新" : "查無使用者",
                Id = id,
            };
            return Ok(res);
        }


        [HttpDelete]
        [Route("api/deleteUser/{id}")]
        public IHttpActionResult deleteUser(int id)
        {
            var user = db.User.FirstOrDefault(u => u.Id == id);
            bool hasUser = user != null;

            var res = new
            {
                StatusCode = hasUser ? 200 : 400,
                msg = hasUser ? "已刪除使用者" : "查無使用者",
            };
            if (hasUser)
            {
                db.User.Remove(user);
                db.SaveChanges();
            }

            return Ok(res);
        }

        [HttpGet]
        [Route("api/user/{id}")]
        public IHttpActionResult GetOneUser(int id)
        {
            var user = db.User
                                   .Where(u => u.Id == id)
                                   .Select(u => new
                                   {
                                       u.AccountName,
                                       u.AccountEmail
                                   })
                                   .FirstOrDefault(); bool hasUser = user != null;
            var res = new
            {
                StatusCode = hasUser ? 200 : 400,
                msg = hasUser?"找到使用者":"查無使用者",
                data = hasUser ? user : null,
            };

            if (user == null)
            {
                return Ok(res);
            }

            return Ok(user);
        }


        [HttpGet]
        [Route("api/users")]
        public IHttpActionResult showUsers()
        {
            var users = db.User
                                .Select(u => new { u.Id, u.AccountName, u.AccountEmail })
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
