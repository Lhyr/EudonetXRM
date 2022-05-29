using System;
using System.Collections.Generic;
using System.Linq;
using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;


namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Manager de eAdminTabs
    /// </summary>
    public class eAdminModuleManager : eAdminManager
    {
        /// <summary>
        /// Gestion de la demande de rendu de module d'admin
        /// </summary>
        protected override void ProcessManager()
        {
            //Initialisation
            int nHeight = _requestTools.GetRequestFormKeyI("h") ?? 600;
            int nWidth = _requestTools.GetRequestFormKeyI("w") ?? 800;
            int nPage = _requestTools.GetRequestFormKeyI("p") ?? 1;
            int nRows = _requestTools.GetRequestFormKeyI("r") ?? 0;

            bool bFullRenderer = _requestTools.GetRequestFormKeyS("f") == "1";
            string sInitialTab = _requestTools.GetRequestFormKeyS("it") ?? "description";

            eUserOptionsModules.USROPT_MODULE targetModule = eUserOptionsModules.USROPT_MODULE.UNDEFINED;
            if (_context.Request.Form["module"] != null)
            {
                JSONReturnHTMLContent res = new JSONReturnHTMLContent();
                int nTargetModule = 0;
                int.TryParse(_context.Request.Form["module"].ToString(), out nTargetModule);
                targetModule = (eUserOptionsModules.USROPT_MODULE)nTargetModule;

                eAdminRenderer rdr = null;

                switch (targetModule)
                {
                    // Pour tous les renderers de "section" (page principale ou autres), on fait appel au renderer racine qui affichera les blocs
                    // de liens correspondant à la section en question
                    // Administration
                    case eUserOptionsModules.USROPT_MODULE.ADMIN:
        
                        rdr = eAdminRendererFactory.CreateAdminPortalRenderer(_pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME_V7_HOMEPAGES:
                        rdr = eAdminRendererFactory.CreateAdminHomepagesRenderer(_pref, nWidth, nHeight);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME_XRM_HOMEPAGES:
                        rdr = eAdminRendererFactory.CreateXrmHomePageListRenderer(_pref, nPage, nRows, nWidth, nHeight);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME_XRM_HOMEPAGE:
                        int pageId = _requestTools.GetRequestFormKeyI("pageId") ?? 0;
                        rdr = eAdminRendererFactory.CreateAdminXrmHomePageRenderer(_pref, pageId, nWidth, nHeight);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME_XRM_HOMEPAGE_GRID:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_TAB_GRID:
                        int gridId = _requestTools.GetRequestFormKeyI("gridId") ?? 0;
                        rdr = eAdminRendererFactory.CreateAdminXrmGridRenderer(_pref, gridId, nWidth, nHeight);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_ACCESS:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_ACCESS_USERGROUPS:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_ACCESS_SECURITY:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_ACCESS_PREF:
                        rdr = eAdminRendererFactory.CreateAdminAccessRenderer(_pref, targetModule, bFullRenderer, nPage, nRows, nWidth, nHeight);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_GENERAL:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_ORM:
                        rdr = eAdminParameters.CreateAdminParameters(_pref, nWidth, nHeight, targetModule);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS:

                        #region critères de recherche

                        StoreListTypeRefresh typRefresh = (StoreListTypeRefresh)(_requestTools.GetRequestFormKeyI("typ") ?? 0);

                        // Cas des valeurs TOUS et AUCUNE
                        string cat = _requestTools.GetRequestFormKeyS("fc") ?? string.Empty;
                        if (cat.ToLower().Trim().Equals("all"))
                            cat = string.Empty;
                        else if (cat.ToLower().Trim().Equals("none"))
                            cat = "-1";

                        StoreListCriteres criteres = new StoreListCriteres();
                        criteres.Search = _requestTools.GetRequestFormKeyS("fs") ?? string.Empty;
                        criteres.Category = cat;
                        List<int> offres = (_requestTools.GetRequestFormKeyS("fo") ?? string.Empty).ConvertToListInt(";");
                        criteres.Offres = offres.Select(it => (ExtEnum.DATA_PRODUCT_OFFER)it);
                        criteres.DisplayType = (StoreListDisplayType)(_requestTools.GetRequestFormKeyI("fd") ?? 0);
                        List<int> otherFilters = (_requestTools.GetRequestFormKeyS("fof") ?? string.Empty).ConvertToListInt(";");
                        criteres.OtherFilters = otherFilters.Select(it => (StoreListOtherFilter)it);

                        #endregion

                        if (eAdminExtension.IsNewStore)
                        {
                            res = new ModuleJSONReturnHTMLContent();
                            rdr = eAdminRendererFactory.CreateAdminStoreListRenderer(_pref, nPage, nRows, typRefresh, criteres);

                            ModuleJSONReturnHTMLContent resModule = res as ModuleJSONReturnHTMLContent;
                            eAdminStoreListRenderer rdrModule = rdr as eAdminStoreListRenderer;

                            if (resModule != null && rdrModule != null)
                            {
                                resModule.iCountModule = rdrModule.TotalExtensionsCount;
                                resModule.iPagesModules = rdrModule.TotalExtensionsPages;
                            }
                        }
                        else
                            rdr = eAdminRendererFactory.CreateAdminExtensionListRenderer(_pref, nPage, nRows, criteres.Search, criteres.Category, 0);

                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_MOBILE:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_LINKEDIN:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_OFFICE365:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SMS:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_CTI:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_API:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_VCARD:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SNAPSHOT:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_GRID:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_FROMSTORE:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_CARTO:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_CEGID:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SAGE:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_EBP:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SIGMA:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_IN_UBIFLOW:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_IN_HBS:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_DOCUSIGN:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ZAPIER:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SMS_NETMESSAGE:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EXTRANET:
                    //SHA : tâche #1 873
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ADVANCED_FORM:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_DEDICATED_IP:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_MAIL_STATUS_VERIFICATION:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_WORLDLINE_PAYMENT:
                        int nExtensionFileId = _requestTools.GetRequestFormKeyI("extid") ?? 0;

                        if (eAdminExtension.IsNewStore)
                        {
                            bool bNoInternet = eLibTools.GetServerConfig("ServerWithoutInternet", "0") == "1";
                            eAdminExtension ext = bNoInternet ? eAdminExtension.initExtensionFromJson(targetModule, _pref) :
                                eAdminExtension.InitFromModule(targetModule, _pref, nExtensionFileId);

                            rdr = eAdminRendererFactory.CreateAdminStoreFileRenderer(_pref, ext);
                        }
                        else
                            rdr = eAdminRendererFactory.CreateAdminExtensionFileRenderer(_pref, targetModule, nExtensionFileId, sInitialTab);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_DASHBOARD:
                        int year = _requestTools.GetRequestFormKeyI("year") ?? 0;
                        rdr = eAdminRendererFactory.CreateAdminDashboardRenderer(_pref, nWidth, nHeight, year);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_DASHBOARD_RGPD:
                        rdr = eAdminRendererFactory.CreateAdminDashboardRGPDRenderer(_pref, nWidth, nHeight);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_DASHBOARD_RGPDTREATMENTLOG:
                        rdr = eAdminRendererFactory.CreateAdminDashboardRGPDTreatmentLogRenderer(_pref, nWidth, nHeight);
                        break;
                    // Autres cas : renderer non implémenté
                    case eUserOptionsModules.USROPT_MODULE.UNDEFINED:
                    default:
                        rdr = null;
                        break;
                }

                if (rdr == null)
                {
                    ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), "Renderer de module admin non implémenté");
                    LaunchError();
                }
                else if (rdr.ErrorMsg.Length > 0)
                {
                    ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), rdr.ErrorMsg);
                    LaunchError();
                }
                else
                {
                    res.Success = true;
                    res.Full = bFullRenderer;

                    if (rdr.DicContent.Count > 1)
                    {
                        foreach (var kvp in rdr.DicContent)
                        {
                            res.MultiPartContent.Add(new PartContent()
                            {
                                Name = kvp.Key,
                                ID = kvp.Value.Ctrl.ID,
                                Content = GetResultHTML(kvp.Value.Ctrl),
                                Mode = kvp.Value.Mode,
                                CallBack = kvp.Value.CallBackScript
                            });
                        }
                    }
                    else
                        res.Html = GetResultHTML(rdr.GetContents(), true);

                    res.CallBack = rdr.GetCallBackScript;
                    RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
                }
            }
        }
    }
}