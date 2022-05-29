using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Rendu du bloc "Liens web et traitements spécifiques"
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eda.eAdminBlockRenderer" />
    public class eAdminWebLinksAndTreatmentsRenderer : eAdminBlockRenderer
    {
        List<eSpecif> _specifs;

        private eAdminWebLinksAndTreatmentsRenderer(ePref pref, int tab, string title) : base(pref, tab, title, "", idBlock: "WebLinksPart")
        {
            this._tab = tab;
        }
        /// <summary>
        /// Création du rendu du bloc "Liens web et traitements spécifiques"
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tab"></param>
        /// <param name="title">Titre du bloc</param>
        /// <returns></returns>
        public static eAdminWebLinksAndTreatmentsRenderer CreateAdminWebLinksAndTreatments(ePref pref, int tab, string title)
        {
            eAdminWebLinksAndTreatmentsRenderer blockRenderer = new eAdminWebLinksAndTreatmentsRenderer(pref, tab, title);
            return blockRenderer;
        }

        /// <summary>
        /// Récupère toutes les specifs de type page web
        /// </summary>
        protected override Boolean Init()
        {
            if (!base.Init())
                return false;

            try
            {
                _tabInfos = Internal.eda.eAdminTableInfos.GetAdminTableInfos(_ePref, _tab);
                _specifs = eSpecif.GetSpecifList(Pref, new List<eLibConst.SPECIF_TYPE>() { eLibConst.SPECIF_TYPE.TYP_FILE, eLibConst.SPECIF_TYPE.TYP_LIST, eLibConst.SPECIF_TYPE.TYP_SPECIF_CALENDAR_MODE }, _tab);
            }
            catch (Exception ex)
            {
                _sErrorMsg = "eWebTab::Init : " + ex.Message;
                _eException = ex;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Construction du squelette du bloc : header + div contenu
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            base.Build();

            //#region Bouton modèle : caché
            //GenerateWebLink("weblink_template");
            //#endregion



            // Bouton ajouter un lien web
            eAdminButtonField btn = new eAdminButtonField(eResApp.GetRes(this.Pref, 7804), "btnAddLink", eResApp.GetRes(this.Pref, 7805), "nsAdmin.addWebLink(this);", iconClass: "icon-plus-circle");
            btn.Generate(_panelContent);

            //Traductions
            eAdminField button = new eAdminButtonField(eResApp.GetRes(Pref, 7716), "", onclick: String.Format("nsAdmin.openTranslations({0}, {1});", this._tab, (int)eAdminTranslation.NATURE.WebLink));
            button.Generate(_panelContent);

            // Fiche
            eAdminFieldBuilder.BuildSeparator(_panelContent, eResApp.GetRes(Pref, 190));
            Panel subPart = new Panel();
            subPart.CssClass = "subPart";
            foreach (eSpecif spec in _specifs.FindAll(s => s.Type == eLibConst.SPECIF_TYPE.TYP_FILE))
            {
                GenerateWebLink(subPart, String.Concat("weblink_", spec.SpecifId), spec.Label, spec.SpecifId);
            }
            _panelContent.Controls.Add(subPart);

            // Liste
            eAdminFieldBuilder.BuildSeparator(_panelContent, eResApp.GetRes(Pref, 179));
            subPart = new Panel();
            subPart.CssClass = "subPart";
            foreach (eSpecif spec in _specifs.FindAll(s => s.Type == eLibConst.SPECIF_TYPE.TYP_LIST))
            {
                GenerateWebLink(subPart, String.Concat("weblink_", spec.SpecifId), spec.Label, spec.SpecifId);
            }
            _panelContent.Controls.Add(subPart);


            if (_tabInfos.EdnType == EudoQuery.EdnType.FILE_PLANNING)
            {
                // Calendrier
                eAdminFieldBuilder.BuildSeparator(_panelContent, eResApp.GetRes(Pref, 135));
                subPart = new Panel();
                subPart.CssClass = "subPart";
                foreach (eSpecif spec in _specifs.FindAll(s => s.Type == eLibConst.SPECIF_TYPE.TYP_SPECIF_CALENDAR_MODE))
                {
                    GenerateWebLink(subPart, String.Concat("weblink_", spec.SpecifId), spec.Label, spec.SpecifId);
                }
                _panelContent.Controls.Add(subPart);
            }




            return true;
        }

        /// <summary>
        /// Génére un lien web (bouton)
        /// </summary>
        /// <param name="panelWrapper">Conteneur</param>
        /// <param name="panelID">ID du conteneur</param>
        /// <param name="label">Libellé</param>
        /// <param name="specifId">ID spécif</param>
        void GenerateWebLink(Panel panelWrapper, String panelID, String label = "", int specifId = 0)
        {
            Panel panel = new Panel();
            panel.ID = panelID;
            panel.CssClass = "field linkButton";

            HtmlGenericControl a = new HtmlGenericControl("a");
            a.Attributes.Add("href", "#");
            a.InnerText = String.IsNullOrEmpty(label) ? eResApp.GetRes(this.Pref, 7807) : label;
            panel.Controls.Add(a);

            // Configuration
            HtmlGenericControl menuConfig = new HtmlGenericControl("ul");
            menuConfig.Attributes.Add("class", "fieldOptions");
            HtmlGenericControl itemConfig = new HtmlGenericControl("li");
            itemConfig.Attributes.Add("class", "icon-cog configOption");
            itemConfig.Attributes.Add("onclick", String.Concat("nsAdmin.editWebLinkProperties(", _tab, ", ", specifId, ");"));
            menuConfig.Controls.Add(itemConfig);
            itemConfig = new HtmlGenericControl("li");
            itemConfig.Attributes.Add("class", "deleteOption icon-delete");
            itemConfig.Attributes.Add("onclick", String.Concat("nsAdmin.deleteWebLink(", _tab, ", ", specifId, ");"));
            menuConfig.Controls.Add(itemConfig);

            panel.Controls.Add(menuConfig);

            panelWrapper.Controls.Add(panel);
        }
    }
}