using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace Ramp.Services.Helpers
{
    public static class CommonHelper
    {
        public static readonly IDictionary<string, TrainingDocumentTypeEnum> _ExtentionTypeMappings = new Dictionary<string, TrainingDocumentTypeEnum>
        {
            { "jpg" , TrainingDocumentTypeEnum.Image        },
            { "jpeg", TrainingDocumentTypeEnum.Image        },
            { "png", TrainingDocumentTypeEnum.Image         },
            { "gif" , TrainingDocumentTypeEnum.Image        },
            { "bmp",  TrainingDocumentTypeEnum.Image        },
            { "pdf", TrainingDocumentTypeEnum.Pdf           },
            { "xls", TrainingDocumentTypeEnum.Excel         },
            { "xlsx", TrainingDocumentTypeEnum.Excel        },
            { "pps", TrainingDocumentTypeEnum.PowerPoint    },
            { "ppsx", TrainingDocumentTypeEnum.PowerPoint   },
            { "ppt", TrainingDocumentTypeEnum.PowerPoint    },
            { "pptx", TrainingDocumentTypeEnum.PowerPoint   },
            { "doc", TrainingDocumentTypeEnum.WordDocument  },
            { "docx", TrainingDocumentTypeEnum.WordDocument },
            { "mp4", TrainingDocumentTypeEnum.Video         },
            { "avi", TrainingDocumentTypeEnum.Video         },
            { "wmv", TrainingDocumentTypeEnum.Video         },
            { "mov", TrainingDocumentTypeEnum.Video         },
            { "mpeg", TrainingDocumentTypeEnum.Video        },
            { "3gp", TrainingDocumentTypeEnum.Video         },
            { "mp3", TrainingDocumentTypeEnum.Audio         }
        };
        public static TrainingDocumentTypeEnum GetDocumentType(string documentName)
        {
            var extention = Path.GetExtension(documentName ?? ".error").ToLower().Substring(1);
            if (_ExtentionTypeMappings.ContainsKey(extention))
            {
                return _ExtentionTypeMappings[extention];
            }
            return TrainingDocumentTypeEnum.Image;
        }

        public static string GetCompanyConectionStringById(Guid companyId)
        {
            return ServiceLocator.Current.GetInstance<ICustomerRepositoryFactory>().GetConnectionString(companyId);
        }

        public static string GetYouTubeImage(string videoUrl)
        {
            int mInd = videoUrl.IndexOf("?v", System.StringComparison.Ordinal);
            if (mInd != -1)
            {
                string strVideoCode = videoUrl.Substring(videoUrl.IndexOf("?v", System.StringComparison.Ordinal) + 3);
                int ind = strVideoCode.IndexOf("?", StringComparison.Ordinal);
                strVideoCode = strVideoCode.Substring(0, ind == -1 ? strVideoCode.Length : ind);
                return "https://img.youtube.com/vi/" + strVideoCode + "/default.jpg";
            }
            return "";
        }

        /// <summary>
        /// Method to resize, convert and save the image.
        /// </summary>
        /// <param name="image">Bitmap image.</param>
        /// <param name="maxWidth">resize width.</param>
        /// <param name="maxHeight">resize height.</param>
        /// <param name="quality">quality setting value.</param>
        /// <param name="filePath">file path.</param>
        public static void SaveImage(Stream data, int? maxWidth, int? maxHeight, string filePath)
        {
            using (Image image = Image.FromStream(data))
            {
                // Get the image's original width and height
                int originalWidth = image.Width;
                int originalHeight = image.Height;

                if (!maxWidth.HasValue)
                    maxWidth = originalWidth;

                if (!maxHeight.HasValue)
                    maxHeight = originalHeight;

                // To preserve the aspect ratio
                float ratioX = (float)maxWidth / (float)originalWidth;
                float ratioY = (float)maxHeight / (float)originalHeight;
                float ratio = Math.Min(ratioX, ratioY);

                // New width and height based on aspect ratio
                int newWidth = (int)(originalWidth * ratio);
                int newHeight = (int)(originalHeight * ratio);

                using (var newImg = image.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero))
                {
                    newImg.Save(filePath);
                }
            }
        }

        /// <summary>
        /// Method to get encoder infor for given image format.
        /// </summary>
        /// <param name="format">Image format</param>
        /// <returns>image codec info.</returns>
        private static ImageCodecInfo GetEncoderInfo(ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().SingleOrDefault(c => c.FormatID == format.Guid);
        }
    }
}