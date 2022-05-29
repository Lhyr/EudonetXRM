using Com.Eudonet.Common.Cryptography;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Web.UI;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// redirige vers un mode fiche en fonction des paramètres fourni en querystring
    /// </summary>
    public partial class eGoToFile : eEudoPage
    {

        /// <summary>
        /// js executé par la page
        /// </summary>
        protected String _jsToExecute;

        /// <summary>
        /// chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {

            try
            {
                _jsToExecute = String.Empty;
                int nTab, nFid;

                String error = String.Empty;

                string sTab = Request.QueryString["tab"];
                string sFid = Request.QueryString["fid"];
                string sBkmTab = Request.QueryString["bkmtab"];
                string sBkmFid = Request.QueryString["bkmfid"];
                string sHash = Request.QueryString["hash"];

                formRedirect.Action = "eMain.aspx";

                if (!String.IsNullOrEmpty(sTab) && !String.IsNullOrEmpty(sHash))
                {
                    string testHash = HashSHA.GetHashSHA1(String.Concat("EUD0N3T", "tab=", sTab, "&fid=", sFid, "XrM"));

                    if (testHash.ToLower() != sHash.ToLower())
                    {
                        ErrorContainer = eErrorContainer.GetDevUserError(
                                            eLibConst.MSG_TYPE.EXCLAMATION,
                                            eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                                            eResApp.GetRes(_pref, 1819),
                                            eResApp.GetRes(_pref, 72),  //   titre
                                            "La clé de hashage n'est pas valide"
                                            );
                        LaunchErrorHTML(true, ErrorContainer);
                    }
                    else
                    {
                        nTab = eLibTools.GetNum(sTab);
                        if (nTab != 0)
                        {
                            redirTabID.Value = sTab;

                            #region Redirection vers une fiche
                            if (!String.IsNullOrEmpty(sFid))
                            {
                                // Dans le cas de redirection vers une fiche, il faut savoir :
                                // - si c'est un TEMPLATE ou pas, pour l'ouvrir en mode popup
                                // - si la table est de type Email 
                                fileInPopup.Value = "0";
                                redirTPLMail.Value = "0";

                                eudoDAL eDal = eLibTools.GetEudoDAL(_pref);
                                try
                                {
                                    // Récupération des infos de la table
                                    TableLite tl = new TableLite(nTab);
                                    eDal.OpenDatabase();
                                    if (tl.ExternalLoadInfo(eDal, out error))
                                    {
                                        if (tl.ShortField == "TPL")
                                        {
                                            fileInPopup.Value = "1";
                                            if (tl.EdnType == EdnType.FILE_MAIL || tl.EdnType == EdnType.FILE_SMS)
                                            {
                                                redirTPLMail.Value = "1";
                                            }
                                        }

                                        #region Redirection depuis une fiche parente
                                        //if (!String.IsNullOrEmpty(sBkmTab))
                                        //{
                                        //    if (eLibTools.GetNum(sBkmTab) != 0)
                                        //    {
                                        //        redirBkmTabID.Value = sBkmTab;

                                        //        if (!String.IsNullOrEmpty(sBkmFid))
                                        //        {
                                        //            if (eLibTools.GetNum(sBkmFid) != 0)
                                        //            {
                                        //                redirBkmFileID.Value = sBkmFid;
                                        //            }
                                        //        }
                                        //    }

                                        //}
                                        #endregion
                                    }
                                    else
                                    {
                                        ErrorContainer = eErrorContainer.GetDevUserError(
                                            eLibConst.MSG_TYPE.EXCLAMATION,
                                            eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                                            eResApp.GetRes(_pref, 1819),
                                            eResApp.GetRes(_pref, 72),  //   titre
                                            String.Concat(error)
                                            );
                                        LaunchErrorHTML(true, ErrorContainer);
                                    }
                                }

                                finally
                                {
                                    eDal.CloseDatabase();
                                }


                                nFid = eLibTools.GetNum(sFid);
                                if (nFid != 0)
                                    redirFileID.Value = sFid;
                            }
                            #endregion
                        }
                    }




                    //_jsToExecute = String.Concat("nGlobalActiveTab = 0; goTabList(", nTab, ", true);");

                }
            }
            catch(eEndResponseException)
            {

                Response.End();
            }

            


        }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }
    }
}