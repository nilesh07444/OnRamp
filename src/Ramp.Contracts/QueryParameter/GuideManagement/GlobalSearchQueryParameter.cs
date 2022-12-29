using Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.QueryParameter.GuideManagement
{
   public class GlobalSearchQueryParameter : IQuery
    {

        public string SearchText { get; set; }

        public SearchTypes SearchType { get; set; }

        public string SearchUrl { get; set; }

        public bool IsCustAdmin { get; set; }

        public Guid UserId { get; set; }

        public Guid LogedInCompanyId { get; set; }
    }
    public enum SearchTypes
    {
        All,
        Tests,
        Playbooks,
    }
    public enum SearchDepth
    {
        AddDirectories,
        TopDirectories,
    }
    
}
