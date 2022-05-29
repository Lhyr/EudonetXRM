using Com.Eudonet.Internal;
using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.import
{
    /// <summary>
    /// Classe de rendu pour la page de liste des template
    /// </summary>
    public class eImportTemplateListRenderer : eActionListRenderer
    {
        private Int32 _nTabTpl = 0;
        private Int32 _nPage = 1;

        /// <summary>
        /// Retourne une instance de eMailTemplateListRendrer
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="height">Hauteur du renderer</param>
        /// <param name="width">Largeur du renderer</param>
        /// <param name="nTabTpl">DescId du template mail</param>
        /// <param name="nPage">Numéro de page</param>
        /// <returns></returns>
        public static eImportTemplateListRenderer GetImportTemplateListRenderer(ePref pref, Int32 height, Int32 width, Int32 nTabTpl, Int32 nPage = 1)
        {
            eImportTemplateListRenderer myRenderer = new eImportTemplateListRenderer(pref, height, width, nTabTpl, nPage);
            myRenderer._tab = nTabTpl;
            myRenderer._page = nPage;
            myRenderer._rType = RENDERERTYPE.ImportTemplate;

            return myRenderer;
        }


        /// <summary>
        /// Constructeur privé 
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="height">Hauteur du renderer</param>
        /// <param name="width">Largeur du renderer</param>
        /// <param name="nTabTpl">DescId du template mail</param>
        /// <param name="nPage">Numéro de page</param>
        private eImportTemplateListRenderer(ePref pref, Int32 height, Int32 width, Int32 nTabTpl, Int32 nPage = 1)
            : base(pref)
        {
            _height = height;
            _width = width;
            _nTabTpl = nTabTpl;
            _rType = RENDERERTYPE.ImportTemplate;
            _nPage = nPage;

        }



        /// <summary>
        /// Vérifie les droits de traitement
        /// </summary>
        protected override void InitDrawButtonsAction()
        {
            this._drawBtnDuplicate = false;
            this._drawBtnEdit = false;
            this._drawBtnRename = true;
            this._drawBtnDelete = true;
            this._drawBtnTooltip = true;

        }

        /// <summary>
        /// Construction de la liste
        /// </summary>
        /// <returns>retourne true si l'opération a réussi</returns>
        protected override bool Init()
        {
            // On désactive les boutons actions non necessaire
            _drawBtnTooltip = false;
            _list = eListFactory.CreateImportTemplatetList(Pref, _nTabTpl, nPage: _nPage);

            if (!base.Init())
                return false;

            _rows = _list.RowsByPage;
            _page = _nPage;
            return true;
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
        /// Ajout des attributes specifiques sur le bouton d'edition (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionEdit(WebControl webCtrl, eRecord row)
        {
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
        /// pas de tooltip sur ce rendu
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionTooltip(WebControl webCtrl, eRecord row)
        {
            webCtrl.Attributes.Add("onmouseover", "eCurrentSelectedTemplateImport.ImportTemplateInternal.ShImportTemplateDesc(event,this);");
            webCtrl.Attributes.Add("onmouseout", "eCurrentSelectedTemplateImport.ImportTemplateInternal.ClearTimeOut();");

        }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de supp (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionDelete(WebControl webCtrl, eRecord row)
        {
            webCtrl.Attributes.Add("onclick", " eCurrentSelectedTemplateImport.ImportTemplateInternal.DeleteTemplate(event,this)");
        }



        /// <summary>
        /// Ajoute les specifités sur la row en fonction du rendu
        /// </summary>
        /// <param name="row">record</param>
        /// <param name="trRow">Objet tr courant</param>
        /// <param name="idxLine">index de la ligne</param>
        protected override void CustomTableRow(eRecord row, TableRow trRow, Int32 idxLine)
        {
            base.CustomTableRow(row, trRow, idxLine);
            trRow.Attributes.Add("onclick", " eCurrentSelectedTemplateImport.ImportTemplateInternal.SelectTemplate(this);");
            trRow.Attributes.Add("ondblclick", "eCurrentSelectedTemplateImport.ImportTemplateInternal.ApplayTemplateParams(this);return false;");
            trRow.Attributes.Add("mtid", row.MainFileid.ToString());

        }

    }
}