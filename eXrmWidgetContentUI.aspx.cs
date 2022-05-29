using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Web;
using System.Web.UI;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public partial class eXrmWidgetContentUI : eEudoPage
    {
        protected XrmWidgetType _wt;
        protected int _wid = 0;
        protected int _tab = 0;
        protected string _context = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            eudoDAL eDal = null;

            Stopwatch watch = Stopwatch.StartNew();

            try
            {

                PageRegisters.AddScript("eMain");
                PageRegisters.AddScript("eTools");
                PageRegisters.AddScript("eList");
                PageRegisters.AddScript("eModalDialog");

                PageRegisters.AddCss("eMain");
                PageRegisters.AddCss("eIcon");
                PageRegisters.AddCss("eList");
                PageRegisters.AddCss("eTitle");
                PageRegisters.AddCss("eXrmHomePage");

                String sError = string.Empty;
                int width = 0, height = 0;
                bool bZoom = false;
                int nWidgetType = -1;
                eXrmWidgetContext context = new eXrmWidgetContext(0);

                _wid = _requestTools.GetRequestQSKeyI("wid") ?? 0;

                nWidgetType = _requestTools.GetRequestQSKeyI("wt") ?? -1;
                _wt = (XrmWidgetType)nWidgetType;

                bZoom = _requestTools.GetRequestQSKeyB("z") ?? false;

                width = _requestTools.GetRequestQSKeyI("w") ?? 0;
                height = _requestTools.GetRequestQSKeyI("h") ?? 0;

                _context = _requestTools.GetRequestQSKeyS("c");

                try
                {
                    if (!String.IsNullOrEmpty(_context))
                    {
                        _context = _context.Replace(Environment.NewLine, "");
                        context = JsonConvert.DeserializeObject<eXrmWidgetContext>(HttpUtility.UrlDecode(_context));
                    }

                }
                catch (Exception)
                {

                }


                hidWidgetType.Value = nWidgetType.ToString();
                hidWid.Value = _wid.ToString();

                if (_wt == XrmWidgetType.Editor)
                {
                    #region Type éditeur HTML
                    if (_wid > 0)
                    {

                        eFile file = eFileMain.CreateMainFile(_pref, (int)TableType.XRMWIDGET, _wid, -2);
                        if (file != null)
                        {
                            if (!bZoom)
                            {
                                // Affichage de la display value
                                eFieldRecord field = file.GetField((int)XrmWidgetField.ContentSource);
                                contentWrapper.InnerHtml = field.DisplayValue;
                            }
                            else
                            {
                                // Mode popup plein écran
                                IXrmWidgetUI IWidget = new eXrmWidgetContent(eXrmWidgetFactory.GetWidgetUI(_pref, file.Record), _pref, width - 30, height - 120);
                                eXrmWidgetParam widgetParam = new eXrmWidgetParam(_pref, file.FileId);
                                IWidget.Init(file.Record, new eXrmWidgetPref(), widgetParam, context);
                                IWidget.Build(contentWrapper);
                            }
                        }

                    }
                    #endregion
                }
                else if (_wt == XrmWidgetType.ExpressMessage)
                {
                    #region Type Message Express
                    eDal = eLibTools.GetEudoDAL(_pref);

                    try
                    {
                        eDal.OpenDatabase();
                        eda.eAdminHomeExpressMessage message = eda.eSqlHomeExpressMessage.GetExpressMessageByUserID(_pref, eDal, out sError);

                        if (!String.IsNullOrEmpty(sError))
                            throw new Exception(sError);

                        if (message != null)
                            contentWrapper.InnerHtml = message.Content;
                    }
                    finally
                    {
                        eDal.CloseDatabase();
                    }
                    #endregion
                }
                else if (_wt == XrmWidgetType.Kanban)
                {
                    #region Type Kanban

                    PageRegisters.AddCss("eKanban");
                    PageRegisters.AddScript("eUpdater");
                    PageRegisters.AddScript("eEngine");
                    PageRegisters.AddScript("eContextMenu");
                    PageRegisters.AddScript("eDragDropManager");
                    PageRegisters.AddScript("grid/widget/eWidgetKanban");

                    int selectedSLDescid = 0;
                    bool selectedSLIsGroup = false;

                    // Récup des pref en session
                    if (_pref.Context.KanbanSelectedSwimlane.ContainsKey(_wid))
                    {
                        eKanbanSwimlane sl = _pref.Context.KanbanSelectedSwimlane[_wid];
                        if (sl != null)
                        {
                            selectedSLDescid = sl.Descid;
                            selectedSLIsGroup = sl.Settings.IsGroup;
                        }

                    }

                    eKanbanRenderer rdr = eKanbanRenderer.CreateKanbanRenderer(_pref, _wid, context, selectedSLDescid, selectedSLIsGroup);
                    rdr.Generate();

                    if (!String.IsNullOrEmpty(rdr.ErrorMsg))
                    {
                        ErrorContainer = eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            eResApp.GetRes(_pref, 544),
                            eResApp.GetRes(_pref, 416),
                            String.Concat(rdr.ErrorMsg, Environment.NewLine, (rdr.InnerException != null) ? String.Concat(rdr.InnerException.Message, " - ", rdr.InnerException.StackTrace) : ""));
                        LaunchErrorHTML(true);
                    }
                    else
                    {
                        _tab = rdr.Tab;
                        contentWrapper.Controls.Add(rdr.PgContainer);
                    }
                    #endregion

                }
                else if (_wt == XrmWidgetType.List)
                {
                    #region Type List
                    PageRegisters.AddCss("eContextMenu");
                    PageRegisters.AddScript("eEngine");
                    PageRegisters.AddScript("eUpdater");
                    PageRegisters.AddScript("eCalendar");
                    PageRegisters.AddScript("ePopup");
                    PageRegisters.AddScript("eContextMenu");
                    PageRegisters.AddScript("eExpressFilter");
                    PageRegisters.AddScript("eFieldEditor");
                    PageRegisters.AddScript("ePlanning");
                    PageRegisters.AddScript("grid/widget/eListWidget");

                    _tab = _requestTools.GetRequestQSKeyI("tab") ?? 0;

                    eListWidgetRenderer rdr = eListWidgetRenderer.CreateListWidgetRenderer(_pref, _wid, _tab, widgetContext: context);
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
                        contentWrapper.Controls.Add(rdr.PgContainer);
                    }
                    #endregion
                }
                else if (_wt == XrmWidgetType.RSS)
                {
                    #region Type RSS
                    PageRegisters.AddCss("eRSS");

                    eRSSRenderer rdr = eRSSRenderer.CreateRSSRenderer(_pref, _wid);
                    rdr.Generate();

                    if (!String.IsNullOrEmpty(rdr.ErrorMsg))
                    {
                        ErrorContainer = eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            rdr.ErrorMsg,
                            eResApp.GetRes(_pref, 416),
                            String.Concat((rdr.InnerException != null) ? String.Concat(rdr.InnerException.Message, " - ", rdr.InnerException.StackTrace) : ""));
                        LaunchErrorHTML(true);

                    }
                    else
                    {
                        contentWrapper.Controls.Add(rdr.PgContainer);
                    }
                    #endregion
                }

                watch.Stop();

                // Statistique de chargement de widgets
                eRequestReportManager.CallInfo cf = new eRequestReportManager.CallInfo();
                cf.ElapsedTime = (int)watch.ElapsedMilliseconds;
                cf.BaseName = _pref.GetBaseName;
                cf.User = _pref.User.UserLogin;
                cf.Action = "LOAD_CONTENT_" + _wt.ToString().ToUpper();
                cf.Widget = _wid;
                if (context != null)
                    cf.Grid = context.GridId;

                eRequestReportManager.Instance.Add(cf);
            }
            catch (eEndResponseException) { }
            catch (Exception exc)
            {

#if DEBUG
                ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, exc.Message, exc.StackTrace, eResApp.GetRes(_pref, 72), exc.StackTrace);
#else
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminAutomationListDialog : ", exc.StackTrace));      
            
#endif

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }
            finally
            {
                if (eDal != null)
                    eDal.CloseDatabase();

                watch = null;
            }
        }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

    }
}
