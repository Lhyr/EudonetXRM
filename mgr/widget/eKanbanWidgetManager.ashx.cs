using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.mgr.widget
{
    /// <summary>
    /// Description résumée de eKanbanWidgetManager
    /// </summary>
    public class eKanbanWidgetManager : eEudoManager
    {
        /// <summary>
        /// Action sur le Kanabn
        /// </summary>
        public enum KanbanAction
        {
            /// <summary>
            /// Rafraîchissement de tout le Kanban
            /// </summary>
            RefreshAll = 0,
            /// <summary>
            /// Rafraîchissement des agrégats
            /// </summary>
            RefreshAggregates = 1
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            int nAction = _requestTools.GetRequestFormKeyI("action") ?? 0;
            KanbanAction action = (KanbanAction)nAction;
            int wid = _requestTools.GetRequestFormKeyI("wid") ?? 0;
            int slDid = _requestTools.GetRequestFormKeyI("sldid") ?? 0;
            bool slIsGroup = _requestTools.GetRequestFormKeyB("slisgroup") ?? false;
            string context = _requestTools.GetRequestFormKeyS("context");
            eXrmWidgetContext widgetContext = new eXrmWidgetContext(0);
            try
            {
                if (!String.IsNullOrEmpty(context))
                {
                    context = HttpUtility.UrlDecode(context);
                    widgetContext = JsonConvert.DeserializeObject<eXrmWidgetContext>(context);
                }

            }
            catch (Exception)
            {

            }


            if (action == KanbanAction.RefreshAll)
            {
                // Si côté JS nous n'avons pas la ligne de couloir sélectionnée, il faut la récupérer des pref
                if (slDid == 0)
                {
                    if (_pref.Context.KanbanSelectedSwimlane.ContainsKey(wid))
                    {
                        eKanbanSwimlane sl = _pref.Context.KanbanSelectedSwimlane[wid];
                        if (sl != null)
                        {
                            slDid = sl.Descid;
                            slIsGroup = sl.Settings.IsGroup;
                        }

                    }
                }
                else if (slDid == -1)
                {
                    // Cas où on désactive la ligne de couloir
                    slDid = 0;
                    slIsGroup = false;
                }

                eKanbanRenderer rdr = eKanbanRenderer.CreateKanbanRenderer(_pref, wid, widgetContext, slDid, slIsGroup);
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
                    // Mise à jour de la pref en session
                    _pref.Context.KanbanSelectedSwimlane[wid] = rdr.SelectedSL;

                    RenderResultHTML(rdr.PgContainer);
                }
            }
            else if (action == KanbanAction.RefreshAggregates)
            {
                int colId = _requestTools.GetRequestFormKeyI("colid") ?? 0;
                int origColId = _requestTools.GetRequestFormKeyI("origcolid") ?? 0;

                eudoDAL eDal = eLibTools.GetEudoDAL(_pref);

                try
                {
                    eDal.OpenDatabase();
                    List<eKanbanColumn> listCol = eKanbanColumn.LoadKanbanColumnsAggregates(_pref, eDal, wid, new List<int>() { colId, origColId });
                    RenderResult(RequestContentType.TEXT, delegate ()
                    {
                        return JsonConvert.SerializeObject(listCol);
                    });
                }
                catch (eEndResponseException)
                {


                }
                catch (System.Threading.ThreadAbortException)
                {

                }
                catch (Exception exc)
                {
                    StringBuilder sbError = new StringBuilder(exc.Message);
                    if (exc.InnerException != null)
                    {
                        sbError.Append(exc.InnerException.Message).Append(Environment.NewLine).Append(exc.InnerException.StackTrace);
                    }

                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 544),
                        "Erreur lors de la récupération des agrégats",
                        sbError.ToString());
                    LaunchError();

                }
                finally
                {
                    eDal.CloseDatabase();
                }

            }

        }
    }
}