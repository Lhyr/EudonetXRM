using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{

    /// <summary>
    /// Objet métier représentant la Liste des Traductions
    /// </summary>
    public class eAdminTranslationsList
    {
        private ePref Pref;

        #region propriétés publiques
        /// <summary>Filtre sur Nature</summary>
        public eAdminTranslation.NATURE Nature = eAdminTranslation.NATURE.None;
        /// <summary>Filtre sur Langue </summary>
        public Int32 LangId = -1;
        /// <summary>Filtre sur la recherche</summary>
        public String Search = "";
        /// <summary>Filtre sur un onglet ou une rubrique (dépend du contexte d'appel)</summary>
        public Int32 DescId = 0;
        /// <summary>Diffère du DescId pour les catalogues avancés et les specifs</summary>
        public Int32 ResId = 0;
        /// <summary>ID de la fiche</summary>
        public Int32 FileId = 0;
        /// <summary>Erreur</summary>
        public String Error { get; private set; }
        /// <summary>Liste des traductions obtenues après application des filtres</summary>
        public List<eAdminTranslation> TranslationsList { get; private set; }
        /// <summary>Langues activées sur la base</summary>
        public Dictionary<String, String> ActiveLangs { get; private set; }
        /// <summary>Tris à appliquer à la liste des traductions </summary>
        public List<OrderBy> Sorts = new List<OrderBy>();
        #endregion
        #region Constructeur
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref"></param>
        public eAdminTranslationsList(ePref pref)
        {
            Pref = pref;

            // définition des tris par défaut
            Sorts.Add(new OrderBy(COLS.PATH));
            Sorts.Add(new OrderBy(COLS.SOURCE));
            Sorts.Add(new OrderBy(COLS.SYSID));
            Sorts.Add(new OrderBy(COLS.NATURE));
            Sorts.Add(new OrderBy(COLS.LANGID));
            Sorts.Add(new OrderBy(COLS.TRANSLATION));
        }
        #endregion

        #region Méthodes publiques

        /// <summary>
        /// Chargement des Traductions
        /// </summary>
        public void LoadTranslations()
        {
            Error = "";
            String sError = "";
            TranslationsList = new List<eAdminTranslation>();
            HashSet<Int32> hsLangIds = new HashSet<int>();
            ActiveLangs = eDataTools.GetUsersLang(Pref);
            if (LangId > -1)
            {
                hsLangIds.Add(LangId);
            }
            else
            {

                //Langues Actives
                hsLangIds = new HashSet<int>(ActiveLangs.Keys.Select(k => eLibTools.GetNum(k)));


            }

            eudoDAL dal = eLibTools.GetEudoDAL(Pref);
            dal.OpenDatabase();

            try
            {
                List<String> lstSqlRequests = new List<string>();
                try
                {
                    switch (Nature)
                    {
                        case eAdminTranslation.NATURE.None:
                            lstSqlRequests.AddRange(eSqlTranslations.GetResSql(Nature, hsLangIds, Pref.LangId));
                            lstSqlRequests.AddRange(eSqlTranslations.GetResAdvSql(Nature, hsLangIds, Pref.LangId));
                            lstSqlRequests.AddRange(eSqlTranslations.GetCatalogValueResSql(Nature, hsLangIds, Pref.LangId));
                            lstSqlRequests.AddRange(eSqlTranslations.GetCatalogValueToolTipResSql(Nature, hsLangIds, Pref.LangId));
                            lstSqlRequests.AddRange(eSqlTranslations.GetSpecifResSql(Nature, hsLangIds, Pref.LangId));

                            addMissingSpecifRes(Pref, dal, out sError);
                            if (sError.Length > 0)
                            {
                                throw new Exception(String.Concat("addMissingSpecifRes : ", sError));
                            }


                            break;
                        case eAdminTranslation.NATURE.Tab:
                        case eAdminTranslation.NATURE.Field:
                        case eAdminTranslation.NATURE.PJ:
                            lstSqlRequests.AddRange(eSqlTranslations.GetResSql(Nature, hsLangIds, Pref.LangId));
                            lstSqlRequests.AddRange(eSqlTranslations.GetResAdvSql(Nature, hsLangIds, Pref.LangId));

                            if (DescId % 100 == 0)
                                addMissingTabResAdv(DescId, out sError);
                            break;
                        case eAdminTranslation.NATURE.Catalog:
                            lstSqlRequests.AddRange(eSqlTranslations.GetCatalogValueResSql(Nature, hsLangIds, Pref.LangId));
                            lstSqlRequests.AddRange(eSqlTranslations.GetCatalogValueToolTipResSql(Nature, hsLangIds, Pref.LangId));
                            break;
                        case eAdminTranslation.NATURE.CatalogValue:
                            lstSqlRequests.AddRange(eSqlTranslations.GetCatalogValueResSql(Nature, hsLangIds, Pref.LangId));
                            break;
                        case eAdminTranslation.NATURE.CatalogValueToolTip:
                            lstSqlRequests.AddRange(eSqlTranslations.GetCatalogValueToolTipResSql(Nature, hsLangIds, Pref.LangId));
                            break;
                        case eAdminTranslation.NATURE.WebTab:
                        case eAdminTranslation.NATURE.WebLink:
                            lstSqlRequests.AddRange(eSqlTranslations.GetSpecifResSql(Nature, hsLangIds, Pref.LangId));
                            addMissingSpecifRes(Pref, dal, out sError);
                            if (sError.Length > 0)
                            {
                                throw new Exception(String.Concat("addMissingSpecifRes : ", sError));
                            }
                            break;
                        case eAdminTranslation.NATURE.FileValue:
                            lstSqlRequests.Add(eSqlTranslations.GetFilesResSql(hsLangIds, Pref.LangId, DescId, FileId));
                            if (this.FileId > 0)
                            {
                                if (DescId == TableType.XRMHOMEPAGE.GetHashCode())
                                {
                                    AddMissingFilesRES(dal, TableType.XRMHOMEPAGE.ToString(), XrmHomePageField.Title.ToString(), TableType.XRMHOMEPAGE.ToString() + "ID", this.FileId);
                                    AddMissingFilesRES(dal, TableType.XRMHOMEPAGE.ToString(), XrmHomePageField.Tooltip.ToString(), TableType.XRMHOMEPAGE.ToString() + "ID", this.FileId);

                                }
                                else if (DescId == TableType.XRMGRID.GetHashCode())
                                {
                                    AddMissingFilesRES(dal, TableType.XRMGRID.ToString(), XrmGridField.Title.ToString(), TableType.XRMGRID.ToString() + "ID", this.FileId);
                                }
                                else if (DescId == TableType.XRMWIDGET.GetHashCode())
                                {
                                    AddMissingFilesRES(dal, TableType.XRMWIDGET.ToString(), XrmWidgetField.Title.ToString(), TableType.XRMWIDGET.ToString() + "ID", this.FileId);
                                    AddMissingFilesRES(dal, TableType.XRMWIDGET.ToString(), XrmWidgetField.SubTitle.ToString(), TableType.XRMWIDGET.ToString() + "ID", this.FileId);
                                    AddMissingFilesRES(dal, TableType.XRMWIDGET.ToString(), XrmWidgetField.Tooltip.ToString(), TableType.XRMWIDGET.ToString() + "ID", this.FileId);
                                }
                            }

                            break;
                            //case eAdminTranslation.NATURE.WidgetParam:
                            //    lstSqlRequests.Add(eSqlTranslations.GetWidgetParamsResSQL());
                            //    break;
                    }
                }
                catch (Exception e)
                {
                    Error = String.Concat("LoadTranslations : ", e.Message, Environment.NewLine, e.StackTrace);
                    return;
                }
                finally
                {
                    dal?.CloseDatabase();
                }


                if (lstSqlRequests.Count > 0)
                {
                    //String sSQL = Nature != eAdminTranslation.NATURE.FileValue ? eSqlTranslations.Join(Pref, lstSqlRequests, this) : lstSqlRequests[0];
                    String sSQL = eSqlTranslations.Join(Pref, lstSqlRequests, this);

                    RqParam rq = new RqParam(sSQL);

                    #region Insertion des paramètres

                    rq.AddInputParameter("@PjFieldDid", SqlDbType.Int, (int)AllField.ATTACHMENT);
                    rq.AddInputParameter("@DescId", SqlDbType.Int, DescId);
                    rq.AddInputParameter("@ResId", SqlDbType.Int, ResId);
                    rq.AddInputParameter("@Search", SqlDbType.VarChar, Search);
                    rq.AddInputParameter("@FileId", SqlDbType.Int, this.FileId);

                    foreach (eAdminTranslation.NATURE nature in Enum.GetValues(typeof(eAdminTranslation.NATURE)))
                    {
                        rq.AddInputParameter(String.Format("@{0}Label", nature), SqlDbType.VarChar, eAdminTranslation.GetNatureLabel(Pref, nature));
                    }

                    foreach (eAdminTranslation.LOCATION location in Enum.GetValues(typeof(eAdminTranslation.LOCATION)))
                    {
                        rq.AddInputParameter(String.Format("@Location{0}", location), SqlDbType.Int, (int)location);
                    }

                    foreach (eLibConst.SPECIF_TYPE stype in Enum.GetValues(typeof(eLibConst.SPECIF_TYPE)))
                    {
                        rq.AddInputParameter(String.Format("@SType{0}", stype), SqlDbType.Int, (int)stype);
                    }



                    #endregion

                    try
                    {

                        DataTableReaderTuned dtr = dal.Execute(rq, out sError);
                        if (sError.Length > 0)
                        {
                            Error = String.Concat("LoadTranslations : Erreur lors de l'execution de la requête : ", Environment.NewLine, sError);
                            return;
                        }
                        while (dtr.Read())
                        {

                            int nType = -1;
                            int.TryParse(dtr.GetString("TYPE"), out nType);

                            TranslationsList.Add(new eAdminTranslation()
                            {
                                Source = dtr.GetString("SOURCE"),
                                Translation = dtr.GetString("TRANSLATION"),
                                Path = dtr.GetString("PATH"),
                                NatureLabel = dtr.GetString("NATURE"),
                                SysID = dtr.GetString("SYSID"),
                                LangId = dtr.GetInt32("LANGID"),
                                Lang = GetLangLabel(dtr.GetInt32("LANGID")),
                                Location = (eAdminTranslation.LOCATION)dtr.GetInt32("LOCATION"),
                                Resid = dtr.GetInt32("RESID"),
                                Type = nType
                            });
                        }


                        #region traitements posterieurs

                        #region compléments de libellé pour les resadv

                        TranslationsList.Where(t => t.Location == eAdminTranslation.LOCATION.ResAdv).ToList()
                            .ForEach(t => t.Path += $" ({eAdminTranslation.GetResAdvTypeLabel(Pref, t.Type)})");

                        #endregion

                        #endregion

                    }
                    catch (Exception e)
                    {

                        Error = String.Concat("LoadTranslations : ", e.Message, Environment.NewLine, e.StackTrace, Environment.NewLine, rq.GetSqlCommandText());
                        return;
                    }
                }

                // Ceci doit être géré autrement car ne peut pas s'adapter avec la requête globale
                if (Nature == eAdminTranslation.NATURE.WidgetParam)
                {
                    TranslationsList.AddRange(GetWidgetParamsTranslations(dal));
                }
            }
            finally
            {
                dal?.CloseDatabase();
            }

        }

        /// <summary>
        /// Renvoie le libellé de la langue suivi de son code (ex: Français (LANG_00))
        /// </summary>
        /// <param name="iLangId"></param>
        /// <returns></returns>
        public String GetLangLabel(Int32 iLangId)
        {
            Error = "";
            try
            {
                return String.Format("{0} (LANG_{1})", ActiveLangs[iLangId.ToString()], iLangId.ToString("00"));
            }
            catch (Exception e)
            {
                Error = String.Concat("GetLangLabel(", iLangId, ") : ", e.Message, Environment.NewLine, e.StackTrace);
                throw new Exception(Error);
            }

        }
        #endregion

        #region méthodes privées
        private void addMissingSpecifRes(ePref pref, eudoDAL dal, out string sError)
        {
            sError = "";
            String sql = "";
            try
            {
                sql = eSqlTranslations.InsertMissingResSpecif(pref);
                dal.ExecuteNonQuery(new RqParam(sql), out sError);
            }
            catch (Exception e)
            {
                sError = string.Concat("addMissingSpecifRes : ", e.Message, Environment.NewLine, e.StackTrace, Environment.NewLine, "SQL :", sql);
            }
        }

        private void addMissingTabResAdv(int iTab, out string sError)
        {
            try
            {
                eSqlRes.CreateTabResAdv(Pref, iTab, out sError);
            }
            catch (Exception e)
            {
                sError = string.Concat("addMissingTabResAdv : ", e.Message, Environment.NewLine, e.StackTrace);
            }
        }
        /// <summary>
        /// Adds the missing files resource.
        /// </summary>
        /// <param name="dal">EudoDAL</param>
        /// <param name="tableSQLName">Name of the table SQL.</param>
        /// <param name="fieldSQLName">Name of the field SQL.</param>
        /// <param name="idFieldSQLName">Name of the identifier field SQL.</param>
        /// <param name="fileID">ID de la fiche</param>
        /// <param name="dicResLang">Dictionnaire pour spécifier à la main les Res pour certaines langues</param>
        /// <exception cref="System.Exception">
        /// </exception>
        public static void AddMissingFilesRES(eudoDAL dal, String tableSQLName, String fieldSQLName, String idFieldSQLName, int fileID, Dictionary<int, string> dicResLang = null)
        {
            String error = "";
            String sql = "";
            RqParam rqParam = null;

            try
            {
                Dictionary<string, string> dicAdditionalparameters;
                sql = eSqlTranslations.InsertMissingFilesRes(tableSQLName, fieldSQLName, idFieldSQLName, out dicAdditionalparameters, dicResLang);
                rqParam = new RqParam(sql);
                rqParam.AddInputParameter("@FileId", SqlDbType.Int, fileID);

                foreach (KeyValuePair<string, string> additionalparameter in dicAdditionalparameters)
                {
                    rqParam.AddInputParameter(additionalparameter.Key, SqlDbType.NVarChar, additionalparameter.Value);
                }

                dal.ExecuteNonQuery(rqParam, out error);
                if (!String.IsNullOrEmpty(error))
                    throw new Exception(string.Concat("AddMissingFilesRES : ", error, Environment.NewLine, "SQL :", sql));
            }
            catch (Exception e)
            {
                throw new Exception(string.Concat("AddMissingFilesRES : ", e.Message, Environment.NewLine, e.StackTrace, Environment.NewLine, "SQL :", sql));
            }
        }

        /// <summary>
        /// Renvoie le libellé du chemin à partir de la valeur dans RESLOCATION.PATH
        /// </summary>
        /// <param name="listRes">Liste des obj eResLocation à mettre à jour au fur-et-à-mesure</param>
        /// <param name="pref">ePref</param>
        /// <param name="dal">eudoDAL</param>
        /// <param name="resLocId">RESLOCATION ResLocationid</param>
        /// <param name="path">Chemin</param>
        /// <returns></returns>
        private static string GetWidgetLabelByPath(ref List<eResLocation> listRes, ePref pref, eudoDAL dal, int resLocId, string path)
        {

            StringBuilder sbLabel = new StringBuilder();
            string error = string.Empty;

            eResLocation resLoc = listRes.Find(res => res.IdRes == resLocId);

            if (resLoc != null)
            {
                return resLoc.PathLabel;
            }
            else
            {
                Dictionary<int, string> dicLabels = new Dictionary<int, string>();

                string query = eSqlTranslations.BuildWidgetParamPathLabelSQL(pref, path, pref.LangId);

                DataTableReaderTuned dtr = dal.Execute(new RqParam(query), out error);

                if (error.Length > 0)
                    return eResApp.GetRes(pref, 416); // Erreur

                while (dtr.Read())
                {
                    dicLabels.Add(dtr.GetInt32("ID"), dtr.GetString("VALUE"));
                }
                dtr.Dispose();

                string[] arrPath = path.Split('/');
                string part;
                string[] arrPart;
                int tab = 0;

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < arrPath.Length; i++)
                {
                    if (sb.Length > 0)
                        sb.Append(" - ");

                    part = arrPath[i];
                    arrPart = part.Split('-');
                    if (arrPart.Length == 0 || arrPart.Length > 2)
                        continue;

                    tab = eLibTools.GetNum(arrPart[0]);
                    if (tab == 0)
                        continue;

                    if (tab == (int)TableType.XRMGRID)
                    {
                        sb.Append(eResApp.GetRes(pref, 8065));
                    }
                    else if (tab == (int)TableType.XRMWIDGET)
                    {
                        sb.Append(eResApp.GetRes(pref, 8719));
                    }
                    else if (tab == (int)TableType.XRMHOMEPAGE)
                    {
                        sb.Append(eResApp.GetRes(pref, 8071));
                    }
                    else
                        sb.Append(eResApp.GetRes(pref, 264));

                    if (dicLabels.ContainsKey(tab))
                        sb.Append($" <{dicLabels[tab]}>");
                    else
                        sb.Append($" <{ eResApp.GetRes(pref, 776)}>"); // Introuvable
                }

                listRes.Add(new eResLocation(resLocId, path, sb.ToString()));
                return sb.ToString();


            }



        }



        /// <summary>
        /// Retourne la liste des traductions des params du widget
        /// </summary>
        /// <param name="dal">eudoDAL</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        private List<eAdminTranslation> GetWidgetParamsTranslations(eudoDAL dal)
        {
            string error = string.Empty;

            List<eAdminTranslation> trList = new List<eAdminTranslation>();
            List<eResLocation> resLocations = new List<eResLocation>();
            int resLocId = 0;
            int langId = 0;

            string query = eSqlTranslations.GetWidgetParamsTranslationsSQL(this.LangId, this.FileId, this.Sorts);
            RqParam rq = new RqParam(query);
            rq.AddInputParameter("@langId", SqlDbType.Int, this.LangId);
            DataTableReaderTuned dtr = dal.Execute(rq, out error);

            if (!String.IsNullOrEmpty(error))
                throw new Exception(error);

            while (dtr.Read())
            {
                resLocId = dtr.GetInt32("LOCID");
                langId = dtr.GetInt32("LANGID");

                if (ActiveLangs.ContainsKey(langId.ToString()))
                {
                    trList.Add(new eAdminTranslation()
                    {
                        Source = dtr.GetString("SOURCE"),
                        Translation = dtr.GetString("TRANSLATION"),
                        Path = GetWidgetLabelByPath(ref resLocations, this.Pref, dal, resLocId, dtr.GetString("PATH")),
                        NatureLabel = eAdminTranslation.GetNatureLabel(this.Pref, eAdminTranslation.NATURE.WidgetParam),
                        SysID = eAdminTranslation.GetWidgetParamLabel(this.Pref, dtr.GetString("SYSID")),
                        LangId = langId,
                        Lang = GetLangLabel(langId),
                        Location = eAdminTranslation.LOCATION.ResCode,
                        Resid = dtr.GetEudoNumeric("RESID")
                    });

                }


            }

            return trList;
        }

        #endregion

        #region enum
        /// <summary>
        /// représente les colonnes affichées dans la liste des traductions
        /// En cas de modification de l'ordre de ces colonnes, il faut également modifier l'ordre de l'enum.
        /// </summary>
        public enum COLS
        {
            SOURCE,
            TRANSLATION,
            PATH,
            NATURE,
            SYSID,
            LANGID
        }
        #endregion



        /// <summary>
        /// Classe permettant d'affecter un ou plusieurs tri à la liste
        /// </summary>
        [DataContract]
        public class OrderBy
        {
            /// <summary>
            /// Colonne
            /// </summary>
            [DataMember]
            public eAdminTranslationsList.COLS Col;
            /// <summary>
            /// Sens du tri
            /// </summary>
            [DataMember]
            public SORT Sort = SORT.ASC;

            /// <summary>
            /// Constructeur
            /// </summary>
            /// <param name="col"></param>
            /// <param name="sort"></param>
            public OrderBy(eAdminTranslationsList.COLS col, SORT sort = SORT.ASC)
            {
                Col = col;
                Sort = sort;
            }

            /// <summary>
            /// Overrid de la méthode ToString pour que l'objet OrderBy puisse être implémenté directement à une reqête SQL
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return String.Concat(Col, " ", Sort);
            }

            /// <summary>
            /// Enum représentant le sens du Tri
            /// </summary>
            public enum SORT
            {
                /// <summary>Ascendant </summary>
                ASC,
                /// <summary>Descendant</summary>
                DESC
            }
        }

    }

    /// <summary>
    /// objet représentant une traduction
    /// </summary>
    [DataContract]
    public class eAdminTranslation
    {
        public String Error { get; private set; }
        public String Source;
        public String Path;
        public String NatureLabel;
        public String SysID;
        public String Lang;

        /// <summary>Type pour les resadv</summary>
        [DataMember(Name = "tp")]
        public int Type { get; set; }

        [DataMember(Name = "loc")]
        public eAdminTranslation.LOCATION Location;
        [DataMember(Name = "id")]
        public Int32 Resid;
        [DataMember(Name = "lng")]
        public Int32 LangId;
        [DataMember(Name = "tl")]
        public String Translation;


        private ePref _pref;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Location : ").Append(Location).AppendLine()
                .Append("Resid : ").Append(Resid).AppendLine()
                .Append("LangId : ").Append(LangId).AppendLine()
                .Append("Translation : ").Append(Translation).AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="eAdminTranslation" /> class.
        /// </summary>
        public eAdminTranslation()
        {
            Error = "";
        }

        /// <summary>
        /// Emplacement des traductions
        /// </summary>
        public enum LOCATION
        {
            /// <summary>Res classiques tables et rubriques </summary>
            Res,
            /// <summary> Res des onglets web et liens de droite</summary>
            Res_Specifs,
            /// <summary>Traduction des valeurs de catalogues</summary>
            FileData,
            /// <summary> Traduction des tooltips des valeurs de catalogue</summary>
            FileData_ToolTips,
            /// <summary>Traduction des RES de valeurs de fiches</summary>
            Res_Files,
            /// <summary>
            /// Traduction par code
            /// </summary>
            ResCode,
            /// <summary> Traductions dans les ResAdv </summary>
            ResAdv
        }

        /// <summary>
        /// Nature de la traduction
        /// </summary>
        public enum NATURE
        {
            None,
            Tab,
            Field,
            PJ,
            Catalog,
            CatalogValue,
            CatalogValueToolTip,
            WebTab,
            WebLink,
            FileValue,
            /// <summary> The widget parameter</summary>
            WidgetParam
        }

        /// <summary>
        /// Retourne le libellé de la nature
        /// </summary>
        /// <param name="Pref">The preference.</param>
        /// <param name="Nature">The nature.</param>
        /// <returns></returns>
        public static String GetNatureLabel(ePref Pref, NATURE Nature)
        {
            Int32 iResId = 0;

            switch (Nature)
            {
                case NATURE.None:
                    iResId = 435;
                    break;
                case NATURE.Tab:
                    iResId = 264;
                    break;
                case NATURE.Field:
                    iResId = 222;
                    break;
                case NATURE.PJ:
                    iResId = 5042;
                    break;
                case NATURE.Catalog:
                    iResId = 225;
                    break;
                case NATURE.CatalogValue:
                    iResId = 6828;
                    break;
                case NATURE.CatalogValueToolTip:
                    return String.Format("{0} ({1})", eResApp.GetRes(Pref, 6828), eResApp.GetRes(Pref, 7557));
                case NATURE.WebTab:
                    iResId = 6945;
                    break;
                case NATURE.WebLink:
                    iResId = 7780;
                    break;
                case NATURE.FileValue:
                    iResId = 8287;
                    break;
                case NATURE.WidgetParam:
                    iResId = 8716;
                    break;
                default:
                    break;
            }
            if (iResId == 0)
                return "";

            return eResApp.GetRes(Pref, iResId);

        }

        /// <summary>
        /// Gets the widget parameter label.
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="identifier">The identifier.</param>
        /// <returns></returns>
        public static string GetWidgetParamLabel(ePref pref, string identifier)
        {
            switch (identifier)
            {
                case "unit":
                    return eResApp.GetRes(pref, 1353);
                case "libelle":
                    return eResApp.GetRes(pref, 223);
                case "kanban-aggr-unit":
                    return eResApp.GetRes(pref, 8717);
                case "kanban-aggr-label":
                    return eResApp.GetRes(pref, 8718);
            }
            return string.Empty;
        }


        /// <summary>
        /// Fournit le label correspondant au type de resadv 
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="iType"></param>
        /// <returns></returns>
        public static string GetResAdvTypeLabel(ePref pref, int iType)
        {
            eLibConst.RESADV_TYPE type;
            try
            {
                type = (eLibConst.RESADV_TYPE)iType;
            }
            catch (InvalidCastException e)
            {
                return "";
            }

            switch (type)
            {
                case eLibConst.RESADV_TYPE.WATERMARK:
                    break;
                case eLibConst.RESADV_TYPE.TOOLTIP:
                    break;
                case eLibConst.RESADV_TYPE.FEATURE:
                    break;
                case eLibConst.RESADV_TYPE.RESULT_LABEL_SINGULAR:
                    return eResApp.GetRes(pref, 2699);
                    break;
                case eLibConst.RESADV_TYPE.RESULT_LABEL_PLURAL:
                    return eResApp.GetRes(pref, 2700);
                    break;
                default:
                    break;
            }
            return "";
        }

        #region méthodes de mise à jour        
        /// <summary>
        /// Mise à jour de traduction
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <returns></returns>
        /// <exception cref="EudoAdminInvalidRightException"></exception>
        public eAdminResult Update(ePref pref)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();



            string sError = "";
            _pref = pref;
            eAdminResult adminResult = new eAdminResult();
            eudoDAL dal = eLibTools.GetEudoDAL(pref);
            dal.OpenDatabase();
            try
            {


                //Pour les modifications de valeur de catalogue, on vérifie les droits sur le catalogue en question
                if ((this.Location == LOCATION.FileData || this.Location == LOCATION.FileData_ToolTips)
                    && pref.User.UserLevel < (int)UserLevel.LEV_USR_PRODUCT)
                {
                    RqParam rqGetResInfo = new RqParam("SELECT [descid] FROM [FILEDATA] where dataid = @resid");
                    rqGetResInfo.AddInputParameter("@resid", SqlDbType.Int, this.Resid);
                    string serr = "";
                    var res = dal.Execute(rqGetResInfo, out serr);
                    if (serr.Length > 0)
                    {
                        adminResult.Success = false;
                        adminResult.UserErrorMessage = "Mise à jour Impossible.";
                        adminResult.InnerException = dal.InnerException;
                        return adminResult;
                    }

                    if (res.Read())
                    {
                        int nD = res.GetEudoNumeric("descid");
                        if (nD > 0)
                        {
                            eAdminFiledataParam fdb = eSqlFiledataParam.GetFiledataParam(dal, nD);
                            if (fdb.UpdatePermission > 0)
                            {
                                ePermission eMode = new ePermission(fdb.UpdatePermission, dal);
                                if (eMode != null && eMode.PermLevel >= (int)UserLevel.LEV_USR_PRODUCT)
                                {
                                    adminResult.Success = false;
                                    adminResult.UserErrorMessage = eResApp.GetRes(pref, 6434);
                                    adminResult.InnerException = dal.InnerException;
                                    return adminResult;
                                }
                            }
                        }
                    }

                    else
                    {
                        adminResult.Success = false;
                        adminResult.UserErrorMessage = eResApp.GetRes(pref, 6696);
                        adminResult.InnerException = dal.InnerException;
                        return adminResult;
                    }


                }

                if (Location == LOCATION.Res)
                {
                    eAdminRes adminRes = new eAdminRes(pref);
                    HashSet<SetParam<int>> hslang = new HashSet<SetParam<int>>();
                    hslang.Add(new SetParam<int>(LangId, Translation));
                    adminResult = adminRes.SetRes(Resid, hslang);
                }
                else if (Location == LOCATION.ResAdv)
                {
                    eAdminRes adminRes = new eAdminRes(pref);
                    List<SetParam<KeyResADV>> lstRes = new List<SetParam<KeyResADV>>();
                    lstRes.Add(new SetParam<KeyResADV>(new KeyResADV((eLibConst.RESADV_TYPE)Type, Resid, LangId), Translation));
                    adminResult = adminRes.SetResAdv(Resid, lstRes);
                }
                else
                {
                    String sSql = "";
                    switch (Location)
                    {
                        case eAdminTranslation.LOCATION.Res_Specifs:
                            sSql = eSqlTranslations.UpdateResSpecifs(LangId);
                            break;
                        case eAdminTranslation.LOCATION.FileData:
                            sSql = eSqlTranslations.UpdateFileData(LangId);
                            break;
                        case eAdminTranslation.LOCATION.FileData_ToolTips:
                            sSql = eSqlTranslations.UpdateFileDataToolTip(LangId);
                            break;
                        case LOCATION.Res_Files:
                            sSql = eSqlTranslations.UpdateFilesRes(LangId);
                            break;
                        case LOCATION.ResCode:
                            sSql = eSqlTranslations.UpdateResCode();
                            break;
                        default:
                            break;
                    }
                    RqParam rq = new RqParam(sSql);
                    rq.AddInputParameter("@ResId", SqlDbType.Int, Resid);
                    rq.AddInputParameter("@Translation", SqlDbType.VarChar, Translation);
                    dal.ExecuteNonQuery(rq, out sError);
                    if (sError.Length > 0)
                    {
                        adminResult.Success = false;
                        adminResult.DebugErrorMessage = String.Concat("eAdminTranslation.Update() : ", Environment.NewLine, this, sError);
                        adminResult.UserErrorMessage = eResApp.GetRes(pref, 7657);
                        adminResult.Criticity = ((int)eLibConst.MSG_TYPE.CRITICAL);
                        Error = sError;
                    }
                    else
                    {
                        adminResult.Success = true;

                    }
                }
            }
            catch (Exception e)
            {
                adminResult.Success = false;
                Error = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
                adminResult.DebugErrorMessage = String.Concat("eAdminTranslation.Update() : ", Environment.NewLine, this, Environment.NewLine, Error);
                adminResult.UserErrorMessage = eResApp.GetRes(pref, 7657);
                adminResult.Criticity = ((int)eLibConst.MSG_TYPE.CRITICAL);
            }
            finally
            {
                dal?.CloseDatabase();
                adminResult.Result.Add(this);
            }
            return adminResult;
        }


        #endregion
    }

    /// <summary>
    /// classe incluant les infiormations à présenter dans le json pour pouvoir identifier au niveau du js les informations à mettre à jour
    /// </summary>
    [DataContract]
    public class eAdminTranslationUpdateInfo
    {
        protected eAdminTranslation _translation;

        [DataMember(Name = "loc")]
        public eAdminTranslation.LOCATION Location { get { return _translation.Location; } }
        [DataMember(Name = "id")]
        public Int32 Resid { get { return _translation.Resid; } }
        [DataMember(Name = "lng")]
        public Int32 LangId { get { return _translation.LangId; } }

        [DataMember(Name = "tp")]
        public int Type { get { return _translation.Type; } }

        public eAdminTranslationUpdateInfo(eAdminTranslation translation)
        {
            _translation = translation;
        }
    }


}