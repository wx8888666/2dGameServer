using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using Yitter.IdGenerator;

namespace _2DSurviveGameServer.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly string tempPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "temp"); // 临时缓存目录
        private readonly string resourcesPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "resources"); // 资源文件目录
        private readonly string manifestFilePath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "manifest.json"); // 资源清单文件路径

        // 上传图片
        [HttpPost]
        public ActionResult UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            // 创建临时目录（如果不存在）
            Directory.CreateDirectory(tempPath);
            // 生成唯一的文件名
            string fileName = YitIdHelper.NextId() + Path.GetExtension(file.FileName);
            // 生成文件保存路径
            string filePath = Path.Combine(tempPath, fileName);
            // 将文件保存到指定路径
            using (FileStream fs = System.IO.File.Create(filePath))
            {
               //创建一个新的 FileStream 对象，负责将文件写入磁盘。
              //using: 确保 FileStream 对象在 using 块结束时会调用其 Dispose 方法，释放文件句柄和相关资源。
                file.CopyTo(fs);
                fs.Flush();

            }
            // 返回文件名作为响应
            return Ok(fileName);
        }


       //上传多个文件（包括 AB 包和 Manifest 文件）
        [HttpPost]
        public async Task<ActionResult> UploadFiles([FromForm] IFormFileCollection files)
        {
            if (files == null || files.Count == 0)
            {
                this.Log("文件为空");
                return BadRequest("No files uploaded.");
            }

            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    continue;
                }

                if (file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    await SaveFileAsync(file, tempPath);
                    UpdateManifest(file.FileName);
              
                }
               
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string newResourcesPath = Path.Combine(resourcesPath, timestamp);
                    Directory.CreateDirectory(newResourcesPath);
                    await SaveFileAsync(file, newResourcesPath);
                
            }

            return Ok("Files uploaded successfully.");
        }

        // 保存文件的方法
        private async Task SaveFileAsync(IFormFile file, string folderPath)
        {
            string filePath = Path.Combine(folderPath, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }

        // 更新 manifest.json
        private void UpdateManifest(string newFileName)
        {
            Dictionary<string, string> manifest;

            if (System.IO.File.Exists(manifestFilePath))
            {
                string manifestContent = System.IO.File.ReadAllText(manifestFilePath);
                manifest = JsonSerializer.Deserialize<Dictionary<string, string>>(manifestContent);
            }
            else
            {
                manifest = new Dictionary<string, string>();
            }

            string fileHash = CalculateFileHash(Path.Combine(tempPath, newFileName));
            manifest[newFileName] = fileHash;

            string updatedManifestContent = JsonSerializer.Serialize(manifest);
            System.IO.File.WriteAllText(manifestFilePath, updatedManifestContent);
        }

        // 计算文件的 SHA256 哈希值
        private string CalculateFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    byte[] hash = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        [HttpGet]
        public ActionResult DownloadLatestFolder()
        {
            var directories = Directory.GetDirectories(resourcesPath);
            if (directories.Length == 0)
            {
                return NotFound("No folders found.");
            }

            // 获取最近修改的文件夹
            var latestFolder = directories
                .OrderByDescending(d => new DirectoryInfo(d).LastWriteTime)
                .FirstOrDefault();

            if (latestFolder != null)
            {
                var tempFilePath = Path.GetTempFileName();
                var zipFilePath = tempFilePath + ".zip";

                try
                {
                    // 创建压缩文件
                    ZipFile.CreateFromDirectory(latestFolder, zipFilePath);

                    // 返回压缩文件
                    var fileBytes = System.IO.File.ReadAllBytes(zipFilePath);
                    return File(fileBytes, "application/zip", "resources.zip");
                }
                finally
                {
                    // 清理临时文件
                    if (System.IO.File.Exists(zipFilePath))
                    {
                        System.IO.File.Delete(zipFilePath);
                    }
                }
            }

            return NotFound("No folder found.");
        }
        //热更新下载服务器上最新文件逻辑
        //[HttpGet]
        //public ActionResult GetLatestFolder()
        //{
        //    var directories = Directory.GetDirectories(resourcesPath);
        //    if (directories.Length == 0)
        //    {
        //        return NotFound("No folders found.");
        //    }

        //    // 获取最近修改的文件夹
        //    var latestFolder = directories
        //        .OrderByDescending(d => new DirectoryInfo(d).LastWriteTime)
        //        .FirstOrDefault();

        //    if (latestFolder != null)
        //    {
        //        var folderName = Path.GetFileName(latestFolder);
        //        return Ok(folderName);
        //    }
        //    return NotFound("No folder found.");
        //}
        //[HttpGet("DownloadFolder/{folderName}")]
        //public ActionResult DownloadFolder(string folderName)
        //{
        //    var folderPath = Path.Combine(resourcesPath, folderName);
        //    if (!Directory.Exists(folderPath))
        //    {
        //        return NotFound("Folder not found.");
        //    }

        //    var tempFilePath = Path.GetTempFileName();
        //    var zipFilePath = tempFilePath + ".zip";

        //    try
        //    {
        //        // 创建压缩文件
        //        ZipFile.CreateFromDirectory(folderPath, zipFilePath);

        //        // 返回压缩文件
        //        var fileBytes = System.IO.File.ReadAllBytes(zipFilePath);
        //        return File(fileBytes, "application/zip", "resources.zip");
        //    }
        //    finally
        //    {
        //        // 清理临时文件
        //        if (System.IO.File.Exists(zipFilePath))
        //        {
        //            System.IO.File.Delete(zipFilePath);
        //        }
        //    }
        //}
    }

}
