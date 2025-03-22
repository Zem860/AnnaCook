using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using System.Web.UI.WebControls;
using RecipeTest.Models;

namespace RecipeTest.Security
{
    public class UserEncryption
    {
        public byte[] createSalt()
        {
            var buffer = new byte[16];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(buffer);
            return buffer;
        }


        public byte[] HashPassword(string password, byte[] salt)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
            argon2.Salt = salt;
            argon2.DegreeOfParallelism = 8;
            argon2.Iterations = 4;
            argon2.MemorySize = 1024 * 1024;
            return argon2.GetBytes(16);
        }

        public bool loginSuccess(string account, string password, byte[] salt)
        {


            return false;
        }

    }
}