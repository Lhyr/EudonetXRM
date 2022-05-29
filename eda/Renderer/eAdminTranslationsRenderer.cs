using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer pour la liste des traductions
    /// </summary>
    public class eAdminTranslationsRenderer : eAdminRenderer
    {
        /// <summary>
        /// représente le contexte du renderer
        /// </summary>
        public enum ACTION
        {
            /// <summary>Premier affichage </summary>
            Initial = 0,
            /// <summary>Rafraichissement de la liste</summary>
            RefreshList = 1
        }

        /// <summary>
        /// renvoie le bon renderer en fonction de l'utilisation souhaitée
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static eAdminTranslationsRenderer CreateAdminTranslationsRenderer(ePref pref, ACTION action)
        {
            eAdminTranslationsRenderer rdr = null;
            switch (action)
            {
                case ACTION.Initial:
                    rdr = new eAdminTranslationsRenderer(pref);
                    break;
                case ACTION.RefreshList:
                    rdr = new eAdminTranslationsListRenderer(pref);
                    break;
                default:
                    break;
            }
            return rdr;

        }

        /// <summary>
        /// The nature
        /// </summary>
        public eAdminTranslation.NATURE Nature = eAdminTranslation.NATURE.None;
        /// <summary>
        /// The language identifier
        /// </summary>
        public Int32 LangId = -1;
        /// <summary>
        /// The search
        /// </summary>
        public String Search = "";
        /// <summary>
        /// The desc identifier
        /// </summary>
        public Int32 DescId = 0;
        /// <summary>
        /// The file identifier
        /// </summary>
        public int FileId = 0;
        /// <summary>Diffère du DescId pour les catalogues avancés et les specifs</summary>
        public Int32 ResId = 0;
        /// <summary>
        /// The sorts
        /// </summary>
        public List<eAdminTranslationsList.OrderBy> Sorts = null;

        private eAdminTranslationsList translationsList;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref"></param>
        public eAdminTranslationsRenderer(ePref pref)
        {
            Pref = pref;
        }

        /// <summary>
        /// génère l'objet métier représentant la liste des traductions
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            translationsList = new eAdminTranslationsList(Pref);
            translationsList.DescId = DescId;
            translationsList.ResId = ResId;
            translationsList.Nature = Nature;
            translationsList.LangId = LangId;
            translationsList.Search = Search;
            translationsList.FileId = FileId;
            if (Sorts != null)
                translationsList.Sorts = Sorts;
            translationsList.LoadTranslations();
            if (translationsList.Error.Length > 0)
            {
                _sErrorMsg = translationsList.Error;
                return false;
            }

            return true;
        }


        /// <summary>
        /// Génère les différentes parties de l'écran des traductions
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            PgContainer.ID = "divTranslations";


            CreateHeader();
            CreateListContainer();

            return true;
        }

        /// <summary>
        /// Crée en entete les ddl et recherche permettant de faire des filtres sur la liste
        /// </summary>
        protected virtual void CreateHeader()
        {

            Panel panel = new Panel();
            panel.ID = "headerFilters";
            PgContainer.Controls.Add(panel);

            HtmlInputHidden inputDescid = new HtmlInputHidden()
            {
                ID = "inptDescid",
                Value = DescId.ToString()
            };
            panel.Controls.Add(inputDescid);

            HtmlInputHidden inptResid = new HtmlInputHidden()
            {
                ID = "inptResid",
                Value = ResId.ToString()
            };
            panel.Controls.Add(inptResid);

            HtmlInputHidden inptFileid = new HtmlInputHidden()
            {
                ID = "inptFileid",
                Value = FileId.ToString()
            };
            panel.Controls.Add(inptFileid);

            #region  Filtre sur Nature
            Panel field = new Panel();
            panel.Controls.Add(field);
            field.CssClass = "field";
            field.ID = "fltNature";

            HtmlGenericControl label = new HtmlGenericControl("label");
            label.InnerText = String.Concat(eResApp.GetRes(Pref, 7727), " :");
            field.Controls.Add(label);

            DropDownList ddl = new DropDownList();
            ddl.ID = "ddlNature";
            ddl.Attributes.Add("onchange", "nsAdminTranslations.refreshList(this);");
            field.Controls.Add(ddl);

            if (Nature != eAdminTranslation.NATURE.FileValue)
            {
                foreach (eAdminTranslation.NATURE nature in Enum.GetValues(typeof(eAdminTranslation.NATURE)))
                {
                    ListItem li = new ListItem(eAdminTranslation.GetNatureLabel(Pref, nature), ((int)nature).ToString());
                    ddl.Items.Add(li);
                    li.Selected = Nature == nature;
                }
            }
            else
            {
                // S'il s'agit de la liste des valeurs de la table, on n'affiche pas d'autres natures
                ListItem li = new ListItem(eAdminTranslation.GetNatureLabel(Pref, Nature), ((int)Nature).ToString());
                ddl.Items.Add(li);
                li.Selected = true;

                li = new ListItem(eAdminTranslation.GetNatureLabel(Pref, eAdminTranslation.NATURE.WidgetParam), ((int)eAdminTranslation.NATURE.WidgetParam).ToString());
                ddl.Items.Add(li);
            }

            #endregion

            #region filtre sur la langue
            field = new Panel();
            panel.Controls.Add(field);
            field.CssClass = "field";
            field.ID = "fltLang";

            label = new HtmlGenericControl("label");
            field.Controls.Add(label);
            label.InnerText = String.Concat(eResApp.GetRes(Pref, 6746), " :");

            ddl = new DropDownList();
            ddl.ID = "ddlLang";
            ddl.Attributes.Add("onchange", "nsAdminTranslations.refreshList(this);");
            field.Controls.Add(ddl);

            ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 435), "-1"));

            foreach (KeyValuePair<String, String> kvp in translationsList.ActiveLangs)
            {
                ListItem li = new ListItem(translationsList.GetLangLabel(int.Parse(kvp.Key)), kvp.Key);
                ddl.Items.Add(li);
                li.Selected = kvp.Key.Equals(LangId.ToString());
            }
            #endregion

            #region recherche

            panel.Controls.Add(eAdminTools.CreateSearchBar("eFS", "tabTranslations", onBlur: "searchBlur();nsAdminTranslations.refreshList(this);"));

            #endregion

        }

        /// <summary>
        /// Ajoute la liste des traductions au container principal
        /// </summary>
        protected virtual void CreateListContainer()
        {

            PgContainer.Controls.Add(GetTranslationsListContainer());
        }

        /// <summary>
        /// Crée le container de la liste des traductions
        /// </summary>
        /// <returns></returns>
        protected Panel GetTranslationsListContainer()
        {
            Panel panel = new Panel();
            panel.ID = "TranslationsList";

            panel.Attributes.Add("sorts", JsonConvert.SerializeObject(translationsList.Sorts));

            int lineNum = 1;
            List<String> listHeaders = new List<String> {
                    eResApp.GetRes(Pref, 7728), //Ressource
                    eResApp.GetRes(Pref, 7729), //Traduction
                    eResApp.GetRes(Pref, 1241), //Chemin
                    eResApp.GetRes(Pref, 7727), //Nature
                    eResApp.GetRes(Pref, 7730), //Identifiant
                    eResApp.GetRes(Pref, 4) //Langue
            };

            int[] colsWidth = new int[] { 20, 20, 20, 20, 20, 20 };

            HtmlGenericControl htmlTable = new HtmlGenericControl("table");
            HtmlGenericControl sectionThead = new HtmlGenericControl("thead");
            HtmlGenericControl sectionTbody = new HtmlGenericControl("tbody");
            //sectionTbody.Style.Add("height", "500px");
            HtmlGenericControl tableTr = new HtmlGenericControl("tr");
            HtmlGenericControl tableTh = new HtmlGenericControl("th");
            HtmlGenericControl tableTd = new HtmlGenericControl("td");

            htmlTable.ID = "tabTranslations";
            htmlTable.Attributes.Add("class", "mTab");
            panel.Controls.Add(htmlTable);

            #region HEADER
            htmlTable.Controls.Add(sectionThead);

            sectionThead.Controls.Add(tableTr);
            sectionThead.Attributes.Add("class", "hdBgCol");

            int colNum = 0;
            foreach (String headerCol in listHeaders)
            {
                tableTh = new HtmlGenericControl("th");
                tableTh.Style.Add("width", String.Format("{0}%", colsWidth[colNum]));
                tableTh.InnerText = headerCol;
                tableTr.Controls.Add(tableTh);
                AddSortIconsToTableHeader(tableTh, (eAdminTranslationsList.COLS)colNum);
                colNum++;
            }
            #endregion

            #region BODY

            htmlTable.Controls.Add(sectionTbody);

            //sectionTbody.Style.Add("height", _nTableH + "px");
            foreach (eAdminTranslation translation in translationsList.TranslationsList)
            {
                tableTr = new HtmlGenericControl("tr");
                sectionTbody.Controls.Add(tableTr);
                tableTr.Attributes.Add("class", String.Concat("line", lineNum));
                lineNum = (lineNum == 1) ? 2 : 1;
                colNum = 0;

                // Ressource
                tableTd = new HtmlGenericControl("td");
                tableTd.InnerText = translation.Source;
                tableTr.Controls.Add(tableTd);

                // Traduction
                tableTd = new HtmlGenericControl("td");
                tableTd.InnerText = translation.Translation;
                eAdminTranslationUpdateInfo updInfo = new eAdminTranslationUpdateInfo(translation);
                tableTd.Attributes.Add("upd", JsonConvert.SerializeObject(updInfo));
                tableTr.Controls.Add(tableTd);

                // Chemin
                tableTd = new HtmlGenericControl("td");
                tableTd.InnerText = translation.Path;
                tableTd.Attributes.Add("title", translation.Path);
                tableTr.Controls.Add(tableTd);

                // Nature
                tableTd = new HtmlGenericControl("td");
                tableTd.InnerText = translation.NatureLabel;
                tableTr.Controls.Add(tableTd);

                // Identifiant
                tableTd = new HtmlGenericControl("td");
                tableTd.InnerText = translation.SysID;
                tableTr.Controls.Add(tableTd);

                // Langue
                tableTd = new HtmlGenericControl("td");
                tableTd.InnerText = translation.Lang;
                tableTr.Controls.Add(tableTd);


            }
            #endregion

            return panel;
        }


        /// <summary>
        /// Ajoute les icônes de tri sur les colonnes indiquées du tableau généré
        /// </summary>
        /// <param name="tableHeaderCell">Cellule d'entête sur laquelle rajouter le critère de tri</param>
        /// <param name="col">The col.</param>
        private void AddSortIconsToTableHeader(HtmlGenericControl tableHeaderCell, eAdminTranslationsList.COLS col)
        {
            AddSortIconsToTableHeader(tableHeaderCell, col, eAdminTranslationsList.OrderBy.SORT.ASC);
            AddSortIconsToTableHeader(tableHeaderCell, col, eAdminTranslationsList.OrderBy.SORT.DESC);


        }

        /// <summary>
        /// Ajoute les icônes de tri sur les colonnes indiquées du tableau généré
        /// </summary>
        /// <param name="tableHeaderCell">Cellule d'entête sur laquelle rajouter le critère de tri</param>
        /// <param name="col">The col.</param>
        /// <param name="sort">The sort.</param>
        private void AddSortIconsToTableHeader(HtmlGenericControl tableHeaderCell, eAdminTranslationsList.COLS col, eAdminTranslationsList.OrderBy.SORT sort)
        {
            tableHeaderCell.Controls.Add(new LiteralControl(" "));
            HtmlImage imgIcn = new HtmlImage();
            tableHeaderCell.Controls.Add(imgIcn);
            imgIcn.Src = "ghost.gif";
            imgIcn.Attributes.Add("class", String.Concat("rIco Sort", eLibTools.GetCase(EudoQuery.CaseField.CASE_CAPITALIZE, sort.ToString())));
            imgIcn.Attributes.Add("onclick", String.Format("srt(this, {0}, {1});", (int)col, (int)sort));
            if (translationsList.Sorts[0].Col == col && translationsList.Sorts[0].Sort == sort)
                imgIcn.Attributes["class"] += " active";

        }
    }

    /// <summary>
    /// Rendu de la liste des traductions
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eda.eAdminTranslationsRenderer" />
    public class eAdminTranslationsListRenderer : eAdminTranslationsRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="eAdminTranslationsListRenderer"/> class.
        /// </summary>
        /// <param name="pref"></param>
        public eAdminTranslationsListRenderer(ePref pref) : base(pref) { }

        /// <summary>
        /// Crée en entete les ddl et recherche permettant de faire des filtres sur la liste
        /// </summary>
        protected override void CreateHeader()
        {
            return;
        }

        /// <summary>
        /// Ajoute la liste des traductions au container principal
        /// </summary>
        protected override void CreateListContainer()
        {
            _pgContainer = GetTranslationsListContainer();
        }

    }
}