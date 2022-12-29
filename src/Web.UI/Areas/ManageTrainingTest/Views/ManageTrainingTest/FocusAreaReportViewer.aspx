<%@ Page Language="C#" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=12.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<script runat="server">
    Ramp.Contracts.ViewModel.FocusAreaReportDataSources tModel;
    protected void Page_Load(object sender, EventArgs e)
    {
        ReportViewer1.LocalReport.ReportPath = Server.MapPath(Url.Content("~/Areas/ManageTrainingTest/Views/ManageTrainingTest/FocusAreaReport.rdlc"));
        tModel = Model as Ramp.Contracts.ViewModel.FocusAreaReportDataSources;
        var questionDS1 = new ReportDataSource("QuestionDS", tModel.Questions);
        ReportViewer1.LocalReport.DataSources.Add(questionDS1);
        ReportViewer1.LocalReport.ShowDetailedSubreportMessages = true;
        ReportViewer1.LocalReport.SubreportProcessing += LocalReport_SubreportProcessing;

    }

    private void LocalReport_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
    {

        if (e.ReportPath == "FocusAreaOptionsReport")
        {
            var question = tModel.Questions.First(f => f.Id == e.Parameters["QuestionId"].Values[0]);

            var reportDs = new ReportDataSource("OptionDS", question.Options);
            e.DataSources.Add(reportDs);
        }

    }
</script>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        .viewerControl {}
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" AsyncRendering="False" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="100%" Height="800px" EnableTheming="False" InteractivityPostBackMode="AlwaysSynchronous" PageCountMode="Actual" ShowBackButton="False" ShowFindControls="False" ShowPageNavigationControls="False" ShowPrintButton="False" ShowRefreshButton="False" ShowZoomControl="False" BackColor="#555555" CssClass="viewerControl" InternalBorderColor="White" LinkActiveColor="White" LinkActiveHoverColor="White" SplitterBackColor="White" ToolBarItemBorderColor="White" ToolBarItemHoverBackColor="Transparent" ZoomMode="PageWidth">
            <LocalReport ReportPath="Areas\ManageTrainingTest\Views\ManageTrainingTest\FocusAreaReport.rdlc">
            </LocalReport>
        </rsweb:ReportViewer>
        <asp:ScriptManager ID="scriptManager" runat="server"></asp:ScriptManager>
    </div>
    </form>
</body>
</html>
