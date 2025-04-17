using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeTest.Models
{
    public class FeaturedSection
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int Id { get; set; }


        [Required]
        [Column (TypeName = "int")]
        [Display(Name = "特色主題位置")]
        public int SectionPos { get; set; }
        [Required]
        [MaxLength(100)]
        [Column(TypeName ="nvarchar")]
        [Display(Name = "特色主題名稱")]
        public string FeaturedSectionName { get; set; }

        [Required]
        [Column(TypeName = "nvarchar")]
        [StringLength(100)]
        [Display(Name = "特色主題tags")]
        public string FeaturedSectionTags { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        [Display(Name = "是否進行中")]
        public bool IsActive { get; set; }

        [Column(TypeName ="DateTime")]
        [Display(Name = "建立時間")]

        public DateTime CreatedAt { get; set; }

        [Column(TypeName = "DateTime")]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; }
    }
}