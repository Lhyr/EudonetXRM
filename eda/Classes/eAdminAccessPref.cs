using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Core.Model.prefs;
using Com.Eudonet.Engine.Result;
using Com.Eudonet.Engine.Result.Xrm;
using Com.Eudonet.Common.CommonDTO;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Engine;

namespace Com.Eudonet.Xrm.eda
{

    /// <summary>
    /// Objet Metier de manipulation des préférences 
    ///  - RaZ
    ///  - Recopie
    ///  - Affectation en masse
    /// </summary>
    public class eAdminAccessPref
    {
        /// <summary>
        /// raz toutes les préférences de tous les utilisateurs
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static Boolean ResetAllUsersPref(ePref pref, out String error)
        {
            Boolean success = false;
            error = String.Empty;

            if (pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                throw new EudoAdminInvalidRightException();

            eudoDAL eDal = eLibTools.GetEudoDAL(pref);

            try
            {
                eDal.OpenDatabase();

                ResetUserPref(pref, eDal, new List<int>() { -1 });

                //List<int> list = eSqlUser.GetNonProfileUsersList(eDal);

                //foreach (int user in list)
                //{

                //}

                success = true;
            }
            catch (Exception exc)
            {
                error = exc.Message;
            }
            finally
            {
                eDal.CloseDatabase();
            }

            return success;
        }

        /// <summary>
        /// Réinitialisation des préférences des utilisateurs
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="eDal"></param>
        /// <param name="lstUser">Liste des userid a raz (-1 pour tous)</param>
        private static void ResetUserPref(ePref pref, eudoDAL eDal, List<int> lstUser)
        {



            String error = String.Empty;
            StringBuilder sbSql = new StringBuilder();

            RqParam rq = new RqParam();
            //rq.AddInputParameter("@userId", SqlDbType.Int, userID);

            String queryUser = " [UserId] NOT IN (SELECT [USER].[UserId] FROM [USER] WHERE ISNULL([USER].[IsProfile], 0) = 1)";


            if (lstUser != null && !lstUser.Contains(-1))
            {
                queryUser = string.Concat(queryUser, " AND [UserId] IN (", String.Join(",", lstUser), ")");
            }

            #region Clean de la table CONFIG

            //String tabOrderSql = "SELECT TOP 1 [SELECTID] FROM [SELECTIONS] WHERE [TAB] = 0 AND USERID = @userId AND IsNull([DEFAULTSELECTID],0) <> 0 ORDER BY [SELECTID] desc";

            sbSql.Length = 0;
            sbSql.Append("UPDATE [Config] SET [TabOrder] = NULL, [TABORDERID] = NULL WHERE " + queryUser);

            rq.SetQuery(sbSql.ToString());
            eDal.ExecuteNonQuery(rq, out error);
            if (!String.IsNullOrEmpty(error))
                throw new EudoAdminSQLException("eAdminAccessPref.ResetUserPref", error);

            #endregion

            #region Clean de la table PREF

            //String selectionsSql = "SELECT TOP 1 [SELECTID] FROM [SELECTIONS] WHERE [TAB] <> 0 AND [TAB] = [PREF].[TAB] AND USERID = @userId AND IsNull([DEFAULTSELECTID],0) <> 0 ORDER BY [SELECTID] desc";

            sbSql.Length = 0;
            sbSql.Append("UPDATE [Pref] SET ")
                .Append("[MenuUserId]	= NULL,")
                .Append("[MenuGroupId]	= NULL,")
                .Append("[CharIndex]	= NULL,")
                .Append("[ListCol]		= NULL,")
                .Append("[ListSort]		= NULL,")
                .Append("[ListOrder]	= NULL,")
                .Append("[ListSelCol]	= NULL,")
                .Append("[ListSelSort]	= NULL,")
                .Append("[ListSelOrder] = NULL,")
                .Append("[Header_200]   = NULL,")
                .Append("[Header_300]   = NULL,")
                .Append("[Histo]		= NULL,")
                .Append("[BkmOrder]		= NULL,")
                .Append("[BkmCol_100]	= NULL,")
                .Append("[BkmSort_100]	= NULL,")
                .Append("[BkmOrder_100] = NULL,")
                .Append("[BkmHisto_100] = NULL,")
                .Append("[BkmCol_200]	= NULL,")
                .Append("[BkmSort_200]	= NULL,")
                .Append("[BkmOrder_200] = NULL,")
                .Append("[BkmHisto_200] = NULL,")
                .Append("[BkmCol_300]	= NULL,")
                .Append("[BkmSort_300]	= NULL,")
                .Append("[BkmOrder_300] = NULL,")
                .Append("[BkmHisto_300] = NULL,")
                .Append("[BkmFilterCol]         = NULL, ")
                .Append("[BkmFilterCol_100]     = NULL, ")
                .Append("[BkmFilterCol_200]     = NULL, ")
                .Append("[BkmFilterCol_300]     = NULL, ")
                .Append("[BkmFilterValue]       = NULL, ")
                .Append("[BkmFilterValue_100]   = NULL, ")
                .Append("[BkmFilterValue_200]   = NULL, ")
                .Append("[BkmFilterValue_300]   = NULL, ")
                .Append("[BkmFilterOp]          = NULL, ")
                .Append("[BkmFilterOp_100]      = NULL, ")
                .Append("[BkmFilterOp_200]      = NULL, ")
                .Append("[BkmFilterOp_300]      = NULL, ")
                .Append("[SelectId] = NULL ")
                .Append(" WHERE Tab NOT IN (@reportTab, @xrmHomepageTab, @filtersTab) AND ")
                .Append(queryUser);

            rq.SetQuery(sbSql.ToString());
            rq.AddInputParameter("@reportTab", SqlDbType.Int, TableType.REPORT.GetHashCode());
            rq.AddInputParameter("@xrmHomepageTab", SqlDbType.Int, TableType.XRMHOMEPAGE.GetHashCode());
            rq.AddInputParameter("@filtersTab", SqlDbType.Int, TableType.FILTER.GetHashCode());
            eDal.ExecuteNonQuery(rq, out error);
            if (!String.IsNullOrEmpty(error))
                throw new EudoAdminSQLException("eAdminAccessPref.ResetUserPref", error);

            #endregion



            #region Clean de la table SELECTIONS

            // #54337 : Sauf pour la table REPORT
            // #56915 : Sauf pour la table XRMHOMEPAGE

            sbSql.Length = 0;
            sbSql.Append("DELETE FROM [SELECTIONS]  ")
                .Append("WHERE " + queryUser + " AND Tab NOT IN (@reportTab, @xrmHomepageTab, @filtersTab)")
                ;

            rq.SetQuery(sbSql.ToString());
            rq.AddInputParameter("@reportTab", SqlDbType.Int, TableType.REPORT.GetHashCode());
            rq.AddInputParameter("@xrmHomepageTab", SqlDbType.Int, TableType.XRMHOMEPAGE.GetHashCode());
            rq.AddInputParameter("@filtersTab", SqlDbType.Int, TableType.FILTER.GetHashCode());
            eDal.ExecuteNonQuery(rq, out error);
            if (!String.IsNullOrEmpty(error))
                throw new EudoAdminSQLException("eAdminAccessPref.ResetUserPref", error);
            #endregion

            #region Clean de la table PrefAdv

            sbSql.Length = 0;
            sbSql.Append("DELETE FROM [PREFADV] ")
                .Append("WHERE " + queryUser)
                ;

            rq.SetQuery(sbSql.ToString());
            eDal.ExecuteNonQuery(rq, out error);
            if (!String.IsNullOrEmpty(error))
                throw new EudoAdminSQLException("eAdminAccessPref.ResetUserPref", error);
            #endregion

            #region Détachement des sélections par défaut qui ont été supprimée
            sbSql.Length = 0;
            sbSql.Append("UPDATE [SELECTIONS] SET [DefaultSelectId] = NULL WHERE [DefaultSelectId] NOT IN ( SELECT SELECTID FROM [SELECTIONS] )");

            rq.SetQuery(sbSql.ToString());
            eDal.ExecuteNonQuery(rq, out error);
            if (!String.IsNullOrEmpty(error))
                throw new EudoAdminSQLException("eAdminAccessPref.ResetUserPref", error);
            #endregion

            #region Clean de la table BKMPREF

            sbSql.Length = 0;
            sbSql.Append("UPDATE [BKMPREF] SET ")
                .Append("[bkmcol]       = NULL, ")
                .Append("[bkmcolwidth]  = NULL, ")
                .Append("[bkmsort]      = NULL, ")
                .Append("[bkmorder]     = NULL, ")
                .Append("[bkmhisto]     = NULL, ")
                .Append("[bkmfilterCol] = NULL, ")
                .Append("[bkmfilterOp]  = NULL, ")
                .Append("[bkmfilterValue] = NULL ")
                .Append("WHERE " + queryUser);

            rq.SetQuery(sbSql.ToString());
            eDal.ExecuteNonQuery(rq, out error);
            if (!String.IsNullOrEmpty(error))
                throw new EudoAdminSQLException("eAdminAccessPref.ResetUserPref", error);

            #endregion

            #region Clean de la table FINDERPREF

            sbSql.Length = 0;
            sbSql.Append("UPDATE [FINDERPREF] SET ")
                .Append("[findercol]       = NULL, ")
                .Append("[findercolwidth]  = NULL, ")
                .Append("[findersort]      = NULL, ")
                .Append("[finderorder]     = NULL, ")
                .Append("[finderfilteroptions] = NULL ")
                .Append("WHERE " + queryUser);

            rq.SetQuery(sbSql.ToString());
            eDal.ExecuteNonQuery(rq, out error);
            if (!String.IsNullOrEmpty(error))
                throw new EudoAdminSQLException("eAdminAccessPref.ResetUserPref", error);

            #endregion

            #region Clean de la table COLSPREF

            sbSql.Length = 0;
            sbSql.Append("UPDATE [COLSPREF] SET ")
                .Append("[col]       = NULL, ")
                .Append("[colwidth]  = NULL, ")
                .Append("[sort]      = NULL, ")
                .Append("[sortorder]     = 0, ")
                .Append("[filteroptions] = NULL ")
                .Append("WHERE " + queryUser);

            rq.SetQuery(sbSql.ToString());
            eDal.ExecuteNonQuery(rq, out error);
            if (!String.IsNullOrEmpty(error))
                throw new EudoAdminSQLException("eAdminAccessPref.ResetUserPref", error);

            #endregion

            #region Raz - Filtre sur main field

            sbSql.Length = 0;
            sbSql.Append("UPDATE [USERVALUE] SET [VALUE] = NULL WHERE [TYPE] = @TYPE AND " + queryUser);

            rq.SetQuery(sbSql.ToString());
            rq.AddInputParameter("@TYPE", SqlDbType.Int, EudoQuery.TypeUserValue.SEARCH_MAINFIELD.GetHashCode());
            eDal.ExecuteNonQuery(rq, out error);
            if (!String.IsNullOrEmpty(error))
                throw new EudoAdminSQLException("eAdminAccessPref.ResetUserPref", error);

            #endregion

            // force le menu escamotable fixe
            eLibTools.AddOrUpdatePrefAdv(pref, eLibConst.PREFADV.RIGHTMENUPINNED, "1", eLibConst.PREFADV_CATEGORY.MAIN);

            //Remet la taille de police a 8 par defaut
            eLibTools.AddOrUpdatePrefAdv(pref, eLibConst.PREFADV.FONT_SIZE, "8", eLibConst.PREFADV_CATEGORY.MAIN);

        }


        /// <summary>
        /// Démarre la copie des préférences (config, pref, bkmpref...)  de l'utilisateur source sur la liste des utilisateur destination
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nUserIdSrc"></param>
        /// <param name="usrList"></param>
        /// <returns></returns>
        public static void LaunchCopyPref (ePref pref, eudoDAL eDal, int nUserIdSrc, List<int> usrList)
        {
            // Supprime les préférences des destinataires
            ResetUserPref(pref, eDal, usrList);

            copyConfig(pref, nUserIdSrc, usrList);

            copyPref(pref, nUserIdSrc, usrList);

            copyColsPref(pref, nUserIdSrc, usrList);

            copyBkmPref(pref, nUserIdSrc, usrList);

            copySelections(pref, nUserIdSrc, usrList);

            copyPermission(pref, nUserIdSrc, usrList);

            copyPrefAdv(pref, nUserIdSrc, usrList);

            copyWidgetPref(pref, nUserIdSrc, usrList);
    }

        /// <summary>
        /// Recopie les préférences (config, pref, bkmpref...)  de l'utilisateur source sur la liste des utilisateur destination
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nUserIdSrc"></param>
        /// <param name="lstUserGroup"></param>
        /// <returns></returns>
        public static bool CopyUserAllPrefs(ePref pref, int nUserIdSrc, List<string> lstUserGroup)
        {
            if (pref.User.UserLevel < 99)
                throw new EudoAdminInvalidRightException();

            if (lstUserGroup.Contains(nUserIdSrc.ToString()))
            {
                //Paramètres Invalides : Vous ne pouvez pas affecter les préférences d'un utilisateur à cet utilisateur.
                throw new EudoException(eResApp.GetRes(pref, 8379), eResApp.GetRes(pref, 8379));

            }

            eudoDAL eDal = eLibTools.GetEudoDAL(pref);
            try
            {

                eDal.OpenDatabase();

                //Récupération de la liste des users
                eUser usrObj = new eUser(eDal, pref.User, eUser.ListMode.USERS_AND_GROUPS, pref.GroupMode, lstUserGroup);
                StringBuilder sbError = new StringBuilder();

                var usrList = usrObj.GetUserList(false, false, "", lstUserGroup, sbError)
                      .Where(zz => zz.Type == eUser.UserListItem.ItemType.USER) // Que les utilisateurs 
                      .Select(aa => Int32.Parse(aa.ItemCode)) // on veut juste le userid
                      .Where(e => e != nUserIdSrc) // Pas l'utilisateurs  source
                      .ToList(); // 


                if (lstUserGroup.Contains("0"))
                    usrList.Add(0);

                if (usrList.Count == 0)
                    throw new EudoException(eResApp.GetRes(pref, 8380), eResApp.GetRes(pref, 8380)); //Liste des utilisateurs à mettre à jour vide ou invalide"

                LaunchCopyPref(pref, eDal, nUserIdSrc, usrList);
            }
            finally
            {
                eDal.CloseDatabase();
            }

            return true;
        }

        /// <summary>
        /// Met à jour les préférences de profile utilisateur
        /// Update user's profile preferences
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nUserIdSrc"></param>
        /// <param name="lstUserGroup"></param>
        /// <returns></returns>
        public static void UpdateUserProfilePrefs(ePref pref, int nUserIdSrc, List<string> lstUserGroup)
        {
            ResultStrategy resStrategy = new ResultXrmCru();
            Engine.Engine eng = eModelTools.GetEngine(pref, (int)TableType.USER, eEngineCallContext.GetCallContext(EngineContext.APPLI));
            for (int i = 0; i < lstUserGroup.Count; i++)
            {
                int usrFileId = 0;
                Int32.TryParse(lstUserGroup[i], out usrFileId);
                eng.FileId = usrFileId;
                eng.AddNewValue((int)UserField.USER_PROFILE, nUserIdSrc.ToString());
            }

            eng.EngineProcess(new StrategyCruUser(), resStrategy);
        }
        /// <summary>
        /// recopie des prefAdv
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="nUserIdSrc">User Id Source</param>
        /// <param name="lstUserGroup">Liste des userid à maj</param>
        private static void copyPrefAdv(ePref pref, int nUserIdSrc, List<int> usrList)
        {
            if (usrList.Contains(nUserIdSrc))
            {
                throw new EudoException(eResApp.GetRes(pref, 8379), eResApp.GetRes(pref, 8379));

            }
            eudoDAL dal = eLibTools.GetEudoDAL(pref);
            try
            {
                dal.OpenDatabase();

                //requete pour recopier les prefadv existantes
                StringBuilder strQueryUpdate = new StringBuilder();
                strQueryUpdate.AppendLine("UPDATE [dst] SET");
                strQueryUpdate.AppendLine("[dst].[Value] = [src].[Value]");
                strQueryUpdate.AppendLine(",[dst].[Category] = [src].[Category]");
                strQueryUpdate.AppendLine("FROM [PREFADV] AS [src]");
                strQueryUpdate.AppendLine("INNER JOIN [PREFADV] AS [dst] ON [src].[Parameter] = [dst].[Parameter] AND [src].[Tab] = [dst].[Tab] AND [src].[UserId] <> [dst].[UserId]");
                strQueryUpdate.AppendLine("WHERE [src].[UserId] = @srcUserId");
                strQueryUpdate.AppendLine(String.Format("AND [dst].[UserId] IN ({0});", String.Join(", ", usrList)));
                strQueryUpdate.AppendLine();

                //requete pour insérer les prefadv manquantes
                StringBuilder strQueryInsert = new StringBuilder();
                strQueryInsert.AppendLine("INSERT INTO [PREFADV] ([UserId], [Parameter], [Value], [Category], [Tab])");
                strQueryInsert.AppendLine("SELECT {0}, [slct].[Parameter], [slct].[Value], [slct].[Category], [slct].[Tab]");
                strQueryInsert.AppendLine("FROM [PREFADV] AS [slct]");
                strQueryInsert.AppendLine("WHERE [slct].[UserId] = @srcUserId");
                strQueryInsert.AppendLine("AND [slct].[Id] NOT IN (");
                strQueryInsert.AppendLine("    SELECT [src].[Id]");
                strQueryInsert.AppendLine("    FROM [PREFADV] AS [src]");
                strQueryInsert.AppendLine("    INNER JOIN [PREFADV] AS [dest] ON [src].[Parameter] = [dest].[Parameter] AND [src].[Tab] = [dest].[Tab]");
                strQueryInsert.AppendLine("    WHERE [src].[UserId] = @srcUserId");
                strQueryInsert.AppendLine("    AND [dest].[UserId] = {0}");
                strQueryInsert.AppendLine(");");
                strQueryInsert.AppendLine();

                RqParam rq = new RqParam();
                rq.AddInputParameter("@srcUserId", SqlDbType.Int, nUserIdSrc);

                string query = strQueryUpdate.ToString();
                int i = 0;
                foreach (int nUserIdDest in usrList)
                {
                    string userIdParam = String.Concat("@dstUserId", i.ToString());
                    rq.AddInputParameter(userIdParam, SqlDbType.Int, nUserIdDest);

                    query = String.Concat(query, String.Format(strQueryInsert.ToString(), userIdParam));

                    ++i;
                }

                rq.SetQuery(query);

                string sError;

                dal.ExecuteNonQuery(rq, out sError);
                if (sError.Length > 0)
                    throw new EudoException(
                            eResApp.GetRes(pref, 8381),
                            eResApp.GetRes(pref, 8381), //Impossible de reprendre les prefadv
                            dal?.InnerException ?? new Exception(sError)

                            );
            }
            finally
            {
                dal?.CloseDatabase();
            }
        }


        /// <summary>
        /// recopie des permissions
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="nUserIdSrc">User Id Source</param>
        /// <param name="lstUserGroup">Liste des userid à maj</param>
        private static void copyPermission(ePref pref, int nUserIdSrc, List<int> usrList)
        {

            if (usrList.Contains(nUserIdSrc))
            {
                throw new EudoException(eResApp.GetRes(pref, 8379), eResApp.GetRes(pref, 8379));

            }
            eudoDAL dal = eLibTools.GetEudoDAL(pref);
            try
            {
                dal.OpenDatabase();

                string sUpdatePerm = String.Concat(
                        //SHA : correction bug #70 596
                        " ALTER TABLE [PERMISSION] ALTER COLUMN [User] VARCHAR(1000) ",

                        " update permission set [user] = [user] +';' +  t.uid  ",
                        " FROM [PERMISSION] ",
                        String.Format(" CROSS APPLY (select cast(userid as varchar(10))  uid from [user] where userid in( {0} )) as t ", string.Join(", ", usrList)),
                        " WHERE ISNULL( [USER] ,'')<>'' ",
                        " AND CHARINDEX(';' + @PROFILUSERID +';', ';' + [user] + ';') > 0 ",
                        " AND CHARINDEX(';'+ t.uid +';',';' + [user] +';') = 0 ");


                RqParam rq = new RqParam(sUpdatePerm);
                rq.AddInputParameter("@PROFILUSERID", SqlDbType.VarChar, nUserIdSrc.ToString());

                string sError;

                dal.ExecuteNonQuery(rq, out sError);
                if (sError.Length > 0)
                    throw new EudoException(
                            eResApp.GetRes(pref, 8382),
                            eResApp.GetRes(pref, 8382), //Impossible de reprendre les permissions
                            dal?.InnerException ?? new Exception(sError)

                            );

            }
            finally
            {
                dal?.CloseDatabase();

            }
        }


        /// <summary>
        /// recopie des sélections
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="nUserIdSrc">User Id Source</param>
        /// <param name="lstUserGroup">Liste des userid à maj</param>
        private static void copySelections(ePref pref, int nUserIdSrc, List<int> usrList)
        {

            if (usrList.Contains(nUserIdSrc))
            {
                throw new EudoException(eResApp.GetRes(pref, 8379), eResApp.GetRes(pref, 8379));

            }

            string sError = "";
            try
            {

                eudoDAL dal = eLibTools.GetEudoDAL(pref);
                try
                {
                    dal.OpenDatabase();

                    //Dupplication des sélections 
                    // -> userlist sert à "sauvegarder" l'id de la selection "source". (// cf esp_createUserpref )
                    string sSQLCopySelections = String.Concat(
                        " INSERT INTO SELECTIONS  ( [DEFAULTSELECTID], [TAB], [USERID], [LABEL], [TABORDER], [LISTCOL], [LISTCOLWIDTH], [USERLIST] ) ",
                        " SELECT [DEFAULTSELECTID], [TAB], lst.[USERID], [LABEL], [TABORDER], [LISTCOL], [LISTCOLWIDTH], [SELECTID] FROM [SELECTIONS] ",


                " CROSS APPLY  ( ",
                        String.Format(" SELECT USERID FROM[USER] WHERE[USER].[USERID]  IN ({0}) ", string.Join(", ", usrList)), //Liste des user
                        usrList.Contains(0) ? " UNION  select 0 " : "", // user par défaut
                    ") lst ",

                        " WHERE  [SELECTIONS].[USERID] = @PROFILUSERID ");

                    RqParam rq = new RqParam(sSQLCopySelections);
                    rq.AddInputParameter("@PROFILUSERID", SqlDbType.Int, nUserIdSrc);

                    dal.ExecuteNonQuery(rq, out sError);
                    if (sError.Length > 0)
                        throw new EudoException(eResApp.GetRes(pref, 8383), eResApp.GetRes(pref, 8383), dal?.InnerException ?? new Exception(sError)); //Impossible de duppliquer les sélecions

                    //Fait pointer correctement les selections nouvellement créées

                    // Liste des ongles
                    string sSQLUpdate = String.Concat(
                        "UPDATE [CONFIG]",
                        " SET [CONFIG].[TABORDERID] = sChild.[SELECTID]",
                        " FROM [CONFIG] cParent",
                        " inner join [SELECTIONS] sParent on sParent.[SELECTID] = cParent.[TABORDERID]",
                        " inner join [SELECTIONS] sChild on  isnumeric(sChild.UserList)=1 and sChild.[UserList] = sParent.[SELECTID] ",
                        " inner join [CONFIG] on [CONFIG].[USERID] = sChild.[USERID]",
                        String.Format(" WHERE cParent.userid = @PROFILUSERID and [CONFIG].[USERID] IN ({0}) and IsNull(cParent.[TABORDERID],0) <> 0", string.Join(", ", usrList))



                );
                    RqParam rqTab = new RqParam(sSQLUpdate);
                    rqTab.AddInputParameter("@PROFILUSERID", SqlDbType.Int, nUserIdSrc);

                    dal.ExecuteNonQuery(rqTab, out sError);
                    if (sError.Length > 0)
                        throw new EudoException(eResApp.GetRes(pref, 8384), eResApp.GetRes(pref, 8384), dal?.InnerException ?? new Exception(sError)); //Impossible d'affecter les sélections d'onglet


                    string sSQLUpdateListCol = String.Concat(
                    "UPDATE [PREF]",
                        " SET [PREF].[SELECTID] = sChild.[SELECTID]",
                        " FROM [PREF] pParent",
                        " inner join [SELECTIONS] sParent on sParent.[SELECTID] = pParent.[SELECTID]",
                        " inner join [SELECTIONS] sChild on  isnumeric(sChild.UserList)=1 and sChild.[UserList] = sParent.[SELECTID]",
                        " inner join [PREF] on [PREF].[USERID] = sChild.[USERID]",
                        String.Format(" WHERE pParent.userid = @PROFILUSERID and [PREF].[USERID] IN ({0}) ", string.Join(", ", usrList)),
                        " and pParent.[TAB] <> 0 and pParent.[TAB] = [PREF].[TAB] and IsNull(pParent.[SELECTID],0) <> 0"
                        );

                    RqParam rqListCol = new RqParam(sSQLUpdateListCol);
                    rqListCol.AddInputParameter("@PROFILUSERID", SqlDbType.Int, nUserIdSrc);

                    dal.ExecuteNonQuery(rqListCol, out sError);
                    if (sError.Length > 0)
                        throw new EudoException(eResApp.GetRes(pref, 8385), eResApp.GetRes(pref, 8385), dal?.InnerException ?? new Exception(sError)); //Impossible d'affecter les sélections de liste de colonnes



                    string sqlCleanUp = String.Format("UPDATE [SELECTIONS] SET [USERLIST] = NULL WHERE IsNull([USERLIST], '') <> '' and [USERID] IN ({0}) ", string.Join(", ", usrList));


                    RqParam rqLCleanUp = new RqParam(sqlCleanUp);
                    dal.ExecuteNonQuery(rqLCleanUp, out sError);
                    if (sError.Length > 0)
                        throw new EudoException(eResApp.GetRes(pref, 8386), eResApp.GetRes(pref, 8386), dal?.InnerException ?? new Exception(sError)); //Erreur sur la fin de traitement de la recopie des sélections
                }
                finally
                {
                    dal?.CloseDatabase();
                }

            }
            catch (EudoException) { throw; }
            catch (Exception e) { throw new EudoException(e.Message, eResApp.GetRes(pref, 8387), e); } //Impossible de recopier les sélections
        }


        /// <summary>
        /// /
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="nUserIdSrc">User Id Source</param>
        /// <param name="lstUserGroup">Liste des userid à maj</param>
        private static void copyBkmPref(ePref pref, int nUserIdSrc, List<int> usrList)
        {


            if (usrList.Contains(nUserIdSrc))
            {
                throw new EudoException(eResApp.GetRes(pref, 8379), eResApp.GetRes(pref, 8379));

            }

            try
            {
                pref.CopyBkmPref(nUserIdSrc, usrList);

            }
            catch (EudoException) { throw; }
            catch (Exception e) { throw new EudoException(e.Message, eResApp.GetRes(pref, 8388), e); } //Impossible de recopier les COLSPREF
        }

        /// <summary>
        /// Copie des preferences widget
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="nUserIdSrc">User Id Source</param>
        /// <param name="lstUserGroup">Liste des userid à maj</param>
        private static void copyWidgetPref(ePref pref, int nUserIdSrc, List<int> usrList)
        {


            if (usrList.Contains(nUserIdSrc))
            {
                throw new EudoException(eResApp.GetRes(pref, 8379), eResApp.GetRes(pref, 8379));

            }

            try
            {
                pref.CopyWidgetPref(nUserIdSrc, usrList);

            }
            catch (EudoException) { throw; }
            catch (Exception e) { throw new EudoException(e.Message, eResApp.GetRes(pref, 8389), e); } //Impossible de recopier les WidgetPref
        }


        /// <summary>
        /// Met à jour les champs de config 
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="nUserIdSrc">User Id Source</param>
        /// <param name="lstUserGroup">Liste des userid à maj</param>
        /// <returns></returns>
        private static void copyConfig(ePref pref, int nUserIdSrc, List<int> lstUserGroup)
        {
            if (lstUserGroup.Contains(nUserIdSrc))
            {
                throw new EudoException(eResApp.GetRes(pref, 8379), eResApp.GetRes(pref, 8379));

            }

            Dictionary<KeyConfig, string> dic = new Dictionary<KeyConfig, string>();
            List<SetParam<eLibConst.PREF_CONFIG>> listConfig = new List<SetParam<eLibConst.PREF_CONFIG>>();

            try
            {
                dic = pref.LoadUserConfig(nUserIdSrc);
                listConfig = new List<SetParam<eLibConst.PREF_CONFIG>>();

                foreach (var kvp in dic)
                {
                    if (eLibTools.GetEnumFromCode<eLibConst.PREF_CONFIG>(kvp.Key.GetHashCode()) != eLibConst.PREF_CONFIG.TABORDERID)
                        listConfig.Add(new SetParam<eLibConst.PREF_CONFIG>(eLibTools.GetEnumFromCode<eLibConst.PREF_CONFIG>(kvp.Key.GetHashCode()), kvp.Value));
                }
            }
            catch (EudoException) { throw; }
            catch (Exception e) { throw new EudoException(e.Message, eResApp.GetRes(pref, 8392), e); } //Impossible de charger les préférences de config l'utilisateur source

            try
            {
                if (listConfig.Count > 0)
                    pref.SetConfig(listConfig, lstUserGroup);
            }
            catch (EudoException) { throw; }
            catch (Exception e) { throw new EudoException(e.Message, eResApp.GetRes(pref, 8390), e); } //Impossible de mettres à jours les préférences de config des utilisateurs cibles



        }


        /// <summary>
        /// Copie les informations de PREF
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nUserIdSrc"></param>
        /// <param name="usrList"></param>
        private static void copyPref(ePref pref, int nUserIdSrc, List<int> usrList)
        {


            if (usrList.Contains(nUserIdSrc))
            {
                throw new EudoException(eResApp.GetRes(pref, 8379), eResApp.GetRes(pref, 8379));

            }

            try
            {
                pref.CopyPref(nUserIdSrc, usrList);
            }
            catch (EudoException) { throw; }
            catch (Exception e) { throw new EudoException(e.Message, eResApp.GetRes(pref, 8391), e); } //Impossible de mettres à jours les préférences des utilisateurs cibles

        }


        /// <summary>
        /// Recopie de COLSPREF
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nUserIdSrc"></param>
        /// <param name="usrList"></param>
        private static void copyColsPref(ePref pref, int nUserIdSrc, List<int> usrList)
        {

            if (usrList.Contains(nUserIdSrc))
            {
                throw new EudoException(eResApp.GetRes(pref, 8379), eResApp.GetRes(pref, 8379));

            }

            try
            {
                pref.CopyColsPref(nUserIdSrc, usrList);

            }
            catch (EudoException) { throw; }
            catch (Exception e) { throw new EudoException(e.Message, eResApp.GetRes(pref, 8388), e); } //Impossible de recopier les COLSPREF
        }


    }
}