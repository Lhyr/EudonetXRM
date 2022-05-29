using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu pour la page de liste des rapports
    /// </summary>
    public class eReportListRenderer : eActionListRenderer
    {
        // Taile de l'écran
        // kha annulé car déjà hérité de eListRendererMain
        //private int _height = eConst.DEFAULT_WINDOW_WIDTH;
        //private int _width = eConst.DEFAULT_WINDOW_HEIGHT;

        /// <summary>
        /// Peut-on désélectionner une ligne ?
        /// </summary>
        private bool _deselectAllowed = false;

        private eRightReport RightManagerReport;

        #region accesseurs

        /// <summary>
        /// Type de Filtre pour la liste des rapports
        /// <example>
        /// TypeReport.PRINT : Impression
        /// TypeReport.EXPORT : Export
        /// </example>
        /// </summary>
        public TypeReport ReportType { get; set; }


        /// <summary>
        /// FileId de la fiche depuis laquelle le mode fiche est lancé
        /// </summary>
        public int FileId { get; set; }

        #endregion

        /// <summary>
        /// Retourne une instance de eFilterReportListRenderer
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="height">Hauteur du conteneur</param>
        /// <param name="width">Largeur du conteneur</param>
        /// <param name="nTab">Table</param>
        /// <param name="nPage">Numéro de page</param>
        /// <returns></returns>
        public static eReportListRenderer GetReportListRenderer(ePref pref, int height, int width, int nTab, IRightTreatment oRightManager, int nPage = 1, bool deselectAllowed = false)
        {
            eReportListRenderer myRenderer = new eReportListRenderer(pref, height, width);
            myRenderer._tab = nTab;
            myRenderer._page = nPage;
            myRenderer.RightManager = oRightManager;
            myRenderer._deselectAllowed = deselectAllowed;

            return myRenderer;
        }

        /// <summary>
        /// Constructeur privé du renderer
        /// Pour obtenir une instance de l'objet, utiliser GetFilterReportListRenderer
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        ///  <param name="height">Hauteur du conteneur</param>
        /// <param name="width">Largeur du conteneur</param>
        private eReportListRenderer(ePref pref, int height, int width)
            : base(pref)
        {
            _height = height;
            _width = width;
            _rType = RENDERERTYPE.FilterReportList;
        }

        /// <summary>
        /// Initialise les objets d'accès aux données
        /// </summary>
        /// <returns>retourne true si l'opération a réussi</returns>
        protected override bool Init()
        {
            // Récupèration des droits 
            if (RightManager == null)
                RightManager = new eRightReport(Pref, this.ReportType);

            RightManagerReport = RightManager as eRightReport;

            _list = eListFactory.CreateReportList(Pref, _tab, ReportType == TypeReport.FIELDCHART ? TypeReport.CHARTS : ReportType, FileId);

            if (!base.Init())
                return false;

            //ALISTER Permet de vérifier si c'est un rapport de type graphique, si oui, on cache "IS_SCHEDULED" /
            //Check if it's report of charts type, if yes, we hide "IS_SCHEDULED"
            if (ReportType != TypeReport.EXPORT &&
                ReportType != TypeReport.PRINT &&
                ReportType != TypeReport.MERGE)
            {
                for (int i = 0; i < _list.FldFieldsInfos.Count; i++)
                {
                    if (_list.FldFieldsInfos[i].Descid == Convert.ToInt32(ReportField.IS_SCHEDULED))
                    {
                        _list.FldFieldsInfos.RemoveAt(i);
                    }
                }

            }

            // Pas de paging sur ces mode de liste
            _rows = _list.ListRecords.Count;

            return true;
        }

        #region Compléments

        /// <summary>
        /// Vérifie les droit de traitement sur la ligne
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        protected override bool HasRight(eRecord row)
        {
            /*0
             *  Pour le publipostage, on peut avoir les droits de traitement sur les pdf par exemple et pas sur word ou autres,
             *  les boutons d'action donc s'affichent ou pas en conséquence
             */
            if (this.ReportType == TypeReport.MERGE
                || this.ReportType == TypeReport.EXPORT
                || this.ReportType == TypeReport.ALLFORWIZARD)
            {
                eRecordReport rowReport = (eRecordReport)row;
                ReportFormat formatCurrentReport = rowReport.RepFormat;
                TypeReport typeCurrentReport = rowReport.RepType;

                // Si publipostage
                // ou si le type du report de la ligne en cours est du publipostage
                // ou si le type du report de la ligne en cours est de l'export et en format POWERBI - #63 663 - Droits spécifiques pour Power BI
                if (this.ReportType == TypeReport.MERGE
                    || (this.ReportType == TypeReport.ALLFORWIZARD && typeCurrentReport == TypeReport.MERGE)
                    || (typeCurrentReport == TypeReport.EXPORT && formatCurrentReport == ReportFormat.POWERBI))
                {
                    this._drawBtnEdit = RightManagerReport.HasRightByFormat(eRightReport.RightByFormat.Operation.EDIT, formatCurrentReport);
                    this._drawBtnRename = RightManagerReport.HasRightByFormat(eRightReport.RightByFormat.Operation.RENAME, formatCurrentReport);
                    this._drawBtnDelete = RightManagerReport.HasRightByFormat(eRightReport.RightByFormat.Operation.DELETE, formatCurrentReport);
                    return RightManagerReport.HasRightByFormat(eRightReport.RightByFormat.Operation.GLOBAL, formatCurrentReport);
                }
                else if (this.ReportType == TypeReport.ALLFORWIZARD)
                {
                    RightManagerReport.Typ = typeCurrentReport;

                    // #42881 CRU : Si on est sur "Sélectionnez un rapport : Tous", on retourne les droits par rapport à chaque type de rapport
                    if (typeCurrentReport == TypeReport.EXPORT
                        || typeCurrentReport == TypeReport.CHARTS
                        || typeCurrentReport == TypeReport.PRINT
                        || typeCurrentReport == TypeReport.PRINT_FILE)
                    {
                        this._drawBtnEdit = RightManagerReport.CanEditItem();
                        this._drawBtnRename = RightManagerReport.CanRenameItem();
                        this._drawBtnDelete = RightManagerReport.CanDeleteItem();
                    }

                    switch (typeCurrentReport)
                    {
                        case TypeReport.EXPORT:
                            return RightManagerReport.HasRight(eLibConst.TREATID.EXPORT);

                        case TypeReport.CHARTS:
                            return RightManagerReport.HasRight(eLibConst.TREATID.GRAPHIQUE);

                        case TypeReport.PRINT:
                        case TypeReport.PRINT_FILE:
                            return RightManagerReport.HasRight(eLibConst.TREATID.PRINT);

                        case TypeReport.FAXING:
                            return RightManagerReport.HasRight(eLibConst.TREATID.FAXING);
                    }
                }
            }

            //pour les autres reports
            return true;
        }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton d'edition (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionEdit(WebControl webCtrl, eRecord row)
        {
            eRecordReport rowReport = row as eRecordReport;

            //ASY #22215 : [Dev] - Ajouter Tous dans l'assistant Rapport - prise en compte du type tous=99
            //  permet que le type de rapport de chaque ligne soit identifié ( utile dans le cas ou on choisi tous=99)
            //webCtrl.Attributes.Add("onclick", string.Concat("editReport(", row.MainFileid, ", ", ReportType.GetHashCode().ToString(), ")"));
            if (!row.RightIsUpdatable)
                return;

            webCtrl.Attributes.Add("onclick", string.Concat("editReport(", row.MainFileid, ", ", ((int)rowReport.RepType).ToString(), ")"));
        }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de duplication (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionDuplicate(WebControl webCtrl, eRecord row)
        {
            webCtrl.Attributes.Add("onclick", "duplicateReport('" + webCtrl.ID + "', this);");
        }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de rename (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="sElementValueId">Id de l'élément dont il faut modifier le contenu</param>
        /// <param name="row">record</param>
        protected override void BtnActionRename(WebControl webCtrl, string sElementValueId, eRecord row)
        {
            string sElementId = sElementValueId.Length > 0 ? sElementValueId : webCtrl.ID;
            webCtrl.Attributes.Add("onclick", "renReport('" + sElementId + "', this);");
        }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de tooltip (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionTooltip(WebControl webCtrl, eRecord row)
        {
            webCtrl.Attributes.Add("onmouseover", "shReportDesc(event, this);");
            webCtrl.Attributes.Add("onmouseout", "clearTimeout(toTT); ht();");

        }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de supp (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionDelete(WebControl webCtrl, eRecord row)
        {
            webCtrl.Attributes.Add("onclick", "delReport(" + row.MainFileid.ToString() + ")");
        }

        /// <summary>
        /// Ajoute les specifités sur la row en fonction du rendu
        /// </summary>
        /// <param name="row">record</param>
        /// <param name="trRow">Objet tr courant</param>
        /// <param name="idxLine">index de la ligne</param>
        protected override void CustomTableRow(eRecord row, TableRow trRow, int idxLine)
        {
            _drawBtnTooltip = true;

            //Vérifie si le rapport n'est pas associé à un filtre formulaire
            bool bConfirmTitle = false;
            string reportTitle = "";
            AdvFilter reportFilter = null;

            try
            {
                eRecordReport rowReport = row as eRecordReport;

                //Confirmer le title
                if (rowReport.RepType == TypeReport.PRINT)
                {
                    bConfirmTitle = rowReport.GetParamBool("modifytitle");
                    reportTitle = rowReport.GetParamValue("title");
                }

                // Chargement des informations sur le filtre
                int filterId = rowReport.GetParamInt("filterid");
                if (filterId > 0)
                {
                    string error = string.Empty;
                    AdvFilter ef = new AdvFilter(_ePref, filterId);
                    if (AdvFilter.Load(_ePref, ef, out error) && error.Length == 0)
                    {
                        reportFilter = ef;
                        if (ef.IsQuestionFilter)
                            trRow.Attributes.Add("hasform", string.Format("{0}|{1}", ef.FilterTab, ef.FilterId));
                    }
                }

                //ajout de l'action sur le double clic
                if (ReportType != TypeReport.FIELDCHART)
                    AddDblClicAction(row, trRow, reportTitle, bConfirmTitle, reportFilter);

                // 
                trRow.Attributes.Add("onclick", "selectLine(this, " + (_deselectAllowed ? "true" : "false") + ");");
                trRow.Attributes.Add("eFT", _tab.ToString());

                // ASY #22515 : [Dev] - Ajouter Tous dans l'assistant Rapport - prise en compte du type tous=99
                // permet de recupérer le type pour chaque ligne, pour lancer le bon assistant, lors de la modif si on est sur afficher tous
                // MOU #31836 : [Bug] - L'export, publipostage et impression de "liste en cours" se fait toujours en mode impression quelque soit le type choisi    
                if (ReportType == TypeReport.ALLFORWIZARD || ReportType == TypeReport.FIELDCHART)
                    //on prend celui recupéré depuis l'objet Report
                    trRow.Attributes.Add("typ", ((int)rowReport.RepType).ToString());
                else
                    //on prend le type selectionné dans la liste
                    trRow.Attributes.Add("typ", this.ReportType.GetHashCode().ToString());
            }
            catch (Exception e)
            {
                throw new Exception(string.Concat("Impossible de rechercher les informations sur le report ", row.MainFileid), e);
            }
        }

        /// <summary>
        /// Ajoute l'action du double clic sur la ligne
        /// </summary>
        /// <param name="row">record</param>
        /// <param name="trRow">Objet tr courant</param>
        /// <param name="reportTitle">Titre, pour la confirmation du titre des rapports d'impression</param>
        /// <param name="bConfirmTitle">Active la confirmation du titre des rapports d'impression</param>
        /// <param name="reportFilter">Filtre du graphique, pour afficher les filtres formulaires</param>
        protected virtual void AddDblClicAction(eRecord row, TableRow trRow, string reportTitle, bool bConfirmTitle, AdvFilter reportFilter)
        {
            eRecordReport rowReport = (eRecordReport)row;

            #region  Double click pour lancer le rapport

            if (rowReport.IsSpecif())
            {
                trRow.Attributes.Add("ondblclick", string.Concat("exportToLinkToV7(", row.MainFileid, ",0,", eLibConst.SPECIF_TYPE.TYP_REPORT.GetHashCode(), ")"));
                trRow.Attributes.Add("spec", "1");

                // ASY (26782) :  Pour les etats specifiques on n'affiche pas le bouton infos
                _drawBtnTooltip = false;
            }
            else
            {
                // ASY #22215 : [Dev] - Ajouter Tous dans l'assistant Rapport - prise en compte du type tous=99
                if (rowReport.RepType == TypeReport.CHARTS)
                {
                    if (reportFilter?.IsQuestionFilter ?? false)
                        trRow.Attributes.Add("ondblclick", string.Concat("doFormularFilter(", reportFilter.FilterTab.ToString(), ", ", reportFilter.FilterId.ToString(), ", 1);"));
                    else
                        trRow.Attributes.Add("ondblclick", string.Concat("displayChart(", row.MainFileid, ");"));
                }
                else if (rowReport.RepType == TypeReport.SPECIF)
                {
                    int nSpecID;
                    int.TryParse(rowReport.Param, out nSpecID);

                    trRow.Attributes.Add("ondblclick", string.Concat("runSpec(", nSpecID, ",", _tab, ");"));
                    trRow.Attributes.Add("specid", nSpecID.ToString());
                }
                else
                {
                    if (bConfirmTitle)
                    {
                        // Pour les rapport d'impression, on a la possibilité de confirmer le titre
                        //trRow.Attributes.Add("ondblclick", "ConfirmTitle(this);");
                        trRow.Attributes.Add("confirm", "1");
                        trRow.Attributes.Add("dbv", reportTitle);
                    }

                    if (rowReport.RepFormat == ReportFormat.POWERBI)
                        trRow.Attributes.Add("ondblclick", string.Concat("showReportInformation(", row.MainFileid, ");"));
                    else
                        trRow.Attributes.Add("ondblclick", string.Concat("CallExecuteInsert();"));

                }
            }
            #endregion
        }
        #endregion
    }



}