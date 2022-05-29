using Com.Eudonet.Internal;
using EudoProcessInterfaces;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Linq;
using EudoQuery;
using System.Data;
using Com.Eudonet.Merge;
using System.Text;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal.wcfs.data.report;
using Com.Eudonet.Internal.wcfs.data.common;

namespace Com.Eudonet.Xrm
{

    /******************************************************
    'DEVELOPPEUR  : Simon PHAM
    'DATE	:  26/07/2013
    'VERSION	:  XRM ALPHA
    'DESCRIPTION  : Classe de rendu de la liste des export/publipostage 
    '				de l'utilisateur en cours
    '******************************************************/

    /// <summary>
    /// Classe de rendu pour la liste des export
    /// Les exports (publipostages, etc...) étant stockées sur une base différente
    /// et utilisant un système différent des tables "classiques", cette classe n'hérite pas de eRenderer
    /// </summary>
    public class eReportUserListRenderer
    {
        private IEnumerable<IReportInfos> lst;
        private Exception _eException;
        private ePref _pref;
        private Boolean _bOnlyArchived = false;
        private int _importLifetimeInDays = eLibConst.IMPORT_FILES_RETENTION_POLICY_DEFAULT;


        /// <summary>
        /// Exception Interne
        /// </summary>
        public Exception InnerException
        {
            get { return _eException; }
        }



        /// <summary>Div global rendu par le renderer</summary>
        private Panel _pgContainer = new Panel();

        /// <summary>Div global rendu par le renderer</summary>
        public Panel PgContainer
        {
            get { return _pgContainer; }

        }

        /// <summary>Message d'erreur</summary>
        protected String _sErrorMsg = String.Empty;

        /// <summary>numéro de l'erreur</summary>
        /// <summary>
        /// Constructeur
        /// </summary>
        private eReportUserListRenderer(ePref pref)
        {
            lst = new List<IReportInfos>();
            _pref = pref;
        }


        /// <summary>
        /// Retourne un renderer contenant la liste des rapports du user
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="bOnlyArchived"></param>
        /// <returns></returns>
        public static eReportUserListRenderer GetUserListRenderer(ePref pref, Boolean bOnlyArchived)
        {

            eReportUserListRenderer er = new eReportUserListRenderer(pref);
            er._bOnlyArchived = bOnlyArchived;
            if (er.Generate())
                return er;
            else
            {
                if (er.InnerException != null)
                    throw er.InnerException;
                else
                    throw new Exception(er._sErrorMsg);
            }

        }



        /// <summary>
        /// appel la séquence de génération renderer
        /// </summary>
        /// <returns></returns>
        private Boolean Generate()
        {

            if (!GetList())
                return false;

            if (!BuildHTML())
                return false;

            return true;
        }

        /// <summary>
        /// Création de la liste de report
        /// </summary>
        /// <returns></returns>
        private bool GetList()
        {
            try
            {

                lst = eReport.GetReportList(_pref, _bOnlyArchived);
                if (lst == null)
                    lst = new List<IReportInfos>();

                lst = lst.Union(GetImportReport(_pref));
            }
            catch (Exception e)
            {
                _eException = e;
                _sErrorMsg = e.Message;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Retourne la liste des Import terminé de l'utilisateur en cours
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        private IEnumerable<eServerImportInfos> GetImportReport(ePref pref)
        {
            if (_bOnlyArchived)
                return new List<eServerImportInfos>();

            if (lst.Where(e => e.ReportType == (int)ReportType.REPORT_IMPORT).Count() > 0)
            {
                var val = eLibTools.GetConfigAdvValues(_pref, eLibConst.CONFIGADV.IMPORT_FILES_RETENTION_POLICY)[eLibConst.CONFIGADV.IMPORT_FILES_RETENTION_POLICY];
                if (!int.TryParse(val, out _importLifetimeInDays))
                    _importLifetimeInDays = eLibConst.IMPORT_FILES_RETENTION_POLICY_DEFAULT;
            }

            eServerImportInfos s = new eServerImportInfos();

            // Recherche si le user à des reports d'import disponibles
            RqParam rqImportReportList = new RqParam();
            rqImportReportList.SetQuery(
                @" SELECT 
                      Pj.Pjid as ReportId,
                      Pj.Libelle as Label,  
                      Pj.TooltipText as Tooltip,   
                      server.id as ServerReportId,
                      server.userId as UserId,
                      0 as Archived,
                      server.status as Status, 
                      '' as WebPath,
                      server.dateCreation as DateCrea,
                      isnull(server.dateEnd, server.dateStart) as DateRun,
                      server.dataSourceName as ReportName,
                      server.importSettings as Param                   
                                    
                   FROM [EUDOTRAIT].[dbo].[ServerImport] server
                   INNER JOIN [PJ] ON CHARINDEX('Import_' + cast(server.id as varchar(18)) +'_', pj.libelle) > 0 
                                      AND (Pj.[Type] = @ImportType OR Pj.[Type] = @ImportReportType)      
                                      AND Pj.[File] = @File
                                      AND Pj.FileId = @FileId
                   WHERE DATEADD(DAY, @daysOld, isnull(server.dateEnd, server.dateStart)) > GetDate()  ");

            rqImportReportList.AddInputParameter("@ImportType", SqlDbType.Int, (int)PjType.IMPORT);
            rqImportReportList.AddInputParameter("@ImportReportType", SqlDbType.Int, (int)PjType.IMPORT_REPORTS);
            rqImportReportList.AddInputParameter("@FileId", SqlDbType.Int, pref.UserId);
            rqImportReportList.AddInputParameter("@daysOld", SqlDbType.Int, _importLifetimeInDays);
            rqImportReportList.AddInputParameter("@basename", SqlDbType.VarChar, pref.GetBaseName);
            rqImportReportList.AddInputParameter("@File", SqlDbType.VarChar, TableType.USER.ToString());


            Dictionary<int, eServerImportInfos> lookup = new Dictionary<int, eServerImportInfos>();

            using (eudoDAL dal = eLibTools.GetEudoDAL(pref))
            {
                dal.OpenDatabase();

                string error;
                DataTableReaderTuned dtr = dal.Execute(rqImportReportList, out error);
                if (!string.IsNullOrEmpty(error))
                    throw new Exception(error);

                if (dtr == null)
                    return new List<eServerImportInfos>();




                string alias = eResApp.GetRes(_pref, 6344);
                while (dtr.Read())
                {
                    int serverReportId = dtr.GetEudoNumeric("ServerReportId");
                    if (lookup.ContainsKey(serverReportId))
                    {
                        lookup[serverReportId].Attachments.Add(new Tuple<string, string>(GetSecuredLink(dtr.GetEudoNumeric("ReportId")), dtr.GetString("Tooltip")));
                    }
                    else
                    {
                        lookup[serverReportId] = new eServerImportInfos()
                        {
                            ReportId = dtr.GetEudoNumeric("ReportId"),
                            ServerReportId = dtr.GetEudoNumeric("ServerReportId"),
                            UserId = dtr.GetEudoNumeric("UserId"),
                            Archived = dtr.GetString("Archived").Equals("1"),
                            Status = (eProcessStatus)Enum.Parse(typeof(eProcessStatus), dtr.GetInt32("Status").ToString()),
                            WebPath = dtr.GetString("WebPath"),
                            ReportType = (int)ReportType.REPORT_IMPORT,
                            DateCrea = dtr.GetDateTime("DateCrea"),
                            DateRun = dtr.GetDateTime("DateRun"),
                            ReportName = string.Concat(alias, " \"", dtr.GetString("ReportName"), "\""),
                            Param = dtr.GetString("Param"),
                            Attachments = new List<Tuple<string, string>>() { new Tuple<string, string>(GetSecuredLink(dtr.GetEudoNumeric("ReportId")), dtr.GetString("Tooltip")) }
                        };
                    }
                }

                return lookup.Values;
            }
        }

        /// <summary>
        /// Retourne le lien sécurisé de l'annexe spécifiée qui est liée à la table User
        /// </summary>
        /// <param name="pjId"></param>
        /// <returns></returns>
        private string GetSecuredLink(int pjId)
        {
            PjBuildParam paramPj = new PjBuildParam()
            {
                AppExternalUrl = _pref.AppExternalUrl,
                Uid = _pref.DatabaseUid,
                TabDescId = (int)TableType.USER,
                PjId = pjId,
                UserId = _pref.UserId,
                UserLangId = _pref.LangId
            };

            return ExternalUrlTools.GetLinkPJ(paramPj);
        }

        /// <summary>
        /// Construit le HTML du rendu
        /// </summary>
        /// <returns></returns>
        private Boolean BuildHTML()
        {
            _pgContainer.ID = "divReportListId";

            System.Web.UI.WebControls.Table tblList = new System.Web.UI.WebControls.Table();
            _pgContainer.Controls.Add(tblList);
            tblList.CssClass = "eTblReportList mTab";


            //Entête
            TableHeaderRow trHeader = new TableHeaderRow();
            tblList.Controls.Add(trHeader);

            TableHeaderCell th = new TableHeaderCell();
            th.Attributes.Add("width", "35%");
            th.Text = eResApp.GetRes(_pref, 1153);  //Rapport
            th.Attributes.Add("title", eResApp.GetRes(_pref, 1153));
            trHeader.Controls.Add(th);

            th = new TableHeaderCell();
            th.Attributes.Add("width", "10%");
            th.Text = eResApp.GetRes(_pref, 105);  // Type
            th.Attributes.Add("title", eResApp.GetRes(_pref, 105));
            trHeader.Controls.Add(th);

            th = new TableHeaderCell();
            th.Attributes.Add("width", "10%");
            th.Text = eResApp.GetRes(_pref, 5093);  // Statut
            th.Attributes.Add("title", eResApp.GetRes(_pref, 5093));
            trHeader.Controls.Add(th);

            th = new TableHeaderCell();
            th.Attributes.Add("width", "10%");
            th.Text = eResApp.GetRes(_pref, 1500); // Lien
            th.Attributes.Add("title", eResApp.GetRes(_pref, 1500));
            trHeader.Controls.Add(th);

            th = new TableHeaderCell();
            th.Attributes.Add("width", "15%");
            th.Text = eResApp.GetRes(_pref, 231); // Date
            th.Attributes.Add("title", eResApp.GetRes(_pref, 231));
            trHeader.Controls.Add(th);

            th = new TableHeaderCell();
            th.Attributes.Add("width", "15%");
            th.Text = "Disponibilté"; // Disponibilté
            th.Attributes.Add("title", "Disponibilté");
            trHeader.Controls.Add(th);


            th = new TableHeaderCell();
            th.Attributes.Add("width", "10%");
            th.Text = eResApp.GetRes(_pref, 6138); // archivé
            th.Attributes.Add("title", eResApp.GetRes(_pref, 6138));
            trHeader.Controls.Add(th);


            if (lst.Count() > 0)
            {
                lst.OrderBy(e => e.DateRun);
                bool bAltLine = false;

                foreach (IReportInfos er in lst)
                {
                    TableRow tr = new TableRow();
                    tblList.Controls.Add(tr);


                    //Libellé
                    TableCell td = new TableCell();
                    if (er.ReportName.Length == 0 && er.ReportId == -1)
                        td.Text = String.Concat(eResApp.GetRes(_pref, 958), " ", GetTabRes(er.Param), ""); // TODO : si liste en cours, récupérer le nom de la table
                    else
                        td.Text = er.ReportName;
                    tr.Controls.Add(td);


                    //Type
                    td = new TableCell();
                    switch (er.ReportType)
                    {
                        case (int)ReportType.REPORT_PRINT:
                            td.Text = eResApp.GetRes(_pref, 6451);
                            break;
                        case (int)ReportType.REPORT_MERGE:
                            td.Text = eResApp.GetRes(_pref, 438);
                            break;
                        case (int)ReportType.REPORT_EXPORT:
                            td.Text = eResApp.GetRes(_pref, 7538);
                            break;
                        case (int)ReportType.REPORT_IMPORT:
                            td.Text = eResApp.GetRes(_pref, 6340);
                            break;
                        default:
                            td.Text = eResApp.GetRes(_pref, 75);
                            break;
                    }
                    tr.Controls.Add(td);

                    //Statut
                    td = new TableCell();
                    switch (er.Status)
                    {
                        case eProcessStatus.WAIT:
                            td.Text = eResApp.GetRes(_pref, 1715);
                            break;
                        case eProcessStatus.RUNNING:
                            td.Text = eResApp.GetRes(_pref, 1714);
                            break;
                        case eProcessStatus.ERROR:
                            td.Text = eResApp.GetRes(_pref, 1713);
                            break;
                        case eProcessStatus.SUCCESS:
                            td.Text = eResApp.GetRes(_pref, 1712);
                            break;
                        case eProcessStatus.MAIL_ERROR:
                            td.Text = eResApp.GetRes(_pref, 1713);
                            break;
                        default:
                            break;
                    }

                    tr.Controls.Add(td);

                    //Lien
                    td = new TableCell();
                    if (er.Status == eProcessStatus.SUCCESS || er.Status == eProcessStatus.MAIL_ERROR)
                    {
                        foreach (var e in er.Attachments)
                        {
                            HyperLink icnSpan = new HyperLink();
                            td.Controls.Add(icnSpan);
                            icnSpan.Attributes.Add("href", e.Item1);
                            icnSpan.Target = "_blank";
                            icnSpan.CssClass = "icon-annex";

                            if (!string.IsNullOrEmpty(e.Item2))
                                icnSpan.Attributes.Add("title", e.Item2);
                            else
                                icnSpan.Attributes.Add("title", eResApp.GetRes(_pref, 5002));
                        }
                    }

                    tr.Controls.Add(td);

                    // Date de lancement
                    td = new TableCell();
                    td.Text = er.DateRun.ToString();
                    tr.Controls.Add(td);

                    // Disponibilté
                    td = new TableCell();

                    if (er.ReportType == (int)ReportType.REPORT_IMPORT)
                        td.Text = er.DateRun.AddDays(_importLifetimeInDays).ToString();
                    else if (er.Archived)
                        td.Text = "Sans limite";
                    else
                        td.Text = er.DateRun.AddHours(eLibConst.REPORT_LIFETIME_HOURS).ToString();

                    tr.Controls.Add(td);

                    //Archivé
                    td = new TableCell();


                    eCheckBoxCtrl chkFilter = new eCheckBoxCtrl(er.Archived, false);
                    chkFilter.ID = String.Concat("chk", er.ServerReportId);
                    chkFilter.AddClick(String.Concat("archiveReport(", er.ServerReportId, ");"));

                    if (er.ReportType == (int)ReportType.REPORT_IMPORT)
                        chkFilter.SetDisabled(true);

                    td.Controls.Add(chkFilter);

                    tr.Controls.Add(td);

                    if (bAltLine)
                    {
                        tr.CssClass = "line2";
                        bAltLine = false;
                    }
                    else
                    {
                        tr.CssClass = "line1";
                        bAltLine = true;
                    }

                }

            }
            else
            {
                // Pas de rapport

                TableRow trNoReport = new TableRow();
                tblList.Controls.Add(trNoReport);

                TableCell tdNoReport = new TableCell();
                trNoReport.Controls.Add(tdNoReport);
                tdNoReport.CssClass = "eTblReportNoReport";
                tdNoReport.ColumnSpan = 7;

                tdNoReport.Text = eResApp.GetRes(_pref, 6134);
            }


            //
            TableRow trFooter = new TableRow();
            tblList.Controls.Add(trFooter);

            TableCell tdFooter = new TableCell();
            trFooter.Controls.Add(tdFooter);

            tdFooter.ColumnSpan = 7;
            tdFooter.CssClass = "eTblReportListFoot";


            HyperLink hlink = new HyperLink();
            hlink.CssClass = "btnGreen";
            tdFooter.Controls.Add(hlink);

            hlink.Text = eResApp.GetRes(_pref, _bOnlyArchived ? 6135 : 6136);
            //hlink.NavigateUrl = String.Concat("javascript:reloadReportList(", _bOnlyArchived ? "1" : "0", ")");
            hlink.Attributes.Add("onclick", String.Concat("javascript:reloadReportList(", _bOnlyArchived ? "1" : "0", ")"));


            return true;
        }
        Dictionary<int, string> libelleCache = new Dictionary<int, string>();

        private string GetTabRes(string parameters)
        {
            int tab = 0;
            int parentTab = 0;
            int parentFileId = 0;

            TryParseValueParam<int>(parameters, "reportTab", (resultValue) => tab = resultValue, () => tab = 0);
            TryParseValueParam<int>(parameters, "reportTabFrom", (resultValue) => parentTab = resultValue, () => parentTab = 0);
            TryParseValueParam<int>(parameters, "reportTab", (resultValue) => parentFileId = resultValue, () => parentFileId = 0);


            if (tab == 0 && parentTab == 0)
                return string.Empty;


            string format = "de \"{0}\"{1}";
            if (parentTab > 0)            
                format = "de \"{0}\" depuis \"{1}\"";            

            string parentTabLibelle = string.Empty;
            string tabLibelle = string.Empty;

            // Dans le cas de l'onglet
            if (tab > 0 && libelleCache.ContainsKey(tab))
            {
                tabLibelle = libelleCache[tab];
                if (parentTab == 0)
                    return string.Format(format, tabLibelle, parentTabLibelle);
            }

            // Dans le cas du signet
            if (parentTab > 0 && libelleCache.ContainsKey(parentTab))            
                parentTabLibelle = libelleCache[parentTab];               
            

            if (!string.IsNullOrEmpty(tabLibelle) && !string.IsNullOrEmpty(parentTabLibelle))
                return string.Format(format, tabLibelle, parentTabLibelle);

            using (eudoDAL dal = eLibTools.GetEudoDAL(_pref))
            {

                dal.OpenDatabase();
                if (tab > 0 && string.IsNullOrEmpty(tabLibelle))
                {
                    tabLibelle = eLibTools.GetFullNameByDescId(dal, tab, _pref.Lang);
                    libelleCache[tab] = tabLibelle;
                }

                if (parentTab > 0 && string.IsNullOrEmpty(parentTabLibelle))
                {
                    parentTabLibelle = eLibTools.GetFullNameByDescId(dal, parentTab, _pref.Lang);
                    libelleCache[parentTab] = parentTabLibelle;
                }
            }

            return string.Format(format, tabLibelle, parentTabLibelle);
        }

        /// <summary>
        /// recupère la valeur
        /// </summary>
        /// <param name="parameters">liste des parametres séparé par des e commercial</param>
        /// <param name="paramName">nom du param a chercher</param>
        /// <param name="whenFound">fonction executée quand le param est trouve avec le bon format de la valeur</param>
        /// <param name="whenNotFoundOrInvalidFormat">fonction executée quand le param n'est pas trouvé ou format non valide</param>
        private static void TryParseValueParam<T>(string parameters, string paramName, Action<T> whenFound, Action whenNotFoundOrInvalidFormat)
        {
            if (parameters.Length > 0 && parameters.Contains(string.Concat("&", paramName, "=")))
            {
                string[] aParam = parameters.Split('&');

                foreach (string param in aParam)
                {
                    // Ajout du test << param.Contains("=") >> pour eviter les faux paramètres
                    if (string.IsNullOrEmpty(param) || !param.Contains("="))
                        continue;

                    string[] aMyParam = param.Split('=');
                    if (!string.IsNullOrEmpty(aMyParam[1]) && aMyParam[0].ToLower() == paramName.ToLower())
                    {
                        try
                        {
                            T value = (T)Convert.ChangeType(aMyParam[1], typeof(T));
                            whenFound(value);
                            return;
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                }
            }

            // Si on a pas trouvé ou format non valid
            whenNotFoundOrInvalidFormat();
        }

    }



    /// <summary>
    /// Infos sur un import terminé
    /// </summary>

    public class eServerImportInfos : IReportInfos
    {
        /// <summary>ID du rapport</summary>
        public int ReportId { get; set; }

        /// <summary>ID du Sever rapport</summary>
        public int ServerReportId { get; set; }

        /// <summary>Id du user</summary>
        public int UserId { get; set; }

        /// <summary>Indique si le rapport est archivé</summary>
        public bool Archived { get; set; }

        /// <summary>Status</summary>
        public eProcessStatus Status { get; set; }

        /// <summary>Paramètres Additionels</summary>
        public string Param { get; set; }

        /// <summary>Chemin vers le fichier</summary>
        public string WebPath { get; set; }

        /// <summary> Date de création </summary>
        public DateTime DateCrea { get; set; }

        /// <summary>Date de lancement </summary>
        public DateTime DateRun { get; set; }

        /// <summary>
        /// retourne le type de rapport qui import
        /// </summary>
        /// <returns></returns>
        public int ReportType { get; set; }

        /// <summary>
        /// Liste des annexes ratachées
        /// Tuple: Lien, Tooltip
        /// </summary>
        public List<Tuple<string, string>> Attachments { get; set; }

        /// <summary>
        /// Nom du raport
        /// </summary>
        public string ReportName { get; set; }
    }

}