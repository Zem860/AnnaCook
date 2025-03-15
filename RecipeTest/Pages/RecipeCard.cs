using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RecipeTest.Pages
{
    public class RecipeCard
    {
        public string RecipeName { get; set; }
        public string CoverPhoto { get; set; }
        public string RecipeIntro { get; set; }
        public string CookingTime { get; set; }
        public decimal Portion { get; set; }
        public decimal Rating { get; set; }
    }
}