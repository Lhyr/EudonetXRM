using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using System.Collections.Generic;
using Com.Eudonet.Core.Model;
using System.Web.UI;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu pour la page de liste des filtres
    /// </summary>
    public class eFormularListRenderer : eActionListRenderer
    {

        Boolean _bHideActionBtn = false;


        private eFormularListFilterParams _formularFilter = new eFormularListFilterParams();

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

        /// <summary>
        /// identifie les paramètres de pagination
        /// Les listes filtre
        /// </summary>
        protected override void SetPagingInfo()
        {
            base.SetPagingInfo();
        }

        /// <summary>
        /// Retourne un renderer pour une liste de report/filtre
        /// </summary>
        /// <param name="pref">préférence utilisateur</param>
        /// <param name="height">Hauteur du conteneur</param>
        /// <param name="width">Largeur du conteneur</param>
        /// <param name="nTab">Table</param>
        /// <param name="oRightManager">Gestionnaire de droits</param>
        /// <param name="nPage">Numéro de page</param>
        /// <param name="filter">Filtre sur les formualires</param>
        /// <param name="bHideActionBtn">Masquer les actions qui ne doivent pas être accessible dans certains modes</param>
        /// <returns></returns>
        public static eFormularListRenderer GetFormularListRenderer(ePref pref, Int32 height, Int32 width, Int32 nTab, IRightTreatment oRightManager, eFormularListFilterParams filter, Int32 nPage = 1, Boolean bHideActionBtn = false)
        {
            eFormularListRenderer myRenderer = new eFormularListRenderer(pref, height, width, filter);
            myRenderer.RightManager = oRightManager;
            myRenderer._tab = nTab;
            myRenderer._page = nPage;
            myRenderer._bHideActionBtn = bHideActionBtn;
            myRenderer._rType = RENDERERTYPE.FilterReportList;
            return myRenderer;
        }

        /// <summary>
        /// Constructeur privé du renderer
        /// Pour obtenir une instance de l'objet, utiliser GetFormularListRenderer
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        protected eFormularListRenderer(ePref pref, Int32 height, Int32 width, eFormularListFilterParams filter)
            : base(pref)
        {
            _height = height;
            _width = width;

            _formularFilter = filter;
        }



        /// <summary>
        /// Vérifie les droits de traitement
        /// </summary>
        protected override void InitDrawButtonsAction()
        {
            base.InitDrawButtonsAction();

            //Masquer les actions qui ne doivent pas être accessible dans certains modes
            if (_bHideActionBtn)
            {
                _drawBtnEdit = false;
                _drawBtnDuplicate = false;
                _drawBtnRename = false;
                _drawBtnDelete = false;

            }
            _drawBtnTooltip = false;
        }

        /// <summary>
        /// Génération d'une liste de filtre pour la sélection/modification d'un filtre
        /// </summary>
        protected override void GenerateList()
        {
            _list = eListFactory.CreateFormularList(Pref, _tab, _page, _formularFilter);
        }

        /// <summary>
        /// Initialise les objets d'accès aux données
        /// </summary>
        /// <returns>retourne true si l'opération a réussi</returns>
        protected override bool Init()
        {
            // Récupèration des droits
            if (RightManager == null)
                RightManager = new eRightFormular(Pref);

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
            webCtrl.Attributes.Add("onclick", String.Concat("editForm(", row.MainFileid, ", ", row.ViewTab, ",0," + (_formularFilter.FormularType != FORMULAR_TYPE.TYP_ADVANCED ? " 0" : "1") + ")"));
        }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de duplication (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionDuplicate(WebControl webCtrl, eRecord row)
        {
            webCtrl.Attributes.Add("onclick", String.Concat("duplicateFormular(", row.MainFileid, ")"));
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
            webCtrl.Attributes.Add("onclick", String.Concat("renFormular('" + sElementId + "', this);"));
            webCtrl.Attributes.Add("ondblclick", String.Concat("stopEvent();return false;"));
        }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de tooltip (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionTooltip(WebControl webCtrl, eRecord row)
        {
            webCtrl.Attributes.Add("onmouseover", "shFormularDesc(event, this);");
            webCtrl.Attributes.Add("onmouseout", "ht();");
        }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de supp (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionDelete(WebControl webCtrl, eRecord row)
        {
            webCtrl.Attributes.Add("onclick", "delFormular(" + row.MainFileid.ToString() + ")");
        }

        /// <summary>
        /// Ajoute les specifités sur la row en fonction du rendu
        /// </summary>
        /// <param name="row">record</param>
        /// <param name="trRow">Objet tr courant</param>
        /// <param name="idxLine">index de la ligne</param>
        protected override void CustomTableRow(eRecord row, TableRow trRow, Int32 idxLine)
        {
            //double clique action spécifique aux Formulaires
            if (_formularFilter.FormularType != FORMULAR_TYPE.TYP_ADVANCED)
                trRow.Attributes.Add("advf", "0");
            else
                trRow.Attributes.Add("advf", "1");

            trRow.Attributes.Add("ondblclick", "DblClickFormular(this)");
            trRow.Attributes.Add("onclick", "selectLine(this);");

            trRow.Attributes.Add("eft", row.ViewTab.ToString());
        }


        /// <summary>
        /// Retourne le control contenant toutes les actions de la ligne
        /// </summary>
        /// <param name="row">Ligne sur laquel on generé les actions</param>
        /// <param name="sElementValueId">Id de l'élément dont il faut modifier le contenu notament dans le cas du renommage</param>
        /// <returns>retourne le control</returns>
        protected override Control GetActionButton(eRecord row, String sElementValueId = "")
        {
            Panel div;
            int nNbAction = 0;
            bool btnActionDel = false;

            Panel divAction = new Panel();
            divAction.CssClass = GetActionCssClass();
            ;

            //Dernier filtre non sauvegardé
            bool bIsLastUnsavedFilter = (row.GetFields[0].Value.Length == 0);

            //EDITER
            if (_drawBtnEdit && row.RightIsUpdatable)
            {
                div = new Panel();
                div.CssClass = "icon-edn-pen";
                div.ToolTip = eResApp.GetRes(Pref, 151);
                BtnActionEdit(div, row);
                divAction.Controls.Add(div);
                nNbAction++;
            }

            

            if (_drawBtnRename && !bIsLastUnsavedFilter && row.RightIsUpdatable)
            {
                //RENOMMER
                div = new Panel();
                div.CssClass = "icon-abc";
                div.ToolTip = eResApp.GetRes(Pref, 86);
                BtnActionRename(div, sElementValueId, row);
                divAction.Controls.Add(div);
                nNbAction++;
            }

            //Dans le cas de liste en cours ou d'etat specifiques on n'affiche pas le bouton 
            if (_drawBtnTooltip && row.MainFileid != -1)
            {
                //TOOLTIP
                div = new Panel();
                div.CssClass = "icon-edn-info";
                BtnActionTooltip(div, row);
                divAction.Controls.Add(div);
                nNbAction++;
            }


            //SUPPRIMER
            // HLA - Afficher la poubelle pour les derniers filtres non sauvegardés - Backlog 675
            if (_drawBtnDelete && row.RightIsUpdatable)
            {
                div = new Panel();
                div.CssClass = "icon-delete";
                div.ToolTip = eResApp.GetRes(Pref, 19);
                BtnActionDelete(div, row);
                divAction.Controls.Add(div);
                nNbAction++;
                btnActionDel = true;
            }

            //DUPLIQUER
            //toujours visible sauf pour les listes SMS et Mailing
            if(_formularFilter.FormularType != FORMULAR_TYPE.TYP_UNSPECIFIED)
            {
                div = new Panel();
                div.CssClass = "icon-duplicate";
                div.ToolTip = eResApp.GetRes(Pref, 534); // dupliquer
                BtnActionDuplicate(div, row);
                divAction.Controls.Add(div);
                nNbAction++;
            }

            int width = nNbAction * 24;
            if (btnActionDel)       // Le margin du btn sup
                width += 8;

            if (nNbAction > _nNbMaxAction)
                _nNbMaxAction = nNbAction;

            if (width > 0)
            {
                return divAction;
            }
            else
                return null;
        }


        #endregion
    }



}