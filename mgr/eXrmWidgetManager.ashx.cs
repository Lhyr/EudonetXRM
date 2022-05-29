using Com.Eudonet.Internal;
using EudoQuery;
using System.Collections.Generic;
using System.Diagnostics;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet permettant de créer ou mettre à jour un widget
    /// </summary>
    public class eXrmWidgetManager : eEudoManagerReadOnly
    {
        /// <summary>
        /// Creation mise à jour des widgets
        /// </summary>
        protected override void ProcessManager()
        {

            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                eRequestTools tools = new eRequestTools(_context);

                int gridId = tools.GetRequestFormKeyI("gridid") ?? 0; // l'id de la grille
                int widgetId = tools.GetRequestFormKeyI("widgetid") ?? 0; // l'id du widget
                int sid = tools.GetRequestFormKeyI("sid") ?? 0; // ID Specif (dans le cas d'une specif Eudo)
                int tab = (int)TableType.XRMWIDGET; // descid de la table du widget

                #region Contexte
                // Cas des signets grilles : récupération du contexte
                int parentTab = tools.GetRequestFormKeyI("parenttab") ?? 0;
                int parentFid = tools.GetRequestFormKeyI("parentfid") ?? 0;

                eXrmWidgetContext widgetContext = new eXrmWidgetContext(gridId, parentTab, parentFid);

                #endregion

                eXrmEngineProxy engineProxy = new eXrmEngineProxy(_pref, tab, gridId, widgetId, widgetContext);
                widgetContext.GridId = gridId;
                widgetContext.WidgetId = widgetId;

                if (gridId <= 0)
                {
                    engineProxy.SetErrorMsg("Page d'accueil introuvable", "Id de la page d'accueil non fourni");
                    RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(engineProxy.Result); });
                    return;
                }

                engineProxy.Result.Action = tools.GetRequestFormKeyI("action") ?? -1;
                switch (engineProxy.Result.Action)
                {
                    case (int)XrmWidgetAction.CREATE_WIDGET:
                        engineProxy.NewWidget(tools);
                        engineProxy.LinkWidgetPage();
                        engineProxy.AddTranslationsToWidget();
                        engineProxy.ReloadWidget();
                        engineProxy.Result.ClientAction = (int)XrmClientWidgetAction.NEW_WIDGET;
                        break;

                    case (int)XrmWidgetAction.UPDATE_WIDGET:

                        if (_pref.AdminMode)
                            engineProxy.ResetAllWidgetPref();

                        engineProxy.UpdateWidget(tools);
                        break;

                    case (int)XrmWidgetAction.DELETE_WIDGET:
                        engineProxy.UnlinkWidgetPage();
                        engineProxy.DeleteWidget();

                        //Si ok on supprime la specif éventuelle

                        engineProxy.DeleteAssociedSpecif(sid);


                        #region Suppression des traductions associées
                        if (!eSqlResFiles.DeleteFileResList(_pref, tab, widgetId, out _sMsgError))
                        {
                            engineProxy.Result.Success = false;
                            engineProxy.Result.ErrorMsg = _sMsgError;
                        }
                        if (!eResCodeTranslationManager.DeleteWidgetTranslations(_pref, null, widgetId, out _sMsgError))
                        {
                            engineProxy.Result.Success = false;
                            engineProxy.Result.ErrorMsg = _sMsgError;
                        }
                        #endregion
                        break;

                    case (int)XrmWidgetAction.LINK_WIDGET:
                        engineProxy.LinkWidgetPage();
                        break;

                    case (int)XrmWidgetAction.UNLINK_WIDGET:
                        engineProxy.UnlinkWidgetPage();
                        break;

                    case (int)XrmWidgetAction.SAVE_WIDGET_PREF:
                        if (_pref.AdminMode)
                        {
                            engineProxy.ResetAllWidgetPref();
                            engineProxy.UpdateWidget(tools);
                        }
                        else
                            engineProxy.SaveWidgetPref(tools);
                        break;
                    case (int)XrmWidgetAction.SAVE_VISIBLE_PREF:
                        engineProxy.SaveVisiblePref(tools);
                        break;

                    case (int)XrmWidgetAction.DELETE_WIDGET_PREF:
                        engineProxy.DeleteWidgetPref();
                        break;
                    case (int)XrmWidgetAction.REFRESH_WIDGET:
                        engineProxy.ReloadWidget();
                        break;

                    case (int)XrmWidgetAction.SAVE_WIDGET_PARAM:
                        string param = tools.GetRequestFormKeyS("paramname") ?? "";
                        string value = tools.GetRequestFormKeyS("paramvalue") ?? "";
                        engineProxy.SaveWidgetParam(tools, param, value);
                        break;
                    case (int)XrmWidgetAction.DELETE_WIDGET_PARAM:
                        // Non utilisé pour l'instant
                        List<string> paramsList = SerializerTools.JsonDeserialize<List<string>>(tools.GetRequestFormKeyS("paramslist"));
                        engineProxy.DeleteWidgetParam(paramsList);
                        break;
                    case (int)XrmWidgetAction.UNKNOWN_ACTION:
                    default:
                        engineProxy.SetErrorMsg("Demande incorrecte", "Pas d'action définie pour ce cas");
                        break;
                }

                watch.Stop();
                RenderResult(RequestContentType.SCRIPT, delegate ()
                {
                    eRequestReportManager.CallInfo cf = new eRequestReportManager.CallInfo();
                    cf.ElapsedTime = (int)watch.ElapsedMilliseconds;
                    cf.BaseName = _pref.GetBaseName;
                    cf.User = _pref.User.UserLogin;
                    cf.Action = ((XrmWidgetAction)engineProxy.Result.Action).ToString();
                    cf.Grid = engineProxy.Result.GridId;
                    cf.Widget = engineProxy.Result.WidgetId;
                    eRequestReportManager.Instance.Add(cf);

                    return SerializerTools.JsonSerialize(engineProxy.Result);
                });
            }
            finally
            {
                watch = null;
            }
        }
    }
}