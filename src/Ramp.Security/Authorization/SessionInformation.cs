using Domain.Enums;
using Ramp.Contracts;
using Ramp.Contracts.Security;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramp.Security.Authorization
{
    public class SessionInformation
    {
        private const string ShowSurveyModal = "ShowSurveyModal", 
                             IS_TEST_EDITED = "IS_TEST_EDITED",
                             TestFeedbackModal = "ShowTestFeedbackModal",
                             DisclaimerPoppedUp = "DisclaimerPoppedUp";

        public string TestReferenceId {
            get
            {
                if (GetSessionVariable("TrainingTestReferenceId") != null)
                    return GetSessionVariable("TrainingTestReferenceId").ToString();
                return string.Empty;
            }
            set { SetSessionVariable("TrainingTestReferenceId", value); }
        } 
        public bool ShowSurveyModel
        {
            get
            {
                if (GetSessionVariable(ShowSurveyModal) != null)
                    return (bool)GetSessionVariable(ShowSurveyModal);
                return false;
            }
            set { SetSessionVariable(ShowSurveyModal, value); }
        }
        public bool DisclaimerAcknowledged
        {
            get
            {
                if (GetSessionVariable(DisclaimerPoppedUp) != null)
                    return Convert.ToBoolean(GetSessionVariable(DisclaimerPoppedUp));
                return false;
            }
            set { SetSessionVariable(DisclaimerPoppedUp, value); }
        }

		public bool ShowInProgressPopup { get; set; } = false;
		public bool UserHasAcceptedDisclaimer { get; set; }
		public string StandardUserId { get; set; }
		public bool ShowTestFeedbackModal
        {
            get
            {
                if (GetSessionVariable(TestFeedbackModal) != null)
                    return (bool)GetSessionVariable(TestFeedbackModal);
                return false;
            }
            set { SetSessionVariable(TestFeedbackModal, value); }
        }

        public static bool IsTestEdited
        {
            get
            {
                if (GetSessionVariable(IS_TEST_EDITED) != null)
                    return (bool)GetSessionVariable(IS_TEST_EDITED);
                return false;
            }
            set
            {
                SetSessionVariable(IS_TEST_EDITED, value);
            }
        }

        private static void SetSessionVariable(string identifier, object value)
        {
            if (System.Web.HttpContext.Current != null)
            {
                System.Web.HttpContext.Current.Session[identifier] = value;
            }
        }

        private static object GetSessionVariable(string identifier)
        {
            if (System.Web.HttpContext.Current != null)
            {
                return System.Web.HttpContext.Current.Session[identifier];
            }
            return null;
        }
    }
}