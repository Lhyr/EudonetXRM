using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu pour la page de liste des filtres
    /// </summary>
    public class eFilterListRenderer : eActionListRenderer
    {
        // Taile de l'écran
        // kha annulé car déjà hérité de eListRendererMain
        //private int _height = eConst.DEFAULT_WINDOW_WIDTH;
        //private int _width = eConst.DEFAULT_WINDOW_HEIGHT;

        private EudoQuery.TypeFilter _typeFilter = TypeFilter.USER;
        private bool _deselectAllowed = false;
        private bool _adminMode = false;
        private bool _selectFilterMode = false;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {



            return base.End();
        }



        /// <summary>
        /// Peuple un panel avec les boutons pour le paging dans la liste des filtres/report
        ///   (uniquement filtre au 07/04/2014)
        /// </summary>
        /// <returns></returns>
        public override void CreatePagingBar(HtmlGenericControl pnTitle)
        {
            base.CreatePagingBar(pnTitle);
        }

        #region accesseurs

        /// <summary>
        /// Type de Filtre pour la liste des filtres
        /// </summary>
        public EudoQuery.TypeFilter FilterType
        {
            get { return _typeFilter; }
            set { _typeFilter = value; }
        }



        #endregion



        /// <summary>
        /// identifie les paramètres de pagination
        /// Les listes filtre
        /// </summary>
        protected override void SetPagingInfo()
        {

            _tblMainList.Attributes.Add("eNbCnt", _list.NbTotalRows.ToString());
            _tblMainList.Attributes.Add("eNbTotal", _list.NbTotalRows.ToString());

            _tblMainList.Attributes.Add("eHasCount", "1");
            _tblMainList.Attributes.Add("nbPage", _list.NbPage.ToString());
            _tblMainList.Attributes.Add("cnton", "0");
        }

        /// <summary>
        /// Retourne un renderer pour une liste de report/filtre
        /// </summary>
        /// <param name="pref">préférence utilisateur</param>
        /// <param name="height">Hauteur du conteneur</param>
        /// <param name="width">Largeur du conteneur</param>
        /// <param name="nTab">Table</param>
        /// <param name="nPage">Numéro de page</param>
        /// <param name="bAddPublicItem">indique s'il faut rajouter les filtres publics. (true par défaut)</param>
        /// <returns></returns>
        public static eFilterListRenderer GetFilterListRenderer(ePref pref, int height, int width, int nTab, IRightTreatment oRightManager, int nPage = 1,
            Boolean bAddPublicItem = true, bool deselectAllowed = false, bool adminMode = false, bool selectFilterMode = false)
        {
            eFilterListRenderer myRenderer = new eFilterListRenderer(pref, height, width);
            myRenderer._tab = nTab;
            myRenderer._page = nPage;
            myRenderer.RightManager = oRightManager;
            myRenderer._deselectAllowed = deselectAllowed;
            myRenderer._adminMode = adminMode;
            myRenderer._selectFilterMode = selectFilterMode;

            return myRenderer;
        }

        /// <summary>
        /// Constructeur privé du renderer
        /// Pour obtenir une instance de l'objet, utiliser GetFilterReportListRenderer
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        protected eFilterListRenderer(ePref pref, int height, int width)
            : base(pref)
        {
            _height = height;
            _width = width;
            _rType = RENDERERTYPE.FilterReportList;
        }


        /// <summary>
        /// Génération d'une liste de filtre pour la sélection/modification d'un filtre
        /// </summary>
        protected override void GenerateList()
        {
            _list = eListFactory.CreateFilterList(Pref, _tab, _page, FilterType);
        }

        /// <summary>
        /// Initialise les objets d'accès aux données
        /// </summary>
        /// <returns>retourne true si l'opération a réussi</returns>
        protected override bool Init()
        {
            // Récupèration des droits
            if (RightManager == null)
                RightManager = new eRightFilter(Pref);

            //pas de bouton duppliqué
            _drawBtnDuplicate = false;

            if (!base.Init())
                return false;

            // Pas de paging sur ces mode de liste
            // if (!IsInvit)
            //_rows = _list.ListRecords.Count;
            // sph : 24/03/2014  - en fait, si
            _rows = _list.RowsByPage;

            return true;
        }



        #region Compléments

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton d'edition (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionEdit(WebControl webCtrl, eRecord row)
        {
            string adminMode = _adminMode ? "true" : "false";
            string selectFilterMode = _selectFilterMode ? "true" : "false";
            webCtrl.Attributes.Add("onclick", String.Concat("editFilterV2(", row.MainFileid, ", ", row.ViewTab, ", null, false, ", adminMode, ", ", selectFilterMode ,")"));
        }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de duplication (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionDuplicate(WebControl webCtrl, eRecord row)
        {

        }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de rename (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="sElementValueId">Id de l'élément dont il faut modifier le contenu</param>
        /// <param name="row">record</param>
        protected override void BtnActionRename(WebControl webCtrl, String sElementValueId, eRecord row)
        {
            String sElementId = sElementValueId.Length > 0 ? sElementValueId : webCtrl.ID;
            webCtrl.Attributes.Add("onclick", String.Concat("renFilter('" + sElementId + "', this);"));
            webCtrl.Attributes.Add("ondblclick", String.Concat("stopEvent();return false;"));
        }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de tooltip (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionTooltip(WebControl webCtrl, eRecord row)
        {
            webCtrl.Attributes.Add("onmouseover", "shFilterDesc(event, this);");
            webCtrl.Attributes.Add("onmouseout", "clearTimeout(toTT); ht();");
        }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de supp (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionDelete(WebControl webCtrl, eRecord row)
        {
            webCtrl.Attributes.Add("onclick", "delFilter(" + row.MainFileid.ToString() + ", " + _tab.ToString() + ")");
        }

        /// <summary>
        /// Ajoute les specifités sur la row en fonction du rendu
        /// </summary>
        /// <param name="row">record</param>
        /// <param name="trRow">Objet tr courant</param>
        /// <param name="idxLine">index de la ligne</param>
        protected override void CustomTableRow(eRecord row, TableRow trRow, int idxLine)
        {
            //Cas ++ nous n'avons pas besoin des evenements JS
            if (_typeFilter == TypeFilter.USER)
                trRow.Attributes.Add("ondblclick", String.Concat("onFilterDblClick(this)"));

            trRow.Attributes.Add("onclick", "selectLine(this, " + (_deselectAllowed ? "true" : "false") + ");");

            trRow.Attributes.Add("eft", _tab.ToString());

        }

        #endregion
    }
}