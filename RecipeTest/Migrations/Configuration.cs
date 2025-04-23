namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using RecipeTest.Models;
    using Newtonsoft.Json;
    using System.IO;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using RecipeTest.Security;

    internal sealed class Configuration : DbMigrationsConfiguration<RecipeTest.Models.RecipeModel>
    {

        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }


        protected override void Seed(RecipeTest.Models.RecipeModel context)
        {

            //1️⃣ 確保 Admin 存在
            var admin = context.Users.FirstOrDefault(u => u.AccountEmail == "admin@annacook.com");
            if (admin == null)
            {
                var encryption = new UserEncryption();
                byte[] salt = encryption.createSalt();
                string saltString = Convert.ToBase64String(salt);
                byte[] hash = encryption.HashPassword("ABC123123123", salt);
                string hashString = Convert.ToBase64String(hash);
                admin = new Users
                {
                    DisplayId = "A000001",
                    AccountEmail = "admin@annacook.com",
                    PasswordHash = hashString,
                    Salt = saltString,
                    AccountName = "Admin",
                    UserIntro = "我是管理員",
                    IsVerified = true,
                    IsBanned = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    LoginProvider = RecipeTest.Enums.LoginProvider.Local,
                    UserRole = RecipeTest.Enums.UserRoles.Admin,
                };

                context.Users.Add(admin);
                context.SaveChanges(); // 拿到 admin.Id
            }

        }
    }
}
