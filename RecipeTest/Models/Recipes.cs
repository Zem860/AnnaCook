using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace RecipeTest.Models
{
    public class Recipes
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name ="食譜編號")]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "int")]
        [Display(Name = "使用者Id")]
        public int UserId { get; set; }

        [JsonIgnore]
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }

        [MaxLength(20)]
        [Display(Name = "食譜名稱")]

        [Column(TypeName ="nvarchar")]
        public string RecipeName { get; set; }

        [Column(TypeName = "bit")]
        [Display(Name ="已發布")]
        public bool IsPublished { get; set; } = false;
        [MaxLength(500)]
        [Column(TypeName ="nvarchar")]
        public string RecipeIntro { get; set; } = null;

        [Column(TypeName = "decimal")]
        [Display(Name = "烹調時間")]
        public decimal CookingTime { get; set; } = 0;

        [Column(TypeName = "decimal")]
        [Display(Name = "份數")]
        public decimal Portion { get; set; } = 0;

        [Column(TypeName = "int")]
        [Display(Name = "按讚數")]
        public int LikedNumber { get; set; } = 0;

        [Column(TypeName = "decimal")]
        [Display(Name = "評分")]
        public decimal Rating { get; set; } = 0;
        
        [Column(TypeName ="nvarchar")]
        [Display(Name ="食譜影片Id")]
        public string RecipeVideoLink { get; set; } = null;
        [Column(TypeName = "DATETIME")]
        [Display(Name = "創建時間")]

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Column(TypeName = "DATETIME")]
        [Display(Name ="更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<RecipePhotos> RecipesPhotos { get; set; }
        public virtual ICollection<Ingredients> Ingredients { get; set; }
        public virtual ICollection<Steps> Steps { get; set; }

        public virtual ICollection<StepPhotos> StepPhotos { get; set; }

        public virtual ICollection<Favorites>Favorites { get; set; }
        public virtual ICollection<RecipeTags> RecipeTags { get; set; }

        //internal static void Add(string recipeName)
        //{
        //    throw new NotImplementedException();
        //}
    }
}