<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Main.aspx.cs" Inherits="BoxDemo.Main" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Button ID="btnInitialize" runat="server" Text="Initialize App" BackColor="#00FF99" OnClick="btnInitialize_Click" />
        <asp:Panel ID="divJsonResults" runat="server">
            <h2>JSON Response</h2><hr />
            <asp:TextBox ID="jsonTextArea" runat="server" Rows="10" style="width:100%" TextMode="MultiLine"></asp:TextBox><br />
            <asp:Label ID="Label2" runat="server" Text="Returned Item Limit:"></asp:Label><asp:TextBox ID="txtLimit" runat="server">100</asp:TextBox><br />
        </asp:Panel>
        <asp:Panel ID="divCommonFields" runat="server">
            <h2>Common Query Parameters</h2><hr />
            <asp:Label ID="Label11" runat="server" Text="Fields:"></asp:Label><asp:TextBox ID="txtFields" runat="server" style="width:100%"></asp:TextBox>
        </asp:Panel>
        <asp:Panel ID="divFolderItems" runat="server">
            <hr />
            <h3>Get Folder Info</h3>
            <asp:Button ID="btnGetRootDirectories" runat="server" Text="Get Directory Items" OnClick="btnGetRootDirectories_Click" />
            <asp:Button ID="btnGetFolderInfo" runat="server" Text="Get Directory Info" OnClick="btnGetFolderInfo_Click" />
            <asp:Button ID="btnGetFolderTags" runat="server" Text="Get Tags" OnClick="btnGetFolderTags_Click" />
            <br /><asp:Label ID="Label1" runat="server" Text="Folder ID:"></asp:Label><asp:TextBox ID="txtFolderID" runat="server">0</asp:TextBox><br />
            
        </asp:Panel>
        <asp:Panel ID="divSearch" runat="server">
            <hr />
            <h3>Search</h3>
            <asp:Button ID="btnSearch" runat="server" Text="SEARCH" OnClick="btnSearch_Click" />
            <asp:Label ID="Label3" runat="server" Text="Search Text"></asp:Label><asp:TextBox ID="txtSearchText" runat="server"></asp:TextBox><br />
            <asp:Label ID="Label4" runat="server" Text="CSV Ancestor Folder IDs"></asp:Label><asp:TextBox ID="txtSearchAncestorFolders" runat="server">31296461418</asp:TextBox><br />
            <asp:Label ID="Label5" runat="server" Text="CSV File Extensions"></asp:Label><asp:TextBox ID="txtFileExtensions" runat="server"></asp:TextBox><br />
            <asp:Label ID="Label6" runat="server" Text="CSV Owner IDs"></asp:Label><asp:TextBox ID="txtOwnerIDs" runat="server"></asp:TextBox><br />
            <asp:Label ID="Label7" runat="server" ToolTip="What you are searching for.  Possible values: name, description, file_content, comments, tags" 
                Text="Content Type"></asp:Label><asp:TextBox ID="txtContentType" runat="server"></asp:TextBox><br />
            <asp:Label ID="Label8" runat="server" ToolTip="0>> file; 1>> folder; 2>> web_link" 
                Text="Object Type"></asp:Label><asp:TextBox ID="txtObjectType" runat="server"></asp:TextBox><br />
        </asp:Panel>
        <asp:Panel ID="divCreateFolder" runat="server">
            <hr />
            <h3>Create Folder</h3>
            <asp:Button ID="btnCreateFolder" runat="server" Text="Create" OnClick="btnCreateFolder_Click" />
            <asp:Label ID="Label9" runat="server" Text="Parent Folder ID"></asp:Label><asp:TextBox ID="txtCreateFolderParentID" runat="server">31296461418</asp:TextBox><br />
            <asp:Label ID="Label10" runat="server" Text="Folder Name"></asp:Label><asp:TextBox ID="txtCreateFolderName" runat="server"></asp:TextBox><br />
            
        </asp:Panel>
        <asp:Panel ID="divDeleteFolder" runat="server">
            <hr />
            <h3>Delete Folder</h3>
            <asp:Button ID="btnDeleteFolder" runat="server" Text="Delete" OnClick="btnDeleteFolder_Click" />
            <asp:Label ID="Label12" runat="server" Text="Target Folder ID"></asp:Label><asp:TextBox ID="txtFolderIDToDelete" runat="server"></asp:TextBox><br />
            <asp:Label ID="Label13" runat="server" Text="Recursive Deletion"></asp:Label><asp:TextBox ID="txtRecursiveDelete" runat="server">true</asp:TextBox><br />
            
        </asp:Panel>
        <asp:Panel ID="divTags" runat="server">
            <hr />
            <h3>Add/Remove Tags</h3>
            <asp:Button ID="btnAddTags" runat="server" Text="Add Tags" OnClick="btnAddTags_Click" />
            <asp:Button ID="btnRemoveTags" runat="server" Text="Remove Tags" OnClick="btnRemoveTags_Click" />
            <br /><asp:Label ID="Label14" runat="server" Text="Target Folder ID"></asp:Label><asp:TextBox ID="txtTagFolderID" runat="server"></asp:TextBox><br />
            <asp:Label ID="Label15" runat="server" Text="CSV Tags"></asp:Label><asp:TextBox ID="txtTags" runat="server"></asp:TextBox><br />
            
        </asp:Panel>
        <asp:Panel ID="divCollaborations" runat="server">
            <hr />
            <h3>Collaboration</h3>
            <asp:Button ID="btnGetCollaboratorInfo" runat="server" Text="Get Collaboration Info" OnClick="btnGetCollaboratorInfo_Click" />
            <asp:Button ID="btnAddCollaborator" runat="server" Text="Add Collaborator" OnClick="btnAddCollaborator_Click" />
            <asp:Button ID="btnRemoveCollaborator" runat="server" Text="Remove Collaborator" OnClick="btnRemoveCollaborator_Click" />
            <asp:Button ID="btnUpdateCollaborator" runat="server" Text="Update Collaborator" OnClick="btnUpdateCollaborator_Click" />
            <br />
            <asp:Label ID="Label18" runat="server" Text="Item Type:"></asp:Label>
                <asp:DropDownList ID="cboCollabItemType" runat="server">
                    <asp:ListItem Text="File" Value="0"></asp:ListItem>
                    <asp:ListItem Text="Folder" Value="1" Selected="True"></asp:ListItem>
                </asp:DropDownList>
            <asp:Label ID="Label16" runat="server" Text="Target Item ID"></asp:Label><asp:TextBox ID="txtCollabItemID" runat="server">31380978121</asp:TextBox><br />
            <asp:Label ID="Label21" runat="server" Text="Target Collaboration ID (required when removing a user from a folder)"></asp:Label><asp:TextBox ID="txtCollabRemoveID" runat="server"></asp:TextBox><br />
            <asp:Label ID="Label17" runat="server" Text="Collaborator Email"></asp:Label><asp:TextBox ID="txtCollabEmail" runat="server">dwatson8@tutanota.com</asp:TextBox><br />
            <asp:Label ID="Label19" runat="server" Text="Collaborator Role"></asp:Label>
                <asp:DropDownList ID="cboCollabRole" runat="server">
                    <asp:ListItem Text="Editor" Value="0"></asp:ListItem>
                    <asp:ListItem Text="Viewer" Value="1" ></asp:ListItem>
                    <asp:ListItem Text="Previewer" Value="2" ></asp:ListItem>
                    <asp:ListItem Text="Uploader" Value="3" ></asp:ListItem>
                    <asp:ListItem Text="Previewer/Uploader" Value="4" ></asp:ListItem>
                    <asp:ListItem Text="Viewer/Uploader" Value="5" ></asp:ListItem>
                    <asp:ListItem Text="Co-owner" Value="6" ></asp:ListItem>
                    <asp:ListItem Text="Owner" Value="7" ></asp:ListItem>
                </asp:DropDownList>
                <asp:Label ID="Label20" runat="server" Text="Notify Collaborator"></asp:Label>
                <asp:DropDownList ID="cboCollabNotify" runat="server">
                    <asp:ListItem Text="NO" Value="0"></asp:ListItem>
                    <asp:ListItem Text="YES" Value="1" Selected="True" ></asp:ListItem>
                </asp:DropDownList>
            <br />
            
        </asp:Panel>
        <asp:Panel ID="divFileUpload" runat="server">
            <hr />
            <h3>File Actions</h3>
            <asp:Button ID="btnGetFileInfo" runat="server" Text="Get File Info" OnClick="btnGetFileInfo_Click"  />
            <asp:Button ID="btnUpload" runat="server" Text="Upload New File" OnClick="btnUpload_Click"  />
            <asp:Button ID="btnUpdateUploadedFile" runat="server" Text="ReUpload File" OnClick="btnUpdateUploadedFile_Click" />
            <asp:Button ID="btnDownloadFile" runat="server" Text="Download File" OnClick="btnDownloadFile_Click" />
            <asp:Button ID="btnDeleteFile" runat="server" Text="Delete File" OnClick="btnDeleteFile_Click" />
            <br />
            <asp:Label ID="Label22" runat="server" Text="File Name (for this demo, file must already be saved in this App's 'files' folder):"></asp:Label><br />
            <asp:TextBox ID="txtUploadFileName" runat="server">test1.txt</asp:TextBox><br />
            <asp:Label ID="Label23" runat="server" Text="ID of Parent Folder"></asp:Label>
            <asp:TextBox ID="txtUploadParentID" runat="server">31390927070</asp:TextBox><br />
            <asp:Label ID="Label24" runat="server" Text="Target File ID:"></asp:Label>
            <asp:TextBox ID="txtFileID" runat="server"></asp:TextBox><br />            
        </asp:Panel>
        <asp:Panel ID="divUpdateItem" runat="server">
            <hr />
            <h3>Update Files/Folder</h3>
            <asp:Button ID="btnUpdateItem" runat="server" Text="Update Item" OnClick="btnUpdateItem_Click" />
            <br />
            <asp:Label ID="Label25" runat="server" Text="Item Type:"></asp:Label>
                <asp:DropDownList ID="cboUpdateItemType" runat="server">
                    <asp:ListItem Text="File" Value="0"></asp:ListItem>
                    <asp:ListItem Text="Folder" Value="1" Selected="True"></asp:ListItem>
                </asp:DropDownList>
            <asp:Label ID="Label26" runat="server" Text="Target Item ID"></asp:Label><asp:TextBox ID="txtUpateItemID" runat="server">31390947311</asp:TextBox><br />
            <asp:Label ID="Label27" runat="server" Text="New Parent Item ID (New Parent Folder)"></asp:Label><asp:TextBox ID="txtUpdateItemNewParentID" runat="server">31390924344</asp:TextBox>
            <asp:Button ID="btnCopyItem" runat="server" Text="COPY Item" /><br />
            <asp:Label ID="Label28" runat="server" Text="Name"></asp:Label><asp:TextBox ID="txtUpdateItemName" runat="server"></asp:TextBox><br />
            <asp:Label ID="Label29" runat="server" Text="Description"></asp:Label><asp:TextBox ID="txtUpdateItemDescription" runat="server"></asp:TextBox><br />
            <br />
            
        </asp:Panel>
        <asp:Panel ID="divSharedLink" runat="server">
            <hr />
            <h3>Shared Links</h3>
            <asp:Button ID="btnGetSharedLink" runat="server" Text="GET Shared Link" OnClick="btnGetSharedLink_Click" />
            <asp:Button ID="btnCreateSharedLink" runat="server" Text="CREATE Shared Link" OnClick="btnCreateUpdateSharedLink_Click" />
            <asp:Button ID="btnUpdateSharedLink" runat="server" Text="UPDATE Shared Link" OnClick="btnCreateUpdateSharedLink_Click" />
            <asp:Button ID="btnDeleteSharedLink" runat="server" Text="DELETE Shared Link" OnClick="btnDeleteSharedLink_Click" />
            <asp:Button ID="btnRemoveSharedLinkExpDate" runat="server" Text="Remove Shared Link Expiration Date" OnClick="btnRemoveSharedLinkExpDate_Click" />
            <br />
            <asp:Label ID="Label30" runat="server" Text="Item Type:"></asp:Label>
                <asp:DropDownList ID="cboSharedLinkType" runat="server">
                    <asp:ListItem Text="File" Value="0"></asp:ListItem>
                    <asp:ListItem Text="Folder" Value="1" Selected="True"></asp:ListItem>
                </asp:DropDownList>
            <asp:Label ID="Label31" runat="server" Text="Target Item ID"></asp:Label><asp:TextBox ID="txtSharedLinkTargetID" runat="server">31390924344</asp:TextBox><br /><br />
            <asp:Label ID="Label32" runat="server" Text="Shared Link Access Level:"></asp:Label>
            <asp:DropDownList ID="cboSharedLinkAccess" runat="server">
                <asp:ListItem Text="Open - anyone with link" Value="0"></asp:ListItem>
                <asp:ListItem Text="Company - anyone in company" Value="1"></asp:ListItem>
                <asp:ListItem Text="Collaborators - folder collaborators only" Value="2" ></asp:ListItem>
            </asp:DropDownList>       
        </asp:Panel>
        <asp:Panel ID="divWebLink" runat="server">
            <hr />
            <h3>Web Links (Bookmarks)</h3>
            <asp:Button ID="btnGetWebLink" runat="server" Text="GET Web Link" OnClick="btnGetWebLink_Click" />
            <asp:Button ID="btnCreateWebLink" runat="server" Text="CREATE Web Link" OnClick="btnCreateWebLink_Click" />
            <asp:Button ID="btnUpdateWebLink" runat="server" Text="UPDATE Web Link" OnClick="btnUpdateWebLink_Click" />
            <asp:Button ID="btnDeleteWebLink" runat="server" Text="DELETE Web Link" OnClick="btnDeleteWebLink_Click" />
            <br />
            <asp:Label ID="Label37" runat="server" Text="Web Link ID:"></asp:Label><asp:TextBox ID="txtWebLinkID" runat="server"></asp:TextBox><br />
            <asp:Label ID="Label33" runat="server" Text="URL:"></asp:Label><asp:TextBox ID="txtWebLinkURL" runat="server"></asp:TextBox><br />
            <asp:Label ID="Label36" runat="server" Text="Parent Folder ID:"></asp:Label><asp:TextBox ID="txtWebLinkParentFolderID" runat="server"></asp:TextBox><br />
            <asp:Label ID="Label34" runat="server" Text="Web Link Name"></asp:Label><asp:TextBox ID="txtWebLinkName" runat="server"></asp:TextBox><br />
            <asp:Label ID="Label35" runat="server" Text="Web Link Description"></asp:Label><asp:TextBox ID="txtWebLinkDescription" runat="server"></asp:TextBox><br />
      
        </asp:Panel>
    </form>
</body>
</html>
