using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{



    /// <summary>
    /// génération du code html de la fenêtre des actions conditionnel (visu, modif...)
    /// </summary>
    public class eAdminConditionsDialogRenderer : eAdminRenderer
    {

        #region propriétés

        /// <summary>
        /// Id de la modal du renderer
        /// </summary>
        private string _sIdModal = "";


        /// <summary>
        /// DescId de la rubrique
        /// </summary>
        private Int32 _nDescId;

        /// <summary>
        /// Table "parente" pour le cas du paramétrage des signets
        /// </summary>
        private Int32 _nParentTab;

        /// <summary>
        /// Info sur la table de la conditions
        /// </summary>
        private eAdminTableInfos _tabInfos;

        private eAdminTableInfos _parentTabInfos;

        private eAdminFieldInfos _fieldInfos;

        /// <summary>
        /// //Type de la conditions
        /// </summary>
        private TypeTraitConditionnal _type;

        /// <summary>
        /// /titre de la conditions
        /// </summary>
        private string _sBaseName;

        private bool _bIsField = false;

        private bool _bIsFieldImportForbidden = false;
        #endregion


        #region Accesseur

        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="nParentTab">Id table parente</param>
        /// <param name="sIdModal"></param>
        /// <param name="typ"></param>
        private eAdminConditionsDialogRenderer(ePref pref, int nTab, int nParentTab, string sIdModal, TypeTraitConditionnal typ)
        {
            Pref = pref;

            _tab = eLibTools.GetTabFromDescId(nTab);

            if (nTab % 100 == (int)AllField.ATTACHMENT || nTab == (int)TableType.DOUBLONS)
                _tab = nTab;

            _nDescId = nTab;
            _type = typ;
            _sIdModal = sIdModal;
            _nParentTab = nParentTab;

            _bIsField = nTab != (int)TableType.DOUBLONS && ((_tab != _nDescId) || (_nParentTab != _nDescId));


        }


        /// <summary>
        /// Accessur vers le construteur
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="nParentTab"></param>
        /// <param name="typ"></param>
        /// <param name="sIdModal"></param>
        /// <returns></returns>
        public static eAdminConditionsDialogRenderer GetAdminConditionsDialogRenderer(ePref pref, int nTab, int nParentTab, string sIdModal, TypeTraitConditionnal typ)
        {
            if (typ == TypeTraitConditionnal.Undefined)
                throw new Exception("Paramètre TypeTraitConditionnal invalide.");

            if (pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                throw new EudoAdminInvalidRightException();

            return new eAdminConditionsDialogRenderer(pref, nTab, nParentTab, sIdModal, typ);

        }
        #endregion


        #region OverRide

        /// <summary>
        /// Initialisation des informations pour le rendu
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {

            if (base.Init())
            {

                //Information sur la table
                _tabInfos = new eAdminTableInfos(Pref, _tab);
                if (_tab != _nParentTab)
                    _parentTabInfos = new eAdminTableInfos(Pref, _nParentTab);
                else
                    _parentTabInfos = _tabInfos;

                #region titre de la fenêtre
                StringBuilder sb = new StringBuilder();

                //
                string sTitle = eResApp.GetRes(Pref, 7417);// "Modifier l'automatisme prédéfini \"<NAME>\"";


                if (_bIsField)
                {
                    _fieldInfos = eAdminFieldInfos.GetAdminFieldInfos(Pref, _nDescId);

                    if (_type == TypeTraitConditionnal.FieldForbidImport)
                        _bIsFieldImportForbidden = _fieldInfos.Format != FieldFormat.TYP_PASSWORD &&  eSqlSync.IsFieldImportForbidden(Pref, _fieldInfos.DescId);

                }
                switch (_type)
                {
                    case TypeTraitConditionnal.BkmUpdate:
                    case TypeTraitConditionnal.Update:
                        _sBaseName = eResApp.GetRes(Pref, 7418);//   "Conditions de modification de la fiche \"<TAB>\"";
                        break;
                    case TypeTraitConditionnal.BkmDelete:
                    case TypeTraitConditionnal.Delete:
                        _sBaseName = eResApp.GetRes(Pref, 7419);//  "Conditions de suppression de la fiche \"<TAB>\"";
                        break;
                    case TypeTraitConditionnal.Color:
                        _sBaseName = eResApp.GetRes(Pref, 7420);//  "Pictogramme et couleur conditionnel de la fiche \"<TAB>\"";
                        break;
                    case TypeTraitConditionnal.Header_View:
                        _sBaseName = eResApp.GetRes(Pref, 7421);//  "Conditions d’affichage de l'entête de la fiche \"<TAB>\"";
                        break;
                    case TypeTraitConditionnal.Header_Update:
                        _sBaseName = eResApp.GetRes(Pref, 7422);// "Conditions de modification de l'entête de la fiche \"<TAB>\"";
                        break;
                    case TypeTraitConditionnal.Export:
                        _sBaseName = eResApp.GetRes(Pref, 7423);//  "Conditions d'export de la fiche \"<TAB>\"";
                        break;
                    case TypeTraitConditionnal.Mailing:
                        _sBaseName = eResApp.GetRes(Pref, 7424);// "Conditions de mailing de la fiche \"<TAB>\"";
                        break;
                    case TypeTraitConditionnal.Merge:
                        _sBaseName = eResApp.GetRes(Pref, 7425);//  "Conditions de publipostage de la fiche \"<TAB>\"";
                        break;
                    case TypeTraitConditionnal.Faxing:
                        _sBaseName = eResApp.GetRes(Pref, 7426);//  "Conditions de faxing de la fiche \"<TAB>\"";
                        break;
                    case TypeTraitConditionnal.Voicing:
                        _sBaseName = eResApp.GetRes(Pref, 7427);//  "Conditions de voicing de la fiche \"<TAB>\"";
                        break;

                    case TypeTraitConditionnal.BkmView:
                        _sBaseName = eResApp.GetRes(Pref, 7428);//  "Conditions de visualisation du signet \"<TAB>\"  depuis \"<PARENTTAB>\"";
                        break;
                    case TypeTraitConditionnal.BkmAdd:
                        _sBaseName = eResApp.GetRes(Pref, 7429);// "Conditions d'ajout des fiches du signet \"<TAB>\"  depuis \"<PARENTTAB>\"";
                        break;
                    case TypeTraitConditionnal.FieldView:
                        _sBaseName = eResApp.GetRes(Pref, 8369).Replace("<FIELD>", _fieldInfos.Labels[Pref.LangId]); //Conditions de visualisation du champ "<FIELD>"

                        break;
                    case TypeTraitConditionnal.FieldObligat:
                        _sBaseName = eResApp.GetRes(Pref, 8370).Replace("<FIELD>", _fieldInfos.Labels[Pref.LangId]); //Conditions de saisie obligatoire du champ "<FIELD>"

                        break;
                    case TypeTraitConditionnal.FieldUpdate:
                        _sBaseName = eResApp.GetRes(Pref, 8371).Replace("<FIELD>", _fieldInfos.Labels[Pref.LangId]); //Conditions de modification du champ "<FIELD>"
                        break;
                    case TypeTraitConditionnal.FieldForbidClone:
                        if (_fieldInfos.NoDefaultClone || _fieldInfos.IsUnique)//#77 243, KJE: si la rubrique est unique, on n'autorise pasla duplication
                            _sBaseName = eResApp.GetRes(Pref, 7938); // La valeur de la rubrique <FIELD> n'est jamais dupliquée"                  
                        else
                            _sBaseName = eResApp.GetRes(Pref, 7937); // La valeur de la rubrique <FIELD> est duplicable
                        _sBaseName = _sBaseName.Replace("<FIELD>", _fieldInfos.Labels[Pref.LangId]);
                        break;
                    case TypeTraitConditionnal.FieldForbidImport:
                        if (_bIsFieldImportForbidden)
                            _sBaseName = eResApp.GetRes(Pref, 7954); // L'import d'une valeur dans la rubrique <FIELD> est interdit
                        else
                            _sBaseName = eResApp.GetRes(Pref, 7953); // L'import d'une valeur dans la rubrique <FIELD> est autorisé
                        _sBaseName = _sBaseName.Replace("<FIELD>", _fieldInfos.Labels[Pref.LangId]);
                        break;
                    default:
                        _sBaseName = _type.ToString();
                        break;
                }

                _sBaseName = _sBaseName.Replace("<TAB>", _tabInfos.TableLabel);
                _sBaseName = _sBaseName.Replace("<PARENTTAB>", _parentTabInfos.TableLabel);


                //  sTitle = sTitle.Replace("<NAME>", sBaseName);
                sTitle = _sBaseName; // modification de la convention de nommage (17/11/2016)


                sb.Append("var oTitle = top.document.getElementById('td_title_").Append(_sIdModal).AppendLine("');");
                sb.Append("oTitle.innerHTML='").Append(sTitle.Replace("'", "\\'")).Append("';");

                AddCallBackScript(sb.ToString());


                #endregion

                return true;
            }

            return false;
        }


        protected override bool Build()
        {
            _pgContainer.ID = "conditionsAdminModalContent";
            _pgContainer.Attributes.Add("class", "adminModalContent");


            try
            {
                CreateMainDiv();
            }
            catch (Exception e)
            {
                string s = e.Message;
                throw e;
            }

            return true;
        }

        #endregion


        #region Méthodes spécifiques


        /// <summary>
        /// Création des blocs du rendu 
        /// </summary>
        private void CreateMainDiv()
        {

            //cadre titre
            _pgContainer.Controls.Add(GenerateInfosSection());

            //cadre définition des étapes
            _pgContainer.Controls.Add(GenerateSectionRules());


            //cadre définition des caractéristiques
            _pgContainer.Controls.Add(GenerateSectionCarac());

        }


        /// <summary>
        /// Retourne le libellé du résultat de la condition
        /// </summary>
        /// <returns></returns>
        private HtmlGenericControl GetActionLabel(RulesDefinition rDef)
        {
            HtmlGenericControl span = new HtmlGenericControl("span");




            switch (_type)
            {
                case TypeTraitConditionnal.Undefined:
                    span.InnerHtml = eResApp.GetRes(Pref, 7532); //Condition inconnue
                    break;
                case TypeTraitConditionnal.Update:

                    span.InnerHtml = eResApp.GetRes(Pref, 7430); // fiche modifiable
                    break;
                case TypeTraitConditionnal.FieldUpdate:

                    span.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 7551) + "&nbsp;")); // Le champ est 

                    Dictionary<string, string> dicOp = new Dictionary<string, string>();
                    dicOp.Add("0", eResApp.GetRes(Pref, 7552)); // Modifiable
                    dicOp.Add("1", eResApp.GetRes(Pref, 7553)); //En lecture seule


                    HtmlGenericControl sel = eTools.GetSelectCombo(String.Concat("SEL_UPDT_", rDef.RuleId), dicOp, rDef.RuleId != "DEF_NORULES", "", "", ((_fieldInfos?.ReadOnly ?? false) ? "1" : "0"));

                    span.Controls.Add(sel);

                    break;
                case TypeTraitConditionnal.Delete:

                    span.InnerHtml = eResApp.GetRes(Pref, 7431); // "La fiche est supprimable";
                    break;
                case TypeTraitConditionnal.BkmDelete:
                    span.InnerHtml = eResApp.GetRes(Pref, 7431); // "La fiche est supprimable";
                    break;
                case TypeTraitConditionnal.BkmUpdate:
                    span.InnerHtml = eResApp.GetRes(Pref, 7430); // fiche modifiable
                    break;
                case TypeTraitConditionnal.BkmView:
                    span.InnerHtml = eResApp.GetRes(Pref, 7432); // "Le signet est visible";
                    break;
                case TypeTraitConditionnal.BkmAdd:
                    span.InnerHtml = eResApp.GetRes(Pref, 8393); //Il est possible d'ajouter une fiche
                    break;
                case TypeTraitConditionnal.FieldView:
                    span.InnerHtml = eResApp.GetRes(Pref, 7433); // "Le champ est visible";
                    break;
                case TypeTraitConditionnal.Header_View:
                    span.InnerHtml = eResApp.GetRes(Pref, 7434); // "L'entête de la fiche est affiché";
                    break;
                case TypeTraitConditionnal.Header_Update:
                    span.InnerHtml = eResApp.GetRes(Pref, 7435); // "L'entête de la fiche est modifiable";
                    break;
                case TypeTraitConditionnal.Notification:
                    span.InnerHtml = "";
                    break;
                case TypeTraitConditionnal.Export:
                    span.InnerHtml = eResApp.GetRes(Pref, 7436); //  "Permettre l'export (? PAS DE RES FOURNIE 17/11/2016 ?)";
                    break;
                case TypeTraitConditionnal.Merge:
                    span.InnerHtml = eResApp.GetRes(Pref, 7437); //  "Permettre le publipostage (? PAS DE RES FOURNIE 17/11/2016 ?)";
                    break;
                case TypeTraitConditionnal.Mailing:
                    span.InnerHtml = eResApp.GetRes(Pref, 7438); //"Permettre le mailing (? PAS DE RES FOURNIE 17/11/2016?)";
                    break;
                case TypeTraitConditionnal.Color:
                    span = GetRulesColorPicker(rDef);
                    break;
                case TypeTraitConditionnal.FieldObligat:
                    span.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 8373) + "&nbsp;")); // "La saisie du champ est :"

                    Dictionary<string, string> dicObligat = new Dictionary<string, string>();
                    dicObligat.Add("0", eResApp.GetRes(Pref, 7554)); // facultative
                    dicObligat.Add("1", eResApp.GetRes(Pref, 7555)); //obligatoire

                    bool disable = rDef.RuleId != "DEF_NORULES";

                    HtmlGenericControl selObligat = eTools.GetSelectCombo(String.Concat("SEL_OBL_", rDef.RuleId), dicObligat, disable, "", "", disable ? "1" : ((_fieldInfos?.Obligat ?? false) ? "1" : "0"));

                    span.Controls.Add(selObligat);



                    break;
                case TypeTraitConditionnal.FieldForbidClone:
                    //span.Controls.Add(new LiteralControl("La valeur de la rubrique :&nbsp;"));

                    Dictionary<string, string> dicClone = new Dictionary<string, string>();
                    dicClone.Add("0", eResApp.GetRes(Pref, 7935)); // La valeur de la rubrique est duplicable
                    dicClone.Add("1", eResApp.GetRes(Pref, 7936)); // La valeur de la rubrique n'est jamais dupliquée

                    HtmlGenericControl selClone = eTools.GetSelectCombo(String.Concat("SEL_CLN_", rDef.RuleId), dicClone, rDef.RuleId != "DEF_NORULES", "", "", (((_fieldInfos?.NoDefaultClone ?? false) || _fieldInfos.IsUnique)  ? "1" : "0"));//#77 243: on désactive le clonage pour les champs de type login (unique)
                    if (_fieldInfos.IsUnique)
                        selClone.Attributes.Add("disabled", "disabled");
                    span.Controls.Add(selClone);
                    break;
                case TypeTraitConditionnal.FieldForbidImport:
                    //span.Controls.Add(new LiteralControl("L'import d'une valeur dans la rubrique :&nbsp;"));

                    Dictionary<string, string> dicImport = new Dictionary<string, string>();
                    dicImport.Add("0", eResApp.GetRes(Pref, 7951)); // L'import d'une valeur dans la rubrique est autorisé
                    dicImport.Add("1", eResApp.GetRes(Pref, 7952)); // L'import d'une valeur dans la rubrique est interdit

                    HtmlGenericControl selImport = eTools.GetSelectCombo(String.Concat("SEL_IMPRT_", rDef.RuleId), dicImport, rDef.RuleId != "DEF_NORULES", "", "", _bIsFieldImportForbidden ? "1" : "0");

                    span.Controls.Add(selImport);
                    break;
                default:
                    span.InnerHtml = eResApp.GetRes(Pref, 7432);// "Type de condition inconnue";
                    break;
            }

            return span;
        }

        /// <summary>
        /// Retourne  un bloc colorpicker pour la règles=
        /// </summary>
        /// <param name="rDef"></param>
        /// <returns></returns>
        private HtmlGenericControl GetRulesColorPicker(RulesDefinition rDef)
        {

            HtmlGenericControl div = new HtmlGenericControl("div");
            div.Attributes.Add("class", "field");

            HtmlGenericControl labl = new HtmlGenericControl("label");
            labl.Controls.Add(new LiteralControl(String.Concat(eResApp.GetRes(Pref, 6819), " : "))); //Pictogramme
            div.Controls.Add(labl);

            HtmlGenericControl divBtn = new HtmlGenericControl("div");
            div.Controls.Add(divBtn);
            divBtn.ID = "btnSelectPicto_" + rDef.RuleId;
            divBtn.Attributes.Add("onclick", "nsConditions.pickPicto(this)");
            divBtn.Style.Add("display", "inherit");

            HtmlGenericControl spanIco = new HtmlGenericControl("span");
            divBtn.Controls.Add(spanIco);
            spanIco.ID = "selectedPicto_" + rDef.RuleId;


            spanIco.Attributes.Add("class", eFontIcons.GetFontIcon(rDef.Icon).CssName);

            spanIco.Attributes.Add("picto-key", rDef.Icon);
            spanIco.Attributes.Add("picto-color", rDef.Color);
            spanIco.Attributes.Add("picto-class", eFontIcons.GetFontIcon(rDef.Icon).CssName);
            spanIco.Style.Add("color", rDef.Color);

            spanIco.Style.Add("background-color", rDef.Background);


            return div;
        }

        /// <summary>
        /// Block des infos "basiques" sur la conditions
        /// </summary>
        /// <returns></returns>
        private Panel GenerateInfosSection()
        {
            Panel panelTitle = new Panel();

            HtmlGenericControl titleDiv = new HtmlGenericControl("div");
            titleDiv.Attributes.Add("class", "edaFormulaField");
            panelTitle.Controls.Add(titleDiv);


            //Nom
            string titleName = "edaFormulaTitle";

            HtmlGenericControl titleLabel = new HtmlGenericControl("label");
            titleLabel.Attributes.Add("for", titleName);
            titleLabel.Attributes.Add("class", titleName);
            titleLabel.InnerHtml = String.Concat(eResApp.GetRes(Pref, 7216), " :&nbsp;");
            titleDiv.Controls.Add(titleLabel);

            HtmlGenericControl titleTextbox = new HtmlGenericControl("input");
            titleTextbox.Attributes.Add("id", titleName);
            titleTextbox.Attributes.Add("name", titleName);
            titleTextbox.Attributes.Add("class", titleName);
            titleTextbox.Attributes.Add("type", "text");
            titleTextbox.Attributes.Add("disabled", "disabled");
            titleTextbox.Attributes.Add("value", _sBaseName);
            titleDiv.Controls.Add(titleTextbox);


            // Actif/Inactif
            HtmlGenericControl actifDiv = new HtmlGenericControl("div");
            actifDiv.Attributes.Add("class", "edaFormulaField");
            actifDiv.Style.Add("display", "none"); // on n'affiche plus cela (au 17/11/2016)
            panelTitle.Controls.Add(actifDiv);

            string activeName = "edaFormulaActive";
            HtmlGenericControl actifLabel = new HtmlGenericControl("label");
            actifLabel.Attributes.Add("for", activeName);
            actifLabel.Attributes.Add("class", activeName);
            actifLabel.InnerHtml = String.Concat(eResApp.GetRes(Pref, 7218), " &nbsp;");
            actifDiv.Controls.Add(actifLabel);

            HtmlGenericControl actifSelect = new HtmlGenericControl("select");
            actifSelect.Attributes.Add("id", activeName);
            actifSelect.Attributes.Add("name", activeName);
            actifSelect.Attributes.Add("class", activeName);
            actifSelect.Attributes.Add("disabled", "disabled");
            actifDiv.Controls.Add(actifSelect);

            HtmlGenericControl actifOption = new HtmlGenericControl("option");
            actifOption.Attributes.Add("selected", "selected");
            actifOption.Attributes.Add("value", "");
            actifOption.InnerText = eResApp.GetRes(Pref, 7219);
            actifSelect.Controls.Add(actifOption);


            //Concerne..
            HtmlGenericControl ongletDiv = new HtmlGenericControl("div");
            ongletDiv.Attributes.Add("class", "edaFormulaField");
            panelTitle.Controls.Add(ongletDiv);

            HtmlGenericControl ongletSpan = new HtmlGenericControl("span");
            //ongletSpan.InnerHtml = "L'automatisme concerne l'onglet :&nbsp;";
            ongletSpan.InnerHtml = eResApp.GetRes(Pref, 8372).Replace("<TAB>", _tabInfos.TableLabel); //L'automatisme concerne l'onglet <TAB>

            ongletDiv.Controls.Add(ongletSpan);


            return panelTitle;
        }

        /// <summary>
        /// génération de la liste des règles
        /// </summary>
        /// <returns></returns>
        protected virtual Panel GenerateSectionRules()
        {


            #region Initialisation des objets métiers principaux
            //Récupération des règles
            eRules eCondRules = eRules.GetRules(_type, _nDescId, Pref, _nParentTab);


            //Récupération des filtres disponibles pour la règle
            HashSet<int> lTabRules = new HashSet<int>();
            if (_tab != _nParentTab && ((_type == TypeTraitConditionnal.BkmView || _type == TypeTraitConditionnal.BkmAdd)
                || _tab % 100 == AllField.ATTACHMENT.GetHashCode()))
            {
                //Depuis Bkm - Vu ou Visu - la règle est à prendre sur le parent
                //Pour le signet annexe aussi 
                lTabRules.Add(_nParentTab);
                if (_type != TypeTraitConditionnal.BkmView)
                {
                    if (_parentTabInfos.InterEVT)
                        lTabRules.Add(_tabInfos.InterEVTDescid);

                    if (_parentTabInfos.InterPM)
                        lTabRules.Add((int)TableType.PM);


                    if (_parentTabInfos.InterPP)
                        lTabRules.Add((int)TableType.PP);
                }
            }
            else
            {

                lTabRules.Add(_tab);
                if (_type != TypeTraitConditionnal.Color
                    && _type != TypeTraitConditionnal.Header_Update
                    && _type != TypeTraitConditionnal.Header_View
                    )
                {
                    if (_tabInfos.InterEVT)
                        lTabRules.Add(_tabInfos.InterEVTDescid);

                    if (_tabInfos.InterPM)
                        lTabRules.Add((int)TableType.PM);


                    if (_tabInfos.InterPP)
                        lTabRules.Add((int)TableType.PP);
                }
            }

            IEnumerable<AdvFilter> lstAllAvailableFilters = AdvFilter.GetFilterList(Pref, TypeFilter.RULES, lTabRules);

            #endregion

            #region Initialisation des paramètres

            int nCat;
            int nType;


            switch (_type)
            {
                case TypeTraitConditionnal.Undefined:
                    throw new NotImplementedException();

                case TypeTraitConditionnal.Update:
                case TypeTraitConditionnal.BkmUpdate:
                case TypeTraitConditionnal.FieldUpdate:
                    nCat = (int)eAdminUpdateProperty.CATEGORY.DESC;
                    nType = (int)eLibConst.DESC.CHANGERULESID;
                    break;
                case TypeTraitConditionnal.Delete:
                case TypeTraitConditionnal.BkmDelete:
                    nCat = (int)eAdminUpdateProperty.CATEGORY.DESC;
                    nType = (int)eLibConst.DESC.DELETERULESID;
                    break;
                case TypeTraitConditionnal.Color:
                    nCat = (int)eAdminUpdateProperty.CATEGORY.COLOR_RULES;
                    nType = (int)eLibConst.DESC.COLORRULESID;
                    break;
                case TypeTraitConditionnal.Header_View:
                    nCat = (int)eAdminUpdateProperty.CATEGORY.USERVALUE;
                    nType = (int)TypeUserValueAdmin.VIEW_RULES_HEADER;
                    break;

                case TypeTraitConditionnal.Header_Update:
                    nCat = (int)eAdminUpdateProperty.CATEGORY.USERVALUE;
                    nType = (int)TypeUserValueAdmin.UPDATE_RULES_HEADER;
                    break;
                case TypeTraitConditionnal.Export:
                    nCat = (int)eAdminUpdateProperty.CATEGORY.CONDITIONALSENDINGRULES;
                    nType = (int)eLibConst.TREATID.CONDITIONAL_SEND_EXPORT;
                    break;
                case TypeTraitConditionnal.Faxing:
                    nCat = (int)eAdminUpdateProperty.CATEGORY.CONDITIONALSENDINGRULES;
                    nType = (int)eLibConst.TREATID.CONDITIONAL_SEND_FAX;
                    break;

                case TypeTraitConditionnal.Mailing:
                    nCat = (int)eAdminUpdateProperty.CATEGORY.CONDITIONALSENDINGRULES;
                    nType = (int)eLibConst.TREATID.CONDITIONAL_SEND_MAIL;
                    break;

                case TypeTraitConditionnal.Merge:
                    nCat = (int)eAdminUpdateProperty.CATEGORY.CONDITIONALSENDINGRULES;
                    nType = (int)eLibConst.TREATID.CONDITIONAL_SEND_MERGE;
                    break;
                case TypeTraitConditionnal.Voicing:
                    nCat = (int)eAdminUpdateProperty.CATEGORY.CONDITIONALSENDINGRULES;
                    nType = (int)eLibConst.TREATID.CONDITIONAL_SEND_VOICE;
                    break;

                case TypeTraitConditionnal.BkmAdd:
                    nCat = (int)eAdminUpdateProperty.CATEGORY.DESC;

                    if (_nParentTab == (int)TableType.PP)
                        nType = (int)eLibConst.DESC.BKMADDRULESID_200;
                    else if (_nParentTab == (int)TableType.PM)
                        nType = (int)eLibConst.DESC.BKMADDRULESID_300;
                    else
                        nType = (int)eLibConst.DESC.BKMADDRULESID_100;

                    break;

                case TypeTraitConditionnal.BkmView:
                    nCat = (int)eAdminUpdateProperty.CATEGORY.DESC;

                    if (_nParentTab == (int)TableType.PP)
                        nType = (int)eLibConst.DESC.BKMVIEWRULESID_200;
                    else if (_nParentTab == (int)TableType.PM)
                        nType = (int)eLibConst.DESC.BKMVIEWRULESID_300;
                    else
                        nType = (int)eLibConst.DESC.BKMVIEWRULESID_100;


                    break;

                case TypeTraitConditionnal.FieldView:
                    nCat = (int)eAdminUpdateProperty.CATEGORY.DESC;
                    nType = (int)eLibConst.DESC.VIEWRULESID;

                    break;
                case TypeTraitConditionnal.FieldObligat:
                    nCat = (int)eAdminUpdateProperty.CATEGORY.DESC;
                    nType = (int)eLibConst.DESC.OBLIGATRULESID;

                    break;

                case TypeTraitConditionnal.FieldForbidClone:
                    nCat = (int)eAdminUpdateProperty.CATEGORY.DESC;
                    nType = (int)eLibConst.DESC.NODEFAULTCLONE;
                    break;
                case TypeTraitConditionnal.FieldForbidImport:
                    nCat = (int)eAdminUpdateProperty.CATEGORY.SYNC;
                    nType = (int)eConst.SYNC_PARAMETER.FORBID_IMPORT_FIELD_VALUE;
                    break;

                case TypeTraitConditionnal.Notification:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();

            }

            #endregion

            Panel panelStep = new Panel();
            panelStep.CssClass = "divStep";


            HtmlInputText inpt = new HtmlInputText("hidden");
            inpt.ID = "RULES_TAB";
            inpt.Value = _nDescId.ToString();
            panelStep.Controls.Add(inpt);

            HtmlGenericControl inptMain = new HtmlGenericControl("div");
            inptMain.ID = "mainDiv";                //Nécessaire pour la gestion de l'ajout de rules - la page des filtres s'attend a avoir un "mainDiv" avec un "type" 
            inptMain.Attributes.Add("type", "2");  // qui correspond au "type" du filtre (2 pour les règles)
            inptMain.Style.Add("display", "none");
            panelStep.Controls.Add(inptMain);

            // Information de type/catégorie pour la capsule JSON
            HtmlInputText inptCat = new HtmlInputText("hidden");
            panelStep.Controls.Add(inptCat);
            inptCat.ID = "RULES_CAT";
            inptCat.Value = nCat.ToString();

            HtmlInputText inptTyp = new HtmlInputText("hidden");
            inptTyp.ID = "RULES_TYPE";
            inptTyp.Value = nType.ToString();
            panelStep.Controls.Add(inptTyp);

            HtmlInputText inptTreat = new HtmlInputText("hidden");
            inptTreat.ID = "RULES_TREAT";
            inptTreat.Value = ((int)_type).ToString();

            panelStep.Controls.Add(inptTreat);


            if (_type != TypeTraitConditionnal.FieldForbidClone && _type != TypeTraitConditionnal.FieldForbidImport)
            {
                // Titre de la règle
                //Modif suite aux modifs de spec.  17/11/2016
                // apparence style séparateur d'étape validé par RMA 
                //HtmlGenericControl Title = GenerateStepTitle("1", eResApp.GetRes(Pref, 7226), true);
                HtmlGenericControl blockTITLE = GenerateStepTitle("", "", true);
                blockTITLE.Style.Add("cursor", "default"); // pas d'action sur le titre -> pas de curseur particulier


                HtmlGenericControl blockBtnAddRules = new HtmlGenericControl("span");
                blockBtnAddRules.ID = "SHOW_CHOICE";
                blockBtnAddRules.Attributes.Add("class", "stepRulesAllBtn");

                HtmlGenericControl spanIco = new HtmlGenericControl("span");
                spanIco.Attributes.Add("class", " icon-add active");
                blockBtnAddRules.Controls.Add(spanIco);

                blockBtnAddRules.Controls.Add(new LiteralControl("&nbsp;" + eResApp.GetRes(Pref, 7440))); //Nouvelle Condition



                if (_type == TypeTraitConditionnal.Color)
                    blockBtnAddRules.Attributes.Add("onclick", "nsConditions.addCondition()");
                else
                {
                    blockBtnAddRules.Attributes.Add("onclick", "nsConditions.showConditions()");

                    blockBtnAddRules.Attributes.Add("style", "display:" + (eCondRules.AllRules.Count == 0 ? "" : "none"));
                }
                blockTITLE.Controls.Add(blockBtnAddRules);

                panelStep.Controls.Add(blockTITLE);
            }

            int nNbBaseRules = eCondRules.AllRules.Count;
            // Ajout d'une pseudo-rules pour le cas "par défaut" si pas de règles défini et toujours pour les couleurs conditionnels
            if (_type == TypeTraitConditionnal.Color)
            {

                RulesDefinition rdDefaultColor = RulesDefinition.GetRulesDefinition(_type, "DEF_ACTION", "", _tabInfos.IconColor, "", _tabInfos.Icon, new List<Tuple<AdvFilter, InterOperator>>());

                eCondRules.AllRules.Add(rdDefaultColor);

            }
            //    else
            {

                //Block "dans tous les cas"
                RulesDefinition rdDefault = RulesDefinition.GetRulesDefinition(_type, "DEF_NORULES", "", _tabInfos.IconColor, "", _tabInfos.Icon, new List<Tuple<AdvFilter, InterOperator>>());

                eCondRules.AllRules.Add(rdDefault);
            }



            //Block "masqué" de règle pour l'ajout de nouvelle règle
            eCondRules.AllRules.Add(RulesDefinition.GetRulesDefinition(_type, "BASE_FILTER", "", "", "", "", new List<Tuple<AdvFilter, InterOperator>>()));


            int nCmpt = 0;
            //création des blocs de règles
            foreach (var currRules in eCondRules.AllRules)
            {

                //Génération du block règles
                HtmlGenericControl blockFilter = GenerateRules(currRules, lstAllAvailableFilters, nNbBaseRules);
                panelStep.Controls.Add(blockFilter);


                // Block "caché" de règle pour être duppliqué en js pour l'ajout de règle
                if (currRules.RuleId == "BASE_FILTER")
                    blockFilter.Style.Add("display", "none");
                else
                {
                    nCmpt++;
                    if (nCmpt >= 1 && nCmpt < eCondRules.AllRules.Count)
                    {
                        //Ajoute un espace après chaque bloc sauf le dernier
                        HtmlGenericControl divSpace = new HtmlGenericControl("div");
                        divSpace.Attributes.Add("class", "divAdminRulesSpace");
                        panelStep.Controls.Add(divSpace);
                    }
                }

            }


            return panelStep;

        }

        /// <summary>
        /// Génère le bloc d'une règle (block de la règle ou bloc "dans tous les cas")        
        /// </summary>>
        /// <param name="currRules">Liste à créer</param>
        /// <param name="lstAllAvailableFilter">Liste des filtres disponible pour la règle</param>
        /// <param name="nbTotalRules">Nombre total de rules</param>
        /// <returns></returns>
        protected HtmlGenericControl GenerateRules(RulesDefinition currRules, IEnumerable<AdvFilter> lstAllAvailableFilter, int nbTotalRules)
        {

            HtmlGenericControl blockElse;
            HtmlGenericControl blockRuleFilers;

            //Bloc "Dans tous les cas" : Pas de filtre dans la rèlge . BASE_FILTER correspond à une pseudo-règle utilisé pour générer les nouvelles règles via interface user.
            // cf eAdminCondition.js méhtode addCondition/showCondition
            if (currRules.ListFilter.Count == 0 && currRules.RuleId != "BASE_FILTER")
            {
                #region Pas de filtres choisis pour la règle - Bloc affiché si nbTotalRules == 0

                blockElse = new HtmlGenericControl("div");
                blockElse.Attributes.Add("class", "stepContent stepRules stepRulesAll");
                blockElse.ID = "RULES_ALL_" + currRules.RuleId;

                if ((currRules.RuleId == "DEF_NORULES" && (nbTotalRules > 0)))
                    blockElse.Attributes.Add("data-active", "0");
                else if (currRules.RuleId == "DEF_ACTION" && currRules.Type == TypeTraitConditionnal.Color && nbTotalRules == 0)
                    blockElse.Attributes.Add("data-active", "0");
                else
                    blockElse.Attributes.Add("data-active", currRules.ListFilter.Count == 0 ? "1" : "0");


                blockElse.Attributes.Add("ednRuleId", currRules.RuleId.ToString());


                HtmlGenericControl dBlocAll = new HtmlGenericControl("div");
                blockElse.Controls.Add(dBlocAll);


                Panel pAllCase = new Panel();
                dBlocAll.Controls.Add(pAllCase);
                pAllCase.Attributes.Add("class", "stepRulesConditionalBloc");

                if ((nbTotalRules >= 1 && currRules.RuleId != "DEF_NORULES") || (currRules.RuleId == "DEF_ACTION" && currRules.Type == TypeTraitConditionnal.Color))
                    pAllCase.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 7439).ToUpper())); // Sinon
                else
                    pAllCase.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 7227).ToUpper())); // dans tous les cas


                Panel pText = new Panel();
                dBlocAll.Controls.Add(pText);
                pText.Attributes.Add("class", "stepRulesConditionalTextSpace");
                pText.Controls.Add(GetActionLabel(currRules)); // Action/Libelle "Dans tous les cas"


                Panel pEnd = new Panel();
                dBlocAll.Controls.Add(pEnd);
                pEnd.Attributes.Add("class", "stepRulesConditionalBloc");
                pEnd.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 271).ToUpper())); //fin
                #endregion

                return blockElse;
            }
            else
            {
                #region BLoc des filtres

                blockRuleFilers = new HtmlGenericControl("div");
                blockRuleFilers.Attributes.Add("class", "stepContent stepRules");
                blockRuleFilers.ID = "RULES_CHOICE_" + currRules.RuleId;
                blockRuleFilers.Attributes.Add("data-active", currRules.ListFilter.Count == 0 ? "0" : "1"); //règle css pour afficher/masquer un bloc

                Panel pAllCaseChoice = new Panel();
                blockRuleFilers.Controls.Add(pAllCaseChoice);
                pAllCaseChoice.Attributes.Add("class", "stepRulesConditionalBloc");
                HtmlGenericControl lcIf = new HtmlGenericControl("span");
                lcIf.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 7444)));
                lcIf.Attributes.Add("onclick", "nsConditions.showHideSelectedFilter()");
                pAllCaseChoice.Controls.Add(lcIf);

                HtmlGenericControl del = new HtmlGenericControl("span");
                del.ID = "DEL_CHOICE_" + currRules.RuleId;
                del.Attributes.Add("class", "stepRulesNewRulesBtn icon-delete");
                del.Attributes.Add("onclick", "nsConditions.removeCondition(this)");
                pAllCaseChoice.Controls.Add(del);

                HtmlGenericControl gg = new HtmlGenericControl("span");
                gg.ID = "SHOW_CHOICE_" + currRules.RuleId;
                gg.Attributes.Add("class", "stepRulesNewRulesBtn");

                HtmlGenericControl ico = new HtmlGenericControl("span");
                ico.Attributes.Add("class", "icon-add active");
                gg.Controls.Add(ico);

                gg.Controls.Add(new LiteralControl(String.Concat(eResApp.GetRes(Pref, 7173)))); // todo res
                gg.Attributes.Add("onclick", "nsConditions.addNewRules(" + _nParentTab + ")");

                pAllCaseChoice.Controls.Add(gg);

                Dictionary<string, string> dicOp = new Dictionary<string, string>();
                dicOp.Add(((int)InterOperator.OP_AND).ToString(), eResApp.GetRes(Pref, 269));
                dicOp.Add(((int)InterOperator.OP_OR).ToString(), eResApp.GetRes(Pref, 270));

                int n = 0; //Compteur de filtre sélectionné dans la règle

                //Création des lignes de filtres disponibles
                foreach (AdvFilter r in lstAllAvailableFilter)
                {
                    HtmlGenericControl lFilter = new HtmlGenericControl("div");
                    lFilter.Attributes.Add("class", "stepRulesConditionalFilterBloc");

                    //Recherche du filtre et de son inter opérateur dans la liste des filtres sélectionné pour la règle
                    Tuple<AdvFilter, InterOperator> eSelFilterTuple = currRules.ListFilter.Find(zz => zz.Item1.FilterId == r.FilterId);

                    bool bIsChecked = eSelFilterTuple != null;

                    if (bIsChecked)
                        n++;

                    //
                    eCheckBoxCtrl myCheck = new eCheckBoxCtrl(bIsChecked, false);
                    myCheck.AddClick("nsConditions.optionOnClick(this )");
                    myCheck.ID = String.Concat("CHK_ID_", currRules.RuleId, "_", r.FilterId);
                    myCheck.ToolTip = eResApp.GetRes(Pref, 7639);
                    lFilter.Controls.Add(myCheck);

                    //
                    HtmlGenericControl sel = eTools.GetSelectCombo(String.Concat("SEL_OP_", currRules.RuleId, "_", r.FilterId), dicOp, false, "", "",
                        ((int)(eSelFilterTuple != null ? eSelFilterTuple.Item2 : InterOperator.OP_AND)).ToString());

                    //sel.Style.Add("width", "40px");

                    //Le "select" ne doit pas être affiché pour le 1er filtre sélectionné (ni pour les filtres non sélectionné)
                    if (!bIsChecked || n == 1)
                        sel.Style.Add("visibility", "hidden");

                    lFilter.Controls.Add(sel);

                    HtmlGenericControl div = new HtmlGenericControl("div");
                    lFilter.Controls.Add(div);
                    div.Attributes.Add("class", "rulesDescription");
                    div.Controls.Add(new LiteralControl(r.GetFilterDescription()));
                    blockRuleFilers.Controls.Add(lFilter);
                }

                Panel pEndChoice = new Panel();
                blockRuleFilers.Controls.Add(pEndChoice);
                pEndChoice.Attributes.Add("class", "stepRulesConditionalBloc");
                // 7445 - ALORS
                pEndChoice.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 7445)));

                Panel pTextChoice = new Panel();
                blockRuleFilers.Controls.Add(pTextChoice);
                pTextChoice.Attributes.Add("class", "stepRulesConditionalText");
                pTextChoice.Controls.Add(GetActionLabel(currRules));

                if (nbTotalRules == 1 && currRules.Type != TypeTraitConditionnal.Color)
                {
                    Panel pEnd = new Panel();
                    blockRuleFilers.Controls.Add(pEnd);
                    pEnd.Attributes.Add("class", "stepRulesConditionalBloc");
                    pEnd.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 271))); //fin
                }
                #endregion

                return blockRuleFilers;
            }
        }

        /// <summary>
        /// Section caractéristique
        ///  - retiré au 17/11/2016 - changement de spec
        /// </summary>
        /// <returns></returns>
        private Panel GenerateSectionCarac()
        {


            Panel panelStep = new Panel();

            return panelStep;

            #region obsolete pour l'instant - à conserver pour l'instant cependant (17/11/2016)
            // on affiche plus ce bloc (17/11/2016)
            /*
            panelStep.CssClass = "divStep";

            panelStep.Controls.Add(
                GenerateStepTitle("2", eResApp.GetRes(Pref, 7220), true)
                );

            HtmlGenericControl stepContent = new HtmlGenericControl("div");
            stepContent.Attributes.Add("class", "stepContent");
            stepContent.Attributes.Add("data-active", "1");
            panelStep.Controls.Add(stepContent);

            HtmlGenericControl triggerLabelDiv = new HtmlGenericControl("div");
            stepContent.Controls.Add(triggerLabelDiv);
            HtmlGenericControl triggerLabel = new HtmlGenericControl("label");
            triggerLabel.InnerText = String.Concat(eResApp.GetRes(Pref, 7221), " :");
            triggerLabelDiv.Controls.Add(triggerLabel);

            string chbxCssClass = "edaFormulaStep1Chbx";

            HtmlGenericControl chbxCreateDiv = new HtmlGenericControl("div");
            stepContent.Controls.Add(chbxCreateDiv);
            eCheckBoxCtrl chbxCreate = new eCheckBoxCtrl(true, true);
            chbxCreate.AddText(eResApp.GetRes(Pref, 7222));
            chbxCreate.AddClass(chbxCssClass);
            chbxCreate.AddClick();
            chbxCreateDiv.Controls.Add(chbxCreate);

            HtmlGenericControl chbxDeletelDiv = new HtmlGenericControl("div");
            stepContent.Controls.Add(chbxDeletelDiv);
            eCheckBoxCtrl chbxDelete = new eCheckBoxCtrl(false, true);
            chbxDelete.AddText(eResApp.GetRes(Pref, 7223));
            chbxDelete.AddClass(chbxCssClass);
            chbxDelete.AddClick();
            chbxDeletelDiv.Controls.Add(chbxDelete);

            HtmlGenericControl chbxUpdateDiv = new HtmlGenericControl("div");
            stepContent.Controls.Add(chbxUpdateDiv);
            eCheckBoxCtrl chbxUpdate = new eCheckBoxCtrl(true, true);
            chbxUpdate.AddText(eResApp.GetRes(Pref, 7224));
            chbxUpdate.AddClass(chbxCssClass);
            chbxUpdate.AddClick();
            chbxUpdateDiv.Controls.Add(chbxUpdate);

            HtmlGenericControl updateFieldDiv = new HtmlGenericControl("div");
            stepContent.Controls.Add(updateFieldDiv);
            eCheckBoxCtrl chbxUpdateField = new eCheckBoxCtrl(false, true);
            chbxUpdateField.AddText(eResApp.GetRes(Pref, 7225));
            chbxUpdateField.AddClass(chbxCssClass);
            chbxUpdateField.AddClick();
            updateFieldDiv.Controls.Add(chbxUpdateField);

            HtmlGenericControl updateFieldSelect = new HtmlGenericControl("select");
            updateFieldSelect.Attributes.Add("disabled", "disabled");
            updateFieldSelect.Attributes.Add("class", "edaFormulaStep1UpdDdl");
            updateFieldDiv.Controls.Add(updateFieldSelect);



            return panelStep;
            */
            #endregion
        }

        /// <summary>
        /// Génère la partie titre de l'étape
        /// </summary>
        /// <param name="sNum"></param>
        /// <param name="title"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        private HtmlGenericControl GenerateStepTitle(String sNum, String title, Boolean active)
        {
            String classActive = (active) ? " active" : String.Empty;

            HtmlGenericControl step = new HtmlGenericControl("div");
            step.ID = String.Concat("stepTitle", sNum);
            step.Attributes.Add("class", "paramStep" + classActive);
            if (sNum.Length > 0)
            {
                HtmlGenericControl span = new HtmlGenericControl();
                span.InnerText = sNum;
                span.Attributes.Add("class", "stepNum");
                step.Controls.Add(span);
                span = new HtmlGenericControl();
                span.InnerText = title;
                span.Attributes.Add("class", "stepTitle");
                step.Controls.Add(span);
            }
            return step;
        }


        #endregion

    }
}