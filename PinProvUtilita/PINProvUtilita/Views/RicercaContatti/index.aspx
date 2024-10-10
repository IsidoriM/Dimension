<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site1.Master" Inherits="System.Web.Mvc.ViewPage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">


</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <script type="text/javascript" src="<%: Url.Content("~//Scripts/jquery-1.8.3.min.js") %>"> </script>
    <script type="text/javascript" src="<%: Url.Content("~//Scripts/jquery-1.9.1.js") %>"> </script>
    <script type="text/javascript" src="<%: Url.Content("~//Scripts/jquery-1.9.1.min.js") %>"> </script>
    <script type="text/javascript" src="<%: Url.Content("~//Scripts/bootstrap.js") %>"> </script>
    <script type="text/javascript" src="<%: Url.Content("~//Scripts/bootstrap.min.js") %>"> </script>

    <link href="../../Content/Bootstrap/bootstrap.css" rel="stylesheet" />
    <link href="../../Content/Bootstrap/bootstrap.min.css" rel="stylesheet" />
    <link href="../../Content/Bootstrap/bootstrap-theme.css" rel="stylesheet" />
    <link href="../../Content/Bootstrap/bootstrap-theme.min.css" rel="stylesheet" />
    <link href="../../css/Contatti.css" rel="stylesheet" />


<script type="text/javascript">
    function AttivaMail() {
        $('#pec').val("");
        $('#cellulare').val("");
        $('#email').attr("disabled", false);
        $('#pec').attr("disabled", true);
        $('#cellulare').attr("disabled", true);
    }

    function AttivaPec() {
        $('#email').val("");
        $('#cellulare').val("");
        $('#email').attr("disabled", true);
        $('#pec').attr("disabled", false);
        $('#cellulare').attr("disabled", true);
    }

    function AttivaCellulare() {
        $('#email').val("");
        $('#pec').val("");
        $('#email').attr("disabled", true);
        $('#pec').attr("disabled", true);
        $('#cellulare').attr("disabled", false);
    }

    function soloNumeri(evt) {
        var charCode = (evt.which) ? evt.which : event.keyCode
        if (charCode > 31 && (charCode < 48 || charCode > 57)) {
            return false;
        }
        return true;
    }

</script>
    <div class="centerFuori">
        <div class="divSanitaria">   
            <% Html.RenderPartial("~/Views/RicercaContatti/Tabs.cshtml"); %>  
        </div>
    </div>
    <% Html.RenderPartial("~/Views/Home/AlertMessage.cshtml"); %>  
   
</asp:Content>
