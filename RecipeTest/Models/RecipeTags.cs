using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeTest.Models
{
    public class RecipeTags
    {
        [Key]
        [Column(Order = 0)]
        public int RecipeId { get; set; }
        [Key]
        [Column(Order = 1)]
        public int TagId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        [ForeignKey("RecipeId")]
        public virtual Recipes Recipes { get; set; }
        [ForeignKey("TagId")]

        public virtual Tags Tags { get; set; }
    }
}