using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RecipeTest.Models;

namespace RecipeTest.Pages
{
    public class RecipeRelated
    {
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