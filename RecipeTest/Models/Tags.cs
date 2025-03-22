using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace RecipeTest.Models
{
    public class Tags
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name ="編號")]
        public int Id { get; set; }
        [Required]
        [MaxLength(10)]
        [Column(TypeName = "nvarchar")]
        [Display(Name = "標籤名稱")]
        public string TagName { get; set; }
        [Column(TypeName = "DATETIME")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Column(TypeName = "DATETIME")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<RecipeTags> RecipeTags { get; set; }

    }
}