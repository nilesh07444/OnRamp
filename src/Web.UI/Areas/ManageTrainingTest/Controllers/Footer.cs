using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Web.UI.Areas.ManageTrainingTest.Controllers
{
    public partial class Footer : PdfPageEventHelper, IPdfPageEvent
    {
        public override void OnEndPage(PdfWriter writer, Document doc)
        {
            var content = writer.DirectContent;
            var pageBorder = new Rectangle(doc.PageSize);

            pageBorder.Left += doc.LeftMargin;
            pageBorder.Right -= doc.RightMargin;
            pageBorder.Top -= doc.TopMargin;
            pageBorder.Bottom += doc.BottomMargin;

            //content.SetColorStroke(BaseColor.BLACK);
            content.SetColorStroke(BaseColor.GRAY);
            //content.SetColorFill(229.00, 229.00, 229.00);
            //content.SetColorStroke(229.00, 229.00, 229.00);
            content.Rectangle(pageBorder.Left, pageBorder.Bottom, pageBorder.Width, pageBorder.Height);
            content.Stroke();
        }
    }
}
