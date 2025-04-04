using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System.ComponentModel;
using RecipeTest.Enums;
namespace RecipeTest.Models
{
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name ="編號")]
        public int Id { get; set; }

        [Required]
        [Index(IsUnique = true)]
        [MaxLength(10)]
        [Column(TypeName = "nvarchar")]
        [Display(Name = "使用者顯示ID")]
        public string DisplayId { get; set; }

        [Required(ErrorMessage ="{0}為必填")]
        [MaxLength(100)]
        [Column(TypeName ="nvarchar")]
        [Display(Name = "電子郵件")]
        public string AccountEmail {  get; set; }
        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(100)]
        [Display(Name = "密碼")]
        public string PasswordHash { get; set; } // 儲存哈希值
        [Required(ErrorMessage = "{0}為必填")]
        [MaxLength(100)]
        [Display(Name = "鹽巴")]
        public string Salt { get; set; } // 改為 byte[] 存儲安全的鹽值

        [Required(ErrorMessage ="{0}必填")]
        [MaxLength(50)]
        [Display(Name ="使用者名稱")]
        [Column(TypeName ="nvarchar")]
        public string AccountName {  get; set; }

        [MaxLength(500)]
        [Column(TypeName = "nvarchar")]
        [Display(Name = "使用者照片")]
        public string AccountProfilePhoto { set; get; } = null;

        [MaxLength(1000)]
        [Column(TypeName = "nvarchar")]
        [Display(Name = "使用者簡介")]
        public string UserIntro { get; set; } = null;

        [Column(TypeName = "bit")]
        [Display(Name = "是否以驗證")]
        public bool IsVerified { get; set; } = false;


        [Column(TypeName = "DATETIME")]
        [Display(Name = "創建時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "DATETIME")]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Display(Name ="登入方式")]
        public LoginProvider LoginProvider { get; set; } = LoginProvider.Local;
        [Display(Name ="使用者角色")]
        public UserRoles UserRole { get; set; } = UserRoles.User;

        public virtual ICollection<Follows> Following { get; set; }  // 我追蹤了誰
        public virtual ICollection<Follows> Followers { get; set; }  // 誰追蹤我
        public virtual ICollection<Favorites> Favorites { get; set; }
        public virtual ICollection<Recipes> Recipes { get; set; }
        public virtual ICollection<Comments> Comments { get; set; }
        public virtual ICollection<Ratings> Ratings { get; set; }

    }
}