using System.Collections.Generic;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using static RecipeTest.Pages.UserRelated;

namespace RecipeTest.Security
{
    public class UserEncryption
    {
        public UserDto GetUserFromJWT()
        {
            var payload = HttpContext.Current.Items["jwtUser"] as Dictionary<string, object>;

            return new UserDto
            {
                Id = int.Parse(payload["Id"].ToString()),
                AccountEmail = payload["Account"].ToString(),
                AccountName = payload["AccountName"].ToString(),
            };

        }
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