<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site1.Master" Inherits="System.Web.Mvc.ViewPage<PINProvUtilita.Models.Certificati>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    I.N.P.S. - PIN PROVISIONING - Delegati Entratel

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <script type="text/javascript" src="<%: Url.Content("~//Scripts/jquery-1.8.3.min.js") %>"> </script>
    <script type="text/javascript" src="<%: Url.Content("~//Scripts/jquery-1.9.1.js") %>"> </script>
    <script type="text/javascript" src="<%: Url.Content("~//Scripts/jquery-1.9.1.min.js") %>"> </script>


<div class="myLabel">DELEGATI CERTIFICATI ENTRATEL </div>
<div class="centerFuori">
     <div class="divSanitaria">

<fieldset style="text-align: center;">
    
        <%= Html.ValidationSummary(true) %>
               

             <% 

                 List<PINProvUtilita.Models.Certificati> listaCertificati = new List<PINProvUtilita.Models.Certificati>();

                 listaCertificati = (List<PINProvUtilita.Models.Certificati>)(Session["listaGetDatiGrid"]);


                  %>
                <% Html.RenderPartial("~/Views/Home/CertificatoSelezionato.cshtml", listaCertificati); %>
                <br /><br />
              <% 
              using (Html.BeginForm("SaveEntratel", "Home", FormMethod.Post))
              { %>
                  <div class="myLabel">
                    <table>
                     <tr>
                       <td>
                        <%= Html.LabelFor(model => model.Codicefisc) %>
                        </td>
                        <td>
                                        <%
                        if (ViewData["Codicefisc"] != null)
                        {    
                        %>   
                            <%=Html.TextBoxFor(model => model.Codicefisc, new { @style = "text-transform:uppercase",  @maxlength = "16", @size="25", @Value= (String)Session["Codicefisc"] } )%>
                        <%
                        }
                        else
                        {
                         %>
                            <%=Html.TextBoxFor(model => model.Codicefisc, new { @style = "text-transform:uppercase",  @maxlength = "16", @size="25" } )%>
                    <% } %>   
                        
                        </td>
                        <td>
                        <div style="width: 100px;">
                        <div style="float: left; padding: 0; margin: 0; vertical-align: top;">
 
                          <input  class="BtnRevoca" type="submit" value="Salva" />
                        </div>


                      <div style="clear: both;">
                        </div>
                        </div>
                       </td>
                       </tr>
                     </table>
                    </div>
       <%
                }
                
        %>
           <br /><br /> 
 

 </fieldset>

  </div>
    </div>

    <% Html.RenderPartial("~/Views/Home/AlertMessage.cshtml"); %> 

</asp:Content>

   