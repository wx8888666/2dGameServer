using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yitter.IdGenerator;

namespace _2DSurviveGameServer.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        string tempPath = AppContext.BaseDirectory + "wwwroot\\temp\\";//临时缓存目录


        [HttpPost]
        public ActionResult UploadImage(IFormFile file)
        {
            Directory.CreateDirectory(tempPath);
            string fileName = YitIdHelper.NextId() + Path.GetExtension(file.FileName);
            using(FileStream fs = System.IO.File.Create(tempPath + fileName))
            {
                file.CopyTo(fs);
                fs.Flush();
            }

            return Ok(fileName);
        }

    }
}
