using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Enumerations;

namespace Com.Eudonet.Xrm
{
    /// <className>ePlanningManager</className>
    /// <summary>Gestion des traitement asynchrone du planning : suppression de RDV... //TODO</summary>
    /// <purpose></purpose>
    /// <authors>JBE/GCH</authors>
    /// <date>2012-05-11</date>
    public class ePlanningManager : eEudoManager
    {
        /// <summary>Appelé à l'appel de la page</summary>
        protected override void ProcessManager()
        {
            Int32 nFileId = 0;
            String sFileId = String.Empty;
            Int32 nTab = 0;
            Boolean bConfidential = true;
            String action = string.Empty;         //Boolean bWithButton = false;
            string divId = string.Empty;

            if (_requestTools.AllKeys.Contains("fileid") && !String.IsNullOrEmpty(_context.Request.Form["fileid"]))
            {
                Int32.TryParse(_context.Request.Form["fileid"].ToString(), out nFileId);
                if (nFileId == 0)
                    sFileId = _context.Request.Form["fileid"].ToString();
            }

            if (_requestTools.AllKeys.Contains("tab") && !String.IsNullOrEmpty(_context.Request.Form["tab"]))
                Int32.TryParse(_context.Request.Form["tab"].ToString(), out nTab);

            //RDV confidentiel ?
            if (_requestTools.AllKeys.Contains("eCf") && !String.IsNullOrEmpty(_context.Request.Form["eCf"]))
                bConfidential = (_context.Request.Form["eCf"].ToString() == "1");

            if (_requestTools.AllKeys.Contains("action") && !String.IsNullOrEmpty(_context.Request.Form["action"]))
                action = _context.Request.Form["action"].ToString();

            if (_requestTools.AllKeys.Contains("divid") && !String.IsNullOrEmpty(_context.Request.Form["divid"]))
                divId = _context.Request.Form["divid"].ToString();


            if ((action == "clear" || nFileId > 0 || !String.IsNullOrEmpty(sFileId)) && nTab > 0)
            {
                eudoDAL dal = null;

                try
                {
                    dal = eLibTools.GetEudoDAL(_pref);
                    dal.OpenDatabase();
                    switch (action)
                    {
                        #region VCARD DEPUIS PLANNING

                        case "tooltip":
                            HtmlGenericControl hgcConf = new HtmlGenericControl("span");
                            if (!String.IsNullOrEmpty(sFileId))
                            {
                                List<Int32> listFileId = new List<Int32>();
                                String[] tabFileId = sFileId.Split(";");
                                for (int i = 0; i < tabFileId.Length; i++)
                                {
                                    Int32 currentFileId = 0;
                                    Int32.TryParse(tabFileId[i], out currentFileId);
                                    listFileId.Add(currentFileId);
                                }
                                ePlanningItem ePI = new ePlanningItem(dal, _pref, nTab, listFileId);
                                hgcConf.Controls.Add(ePI.GetToolTipRenderer(_pref, divId));
                            }
                            else
                            {
                                ePlanningItem ePI = new ePlanningItem(dal, _pref, nTab, nFileId);
                                hgcConf.Controls.Add(ePI.GetToolTipRenderer(_pref, divId));
                            }


                            RenderVcardResult(hgcConf, _context);
                            break;
                        #endregion
                        #region CONFLIT PLANNING

                        case "conflict":
                            PlanningType tyPlanning = PlanningType.CALENDAR_ITEM_TASK;

                            int scheduleId = _requestTools.GetRequestFormKeyI("scheduleid") ?? 0;
                            string owner = _requestTools.GetRequestFormKeyS("owner") ?? String.Empty;
                            string multiOwner = _requestTools.GetRequestFormKeyS("multiowner") ?? String.Empty;

                            string beginDate = _requestTools.GetRequestFormKeyS("begindate") ?? String.Empty;
                            string endDate = _requestTools.GetRequestFormKeyS("enddate") ?? String.Empty;
                            string beginDateDescid = _requestTools.GetRequestFormKeyS("begindatedescid") ?? String.Empty;

                            string typePlanning = _requestTools.GetRequestFormKeyS("typeplanning");

                            String modeBkm = _requestTools.GetRequestFormKeyS("modelist");
                            Boolean bModeBkm = (modeBkm == "1") ? true : false;

                            if (typePlanning != null && typePlanning != "-1")
                            {
                                if (!Enum.TryParse(typePlanning, true, out tyPlanning) || !Enum.IsDefined(typeof(PlanningType), tyPlanning))
                                    tyPlanning = PlanningType.CALENDAR_ITEM_TASK;
                            }
                            else if (nFileId > 0)
                            {
                                ePlanningItem ePITest = new ePlanningItem(dal, _pref, nTab, nFileId, bModeBkm);
                                tyPlanning = ePITest.TypePlanning;
                                if (bModeBkm)
                                {
                                    scheduleId = ePITest.ScheduleId;
                                    owner = (String.IsNullOrEmpty(owner)) ? ePITest.OwnerId : owner;
                                    multiOwner = (String.IsNullOrEmpty(multiOwner)) ? ePITest.MultiOwnerId : multiOwner;
                                    beginDate = (String.IsNullOrEmpty(beginDate)) ? ePITest.DateBegin.ToString() : beginDate;
                                    endDate = (String.IsNullOrEmpty(endDate)) ? ePITest.DateEnd.ToString() : endDate; 
                                    beginDateDescid = ePITest.DateBeginDescid.ToString();
                                }
                            }

                            string err = string.Empty;
                            XmlDocument xmlResult = new XmlDocument();

                            if (tyPlanning == PlanningType.CALENDAR_ITEM_TASK)
                            {

                                xmlResult = new XmlDocument();
                                XmlNode nodeResult = xmlResult.CreateElement("result");
                                xmlResult.AppendChild(nodeResult);
                                XmlNode nodeHeader = xmlResult.CreateElement("header");
                                nodeResult.AppendChild(nodeHeader);

                                XmlNode nodeDatas = xmlResult.CreateElement("datas");
                                nodeResult.AppendChild(nodeDatas);
                                RenderResult(RequestContentType.XML, delegate() { return xmlResult.OuterXml; });
                            }

                            eDataTools.GetConflictInfos(owner, multiOwner, _pref, nTab, nFileId, beginDate, beginDateDescid, endDate, out xmlResult, out err);

                            if (err.Length > 0)
                                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 72), string.Empty, eResApp.GetRes(_pref, 72), err));

                            RenderResult(RequestContentType.XML, delegate() { return xmlResult.OuterXml; });
                            break;


                        #endregion
                        #region Couper planning
                        case "cut":
                            _pref.Context.CuttedItems.AddOrUpdateValue(nTab, nFileId, true);
                            _pref.Context.CopiedItems.Remove(nTab);
                            break;
                        case "copy":
                            _pref.Context.CopiedItems.AddOrUpdateValue(nTab, nFileId, true);
                            _pref.Context.CuttedItems.Remove(nTab);
                            break;

                        case "copyPast":
                            //_pref.Context.CopiedItems.AddOrUpdateValue(nTab, nFileId, true);
                            //_pref.Context.CuttedItems.;
                            break;

                        case "clear":
                            _pref.Context.CopiedItems.Remove(nTab);
                            _pref.Context.CuttedItems.Remove(nTab);
                            break;
                        #endregion
                        #region Périodicité
                        case "schedule":
                            Control ctrl = getScheduleOptionsWindow();
                            break;
                        #endregion
                        default:
                            break;

                    }
                }
                catch (eEndResponseException) { }
                catch (ThreadAbortException) { }    // Laisse passer le response.end du RenderResult
                catch (Exception e)
                {
                    LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 72), String.Concat("Erreur non gérée : ", Environment.NewLine, e.ToString())));
                }
                finally
                {
                    if (dal != null)
                        dal.CloseDatabase();
                }
            }
            else
                LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 6524), eResApp.GetRes(_pref, 72)));
        }

        private Control getScheduleOptionsWindow()
        {
            Panel divFreq = new Panel();
            divFreq.ID = "divFreq";
            divFreq.CssClass = "divFreq";
            divFreq.Controls.Add(new LiteralControl("Fréquence"));

            HtmlGenericControl lstFreq = new HtmlGenericControl("select");
            divFreq.Controls.Add(lstFreq);

            HtmlGenericControl opt1 = new HtmlGenericControl("option");
            opt1.InnerText = eResApp.GetRes(_pref, 1050);
            HtmlGenericControl opt2 = new HtmlGenericControl("option");
            opt2.InnerText = eResApp.GetRes(_pref, 1051);
            HtmlGenericControl opt3 = new HtmlGenericControl("option");
            opt3.InnerText = eResApp.GetRes(_pref, 1052);
            HtmlGenericControl opt4 = new HtmlGenericControl("option");
            opt4.InnerText = eResApp.GetRes(_pref, 1053);



            Panel divPer = new Panel();
            divPer.ID = "divPer";
            divPer.CssClass = "divPer";
            divPer.Controls.Add(new LiteralControl(eResApp.GetRes(_pref, 402)));

            return divFreq;
        }

        /// <summary>
        /// Renvoi le rendu de la VCARD
        /// </summary>
        /// <param name="PanDisp">Controle à affichée</param>
        /// <param name="context">Page</param>
        private void RenderVcardResult(Control PanDisp, HttpContext context)
        {

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter tw = new HtmlTextWriter(sw);

            PanDisp.RenderControl(tw);
            context.Response.Clear();
            String fileWebName = eTools.WebPathCombine("themes", _pref.ThemeXRM.Folder, "css", "theme.css");
            String s = string.Concat("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 //EN\">", Environment.NewLine
                , "<html>", Environment.NewLine
                , "<head>", Environment.NewLine
                , "<link type='text/css' rel='stylesheet' href='", fileWebName, "?ver=", eConst.VERSION, ".", eConst.REVISION, "'>", Environment.NewLine
                , "<link type='text/css' rel='stylesheet' href='themes/default/css/ePlanning.css?ver=", eConst.VERSION, ".", eConst.REVISION, "'>", Environment.NewLine
                , "<link type='text/css' rel='stylesheet' href='themes/default/css/eudoFont.css?ver=", eConst.VERSION, ".", eConst.REVISION, "'>", Environment.NewLine
                , "</head>", Environment.NewLine
                , "<body class='pl_tt_body'>", Environment.NewLine
                , sb.ToString(), Environment.NewLine
                , "</body>", Environment.NewLine
                , "</html>");
            context.Response.Write(s);
        }




    }
}