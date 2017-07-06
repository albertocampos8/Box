using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Xml;
using System.Web;
using BoxObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Box
{
    public class BoxClient
    {
        public enum ReqVerb { GET, POST, PUT, DELETE }

        string m_errMsg = "";

        string m_boxAppClientID = "";       //Comes from the Box Project App Page when you register your app.
        string m_boxAppClientSecret = "";   //Comes from the Box Project App Page when you register your app.

        //**************  OAuth2 Parameters ****************** 
        Boolean m_blOAuth2 = false;
        Boolean m_blAttemptedTokenRefresh = false;  //This gets set true if a class method has to call method
        //GetLatestOAuth2 tokens.  If gets set false if the API Call fails and this value is already TRUE.
        private string m_boxUserOAuth2AccessToken = ""; //The access token used to make a request to the API.
        private string m_boxUserOAuth2RefreshToken = "";    //The token used to refresh the access token, which only lasts for 60minutes. 

        private string m_bURI_OAuth2Redirect = "";    //This is the site in your Web Project where you Box will redirect once the user has
                                              //granted access to your App.  The page_load method of that page should expect a Query String
                                              //and call this method's GetAuthToken method.
                                              //Also note this site needs to be registered in your App's Project Control Panel.
                                              //Ideally, you only sign in once, and the app automatically refreshes/stores the required
                                              //tokens.
        private string m_OAuth2TokenFileLocation = "";  //The file where the Access and Refresh OAuth2 Tokens are stored.
        //************* END OAUTH2 PARAMETERS ****************

        //BOX API URIs
        private string m_boxURI_TokenExchange = "https://api.box.com/oauth2/token";
        private string m_boxURI_Folder = "https://api.box.com/2.0/folders";
        private string m_boxURI_File = "https://api.box.com/2.0/files";
        private string m_boxURI_File_Upload = "https://upload.box.com/api/2.0/files";
        private string m_boxURI_Search = "https://api.box.com/2.0/search";
        private string m_boxURI_Collaboration = "https://api.box.com/2.0/collaborations";

        //JSON serialization settings
        public JsonSerializerSettings JSS =
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };

        //Curl Location-- needed when uploading files (workaround until I figure out how to get
        //http request working)
        private string m_curlFullPath = "";


        public string ErrMsg
        {
            get
            {
                return m_errMsg;
            }

            set
            {
                m_errMsg = value;
            }
        }

        public string BoxAppClientID
        {
            get
            {
                return m_boxAppClientID;
            }

            set
            {
                m_boxAppClientID = value;
            }
        }

        public string BoxAppClientSecret
        {
            get
            {
                return m_boxAppClientSecret;
            }

            set
            {
                m_boxAppClientSecret = value;
            }
        }

        public bool BlOAuth2
        {
            get
            {
                return m_blOAuth2;
            }

            set
            {
                m_blOAuth2 = value;
            }
        }

        public bool BlAttemptedTokenRefresh
        {
            get
            {
                return m_blAttemptedTokenRefresh;
            }

            set
            {
                m_blAttemptedTokenRefresh = value;
            }
        }

        public string BoxUserOAuth2AccessToken
        {
            get
            {
                return m_boxUserOAuth2AccessToken;
            }

            set
            {
                m_boxUserOAuth2AccessToken = value;
            }
        }

        public string BoxUserOAuth2RefreshToken
        {
            get
            {
                return m_boxUserOAuth2RefreshToken;
            }

            set
            {
                m_boxUserOAuth2RefreshToken = value;
            }
        }

        public string BURI_OAuth2Redirect
        {
            get
            {
                return m_bURI_OAuth2Redirect;
            }

            set
            {
                m_bURI_OAuth2Redirect = value;
            }
        }

        public string OAuth2TokenFileLocation
        {
            get
            {
                return m_OAuth2TokenFileLocation;
            }

            set
            {
                m_OAuth2TokenFileLocation = value;
            }
        }

        public string CurlFullPath
        {
            get
            {
                return m_curlFullPath;
            }

            set
            {
                m_curlFullPath = value;
            }
        }

        //End API URIS

        /// <summary>
        /// Instantiate this object when usingn OAuth2 authentication.
        /// User should call CheckTokens after this to make sure tokens are valid (and procure valid tokens if they are not).
        /// </summary>
        /// <param name="appClientID"></param>
        /// <param name="appClientSecret"></param>
        /// <param name="oAuth2RedirectURI"></param>
        public BoxClient(string appClientID, string appClientSecret, string oAuth2RedirectURI)
        {
            try
            {
                BoxAppClientID = appClientID;
                BoxAppClientSecret = appClientSecret;
                BURI_OAuth2Redirect = oAuth2RedirectURI;
                BlOAuth2 = true;
                //Next, user should call CheckTokens
            }
            catch (Exception ex)
            {
                ErrMsg = ex.Message + System.Environment.NewLine + ex.StackTrace;
            }
        }

        /// <summary>
        /// This method should be put in the Page_load method of m_bURI_OAuth2Redirect, which is where Box will send its code
        /// if user authorizes access to the user's Box account.
        /// </summary>
        /// <param name="code">The value returned by Box to the app; get this value from QueryString["code"]</param>
        public Boolean GetOAuth2Token(string code)
        {
            try
            {
                string boxResp = ExecuteAPICall(m_boxURI_TokenExchange,
                     ReqVerb.POST,
                    "grant_type=authorization_code" +
                    "&code=" + code +
                    "&client_id=" + BoxAppClientID +
                    "&client_secret=" + BoxAppClientSecret,
                     false);


                //Analyze the response
                if (boxResp != "")
                {
                    List<string> lst = new List<string>();
                    lst.Add("access_token");
                    lst.Add("refresh_token");
                    Dictionary<string, string> result = ExtractParametersFromBoxResponse(boxResp, lst);
                    if (result.Keys.Count == lst.Count)
                    {
                        BoxUserOAuth2AccessToken = result["access_token"];
                        BoxUserOAuth2RefreshToken = result["refresh_token"];
                        //Save to database-- or in this case, file
                        SaveTokens();
                        return true;
                    }
                    else
                    {
                        ErrMsg = boxResp;
                        return false;
                    }
                }
                else
                {
                    ErrMsg = "Unexpected error: API Call returned no string!";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrMsg = ex.Message + System.Environment.NewLine + ex.StackTrace;
                return false;
            }
        }
        /// <summary>
        /// This method attempts to retrieve the *latest* OAuth2 Access/Refresh tokens, as saved either in
        /// a file (implemented here) or database (not implemented here).
        /// If an API call fails with an Error, methods in this Class should try again after updating the tokens with this method.
        /// 
        /// </summary>
        /// <returns></returns>
        public Boolean GetLatestOAuth2Tokens()
        {
            try
            {
                if (File.Exists(OAuth2TokenFileLocation))
                {
                    using (StreamReader sR = new StreamReader(OAuth2TokenFileLocation))
                    {
                        BoxUserOAuth2AccessToken = sR.ReadLine();
                        BoxUserOAuth2RefreshToken = sR.ReadLine();
                    }
                }
                else
                {
                    //expect to get values from database
                    BoxUserOAuth2AccessToken = "";
                    BoxUserOAuth2RefreshToken = "";
                }
                BlAttemptedTokenRefresh = true;
                return true;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.Message + System.Environment.NewLine + ex.StackTrace;
                return false;
            }
        }

        /// <summary>
        /// Makes a call to the Box Authorize API to renew the user's Access and Refresh tokens.
        /// If new tokens are obtained, saves the tokens, either to the file specified by m_OAuth2TokenFileLocation,
        /// or else a database (not implemented here).
        /// Returns TRUE if refresh was succesful, false otherwise.
        /// If caller receives False, then user needs to be redirected to the Box API, where user will enter credentials to
        /// (re-)authorize the app.
        /// </summary>
        /// <returns></returns>
        private Boolean RefreshOAuth2Tokens()
        {
            try
            {
                string boxResp = ExecuteAPICall(m_boxURI_TokenExchange,
                    ReqVerb.POST,
                    "grant_type=refresh_token" +
                    "&refresh_token=" + BoxUserOAuth2RefreshToken +
                    "&client_id=" + BoxAppClientID +
                    "&client_secret=" + BoxAppClientSecret,
                    false);
                //Indicate we TRIED a token refresh, regardless of whether or not it was successful.
                BlAttemptedTokenRefresh = true;
                //Analyze the response
                if (boxResp != "")
                {
                    List<string> lst = new List<string>();
                    lst.Add("access_token");
                    lst.Add("refresh_token");
                    Dictionary<string, string> result = ExtractParametersFromBoxResponse(boxResp, lst);
                    if (result.Keys.Count == lst.Count)
                    {
                        BoxUserOAuth2AccessToken = result["access_token"];
                        BoxUserOAuth2RefreshToken = result["refresh_token"];
                        SaveTokens();
                        return true;
                    }
                    else
                    {
                        ErrMsg = boxResp;
                        return false;
                    }
                }
                else
                {
                    ErrMsg = "Unexpected error: API Call returned no string!";
                    return false;
                }

            }
            catch (Exception ex)
            {
                ErrMsg = ex.Message + System.Environment.NewLine + ex.StackTrace;
                return false;
            }
        }

        /// <summary>
        /// Analyzes json response 'response' and extracts all parameters listed in lstP.
        /// Returns a dictionary whose key are the parameter names in lstP and whose values are the values in response.
        /// </summary>
        /// <param name="response">JSON string obtained from a BOX API call</param>
        /// <param name="lstP">List of parameter names whose value we want.  Use full name; omit surrounding quotes and ':'</param>
        /// <returns></returns>
        public Dictionary<string, string> ExtractParametersFromBoxResponse(string response, List<string> lstP)
        {
            Dictionary<string, string> d = new Dictionary<string, string>();
            try
            {
                string[] arrjson = response.Split(',');
                for (int i = 0; i < arrjson.Length; i++)
                {
                    for (int j = 0; j < lstP.Count; j++)
                    {
                        if (arrjson[i].ToLower().Contains("\"" + lstP[j].ToLower() + "\"" + ":"))
                        {
                            d.Add(lstP[j], arrjson[i].Split(':')[1].Replace("\"", ""));
                        }
                    }
                }
                return d;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.Message + System.Environment.NewLine + ex.StackTrace;
                return d;
            }
        }

        /// <summary>
        /// Returns the JSON response from the box API for a given call
        /// </summary>
        /// <param name="apiURL">The BOX API resource from which we are requesting information</param>
        /// <param name="reqType">Request Type (POST, GET, etc.)</param>
        /// <param name="dataPayload">Data want to send to the server.  Usually required for a Post Request</param>
        /// <param name="blIncludeAccessTokenInHeader">Set TRUE if the Box API Call requires an Access Token
        /// (basically, this only gets set FALSE when doing authorization)</param>
        /// <param name="contentType">Specify how the dataPayload should be encoded</param>
        /// <param name="lstReqHeader">If you need to include any additional key:value pairs in the header, add them to this list.</param>
        /// <returns></returns>
        public string ExecuteAPICall(string apiURL,
            ReqVerb reqType,
            string dataPayload,
            Boolean blIncludeAccessTokenInHeader = true,
            string contentType = "application/x-www-form-urlencoded",
            List<string> lstReqHeader = null)
        {
            HttpWebRequest req;
            HttpWebResponse res;
            try
            {
                string verb = "";
                switch (reqType)
                {
                    case ReqVerb.DELETE:
                        verb = "DELETE";
                        break;
                    case ReqVerb.GET:
                        verb = "GET";
                        break;
                    case ReqVerb.POST:
                        verb = "POST";
                        break;
                    case ReqVerb.PUT:
                        verb = "PUT";
                        break;
                }
                string strReq = apiURL;
                req = (HttpWebRequest)WebRequest.Create(apiURL);
                //Add the data payload to the request:
                req.Method = verb;
                req.KeepAlive = true;

                //add data to the request, if needed
                if (dataPayload != "")
                {
                    req.ContentType = contentType;

                    switch (contentType)
                    {
                        case "application/x-www-form-urlencoded":
                            byte[] byteData = Encoding.UTF8.GetBytes(dataPayload);
                            using (Stream reqStream = req.GetRequestStream())
                            {
                                reqStream.Write(byteData, 0, byteData.Length);
                                reqStream.Close();
                            }
                            break;
                    }

                }

                //Add token header, if required
                if (blIncludeAccessTokenInHeader && BlOAuth2)
                {
                    req.Headers.Add("Authorization: Bearer " + BoxUserOAuth2AccessToken);
                }
                //Add additional headers, if required
                if (lstReqHeader != null)
                {
                    for (int i = 0; i < lstReqHeader.Count; i++)
                    {
                        req.Headers.Add(lstReqHeader[i]);
                    }
                }

                res = (HttpWebResponse)req.GetResponse();
                using (Stream webStream = res.GetResponseStream())
                {
                    StreamReader webStreamReader = new StreamReader(webStream);
                    return webStreamReader.ReadToEnd();
                }

            }
            catch (Exception ex)
            {
                ErrMsg = ex.Message + System.Environment.NewLine + ex.StackTrace;
                return ErrMsg;
            }
        }

        /// <summary>
        /// Saves Authorization/Refresh token values.
        /// This should be called after the tokens have been intialized or refreshed.
        /// Derived class can override this method to save tokens however it wants (e.g.,
        /// save to database).  Base implementation writes tokens to the file specified by
        /// property OAuth2TokenFileLocation.
        /// </summary>
        /// <returns></returns>
        public virtual Boolean SaveTokens()
        {
            try
            {
                if (BlOAuth2)
                {
                    if (OAuth2TokenFileLocation != "")
                    {
                        using (StreamWriter sW = new StreamWriter(OAuth2TokenFileLocation, false))
                        {
                            sW.WriteLine(BoxUserOAuth2AccessToken);
                            sW.WriteLine(BoxUserOAuth2RefreshToken);
                        }
                    }
                    else
                    {
                        //Code for saving to a database goes here.
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.Message + System.Environment.NewLine + ex.StackTrace;
                return false;
            }
        }

        /// <summary>
        /// Gets information about an item (file or folder)
        /// </summary>
        /// <param name="itemType">Type of item (enumeration value for file or folder)</param>
        /// <param name="folderID">The item ID</param>
        /// <param name="csvFields">Response fields from the server in which you are interested.  Use this to lessen the data to parse.</param>
        /// <returns></returns>
        public string JSON_Item_GetInfo(BoxEnums.ObjectType itemType, Int64 itemID, string csvFields = "")
        {
            try
            {
                string pcsvFields = generateQueryParameter("fields", csvFields);
                string qParams = pcsvFields;
                if (qParams.Length > 0)
                {
                    qParams = "?" + qParams.Substring(1);
                }
                string apiResource = m_boxURI_Folder + "/" + itemID + qParams;
                if (itemType == BoxEnums.ObjectType.FILE)
                {
                    apiResource = m_boxURI_File + "/" + itemID + qParams;
                }
                string boxResp = ExecuteAPICall(apiResource,
                                                ReqVerb.GET,
                                                "",
                                                true);
                switch (CheckJSONResult(boxResp))
                {
                    case -1:
                        return "";
                    case -2:
                        return JSON_Item_GetInfo(itemType, itemID, csvFields);
                    case -3:
                        return m_errMsg;
                    default:
                        return boxResp;
                }

            }
            catch (Exception ex)
            {
                return ex.Message + Environment.NewLine + ex.StackTrace;
            }
            finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }

        /// <summary>
        /// Updates a file/folder object
        /// </summary>
        /// <param name="itemType">The type of object (file or folder) to update</param>
        /// <param name="itemID">ID of the item to update</param>
        /// <param name="newName">New name for the item</param>
        /// <param name="description">Description for the item</param>
        /// <param name="newParentID">New (folder) Parent ID; use this to move a file/folder to a new loction</param>
        /// <param name="sharedLinkAccess">Level of access; values 'open' or 'company', or 'collaborators'</param>
        /// <param name="sharedLinkPassword">Password required to access shared link</param>
        /// <param name="sharedLinkUnsharedAt">Date when shared link will be unshared</param>
        /// <param name="sharedLinkPermissionsCanDownload">Default is TRUE.  Only valid if sharedLinkAccess is
        /// 'open' or 'company'</param>
        /// <param name="ownedByUserID">ONly valid for folders.  Per API, this should be set to the USER ID
        /// (does this mean we can't change owner?)</param>
        /// <param name="syncState">Only Applies to Folder Items.  Not implemented yet.  Indicates whether box 
        /// clients should sync ('synced') or not sync ('not_synced') with this folder</param>
        /// <param name="tags">Array of tags to apply to this item.  NOTE: this will replace all existing tags</param>
        /// <param name="csvFields"></param>
        /// <returns></returns>
        public string JSON_Item_Update(BoxEnums.ObjectType itemType, Int64 itemID, 
            string newName = null, string description = null, string newParentID = null, 
            string sharedLinkAccess = null,
            string sharedLinkPassword = null, string sharedLinkUnsharedAt = null, 
            Boolean sharedLinkPermissionsCanDownload = true, 
            string ownedByUserID = null, string syncState = null, 
            string[] tags = null, string csvFields = "")
        {
            try
            {
                //Clean up-- if the user set name "", they really mean null (ignore-- you can't name an item an empty string)
                if (newName == "")
                {
                    newName = null;
                }
                //if (description == "")
                //{
                //    description = null;
                //}
                //Query Parameters
                string pcsvFields = generateQueryParameter("fields", csvFields);
                string qParams = pcsvFields;
                if (qParams.Length > 0)
                {
                    qParams = "?" + qParams.Substring(1);
                }

                //Api-- for files or folders?
                string apiResource = m_boxURI_Folder + "/" + itemID + qParams;
                if (itemType == BoxEnums.ObjectType.FILE)
                {
                    apiResource = m_boxURI_File + "/" + itemID + qParams;
                }

                //Need the payload for an item object
                //According to the API, the concept of updating the owner only applies to folders
                BoxUser mu = null;
                if (ownedByUserID != null)
                {
                    mu = new BoxUser(ownedByUserID);
                }
                SharedLink sl = null;
                if (sharedLinkAccess!=null || sharedLinkPassword!=null || sharedLinkUnsharedAt !=null || !sharedLinkPermissionsCanDownload)
                {
                    sl = new SharedLink(sharedLinkAccess, sharedLinkPassword, sharedLinkUnsharedAt,
                        new BoxPermissions(sharedLinkPermissionsCanDownload));
                }
                Item_FileFolder itm = new Item_FileFolder(newName, description,
                    new Parent(newParentID),
                    sl,
                    tags,
                    mu,
                    syncState);

                string data = JsonConvert.SerializeObject(itm, JSS);
                string boxResp = ExecuteAPICall(apiResource,
                                                ReqVerb.PUT,
                                                data,
                                                true);

                switch (CheckJSONResult(boxResp))
                {
                    case -1:
                        return "";
                    case -2:
                        return JSON_Item_Update(itemType, itemID, newName, description, newParentID,
                                    sharedLinkAccess, sharedLinkPassword, sharedLinkUnsharedAt,
                                    sharedLinkPermissionsCanDownload, ownedByUserID, syncState,
                                    tags);
                    case -3:
                        return m_errMsg;
                    default:
                        return boxResp;
                }

            }
            catch (Exception ex)
            {
                return ex.Message + Environment.NewLine + ex.StackTrace;
            }
            finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }

        /// <summary>
        /// Returns the JSON string obtained by querying the Box API about a given folder ID. JSON string should contain all
        /// files/folders in the specified folder ID.
        /// </summary>
        /// <param name="folderID">The folder ID</param>
        /// <param name="csvFields">Response fields from the server in which you are interested.  Use this to lessen the data to parse.</param>
        /// <param name="marker">Position marker at which to begin response</param>
        /// <param name="offset">Offset of item at which to begin response</param>
        /// <param name="limit">Max number of items to return; if unspecified, value is 100; maximum is 1000</param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public string JSON_Folder_GetItems(Int64 folderID, string csvFields = "", int marker = -1, int offset = -1, int limit = -1,
            string folderName = "")
        {
            try
            {
                string pcsvFields = generateQueryParameter("fields", csvFields);
                string pmarker = generateQueryParameter("marker", marker);
                string poffset = generateQueryParameter("offset", offset, -1);
                string plimit = generateQueryParameter("limit", limit, -1);

                string qParams = pcsvFields + pmarker + poffset + plimit;
                if (qParams.Length > 0)
                {
                    qParams = "?" + qParams.Substring(1);
                }
                string apiResource = m_boxURI_Folder + "/" + folderID + "/items" + qParams;
                string boxResp = ExecuteAPICall(apiResource,
                                                ReqVerb.GET,
                                                "",
                                                true);
                //If successful, we should have total_count value in the response
                int n = GetResultCount(boxResp);
                switch (n)
                {
                    case -1:
                        return "";
                    case -2:
                        return JSON_Folder_GetItems(folderID, csvFields, marker, offset, limit, folderName);
                    case -3:
                        return m_errMsg;
                    default:
                        return boxResp;
                }

            }
            catch (Exception ex)
            {
                return ex.Message + Environment.NewLine + ex.StackTrace;
            }
            finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }




        /// <summary>
        /// Create a folder in box
        /// </summary>
        /// <param name="parentFolderID">ID of the parent folder</param>
        /// <param name="folderName">Name of the folder to create</param>
        /// <param name="csvFields">Fields to return in response.
        /// Common fields: id,shared_link (id give you the id of the created folder, shared_link object has url, which
        /// is the url to access the folder)</param>
        /// <returns></returns>
        public string JSON_Folder_Create(Int64 parentFolderID, string folderName, 
            ref Int64 createdFolder_ID, ref string createdFolder_URL, string csvFields = "")
        {
            string boxResp = "";
            try
            {
                string pcsvFields = generateQueryParameter("fields", csvFields);

                string qParams = pcsvFields;
                if (qParams.Length > 0)
                {
                    qParams = "?" + qParams.Substring(1);
                }
                //We need to generate JSON for the data payload
                NewFolder nf = new NewFolder(folderName, new Parent(parentFolderID.ToString()));
                string data = JsonConvert.SerializeObject(nf);
                string apiResource = m_boxURI_Folder + qParams;
                boxResp = ExecuteAPICall(apiResource,
                                                ReqVerb.POST,
                                                data,
                                                true);
                //If successful, we should be able to create a BoxFolderObject
                switch(CheckJSONResult(boxResp))
                {
                    case -1:
                       return "";
                    case -2:
                        return JSON_Folder_Create(parentFolderID, folderName,
                               ref createdFolder_ID, ref createdFolder_URL, csvFields);
                    case -3:
                        return m_errMsg;
                    default:
                        try
                        {
                            JObject jO = JObject.Parse(boxResp);
                            Int64.TryParse(jO["id"].ToString(), out createdFolder_ID);
                            createdFolder_URL = jO["shared_link"]["url"].ToString();
                            return boxResp;
                        } catch (Exception)
                        {
                            //Most likely, this means there is no shared link
                            return boxResp;
                        }

                }
            }
            catch (Exception ex)
            {
                return ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + boxResp;
            }
            finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }

        /// <summary>
        /// Creates/Updates a shared link on an item
        /// </summary>
        /// <param name="itemType">The type of object (file or folder) to update</param>
        /// <param name="itemID">ID of the item to update</param>
        /// <param name="sharedLinkAccess">Level of access; values 'open' or 'company', or 'collaborators'</param>
        /// <param name="sharedLinkPassword">Password required to access shared link</param>
        /// <param name="sharedLinkUnsharedAt">Date when shared link will be unshared</param>
        /// <param name="sharedLinkPermissionsCanDownload">Default is TRUE.  Only valid if sharedLinkAccess is
        /// 'open' or 'company'</param>
        /// <param name="csvFields">Fields to return in response.  This is set to 'shared_link' by default
        /// so that the link is available in the returned json string</param>
        /// <returns></returns>
        public string JSON_SharedLink_CreateUpdate(BoxEnums.ObjectType itemType, Int64 itemID,
            BoxEnums.SharedLinkAccess sharedLinkAccess,
            string sharedLinkPassword = null, string sharedLinkUnsharedAt = null,
            Boolean sharedLinkPermissionsCanDownload = true,
            string csvFields="shared_link")
        {
            try
            {
                string slA = BoxEnums.DecodeBoxSharedLinkAccess(sharedLinkAccess);
                //Query Parameters
                string pcsvFields = generateQueryParameter("fields", csvFields);
                string qParams = pcsvFields;
                if (qParams.Length > 0)
                {
                    qParams = "?" + qParams.Substring(1);
                }

                //Api-- for files or folders?
                string apiResource = m_boxURI_Folder + "/" + itemID + qParams;
                if (itemType == BoxEnums.ObjectType.FILE)
                {
                    apiResource = m_boxURI_File + "/" + itemID + qParams;
                }

                //Need the payload for an item object
                //According to the API, the concept of updating the owner only applies to folders
                SharedLink sl = null;
                sl = new SharedLink(slA, sharedLinkPassword, sharedLinkUnsharedAt,
                    new BoxPermissions(sharedLinkPermissionsCanDownload));

                Item_FileFolder itm = new Item_FileFolder();
                itm.shared_link = sl;

                string data = JsonConvert.SerializeObject(itm, JSS);
                string boxResp = ExecuteAPICall(apiResource,
                                                ReqVerb.PUT,
                                                data,
                                                true);

                switch (CheckJSONResult(boxResp))
                {
                    case -1:
                        return "";
                    case -2:
                        return JSON_SharedLink_CreateUpdate(itemType, itemID,
                                    sharedLinkAccess, sharedLinkPassword, sharedLinkUnsharedAt,
                                    sharedLinkPermissionsCanDownload, csvFields);
                    case -3:
                        return m_errMsg;
                    default:
                        return boxResp;
                }

            }
            catch (Exception ex)
            {
                return ex.Message + Environment.NewLine + ex.StackTrace;
            }
            finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }


        /// <summary>
        /// Get shared link json for an item.
        /// Note this just a wrapper for JSON_Item_GetInfo
        /// </summary>
        /// <param name="itemType">The type of object (file or folder) to update</param>
        /// <param name="itemID">ID of the item to update</param>
        /// <param name="csvFields">Fields to return in response.  This is set to 'shared_link' by default
        /// so that the link is available in the returned json string</param>
        /// <returns></returns>
        public string JSON_SharedLink_Get(BoxEnums.ObjectType itemType, Int64 itemID,
            string csvFields = "shared_link")
        {
            try
            {
                //Query Parameters
                string pcsvFields = generateQueryParameter("fields", csvFields);
                string qParams = pcsvFields;
                if (qParams.Length > 0)
                {
                    qParams = "?" + qParams.Substring(1);
                }

                //Api-- for files or folders?
                string apiResource = m_boxURI_Folder + "/" + itemID + qParams;
                if (itemType == BoxEnums.ObjectType.FILE)
                {
                    apiResource = m_boxURI_File + "/" + itemID + qParams;
                }
                
                string boxResp = ExecuteAPICall(apiResource,
                                                ReqVerb.GET,
                                                "",
                                                true);

                switch (CheckJSONResult(boxResp))
                {
                    case -1:
                        return "";
                    case -2:
                        return JSON_SharedLink_Get(itemType, itemID, csvFields);
                    case -3:
                        return m_errMsg;
                    default:
                        return boxResp;
                }

            }
            catch (Exception ex)
            {
                return ex.Message + Environment.NewLine + ex.StackTrace;
            }
            finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }

        /// <summary>
        /// Deletes a shared link on an item
        /// </summary>
        /// <param name="itemType">The type of object (file or folder) from which the shared link is deleted</param>
        /// <param name="itemID">ID of the item to delete</param>
        /// <param name="csvFields">select the fields of the json object you want returned.  Default is
        /// "shared_link"; set to "" to obtain all info about the shared link after deletion</param>
        /// <returns></returns>
        public string JSON_SharedLink_Delete(BoxEnums.ObjectType itemType, Int64 itemID, 
            string csvFields = "shared_link")
        {
            try
            {
                //Query Parameters
                string pcsvFields = generateQueryParameter("fields", "shared_link");
                string qParams = pcsvFields;
                if (qParams.Length > 0)
                {
                    qParams = "?" + qParams.Substring(1);
                }

                //Deletion requires setting the shared_link object ONLY to null;
                //generate a custom JSON string for this
                string data = "{" +
                                "\"" + "type" + "\"" + ":" + "\"" + "folder" + "\"," +
                                "\"" + "id" + "\"" + ":" + "\"" + itemID.ToString() + "\"," +
                                "\"" + "shared_link" + "\"" + ":" + "null" +
                                "}";
                //Api-- for files or folders?
                string apiResource = m_boxURI_Folder + "/" + itemID + qParams;
                if (itemType == BoxEnums.ObjectType.FILE)
                {
                    apiResource = m_boxURI_File + "/" + itemID + qParams;
                    data = data.Replace("\"" + "folder" + "\",", "\"" + "file" + "\",");
                }

                string boxResp = ExecuteAPICall(apiResource,
                                                ReqVerb.PUT,
                                                data,
                                                true);

                switch (CheckJSONResult(boxResp))
                {
                    case -1:
                        return "";
                    case -2:
                        return JSON_SharedLink_Delete(itemType, itemID);
                    case -3:
                        return m_errMsg;
                    default:
                        return boxResp;
                }

            }
            catch (Exception ex)
            {
                return ex.Message + Environment.NewLine + ex.StackTrace;
            }
            finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }


        /// <summary>
        /// Removes the expiration date of the shared link for an item
        /// </summary>
        /// <param name="itemType">The type of object (file or folder) owning the shared link</param>
        /// <param name="itemID">ID of the item with the shared link</param>
        /// <returns></returns>
        public string JSON_SharedLink_RemoveExpirationDate(BoxEnums.ObjectType itemType, Int64 itemID,
            string csvFields = "shared_link")
        {
            try
            {
                //Query Parameters
                string pcsvFields = generateQueryParameter("fields", csvFields);
                string qParams = pcsvFields;
                if (qParams.Length > 0)
                {
                    qParams = "?" + qParams.Substring(1);
                }

                //Removing expiration date requires setting the unshared_at property of the
                //shared_link property ONLY to null;
                //generate a custom JSON string for this
                string data = "{" +
                                "\"" + "type" + "\"" + ":" + "\"" + "folder" + "\"," +
                                "\"" + "id" + "\"" + ":" + "\"" + itemID.ToString() + "\"," +
                                "\"" + "shared_link" + "\"" + ":" + 
                                    "{" + "\"" + "unshared_at" + "\"" + ":" + "null" + "}" + 
                                "}";
                //Api-- for files or folders?
                string apiResource = m_boxURI_Folder + "/" + itemID + qParams;
                if (itemType == BoxEnums.ObjectType.FILE)
                {
                    apiResource = m_boxURI_File + "/" + itemID + qParams;
                    data = data.Replace("\"" + "folder" + "\",", "\"" + "file" + "\",");
                }

                string boxResp = ExecuteAPICall(apiResource,
                                                ReqVerb.PUT,
                                                data,
                                                true);

                switch (CheckJSONResult(boxResp))
                {
                    case -1:
                        return "";
                    case -2:
                        return JSON_SharedLink_RemoveExpirationDate(itemType, itemID);
                    case -3:
                        return m_errMsg;
                    default:
                        return boxResp;
                }

            }
            catch (Exception ex)
            {
                return ex.Message + Environment.NewLine + ex.StackTrace;
            }
            finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }

        /// <summary>
        /// Download a file from BOX
        /// </summary>
        /// <param name="fileID">ID of the file to download</param>
        /// <param name="downloadPathAndName">Full path and name where the file will be saved</param>
        /// <param name="deleteIfExists">Leave TRUE to delete the file downloadPathAndName already exists</param>
        /// <param name="version">Option to delete an old version of the file</param>
        /// <returns></returns>
        public string File_Download(Int64 fileID, string downloadPathAndName, 
            Boolean deleteIfExists = true, string version = "")
        {
            string boxResp = "";
            try
            {

                string pversion = generateQueryParameter("version", version);

                string qParams = pversion;
                if (qParams.Length > 0)
                {
                    qParams = "?" + qParams.Substring(1);
                }
                //We need to generate the API
                string apiResource = m_boxURI_File + "/" + fileID.ToString() + "/content" + qParams;
                //If successful, we should see a 302 and a URL (??)
                //Don't do the following: it appears calling the API does an automatic redirect and starts
                //giving you the file data.  Use the WebClient's download method
                //boxResp = ExecuteAPICall(apiResource,
                //                        ReqVerb.GET,
                //                        "",
                //                        true);
                WebClient req = new WebClient();
                req.Headers.Add("Authorization: Bearer " + BoxUserOAuth2AccessToken);
                req.DownloadFile(apiResource, downloadPathAndName);
                if (File.Exists(downloadPathAndName)) {
                    return "true";
                } else
                {
                    return "false";
                }
            }
            catch (Exception ex)
            {
                return ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + boxResp;
            }
            finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }

        /// <summary>
        /// Delete a folder from BOX
        /// </summary>
        /// <param name="folderIDToDelete">ID of the target folder</param>
        /// <param name="blSuccessful">This REF variable is set TRUE if we detect a 204 response,
        /// indicating empty.</param>
        /// <param name="recursive">//Leave TRUE to delete even if it contains items</param>
        /// <returns></returns>
        public string JSON_Folder_Delete(Int64 folderIDToDelete, ref Boolean blSuccessful, Boolean recursive = true)
        {
            string boxResp = "";
            try
            {

                string precursive = generateQueryParameter("recursive", recursive).ToLower();

                string qParams = precursive;
                if (qParams.Length > 0)
                {
                    qParams = "?" + qParams.Substring(1);
                }
                //We need to generate JSON for the data payload
                string apiResource = m_boxURI_Folder + "/" + folderIDToDelete.ToString() + qParams;
                boxResp = ExecuteAPICall(apiResource,
                                                ReqVerb.DELETE,
                                                "",
                                                true);
                //If successful, we should see 204 in the response
                if (boxResp=="")
                {
                    blSuccessful = true;                    
                } else
                {
                    switch (CheckJSONResult(boxResp))
                    {
                        case -1:
                            return "";
                        case -2:
                            return JSON_Folder_Delete(folderIDToDelete, ref blSuccessful, recursive);
                        case -3:
                            return m_errMsg;                          
                    }
                }
                return boxResp;
            }
            catch (Exception ex)
            {
                return ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + boxResp;
            }
            finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }

        /// <summary>
        /// Uploads a file to box.
        /// </summary>
        /// <param name="folderID">The folder ID</param>
        /// <param name="pathAndNameOfFile">path and name of file we are uploading</param>
        /// <returns></returns>
        public string JSON_File_Upload(Int64 folderID, string pathAndNameOfFile)
        {
            try
            {
                string fileName = pathAndNameOfFile.Split('\\')[pathAndNameOfFile.Split('\\').GetUpperBound(0)];
                Item_FileFolder f = new Item_FileFolder(fileName, folderID);
                string data = JsonConvert.SerializeObject(f, JSS);

                HttpContent fileAttributes = new StringContent(data);
                byte[] fileBytes = File.ReadAllBytes(pathAndNameOfFile);
                HttpContent fileData = new ByteArrayContent(fileBytes);

                using (var client = new HttpClient())
                {
                    //Add the authorization header to the client
                    /*
                     Problem APPEARS to be related to the header;  I keep getting 'Bad Request'.  I
                     see this in curl if I intentionally mis-shape the JSON, but in this case, it may
                     also be I am using the incorrect encoding/media type.  Need to figure out how to get
                     more information about the error than just 'bad request' (in curl, I could see the
                     full error message; here, the request object immediately errors out at 404, so I
                     can't get my hands to intercept the message).
                     */
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + BoxUserOAuth2AccessToken);
                    using (var formData = new MultipartFormDataContent())
                    {
                        //Add the json string (I don't know-- as a stream?  string didn't work)
                        formData.Add(new StringContent(data),"attributes");
                        //byte[] byteData = Encoding.UTF8.GetBytes(data);
                        //using (MemoryStream reqStream = new MemoryStream(byteData))
                        //{
                        //    HttpContent streamContent = new StreamContent(reqStream);
                        //    formData.Add(streamContent,"attributes");
                        //    reqStream.Close();
                        //}

                        //Add to the byte array:
                        formData.Add(fileData,"file");

                        //The following actually sends the response to the server
                        var response = client.PostAsync(m_boxURI_File_Upload, formData).Result;


                        if (!response.IsSuccessStatusCode)
                        {
                            //Failure occured
                            return response.StatusCode + Environment.NewLine + response.ReasonPhrase + formData.ToString();
                        } else
                        {
                            return response.Content.ReadAsStringAsync().Result;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message + Environment.NewLine + ex.StackTrace;
            }
            finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }

        /// <summary>
        /// Uploads a file to box using curl.
        /// </summary>
        /// <param name="folderID">The folder ID</param>
        /// <param name="pathAndNameOfFile">location/file we are uploading</param>
        /// <returns></returns>
        public string JSON_File_Upload_Curl(Int64 folderID, string pathAndNameOfFile)
        {
            try
            {
                string fileName = pathAndNameOfFile.Split('\\')[pathAndNameOfFile.Split('\\').GetUpperBound(0)];
                Item_FileFolder f = new Item_FileFolder(fileName, folderID);
                string data = JsonConvert.SerializeObject(f, JSS);
                //Since we will be executing this on the command line, it turns out that all the single
                //quotes need to get converted to TRIPLE quotes...
                data = data.Replace("\"", "\"" + "\"" + "\"");
                //.. and then we want the entire object encased in double quotes (single quotes don't work
                //in command line)
                data = "\"" + data.Replace("'", "") + "\"";

                //We need to construct our authorization header (note the double quotes, for cmd line purposes)
                string header = "\""+ "Authorization: Bearer " + BoxUserOAuth2AccessToken + "\"";

                //Now, construct the full curl string; the first argument is the api resource:
                string args = m_boxURI_File_Upload + "/content " +
                              "-H " + header + " " +
                              "-X POST " +
                              "-F attributes=" + data + " " +
                              "-F file=@" + pathAndNameOfFile;

                //Run these args through curl
                Curl c = new Curl(m_curlFullPath);
                string boxResp = c.Execute(args);
                switch (CheckJSONResult(boxResp))
                {
                    case -1:
                        return "";
                    case -2:
                        return JSON_File_Upload_Curl(folderID, pathAndNameOfFile);
                    case -3:
                        return m_errMsg;
                }
                return boxResp;

            }
            catch (Exception ex)
            {
                return ex.Message + Environment.NewLine + ex.StackTrace;
            }
            finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }

        /// <summary>
        /// Uploads a new VERSION of a file to box using curl.
        /// </summary>
        /// <param name="fileID">The ID of the file to be updated</param>
        /// <param name="pathAndNameOfFile">location/file we are uploading</param>
        /// <returns></returns>
        public string JSON_File_Update_Curl(Int64 fileID, string pathAndNameOfFile)
        {
            try
            {
                //We need to construct our authorization header (note the double quotes, for cmd line purposes)
                string header = "\"" + "Authorization: Bearer " + BoxUserOAuth2AccessToken + "\"";

                //Now, construct the full curl string; the first argument is the api resource:
                string args = m_boxURI_File_Upload + "/" + fileID.ToString() + "/content " +
                              "-H " + header + " " +
                              "-X POST " +
                              "-F file=@" + pathAndNameOfFile;

                //Run these args through curl
                Curl c = new Curl(m_curlFullPath);
                string boxResp = c.Execute(args);
                switch (CheckJSONResult(boxResp))
                {
                    case -1:
                        return "";
                    case -2:
                        return JSON_File_Update_Curl(fileID, pathAndNameOfFile);
                    case -3:
                        return m_errMsg;
                }
                return boxResp;

            }
            catch (Exception ex)
            {
                return ex.Message + Environment.NewLine + ex.StackTrace;
            }
            finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }

        /// <summary>
        /// Returns JSON search results form box
        /// </summary>
        /// <param name="query">String for Box to match.  Per the API, Box matches against
        /// object names
        /// descriptions
        /// text contents of files
        /// other data (!)</param>
        /// <param name="type">What to include in search results.  Use member of this class's BoxContentType enumeration</param>
        /// <param name="csvFields">CSV list of fields you want included in the json result</param>
        /// <param name="limit">Upper bound on number of results to return</param>
        /// <param name="offset">Offset value from which to return query (for pagination calls)</param>
        /// <param name="csvFileExtensions">CSV list of file extensions to match (when searching files)</param>
        /// <param name="scope">Possible values:
        /// user_content (default)
        /// enterprise_content (require permission from box admin)</param>
        /// <param name="createdAtRange">possibleDateCreated_start,possibleDateCreated_End
        /// If specified, you can leave field blank, but COMMA IS REQUIRED</param>
        /// <param name="updatedAtRange">possibleDateUpdated_start,possibleDateUpdated_End
        /// If specified, you can leave field blank, but COMMA IS REQUIRED</param>
        /// <param name="sizeRange">Size range, in bytes.  1MB == 1000000.  You can leave upper/lower bound specified, but COMMA IS REQUIRED</param>
        /// <param name="csvOwnerIDs">csv list of Owners who must own the content in order to have it in the search results</param>
        /// <param name="csvAncestorFolderIDs">csv list of Box Folders IDs.  Box will only returns results found in these
        /// folders or subfolders.</param>
        /// <param name="csvContentTypes">csv list of Box Content Types
        /// Use this to determine the type of content Box will include in its search results.
        /// Possible values:
        /// name
        /// description
        /// file_content
        /// comments
        /// tags
        /// Be aware fo this class's BoxContentType enumeration and the associated DecodeBoxContent method.</param>
        /// <param name="trashContent">Tell box whether to search in user's trash.  Values:
        /// non_trashed_only (default)
        /// trashed only</param>
        /// <param name="mdFilters">meta-data filter.  See Box API.</param>
        /// <param name="mdFiltersScope">meta-data scope.  See Box API.</param>
        /// <param name="mdFiltersTemlplateKey">meta-data template.  See Box API.</param>
        /// <param name="mdFiltersFilters">meta-data filter filter.  See Box API.</param>
        /// <returns></returns>
        public string JSON_Search(string query,
                                  BoxEnums.ObjectType type = BoxEnums.ObjectType.UNSPECIFIED,
                                  string csvFields = "",
                                  int limit = -1,
                                  int offset = -1,
                                  string csvFileExtensions = "",
                                  string scope = "user_content",
                                  string createdAtRange = "",
                                  string updatedAtRange = "",
                                  string sizeRange = "",
                                  string csvOwnerIDs = "",
                                  string csvAncestorFolderIDs = "",
                                  string csvContentTypes = "",
                                  string trashContent = "",
                                  string mdFilters = "",
                                  string mdFiltersScope = "",
                                  string mdFiltersTemlplateKey = "",
                                  string mdFiltersFilters = "")
        {
            try
            {
                string pquery = generateQueryParameter("query", query);
                string ptype = generateQueryParameter("type", BoxEnums.DecodeBoxObjectType(type));
                string pcsvFields = generateQueryParameter("fields", csvFields);
                string plimit = generateQueryParameter("limit", limit, -1);
                string poffset = generateQueryParameter("offset", offset, -1);
                string pcsvFileExtensions = generateQueryParameter("file_extensions", csvFileExtensions); ;
                string pscope = generateQueryParameter("scope", scope);
                string pcreatedAtRange = generateQueryParameter("created_at_range", createdAtRange, blConvertTimeRange: true);
                string pupdatedAtRange = generateQueryParameter("updated_at_range", updatedAtRange, blConvertTimeRange: true);
                string psizeRange = generateQueryParameter("size_range", sizeRange);
                string pcsvOwnerIDs = generateQueryParameter("owner_user_ids", csvOwnerIDs);
                string pcsvAncestorFolderIDs = generateQueryParameter("ancestor_folder_ids", csvAncestorFolderIDs); ;
                string pcsvContentTypes = generateQueryParameter("content_types", csvContentTypes);
                string ptrashContent = generateQueryParameter("trash_content", trashContent);
                string pmdFilters = generateQueryParameter("mdFilters", mdFilters);
                string pmdFiltersScope = generateQueryParameter("mdfilters.scope", mdFiltersScope);
                string pmdFiltersTemlplateKey = generateQueryParameter("mdfilters.templateKey", mdFiltersTemlplateKey);
                string pmdFiltersFilters = generateQueryParameter("mdfilters.filters", mdFiltersFilters);

                string options = pquery + ptype + pcsvFields + plimit + poffset + pcsvFileExtensions + pscope + pcreatedAtRange +
                    pupdatedAtRange + psizeRange + pcsvOwnerIDs + pcsvAncestorFolderIDs + pcsvContentTypes + ptrashContent +
                    pmdFilters + pmdFiltersScope + pmdFiltersTemlplateKey + pmdFiltersFilters;
                if (options != "")
                {
                    options = "?" + options;
                }
                string apiResource = m_boxURI_Search + options;
                string boxResp = ExecuteAPICall(apiResource,
                                                ReqVerb.GET,
                                                "",
                                                true);
                //If successful, we should have total_count value in the response
                int n = GetResultCount(boxResp);
                switch (n)
                {
                    case -1:
                        return "";
                    case -2:
                        return JSON_Search(query,
                                  type,
                                  csvFields,
                                  limit,
                                  offset,
                                  csvFileExtensions,
                                  scope,
                                  createdAtRange,
                                  updatedAtRange,
                                  sizeRange,
                                  csvOwnerIDs,
                                  csvAncestorFolderIDs,
                                  csvContentTypes,
                                  trashContent,
                                  mdFilters,
                                  mdFiltersScope,
                                  mdFiltersTemlplateKey,
                                  mdFiltersFilters);
                    case -3:
                        return m_errMsg;
                    default:
                        return boxResp;
                }

            }
            catch (Exception ex)
            {
                return ex.Message + Environment.NewLine + ex.StackTrace;
            }
            finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }

        /// <summary>
        /// Adds a user as a collaborator to the given item.
        /// </summary>
        /// <param name="csvFields">CSV list of Box API Fields to return in the response.</param>
        /// <param name="notifyUsers">Set TRUE to let collaborator receive email updates about changes
        /// to the shared item.</param>
        /// <param name="itemType">Use enum for file or folder</param>
        /// <param name="itemID">Box ID of the item to be shared</param>
        /// <param name="collaboratorEmail">Email address of the collaborator</param>
        /// <param name="role">Permission level granted to the collaborator.</param>
        /// <returns></returns>
        public string JSON_Collaboration_Add(string csvFields, Boolean notifyUsers, 
            BoxEnums.ObjectType itemType, 
            Int64 itemID, string collaboratorEmail, BoxEnums.CollaboratorRole role)
        {
            string boxResp = "";
            try
            {


                string pcsvFields = generateQueryParameter("fields", csvFields);
                string pnotifyUsers = generateQueryParameter("notify", notifyUsers).ToLower();
                string qParams = pcsvFields;
                if (qParams.Length > 0)
                {
                    qParams = "?" + qParams.Substring(1);
                }

                //We need to generate JSON for the data payload
                Collaboration c = new Collaboration(
                    new BoxUser("user", login: collaboratorEmail),
                    new Item_FileFolder(BoxEnums.DecodeBoxObjectType(itemType), itemID.ToString()),
                    BoxEnums.DecodeBoxCollaboratorRole(role));

                string data = JsonConvert.SerializeObject(c, JSS);
                string apiResource = m_boxURI_Collaboration + qParams;
                boxResp = ExecuteAPICall(apiResource,
                                                ReqVerb.POST,
                                                data,
                                                true);
                //If successful, we should be able to create a BoxFolderObject
                switch (CheckJSONResult(boxResp))
                {
                    case -1:
                        return "";
                    case -2:
                        return JSON_Collaboration_Add(csvFields, notifyUsers, itemType, itemID,
                            collaboratorEmail, role);
                    case -3:
                        return m_errMsg;
                    default:
                        return boxResp;
                }
            }
            catch (Exception ex)
            {
                return ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + boxResp;
            }
            finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }

        /// <summary>
        /// Returns a JSON string of all collaborators for a given file/folder
        /// </summary>
        /// <param name="csvFields">CSV list of Box API Fields to return in the response.</param>
        /// <param name="itemType">Use enum for file or folder</param>
        /// <param name="itemID">Box ID of the subject item</param>
        /// <returns></returns>
        public string JSON_Collaboration_Get(string csvFields, BoxEnums.ObjectType itemType,
            Int64 itemID)
        {
            string boxResp = "";
            try
            {
                string pcsvFields = generateQueryParameter("fields", csvFields);
                string qParams = pcsvFields;
                if (qParams.Length > 0)
                {
                    qParams = "?" + qParams.Substring(1);
                }

                string apiResource = m_boxURI_Folder;
                if (itemType == BoxEnums.ObjectType.FILE)
                {
                    apiResource = m_boxURI_File;
                }
                apiResource = apiResource + "/" + itemID.ToString() + "/collaborations" + qParams;
                boxResp = ExecuteAPICall(apiResource,
                                                ReqVerb.GET,
                                                "",
                                                true);
                //If successful, we should be able to create a Collaboration object
                switch (CheckJSONResult(boxResp))
                {
                    case -1:
                        return "";
                    case -2:
                        return JSON_Collaboration_Get(csvFields, itemType, itemID);
                    case -3:
                        return m_errMsg;
                    default:
                        return boxResp;
                }
            }
            catch (Exception ex)
            {
                return ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + boxResp;
            }
            finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }

        /// <summary>
        /// Removes a user as a collaborator by the Collaboration ID.
        /// NOTE: Since each collaboration assignment is unique to a file/folder, the
        /// collaborationID is sufficient to remove the user's access.
        /// </summary>
        /// <param name="collaborationID">use the JSON_Collaboration_Get method to get the 
        /// collaborationID of a specific user on a given item</param>
        /// <param name="blSuccessful">This gets sets TRUE if the deletion was successful.
        /// If TRUE, then this method's return value is "".</param>
        /// <returns></returns>
        public string JSON_Collaboration_Delete(string collaborationID, ref Boolean blSuccessful)
        {
            string boxResp = "";
            try
            {
                string apiResource = m_boxURI_Collaboration +"/" + collaborationID;
                boxResp = ExecuteAPICall(apiResource,
                                                ReqVerb.DELETE,
                                                "",
                                                true);
                //If successful, we should see 204 in the response
                if (boxResp == "")
                {
                    blSuccessful = true;
                }
                else
                {
                    blSuccessful = false;
                    switch (CheckJSONResult(boxResp))
                    {
                        case -1:
                            return "";
                        case -2:
                            return JSON_Collaboration_Delete(collaborationID, ref blSuccessful);
                        case -3:
                            return m_errMsg;
                    }
                }
                return boxResp;
            }
            catch (Exception ex)
            {
                return ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + boxResp;
            }
            finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }

        /// <summary>
        /// Removes a user as a collaborator by the Collaboration ID.
        /// NOTE: Since each collaboration assignment is unique to a file/folder, the
        /// collaborationID is sufficient to remove the user's access.
        /// </summary>
        /// <param name="collaborationID">use the JSON_Collaboration_Get method to get the 
        /// collaborationID of a specific user on a given item</param>
        /// <param name="role">The new role for this collaborator.</param>
        /// <returns></returns>
        public string JSON_Collaboration_Modify(string collaborationID, BoxEnums.CollaboratorRole role)
        {
            string boxResp = "";
            try
            {
                string apiResource = m_boxURI_Collaboration + "/" + collaborationID;
                Collaboration c = new Collaboration();
                c.role = BoxEnums.DecodeBoxCollaboratorRole(role);
                //For payload, we're just chanign the role, so we generate JSON ourself
                string data = JsonConvert.SerializeObject(c, JSS);
                boxResp = ExecuteAPICall(apiResource,
                                                ReqVerb.PUT,
                                                data,
                                                true);
                //If successful, we should see 204 in the response
                switch (CheckJSONResult(boxResp))
                {
                    case -1:
                        return "";
                    case -2:
                        return JSON_Collaboration_Modify(collaborationID, role);
                    case -3:
                        return m_errMsg;
                }

                return boxResp;
            }
            catch (Exception ex)
            {
                return ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + boxResp;
            }
            finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }


        /// <summary>
        /// Returns a list of all tags for a folder.
        /// Use ref variable error to see if an error occurred
        /// </summary>
        /// <param name="folderID"></param>
        /// <returns></returns>
        public List<string> GetFolderTags(Int64 folderID, ref Boolean errOccurred, ref Boolean OAuth2Required)
        {
            try
            {
                errOccurred = false;
                List<string> result = new List<string>();
                string x = "";
                //Get the tags for the folder
                string jsonResp = JSON_Item_GetInfo(BoxEnums.ObjectType.FOLDER, folderID, "tags");
                switch(CheckJSONResult(jsonResp)) { 
                    case -1:
                        errOccurred = true;
                        OAuth2Required = true;
                        return result;
                    case -2:
                        errOccurred = false;
                        return GetFolderTags(folderID, ref errOccurred, ref OAuth2Required);
                    case -3:
                        errOccurred = true;
                        return result;
                    default:
                        JObject jO = JObject.Parse(jsonResp);
                        JArray jA = JArray.Parse(jO["tags"].ToString());
                        for(int i = 0; i< jA.Count();i++) {
                            result.Add(jA[i].ToString());
                        }
                        return result;
                }
            } catch(Exception ex)
            {
                m_errMsg = ex.Message + Environment.NewLine + ex.StackTrace;
                //Return an empty list
                errOccurred = true;
                return new List<string>();
            } finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }

        /// <summary>
        /// Adds tags to the folderID and returns the complete set of tags
        /// </summary>
        /// <param name="folderID"></param>
        /// <param name="newTags"></param>
        /// <param name="errOccurred"></param>
        /// <param name="OAuth2Required"></param>
        /// <returns></returns>
        public List<string> AddFolderTags(Int64 folderID, 
            List<string> newTags,
            ref Boolean errOccurred, 
            ref Boolean OAuth2Required)
        {
            try
            {
                //Get the old tags
                List<string> lstOldTags = GetFolderTags(folderID, ref errOccurred, ref OAuth2Required);
                //Add new tags to Old tags if they don't exist yet
                for (int i = 0; i < lstOldTags.Count-1; i++)
                {
                    if (!newTags.Contains(lstOldTags[i]))
                    {
                        newTags.Add(lstOldTags[i]);
                    }
                }
                if (newTags.Count > 0)
                {
                    TagQueryResult tqr = new TagQueryResult();
                    tqr.tags = newTags.ToArray();
                    string boxResp = ExecuteAPICall(m_boxURI_Folder + "/" + folderID.ToString(),
                        ReqVerb.PUT,
                        JsonConvert.SerializeObject(tqr));
                    switch (CheckJSONResult(boxResp))
                    {
                        case -1:
                            errOccurred = true;
                            OAuth2Required = true;
                            return new List<string>();
                        case -2:
                            errOccurred = false;
                            return AddFolderTags(folderID, newTags, ref errOccurred, ref OAuth2Required);
                        case -3:
                            errOccurred = true;
                            return new List<string>();
                        default:
                            return GetFolderTags(folderID, ref errOccurred, ref OAuth2Required);
                    }
                } else
                {
                    return new List<string>();
                }
            }
            catch (Exception ex)
            {
                m_errMsg = ex.Message + Environment.NewLine + ex.StackTrace;
                //Return an empty list
                errOccurred = true;
                return new List<string>();
            } finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }

        /// <summary>
        /// Removes tags from the folderID and returns the complete set of tags
        /// </summary>
        /// <param name="folderID"></param>
        /// <param name="removeTags"></param>
        /// <param name="errOccurred"></param>
        /// <param name="OAuth2Required"></param>
        /// <returns></returns>
        public List<string> RemoveFolderTags(Int64 folderID,
            List<string> removeTags,
            ref Boolean errOccurred,
            ref Boolean OAuth2Required)
        {
            try
            {
                //Get the old tags
                List<string> lstOldTags = GetFolderTags(folderID, ref errOccurred, ref OAuth2Required);
                //Remove old tags from list if they are in removeTags
                for (int i =  lstOldTags.Count - 1; i>=0; i--)
                {
                    if (removeTags.Contains(lstOldTags[i]))
                    {
                        lstOldTags.RemoveAt(i);
                    }
                }
                TagQueryResult tqr = new TagQueryResult();
                tqr.tags = lstOldTags.ToArray();
                string boxResp = ExecuteAPICall(m_boxURI_Folder + "/" + folderID.ToString(),
                    ReqVerb.PUT,
                    JsonConvert.SerializeObject(tqr));
                switch (CheckJSONResult(boxResp))
                {
                    case -1:
                        errOccurred = true;
                        OAuth2Required = true;
                        return new List<string>();
                    case -2:
                        errOccurred = false;
                        return AddFolderTags(folderID, lstOldTags, ref errOccurred, ref OAuth2Required);
                    case -3:
                        errOccurred = true;
                        return new List<string>();
                    default:
                        return GetFolderTags(folderID, ref errOccurred, ref OAuth2Required);
                }
            }
            catch (Exception ex)
            {
                m_errMsg = ex.Message + Environment.NewLine + ex.StackTrace;
                //Return an empty list
                errOccurred = true;
                return new List<string>();
            }
            finally
            {
                m_blAttemptedTokenRefresh = false;
            }
        }

        /// <summary>
        /// Parses the JSON string to determine if it's OK to use.  Possible return values:
        /// -1: Token expired; need to redirect user to re-authorize the tokens.
        /// -2: Token expired, but able to refresh ; call should recursively call itself to try again.
        /// -3: Error occurred.
        /// 0: No problem detected; return string
        /// </summary 
        /// <param name="jsonString"></param>
        /// <returns></returns>
        private int CheckJSONResult(string jsonString)
        {
            try
            {
                if (jsonString.ToLower().Contains("The remote server returned an error".ToLower()) || 
                    jsonString.ToLower().Contains("\"" + "error" + "\""))
                {


                    //Possibly, we got no result because the tokens expired.  Refresh and try again
                    if (m_blOAuth2 && !m_blAttemptedTokenRefresh)
                    {
                        //Check the json to see if it contains any error code strings requiring it to refresh itself
                        //Boolean needToTryRefresh = false;
                        //string lowerjson = jsonString.ToLower();
                        //if (lowerjson)
                        //{
                        //    needToTryRefresh = true;
                        //}
                        if ( !RefreshOAuth2Tokens())
                        {
                            //This means refresh didn't work.
                            //Return -1 to signal tokens have expired. need to redirect user
                            //to authorize app
                            return -1;
                        } else
                        {
                            //Return -2 telling the calling function to recursively call itself, now that we have refreshed the tokens.
                            return -2;
                        }

                    }
                    else
                    {
                        if (m_blOAuth2)
                        {
                            //this means we already attempted the token refresh, and got an error.
                            return -3;
                        } else
                        {
                            //Reserved for AppKey Auth
                            return -3;
                        }
                    }
                } else
                {
                    return 0;
                }
                
            }
            catch (Exception ex)
            {
                m_errMsg = ex.Message + Environment.NewLine + ex.StackTrace;
                return -3;
            }
        }
        /// <summary>
        /// Parses the JSON response and returns the total number of results.
        /// If you receive -1, then:
        /// 1) There are no results.
        /// 2) This method already tried to update the OAuth2 tokens, but failed.
        /// You should redirect the user to the Box Page to re-authorize the app.
        /// If you receive -2, then
        /// 1) There are no results (i.e., the 'total_count' field is not even in the JSON, so we don't if the answer is 0).
        /// 2) This method successfully refreshed the OAuth2 tokens.
        /// You should (re-cursively) re-attempt your API call to see if you get results.
        /// If you receive -3, then
        /// An error occurred.  The Error message is availabe in this object's ErrMsg property.
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        private int GetResultCount(string jsonString)
        {
            try
            {
                List<string> lst = new List<string>();
                lst.Add("total_count");
                Dictionary<string, string> d = ExtractParametersFromBoxResponse(jsonString, lst);
                if (d.Keys.Count == 0 && m_blOAuth2 && !m_blAttemptedTokenRefresh)
                {
                    //Possibly, we got no result because the tokens expired.  Refresh and try again
                    if (!RefreshOAuth2Tokens())
                    {
                        //This means refresh didn't work.
                        //Return -1 to signal tokens have expired. need to redirect user
                        //to authorize app
                        return -1;
                    }
                    else
                    {
                        //Return -2 telling the calling function to recursively call itself, now that we have refreshed the tokens.
                        return -2;
                    }
                }
                return int.Parse(d["total_count"]);
            }
            catch (Exception ex)
            {
                m_errMsg = ex.Message + Environment.NewLine + ex.StackTrace;
                return -3;
            }
        }

        /// <summary>
        /// Given a csv date range string:
        /// start_date,enddate
        /// ...returns a csv RFC339 encoded range string
        /// </summary>
        /// <param name="daterange">start,end.  OK to leave start/end blank; Box requires a command when processing the date</param>
        private string convertDateRangeToRFC339(string daterange)
        {
            try
            {
                string[] arrDates = daterange.Split(',');
                string start = arrDates[0];
                string end = arrDates[1];
                if (start != "")
                {
                    start = XmlConvert.ToDateTime(start, XmlDateTimeSerializationMode.Utc).ToString();
                }
                if (end != "")
                {
                    end = XmlConvert.ToDateTime(end, XmlDateTimeSerializationMode.Utc).ToString();
                }
                return start + "," + end;
            }
            catch (Exception ex)
            {
                m_errMsg = ex.Message + Environment.NewLine + ex.StackTrace;
                return ",";
            }
        }

        /// <summary>
        /// Returns a string formatted as 
        /// &parameterName=ParameterValue
        /// ...to be used in GET queries
        /// </summary>
        /// <param name="parameterName">Name of parameter</param>
        /// <param name="parameterValue">Value of parameter</param>
        /// <param name="blConvertTimeRange">Set TRUE if parameterValue is a time RANGE.
        /// This means it has a comma to separate start and end times (though either may be empty).  The function will
        /// convert it to RFC339 required by Box</param>
        /// <param name="valueThatReturnsBlank">Normally, if the parameterValue is "", then you don't want any query parameter returned.
        /// However, there are cases where you want a differet value to return blank.  For example, integer fielsds might use -1
        /// to signal the parameter should be ignored.  In that case, enter -1 in this field, and the parameter will be ignored.</param>
        private string generateQueryParameter(string parameterName, object parameterValue,
            object valueThatReturnsBlank = null, Boolean blConvertTimeRange = false)
        {
            try
            {
                string ignoreValue = "";
                if (valueThatReturnsBlank != null)
                {
                    ignoreValue = "-1";
                }
                if (parameterValue.ToString() == ignoreValue)
                {
                    return "";
                }
                else
                {
                    if (blConvertTimeRange)
                    {
                        return "&" + parameterName + "=" + convertDateRangeToRFC339(parameterValue.ToString());
                    }
                    else
                    {
                        return "&" + parameterName + "=" + HttpUtility.UrlEncode(parameterValue.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                m_errMsg = ex.Message + Environment.NewLine + ex.StackTrace;
                return "";
            }
        }

    }
}
