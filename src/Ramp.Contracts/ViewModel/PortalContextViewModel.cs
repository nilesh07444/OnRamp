using Domain.Enums;
using Domain.Models;
using System.Collections.Generic;

namespace Ramp.Contracts.ViewModel
{
    public class PortalContextViewModel
    {
        public string LogoFileName { get; set; }
        public string Name { get; set; }
        public string Reseller { get; set; }
        public CompanyType? Type { get; set; }
        public CompanyViewModel UserCompany { get; set; }
        public UserViewModel UserDetail { get; set; }
		public VirtualClassModel VirtualClassModel { get; set; }
		public string EmbedCss()
        {
            if (this.UserCompany != null && this.UserCompany.CustomColours != null)
            {
                var cc = this.UserCompany.CustomColours;

                string styles = "";
                
                styles += ".login-col a {color : " + cc.ButtonColour + "; }";
                styles += ".fc-event-title-container {background-color : " + cc.ButtonColour + ";}";
                styles += ".fc-button-primary {background-color : " + cc.ButtonColour + " !important; border-color : " + cc.ButtonColour + " !important;}";
                styles += ".fc-h-event {border : " + cc.ButtonColour + " !important;}";
                styles += ".navbar-bottom-wording-custom {color:" + cc.FooterColour + "; }";
                styles += ".btn-primary, .btn-primary:hover, .btn-primary:focus, .btn-primary:active, .btn-primary:visited, .btn-primary.active {background-color: " + cc.ButtonColour + "!important ;border-color: " + cc.ButtonColour + " !important; }";
                styles += ".navbar-default { border-color: " + cc.NavigationColour + "; }";
                styles += ".navbar-top-custom { background-color: " + cc.HeaderColour + ";border-color: " + cc.HeaderColour + "; }";
                styles += ".navbar-default .navbar-collapse,.navbar-default .navbar-form {border-color: " + cc.NavigationColour + ";} .navbar-default .navbar-nav > .open > a,.navbar-default .navbar-nav > .open > a:hover,.navbar-default .navbar-nav > .open > a:focus {background-color: " + cc.NavigationColour + ";border-color: " + cc.NavigationColour + ";}";
                styles += ".navbar-default .navbar-nav > .active > a,.navbar-default .navbar-nav > .active > a:hover,.navbar-default .navbar-nav > .active > a:focus,.navbar-default .navbar-nav > .active.open > a:hover {background-color: " + cc.ButtonColour + " !important; color: #fff;}";
                styles += ".navbar-bottom-custom { background-color: " + cc.FooterColour + ";border-color: " + cc.FooterColour + "; }";
        
                styles += ".modalSearch {background-color: " + cc.SearchColour + ";}";

                styles += ".modal-header-custom {background-color: " + cc.FeedbackColour + ";}";
                styles += ".modal-footer>.btn-primary ,.modal-footer>.btn-primary:hover,.modal-footer>.btn-primary:focus,.modal-footer>.btn-primary:active { background-color : " + cc.FeedbackColour + ";}";

                styles += ".modal-login {background-color: " + cc.LoginColour + ";}";
                styles += ".category-icon {color: " + cc.ButtonColour + ";}";
                styles += ".play-circle {color: " + cc.ButtonColour + ";}";
                styles += ".pagination > .active > a, .pagination > .active > a:focus {background-color: " + cc.ButtonColour + ";border-color: " + cc.ButtonColour +";}";
                styles += "input:checked + .slider  { background-color: " + cc.ButtonColour + ";}";
                styles += "input:focus + .slider  { box-shadow: 0 0 1px " + cc.ButtonColour + ";}";
                styles += "input:checked + .sliders  { background-color: " + cc.ButtonColour + ";}";
                styles += "input:focus + .sliders  { box-shadow: 0 0 1px " + cc.ButtonColour + ";}";

                return styles;
            }
            return "";
        }
        public IDictionary<IconType, string> Icons { get; set; } = new Dictionary<IconType, string>();
        public bool? UserHasAcceptedDisclaimer { get; set; }
		public bool ShowInProgressPopup { get; set; } = false;
		public bool ShouldShowDisclaimer(bool haveAcceptedDisclaimer,bool hasAcknoweledgedDisclaimer)
        {
            if (UserCompany != null)
            {
                switch (UserCompany.LegalDisclaimerActivationType)
                {
                    case LegalDisclaimerActivationType.Disabled:
                        return false;
                    case LegalDisclaimerActivationType.ShowOnLogin:
                        return true && !hasAcknoweledgedDisclaimer;
                    case LegalDisclaimerActivationType.ShowOnLoginOnce:
                        return !haveAcceptedDisclaimer && !hasAcknoweledgedDisclaimer;
                    default:
                        return false;
                }
            }
            return false;
        }
    }
}