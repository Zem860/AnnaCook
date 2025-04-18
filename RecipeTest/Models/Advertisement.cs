using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeTest.Models
{
    public class Advertisement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "廣告編號")]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        [Column(TypeName = "nvarchar")]
        [Display(Name = "廣告名稱")]
        public string AdName { get; set; }

        [Required]
        [Display(Name = "投放位置(類型)")]
        [Column(TypeName = "int")]
        public int AdDisplayPage { get; set; } // 0:首頁, 1:食譜列表頁, 2:食譜內頁

        [Column(TypeName = "bit")]
        [Display(Name = "是否上架")]
        public bool IsEnabled { get; set; } = false;

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        [Display(Name = "圖片連結")]
        public string LinkUrl { get; set; }

        [Column(TypeName = "decimal")]
        [Display(Name = "廣告商品價")]
        public decimal? AdPrice { get; set; } = 0;

        [Required]
        [MaxLength(20)]
        [Column(TypeName = "nvarchar")]
        [Display(Name = "幣別")]
        public string Currency { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName ="nvarchar")]
        public string AdIntro { get; set; }

        [Column(TypeName = "DATETIME")]
        [Display(Name = "上架日期")]
        public DateTime StartDate { get; set; } = DateTime.Now;


        [Column(TypeName = "DATETIME")]
        [Display(Name = "上架日期")]
        public DateTime EndDate { get; set; } = DateTime.Now;

        [Column(TypeName = "DATETIME")]
        [Display(Name = "創建時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "DATETIME")]
        [Display(Name = "更新時間")]
        public DateTime UpdateTime { get; set; } = DateTime.Now;

        public virtual ICollection<AdTags> AdTags { get; set; }
        public virtual ICollection<AdImgs> AdImgs { get; set; }
    }
}