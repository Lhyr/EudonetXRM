using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Core.Model.eda;

namespace Com.Eudonet.Xrm.eda.Renderer
{
    public class eAdminFieldsListRendererForExport : eAdminFieldsListRenderer
    {
        public eAdminFieldsListRendererForExport(ePref pref, int tab) : base(pref, tab)
        {

        }

        /// <summary>
        /// Pour l'export on retourne True
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            return true;
        }

        /// <summary>
        /// Retourner les libellé des rubrique pour construire le header
        /// </summary>
        /// <returns></returns>
        public List<string> GetListForHeader()
        {
            return listColumns.Select(c => c.Label).ToList();
        }

        /// <summary>
        /// Retourner la liste des rubriques à exporter
        /// </summary>
        /// <returns></returns>
        public List<eAdminFieldInfos> GetFieldsListForBody()
        {
            return _fields;
        }

        /// <summary>
        /// Retourner la liste des rubriques à exporter
        /// </summary>
        /// <returns></returns>
        public eResInternal GetResInternal()
        {
            return _res;
        }

        /// <summary>
        /// Retourner la liste des Déclencheurs
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, List<eAdminTriggerField>> GetDicTriggers()
        {
            return _dicTriggers;
        }
    }
}