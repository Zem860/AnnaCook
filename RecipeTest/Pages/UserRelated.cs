using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RecipeTest.Models;

namespace RecipeTest.Pages
{


    public class UserRelated
    {

        public class UserUpdateData
        {
            public int Id { get; set; }
            public string AccountName { get; set; }
            public string AccountEmail { get; set; }
        }

        public class ClientRegisterData
        {
            public int Id { get; set; }
            public string AccountEmail { get; set; }
            public string AccountName { get; set; }
            public string Password { get; set; }   
        }

        public class UserRegisterData
        {
            public int Id { get; set; }
            public string AccountEmail { get; set; }
            public string AccountName { get; set; }
            public byte[] Password { get; set; }
            public byte[] Salt { get; set; }


        }


        public class UserTokenData
        {

            public int Id { get; set; }
            public string Account { get; set; }
            public string AccountName { get; set; }

            public string Exp { get; set; } 

        }
    }

}