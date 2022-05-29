using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Result.Xrm;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.eda;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.CommonDTO;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet proxy permetant d'interagir avec engine pour mise a jour de la base
    /// </summary>
    public class eXrmEngineProxy
    {
        private Engine.Engine _engine;
        private eXrmWidgetJsonResult _result;
        private int _gridId;
        private int _tab;
        private int _widgetId;
        private ePref _pref;
        private eXrmWidgetContext _context;

        /// <summary>
        /// Constructeur du proxy
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="tab">The tab.</param>
        /// <param name="gridId">Id de la grid</param>
        /// <param name="widgetId">The widget identifier.</param>
        /// <param name="context">The context.</param>
        public eXrmEngineProxy(ePref pref, int tab, int gridId, int widgetId, eXrmWidgetContext context)
        {
            _pref = pref;
            _tab = tab;
            _gridId = gridId;
            _widgetId = widgetId;
            _context = context;

            // Pas d'erreur par défaut
            _result = new eXrmWidgetJsonResult();
            _result.Success = true;
            _result.GridId = _gridId;
            _result.WidgetId = _widgetId;
        }

        /// <summary>
        /// Permet de créer un nouveau widget via Engine
        /// </summary>
        /// <param name="tools"></param>
        public void NewWidget(eRequestTools tools)
        {
            if (!_result.Success)
                return;

            if (_widgetId == 0)
            {
                UpdateWidget(tools);
            }
            else
            {
                _result.Success = true;
                _result.GridId = _gridId;
                _result.WidgetId = _widgetId;
#if DEBUG
                _result.DebugMsg = "Widget existe déjà";
#endif
            }
        }

        /// <summary>
        /// Mise a jour du widget en cours
        /// </summary>
        /// <param name="tools"></param>
        public void UpdateWidget(eRequestTools tools)
        {
            if (!_result.Success)
                return;

            string title = eResApp.GetRes(_pref, 8160);

            title = tools.GetRequestFormKeyS("title");
            title = (_widgetId > 0) ? title : eResApp.GetRes(_pref, 8160);

            // position de la widget
            int x = tools.GetRequestFormKeyI("posx") ?? 0;
            int y = tools.GetRequestFormKeyI("posy") ?? 0;

            // Largeur et hauteur du widget en unité
            int height = tools.GetRequestFormKeyI("height") ?? 1;
            int width = tools.GetRequestFormKeyI("width") ?? 1;

            XrmWidgetType widgetType;

            if (!Enum.TryParse((tools.GetRequestFormKeyI("type") ?? 0).ToString(), out widgetType))
                widgetType = XrmWidgetType.Image;

            _engine = eModelTools.GetEngine(_pref, _tab, eEngineCallContext.GetCallContext(Common.Enumerations.EngineContext.APPLI));
#if DEBUG
            _engine.ModeDebug = true;
#endif
            _engine.FileId = _widgetId;

            if (title != null)
                _engine.AddNewValue((int)XrmWidgetField.Title, title, true);

            if (_widgetId == 0)
            {
                _engine.AddNewValue((int)XrmWidgetField.Type, ((int)widgetType).ToString(), true);
                _engine.AddNewValue((int)XrmWidgetField.Resize, "1", true);
                _engine.AddNewValue((int)XrmWidgetField.Move, "1", true);
                _engine.AddNewValue((int)XrmWidgetField.ManuelRefresh, "1", true);
            }

            _engine.AddNewValue((int)XrmWidgetField.DefaultPosX, x.ToString(), true);
            _engine.AddNewValue((int)XrmWidgetField.DefaultPosY, y.ToString(), true);
            _engine.AddNewValue((int)XrmWidgetField.DefaultWidth, width.ToString(), true);
            _engine.AddNewValue((int)XrmWidgetField.DefaultHeight, height.ToString(), true);
            //_engine.AddNewValue((int)XrmWidgetField.Owner, _pref.UserId.ToString(), true);

            _engine.EngineProcess(new StrategyCruSimple(), new ResultXrmCru());

            if (_engine.Result.Success)
            {
                _result.Success = true;
                _result.GridId = _gridId;
                // on a un seul widget crée à la fois, on prend le premier
                if (_widgetId == 0)
                {
                    _widgetId = _engine.Result.NewRecord.FilesId[0];
                    _result.WidgetId = _widgetId;
                }
            }
            else
            {
                SetErrorResult(_engine.Result);
            }
        }

        internal void DeleteAssociedSpecif(int sid)
        {
            if (!_result.Success)
                return;



            if (sid > 0)
            {
                bool isSpecifUsed = false;
                using (eudoDAL dal = eLibTools.GetEudoDAL(_pref))
                {
                    try
                    {
                        string sError = "";
                        dal.OpenDatabase();
                        RqParam r = new RqParam("select count(1) from XrmWidget where ContentSource = @spcid and [type]=@tp and [XrmWidgetId]<>@wid");
                        r.AddInputParameter("@spcid", SqlDbType.VarChar, sid.ToString());
                        r.AddInputParameter("@tp", SqlDbType.Int, (int)XrmWidgetType.Specif);
                        r.AddInputParameter("@wid", SqlDbType.Int, _widgetId);
                        isSpecifUsed = dal.ExecuteScalar<int>(new RqParam(""), out sError) > 0;

                    }
                    catch (Exception e)
                    {
                        _result.Success = false;
                        //  _result.ErrorMsg = e.Message;
                    }
                    finally
                    {
                        dal.CloseDatabase();
                    }
                }

                if (isSpecifUsed)
                {
                    SpecifTreatmentResult result = eSpecif.DeleteSpecif(_pref, _tab, sid);
                    if (!result.Success)
                    {
                        _result.Success = false;
                        _result.ErrorMsg = result.ErrorMessage;
                    }
                }
            }
        }

        /// <summary>
        /// Mise a jour de la propriété Visible du widgetPref
        /// </summary>
        /// <param name="tools"></param>
        public void SaveVisiblePref(eRequestTools tools)
        {
            if (!_result.Success)
                return;

            string widgetIds = tools.GetRequestFormKeyS("ids");
            if (widgetIds == null)
                widgetIds = string.Empty;

            var lst = widgetIds.SplitToInt(";");
            // On se limite a 1000 widget par grille
            if (lst.Length > 1000)
                throw new ArgumentException(" Le nombre maximal de widget par grille est atteint ! count = " + 1000);


            string error;
            string savePref = @"                

                MERGE INTO xrmWidgetPref AS TARGET
                USING (                  
	                SELECT  
                        gw.Id as XrmGridWidgetId,
	                    wp.UserId, 
	                    isnull(wp.PosX, w.DefaultPosX) PosX,
	                    isnull(wp.PosY, w.DefaultPosY) PosY,
	                    isnull(wp.Width, w.DefaultWidth) Width ,
	                    isnull(wp.Height, w.DefaultHeight) Height,
	                    w.DefaultPosX ,
	                    w.DefaultPosY, 
	                    w.DefaultWidth,
	                    w.DefaultHeight,   
	                    isnull(w.DisplayOption, @defaultVisible) DisplayOption,	
	                    (CASE WHEN(SelectedWidget.XrmWidgetId IS NULL) THEN  0 ELSE 1 end) Visible		 
	                FROM [XrmWidget] w
	                INNER JOIN  [XrmGridWidget] gw on w.XrmWidgetId = gw.[XrmWidgetId] AND gw.XrmGridId = @gridId
	                LEFT JOIN XrmWidget SelectedWidget on w.XrmWidgetId = SelectedWidget.XrmWidgetId AND SelectedWidget.XrmWidgetId in (@list)	 
	                LEFT JOIN [XrmWidgetPref] wp on wp.XrmGridWidgetId =  gw.Id AND wp.UserId = @userId
                ) AS Source
                ON TARGET.XrmGridWidgetId = Source.XrmGridWidgetId AND TARGET.UserId = Source.UserId               
                WHEN MATCHED THEN 
                    UPDATE SET [Visible] =  CASE (Source.DisplayOption) WHEN @alwaysVisible THEN 1 ELSE Source.Visible END               
                WHEN NOT MATCHED BY TARGET THEN 
	                INSERT ([XrmGridWidgetId], [UserId], [PosX], [PosY], [Width], [Height], [Visible] ) 
	                VALUES (Source.XrmGridWidgetId, @userId, Source.DefaultPosX, Source.DefaultPosY, Source.DefaultWidth, Source.DefaultHeight, 
                        CASE (Source.DisplayOption) WHEN @alwaysVisible THEN 1 ELSE Source.Visible END );              
             ";

            RqParam rqSavePref;
            if (lst.Length > 0)
                rqSavePref = new RqParam(savePref.Replace("@list", eLibTools.Join(",", lst)));
            else
                // Pas de widget affiché
                rqSavePref = new RqParam(savePref.Replace("@list", "null"));

            rqSavePref.AddInputParameter("@userId", SqlDbType.Int, _pref.UserId);
            rqSavePref.AddInputParameter("@gridId", SqlDbType.Int, _gridId);
            rqSavePref.AddInputParameter("@alwaysVisible", SqlDbType.Int, (int)WidgetDisplayOption.ALWAYS_VISIBLE);
            rqSavePref.AddInputParameter("@defaultVisible", SqlDbType.Int, (int)WidgetDisplayOption.DEFAULT_VISIBLE);


            using (eudoDAL dal = eLibTools.GetEudoDAL(_pref))
            {
                dal.OpenDatabase();
                dal.ExecuteNonQuery(rqSavePref, out error);
            }

            if (!string.IsNullOrEmpty(error))
            {
                _result.Success = false;
                _result.ErrorTitle = eResApp.GetRes(_pref, 72);
                _result.ErrorMsg = "Impossible d'enregistrer la sélectionne";
                _result.ClientAction = (int)XrmClientWidgetAction.DO_NOTHING;
                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, error), _pref);

#if DEBUG
                _result.DebugMsg = error;
                _result.Html = rqSavePref.GetSqlCommandText();
#endif               

            }
        }

        /// <summary>
        /// Supprime le widget sans la liaison
        /// </summary>
        public void ResetAllWidgetPref()
        {

            if (!_result.Success)
                return;

            string error = string.Empty;
            string query = @" 
             DELETE Pref FROM [XrmWidgetPref] Pref
             INNER JOIN [XrmGridWidget] link ON Pref.[XrmGridWidgetId] = link.[Id]   
             WHERE link.[XrmGridId] = @gridId 
             ";

            RqParam rq = new RqParam(query);
            rq.AddInputParameter("@gridId", SqlDbType.Int, _gridId);
            using (eudoDAL dal = eLibTools.GetEudoDAL(_pref))
            {
                dal.OpenDatabase();

                int affected = dal.ExecuteNonQuery(rq, out error);
                if (error.Length != 0)
                {
                    _result.Success = false;
                    _result.ErrorTitle = eResApp.GetRes(_pref, 72);
                    _result.ErrorMsg = "Impossible de réinitialiser les préférences utilisateurs pour les widgets de la grille";
                    _result.ClientAction = (int)XrmClientWidgetAction.DO_NOTHING;
                    eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, error), _pref);

#if DEBUG
                    _result.ErrorMsg = error;
#endif
                    return;
                }
            }

        }

        /// <summary>
        /// Supprime le widget sans la liaison
        /// </summary>
        public void DeleteWidget()
        {
            if (!_result.Success)
                return;

            _engine = eModelTools.GetEngine(_pref, _tab, eEngineCallContext.GetCallContext(Common.Enumerations.EngineContext.APPLI));
            _engine.FileId = _widgetId;
            _engine.EngineProcess(new StrategyDelSimple());

            if (!_engine.Result.Success)
                SetErrorResult(_engine.Result);
        }

        /// <summary>
        /// Créer une association entre la grille et le widget
        /// </summary>
        public void LinkWidgetPage()
        {
            if (!_result.Success)
                return;

            string error = string.Empty;
            string query = @"

            /* Verification d'existance de liaison */
            IF NOT EXISTS( SELECT 1 FROM [XrmGridWidget] WHERE [XrmWidgetId] = @widgetId AND [XrmGridId] = @gridId)  
               BEGIN
                     INSERT INTO [XrmGridWidget] ([XrmWidgetId], [XrmGridId]) SELECT  @widgetId, @gridId;
                     SELECT SCOPE_IDENTITY();
               END
            ELSE
               BEGIN
                     /* On retourne la symétrie de son id */
                     SELECT (-1 * [Id]) FROM [XrmGridWidget] WHERE [XrmWidgetId] = @widgetId AND [XrmGridId] = @gridId;
               END;            
            ";

            RqParam rq = new RqParam(query);
            rq.AddInputParameter("@widgetId", SqlDbType.Int, _widgetId);
            rq.AddInputParameter("@gridId", SqlDbType.Int, _gridId);

            using (eudoDAL dal = eLibTools.GetEudoDAL(_pref))
            {
                dal.OpenDatabase();

                int id = dal.ExecuteScalar<int>(rq, out error);
                if (error.Length != 0)
                {
                    _result.Success = false;
                    _result.ErrorTitle = eResApp.GetRes(_pref, 72);
                    _result.ErrorMsg = "Impossible de lier le widget à la grille";
                    _result.ClientAction = (int)XrmClientWidgetAction.REMOVE_FROM_DOM;
                    eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, error), _pref);
#if DEBUG
                    _result.ErrorMsg = error;
#endif
                    return;
                }

                _result.LinkId = Math.Abs(id);
            }
        }

        /// <summary>
        /// Supprime l'association entre la page et le widget
        /// Utilisateur en cours doit avoir des les droits
        /// </summary>
        public void UnlinkWidgetPage()
        {
            if (!_result.Success)
                return;

            string error = string.Empty;
            string query = @"
             /* On supprime toutes les prefs des autres utilisateurs utilisant la meme page avec le meme widget */
             DELETE Pref FROM [XrmWidgetPref] Pref
             INNER JOIN [XrmGridWidget] link ON Pref.[XrmGridWidgetId] = link.[Id]   
             WHERE link.[XrmWidgetId] = @widgetId AND link.[XrmGridId] = @gridId

             /* suppression de la liaison */
             DELETE x FROM [XrmGridWidget] x WHERE [XrmWidgetId] = @widgetId AND [XrmGridId] = @gridId 
           ";

            RqParam rq = new RqParam(query);
            rq.AddInputParameter("@widgetId", SqlDbType.Int, _widgetId);
            rq.AddInputParameter("@gridId", SqlDbType.Int, _gridId);

            using (eudoDAL dal = eLibTools.GetEudoDAL(_pref))
            {
                dal.OpenDatabase();

                int affected = dal.ExecuteNonQuery(rq, out error);
                if (error.Length != 0)
                {
                    _result.Success = false;
                    _result.ErrorMsg = "Suppression de liaison impossible";
#if DEBUG
                    _result.ErrorMsg = error;
#endif
                    return;
                }

            }
        }

        /// <summary>
        /// Sauvegarde les prefs du widget en cours en deux etape
        /// 1- recup l'id de la pref si existe
        /// 2- mettre a jour la pref
        /// </summary>
        /// <param name="tools"></param>
        public void SaveWidgetPref(eRequestTools tools)
        {
            if (!_result.Success)
                return;

            // position de la widget, largeur et hauteur du widget en unité
            int x = tools.GetRequestFormKeyI("posx") ?? 0;
            int y = tools.GetRequestFormKeyI("posy") ?? 0;
            int h = tools.GetRequestFormKeyI("height") ?? 1;
            int w = tools.GetRequestFormKeyI("width") ?? 1;

            string error = string.Empty;

            // 1 - Permet la selection de l'id de liaison
            string selectLinkId = @"           
               SELECT link.[Id] linkId 
               FROM [XrmGridWidget] link
               INNER JOIN [XrmWidget] widget on widget.[XrmWidgetId] = link.[XrmWidgetId] /* Les widgets doivent exister */
               INNER JOIN [XrmGrid] grid on grid.[XrmGridId] = link.[XrmGridId] /* La page doit exister */
               WHERE link.[XrmWidgetId] = @widgetId AND link.[XrmGridId] = @gridId
            ";
            RqParam rqLinkId = new RqParam(selectLinkId);
            rqLinkId.AddInputParameter("@widgetId", SqlDbType.Int, _widgetId);
            rqLinkId.AddInputParameter("@gridId", SqlDbType.Int, _gridId);
            rqLinkId.AddInputParameter("@userId", SqlDbType.Int, _pref.UserId);


            // 2 -  Permet de mettre à jour les prefs de widget
            string savePref = @"           
            IF NOT EXISTS(SELECT 1 FROM [XrmWidgetPref] WHERE [XrmWidgetPref].[XrmGridWidgetId] = @linkId AND [XrmWidgetPref].[UserId] = @userId)                      
               INSERT INTO [XrmWidgetPref] ([XrmGridWidgetId], [UserId], [PosX], [PosY], [Width], [Height], [Visible] ) SELECT @linkId, @userId, @x, @y, @w, @h, 1;           
            ELSE              
               UPDATE [XrmWidgetPref] SET [XrmGridWidgetId]=@linkId, [UserId]=@userId, [PosX]=@x, [PosY]=@y, [Width]=@w, [Height]=@h, [Visible]= 1 
               WHERE [XrmWidgetPref].[XrmGridWidgetId] = @linkId AND [XrmWidgetPref].[UserId] = @userId;          
            ";

            RqParam rqSavePref = new RqParam(savePref);
            rqSavePref.AddInputParameter("@userId", SqlDbType.Int, _pref.UserId);
            rqSavePref.AddInputParameter("@x", SqlDbType.Int, x);
            rqSavePref.AddInputParameter("@y", SqlDbType.Int, y);
            rqSavePref.AddInputParameter("@w", SqlDbType.Int, w);
            rqSavePref.AddInputParameter("@h", SqlDbType.Int, h);

            using (eudoDAL dal = eLibTools.GetEudoDAL(_pref))
            {
                dal.OpenDatabase();

                DataTableReaderTuned dtrTuned = dal.Execute(rqLinkId, out error);
                if (error.Length != 0)
                {
                    _result.Success = false;
                    _result.ErrorTitle = "Une erreur est sourvenue !";
                    _result.ErrorMsg = eResApp.GetRes(_pref, 6236);
                    _result.ClientAction = (int)XrmClientWidgetAction.RELOAD_WIDGET;

#if DEBUG
                    _result.DebugMsg = error;
#endif
                    eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, error), _pref);
                    return;
                }

                // l'id de la laisaion
                int linkId = 0;
                if (dtrTuned.Read())
                    linkId = dtrTuned.GetEudoNumeric("linkId");

                _result.LinkId = linkId;
                if (linkId == 0)
                {
                    _result.Success = false;
                    _result.ErrorTitle = "Le widget a été supprimé ou retiré de la grille";
                    _result.ErrorMsg = "Pour plus d'information, veuillez contacter votre administrateur";
                    _result.ClientAction = (int)XrmClientWidgetAction.REMOVE_FROM_DOM;
#if DEBUG
                    _result.DebugMsg = error;
#endif
                    return;

                }


                rqSavePref.AddInputParameter("@linkId", SqlDbType.Int, linkId);
                int affected = dal.ExecuteNonQuery(rqSavePref, out error);
                if (error.Length != 0)
                {
                    _result.Success = false;
                    _result.ErrorTitle = "Impossible de sauvegarder les nouvelles propriétés du widget";
                    _result.ErrorMsg = "Les modifications ne sont pas prises en compte";
                    _result.ClientAction = (int)XrmClientWidgetAction.RELOAD_WIDGET;
#if DEBUG
                    _result.DebugMsg = error;
#endif
                    eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, error), _pref);
                    return;
                }

            }
        }

        /// <summary>
        /// Supprime les prefs du widget en cours
        /// </summary>
        public void DeleteWidgetPref()
        {
            if (!_result.Success)
                return;

            string error = string.Empty;
            string query = @" 
             DELETE Pref FROM [XrmWidgetPref] Pref
             INNER JOIN [XrmGridWidget] link ON Pref.[XrmGridWidgetId] = link.[Id]   
             WHERE link.[XrmWidgetId] = @widgetId AND link.[XrmGridId] = @gridId AND Pref.[UserId]=@userId
             ";

            RqParam rq = new RqParam(query);
            rq.AddInputParameter("@widgetId", SqlDbType.Int, _widgetId);
            rq.AddInputParameter("@gridId", SqlDbType.Int, _gridId);
            rq.AddInputParameter("@userId", SqlDbType.Int, _pref.UserId);

            using (eudoDAL dal = eLibTools.GetEudoDAL(_pref))
            {
                dal.OpenDatabase();

                int affected = dal.ExecuteNonQuery(rq, out error);
                if (error.Length != 0)
                {
                    _result.Success = false;
                    _result.ErrorTitle = "Une erreur s'est produite lors de la suppression du widget";
                    _result.ErrorMsg = eResApp.GetRes(_pref, 6236);
                    eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, error), _pref);
#if DEBUG
                    _result.ErrorMsg = error;
#endif  
                    return;
                }

            }
        }

        /// <summary>
        /// Enregistrement du paramètre du widget
        /// </summary>
        /// <param name="tools">The tools.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="value">The value.</param>
        public void SaveWidgetParam(eRequestTools tools, string param, string value)
        {
            if (String.IsNullOrEmpty(param))
            {
                _result.Success = false;
                _result.ErrorTitle = eResApp.GetRes(_pref, 8018);
                _result.ErrorMsg = String.Format(eResApp.GetRes(_pref, 8236), "paramName");
                return;
            }

            bool success = false;

            string error = string.Empty;
            Dictionary<string, string> dicParams = new Dictionary<string, string>();
            dicParams.Add(param, value);
            eudoDAL eDal = eLibTools.GetEudoDAL(_pref);
            try
            {
                eDal.OpenDatabase();
                if (eXrmWidgetParam.SaveParams(eDal, _widgetId, dicParams, out error))
                {
                    success = true;

                    // Si onglet modifié + widget de type Kanban, il faut vider quelques paramètres

                    XrmWidgetType widgetType = (XrmWidgetType)(tools.GetRequestFormKeyI("type") ?? 0);

                    List<string> paramsToDelete = new List<string>();
                    if (widgetType == XrmWidgetType.Kanban)
                    {
                        if (param == "tab")
                        {
                            paramsToDelete = new List<string>() { "catalog", "catvalues", "mapping", "filterid", "swimlanes" };
                        }

                    }
                    else if (widgetType == XrmWidgetType.Tuile)
                    {
                        if (param == "tab")
                        {
                            paramsToDelete = new List<string>() { "fileid" };
                        }
                    }

                    if (paramsToDelete.Count > 0)
                    {
                        success = eXrmWidgetParam.DeleteParams(eDal, _widgetId, paramsToDelete, out error);
                    }

                }

                if (success)
                {
                    _result.Success = true;
                }
                else
                {
                    _result.Success = false;
                    _result.ErrorTitle = eResApp.GetRes(_pref, 92);
                    _result.ErrorMsg = eResApp.GetRes(_pref, 6172);
                    _result.DebugMsg = error;

                    eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, error), _pref);
                }


            }
            catch (Exception exc)
            {
                error = String.Concat(exc.Message, Environment.NewLine, exc.StackTrace);

                _result.Success = false;
                _result.ErrorTitle = eResApp.GetRes(_pref, 92);
                _result.ErrorMsg = eResApp.GetRes(_pref, 6172);
                _result.DebugMsg = error;

                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, error), _pref);
            }
            finally
            {
                eDal.CloseDatabase();
            }
        }

        /// <summary>
        /// Suppression des paramètres pour un widget
        /// </summary>
        /// <param name="paramsList">Liste des paramètres à supprimer</param>
        public void DeleteWidgetParam(List<string> paramsList)
        {

            if (paramsList != null && paramsList.Count > 0)
            {
                string error = string.Empty;

                eudoDAL eDal = eLibTools.GetEudoDAL(_pref);
                try
                {
                    eDal.OpenDatabase();
                    if (eXrmWidgetParam.DeleteParams(eDal, _widgetId, paramsList, out error))
                    {
                        _result.Success = true;
                    }
                    else
                    {
                        _result.Success = false;
                        _result.ErrorTitle = eResApp.GetRes(_pref, 92);
                        _result.ErrorMsg = eResApp.GetRes(_pref, 6172);
                        _result.DebugMsg = error;

                        eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, error), _pref);
                    }
                }
                finally
                {
                    eDal.CloseDatabase();
                }
            }
            else
            {
                _result.Success = false;
                _result.ErrorTitle = eResApp.GetRes(_pref, 8018);
                _result.ErrorMsg = String.Format(eResApp.GetRes(_pref, 8236), "paramsList");
                return;
            }
        }

        /// <summary>
        /// Rafraichis le widget
        /// </summary>
        public void ReloadWidget()
        {

            try
            {
                HtmlGenericControl container = new HtmlGenericControl("div");

                eFile widget = eFileMain.CreateMainFile(_pref, (int)TableType.XRMWIDGET, _widgetId, -2);
                eXrmWidgetPrefCollection WidgetPrefCollection = new eXrmWidgetPrefCollection(_pref, _gridId, _widgetId);
                eXrmWidgetParam widgetParam = new eXrmWidgetParam(_pref, _widgetId);

                // rendu du contenu de la div 
                IXrmWidgetUI IWidget = new eXrmWidgetContent(eXrmWidgetFactory.GetWidgetUI(_pref, widget.Record), _pref);

                // ajout de la div pour le déplacement
                // Ajout de la div pour redimensionner
                // ajout de container
                IWidget = new eXrmWidgetWrapper(new eXrmWidgetWithResize(new eXrmWidgetWithToolbar(_pref, IWidget, (_pref.AdminMode && _tab > 0 && widgetParam.GetParamValue("noAdmin") != "1")), _pref), _pref);

                // Initilisation
                IWidget.Init(widget.Record, WidgetPrefCollection[widget.Record], widgetParam, _context);

                // Construction
                IWidget.Build(container);

                Result.Success = true;

                StringBuilder sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    using (var writer = new HtmlTextWriter(sw))
                    {
                        container.RenderControl(writer);
                        Result.Html = sb.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                _result.Success = false;
                _result.ErrorTitle = "";
                _result.ErrorMsg = "Impossible de rafraichir le widget.";
                _result.ClientAction = (int)XrmClientWidgetAction.REMOVE_FROM_DOM;
                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, ex.StackTrace), _pref);

            }
        }

        /// <summary>
        /// Création des traductions pour le widget
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void AddTranslationsToWidget()
        {
            eudoDAL eDal = eLibTools.GetEudoDAL(_pref);

            Dictionary<int, string> dicoResLang = new Dictionary<int, string>();
            for (int i = 0; i <= eLibConst.MAX_LANG; i++)
            {
                dicoResLang.Add(i, eResApp.GetRes(i, 8160));
            }

            try
            {
                eDal.OpenDatabase();
                eAdminTranslationsList.AddMissingFilesRES(eDal, TableType.XRMWIDGET.ToString(), XrmWidgetField.Title.ToString(), TableType.XRMWIDGET.ToString() + "ID", _widgetId, dicoResLang);
                eAdminTranslationsList.AddMissingFilesRES(eDal, TableType.XRMWIDGET.ToString(), XrmWidgetField.SubTitle.ToString(), TableType.XRMWIDGET.ToString() + "ID", _widgetId);
                eAdminTranslationsList.AddMissingFilesRES(eDal, TableType.XRMWIDGET.ToString(), XrmWidgetField.Tooltip.ToString(), TableType.XRMWIDGET.ToString() + "ID", _widgetId);
            }
            catch (Exception e)
            {
                _result.Success = false;
                _result.ErrorTitle = eResApp.GetRes(_pref, 92);
                _result.ErrorMsg = "Impossible de créer les traductions du widget";
                _result.DebugMsg = string.Concat("eXrmEngineProxy.AddTranslationsToWidget : ", e.Message, Environment.NewLine, e.StackTrace);
                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, _result.DebugMsg), _pref);
            }
            finally
            {
                eDal.CloseDatabase();
            }
        }

        /// <summary>
        /// Duplication des traductions pour le widget
        /// Duplication of translations for widget
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void CopyTranslationsToWidget(int oldWidgetId, string title, out String sError)
        {
            int descidTitle = (int)XrmGridField.Title;

            //Récupération de la liste des RES des widgets
            //Retrieve list of widgets's RES
            List<eSqlResFiles> listRes = eSqlResFiles.LoadRes(_pref, new List<int> { descidTitle }, oldWidgetId, _pref.LangId, out sError);

            eudoDAL eDal = eLibTools.GetEudoDAL(_pref);

            Dictionary<int, string> dicoResLang = new Dictionary<int, string>();

            //Si la ressource n'existe pas, on créera des nouvelles resources pour RES_FILES
            //If resource doesn't exist, we'll create new resources for RES_FILES
            if (listRes.Count <= 0)
            {
                for (int i = 0; i <= eLibConst.MAX_LANG; i++)
                {
                    //ALISTER => Il vaut mieux utiliser LangServId au lieu de LangId / We should use LangServId instead of LangId
                    if (i != _pref.LangServId)
                        dicoResLang.Add(i, eResApp.GetRes(i, 8160));
                    else
                        dicoResLang.Add(_pref.LangServId, title); //On a besoin du titre du widget / We need the title of widget
                }
            }
            else
            {
                for (int i = 0; i <= listRes.Count; i++)
                {
                    dicoResLang.Add(i, listRes[i].Value);
                }
            }

            try
            {
                eDal.OpenDatabase();
                eAdminTranslationsList.AddMissingFilesRES(eDal, TableType.XRMWIDGET.ToString(), XrmWidgetField.Title.ToString(), TableType.XRMWIDGET.ToString() + "ID", _widgetId, dicoResLang);
                eAdminTranslationsList.AddMissingFilesRES(eDal, TableType.XRMWIDGET.ToString(), XrmWidgetField.SubTitle.ToString(), TableType.XRMWIDGET.ToString() + "ID", _widgetId);
                eAdminTranslationsList.AddMissingFilesRES(eDal, TableType.XRMWIDGET.ToString(), XrmWidgetField.Tooltip.ToString(), TableType.XRMWIDGET.ToString() + "ID", _widgetId);
            }
            catch (Exception e)
            {
                _result.Success = false;
                _result.ErrorTitle = eResApp.GetRes(_pref, 92);
                _result.ErrorMsg = "Impossible de créer les traductions du widget";
                _result.DebugMsg = string.Concat("eXrmEngineProxy.AddTranslationsToWidget : ", e.Message, Environment.NewLine, e.StackTrace);
                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, _result.DebugMsg), _pref);
            }
            finally
            {
                eDal.CloseDatabase();
            }
        }

        /// <summary>
        /// Mét a jour l'erreur
        /// </summary>
        /// <param name="title"></param>
        /// <param name="msg"></param>
        public void SetErrorMsg(string title, string msg)
        {
            _result.Success = false;
            _result.ErrorTitle = title;
            _result.ErrorMsg = msg;

        }

        /// <summary>
        /// Mét a jour l'erreur
        /// </summary>
        /// <param name="engineResult">The engine result.</param>
        private void SetErrorResult(Engine.Result.EngineResult engineResult)
        {
            _result.Success = false;
            _result.ErrorTitle = engineResult.Error.Title;
            _result.ErrorMsg = engineResult.Error.Msg;
            _result.DebugMsg = engineResult.Error.DebugMsg;
        }

        /// <summary>
        /// Flux résultant de l'opération effectuée
        /// </summary>
        public eXrmWidgetJsonResult Result
        {
            get
            {
                return _result;
            }
        }
    }
}