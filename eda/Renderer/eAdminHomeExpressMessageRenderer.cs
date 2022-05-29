using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminHomeExpressMessageRenderer : eAdminModuleRenderer
    {
        Panel _pnlContents;
        Panel _pnlSubTitle;
        List<eAdminHomeExpressMessage> _list;
        eudoDAL _dal;

        String _tableID;

        public string SortCol { get; internal set; }
        public int Sort { get; internal set; }


        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminHomeExpressMessageRenderer(ePref pref, int nWidth, int nHeight)
            : base(pref)
        {
            Pref = pref;
            _width = nWidth - 30;
            _height = nHeight - 60; // marge pour tenir compte de l'affichage du titre et de la barre de recherche
            _tableID = "tableHomepages";
        }

        public static eAdminHomeExpressMessageRenderer CreateAdminHomepagesRenderer(ePref pref, int nWidth, int nHeight)
        {
            return new eAdminHomeExpressMessageRenderer(pref, nWidth, nHeight);
        }

        protected override bool Init()
        {
            if (base.Init())
            {
                return eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminHome);
            }
            return false;
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {
            eudoDAL eDal = null;
            try
            {
                eDal = eLibTools.GetEudoDAL(this.Pref);
                eDal.OpenDatabase();
                _list = eAdminHomeExpressMessage.GetHomeExpressMessageList(Pref, eDal);

                BuildSubTitle(String.Concat(_list.Count, " ", _list.Count > 1 ? eResApp.GetRes(Pref, 350) : eResApp.GetRes(Pref, 7815)));

                // ʕ´•ᴥ•`ʔ #64821 : Paramétrage retiré
                //Boolean bHideHomePageFooter = Pref.GetConfigDefault(new HashSet<eLibConst.CONFIG_DEFAULT> { eLibConst.CONFIG_DEFAULT.HIDEHOMEPAGEFOOTER })[eLibConst.CONFIG_DEFAULT.HIDEHOMEPAGEFOOTER] == "1";

                //BuildHideHomePageFooter(bHideHomePageFooter);

                BuildSearchBar();

                InitInnerContainer();

                BuildList();

            }
            finally
            {
                if (eDal != null)
                    eDal.CloseDatabase();
            }

            return true;
        }

        /// <summary>
        /// Construit un sous titre
        /// </summary>
        /// <param name="subTitle"></param>
        private void BuildSubTitle(String subTitle)
        {
            Literal text = new Literal();
            text.Text = subTitle;

            _pnlSubTitle = new Panel();
            _pnlSubTitle.CssClass = "adminCntntTtl adminExpressMessage";
            _pnlSubTitle.Controls.Add(text);
            _pgContainer.Controls.Add(_pnlSubTitle);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hidehomepagefooter"></param>
        private void BuildHideHomePageFooter(bool hidehomepagefooter)
        {
            try
            {
                Panel check = new Panel();
                check.CssClass = "adminCntntCheck adminExpressMessage";
                check.Attributes.Add("data-active", "1");
                // Champ caché pour obtenir l'attribut "dsc"
                Control homepagefooter = eAdminFieldBuilder.BuildField(check, AdminFieldType.ADM_TYPE_HIDDEN, "", eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.HIDEHOMEPAGEFOOTER.GetHashCode());
                homepagefooter.ID = "homepagefooter";

                Dictionary<string, string> dicRB = new Dictionary<string, string>();
                dicRB.Add("0", eResApp.GetRes(Pref, 58));
                dicRB.Add("1", eResApp.GetRes(Pref, 59));


                Dictionary<string, string> dicRBAttributes = new Dictionary<string, string>();
                dicRBAttributes.Add("tabfld", "configdefault");

                eAdminField rb = new eAdminRadioButtonField(_tab, eResApp.GetRes(Pref, 7868),
                    eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.HIDEHOMEPAGEFOOTER.GetHashCode(), "rbHomePagefooter", dicRB, value: (String.IsNullOrEmpty(hidehomepagefooter.ToString()) ? "0" : hidehomepagefooter ? "1" : "0"), onclick: DEFAULT_OPTION_ONCHANGE, customRadioButtonAttributes: dicRBAttributes);
                rb.SetFieldControlID("homepagefooterControl");
                rb.IsLabelBefore = true;
                rb.Generate(check);
                _pgContainer.Controls.Add(check);

            }
            catch (Exception e)
            {

                throw;
            }

        }

        /// <summary>
        /// Construit la barre de recherche (côté client)
        /// </summary>
        private void BuildSearchBar()
        {
            Panel eFS = eAdminTools.CreateSearchBar("eFS", _tableID);

            _pgContainer.Controls.Add(eFS);
        }

        /// <summary>
        /// On instancie le container 
        /// </summary>
        private void InitInnerContainer()
        {
            _pnlContents = new Panel();
            _pnlContents.CssClass = "adminCntnt adminExpressMessage";
            _pnlContents.Style.Add(HtmlTextWriterStyle.Width, String.Concat(_width, "px"));
            _pnlContents.Style.Add(HtmlTextWriterStyle.Height, String.Concat(_height.ToString(), "px"));
            _pgContainer.Controls.Add(_pnlContents);
        }

        /// <summary>
        /// Construit la liste des onglets
        /// </summary>
        private void BuildList()
        {
            List<String> colHeaders = new List<String> { eResApp.GetRes(Pref, 223), eResApp.GetRes(Pref, 7556) };
            List<List<String>> rowCellsLabels = new List<List<String>>();
            List<List<String>> rowCellsLinks = new List<List<String>>();
            List<List<String>> rowCellsTooltips = new List<List<String>>();
            List<List<AttributeCollection>> rowCellsAttributes = new List<List<AttributeCollection>>();
            List<String> rowLinks = new List<String>();
            List<String> rowTooltips = new List<String>();
            List<AttributeCollection> rowAttributes = new List<AttributeCollection>();
            AttributeCollection rowAttr, cellAttr;
            List<AttributeCollection> listCellsAttr;

            foreach (eAdminHomeExpressMessage hp in _list)
            {
                rowCellsLabels.Add(new List<String> { hp.Label, (String.IsNullOrEmpty(hp.DisplayUsers)) ? eResApp.GetRes(Pref, 6869) : hp.DisplayUsers });
                rowCellsLinks.Add(new List<String> { "", String.Concat("top.nsAdminHomepages.openUserCatalog(this,'usrs_", hp.Id.ToString(), "')") });
                rowCellsTooltips.Add(new List<String> { "", "" });

                // Attributs sur les cellules
                cellAttr = new AttributeCollection(new StateBag());
                cellAttr.Add("data-hpId", hp.Id.ToString());
                cellAttr.Add("mult", "1");
                cellAttr.Add("dbv", hp.IDUsers);
                cellAttr.Add("id", String.Concat("usrs_", hp.Id.ToString()));
                cellAttr.Add("did", "0");
                cellAttr.Add("oldval", hp.IDUsers);
                cellAttr.Add("dsc", string.Concat((int)eAdminUpdateProperty.CATEGORY.CONFIG, "|", (int)eLibConst.PREF_CONFIG.EXPRESSMSG));
                listCellsAttr = new List<AttributeCollection>();
                listCellsAttr.Add(new AttributeCollection(new StateBag()));
                listCellsAttr.Add(cellAttr);

                rowCellsAttributes.Add(listCellsAttr);

                // Attributs sur la ligne
                rowAttr = new AttributeCollection(new StateBag());
                rowAttr.Add("data-hpId", hp.Id.ToString());
                rowAttributes.Add(rowAttr);
            }

            HtmlTable table = GetTable(_tableID, colHeaders, rowCellsLabels, rowCellsLinks, rowCellsTooltips, rowCellsAttributes, rowLinks, rowTooltips, rowAttributes, encodeRowCellLabels: false);
            AddIcons(table);

            HtmlGenericControl p = new HtmlGenericControl("p");
            p.InnerText = eResApp.GetRes(_ePref, 1942);
            _pnlContents.Controls.Add(p);

            _pnlContents.Controls.Add(table);
        }

        /// <summary>
        /// Ajout des icônes de suppression sur la table
        /// </summary>
        /// <param name="table"></param>
        private void AddIcons(HtmlTable table)
        {
            HtmlTableRow tr;
            HtmlGenericControl spanIcon;

            for (int i = 1; i < table.Rows.Count; i++)
            {
                tr = table.Rows[i];

                Int32 eMiD = eLibTools.GetNum(tr.Attributes["data-hpId"]);

                Panel panelActions = new Panel();
                panelActions.CssClass = "actionsBar";
                tr.Cells[0].Controls.Add(panelActions);

                spanIcon = new HtmlGenericControl();
                spanIcon.Attributes.Add("class", "icon-edn-pen");
                spanIcon.Attributes.Add("title", eResApp.GetRes(Pref, 85));
                spanIcon.Attributes.Add("onclick", string.Concat("nsAdminHomepages.editExpressMessage(", eMiD, ")"));
                panelActions.Controls.Add(spanIcon);

                spanIcon = new HtmlGenericControl();
                spanIcon.Attributes.Add("class", "icon-delete");
                spanIcon.Attributes.Add("title", eResApp.GetRes(Pref, 19));
                spanIcon.Attributes.Add("onclick", string.Concat("nsAdminHomepages.deleteExpressMessage(event,", eMiD, ")"));
                panelActions.Controls.Add(spanIcon);
            }
        }
    }
}