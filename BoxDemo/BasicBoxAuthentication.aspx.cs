using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Text;
using System.IO;

namespace BoxDemo
{
    /// <summary>
    /// This form shows how I initially understood OAuth2 identification.  Code in this page was refactored as the basis of the Box Class,
    /// which in turn is used for the demo functions in Main.aspx.
    /// </summary>
    public partial class BasicBoxAuthentication : System.Web.UI.Page
    {
        string m_boxAppClientID = "k10tic17v3d3i6gw6o7xb5det4b9n8h5";
        string m_boxAppClientSecret = "OfSRlm6gj3TgRxEkXgVgOkDI0eGYZNWw";
        string m_boxAppRedirectURI = "http://localhost:50180/Main.aspx";
        string m_boxUserOAuth2AccessToken = "";
        string m_boxUserOAuth2RefreshToken = "";
        string m_authFile = HttpContext.Current.Server.MapPath("files") + "\\auth.txt";
        string m_boxTokenExchangeURI = "https://api.box.com/oauth2/token";
        string BOXCLIENT = "boxclient";

        /// <summary>
        /// Step1 of OAuth2 Verification
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnInitialize_Click(object sender, EventArgs e)
        {
            //This is Step 1 of the OAuth2 3-legged verification process.
            //We contact the Box server at the given API link, sending it:
            //1) the client_id that was generated for our BoxApp; this tells Box which App the user is authorizing to act on his behalf
            //2) the redirect URI where box will send its response (an Authorization Code)
            Response.Redirect("https://account.box.com/api/oauth2/authorize?response_type=code&client_id=" + m_boxAppClientID +
                "&redirect_uri=" + HttpUtility.UrlEncode(m_boxAppRedirectURI));
            //Now, the user will sign in with his SSO credentials.  If everythin checks out, Box will sent a GET request to us;
            //this has a QUERY STRING, meaning the Page Load Method will be called.  Go there to see Step 2.
        }

        //Step 2 & 3 of OAuth2 Verification
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                //This page was provided as Box's call back; Box would have called it with a query string that includes 'code' parameter.
                if (Request.QueryString["code"] != null)
                {
                    //The authCode that box gave us:
                    string authCode = Request.QueryString["code"];

                    //The authCode is only good for 60 seconds.
                    //For Step 2, the server makes a POST request to Box
                    string strReq = m_boxTokenExchangeURI;
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(strReq);
                    //Add the data payload to the request:
                    req.Method = "POST";
                    req.KeepAlive = true;
                    req.ContentType = "application/x-www-form-urlencoded";

                    string payload = "grant_type=authorization_code" +
                                        "&code=" + authCode +
                                        "&client_id=" + m_boxAppClientID +
                                        "&client_secret=" + m_boxAppClientSecret;
                    byte[] byteData = Encoding.UTF8.GetBytes(payload);
                    using (Stream reqStream = req.GetRequestStream())
                    {
                        reqStream.Write(byteData, 0, byteData.Length);
                        reqStream.Close();
                    }

                    //In Step 3, Bo returns a response to us.
                    HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                    string boxResp = "";
                    using (Stream webStream = res.GetResponseStream())
                    {
                        StreamReader webStreamReader = new StreamReader(webStream);
                        boxResp = webStreamReader.ReadToEnd();
                    }
                    //boxResp is in JSON format, and we want two pieces-- the access_token and the refresh_token
                    if (boxResp != "")
                    {
                        string[] arrjson = boxResp.Split(',');
                        for (int i = 0; i < arrjson.Length; i++)
                        {
                            if (arrjson[i].ToLower().Contains("access_token"))
                            {
                                m_boxUserOAuth2AccessToken = arrjson[i].Split(':')[1].Replace("\"", "");
                            }
                            else if (arrjson[i].ToLower().Contains("refresh_token"))
                            {
                                m_boxUserOAuth2RefreshToken = arrjson[i].Split(':')[1].Replace("\"", "");
                            }
                        }

                        //We should save these tokens some where they will persist-- a file, a database, or in this case, the App object
                        Application.Add("accessToken", m_boxUserOAuth2AccessToken);
                        Application.Add("refreshToken", m_boxUserOAuth2RefreshToken);
                        
                    }
      
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message + ex.StackTrace;
            }
        }


        protected void btnExecuteAPI_Click(object sender, EventArgs e)
        {
            try
            {
                //Here we use the Access Token to make an API Call.  The token is good for 60 minutes.  After that, we need to ask Box to refresh
                //(shown in next method)
                m_boxUserOAuth2AccessToken = Application["accessToken"].ToString();
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://api.box.com/2.0/folders/0/items?limit=100");
                //Add the data payload to the request:
                req.Method = "GET";
                req.KeepAlive = true;
                req.Headers.Add("Authorization: Bearer " + m_boxUserOAuth2AccessToken);
                req.ContentType = "application/x-www-form-urlencoded";

                string payload = "grant_type=refresh_token" +
                                 "&refresh_token=" + m_boxUserOAuth2RefreshToken +
                                 "&client_id=" + m_boxAppClientID +
                                 "&client_secret=" + m_boxAppClientSecret;
                byte[] byteData = Encoding.UTF8.GetBytes(payload);
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(byteData, 0, byteData.Length);
                    reqStream.Close();
                }


                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                string boxResp = "";
                using (Stream webStream = res.GetResponseStream())
                {
                    StreamReader webStreamReader = new StreamReader(webStream);
                    boxResp = webStreamReader.ReadToEnd();
                    //Do something with the json stream, which has information about the root folders.
                }
            } catch (Exception ex)
            {

            }
 
        }

        //this shows how to Ask Box to refresh your tokens.  Your Access Token expires after 60 minutes.  You need the refresh token to get a new one.
        protected void btnRefreshTokens_Click(object sender, EventArgs e)
        {
            try
            {
                m_boxUserOAuth2AccessToken = Application["accessToken"].ToString();
                m_boxUserOAuth2RefreshToken = Application["refreshToken"].ToString();

                string strReq = m_boxTokenExchangeURI;
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(m_boxTokenExchangeURI);
                //Add the data payload to the request:
                req.Method = "POST";
                req.KeepAlive = true;
                req.ContentType = "application/x-www-form-urlencoded";

                string payload = "grant_type=refresh_token" +
                                    "&refresh_token=" + m_boxUserOAuth2RefreshToken +
                                    "&client_id=" + m_boxAppClientID +
                                    "&client_secret=" + m_boxAppClientSecret;
                byte[] byteData = Encoding.UTF8.GetBytes(payload);
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(byteData, 0, byteData.Length);
                    reqStream.Close();
                }


                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                string boxResp = "";
                using (Stream webStream = res.GetResponseStream())
                {
                    StreamReader webStreamReader = new StreamReader(webStream);
                    boxResp = webStreamReader.ReadToEnd();
                }
                //boxResp is in JSON format, and we want two pieces-- the access_token and the refresh_token
                if (boxResp != "")
                {
                    string[] arrjson = boxResp.Split(',');
                    for (int i = 0; i < arrjson.Length; i++)
                    {
                        if (arrjson[i].ToLower().Contains("access_token"))
                        {
                            m_boxUserOAuth2AccessToken = arrjson[i].Split(':')[1].Replace("\"", "");
                        }
                        else if (arrjson[i].ToLower().Contains("refresh_token"))
                        {
                            m_boxUserOAuth2RefreshToken = arrjson[i].Split(':')[1].Replace("\"", "");
                        }
                    }
                    //Update the saved values of the tokens
                    Application["accessToken"]= m_boxUserOAuth2AccessToken;
                    Application["refreshToken"]= m_boxUserOAuth2RefreshToken;
                }

            }
            catch (Exception ex)
            {
                string err = ex.Message + ex.StackTrace;
            }
        }
    }
}