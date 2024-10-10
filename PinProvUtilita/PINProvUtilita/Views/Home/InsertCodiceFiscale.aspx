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
                 List<PINProvUtilita.Models.Certificati> listaGetDatiGridCF = new List<PINProvUtilita.Models.Certificati>();

              

                 listaGetDatiGridCF = (List<PINProvUtilita.Models.Certificati>)(Session["listaGetDatiGrid"]);

                 if(listaGetDatiGridCF != null)
                 {  
                     if(listaGetDatiGridCF.Count>0)
                     {    
                      %>

                            <% Html.RenderPartial("~/Views/Home/CertificatiCF.cshtml", listaGetDatiGridCF); %>

                        <br /><br /><br />

                      <%
                      } 
                      else
                      {
                      %>
                          <p class="myLabel">
                               <%= Html.Label("Non sono presenti certificati Entratel per il codice fiscale : " + ((string)Session["CodiceFiscale"]).ToUpper())%>
                          </p>
                     <%
                      }                                        
                 }
                 else
                 {
                 %>
                      <p class="myLabel">
                           <%= Html.Label("Non sono presenti certificati Entratel per il codice fiscale : " + ((string)Session["CodiceFiscale"]).ToUpper())%>
                      </p>
               <%
                }
                %>
               <% using (Html.BeginForm("InsertCodiceFiscale", "Home", FormMethod.Post))
               { %>
                
                   <div class="myLabel">
                    <table>
                     <tr>
                       <td>
                             <%= Html.Label("Partita Iva") %>
                        </td>
                        <td>
                            <%
                            if (ViewData["partitaIva"] != null)
                            {    
                            %>   
                                <%=Html.TextBoxFor(model => model.CodiceFiscale, new { @style = "text-transform:uppercase",  @maxlength = "11",  @Value= (String)Session["partitaIva"] } )%>
                            <%
                            }
                            else
                            {
                             %>
                                  <%=Html.TextBoxFor(model => model.CodiceFiscale, new { @maxlength = "11" }  )%>
                        <% } %>   


                        </td>
                        <td>
                        <div style="width: 100px;">
                        <div style="float: left; padding: 0; margin: 0; vertical-align: top;">
 
                        <input class ="BtnGrafica" type="submit" value="Cerca" />
                        </div>
                        <div style="float: left; padding: 0; margin: 0; vertical-align: top;">
         
                       
                       <img src="<%: Url.Content("~/Images/arrow-right.gif") %>" />
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
  
        <%    
        if (ViewData["RicercaCodici"] != null)
        {%>

                         <%
            List<PINProvUtilita.Models.Certificati> listaGetCertificatiPIVA = new List<PINProvUtilita.Models.Certificati>();

            listaGetCertificatiPIVA = (List<PINProvUtilita.Models.Certificati>)(ViewData["listaGetCertificatiPIVA"]);
            
            if (listaGetCertificatiPIVA.Count > 0)
            {    
                %>
                    


                            <% Html.RenderPartial("~/Views/Home/CertificatiPIVAXCF.cshtml", listaGetCertificatiPIVA); %>


                        <%
                    
             } else 
             { %>
                   <%=Html.Label("Non sono presenti deleghe",new { @class = "myLabel"})  %> 
                
          <% }
                            %>

        <%
        }
        %>

 

 </fieldset>
</div>
      </div>

      <% Html.RenderPartial("~/Views/Home/AlertMessage.cshtml"); %>  


</asp:Content>

    