using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace RecipeTest.Models
{
    public class Steps
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "步驟編號")]
        public int Id { get; set; }

        [Required]
        public int RecipeId { get; set; }
        [JsonIgnore]
        [ForeignKey("RecipeId")]
        public virtual Recipe Recipe { get; set; }
        [Required]
        [Column(TypeName = "int")]
        [Display(Name = "步驟")]
        public int StepOrder { get; set; }

        [Required]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar")]
        [Display(Name = "步驟說明")]
        public string StepDescription { get; set; }
        [Required]
        [Column(TypeName = "int")]
        [Display(Name = "影片開始秒數")]
        public int VideoStart { get; set; }

        [Required]
        [Column(TypeName = "int")]
        [Display(Name = "影片結束秒數")]
        public int VideoEnd { get; set; }

        [Required]
        [Column(TypeName = "DATETIME")]
        [Display(Name = "創建時間")]

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        [Column(TypeName = "DATETIME")]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set;} = DateTime.UtcNow;
        public virtual ICollection<StepPhotos> StepPhotos { get; set; }
        public virtual ICollection<SubSteps> SubSteps { get; set; }

    }
}