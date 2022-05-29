using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe qui s'occupe d'afficher les information pour l'impression
    /// Cette classe à été crée trop rapidement, il faut la refaire completement
    /// </summary>
    /// <autheur>NBA</autheur>
    public partial class ePrintFile : eEudoPage
    {
        #region Variables membres

        /// <summary>Liste des Bookmarks séléctionnés lors de l'assistant impression descid séparés par des ";"</summary>
        private String _lstSelectBKM = String.Empty;

        /// <summary>Entete de page</summary>
        private String _topHead = String.Empty;

        /// <summary>Bas de page</summary>
        private String _buttomHead = String.Empty;

        /// <summary>Titre du rapport</summary>
        private String _title = String.Empty;
        /// <summary>
        /// JS exécuté : obsolète ?
        /// </summary>
        public string _sJs = String.Empty;
        /// <summary>
        /// TabID de l'onglet concerné (pour la mini-fiche/VCard, iso eFileManager.ashx)
        /// </summary>
        public Int32 _tab = 0;
        #endregion

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            string error = string.Empty;

            bool printAllowed = false;

            try
            {
                printAllowed = eLibDataTools.IsTreatmentAllowed(_pref, _pref.User, eLibConst.TREATID.PRINT);
            }
            catch (Exception exp)
            {
                printAllowed = false;

                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 259), exp.Message), _pref);
            }

            if (!printAllowed)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.EXCLAMATION,
                    eResApp.GetRes(_pref, 72),
                    eResApp.GetRes(_pref, 6342),
                    eResApp.GetRes(_pref, 13),
                    String.Concat("Création du renderer pour l'impression mode fiche (ePrintFile.aspx)", Environment.NewLine, "Vous n'avez pas les droits pour editer ce rapport."));

                try
                {
                    _sJs = "self.close()";

                    LaunchErrorHTML(true);
                }
                catch (eEndResponseException)
                { };

                return;
            }

            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eFile");
            PageRegisters.AddCss("eList");
            PageRegisters.AddCss("ePrint");

            #region Recuperation des variables de la fiche ou de la liste
            Int32 _nFileId = 0;
            _tab = 200;

            // Récupération des variables passées en POST
            if (Request.QueryString["nfileID"] != null)
                Int32.TryParse(Request.QueryString["nfileID"], out _nFileId);
            if (Request.QueryString["nTab"] != null)
                Int32.TryParse(Request.QueryString["nTab"], out _tab);

            #endregion

            #region Recuperation des paramètres d'impression

            ePrintParams _printParams = new ePrintParams();
            _printParams.ButtomTitlePage = "";
            //_printParams.TopTitlePage = "Mon entête de page";
            //_printParams.Title = "Mon titre méga Cool";
            _printParams.BViewTitle = true;
            _printParams.BViewGeneralHeadings = true;
            _printParams.LstSelectBKM = "1200;400";

            #endregion

            eudoDAL dal = eLibTools.GetEudoDAL(_pref);

            eRenderer efRend = eRenderer.CreateRenderer();
            try
            {
                dal.OpenDatabase();
                TableLite tl = new TableLite(_tab);
                tl.ExternalLoadInfo(dal, out error);

                // Création du renderer spécifique à l'impression
                efRend = eRendererFactory.CreatePrintFileRenderer(_pref, tl, _tab, _nFileId, _printParams);

                // Envoi du rendu HTML à la page
                if (efRend != null && efRend.ErrorMsg.Length == 0)
                    PrintContent.Controls.Add(efRend.PgContainer);
                //RenderResult(efRend.PgContainer, eConst.eFileType.FILE_PRINT);
                else
                {
                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.EXCLAMATION,
                        eResApp.GetRes(_pref, 72),
                         eResApp.GetRes(_pref, 6342),
                         eResApp.GetRes(_pref, 13),
                        String.Concat("Création du renderer pour l'impression mode fiche (ePrintFile.aspx)", Environment.NewLine, efRend.ErrorMsg));

                    //Arrete le traitement et envoi l'erreur
                    LaunchErrorHTML(true);
                }
            }
            catch (eEndResponseException) { }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.EXCLAMATION,
                    eResApp.GetRes(_pref, 72),
                     eResApp.GetRes(_pref, 6342),
                     eResApp.GetRes(_pref, 13),
                    String.Concat("Création du renderer pour l'impression mode fiche (ePrintFile.aspx)", Environment.NewLine, exc.Message));

                //Arrete le traitement et envoi l'erreur
                try
                {
                    LaunchErrorHTML(true);
                }
                catch (eEndResponseException)
                { };
            }
            finally
            {
                dal.CloseDatabase();
            }
        }

        /// <summary>
        /// Rendu HTML
        /// </summary>
        /// <param name="monPanel"></param>
        /// <param name="efType">0 : Type de rendu de la fiche</param>
        private void RenderResult(Panel monPanel, eConst.eFileType efType)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter tw = new HtmlTextWriter(sw);
            monPanel.RenderControl(tw);

            // Comme l'impression est en mode popup on construit le contenu HTML de base
            // MAB/ELAIZ - 2019-10-15 - US #1074 - Backlog #1660 - VCard : On précise le TabID de l'onglet en cours pour différencier en CSS
            if (efType == eConst.eFileType.FILE_PRINT)
            {
                String s = String.Concat(@"<html><head>
<meta http-equiv='X-UA-Compatible' content='IE=edge' />
<meta http-equiv='content-type' content='text/html; charset=UTF-8' />
<link type='text/css' rel='stylesheet' href='themes/default/css/eMain.css'>
<link type='text/css' rel='stylesheet' href='themes/default/css/eFile.css'>
<link type='text/css' rel='stylesheet' href='themes/default/css/eVcard.css'>
<link type='text/css' rel='stylesheet' href='themes/default/css/eList.css'>
<link type='text/css' rel='stylesheet' href='themes/default/css/ePrint.css'>
</head>
<body class='VCardTT openTransition' tab='", _tab, "'>", sb.ToString(), "</body></html>");

                Response.Clear();
                Response.Write(s);
            }
            else
            {
                Response.Clear();
                Response.Write(sb.ToString());
            }
        }
    }
    /// <summary>
    /// Classe qui définit les options nécessaires à l'impression
    /// </summary>
    public class ePrintParams
    {
        #region variables membres

        /// <summary>
        /// Liste des Bookmarks séléctionnés lors de l'assistant impression descid séparés par des ";"
        /// </summary>
        private String _lstSelectBKM = String.Empty;
        /// <summary>
        /// Indique si on affiche les rubriques générales de la fiche
        /// </summary>
        private bool _bViewGeneralHeadings = true;
        /// <summary>
        /// Entete de page
        /// </summary>
        private String _topTitlePage = String.Empty;
        /// <summary>
        /// Bas de page
        /// </summary>
        private String _buttomTitlePage = String.Empty;
        /// <summary>
        /// Titre du rapport
        /// </summary>
        private String _title = String.Empty;
        /// <summary>
        /// Indique si l'on doit afficher le titre du rapport en entete de page
        /// </summary>
        private bool _bViewTitle = false;

        #endregion

        #region Accesseurs

        /// <summary>
        /// Liste des Bookmarks séléctionnés lors de l'assistant impression descid séparés par des ";"
        /// </summary>
        public String LstSelectBKM
        {
            get { return _lstSelectBKM; }
            set { _lstSelectBKM = value; }
        }

        /// <summary>
        /// Indique si on affiche les rubriques générales de la fiche
        /// </summary>
        public bool BViewGeneralHeadings
        {
            get { return _bViewGeneralHeadings; }
            set { _bViewGeneralHeadings = value; }
        }

        /// <summary>
        /// Entete de page
        /// </summary>
        public String TopTitlePage
        {
            get { return _topTitlePage; }
            set { _topTitlePage = value; }
        }

        /// <summary>
        /// Bas de page
        /// </summary>
        public String ButtomTitlePage
        {
            get { return _buttomTitlePage; }
            set { _buttomTitlePage = value; }
        }

        /// <summary>
        /// Titre du rapport
        /// </summary>
        public String Title
        {
            get { return _title; }
            set { _title = value; }
        }

        /// <summary>
        /// Indique si l'on doit afficher le titre du rapport en entete de page
        /// </summary>
        public bool BViewTitle
        {
            get { return _bViewTitle; }
            set { _bViewTitle = value; }
        }

        #endregion
    }
}