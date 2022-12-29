using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtuaCon;

namespace Domain.Customer
{
    public enum DocumentPreviewMode
    {
        [EnumFriendlyName("Portrait")]
        Portrait,
        [EnumFriendlyName("Storybook")]
        Landscape
    }
    public enum DocumentStatus
    {
        [EnumFriendlyName("Draft")]
        Draft,
        [EnumFriendlyName("Published")]
        Published,
        [EnumFriendlyName("Recalled")]
        Recalled

    }
    public enum DocumentType
    {
        Unknown,
        [EnumFriendlyName("Training Manual")]
        TrainingManual,
        Test,
        Policy,
        Memo,
        Certificate,
        [EnumFriendlyName("Activity Book")]
        Checklist,
        [EnumFriendlyName("Custom Document")]
        custom,
        [EnumFriendlyName("Virtual Class Room")]
        VirtualClassRoom,
        AcrobatField,
        Form

    }
    public enum DocumentTypeWithoutVirtual
    {
        Unknown,
        [EnumFriendlyName("Training Manual")]
        TrainingManual,
        Test,
        Policy,
        Memo,
        Certificate,
        [EnumFriendlyName("Activity Book")]
        Checklist,
        Custom
    }

    public enum DocumentWithoutType
    {
        Unknown,
        [EnumFriendlyName("Training Manual")]
        TrainingManual,
        Test,
        Policy,
        Memo,
        Certificate,
        Custom

    }

    public enum TestExpiryNotificationInterval
    {
        [EnumFriendlyName("None")]
        None,
        [EnumFriendlyName("One Day Before")]
        OneDayBefore,
        [EnumFriendlyName("Daily")]
        Daily,
        [EnumFriendlyName("Custom")]
        Custom
    }
    public enum UserFeedbackType
    {
        Other,
        Document,
        System
    }
    public enum UserFeedbackContentType
    {
        Other,
        Question,
        Complaint,
        Praise,
        Suggestion
    }
    public enum ChecklistSubmissionStatus
    {
        Incomplete,
        Complete
    }
    public enum GlobalAccess
    {
        Assigned,
        Global
    }
    public enum UserFeedbackCategories
    {
        [EnumFriendlyName("I need support")] Support,
        [EnumFriendlyName("I want to report a bug")] Bug,
        [EnumFriendlyName("I have a feature request")] Feature_Demand,
        [EnumFriendlyName("I have a system improvement idea")] Improvement
    }

    public enum DocumentUsageStatus
    {
        //Old Enums
        //Pending = 1,
        //Viewed = 2,
        //Incomplete = 3,
        //Completed = 4,
        //Submitted = 5,
        //Approved = 6,
        //declined = 7,

        //New Enums Added By Softude
        Pending = 1,
        InProgress = 2,
        UnderReview = 3,
        ActionRequired = 4,
        Complete = 5,
        Passed = 6,
    }

    public enum FieldType
    {
        //Old Enums
        //Pending = 1,
        //Viewed = 2,
        //Incomplete = 3,
        //Completed = 4,
        //Submitted = 5,
        //Approved = 6,
        //declined = 7,

        //New Enums Added By Softude
        [EnumFriendlyName("Select Field Type")]
        SelectFieldType = 0,
        Pending = 1,
        InProgress = 2,
        UnderReview = 3,
        ActionRequired = 4,
        Complete = 5,
        Passed = 6,
    }

}
