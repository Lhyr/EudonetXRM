using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;

using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// requête pour obtenir les traductions
    /// </summary>
    public class eSqlTranslations
    {
        /// <summary>
        /// Construit la sous-requête permettant d'accéder aux traductions enregistrées dans la table RES
        /// </summary>
        /// <param name="nature">Filtre sur Nature</param>
        /// <param name="hsLangIds">Filtre sur les langues</param>
        /// <param name="iRefLangId">Langue de référence (Utilisée par l'administrateur)</param>
        /// <returns></returns>
        public static HashSet<string> GetResSql(eAdminTranslation.NATURE nature, HashSet<Int32> hsLangIds, Int32 iRefLangId)
        {

            HashSet<String> hsSql = new HashSet<string>();

            switch (nature)
            {
                case eAdminTranslation.NATURE.None:
                case eAdminTranslation.NATURE.Tab:
                case eAdminTranslation.NATURE.PJ:
                case eAdminTranslation.NATURE.Field:
                    break;
                default:
                    return hsSql;
                    break;
            }


            foreach (Int32 iLangId in hsLangIds)
            {
                StringBuilder sbSQL = new StringBuilder();
                sbSQL.AppendLine().Append("SELECT ResId AS DESCID ").AppendLine()
                           .Append("  , ResId AS RESID").AppendLine()
                     .AppendFormat("  , LANG_{0} AS {1} ", iRefLangId.ToString("00"), eAdminTranslationsList.COLS.SOURCE).AppendLine()
                     .AppendFormat("  , LANG_{0} AS TRANSLATION", iLangId.ToString("00")).AppendLine()
                     .AppendFormat("  , CASE WHEN ResId % 100 = 0 THEN @{0}Label WHEN ResId % 100 = @PjFieldDid Then @{1}Label ELSE @{2}Label END AS NATURE ", eAdminTranslation.NATURE.Tab, eAdminTranslation.NATURE.PJ, eAdminTranslation.NATURE.Field).AppendLine()
                     .AppendFormat("  , {0} AS LANGID ", iLangId).AppendLine()
                     .AppendFormat("  , @Location{0} AS LOCATION ", eAdminTranslation.LOCATION.Res).AppendLine()
                           .Append("  , NULL AS PATHCOMP").AppendLine()
                           .Append("  , NULL AS [TYPE]").AppendLine()
                           .Append("FROM RES ").AppendLine();

                switch (nature)
                {
                    case eAdminTranslation.NATURE.None:
                        sbSQL.Append("WHERE ResId >= 100 ").AppendLine();
                        break;
                    case eAdminTranslation.NATURE.Tab:
                        sbSQL.Append("WHERE ResId % 100 = 0").AppendLine();
                        break;
                    case eAdminTranslation.NATURE.PJ:
                        sbSQL.Append("WHERE ResId % 100 = @PjFieldDid").AppendLine();
                        break;
                    case eAdminTranslation.NATURE.Field:
                        sbSQL.Append("WHERE ResId % 100 > 0").AppendLine();
                        break;
                    default:
                        break;
                }
                hsSql.Add(sbSQL.ToString());

            }
            return hsSql;

        }

        /// <summary>
        /// Construit la sous-requête permettant d'accéder aux traductions enregistrées dans la table RES
        /// </summary>
        /// <param name="nature">Filtre sur Nature</param>
        /// <param name="hsLangIds">Filtre sur les langues</param>
        /// <param name="iRefLangId">Langue de référence (Utilisée par l'administrateur)</param>
        /// <returns></returns>
        public static HashSet<string> GetResAdvSql(eAdminTranslation.NATURE nature, HashSet<Int32> hsLangIds, Int32 iRefLangId)
        {

            HashSet<String> hsSql = new HashSet<string>();

            switch (nature)
            {
                case eAdminTranslation.NATURE.None:
                case eAdminTranslation.NATURE.Tab:
                    break;
                default:
                    return hsSql;
                    break;
            }


            foreach (Int32 iLangId in hsLangIds)
            {


                StringBuilder sbSQL = new StringBuilder($@"SELECT Source.DESCID AS DESCID 
                       , Source.DESCID AS RESID
                       , Source.LANG AS SOURCE
                       , Translation.LANG AS TRANSLATION
                       , CASE WHEN Source.DESCID % 100 = 0 THEN @{eAdminTranslation.NATURE.Tab}Label 
                            WHEN Source.DESCID % 100 = @PjFieldDid Then @{eAdminTranslation.NATURE.PJ}Label 
                            ELSE @{eAdminTranslation.NATURE.Field}Label END AS NATURE
                      , Translation.ID_LANG AS LANGID  
                      , @Location{eAdminTranslation.LOCATION.ResAdv} AS LOCATION 
                      , NULL AS PATHCOMP
                      , Source.[Type] AS [TYPE]
                      FROM RESADV Source Inner Join RESADV Translation ON Source.DESCID = Translation.DESCID
                      AND Source.TYPE = Translation.TYPE 
                      WHERE Source.ID_LANG = {iRefLangId}
                      AND TRANSLATION.ID_LANG in ({eLibTools.Join(",", hsLangIds)})
                      ");

                switch (nature)
                {
                    case eAdminTranslation.NATURE.None:
                        sbSQL.Append("AND Source.DESCID >= 100 ").AppendLine();
                        break;
                    case eAdminTranslation.NATURE.Tab:
                        sbSQL.Append("AND Source.DESCID % 100 = 0").AppendLine();
                        break;
                    case eAdminTranslation.NATURE.PJ:
                        sbSQL.Append("AND Source.DESCID % 100 = @PjFieldDid").AppendLine();
                        break;
                    case eAdminTranslation.NATURE.Field:
                        sbSQL.Append("WHERE Source.DESCID % 100 > 0").AppendLine();
                        break;
                    default:
                        break;
                }
                hsSql.Add(sbSQL.ToString());

            }
            return hsSql;

        }
        /// <summary>
        /// Construit la sous-requête permettant d'accéder aux traductions enregistrées dans la table FileData
        /// </summary>
        /// <param name="nature">Filtre sur Nature</param>
        /// <param name="hsLangIds">Filtre sur les langues</param>
        /// <param name="iRefLangId">Langue de référence (Utilisée par l'administrateur)</param>
        /// <returns></returns>
        internal static IEnumerable<string> GetCatalogValueResSql(eAdminTranslation.NATURE nature, HashSet<int> hsLangIds, int iRefLangId)
        {
            HashSet<String> hsSql = new HashSet<string>();

            switch (nature)
            {
                case eAdminTranslation.NATURE.None:
                case eAdminTranslation.NATURE.Catalog:
                case eAdminTranslation.NATURE.CatalogValue:
                    break;
                default:
                    return hsSql;
                    break;
            }

            foreach (Int32 iLangId in hsLangIds)
            {
                StringBuilder sbSQL = new StringBuilder();
                sbSQL.AppendLine().Append("SELECT DescId AS DESCID ").AppendLine()
                           .Append("  , DataId AS RESID").AppendLine()
                     .AppendFormat("  , LANG_{0} AS {1} ", iRefLangId.ToString("00"), eAdminTranslationsList.COLS.SOURCE).AppendLine()
                     .AppendFormat("  , LANG_{0} AS {1}", iLangId.ToString("00"), eAdminTranslationsList.COLS.TRANSLATION).AppendLine()
                     .AppendFormat("  , @CatalogValueLabel AS {0} ", eAdminTranslationsList.COLS.NATURE).AppendLine()
                     .AppendFormat("  , {0} AS {1} ", iLangId, eAdminTranslationsList.COLS.LANGID).AppendLine()
                     .AppendFormat("  , @Location{0} AS LOCATION ", eAdminTranslation.LOCATION.FileData).AppendLine()
                           .Append("  , NULL AS PATHCOMP").AppendLine()
                           .Append("  , NULL AS [TYPE] ").AppendLine()
                           .Append("FROM FILEDATA ").AppendLine();


                hsSql.Add(sbSQL.ToString());

            }
            return hsSql;


        }

        /// <summary>
        /// Construit la sous-requête permettant d'accéder aux traductions des infobulles enregistrées dans la table FileData
        /// </summary>
        /// <param name="nature">Filtre sur Nature</param>
        /// <param name="hsLangIds">Filtre sur les langues</param>
        /// <param name="iRefLangId">Langue de référence (Utilisée par l'administrateur)</param>
        /// <returns></returns>
        internal static IEnumerable<string> GetCatalogValueToolTipResSql(eAdminTranslation.NATURE nature, HashSet<int> hsLangIds, int iRefLangId)
        {
            HashSet<String> hsSql = new HashSet<string>();

            switch (nature)
            {
                case eAdminTranslation.NATURE.None:
                case eAdminTranslation.NATURE.Catalog:
                case eAdminTranslation.NATURE.CatalogValueToolTip:
                    break;
                default:
                    return hsSql;
                    break;
            }


            foreach (Int32 iLangId in hsLangIds)
            {
                StringBuilder sbSQL = new StringBuilder();
                sbSQL.AppendLine().Append("SELECT DescId AS DESCID ").AppendLine()
                          .Append("  , DataId AS RESID").AppendLine()
                    .AppendFormat("  , CAST(Tip_LANG_{0} AS NVARCHAR(500)) AS {1} ", iRefLangId.ToString("00"), eAdminTranslationsList.COLS.SOURCE).AppendLine()
                    .AppendFormat("  , CAST(Tip_LANG_{0} AS NVARCHAR(500)) AS {1} ", iLangId.ToString("00"), eAdminTranslationsList.COLS.TRANSLATION).AppendLine()
                    .AppendFormat("  , @CatalogValueToolTipLabel AS {0} ", eAdminTranslationsList.COLS.NATURE).AppendLine()
                    .AppendFormat("  , {0} AS {1} ", iLangId, eAdminTranslationsList.COLS.LANGID).AppendLine()
                    .AppendFormat("  , @Location{0} AS LOCATION ", eAdminTranslation.LOCATION.FileData_ToolTips).AppendLine()
                    .AppendFormat("  , '.' + LANG_{0} AS PATHCOMP", iRefLangId.ToString("00")).AppendLine()
                      .AppendLine("  , NULL AS [TYPE] ")
                          .Append("FROM FILEDATA ").AppendLine();


                hsSql.Add(sbSQL.ToString());


            }
            return hsSql;

        }

        /// <summary>
        /// Construit la sous-requête permettant d'accéder aux traductions enregistrées dans la table RES_SPECIFS
        /// </summary>
        /// <param name="nature">Filtre sur Nature</param>
        /// <param name="hsLangIds">Filtre sur les langues</param>
        /// <param name="iRefLangId">Langue de référence (Utilisée par l'administrateur)</param>
        /// <returns></returns>
        internal static IEnumerable<string> GetSpecifResSql(eAdminTranslation.NATURE nature, HashSet<int> hsLangIds, int iRefLangId)
        {
            HashSet<String> hsSql = new HashSet<string>();

            switch (nature)
            {
                case eAdminTranslation.NATURE.None:
                case eAdminTranslation.NATURE.WebTab:
                case eAdminTranslation.NATURE.WebLink:
                    break;
                default:
                    return hsSql;
                    break;
            }
            String sWebTabSpecifVars = String.Format("@SType{0}, @SType{1}", eLibConst.SPECIF_TYPE.TYP_WEBTAB_EXTERNAL, eLibConst.SPECIF_TYPE.TYP_WEBTAB_INTERNAL);
            String sWebLinkSpecifVars = String.Format("@SType{0}, @SType{1}, @SType{2}", eLibConst.SPECIF_TYPE.TYP_FILE, eLibConst.SPECIF_TYPE.TYP_LIST, eLibConst.SPECIF_TYPE.TYP_FAVORITE);
            HashSet<String> hsWhere = new HashSet<string>();
            hsWhere.Add(sWebTabSpecifVars);
            hsWhere.Add(sWebLinkSpecifVars);

            switch (nature)
            {
                case eAdminTranslation.NATURE.WebTab:
                    hsWhere.Remove(sWebLinkSpecifVars);
                    break;
                case eAdminTranslation.NATURE.WebLink:
                    hsWhere.Remove(sWebTabSpecifVars);
                    break;
            }



            foreach (Int32 iLangId in hsLangIds)
            {
                StringBuilder sbSQL = new StringBuilder();
                sbSQL.AppendLine().Append("SELECT SPECIFS.Tab AS DESCID ").AppendLine()
                           .Append("  , RES_SPECIFS.SpecifId AS RESID").AppendLine()
                     .AppendFormat("  , ISNULL(RES_SPECIFS.LANG_{0}, SPECIFS.Label) AS {1} ", iRefLangId.ToString("00"), eAdminTranslationsList.COLS.SOURCE).AppendLine()
                     .AppendFormat("  , LANG_{0} AS {1}", iLangId.ToString("00"), eAdminTranslationsList.COLS.TRANSLATION).AppendLine()
                           .Append("  , CASE WHEN SpecifType in (").Append(sWebTabSpecifVars).AppendFormat(") THEN @{0}Label ", eAdminTranslation.NATURE.WebTab).AppendLine()
                           .Append("         WHEN SpecifType in (").Append(sWebLinkSpecifVars).AppendFormat(") THEN @{0}Label ", eAdminTranslation.NATURE.WebLink).AppendLine()
                     .AppendFormat("         ELSE '' END AS {0} ", eAdminTranslationsList.COLS.NATURE).AppendLine()
                     .AppendFormat("  , {0} AS {1} ", iLangId, eAdminTranslationsList.COLS.LANGID).AppendLine()
                     .AppendFormat("  , @Location{0} AS LOCATION ", eAdminTranslation.LOCATION.Res_Specifs).AppendLine()
                           .Append("  , NULL AS PATHCOMP").AppendLine()
                           .Append("  , NULL AS [TYPE] ").AppendLine()
                           .Append("FROM SPECIFS LEFT JOIN RES_SPECIFS ON RES_SPECIFS.SpecifId = SPECIFS.SpecifId ").AppendLine();
                if (hsWhere.Count > 0)
                    sbSQL.AppendFormat("WHERE SpecifType in ({0})", eLibTools.Join<String>(",", hsWhere)).AppendLine();

                hsSql.Add(sbSQL.ToString());

            }
            return hsSql;
        }

        /// <summary>
        /// Requête pour retrouver les RES_FILES d'une table
        /// </summary>
        /// <param name="hsLangIds">The hs language ids.</param>
        /// <param name="iRefLangId">The i reference language identifier.</param>
        /// <returns></returns>
        internal static String GetFilesResSql(HashSet<int> hsLangIds, int iRefLangId, int descID, int fileID)
        {

            StringBuilder sbSQL = new StringBuilder();
            sbSQL.AppendLine("SELECT R.DescId, ID AS [RESID], [DESC].[File] + '.' + [DESC].[Field] AS [SYSID], ")
                .AppendFormat("(SELECT LANG FROM [RES_FILES] WHERE Descid = R.DescId AND ID_LANG = {0} AND FileId = R.FileId) AS [SOURCE], ", iRefLangId.ToString())
                .AppendLine("ID_LANG AS [LANGID], ")
                .AppendLine("LANG AS [TRANSLATION], ")
                .AppendFormat("@Location{0} AS [LOCATION], ", eAdminTranslation.LOCATION.Res_Files)
                .AppendFormat("@{0}Label AS [NATURE], ", eAdminTranslation.NATURE.FileValue)
                .AppendLine("NULL AS [PATHCOMP], ")
                .AppendLine("NULL AS [TYPE] ")
                .AppendLine("FROM RES_FILES R INNER JOIN [DESC] ON [DESC].[DESCID] = R.DESCID ")
            .AppendFormat("WHERE R.ID_LANG IN ({0}) ", String.Join(",", hsLangIds));
            if (fileID > 0)
                sbSQL.Append("AND R.FileId = @FileId ");
            if (descID > 0)
                sbSQL.Append("AND (R.DESCID BETWEEN @DescId AND @DescId + 99) --ORDER BY DESCID ASC, SOURCE ASC, NATURE ASC , LANGID ASC, TRANSLATION ASC");

            return sbSQL.ToString();
        }

        /// <summary>
        /// Retourne la requête permettant de récupérer les traductions des paramètres des widgets
        /// </summary>
        /// <param name="langId">The language identifier.</param>
        /// <param name="widgetId">The widget identifier.</param>
        /// <param name="sortsList">The sorts list.</param>
        /// <returns></returns>
        internal static string GetWidgetParamsTranslationsSQL(int langId = -1, int widgetId = 0, List<eAdminTranslationsList.OrderBy> sortsList = null)
        {
            string widgetSuffix = string.Empty;
            if (widgetId > 0)
            {
                widgetSuffix = String.Concat("-", widgetId, "/");
            }

            StringBuilder sbQuery = new StringBuilder($@"SELECT 
                (SELECT ResValue FROM [RESCODE] WHERE LangId = 0 AND Code = RCode.Code) AS [SOURCE],
                ResValue AS [TRANSLATION]
                , ResPath AS [PATH]
                , {(int)eAdminTranslation.NATURE.WidgetParam} AS [Nature]
                , Identifier AS [SYSID]
                , [LangId] AS [LANGID]
                , ResLoc.ResLocationId AS [LOCID]
                , RCode.ResCodeId AS [RESID]
                FROM RESCODE RCode
                LEFT JOIN RESLOCATION ResLoc ON ResLoc.ResLocationId = RCode.ResLocationId
                WHERE CHARINDEX('/{(int)TableType.XRMWIDGET}{widgetSuffix}', '/' + ResLoc.ResPath + '/') > 0");

            if (langId >= 0)
                sbQuery.AppendLine(" AND [LangId] = @langId");

            if (sortsList != null && sortsList.Count > 0)
            {
                sbQuery.Append(" ORDER BY ").Append(eLibTools.Join(", ", sortsList));
            }

            return sbQuery.ToString();
        }

        /// <summary>
        /// Construit la requête permettant d'afficher le chemin de l'emplacement du paramètre du widget
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="path">The path.</param>
        /// <param name="langId">The language identifier.</param>
        /// <returns></returns>
        internal static string BuildWidgetParamPathLabelSQL(ePref pref, string path, int langId = 0)
        {
            string[] arrPath = path.Split('/');
            string part;
            string[] arrPart;
            int tab = 0, fileid = 0;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < arrPath.Length; i++)
            {
                if (sb.Length > 0)
                    sb.AppendLine(" UNION ");

                part = arrPath[i];
                arrPart = part.Split('-');
                if (arrPart.Length == 0 || arrPart.Length > 2)
                    continue;

                tab = eLibTools.GetNum(arrPart[0]);
                if (tab == 0)
                    continue;

                fileid = 0;
                if (arrPart.Length == 2)
                    fileid = eLibTools.GetNum(arrPart[1]);

                if (tab != (int)TableType.XRMGRID && tab != (int)TableType.XRMWIDGET && tab != (int)TableType.XRMHOMEPAGE)
                {
                    sb.Append($"SELECT CAST(RESID AS INT) [ID], LANG_{langId.ToString("00")} [VALUE] FROM [RES] WHERE RESID = {tab}");
                    continue;
                }
                else
                {
                    sb.Append($"SELECT TAB [ID], LANG [VALUE] FROM [RES_FILES] WHERE ID_LANG = {langId} AND TAB = {tab} AND DESCID = {tab + 1} AND FILEID = {fileid}");
                }

            }
            return sb.ToString();
        }

        /// <summary>
        /// Procède à un Union sur les sous-requêtes sélectionnées
        /// Effectue les filtres et les tris demandé
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="lstSqlRequests"></param>
        /// <param name="tl"></param>
        /// <returns>La requête SQL complète correspondant aux besoin de eAdminTranslationsLis</returns>
        internal static string Join(ePref pref, List<string> lstSqlRequests, eAdminTranslationsList tl)
        {
            StringBuilder sbSQL = new StringBuilder();
            string sSubRq = "";
            try
            {
                sSubRq = eLibTools.Join<String>(String.Concat(Environment.NewLine, "UNION", Environment.NewLine), lstSqlRequests);

                sbSQL.AppendFormat("SELECT DESCID, RESID, ").Append(eAdminTranslationsList.COLS.SOURCE).Append(", ").Append(eAdminTranslationsList.COLS.TRANSLATION).Append(", ").Append(eAdminTranslationsList.COLS.PATH).Append(", ").Append(eAdminTranslationsList.COLS.NATURE).Append(", ").Append(eAdminTranslationsList.COLS.SYSID).Append(", ").Append(eAdminTranslationsList.COLS.LANGID).Append(", LOCATION, ISNULL(TYPE, -1) AS TYPE FROM (").AppendLine()
                    .Append("   SELECT ALLRES.DESCID ").AppendLine()
                    .Append("     , CAST(ALLRES.RESID AS int) AS RESID").AppendLine()
              .AppendFormat("     , ALLRES.{0}", eAdminTranslationsList.COLS.SOURCE).AppendLine()
              .AppendFormat("     , ALLRES.{0}", eAdminTranslationsList.COLS.TRANSLATION).AppendLine()
              .AppendFormat("     , CASE WHEN TabRes.ResId IS NOT NULL THEN TabRes.LANG_{0} + '.' ELSE '' END + RES.LANG_{0} + ISNULL(ALLRES.PATHCOMP,'') AS PATH ", pref.LangId.ToString("00"), eAdminTranslation.LOCATION.FileData_ToolTips).AppendLine()
              .AppendFormat("     , ALLRES.{0} ", eAdminTranslationsList.COLS.NATURE).AppendLine()
              .AppendFormat("     , [DESC].[FILE] + CASE WHEN [DESC].DescId % 100 > 0 THEN '.' + [DESC].[FIELD] ELSE '' END AS {0}", eAdminTranslationsList.COLS.SYSID).AppendLine()
              .AppendFormat("     , ALLRES.{0} ", eAdminTranslationsList.COLS.LANGID).AppendLine()
                    .Append("     , ALLRES.LOCATION ").AppendLine()
                    .Append("     , ALLRES.[TYPE] ").AppendLine()
                    .Append("   FROM ( ").AppendLine()
                    .Append("   ").Append(sSubRq).AppendLine()
                    .Append("   ) ALLRES ").AppendLine()
                    .Append("   INNER JOIN [DESC] ON [DESC].DescId = ALLRES.DESCID ").AppendLine()
                    .Append("   INNER JOIN [RES] ON ALLRES.DESCID = RES.ResId ").AppendLine()
                    .Append("   LEFT JOIN [RES] TabRes ON ALLRES.DESCID % 100 > 0 AND TabRes.ResId = ALLRES.DESCID - (ALLRES.DESCID % 100) ").AppendLine()
                    .Append(") ALLWENEED").AppendLine();

                HashSet<string> hsWhere = new HashSet<string>();
                if (tl.ResId > 0)
                {
                    hsWhere.Add("RESID = @ResId");
                }
                if (tl.DescId > 0)
                {
                    if (tl.DescId % 100 == 0)
                    {
                        hsWhere.Add("DESCID BETWEEN @DescId AND @DescId + 99");
                    }
                    else
                    {
                        hsWhere.Add("DESCID = @DescId");
                    }
                }

                if (tl.Search.Length > 0)
                {
                    String sCriteria = "CHARINDEX(@Search, {0}) > 0";
                    HashSet<string> hsWhereSearch = new HashSet<string>();
                    hsWhereSearch.Add(String.Format(sCriteria, eAdminTranslationsList.COLS.SOURCE));
                    hsWhereSearch.Add(String.Format(sCriteria, eAdminTranslationsList.COLS.TRANSLATION));
                    hsWhereSearch.Add(String.Format(sCriteria, eAdminTranslationsList.COLS.PATH));
                    hsWhereSearch.Add(String.Format(sCriteria, eAdminTranslationsList.COLS.NATURE));
                    hsWhereSearch.Add(String.Format(sCriteria, eAdminTranslationsList.COLS.SYSID));
                    hsWhere.Add(eLibTools.Join<String>(" OR ", hsWhereSearch));
                }

                if (hsWhere.Count > 0)
                {
                    sbSQL.AppendFormat("WHERE ({0})", eLibTools.Join<String>(") AND (", hsWhere)).AppendLine();
                }

                //Application des tris
                if (tl.Sorts != null && tl.Sorts.Count > 0)
                {
                    sbSQL.Append("ORDER BY ").Append(eLibTools.Join(", ", tl.Sorts));
                }
            }
            catch (Exception e)
            {
                throw new Exception(String.Concat("eSqlTranslations.Join() : ", e.Message, Environment.NewLine, "sbSQL : ", sbSQL, Environment.NewLine, "sSubRq : ", sSubRq, Environment.NewLine, e.StackTrace));
            }
            //A NOTER : les filtres sur Langue et sur Nature ne sont pas filtrées dans le Where : 
            //          dans ces cas on n'implémente que les sous-requêtes concernées.

            return sbSQL.ToString();
        }


        /// <summary>
        /// Création de ressource manquante
        /// </summary>
        /// <param name="pref">pref user</param>
        /// <returns></returns>
        public static String InsertMissingResSpecif(ePrefLite pref)
        {

            StringBuilder sbSQL = new StringBuilder();
            StringBuilder sbSqlTo = new StringBuilder();
            StringBuilder sbSqlSelect = new StringBuilder();

            sbSqlTo.Append("INSERT INTO RES_SPECIFS(SpecifId ");
            sbSqlSelect.Append("SELECT SpecifId ");
            for (int i = 0; i <= eLibConst.MAX_LANG; i++)
            {
                sbSqlTo.AppendFormat(", LANG_{0}", i.ToString("00"));
                sbSqlSelect.Append(", Label");
            }

            sbSqlTo.Append(")").AppendLine();


            sbSQL.Append(sbSqlTo).Append(sbSqlSelect).AppendLine()
                .Append("FROM SPECIFS WHERE NOT EXISTS (SELECT SpecifId FROM RES_SPECIFS WHERE RES_SPECIFS.SpecifId = SPECIFS.SpecifId)");

            return sbSQL.ToString();
        }

        /// <summary>
        /// Retourne les requêtes pour créer les lignes manquantes dans RES_FILES 
        /// </summary>
        /// <param name="pref">ePrefLite</param>
        /// <param name="tableSQLName">Name of the table SQL.</param>
        /// <param name="fieldSQLName">Name of the field SQL.</param>
        /// <param name="dicAdditionalparameters">Paramètres additionnels à rajouter à la requête</param>
        /// <param name="dicResLang">Dictionnaire pour spécifier à la main les Res pour certaines langues</param>
        /// <returns></returns>
        public static String InsertMissingFilesRes(String tableSQLName, String fieldSQLName, String idFieldSQLName, out Dictionary<string, string> dicAdditionalparameters, Dictionary<int, string> dicResLang = null)
        {
            StringBuilder sbSQL = new StringBuilder();
            dicAdditionalparameters = new Dictionary<string, string>();

            for (int i = 0; i <= eLibConst.MAX_LANG; i++)
            {
                sbSQL.AppendLine("INSERT INTO RES_FILES (TAB, DESCID, FILEID, LANG, ID_LANG) ");

                string langField = fieldSQLName;
                if (dicResLang != null && dicResLang.ContainsKey(i) && !String.IsNullOrEmpty(dicResLang[i]))
                {
                    langField = String.Concat("@resLang_", i.ToString());
                    dicAdditionalparameters.Add(langField, dicResLang[i]);
                }

                sbSQL.AppendFormat("SELECT [DESC].DescId - [DESC].DescId % 100, [DESC].DescId, @FileId, {2}, {0} FROM [{1}]", i.ToString(), tableSQLName, langField)
                .AppendFormat("INNER JOIN [DESC] ON [DESC].[File] = '{0}' AND [DESC].[Field] = '{1}' ", tableSQLName, fieldSQLName)
                .Append("WHERE NOT EXISTS (")
                .AppendFormat("SELECT ID FROM [RES_FILES] RF WHERE RF.DESCID = [DESC].DescId AND ID_LANG = {0} AND RF.FILEID = @FileId ", i.ToString())
                .AppendFormat(") AND [{0}].[{1}] = @FileId;", tableSQLName, idFieldSQLName)
                .AppendLine();
            }
            return sbSQL.ToString();
        }

        /// <summary>
        /// renvoie la requête permettant la mise à jour d'une traduction
        /// </summary>
        /// <param name="iLangId"></param>
        /// <returns></returns>
        public static string UpdateResSpecifs(Int32 iLangId)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("UPDATE RES_SPECIFS").AppendLine()
                .AppendFormat("SET LANG_{0} = @Translation", iLangId.ToString("00")).AppendLine()
                .Append("WHERE SpecifId = @ResId");
            return sbSql.ToString();
        }

        /// <summary>
        /// renvoie la requête permettant la mise à jour d'une traduction
        /// </summary>
        /// <param name="iLangId"></param>
        /// <returns></returns>
        public static string UpdateFileData(Int32 iLangId)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("UPDATE FileData").AppendLine()
                .AppendFormat("SET LANG_{0} = @Translation", iLangId.ToString("00")).AppendLine()
                .Append("WHERE DataId = @ResId");
            return sbSql.ToString();
        }

        /// <summary>
        /// renvoie la requête permettant la mise à jour d'une traduction
        /// </summary>
        /// <param name="iLangId"></param>
        /// <returns></returns>
        public static string UpdateFileDataToolTip(Int32 iLangId)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("UPDATE FileData").AppendLine()
                .AppendFormat("SET Tip_Lang_{0} = @Translation", iLangId.ToString("00")).AppendLine()
                .Append("WHERE DataId = @ResId");
            return sbSql.ToString();
        }

        /// <summary>
        /// Renvoie la requête permettant la mise à jour d'une traduction de valeur de rubrique
        /// </summary>
        /// <param name="iLangId">LangId</param>
        /// <returns></returns>
        public static String UpdateFilesRes(Int32 iLangId)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("UPDATE [RES_FILES] ")
                .AppendFormat("SET ID_LANG = {0}, LANG = @Translation ", iLangId.ToString("00"))
                .AppendLine("WHERE ID = @ResId");
            return sbSql.ToString();
        }

        /// <summary>
        /// Renvoie la requête permettant de mettre à jour une ligne dans RESCODE
        /// </summary>
        /// <returns></returns>
        public static string UpdateResCode()
        {
            string query = "UPDATE [RESCODE] SET ResValue = @Translation WHERE ResCodeId = @ResId";
            return query;
        }

    }
}