using Common.Data;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class CKEditorUploadResultViewModel
    {
        public uploadResult uploaded { get; set; }
        public string fileName { get; set; }
        public string url { get; set; }
        public error error { get; set; }
        public Guid Id { get; set; }
    }
    public enum uploadResult
    {
        error = 0,
        success = 1
    }
    public class error
    {
        public string message { get; set; }
    }
    public class FileUploadResultViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Size { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public string DeleteUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string DeleteType { get; set; }
        public bool InProcess { get; set; }
        public string Progress { get; set; }
        public int QuestionNUmber { get; set; }
        public int Number { get; set; }
        public bool Embeded { get; set; }
        public Guid Id { get; set; }
        public string PreviewPath { get; set; }
    }
    public class UploadResultViewModel : IdentityModel<string>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Size { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public byte[] Data { get; set; }
        public string DeleteUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string DeleteType { get; set; }
        public bool InProcess { get; set; }
        public string Progress { get; set; }
        public int QuestionNumber { get; set; }
        public int Number { get; set; }
        public bool Embeded { get; set; }
        public string PreviewPath { get; set; }
        public bool isSignOff { get; set; }
        public List<acroField> AcrofieldJson { get; set; }
    }
    public class acroField { 
    public string fields { get; set; }
    }
    public class FileUploadViewModel : IViewModel
    {
        public string Type { get; set; }
        public string ContentType { get; set; }
        public byte[] Data { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid Id { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public FileUploadType? FileUploadType { get; set; }
        public string Url { get; set; }
    }
    public class UploadModel : IdentityModel<string>
    {
        public string Type { get; set; }
        public string ContentType { get; set; }
        public byte[] Data { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public FileUploadType? FileUploadType { get; set; }
        public string Url { get; set; }
    }
    public class UploadTrophyViewModel : IViewModel
    {

        public UploadTrophyViewModel()
        {
            uploadedList = new List<string>();

        }
        public Guid uploadId { get; set; }
        public string upladName { get; set; }
        public string path { get; set; }
        public List<string> uploadedList { get; set; }


    }
    public class UploadFromContentToolsResultModel
    {
        public IEnumerable<string> size { get; set; } = new List<string>();
        public string url { get; set; }
    }
}
