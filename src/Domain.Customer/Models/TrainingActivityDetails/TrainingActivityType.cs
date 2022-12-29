using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtuaCon;
namespace Domain.Customer.Models
{
    public enum TrainingActivityType
    {
        [EnumFriendlyName("Toolbox Talk")]
        ToolboxTalk,
        [EnumFriendlyName("Internal")]
        Internal,
        [EnumFriendlyName("Mentoring And Coaching")]
        MentoringAndCoaching,
        [EnumFriendlyName("Bursary")]
        Bursary,
        [EnumFriendlyName("External")]
        External
    }
}
