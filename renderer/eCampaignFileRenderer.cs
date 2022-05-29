using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{


    /// <summary>
    /// Classe de rendu pour afficher une Campagne
    /// </summary>
    public class eCampaignFileRenderer : eEditFileLiteRenderer
    {
        const Int32 NB_FIELDS_BY_LINE = 5;  //Demande #29697 - [eCircle - Mode Fiche] - Bilan de la campagne - Supprimer les 2 encarts gris (enveloppes à droite) et aligner toutes les autres stats sur 1 ligne
        const Int32 NB_COLS_BY_FIELD = 2;


        private eCampaignReport _campaignReport;
        private MAILINGSENDTYPE _sendType = MAILINGSENDTYPE.EUDONET;
        private String _sStatusCellId = String.Empty;
        private CampaignStatus _campaignStatus;

        /// <summary>
        /// Affichage pour la création et la modification
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nFileId"></param>
        public eCampaignFileRenderer(ePref pref, Int32 nFileId)
            : base(pref, TableType.CAMPAIGN.GetHashCode(), nFileId)
        {
            _rType = RENDERERTYPE.EditFile;
        }

        /// <summary>
        /// retourne la ligne à partir de laquelle les rubriques doivent passer sous la barre des signets
        /// </summary>
        /// <param name="nBreakLine"></param>
        /// <returns></returns>
        protected override Int32 GetBreakLine(Int32 nBreakLine = 0)
        {
            // dans le cas d'une campagne, il n'y a pas de BreakLine
            return 0;

        }

        /// <summary>
        /// rend le block HTML de tous les signets dans une méthode overridable
        /// </summary>
        /// <returns></returns>
        protected override void GetBookMarkBlock()
        {
            
            if ((_nFileId > 0 && !_bPopupDisplay))
                base.GetBookMarkBlock();
        }


        /// <summary>
        /// Prépare un tableau contenant les rubriques à afficher en signet et y deverse les lignes du tableau principal qui sont concernées
        /// </summary>
        /// <param name="fileTabBody"></param>
        /// <param name="nbColByLine"></param>
        /// <param name="nBreakLine"></param>
        protected override System.Web.UI.WebControls.Table SetHtmlTabInBkm(System.Web.UI.WebControls.Table fileTabBody, int nbColByLine, int nBreakLine)
        {
            if (_myFile.ActiveBkm != EudoQuery.ActiveBkm.DISPLAYFIRST.GetHashCode())
                _backBoneRdr.PnFilePart2.Style.Add("display", "none");

            _bFileTabInBkm = true;

            return null;
        }

        /// <summary>
        /// initialisation de l'objet
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            Boolean bReturn = base.Init();
            if (!bReturn)
                return false;

            // identification du type d'envoi de la campagne
            String sSendTypeAlias = String.Concat(TableType.CAMPAIGN.GetHashCode(), "_", CampaignField.SENDTYPE.GetHashCode());
            eFieldRecord fldSendType = _myFile.Record.GetFieldByAlias(sSendTypeAlias);
            if (fldSendType == null)
            {
                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, "eCampaignFileRenderer : Le Champ SendType est absent de la Table Campaign"), Pref);
            }
            else
            {
                Int32 iSendType = 0;
                Int32.TryParse(fldSendType.Value, out iSendType);

                try
                {
                    _sendType = (MAILINGSENDTYPE)iSendType;
                }
                catch (Exception)
                {
                    eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, String.Concat("eCampaignFileRenderer : Le type d'envoi de la campagne ", _myFile.FileId, " est dans un format incorrect")), Pref);
                }
            }

            // Identification du statut de la demande.
            String sSendStatusAlias = String.Concat(TableType.CAMPAIGN.GetHashCode(), "_", CampaignField.STATUS.GetHashCode());
            eFieldRecord fldSendStatus = _myFile.Record.GetFieldByAlias(sSendStatusAlias);
            if (fldSendStatus == null)
            {
                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, "eCampaignFileRenderer : Le Champ Status est absent de la Table Campaign"), Pref);
            }
            else
            {
                _sStatusCellId = eTools.GetFieldValueCellId(_myFile.Record, fldSendStatus);

                Int32 iStatus = 0;
                Int32.TryParse(fldSendStatus.Value, out iStatus);

                try
                {
                    _campaignStatus = (CampaignStatus)iStatus;
                }
                catch (Exception)
                {
                    eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, String.Concat("eCampaignFileRenderer : Le Statut de la campagne ", _myFile.FileId, " est dans un format incorrect")), Pref);
                }
            }

            return true;
        }

        /// <summary>
        /// Méthode permettant de finaliser l'affichage du mode fiche édition
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            Boolean bReturn = base.End();
            if (!bReturn)
                return bReturn;

            FillDetailBookmark();

            return bReturn;
        }




        private void FillDetailBookmark()
        {
            _campaignReport = eCampaignReport.CreateCampaignReport(Pref, _nFileId);
            if (_campaignReport.Errors != null)
            {
                StringBuilder sbErrorMsg = new StringBuilder();
                foreach (KeyValuePair<String, Exception> kvp in _campaignReport.Errors)
                {
                    sbErrorMsg.Append(kvp.Value.Message);
                    eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, String.Concat(kvp.Key, " : ", kvp.Value.Message)), Pref);

                }
                if (sbErrorMsg.Length > 0)
                    _sErrorMsg = sbErrorMsg.ToString();
            }

            if (_campaignReport.Warning.Length > 0)
            {
                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, _campaignReport.Warning), Pref);
            }

            System.Web.UI.WebControls.Table tbStats = new System.Web.UI.WebControls.Table();

            _backBoneRdr.PnFilePart2.Controls.Add(tbStats);

            tbStats.CssClass = "campStats";

            tbStats.Rows.Add(new TableRow());
            tbStats.Rows[0].Cells.Add(new TableCell());

            tbStats.Rows[0].Cells[0].Controls.Add(getCampaignReportPanel());


            Panel pnDataToChart = new Panel();
            pnDataToChart.ID = "DataToChart";
            pnDataToChart.Style.Add("display", "none");
            _backBoneRdr.PnFilePart2.Controls.AddAt(0, pnDataToChart);

            //[44366] Pour la campagne SMS on ne gére pas les stats
            if (_sendType == MAILINGSENDTYPE.EUDONET_SMS)
                return;

            //Pour envoi eudonet, on est sur 2 colonnes mais pour eCircle, 3 colonnes
            Int32 nColSpan = (_sendType == MAILINGSENDTYPE.EUDONET) ? 2 : 3;
            tbStats.Rows[0].Cells[0].ColumnSpan = nColSpan;
            Dictionary<eStatsRenderer, String> liStatsRenderer = new Dictionary<eStatsRenderer, String>();

            //Graphique "Taux d'ouverture"
            liStatsRenderer.Add(GetReadingRateStatsRenderer(), eResApp.GetRes(Pref, 6475));

            //Graphique "Motif de rejet" par pour type eudonet
            if (_sendType == MAILINGSENDTYPE.ECIRCLE)
                liStatsRenderer.Add(GetUnreceivedCauseStatsRenderer(), eResApp.GetRes(Pref, 6476));

            //Graphique "Désinscrits"
            liStatsRenderer.Add(GetUnsubscribeStatsRenderer(), eResApp.GetRes(Pref, 6466));

            //Tableau nombre de clics par lien
            liStatsRenderer.Add(GetClics(), eResApp.GetRes(Pref, 6456));

            //Répartition des clics par jour
            liStatsRenderer.Add(GetClicsPerDayStatsRenderer(), eResApp.GetRes(Pref, 6477));

            TableRow tr = new TableRow();
            TableCell tc;
            Int32 i = 0;
            foreach (KeyValuePair<eStatsRenderer, String> kvp in liStatsRenderer)
            {
                if (i % nColSpan == 0)
                {
                    tr = new TableRow();
                    tbStats.Rows.Add(tr);
                }

                tc = new TableCell();
                tr.Cells.Add(tc);
                tc.Controls.Add(encapsulatedPanel(kvp.Key, kvp.Value));
                if (kvp.Key.InptChartData != null)
                    pnDataToChart.Controls.Add(kvp.Key.InptChartData);

                i++;
            }
            //Si dernière cellule ajoutée n'est pas la dernière cellule de la ligne (nombre impaire) on colspan la dernière cellule de l'espace disponible
            if ((i % 2) == 1)
            {
                TableRow trImp = tbStats.Rows[tbStats.Rows.Count - 1];
                TableCell tcImp = trImp.Cells[trImp.Cells.Count - 1];
                tcImp.ColumnSpan = nColSpan - (trImp.Cells.Count - 1);   //colspan de l'espace disponible, c'est à dire : <Nombre de cellules possible> - <Nombre de cellules> - Cellule courant
            }
        }



        /// <summary>
        /// Crée l'encadré "Bilan de la campagne
        /// </summary>
        /// <returns></returns>
        private Panel getCampaignReportPanel()
        {
            Panel pnCampainReport = new Panel();
            pnCampainReport.CssClass = "bkmdiv";
            try
            {
                pnCampainReport.Controls.Add(eBookmarkRenderer.CreateTitleBar(Pref, sLibelle: eResApp.GetRes(Pref, 6462))); // Bilan de la campagne
            }
            catch (Exception e)
            {
                _sErrorMsg = String.Concat("eCampaignFileRenderer.getCampaignReportPanel()>eBookmarkRenderer.CreateTitleBar : ", Environment.NewLine,
                    "bkm: null, sLibelle :", eResApp.GetRes(Pref, 6462), Environment.NewLine,
                    e.Message, Environment.NewLine,
                    e.StackTrace, Environment.NewLine);
                _eException = e;
                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, _sErrorMsg), Pref);

            }

            Panel pnInnerContent = new Panel();
            pnCampainReport.Controls.Add(pnInnerContent);

            System.Web.UI.WebControls.Table tb = new System.Web.UI.WebControls.Table();
            pnInnerContent.Controls.Add(tb);
            tb.CssClass = "camprep";

            TableRow tr = new TableRow();
            tb.Rows.Add(tr);


            Dictionary<CAMPAIGNSTATS_Category, String> diLabels = new Dictionary<CAMPAIGNSTATS_Category, String>();
            diLabels.Add(CAMPAIGNSTATS_Category.NB_TOTAL, eResApp.GetRes(Pref, 6465));

            if (_sendType != MAILINGSENDTYPE.EUDONET_SMS)
            {
                diLabels.Add(CAMPAIGNSTATS_Category.NB_SENT, eResApp.GetRes(Pref, 6324));
                diLabels.Add(CAMPAIGNSTATS_Category.NB_BOUNCE, eResApp.GetRes(Pref, 6464));
                diLabels.Add(CAMPAIGNSTATS_Category.NB_VIEW, eResApp.GetRes(Pref, 6467));
                diLabels.Add(CAMPAIGNSTATS_Category.NB_SINGLECLICKER, eResApp.GetRes(Pref, 6468));
                diLabels.Add(CAMPAIGNSTATS_Category.NB_UNSUBSCRIBE, eResApp.GetRes(Pref, 6466));
            }

            TableCell tc;
            TableCell tcValue;

            foreach (KeyValuePair<CAMPAIGNSTATS_Category, String> kvp in diLabels)
            {
                if (kvp.Key == CAMPAIGNSTATS_Category.NB_BOUNCE && (_sendType == MAILINGSENDTYPE.EUDONET || _sendType == MAILINGSENDTYPE.EUDONET_SMS))
                    continue;

                if (tr.Cells.Count >= NB_FIELDS_BY_LINE * NB_COLS_BY_FIELD) //Au début car sinon il rajoute une ligne mais si pas besoin.
                {
                    tr = new TableRow();
                    tb.Rows.Add(tr);
                }
                Int32 nb = 0;
                String sLabel = kvp.Value;
                _campaignReport.DiGlobalStats.TryGetValue(kvp.Key, out nb);

                tc = new TableCell();
                tr.Cells.Add(tc);
                tc.CssClass = "table_labels";

                tcValue = new TableCell();
                tr.Cells.Add(tcValue);
                tcValue.CssClass = "table_values";

                tc.Controls.Add(new LiteralControl(String.Concat(sLabel, " : ")));
                tcValue.Controls.Add(new LiteralControl(nb.ToString()));
            }

            while (tr.Cells.Count < tb.Rows[0].Cells.Count)
            {
                tr.Cells.Add(new TableCell());
            }
            /*Demande #29697 - [eCircle - Mode Fiche] - Bilan de la campagne - Supprimer les 2 encarts gris (enveloppes à droite) et aligner toutes les autres stats sur 1 ligne
            if (_sendType != MAILINGSENDTYPE.EUDONET)
            {
                // nombre de remis = NbTotal de destinataires - nb Non remis
                tc = new TableCell();
                tb.Rows[0].Cells.Add(tc);
                tc.CssClass = "recml";


                Panel divReceived1 = new Panel();
                divReceived1.CssClass = "recml1";
                tc.Controls.Add(divReceived1);

                Panel divReceived2 = new Panel();
                divReceived2.CssClass = "recml2";
                tc.Controls.Add(divReceived2);

                Panel divReceived3 = new Panel();
                divReceived3.CssClass = "recml3";
                tc.Controls.Add(divReceived3);



                Int32 nbTotal = 0;
                Int32 nbUnreceived = 0;
                _campaignReport.DiGlobalStats.TryGetValue(CAMPAIGNSTATS_Category.NB_TOTAL, out nbTotal);
                _campaignReport.DiGlobalStats.TryGetValue(CAMPAIGNSTATS_Category.NB_BOUNCE, out nbUnreceived);
                Int32 nbReceived = nbTotal - nbUnreceived;
                divReceived2.Controls.Add(new LiteralControl(nbReceived.ToString("# ### ###")));


                //Pourcentage d'ouverture = Nb de mails lu / nb de remis
                tc = new TableCell();
                tb.Rows[1].Cells.Add(tc);
                tc.CssClass = "pctopnml";

                Panel divPctOpening1 = new Panel();
                divPctOpening1.CssClass = "pctopnml1";
                tc.Controls.Add(divPctOpening1);

                Panel divPctOpening2 = new Panel();
                divPctOpening2.CssClass = "pctopnml2";
                tc.Controls.Add(divPctOpening2);

                Panel divPctOpening3 = new Panel();
                divPctOpening3.CssClass = "pctopnml3";
                tc.Controls.Add(divPctOpening3);

                if (nbReceived > 0)
                {
                    Int32 nbRead = 0;
                    _campaignReport.DiGlobalStats.TryGetValue(CAMPAIGNSTATS_Category.NB_VIEW, out nbRead);

                    Double _dPercent = (Double)nbRead / (Double)nbReceived;

                    divPctOpening2.Controls.Add(new LiteralControl(_dPercent.ToString("P")));
                }




            }
            */

            return pnCampainReport;
        }


        /// <summary>
        /// Génère le Cadre "Taux d'ouverture"
        /// </summary>
        /// <returns></returns>
        private eStatsRenderer GetReadingRateStatsRenderer()
        {
            eStatsRenderer statsRenderer = eStatsRenderer.CreateStatRenderer("Read", _campaignReport.DiReadingRate, Pref, new String[] { eResApp.GetRes(Pref, 5163), eResApp.GetRes(Pref, 1756) });


            return statsRenderer;
        }


        /// <summary>
        /// Génère le Cadre "Motifs de rejets"
        /// </summary>
        /// <returns></returns>
        private eStatsRenderer GetUnreceivedCauseStatsRenderer()
        {

            eStatsRenderer statsRenderer = eStatsRenderer.CreateStatRenderer("Unreceived", _campaignReport.DiUnreceiveCause, Pref, new String[] { eResApp.GetRes(Pref, 6476), eResApp.GetRes(Pref, 1756) }, sChartFormat: "Column2D", sValToDispOnChart: "");


            return statsRenderer;
        }


        /// <summary>
        /// Génère le Cadre "Désinscriptions"
        /// </summary>
        /// <returns></returns>
        private eStatsRenderer GetUnsubscribeStatsRenderer()
        {

            eStatsRenderer statsRenderer = eStatsRenderer.CreateStatRenderer("Unsubscribed", _campaignReport.DiUnsubscribed, Pref, new String[] { eResApp.GetRes(Pref, 6476), eResApp.GetRes(Pref, 1756) }, bDisplayDetail: false);


            return statsRenderer;
        }


        /// <summary>
        /// Génère le cadre "Liste des liens cliqués"
        /// </summary>
        /// <returns></returns>
        private eStatsRenderer GetClics()
        {
            eStatsRenderer statsRenderer = eStatsRenderer.CreateStatRenderer("Clics", _campaignReport.DiClics, Pref, new String[] { eResApp.GetRes(Pref, 1500), eResApp.GetRes(Pref, 6478) }, bDisplayChart: false);

            return statsRenderer;
        }

        /// <summary>
        /// Génère le Cadre "Répartitions des clics par jour"
        /// </summary>
        /// <returns></returns>
        private eStatsRenderer GetClicsPerDayStatsRenderer()
        {

            eStatsRenderer statsRenderer = eStatsRenderer.CreateStatRenderer("ClicsPerDay", _campaignReport.DiClicsPerDay, Pref, new String[] { eResApp.GetRes(Pref, 822), eResApp.GetRes(Pref, 6478) }, sChartFormat: "MSCombi3D", bDisplayDetail: false, bMulti: true, sValToDispOnChart: "value");
            return statsRenderer;
        }



        /// <summary>
        /// Encapsule le Panel Fourni par le statsRenderer dans un autre Panel en lui rajoutant une barre de titre
        /// </summary>
        /// <param name="sTitle">Titre du cadre de statistiques</param>
        /// <returns></returns>
        private Panel encapsulatedPanel(eStatsRenderer statsRenderer, String sTitle)
        {
            Panel pnChart = new Panel();

            if (statsRenderer == null || statsRenderer.PgContainer == null)
                return pnChart;

            pnChart.CssClass = "bkmdivCampaign campChartDiv";
            pnChart.ID = String.Concat("GDIV", statsRenderer.DivSuffix);
            try
            {
                pnChart.Controls.Add(eBookmarkRenderer.CreateTitleBar(Pref, sLibelle: sTitle, sDivId: pnChart.ID, bZoomButton: true));
            }
            catch (Exception e)
            {
                _sErrorMsg = String.Concat("eCampaignFileRenderer.encapsulatedPanel()>eBookmarkRenderer.CreateTitleBar : ", Environment.NewLine,
                    "bkm: null, sLibelle :", sTitle, "sDivId: ", pnChart.ID, ", bZoomButton: true", Environment.NewLine,
                    e.Message, Environment.NewLine,
                    e.StackTrace, Environment.NewLine);
                _eException = e;
                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, _sErrorMsg), Pref);

            }


            if (statsRenderer.Error.Length == 0)
                pnChart.Controls.Add(statsRenderer.PgContainer);
            else
            {
                Panel pn = new Panel();
                pn.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 6474)));
                pnChart.Controls.Add(pn);
            }

            return pnChart;

        }



        /// <summary>
        /// Ajoute la cellule contenant le bouton
        /// dans le cas du mode consultation, la cellule est vide
        /// et idem dans le cas ou le champ n'est pas modifiable
        /// </summary>
        /// <param name="cellValue">Cellule conteneur du btn</param>
        /// <param name="bDisplayBtn">Affichage ou pas du bouton</param>
        /// <param name="sIcon">Icon du bouton</param>
        /// <param name="sIconColor">Couleur de l'icon</param>
        /// <param name="field">Objet field pour information complémentaire</param>
        public override TableCell GetButtonCell(TableCell cellValue, Boolean bDisplayBtn, String sIcon = "", String sIconColor = "", Field field = null)
        {
            TableCell tcButton = new TableCell();
            tcButton.RowSpan = cellValue.RowSpan;
            tcButton.CssClass = ""; //"btn";

            if (!cellValue.HasControls())
                return tcButton;

            if (cellValue.Controls[0].GetType() != typeof(TextBox))
                return tcButton;


            TextBox txBoxValue = (TextBox)cellValue.Controls[0];


            //dans le cas de la campagne, le champ statut a un bouton particulier, les autres passent dans le fonctionnement habituel.
            if (txBoxValue.ID != _sStatusCellId)
                return base.GetButtonCell(cellValue, bDisplayBtn, sIcon, sIconColor);


            //[#44366] Pour la campagne sms, on retire les signets suivants car on ne gère pas les stats

            eFieldRecord sendType = _myFile.Record.GetFieldByAlias(TableType.CAMPAIGN.GetHashCode() + "_" + CampaignField.SENDTYPE.GetHashCode());


            switch (_campaignStatus)
            {
                case CampaignStatus.MAIL_IN_PREPARATION:
                    tcButton.CssClass += " icon-play4";

                    // Identification du statut de la demande.
                    String sParentTabIdAlias = String.Concat(TableType.CAMPAIGN.GetHashCode(), "_", CampaignField.PARENTTABID.GetHashCode());
                    String sParentFileIdAlias = String.Concat(TableType.CAMPAIGN.GetHashCode(), "_", CampaignField.PARENTFILEID.GetHashCode());
                    String sBkmTabIdAlias = String.Concat(TableType.CAMPAIGN.GetHashCode(), "_", CampaignField.BKMTABID.GetHashCode());


                    eFieldRecord fldParentTabId = _myFile.Record.GetFieldByAlias(sParentTabIdAlias);
                    eFieldRecord fldParentFileId = _myFile.Record.GetFieldByAlias(sParentFileIdAlias);
                    eFieldRecord flBkmTabId = _myFile.Record.GetFieldByAlias(sBkmTabIdAlias);

                    Int32 iParentTabId = 0;
                    Int32 iParentFileId = 0;
                    Int32 iBkmTabId = 0;

                    Int32 iStartTab = 0;

                    if (fldParentTabId == null)
                    {
                        eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, "eCampaignFileRenderer : Le Champ ParentTabId est absent de la Table Campaign"), Pref);
                    }
                    else
                    {

                        //Si on part d'un ++, la table de addmailing doit être celle de la table inviation, sinon, c'est la table parent
                        Int32.TryParse(fldParentTabId.Value, out iParentTabId);
                        Int32.TryParse(fldParentFileId.Value, out iParentFileId);
                        Int32.TryParse(flBkmTabId.Value, out iBkmTabId);


                        iStartTab = iBkmTabId > 0 ? iBkmTabId : iParentTabId;
                    }

                    tcButton.Attributes.Add("onclick", String.Concat("AddMailing(", iStartTab, ", ", EudoQuery.TypeMailing.MAILING_FROM_CAMPAIGN.GetHashCode(), ", ", _myFile.FileId, ", ", iParentTabId, ", ", iParentFileId, ")"));
                    break;
                case CampaignStatus.MAIL_DELAYED:
                    tcButton.CssClass += " icon-stop3";
                    // #39983 : Ajout de l'expéditeur
                    String senderAlias = String.Concat(TableType.CAMPAIGN.GetHashCode(), "_", CampaignField.SENDER.GetHashCode());
                    eFieldRecord record = _myFile.Record.GetFieldByAlias(senderAlias);
                    String sender = String.Empty;
                    if (record != null)
                    {
                        sender = record.Value;
                    }
                    tcButton.Attributes.Add("onclick", String.Concat("CancelDeleyedCampaign(", _myFile.FileId, ", '", sender, "')"));
                    break;
                case CampaignStatus.MAIL_SENT:
                case CampaignStatus.MAIL_ERROR:
                    tcButton.CssClass += " icon-eye";
                    if (sendType.Value == MAILINGSENDTYPE.EUDONET_SMS.GetHashCode().ToString())
                        tcButton.Attributes.Add("onclick", String.Concat("ReadSMSMailing(", EudoQuery.TypeMailing.MAILING_FROM_CAMPAIGN.GetHashCode(), ", ", _myFile.FileId, ")"));
                    else
                        tcButton.Attributes.Add("onclick", String.Concat("ReadMailing(", EudoQuery.TypeMailing.MAILING_FROM_CAMPAIGN.GetHashCode(), ", ", _myFile.FileId, ")"));
                    break;
                default:
                    break;
            }

            return tcButton;
        }
    }
}