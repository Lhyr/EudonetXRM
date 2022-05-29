using System;
using System.Xml;
using EudoQuery;
using Com.Eudonet.Internal;
using System.Text;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Gestion des uservalue
    /// </summary>
    public class eUserValueManager : eEudoManager
    {
        private Int32 _tab = 0;
        private eudoDAL _dal;
        private TypeUserValue _type = TypeUserValue.FILTER_QUICK;
        private ExtendedDictionary<String, String> _requestDic = new ExtendedDictionary<String, String>();

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            Int32 typ = 0;
            String errorDescription = String.Empty;
            Boolean updateSucess = false;

            #region Variables de session
            Int32 _groupId = _pref.User.UserGroupId;
            Int32 _userLevel = _pref.User.UserLevel;


            String _lang = _pref.Lang;

            Int32 _userId = _pref.User.UserId;
            String _instance = _pref.GetSqlInstance;
            String _baseName = _pref.GetBaseName;

            if (_userId == 0)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 6237),
                    eResApp.GetRes(_pref, 2024).Replace(" <PARAM> ", " "),
                    eResApp.GetRes(_pref, 72),
                    String.Concat(eResApp.GetRes(_pref, 2024).Replace("<PARAM>", " "), " (userId = 0)")
                );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }

            #endregion

            // Param Obligatoire
            if (!_allKeys.Contains("tab")
                    || !_allKeys.Contains("type")
                    || !Int32.TryParse(_context.Request.Form["tab"].ToString(), out _tab)
                    || !Int32.TryParse(_context.Request.Form["type"].ToString(), out typ)
                    || _tab <= 0)
            {
                StringBuilder sbAdditionalDevMsg = new StringBuilder();
                if (_context.Request.Form["tab"] != null)
                    sbAdditionalDevMsg.Append("tab = ").Append(_context.Request.Form["tab"].ToString());
                if (_context.Request.Form["type"] != null)
                    sbAdditionalDevMsg.Append("type = ").Append(_context.Request.Form["type"].ToString());

                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 6237),
                    eResApp.GetRes(_pref, 2024).Replace(" <PARAM> ", " "),
                    eResApp.GetRes(_pref, 72),
                    String.Concat(eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "tab, type"), " (", sbAdditionalDevMsg.ToString(), ")")
                );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }

            //Autres paramètres
            _type = (EudoQuery.TypeUserValue)typ;

            // Init le document XML
            XmlDocument xmlResult = new XmlDocument();
            XmlNode _mainNode = xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlResult.AppendChild(_mainNode);
            XmlNode detailsNode;
            detailsNode = xmlResult.CreateElement("uservaluemanager");
            xmlResult.AppendChild(detailsNode);

            try
            {
                _dal = eLibTools.GetEudoDAL(_pref);
                _dal.OpenDatabase();

                // Chargement de la collection du form
                String _prefValue = String.Empty;
                foreach (String prefFld in _allKeys)
                {
                    _prefValue = _context.Request.Form[prefFld].ToString();
                    _requestDic.AddOrUpdateValue(prefFld.ToLower(), _prefValue, false);
                }

                // Lance la mise à jour
                updateSucess = UpdateUserValue(out errorDescription);
            }
            catch (Exception ex)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 6237),
                    ex.Message,
                    eResApp.GetRes(_pref, 72),
                    ex.StackTrace
                );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }
            finally
            {
                _dal.CloseDatabase();
            }

            XmlNode _resultNode = xmlResult.CreateElement("result");
            if (updateSucess)
            {
                _resultNode.InnerText = "SUCCESS";
                detailsNode.AppendChild(_resultNode);
            }
            else
            {

                _resultNode.InnerText = "ERROR";
                detailsNode.AppendChild(_resultNode);

                if (!String.IsNullOrEmpty(errorDescription))
                {
                    XmlNode _errDesc = xmlResult.CreateElement("errordescription");
                    _errDesc.InnerText = errorDescription;
                    detailsNode.AppendChild(_errDesc);
                }

                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 6237),
                    errorDescription,
                    eResApp.GetRes(_pref, 72),
                    errorDescription
                );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }

            RenderResult(RequestContentType.XML, delegate() { return xmlResult.OuterXml; });
        }
        /// <summary>
        /// Met à jour une uservalue
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        private Boolean UpdateUserValue(out String error)
        {
            error = String.Empty;

            RqParam rq = new RqParam();
            String sqlClean = String.Empty;
            String sqlUpdate = String.Empty;
            String sqlInsert = String.Empty;

            //TODO : gérer les requêtes SQL dans eUserValue (eudoInternal)
            switch (_type)
            {
                #region Recherche champ principal mode liste

                case TypeUserValue.SEARCH_MAINFIELD:
                    String mainFieldValue = _requestDic["value"];

                    sqlUpdate = "UPDATE [USERVALUE] SET [VALUE] = @value WHERE [TAB] = @TAB AND [USERID] = @USERID AND [TYPE] = @TYPE";
                    sqlInsert = "INSERT INTO [USERVALUE] ( [VALUE], [TAB], [USERID], [TYPE] ) VALUES (@VALUE, @TAB,  @USERID , @TYPE)";

                    if (String.IsNullOrEmpty(mainFieldValue))
                        rq.AddInputParameter("@value", System.Data.SqlDbType.VarChar, null);
                    else
                        rq.AddInputParameter("@value", System.Data.SqlDbType.NVarChar, mainFieldValue);

                    rq.AddInputParameter("@USERID", System.Data.SqlDbType.Int, _pref.User.UserId);
                    rq.AddInputParameter("@TAB", System.Data.SqlDbType.Int, _tab);
                    rq.AddInputParameter("@TYPE", System.Data.SqlDbType.Int, TypeUserValue.SEARCH_MAINFIELD.GetHashCode());

                    // RAZ compteur filtre
                    _pref.Context.Paging.resetInfo();

                    break;

                #endregion

                #region [Mode fiche] - Sauvegarde de la minimisation de l'entête de la fiche.

                case TypeUserValue.FILE_MINIMIZE:

                    String sValue = _requestDic["value"];
                    Boolean bIsEnabled = (sValue == "1");


                    sqlUpdate = "UPDATE [USERVALUE] SET [ENABLED] = @value WHERE [TAB] = @TAB AND [USERID] = @USERID AND [TYPE] = @TYPE";
                    sqlInsert = "INSERT INTO [USERVALUE] ( [ENABLED], [TAB], [USERID], [TYPE] ) VALUES (@VALUE, @TAB,  @USERID , @TYPE)";

                    rq.AddInputParameter("@value", System.Data.SqlDbType.Bit, bIsEnabled);
                    rq.AddInputParameter("@USERID", System.Data.SqlDbType.Int, _pref.User.UserId);
                    rq.AddInputParameter("@TAB", System.Data.SqlDbType.Int, _tab);
                    rq.AddInputParameter("@TYPE", System.Data.SqlDbType.Int, _type.GetHashCode());

                    // RAZ compteur filtre
                    _pref.Context.Paging.resetInfo();

                    break;

                #endregion

                #region Memorisation de l'état d'ouverture du titre séparateur

                case TypeUserValue.SEPARATOR_SORT:

                    Int32 iDescid = 0;
                    Int32 iEnabled = 0;

                    if (!Int32.TryParse(_requestDic["descid"], out iDescid))
                    {
                        StringBuilder sbAdditionalDevMsg = new StringBuilder();
                        if (_context.Request.Form["descid"] != null)
                            sbAdditionalDevMsg.Append("descid = ").Append(_context.Request.Form["descid"].ToString());

                        ErrorContainer = eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 6237),
                            eResApp.GetRes(_pref, 2024).Replace(" <PARAM> ", " "),
                            eResApp.GetRes(_pref, 72),
                            String.Concat(eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "descid"), " (", sbAdditionalDevMsg.ToString(), ")")
                        );

                        //Arrete le traitement et envoi l'erreur
                        LaunchError();
                    }

                    if (!Int32.TryParse(_requestDic["enabled"], out iEnabled) || (iEnabled != 1 && iEnabled != 0))
                    {
                        ErrorContainer = eErrorContainer.GetDevUserError(
                                          eLibConst.MSG_TYPE.CRITICAL,
                                          eResApp.GetRes(_pref, 72), // "Une erreur est survenue"
                                          "",
                                          eResApp.GetRes(_pref, 416), //"Erreur"
                                          eResApp.GetRes(_pref, 6612) //"Paramètre activé invalide"
                      );

                        //Arrete le traitement et envoi l'erreur
                        LaunchError();
                    }

                    sqlInsert = "INSERT INTO [USERVALUE]([Tab],[DescId],[UserId],[Type],[Enabled]) SELECT @TAB, @DESCID, @USERID, @TYPE, @ENABLED WHERE NOT EXISTS (SELECT [UserValueId] FROM [USERVALUE] WHERE [Tab] = @TAB AND [DescId] = @DESCID AND [UserId] = @USERID AND [Type] = @TYPE)";
                    sqlUpdate = "UPDATE [USERVALUE] SET [Enabled] = @ENABLED WHERE [Tab] = @TAB AND [DescId] = @DESCID AND [UserId] = @USERID AND [Type] = @TYPE";


                    rq.AddInputParameter("@USERID", System.Data.SqlDbType.Int, _pref.User.UserId);
                    rq.AddInputParameter("@TAB", System.Data.SqlDbType.Int, _tab);
                    rq.AddInputParameter("@TYPE", System.Data.SqlDbType.Int, _type.GetHashCode());
                    rq.AddInputParameter("@DESCID", System.Data.SqlDbType.Int, iDescid);
                    rq.AddInputParameter("@ENABLED", System.Data.SqlDbType.Int, iEnabled);



                    break;

                #endregion

                #region Filtre rapides
                case TypeUserValue.FILTER_QUICK:

                    // Mode administration
                    Boolean modeAdmin = _requestDic.ContainsKey("adm");

                    String quickValue = _requestDic["value"];
                    Int32 quickDescid = eLibTools.GetNum(_requestDic["descid"]);
                    Int32 quickIndex = eLibTools.GetNum(_requestDic["index"]);

                    if (modeAdmin)
                    {
                        sqlClean = "DELETE FROM [USERVALUE] WHERE isnull([type], 0) = @TYPE AND [tab] = @TAB AND isnull([index], 0) = @INDEX AND [userid] IS NOT NULL";

                        sqlUpdate = "UPDATE [USERVALUE] SET [descid] = @DESCID, [value] = @VALUE WHERE isnull([type], 0) = @TYPE AND [tab] = @TAB AND [userid] IS NULL AND [index] = @INDEX";
                        sqlInsert = "INSERT INTO [USERVALUE] ([value], [type], [tab], [descid], [userid], [index], [label]) VALUES (@VALUE, @TYPE, @TAB, @DESCID, NULL, @INDEX, @LABEL)";

                        // Valeur
                        rq.AddInputParameter("@value", System.Data.SqlDbType.VarChar, null);

                    }
                    else
                    {
                        sqlUpdate = "UPDATE [USERVALUE] SET [value] = @VALUE WHERE isnull([type], 0) = @TYPE AND [tab] = @TAB AND [descid] = @DESCID AND [userid] = @USERID AND [index] = @INDEX";
                        sqlInsert = "INSERT INTO [USERVALUE] ([value], [type], [tab], [descid], [userid], [index], [label]) VALUES (@VALUE, @TYPE, @TAB, @DESCID, @USERID, @INDEX, @LABEL)";
                        // Valeur
                        if (String.IsNullOrEmpty(quickValue))
                            rq.AddInputParameter("@value", System.Data.SqlDbType.VarChar, "");
                        else if (quickValue.Equals("-1"))
                            rq.AddInputParameter("@value", System.Data.SqlDbType.VarChar, null);
                        else
                            rq.AddInputParameter("@value", System.Data.SqlDbType.VarChar, quickValue);
                    }

                    rq.AddInputParameter("@TYPE", System.Data.SqlDbType.Int, TypeUserValue.FILTER_QUICK.GetHashCode());
                    rq.AddInputParameter("@TAB", System.Data.SqlDbType.Int, _tab);
                    rq.AddInputParameter("@DESCID", System.Data.SqlDbType.Int, quickDescid);
                    rq.AddInputParameter("@USERID", System.Data.SqlDbType.Int, _pref.User.UserId);
                    rq.AddInputParameter("@INDEX", System.Data.SqlDbType.Int, quickIndex);
                    rq.AddInputParameter("@LABEL", System.Data.SqlDbType.VarChar, _requestDic.ContainsKey("label") ? _requestDic["label"] : null);

                    // RAZ compteur filtre
                    _pref.Context.Paging.resetInfo();

                    break;

                #endregion


                case TypeUserValue.FILEDATA_SORT:
                    #region Tri sur filtre avancé

                    sqlUpdate = "UPDATE [USERVALUE] SET [value] = @VALUE WHERE isnull([type], 0) = @TYPE AND [tab] = @TAB AND [descid] = @DESCID AND [userid] = @USERID";
                    sqlInsert = "INSERT INTO [USERVALUE] ([value], [type], [tab], [descid], [userid]) VALUES (@VALUE, @TYPE, @TAB, @DESCID, @USERID)";
                    sqlClean = "DELETE FROM [USERVALUE] WHERE [TYPE] = @TYPE AND [USERID] = @USERID AND [DESCID] = @DESCID";


                    String sVal = _requestDic["sort"];

                     Int32 nDescid;
                     Int32.TryParse(_requestDic["descid"], out nDescid);

                    
                    String sMask = string.Empty;
                    String sMaskOrder = string.Empty;

                    rq.AddInputParameter("@TYPE", System.Data.SqlDbType.Int, TypeUserValue.FILEDATA_SORT.GetHashCode());
                    rq.AddInputParameter("@TAB", System.Data.SqlDbType.Int, _tab);
                    rq.AddInputParameter("@DESCID", System.Data.SqlDbType.Int, 0);
                    rq.AddInputParameter("@USERID", System.Data.SqlDbType.Int, _pref.User.UserId);


                    rq.AddInputParameter("@value", System.Data.SqlDbType.VarChar, sMask);
                    rq.AddInputParameter("@LABEL", System.Data.SqlDbType.VarChar, sMaskOrder);


                    break;

                    #endregion

                default:
                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 6237),
                        eResApp.GetRes(_pref, 6613),
                        eResApp.GetRes(_pref, 72),
                        String.Concat(eResApp.GetRes(_pref, 6613), " - UpdateUserValue : ", _type)
                    );

                    //Arrete le traitement et envoi l'erreur
                    LaunchError();
                    break;
            }

            if (!String.IsNullOrEmpty(sqlClean))
            {
                rq.SetQuery(sqlClean);

                _dal.ExecuteNonQuery(rq, out error);

                if (!String.IsNullOrEmpty(error))
                    return false;
            }

            Int32 nbRes = 0;

            if (!String.IsNullOrEmpty(sqlUpdate))
            {
                rq.SetQuery(sqlUpdate);

                nbRes = _dal.ExecuteNonQuery(rq, out error);

                if (!String.IsNullOrEmpty(error))
                    return false;

                if (nbRes != 0)
                    return true;
            }

            if (!String.IsNullOrEmpty(sqlInsert))
            {
                rq.SetQuery(sqlInsert);

                nbRes = _dal.ExecuteNonQuery(rq, out error);

                if (String.IsNullOrEmpty(error) && nbRes == 1)
                    return true;
            }

            return false;
        }
    }
}