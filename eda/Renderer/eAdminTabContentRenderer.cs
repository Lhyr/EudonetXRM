using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Contenu de l'onglet
    /// </summary>
    public class eAdminTabContentRenderer : eAdminTabFieldRenderer
    {
        eAbstractTabContentRenderer _tabContentMenu;
        eAdminTableInfos _tabInfos;
        string[] _openedBlocks = null;

        public string[] OpenedBlocks
        {
            get
            {
                return _openedBlocks;
            }

            set
            {
                _openedBlocks = value;
            }
        }

        private eAdminTabContentRenderer(ePref pref, Int32 nTab, string[] openedBlocks = null)
        {
            Pref = pref;
            DescId = nTab;
            OpenedBlocks = openedBlocks;
            _tabInfos = new eAdminTableInfos(pref, nTab);
            _tabContentMenu = eAbstractTabContentRenderer.GetAdminTabContentRenderer(pref, _tabInfos);
        }

        public static eAdminTabContentRenderer CreateAdminTabContentRenderer(ePref pref, Int32 nTab)
        {
            return new eAdminTabContentRenderer(pref, nTab);
        }




        protected override bool Build()
        {
            _pgContainer.ID = "paramTab1";
            _pgContainer.Attributes.Add("class", "paramBlock");
            _pgContainer.Style.Add(HtmlTextWriterStyle.Display, "none");

            bool bGridEnabled;
            if (_tabInfos.EdnType == EudoQuery.EdnType.FILE_PLANNING)
                bGridEnabled = false;
            else
            {
                bGridEnabled = Pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[]
               { eLibConst.CONFIG_DEFAULT.GRIDENABLED })[eLibConst.CONFIG_DEFAULT.GRIDENABLED] == "1";

#if DEBUG
                bGridEnabled = true;
#endif
            }




            // Titre
            HtmlGenericControl title = new HtmlGenericControl("h3");
            title.InnerHtml = eResApp.GetRes(Pref, 6807);

            // Div
            Panel panelContent = new Panel();
            panelContent.CssClass = "paramBlockContent";

            // Colonnes
            _tabContentMenu.RenderColumns(panelContent);

            // Entête
            _tabContentMenu.RenderHeader(panelContent, OpenedBlocks);

            // Rubriques
            _tabContentMenu.RenderField(panelContent);

            //Creation d'onglet et signet web dispo uniquement si extension grille activé
            // + si l'onglet n'est pas de type Planning (#60232)
            if (bGridEnabled)
            {
                // Onglets web
                _tabContentMenu.RenderWebTab(panelContent);
            }

            // Signets web
            _tabContentMenu.RenderWebBkm(panelContent);


            _pgContainer.Controls.Add(title);
            _pgContainer.Controls.Add(panelContent);

            return base.Build();
        }
    }
}