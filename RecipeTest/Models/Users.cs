using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System.ComponentModel;
namespace RecipeTest.Models
{
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name ="編號")]
        public int Id { get; set; }
        [Required (ErrorMessage="{0}必填")]
        [MaxLength(100)]
        [Column(TypeName ="nvarchar")]
        public string AccountEmail {  get; set; }
        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(100)]
        [Display(Name = "密碼")]
        public byte[] PasswordHash { get; set; } // 儲存哈希值

        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(100)]
        [Display(Name = "鹽巴")]
        public byte[] Salt { get; set; } // 改為 byte[] 存儲安全的鹽值

        [Required(ErrorMessage ="{0}必填")]
        [MaxLength(50)]
        [Display(Name ="使用者名稱")]
        [Column(TypeName ="nvarchar")]
        public string AccountName {  get; set; }

        [MaxLength(500)]
        [Column(TypeName = "nvarchar")]
        [Display(Name = "使用者照片")]
        public string AccountProfilePhoto { set; get; } = null;

        [Column(TypeName = "DATETIME")]
        [Display(Name = "創建時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "DATETIME")]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<Follows> Following { get; set; }  // 我追蹤了誰
        public virtual ICollection<Follows> Followers { get; set; }  // 誰追蹤我
        public virtual ICollection<Favorites> Favorites { get; set; }
        public virtual ICollection<Recipes> Recipes { get; set; }
        public virtual ICollection<Comments> Comments { get; set; }
        public virtual ICollection<Ratings> Ratings { get; set; }

    }
}