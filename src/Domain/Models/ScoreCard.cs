
namespace Domain.Models
{
    public class ScoreCard : DomainObject
    {
        public int TrainingTestId { get; set; }
        //public int UserId { get; set; }       
        public virtual User User { get; set; }
        public int NoOfRightAns { get; set; }
        public int NoOfWrongeAns { get; set; }
        public int TimeTaken { get; set; }
        public decimal Percentile { get; set; }
        public string Result { get; set; }
        
    }
}
