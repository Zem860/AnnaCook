using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace RecipeTest.Pages
{
    public class ComplexUserRecipe
    {
        public class RatingDto
        {
            [JsonProperty("rating")]

            public decimal Rating { get; set; }
        }


        public class RatingCommentDto
        {
            [JsonProperty("rating")]

            public decimal Rating { get; set; }
            [JsonProperty("commentContent")]
            public string CommentContent { get; set; }
        }

    }
}