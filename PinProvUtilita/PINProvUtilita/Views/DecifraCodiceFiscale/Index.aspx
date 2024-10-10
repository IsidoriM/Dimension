<%@ Page Title="" Language="C#" 
    MasterPageFile="~/Views/Shared/Site1.Master" 
    Inherits="System.Web.Mvc.ViewPage<PINProvUtilita.Models.DecifraCodiceFiscale>" 

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
        DECIFRA CODICE FISCALE 
    </div>
    <div class="centerFuori">
        <div class="divSanitaria">
                <% using (Html.BeginForm())
                {              
                %>
    
                <%= Html.ValidationSummary(true) %>
            <div class="centra1">
                <div class="divSpacer"></div>
                <fieldset class="field_set_decifraCodiceFiscale">
                    <div class="myLabel">  
                        <table style="height: 80px;">
                            <tr class="disttr">
                                <td> 
                                    <%= Html.LabelFor(model => model.CodiceFiscale) %>
                                </td>
                                <td > 
                                    <%=Html.TextBoxFor(model => model.CodiceFiscale, new { @class = "wideContatti",@maxlength = "20" })%>
                                </td> 
                        
                            </tr>
                            <tr class="disttr">
                                <% string decifra="";
                                   string decifralabel = "";
                                   //string errore = "";
                                   string session = "";

                                   if (Session["Codicefisc"] != null)
                                        session = Session["Codicefisc"].ToString();
                                   if (!string.IsNullOrEmpty(session))
                                   {
                                           decifra = session;
                                           decifralabel = "Codice Fiscale decifrato: ";

                                   }
                                   Session["Codicefisc"] = null;
                                   session = "";

                                %>
                                <td id="lbl1"> 
                                    <%= Html.Label(decifralabel) %>
                                
                                </td>
                                <td id="lbl2"> 
                                    <%= Html.Label(decifra, new { @class = "wideDecifracodicefiscale" })%>
                                </td>
                            </tr>

                            <tr class="disttr">
                                <td></td>
                                <td>
                                    <div style="margin-top:5px;" >
                                        <div style="float: left; padding: 0; margin-right: 3%; vertical-align: top;">
                                            <input id="btnAnnulla" class ="BtnRevoca" type="button" value="Annulla" onclick="Annulla()"/>
                                        </div>
                                    </div>
                                    
                                    <div style="margin-top:5px;" >
                                        <div style="float: left; padding: 0; margin: 0; vertical-align: top;">
                                                <input  class="BtnGrafica" type="submit" value="Decifra" />
                                        </div>
                                        <div style="float:left; padding: 0; margin: 0; vertical-align: top;">
                                            <img src="<%: Url.Content("~/Images/arrow-right.gif") %>" />
                                        </div>
                                
                                        <div style="clear: both;">
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </fieldset>
            </div>
                <%} %>
        </div>
    </div>
     <% Html.RenderPartial("~/Views/Home/AlertMessage.cshtml"); %>  
</asp:Content>
