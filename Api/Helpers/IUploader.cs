using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MovieProfanityDetector.Models;
using Newtonsoft.Json;

namespace MovieProfanityDetector.Helpers
{
    public interface IUploader
    {
        Task<KeyValuePair<string, bool>> UploadFile(IFormFile file, string filePath, string fileName);
        Task<FileUploadResult> UploadChunk(IFormFile file, ChunkMetaData metaDataObject, Guid sourceGuid);
        KeyValuePair<string, bool> DeleteFile(string filePath);
    }

    public class Uploader : IUploader
    {
        private readonly List<string> _allowedExtension = new List<string>
            {".pdf", ".jpg", ".jpeg", ".png", ".mp4", ".mpg", ".webm", "avi"};

        public async Task<KeyValuePair<string, bool>> UploadFile(IFormFile file, string filePath, string fileName)
        {
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);


                var fileExtension = Path.GetExtension(file.FileName);

                if (!_allowedExtension.Contains(fileExtension))
                {
                    return new KeyValuePair<string, bool>("File Extension Is Not Allowed", false);
                }


                //var fileName = sourceId + fileExtension;

                var photoPath = Path.Combine(path, fileName);
                if (File.Exists(photoPath))
                    File.Delete(photoPath);

                await using var stream = new FileStream(photoPath, FileMode.Create);
                await file.CopyToAsync(stream);
                return new KeyValuePair<string, bool>(Path.Combine(filePath, fileName), true);
            }
            catch (Exception)
            {
                return new KeyValuePair<string, bool>("Some Error Occurred While Uploading File", false);
            }
        }

        public async Task<FileUploadResult> UploadChunk(IFormFile file, ChunkMetaData metaDataObject, Guid sourceGuid)
        {
            try
            {
                if (file != null)
                {
                    var tempPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Temp");
                    if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);

                    //var dir = new DirectoryInfo(tempPath);

                    var tempFilePath = Path.Combine(tempPath, metaDataObject.FileGuid + ".tmp");
                    await using (var stream = new FileStream(tempFilePath, FileMode.Append, FileAccess.Write))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var fileExtension = Path.GetExtension(metaDataObject.FileName);
                    var fileName = sourceGuid + fileExtension;

                    if (!_allowedExtension.Contains(fileExtension))
                    {
                        return new FileUploadResult
                        {
                            Message = "File Extension Is Not Allowed",
                            IsUploaded = false
                            // KeyValuePair<string, bool>(, false)
                        };
                    }

                    var orderPath = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot", Path.Combine("Movies", sourceGuid.ToString()));

                    var result = Path.Combine("Movies", sourceGuid.ToString());

                    if (metaDataObject.Index == (metaDataObject.TotalCount - 1))
                    {
                        if (!Directory.Exists(orderPath)) Directory.CreateDirectory(orderPath);
                        var path = Path.Combine(orderPath, fileName);

                        File.Copy(tempFilePath, path);

                        File.Delete(tempFilePath);
                    }

                    //return new KeyValuePair<string, bool>(result, true);
                    return new FileUploadResult
                    {
                        FileName = fileName,
                        FilePath = result,
                        IsUploaded = true
                    };
                }
            }
            catch (Exception e)
            {
                // return new KeyValuePair<string, bool>(e.Message, false);

                return new FileUploadResult
                {
                    Message = e.Message,
                    IsUploaded = false
                };
            }

            return new FileUploadResult
            {
                Message = "Error Uploading File",
                IsUploaded = false
            };

            //return new KeyValuePair<string, bool>("Error Uploading File", false);
        }

        public KeyValuePair<string, bool> DeleteFile(string filePath)
        {
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath);
                if (!File.Exists(path))
                    File.Delete(path);

                return new KeyValuePair<string, bool>(filePath, true);
            }
            catch (Exception)
            {
                return new KeyValuePair<string, bool>("Some Error Occurred While Deleting File", false);
            }
        }
    }

    public class FileUploadResult
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Message { get; set; }
        public bool IsUploaded { get; set; }
    }
}