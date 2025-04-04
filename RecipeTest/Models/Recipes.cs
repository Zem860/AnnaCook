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
        //函式建構子constructor
        //public Recipes()
        //{
        //    RecipesPhotos = new List<RecipePhotos>();
        //}
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

        [Required]
        [Index(IsUnique = true)]
        [MaxLength(10)]
        [Column(TypeName = "nvarchar")]
        [Display(Name = "食譜顯示ID")]
        public string DisplayId { get; set; }


        [Column(TypeName = "int")]
        [Display(Name = "瀏覽次數")]
        public int ViewCount { get; set; } = 0;
        [Required]
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
        [Display(Name = "分享次數")]
        public int SharedCount { get; set; } = 0;

        [Column(TypeName = "decimal")]
        [Display(Name = "評分")]
        public decimal Rating { get; set; } = 0;
        
        [Column(TypeName ="nvarchar")]
        [Display(Name ="食譜影片Id")]
        public string RecipeVideoLink { get; set; } = null;

        //[Column(TypeName = "decimal")]
        //[Display(Name = "食譜影片時長")]
        //public decimal? RecipeVideoDuration { get; set; }  // 單位：秒，nullable 比較安全

        [Column(TypeName = "DATETIME")]
        [Display(Name = "創建時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "DATETIME")]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<RecipePhotos> RecipesPhotos { get; set; }
        public virtual ICollection<Ingredients> Ingredients { get; set; }
        public virtual ICollection<Steps> Steps { get; set; }

        public virtual ICollection<StepPhotos> StepPhotos { get; set; }

        public virtual ICollection<Favorites>Favorites { get; set; }
        public virtual ICollection<RecipeTags> RecipeTags { get; set; }

        public virtual ICollection<Comments> Comments { get; set; }

        public virtual ICollection<Ratings> Ratings { get; set; }
        //internal static void Add(string recipeName)
        //{
        //    throw new NotImplementedException();
        //}
    }
}