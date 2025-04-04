using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MyWebApiProject.Security;
using RecipeTest.Models;
using RecipeTest.Security;
using System.Threading.Tasks;
using System.IO;
using System.Web;

namespace RecipeTest.Controllers
{

    public class ExtraController : ApiController

    {
        private UserEncryption userhash = new UserEncryption();
        private RecipeModel db = new RecipeModel();
        private string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
        private string localStorragePath = HttpContext.Current.Server.MapPath("~/TestPhotos");

        //理論上這隻api應該是要讓recipe的照片用post的方式增加紀錄等寫完會員中心來修正

        [HttpPut]
        [Route("api/recipeCover/{id}")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> replacePhoto(int id)
        {
            var user = userhash.GetUserFromJWT();
            bool hasUser = db.Users.FirstOrDefault(u=>u.Id == user.Id)!= null;
            if (hasUser)
            {
                var recipePhoto = db.RecipePhotos.FirstOrDefault(r => r.Id == id);
                var provider = await Request.Content.ReadAsMultipartAsync();
                var formData = provider.Contents;
                var photoContent = formData.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "profilePhoto");
                if (photoContent != null && photoContent.Headers.ContentDisposition.FileName != null)
                {
                    var file = photoContent.Headers.ContentDisposition.FileName.Trim('"');
                    string extension = Path.GetExtension(file).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        return BadRequest("只允許上傳 .jpg, .jpeg, .png檔案");
                    }

                    string newFileName = Guid.NewGuid().ToString("N") + extension;
                    string relativePath = "/TestPhotos/" + newFileName;
                    recipePhoto.ImgUrl = relativePath;
                    recipePhoto.IsCover = true;
                    string fullPath = Path.Combine(localStorragePath, newFileName);
                    var fileBytes = await photoContent.ReadAsByteArrayAsync();
                    File.WriteAllBytes(fullPath, fileBytes); // ⬅️ 真正把圖片寫進磁碟
                    db.SaveChanges();

                }
                var res = new
                {
                    StatusCode = 200,
                    msg = "修改照片成功",
                    Id = id,
                };
                return Ok(res);

            } else
            {
                return NotFound();
            }



        }
    }
}
