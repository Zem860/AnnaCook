namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using RecipeTest.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<RecipeTest.Models.RecipeModel>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(RecipeTest.Models.RecipeModel context)
        {
//            context.Recipes.AddOrUpdate(r => r.Id,
//                new Recipes
//                {
//                    Id = 3010,
//                    UserId = 2,
//                    RecipeName = "奶油蘑菇雞",
//                    IsPublished = true,
//                    RecipeIntro = "濃郁奶香搭配鮮嫩雞肉，晚餐簡單又高級。",
//                    CookingTime = 35.00m,
//                    Portion = 2.00m,
//                    Rating = 4.6m,
//                    RecipeVideoLink = "https://youtu.be/creamy_mushroom_chicken",
//                    CreatedAt = DateTime.Now,
//                    UpdatedAt = DateTime.Now,
//                    DisplayId = "R000011",
//                    ViewCount = 880,
//                    SharedCount = 51
//                },
//new Recipes
//{
//    Id = 3011,
//    UserId = 2,
//    RecipeName = "韓式泡菜豆腐鍋",
//    IsPublished = true,
//    RecipeIntro = "香辣開胃，適合冷天的一鍋暖食！",
//    CookingTime = 40.00m,
//    Portion = 3.00m,
//    Rating = 4.8m,
//    RecipeVideoLink = "https://youtu.be/kimchi_tofu_stew",
//    CreatedAt = DateTime.Now,
//    UpdatedAt = DateTime.Now,
//    DisplayId = "R000012",
//    ViewCount = 1530,
//    SharedCount = 73
//},
//new Recipes
//{
//    Id = 3012,
//    UserId = 2,
//    RecipeName = "蒜香四季豆炒肉末",
//    IsPublished = true,
//    RecipeIntro = "快速家常料理，香氣十足，超下飯！",
//    CookingTime = 20.00m,
//    Portion = 2.00m,
//    Rating = 4.4m,
//    RecipeVideoLink = "https://youtu.be/garlic_greenbeans_pork",
//    CreatedAt = DateTime.Now,
//    UpdatedAt = DateTime.Now,
//    DisplayId = "R000013",
//    ViewCount = 670,
//    SharedCount = 33
//}



//                );
          
//            context.Recipes.AddOrUpdate(
//    r => r.Id,

//        new Recipes
//        {
//            Id = 3001,
//            UserId = 1,
//            RecipeName = "經典炒蛋",
//            IsPublished = true,
//            RecipeIntro = "簡單快速的炒蛋做法，適合新手上手。",
//            CookingTime = 10.00m,
//            Portion = 2.00m,
//            Rating = 4.5m,
//            RecipeVideoLink = "https://youtu.be/simple_scrambled_eggs",
//            CreatedAt = DateTime.Now,
//            UpdatedAt = DateTime.Now,
//            DisplayId = "R000001",
//            ViewCount = 100,
//            SharedCount = 20
//        },
//    new Recipes
//    {
//        Id = 3002,
//        UserId = 1,
//        RecipeName = "番茄炒蛋",
//        IsPublished = true,
//        RecipeIntro = "經典家常菜，酸甜開胃。",
//        CookingTime = 12.00m,
//        Portion = 2.00m,
//        Rating = 4.7m,
//        RecipeVideoLink = "https://youtu.be/tomato_eggs",
//        CreatedAt = DateTime.Now,
//        UpdatedAt = DateTime.Now,
//        DisplayId = "R000002",
//        ViewCount = 150,
//        SharedCount = 25
//    },
//    new Recipes
//    {
//        Id = 3003,
//        UserId = 1,
//        RecipeName = "日式咖哩飯",
//        IsPublished = true,
//        RecipeIntro = "甜中帶鹹的日式咖哩，溫暖人心。",
//        CookingTime = 40.00m,
//        Portion = 3.00m,
//        Rating = 4.8m,
//        RecipeVideoLink = "https://youtu.be/japanese_curry",
//        CreatedAt = DateTime.Now,
//        UpdatedAt = DateTime.Now,
//        DisplayId = "R000003",
//        ViewCount = 300,
//        SharedCount = 40
//    },
//    new Recipes
//    {
//        Id = 3004,
//        UserId = 1,
//        RecipeName = "奶油培根義大利麵",
//        IsPublished = true,
//        RecipeIntro = "濃郁奶油醬搭配香脆培根，美味又滿足。",
//        CookingTime = 25.00m,
//        Portion = 2.00m,
//        Rating = 4.6m,
//        RecipeVideoLink = "https://youtu.be/cream_bacon_pasta",
//        CreatedAt = DateTime.Now,
//        UpdatedAt = DateTime.Now,
//        DisplayId = "R000004",
//        ViewCount = 220,
//        SharedCount = 30
//    },
//    new Recipes
//    {
//        Id = 3005,
//        UserId = 1,
//        RecipeName = "韓式泡菜煎餅",
//        IsPublished = true,
//        RecipeIntro = "外酥內嫩的韓式小吃，辣中帶酸。",
//        CookingTime = 20.00m,
//        Portion = 2.00m,
//        Rating = 4.4m,
//        RecipeVideoLink = "https://youtu.be/kimchi_pancake",
//        CreatedAt = DateTime.Now,
//        UpdatedAt = DateTime.Now,
//        DisplayId = "R000005",
//        ViewCount = 180,
//        SharedCount = 18
//    },
//    new Recipes
//    {
//        Id = 3006,
//        UserId = 1,
//        RecipeName = "麻婆豆腐",
//        IsPublished = true,
//        RecipeIntro = "香麻帶辣的四川風味，超下飯！",
//        CookingTime = 18.00m,
//        Portion = 3.00m,
//        Rating = 4.9m,
//        RecipeVideoLink = "https://youtu.be/mapo_tofu",
//        CreatedAt = DateTime.Now,
//        UpdatedAt = DateTime.Now,
//        DisplayId = "R000006",
//        ViewCount = 450,
//        SharedCount = 55
//    },
//    new Recipes
//    {
//        Id = 3007,
//        UserId = 1,
//        RecipeName = "三杯雞",
//        IsPublished = true,
//        RecipeIntro = "台灣經典料理，九層塔香氣撲鼻。",
//        CookingTime = 30.00m,
//        Portion = 4.00m,
//        Rating = 4.8m,
//        RecipeVideoLink = "https://youtu.be/three_cup_chicken",
//        CreatedAt = DateTime.Now,
//        UpdatedAt = DateTime.Now,
//        DisplayId = "R000007",
//        ViewCount = 510,
//        SharedCount = 60
//    },
//    new Recipes
//    {
//        Id = 3008,
//        UserId = 1,
//        RecipeName = "香煎鯖魚",
//        IsPublished = true,
//        RecipeIntro = "外酥內嫩的日式家常魚料理。",
//        CookingTime = 15.00m,
//        Portion = 1.00m,
//        Rating = 4.3m,
//        RecipeVideoLink = "https://youtu.be/mackerel_pan_fry",
//        CreatedAt = DateTime.Now,
//        UpdatedAt = DateTime.Now,
//        DisplayId = "R000008",
//        ViewCount = 230,
//        SharedCount = 22
//    },
//    new Recipes
//    {
//        Id = 3009,
//        UserId = 1,
//        RecipeName = "鮮蝦蒸蛋",
//        IsPublished = true,
//        RecipeIntro = "滑嫩蒸蛋搭配鮮蝦，營養又美味。",
//        CookingTime = 20.00m,
//        Portion = 2.00m,
//        Rating = 4.6m,
//        RecipeVideoLink = "https://youtu.be/shrimp_steamed_egg",
//        CreatedAt = DateTime.Now,
//        UpdatedAt = DateTime.Now,
//        DisplayId = "R000009",
//        ViewCount = 310,
//        SharedCount = 26
//    },
//    new Recipes
//    {
//        Id = 3010,
//        UserId = 1,
//        RecipeName = "蔥油餅",
//        IsPublished = true,
//        RecipeIntro = "酥脆又有層次的蔥油餅，早餐或點心都適合。",
//        CookingTime = 30.00m,
//        Portion = 3.00m,
//        Rating = 4.7m,
//        RecipeVideoLink = "https://youtu.be/scallion_pancake",
//        CreatedAt = DateTime.Now,
//        UpdatedAt = DateTime.Now,
//        DisplayId = "R000010",
//        ViewCount = 400,
//        SharedCount = 38
//    }
//);


            context.SaveChanges();
        }

    }
}
