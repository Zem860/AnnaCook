using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RecipeTest.Models;

namespace RecipeTest.Pages
{
    public class UserRelated
    {
        public class UserTokenData
        {

            public int Id { get; set; }
            public string Account { get; set; }
            public string AccountName { get; set; }

            public string Exp { get; set; } 

        }
    }

}