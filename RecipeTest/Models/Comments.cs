using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace RecipeTest.Models
{
    public class Comments
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int Id { get; set; }
        [Required]

        public int UserId { get; set; }
        [JsonIgnore]
        [ForeignKey("UserId")]
        public virtual Users Users { get; set; }

        [Required]
        public int RecipeId { get; set; }
        [JsonIgnore]
        [ForeignKey("RecipeId")]
        public virtual Recipes Recipes { get; set; }

        [Required]
        [MaxLength(500)]
        [Column(TypeName = "nvarchar")]
        [Display(Name = "留言內容")]
        public string CommentContent { get; set; }

        [Column(TypeName = "DATETIME")]
        [Display(Name = "創建時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "DATETIME")]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;


    }
}