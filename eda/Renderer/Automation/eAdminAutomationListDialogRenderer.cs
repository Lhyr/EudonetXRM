using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Render qui permet de construire le contenu de la modal de la liste des automatismes
    /// </summary>
    public class eAdminAutomationListDialogRenderer : eRenderer
    {
        // Descid de la table => libellé de la table
        Dictionary<int, String> tabs;
        IEnumerable<eFieldLiteAdmin> fields;

        AutomationType _autoType = AutomationType.ALL;
        int _targetTab = 0;
        int _targetField = 0;
        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="nField"></param>
        private eAdminAutomationListDialogRenderer(ePref pref, Int32 nTab, Int32 nField, Int32 width, Int32 height, Int32 nPage, AutomationType automationType) : base()
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();


            Pref = pref;
            _targetTab = nTab;
            _targetField = nField;
            _tab = (int)TableType.NOTIFICATION_TRIGGER;
            _width = width;
            _height = height;
            _rType = RENDERERTYPE.AutomationList;
            _autoType = automationType;
        }

        /// <summary>
        /// Initilisation du renderer : recup des tables et les rubriques de la table actuelles
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            tabs = eSqlDesc.LoadTabs(Pref, new int[] { (int)TableType.DOUBLONS });

            fields = eAdminAutomationTools.GetTabFields(Pref, _targetTab);
            if (fields == null)
            {
                _sErrorMsg = String.Concat("Aucunes rubriques pour la table ", _targetTab, " trouvées");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Lance la construction du contenu de la fenetre des automatismes
        /// ajout des filtres sur les tables et les rubriques
        /// ajout un bouton Add et la liste standrad
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            try
            {
                // Construit deux select pour filtrer les tables et les rubriques
                _pgContainer.Controls.Add(CreateHeaderFilters());
                _pgContainer.Controls.Add(CreatAddAutomationBtn());
                _pgContainer.Controls.Add(CreateAutomationList());
            }
            catch (Exception ex)
            {
                _eException = ex;
                _sErrorMsg = "Impossible de construire la liste des automatismes !";

                return false;
            }

            return true;
        }

        /// <summary>Génération des filtres sur les tables et les rubriques</summary>
        private Panel CreateHeaderFilters()
        {
            Panel container = new Panel();
            container.ID = "Header";
            container.CssClass = "header-admin";

            Panel panel = new Panel();
            container.Controls.Add(panel);
                        
            panel.ID = "header-filters";

            #region Filtre sur les tables


            Panel field = new Panel();
            field.CssClass = "admin-field-filter";
            HtmlGenericControl label = new HtmlGenericControl("label");
            label.InnerText = String.Concat(eResApp.GetRes(Pref, 264), " : ");

            HtmlGenericControl ddl = new HtmlGenericControl("select");

            ddl.ID = "ddlListTabs";
            ddl.Attributes.Add("onchange", "nsAdminAutomation.onTabChanged(this);");

            Dictionary<int, String> tabs = eSqlDesc.LoadTabs(Pref, new int[] { (int)TableType.DOUBLONS });

            // on retire les table notification
            tabs.Remove(TableType.NOTIFICATION_TRIGGER.GetHashCode());
            tabs.Remove(TableType.NOTIFICATION.GetHashCode());

            

            HtmlGenericControl fieldOption;
            foreach (var kv in tabs)
            {
                fieldOption = new HtmlGenericControl("option");
                if (kv.Key == _targetTab)
                    fieldOption.Attributes.Add("selected", "selected");

                fieldOption.Attributes.Add("value", kv.Key.ToString());
                fieldOption.InnerText = kv.Value;
                ddl.Controls.Add(fieldOption);
            }

            // Ajout de Onglet et select au panel
            field.Controls.Add(label);
            field.Controls.Add(ddl);

            panel.Controls.Add(field);
            #endregion

            #region Filtre sur les rubriques
            field = new Panel();
            field.CssClass = "admin-field-filter";
            label = new HtmlGenericControl("label");
            label.InnerText = String.Concat(eResApp.GetRes(Pref, 7781), " : ");

            ddl = new HtmlGenericControl("select");

            ddl.ID = "ddlListFields";
            ddl.Attributes.Add("onchange", "nsAdminAutomation.onFldChanged(this);");

            fieldOption = new HtmlGenericControl("option");
            if (_targetField == 0)
                fieldOption.Attributes.Add("selected", "selected");

            fieldOption.Attributes.Add("value", "0");
            fieldOption.InnerText = eResApp.GetRes(Pref, 22); //6875:Toutes
            ddl.Controls.Add(fieldOption);

            foreach (var fld in fields)
            {
                fieldOption = new HtmlGenericControl("option");
                if (fld.Descid == _targetField)
                    fieldOption.Attributes.Add("selected", "selected");

                fieldOption.Attributes.Add("value", fld.Descid.ToString());
                fieldOption.InnerText = fld.Libelle;
                ddl.Controls.Add(fieldOption);
            }

            // Ajout de rubrique et 
            field.Controls.Add(label);
            field.Controls.Add(ddl);
            panel.Controls.Add(field);
            #endregion

            return container;
        }
        
        /// <summary>
        /// Crée un bouton "Nouvelle noutification"
        /// </summary>
        private Panel CreatAddAutomationBtn()
        {
            Panel containerAdd = new Panel();
            containerAdd.ID = "catDivHeadAdv";
            containerAdd.CssClass = "catDivHeadAdv";

            //ul
            HtmlGenericControl catToolAdd = new HtmlGenericControl("ul");
            containerAdd.Controls.Add(catToolAdd);
            catToolAdd.Attributes.Add("class", "catToolAdd");

            //li
            HtmlGenericControl catToolAddLib = new HtmlGenericControl("li");
            catToolAdd.Controls.Add(catToolAddLib);
            catToolAddLib.ID = "btnAdd buttonAddWidth";

            catToolAddLib.Attributes.Add("onclick", "nsAdminAutomation.create(" + _targetTab + ", " + _targetField + ");");
            catToolAddLib.Attributes.Add("class", "buttonAdd");

            // icon plus
            HtmlGenericControl icnSpan = new HtmlGenericControl("span");
            icnSpan.Attributes.Add("class", "icon-add");
            catToolAddLib.Controls.Add(icnSpan);

            // libellé "Nouvelle notification";
            HtmlGenericControl span = new HtmlGenericControl("span");
            span.InnerHtml = eResApp.GetRes(Pref, 7479); 
            span.Attributes.Add("class", "catToolAddLibSp");
            catToolAddLib.Controls.Add(span);

            return containerAdd;
        }
        
        /// <summary>
        /// Construit la liste standard des notification
        /// </summary>
        /// <returns></returns>
        private Panel CreateAutomationList()
        {
            return eAdminAutomationListRenderer.CreateAdminAutomationListRenderer(Pref, _targetTab, _targetField, _width, _height, 1, _autoType).PgContainer;
        }

        /// <summary>
        /// Creation d'une instance de ce renderer
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="nField"> rubrique sur laquelle l'automatisme est activé</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="nPage"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static eRenderer CreateAdminAutomationListDialogRenderer(ePref pref, Int32 nTab, Int32 nField, Int32 width, Int32 height, Int32 nPage, AutomationType type)
        {
            return new eAdminAutomationListDialogRenderer(pref, nTab, nField, width, height, nPage, type);
        }
    }
}