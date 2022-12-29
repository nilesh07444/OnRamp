using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.UI.Code.Helpers
{
    public class GenderHelper
    {
        public static IEnumerable<SelectListItem> ToDropDownList()
        {
            return new SelectListItem[]
            {
                new SelectListItem { Text = GenderEnum.NotSpecified, Value =GenderEnum.Gender.NotSpecified.ToString() },
                new SelectListItem {Text = GenderEnum.Female,Value=GenderEnum.Gender.Female.ToString() },
                new SelectListItem { Text = GenderEnum.Male , Value = GenderEnum.Gender.Male.ToString()}
            }.AsEnumerable();
        }
    }
}