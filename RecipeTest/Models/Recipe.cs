using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeTest.Models
{
    public class Recipe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name ="食譜編號")]
        public int Id { get; set; }
        [Required]
        [MaxLength(20)]
        [Display(Name = "食譜名稱")]

        [Column(TypeName ="nvarchar")]
        public string RecipeName { get; set; }
        [Required]
        [Column(TypeName = "bit")]
        [Display(Name ="已發布")]
        public bool IsPublished { get; set; } = false;
        [Required]
        [MaxLength(500)]
        [Column(TypeName ="nvarchar")]
        public string RecipeIntro { get; set; }
        [Required]
        [Column(TypeName ="decimal")]
        [Display(Name = "烹調時間")]
        public decimal CookingTime { get; set; }
        [Required]
        [Column(TypeName = "decimal")]
        [Display(Name = "份數")]
        public decimal Portion { get; set; }
        [Required]
        [Column(TypeName = "int")]
        [Display(Name = "按讚數")]
        public int LikedNumber { get; set; }
        [Required]
        [Column(TypeName ="decimal")]
        [Display(Name ="評分")]
        public decimal Rating { get; set; }
        [Required]
        [Column(TypeName ="nvarchar")]
        [Display(Name ="食譜影片Id")]
        public string RecipeVideoLink { get; set; }
        [Required]
        [Column(TypeName = "DATETIME")]
        [Display(Name = "創建時間")]

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        [Column(TypeName = "DATETIME")]
        [Display(Name ="更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<RecipePhotos> RecipesPhotos { get; set; }
        public virtual ICollection<Ingredients> Ingredients { get; set; }
        public virtual ICollection<Steps> Steps { get; set; }
    }
}