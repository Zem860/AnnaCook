using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using RecipeTest.Models;

namespace RecipeTest.Pages
{
    public class RecipeRelated
    {
        public class FeatureCustomTags
        {
            [JsonProperty("sectionPos")]
            public int SectionPos { get; set; }
            [JsonProperty("SectionName")]
            public string SectionName { get; set; }
            [JsonProperty("customTags")]
            public List<string> CustomTags { get; set; }
            [JsonProperty("isActive")]
            public bool isActive { get; set; }
        }


        private Recipes GetValidRecipe(int id)
        {
            RecipeModel db = new RecipeModel();
            return db.Recipes.FirstOrDefault(r => r.Id == id && !r.IsDeleted);
        }

        public class StepUpdateListDto
        {
            public string VideoUrl { get; set; }
            public List<StepDto> Steps { get; set; }
        }
        public class StepDto
        {
            public int StepId { get; set; }
            public string Description { get; set; }  // 對應說明
            public decimal StartTime { get; set; }       // 對應影片開始秒數
            public decimal EndTime { get; set; }         // 對應影片結束秒數

        }
        public class UserRecipeDetail
        {
            public string RecipeIntro { get; set; }
            public decimal CookingTime { get; set; }
            public decimal Portion { get; set; }
            public List<UserIngredients> Ingredients{ get; set; }
            public List <string> Tags { get; set; }
        }

        public class UserIngredients
        {
            public string IngredientName { get; set; }
            public decimal IngredientAmount { get; set; }
            public string IngredientUnit { get; set; }
            public bool IsFlavoring { get; set; } // true = 調味料, false = 食材

        }

            public class UserTags
        {
            public string Tag { get; set; }

        }

        public class RecipeCard
        {
            public int RecipeId { get; set; }
            public string RecipeName { get; set; }
            public string CoverPhoto { get; set; }
            public string RecipeIntro { get; set; }
            public string CookingTime { get; set; }
            public decimal Portion { get; set; }
            public decimal Rating { get; set; }
        }

        //Post使用創建食譜需要
        public class RecipePlaceholder
        {
            public string RecipeName { get; set; }
            public string CoverPhoto { get; set; }
        }

   
    }
}