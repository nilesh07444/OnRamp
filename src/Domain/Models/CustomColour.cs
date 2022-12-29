
namespace Domain.Models
{
   public class CustomColour:DomainObject
    {
        public string ButtonColour { get; set; }
        public string FeedbackColour { get; set; }
        public string FooterColour { get; set; }
        public string HeaderColour { get; set; }
        public string LoginColour { get; set; }
        public string NavigationColour { get; set; }
        public string SearchColour { get; set; }
        public virtual Company Company { get; set; }

	}
}
