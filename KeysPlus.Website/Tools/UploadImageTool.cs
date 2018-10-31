using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace KeysPlus.Website.Areas.Tools
{
    public class UploadImageTool
    {
        //check every file's format
        //if format is not supporive, return error message
        public Boolean CheckImageFormat(HttpFileCollectionBase photos)
        {
            List<string> acceptedExtensions = new List<string>
                {
                    ".jpg",
                    ".png",
                    ".gif",
                    ".jpeg"
                };

            foreach (string fileIndex in photos)
            {
                var file = photos[fileIndex];
                var fileExtension = Path.GetExtension(file.FileName);
                if (fileExtension != null && !acceptedExtensions.Contains(fileExtension.ToLower()))
                {
                    //message = "Supported file types are *.jpg, *.png, *.gif, *.jpeg";
                    return false;
                }
            }
            return true;
        }

        //upload image
        //file is one single photo that user uploaded
        public string UploadImage(HttpPostedFileBase file)
        {
            var fileName = Path.GetFileName(file.FileName);
            var index = fileName.LastIndexOf(".");
            var newFileName = fileName.Insert(index, $"{Guid.NewGuid()}");
            var physicalPath = Path.Combine(HttpContext.Current.Server.MapPath("~/images"), newFileName);
            //save file physically on disk
            file.SaveAs(physicalPath);
            //return filename, and save it into database
            return newFileName;
        }
    }
}
