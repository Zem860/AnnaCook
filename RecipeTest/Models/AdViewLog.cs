using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace RecipeTest.Models
{
    public class AdViewLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name ="編號")]
        public int Id { get; set; }

        [Column(TypeName = "int")]
        [Display(Name = "廣告id")]
        public int AdvertisementId { get; set; }

        [JsonIgnore]
        [ForeignKey("AdvertisementId")]
        public virtual Advertisement Advertisement { get; set; }


        [Column(TypeName ="int")]
        [Display(Name ="使用者Id")]
        public int? UserId { get; set; }

        [Column(TypeName = "nvarchar")]
        [Display(Name ="SessionId")]
        [MaxLength(100)]
        public string SessionId { get; set; }

        [Column(TypeName ="bit")]
        [Display(Name ="點擊")]
        public bool IsClick {  get; set; }     // true = 點擊，false = 曝光

        [Display(Name = "曝光頁面")]
        public int AdDisplayPage { get; set; } = 0; // 建議加這個欄位

        [Column(TypeName ="DATETIME")]
        [Display(Name ="觸發時間")]
        public DateTime ViewedAt { get; set; }

    }
}