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
    using RecipeTest.SeedData;
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
            //    var userPath = @"C:\Users\zemmy\Downloads\users.json";
            //    if (!File.Exists(userPath))
            //    {
            //        Console.WriteLine("❌ JSON file not found: " + userPath);
            //        return;
            //    }

            //    // 反序列化：支援多筆使用者（請確保 JSON 格式是陣列）
            //    var userList = JsonConvert.DeserializeObject<List<Users>>(File.ReadAllText(userPath));

            //    // 這裡統一建立一組密碼的 salt 與 hash
            //    var encryption = new UserEncryption();
            //    byte[] salt = encryption.createSalt();
            //    string saltString = Convert.ToBase64String(salt);
            //    byte[] hash = encryption.HashPassword("ABC123123123", salt);
            //    string hashString = Convert.ToBase64String(hash);

            //    // 從資料庫抓出目前已存在的最大 M 使用者
            //    var existingMIds = context.Users
            //        .Where(u => u.DisplayId.StartsWith("M"))
            //        .Select(u => u.DisplayId)
            //        .ToList();

            //    int lastNumber = 0;
            //    if (existingMIds.Any())
            //    {
            //        lastNumber = existingMIds
            //            .Select(id => int.TryParse(id.Substring(1), out int num) ? num : 0)
            //            .Max();
            //    }

            //    foreach (var user in userList)
            //    {
            //        if (user != null)
            //        {
            //            user.DisplayId = "M" + (++lastNumber).ToString("D6");
            //            user.Salt = saltString;
            //            user.PasswordHash = hashString;
            //            user.AccountProfilePhoto = "/UserPhotos/" + "M" + (lastNumber).ToString("D6"); // 預設圖片
            //            user.IsVerified = true;
            //            user.IsBanned = false;
            //            user.LoginProvider = RecipeTest.Enums.LoginProvider.Local;
            //            user.UserRole = RecipeTest.Enums.UserRoles.User;
            //            user.CreatedAt = DateTime.Now;
            //            user.UpdatedAt = DateTime.Now;

            //            context.Users.AddOrUpdate(user); // 或 Add(user)
            //        }
            //    }
            //context.SaveChanges(); // 移出迴圈，只存一次

            //1️⃣ 確保 Admin 存在
            //var admin = context.Users.FirstOrDefault(u => u.AccountEmail == "admin@annacook.com");
            //if (admin == null)
            //{
            //    var encryption = new UserEncryption();
            //    byte[] salt = encryption.createSalt();
            //    string saltString = Convert.ToBase64String(salt);
            //    byte[] hash = encryption.HashPassword("ABC123123123", salt);
            //    string hashString = Convert.ToBase64String(hash);
            //    admin = new Users
            //    {
            //        DisplayId = "A000001",
            //        AccountEmail = "admin@annacook.com",
            //        PasswordHash = hashString,
            //        Salt = saltString,
            //        AccountName = "Admin",
            //        UserIntro = "我是管理員",
            //        IsVerified = true,
            //        IsBanned = false,
            //        CreatedAt = DateTime.Now,
            //        UpdatedAt = DateTime.Now,
            //        LoginProvider = RecipeTest.Enums.LoginProvider.Local,
            //        UserRole = RecipeTest.Enums.UserRoles.Admin,
            //    };

            //    context.Users.Add(admin);
            //    context.SaveChanges(); // 拿到 admin.Id
            //}

            //// 🔍 讀 JSON
            //var jsonPath = @"C:\Users\zemmy\Downloads\sample40.json";
            //if (!File.Exists(jsonPath))
            //{
            //    Console.WriteLine("❌ JSON file not found: " + jsonPath);
            //    return;
            //}

            //var recipeList = JsonConvert.DeserializeObject<List<SeedRecipe>>(File.ReadAllText(jsonPath));
            //if (recipeList == null || !recipeList.Any())
            //{
            //    Console.WriteLine("❌ No recipes found.");
            //    return;
            //}

            //// 🔍 取得 User 2 和 3
            //var user2 = context.Users.FirstOrDefault(u => u.Id == 2);
            //var user3 = context.Users.FirstOrDefault(u => u.Id == 3);
            //if (user2 == null || user3 == null)
            //{
            //    Console.WriteLine("❌ 使用者 ID=2 或 3 不存在，無法寫入食譜！");
            //    return;
            //}

            //var rand = new Random();
            //int lastRecipeNum = 0;

            //Console.WriteLine($"✅ Loaded {recipeList.Count} recipes from JSON");

            //foreach (var recipe in recipeList)
            //{
            //    if (recipe?.Detail == null || recipe.Steps == null) continue;

            //    var selectedUser = rand.Next(0, 2) == 0 ? user2 : user3;

            //    var newRecipe = new Recipes
            //    {
            //        RecipeName = recipe.RecipeName,
            //        RecipeIntro = recipe.Detail.RecipeIntro ?? "",
            //        CookingTime = recipe.Detail.CookingTime,
            //        Portion = recipe.Detail.Portion,
            //        IsPublished = true,
            //        ViewCount = rand.Next(0, 100),
            //        DisplayId = "R" + (++lastRecipeNum).ToString("D6"),
            //        CreatedAt = DateTime.Now,
            //        UpdatedAt = DateTime.Now,
            //        UserId = selectedUser.Id, // ⭐ 寫 UserId 更穩

            //        Ingredients = recipe.Detail.Ingredients?.Select(i => new Ingredients
            //        {
            //            IngredientName = i.IngredientName,
            //            Amount = i.Amount,
            //            Unit = i.Unit,
            //            IsFlavoring = i.IsFlavoring,
            //            CreatedAt = DateTime.Now,
            //            UpdatedAt = DateTime.Now
            //        }).ToList(),

            //        Steps = recipe.Steps?.Select((s, index) => new Steps
            //        {
            //            StepOrder = index + 1,
            //            StepDescription = s.Description,
            //            VideoStart = s.StartTime,
            //            VideoEnd = s.EndTime,
            //            CreatedAt = DateTime.Now
            //        }).ToList(),

            //        RecipeTags = new List<RecipeTags>(),
            //        RecipesPhotos = new List<RecipePhotos>()
            //    };

            //    // ⭐ Tags
            //    foreach (var tagName in recipe.Detail.Tags ?? new List<string>())
            //    {
            //        var tag = context.Tags.FirstOrDefault(t => t.TagName == tagName);
            //        if (tag == null)
            //        {
            //            tag = new Tags { TagName = tagName, CreatedAt = DateTime.Now };
            //            context.Tags.Add(tag);
            //            context.SaveChanges(); // 拿到 tag.Id
            //        }

            //        newRecipe.RecipeTags.Add(new RecipeTags
            //        {
            //            Tags = tag,
            //            CreatedAt = DateTime.Now
            //        });
            //    }

            //    // ⭐ 食譜封面照片
            //    newRecipe.RecipesPhotos.Add(new RecipePhotos
            //    {
            //        ImgUrl = $"/TestPhoto/{newRecipe.DisplayId}.png",
            //        CreatedAt = DateTime.Now,
            //        UpdatedAt = DateTime.Now
            //    });

            //    context.Recipes.AddOrUpdate(newRecipe);
            //    try
            //    {
            //        context.SaveChanges();
            //        Console.WriteLine($"✅ 成功寫入：{newRecipe.RecipeName} 給 UserId={selectedUser.Id}");
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine("❌ SaveChanges 發生錯誤: " + ex.Message);
            //        if (ex.InnerException != null)
            //            Console.WriteLine("🔎 Inner: " + ex.InnerException.Message);
            //        throw;
            //    }
            //}

            //Console.WriteLine("✅ All recipes and photos saved!");
        }
    }
}
