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
    public class User
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
        public string AccountProfilePhoto { set; get; }

        [DisplayFormat(ApplyFormatInEditMode =true, DataFormatString ="{0:d}")]
        [Column(TypeName="datetime")]
        [Display(Name ="創建時間")]
        public DateTime CreatedAt { get; set; }= DateTime.UtcNow;
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString="{0:d}")]
        [Column(TypeName ="datetime")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}