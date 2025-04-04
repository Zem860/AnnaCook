using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace RecipeTest.Models
{
    public class StepPhotos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name ="步驟圖片編號")]
        public int Id { get; set; }
        [Required]
        public int StepId { get; set; }
        [JsonIgnore]
        [ForeignKey("StepId")]
        public virtual Steps Steps {  get; set; }
        [Required]
        [Column(TypeName ="bit")]
        [Display(Name ="是否為封面")]
        public bool IsCover {  get; set; }

        [Column(TypeName = "DATETIME")]
        [Display(Name = "創建時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "DATETIME")]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

    }
}