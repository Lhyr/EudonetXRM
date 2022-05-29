using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminKanbanMgr
    /// </summary>
    public class eAdminKanbanMgr : eAdminManager
    {

        enum Action
        {
            NONE = 0,
            DISPLAYSWIMLANES = 1,
            SAVESWIMLANES = 2,
            REFRESHCARD = 3,
            GETNEWAGGREGATERENDERER = 4
        }
        /// <summary>
        /// Processes the manager.
        /// </summary>
        protected override void ProcessManager()
        {
            int iWidgetId = _requestTools.GetRequestFormKeyI("wid") ?? 0;
            Action action = _requestTools.GetRequestFormEnum<Action>("action");
            if (iWidgetId == 0)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), "Argument WidgetId manquant");
                LaunchError();
            }

            #region Widget context
            eXrmWidgetContext widgetContext = null;
            string context = _requestTools.GetRequestFormKeyS("context") ?? string.Empty;
            if (context.Length > 0)
                widgetContext = JsonConvert.DeserializeObject<eXrmWidgetContext>(context);
            #endregion


            string sSettings;
            eAdminKanbanSettings settings;
            eXrmWidgetParam widgetParam = new eXrmWidgetParam(_pref, iWidgetId);
            eAbstractRenderer rdr;

            switch (action)
            {
                case Action.NONE:
                    break;
                case Action.DISPLAYSWIMLANES:
                    #region Afficher les lignes de couloirs

                    AddHeadAndBody = true;
                    PageRegisters.RegisterFromRoot = true;

                    PageRegisters.AddScript("eTools");
                    PageRegisters.AddScript("eMain");
                    PageRegisters.AddScript("eUpdater");
                    PageRegisters.AddScript("eModalDialog");
                    PageRegisters.RegisterAdminIncludeScript();
                    PageRegisters.RegisterAdminIncludeScript("grid/eAdminWidget");

                    PageRegisters.AddCss("eMain");
                    PageRegisters.AddCss("eButtons");
                    PageRegisters.AddCss("eControl");
                    PageRegisters.AddCss("eAdmin");
                    PageRegisters.AddCss("eAdminMenu");
                    PageRegisters.AddCss("eAdminKanban");

                    BodyCssClass = "bodyWithScroll";
                    OnLoadBody = "nsAdminKanban.PreventColDuplicate();";

                    //récupération du paramétrage existant pour affichage
                    int iTab = widgetParam.GetParamValueInt("tab");
                    if (iTab == 0)
                    {
                        ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), "Aucun onglet n'a été associé au widget.");
                        LaunchError();
                    }

                    #region Swimlanes
                    sSettings = widgetParam.GetParamValue("swimlanes");
                    if (sSettings.Length > 0)
                        settings = JsonConvert.DeserializeObject<eAdminKanbanSettings>(sSettings);
                    else
                        settings = new eAdminKanbanSettings();
                    #endregion


                    rdr = new eAdminKanbanSettingsRenderer(_pref, settings, widgetParam, widgetContext);
                    rdr.Generate();

                    if (rdr.Error.Length > 0)
                    {
                        ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), "Une erreur est survenue lors du chargement de l'interface", devMsg: rdr.Error);
                        LaunchError();
                    }
                    RenderResultHTML<Panel>(rdr.PgContainer);
                    #endregion

                    break;
                case Action.SAVESWIMLANES:
                    #region sauvegarder les lignes de couloir
                    try
                    {
                        eResCode resCode;
                        eResLocation resLoc;
                        string sError;
                        //on s'assure que le format est correct 
                        sSettings = _requestTools.GetRequestFormKeyS("sets");
                        settings = JsonConvert.DeserializeObject<eAdminKanbanSettings>(sSettings);

                        if (settings != null)
                        {
                            if (settings.Aggregates.Count > 0)
                            {
                                eudoDAL eDal = eLibTools.GetEudoDAL(_pref);
                                eDal.OpenDatabase();

                                try
                                {
                                    foreach (eAdminKanbanSettings.Aggregate aggr in settings.Aggregates)
                                    {
                                        foreach (eAdminKanbanSettings.Aggregate.Setting setting in aggr.Settings)
                                        {
                                            resLoc = null;
                                            #region Unité
                                            if (setting.UnitResLoc.Length > 0)
                                            {
                                                resLoc = JsonConvert.DeserializeObject<eResLocation>(setting.UnitResLoc);
                                            }
                                            resCode = new eResCode(_pref, eDal, setting.UnitResCode, setting.Unit, resLoc);
                                            if (setting.UnitResCode == 0)
                                            {
                                                setting.UnitResCode = resCode.CreateNewResCode();
                                            }
                                            else
                                                resCode.Update();

                                            setting.Unit = resCode.GetAlias();
                                            #endregion

                                            resLoc = null;
                                            #region Libellé
                                            if (setting.LabelResLoc.Length > 0)
                                            {
                                                resLoc = JsonConvert.DeserializeObject<eResLocation>(setting.LabelResLoc);
                                            }
                                            resCode = new eResCode(_pref, eDal, setting.LabelResCode, setting.Label, resLoc);
                                            if (setting.LabelResCode == 0)
                                            {
                                                setting.LabelResCode = resCode.CreateNewResCode();
                                            }
                                            else
                                                resCode.Update();

                                            setting.Label = resCode.GetAlias();
                                            #endregion
                                        }

                                    }
                                }
                                finally
                                {
                                    eDal.CloseDatabase();
                                }

                            }
                        }

                        widgetParam.SetParam("swimlanes", JsonConvert.SerializeObject(settings));
                        widgetParam.Save(out sError);

                        if (sError.Length > 0)
                        {
                            ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), "Une erreur est survenue lors de la mise à jour des paramètres de l'interface", devMsg: sError);
                            LaunchError();
                        }

                    }
                    catch (Exception e)
                    {
                        ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), "Une erreur est survenue lors de la mise à jour des paramètres de l'interface", devMsg: String.Format("{0}{1}{2}", e.Message, Environment.NewLine, e.StackTrace));
                        LaunchError();

                    }
                    #endregion
                    break;
                case Action.REFRESHCARD:

                    //eRenderer kbRdr = eKanbanCardRenderer.CreateKanbanCardRenderer(this._pref, iWidgetId, 0);
                    //kbRdr.Generate();
                    //if (kbRdr.ErrorMsg.Length > 0)
                    //{
                    //    ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), "Une erreur est survenue lors du chargement de la carte", devMsg: kbRdr.ErrorMsg);
                    //    LaunchError();
                    //}
                    //RenderResultHTML(kbRdr.PgContainer);

                    break;
                case Action.GETNEWAGGREGATERENDERER:
                    #region Récupère un masque de saisie d'aggrégat
                    string sCatValues = _requestTools.GetRequestFormKeyS("cols");
                    Dictionary<int, string> diCatValues = new Dictionary<int, string>();
                    if (sCatValues?.Length > 0)
                    {
                        try
                        {
                            diCatValues = JsonConvert.DeserializeObject<Dictionary<int, string>>(sCatValues);
                        }
                        catch (Exception)
                        {

                        }
                    }

                    string sOperationFields = _requestTools.GetRequestFormKeyS("OpFields");
                    Dictionary<int, string> diOperationFields = new Dictionary<int, string>();
                    if (sCatValues?.Length > 0)
                    {
                        try
                        {
                            diOperationFields = JsonConvert.DeserializeObject<Dictionary<int, string>>(sOperationFields);
                        }
                        catch (Exception)
                        {

                        }
                    }

                    eResCodeTranslationManager resCodeMgr = new eResCodeTranslationManager(_pref, null,
                        new eResLocation(eModelConst.ResCodeNature.WidgetParam, eResLocation.GetPathFromWidgetContext(widgetContext)));


                    rdr = eAdminKanbanSettingsRenderer.AggregateRenderer.GetAggregateRenderer(_pref, diCatValues, diOperationFields, resCodeMgr);
                    if (rdr.Error.Length > 0)
                    {
                        ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), "Une erreur est survenue lors du chargement d'un nouvel aggrégat", devMsg: rdr.Error);
                        LaunchError();
                    }
                    RenderResultHTML<Panel>(rdr.PgContainer);

                    #endregion
                    break;
                default:
                    break;
            }



        }
    }
}