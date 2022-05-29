using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Affichage de la messagebox de confirmation de suppression des fiches PM
    /// </summary>
    public partial class eConfirmDeletePm : eEudoPage
    {
        /// <summary>
        /// Id de la frame de la modal
        /// </summary>
        public string _sUid = String.Empty;

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region ajout des css

            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eMarkedFile");
            PageRegisters.AddCss("eCatalog");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eModalDialog");

            #endregion


            #region ajout des js


            PageRegisters.AddScript("eTools");

            #endregion

            String sName = String.Empty, sAdrName = String.Empty, sPPName = string.Empty;

            if (_allKeys.Contains("uid") && !String.IsNullOrEmpty(Request.Form["uid"]))
                _sUid = Request.Form["uid"].ToString();

            if (!_allKeys.Contains("name"))
            {
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine, "pas de name");

                ErrorContainer = eErrorContainer.GetDevUserError(
                eLibConst.MSG_TYPE.CRITICAL,
                eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                eResApp.GetRes(_pref, 72),  //   titre
                String.Concat(sDevMsg)
                );

                LaunchError(ErrorContainer);
            }

            sName = Request.Form["name"];

            IDictionary<int, string> diRes = new Dictionary<int, string>();
            List<int> lstDescidRes = new List<int>() { (int)EudoQuery.TableType.ADR, (int)EudoQuery.TableType.PP };
            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            dal.OpenDatabase();
            try
            {
                diRes = eLibTools.GetPrefName(dal, _pref.Lang, lstDescidRes);
            }
            finally
            {
                dal.CloseDatabase();
            }
            sAdrName = diRes[(int)EudoQuery.TableType.ADR];
            sPPName = diRes[(int)EudoQuery.TableType.PP];

            //Création de la table du message
            System.Web.UI.WebControls.Table tbMsg = new System.Web.UI.WebControls.Table();
            msgcontainer.Controls.Add(tbMsg);

            TableRow tableRow;
            TableCell tableCell;

            // Message Principal
            tableRow = new TableRow();
            tableRow.TableSection = TableRowSection.TableBody;
            tbMsg.Rows.Add(tableRow);

            //Logo
            tableCell = new TableCell();

            tableCell.CssClass = String.Concat("td-logo");

            //pour les anciens theme
            if (_pref.ThemeXRM.Version < 2)
            {
                HtmlGenericControl img = new HtmlGenericControl("img");
                img.Attributes.Add("class", "logo-quote");
                img.Attributes.Add("src", "ghost.gif");
                tableCell.Controls.Add(img);
            }
            tableRow.Cells.Add(tableCell);

            //Message
            //#1906 : Les fiches '{0}' rattachées seront conservées, le champde lisaison vers la fiche "{1}" sera vidé.
            String sConfirmText = String.Concat(eResApp.GetRes(_pref, 1135), ".<br/><br/> ", String.Format(eResApp.GetRes(_pref, 1906), sAdrName, sName), "<br/><br/> ");
            if (sName.Length > 0)
                sName = String.Concat("\"", sName, "\"");
            sConfirmText = sConfirmText.Replace("<FILE>", HttpUtility.HtmlEncode(sName));

            tableCell = new TableCell();
            tableCell.ID = String.Concat("msgbox_msg_", _sUid);
            tableCell.CssClass = String.Concat("text-alert-quote confirm-msg");
            //tableCell.Style.Add("text-align", "justify");
            tableCell.Text = sConfirmText;
            tableRow.Cells.Add(tableCell);

            // Message Détaillé
            tableRow = new TableRow();
            tbMsg.Rows.Add(tableRow);

            //Logo
            tableCell = new TableCell();
            tableCell.Text = "&nbsp;";
            tableRow.Cells.Add(tableCell);

            //Message
            String sDetailMsg = eResApp.GetRes(_pref, 1137);   //1137 : Supprimer les fiches <LINKFILE> étant seulement liées à cette fiche
            sDetailMsg = sDetailMsg.Replace("<LINKFILE>", String.Concat("'", sPPName, "'"));

            HtmlGenericControl chk = eTools.GetCheckBoxOption(sDetailMsg, String.Concat("chkId_", _sUid), false, false, "opt-list-chk", "");
            tableCell = new TableCell();
            tableCell.ID = String.Concat("msgbox_from_pm_msgdetails_", _sUid); /*ALISTER => Demande 83 537*/
            tableCell.CssClass = "text-msg-quote";
            tableCell.Controls.Add(new LiteralControl(eResApp.GetRes(_pref, 1136)));
            tableCell.Controls.Add(chk);
            tableRow.Cells.Add(tableCell);

            sDetailMsg = String.Format(eResApp.GetRes(_pref, 1905), sAdrName, sName);   //1905 : Supprimer les fiches '{0}' liées à "{1}"

            chk = eTools.GetCheckBoxOption(sDetailMsg, String.Concat("chkAdrDelete_", _sUid), false, false, "opt-list-chk", "");
            tableCell.Controls.Add(chk);
        }
    }
}