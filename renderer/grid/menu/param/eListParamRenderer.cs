using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Rendu des paramètres du widget liste
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eWidgetSpecificParamRenderer" />
    public class eListParamRenderer : eWidgetSpecificParamRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="eListParamRenderer" /> class.
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        /// <param name="file">The file.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="context">The context.</param>
        public eListParamRenderer(ePref pref, bool isVisible, eFile file, eXrmWidgetParam param, eXrmWidgetContext context) : base(pref, isVisible, file, param, true, context)
        {
        }
        /// <summary>
        /// Builds the widget content part.
        /// </summary>
        protected override void BuildWidgetContentPart()
        {
            //Récupération des tabs actives
            List<Tuple<string, string>> listTab;

            listTab = getTabList();

            #region Nombre de ligne
            //Recuperation du choix du nombre de ligne qu'on converti en liste de tuple
            string sNombreDeLigne = eResApp.GetRes(Pref, 8015);
            List<Tuple<string, string>> lstNbLigne = sNombreDeLigne.Split(',').Select(x => new Tuple<string, string>(x, x)).ToList();
            Control selectControl = BuildSelectOptionField(eResApp.GetRes(Pref, 6373), "", _contentParamField, lstNbLigne, "", "nbrows", _widgetParams.GetParamValue("nbrows"));
            //On surcharge le onchange pour les DDL pour le widget list
            ((HtmlGenericControl)selectControl.Controls[1]).Attributes["onchange"] = "oAdminGridMenu.updateParam(this);";
            _pgContainer.Controls.Add(selectControl);
            #endregion

            // Selection onglet + Rubriques affichés
            listTab.Insert(0, new Tuple<string, string>("-1", string.Concat("<", eResApp.GetRes(Pref, 1300), ">")));

            _pgContainer.Controls.Add(BuildSelectOptionField(eResApp.GetRes(Pref, 6231), "", _file.GetField((int)XrmWidgetField.ContentSource), listTab, "", disableNoValue: true));
            _pgContainer.Controls.Add(BuildBtnField(eResApp.GetRes(Pref, 8017), "", _contentParamField, "oAdminGridMenu.showSelectCols(this);", "listcol", _widgetParams.GetParamValue("listcol")));

            // Filtre associé
            _pgContainer.Controls.Add(BuildBtnField(eResApp.GetRes(Pref, 8016) + ((_widgetParams.GetParamValueInt("filterid") > 0) ? "(1)" : ""),
                "", _contentParamField, "oAdminGridMenu.showSelectFilter(this);", "filterid", _widgetParams.GetParamValue("filterid")));
        }

        /// <summary>
        /// Retourne la liste tes tables disponibles pour selection
        /// </summary>
        /// <returns>Liste de tuple descid libellé</returns>
        private List<Tuple<string, string>> getTabList()
        {
            string error = String.Empty;
            Dictionary<Int32, String> _deletedOrVirtualTabList = new Dictionary<int, string>();
            Dictionary<Int32, String> _tabList;
            List<Tuple<string, string>> _tabListReturn;
            string sortCol = "LABEL";
            int sort = 0;
            eudoDAL _dal = null;

            try
            {
                _dal = eLibTools.GetEudoDAL(Pref);
                _dal.OpenDatabase();
                List<EdnType> fileTypes = new List<EdnType>()
                {
                    EdnType.FILE_ADR,
                    EdnType.FILE_MAIL,
                    EdnType.FILE_MAIN,
                    EdnType.FILE_DISCUSSION,
                    EdnType.FILE_PLANNING,
                    EdnType.FILE_RELATION,
                    EdnType.FILE_SMS,
                    EdnType.FILE_STANDARD,
                    EdnType.FILE_TARGET,
                    EdnType.FILE_HISTO, /* #51211 */
                    EdnType.FILE_PJ, /*#51 628, #50 700*/
                    EdnType.FILE_GRID
                };
                _tabList = eDataTools.GetFiles(_dal, Pref, fileTypes, out error, ref _deletedOrVirtualTabList, sortCol, sort, true);

                //liste des tables non administrable
                IEnumerable<int> ieNoAdmin =
                        eLibTools.GetDescAdvInfo(Pref, new List<int>(_tabList.Keys), new List<DESCADV_PARAMETER>() { DESCADV_PARAMETER.NOAMDMIN })
                        .Where(myKvpWhere => myKvpWhere.Value.Find(descAdvInfo => descAdvInfo.Item1 == DESCADV_PARAMETER.NOAMDMIN && descAdvInfo.Item2 == "1") != null)
                        .Select(myKvpSelect => myKvpSelect.Key);

                foreach (int k in ieNoAdmin)
                    _tabList.Remove(k);

                if (_deletedOrVirtualTabList == null)
                    _deletedOrVirtualTabList = new Dictionary<int, string>();


                // On ajoute les tables orphelines et virtuelles (déclarées dans DESC mais inexistantes en base) dans le tableau pour affichage
                // spécifique, invitant l'administrateur à faire le ménage. On fusionne les 2 tableaux pour que l'affichage se fasse
                // au même niveau avec possibilités de tri ou recherche
                foreach (KeyValuePair<Int32, String> orphanedTab in _deletedOrVirtualTabList)
                    _tabList.Add(orphanedTab.Key, orphanedTab.Value);
                


                if (_context.GridLocation == eXrmWidgetContext.eGridLocation.Bkm)
                {
                    Dictionary<int, string> dicBkm = Internal.eda.eSqlDesc.GetBkm(this.Pref, _context.ParentTab);
                    _tabList = _tabList.Where(t => dicBkm.ContainsKey(t.Key)).OrderBy(b => b.Value).ToDictionary(t => t.Key, t => t.Value);
                }


                //SPH - Pour l'instant (22/02/2017), il n'est pas possible d'ajouter le descadv noadmin de ces tab, le script de création n'étant pas finalisé
                _tabList.Remove((int)TableType.XRMGRID);
                _tabList.Remove((int)TableType.XRMWIDGET);

                _tabListReturn = _tabList.Select(x => new Tuple<string, string>(x.Key.ToString(), x.Value)).ToList();



            }
            finally
            {
                _dal.CloseDatabase();
            }

            return _tabListReturn;
        }
    }
}