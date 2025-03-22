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


namespace RecipeTest.Controllers
{
    public class UserController : ApiController
    {
        private JwtAuthUtil jwt = new JwtAuthUtil();
        private RecipeModel db = new RecipeModel();
        private UserEncryption userhash = new UserEncryption();

        [HttpPost]
        [Route("api/tokenTest")]
        public IHttpActionResult getAllUserData([FromBody] UserRelated.UserTokenData request)
        {
            string token = jwt.GenerateToken(request);
            return Ok(token);
        }

        [HttpPost]
        [Route("api/register")]
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
            var hash = userhash.HashPassword(password, salt);
            var user = new Users
            {
                AccountEmail = request.AccountEmail,
                AccountName = request.AccountName,
                PasswordHash = hash,
                Salt = salt,
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
