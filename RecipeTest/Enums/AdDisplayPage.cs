using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RecipeTest.Enums
{
    public enum AdDisplayPageType
    {
        [Display(Name = "未知")]
        Unknown = 0,

        [Display(Name = "首頁廣告")]
        Home = 1,
        [Display(Name = "食譜列表頁廣告")]
        SearchList = 2,

        [Display(Name = "食譜內頁廣告")]
        RecipeDetail = 3,

    }

}