using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Ramp.Contracts.ViewModel
{
    public class TrainingTestQuestionViewModel : IViewModel
    {
        public TrainingTestQuestionViewModel()
        {
            TestAnswerList = new List<TestAnswerViewModel>();
            Errors = new List<FileUploadResultViewModel>();
        }

        public Guid TrainingTestQuestionId { get; set; }

        public virtual TrainingTestViewModel TrainingTest { get; set; }

        public virtual Guid TrainingTestId { get; set; }

        [Required(ErrorMessage = "Please Enter Question")]
        public string TestQuestion { get; set; }

        public int TestQuestionNumber { get; set; }

        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Count must be an integer")]
        public int AnswerWeightage { get; set; }

        [Required(ErrorMessage = "Please Choose Correct Option")]
        public Guid CorrectAnswerId { get; set; }

        public List<TestAnswerViewModel> TestAnswerList { get; set; }

        public IEnumerable<SerializableSelectListItem> AnswerList { get; set; }

        public string ImageName { get; set; }
        public string VideoName { get; set; }
        public string AudioName { get; set; }
        public bool IsDeleted { get; set; }
        public FileUploadResultViewModel ImageContainer { get; set; }
        public FileUploadResultViewModel VideoContainer { get; set; }
        public FileUploadResultViewModel AudioContainer { get; set; }
        public object ImageUpload { get; set; }
        public object ImageInput { get; set; }
        public object VideoUpload { get; set; }
        public object VideoInput { get; set; }
        public object AudioUpload { get; set; }
        public object AudioInput { get; set; }
        public IEnumerable<FileUploadResultViewModel> FailedUploads { get; set; }
        public List<FileUploadResultViewModel> Errors { get; set; }
    }
}