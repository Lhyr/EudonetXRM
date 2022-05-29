using Com.Eudonet.Internal;
using Com.Eudonet.Internal.Import;
using Com.Eudonet.Merge;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using Com.Eudonet.Core.Model;
using System.Text;

namespace Com.Eudonet.Xrm
{
    public partial class pj : eExternalPage<LoadQueryStringPJ>, System.Web.SessionState.IRequiresSessionState
    {
        /// <summary>
        /// Identifiant de la PJ
        /// </summary>
        protected int _pjId = 0;

        private bool bInLineModeAttachment = false;

        /// <summary>
        /// LangId de l'uilisateur connecté
        /// </summary>
        public int UserLangId { get { return _pref.User.UserLangId; } }

        /// <summary>
        /// Charge les tokens du tracking de la queryString
        /// </summary>
        protected override void LoadQueryString()
        {
            DataParam = new LoadQueryStringPJ(_pageQueryString.UID, _pageQueryString.Cs, _pageQueryString.P);
        }

        /// <summary>
        /// Type d'external page
        /// </summary>
        protected override eExternal.ExternalPageType PgTyp { get { return eExternal.ExternalPageType.PJ; } }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Retourne le type (nom) de la page pour reconstruire l'UR
        /// </summary>
        /// <returns></returns>
        protected override ExternalUrlTools.PageName GetRedirectPageName()
        {
            return ExternalUrlTools.PageName.AT;
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessPage()
        {
            #region ajout des css

            PageRegisters.AddCss("ePJ");

            #endregion

            #region ajout des js 

            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eModalDialog");

            #endregion

            try
            {
                ExtPgTrace("Debut pj.aspx");

                List<Exception> listEx = new List<Exception>();

                bool bTestExpiration = true;

                // Chargement de la PJ
                ePJ pjToDisplay = GetInfosPj();

                // Pas de vérification de la date de fin de validité pour les utilisateurs connectés
                if (Session["pref"] != null)
                {
                    ePref sessionPref = (ePref)Session["pref"];
                    sessionPref.ResetTranDal();
                    // Si on a une session ouverte, même database et que l'utilisateur de la session est le même que celui qui ouvre la PJ, on ne teste pas l'expiration
                    if (sessionPref.DatabaseUid == _pageQueryString.UID && sessionPref.UserId == DataParam.ParamData.UserId)
                        bTestExpiration = false;
                }

                if (bTestExpiration && pjToDisplay.ExpireDay != null)
                {
                    if ((pjToDisplay.ExpireDay.Value - DateTime.Today).TotalDays <= 0)
                        throw new PjExp(eResApp.GetRes(_pref, 8259));
                }

                // Recupération du dossier de la pj           
                eLibConst.FOLDER_TYPE ftype;
                switch (pjToDisplay.PJType)
                {
                    case EudoQuery.PjType.IMPORT:
                    case EudoQuery.PjType.IMPORT_REPORTS:
                        ftype = eLibConst.FOLDER_TYPE.IMPORT;
                        break;

                    case EudoQuery.PjType.REPORTS:
                        ftype = eLibConst.FOLDER_TYPE.REPORTS;
                        break;

                    default:
                        ftype = eLibConst.FOLDER_TYPE.ANNEXES;
                        break;
                }

                string pjPath = string.Concat(eModelTools.GetPhysicalDatasPath(Context, ftype, _pref.GetBaseName), "\\", pjToDisplay.FileName);

                //Verification de la présence de la PJ sur le serveur
                if (File.Exists(pjPath))
                {
                    //Affichage PJ
                    displayPJ(pjToDisplay, pjPath);
                }
                else
                {
                    throw new PjExp(eResApp.GetRes(_pref, 8260));
                }
            }
            catch (PjExp exp)
            {
                // Utile pour le message d'erreur à l'utilisateur
                _panelErrorMsg = exp.Message;

                throw exp;
            }
        }

        /// <summary>
        /// Gestion de l'affichage du message d'erreur à l'utilisateur si une erreur c'est produite
        /// </summary>
        protected override void RendTitleAndErrorMsg()
        {
            // Si _panelErrorMsg est déjà défini alors c'est un message bien identifié pour l'utilisateur
            if (string.IsNullOrEmpty(_panelErrorMsg))
            {
                base.RendTitleAndErrorMsg();
            }
            else
            {
                // On active le panneau d'erreur
                RendType = ExternalPageRendType.ERROR;
            }
        }

        /// <summary>
        /// Recupere un object pj a partir des informations recuperer de la query string
        /// </summary>
        /// <returns>Objet pj a chargé</returns>
        protected ePJ GetInfosPj()
        {
            int pjId = DataParam.ParamData.PjId;

            //Si pas d id fourni on retourne un message d'erreur
            if (pjId == 0)
            {
                throw new PjExp(eResApp.GetRes(_pref, 8261));
            }
            else
            {
                //Si la pj n'a pas été trouvé en base on leve un message
                ePJ pjToDisplay = ePJ.CreatePJ(_pref, pjId);
                if (pjToDisplay == null)
                    throw new PjExp(eResApp.GetRes(_pref, 8262));

                return pjToDisplay;
            }
        }

        /// <summary>
        /// Effectue le rendu de la PJ en fonction de son type mime
        /// </summary>
        /// <param name="pjToDisplay">La PJ a afficher</param>
        /// <param name="pjPath">Le chemin complet vers la PJ</param>
        private void displayPJ(ePJ pjToDisplay, string pjPath)
        {

            bInLineModeAttachment = _requestTools.GetRequestQSKeyS("mdinline") == "attch";

            string sMimeType = eLibTools.GetMimeTypeFromExtension(Path.GetExtension(pjToDisplay.FileName));

            Response.Clear();
            Response.ClearContent();
            Response.ClearHeaders();
            Response.Buffer = true;

            FileStream myFileStream = new FileStream(pjPath, FileMode.Open);
            long fileSize = myFileStream.Length;
            byte[] buffer = new byte[(int)fileSize];
            myFileStream.Read(buffer, 0, (int)fileSize);
            myFileStream.Close();

            Response.Charset = null;
            //Response.ContentEncoding = Encoding.UTF8;
            Response.AddHeader("Content-Length", fileSize.ToString());

            string sF = pjToDisplay.FileName.Replace(" ", "_").Replace(",", "_");

            string contentDisposition = "";

            if (Request.Browser != null && (Request.Browser.Browser == "IE" && (Request.Browser.Version == "7.0" || Request.Browser.Version == "8.0")))
                contentDisposition = (bInLineModeAttachment ? "attachment" : "inline") + "; filename=" + sF;
            else
                contentDisposition = (bInLineModeAttachment ? "attachment" : "inline") + "; filename*=UTF-8''" + Uri.EscapeDataString(sF);

            Response.AddHeader("Content-Disposition", contentDisposition);



            Response.ContentType = sMimeType;

            Response.BinaryWrite(buffer);

            Response.End();
        }
    }
}