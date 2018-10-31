using KeysPlus.Data;
using KeysPlus.Service.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace KeysPlus.Service.Services
{
   public class Base64Image
    {
        public static Base64Image LoadImage(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (!IsAcceptable(filePath.Split('.').Last()))
                throw new ArgumentOutOfRangeException(nameof(filePath));
            try
            {
                using (var data = Image.FromFile(filePath))
                {
                    using (var m = new MemoryStream())
                    {
                        data.Save(m, data.RawFormat);
                        var imageBytes = m.ToArray();
                        return new Base64Image
                        {
                            FileContents = imageBytes,
                            ContentType = filePath.Split('.').Last()
                        };
                    }
                }
            }
            catch (Exception e)
            {
                return new Base64Image
                {
                    FileContents = null,
                    ContentType = e.ToString()
                };

            }
        }

        public static Base64Image Parse(string base64Content)
        {
            if (string.IsNullOrEmpty(base64Content))
            {
                throw new ArgumentNullException(nameof(base64Content));
            }

            var indexOfSemiColon = base64Content.IndexOf(";", StringComparison.OrdinalIgnoreCase);
            var dataLabel = base64Content.Substring(0, indexOfSemiColon);
            var contentType = dataLabel.Split(':').Last();
            if (contentType.Contains('/'))
                contentType = contentType.Split('/').Last();
            var startIndex = base64Content.IndexOf("base64,", StringComparison.OrdinalIgnoreCase) + 7;
            var fileContents = base64Content.Substring(startIndex);
            var bytes = Convert.FromBase64String(fileContents);

            return new Base64Image
            {
                ContentType = contentType,
                FileContents = bytes
            };
        }

        public string ContentType { get; set; }

        public byte[] FileContents { get; set; }

        private static readonly List<string> AcceptedExtensions = new List<string>
            {
                "jpg",
                "png",
                "gif",
                "jpeg",
                ".jpg",
                ".png",
                ".gif",
                ".jpeg"
            };

        public static bool IsAcceptable(string extension)
        {
            return AcceptedExtensions.Contains(extension.ToLower());
        }

        public void Save(string filePath)
        {
            using (var ms = new MemoryStream(FileContents))
            {
                var image = Image.FromStream(ms);
                image.Save(filePath);
            }
        }

        public override string ToString()
        {
            if(FileContents != null)
                return $"data:image/{ContentType};base64,{Convert.ToBase64String(FileContents)}";
            return $"data:image/jpeg;base64,0";
        }
    }

    public static class MediaService
    {
        const string _error = "Please try again later.";
        static Dictionary<string, List<string>> allowedExtentions = new Dictionary<string, List<string>>()
        {
            { "AllFiles" , new List<string> { ".pdf", ".doc", ".docx", ".jpg", ".png", ".gif", ".jpeg" }},
            {"Images" , new List<string> { ".jpg", ".png", ".gif", ".jpeg" }},
            { "Documents", new List<string> { ".pdf", ".doc", ".docx" }}
        };
        static List<string> acceptedExtensions = new List<string> { ".pdf", ".doc", ".docx", ".jpg", ".png", ".gif", ".jpeg" };
        static List<string> imageExtensions = new List<string> { ".jpg", ".png", ".gif", ".jpeg" };
        static List<string> docExtensions = new List<string> { ".pdf", ".doc", ".docx"};
        static string documentPath = HttpContext.Current.Server.MapPath("~/documents");
        static string imagetPath = HttpContext.Current.Server.MapPath("~/images");
        public static ServiceResponseResult SaveMediaFiles(HttpFileCollectionBase files, int maxFiles, string serverPath = null)
        {
            if (files.Count > maxFiles)
            {
                return new ServiceResponseResult { IsSuccess = false, ErrorMessage = $"You can't add more than {maxFiles} photos" };
            };
            var mediaFiles = new List<MediaModel>();
            if (files != null && files.Count > 0)
            {
                var numberOfFiles = files.Count;
                for (int i = 0; i < numberOfFiles; i++)
                {
                    var file = files[i];
                    var fileExtension = Path.GetExtension(file.FileName);
                    var isAccepted = acceptedExtensions.Contains(fileExtension.ToLower());

                    if (fileExtension != null && !isAccepted)
                    {
                        return new ServiceResponseResult { IsSuccess = false, ErrorMessage = $"Supported file types are *.pdf, *.doc*, *.jpg, *.png, *.gif, *.jpeg" };
                    }
                    else
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var index = fileName.LastIndexOf(".");
                        var newFileName = fileName.Insert(0, $"{Guid.NewGuid()}");

                        var physicalPath = serverPath ?? Path.Combine(HttpContext.Current.Server.MapPath("~/documents"), newFileName);
                        // var physicalPath = Path.Combine(serverPath, newFileName);
                        ////// var physicalPath = Path.Combine(serverPath?? "~/documents", newFileName);
                        // var physicalPath = Path.Combine(HttpContext.Current.Server.MapPath("~/documents"), newFileName);

                        //var physicalPath = isDoc ? Path.Combine(HttpContext.Current.Server.MapPath("~/documents"), newFileName)
                        //                    : Path.Combine(HttpContext.Current.Server.MapPath("~/images"), newFileName);
                        file.SaveAs(physicalPath);
                        mediaFiles.Add(new MediaModel { NewFileName = newFileName, OldFileName = fileName });
                    }
                }
                return new ServiceResponseResult { IsSuccess = true, NewObject = mediaFiles};
            }
            else
            {
                return new ServiceResponseResult { IsSuccess = false, ErrorMessage = "You have not specified a file." };
            }
        }
        public static void RemoveMediaFiles(IEnumerable<string> fileNames, string serverPath = null)
        {
            foreach (var file in fileNames)
            {

                var fileName = Path.Combine(serverPath ?? HttpContext.Current.Server.MapPath("~/documents"), file);
                File.Delete(fileName);
            }
        }
        public static void RemoveMediaFile(string fileName)
        {
            var path = Path.Combine(documentPath, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
                return;
            }
            path = Path.Combine(imagetPath, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
                return;
            }
            return;
        }
        public static int GetMediaType(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName);
            return imageExtensions.Contains(fileExtension.ToLower()) ? 1 : 2;
        }

        public static string GetFilePath(string fileName)
        {

            var type = GetMediaType(fileName);
            if (type == 1)
            {
                return Path.Combine(imagetPath, fileName);
            }
            else if (type == 2)
            {
                return Path.Combine(documentPath, fileName);
            }
            else return null;

        }

        public static string GetContentPath(string fileName)
        {
            if(String.IsNullOrWhiteSpace(fileName) || fileName == null)
            {
                return  String.Format("~/images/{0}", "error");
            }
            var filePath = Path.Combine(documentPath, fileName);
            //var encode = new UrlHelper().Encode(fileName);
            var encode = fileName;
            if (File.Exists(filePath))
            {
                return String.Format("~/documents/{0}" , encode);
            }
            filePath = Path.Combine(imagetPath, fileName);
            if (File.Exists(filePath))
            {
                return String.Format("~/images/{0}" , encode);
            }
            var type = GetMediaType(fileName);
            if (type == 1)
            {
                return String.Format("~/images/{0}", encode);
            }
            else if (type == 2)
            {
                return String.Format("~/documents/{0}", encode);
            }
            else return null;
        }
        public static long GetFileSize(string fileName)
        {
            var filePath = GetFilePath(fileName);
            if (filePath == null) return 0;
            if (!File.Exists(filePath)) return 5000;
            return new FileInfo(filePath).Length;
        }
        
        public static ServiceResponseResult SaveMediaFilesWithFilter(HttpFileCollectionBase files, int maxFiles, string filterstring, string serverPath = null)
        {
            if (files.Count > maxFiles)
            {
                return new ServiceResponseResult { IsSuccess = false, ErrorMessage = $"You can't add more than {maxFiles} photos" };
            };
            var mediaFiles = new List<MediaModel>();
            if (files != null && files.Count > 0)
            {
                var numberOfFiles = files.Count;
                for (int i = 0; i < numberOfFiles; i++)
                {
                    if (files.GetKey(i).Contains(filterstring))
                    {

                        var file = files[i];

                        var fileExtension = Path.GetExtension(file.FileName);
                        var isAccepted = acceptedExtensions.Contains(fileExtension.ToLower());

                        if (fileExtension != null && !isAccepted)
                        {
                            return new ServiceResponseResult { IsSuccess = false, ErrorMessage = $"Supported file types are *.pdf, *.doc*, *.jpg, *.png, *.gif, *.jpeg" };
                        }
                        else
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            var index = fileName.LastIndexOf(".");
                            var newFileName = fileName.Insert(0, $"{Guid.NewGuid()}");
                            var physicalPath = GetMediaType(fileName) == 1 ? Path.Combine(imagetPath, newFileName) : Path.Combine(documentPath, newFileName);
                            file.SaveAs(physicalPath);
                            mediaFiles.Add(new MediaModel { NewFileName = newFileName, OldFileName = fileName });
                        }
                    }
                }

                return new ServiceResponseResult { IsSuccess = mediaFiles.Count > 0 ? true : false, NewObject = mediaFiles };
            }
            else
            {
                return new ServiceResponseResult { IsSuccess = false, ErrorMessage = "You have not specified a file." };
            }
        }

        public static Image setImageOrientation(HttpFileCollectionBase files)
        {
            byte[] imageData = new byte[files[0].ContentLength];
            files[0].InputStream.Read(imageData, 0, files[0].ContentLength);

            MemoryStream ms = new MemoryStream(imageData);
            Image originalImage = Image.FromStream(ms);

            if (originalImage.PropertyIdList.Contains(0x0112))
            {
                int rotationValue = originalImage.GetPropertyItem(0x0112).Value[0];
                switch (rotationValue)
                {
                    case 1: // landscape, do nothing
                        break;

                    case 8: // rotated 90 right
                            // de-rotate:
                        originalImage.RotateFlip(rotateFlipType: RotateFlipType.Rotate270FlipNone);
                        break;

                    case 3: // bottoms up
                        originalImage.RotateFlip(rotateFlipType: RotateFlipType.Rotate180FlipNone);
                        break;

                    case 6: // rotated 90 left
                        originalImage.RotateFlip(rotateFlipType: RotateFlipType.Rotate90FlipNone);
                        break;
                }
            }
            return originalImage;
        }

        public static ServiceResponseResult SaveMediaFilesWithFilterAndRotate(HttpFileCollectionBase files, int maxFiles, string filterstring, string serverPath = null)
        {
            if (files.Count > maxFiles)
            {
                return new ServiceResponseResult { IsSuccess = false, ErrorMessage = $"You can't add more than {maxFiles} photos" };
            };
            var mediaFiles = new List<MediaModel>();
            if (files != null && files.Count > 0)
            {
                var numberOfFiles = files.Count;
                for (int i = 0; i < numberOfFiles; i++)
                {
                    if (files.GetKey(i).Contains(filterstring))
                    {

                        var file = files[i];

                        var fileExtension = Path.GetExtension(file.FileName);
                        var isAccepted = acceptedExtensions.Contains(fileExtension.ToLower());

                        if (fileExtension != null && !isAccepted)
                        {
                            return new ServiceResponseResult { IsSuccess = false, ErrorMessage = $"Supported file types are *.pdf, *.doc, *.jpg, *.png, *.gif, *.jpeg" };
                        }
                        else
                        {
                            var fileName = files[i].FileName;
                            var index = fileName.LastIndexOf(".");
                            var newFileName = fileName.Insert(0, $"{Guid.NewGuid()}");
                            var physicalPath = GetMediaType(fileName) == 1 ? Path.Combine(imagetPath, newFileName) : Path.Combine(documentPath, newFileName);

                            Image rotatedImage = setImageOrientation(files);
                            rotatedImage.Save(physicalPath, System.Drawing.Imaging.ImageFormat.Jpeg);

                            //file.SaveAs(physicalPath);
                            mediaFiles.Add(new MediaModel { NewFileName = newFileName, OldFileName = fileName });
                        }
                    }
                }

                return new ServiceResponseResult { IsSuccess = mediaFiles.Count > 0 ? true : false, NewObject = mediaFiles };
            }
            else
            {
                return new ServiceResponseResult { IsSuccess = false, ErrorMessage = "You have not specified a file." };
            }
        }

        public static ServiceResponseResult SaveFiles(HttpFileCollectionBase files, int maxFiles, AllowedFileType fileType)
        {
            if (files.Count > maxFiles)
            {
                return new ServiceResponseResult { IsSuccess = false, ErrorMessage = $"You can't add more than {maxFiles} photos" };
            };
            var mediaFiles = new List<MediaModel>();
            if (files != null && files.Count > 0)
            {
                var numberOfFiles = files.Count;
                for (int i = 0; i < numberOfFiles; i++)
                {
                    var file = files[i];
                    var fileExtension = Path.GetExtension(file.FileName);
                    var isAccepted = allowedExtentions[fileType.ToString()].Contains(fileExtension.ToLower());

                    if (fileExtension != null && !isAccepted)
                    {
                        return new ServiceResponseResult { IsSuccess = false, ErrorMessage = $"Supported file types are *.pdf, *.doc*, *.jpg, *.png, *.gif, *.jpeg" };
                    }
                    else
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var index = fileName.LastIndexOf(".");
                        var newFileName = fileName.Insert(0, $"{Guid.NewGuid()}");
                        var physicalPath = GetMediaType(fileName) == 1 ? Path.Combine(imagetPath, newFileName) : Path.Combine(documentPath, newFileName);
                        file.SaveAs(physicalPath);
                        mediaFiles.Add(new MediaModel { NewFileName = newFileName, OldFileName = fileName });
                    }
                }
                return new ServiceResponseResult { IsSuccess = true, NewObject = mediaFiles };
            }
            else
            {
                return new ServiceResponseResult { IsSuccess = false, ErrorMessage = "You have not specified a file.", NewObject = mediaFiles };
            }
        }

        public static void InjectMediaModelViewProperties(this MediaModel media)
        {
            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            media.Data = urlHelper.Content(GetContentPath(media.NewFileName));
            media.MediaType = GetMediaType(media.NewFileName);
            media.Size = GetFileSize(media.NewFileName);
            media.Status = "load";
        }

        public static void InjectMediaModelViewProperties1(this MediaModel1 media)
        {
            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            media.Data = urlHelper.Content(GetContentPath(media.NewFileName));
            media.MediaType = GetMediaType(media.NewFileName);
            media.Size = GetFileSize(media.NewFileName);
            media.Status = "load";
        }

        public static MediaModel GenerateViewProperties(this MediaModel media)
        {
            InjectMediaModelViewProperties(media);
            return media;
        }
    }
}
