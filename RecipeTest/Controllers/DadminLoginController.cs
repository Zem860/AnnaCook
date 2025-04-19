using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Jose;
using RecipeTest.Enums;
using RecipeTest.Models;
using RecipeTest.Pages;
using RecipeTest.Security;
using static RecipeTest.Pages.UserRelated;

namespace RecipeTest.Controllers
{
    public class DadminLoginController : ApiController
    {
        private RecipeModel db = new RecipeModel();
        private UserEncryption userhash = new UserEncryption();
        private JwtAuthUtil jwt = new JwtAuthUtil();

        [HttpPost]
        [Route("api/admin/login")]
        public IHttpActionResult login([FromBody] UserRelated.UserLoginData request)
        {
            var user = db.Users.FirstOrDefault(u => u.AccountEmail == request.AccountEmail && u.IsVerified == true && u.UserRole == UserRoles.Admin);
            if (user == null)
            {
                var resError = new
                {
                    StatusCode = 401,
                    msg = "此人無權限"
                };
                return Ok(resError);
            }
            var salt = Convert.FromBase64String(user.Salt);
            var hash = userhash.HashPassword(request.Password, salt);
            var stringHash = Convert.ToBase64String(hash);
            //取得使用者ip~成功或錯誤都記起來
            string ip = userhash.getClientIp();
            LoginRecords loginTimeRecord = new LoginRecords();
            loginTimeRecord.UserId = user.Id;
            loginTimeRecord.ActionTime = DateTime.Now;
            loginTimeRecord.IpAddress = ip;
            if (user.PasswordHash != stringHash)
            {
                loginTimeRecord.LoginAction = (int)LoginActions.LoginFail;
                db.LoginRecords.Add(loginTimeRecord);
                db.SaveChanges();
                var resError = new
                {
                    StatusCode = 400,
                    msg = "密碼錯誤"
                };
                return Ok(resError);
            }
            loginTimeRecord.LoginAction = (int)LoginActions.LoginSuccess;

            db.LoginRecords.Add(loginTimeRecord);
            db.SaveChanges();
            //---------------------------------
            var userToken = new UserTokenData();
            userToken.Id = user.Id;
            userToken.AccountEmail = user.AccountEmail;
            userToken.AccountName = user.AccountName;
            userToken.Role = (int)user.UserRole;
            userToken.DisplayId = user.DisplayId;
            var token = jwt.GenerateToken(userToken);
            //------------------------------------------
            var userData = new
            {
                userId = user.Id,
                userDisplayId = user.DisplayId,
                accountEmail = user.AccountEmail,
                accountName = user.AccountName,
                profilePhoto = user.AccountProfilePhoto,
                role = (int)user.UserRole,
                roleName = user.UserRole.ToString(),
            };
            var res = new
            {
                StatusCode = 200,
                msg = "登入成功",
                token = token,
                userData = userData,
            };
            return Ok(res);
        }
    }
}
