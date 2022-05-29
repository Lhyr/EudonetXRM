using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Cryptography;
using Newtonsoft.Json;
using Com.Eudonet.Merge;

namespace Com.Eudonet.Xrm.mgr.external
{
    /// <summary>
    /// Summary description for eAutoConnect
    /// </summary>
    public class eAutoConnect : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";


            string sAuth = context.Request.QueryString["auth"];

            string t = context.Request.QueryString["t"];
            if (t != null)
            {

                string sDecToken = ExternalUrlTools.GetDecrypt(t);

                var def = new { b = "", a = 0, c = 0, u = 0 };
                var tok = JsonConvert.DeserializeAnonymousType(sDecToken, def);

                switch (tok.a)
                {
                    case (int)eLibConst.AuthenticationMode.SAML2:

                        string st = "";
                        string sbt = "";
                        

                        //authentication mode SAML
                        //redirecto to login.ashx
                        //var url = "mgr/saml/eLogin.ashx?h=" + screen.availHeight + "&w=" + screen.availWidth + (lang) + (debug) + "&dbt=" + encode(strDbToken) + "&st=" + encode(strToken);

                        //génération d'un substriber token et d'un base token
                        var tokcnx = eLoginTools.GetExtranetTokens(tok.b, tok.c);

                        string lang = EudoCommonHelper.EudoHelpers.GetUserIdLangFromCookie().ToString();




                        string xrmdir = eLibTools.GetServerConfig("xrmdir", "xrm");
                        string sURL = $"/{xrmdir}/mgr/saml/elogin.ashx?dbt={ tokcnx.Item1.GetTokenCrypted(true)}&st={tokcnx.Item2.GetTokenCrypted(true)}&l={lang}";

                        context.Response.Redirect(sURL);

                        return;


                }

            }
            else if (sAuth == null)
            {





                try
                {
                    CnxInfos cnx = eLoginTools.LoadCnxInfos(sAuth, CryptographyConst.TokenType.AUTO_CONNECT);

                    SubscriberToken st = cnx.SubscriberToken;

                    UserToken ut = cnx.UserToken;
                    DbTokenGeneric db = cnx.DBToken;
                    eLoginOL login = eLoginOL.GetLoginObject(st, ut);
                    login.SetSessionVars(db);

                    if (context.Request.QueryString.AllKeys.Contains("file"))
                    {

                        string sFile = context.Request.QueryString["file"];
                        string sFileId = context.Request.QueryString["fileid"];
                        string testHash = HashSHA.GetHashSHA1(String.Concat("EUD0N3T", "tab=", sFile, "&fid=", sFileId, "XrM"));

                        context.Response.Redirect("../eGotoFile.aspx?tab=" + sFile + "&fid=" + sFileId + "&hash=" + HttpUtility.UrlEncode(testHash), true);
                    }
                    else
                    {
                        string sBase = eModelTools.GetBaseUrlXRM() + "eMain.aspx";

                        context.Response.Redirect(sBase, true);
                    }

                }
                catch (TokenException)
                {
                    //
                    throw;
                }
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}