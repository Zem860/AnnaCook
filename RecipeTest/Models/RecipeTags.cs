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
        [Column(TypeName = "DATETIME")]
        [Display(Name = "創建時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "DATETIME")]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("RecipeId")]
        public virtual Recipes Recipes { get; set; }
        [ForeignKey("TagId")]

        public virtual Tags Tags { get; set; }
    }
}