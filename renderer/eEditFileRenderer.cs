using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Core.Model.Teams;
using System.Linq;

namespace Com.Eudonet.Xrm
{
    /// <summary>r19062
    /// classe de rendu de fiche en modification et création
    /// </summary>
    public class eEditFileRenderer : eMainFileRenderer
    {
        private ISet<Int32> _dicValuesChanged;

        private Boolean _bCreatePPADRFromPM = false;

        /// <summary>Creation de pp sans adresse</summary>
        private Boolean _isWithoutAdr = false;
        /// <summary>Indique si il y a une liaison avec ppid</summary>
        private Boolean _bHasParentLinkToPP = false;


        /// <summary>
        /// Indique si on est dans le cas de la création d'un contact depuis une société.
        /// </summary>
        public Boolean CreatePPADRFromPM
        {
            set { _bCreatePPADRFromPM = value; }
        }

        /// <summary>
        /// Paramètres nécessaire au rendu de la fiche
        /// </summary>
        /// <param name="dicParams"></param>
        public override void SetDicParams(ExtendedDictionary<string, object> dicParams)
        {
            Object myObj = null;

            base.SetDicParams(dicParams);

            if (_dicParams != null)
            {
                //Dic Values NotChanged
                if (_dicParams.TryGetValue("dicvalueschanged", out myObj))
                    _dicValuesChanged = (HashSet<Int32>)myObj;

                //Cas de creation de PP sans Adresse
                if (_dicParams.TryGetValue("withoutadr", out myObj))
                    _isWithoutAdr = (Boolean)myObj;
            }
        }

        /// <summary>
        /// Affichage pour la création et la modification
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="nFileId"></param>
        public eEditFileRenderer(ePref pref, Int32 nTab, Int32 nFileId)
            : base(pref, nTab, nFileId)
        {
            _rType = RENDERERTYPE.EditFile;
        }

        /// <summary>
        /// Construction des objets HTML
        /// Surcharge implémentant le rajout des boutons permettant le retour au mode fiche
        /// </summary>
        /// <returns></returns>
        protected override Boolean Build()
        {
            bool isPopupAdr = _myFile.ViewMainTable.TabType == TableType.ADR && _myFile.IsPopup;

            // #91 709 - Renommage de la propriété en _hasParentLinkToPP (anciennement _asAnPpLnk, littéralement, en langage HLA, "a un lien parent vers PP")
            _bHasParentLinkToPP =
                _myFile.ParentFileId.HasParentLnk(TableType.PP) ||
                (_myFile.DicValues != null && (_myFile.DicValues.ContainsKey(TableType.PP.GetHashCode())));

            // On donne la priorité au param CREATE_PP_ADR_MODE sur la valeur par defaut de ADR92 dans le cas d'une créa d'ADR depuis une créa de PP
            Boolean isFirstLoad = _myFile.DicValues != null && !_myFile.DicValues.ContainsKey(AdrField.PERSO.GetHashCode());
            if (isPopupAdr && Pref.CreatePpAdrMode == eLibConst.CONFIGADV_CREATE_PP_ADR_MODE.DO_NOT_SUGGEST)
                _isWithoutAdr = _isWithoutAdr || (isFirstLoad && !_bCreatePPADRFromPM && !_bHasParentLinkToPP);

            if (!base.Build())
                return false;

            if (isPopupAdr && !this.IsBkmFile)
                ChoosePersoProfessionalAddress();

            return true;
        }

        /// <summary>
        /// renseigne le contenu de la fiche
        /// </summary>
        /// <param name="sortedFields">Liste des champs triés</param>
        protected override void FillContent(List<eFieldRecord> sortedFields)
        {
            //On zappe le rendu de fiche adresse en mode creation pp "sans adresse"
            if (_tab == 400 && !_myFile.ParentFileId.HasParentLnk(TableType.PP) && _isWithoutAdr)
                return;

            base.FillContent(sortedFields);
        }

        /// <summary>
        /// Fait un rendu du champ de type char
        /// </summary>
        /// <param name="row">Ligne de la liste a afficher</param>
        /// <param name="fieldRow">Le record</param>
        /// <param name="ednWebControl">elment html dans lequel on fait le rendu</param>
        /// <param name="sbClass">classe CSS</param>
        /// <param name="sClassAction">le type d action</param>
        /// <returns></returns>
        protected override bool RenderCharFieldFormat(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, System.Text.StringBuilder sbClass, ref string sClassAction)
        {
            Boolean result = base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);

            // HLA - Gestion de l'autobuildname en mode création et modification en popup uniquement pour les EVENT - Dev #33529
            if ((fieldRow.FldInfo.Table.TabType == TableType.EVENT || fieldRow.FldInfo.Table.TabType == TableType.ADR)
                && fieldRow.FldInfo.Descid == fieldRow.FldInfo.Table.MainFieldDescId
                && row.ViewTab == fieldRow.FldInfo.Table.DescId
                && !String.IsNullOrEmpty(fieldRow.FldInfo.Table.GetAutoBuildName))
            {
                ednWebControl.WebCtrl.Attributes.Add("chgedval", _dicValuesChanged == null || !_dicValuesChanged.Contains(fieldRow.FldInfo.Descid) ? "0" : "1");
            }

            // SPH - pour test de changement de ratachement sur une adresse pro
            if (row.MainFileid == 0 && row.CalledTab == TableType.ADR.GetHashCode() && fieldRow.FldInfo.Descid == TableType.PM.GetHashCode() + 1 && fieldRow.IsLink)
            {
                ednWebControl.WebCtrl.Attributes.Add("adrpro", "1");
                ednWebControl.WebCtrl.Attributes.Add("adrproneedconfirm", "1");
            }

            return result;
        }

        /// <summary>
        /// Bouton de choix adresse personnelle
        /// </summary>
        protected virtual void ChoosePersoProfessionalAddress()
        {
            eFieldRecord fldAdrType = _myFile.Record.GetFieldByAlias(String.Concat(TableType.ADR.GetHashCode(), "_", AdrField.PERSO.GetHashCode()));

            Boolean isAdrPerso = !_isWithoutAdr && !_bCreatePPADRFromPM && (fldAdrType.Value == "1" || (!fldAdrType.RightIsVisible && fldAdrType.FldInfo.DefaultValue == "1"));
            Boolean isAdrPro = !_isWithoutAdr && !isAdrPerso;

            Panel rbAddressType = new Panel();
            rbAddressType.ID = eTools.GetFieldValueCellName(_myFile.Record, fldAdrType);
            AddFieldProperties(rbAddressType, _myFile.Record, fldAdrType);
            rbAddressType.Attributes.Add("class", "TypAdr");
            rbAddressType.Attributes.Add("name", "TypAdr");

            //Pro
            RadioButton itemPro = new RadioButton();
            itemPro.ID = String.Concat(rbAddressType.ID, "_0");
            itemPro.Attributes.Add("value", "0");
            itemPro.GroupName = rbAddressType.ID;
            itemPro.Attributes.Add("class", "TypAdrItm");
            if (!_bCreatePPADRFromPM)
                itemPro.Attributes.Add("onclick", String.Concat("warn(\"", itemPro.ID, "\");;"));
            itemPro.Attributes.Add("eName", rbAddressType.ID);
            itemPro.Checked = isAdrPro;
            itemPro.Enabled = fldAdrType.RightIsUpdatable;
            itemPro.Text = eResApp.GetRes(Pref, 99);
            rbAddressType.Controls.Add(itemPro);

            // Perso
            if (!_bCreatePPADRFromPM)
            {
                RadioButton itemPerso = new RadioButton();
                itemPerso.ID = String.Concat(rbAddressType.ID, "_1");
                itemPerso.Attributes.Add("eName", rbAddressType.ID);
                itemPerso.Attributes.Add("value", "1");
                itemPerso.GroupName = rbAddressType.ID;
                itemPerso.Attributes.Add("class", "TypAdrItm");
                itemPerso.Attributes.Add("onclick", String.Concat("warn(\"", itemPerso.ID, "\");"));
                itemPerso.Checked = isAdrPerso;
                itemPerso.Enabled = fldAdrType.RightIsUpdatable;
                itemPerso.Text = fldAdrType.FldInfo.Libelle;
                rbAddressType.Controls.Add(itemPerso);
            }

            bool bIsPPAdrCombined = false;
            _dicParams?.TryGetValueConvert("isPPAdrCombined", out bIsPPAdrCombined);
            // Ajout d'option "Sans adresse" à la création d'une fiche PP. [MOU, #35209]
            // #91 709 : assouplissement de la condition sur _bHasParentLinkToPP (anciennement _asAnPpLnk) pour ne pas qu'elle soit discriminante dans le cas où on crée une
            // adresse depuis PP lorsqu'on recharge la fiche après application d'une modification suite à une règle ou condition de visu (applyRuleOnBlank > updateFile côté JS)
            if (
                bIsPPAdrCombined &&
                (!_bHasParentLinkToPP || _bHasParentLinkToPP && _myFile.ViewMainTable.TabType == TableType.ADR) &&
                !_bCreatePPADRFromPM && Pref.CreatePpAdrMode != eLibConst.CONFIGADV_CREATE_PP_ADR_MODE.FORCE_ADR_CREATION
            )
            {
                RadioButton itemWithout = new RadioButton();
                itemWithout.ID = String.Concat(rbAddressType.ID, "_2");
                itemWithout.Attributes.Add("value", "2");
                itemWithout.GroupName = rbAddressType.ID;
                itemWithout.Attributes.Add("class", "TypAdrItm");
                itemWithout.Attributes.Add("onclick", String.Concat("warn(\"", itemWithout.ID, "\");"));
                itemWithout.Attributes.Add("eName", rbAddressType.ID);
                itemWithout.Checked = _isWithoutAdr;
                itemWithout.Text = eResApp.GetRes(Pref, 6752);
                rbAddressType.Controls.Add(itemWithout);
            }

            _backBoneRdr.PnFilePart1.Controls.AddAt(0, rbAddressType);

            if (!fldAdrType.RightIsVisible)
                rbAddressType.Style.Add(HtmlTextWriterStyle.Display, "none");

            System.Web.UI.WebControls.Table tbHiddenAdrPerso = new System.Web.UI.WebControls.Table();
            tbHiddenAdrPerso.Style.Add("display", "none");

            TableRow tr = new TableRow();
            tbHiddenAdrPerso.Rows.Add(tr);

            // HLA - On transforme le type et la valeur de la rubrique pour prendre en compte 3 valeurs : adr perso / adr pro / sans adr
            FieldFormat saveFormat = fldAdrType.FldInfo.Format;
            String saveValue = fldAdrType.Value;
            String saveDisplayValue = fldAdrType.DisplayValue;
            fldAdrType.FldInfo.Format = FieldFormat.TYP_CHAR;
            fldAdrType.Value = _isWithoutAdr ? "2" : (isAdrPerso ? "1" : "0");
            fldAdrType.DisplayValue = fldAdrType.Value;

            TableCell myLabel = new TableCell();
            GetFieldLabelCell(myLabel, _myFile.Record, fldAdrType);
            TableCell myValue = (TableCell)GetFieldValueCell(_myFile.Record, fldAdrType, 0, Pref);

            // HLA - Restauration des valeurs. Utile ?
            fldAdrType.FldInfo.Format = saveFormat;
            fldAdrType.Value = saveValue;
            fldAdrType.DisplayValue = saveDisplayValue;

            tr.Cells.Add(myLabel);
            tr.Cells.Add(myValue);

            _backBoneRdr.PnFilePart1.Controls.Add(tbHiddenAdrPerso);
        }

        /// <summary>
        /// Ajout de la liaison adresse
        /// </summary>
        /// <param name="myField">eFieldRecord</param>
        /// <param name="maTable">Table webcontrol</param>
        /// <returns></returns>
        protected override bool SetAddrProfLink(eFieldRecord myField, System.Web.UI.WebControls.Table maTable)
        {
            if (myField.FldInfo.Descid == (int)TableType.ADR + 1)
            {
                // HLA - On force la reprise des valeurs de la société si on crée un PP depuis une PM alors que ADR92 a pour valeur par defaut "adresse personnel"
                Boolean isCreatePPADRFromPM = false, isPPAdrCombined = false;
                _dicParams?.TryGetValueConvert("createPpAdrFromPm", out isCreatePPADRFromPM);
                _dicParams?.TryGetValueConvert("isPPAdrCombined", out isPPAdrCombined);

                eFieldRecord fldAdrPerso = _myFile.Record.GetFieldByAlias(String.Concat(TableType.ADR.GetHashCode(), "_", AdrField.PERSO.GetHashCode()));
                bool isPerso = fldAdrPerso.Value == "1" && !isCreatePPADRFromPM;

                if (!isPPAdrCombined)
                    AddAddressLinks(maTable, TableType.PP, true);
                AddAddressLinks(maTable, TableType.PM, !isPerso);

                FieldsDescId.AddContains((AdrField.PERSO.GetHashCode()).ToString());

                ////if (_myFile.FileId == 0)
                ////{
                ////    AddAddressLinks(maTable, TableType.PP, _myFile.FileContext.TabFrom != (int)TableType.PP);

                ////    if (isCreatePPADRFromPM || fldAdrPerso.Value == "0" || (!fldAdrPerso.RightIsVisible && fldAdrPerso.FldInfo.DefaultValue == "0"))
                ////    {
                ////        AddAddressLinks(maTable, TableType.PM, true);
                ////        return true;
                ////    }
                ////}
                ////else
                ////{
                ////    //myField.RightIsUpdatable = false;

                ////    AddAddressLinks(maTable, TableType.PP, true);
                ////    if (fldAdrPerso.Value == "0") {
                ////        AddAddressLinks(maTable, TableType.PM, true);
                ////    }

                ////}
            }
            return false;
        }




        /// <summary>
        /// Création et initialisation de l'objet eFile
        /// Surchage de la classe héritée
        /// </summary>
        /// <returns></returns>
        protected override Boolean Init()
        {
            try
            {


                //Génération d'un objet "métier" de type file
                _myFile = eFileMain.CreateEditMainFile(Pref, _tab, _nFileId, _dicParams);

                _dicParams.TryGetValueConvert("globalaffect", out GlobalAffect);
                _dicParams.TryGetValueConvert("globalinvit", out GlobalInvit);
                _dicParams.TryGetValueConvert("readonly", out _bReadOnly);

                if (_myFile.ErrorMsg.Length > 0)
                {
                    _eException = _myFile.InnerException;
                    _sErrorMsg = String.Concat("eEditFileRenderer.Init ", Environment.NewLine, _myFile.ErrorMsg); ;
                    if (_myFile.InnerException != null && _myFile.InnerException.GetType() == typeof(EudoFileNotFoundException))
                    {
                        _nErrorNumber = QueryErrorType.ERROR_NUM_FILE_NOT_FOUND;
                    }
                    else
                    {
                        _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;
                    }

                    return false;

                }

                if (_bReadOnly)
                {
                    _myFile.Record.RightIsUpdatable = false;
                    foreach (eFieldRecord f in _myFile.GetFileFields)
                    {
                        f.RightIsUpdatable = false;
                    }
                }

                HideTeamsButtons();

                return true;
            }
            catch (Exception e)
            {
                _sErrorMsg = String.Concat("eFileRenderer.Init ", Environment.NewLine, e.Message);
                _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;
                _eException = e;
                return false;
            }

        }


        /// <summary>
        /// rajoute les liaisons vers PP et PM sur address
        /// </summary>
        /// <param name="maTable"></param>
        /// <param name="tabTyp"></param>
        /// <param name="isVisible"></param>
        protected void AddAddressLinks(System.Web.UI.WebControls.Table maTable, TableType tabTyp, Boolean isVisible)
        {
            if (!(_myFile.ViewMainTable.TabType == TableType.ADR))
            {
                return;
            }

            //TODO : trouver un meilleur test pour afficher ou non la liaison d'adresse à contact
            // ici il faut cacher le champ de liaison dans le cas de la création d'un nouveau contact
            eFieldRecord fldLink = _myFile.Record.GetFieldByAlias(String.Concat(_myFile.ViewMainTable.DescId, "_", (int)tabTyp + 1));
            fldLink.RightIsVisible = _myFile.ViewMainTable.PermViewAll;
            fldLink.RightIsUpdatable = _myFile.Record.RightIsUpdatable;
            fldLink.IsLink = true;
            switch (tabTyp)
            {
                case TableType.PP:
                    fldLink.IsMandatory = _myFile.ViewMainTable.InterPPNeeded;
                    break;
                case TableType.PM:
                    //fldLink.IsMandatory = _myFile.ViewMainTable.InterPMNeeded;
                    break;
                default:
                    return;
                    break;
            }



            //if (fldLink.FileId == 0 && tabTyp == TableType.PP)
            //    return;

            if (tabTyp == TableType.PM && _bCreatePPADRFromPM)
            {
                fldLink.FldInfo.ReadOnly = true;
            }

            TableRow myTr = new TableRow();
            maTable.Rows.Add(myTr);
            //if (tabTyp == TableType.PP && _myFile.FileId == 0)
            //    myTr.Style.Add("display", "none");

            //Dans le cas d'une adresse personnelle on vide la liaison PmId dans la table adresse
            if (!isVisible)
            {
                myTr.Style.Add("display", "none");
                fldLink.DisplayValue = string.Empty;
                fldLink.FileId = 0;
                fldLink.IsMandatory = false;
            }

            TableCell myLabel = new TableCell();
            TableCell myValue = new TableCell();
            TableCell myButton = new TableCell();

            // on force la liaison 
            //fldLink.IsMandatory = true;

            //fldLink.IsVisible = isVisible;

            GetFieldLabelCell(myLabel, _myFile.Record, fldLink);
            myLabel.Attributes["did"] = tabTyp.GetHashCode().ToString();
            myLabel.Attributes["popid"] = tabTyp.GetHashCode().ToString();
            myLabel.Attributes.Add("eltvalid", eTools.GetFieldValueCellId(_myFile.Record, fldLink));
            myLabel.Attributes.Add("prt", "1");

            myValue = (TableCell)GetFieldValueCell(_myFile.Record, fldLink, 0, Pref);
            //(eConst.NB_COL_BY_FIELD - 1) corresponds au nombre des cellules système associées : (label, boutons, etc.)
            eFieldRecord fldAdr01 = _myFile.Record.GetFieldByAlias(String.Concat(_myFile.ViewMainTable.DescId, "_401"));
            if (fldAdr01 != null)
                myValue.ColumnSpan = fldAdr01.FldInfo.PosColSpan * eConst.NB_COL_BY_FIELD - (eConst.NB_COL_BY_FIELD - 1);

            TextBox txtBx = (TextBox)myValue.Controls[0];
            if (fldLink.FileId > 0)
                txtBx.Attributes["dbv"] = fldLink.FileId.ToString();
            else
                txtBx.Attributes["dbv"] = String.Empty;

            // 71757 : Finalement, on ne permet plus de changer les liaisons
            //if (/*!_bCreatePPADRFromPM*/_myFile.Record.MainFileid == 0 && fldLink.RightIsUpdatable)
            //    myButton = GetButtonCell(myValue, true);

            //en création on rajoute le bouton permettant la modification du champ
            // on permet aussi désormais de modifier la liaison organisme sur une adresse existante
            if (fldLink.RightIsUpdatable)
                if (_myFile.Record.MainFileid == 0)
                    myButton = GetButtonCell(myValue, true);
                else if (tabTyp == TableType.PM)          //kha a voir si on conserve la modification
                    myButton = GetButtonCell(myValue, true);



            myTr.Cells.Add(myLabel);
            myTr.Cells.Add(myValue);
            myTr.Cells.Add(myButton);
        }

        /// <summary>
        /// remplit le web control avec le contenu souhaité
        /// </summary>
        /// <param name="ednWebCtrl"></param>
        /// <param name="sValue"></param>
        protected override void GetValueContentControl(EdnWebControl ednWebCtrl, string sValue)
        {
            if (ednWebCtrl.TypCtrl == EdnWebControl.WebControlType.TEXTBOX)
                ((TextBox)ednWebCtrl.WebCtrl).Text = HttpUtility.HtmlDecode(sValue);
            else if (ednWebCtrl.TypCtrl == EdnWebControl.WebControlType.PASSWORD_CELL)
            {
                TextBox tb = (TextBox)ednWebCtrl.WebCtrl;
                tb.TextMode = TextBoxMode.Password;
                tb.Text = sValue;
                tb.Attributes.Add("value", sValue);
            }
            else
                base.GetValueContentControl(ednWebCtrl, sValue);
        }

        /// <summary>
        /// remplit le web control avec le contenu souhaité
        /// </summary>
        /// <param name="ednWebCtrl"></param>
        /// <param name="sValue"></param>
        protected override void GetRawMemoControl(EdnWebControl ednWebCtrl, String sValue)
        {
            GetHTMLMemoControl(ednWebCtrl, sValue);
        }

        /// <summary>
        /// rend le block HTML de tous les signets dans une méthode overridable
        /// </summary>
        /// <returns></returns>
        protected override void GetBookMarkBlock()
        {
            int nTabFrom = 0;




            if (_dicParams.ContainsKey("ntabfrom") && _dicParams["ntabfrom"] != null)
                _dicParams.TryGetValueConvert("ntabfrom", out nTabFrom);

            if ((_nFileId > 0 && !_bPopupDisplay) || (_myFile.ViewMainTable.TabType == TableType.PP && _myFile.FileId == 0 && nTabFrom != (int)TableType.ADR))
                base.GetBookMarkBlock();
        }

        /// <summary>
        /// Action spécifique au Site web
        /// </summary>
        /// <param name="fieldRow">Information du champ courant</param>
        /// <param name="ednWebControl">Controle qui va être ajoute</param>
        /// <returns>ClassA utiliser</returns>
        public override String GetFieldValueCell_TYP_WEB(EdnWebControl ednWebControl, eFieldRecord fieldRow)
        {
            String sClassAction = String.Empty;
            // HLA - interpretation champ web - #67618
            String sRegSpecV7 = String.Concat(eLibTools.GetServerConfig("v7dir").TrimEnd('/'), "/app/specif/", Pref.GetBaseName, "/(?:.*)\\.asp(?:.*)$");
            Regex regExp = new Regex(sRegSpecV7, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            MatchCollection mc;

            mc = regExp.Matches(fieldRow.DisplayValue);
            // Url de type Spécif 

            if (fieldRow.DisplayValue.Length > 0 && mc.Count == 1)
            {
                sClassAction = "LNKOPENWEBSPECIF";
            }
            else
            {
                sClassAction = "LNKWEBSIT";
            }

            GetValueContentControl(ednWebControl, fieldRow.DisplayValue);
            return sClassAction;
        }

        /// <summary>
        /// Action spécifique au Site web
        /// </summary>
        /// <param name="fieldRow">Information du champ courant</param>
        /// <param name="ednWebControl">Controle qui va être ajoute</param>
        /// <returns>ClassA utiliser</returns>
        public override String GetFieldValueCell_TYP_SOCIALNETWORK(EdnWebControl ednWebControl, eFieldRecord fieldRow)
        {
            String sClassAction = "LNKSOCNET";

            GetValueContentControl(ednWebControl, fieldRow.DisplayValue);
            return sClassAction;
        }


        /// <summary>
        /// ajoute les liaisons parentes en pied de page
        /// </summary>
        protected override void AddParentInFoot()
        {
            if (_myFile.ViewMainTable.TabType == TableType.ADR)
                return;

            // ajout du pied de page contenant les informations parentes en popup
            if (_bPopupDisplay && !this._myFile.FldFieldsInfos.Exists(delegate (Field field) { return field.Format == FieldFormat.TYP_ALIASRELATION; }))
            {
                eRenderer footRenderer = eRendererFactory.CreateParenttInFootRenderer(Pref, this);
                Panel pgC = null;
                if (footRenderer.ErrorMsg.Length > 0)
                    this._sErrorMsg = footRenderer.ErrorMsg;    //On remonte l'erreur
                if (footRenderer != null)
                    pgC = footRenderer.PgContainer;
                _backBoneRdr.PnlDetailsBkms.Controls.Add(footRenderer.PgContainer);
            }

        }

        /// <summary>
        /// Construit la fin du contenu de la fiche
        /// </summary>
        protected override void EndFillContent()
        {
            if (_bPopupDisplay)
            {
                HtmlInputHidden hidRight = new HtmlInputHidden();
                hidRight.ID = String.Concat("rightInfo_", _tab);
                _divHidden.Controls.Add(hidRight);
                if (_myFile.Record.RightIsDeletable)
                    hidRight.Attributes.Add("del", "1");
                else
                    hidRight.Attributes.Add("del", "0");

                //BSE #49380 Cacher le bouton imprimer sur les template en mode fiche si on a pas les droits
                bool printAllowed = eLibDataTools.IsTreatmentAllowed(Pref, Pref.User, eLibConst.TREATID.PRINT);

                if (printAllowed && _tab != (int)TableType.USER)
                    hidRight.Attributes.Add("print", "1");
                else
                    hidRight.Attributes.Add("print", "0");

                if (eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.File_CancelLastEntries))
                    hidRight.Attributes.Add("cclval", "1");
                else
                    hidRight.Attributes.Add("cclval", "0");

            }
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

            if (_myFile.IsClone)
            {
                _pgContainer.Controls.Add(GetBkmsToClonePanel(_myFile, Pref, new ExtendedDictionary<string, object>()));

            }

            return bReturn;
        }

        /// <summary>
        /// on rend invisible les boutons impliqués dans le mapping teams
        /// </summary>
        protected void HideTeamsButtons()
        {
            try
            {
                eTeamsMapping TeamsMapping = eTeamsFactory.GetMapping(_ePref, _tab, true);

                IEnumerable<eFieldRecord> fldTeamsButtons = _myFile.GetFileFields.Where(f => TeamsMapping.IsBtnImplied(f.FldInfo.Descid) && f.FldInfo.Format == FieldFormat.TYP_BITBUTTON);
                foreach (eFieldRecord field in fldTeamsButtons)
                {
                    field.RightIsVisible = false;
                }
            }
            catch (Exception)
            {
            }

        }

        /// <summary>
        /// Duplication Unitaire - Renvoie le tableau permettant à l'utilisateur de selectionner les signets qu'il souhaite dupliquer
        /// </summary>
        /// <returns></returns>
        public static Panel GetBkmsToClonePanel(eFile myFile, ePref pref, ExtendedDictionary<string, object> dic)
        {
            TableRow tr = null;

            TableCell tc = null;
            Int32 i = 0;
            eCheckBoxCtrl ck = null;

            String sClassCss = String.Empty;

            Panel pnSelBkm = new Panel();
            pnSelBkm.ID = "BkmsCloneDiv";
            pnSelBkm.CssClass = "BkmsCloneDiv";

            HtmlInputHidden hid = new HtmlInputHidden();
            hid.ID = "bkms";
            pnSelBkm.Controls.Add(hid);

            //Remplir Le tableau tout selectionner tout deselectionner
            System.Web.UI.WebControls.Table tbList = new System.Web.UI.WebControls.Table();
            tbList.CssClass = "BkmsAllSelect";
            tbList.ID = "BkmsAllSelect";
            pnSelBkm.Controls.Add(tbList);


            String sFctJSPrefix = "";
            if (dic.ContainsKey("globaltreat"))
            {
                sFctJSPrefix = "oDuppiWizard.";
            }

            tr = getSelUnSelOpt("SelAll", sFctJSPrefix + "selectAll(this);", eResApp.GetRes(pref, 431));
            tbList.Controls.Add(tr);

            tr = getSelUnSelOpt("UnSelAll", sFctJSPrefix + "selectAll(this);", eResApp.GetRes(pref, 432));
            tbList.Controls.Add(tr);

            tr = new TableRow();

            //Remplir La div
            tbList = new System.Web.UI.WebControls.Table();
            tbList.CssClass = "BkmsCloneTb";
            pnSelBkm.Controls.Add(tbList);

            tbList.ID = "BkmsCloneTb";





            List<eBookmark> lst = new List<eBookmark>();

            if (myFile.LstBookmark != null)
            {
                lst = myFile.LstBookmark;
                lst.Sort(eBookmark.CompareByLabel);
            }
            else
            {


                //gestion duplication champ notes
                // todo : check, il semble que les notes soient duppliquées même si pas cochée


            }


            foreach (eBookmark bkm in lst)
            {
                if (!bkm.IsAddAllowed || !bkm.Clonable)
                    continue;

                sClassCss = "cell";

                if (i != 0 && i % 4 == 0)
                {
                    tr = new TableRow();
                }

                tbList.Rows.Add(tr);

                tc = new TableCell();
                tr.Cells.Add(tc);
                tc.CssClass = sClassCss;

                ck = new eCheckBoxCtrl(false, false);

                tc.Controls.Add(ck);


                ck.ID = String.Concat("bkm", i);

                ck.AddText(bkm.Libelle);
                ck.AddClick(sFctJSPrefix + "addBkmToCloneLst(this);");
                ck.Attributes.Add("bkm", bkm.CalledTabDescId.ToString());

                i++;

            }

            return pnSelBkm;
        }

        /// <summary>
        /// Créer ligne d'un tableau les ajouter à une table
        /// </summary>
        /// <param name="idCk">id de la check box</param>
        /// <param name="functionJs">nom de la function js</param>
        /// <param name="text">libelle de check box</param>
        /// <returns></returns>
        public static TableRow getSelUnSelOpt(string idCk, string functionJs, string text)
        {
            TableRow tr = new TableRow();

            TableCell tc = new TableCell();
            tr.Cells.Add(tc);
            tc.CssClass = "";

            eCheckBoxCtrl ck = new eCheckBoxCtrl(false, false);

            tc.Controls.Add(ck);


            ck.ID = idCk;

            ck.AddText(text);
            ck.AddClick(functionJs);

            return tr;
        }



    }
}