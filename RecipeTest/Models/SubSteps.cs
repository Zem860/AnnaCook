using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace RecipeTest.Models
{
    public class SubSteps
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name ="次步驟編號")]
        public int Id { get; set; }
        [Required]
        public int StepId { get; set; }
        [JsonIgnore]
        [ForeignKey("StepId")]
        public virtual Steps Steps { get; set; }
        [Required]
        [MaxLength(45)]
        [Column(TypeName ="nvarchar")]
        [Display(Name ="步驟解說")]
        public string StepContent { get; set; }

        [Column(TypeName = "DATETIME")]
        [Display(Name = "創建時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "DATETIME")]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;


    }
}