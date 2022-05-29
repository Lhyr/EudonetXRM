using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet permettant d'afficher le menu pour parametrer le widget
    /// </summary>
    public class eChartParamRenderer : eWidgetSpecificParamRenderer
    {

        /// <summary>
        /// TODO Besoin d'un objet metier de la table
        /// </summary>    
        public eChartParamRenderer(ePref pref, bool isVisible, eFile file, eXrmWidgetParam param, eXrmWidgetContext context) : base(pref, isVisible, file, param, true, context)
        {
        }

        /// <summary>
        /// Construit un block du contenu specifique au widget
        /// </summary>
        /// <returns></returns>
        protected override void BuildWidgetContentPart()
        {
            //Récupération des tabs actives
            List<Tuple<string, string>> listTab = new List<Tuple<string, string>>();

            if (_context.GridLocation == eXrmWidgetContext.eGridLocation.Default)
                listTab = eAdminTools.GetListTabs(Pref).Select(x => new Tuple<string, string>(x.Item1.ToString(), x.Item2)).ToList();
            else
                listTab = eSqlDesc.GetBkm(this.Pref, _context.ParentTab).Select(x => new Tuple<string, string>(x.Key.ToString(), x.Value)).ToList();

            //DDL Onglet + Bouton graphique affiché
            _pgContainer.Controls.Add(BuildSelectOptionField(eResApp.GetRes(Pref, 8013), "", _contentParamField, listTab, "", "tab", _widgetParams.GetParamValue("tab")));
            _pgContainer.Controls.Add(BuildBtnField(eResApp.GetRes(Pref, 8012), eResApp.GetRes(Pref, 8001), _file.GetField((int)XrmWidgetField.ContentSource), "oAdminGridMenu.showGraphics(this);"));
        }
    }
}