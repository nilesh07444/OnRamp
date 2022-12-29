using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Web.UI.Code.Helpers
{
    public class ValidateFileAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            int MaxContentLength = 1024*1024*3; //3 MB
            //string[] AllowedFileExtensions = new string[] { ".jpg", ".gif", ".png", ".pdf" };
            var AllowedFileExtensions = new string[] {".csv"};
            var file = value as HttpPostedFileBase;
            if (file == null)
                return false;
            else if (!AllowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.'))))
            {
                ErrorMessage = "Please upload Your file of type: " + string.Join(", ", AllowedFileExtensions);
                return false;
            }
            else if (file.ContentLength > MaxContentLength)
            {
                ErrorMessage = "Your file is too large, maximum allowed size is : " + (MaxContentLength/1024) +
                               "MB";
                return false;
            }
            else
                return true;
        }
    }
}