using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Description résumée de eComputedValue
    /// </summary>
    public class eComputedValue : eEudoManager
    {
        private Int32 _nParentTab = 0;
        private Int32 _nParentFileId = 0;
        private Int32 _nParentPmFileId = 0;
        private Int32 _nTab = 0;
        private Int32 _nWidgetFilterId = 0;
        private String _sListCol = String.Empty;

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            //Récupération des paramètres
            if (_requestTools.AllKeys.Contains("tab") && !String.IsNullOrEmpty(_context.Request.Form["tab"]))
                Int32.TryParse(_context.Request.Form["tab"].ToString(), out _nTab);

            if (_requestTools.AllKeys.Contains("ParentTab") && !String.IsNullOrEmpty(_context.Request.Form["ParentTab"]))
                Int32.TryParse(_context.Request.Form["ParentTab"].ToString(), out _nParentTab);

            if (_requestTools.AllKeys.Contains("ParentFileId") && !String.IsNullOrEmpty(_context.Request.Form["ParentFileId"]))
                Int32.TryParse(_context.Request.Form["ParentFileId"].ToString(), out _nParentFileId);

            if (_requestTools.AllKeys.Contains("ParentPmFileId") && !String.IsNullOrEmpty(_context.Request.Form["ParentPmFileId"]))
                Int32.TryParse(_context.Request.Form["ParentPmFileId"].ToString(), out _nParentPmFileId);

            if (_requestTools.AllKeys.Contains("ListCol") && !String.IsNullOrEmpty(_context.Request.Form["ListCol"]))
                _sListCol = _context.Request.Form["ListCol"].ToString();

            if (_requestTools.AllKeys.Contains("WidgetFilterId") && !String.IsNullOrEmpty(_context.Request.Form["WidgetFilterId"]))
                Int32.TryParse(_context.Request.Form["WidgetFilterId"].ToString(), out _nWidgetFilterId);

            //Paramètres invalides
            if (_nTab == 0 || _sListCol.Length == 0)
            {
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine, "(_nTab == 0 || _sListCol.Length == 0)");

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "")), // Paramètres invalides
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg, Environment.NewLine, "Tab:", _nTab, Environment.NewLine, "ListCol:", _sListCol));

                LaunchError();
            }

            Dictionary<String, decimal> dicAliasValue = new Dictionary<String, decimal>();
            HashSet<string> hsFlt = new HashSet<string>();

            List<Int32> lstDescId = new List<int>();

            //Somme de colonnes depuis un bookmark
            Boolean bFromBkm = ((_nParentTab != _nTab) && _nParentFileId > 0 && _nParentTab > 0);

            //Type de Eudoquery a utiliser
            EudoQuery.ViewQuery typeQuery = EudoQuery.ViewQuery.TREATMENT;
            if (bFromBkm)
                typeQuery = EudoQuery.ViewQuery.LIST_BKM;

            Int32 nViewTab = _nTab;
            if (_nTab == _nParentTab + EudoQuery.AllField.BKM_PM_EVENT.GetHashCode())
            {
                typeQuery = EudoQuery.ViewQuery.LIST_BKM_EVENTPM;
                nViewTab = _nParentTab;
            }

            EudoQuery.EudoQuery eq = eLibTools.GetEudoQuery(_pref, nViewTab, typeQuery);
            if (bFromBkm)
            {
                eq.SetParentDescid = _nParentTab;
                eq.SetParentFileId = _nParentFileId;
            }
            else
            {
                //Fiches marquées
                if (_pref.Context.MarkedFiles.ContainsKey(_nTab))
                    eq.SetDisplayMarkedFile = _pref.Context.MarkedFiles[_nTab].Enabled;

                //Filtre Avancé
                if (_pref.Context.Filters.ContainsKey(_nTab))
                    eq.SetFilterId = _pref.Context.Filters[_nTab].FilterSelId;

                // ALISTER => Demande / Request 91554 Ajout d'une condition pour le filtre de widget
                if (_nWidgetFilterId > 0)
                {
                    //Peut être nécessaire un jour ?
                    //eXrmWidgetParam xrmWidgetParam = new eXrmWidgetParam(_pref, _nWidgetId);
                    //eq.SetFilterId = Convert.ToInt32(xrmWidgetParam.GetParamValue("filterid"));

                    eq.SetFilterId = _nWidgetFilterId;
                }
            }

            eq.SetListCol = _sListCol;
            eq.LoadRequest();

            #region gestion erreur
            if (eq.GetError.Length > 0)
            {
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                if (eq.InnerException != null)
                {
                    Exception ex = eq.InnerException;

                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", ex.Message, Environment.NewLine, "Exception StackTrace :", ex.StackTrace);
                }
                else
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Error Message : ", eq.GetError);

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg));


                LaunchError();
                return;
            }
            #endregion

            //Liste des champs rétourné
            List<EudoQuery.Field> lstFieldHeader = eq.GetFieldHeaderList;
            List<EudoQuery.Table> lstTableHeader = eq.GetTableHeaderList;
            EudoQuery.Table mainTable = eq.GetMainTable;

            //Récupération des colonnes à calculer
            string[] listCol = _sListCol.Split(';');
            foreach (String sDescId in listCol)
            {
                Int32 nCurrDescId;

                //Recherche le field à calculer
                if (Int32.TryParse(sDescId, out nCurrDescId) && lstFieldHeader.Exists(delegate(EudoQuery.Field fld) { return fld.Descid == nCurrDescId; }) && !lstDescId.Contains(nCurrDescId))
                {
                    dicAliasValue.Add(lstFieldHeader.Find(delegate(EudoQuery.Field fld) { return fld.Descid == nCurrDescId; }).Alias, 0);
                    lstDescId.Add(nCurrDescId);
                }
            }

            //Aucun champ à calculer - Msg user only
            if (dicAliasValue.Count == 0)
            {
                ErrorContainer = eErrorContainer.GetUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 581)),  //  Aucune rubrique sélectionnée
                   eResApp.GetRes(_pref, 72));

                LaunchError();
            }

            //On doit charger toutes la liste
            eq.SetTopRecord = 0;

            //Construction de la requte
            eq.BuildRequest();

            String sSQL = eq.EqQuery;
            ViewQuery vq = eq.GetQueryType;
            eq.CloseQuery();
            eudoDAL eDal = eLibTools.GetEudoDAL(_pref);
            eDal.OpenDatabase();

            string err = String.Empty;
            RqParam rqList = new RqParam(sSQL);
            if (vq == ViewQuery.LIST_BKM_REL)
            {
                sSQL = sSQL.Replace("$#$fileid$#$", "@fileid");
                rqList.SetQuery(sSQL);
                rqList.AddInputParameter("@fileid", SqlDbType.Int, _nParentFileId);
            }
            else if (vq == ViewQuery.LIST_BKM_EVENTPM)
            {
                sSQL = sSQL.Replace("$#$PMID$#$", "@pmid");
                rqList.SetQuery(sSQL);
                rqList.AddInputParameter("@pmid", SqlDbType.Int, _nParentPmFileId);
            }
            DataTableReaderTuned dtrStats = eDal.Execute(rqList, out err);
            try
            {
                eDal.CloseDatabase();
                //Gestion erreur
                if (err.Length > 0)
                {
                    String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                    if (eDal.InnerException != null)
                    {

                        sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", eDal.InnerException.Message, Environment.NewLine, "Exception StackTrace :", eDal.InnerException.StackTrace);
                    }
                    else
                        sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Error Message : ", err);


                    ErrorContainer = eErrorContainer.GetDevUserError(
                       eLibConst.MSG_TYPE.CRITICAL,
                       eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                       String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                       eResApp.GetRes(_pref, 72),  //   titre
                       String.Concat(sDevMsg));


                    LaunchError();
                }


                string sIdAlias = String.Concat(mainTable.Alias, "_ID");

                while (dtrStats.Read())
                {

                    foreach (Int32 nDescId in lstDescId)
                    {

                        //Champ à calculer
                        Field fldToCompute = lstFieldHeader.Find(delegate(EudoQuery.Field fld)
                        {
                            return fld.Descid == nDescId;
                        });

                        if (fldToCompute != null)
                        {
                            String sAliasId = String.Concat(fldToCompute.Table.Alias, "_ID");
                            Int32 nFieldFileId;
                            if (!Int32.TryParse(dtrStats.GetString(sAliasId), out nFieldFileId))
                                continue;

                            String sKeyValue = String.Concat(sAliasId, "_", fldToCompute.Descid, "_", nFieldFileId);

                            //   if (!lstFldId.Contains(sKeyValue))

                            if (!hsFlt.Contains(sKeyValue))
                            {

                                hsFlt.Add(sKeyValue);

                                //Droit de visu
                                if (eLibTools.AllowedView(fldToCompute, dtrStats, _pref.GroupMode))
                                {
                                    decimal dVal = dtrStats.GetDecimal(fldToCompute.ValueAlias);
                                    dicAliasValue[fldToCompute.Alias] += dVal;

                                }
                            }
                        }

                    }
                }

            }
            finally
            {
                //Libération ressources
                if (dtrStats != null)
                    dtrStats.Dispose();
            }


            //Construction flux de retour

            try
            {

                RenderResult(RequestContentType.XML, delegate() { return ComputeValue(dicAliasValue, lstFieldHeader).OuterXml; });

            }
            catch (eEndResponseException) { }
            catch (System.Threading.ThreadAbortException) { }
            catch (Exception genEx)
            {
                //Gestion d'erreur Générique

                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);


                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", genEx.Message, Environment.NewLine, "Exception StackTrace :", genEx.StackTrace);

                sDevMsg = String.Empty;

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
        /// Génération du flux de retour XML
        /// </summary>
        /// <param name="res">Dictionnaire field alias/valeur des colonnes calculées</param>
        /// <param name="lstFieldHeader">Liste des FIELD utilisé par les colonnes calculés. Permet l'accès aux informations sur ces champs</param>
        /// <returns></returns>
        private XmlDocument ComputeValue(Dictionary<String, decimal> res, List<EudoQuery.Field> lstFieldHeader)
        {



            XmlDocument xmlResult = new XmlDocument();

            XmlNode mainNode = xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlResult.AppendChild(mainNode);

            XmlNode xmlNodeRoot = xmlResult.CreateElement("ednResult");
            xmlResult.AppendChild(xmlNodeRoot);

            // Indicateur de non success
            XmlNode xmlNodeSuccess = xmlResult.CreateElement("success");
            xmlNodeRoot.AppendChild(xmlNodeSuccess);
            xmlNodeSuccess.InnerText = "1";




            // Champs calculés
            XmlElement fieldsNode = xmlResult.CreateElement("fields");
            xmlNodeRoot.AppendChild(fieldsNode);


            foreach (KeyValuePair<String, decimal> kv in res)
            {

                EudoQuery.Field fldToCompute = lstFieldHeader.Find(delegate(EudoQuery.Field fld) { return fld.Alias == kv.Key; });

                XmlElement fieldNode = xmlResult.CreateElement("field");

                XmlAttribute resultAttribute = xmlResult.CreateAttribute("alias");
                resultAttribute.Value = eTools.GetFieldValueCellName(_nTab, kv.Key.ToString());
                fieldNode.Attributes.Append(resultAttribute);


                resultAttribute = xmlResult.CreateAttribute("descid");
                resultAttribute.Value = fldToCompute.Descid.ToString();
                fieldNode.Attributes.Append(resultAttribute);


                resultAttribute = xmlResult.CreateAttribute("value");
                resultAttribute.Value = eNumber.FormatNumber(_pref, kv.Value, fldToCompute.Length, true);
                fieldNode.Attributes.Append(resultAttribute);

                fieldsNode.AppendChild(fieldNode);
            }



            return xmlResult;
        }

    }
}