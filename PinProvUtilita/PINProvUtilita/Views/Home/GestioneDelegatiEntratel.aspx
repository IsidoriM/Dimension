<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site1.Master" Inherits="System.Web.Mvc.ViewPage<PINProvUtilita.Models.Certificati>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    I.N.P.S. - PIN PROVISIONING - Delegati Entratel
 
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <script type="text/javascript" src="<%: Url.Content("~//Scripts/jquery-1.8.3.min.js") %>"> </script>
    <script type="text/javascript" src="<%: Url.Content("~//Scripts/jquery-1.9.1.js") %>"> </script>
    <script type="text/javascript" src="<%: Url.Content("~//Scripts/jquery-1.9.1.min.js") %>"> </script>
            <div class="myLabel">GESTIONE DELEGATI CERTIFICATI ENTRATEL </div>
    <div class="centerFuori">
        <div class="divSanitaria">    
             <% using (Html.BeginForm())
            {              
            %>
            <%= Html.ValidationSummary(true) %>
  
            <div class="center">
                <br />
                <div class="divSpacer">
                </div>
                <fieldset class="field_set">
                    <div class="myLabel">    
                        <table style="height: 80px;">
                            <tr>
                                <td>
                                    <%= Html.LabelFor(model => model.CodiceFiscale) %>
                                </td>
                                <td>
                                    <%
                                    if ( ViewData["partitaIva"] != null)
                                    {    
                                    %>       
                                        <%=Html.TextBoxFor(model => model.CodiceFiscale, new { @style = "text-transform:uppercase",  @maxlength = "16", @size="25", @Value= (String)Session["CodiceFiscale"] } )%>
                                    <%
                                    }
                                    else
                                    {
                                    %>
                                        <%=Html.TextBoxFor(model => model.CodiceFiscale, new { @style = "text-transform:uppercase",  @maxlength = "16", @size="25" } )%>
                                    <% } %>
                                </td>
                                <td>
                                    <div style="width: 100px;">
                                        <div style="float: left; padding: 0; margin: 0; vertical-align: top;">
                                            <input class ="BtnGrafica" type="submit" value="Ricerca" />
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
                </fieldset>
            </div>
         
    <%}
          
         
 
            if (ViewData["RicercaCodici"] != null)
            {%>
            <br /><br />
            <fieldset style="text-align: center;">
                <legend class="myLabel">CERTIFICATI DI <%= Html.Label((String)Session["CodiceFiscale"]).ToString().ToUpper()%>
                </legend>
   
                    <%= Html.ValidationSummary(true) %>
                        <% 
                        List<PINProvUtilita.Models.Lista_Delegati> listaGetDelegati = new List<PINProvUtilita.Models.Lista_Delegati>();
                        listaGetDelegati = (List<PINProvUtilita.Models.Lista_Delegati>)(ViewData["listaGetDelegati"]);
                        String CodiceFiscale = (String)Session["CodiceFiscale"];
                        %>
                    <%--  <%=Html.Raw(TempData["msg"].ToString())%>--%>
                    <% 
                          List<PINProvUtilita.Models.Certificati> listaGetDatiGrid = new List<PINProvUtilita.Models.Certificati>();
                          listaGetDatiGrid = (List<PINProvUtilita.Models.Certificati>)(Session["listaGetDatiGrid"]);
                          if (listaGetDatiGrid.Count > 0)
                          {
                              %>
                                <% Html.RenderPartial("~/Views/Home/Certificati.cshtml", listaGetDatiGrid); %>
                            <br />
                            <br />
                          <%  
                           }  else
                           {%>
                                    <div class="myLabel">
                                   <%= Html.Label("NON SONO PRESENTI CERTIFICATI")%>
                                   </div>
                                   <br />
                                   <br />
                           <%}   
                           if (CodiceFiscale.Length==16)
                           { 
                              if(listaGetDelegati.Count > 0)
                              {  
                           
                                  %>
                                    <div class="myLabel">
                                   <%= Html.Label("DELEGATO PER I SEGUENTI CERTIFICATI DI SOGGETTI GIURIDICI")%>
                                   </div>
 
                                     <% Html.RenderPartial("~/Views/Home/DelegatoCF.cshtml", listaGetDelegati); %>



                           <%} else
                             {
                           
                             %>
                                <div class="myLabel">
                                <%= Html.Encode(ViewData["SecondaTabella"])%>
                                 </div>
                           <%
                             }
                         using (Html.BeginForm("InsertCodiceFiscale", "Home", listaGetDatiGrid, FormMethod.Post))
                         { %>
    
                        <p>
                            <input class="BtnRevoca" type="submit" value="Inserisci Delega"  />
                        </p>
               
                        <% 
                         }
                     }%>
            </fieldset>
            <%                      
            }%>
 
        </div>
    </div>

        <% Html.RenderPartial("~/Views/Home/AlertMessage.cshtml"); %>  
</asp:Content>
