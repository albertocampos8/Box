<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BasicBoxAuthentication.aspx.cs" Inherits="BoxDemo.BasicBoxAuthentication" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Button ID="btnInitialize" runat="server" Text="Initialize" OnClick="btnInitialize_Click" />
        <asp:Button ID="btnExecuteAPI" runat="server" Text="Execute API Call" OnClick="btnExecuteAPI_Click" />
        <asp:Button ID="btnRefreshTokens" runat="server" Text="Refresh Tokens" OnClick="btnRefreshTokens_Click" />
    </div>
    </form>
</body>
</html>
