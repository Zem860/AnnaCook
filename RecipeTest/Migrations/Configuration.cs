namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using RecipeTest.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<RecipeTest.Models.Model1>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(RecipeTest.Models.Model1 context)
        {
            context.Recipe.AddOrUpdate(
                r => r.Id,
                new Recipe
                {
                    Id = 1,
                    RecipeName = "炒鍋煎的粉漿蛋餅",
                    IsPublished = false,
                    RecipeIntro = "好吃好吃總之就是好吃",
                    CookingTime = 5,
                    Portion = 1,
                    Rating = 3.6m,  // 小數需要 `m` 來表示 decimal
                    LikedNumber = 10,
                    RecipeVideoLink = "https://www.youtube.com/watch?v=GG7dQGT_tAs",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                },
                 new Recipe
                 {
                     Id = 2,
                     RecipeName = "布林餅",
                     IsPublished = false,
                     RecipeIntro = "甜甜的好好吃喔",
                     CookingTime = 15,
                     Portion = 3,
                     Rating = 5.0m,  // 小數需要 `m` 來表示 decimal
                     LikedNumber = 14,
                     RecipeVideoLink = "https://www.youtube.com/watch?v=GG7dQGT_tAs",
                     CreatedAt = DateTime.UtcNow,
                     UpdatedAt = DateTime.UtcNow,
                 },
                  new Recipe
                  {
                      Id = 3,
                      RecipeName = "肉醬義大利麵",
                      IsPublished = false,
                      RecipeIntro = "噁心死了都是肉",
                      CookingTime = 30,
                      Portion = 2,
                      Rating = 2.2m,  // 小數需要 `m` 來表示 decimal
                      LikedNumber = 50,
                      RecipeVideoLink = "https://www.youtube.com/watch?v=GG7dQGT_tAs",
                      CreatedAt = DateTime.UtcNow,
                      UpdatedAt = DateTime.UtcNow,
                  }
            );

            context.SaveChanges();
        }

    }
}
