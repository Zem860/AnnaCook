using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeTest.Models
{
    public class AdDisplayPage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "廣告顯示頁面編號")]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        [Column(TypeName = "nvarchar")]
        [Display(Name = "廣告顯示頁面名稱")]
        public string AdDisplayPageName { get; set; }

        [Column(TypeName = "DATETIME")]
        [Display(Name = "創建時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "DATETIME")]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}