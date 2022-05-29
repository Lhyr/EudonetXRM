using System;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.mgr
{
    /// <className>eListManager</className>
    /// <summary>Gestionaire des mode listes</summary>
    /// <authors>SPH</authors>
    /// <date>2011-12-21</date>
    public class eListManager : eEudoManager
    {
        private int _tab = 0;

        private int _nFileid = 0;
        int _height;
        int _width;
        string _type;

        bool _bAddPublicItem = false;
        bool _reloadPaging = false;
        bool _bFullList = false;

        bool _deselectAllowed = false;
        bool _adminMode = false;
        bool _selectFilterMode = false;
        FORMULAR_TYPE _formularType = FORMULAR_TYPE.TYP_CLASSIC;
        TypeMailTemplate _mailTemplateType = TypeMailTemplate.MAILTEMPLATE_EMAILING;
        /// <summary>
        /// Type détaillé, nécessaire dans le cas des rapports, ou le type est "report" mais ou il 
        /// est nécessaire de savoir sur quel type de rapport on filtre en supplément (Impression, publipostage, export, graphique...)
        /// </summary>
        String _subType;

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager ()
        {
            const int USR_ALL = -1;  //TODO à déplacer dfans eConst

            try
            {

                _width = _requestTools.GetRequestFormKeyI("divW") ?? eConst.DEFAULT_WINDOW_WIDTH;
                _height = _requestTools.GetRequestFormKeyI("divH") ?? eConst.DEFAULT_WINDOW_HEIGHT;
                _type = _requestTools.GetRequestFormKeyS("type");
                if (_type.Length == 0)
                    _type = "main";

                _subType = _requestTools.GetRequestFormKeyS("rtype");
                _bFullList = _requestTools.GetRequestFormKeyB("full") ?? false;
                _tab = _requestTools.GetRequestFormKeyI("tab") ?? 200;

                _nFileid = _requestTools.GetRequestFormKeyI("fid") ?? 00;
                _reloadPaging = _requestTools.GetRequestFormKeyB("reloadpaging") ?? false;

                _deselectAllowed = _requestTools.GetRequestFormKeyB("deselectAllowed") ?? false;
                _adminMode = _requestTools.GetRequestFormKeyB("adminMode") ?? false;
                _selectFilterMode = _requestTools.GetRequestFormKeyB("selectFilterMode") ?? false;
                //AAB tâche 1882
                int formularType = 0;
                if (int.TryParse(_requestTools.GetRequestFormKeyS("formularType"), out formularType))
                    _formularType = (FORMULAR_TYPE)formularType;

                // Chargement de param de session
                SECURITY_GROUP securityGroup = _pref.GroupMode;

                //Mode d'affichage - CAS PLANNING - TODO : A faire autrement, ne pas utilise de dal dans les managers
                TableLite tab = new TableLite(_tab);
                eudoDAL dal = null;
                string err = String.Empty;
                try
                {
                    dal = eLibTools.GetEudoDAL(_pref);
                    dal.OpenDatabase();
                    tab.ExternalLoadInfo(dal, out err);
                }
                catch (Exception e)
                {
                    throw dal.InnerException ?? e;
                }
                finally
                {
                    dal?.CloseDatabase();
                }


                if (err.Length > 0)
                    throw new Exception(err);



                bool isPlanning = tab.EdnType == EdnType.FILE_PLANNING;
                bool isCalendarEnabled = isPlanning && _pref.GetPref(_tab, ePrefConst.PREF_PREF.CALENDARENABLED).Equals("1");

                EudoQuery.CalendarViewMode calViewMode = (EudoQuery.CalendarViewMode)eLibTools.GetNum(_pref.GetPref(_tab, ePrefConst.PREF_PREF.VIEWMODE));
                EudoQuery.CalendarTaskMode calTaskMode = (EudoQuery.CalendarTaskMode)eLibTools.GetNum(_pref.GetPref(_tab, ePrefConst.PREF_PREF.CALENDARTASKMODE));
                int nMenuUserId = eLibTools.GetNum(_pref.GetPref(_tab, ePrefConst.PREF_PREF.MENUUSERID));

                bool isListMode = !isPlanning || (calViewMode == CalendarViewMode.VIEW_CAL_LIST || calViewMode == CalendarViewMode.VIEW_CAL_TASK ||
                    (calViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER && calTaskMode != CalendarTaskMode.CALENDAR_ITEM_NO_TASK) &&
                    nMenuUserId > 0) && nMenuUserId != USR_ALL;

                bool isCalendarGraphEnabled = isCalendarEnabled &&
                    (
                                calViewMode == CalendarViewMode.VIEW_CAL_WORK_WEEK
                            || calViewMode == CalendarViewMode.VIEW_CAL_MONTH
                            || calViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER
                            );


                //Si liste ==> doList
                if (isListMode || (_type == "filter" || _type == "automation" || _type == "report" || _type == "lnkfile" || _type == "mailing" || _type == "mailtemplate" || _type == "formular" || _type == "importtemplate") || !isCalendarGraphEnabled)
                    doList();
                else
                    if (isCalendarGraphEnabled)
                    doPlanning(dal);
            }
            catch (eEndResponseException) { }
            catch (System.Threading.ThreadAbortException) { }
            catch (Exception exp)
            {

                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);


                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", exp.Message, Environment.NewLine, "Exception StackTrace :", exp.StackTrace);

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg));


                LaunchError();
            }
        }


        /// <summary>
        /// Génération d'une liste de type planning
        /// </summary>
        /// <param name="dal">Accès aux données</param>
        private void doPlanning (eudoDAL dal)
        {
            if (_bFullList)
            {
                int nRows = 0;
                int nPage = 0;

                nPage = _requestTools.GetRequestFormKeyI("page") ?? 1;
                nRows = _requestTools.GetRequestFormKeyI("rows") ?? 0;


                eRenderer rdrPlanning = eRendererFactory.CreateFullMainListRenderer(_pref, _tab, nPage, nRows, _height, _width);

                if (rdrPlanning.ErrorMsg.Length > 0)
                {
                    ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), rdrPlanning.ErrorMsg);
                    LaunchError();
                }

                JSONReturnGeneric res = GetJSON(rdrPlanning, true);
                RenderResult(RequestContentType.SCRIPT, delegate ()
                {
                    return SerializerTools.JsonSerialize(res);
                });
                return;

            }
            else
            {
                ePlanning cal = new ePlanning(dal, _pref, _tab, _width, _height, null);
                RenderResultHTML(cal.GetWeekRender());
            }
        }

        /// <summary>
        /// Génération d'une liste standard
        /// </summary>
        private void doList ()
        {

            // Spécifique au mailing
            int nTabFilter = 0;

            int nRows = 0;
            int nPage = 0;

            // #55495 : On reset les infos Paging afin de re-calculer le compteur
            if (_reloadPaging)
            {
                _pref.Context.Paging.resetInfo();
            }

            _pref.Context.Paging.Tab = _tab;

            nPage = _requestTools.GetRequestFormKeyI("page") ?? 1;
            nRows = _requestTools.GetRequestFormKeyI("rows") ?? 0;


            _bAddPublicItem = _requestTools.GetRequestFormKeyS("addpub") == "1";


            // TODOMAB : vérifier avec les fenêtres autres que les mails unitaires, qui utilisaient des valeurs à 0...
            int nWidth = _requestTools.GetRequestFormKeyI("width") ?? _width;
            int nHeight = _requestTools.GetRequestFormKeyI("height") ?? _height;

            eRenderer mainList = null;
            IRightTreatment oRightManager;

            //Indique s'il ne faut pas rendre le conteur principal de la liste mais uniquement les controle enfants
            // utilisé lorsqué que  le conteneur principal est un div uniquemennt destiné à contenir les controle a remplacer dans le
            // navigateur clients, eux meme inclus dans un div ne devant pas changer
            // par exemple, sur la liste des filtre, si on ne passe cette valeur a true, on se retrouve avec une imbrication non souhaité de div :
            // <div id="listconent"><div><div id="lv">... au lieu de <div id="listconent"><div id="lv">...
            Boolean bStripTop = false;
            switch (_type)
            {
                case "invit":
                    //[deprecated] servait à la pagination pour la liste des filtre sur sélection ++

                    bool bDelete = _requestTools.GetRequestFormKeyB("delete") ?? false;
                    int nParentFileId = 0;
                    int ntabFrom = _requestTools.GetRequestFormKeyI("tabFrom") ?? 0;

                    mainList = eRendererFactory.CreateSelectInvitWizardRenderer(_pref, _tab, ntabFrom, nParentFileId, nWidth, nHeight, bDelete);

                    break;
                case "filter":
                    bStripTop = true;
                    oRightManager = new eRightFilter(_pref);
                    TypeFilter tfilter = TypeFilter.USER;
                    try
                    {
                        tfilter = (TypeFilter)int.Parse(_subType);
                    }
                    catch (Exception e)
                    {
                        tfilter = TypeFilter.USER;
                    }
                    mainList = eRendererFactory.CreateFilterListRenderer(_pref, _tab, oRightManager, tfilter, nPage,
                         deselectAllowed: _deselectAllowed, adminMode: _adminMode, selectFilterMode: _selectFilterMode);
                    break;

                case "formular":
                    // AABBA tache #1882
                    oRightManager = new eRightFormular(_pref);
                    mainList = eRendererFactory.CreateFormularListRenderer(_pref, nWidth, nHeight, _tab, oRightManager, new eFormularListFilterParams() { AddPublicItem = true, FormularType = _formularType }, nPage);

                    break;
                case "report":
                    TypeReport reportType = (TypeReport)int.Parse(_subType);
                    oRightManager = new eRightReport(_pref, reportType);
                    mainList = eRendererFactory.CreateReportListRenderer(_pref, 800, 600, _tab, reportType, oRightManager, nFileId: _nFileid,
                        deselectAllowed: _deselectAllowed);
                    break;
                case "mailing":
                    if (_allKeys.Contains("filterTab"))
                        int.TryParse(_context.Request.Form["filterTab"].ToString(), out nTabFilter);
                    oRightManager = new eRightMailTemplate(_pref);
                    mainList = eRendererFactory.CreateMailTemplateListRenderer(_pref, nWidth, nHeight, nTabFilter, TypeMailTemplate.MAILTEMPLATE_EMAILING, oRightManager, nPage);
                    break;
                case "mailtemplate":
                    oRightManager = new eRightMailTemplate(_pref);
                    _mailTemplateType = eLibTools.GetEnumFromCode<TypeMailTemplate>(_subType);
                    if (_mailTemplateType == TypeMailTemplate.MAILTEMPLATE_UNDEFINED)
                        _mailTemplateType = TypeMailTemplate.MAILTEMPLATE_EMAIL;

                    mainList = eRendererFactory.CreateMailTemplateListRenderer(_pref, nWidth, nHeight, _tab, _mailTemplateType, oRightManager, nPage);
                    break;
                case "importtemplate":
                    mainList = eRendererFactory.CreateImportTemplateListRenderer(_pref, nWidth, nHeight, _tab, nPage);
                    break;
                case "automation":
                    AutomationType autoType = (AutomationType)(_requestTools.GetRequestFormKeyI("rtype") ?? 1);
                    int field = _requestTools.GetRequestFormKeyI("field") ?? 0;
                    mainList = eAdminRendererFactory.CreateAdminAutomationListDialogRenderer(_pref, _tab, field, nWidth, nHeight, 1, autoType);
                    if (mainList.ErrorNumber != EudoQuery.QueryErrorType.ERROR_NONE)
                    {
                        if (mainList.InnerException != null)
                            throw mainList.InnerException;
                        else
                            throw new Exception(mainList.ErrorMsg + " " + mainList.ErrorNumber.ToString());
                    }


                    break;
                case "lnkfile":
                    throw new NotImplementedException("renderer lnkfile not implemented");
                case "xrmhomepage":
                    mainList = eListFilesXrmWidgetRenderer.GetListFilesXrmWidgetRenderer(_pref, nHeight, nWidth, _tab, nPage, nRows, "2601");
                    mainList.Generate();
                    break;
                //break;
                default:

                    if (_bFullList)
                    {
                        bStripTop = true;
                        mainList = eRendererFactory.CreateFullMainListRenderer(_pref, _tab, nPage, nRows, _height, _width);

                        if (mainList.ErrorMsg.Length > 0)
                        {
                            ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), mainList.ErrorMsg);
                            LaunchError();
                        }

                        JSONReturnGeneric res = GetJSON(mainList, true);
                        RenderResult(RequestContentType.SCRIPT, delegate ()
                        {
                            return SerializerTools.JsonSerialize(res);
                        });

                        return;
                    }
                    else
                    {
                        mainList = eRendererFactory.CreateMainListRenderer(_pref, _tab, nPage, nRows, _height, _width, _bFullList);
                    }
                    break;
            }



            DateTime dtEnd = DateTime.Now;
            if (mainList.ErrorMsg.Length == 0)
            {
                RenderResultHTML(mainList.PgContainer, bStripTop);
            }
            else
            {
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), mainList.ErrorMsg);
                LaunchError();
            }
        }


        /// <summary>
        /// Retourne la représentation JSON d'un renderer
        /// </summary>
        /// <param name="rdr">renderer à transormer</param>
        /// <param name="fullRenderer">flag indiquant un renderer page entière</param>
        /// <returns></returns>
        public virtual JSONReturnGeneric GetJSON (eRenderer rdr, bool fullRenderer)
        {
            JSONReturnHTMLContent res = new JSONReturnHTMLContent();
            if (rdr == null)
                res.Success = false;

            res.Success = rdr.ErrorMsg.Length == 0;
            if (!res.Success)
                return res;


            res.CallBack = rdr.GetCallBackScript;
            res.Full = fullRenderer;
            if (rdr.DicContent.Count > 1)
            {
                foreach (var kvp in rdr.DicContent)
                {
                    res.MultiPartContent.Add(new PartContent()
                    {
                        Name = kvp.Key,
                        ID = kvp.Value.Ctrl.ID,
                        Content = GetResultHTML(kvp.Value.Ctrl),
                        CallBack = kvp.Value.CallBackScript
                    }
                    );
                }
            }
            else
                res.Html = GetResultHTML(rdr.PgContainer, true);

            return res;
        }
    }
}