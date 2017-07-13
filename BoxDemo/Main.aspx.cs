using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Box;
using BoxObjects;
using Newtonsoft.Json;

namespace BoxDemo
{
    public partial class Main : System.Web.UI.Page
    {
        //The first three values depend on the application
        string m_boxAppClientID = "";
        string m_boxAppClientSecret = "";
        string m_boxAppRedirectURI = "";
        string m_boxUserOAuth2AccessToken = "";
        string m_boxUserOAuth2RefreshToken = "";
        string m_authFile = HttpContext.Current.Server.MapPath("files") + "\\auth.txt";
        string m_clientFile = HttpContext.Current.Server.MapPath("files") + "\\clientSettings.txt";
        string BOXCLIENT = "boxclient";

        private string m_curlLocation = "C:\\WebProjects\\Repository\\Box\\Box\\packages\\curl_amd_64\\CURL.EXE";
        //private string m_curlLocation = HttpContext.Current.Server.MapPath("packages") + "\\curl_amd_64\\CURL.EXE";
        //private string m_curlLocation = "CURL.EXE"
        //Above doesn't work if there is a space in the directory; I tried setting it to simply "CURL.EXE" and 
        //putting the .exe in the same directory as the bin/debug/  .dll, but that didn't work.
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (m_boxAppClientID == "" || m_boxAppClientSecret == "")
                {
                    using (StreamReader sR = new StreamReader(m_clientFile))
                    {
                        m_boxAppClientID = sR.ReadLine();
                        m_boxAppClientSecret = sR.ReadLine();
                        m_boxAppRedirectURI = sR.ReadLine();
                    }
                }
                if (Session[BOXCLIENT] == null)
                {
                    BoxClient b = new BoxClient(m_boxAppClientID, m_boxAppClientSecret, m_boxAppRedirectURI);
                    //Also need to specify the path where the tokens will be save.
                    b.OAuth2TokenFileLocation = m_authFile;
                    //If we have the acess and refresh tokens, supply them
                    if (File.Exists(m_authFile))
                    {
                        using (StreamReader sR = new StreamReader(m_authFile))
                        {
                            m_boxUserOAuth2AccessToken = sR.ReadLine();
                            m_boxUserOAuth2RefreshToken = sR.ReadLine();
                        }
                    } else
                    {
                        m_boxUserOAuth2AccessToken = "";
                        m_boxUserOAuth2RefreshToken = "";
                    }
                    b.BoxUserOAuth2AccessToken = m_boxUserOAuth2AccessToken;
                    b.BoxUserOAuth2RefreshToken = m_boxUserOAuth2RefreshToken;
                    Session[BOXCLIENT] = b;
                }
                if (Request.QueryString["code"] != null)
                {
                    //The authCode that box gave us:
                    string authCode = Request.QueryString["code"];
                    BoxClient b = (BoxClient)Session[BOXCLIENT];
                    b.GetOAuth2Token(authCode);
                    //At this point, our Box client should be good, but we don't the code query string in our url.
                    Response.Redirect(Request.Url.AbsoluteUri.Split('?')[0]);
 
                }
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }

        }
        
         protected void btnGetRootDirectories_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                string resp = b.JSON_Folder_GetItems(Int64.Parse(txtFolderID.Text), limit: int.Parse(txtLimit.Text));
                if (resp == "")
                {
                    Response.Redirect("https://account.box.com/api/oauth2/authorize?response_type=code&client_id=" + m_boxAppClientID +
                        "&redirect_uri=" + HttpUtility.UrlEncode(m_boxAppRedirectURI));
                }
                else
                {
                    //Do something with the root directories JSON.
                    jsonTextArea.Text = resp;
                }
            } catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
           
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                BoxEnums.ObjectType bot = BoxEnums.ObjectType.UNSPECIFIED;
                switch (txtObjectType.Text.Split('_')[0])
                {
                    case "file":
                        bot = BoxEnums.ObjectType.FILE;
                        break;
                    case "folder":
                        bot = BoxEnums.ObjectType.FOLDER;
                        break;
                    case "web":
                        bot = BoxEnums.ObjectType.WEB_LINK;
                        break;
                }
                string resp = b.JSON_Search(txtSearchText.Text,
                    csvAncestorFolderIDs: txtSearchAncestorFolders.Text,
                    csvFileExtensions: txtFileExtensions.Text,
                    csvOwnerIDs: txtOwnerIDs.Text,
                    csvContentTypes: txtContentType.Text,
                    type:bot, csvFields:txtFields.Text);
                if (resp == "")
                {
                    Response.Redirect("https://account.box.com/api/oauth2/authorize?response_type=code&client_id=" + m_boxAppClientID +
                        "&redirect_uri=" + HttpUtility.UrlEncode(m_boxAppRedirectURI));
                }
                else
                {
                    jsonTextArea.Text = resp;
                }
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnCreateFolder_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                Int64 boxID = -1;
                string boxURL = "";
                jsonTextArea.Text = b.JSON_Folder_Create(Int64.Parse(txtCreateFolderParentID.Text),
                                                    txtCreateFolderName.Text,
                                                    ref boxID, ref boxURL,
                                                    txtFields.Text);
                
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnDeleteFolder_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                Boolean successful = false;
                string result = b.JSON_Folder_Delete(Int64.Parse(txtFolderIDToDelete.Text),
                                                    ref successful, Boolean.Parse(txtRecursiveDelete.Text));

                string prefix = "NOT PART OF JSON!!! SUCCESS" + Environment.NewLine;
                if (!successful)
                {
                    prefix = "NOT PART OF JSON!!! FAIL" + Environment.NewLine;
                }
                jsonTextArea.Text = prefix + result;
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnGetFolderInfo_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                jsonTextArea.Text = b.JSON_Item_GetInfo(BoxEnums.ObjectType.FOLDER, Int64.Parse(txtFolderID.Text),
                                                    txtFields.Text);
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnGetFolderTags_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                Boolean errOccurred = false;
                Boolean OAuth2Req = false;
                List<string> l = b.GetFolderTags(Int64.Parse(txtFolderID.Text),
                                                    ref errOccurred,
                                                    ref OAuth2Req);
                if (errOccurred)
                {
                    jsonTextArea.Text = b.ErrMsg;
                } else if (OAuth2Req)
                {
                    jsonTextArea.Text = "Need to refresh tokens with call to box.";
                } else
                {
                    jsonTextArea.Text = String.Join(Environment.NewLine, l.ToArray());
                }

            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnAddTags_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                Boolean errOccurred = false;
                Boolean OAuth2Req = false;
                List<string> lstT = txtTags.Text.Split(',').ToList();
                List<string> TagResult = b.AddFolderTags(Int64.Parse(txtTagFolderID.Text),
                                                    lstT,
                                                    ref errOccurred,
                                                    ref OAuth2Req);
                if (errOccurred)
                {
                    jsonTextArea.Text = b.ErrMsg;
                }
                else if (OAuth2Req)
                {
                    jsonTextArea.Text = "Need to refresh tokens with call to box.";
                }
                else
                {
                    jsonTextArea.Text = String.Join(Environment.NewLine, TagResult.ToArray());
                }

            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnRemoveTags_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                Boolean errOccurred = false;
                Boolean OAuth2Req = false;
                List<string> lstT = txtTags.Text.Split(',').ToList();
                List<string> TagResult = b.RemoveFolderTags(Int64.Parse(txtTagFolderID.Text),
                                                    lstT,
                                                    ref errOccurred,
                                                    ref OAuth2Req);
                if (errOccurred)
                {
                    jsonTextArea.Text = b.ErrMsg;
                }
                else if (OAuth2Req)
                {
                    jsonTextArea.Text = "Need to refresh tokens with call to box.";
                }
                else
                {
                    jsonTextArea.Text = String.Join(Environment.NewLine, TagResult.ToArray());
                }

            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnAddCollaborator_Click(object sender, EventArgs e)
        {
            try
            {
                Boolean notify = false;
                if (int.Parse(cboCollabNotify.SelectedValue) == 1)
                {
                    notify = true;
                } 
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                jsonTextArea.Text = b.JSON_Collaboration_Add(txtFields.Text,
                    notify, 
                    (BoxObjects.BoxEnums.ObjectType)int.Parse(cboCollabItemType.SelectedValue), 
                    Int64.Parse(txtCollabItemID.Text),
                    txtCollabEmail.Text, 
                    (BoxEnums.CollaboratorRole)int.Parse(cboCollabRole.SelectedValue));
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnGetCollaboratorInfo_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                jsonTextArea.Text = b.JSON_Collaboration_Get(txtFields.Text,
                    (BoxObjects.BoxEnums.ObjectType)int.Parse(cboCollabItemType.SelectedValue),
                    Int64.Parse(txtCollabItemID.Text));
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnRemoveCollaborator_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                Boolean blOK = false;
                jsonTextArea.Text = b.JSON_Collaboration_Delete(txtCollabRemoveID.Text, ref blOK);
                if (blOK)
                {
                    jsonTextArea.Text = "SUCCESS (this is not part of JSON)" + Environment.NewLine + jsonTextArea.Text;
                } else
                {
                    jsonTextArea.Text = "FAIL (this is not part of JSON)" + Environment.NewLine + jsonTextArea.Text;
                }
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnUpdateCollaborator_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                jsonTextArea.Text = b.JSON_Collaboration_Modify(txtCollabRemoveID.Text,
                    (BoxEnums.CollaboratorRole)int.Parse(cboCollabRole.SelectedValue));
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                b.CurlFullPath = m_curlLocation;
                jsonTextArea.Text = b.JSON_File_Upload_Curl(Int64.Parse(txtUploadParentID.Text),
                    HttpContext.Current.Server.MapPath("files") + "\\" + txtUploadFileName.Text);
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnGetFileInfo_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                jsonTextArea.Text = b.JSON_Item_GetInfo(BoxEnums.ObjectType.FILE, Int64.Parse(txtFileID.Text),
                                                    txtFields.Text);
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnUpdateUploadedFile_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                b.CurlFullPath = m_curlLocation;
                jsonTextArea.Text = b.JSON_File_Update_Curl(Int64.Parse(txtFileID.Text),
                    HttpContext.Current.Server.MapPath("files") + "\\" + txtUploadFileName.Text);
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnDownloadFile_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                jsonTextArea.Text = b.File_Download(Int64.Parse(txtFileID.Text),
                    HttpContext.Current.Server.MapPath("files") + "\\" + txtUploadFileName.Text).ToString();
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnUpdateItem_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                jsonTextArea.Text = b.JSON_Item_Update(
                    (BoxObjects.BoxEnums.ObjectType)int.Parse(cboUpdateItemType.SelectedValue),
                    Int64.Parse(txtUpateItemID.Text),
                    txtUpdateItemName.Text,
                    txtUpdateItemDescription.Text,
                    txtUpdateItemNewParentID.Text,
                    csvFields: txtFields.Text);
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnCreateUpdateSharedLink_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                jsonTextArea.Text = b.JSON_SharedLink_CreateUpdate(
                    (BoxObjects.BoxEnums.ObjectType)int.Parse(cboUpdateItemType.SelectedValue),
                    Int64.Parse(txtSharedLinkTargetID.Text),
                    (BoxObjects.BoxEnums.SharedLinkAccess)int.Parse(cboSharedLinkAccess.SelectedValue),
                    csvFields:txtFields.Text);
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnGetSharedLink_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                jsonTextArea.Text = b.JSON_SharedLink_Get(
                    (BoxObjects.BoxEnums.ObjectType)int.Parse(cboUpdateItemType.SelectedValue),
                    Int64.Parse(txtSharedLinkTargetID.Text)) + Environment.NewLine + Environment.NewLine +
                    "sResult: " + b.sSharedLink_Get((BoxObjects.BoxEnums.ObjectType)int.Parse(cboUpdateItemType.SelectedValue),
                    Int64.Parse(txtSharedLinkTargetID.Text));
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnDeleteSharedLink_Click(object sender, EventArgs e)
        {
            try
            {
                Boolean success = false;
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                jsonTextArea.Text = b.JSON_SharedLink_Delete(
                    (BoxObjects.BoxEnums.ObjectType)int.Parse(cboUpdateItemType.SelectedValue),
                    Int64.Parse(txtSharedLinkTargetID.Text),
                    ref success,
                    txtFields.Text);
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnRemoveSharedLinkExpDate_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                jsonTextArea.Text = b.JSON_SharedLink_RemoveExpirationDate(
                    (BoxObjects.BoxEnums.ObjectType)int.Parse(cboUpdateItemType.SelectedValue),
                    Int64.Parse(txtSharedLinkTargetID.Text));
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnInitialize_Click(object sender, EventArgs e)
        {
            try { 
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                //Do a simple call to see if it works...
                string resp = b.JSON_Folder_GetItems(0, limit: 1);
                if (resp == "")
                {
                    //This kicks off the authentication process
                    Response.Redirect("https://account.box.com/api/oauth2/authorize?response_type=code&client_id=" + m_boxAppClientID +
                        "&redirect_uri=" + HttpUtility.UrlEncode(m_boxAppRedirectURI));
                }

            } catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnGetWebLink_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                jsonTextArea.Text = b.JSON_WebLink_Get(txtWebLinkID.Text);
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnCreateWebLink_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];

                jsonTextArea.Text = b.JSON_WebLink_Create(txtWebLinkURL.Text, 
                                                    txtWebLinkParentFolderID.Text, 
                                                    txtWebLinkName.Text,
                                                    txtWebLinkDescription.Text);
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnUpdateWebLink_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                jsonTextArea.Text = b.JSON_WebLink_Update(txtWebLinkID.Text,
                                                    txtWebLinkURL.Text,
                                                    txtWebLinkParentFolderID.Text,
                                                    txtWebLinkName.Text,
                                                    txtWebLinkDescription.Text);
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnDeleteWebLink_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                Boolean success = false;
                string resp = b.JSON_WebLink_Delete(txtWebLinkID.Text, ref success);
                if (success)
                {
                    resp = "DELETED!";
                }
                jsonTextArea.Text = resp;
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnDeleteFile_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                Boolean success = false;
                string resp = b.JSON_File_Delete(Int64.Parse(txtFileID.Text), ref success);
                if (success)
                {
                    resp = "DELETED!";
                }
                jsonTextArea.Text = resp;
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnCreateSubFolderPath_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                Int64 newFolderID = b.sDirectory_Create(Int64.Parse(txtCreateFolderParentID.Text), txtCreateFolderName.Text);
                jsonTextArea.Text = "NewFolderID: " + newFolderID.ToString() + "\r" +
                                    "Error message: " + b.ErrMsg;
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnCopy_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                string version = null;
                if (txtCopyVersion.Text != "")
                {
                    version = txtCopyVersion.Text;
                }
                string newName = null;
                if (txtCopyNewName.Text != "")
                {
                    newName = txtCopyNewName.Text;
                }
                jsonTextArea.Text = b.JSON_Item_Copy((BoxEnums.ObjectType)int.Parse(cboCopyItemType.SelectedValue),
                    Int64.Parse(txtCopyItemID.Text),
                    Int64.Parse(txtCopyDestinationFolderID.Text),
                    txtFields.Text, newName, version);
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnUploadSimple_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                b.CurlFullPath = m_curlLocation;
                Int64 fileID = -1;
                if(b.sFile_Upload_Curl(Int64.Parse(txtUploadParentID.Text),
                    HttpContext.Current.Server.MapPath("files") + "\\" + txtUploadFileName.Text,
                    ref fileID))
                {
                    jsonTextArea.Text = fileID.ToString();
                } else
                {
                    jsonTextArea.Text = b.ErrMsg;
                }
            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        protected void btnCopySimple_Click(object sender, EventArgs e)
        {
            try
            {
                BoxClient b = (BoxClient)Session[BOXCLIENT];
                Int64 newItemID = -1;
                if (b.sItem_Copy((BoxEnums.ObjectType)int.Parse(cboCopyItemType.SelectedValue),
                    Int64.Parse(txtCopyItemID.Text),
                    Int64.Parse(txtCopyDestinationFolderID.Text),
                    ref newItemID))
                {
                    jsonTextArea.Text = newItemID.ToString();
                } else
                {
                    jsonTextArea.Text = "Unable to copy: " + b.ErrMsg;
                }

            }
            catch (Exception ex)
            {
                jsonTextArea.Text = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }
    }
}