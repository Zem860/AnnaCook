using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeTest.Models
{
    public class Follows
    {
        // **設定複合主鍵**
        [Key]
        [Column(Order = 0)]
        [Required]
        public int UserId { get; set; }

        [Key]
        [Column(Order = 1)]
        [Required]
        public int FollowedUserId { get; set; }

        [Column(TypeName = "DATETIME")]
        [Display(Name = "創建時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "DATETIME")]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;


        [JsonIgnore]
        [ForeignKey("UserId")]
        public virtual Users Follower { get; set; }

        [JsonIgnore]
        [ForeignKey("FollowedUserId")]
        public virtual Users FollowedUser { get; set; }
    }
}
