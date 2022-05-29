using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <className>eActionListRenderer</className>
    /// <summary>Classe de rendu du mode liste avec boutons d'actions sur la première colonne
    /// </summary>
    /// <authors>HLA</authors>
    /// <date>2013-10-11</date>
    public abstract class eActionListRenderer : eListMainRenderer
    {
        protected int _nNbMaxAction = 0;

        /// <summary>Affichage du bouton d'edition</summary>
        protected bool _drawBtnEdit = true;

        /// <summary>Affichage du bouton de duplication</summary>
        protected bool _drawBtnDuplicate = true;

        /// <summary>Affichage du bouton de renomage</summary>
        protected bool _drawBtnRename = true;

        /// <summary>Affichage du bouton de tooltip</summary>
        protected bool _drawBtnTooltip = true;

        /// <summary>Affichage du bouton de suppression</summary>
        protected bool _drawBtnDelete = true;

        ///// <summary>Affichage du bouton PJ</summary>
        //protected bool _drawBtnPJ = false;

        /// <summary>Gestionnaire de droit de traitement</summary>
        protected IRightTreatment RightManager
        {
            get; set;
        }

        /// <summary>
        /// Passe tout le rendu en mode readonly (impacte chaque fieldrecord)
        /// </summary>
        protected override bool ReadonlyRenderer
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Transmet le constructeur de base
        /// </summary>
        /// <param name="pref"></param>
        protected eActionListRenderer(ePref pref)
            : base(pref)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {



            InitDrawButtonsAction();

            return base.Init();
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void InitDrawButtonsAction()
        {
            // On devrait pas tomber sur ce cas, 
            // sauf si le bouton est déjà affiché puis juste après l'administarteur a retiré les droits pour l'utilisateur en cours
            if (!RightManager.CanDisplayItemList())
            {
                this._nErrorNumber = QueryErrorType.ERROR_NUM_TREATMENT_NOT_ALLOWED;
                this._sErrorMsg = eResApp.GetRes(Pref, 149);

                return;
            }

            // En fonction des droits, on affiche ou pas le bouton correspondant
            _drawBtnEdit = RightManager.CanEditItem();
            _drawBtnRename = RightManager.CanRenameItem();
            _drawBtnDelete = RightManager.CanDeleteItem();
            _drawBtnTooltip = RightManager.CanDisplayTooltipItem();

            //On n'a pas le bouton dupliquer
            _drawBtnDuplicate = RightManager.CanDuplicateItem();
        }

        /// <summary>
        /// Ajustement de la taille des colonnes
        /// Dans le cas des filtres, on ajoute à la 1er colonne la taille restante a affecter
        /// </summary>
        protected override void AdjustCol()
        {
            base.AdjustCol();

            #region resize des colonnes
            String sHeadColAlias = String.Concat("HEAD_", _list.ViewMainTable.Alias);

            TableRow trHead = _tblMainList.Rows[0];
            if (trHead != null && trHead.ID == sHeadColAlias)
            {

                //Parcours des cellules du tableau
                TableCell tcFirstCol = null;
                foreach (TableCell tc in trHead.Cells)
                {
                    Unit nWidth = tc.Width;
                    int nWidth2;

                    //Si la colonne est une des colonnes a ajuster
                    if (tc.ID != null && _colMaxValues.ContainsKey(tc.ID))
                    {
                        if (tc.ID == _sFirstColAlias)
                            tcFirstCol = tc;

                        //Si la taille n'est pas définie
                        if (tc.Width.Value == 0)
                        {

                            ListColMaxValues m = _colMaxValues[tc.ID];
                            if (_colMaxValues != null)
                            {

                                nWidth2 = m.GetMaxSize();

                                if (nWidth2 > 0 && nWidth2 > nWidth.Value)
                                    tc.Width = new Unit(nWidth2);
                            }
                        }
                    }

                    _nTotalSize += tc.Width.Value;
                }

                if (tcFirstCol != null && _nTotalSize > 0)
                {
                    int tableRemainingWidth = _width - (int)_nTotalSize - 6;
                    if (tableRemainingWidth > 0)
                    {
                        tcFirstCol.Width = new Unit(tcFirstCol.Width.Value + tableRemainingWidth);
                        tcFirstCol.Attributes.Add("width", String.Concat(tcFirstCol.Width.Value.ToString(), "px"));
                    }
                }
            }



            #endregion
        }

        /// <summary>
        /// Traitement de fin de génération
        /// Ajuste les colonnes
        /// </summary>
        /// <returns>returne true si l'enregistrement a réussi</returns>
        protected override bool End()
        {

            // Taille de la table restant          
            _width = 878;

            if (_colMaxValues.Count == 0)
            {
                // return false;
                _eException = new EudoException(
                    "Erreur sur eActionListRenderer - auncune rubrique disponible",
                    "Votre liste est masqué, aucune rubbrique disponible à l'affichages.");
                return false;
            }


            ListColMaxValues maxValFirstCol = _colMaxValues[_sFirstColAlias];
            StringBuilder sbSpaceAction = new StringBuilder();

            if (maxValFirstCol != null)
            {
                //On joute un espace avant les boutons actions
                sbSpaceAction.Append("|XY|");
                maxValFirstCol.ColMaxValue += sbSpaceAction;
            }

            //reajustement des colonnes d'entete par rapport 
            if (!base.End())
                return false;

            //Ajustement taille entête
            TableCell lenLibCell = _lenHeadList.Rows[0].Cells[0];
            lenLibCell.Text = lenLibCell.Text + sbSpaceAction;

            return true;

        }

        /// <summary>
        /// Classe ajoutée lors de la gestion d'une ligne sur 2
        /// </summary>
        /// <param name="row">Ligne sur laquel on affecte le style</param>
        /// <param name="idxLine">Numéro de ligne</param>
        protected override void cssLine(TableRow row, int idxLine)
        {
            row.CssClass = (idxLine % 2 == 0) ? "list_even" : "list_odd";
        }

        /// <summary>
        /// méthode à override pour décider s'il faut ou non afficher les cases à cocher
        /// </summary>
        /// <returns></returns>
        protected override bool DisplayCheckBox()
        {
            return
                !_list.GetType().Equals(typeof(eMailingPjList)) && (
                _list.ViewMainTable.DescId == TableType.PJ.GetHashCode() ||
                _list.ViewMainTable.DescId % 100 == AllField.ATTACHMENT.GetHashCode() ||
                _rType == RENDERERTYPE.FinderSelection
            );
        }

        /// <summary>
        /// Pas de complexion de ligne dans ce contexte
        /// </summary>
        /// <param name="idxLine"></param>
        protected override void CompleteList(int idxLine)
        {

        }

        /// <summary>
        /// La génération de la list est gérée à part
        /// </summary>
        protected override void GenerateList()
        {

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

            // dans le cas de Liste en cours on paramètre le colspan à toute la ligne.
            if (fieldRow.FileId == -1)
                trCell.ColumnSpan = _tblMainList.Rows[0].Cells.Count;

            // Ajout des icones d'actions sur la première cellule
            if (idxCell != 1)
                return;

            int idxLine = 0;
            try
            {
                System.Web.UI.WebControls.Table tbParent = (System.Web.UI.WebControls.Table)trRow.Parent;
                idxLine = tbParent.Rows.GetRowIndex(trRow);
            }
            catch { }

            Panel divContent = null;
            if (trCell.Controls[trCell.Controls.Count - 1] is Panel)
                divContent = (Panel)trCell.Controls[trCell.Controls.Count - 1];

            Control ctrlAction = GetActionButton(row, divContent?.ID ?? eTools.GetFieldValueCellId(row, fieldRow, idxLine));

            if (ctrlAction == null)
            {
                if (divContent != null)
                    divContent.Attributes.Add("style", "overflow:visible");

                return;
            }

            if (divContent != null)
            {
                int w = eTools.MesureString(fieldRow.DisplayValue);
                divContent.Attributes.Add("ewidth", w.ToString());
            }

            //#77 224: KJE, on ajoute l'attribut "ShowBtnModify" pour pouvoir l'utiliser après dans la gestion des droits de modif coté front
            if (_drawBtnEdit)
                trRow.Attributes.Add("ShowBtnModify", "1");
            else
                trRow.Attributes.Add("ShowBtnModify", "0");

            // On définit si la ligne est modifiable ou non.
            if (row.RightIsUpdatable)
                trRow.Attributes.Add("Updatable", "1");
            else
                trRow.Attributes.Add("Updatable", "0");

            // On le met à la position 0 pour qu'il floate a droite de la divContent
            trCell.Controls.AddAt(0, ctrlAction);
        }

        #region Boutons actions

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton d'edition (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected abstract void BtnActionEdit(WebControl webCtrl, eRecord row);

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de duplication (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected abstract void BtnActionDuplicate(WebControl webCtrl, eRecord row);

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de rename (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="sElementValueId">Id de l'élément dont il faut modifier le contenu</param>
        /// <param name="row">record</param>
        protected abstract void BtnActionRename(WebControl webCtrl, String sElementValueId, eRecord row);

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de tooltip (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected abstract void BtnActionTooltip(WebControl webCtrl, eRecord row);

        /// <summary>
        /// Ajout des attributs specifiques sur le bouton de supp (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected abstract void BtnActionDelete(WebControl webCtrl, eRecord row);



        /// <summary>
        /// Retourne le control contenant toutes les actions de la ligne
        /// </summary>
        /// <param name="row">Ligne sur laquel on generé les actions</param>
        /// <param name="sElementValueId">Id de l'élément dont il faut modifier le contenu notament dans le cas du renommage</param>
        /// <returns>retourne le control</returns>
        protected virtual Control GetActionButton(eRecord row, String sElementValueId = "")
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

            if (_drawBtnDuplicate && !bIsLastUnsavedFilter && row.RightIsUpdatable)
            {
                //DUPLIQUER
                div = new Panel();
                div.CssClass = "icon-duplicate";
                div.ToolTip = eResApp.GetRes(Pref, 534); // dupliquer
                BtnActionDuplicate(div, row);
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

            // ANNEXES
            //if (_drawBtnPJ && row.RightIsUpdatable)
            //{
            //    div = new Panel();
            //    div.CssClass = "icon-annex";
            //    div.ToolTip = eResApp.GetRes(Pref, 6316);
            //    BtnActionPJ(div, row);
            //    divAction.Controls.Add(div);
            //    nNbAction++;
            //}

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

            int width = nNbAction * 24;
            if (btnActionDel)       // Le margin du btn sup
                width += 8;


            if (nNbAction > _nNbMaxAction)
                _nNbMaxAction = nNbAction;

            if (width > 0)
            {
                divAction.Style.Add("width", width.ToString() + "px");
                return divAction;
            }
            else
                return null;
        }

        /// <summary>
        /// retourne la classe css adéquate pour encapsuler les boutons d'actions
        /// </summary>
        /// <returns></returns>

        protected virtual string GetActionCssClass()
        {
            return "logo_modifs";
            // return "logo_modifs w";
        }

        #endregion

        /// <summary>
        /// Définit la taille par défaut d'une colonne
        /// </summary>
        /// <param name="libelleMaxLen">taillé max du libellé (avec filtre et tri)</param>
        /// <param name="field">champ à redimensionner</param>
        protected override void InitFieldWidth(String libelleMaxLen, EudoQuery.Field field)
        {
            if (field.Width <= 0)
            {
                base.InitFieldWidth(libelleMaxLen, field);
                field.Width += 50;
            }
        }
    }
}