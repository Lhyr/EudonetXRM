using Com.Eudonet.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.mgr.widget
{
    /// <summary>
    /// Description résumée de eListWidgetManager
    /// </summary>
    public class eListWidgetManager : eEudoManager
    {
        /// <summary>
        /// Actions possibles du manager
        /// </summary>
        public enum ListAction
        {
            /// <summary>
            /// The refresh
            /// </summary>
            Refresh = 0
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            int nAction = _requestTools.GetRequestFormKeyI("action") ?? 0;
            ListAction action = (ListAction)nAction;
            int wid = _requestTools.GetRequestFormKeyI("wid") ?? 0;
            int tab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            //string jsonSort = _requestTools.GetRequestFormKeyS("sort") ?? string.Empty;
            string jsonFilter = _requestTools.GetRequestFormKeyS("filter") ?? string.Empty;
            bool histoActive = _requestTools.GetRequestFormKeyB("histo") ?? true;
            string jsonWidgetContext = _requestTools.GetRequestFormKeyS("c");


            if (action == ListAction.Refresh)
            {
                //eListWidget.SortInfo sortInfo = null;
                //if (!String.IsNullOrEmpty(jsonSort))
                //{
                //    sortInfo = JsonConvert.DeserializeObject<eListWidget.SortInfo>(jsonSort);
                //}

                List<eListWidget.ExpressFilterInfo> filterInfo = null;
                if (!String.IsNullOrEmpty(jsonFilter))
                {
                    try
                    {
                        filterInfo = JsonConvert.DeserializeObject<List<eListWidget.ExpressFilterInfo>>(jsonFilter);
                    }
                    catch (Exception)
                    {

                    }

                }

                eXrmWidgetContext widgetContext = new eXrmWidgetContext(0);
                try
                {
                    if (!String.IsNullOrEmpty(jsonWidgetContext))
                    {
                        widgetContext = JsonConvert.DeserializeObject<eXrmWidgetContext>(HttpUtility.UrlDecode(jsonWidgetContext));
                    }

                }
                catch (Exception)
                {

                }

                eListWidgetRenderer rdr = eListWidgetRenderer.CreateListWidgetRenderer(_pref, wid, tab, filterInfo, histoActive, widgetContext);
                rdr.Generate();
                if (!String.IsNullOrEmpty(rdr.ErrorMsg))
                {
                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 544),
                        rdr.ErrorMsg,
                        (rdr.InnerException != null) ? String.Concat(rdr.InnerException.Message, " - ", rdr.InnerException.StackTrace) : "");
                    LaunchErrorHTML(true);
                }
                else
                {
                    RenderResultHTML(rdr.PgContainer);
                }
            }

        }
    }
}