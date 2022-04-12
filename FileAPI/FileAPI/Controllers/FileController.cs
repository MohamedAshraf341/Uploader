using FileAPI.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FileAPI.Controllers
{
    [EnableCors("MyPolicy")]

    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly string AppDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        private static List<FileRecord> fileDB = new List<FileRecord>();
        private readonly AppDbContext dBContext;

        public FileController(AppDbContext dBContext)
        {
            this.dBContext = dBContext;
        }
        //AppDbContext dBContext = new AppDbContext();

        [HttpPost("UploadFile")]
        [Consumes("multipart/form-data")]
        public async Task<HttpResponseMessage> UploadFile([FromForm] FileViewModel model)
        {
            try
            {
                FileRecord file = await SaveFileAsync(model.MyFile);

                if (!string.IsNullOrEmpty(file.FilePath))
                {
                    file.Name = model.Name;
                    file.AltText = model.AltText;
                    file.Description = model.Description;
                    //Save to Inmemory object
                    //fileDB.Add(file);
                    //Save to SQL Server DB
                    SaveToDB(file);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message),
                };
            }
        }

        private async Task<FileRecord> SaveFileAsync(IFormFile myFile)
        {
            FileRecord file = new FileRecord();
            if (myFile != null)
            {
                if (!Directory.Exists(AppDirectory))
                    Directory.CreateDirectory(AppDirectory);

                var uniquefileName = DateTime.Now.Ticks.ToString() + Path.GetExtension(myFile.FileName);
                var path = Path.Combine(AppDirectory, uniquefileName);
                file.Id = fileDB.Count() + 1;
                file.FilePath = path;
                file.UniqueName = uniquefileName;
                file.FileFormat = Path.GetExtension(myFile.FileName);
                file.ContentType = myFile.ContentType;

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await myFile.CopyToAsync(stream);
                }

                return file;
            }
            return file;
        }

        private void SaveToDB(FileRecord record)
        {
            if (record == null)
                throw new ArgumentNullException($"{nameof(record)}");
            FileData fileData = new FileData();
            fileData.FilePath = record.FilePath;
            fileData.UniqueName = record.UniqueName;
            fileData.Name = record.Name;
            fileData.FileExtension = record.FileFormat;
            fileData.MimeType = record.ContentType;
            fileData.AltText = record.AltText;
            fileData.Description= record.Description;

            dBContext.FileDatas.Add(fileData);
            dBContext.SaveChanges();
        }

        [HttpGet("GetAllFiles")]
        public List<FileRecord> GetAllFiles()
        {
            //getting data from inmemory obj
            //return fileDB;
            //getting data from SQL DB
            return dBContext.FileDatas.Select(n => new FileRecord
            {
                Id = n.Id,
                Name = n.Name,
                ContentType = n.MimeType,
                FileFormat = n.FileExtension,
                UniqueName = n.UniqueName ,
                FilePath = n.FilePath,
                AltText= n.AltText,
                Description= n.Description,
            }).ToList();
        }

        [HttpGet("DownloadFile/{id}")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            if (!Directory.Exists(AppDirectory))
                Directory.CreateDirectory(AppDirectory);

            //getting file from inmemory obj
            //var file = fileDB?.Where(n => n.Id == id).FirstOrDefault();
            //getting file from DB
            var file = dBContext.FileDatas.Where(n => n.Id == id).FirstOrDefault();

            var path = Path.Combine(AppDirectory, file?.FilePath);

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            var contentType = "APPLICATION/octet-stream";
            var fileName = file.Name;

            return File(memory, contentType, fileName);
        }
        [HttpDelete("DeleteFile/{id}")]
        public async Task<ActionResult<FileData>> DeleteFile(int id)
        {
            var file = await dBContext.FileDatas.FindAsync(id);
            if (file == null)
            {
                return NotFound();
            }

            dBContext.FileDatas.Remove(file);
            await dBContext.SaveChangesAsync();

            return file;
        }
    }
}
