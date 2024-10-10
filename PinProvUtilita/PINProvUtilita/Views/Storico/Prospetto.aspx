<%@ Page Title="" Language="C#" 
    MasterPageFile="~/Views/Shared/Site1.Master" 
    Inherits="System.Web.Mvc.ViewPage<PinProvEntity.UtenteContatti>" 

    %>
 
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    I.N.P.S. - PIN PROVISIONING - Decifra Codice Fiscale
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <script type="text/javascript" src="<%: Url.Content("~//Scripts/jquery-1.8.3.min.js") %>"> </script>
    <script type="text/javascript" src="<%: Url.Content("~//Scripts/jquery-1.9.1.js") %>"> </script>
    <script type="text/javascript" src="<%: Url.Content("~//Scripts/jquery-1.9.1.min.js") %>"> </script>

    <script type="text/javascript">
        function Annulla() {
            $("#CodiceFiscale").val("");
            $("#CodiceFiscale").attr("disabled", false);
            $("#lbl1:first-child").css("visibility", "hidden");
            $("#lbl2").text("");
            $("#lblerrore").text("");
        }
        
    </script>
    <div class="myLabel">
       
    </div>
    <div class="">
        <div class="divStorico">
                <% using (Html.BeginForm())
                {              
                %>
    
                <%= Html.ValidationSummary(true) %>
            <div class="centra1">
                <div class="divSpacer"></div>
                <% Html.RenderPartial("~/Views/Storico/_PartialUtente.cshtml",ViewData["gestioneStorico"]); %>
                <fieldset class="Field_set_decifraCodiceFiscale">
                    <div class="myLabel">  
                       <% Html.RenderPartial("~/Views/Storico/_PartialView.cshtml",ViewData["gestioneStorico"]); %>
                                             
                                       
                          

                         
                       
                    </div>
                </fieldset>
            </div>
                <%} %>
        </div>
    </div>
     <% Html.RenderPartial("~/Views/Home/AlertMessage.cshtml"); %>  
</asp:Content>
