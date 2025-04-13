using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecipeTest.Enums;
namespace RecipeTest.Models
{
    public class LoginRecords
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號" )]
        public int Id { get; set; }

        [Required]
        [Display(Name = "會員編號")]
        public int UserId { get; set; }


        [MaxLength(45)]
        [Column(TypeName = "nvarchar")]
        [Display(Name = "IP 位址")]
        public string IpAddress { get; set; } = null;


        [Column(TypeName = "int")]
        [Display(Name = "登入狀態")]
        public int LoginAction { get; set; } = (int)LoginActions.LoginSuccess;  


        [Display(Name = "動作時間")]
        [Column(TypeName = "DATETIME")]
          
        public DateTime ActionTime { get; set; } = DateTime.Now;

    }
}