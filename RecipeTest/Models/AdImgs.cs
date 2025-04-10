using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace RecipeTest.Models
{
    public class AdImgs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "廣告圖片編號")]
        public int Id { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(max)")]
        [Display(Name = "圖片位置")]
        public string ImgUrl { get; set; }

        public int AdId { get; set; }
        [JsonIgnore]
        [ForeignKey("AdId")]
        public virtual Advertisement Advertisement { get; set; }

        [Column(TypeName ="DATETIME")]
        [Display(Name = "創建時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Column(TypeName = "DATETIME")]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;


    }
}