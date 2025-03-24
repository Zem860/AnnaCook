using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace RecipeTest.Models
{
    public class Ingredients
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name ="食材編號")]
        public int Id { get; set; }
        [Required]
        public int RecipeId { get; set; }
        [JsonIgnore]
        [ForeignKey("RecipeId")]
        public virtual Recipes Recipe { get; set; }

        [Required]
        [MaxLength(20)]
        [Column(TypeName ="nvarchar")]
        public string IngredientName { get; set; }
        [Required]
        [Column(TypeName ="bit")]
        [Display(Name ="是否為食材")]
        public bool IsFlavoring {  get; set; }
        [Required]
        [Column(TypeName = "decimal")]
        [Display(Name = "數量")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(20)]
        [Column(TypeName ="nvarchar")]
        [Display(Name ="單位")]
        public string Unit { get; set; }

        [Column(TypeName = "DATETIME")]
        [Display(Name = "創建時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "DATETIME")]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;


    }
}