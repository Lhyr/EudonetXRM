using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu pour la page de liste des PJ dans la fenêtre de sélection 
    /// </summary>
    public class ePjListRenderer : eActionListRenderer
    {
        protected int _nFileId = 0;
        protected ePjList _pjList;

        protected ePJToAdd _attachment;

        public ePJToAdd Attachment
        {
            get { return _attachment; }
        }


        /// <summary>
        /// Liste des pjids en cas de nouvelle fiche (fileid = 0)
        /// </summary>
        internal string InitialPjIds { get; set; }

        /// <summary>
        /// Dictionnaire contenant les PjIds et le nom de l'annexe / utilisé pour les Mails
        /// </summary>
        public Dictionary<int, string> DicoPj
        {
            get { return _pjList.DicoPj; }
        }

        /// <summary>
        /// Indique si l'ajout d'annexe est autorisé depuis cet onglet
        /// </summary>
        public bool IsAddAllowed
        {
            get { return _pjList.IsAddAllowed; }
        }


        /// <summary>
        /// Retourne une instance de eFilterReportListRenderer
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="nFileId">FileId de la fiche</param>
        /// <param name="nTab">DescId de la fiche template parente</param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static ePjListRenderer GetPjListRenderer(ePref pref, ePJToAdd attachment, int height, int width)
        {
            ePjListRenderer myRenderer = new ePjListRenderer(pref, attachment);
            myRenderer._height = height;
            myRenderer._width = width;
            myRenderer._tab = attachment.Tab;
            myRenderer._nFileId = attachment.FileId;

            // Droits de traitements 
            myRenderer.RightManager = new eRightAttachment(pref, attachment.Tab);

            return myRenderer;
        }

        /// <summary>
        /// Constructeur privé du renderer
        /// Pour obtenir une instance de l'objet, utiliser GetFilterReportListRenderer
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        protected ePjListRenderer(ePref pref, ePJToAdd attachment)
            : base(pref)
        {
            _rType = RENDERERTYPE.PjList;
            _attachment = attachment;

        }

        /// <summary>
        /// Initialise les objets d'accès aux données
        /// </summary>
        /// <returns>retourne true si l'opération a réussi</returns>
        protected override bool Init()
        {

            _list = eListFactory.CreatePjList(Pref, _attachment, InitialPjIds);

            if (_list.ErrorMsg.Length > 0)
                throw _list.InnerException ?? new Exception(_list.ErrorMsg);

            _pjList = (ePjList)_list;
            if (!base.Init())
                return false;

            // On désactive les boutons actions non necessaire aux annexes
            _drawBtnEdit = false;
            _drawBtnDuplicate = false;
            _drawBtnRename = false;

            // On peut supprimer l'annexe qui n'a pas encore une fiche parente #66465
            if (_nFileId == 0)
                _drawBtnDelete = true;

            // Pas de paging sur ces mode de liste
            _rows = _list.ListRecords.Count;


            return true;
        }

        /// <summary>
        /// Traitement de fin de génération
        /// Ajuste les colonnes
        /// </summary>
        /// <returns>returne true si l'enregistrement a réussi</returns>
        protected override bool End()
        {
            bool bReturn = base.End();
            if (!bReturn)
                return false;


            if (!_pjList.IsUpdateAllowed)
                _divmt.Attributes.Add("ro", "1");

            _divmt.Attributes.Add("sup", _pjList.IsDeleteAllowed ? "1" : "0");

            return bReturn;
        }

        /// <summary>
        /// méthode à override pour décider s'il faut ou non afficher les cases à cocher
        /// </summary>
        /// <returns></returns>
        protected override bool DisplayCheckBox()
        {
            return false;
        }


        #region Compléments

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
        protected override void BtnActionRename(WebControl webCtrl, string sElementValueId, eRecord row)
        {

        }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de tooltip (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionTooltip(WebControl webCtrl, eRecord row)
        {
            eRecordPJ rowPj = (eRecordPJ)row;

            if (rowPj.ToolTip != null && rowPj.ToolTip.Length != 0)
            {
                webCtrl.Attributes.Add("onmouseover", string.Concat("st(this, '", rowPj.ToolTip.Replace("'", @"\'"), "');"));
                webCtrl.Attributes.Add("onmouseout", "ht();");
            }
            else
            {
                webCtrl.CssClass = "logo_info_inactif";
            }
        }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de supp (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionDelete(WebControl webCtrl, eRecord row)
        {
            //Les pj ajoutées au mail transféré ont des fileId=0
            webCtrl.Attributes.Add("onclick", string.Concat("DeletePJ('", _tab, "', '", _attachment.MailForwarded ? 0 : _nFileId, "', '", row.MainFileid, "', true);"));
        }

        /// <summary>
        /// Ajoute les specifités sur la cell en fonction du rendu
        /// </summary>
        /// <param name="row">record</param>
        /// <param name="trRow">Objet tr courant</param>
        /// <param name="fieldRow">field record</param>
        /// <param name="trCell">Objet cellule courant</param>
        /// <param name="idxCell">index de la colonne</param>
        protected override void CustomTableCell(eRecord row, TableRow trRow, eFieldRecord fieldRow, TableCell trCell, int idxCell)
        {
            base.CustomTableCell(row, trRow, fieldRow, trCell, idxCell);

        }

        /// <summary>
        /// Ajoute les specifités sur la row en fonction du rendu
        /// </summary>
        /// <param name="row">record</param>
        /// <param name="trRow">Objet tr courant</param>
        /// <param name="idxLine">index de la ligne</param>
        protected override void CustomTableRow(eRecord row, TableRow trRow, int idxLine)
        {
            eRecordPJ rowPJ = (eRecordPJ)row;

            trRow.Attributes.Add("fid", rowPJ.PJFileID.ToString());
            trRow.Attributes.Add("tab", rowPJ.PJTabDescID.ToString());
        }



        /// <summary>
        /// retourne la classe css adéquate pour encapsuler les boutons d'actions
        /// </summary>
        /// <returns></returns>
        protected override string GetActionCssClass()
        {
            return "logo_modifs";
        }

        #endregion
    }


}