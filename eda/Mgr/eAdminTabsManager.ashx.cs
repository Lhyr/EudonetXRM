using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Manager de eAdminTabs
    /// </summary>
    public class eAdminTabsManager : eAdminManager
    {
        private enum TABSMANAGER_ACTION
        {
            /// <summary>par défaut</summary>
            UNDEFINED = -1,
            /// <summary>Instanciation du renderer avec les paramètres demandés</summary>
            RENDERER = 0,
            /// <summary>MAJ des paramètres du renderer</summary>
            UPDATE = 1
        }

        /// <summary>
        /// Gestion de la demande de rendu du menu d'admin
        /// </summary>
        protected override void ProcessManager()
        {
            int nWidth = 800;
            int nHeight = 600;
            String sSortCol = String.Empty;
            Int32 iSort = 0;
            String sSearch = String.Empty;
            int nAction = (int)TABSMANAGER_ACTION.RENDERER;

            //Initialisation
            if (_requestTools.AllKeys.Contains("action") && !String.IsNullOrEmpty(_context.Request.Form["action"]))
                if (!Int32.TryParse(_context.Request.Form["action"].ToString(), out nAction))
                    nAction = (int)TABSMANAGER_ACTION.RENDERER;

            if (nAction == (int)TABSMANAGER_ACTION.RENDERER)
            {
                if (_requestTools.AllKeys.Contains("h") && !String.IsNullOrEmpty(_context.Request.Form["h"]))
                    if (!Int32.TryParse(_context.Request.Form["h"].ToString(), out nHeight))
                        nHeight = 600;

                if (_requestTools.AllKeys.Contains("w") && !String.IsNullOrEmpty(_context.Request.Form["w"]))
                    if (!Int32.TryParse(_context.Request.Form["w"].ToString(), out nWidth))
                        nWidth = 800;
            }

            if (_requestTools.AllKeys.Contains("sortcol") && !String.IsNullOrEmpty(_context.Request.Form["sortcol"]))
                sSortCol = _context.Request.Form["sortcol"].ToString();
            if (_requestTools.AllKeys.Contains("sort") && !String.IsNullOrEmpty(_context.Request.Form["sort"]))
                Int32.TryParse(_context.Request.Form["sort"], out iSort);
            if (_requestTools.AllKeys.Contains("search") && !String.IsNullOrEmpty(_context.Request.Form["search"]))
                sSearch = _context.Request.Form["search"].ToString();

            JSONReturnTabs res = new Mgr.JSONReturnTabs();

            switch (nAction)
            {
                case (int)TABSMANAGER_ACTION.RENDERER: 
                    eAdminRenderer rdr = eAdminRendererFactory.CreateAdminTabsRenderer(_pref, nWidth, nHeight, sSortCol, iSort, sSearch);
                    if (rdr.ErrorMsg.Length > 0)
                    {
                        ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), rdr.ErrorMsg);
                        LaunchError();
                    }
                    else
                    {

                        res.Success = true;
                        res.Html = GetResultHTML(rdr.PgContainer);

                        RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
                    }
                    break;
                // #56 749 - TODO si souhaité
                case (int)TABSMANAGER_ACTION.UPDATE:
                    res.Success = true;
                    res.Html = String.Empty;
                    break;
            }
        }
    }


    /// <summary>
    /// Retour json de type admin tab
    /// </summary>
    public class JSONReturnTabs : JSONReturnHTMLContent
    {


    }
}