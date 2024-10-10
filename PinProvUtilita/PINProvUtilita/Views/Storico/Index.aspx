<%@ Page Title="" Language="C#" 
    MasterPageFile="~/Views/Shared/Site1.Master" 
    Inherits="System.Web.Mvc.ViewPage<PINProvUtilita.Models.Storico>" 

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
    
<style>
.relative {
  position: relative;
  left: 180px;
  color:black;
  margin-left:15px;
  width: 200px;
  margin-right:15px;
  margin-bottom:15px;
  font-family:Verdana;

}
.labelrelative {
  position: relative;
  left: 220px;
  margin-left:15px;
  width: 200px;
  margin-right:15px;
  margin-bottom:15px;
  font-family:Verdana;

}
.ricerca {
  position: relative;
  left: 215px;
  margin-left:15px;
  height:40px;
  margin-right:5px;
  bottom:36px;
  font-family:Verdana;

}

.coderrore {
  position: relative;
  right: 25px;
  margin-left:15px;
  height:40px;
  margin-right:5px;
  bottom:36px;
  font-family:Verdana;

}
.annulla {
  position: relative;
  left: 185px;
  margin-left:15px;
  margin-right:15px;
  bottom:35px;
  font-family:Verdana;

}
.labelerror {
  position: relative;
  left: -190px;
  margin-left:15px;
  width: 200px;
  margin-right:15px;
  margin-bottom:15px;
  font-family:Verdana;
  color: #FF0000;
</style>

    <link href="../../Content/Site.css" rel="stylesheet" />
    <div class="myLabel">
        Storico Contatti 
    </div>
    <div class="centerFuori">
        <div class="divSanitaria">
                <% using (Html.BeginForm())
                {              
                %>
    
                <%= Html.ValidationSummary(true) %>
            <div class="center">
                <div class="divSpacer"></div>
                <fieldset class="field_set_decifraCodiceFiscale">
                    <div class="myLabel">  
                        <table style="height: 80px; width: 1200px">
                            <tr>
                                
                                   
                               <%= @Html.ValidationSummary(true, "", new { @class = "text-danger" })%>
                                <td class="labelrelative"> 
                                    <%= Html.LabelFor(model => model.CodiceFiscale) %>
                                </td>
                                
                                <td class="editor-field"> 
                                    <%=Html.TextBoxFor(model => model.CodiceFiscale, new { @class = "relative",@maxlength = "40", required="required" })%>
                                    <%=Html.ValidationMessageFor(model => model.CodiceFiscale, null, new { @class = "field-validation-error" })%> 
                                   
                                </td> 
                                <td class="labelerror"> 
                                    <%=Html.Label(ViewData["CREATED_ERR"].ToString(), new { @class = "labelerror", @disabled = "disabled" })%>
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
                                    <%= Html.Label(decifra, new { @class = "relative" })%>
                                </td>
                            </tr>

                            <tr class="disttr">
                                <td></td>
                                <td>
                                    <div class="annulla" style="margin-top:5px;" >
                                        
                                    </div>
                                    
                                    <div class="ricerca" style="margin-top:5px" >
                                        <div  style ="float: left; padding: 0; margin: 0; vertical-align: top;">
                                                <input  class="BtnGrafica" type="submit" value="ricerca" />
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
