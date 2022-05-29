using Com.Eudonet.Core.Model;
using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Result;
using Com.Eudonet.Engine.Result.Data;
using Com.Eudonet.Engine.Result.Xrm;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Internal.SpecifTools;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using static Com.Eudonet.Xrm.eUpdateFieldManager;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminDuplicateGrid
    /// </summary>
    public class eAdminDuplicateGrid : eAdminManager
    {
        protected override void ProcessManager()
        {

            int iGrid = 0, iTab = 0, iParentTabFileId = 0;
            string title = "";
            eudoDAL _dal = null;
            string sError = "";
            eAdminResult result = new eAdminResult();
            RqParam rq = new RqParam();

            //Initialisation
            if (_requestTools.AllKeys.Contains("tab") && !String.IsNullOrEmpty(_context.Request.Form["tab"]))
                int.TryParse(_context.Request.Form["tab"].ToString(), out iTab);

            if (_requestTools.AllKeys.Contains("grid") && !String.IsNullOrEmpty(_context.Request.Form["grid"]))
                int.TryParse(_context.Request.Form["grid"].ToString(), out iGrid);

            if (_requestTools.AllKeys.Contains("file") && !String.IsNullOrEmpty(_context.Request.Form["file"]))
                int.TryParse(_context.Request.Form["file"].ToString(), out iParentTabFileId);

            if (_requestTools.AllKeys.Contains("title") && !String.IsNullOrEmpty(_context.Request.Form["title"]))
                title = _context.Request.Form["title"].ToString();

            try
            {
                _dal = eLibTools.GetEudoDAL(_pref);
                _dal.OpenDatabase();

                eAdminDuplicateGridFromGrid(iGrid, iTab, iParentTabFileId, title, _dal, sError);

                if (!string.IsNullOrEmpty(sError))
                {
                    result.Success = false;
                    result.UserErrorMessage = eResApp.GetRes(_pref, 1760);
                    result.DebugErrorMessage = sError;
                    result.Criticity = 0;
                }

                result.Success = true;
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
                //LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 7657), title: " ", devMsg: sError));

                result.Success = false;
                result.UserErrorMessage = eResApp.GetRes(_pref, 1760);
                result.DebugErrorMessage = sError;
                result.Criticity = 0;
                result.InnerException = e;

                LaunchError(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 7143), e), RequestContentType.SCRIPT);
            }
            finally
            {
                _dal?.CloseDatabase();
            }

            RenderResult(RequestContentType.TEXT, () => JsonConvert.SerializeObject(result));

        }

        /// <summary>
        /// Permet de dupliquer une grille avec toutes ses informations
        /// Copy a grid with all his informations
        /// </summary>
        private void eAdminDuplicateGridFromGrid(int iGrid, int iTab, int iParentTabFileId, string title, eudoDAL _dal, string sError)
        {

            int oldViewPermId = 0;
            int oldUpdatePermId = 0;
            int iDisplayOrder = 0;

            #region Chargement des données de la grille / Load grid's data

            //Charger le DataFiller de l'ancienne grille
            eDataFillerGeneric fillerGrid = new eDataFillerGeneric(_pref, (int)TableType.XRMGRID, ViewQuery.CUSTOM);

            //ALISTER Demande/Request 95004, J'ai commenté ParentTab et ParentFileId car ils n'ont pas l'air d'être utile
            //I've commented ParentTab and ParentFileId because it seems they don't be useful
            fillerGrid.EudoqueryComplementaryOptions = delegate (EudoQuery.EudoQuery query)
            {
                query.SetFileId = iGrid;
                query.SetListCol = String.Concat(
                            //(int)XrmGridField.ParentTab, ";", (int)XrmGridField.ParentFileId, ";",
                            (int)XrmGridField.Tooltip, ";", (int)XrmGridField.DisplayOrder, ";",
                            (int)XrmGridField.ViewPermId, ";", (int)XrmGridField.UpdatePermId, ";",
                            (int)XrmGridField.Owner);
            };

            fillerGrid.Generate();

            for (int i = 0; i < fillerGrid.ListRecords[0].GetFields.Count; i++)
            {
                if (fillerGrid.ListRecords[0].GetFields[i].Value != "null" &&
                    fillerGrid.ListRecords[0].GetFields[i].Value != "NULL" &&
                    fillerGrid.ListRecords[0].GetFields[i].Value != "")
                {

                    Field fillerField = fillerGrid.ListRecords[0].GetFields[i].FldInfo;

                    switch (fillerField.Descid)
                    {
                        //ALISTER Demande/Request 95004, N'est peut-être pas utile
                        //Seems to be not useful maybe
                        //case ((int)XrmGridField.ParentTab):
                        //    iTab = Convert.ToInt32(fillerGrid.ListRecords[0].GetFields[i].Value);
                        //    break;
                        //case ((int)XrmGridField.ParentFileId):
                        //    iFile = Convert.ToInt32(fillerGrid.ListRecords[0].GetFields[i].Value);
                        //    break;
                        case ((int)XrmGridField.DisplayOrder):
                            iDisplayOrder = Convert.ToInt32(fillerGrid.ListRecords[0].GetFields[i].Value);
                            break;
                        case ((int)XrmGridField.ViewPermId):
                            oldViewPermId = Convert.ToInt32(fillerGrid.ListRecords[0].GetFields[i].Value);
                            break;
                        case ((int)XrmGridField.UpdatePermId):
                            oldUpdatePermId = Convert.ToInt32(fillerGrid.ListRecords[0].GetFields[i].Value);
                            break;
                        default:
                            break;
                    }
                }
            }

            //On récupere les permissions (vue/modif) de l'ancienne grille (oldPerm) et on les duplique sur la nouvelle (newPerm)
            int newPermId = 0, newUpdatePermId = 0;

            newPermId = eCheckDuplicatePerm(oldViewPermId, _dal);
            newUpdatePermId = eCheckDuplicatePerm(oldUpdatePermId, _dal);

            //On place l'affichage de la future grille après celle qui sera dupliquer
            if (iParentTabFileId > 0)
            {
                StringBuilder sqlDisplayOrder = new StringBuilder();
                sqlDisplayOrder.Append("SELECT ISNULL(MAX(DisplayOrder), 0) + 1 FROM [dbo].[XrmGrid] WHERE ParentTab = @iTab AND ParentFileId = @iFile");

                RqParam rq = new RqParam(String.Concat(sqlDisplayOrder));

                rq.AddInputParameter("@iTab", System.Data.SqlDbType.Int, iTab);
                rq.AddInputParameter("@iDisplayOrder", System.Data.SqlDbType.Int, 2);

                rq.AddInputParameter("@iFile", System.Data.SqlDbType.Int, iParentTabFileId);

                DataTableReaderTuned dtr = _dal.Execute(rq, out sError);

                if (String.IsNullOrEmpty(sError))
                {
                    dtr.Read();
                    if (!String.IsNullOrEmpty(dtr.GetString(0)))
                    {
                        iDisplayOrder = Convert.ToInt32(dtr.GetString(0));
                    }
                }
            }
            else
            {
                StringBuilder sqlDisplayOrder = new StringBuilder();
                sqlDisplayOrder.Append("SELECT @iDisplayOrder = ISNULL(MAX(DisplayOrder), 0) + 1 FROM [dbo].[XrmGrid] ")
                               .Append("WHERE ParentTab = @iTab ");

                RqParam rq = new RqParam(String.Concat(sqlDisplayOrder));

                rq.AddInputParameter("@iTab", System.Data.SqlDbType.Int, iTab);
                rq.AddInputParameter("@iDisplayOrder", System.Data.SqlDbType.Int, 2);


                _dal.ExecuteNonQuery(rq, out sError);

                if (String.IsNullOrEmpty(sError))
                {
                    iDisplayOrder = eLibTools.GetNum(rq.GetParamValue("@iDisplayOrder").ToString());
                }
            }

            #endregion Chargement des données de la grille / Load grid's data

            //On crée la nouvelle grille avec un engine
            Engine.Engine eng = eModelTools.GetEngine(_pref, (int)TableType.XRMGRID);

            //Récupération de liste des widgets de la grille à dupliquer
            EngineResult result = eCreateWithDuplication(eng, iParentTabFileId, iTab, iDisplayOrder, newPermId, newUpdatePermId, title);

            List<RefreshFieldNewValue> listWidget = new List<RefreshFieldNewValue>();

            listWidget = eGetDuplicateResult(result);

            int newGridId = listWidget[0].FileId;
            eAdminDuplicateWidget(iGrid, newGridId, iTab, iParentTabFileId, _dal, sError);
        }

        /// <summary>
        /// Permet de dupliquer une widget avec toutes ses informations
        /// Copy a widget with all his informations
        /// </summary>
        private void eAdminDuplicateWidget(int iGrid, int newGridId, int iTab, int iParentTabFileId, eudoDAL _dal, string sError)
        {
            List<int> widgetListId = new List<int>();
            widgetListId = LoadWidgets(widgetListId, iGrid, _dal, sError);

            for (int i = 0; i < widgetListId.Count; i++)
            {

                #region Chargement des données des widgets / Load widget's data

                eDataFillerGeneric fillerWidget = new eDataFillerGeneric(_pref, (int)TableType.XRMWIDGET, ViewQuery.CUSTOM);

                fillerWidget.EudoqueryComplementaryOptions = delegate (EudoQuery.EudoQuery query)
                {
                    query.SetFileId = widgetListId[i];
                    query.SetListCol = String.Concat((int)XrmWidgetField.Type, ";", (int)XrmWidgetField.ContentParam, ";",
                            (int)XrmWidgetField.ContentSource, ";", (int)XrmWidgetField.ContentType, ";", (int)XrmWidgetField.ViewPermId, ";",
                            (int)XrmWidgetField.DefaultHeight, ";", (int)XrmWidgetField.DefaultPosX, ";", (int)XrmWidgetField.DefaultPosY, ";",
                            (int)XrmWidgetField.DefaultWidth, ";", (int)XrmWidgetField.DisplayOption, ";", (int)XrmWidgetField.ManuelRefresh, ";",
                            (int)XrmWidgetField.Move, ";", (int)XrmWidgetField.Owner, ";", (int)XrmWidgetField.PictoColor, ";",
                            (int)XrmWidgetField.PictoIcon, ";", (int)XrmWidgetField.Resize, ";", (int)XrmWidgetField.ShowHeader, ";",
                            (int)XrmWidgetField.ShowWidgetToolbar, ";", (int)XrmWidgetField.SubTitle, ";", (int)XrmWidgetField.Title, ";",
                            (int)XrmWidgetField.Tooltip);
                };

                fillerWidget.Generate();

                List<eXrmWidgetFields> widgetFieldsDesc = new List<eXrmWidgetFields>();

                for (int j = 0; j < fillerWidget.ListRecords.Count; j++)
                {
                    for (int k = 0; k < fillerWidget.ListRecords[j].GetFields.Count; k++)
                    {
                        String widgetValue = fillerWidget.ListRecords[j].GetFields[k].Value;
                        if (widgetValue != "null" &&
                            widgetValue != "NULL" &&
                            widgetValue != "")
                        {
                            Field widgetField = fillerWidget.ListRecords[j].GetFields[k].FldInfo;
                            widgetFieldsDesc.Add(new eXrmWidgetFields(j, widgetField.RealName, widgetField.Descid, widgetValue));
                        }
                        else
                        {
                            Field widgetField = fillerWidget.ListRecords[j].GetFields[k].FldInfo;
                            widgetFieldsDesc.Add(new eXrmWidgetFields(j, widgetField.RealName, widgetField.Descid, ""));
                        }
                    }
                }

                int oldWidgetViewPermId = 0, newWidgetPermId = 0;

                for (int j = 0; j < widgetFieldsDesc.Count; j++)
                {
                    if (widgetFieldsDesc[j].DescId == (int)XrmWidgetField.ViewPermId && widgetFieldsDesc[j].Value != "")
                        oldWidgetViewPermId = Convert.ToInt32(widgetFieldsDesc[j].Value);
                }

                newWidgetPermId = eCheckDuplicatePerm(oldWidgetViewPermId, _dal);

                #endregion Chargement des données des widgets / Load widget's data

                //Récupération de l'id du nouveau widget
                Engine.Engine engWidget = eModelTools.GetEngine(_pref, (int)TableType.XRMWIDGET);

                EngineResult result = eCreateWithDuplication(engWidget, widgetFieldsDesc, newWidgetPermId);

                List<RefreshFieldNewValue> newWidgetId = new List<RefreshFieldNewValue>();

                newWidgetId = eGetDuplicateResult(result);

                eRequestTools tools = new eRequestTools(_context);
                eXrmWidgetContext widgetContext = new eXrmWidgetContext(newGridId, iTab, iParentTabFileId);
                eXrmEngineProxy engineProxy = new eXrmEngineProxy(_pref, iTab, newGridId, newWidgetId[0].FileId, widgetContext);

                widgetContext.GridId = newGridId;
                widgetContext.WidgetId = newWidgetId[0].FileId;

                if (newGridId <= 0)
                {
                    engineProxy.SetErrorMsg("Page d'accueil introuvable", "Id de la page d'accueil non fourni");
                    RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(engineProxy.Result); });
                    return;
                }

                engineProxy.NewWidget(tools);
                engineProxy.LinkWidgetPage();
                engineProxy.CopyTranslationsToWidget(widgetListId[i], widgetFieldsDesc[19].Value.ToString(), out sError);
                engineProxy.ReloadWidget();
                engineProxy.Result.ClientAction = (int)XrmClientWidgetAction.NEW_WIDGET;

                if (sError.Length > 0)
                {
                    ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), "Une erreur est survenue lors de la duplication du widget", devMsg: sError);
                    LaunchError();
                }

                eNewWidgetParam(widgetFieldsDesc, widgetContext, iGrid, widgetContext.GridId, widgetListId[i], widgetContext.WidgetId, iTab, iParentTabFileId, _dal, sError);

            }

        }

        /// <summary>
        /// Crée des nouveaux paramètres pour le nouveau widget
        /// Create new parameters for the new widget
        /// </summary>
        private void eNewWidgetParam(List<eXrmWidgetFields> widgetFieldsDesc, eXrmWidgetContext widgetContext, int oldGrid, int iGrid, int oldWidgetId, int iWidget, int iTab, int iParentTabFileId, eudoDAL _dal, string sError)
        {
            for (int j = 0; j < widgetFieldsDesc.Count; j++)
            {
                //Selon le type du widget, il faut changer certaines valeurs supplémentaires
                //For some type of widget, we should change value
                if (widgetFieldsDesc[j].DescId == (int)XrmWidgetField.Type && widgetFieldsDesc[j].Value != "")
                {
                    //On crée un nouveau widgetParam avec pour paramètres le nouveau widget id
                    //New widgetParam for the new widget
                    eXrmWidgetParam xrmOldWidgetParam = new eXrmWidgetParam(_pref, oldWidgetId);
                    eXrmWidgetParam xrmWidgetParam = new eXrmWidgetParam(_pref, iWidget);

                    switch (Convert.ToInt32(widgetFieldsDesc[j].Value))
                    {
                        case (int)XrmWidgetType.Indicator:
                            #region Indicator

                            #region resLocation informations

                            xrmWidgetParam.SetWidgetParams(xrmOldWidgetParam.GetWidgetParams());

                            //On crée un chemin basé sur le widget à copier pour le resLocation
                            //Create a path based on the duplicated widget for resLocation
                            string path = iTab.ToString() + "-" + iParentTabFileId.ToString() + "/" + ((int)TableType.XRMGRID).ToString() + "-" + oldGrid.ToString() + "/" + ((int)TableType.XRMWIDGET).ToString() + "-" + oldWidgetId.ToString();

                            StringBuilder sqlResLoc = new StringBuilder();
                            sqlResLoc.Append("SELECT * FROM [dbo].[RESLOCATION] WHERE ResPath LIKE ");
                            sqlResLoc.Append("'" + path + "'");

                            RqParam rq = new RqParam(String.Concat(sqlResLoc));

                            DataTableReaderTuned dtr = _dal.Execute(rq, out sError);

                            List<eResLocation> oldResLocList = new List<eResLocation>();

                            if (String.IsNullOrEmpty(sError))
                            {
                                while (dtr.Read())
                                {
                                    oldResLocList.Add(new eResLocation(Convert.ToInt32(dtr.GetString(0)), dtr.GetString(1), dtr.GetString(3)));
                                }
                            }
                            #endregion resLocation informations

                            //Cela permettra d'entrer les nouvelles données d'unit et libelle pour les nouveaux resCode
                            //It'll permits to add new value of unit and label for the new resCode
                            for (int i = 0; i < oldResLocList.Count; i++)
                            {
                                string identifier = oldResLocList[i].PathLabel;

                                #region resValue informations
                                //Récupérer le resCode du resCode à copier
                                StringBuilder sqlResCode = new StringBuilder();

                                //On a besoin de récupérer la valeur du resValue pour le nouveau resCode
                                //We need to retrieve the value of resValue for the new resCode
                                sqlResCode.Append("SELECT ResValue FROM [dbo].[RESCODE] WHERE LangId = 0 AND ResLocationId = @resLocId ");

                                RqParam rqResCode = new RqParam(String.Concat(sqlResCode));

                                rqResCode.AddInputParameter("@resLocId", System.Data.SqlDbType.Int, oldResLocList[i].IdRes.ToString());

                                DataTableReaderTuned dtrResCode = _dal.Execute(rqResCode, out sError);

                                string resValue = "";

                                if (String.IsNullOrEmpty(sError))
                                {
                                    dtrResCode.Read();
                                    {
                                        resValue = dtrResCode.GetString(0);
                                    }
                                }
                                #endregion resValue informations

                                //On crée un nouveau resLocation qui permet de lier xrmGridWidget et resCode à l'aide de la création du resCode
                                // We'll create the new resLocation  which permits to link xrmGridWidget and resCode with resCode's creation help
                                eResLocation resLoc = new eResLocation(eModelConst.ResCodeNature.WidgetParam, eResLocation.GetPathFromWidgetContext(widgetContext), identifier);

                                //On crée un nouveau resCode / We create a new resCode
                                eResCode resCode = new eResCode(_pref, _dal, 0, resValue, resLoc);
                                resCode.CreateNewResCode();

                                xrmWidgetParam.SetParam(identifier, resCode.GetAlias());//"<#[" + resCode.Code.ToString() + "]#>");

                            }

                            //On sauvegarde les nouveaux paramètres dans le widgetParam / We save new parameters in widgetParam
                            xrmWidgetParam.Save(out sError);
                            if (sError.Length > 0)
                            {
                                ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), "Une erreur est survenue lors de la duplication des paramètres de l'interface", devMsg: sError);
                                LaunchError();
                            }

                            #endregion Indicator
                            break;
                        case (int)XrmWidgetType.Kanban:
                            //ALISTER => Demande / Request 80479
                            #region Kanban
                            eDuplicateWidgetFilter(xrmOldWidgetParam, xrmWidgetParam, _dal, sError);
                            #endregion Kanban
                            break;
                        default:
                            //ALISTER => Pour les autres widgets, il n'y a pas besoin d'entrée des nouvelles valeurs
                            //il suffit de les dupliquer
                            //For others widget, we don't need to set new parameters, just duplicate them

                            xrmWidgetParam.SetWidgetParams(xrmOldWidgetParam.GetWidgetParams());
                            xrmWidgetParam.Save(out sError);

                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Duplique les permissions
        /// Duplicate rights
        /// </summary>
        private int eCheckDuplicatePerm(int oldPermId, eudoDAL _dal)
        {
            if (oldPermId > 0)
            {
                ePermission oldPerm = new ePermission(oldPermId, _dal, _pref);
                ePermission newPerm = new ePermission(0, _dal, _pref);

                newPerm = oldPerm.Clone(_dal);

                return newPerm.PermId;
            }

            return 0;

        }

        /// <summary>
        /// Crée les informations du nouvel élément avec les informations de l'élément copié (pour Grille)
        /// Create informations of the new element with informations of the duplicated element (for Grid)
        /// </summary>
        private EngineResult eCreateWithDuplication(Engine.Engine eng, int iParentTabFileId, int iTab, int iDisplayOrder, int newPermId, int newUpdatePermId, String title)
        {
            eng.FileId = 0;
            eng.Tab = (int)TableType.XRMGRID;

            //ALISTER Demande/Request 95004, Si il est plus grand que 0, cela correspond à une page d'accueil
            //If it's greater than 0, it matches to homepage
            if (iParentTabFileId > 0)
                eng.AddNewValue((int)XrmGridField.ParentFileId, iParentTabFileId.ToString());
            //else
            //    eng.AddNewValue((int)XrmGridField.ParentFileId, "NULL");

            eng.AddNewValue((int)XrmGridField.ParentTab, iTab.ToString());
            eng.AddNewValue((int)XrmGridField.DisplayOrder, iDisplayOrder.ToString());

            if (newPermId > 0)
                eng.AddNewValue((int)XrmGridField.ViewPermId, newPermId.ToString());

            if (newUpdatePermId > 0)
                eng.AddNewValue((int)XrmGridField.ViewPermId, newUpdatePermId.ToString());

            eng.AddNewValue((int)XrmGridField.Title, title);

            ResultStrategy resStrategy = new ResultXrmCru();
            eng.EngineProcess(new StrategyCruSimple(), resStrategy);

            return eng.Result;
        }

        /// <summary>
        /// Crée les informations du nouvel élément avec les informations de l'élément copié (pour Widget)
        /// Create informations of the new element with informations of the duplicated element (for Widget)
        /// </summary>
        private EngineResult eCreateWithDuplication(Engine.Engine eng, List<eXrmWidgetFields> widgetFieldsDesc, int newPermId)
        {
            eng.FileId = 0;
            eng.Tab = (int)TableType.XRMWIDGET;

            eng.AddNewValue((int)XrmWidgetField.Type, widgetFieldsDesc[0].Value.ToString());
            eng.AddNewValue((int)XrmWidgetField.ContentParam, widgetFieldsDesc[1].Value.ToString());
            eng.AddNewValue((int)XrmWidgetField.ContentSource, widgetFieldsDesc[2].Value.ToString());
            eng.AddNewValue((int)XrmWidgetField.ContentType, widgetFieldsDesc[3].Value.ToString());

            if (newPermId > 0)
                eng.AddNewValue((int)XrmWidgetField.ViewPermId, newPermId.ToString());

            eng.AddNewValue((int)XrmWidgetField.DefaultHeight, widgetFieldsDesc[5].Value.ToString());
            eng.AddNewValue((int)XrmWidgetField.DefaultPosX, widgetFieldsDesc[6].Value.ToString());
            eng.AddNewValue((int)XrmWidgetField.DefaultPosY, widgetFieldsDesc[7].Value.ToString());
            eng.AddNewValue((int)XrmWidgetField.DefaultWidth, widgetFieldsDesc[8].Value.ToString());
            eng.AddNewValue((int)XrmWidgetField.DisplayOption, widgetFieldsDesc[9].Value.ToString());
            eng.AddNewValue((int)XrmWidgetField.ManuelRefresh, widgetFieldsDesc[10].Value.ToString());
            eng.AddNewValue((int)XrmWidgetField.Move, widgetFieldsDesc[11].Value.ToString());
            eng.AddNewValue((int)XrmWidgetField.Owner, widgetFieldsDesc[12].Value.ToString());
            eng.AddNewValue((int)XrmWidgetField.PictoColor, widgetFieldsDesc[13].Value.ToString());
            eng.AddNewValue((int)XrmWidgetField.PictoIcon, widgetFieldsDesc[14].Value.ToString());
            eng.AddNewValue((int)XrmWidgetField.Resize, widgetFieldsDesc[15].Value.ToString());
            eng.AddNewValue((int)XrmWidgetField.ShowHeader, widgetFieldsDesc[16].Value.ToString());
            eng.AddNewValue((int)XrmWidgetField.ShowWidgetToolbar, widgetFieldsDesc[17].Value.ToString());
            eng.AddNewValue((int)XrmWidgetField.SubTitle, widgetFieldsDesc[18].Value.ToString());
            eng.AddNewValue((int)XrmWidgetField.Title, widgetFieldsDesc[19].Value.ToString());
            eng.AddNewValue((int)XrmWidgetField.Tooltip, widgetFieldsDesc[20].Value.ToString());

            ResultStrategy resStrategy = new ResultXrmCru();
            eng.EngineProcess(new StrategyCruSimple(), resStrategy);

            return eng.Result;
        }

        /// <summary>
        /// Récupère les informations du nouvel élément existant
        /// Retrieve informations of the new existing element
        /// </summary>
        private List<RefreshFieldNewValue> eGetDuplicateResult(EngineResult result)
        {
            List<RefreshFieldNewValue> listResult = new List<RefreshFieldNewValue>();

            foreach (ListRefreshFieldNewValue refreshFieldNewValue in result.ListRefreshFields)
            {
                listResult = refreshFieldNewValue.List;
            }

            return listResult;
        }

        /// <summary>
        /// Charge la liste des widgets
        /// Load widgets list
        /// </summary>
        private List<int> LoadWidgets(List<int> widgetList, int iGrid, eudoDAL _dal, String sError)
        {
            String error = String.Empty;
            String sql = String.Concat(" SELECT [XrmWidgetId] FROM [XrmGridWidget] WHERE [XrmGridId] = @fileId ");

            RqParam rqSql = new RqParam(sql);
            rqSql.AddInputParameter("@fileId", SqlDbType.Int, iGrid);
            DataTableReaderTuned dtr = _dal.Execute(rqSql, out sError);
            try
            {
                if (error.Length != 0)

                    if (dtr == null)
                        return widgetList;

                widgetList = new List<Int32>();

                while (dtr.Read())
                    widgetList.Add(dtr.GetEudoNumeric(0));
            }
            finally
            {
                dtr?.Dispose();
            }

            return widgetList;
        }


        private void eDuplicateWidgetFilter(eXrmWidgetParam xrmOldWidgetParam, eXrmWidgetParam xrmWidgetParam, eudoDAL _dal, string sError)
        {
            //"identifier" pour les filtres c'est filterid / "identifier" for filters is filterid
            String filterId = "filterid";

            //On crée un nouveau filtre avec les données de l'ancien / We create a new filter with old data
            eSpecifFilter newFilter = new eSpecifFilter(_pref, _dal, Convert.ToInt32(xrmOldWidgetParam.GetParamValue(filterId)), out sError);
            newFilter.FilterId = 0;

            //On clone les permissions de ViewPermId et UpdatePermId / We clone permissions of ViewPermId et UpdatePermId
            if (newFilter.ViewPermId > 0)
            {
                eSpecifPermission pView = new eSpecifPermission(newFilter.ViewPermId, _dal);
                eSpecifPermission pViewClone = new eSpecifPermission();
                pViewClone.PermLevel = pView.PermLevel;
                pViewClone.PermMode = pView.PermMode;
                pViewClone.PermUser = pView.PermUser;
                pViewClone.Save(_dal);

                newFilter.ViewPermId = pViewClone.PermId;
            }

            if (newFilter.UpdatePermId > 0)
            {
                eSpecifPermission pUpdate = new eSpecifPermission(newFilter.UpdatePermId, _dal);
                pUpdate.Save(_dal);
                newFilter.ViewPermId = pUpdate.PermId;
            }

            //On sauvegarde le nouveau filtre / We save the new filter
            newFilter.Save(_dal);

            //On l'attribue au widget dupliqué / We set to the duplicated widget
            xrmWidgetParam.SetWidgetParams(xrmOldWidgetParam.GetWidgetParams());
            xrmWidgetParam.SetParam(filterId, newFilter.FilterId.ToString());

            xrmWidgetParam.Save(out sError);
        }

    }


}