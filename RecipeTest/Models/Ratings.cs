using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace RecipeTest.Models
{
    public class Ratings
    {
        [Key]
        [Column( Order=0)]
        public int UserId { get; set; }
        [JsonIgnore]

        [ForeignKey("UserId")]

        public virtual Users Users { get; set; }
        [Key]
        [Column(Order = 1)]

        public int RecipeId { get; set; }
        [JsonIgnore]
        [ForeignKey("RecipeId")]
        public virtual Recipes Recipes { get; set; }

        [Column(TypeName = "decimal")]
        [Display(Name = "評分")]
        public decimal Rating { get; set; } = 0;

        [Column(TypeName = "DATETIME")]
        [Display(Name = "創建時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "DATETIME")]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

    }
}