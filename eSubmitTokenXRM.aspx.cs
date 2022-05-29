using System;
using System.Web.UI;
using Com.Eudonet.Internal;
using System.Collections.Generic;
using Com.Eudonet.Merge;
using System.Web;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// "POST" un token xrm vers une specif
    /// </summary>
    public partial class eSubmitTokenXRM : eEudoPage
    {
        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return null;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            bool helpExtranet = false;
            if (_requestTools.AllKeysQS.Contains("helpextranet"))
                helpExtranet = Request.QueryString["helpextranet"] == "1";

            if (helpExtranet)
            {
                bool helpExtranetEnabled = eLibTools.GetServerConfig("HelpDeskEnabled", "0") == "1";                

                if (helpExtranetEnabled)
                {
                    string helpExtranetUrl = eLibTools.GetServerConfig("HelpDeskUrl", String.Empty);
                    if(!String.IsNullOrEmpty(helpExtranetUrl))
                    {
                        string helpExtranetToken = eSpecifTokenLight.GetSpecifTokenLight(_pref).Token;
                        if (!String.IsNullOrEmpty(helpExtranetToken))
                        {
                            exporttospecif.Action = helpExtranetUrl;
                            t.Value = helpExtranetToken;
                            body.Attributes.Add("onload", "document.getElementById('exporttospecif').submit();");
                        }
                    }
                }
            }
            else
            {
                Int32 nSpecifId = 0;
                Int32 nFileId = 0;
                Int32 nTab = 0;
                Int32 nParentTab = 0;
                Int32 nParentFileId = 0;
                Int32 nDescId = 0;




                string tok = _requestTools.GetRequestQSKeyS("t");

                if (!string.IsNullOrEmpty(tok))
                {
                    string param = ExternalUrlTools.GetDecrypt(tok);
                    var specifparams = HttpUtility.ParseQueryString(param);

                    foreach(string sname in specifparams)
                    {
                        switch(sname)
                        {
                            case "sid":
                                Int32.TryParse(specifparams[sname], out nSpecifId);
                                break;
                            case "fid":
                                Int32.TryParse(specifparams[sname], out nFileId);
                                break;
                            case "tab":
                                Int32.TryParse(specifparams[sname], out nTab);
                                break;
                            case "parenttab":
                                Int32.TryParse(specifparams[sname], out nParentTab);
                                break;
                            case "parentfid":
                                Int32.TryParse(specifparams[sname], out nParentFileId);
                                break;
                            case "descid":
                                Int32.TryParse(specifparams[sname], out nDescId);
                                break;
                        }
                    }


                }


                /*if (_requestTools.AllKeysQS.Contains("sid"))
                    Int32.TryParse(Request.QueryString["sid"].ToString(), out nSpecifId);
                    */

                if (nSpecifId == 0)
                    LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6701), ""));

                eSpecif spec = eSpecif.GetSpecif(_pref, nSpecifId);

                if (spec == null)
                    LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6701), ""));

                /*
                if (_requestTools.AllKeysQS.Contains("fid"))
                    Int32.TryParse(Request.QueryString["fid"].ToString(), out nFileId);

                if (_requestTools.AllKeysQS.Contains("tab"))
                    Int32.TryParse(Request.QueryString["tab"].ToString(), out nTab);

                if (_requestTools.AllKeysQS.Contains("parenttab"))
                    Int32.TryParse(Request.QueryString["parenttab"].ToString(), out nParentTab);

                if (_requestTools.AllKeysQS.Contains("parentfid"))
                    Int32.TryParse(Request.QueryString["parentfid"].ToString(), out nParentFileId);

                if (_requestTools.AllKeysQS.Contains("descid"))
                    Int32.TryParse(Request.QueryString["descid"].ToString(), out nDescId);

    */


                eSpecifToken specToken = eSpecifToken.GetSpecifTokenXRM(_pref, spec, nTab, nFileId, nParentTab, nParentFileId, nDescId);
                if (spec.IsViewable && !specToken.IsError)
                {
                    t.Value = specToken.Token;

                    exporttospecif.Action = spec.GetRelativeUrlFromRoot(_pref);

                    //SPH #52291  : il semblerait qu'il faille ajouter l'urlparam (normalement destiné a des commande interne) à l'url
                    if (spec.UrlParam.Length > 0)
                    {
                        if (exporttospecif.Action.Contains("?"))
                            exporttospecif.Action = exporttospecif.Action + "&" + spec.UrlParam;
                        else
                            exporttospecif.Action = exporttospecif.Action + "?" + spec.UrlParam;
                    }

                    if(spec.IsStatic)
                    {
                        if (exporttospecif.Action.Contains("?"))
                            exporttospecif.Action = exporttospecif.Action + "&token=" + HttpUtility.UrlEncode( specToken.ShortToken);
                        else
                            exporttospecif.Action = exporttospecif.Action + "?token=" + HttpUtility.UrlEncode( specToken.ShortToken);

                        Response.Redirect(exporttospecif.Action);
                    }
                    else 
                        body.Attributes.Add("onload", "document.getElementById('exporttospecif').submit();");
                }
                else
                {
                    body.InnerText =  specToken.ErrorMsg;
                }
            }
        }
    }
}