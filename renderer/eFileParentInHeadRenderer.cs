using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using EudoQuery;
using Com.Eudonet.Internal;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Classe de rendu pour l'entête des fiches
    /// </summary>
    /// <authors>SPH/KHA</authors>
    /// <date>2012-09-26</date>
    public class eFileParentInHeadRenderer : eRenderer
    {
        private Int32 _nLeftColspan = 1, _nRightColspan = 1;

        /// <summary>
        /// objet représentant la fiche en cours
        /// </summary>
        protected eFile _myFile;
        /// <summary>
        /// Objet renderer dans le quel est intégré le cadre d'entete
        /// </summary>
        protected eMasterFileRenderer _masterRdr;

        /// <summary>
        /// constructeur par défaut
        /// </summary>
        public eFileParentInHeadRenderer()
        {
            _rType = RENDERERTYPE.FileParentInHead;
        }




        /// <summary>
        /// Renderer des en-tête de fiche
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="efRdr"></param>
        public eFileParentInHeadRenderer(ePref pref, eMasterFileRenderer efRdr)
        {
            this.Pref = pref;
            this._myFile = efRdr.File;
            this._masterRdr = efRdr;
            this._rType = RENDERERTYPE.FileParentInHead;
        }

        /// <summary>
        /// Renderer des en-tête de fiche
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ef"></param>
        public eFileParentInHeadRenderer(ePref pref, eFile ef)
        {
            this.Pref = pref;
            this._myFile = ef;
            this._rType = RENDERERTYPE.FileParentInHead;
        }


        /// <summary>
        /// Surcharge de Build propre au FileHeader
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            System.Web.UI.WebControls.Table myTable = new System.Web.UI.WebControls.Table();
            _pgContainer.Controls.Add(myTable);
            _pgContainer.CssClass = "divHeadInfo mTabFile";
            _pgContainer.ID = String.Concat("divPrt_", _myFile.ViewMainTable.Alias);
            _pgContainer.Attributes.Add("fmt", "head");


            myTable.ID = String.Concat("ftp_", _myFile.ViewMainTable.Alias);
            myTable.Attributes.Add("shown", "1");

            eMasterFileRenderer.AddEmptyCellsInHead(_myFile, myTable);

            Int32 nRange = Math.Max(_myFile.GetHeaderFieldsRight.Count, _myFile.GetHeaderFieldsLeft.Count);

            Int32 nMiddleColspan = ((_myFile.ViewMainTable.ColByLine - 2) * eConst.NB_COL_BY_FIELD);


            if (_myFile.ViewMainTable.ColByLine > 2)
            {
                _nRightColspan = _myFile.ViewMainTable.ColByLine / 2;
                if (_myFile.ViewMainTable.ColByLine % 2 == 0)
                {
                    _nLeftColspan = _nRightColspan;
                }
                else
                {
                    _nLeftColspan = _nRightColspan + 1;
                }
            }

            for (Int32 i = 0; i < nRange; i++)
            {


                TableRow myTr = new TableRow();
                myTable.Controls.Add(myTr);

                addCellHead(myTr, _myFile.GetHeaderFieldsLeft, i, 0, _nLeftColspan);

                //if (i == 0 && nMiddleColspan > 0)
                //{
                //    TableCell tdMiddle = new TableCell();
                //    myTr.Cells.Add(tdMiddle);
                //    tdMiddle.RowSpan = nRange;
                //    tdMiddle.ColumnSpan = nMiddleColspan;
                //    tdMiddle.CssClass = "midHeadInfo";
                //}

                addCellHead(myTr, _myFile.GetHeaderFieldsRight, i, 1, _nRightColspan);


            }

            #region Masquer / Afficher les informations parentes en entête (Canceled by KHA)

            /*
            TableRow trFirst = myTable.Rows[0];
            TableCell tdHideHead = new TableCell();
            trFirst.Controls.Add(tdHideHead);
            tdHideHead.RowSpan = nRange;
            tdHideHead.CssClass = "tdHideHead";


            Image imgHideHead = new Image();
            tdHideHead.Controls.Add(imgHideHead);
            imgHideHead.ImageUrl = eConst.GHOST_IMG;
            imgHideHead.CssClass = "shownHead";
            imgHideHead.ID = "imgHideHeaderInfos";
            imgHideHead.Attributes.Add("onclick", "showHideHeadInfo();");
            */
            #endregion


            #region Séparateur entre le tableau d'en tête et le corps de page

            //TableRow trSep = new TableRow();
            //myTable.Rows.Add(trSep);

            //TableCell tcSep = new TableCell();
            //tcSep.ColumnSpan = _myFile.ViewMainTable.ColByLine * eConst.NB_COL_BY_FIELD;
            //tcSep.CssClass = "table_labels";
            //trSep.Cells.Add(tcSep);

            //Panel divSep1 = new Panel();
            //divSep1.CssClass = "sub_sep";


            //Panel divSep2 = new Panel();
            //divSep2.CssClass = "separateur_glbl LNKSEP";
            //divSep2.Controls.Add(divSep1);
            //tcSep.Controls.Add(divSep2);



            #endregion

            //if (_masterRdr != null && _masterRdr.RendererType == RENDERERTYPE.AdminFile)
            //{
            //    //eMasterFileRenderer.AddDropFieldsArea(_myFile, myTable);
            //}


            return true;

        }

        /// <summary>
        /// remplit le web control avec le contenu souhaité
        /// </summary>
        /// <param name="ednWebControl"></param>
        /// <param name="sValue"></param>
        protected override void GetRawMemoControl(EdnWebControl ednWebControl, String sValue)
        {
            GetHTMLMemoControl(ednWebControl, sValue);
        }

        /// <summary>
        /// ajoute le champ(libellé, valeur) dans le tableau d'en-tête de la fiche
        /// </summary>
        /// <param name="tr">TableRow</param>
        /// <param name="liHeader">Liste des descid  afficher en entête</param>
        /// <param name="i">Index de la ligne sur laquelle on se trouve</param>
        /// <param name="indexCol">Index de la colonne sur laquelle on se trouve</param>
        /// <param name="iColspan">ColSpan</param>
        private void addCellHead(TableRow tr, List<eFieldRecord> liHeader, Int32 i, Int32 indexCol, Int32 iColspan = 1)
        {
            eFieldRecord myField = null;
            if (liHeader.Count > i)
            {
                myField = liHeader[i];
                myField.FldInfo.PosColSpan = iColspan;
            }
            AddFieldCellsToTR(myField, tr, i, indexCol);
        }

        /// <summary>
        /// ajoute le champ(libellé, valeur, bouton) dans le tableau d'en-tête de la fiche
        /// </summary>
        /// <param name="fldRec">Champ à afficher</param>
        /// <param name="tr">TableRow</param>
        /// <param name="rowIndex">Index of the row.</param>
        /// <param name="colIndex">Index of the col.</param>
        /// <param name="forceReadOnly">Force le rendu de la rubrique en lecture seule</param>
        protected void AddFieldCellsToTR(eFieldRecord fldRec, TableRow tr, Int32 rowIndex = 0, Int32 colIndex = 0, Boolean forceReadOnly = false)
        {
            List<eFieldRecord> lstFld = new List<eFieldRecord>();
            lstFld.Add(fldRec); //On ajoute toujours au moins le field de base
            if (fldRec != null)
            {
                if (fldRec.FldInfo.Descid == PMField.CP.GetHashCode())  //Si CP on affiche aussi Ville
                    lstFld.Add(_myFile.GetField(PMField.VILLE.GetHashCode()));
                else if (fldRec.FldInfo.Descid == PMField.TEL.GetHashCode())  //Si TEL on affiche aussi Fax
                    lstFld.Add(_myFile.GetField(PMField.FAX.GetHashCode()));
            }
            AddFieldCellsToTR(lstFld, tr, rowIndex, colIndex, forceReadOnly);
        }

        /// <summary>
        /// ajoute une liste de champ pour une même ligne (libellé, valeur, bouton) dans le tableau d'en-tête de la fiche
        /// </summary>
        /// <param name="listFld">liste de champs à afficher</param>
        /// <param name="tr">TableRow</param>
        /// <param name="rowIndex">Index of the row.</param>
        /// <param name="colIndex">Index of the col.</param>
        /// <param name="forceReadOnly">Force le rendu de la rubrique en lecture seule</param>
        protected void AddFieldCellsToTR(List<eFieldRecord> listFld, TableRow tr, Int32 rowIndex = 0, Int32 colIndex = 0, Boolean forceReadOnly = false)
        {
            eFieldRecord fldRec = listFld[0];
            if (fldRec == null)
            {
                AddGhostTableCell(tr, rowIndex, colIndex);
                return;
            }

            Int32 descid = fldRec.FldInfo.Descid;
            Int32 parentTab = eLibTools.GetTabFromDescId(descid);
            Boolean NoFileLinked = false;


            if (descid % 100 == 1)
            {
                //TODO lecture seule pour PM si Adresse pro
                if (forceReadOnly)
                    fldRec.RightIsUpdatable = false;
                else
                    fldRec.RightIsUpdatable = _myFile.IsHeaderUpdatable && (_myFile.Record.RightIsUpdatable || _myFile.FileId == 0);

                if (_myFile.ViewMainTable.InterPP && parentTab == TableType.PP.GetHashCode())
                {
                    fldRec.IsMandatory = _myFile.ViewMainTable.InterPPNeeded;
                    NoFileLinked = !_myFile.ParentFileId.HasParentLnk(TableType.PP);
                }
                else if (_myFile.ViewMainTable.InterPM && parentTab == TableType.PM.GetHashCode())
                {
                    fldRec.IsMandatory = _myFile.ViewMainTable.InterPMNeeded;
                    NoFileLinked = !_myFile.ParentFileId.HasParentLnk(TableType.PM);
                }
                else if (_myFile.ViewMainTable.InterEVT && parentTab == _myFile.ViewMainTable.InterEVTDescid)
                {
                    fldRec.IsMandatory = _myFile.ViewMainTable.InterEVTNeeded;
                    NoFileLinked = !_myFile.ParentFileId.HasParentLnk(TableType.EVENT);
                }

                // On applique pas les règles d'affichage conditionnel/droits de visus sur la rubrique de liaison (evt01, pm01, pp01 ...) si celle-ci n'est pas renseignée 
                fldRec.RightIsVisible = fldRec.RightIsVisible || NoFileLinked; // #37132
                fldRec.IsLink = true;

            }
            else if (descid % 100 != AllField.MEMO_NOTES.GetHashCode())
            {
                fldRec.RightIsUpdatable = false;
            }

            if (!fldRec.RightIsVisible)
            {
                // #46517 : Ajout d'une cellule vide pour éviter que les rubriques soient décalées lorsque certaines ne s'affichent pas
                AddGhostTableCell(tr, rowIndex, colIndex);
                return;
            }

            TableCell tcLabel = new TableCell();
            tr.Cells.Add(tcLabel);
            //tc.Text = fldRec.FldInfo.Libelle;

            bool drawLabel = (_masterRdr != null && _masterRdr.RendererType == RENDERERTYPE.AdminFile);

            GetFieldLabelCell(tcLabel, _myFile.Record, listFld, drawLabel);

            tcLabel.CssClass = String.Concat("table_labels", fldRec.IsMandatory ? " mandatory_Label" : "");
            tcLabel.Attributes.Add("eltvalid", eTools.GetFieldValueCellId(_myFile.Record, fldRec));
            tcLabel.Attributes.Add("colindex", colIndex.ToString());
            tcLabel.Attributes.Add("cellpos", String.Concat(rowIndex, ";", colIndex));
            tcLabel.Attributes.Add("edo", (rowIndex + 1).ToString());

            TableCell tcValue;


            if (_masterRdr != null && _masterRdr.RendererType == RENDERERTYPE.AdminFile)
            {
                tcValue = new TableCell();
            }
            else
            {
                tcValue = (TableCell)GetFieldValueCell(_myFile.Record, listFld, 0, Pref);
                // En champ de Liaison, le champ Mémo doit être affiché sur davantage de lignes (5 "en dur") que les autres de type INPUT
                if (descid % 100 == AllField.MEMO_NOTES.GetHashCode())
                {
                    //
                    if (fldRec.FileId == 0)
                    {
                        tcValue.CssClass += " memoEmpty";
                    }

                    tcValue.Height = 5 * eConst.HEADER_LINE_HEIGHT;
                }
                else
                    tcValue.Height = eConst.HEADER_LINE_HEIGHT;
            }

            tcValue.CssClass = "table_values";

            tcValue.Attributes.Add("colindex", colIndex.ToString());
            tcValue.Attributes.Add("cellpos", String.Concat(rowIndex, ";", colIndex));
            tcValue.Attributes.Add("edo", (rowIndex + 1).ToString());
            tcValue.Attributes.Add("did", fldRec.FldInfo.Descid.ToString());

            SetTableCellColspan(tcValue, fldRec);

            if (fldRec.FldInfo.Descid % 100 == 1)
            {
                tcLabel.Attributes["did"] = eLibTools.GetTabFromDescId(fldRec.FldInfo.Descid).ToString();
                tcLabel.Attributes["popid"] = tcLabel.Attributes["did"];
                tcLabel.Attributes.Add("prt", "1");
                tcLabel.Attributes.Add("pop", "2");

                if (fldRec.FileId > 0)
                {
                    TextBox txtBx = (TextBox)tcValue.Controls[0];
                    txtBx.Attributes["dbv"] = fldRec.FileId.ToString();
                }
            }


            if (fldRec.FldInfo.Format == FieldFormat.TYP_MEMO && _masterRdr != null)
                _masterRdr.MemoIds.Add(tcValue.ID);

            tr.Cells.Add(tcValue);

            TableCell buttonCell;

            if (!fldRec.RightIsUpdatable && fldRec.FldInfo.Descid % 100 == 1)
            {
                buttonCell = GetButtonCell(tcValue, false);
            }
            else
            {
                buttonCell = GetButtonCell(tcValue, true);
            }

            buttonCell.Attributes.Add("cellpos", String.Concat(rowIndex, ";", colIndex));

            tr.Cells.Add(buttonCell);


        }

        /// <summary>
        /// ajoute le champ(libellé, valeur, bouton) dans le tableau d'en-tête de la fiche
        /// </summary>
        /// <param name="fldRec">Champ à afficher</param>
        /// <param name="tr">TableRow</param>
        /// <param name="rowIndex">Index of the row.</param>
        /// <param name="colIndex">Index of the col.</param>
        /// <param name="forceReadOnly">Force le rendu de la rubrique en lecture seule</param>
        protected void AddLabelCellsToTR_SMS(eFieldRecord fldRec, TableRow tr, Int32 rowIndex = 0, Int32 colIndex = 0, Boolean forceReadOnly = false)
        {
            List<eFieldRecord> lstFld = new List<eFieldRecord>();
            lstFld.Add(fldRec); //On ajoute toujours au moins le field de base
            if (fldRec != null)
            {
                if (fldRec.FldInfo.Descid == PMField.CP.GetHashCode())  //Si CP on affiche aussi Ville
                    lstFld.Add(_myFile.GetField(PMField.VILLE.GetHashCode()));
                else if (fldRec.FldInfo.Descid == PMField.TEL.GetHashCode())  //Si TEL on affiche aussi Fax
                    lstFld.Add(_myFile.GetField(PMField.FAX.GetHashCode()));
            }
            AddLabelCellsToTR_SMS(lstFld, tr, rowIndex, colIndex, forceReadOnly);
        }

        /// <summary>
        /// ajoute le champ(libellé, valeur, bouton) dans le tableau d'en-tête de la fiche
        /// </summary>
        /// <param name="fldRec">Champ à afficher</param>
        /// <param name="tr">TableRow</param>
        /// <param name="rowIndex">Index of the row.</param>
        /// <param name="colIndex">Index of the col.</param>
        /// <param name="forceReadOnly">Force le rendu de la rubrique en lecture seule</param>
        protected void AddFieldCellsToTR_SMS(eFieldRecord fldRec, TableRow tr, Int32 rowIndex = 0, Int32 colIndex = 0, Boolean forceReadOnly = false)
        {
            List<eFieldRecord> lstFld = new List<eFieldRecord>();
            lstFld.Add(fldRec); //On ajoute toujours au moins le field de base
            if (fldRec != null)
            {
                if (fldRec.FldInfo.Descid == PMField.CP.GetHashCode())  //Si CP on affiche aussi Ville
                    lstFld.Add(_myFile.GetField(PMField.VILLE.GetHashCode()));
                else if (fldRec.FldInfo.Descid == PMField.TEL.GetHashCode())  //Si TEL on affiche aussi Fax
                    lstFld.Add(_myFile.GetField(PMField.FAX.GetHashCode()));
            }
            AddFieldCellsToTR_SMS(lstFld, tr, rowIndex, colIndex, forceReadOnly);
        }

        /// <summary>
        /// ajoute une liste de champ pour une même ligne (libellé, valeur, bouton) dans le tableau d'en-tête de la fiche
        /// </summary>
        /// <param name="listFld">liste de champs à afficher</param>
        /// <param name="tr">TableRow</param>
        /// <param name="rowIndex">Index of the row.</param>
        /// <param name="colIndex">Index of the col.</param>
        /// <param name="forceReadOnly">Force le rendu de la rubrique en lecture seule</param>
        protected void AddLabelCellsToTR_SMS(List<eFieldRecord> listFld, TableRow tr, Int32 rowIndex = 0, Int32 colIndex = 0, Boolean forceReadOnly = false)
        {
            eFieldRecord fldRec = listFld[0];
            if (fldRec == null)
            {
                AddGhostTableCell(tr, rowIndex, colIndex);
                return;
            }

            Int32 descid = fldRec.FldInfo.Descid;
            Int32 parentTab = eLibTools.GetTabFromDescId(descid);
            Boolean NoFileLinked = false;


            if (descid % 100 == 1)
            {
                //TODO lecture seule pour PM si Adresse pro
                if (forceReadOnly)
                    fldRec.RightIsUpdatable = false;
                else
                    fldRec.RightIsUpdatable = _myFile.IsHeaderUpdatable && (_myFile.Record.RightIsUpdatable || _myFile.FileId == 0);

                if (_myFile.ViewMainTable.InterPP && parentTab == TableType.PP.GetHashCode())
                {
                    fldRec.IsMandatory = _myFile.ViewMainTable.InterPPNeeded;
                    NoFileLinked = !_myFile.ParentFileId.HasParentLnk(TableType.PP);
                }
                else if (_myFile.ViewMainTable.InterPM && parentTab == TableType.PM.GetHashCode())
                {
                    fldRec.IsMandatory = _myFile.ViewMainTable.InterPMNeeded;
                    NoFileLinked = !_myFile.ParentFileId.HasParentLnk(TableType.PM);
                }
                else if (_myFile.ViewMainTable.InterEVT && parentTab == _myFile.ViewMainTable.InterEVTDescid)
                {
                    fldRec.IsMandatory = _myFile.ViewMainTable.InterEVTNeeded;
                    NoFileLinked = !_myFile.ParentFileId.HasParentLnk(TableType.EVENT);
                }

                // On applique pas les règles d'affichage conditionnel/droits de visus sur la rubrique de liaison (evt01, pm01, pp01 ...) si celle-ci n'est pas renseignée 
                fldRec.RightIsVisible = fldRec.RightIsVisible || NoFileLinked; // #37132
                fldRec.IsLink = true;

            }
            else if (descid % 100 != AllField.MEMO_NOTES.GetHashCode())
            {
                fldRec.RightIsUpdatable = false;
            }

            if (!fldRec.RightIsVisible)
            {
                // #46517 : Ajout d'une cellule vide pour éviter que les rubriques soient décalées lorsque certaines ne s'affichent pas
                AddGhostTableCell(tr, rowIndex, colIndex);
                return;
            }

            TableCell tcLabel = new TableCell();
            tr.Cells.Add(tcLabel);
            //tc.Text = fldRec.FldInfo.Libelle;

            bool drawLabel = (_masterRdr != null && _masterRdr.RendererType == RENDERERTYPE.AdminFile);

            GetFieldLabelCell(tcLabel, _myFile.Record, listFld, drawLabel);

            tcLabel.CssClass = String.Concat("table_labels", fldRec.IsMandatory ? " mandatory_Label" : "");
            tcLabel.Attributes.Add("eltvalid", eTools.GetFieldValueCellId(_myFile.Record, fldRec));
            tcLabel.Attributes.Add("colindex", colIndex.ToString());
            tcLabel.Attributes.Add("cellpos", String.Concat(rowIndex, ";", colIndex));
            tcLabel.Attributes.Add("edo", (rowIndex + 1).ToString());

            if (fldRec.FldInfo.Descid % 100 == 1)
            {
                tcLabel.Attributes["did"] = eLibTools.GetTabFromDescId(fldRec.FldInfo.Descid).ToString();
                tcLabel.Attributes["popid"] = tcLabel.Attributes["did"];
                tcLabel.Attributes.Add("prt", "1");
                tcLabel.Attributes.Add("pop", "2");

            }

        }

        /// <summary>
        /// ajoute une liste de champ pour une même ligne (libellé, valeur, bouton) dans le tableau d'en-tête de la fiche
        /// </summary>
        /// <param name="listFld">liste de champs à afficher</param>
        /// <param name="tr">TableRow</param>
        /// <param name="rowIndex">Index of the row.</param>
        /// <param name="colIndex">Index of the col.</param>
        /// <param name="forceReadOnly">Force le rendu de la rubrique en lecture seule</param>
        protected void AddFieldCellsToTR_SMS(List<eFieldRecord> listFld, TableRow tr, Int32 rowIndex = 0, Int32 colIndex = 0, Boolean forceReadOnly = false)
        {
            eFieldRecord fldRec = listFld[0];
            if (fldRec == null)
            {
                AddGhostTableCell(tr, rowIndex, colIndex);
                return;
            }

            Int32 descid = fldRec.FldInfo.Descid;
            Int32 parentTab = eLibTools.GetTabFromDescId(descid);
            Boolean NoFileLinked = false;


            if (descid % 100 == 1)
            {
                //TODO lecture seule pour PM si Adresse pro
                if (forceReadOnly)
                    fldRec.RightIsUpdatable = false;
                else
                    fldRec.RightIsUpdatable = _myFile.IsHeaderUpdatable && (_myFile.Record.RightIsUpdatable || _myFile.FileId == 0);

                if (_myFile.ViewMainTable.InterPP && parentTab == TableType.PP.GetHashCode())
                {
                    fldRec.IsMandatory = _myFile.ViewMainTable.InterPPNeeded;
                    NoFileLinked = !_myFile.ParentFileId.HasParentLnk(TableType.PP);
                }
                else if (_myFile.ViewMainTable.InterPM && parentTab == TableType.PM.GetHashCode())
                {
                    fldRec.IsMandatory = _myFile.ViewMainTable.InterPMNeeded;
                    NoFileLinked = !_myFile.ParentFileId.HasParentLnk(TableType.PM);
                }
                else if (_myFile.ViewMainTable.InterEVT && parentTab == _myFile.ViewMainTable.InterEVTDescid)
                {
                    fldRec.IsMandatory = _myFile.ViewMainTable.InterEVTNeeded;
                    NoFileLinked = !_myFile.ParentFileId.HasParentLnk(TableType.EVENT);
                }

                // On applique pas les règles d'affichage conditionnel/droits de visus sur la rubrique de liaison (evt01, pm01, pp01 ...) si celle-ci n'est pas renseignée 
                fldRec.RightIsVisible = fldRec.RightIsVisible || NoFileLinked; // #37132
                fldRec.IsLink = true;

            }
            else if (descid % 100 != AllField.MEMO_NOTES.GetHashCode())
            {
                fldRec.RightIsUpdatable = false;
            }

            if (!fldRec.RightIsVisible)
            {
                // #46517 : Ajout d'une cellule vide pour éviter que les rubriques soient décalées lorsque certaines ne s'affichent pas
                AddGhostTableCell(tr, rowIndex, colIndex);
                return;
            }

            //TableCell tcLabel = new TableCell();
            //tr.Cells.Add(tcLabel);
            //tc.Text = fldRec.FldInfo.Libelle;

            //bool drawLabel = (_masterRdr != null && _masterRdr.RendererType == RENDERERTYPE.AdminFile);

            //GetFieldLabelCell(tcLabel, _myFile.Record, listFld, drawLabel);

            //tcLabel.CssClass = String.Concat("table_labels", fldRec.IsMandatory ? " mandatory_Label" : "");
            // tcLabel.Attributes.Add("eltvalid", eTools.GetFieldValueCellId(_myFile.Record, fldRec));
            //tcLabel.Attributes.Add("colindex", colIndex.ToString());
            //tcLabel.Attributes.Add("cellpos", String.Concat(rowIndex, ";", colIndex));
            //tcLabel.Attributes.Add("edo", (rowIndex + 1).ToString());

            TableCell tcValue;


            if (_masterRdr != null && _masterRdr.RendererType == RENDERERTYPE.AdminFile)
            {
                tcValue = new TableCell();
            }
            else
            {
                tcValue = (TableCell)GetFieldValueCell(_myFile.Record, listFld, 0, Pref);
                
                // En champ de Liaison, le champ Mémo doit être affiché sur davantage de lignes (5 "en dur") que les autres de type INPUT
                if (descid % 100 == AllField.MEMO_NOTES.GetHashCode())
                {
                    //
                    if (fldRec.FileId == 0)
                    {
                        tcValue.CssClass += " memoEmpty";
                    }

                    tcValue.Height = 5 * eConst.HEADER_LINE_HEIGHT;
                }
                else
                    tcValue.Height = eConst.HEADER_LINE_HEIGHT;
            }

            tcValue.CssClass = "table_values";

            tcValue.Attributes.Add("colindex", colIndex.ToString());
            tcValue.Attributes.Add("cellpos", String.Concat(rowIndex, ";", colIndex));
            tcValue.Attributes.Add("edo", (rowIndex + 1).ToString());
            tcValue.Attributes.Add("did", fldRec.FldInfo.Descid.ToString());

            SetTableCellColspan(tcValue, fldRec);

            if (fldRec.FldInfo.Descid % 100 == 1)
            {
                //tcLabel.Attributes["did"] = eLibTools.GetTabFromDescId(fldRec.FldInfo.Descid).ToString();
                //tcLabel.Attributes["popid"] = tcLabel.Attributes["did"];
                //tcLabel.Attributes.Add("prt", "1");
                //tcLabel.Attributes.Add("pop", "2");

                if (fldRec.FileId > 0)
                {
                    TextBox txtBx = (TextBox)tcValue.Controls[0];
                    txtBx.Attributes["dbv"] = fldRec.FileId.ToString();
                }
            }


            if (fldRec.FldInfo.Format == FieldFormat.TYP_MEMO && _masterRdr != null)
                _masterRdr.MemoIds.Add(tcValue.ID);

            tr.Cells.Add(tcValue);

            TableCell buttonCell;

            if (!fldRec.RightIsUpdatable && fldRec.FldInfo.Descid % 100 == 1)
            {
                buttonCell = GetButtonCell(tcValue, false);
            }
            else {
                buttonCell = GetButtonCell(tcValue, true);
            }

            buttonCell.Attributes.Add("cellpos", String.Concat(rowIndex, ";", colIndex));

            tr.Cells.Add(buttonCell);


        }

        /// <summary>
        /// Ajoute une cellule vide avec le bon colspan
        /// </summary>
        /// <param name="tr">Table row</param>
        /// <param name="rowIndex">Index of the row.</param>
        /// <param name="colIndex">Index of the col.</param>
        protected void AddGhostTableCell(TableRow tr, Int32 rowIndex, Int32 colIndex)
        {
            TableCell tcGhost = new TableCell();
            if (tr.Cells.Count == 0)
                tcGhost.ColumnSpan = _nLeftColspan * eConst.NB_COL_BY_FIELD;
            else
                tcGhost.ColumnSpan = _nRightColspan * eConst.NB_COL_BY_FIELD;

            tcGhost.Attributes.Add("colindex", colIndex.ToString());
            tcGhost.Attributes.Add("cellpos", String.Concat(rowIndex, ";", colIndex));
            tcGhost.Attributes.Add("edo", (rowIndex + 1).ToString());

            tr.Cells.Add(tcGhost);
        }


        /// <summary>
        /// Sets the table cell colspan.
        /// </summary>
        /// <param name="tcValue">TableCell</param>
        /// <param name="fldRec">The field record.</param>
        protected virtual void SetTableCellColspan(TableCell tcValue, eFieldRecord fldRec)
        {
            eTools.SetTableCellColspan(tcValue, fldRec);
        }
    }
}