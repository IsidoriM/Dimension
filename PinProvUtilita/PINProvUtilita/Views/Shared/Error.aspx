﻿<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<!DOCTYPE html>
<html>
<head id="Head1" runat="server">
    <title><asp:ContentPlaceHolder ID="TitleContent" runat="server" /></title>
    <link href="../../Content/Site.css" rel="stylesheet" type="text/css" />
    <link href="../../Content/FoglioStyle.css" rel="stylesheet" type="text/css" />
    <script src="<%: Url.Content("~/Scripts/jquery-1.4.4.min.js") %>" type="text/javascript"></script>
</head>

<body>
    <div class="page">

        <div id="header">
            <div id="title">
                <h1>My MVC Application</h1>
            </div>
              
            <div id="logindisplay">
                <% Html.RenderPartial("LogOnUserControl"); %>
            </div> 
            
            <div id="menucontainer">
            
                <ul id="menu">              
                    <li><%: Html.ActionLink("Home", "Index", "Home")%></li>
                    <li><%: Html.ActionLink("About", "About", "Home")%></li>
                </ul>
            
            </div>
        </div>

        <div id="main">
            <asp:ContentPlaceHolder ID="MainContent" runat="server" />

            <div id="footer">
            </div>
        </div>
    </div>
</body>
</html>