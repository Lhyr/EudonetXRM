using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// interface de sélection des rubriques avant la fusion
    /// </summary>
    public partial class eMergeFiles : eEudoPage
    {
        // Fiches
        private eFile _efMaster = null;
        private eFile _efDbl = null;
        // Res (libelles) des tables PP, PM, ADR
        private TableLabels _tabLbl = new TableLabels();
        // List des lignes de comparaisons
        private List<TableRow> _lstResponseTbRows = new List<TableRow>();
        // Données Owner
        private CompData _ownerData = null;
        // Données MultiOwner
        private CompData _multiOwnerData = null;

        private class CompData
        {
            internal eFieldRecord FldMaster { get; set; }
            internal eFieldRecord FldDoublon { get; set; }
        }

        struct TableLabels
        {
            internal String pm;
            internal String pp;
            internal String adr;
        }

        enum ChoiceType
        {
            RADIO,
            CHECK
        }

        enum CompType
        {
            MASTER,
            DOUBLON
        }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// méthode de chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region ajout des css

            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eFile");
            PageRegisters.AddCss("eMergeFiles");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eTitle");
            PageRegisters.AddCss("eudoFont");

            #endregion

            #region ajout des js


            PageRegisters.AddScript("eMergeFiles");
            PageRegisters.AddScript("eTools");

            #endregion

            try
            {
                if (!_allKeys.Contains("tab") || !_allKeys.Contains("fromfileid") || !_allKeys.Contains("bkmfileid"))
                    return;

                Int32 nTab = eLibTools.GetNum(Request.Form["tab"].ToString());
                Int32 nFromFileId = eLibTools.GetNum(Request.Form["fromfileid"].ToString());
                Int32 nBkmFileId = eLibTools.GetNum(Request.Form["bkmfileid"].ToString());

                if (nTab == 0 || nFromFileId == 0 || nBkmFileId == 0)
                    return;

                // Charge les valeurs de fiches
                _efMaster = eFileLite.CreateFileLite(_pref, nTab, nFromFileId);
                _efDbl = eFileLite.CreateFileLite(_pref, nTab, nBkmFileId);

                // Trie les rubriques par ordre d'affichage (disporder)
                List<eFieldRecord> efSortedFlds = _efMaster.GetFileFields;
                efSortedFlds.Sort(eFieldRecord.CompareByDisporder);

           

                if (!_efDbl.Record.RightIsDeletable)
                {
                    ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.EXCLAMATION,
                        eResApp.GetRes(_pref, 1828), " ", eResApp.GetRes(_pref, 814),
                        String.Concat("Vous n'avez pas les droits de suppression sur la fiche doublon.",
                            " Table : ", _efMaster.ViewMainTable.DescId, " MasterId : ", _efMaster.FileId, " DblId : ", _efDbl.FileId));
                    return;
                }

                eRes tabRes = new eRes(_pref, "200,300,400");
                _tabLbl.adr = tabRes.GetRes(400, "adresses");
                _tabLbl.pp = tabRes.GetRes(200, "contacts");
                _tabLbl.pm = tabRes.GetRes(300, "sociétés");

                // Charge les données des rubriques à comparer
                ICollection<CompData> lstComp = GetCompList(efSortedFlds);

                // Construction HTML des lignes de comparaison
                foreach (CompData compData in lstComp)
                {
                    TableRow row = GetFieldRend(compData);
                    if (row != null)
                        _lstResponseTbRows.Add(row);
                }
            }
            catch (Exception exp)
            {
                ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL,
                    String.Concat("Erreur de source inconnue. ", Environment.NewLine, exp.Message, Environment.NewLine, exp.StackTrace));
            }
            finally
            {
                DoResponse();
            }
        }

        private ICollection<CompData> GetCompList(ICollection<eFieldRecord> efSortedFlds)
        {
            ICollection<CompData> lstComp = new List<CompData>();

            foreach (eFieldRecord fldMaster in efSortedFlds)
            {
                eFieldRecord fldDbl = _efDbl.Record.GetFieldByAlias(fldMaster.FldInfo.Alias);
                if (fldDbl == null)
                    continue;

                if (fldMaster.FldInfo.Format == FieldFormat.TYP_ALIAS || fldMaster.FldInfo.Format == FieldFormat.TYP_ALIASRELATION)
                    continue;

                // Recupération du Owner et du MultiOwner
                    if (fldMaster.FldInfo.Descid == _efMaster.ViewMainTable.GetOwnerDescId())
                    _ownerData = new CompData() { FldMaster = fldMaster, FldDoublon = fldDbl };
                if (fldMaster.FldInfo.Descid == _efMaster.ViewMainTable.GetMultiOwnerDescId())
                    _multiOwnerData = new CompData() { FldMaster = fldMaster, FldDoublon = fldDbl };

                // On compare uniquement les valeurs différentes
                if (fldDbl.FldInfo.Format == EudoQuery.FieldFormat.TYP_IMAGE && fldDbl.FldInfo.ImgStorage == ImageStorage.STORE_IN_DATABASE)
                {
                    if (fldDbl.DisplayValue.ToUpper() == fldMaster.DisplayValue.ToUpper())
                        continue;
                }
                else if (fldDbl.Value.ToUpper() == fldMaster.Value.ToUpper())
                    continue;

                if (!fldMaster.RightIsVisible || !fldMaster.RightIsUpdatable                   
                     || !fldDbl.RightIsVisible)
                    continue;

                if (fldMaster.FldInfo.Format == EudoQuery.FieldFormat.TYP_MEMO
                    || fldMaster.FldInfo.Format == EudoQuery.FieldFormat.TYP_AUTOINC)
                    continue;

                // Pas de rubriques systèmes
                if (fldMaster.FldInfo.Descid >= _efMaster.ViewMainTable.DescId + eLibConst.MAX_NBRE_FIELD
                    && fldMaster.FldInfo.Descid != _efMaster.ViewMainTable.GetOwnerDescId()
                    && fldMaster.FldInfo.Descid != _efMaster.ViewMainTable.GetMultiOwnerDescId()
                    && fldMaster.FldInfo.Descid % 100 != (int)AllField.AVATAR
                    && fldMaster.FldInfo.Descid % 100 != (int)AllField.GEOGRAPHY
                    )
                    continue;

                lstComp.Add(new CompData() { FldMaster = fldMaster, FldDoublon = fldDbl });
            }

            // On force l'init des owner si ils sont resté vide
            if (_ownerData == null)
                _ownerData = new CompData();
            if (_multiOwnerData == null)
                _multiOwnerData = new CompData();

            return lstComp;
        }

        private TableRow GetFieldRend(CompData compData)
        {
            eFieldRecord fldRecMaster = compData.FldMaster;
            eFieldRecord fldRecDbl = compData.FldDoublon;
            Int32 fldDid = fldRecMaster.FldInfo.Descid;

            TableRow tr = new TableRow();

            // Libellé du champ
            TableCell tc = new TableCell();
            tr.Cells.Add(tc);
            tc.Controls.Add(new LiteralControl(fldRecMaster.FldInfo.Libelle));
            tc.CssClass = "table_labels";

            // MultiOwner
            if (fldDid == _efMaster.ViewMainTable.GetMultiOwnerDescId())
            {
                String multiOwnersId = String.Concat("VAL_", fldDid, "_", fldRecMaster.FileId);

                tr.Cells.Add(new TableCell());

                // Cellule valeur
                TableCell tcOwnerVal = new TableCell();
                tr.Cells.Add(tcOwnerVal);
                tcOwnerVal.CssClass = "CompFileOwner";
                tcOwnerVal.ColumnSpan = 3;

                HtmlInputText input = new HtmlInputText();
                tcOwnerVal.Controls.Add(input);
                input.Value = GetOwnersName(
                    (_ownerData == null || _ownerData.FldMaster == null) ? String.Empty : _ownerData.FldMaster.DisplayValue,
                    (_ownerData == null || _ownerData.FldDoublon == null) ? String.Empty : _ownerData.FldDoublon.DisplayValue,
                    (_multiOwnerData == null || _multiOwnerData.FldMaster == null) ? String.Empty : _multiOwnerData.FldMaster.DisplayValue,
                    (_multiOwnerData == null || _multiOwnerData.FldDoublon == null) ? String.Empty : _multiOwnerData.FldDoublon.DisplayValue);
                input.ID = multiOwnersId;
                input.Attributes.Add("readonly", "1");
                input.Attributes.Add("class", "CompVal");
                input.Attributes.Add("onmouseover", String.Concat("ste(event, '", multiOwnersId, "');"));
                input.Attributes.Add("onmouseout", "ht();");
            }
            else
            {
                // Master - Cellule checkbox
                TableCell tcMasterChk = new TableCell();
                tr.Cells.Add(tcMasterChk);
                tcMasterChk.CssClass = "CompFileCtrl";

                // Master - Cellule valeur
                TableCell tcMasterVal = new TableCell();
                tr.Cells.Add(tcMasterVal);
                tcMasterVal.CssClass = "CompFileVal";

                // Doublon - Cellule checkbox
                TableCell tcDoublonChk = new TableCell();
                tr.Cells.Add(tcDoublonChk);
                tcDoublonChk.CssClass = "CompFileCtrl";

                // Doublon - Cellule valeur
                TableCell tcDoublonVal = new TableCell();
                tr.Cells.Add(tcDoublonVal);
                tcDoublonVal.CssClass = "CompFileVal";

                if (fldRecMaster.FldInfo.Multiple)
                {
                    /*eCheckBoxCtrl chkMaster = new eCheckBoxCtrl(!String.IsNullOrEmpty(fldRecMaster.Value), false);
                    chkMaster.ID = String.Concat("CB_MAST_", fldDid);
                    chkMaster.AddClick();
                    tcMasterChk.Controls.Add(chkMaster);

                    eCheckBoxCtrl chkDbl = new eCheckBoxCtrl(!String.IsNullOrEmpty(fldRecDbl.Value), false);
                    chkDbl.ID = String.Concat("CB_DBL_", fldDid);
                    chkDbl.AddClick();
                    tcDoublonChk.Controls.Add(chkDbl);*/

                    tcMasterChk.Controls.Add(GetChoice(ChoiceType.CHECK, CompType.MASTER, fldDid, !String.IsNullOrEmpty(fldRecMaster.Value)));
                    tcDoublonChk.Controls.Add(GetChoice(ChoiceType.CHECK, CompType.DOUBLON, fldDid, !String.IsNullOrEmpty(fldRecDbl.Value)));
                }
                else
                {
                    // Selection de la radiobox
                    Boolean dblChecked = false;
                    switch (fldRecMaster.FldInfo.Format)
                    {
                        case EudoQuery.FieldFormat.TYP_BIT:
                            // 308 - Cochée / 309 - Décochée
                            fldRecMaster.DisplayValue = fldRecMaster.Value == "1" ? eResApp.GetRes(_pref, 308) : eResApp.GetRes(_pref, 309);
                            fldRecDbl.DisplayValue = fldRecDbl.Value == "1" ? eResApp.GetRes(_pref, 308) : eResApp.GetRes(_pref, 309);

                            dblChecked = (fldRecMaster.Value != "1" && fldRecDbl.Value == "1");
                            break;
                        default:
                            dblChecked = (String.IsNullOrEmpty(fldRecMaster.Value) && !String.IsNullOrEmpty(fldRecDbl.Value));
                            break;
                    }

                    HtmlInputRadioButton rbMaster = new HtmlInputRadioButton();
                    //tcMasterChk.Controls.Add(rbMaster);
                    rbMaster.ID = String.Concat("RB_MAST_", fldDid);
                    rbMaster.Name = String.Concat("choice_", fldDid);
                    rbMaster.Value = fldDid.ToString();
                    rbMaster.Attributes.Add("mast", "");

                    HtmlInputRadioButton rbDbl = new HtmlInputRadioButton();
                    //tcDoublonChk.Controls.Add(rbDbl);
                    rbDbl.ID = String.Concat("RB_DBL_", fldDid);
                    rbDbl.Name = String.Concat("choice_", fldDid);
                    rbDbl.Value = fldDid.ToString();
                    rbDbl.Attributes.Add("dbl", "");

                    tcMasterChk.Controls.Add(GetChoice(ChoiceType.RADIO, CompType.MASTER, fldDid, !dblChecked));
                    tcDoublonChk.Controls.Add(GetChoice(ChoiceType.RADIO, CompType.DOUBLON, fldDid, dblChecked));
                }

                HtmlInputText input = new HtmlInputText();
                tcMasterVal.Controls.Add(input);
                input.Value = fldRecMaster.DisplayValue;
                input.ID = String.Concat("VM", fldDid);
                input.Attributes.Add("readonly", "1");
                input.Attributes.Add("dbv", fldRecMaster.Value);
                input.Attributes.Add("class", "CompVal");

                input = new HtmlInputText();
                tcDoublonVal.Controls.Add(input);
                input.Value = fldRecDbl.DisplayValue;
                input.ID = String.Concat("VD", fldDid);
                input.Attributes.Add("readonly", "1");
                input.Attributes.Add("dbv", fldRecDbl.Value);
                input.Attributes.Add("class", "CompVal");
            }

            return tr;
        }

        private Control GetChoice(ChoiceType choiceTyp, CompType compTyp, Int32 fldDid, Boolean ctrlChecked)
        {
            Control ctrl = null;
            AttributeCollection ctrlAttributes = null;

            switch (choiceTyp)
            {
                case ChoiceType.CHECK:
                    eCheckBoxCtrl chk = new eCheckBoxCtrl(ctrlChecked, false);
                    chk.AddClick();

                    ctrlAttributes = chk.Attributes;
                    ctrl = chk;
                    break;
                case ChoiceType.RADIO:
                    HtmlInputRadioButton rb = new HtmlInputRadioButton();
                    rb.Name = String.Concat("choice_", fldDid);
                    rb.Value = "";
                    rb.Checked = ctrlChecked;

                    ctrlAttributes = rb.Attributes;
                    ctrl = rb;
                    break;
            }

            String id = String.Concat(choiceTyp, "_", compTyp, "_", fldDid);
            String identifier = compTyp.ToString();

            ctrl.ID = id;
            ctrlAttributes.Add(identifier, "");
            ctrlAttributes.Add("did", fldDid.ToString());

            return ctrl;
        }

        private void DoResponse()
        {
            Panel divInfo = null;

            try
            {
                // En cas, d'erreur le script est arrêté et informe l'interface de l'erreur
                LaunchError();
            }
            catch (eEndResponseException) { Response.End(); }

            HtmlGenericControl ulGlobal = new HtmlGenericControl("ul");
            ulGlobal.Attributes.Add("class", "ulCompFiles");
            ulGlobal.ID = "ulGlobal";

            #region On force la validation de la fenêtre si Pas de rubrique à sélectionner et que l'on ne vient pas de PP ou PM
            Boolean bForceValid = false;
            if (_lstResponseTbRows.Count <= 0
                && _efMaster.ViewMainTable.TabType != TableType.PP && _efMaster.ViewMainTable.TabType != TableType.PM)
                bForceValid = true;
            ulGlobal.Attributes.Add("eForceValid", bForceValid ? "1" : "0");
            #endregion

            divCompFiles.Controls.Add(ulGlobal);

            // <Nom de la table> : <Nom de la fiche>
            HtmlGenericControl li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "liMainLab");
            ulGlobal.Controls.Add(li);
            HtmlGenericControl spanLbl = new HtmlGenericControl("span");
            spanLbl.Controls.Add(new LiteralControl(String.Concat(_efDbl.ViewMainTable.Libelle, " : ")));
            spanLbl.Attributes.Add("class", "table_labels");
            li.Controls.Add(spanLbl);
            HtmlGenericControl spanVal = new HtmlGenericControl("span");
            spanVal.Controls.Add(new LiteralControl(_efMaster.Record.MainFileLabel));
            spanVal.Attributes.Add("class", "file_labels");
            li.Controls.Add(spanVal);


            // Tableau de rubriques
            // Si tableau de rubrique vide, ne pas proposer cette etape
            if (_lstResponseTbRows.Count > 0)
            {
                // 811 - Sélectionner les rubriques à conserver pour la fusion
                li = new HtmlGenericControl("li");
                li.Attributes.Add("class", "liLabSelFld");
                li.Controls.Add(new LiteralControl(String.Concat(eResApp.GetRes(_pref, 811), " : ")));
                ulGlobal.Controls.Add(li);

                li = new HtmlGenericControl("li");
                li.Attributes.Add("class", "liCompFiles");
                ulGlobal.Controls.Add(li);

                Panel divCompValues = new Panel();
                divCompValues.ID = "divCompValues";
                divCompValues.CssClass = "divCompValues";
                divCompValues.Style.Add("display", "none");      // HLA - Sera par la suite apparent
                li.Controls.Add(divCompValues);

                System.Web.UI.WebControls.Table tbCompValues = new System.Web.UI.WebControls.Table();
                divCompValues.Controls.Add(tbCompValues);
                tbCompValues.CssClass = "tbCompValues";

                TableRow tr = null;
                TableCell tc = null;

                #region Fiche en cours / Fiche doublon

                tr = new TableRow();
                tbCompValues.Rows.Add(tr);

                // Vide
                tr.Cells.Add(new TableCell());

                // Vide
                tr.Cells.Add(new TableCell());

                // 992 - Fiche en cours
                tc = new TableCell();
                tc.CssClass = "tbLabCol";
                tc.Controls.Add(new LiteralControl(eResApp.GetRes(_pref, 992)));
                tr.Cells.Add(tc);

                // Vide
                tr.Cells.Add(new TableCell());

                // 993 - Doublon
                tc = new TableCell();
                tc.CssClass = "tbLabCol";
                tc.Controls.Add(new LiteralControl(eResApp.GetRes(_pref, 993)));
                tr.Cells.Add(tc);

                #endregion

                tbCompValues.Rows.AddRange(_lstResponseTbRows.ToArray());
            }

            #region Garder toutes les adresses

            if (_efMaster.ViewMainTable.TabType == TableType.PP || _efMaster.ViewMainTable.TabType == TableType.PM)
            {
                li = new HtmlGenericControl("li");
                li.Attributes.Add("class", "liKeepAllAdr");
                ulGlobal.Controls.Add(li);

                // 1793	- Garder toutes les fiches '<ADR>'
                // 6753	- Garder les '<ADR>' identiques en doublons #35211
                eCheckBoxCtrl chkKeepAllAdr = new eCheckBoxCtrl(true, false);
                chkKeepAllAdr.AddClick();
                chkKeepAllAdr.ID = "KeepAllAdr";
                chkKeepAllAdr.AddText(eResApp.GetRes(_pref, 6753).Replace("<ADR>", _tabLbl.adr));
                li.Controls.Add(chkKeepAllAdr);

                // 1791 - Dans le cas où les fiches fusionnées possédent des <ADR> identiques (fiches '<ADR>' attachée à la même '<PM>'), conserver toutes les fiches '<ADR>' au risque d'avoir des doublons de '<ADR>'
                divInfo = new Panel();
                divInfo.CssClass = "icon-edn-info";
                divInfo.Attributes.Add("onmouseover", String.Concat("st(this, '",
                    eResApp.GetRes(_pref, 1791).Replace("<ADR>", _tabLbl.adr)
                    .Replace("<PM>", (_efMaster.ViewMainTable.TabType == TableType.PP) ? _tabLbl.pm : _tabLbl.pp)
                    .Replace("'", @"\'")
                    , "');"));
                divInfo.Attributes.Add("onmouseout", "ht();");
                li.Controls.Add(divInfo);
            }

            #endregion

            #region Mettre à jour les '<ADR>' de tous les '<PP>'

            if (_efMaster.ViewMainTable.TabType == EudoQuery.TableType.PM)
            {
                li = new HtmlGenericControl("li");
                li.Attributes.Add("class", "liOverwriteAdrInfos");
                ulGlobal.Controls.Add(li);

                // 1256 - Mettre à jour les '<ADR>' de tous les '<PP>'
                eCheckBoxCtrl chkUpdAllPostalAdr = new eCheckBoxCtrl(true, false);
                chkUpdAllPostalAdr.AddClick();
                chkUpdAllPostalAdr.ID = "OverwriteAdrInfos";
                chkUpdAllPostalAdr.AddText(eResApp.GetRes(_pref, 1256).Replace("<ADR>", _tabLbl.adr).Replace("<PP>", _tabLbl.pp));
                li.Controls.Add(chkUpdAllPostalAdr);

                // 1792 - Recopier automatiquement les rubriques postales de la fiche '<PM>' qui est conservée sur les fiches '<ADR>' rattachées par la fusion
                divInfo = new Panel();
                divInfo.CssClass = "icon-edn-info";
                divInfo.Attributes.Add("onmouseover", String.Concat("st(this, '",
                    eResApp.GetRes(_pref, 1792).Replace("<ADR>", _tabLbl.adr).Replace("<PM>", _tabLbl.pm).Replace("'", @"\'"), "');"));
                divInfo.Attributes.Add("onmouseout", "ht();");
                li.Controls.Add(divInfo);
            }

            #endregion
        }

        private String GetOwnersName(params String[] tabOwners)
        {
            StringBuilder sb = new StringBuilder();
            foreach (String usersName in tabOwners)
                if (usersName.Length > 0)
                    sb.Append(String.Concat(usersName, " ; "));
            if (sb.Length > 0)
                sb.Length = sb.Length - 3;
            return sb.ToString();
        }
    }
}