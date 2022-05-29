using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminHomepagesRenderer : eAdminModuleRenderer
    {
        Panel _pnlContents;
        Panel _pnlSubTitle;
        List<eAdminHomepage> _list;
        eudoDAL _dal;

        String _tableID;

        public string SortCol { get; internal set; }
        public int Sort { get; internal set; }

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminHomepagesRenderer(ePref pref, int nWidth, int nHeight)
            : base(pref)
        {
            Pref = pref;
            _width = nWidth - 30;
            _height = nHeight - 60; // marge pour tenir compte de l'affichage du titre et de la barre de recherche
            _tableID = "tableHomepages";
        }

        public static eAdminHomepagesRenderer CreateAdminHomepagesRenderer(ePref pref, int nWidth, int nHeight)
        {
            return new eAdminHomepagesRenderer(pref, nWidth, nHeight);
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
                _list = eAdminHomepage.GetHomepagesList(this.Pref, eDal);

                BuildSubTitle(String.Concat(_list.Count, " ", eResApp.GetRes(Pref, 348)));

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
            _pnlSubTitle.CssClass = "adminCntntTtl adminCntntTtlHomepages";
            _pnlSubTitle.Controls.Add(text);
            _pgContainer.Controls.Add(_pnlSubTitle);
        }

        /// <summary>
        /// Construit la barre de recherche (côté client)
        /// </summary>
        private void BuildSearchBar()
        {
            Panel eFS = eAdminTools.CreateSearchBar("eFSContainerAdminHomepages", _tableID);

            _pgContainer.Controls.Add(eFS);
        }

        /// <summary>
        /// On instancie le container 
        /// </summary>
        private void InitInnerContainer()
        {
            _pnlContents = new Panel();
            _pnlContents.CssClass = "adminCntnt adminCntntHomepages";
            _pnlContents.Style.Add(HtmlTextWriterStyle.Width, String.Concat(_width, "px"));
            _pnlContents.Style.Add(HtmlTextWriterStyle.Height, String.Concat(_height.ToString(), "px"));
            _pgContainer.Controls.Add(_pnlContents);
        }

        /// <summary>
        /// Construit la liste des onglets
        /// </summary>
        private void BuildList()
        {
            List<String> colHeaders = new List<String> { eResApp.GetRes(Pref, 223), eResApp.GetRes(Pref, 7557), eResApp.GetRes(Pref, 7556) };
            List<List<String>> rowCellsLabels = new List<List<String>>();
            List<List<String>> rowCellsLinks = new List<List<String>>();
            List<List<String>> rowCellsTooltips = new List<List<String>>();
            List<List<AttributeCollection>> rowCellsAttributes = new List<List<AttributeCollection>>();
            List<String> rowLinks = new List<String>();
            List<String> rowTooltips = new List<String>();
            List<AttributeCollection> rowAttributes = new List<AttributeCollection>();
            AttributeCollection rowAttr, cellAttr;
            List<AttributeCollection> listCellsAttr;

            foreach (eAdminHomepage hp in _list)
            {
                rowCellsLabels.Add(new List<String> { hp.Label, hp.Tooltip, (String.IsNullOrEmpty(hp.DisplayUsers)) ? eResApp.GetRes(Pref, 6869) : hp.DisplayUsers });
                rowCellsLinks.Add(new List<String> { "", "", "" });
                rowCellsTooltips.Add(new List<String> { "", "", (String.IsNullOrEmpty(hp.DisplayUsers)) ? eResApp.GetRes(Pref, 6869) : hp.DisplayUsers });

                // Attributs sur les cellules
                cellAttr = new AttributeCollection(new StateBag());
                cellAttr.Add("mult", "1");
                cellAttr.Add("dbv", hp.IDUsers);
                cellAttr.Add("class", "cell hpUsers");
                cellAttr.Add("data-hpId", hp.Id.ToString());
                listCellsAttr = new List<AttributeCollection>();
                listCellsAttr.Add(new AttributeCollection(new StateBag()));
                listCellsAttr.Add(new AttributeCollection(new StateBag()));
                listCellsAttr.Add(cellAttr);

                rowCellsAttributes.Add(listCellsAttr);

                // Attributs sur la ligne
                rowAttr = new AttributeCollection(new StateBag());
                rowAttr.Add("data-hpId", hp.Id.ToString());
                rowAttributes.Add(rowAttr);
            }

            HtmlTable table = GetTable(_tableID, colHeaders, rowCellsLabels, rowCellsLinks, rowCellsTooltips, rowCellsAttributes, rowLinks, rowTooltips, rowAttributes, encodeRowCellLabels: false);
            AddIconsDelete(table);

            _pnlContents.Controls.Add(table);
        }

        /// <summary>
        /// Ajout des icônes de suppression sur la table
        /// </summary>
        /// <param name="table"></param>
        private void AddIconsDelete(HtmlTable table)
        {
            HtmlTableRow tr;
            HtmlGenericControl spanIcon;

            for (int i = 1; i < table.Rows.Count; i++)
            {
                tr = table.Rows[i];

                Int32 hpID = eLibTools.GetNum(tr.Attributes["data-hpId"]);

                // Actions

                Panel panelActions = new Panel();
                panelActions.CssClass = "actionsBar";
                tr.Cells[0].Controls.Add(panelActions);

                spanIcon = new HtmlGenericControl();
                spanIcon.Attributes.Add("class", "icon-delete");
                spanIcon.Attributes.Add("title", eResApp.GetRes(Pref, 19));
                spanIcon.Attributes.Add("onclick", "nsAdminHomepages.deleteHomepageV7(" + hpID + ")");
                panelActions.Controls.Add(spanIcon);

                /*
                spanIcon = new HtmlGenericControl();
                spanIcon.Attributes.Add("class", "icon-duplicate");
                spanIcon.Attributes.Add("title", eResApp.GetRes(Pref, 534));
                spanIcon.Attributes.Add("onclick", "nsAdminHomepages.cloneHomepage(" + hpID + ")");
                panelActions.Controls.Add(spanIcon);
                */
            }
        }
    }
}