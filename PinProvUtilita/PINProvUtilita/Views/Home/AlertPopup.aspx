<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<PINProvUtilita.Models.messaggi>" %>

<!DOCTYPE html>

<html>
<head id="Head1" runat="server">
    <meta name="viewport" content="width=device-width" />
    <title>ProvaUser</title>
    <link href="../../css/menu/FoglioStyle.css" rel="stylesheet" />
 <script>

     function Close() {

         window.parent.modalWin.HideModalPopUp();
     }

</script>

</head>
<body>
 

 <asp:Panel ID="modalPopUpAlert" runat="server" CssClass="modalPopUp" Visible="true"> 
  
    <div class="ppHeader" id="HeaderDropAlert" runat="server">
       <%=Html.Label("Attenzione") %></div>
    <div class="ppContent">
        <table style="width: 100%">
            <tr>
                <td style="vertical-align: middle; text-align: center;">
                    <img src="<%: Url.Content("~/Images/icon_exclamation.png") %>" />
                </td>
                <td>
                    <%= Html.Raw(Model.MsgErrore.ToString())%>
                </td>
            </tr>
        </table>
    </div>

 <div style="text-align:center">
     <br />
<input type="button" style="width: 50px" onclick="Close()" 
        value="OK" /><br /><br />
<div id="divShowChildWindowValues" style="display:none; border:1px dashed black;padding:10px;color:Green; width:300px; font-size:12pt;text-align:left"  >

</div>

</div>


</asp:Panel>


</body>
</html>
