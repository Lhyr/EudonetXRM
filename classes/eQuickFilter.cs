using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <className>eQuickFilter</className>
    /// <summary>Classe de la gestion du rendu filtre rapide d'une rubrique</summary>
    /// <purpose>Gestion du rendu filtre rapide d'une rubrique</purpose>
    /// <authors>HLA</authors>
    /// <date>2011-12-22</date>
    public class eQuickFilter
    {
        private Field _field = null;
        private EudoQuery.Table _mainTable = null;

        /// <summary>préférences de l'utilisateur</summary>
        private ePref _pref = null;
        /// <summary>Indicateur si c'est une nouvelle connexion locale (connexion propre au filtre rapide)</summary>
        private Boolean _localDal = false;
        /// <summary>Connexion</summary>
        private eudoDAL _dal = null;

        private List<Field> _AllQuickFieldFilter = new List<Field>();

        #region propriétés

        /// <summary>Indicateur si l'on affiche &lt;TOUS&gt; dans le choix des valeurs</summary>
        private Boolean _optShowAll = false;
        /// <summary>Indicateur si l'on affiche &lt;AVANCE&gt; dans le choix des valeurs</summary>
        private Boolean _optShowAdv = false;
        /// <summary>Indicateur si l'on affiche &lt;FICHE PUBLIQUE&gt; dans le choix des valeurs</summary>
        private Boolean _optShowPublic = false;
        /// <summary>Indicateur si l'on affiche les groupes dans le choix des valeurs</summary>
        private Boolean _optShowGroup = false;

        #endregion

        #region accesseurs

        /// <summary>
        /// Main table de la requête EudoQuery
        /// </summary>
        public EudoQuery.Table MainTable
        {
            get { return _mainTable; }
            set { _mainTable = value; }
        }

        /// <summary>
        /// Defini le field sur lequel se porte le filtre rapide
        /// </summary>
        public Field SetField
        {
            set { _field = value; }
        }

        /// <summary>Indicateur si l'on affiche &lt;TOUS&gt; dans le choix des valeurs</summary>
        private Boolean OptShowAll
        {
            get { return _optShowAll; }
            set { _optShowAll = value; }
        }
        /// <summary>Indicateur si l'on affiche &lt;AVANCE&gt; dans le choix des valeurs</summary>
        private Boolean OptShowAdv
        {
            get { return _optShowAdv; }
            set { _optShowAdv = value; }
        }
        /// <summary>Indicateur si l'on affiche &lt;FICHE PUBLIQUE&gt; dans le choix des valeurs</summary>
        private Boolean OptShowPublic
        {
            get { return _optShowPublic; }
            set { _optShowPublic = value; }
        }
        /// <summary>Indicateur si l'on affiche les groupes dans le choix des valeurs</summary>
        private Boolean OptShowGroup
        {
            get { return _optShowGroup; }
        }


        /// <summary>
        /// Liste des field de quick filter
        /// Utilisé pour la dépendance entre filtr
        /// </summary>
        public List<Field> AllQuickFieldFilter
        {
            get
            {
                return _AllQuickFieldFilter ?? new List<Field>();
            }

            set
            {
                this._AllQuickFieldFilter = value;
            }
        }

        #endregion

        /// <summary>
        /// Constructeur de l'objet de rendu d'une rubrique
        /// Remarque : toujours appeler la méthode "Close" après avoir terminé le traitement pour libérer les connexions et ressources 
        /// </summary>
        /// <param name="fld">Rubrique source du rendu (Valeur obligatoire)</param>
        /// <param name="pref">Pref de utilisateur en session (Valeur obligatoire)</param>
        /// <param name="eDal">Connexion (Valeur null possible)</param>
        public eQuickFilter(Field fld, ePref pref, eudoDAL eDal)
        {
            _field = fld;
            _pref = pref;

            // Si connexion "null" => connexion local
            _localDal = (eDal == null);
            if (_localDal)
            {
                // Nouvelle connexion locale
                _dal = eLibTools.GetEudoDAL(pref);
                _dal.OpenDatabase();
            }
        }

        /// <summary>
        /// Fermeture de la connexion local si elle existe
        /// </summary>
        public void Close()
        {
            // Si connexion locale => fermeture de celle-ci
            if (_localDal)
                _dal.CloseDatabase();
        }

        /// <summary>
        /// Rendu du filtre rapide du field en mode liste
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public Control RendQuickFilterList(out Int32 idx)
        {
            idx = 0;

            if (!_field.QuickFilterInfo.Has)
                return null;

            Field fldQuickFilter = null;
            String dbParam = String.Empty;
            EudoQuery.TypeUserValue typQuickFilter;

            Boolean mruMenu = _pref.GetConfig(eLibConst.PREF_CONFIG.MENULISTMRUENABLED).Equals("1");

            HtmlTable qckFltTable = new HtmlTable();
            HtmlTableRow qckFltTableRow = new HtmlTableRow();
            qckFltTable.Controls.Add(qckFltTableRow);

            // Affichage en liste
            List<ListItem> list = null;
            // Affichage en bouton
            HtmlTableCell qckFltTableCellBtn = new HtmlTableCell();
            HtmlGenericControl qckFltBtn = null;

            #region Informations sur le filtre rapide

            if (_field.QuickFilterInfo.DbParam.TryGetValue("index", out dbParam))
                idx = eLibTools.GetNum(dbParam);

            typQuickFilter = EudoQuery.TypeUserValue.FILTER_QUICK;
            if (_field.QuickFilterInfo.DbParam.TryGetValue("type", out dbParam))
                typQuickFilter = (EudoQuery.TypeUserValue)eLibTools.GetNum(dbParam);

            Boolean treeViewQuickUserFilter = false;
            if (_field.QuickFilterInfo.DbParam.TryGetValue("treeviewuserlist", out dbParam))
                treeViewQuickUserFilter = dbParam.Equals("1");

            #endregion

            fldQuickFilter = _field;

            if (typQuickFilter != EudoQuery.TypeUserValue.FILTER_QUICK
                && !_field.QuickFilterInfo.IsQuickUserFilter)
                return null;

            // Cas spécifique - MRU
            if (_field.Format == FieldFormat.TYP_USER)
            {
                qckFltBtn = GetBtnQuickFilterUser(idx);
            }

            // Cas spécifique - Rubrique logique
            else if (!_field.QuickFilterInfo.SpecialField && _field.Format == FieldFormat.TYP_BIT)
            {
                list = GetListQuickFilterBit();
            }
            // Autres cas
            else
            {
                list = GetListQuickFilterValue(out fldQuickFilter);
            }

            // Conteneur
            qckFltTableRow.ID = String.Concat("SPAN_", fldQuickFilter.Descid);

            // Label de la rubrique
            HtmlTableCell htmlLabel = new HtmlTableCell();
            htmlLabel.InnerHtml = String.Concat(System.Web.HttpUtility.HtmlEncode(fldQuickFilter.Libelle), " : ");
            htmlLabel.Attributes.Add("class", "qckfltlbl");
            qckFltTableRow.Controls.Add(htmlLabel);

            if (list != null)
            {
                HtmlTableCell htmlListCell = new HtmlTableCell();

                // Liste de la rubrique avec ces propriétés
                HtmlSelect htmlList = new HtmlSelect();     // Objet liste "<SELECT>"
                htmlList.ID = String.Concat("QuickF_", fldQuickFilter.Descid.ToString());
                htmlList.Attributes.Add("size", "1");
                htmlList.Attributes.Add("ednIdx", idx.ToString());
                htmlList.Attributes.Add("onchange", "doQuickFilter(this);");
                htmlList.Items.AddRange(list.ToArray());

                // Remplacement du taquet vert moche
                if (_field.QuickFilterInfo.Actived)
                    htmlList.Attributes.Add("class", "activeQF");

                htmlListCell.Controls.Add(htmlList);
                qckFltTableRow.Controls.Add(htmlListCell);

                qckFltTableRow.Attributes.Add("class", "QuickFilterCont");
            }
            else if (qckFltBtn != null)
            {
                qckFltTableCellBtn.Controls.Add(qckFltBtn);
                qckFltTableRow.Controls.Add(qckFltTableCellBtn);
                qckFltTableRow.Attributes.Add("class", "QckUsrFltContainer");
            }
            else
                return null;        // Contenu vide

            return qckFltTable;
        }


        #region valeurs communes (<VIDE>, <PUBLIQUE>, <TOUS>, <AVANCE>, ...)

        private ListItem QuickFilterItemEmpty(Boolean selected)
        {
            ListItem item = new ListItem();
            item.Text = eResApp.GetRes(_pref, 141).ToUpper();
            item.Attributes.Add("title", item.Text);
            item.Value = "";
            item.Selected = selected;
            item.Attributes.Add("style", String.Concat("background-color:", eConst.COL_USR_SPEC, ";"));
            return item;
        }

        private ListItem QuickFilterItemAll(Boolean selected)
        {
            ListItem item = new ListItem();
            item.Text = eResApp.GetRes(_pref, 22).ToUpper();
            item.Attributes.Add("title", item.Text);
            item.Value = "-1";
            item.Selected = selected;
            item.Attributes.Add("style", String.Concat("background-color:", eConst.COL_USR_SPEC, ";"));
            return item;
        }

        private ListItem QuickFilterItemPublic(Boolean selected)
        {
            ListItem item = new ListItem();
            item.Text = eResApp.GetRes(_pref, 53).ToUpper();
            item.Attributes.Add("title", item.Text);
            item.Value = "0";
            item.Selected = selected;
            item.Attributes.Add("style", String.Concat("background-color:", eConst.COL_USR_SPEC, ";"));
            return item;
        }

        private ListItem QuickFilterItemAdv(Boolean selected)
        {
            ListItem item = new ListItem();
            item.Text = eResApp.GetRes(_pref, 501).ToUpper();
            item.Attributes.Add("title", item.Text);
            item.Value = "ADV";
            item.Selected = selected;
            item.Attributes.Add("style", String.Concat("background-color:", eConst.COL_USR_SPEC, ";"));
            return item;
        }

        #endregion

        #region Les rendus

        /// <summary>
        /// Renvoie le code HTML d'un filtre rapide type Utilisateur (champ input avec bouton séparé pour imiter le style d'un select avec menu déroulant)
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        private HtmlGenericControl GetBtnQuickFilterUser(Int32 idx)
        {
            #region Déclaration éléments
            HtmlGenericControl qckFltBtnContainer = new HtmlGenericControl("ul");
            HtmlGenericControl qckFltBtnPuce = new HtmlGenericControl("li");
            HtmlGenericControl qckFltBtnInput = new HtmlGenericControl("li");
            HtmlGenericControl qckFltBtnArrow = new HtmlGenericControl("li");
            #endregion

            #region Construction des contrôles
            #region Champ input
            HtmlInputText text = new HtmlInputText();
            text.ID = String.Concat("QckUsrFltTxt_", _field.Descid);
            text.Attributes.Add("readonly", "true");
            text.Attributes.Add("dbv", _field.QuickFilterInfo.Value);
            text.Attributes.Add("fulluserlist", _field.IsFullUserList ? "1" : "0");
            text.Attributes.Add("treeviewuserlist", "0");       // Pas d'arbo dans ce cas pour l'option <AVANCE>
            text.Attributes.Add("mult", "1");       // Toujours en multiple pour l'option <AVANCE>
            text.Attributes.Add("showemptygroup", "0");       // Ne pas afficher les groupe vide pour l'option <AVANCE>
            //text.Attributes.Add("class", "qckUsrFltInput");
            text.Attributes.Add("ednIdx", idx.ToString());
            #endregion

            #region Puce (taquet vert/blanc)
            HtmlGenericControl btnPuce = new HtmlGenericControl("div");
            btnPuce.ID = String.Concat("QckUserFltTag_", _field.Descid);

            if (_field.QuickFilterInfo.Actived)
            {
                btnPuce.Attributes.Add("class", "puce");

                if (_field.QuickFilterInfo.Op == Operator.OP_IS_EMPTY || _field.QuickFilterInfo.Op == Operator.OP_0_EMPTY)
                {
                    if (_field.Descid == _field.Table.GetOwnerDescId() || _field.Descid == _field.Table.GetMultiOwnerDescId())
                    {
                        // Fiche publique
                        text.Value = eResApp.GetRes(_pref, 53);
                    }
                    else
                    {
                        // <VIDE>
                        text.Value = eResApp.GetRes(_pref, 141).ToUpper();
                    }
                }
                else if (_field.QuickFilterInfo.Value == "0")
                {
                    // Fiche publique
                    text.Value = eResApp.GetRes(_pref, 53);
                }
                else
                {
                    text.Value = _field.QuickFilterInfo.DisplayValue;
                }
            }
            else
            {
                // <TOUS>
                btnPuce.Attributes.Add("class", "nopuce");
                text.Value = eResApp.GetRes(_pref, 435).ToUpper();
            }
            /*
            int nLetterSize = 11;
            int nMaxLetters = 0;
            while (nMaxLetters * nLetterSize < 137 && nMaxLetters < text.Value.Length)
                nMaxLetters++;
            text.Value = text.Value.Substring(0, nMaxLetters) + "...";
            */

            text.Attributes.Add("title", text.Value);

            // Pas d'affichage de puce "filtre activé" sur le champ "A faire par" de l'onglet Planning
            // Le contrôle doit tout de même se trouver sur la page pour permettre le fonctionnement des JS
            bool isPlanningMultiOwner = _mainTable.EdnType == EdnType.FILE_PLANNING && _field.Descid == _mainTable.DescId + AllField.TPL_MULTI_OWNER.GetHashCode();
            if (isPlanningMultiOwner)
            {
                btnPuce.Style.Add("visibility", "hidden"); // Visible = false n'ajoute pas le contrôle sur la page...
            }

            text.Attributes.Add("class", "activeQF");

            #endregion

            #region Bouton fléché
            qckFltBtnArrow.Attributes.Add("class", "icon-input");
            #endregion

            #region Action au clic
            StringBuilder sbOnClick = new StringBuilder("doQckUsrFlt('");
            sbOnClick.Append(text.ID).Append("', ").Append(_field.Descid).Append(", ").Append(_field.Format.GetHashCode())
                .Append(", ").Append(_field.Table.DescId).Append(", ").Append(_field.Table.EdnType.GetHashCode())
                .Append(", ").Append(_field.Multiple ? "true" : "false").Append(", ")
                .Append(_pref.User.UserId).Append(", '").Append(_pref.User.UserDisplayName.Replace("'", @"\'")).Append("'");
            sbOnClick.Append(")");

            qckFltBtnArrow.Attributes["onclick"] = sbOnClick.ToString();
            text.Attributes["onclick"] = sbOnClick.ToString();
            #endregion
            #endregion

            #region Construction du tableau
            qckFltBtnContainer.Attributes.Add("class", "QckUsrFltContent");
            qckFltBtnPuce.Controls.Add(btnPuce);
            qckFltBtnInput.Controls.Add(text);
            qckFltBtnContainer.Controls.Add(qckFltBtnPuce);
            qckFltBtnContainer.Controls.Add(qckFltBtnInput);
            qckFltBtnContainer.Controls.Add(qckFltBtnArrow);
            #endregion

            return qckFltBtnContainer;
        }


        /// <summary>
        /// Renvoi la liste des valeurs du field pour les filtre rapidede type logique
        /// </summary>
        /// <returns></returns>
        private List<ListItem> GetListQuickFilterBit()
        {
            ListItem item;
            Boolean itemSelected;
            List<ListItem> list = new List<ListItem>();

            // Rubrique non visible
            if (!_field.PermViewAll)
                return null;

            Boolean fldLogiqueChecked = _field.QuickFilterInfo.Value.Equals("1");

            item = new ListItem();
            item.Text = eResApp.GetRes(_pref, 308);
            item.Value = "1";
            item.Selected = _field.QuickFilterInfo.Actived && fldLogiqueChecked;
            list.Add(item);

            item = new ListItem();
            item.Text = eResApp.GetRes(_pref, 309);
            item.Value = "0";
            item.Selected = _field.QuickFilterInfo.Actived && !fldLogiqueChecked;
            list.Add(item);

            // Option <TOUS>
            itemSelected = String.IsNullOrEmpty(_field.QuickFilterInfo.Value) && !_field.QuickFilterInfo.Actived;
            list.Add(QuickFilterItemAll(itemSelected));

            return list;
        }

        /// <summary>
        /// Renvoi la liste des valeurs du field pour les filtre rapide
        /// </summary>
        /// <param name="fldQuickFilter">objet field en retour correspondant à la rubrique demandée</param>
        /// <returns></returns>
        private List<ListItem> GetListQuickFilterValue(out Field fldQuickFilter)
        {
            ListItem item;
            Boolean itemSelected;
            List<ListItem> list = new List<ListItem>();

            Boolean doSortList = false;

            // Table
            Int32 tabQuickFilter = _field.QuickFilterInfo.TabFor != 0 ? _field.QuickFilterInfo.TabFor : _field.Table.DescId;
            // Field
            Int32 colQuickFilter = _field.QuickFilterInfo.SpecialField ? tabQuickFilter + 1 : _field.Descid;

            #region EudoQuery

            int nMainDescId = /* _field.SpecialPopup ? _field.PopupDescId - 1 :*/ MainTable.DescId;





            EudoQuery.EudoQuery subQuery = eLibTools.GetEudoQuery(_pref, nMainDescId, ViewQuery.FIELD_VALUES);
            String error = subQuery.GetError;
            if (!String.IsNullOrEmpty(error))
                throw new Exception("Erreur EudoQuery.Init pour " + error);

            //Flag Filtre Rapide depuis XRM
            subQuery.AddParam("FROMQFXRM", "1");
            subQuery.SetListCol = colQuickFilter.ToString();


            //Catalogue lié : ajout de la condition sur la liason
            if (_field.BoundDescid > 0)
            {
                var parent = (AllQuickFieldFilter.Find(fld => fld.PopupDescId == _field.BoundDescid));
                if (parent != null)
                {
                    var v = parent.QuickFilterInfo.Value;
                    if (parent.QuickFilterInfo.Actived && !string.IsNullOrEmpty(v))
                        subQuery.AddCustomFilter(new WhereCustom(parent.Descid.ToString(), Operator.OP_EQUAL, v));
                }
            }

            subQuery.LoadRequest();
            error = subQuery.GetError;
            if (!String.IsNullOrEmpty(error))
                throw new Exception("Erreur EudoQuery.LoadRequest pour " + error);

            subQuery.BuildRequest();
            error = subQuery.GetError;
            if (!String.IsNullOrEmpty(error))
                throw new Exception("Erreur EudoQuery.BuildRequest pour " + error);

            String request = subQuery.EqQuery;
            error = subQuery.GetError;
            if (!String.IsNullOrEmpty(error))
                throw new Exception("Erreur EudoQuery.GetQuery pour " + error);

            List<Field> subFields = subQuery.GetFieldHeaderList;

            subQuery.CloseQuery();

            #endregion

            #region Rubrique du filtre express

            fldQuickFilter = null;

            foreach (Field subFld in subFields)
            {
                if (subFld.DrawField && subFld.Descid == colQuickFilter)
                {
                    fldQuickFilter = subFld;
                    break;
                }
            }

            if (fldQuickFilter == null)
                throw new Exception("Méchant fldQuickFilter pas trouvé");

            // Rubrique non visible
            if (!fldQuickFilter.PermViewAll)
                return null;

            #endregion

            DataTableReader dtrValues = _dal.ExecuteDTR(request, out error);

            String valueToTest = String.Empty;      // Valeur retourné par EudoQuery et comparé pour selectionner la valeur en cours
            String optText = String.Empty;          // Text à l'affichage
            String optValue = String.Empty;         // Valeur de l'option de la HtmlSelect
            List<String> valuesAlreadyAdded = new List<String>();       // Pour dédoublonner les valeurs, liste des valeurs déjà ajoutée à notre liste de listitem
            try
            {
                if (!String.IsNullOrEmpty(error))
                    throw new Exception("Erreur requête |" + request + "|" + error);
                // Pas de parcours des valeurs si aucune valeurs trouvées dans les fiches
                if (dtrValues != null)
                {
                    #region Parcours des valeurs
                    Boolean bSelectedItem = false;
                    while (dtrValues.Read())
                    {
                        // Valeurs
                        optValue = EscapeQuotes(dtrValues.GetSafeValue(fldQuickFilter.Alias));
                        optText = EscapeQuotes(dtrValues.GetSafeValue(fldQuickFilter.ValueAlias));

                        if (fldQuickFilter.Popup == PopupType.DATA)
                        {
                            // Si la valeur du filedata n'existe plus, la variable optText est vide.
                            // On n'affiche pas cette valeur dans le filtre rapide, ça présente aucun intérêt.
                            if (String.IsNullOrEmpty(optText))
                                continue;

                            eLibTools.GetPopupData(optText, out optText, out optValue);
                        }
                        else if (fldQuickFilter.Format == FieldFormat.TYP_GROUP)
                        {

                            if (String.IsNullOrEmpty(optText))
                                continue;

                            eLibTools.GetUserMultipleData(optText, out optText, out optValue);
                        }
                        else
                        {
                            if (_field.QuickFilterInfo.SpecialField)
                            {
                                if (fldQuickFilter.Descid == 201 || fldQuickFilter.PopupDescId == 201)
                                    optText = eLibTools.GetPpName(dtrValues, fldQuickFilter, fldQuickFilter.FldPrenom, fldQuickFilter.FldParticule, fldQuickFilter.Table.NameFormat);

                                optValue = dtrValues[fldQuickFilter.Table.Alias + "_ID"].ToString();
                            }
                        }

                        if (fldQuickFilter.Multiple)
                        {
                            doSortList = true;

                            optText = optText.Replace(" ; ", ";");
                            optValue = optValue.Replace(" ; ", ";");

                            String[] aText = optText.Split(";");
                            String[] aValue = optValue.Split(";");

                            for (int idxVal = 0; idxVal < aValue.Length; idxVal++)
                            {
                                String val = aValue[idxVal].Trim();

                                if (String.IsNullOrEmpty(val))
                                    continue;

                                if (valuesAlreadyAdded.Contains(val))
                                    continue;

                                // Ajoute des valeurs
                                item = new ListItem();
                                item.Text = aText[idxVal];
                                item.Value = val;
                                item.Selected = _field.QuickFilterInfo.Value.Equals(val);
                                list.Add(item);
                                valuesAlreadyAdded.Add(val);
                            }
                        }
                        else
                        {
                            //Pas de doublon
                            if (list.Exists(zz => (zz.Text == optText && zz.Value == optValue)))
                                continue;

                            // Option
                            item = new ListItem();
                            item.Text = optText;
                            item.Value = optValue;


                            if (!bSelectedItem && _field.QuickFilterInfo.Value.Equals(item.Value))
                            {
                                item.Selected = _field.QuickFilterInfo.Value.Equals(item.Value);
                                bSelectedItem = true;
                            }


                            list.Add(item);

                        }
                    }

                    if (!dtrValues.IsClosed)
                    {
                        dtrValues.Close();
                        dtrValues.Dispose();
                    }

                    #endregion

                    if (doSortList)
                        list.Sort(new ListItemComparer());
                }
            }
            finally
            {
                if (dtrValues != null)
                    dtrValues.DisposeClose();
            }
            Int32 cntItem = list.Count;

            // Option <TOUS>
            itemSelected = String.IsNullOrEmpty(_field.QuickFilterInfo.Value) && !_field.QuickFilterInfo.Actived;
            list.Insert(0, QuickFilterItemAll(itemSelected));

            // Option <VIDE>
            if ((!fldQuickFilter.Multiple && MainTable.TabType != TableType.HISTO) || fldQuickFilter.Format == FieldFormat.TYP_GROUP)
            {
                itemSelected = String.IsNullOrEmpty(_field.QuickFilterInfo.Value) && _field.QuickFilterInfo.Actived;
                list.Insert(1, QuickFilterItemEmpty(itemSelected));
            }

            if (cntItem >= 20)
            {
                // Option <VIDE>
                if ((!fldQuickFilter.Multiple && MainTable.TabType != TableType.HISTO) || fldQuickFilter.Format == FieldFormat.TYP_GROUP)
                {
                    itemSelected = false;       // Second affichage
                    list.Add(QuickFilterItemEmpty(itemSelected));
                }

                // Option <TOUS>
                itemSelected = false;       // Second affichage
                list.Add(QuickFilterItemAll(itemSelected));
            }

            return list;
        }

        /// <summary>
        /// La chaine a inséré dans un sélect ne doit pas avoir un " ou ' sinon ca coupe la valeur
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string EscapeQuotes(string str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                str = str.Replace('"', '\"');
                str = str.Replace("'", "\'");
            }

            return str;
        }

        #endregion
    }
}