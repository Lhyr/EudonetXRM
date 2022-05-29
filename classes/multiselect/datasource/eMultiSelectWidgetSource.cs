using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet permetant d'implémenter la source de donnée pour le multi-select des widgets
    /// </summary>
    public class eMultiSelectWidgetSource : eMultiSelectDataSourceInterface
    {
        private ePref _pref;
        private int _gridId;

        private List<eMultiSelectItem> _items;

        /// <summary>
        /// Contructeur de multi-select de widgets
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="param">contient l'id de la grille, l'id la paged'accueil</param>
        /// <param name="values"></param>
        public eMultiSelectWidgetSource(ePref pref, eMultiSelectParam param)
        {
            _pref = pref;
            _gridId = param.GetIntValue("gridId");
            if (_gridId <= 0)
                throw new eMultiSelectException(eResApp.GetRes(_pref, 8172));

            _items = new List<eMultiSelectItem>();
        }

        /// <summary>
        /// Lance le process de récupèration des widgets
        /// </summary>
        /// <returns></returns>
        public bool Generate()
        {
            eList list = eListFactory.GetWidgetList(_pref, _gridId);
            eXrmWidgetPrefCollection WidgetPrefCollection = new eXrmWidgetPrefCollection(_pref, _gridId);
            eXrmWidgetPref widgetPref; bool alwaysVisible;
            foreach (eRecord record in list.ListRecords)
            {
                widgetPref = WidgetPrefCollection[record];
                alwaysVisible = record.GetFieldByAlias(eXrmWidgetTools.GetAlias(XrmWidgetField.DisplayOption)).Value == ((int)WidgetDisplayOption.ALWAYS_VISIBLE).ToString();

                // TODO - A voir si on affiche l'item dans la fenetre désactivé non sélectionnable
                if (alwaysVisible)
                    continue;

                _items.Add(new eMultiSelectItem() {
                    Id = record.MainFileid,
                    Title = record.GetFieldByAlias(eXrmWidgetTools.GetAlias(XrmWidgetField.Title)).Value,
                    Tooltip = record.GetFieldByAlias(eXrmWidgetTools.GetAlias(XrmWidgetField.Tooltip)).Value,
                    // si toujours affiché, on la sélectionne
                    Selected = widgetPref.Visible || alwaysVisible,
                    // si toujours affiché, on désactive le deplacement de l'élement
                    Disabled = alwaysVisible 
                });
            }           

            return true;
        }

        /// <summary>
        /// Titre de l'entete de la fenêtre
        /// </summary>
        /// <returns></returns>
        public string GetHeaderTitle()
        {
            return eResApp.GetRes(_pref, 8169);// "Sélectionner et glissez les widgets à afficher dans la grille :";
        }

        /// <summary>
        /// Titre des widgets disponibles
        /// </summary>
        /// <returns></returns>
        public string GetSourceItemsTitle()
        {
            return eResApp.GetRes(_pref, 8170); //"Widgets masqués";
        }

        /// <summary>
        /// Titre des widgets affiché
        /// </summary>
        /// <returns></returns>
        public string GetTargetItemsTitle()
        {
            return eResApp.GetRes(_pref, 8171); //"Widgets affichés";
        }

        /// <summary>
        /// liste des widgets affichés
        /// </summary>
        /// <returns></returns>
        public IEnumerable<eMultiSelectItem> GetTargetItems()
        {
            return _items.Where(e => e.Selected);
        }

        /// <summary>
        /// Liste des widgets disponibles
        /// </summary>
        /// <returns></returns>
        public IEnumerable<eMultiSelectItem> GetSourceItems()
        {
            return _items.Where(e => !e.Selected);
        }
    }
}