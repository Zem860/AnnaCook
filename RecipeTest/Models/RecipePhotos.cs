using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace RecipeTest.Models
{
    public class RecipePhotos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name ="食譜圖片編號")]
        public int Id { get; set; }

        [Required]
        public int RecipeId {  get; set; }
        [JsonIgnore]
        [ForeignKey("RecipeId")]
        public virtual Recipes Recipe { get; set; }
        [Required]
        [Column(TypeName ="nvarchar(max)")]
        [Display(Name ="圖片位置")]
        public string ImgUrl {  get; set; }

        [Column(TypeName = "bit")]
        [Display(Name = "是否為封面")]
        public bool IsCover { get; set; } = true;

        [Column(TypeName = "DATETIME")]
        [Display(Name = "創建時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "DATETIME")]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;


    }
}