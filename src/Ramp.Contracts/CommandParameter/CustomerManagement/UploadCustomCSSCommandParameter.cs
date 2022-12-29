using Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ramp.Contracts.CommandParameter.CustomerManagement
{
  public class UploadCustomCSSCommandParameter :ICommand
    {
      public Guid CompanyId { get; set; }

      public HttpPostedFileBase CustomCSSFile { get; set; }

      public string path { get; set; }

      public byte[] CSSFile { get; set; }

    }
}
