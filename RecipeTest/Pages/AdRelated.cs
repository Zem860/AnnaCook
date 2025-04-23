using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace RecipeTest.Pages
{
    public class AdRelated
    {
        public class AdLogDto
        {
            [JsonProperty("adId")]
            public int AdId { get; set; }
            [JsonProperty("isClick")]

            public bool IsClick {  get; set; }
            [JsonProperty("sessionId")]

            public Guid SessionId { get; set; }
            [JsonProperty("pos")]

            public string Pos { get; set; } // ✅ 用 string 接收 "home", "search", "recipe"
        }

        public class AdData 
        {
            [JsonProperty("adName")]
            public string AdName { get; set; }
            [JsonProperty("linkUrl")]
            public string LinkUrl { get; set; }

            [JsonProperty("pos")]
            public string Pos { get; set; } // ✅ 用 string 接收 "home", "search", "recipe"

            [JsonProperty("adIntro")]
            public string AdIntro { get; set; }

            [JsonProperty("adPrice")]
            public string AdTitle { get; set; }

            [JsonProperty("adImages")]
            public List<string> AdImages { get; set; } = new List<string>();
            [JsonProperty("adStatus")]
            public string AdStatus { get; set; } // ✅ 用 string 接收 "草稿", "排程", "進行中", "已結束"

            [JsonProperty("startDate")]
            public DateTime StartDate { get; set; }
            [JsonProperty("endDate")]
            public DateTime EndDate { get; set; }
        }
    }
}