using System;
using Com.Eudonet.Internal;
using EudoQuery;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using System.Text;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <className>eQueryManagerAsh</className>
    /// <summary>Retourne le résultat d'une requête quelconque
    ///  -> exemple d'usage : recherche MRU sur onglet</summary>
    /// <purpose> 
    /// <result>
    ///     <error></error>
    ///     <errorMsg></errorMsg>
    ///     <content></content>
    ///  </result>
    /// </purpose>
    /// <authors>SPH</authors>
    /// <date>2012-04-10</date>
    public class eQueryManagerAsh : eEudoManager
    {
        enum MANAGER_CALL_TYPE
        {
            SEARCH_MRU = 1,
            COUNT_ADR_FROM_PM = 2,
            GET_LIST_ID = 3
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            MANAGER_CALL_TYPE type = 0;
            Int32 tab = 0;

            // Table
            if (_allKeys.Contains("tab") && !String.IsNullOrEmpty(_context.Request.Form["tab"]))
                tab = eLibTools.GetNum(_context.Request.Form["tab"].ToString());
            else
                LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), "Paramètre 'tab' non fourni", eResApp.GetRes(_pref, 72)));  

            // Type
            if (_allKeys.Contains("type") && !String.IsNullOrEmpty(_context.Request.Form["type"]))
                type = (MANAGER_CALL_TYPE)eLibTools.GetNum(_context.Request.Form["type"].ToString());
            else
                LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), "Paramètre 'type' non fourni", eResApp.GetRes(_pref, 72)));

            #region Chargement des informations parentes - Paging bkm

            Int32 parentTab = 0;
            Int32 parentFileId = 0;
            if (type == MANAGER_CALL_TYPE.COUNT_ADR_FROM_PM || type == MANAGER_CALL_TYPE.GET_LIST_ID)
            {
                
                if (_allKeys.Contains("parenttab") && _context.Request.Form["parenttab"] != null)
                {
                    parentTab = eLibTools.GetNum(_context.Request.Form["parenttab"]);

                    if (_allKeys.Contains("parentfileid") && _context.Request.Form["parentfileid"] != null)
                        parentFileId = eLibTools.GetNum(_context.Request.Form["parentfileid"]);

                    if (parentTab == 0 || parentFileId == 0)
                        LaunchError(eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            "",
                            eResApp.GetRes(_pref, 72),
                            "Paramètre Parenttab/parentfileid invalide"));
                }
            }

            #endregion

            eQueryResult eqrQueryManager = null;

            //
            switch (type)
            {
                // Recherche MRU
                case MANAGER_CALL_TYPE.SEARCH_MRU:

                    #region Recherche MRU

                    String searchValue = String.Empty;
                    if (_allKeys.Contains("search") && !String.IsNullOrEmpty(_context.Request.Form["search"]))
                        searchValue = _context.Request.Form["search"].ToString();

                    Boolean MultiWordSearch = false;
                    if (_allKeys.Contains("multiword") && !String.IsNullOrEmpty(_context.Request.Form["multiword"]))
                        MultiWordSearch = _context.Request.Form["multiword"].ToString() == "1";

                    try
                    {
                        eqrQueryManager = new eQueryResult(_pref, tab);

                        XmlDocument xmlDoc = eqrQueryManager.GetRecords(delegate(EudoQuery.EudoQuery query)
                            {
                                query.SetListCol = (tab + 1).ToString();

                                query.SetTopRecord = 7;

                                //Ajout le critère de filter sur la rubrique principale de la table
                                query.AddParam("MainSearchValue", searchValue);
                                query.AddParam("ActiveMainSearchValue", "1");
                                query.AddParam("ActiveMultiwordSearch", MultiWordSearch ? "1" : "0");
                                query.AddParam("ActiveHisto", "1");
                                //query.AddParam("Histo", "0");
                                
                            });

                        WriteContentAsXML(eQueryResult.GetContentAsXML(xmlDoc));
                    }

                    catch (eEndResponseException) { }
                    catch (ThreadAbortException){}
                    catch (Exception exp)
                    {
                        LaunchError(eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            "",
                            eResApp.GetRes(_pref, 72),
                            exp.Message));
                    }

                    #endregion

                    break;
                case MANAGER_CALL_TYPE.COUNT_ADR_FROM_PM:

                    #region Compteur d'adresse depuis PM

                    try
                    {
                        eqrQueryManager = new eQueryResult(_pref, tab, ViewQuery.LIST_BKM);

                        Int32 nRes = eqrQueryManager.GetCount(delegate(EudoQuery.EudoQuery query)
                            {
                                query.SetParentDescid = parentTab;
                                query.SetParentFileId = parentFileId;
                            });

                        RenderResult(RequestContentType.TEXT, delegate() { return nRes.ToString(); });
                    }
                    catch (eEndResponseException) { }
                    catch (ThreadAbortException)
                    {
                    }
                    catch (Exception exp)
                    {
                        ErrorContainer = GetStandardError(exp);
                        LaunchError();
                    }

                    #endregion

                    break;
                case MANAGER_CALL_TYPE.GET_LIST_ID:

                    #region Retourne la liste des id pour le mode liste demandé

                    // Ligne par page
                    Int32 rows = 0;
                    if (_allKeys.Contains("rows") && !String.IsNullOrEmpty(_context.Request.Form["rows"]))
                        rows = eLibTools.GetNum(_context.Request.Form["rows"]);

                    //Num page
                    Int32 page = 0;
                    if (_allKeys.Contains("page") && !String.IsNullOrEmpty(_context.Request.Form["page"]))
                        page = eLibTools.GetNum(_context.Request.Form["page"]);

                    try
                    {
                        ICollection<Int32> lst = eListFactory.GetListIds(_pref, tab, page, rows, parentTab, parentFileId);
                        if (lst.Count > 0)
                            RenderResult(RequestContentType.TEXT, delegate() { return String.Join<Int32>(";", lst); });
                        else
                            RenderResult(RequestContentType.TEXT, delegate() { return "-1"; });
                    }
                    catch (eEndResponseException) { }
                    catch (ThreadAbortException)
                    {
                    }
                    catch (Exception exp)
                    {
                        ErrorContainer = GetStandardError(exp);
                        LaunchError();
                    }

                    #endregion

                    break;
                default:

                    break;
            }
        }

        /// <summary>
        /// Envoi au navigateur le flux XML résultat
        /// </summary>
        private void WriteContentAsXML(String sRes)
        {
            // Envoie du Flux
            _context.Response.ContentType = "text/xml";
            _context.Response.Write(sRes);
            throw new eEndResponseException();
            //            _context.Response.End();
        }

        private eErrorContainer GetStandardError(Exception exp)
        {
            String pageName = System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1];

            String devMsg = new StringBuilder()
                        .Append("Erreur sur la page : ").Append(pageName).AppendLine()
                        .Append("Exception Message : ").Append(exp.Message).AppendLine()
                        .Append("Exception StackTrace :").Append(exp.StackTrace).ToString();

            return eErrorContainer.GetDevUserError(
               eLibConst.MSG_TYPE.CRITICAL,
               eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
               String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
               eResApp.GetRes(_pref, 72),  //   titre
               devMsg);
        }
    }
}