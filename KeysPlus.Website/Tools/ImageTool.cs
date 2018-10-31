using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Web;

namespace KeysPlus.Website.Areas.Tools
{
   // public class Base64Image
   // {
   //     public static Base64Image LoadImage(string filePath)
   //     {
   //         if (string.IsNullOrEmpty(filePath))
   //             throw new ArgumentNullException(nameof(filePath));

   //         using (var data = Image.FromFile(filePath))
   //         {
   //             using (var m = new MemoryStream())
   //             {
   //                 data.Save(m, data.RawFormat);
   //                 var imageBytes = m.ToArray();
   //                 return new Base64Image
   //                 {
   //                     FileContents = imageBytes,
   //                     ContentType = filePath.Split('.').Last()
   //                 };
   //             }
   //         }
   //     }

   //     public static Base64Image Parse(string base64Content)
   //     {
   //         if (string.IsNullOrEmpty(base64Content))
   //         {
   //             throw new ArgumentNullException(nameof(base64Content));
   //         }

   //         var indexOfSemiColon = base64Content.IndexOf(";", StringComparison.OrdinalIgnoreCase);
   //         var dataLabel = base64Content.Substring(0, indexOfSemiColon);
   //         var contentType = dataLabel.Split(':').Last();
   //         if (contentType.Contains('/'))
   //             contentType = contentType.Split('/').Last();
   //         var startIndex = base64Content.IndexOf("base64,", StringComparison.OrdinalIgnoreCase) + 7;
   //         var fileContents = base64Content.Substring(startIndex);
   //         var bytes = Convert.FromBase64String(fileContents);

   //         return new Base64Image
   //         {
   //             ContentType = contentType,
   //             FileContents = bytes
   //         };
   //     }

   //     public string ContentType { get; set; }

   //     public byte[] FileContents { get; set; }

   //     private static readonly List<string> AcceptedExtensions = new List<string>
   //         {
   //             "jpg",
   //             "png",
   //             "gif",
   //             "jpeg",
   //             ".jpg",
   //             ".png",
   //             ".gif",
   //             ".jpeg"
   //         };

   //     public static bool IsAcceptable(string extension)
   //     {
   //         return AcceptedExtensions.Contains(extension.ToLower());
   //     }

   //     public void Save(string filePath)
   //     {
			//using (var ms = new MemoryStream(FileContents))
			//{
			//	var image = Image.FromStream(ms);
			//	image.Save(filePath);
			//}
   //     }

   //     public override string ToString()
   //     {
   //         return $"data:image/{ContentType};base64,{Convert.ToBase64String(FileContents)}";
   //     }
   // }
}
