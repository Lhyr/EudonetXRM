using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Classe gestion des wizards
    /// </summary>
    public class eReportWizard
    {
        private eReport _Report;
        private TypeReport _reportType;
        private Int32 _nTab = 0;
        /// <summary>préférences de l'utilisateur.</summary>
        private ePref _pref;
        private List<KeyValuePair<String, String>> _lstFiles;
        private List<eReportWizardField> _lstFields;
        private List<String> _lstWorkingDescids;
        private eudoDAL _dal;
        private EdnType _FileType;
        private Boolean _bIsEvent;
        private Boolean _bInterPP;
        private Boolean _bInterPM;
        private Boolean _bInterEvent;
        private Boolean _bAdrJoin;
        private Int32 _nInterEventNum;
        private Boolean _bFromBkm;
        private String _strTabName = String.Empty;
        private String _strWizardError;
        private Dictionary<Int32, String> _lstRelatedFilters;

        #region accesseurs

        /// <summary>
        /// Liste des filtres accessible et utilisables dans le rapport
        /// </summary>
        public Dictionary<Int32, String> RelatedFilters
        {
            get { return this._lstRelatedFilters; }
        }

        /// <summary>
        /// Accesseurs de la liste de fichiers relatif au fichier en cours.
        /// </summary>
        public List<KeyValuePair<String, String>> AvailableFiles
        {
            get { return this._lstFiles; }
        }

        /// <summary>
        /// Liste des champs disponible dans l'assistant.
        /// </summary>
        public List<eReportWizardField> Fields
        {
            get { return this._lstFields; }
        }

        #endregion

        /// <summary>
        /// Constructeur de la classe utilisé dans le cas de l'édition ou l'on a déjà un objet eReport existant.
        /// </summary>
        /// <param name="ePref">Préférences de l'utilisateur</param>
        /// <param name="nTab">Tab en cours</param>
        /// <param name="report">Objet eReport</param>
        /// <param name="bFromBkm">Provenance d'un signet (probablement à revoir)</param>
        public eReportWizard(ePref ePref, Int32 nTab, eReport report, Boolean bFromBkm = false)
            : this(ePref, nTab, report.ReportType, bFromBkm)
        {
            this._Report = report;
            List<KeyValuePair<String, String>> linkedFromFiles = AddLinkedFromFiles();

            this._lstFiles.AddRange(linkedFromFiles);
            foreach (KeyValuePair<String, String> kvp in linkedFromFiles)
            {
                String[] descidInfos = kvp.Key.Split('_');
                Int32 sourceTab = 0;
                Int32 linkedFromTab = 0;

                if (descidInfos.Length == 2)
                {
                    //todo : vérifier le dal. il est ouvert et fermé dans le construceur parent
                    // du coup, il ne devrait pas être dispo ici
                    if (Int32.TryParse(descidInfos[0], out sourceTab) && Int32.TryParse(descidInfos[1], out linkedFromTab))
                        _lstFields.AddRange(eReportWizard.GetLinkedFields(ePref, _dal, sourceTab, linkedFromTab));
                }
            }
        }


        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="ePref">Préférences de l'utilisateur</param>
        /// <param name="nTab">Tab en cours</param>
        /// <param name="reportType">Type de Report</param>
        /// <param name="bFromBkm">Provenance d'un signet (probablement à revoir)</param>
        public eReportWizard(ePref ePref, Int32 nTab, TypeReport reportType, Boolean bFromBkm = false)
        {


            this._reportType = reportType;
            this._pref = ePref;
            this._bFromBkm = bFromBkm;
            this._nTab = nTab;
            this._lstFiles = new List<KeyValuePair<String, String>>();
            this._lstFields = new List<eReportWizardField>();
            this._lstWorkingDescids = new List<string>();
            this._lstRelatedFilters = new Dictionary<Int32, String>();

            _dal = eLibTools.GetEudoDAL(ePref);



            try
            {
                _dal.OpenDatabase();


                LoadTabInfo();


                AddFiles();
                foreach (KeyValuePair<String, String> kvp in this._lstFiles)
                {
                    _lstFields.AddRange(AddFields(Int32.Parse(kvp.Key)));
                }



                this.LoadAvailableFilters();


            }
            catch
            {

                throw;
            }
            finally
            {
                _dal.CloseDatabase();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ePref"></param>
        /// <param name="report"></param>
        public eReportWizard(ePref ePref, eReport report)
        {
            this._Report = report;
        }

        /// <summary>
        /// Charge les informations relative à la table en cours (type, nom, liaisons...)
        /// </summary>
        private void LoadTabInfo()
        {
            StringBuilder sbQuery = new StringBuilder();
            sbQuery.Append("SELECT [DESC].[FILE], ISNULL([DESC].[TYPE],0) [TYPE], ISNULL([DESC].[INTERPP],0) INTERPP, ISNULL([DESC].[INTERPM],0) INTERPM, ")
                .Append(" ISNULL([DESC].[INTEREVENT],0) INTEREVENT, ISNULL([DESC].[INTEREVENTNUM],0) INTEREVENTNUM, ISNULL([PREF].[ADRJOIN],0) ADRJOIN, [RES].[")
                .Append(_pref.Lang).Append("] FROM  [DESC] INNER JOIN [RES] ON ")
                .Append(" [DESC].[DESCID] = [RES].[RESID]  INNER JOIN [PREF] ON [DESC].[DESCID] = [PREF].[TAB] AND [PREF].[USERID] = @userid")
                .Append(" WHERE [DESC].[DESCID] = @ntab");

            RqParam rqDesc = new RqParam(sbQuery.ToString());
            rqDesc.AddInputParameter("@ntab", System.Data.SqlDbType.Int, _nTab);
            rqDesc.AddInputParameter("@userid", System.Data.SqlDbType.Int, _pref.User.UserId);

            DataTableReaderTuned dtr = _dal.Execute(rqDesc, out _strWizardError);
            try
            {
                if (!String.IsNullOrEmpty(_strWizardError))
                    throw new Exception(String.Concat("eReportWizard.LoadTabInfo() !", _strWizardError));

                if (dtr.HasRows)
                {
                    dtr.Read();
                    Int32 type = dtr.GetEudoNumeric("TYPE");
                    _FileType = (EdnType)type;
                    this._bInterPP = dtr.GetBoolean("INTERPP");
                    this._bInterPM = dtr.GetBoolean("INTERPM");
                    this._bInterEvent = dtr.GetBoolean("INTEREVENT");
                    this._bAdrJoin = dtr.GetBoolean("ADRJOIN");
                    this._nInterEventNum = dtr.GetEudoNumeric("INTEREVENTNUM");

                    _bIsEvent = _nTab == 100 || (_FileType.Equals(EdnType.FILE_MAIN) && _nTab >= 1000);
                    _strTabName = dtr.GetString("FILE");
                }
            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();
            }
        }

        /// <summary>
        /// Peuple la liste interne avec les descid/libellés des fichiers liés/dépendants de la table
        /// </summary>
        private void AddFiles()
        {
            Boolean bAddTemplate = _reportType.Equals(TypeReport.PRINT) || _reportType.Equals(TypeReport.EXPORT) || _reportType.Equals(TypeReport.MERGE);

            List<int> fileDescIds = new List<int>();



            AddListValue(_nTab, fileDescIds);
            switch (_nTab)
            {
                case (int)TableType.PP:
                    AddListValue(TableType.PM.GetHashCode(), fileDescIds);
                    AddListValue(TableType.ADR.GetHashCode(), fileDescIds);
                    break;
                case (int)TableType.PM:
                    AddListValue(TableType.PP.GetHashCode(), fileDescIds);
                    AddListValue(TableType.ADR.GetHashCode(), fileDescIds);
                    break;
                case (int)TableType.ADR:
                    AddListValue(TableType.PP.GetHashCode(), fileDescIds);
                    AddListValue(TableType.PM.GetHashCode(), fileDescIds);
                    break;
                default:
                    if (_bIsEvent)
                    {
                        if (_bInterPP)
                            AddListValue(TableType.PP.GetHashCode(), fileDescIds);
                        if (_bInterPM)
                            AddListValue(TableType.PM.GetHashCode(), fileDescIds);
                        if (_bInterPP && _bInterPM)
                            AddListValue(TableType.ADR.GetHashCode(), fileDescIds);
                    }
                    break;
            }
            if (!_FileType.Equals(EdnType.FILE_MAIN))
            {
                if (_bInterPP)
                {
                    AddListValue(TableType.PP.GetHashCode(), fileDescIds);
                    if (this._bAdrJoin || _bFromBkm)
                        AddListValue(TableType.ADR.GetHashCode(), fileDescIds);
                }
                if (this._bInterPM || (this._bFromBkm || this._bAdrJoin))
                    AddListValue(TableType.PM.GetHashCode(), fileDescIds);

                if (this._bInterEvent)
                    if (this._nInterEventNum == 0)
                        AddListValue(100, fileDescIds);
                    else
                        AddListValue(((this._nInterEventNum + 10) * 100), fileDescIds);
            }

            AddSpecialLink(fileDescIds);

            AddDependentFiles(fileDescIds);

            /*
             * une fois tous les descid de tables liées chargés on actualise la liste
             * En retirant les tables pour lesquelles l'utilisateur en cours
             * n'a pas les droits de visu.
             * */
            fileDescIds = GetViewableDescids(_pref, _dal, fileDescIds, true);

            String strRes = String.Join(",", fileDescIds.ToArray());
            Boolean bFound = false;
            String strFileRes = String.Empty;

            eRes res = new eRes(_pref, strRes);

            foreach (int DescId in fileDescIds)
            {

                strFileRes = res.GetRes(DescId, out bFound);
                if (bFound)
                {
                    this._lstFiles.Add(new KeyValuePair<String, String>(DescId.ToString(), strFileRes));
                    bFound = false;
                }
            }
            /*this._lstFiles.Sort(
                delegate(KeyValuePair<String, String> firstPair,
                KeyValuePair<String, String> nextPair)
                {
                    return firstPair.Value.CompareTo(nextPair.Value);
                }
            );*/

        }

        /// <summary>
        /// Ajout dans la liste passée en paramètre les descid des champs liés par des champs de liaison
        /// </summary>
        /// <param name="fileDescIds">Liste de DescId</param>
        private void AddSpecialLink(List<int> fileDescIds)
        {
            //#74805
            if (_nTab == (int)TableType.ADR)
                return;

            StringBuilder sbQuery = new StringBuilder();
            sbQuery.Append("SELECT  [A].[DESCID], [A].[POPUPDESCID], [A].[FIELD], (B.[DESCID] - B.[DESCID] % 100) LINKEDFILE, B.[FILE], B.[FIELD]+ 'ID' ")
                .Append(" FROM [DESC] [A] INNER JOIN [DESC] B ON ((CAST( A.[POPUPDESCID] / 100 AS INT ) ) * 100) = B.[DESCID] ")
                .Append(" AND ISNULL([A].[RELATION],0) <> 0 AND ISNULL([A].[POPUP],0) = 2 AND [A].[DESCID] > @ntab AND [A].[DESCID] < @ntab +100 ")
                .Append(" AND A.POPUPDESCID IN (SELECT [DESCID]+1 FROM [DESC] WHERE CAST([DESCID] AS INT) % 100 = 0 AND ISNULL([TYPE],0) = 0)");

            RqParam rqSpecialLink = new RqParam(sbQuery.ToString());
            rqSpecialLink.AddInputParameter("@ntab", SqlDbType.Int, _nTab);

            DataTableReaderTuned dtr = _dal.Execute(rqSpecialLink, out _strWizardError);
            try
            {
                if (!String.IsNullOrEmpty(_strWizardError))
                    throw new Exception(String.Concat("eReportWizard.GetSpecialLink() : ", _strWizardError));
                if (dtr.HasRows)
                {
                    Int32 nPopupDescId = 0;
                    while (dtr.Read())
                    {
                        nPopupDescId = dtr.GetEudoNumeric("POPUPDESCID");
                        nPopupDescId = nPopupDescId - (nPopupDescId % 100);
                        if (!fileDescIds.Contains(nPopupDescId))
                            fileDescIds.Add(nPopupDescId);
                    }
                }
            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();
            }
        }

        /// <summary>
        /// Ajoute les fichier dépendant du fichier duquel on exporte.
        /// </summary>
        /// <param name="fileDescIds">Liste de DescId</param>
        private void AddDependentFiles(List<int> fileDescIds)
        {
            StringBuilder sbQuery = new StringBuilder();
            StringBuilder sbWhere = new StringBuilder();

            sbQuery.Append("SELECT [DESC].[DESCID] ,[DESC].[FILE],[RES].[").Append(_pref.Lang).Append("] AS LIBELLE ")
                .Append(" FROM [DESC] LEFT JOIN [RES] ON [DESC].[DESCID] = [RES].[RESID] WHERE CAST( [DESC].[DESCID]  AS INT ) % 100 = 0 ");

            switch (_nTab)
            {
                case (int)TableType.PP:
                    sbWhere.Append(" ISNULL([DESC].[INTERPP],0) <> 0 ");
                    break;
                case (int)TableType.PM:
                    sbWhere.Append("( ISNULL( [INTERPM], 0 ) <> 0 OR [DESC].[DESCID] IN (SELECT [PREF].[TAB] FROM [PREF] WHERE ISNULL([PREF].[ADRJOIN],0) <> 0) )");
                    break;
                default:
                    if (_bIsEvent)
                    {
                        sbWhere.Append(" ( ISNULL( [INTEREVENT], 0 ) <> 0 AND ISNULL( [INTEREVENTNUM], 0 ) = @intereventnum ) OR ")
                         .Append(" ( [DESC].[DESCID] = ( SELECT 200 WHERE EXISTS( SELECT INTERPP FROM [DESC] WHERE DESCID = @ntab AND ISNULL(INTERPP,0) <> 0 ) ) OR ")
                         .Append(" [DESC].[DESCID] = ( SELECT 300 WHERE EXISTS( SELECT INTERPM FROM [DESC] WHERE DESCID = @ntab AND ISNULL(INTERPM,0) <> 0 ) ) OR ")
                         .Append(" [DESC].[DESCID] = ( SELECT 100 WHERE EXISTS( SELECT INTEREVENTNUM FROM [DESC] WHERE DESCID = @ntab AND ISNULL(INTEREVENT,0) <> 0 AND ISNULL(INTEREVENTNUM,0) = 0 ) ) OR ")
                         .Append(" [DESC].[DESCID] = ( SELECT 400 WHERE EXISTS( SELECT ADRJOIN FROM [PREF] WHERE TAB = @ntab AND USERID = 0 AND ISNULL(ADRJOIN,0) <> 0 ) ) OR ")
                         .Append(" [DESC].[DESCID] = ( SELECT (CASE WHEN (SELECT ISNULL(INTEREVENTNUM,0) FROM [DESC] WHERE DESCID = @ntab) = 0 THEN 100 ELSE ((SELECT ISNULL(INTEREVENTNUM,0) FROM [DESC] WHERE DESCID = @ntab)+10)*100 END) WHERE EXISTS( SELECT INTEREVENT FROM [DESC] WHERE DESCID = @ntab AND ISNULL(INTEREVENT,0) <> 0 ) ) )");

                    }
                    else
                    {
                        sbWhere.Append(" ( [DESC].[DESCID] = ( SELECT 200 WHERE EXISTS( SELECT INTERPP FROM [DESC] WHERE DESCID = @ntab AND ISNULL(INTERPP,0) <> 0 ) ) OR ")
                             .Append(" [DESC].[DESCID] = ( SELECT 300 WHERE EXISTS( SELECT INTERPM FROM [DESC] WHERE DESCID = @ntab AND ISNULL(INTERPM,0) <> 0 ) ")
                             .Append(" OR EXISTS( SELECT ADRJOIN FROM [PREF] WHERE TAB = @ntab AND USERID = 0 AND ISNULL(ADRJOIN,0) <> 0 ) ) OR ")
                             .Append(" [DESC].[DESCID] = ( SELECT 400 WHERE EXISTS( SELECT ADRJOIN FROM [PREF] WHERE TAB = @ntab AND USERID = 0 AND ISNULL(ADRJOIN,0) <> 0 ) ) OR ")
                             .Append(" [DESC].[DESCID] = ( SELECT (CASE WHEN (SELECT ISNULL(INTEREVENTNUM,0) FROM [DESC] ")
                             .Append(" WHERE DESCID = @ntab) = 0 THEN 100 ELSE ((SELECT ISNULL(INTEREVENTNUM,0) FROM [DESC] WHERE DESCID = @ntab)+10)*100 END) ")
                             .Append(" WHERE EXISTS( SELECT INTEREVENT FROM [DESC] WHERE DESCID = @ntab AND ISNULL(INTEREVENT,0) <> 0 ) ) ) ");
                    }
                    break;
            }

            sbQuery.Append(" AND ( ").Append(sbWhere.ToString())
                .Append(" OR [DESC].[DESCID] IN (SELECT CAST(([DESCID] / 100) AS INT) * 100 FROM [DESC] WHERE [POPUP] = 2 AND ISNULL([RELATION],0) <> 0 AND [POPUPDESCID] = (@ntab +1)) ");

            if (
                _nTab != (int)TableType.PP
                && _nTab != (int)TableType.PM
                && _nTab != (int)TableType.ADR
                )
                sbQuery.Append(" OR [DESC].[DESCID] IN (SELECT POPUPDESCID - 1 FROM [DESC] WHERE [POPUP] = 2 AND ISNULL([RELATION],0) <> 0 AND CAST(([DESCID] / 100) AS INT) * 100 = @ntab)");

            sbQuery.Append("  ) ");

            sbQuery.Append(" ORDER BY LIBELLE");

            RqParam rqDependantFile = new RqParam(sbQuery.ToString());
            rqDependantFile.AddInputParameter("@ntab", SqlDbType.Int, _nTab);
            rqDependantFile.AddInputParameter("@intereventnum", SqlDbType.Int, this._nTab == 100 ? 0 : ((_nTab / 100) - 10));

            DataTableReaderTuned dtr = _dal.Execute(rqDependantFile, out _strWizardError);
            try
            {
                if (!String.IsNullOrEmpty(_strWizardError))
                    throw new Exception(String.Concat("eReportWizard.AddDependentFiles() : ", _strWizardError));
                if (dtr.HasRows)
                {
                    Int32 nDescId = 0;
                    while (dtr.Read())
                    {
                        nDescId = dtr.GetEudoNumeric("DESCID");
                        nDescId = nDescId - (nDescId % 100);
                        if (!fileDescIds.Contains(nDescId))
                            fileDescIds.Add(nDescId);
                    }
                }
            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();
            }
        }

        /// <summary>
        /// Ajoute les fichier liés depuis un fichier du rapport
        /// </summary>
        /// <returns>Liste des fichiers liés</returns>
        public List<KeyValuePair<String, String>> AddLinkedFromFiles()
        {
            List<KeyValuePair<String, String>> fileList = new List<KeyValuePair<string, string>>();
            //Exceptions à ne pas inclure

            if (this._Report == null)
                return null;
            List<Int32[]> linkedTabs = new List<Int32[]>();
            Int32 tabDescId = 0;
            Int32 fromTabDescId = 0;
            Int32 selectedField = 0;
            eRes res;
            List<int> resList = new List<int>();
            String[] fields = _Report.GetParamValue("field").Split(';');
            Boolean bFound = false;

            foreach (String fieldElement in fields)
            {
                if (fieldElement.IndexOf(',') < 0)
                    continue;
                else
                {
                    if (!Int32.TryParse(fieldElement.Split(',')[0], out selectedField) || !Int32.TryParse(fieldElement.Split(',')[1], out fromTabDescId))
                    {
                        //TODO : Erreur de champ dans le param à remonter ?
                        continue;
                    }
                    else
                    {
                        tabDescId = selectedField - selectedField % 100;
                        if (!resList.Contains(tabDescId))
                            resList.Add(tabDescId);
                        if (!resList.Contains(fromTabDescId))
                            resList.Add(fromTabDescId);

                        Int32[] tabInfos = { tabDescId, fromTabDescId };

                        if (!LinkedFromTabExists(tabInfos, linkedTabs))
                            linkedTabs.Add(tabInfos);

                    }
                }
            }
            //aucun descid dans la liste à retourner donc on ne prends pas la peine d'essayer de la construire, on sors de la méthode.
            if (resList.Count == 0)
                return fileList;
            res = new eRes(this._pref, String.Join(",", resList.ToArray()));
            List<int> viewableDescIds = GetViewableDescids(_pref, _dal, resList, true);


            //parcours des fichiers liés pour comparer avec les descid autorisés en visu afin de les ajouter dans la liste retournée.
            foreach (Int32[] tabInfos in linkedTabs)
            {
                //Si le fichier depuis lequel ce fichier est lié n'est plus visible, on masque le fichier lié depuis également ?
                //TODO check si c'est un bon fonctionnement
                if (!viewableDescIds.Contains(tabInfos[0]) || !viewableDescIds.Contains(tabInfos[1]))
                    continue;
                String tabRes = res.GetRes(tabInfos[0], out bFound);
                if (!bFound)
                    throw new Exception("eReportWizard.AddlinkedFromFiles() : Libellé du fichier \"lié\" non trouvé.");
                String fromTabRes = res.GetRes(tabInfos[1], out bFound);
                if (!bFound)
                    throw new Exception("eReportWizard.AddlinkedFromFiles() : Libellé du fichier \"lié depuis\" non trouvé.");

                fileList.Add(new KeyValuePair<String, String>(String.Concat(tabInfos[0], "_", tabInfos[1]), eReportWizard.GetLinkedFileLabel(tabRes, fromTabRes)));
            }
            return fileList;
        }

        /// <summary>
        /// Vérifie si la paire DescId du fichier/DescId du fichier de liaison est déjà présente dans la liste passée en paramètre
        /// </summary>
        /// <param name="tabInfos">paire DescId du fichier/DescId du fichier de liaison</param>
        /// <param name="linkedFromTabs">Liste des fichiers liés</param>
        /// <returns>True si la valeur existe, false sinon</returns>
        private Boolean LinkedFromTabExists(Int32[] tabInfos, List<Int32[]> linkedFromTabs)
        {
            if (linkedFromTabs.Count == 0)
                return false;
            foreach (Int32[] linkedFromTab in linkedFromTabs)
            {
                if (linkedFromTab[0] == tabInfos[0] && linkedFromTab[1] == tabInfos[1])
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Retourne la liste des fichiers visible et exploitable pour le fichier passé en paramètres
        /// </summary>
        /// <param name="workingTab">Fichier cible</param>
        /// <returns>List de eReportWizardFields</returns>
        private List<eReportWizardField> AddFields(Int32 workingTab)
        {
            return eReportWizard.AddFields(this._pref, this._dal, workingTab, null, out this._strWizardError);


            #region old method
            /*List<eReportWizardField> fieldList =  new List<eReportWizardField>();

            StringBuilder sbQuery = new StringBuilder();
            sbQuery.Append("SELECT [DESC].[DESCID], [DESC].[FORMAT], [DESC].[TOOLTIPTEXT], ISNULL([DESC].[POPUP],0) [POPUP], ISNULL([DESC].[POPUPDESCID],0) [POPUPDESCID],")        
            .Append(" CASE WHEN ([DESC].[POPUP] = 2 AND [DESC].[POPUPDESCID] <> [DESC].[DESCID]) THEN ")
            .Append(" (SELECT R2.[LANG_00] FROM RES R2 WHERE R2.RESID = ([DESC].[POPUPDESCID] - [DESC].[POPUPDESCID] % 100)) ELSE '' END [FILELABEL], ")
            .Append(" [RES].[").Append(_pref.Lang).Append("] AS LABEL")
            .Append(" FROM [DESC] INNER JOIN [RES] ON [DESC].[DESCID] = [RES].[RESID] WHERE ")
            .Append(" [DESCID] > @ntab AND [DESCID] < @ntab + 100 AND ")
            .Append(" ( [DESC].[FORMAT] NOT IN (0, 15)  OR [DESC].[DESCID] = @ntab + 91) ")
            .Append(" AND [DESC].[DESCID] % 100 NOT IN (76,77,78,79,80) ");
            RqParam rqFields = new RqParam(sbQuery.ToString());
            rqFields.AddInputParameter("@ntab", SqlDbType.Int, workingTab);
            DataTableReader dtr = _dal.ExecuteQuery(rqFields, out this._strWizardError);
            if(!String.IsNullOrEmpty(_strWizardError))
                throw new Exception(String.Concat("eReportWizard.AddFields()  : ",_strWizardError));

            if(dtr.HasRows)
            {
                eReportWizardField field;
                while(dtr.Read())
                {
                    field = new eReportWizardField(dtr["DESCID"].ToString(), 
                        (FieldFormat)Int32.Parse(dtr["FORMAT"].ToString()), 
                        dtr["TOOLTIPTEXT"].ToString(), 
                        dtr["LABEL"].ToString(),
                        (PopupType)Enum.Parse(typeof(PopupType), dtr["POPUP"].ToString()),
                        Int32.Parse(dtr["POPUPDESCID"].ToString()), dtr["FILELABEL"].ToString());
                    fieldList.Add(field);
                    this._lstWorkingDescids.Add(dtr["DESCID"].ToString());
                }
                fieldList = ApplyFieldViewRights(this._pref, this._dal, fieldList, this._lstWorkingDescids);
            }

            return fieldList;*/
            #endregion
        }

        /// <summary>
        /// Retourne la liste des fichiers visible et exploitable pour le fichier passé en paramètres
        /// </summary>
        /// <param name="pref">préférences de l'utilisateur en cours</param>
        /// <param name="dal">EudoDal ouvert la connexion à la base de données</param>
        /// <param name="workingTab">Fichier cible</param>
        /// <param name="linkedFromTab">Id de la table liéee</param>
        /// <param name="error">Message d'Erreur</param>
        /// <returns>List de eReportWizardFields</returns>
        public static List<eReportWizardField> AddFields(ePref pref, eudoDAL dal, Int32 workingTab,Int32? linkedFromTab, out String error)
        {
            List<eReportWizardField> fieldList = new List<eReportWizardField>();
            List<String> workingDescIds = new List<String>();

            //BSE #50 495 : une excpetion pour le champ system Type sur les Exports
            TableLite wTab = new TableLite(workingTab);
            wTab.ExternalLoadInfo(dal, out error);

            StringBuilder sbQuery = new StringBuilder();
            sbQuery.Append("SELECT [DESC].[DESCID], [DESC].[FORMAT], [DESC].[TOOLTIPTEXT], ISNULL([DESC].[POPUP],0) [POPUP], ISNULL([DESC].[POPUPDESCID],0) [POPUPDESCID],")
            .Append(" CASE WHEN ([DESC].[POPUP] = 2 AND [DESC].[POPUPDESCID] <> [DESC].[DESCID]) THEN ")
            .Append(" (SELECT R2.[LANG_00] FROM RES R2 WHERE R2.RESID = ([DESC].[POPUPDESCID] - [DESC].[POPUPDESCID] % 100)) ELSE '' END [FILELABEL], ")
            .Append(" [RES].[").Append(pref.Lang).Append("] AS LABEL, ISNULL(P,1) AS P, ")
            .Append(" ISNULL([DESC].[PROSPECTENABLED], 0) PROSPECTENABLED, ")
            .Append(" ISNULL([DESC].[MULTIPLE], 0) AS MULTIPLE ")
            .Append(" FROM [DESC] INNER JOIN [RES] ON [DESC].[DESCID] = [RES].[RESID] ")
            .Append(" left join cfc_getPermInfo(").Append(pref.User.UserId).Append(",").Append(pref.User.UserLevel).Append(",").Append(pref.User.UserGroupId).Append(") A on A.permissionid=[DESC].viewpermid ")
            .Append(" WHERE ")

            .Append(" [DESCID] > @ntab AND [DESCID] < @ntab + 100 AND ")
            .Append(" ( [DESC].[FORMAT] NOT IN (0, 15, 32)  OR [DESC].[DESCID] = @ntab + 91) ")
            .Append(" AND [DESC].[DESCID] % 100 NOT IN (76,77,78,79,80 ").Append(!(wTab.EdnType == EdnType.FILE_PLANNING) ? ",83)" : ")")
            .Append(" ORDER BY LABEL ");
            RqParam rqFields = new RqParam(sbQuery.ToString());
            rqFields.AddInputParameter("@ntab", SqlDbType.Int, workingTab);
            DataTableReaderTuned dtr = dal.Execute(rqFields, out error);
            try
            {
                if (!String.IsNullOrEmpty(error))
                    throw new Exception(String.Concat("eReportWizard.AddFields()  : ", error));

                if (dtr.HasRows)
                {
                    eReportWizardField field;
                    while (dtr.Read())
                    {
                        if (dtr.GetString("P") == "1")
                        {

                            field = new eReportWizardField(dtr.GetString("DESCID"),
                                (FieldFormat)(dtr.GetInt16("FORMAT")),
                                dtr.GetString("TOOLTIPTEXT"),
                                dtr.GetString("LABEL"),
                                (PopupType)Enum.Parse(typeof(PopupType), dtr.GetString("POPUP")),
                                eLibTools.GetNum(dtr.GetString("POPUPDESCID")),
                                dtr.GetString("FILELABEL"),
                                dtr.GetString("MULTIPLE").Equals("1"));


                            //KJE #65 565: s'il s'agit d'un élément appartenant à une table liéée, on renseigne de la table de liaison
                            if (linkedFromTab.HasValue)
                                field.LinkedFromTab = linkedFromTab.Value;
                            field.TargetDescdId = dtr.GetEudoNumeric("ProspectEnabled");


                            fieldList.Add(field);
                            //  workingDescIds.Add(dtr["DESCID"].ToString());
                        }
                    }

                    //fieldList = ApplyFieldViewRights(pref, dal, fieldList, workingDescIds);
                }
            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();
            }

            return fieldList;
        }

        /// <summary>
        /// Charge la liste des filtres associé à l'onglet
        /// </summary>
        private void LoadAvailableFilters()
        {
            eList List = eListFactory.CreateFilterList(_pref, this._nTab, 1, EudoQuery.TypeFilter.USER);
            if (List.ErrorMsg.Length > 0)
            {


                if (List.InnerException != null)
                    throw List.InnerException;
                else

                    throw new Exception(String.Concat("eReportWizard.LoadAvailableFilters List.cs", " ", List.ErrorMsg));
            }

            foreach (eRecord rec in List.ListRecords)
            {
                if (!String.IsNullOrEmpty(rec.GetFields[0].Value))
                    this._lstRelatedFilters.Add(rec.MainFileid, rec.GetFields[0].DisplayValue);
            }
        }

        /// <summary>
        /// Détermine si le Fichier transmis est liable par la table active.
        /// En prenant en compte le fait que PP, PM  et ADRESSE sont lié par défaut entre eux.
        /// </summary>
        /// <param name="tab">Fichier lié (cible)</param>
        /// <param name="currentTab">Fichier en cours(source)</param>
        /// <returns>True si c'est liable, false sinon</returns>
        private static Boolean IsFileLinkable(Int32 tab, Int32 currentTab)
        {
            switch (currentTab)
            {
                case (int)TableType.PP:
                    if (tab == (int)TableType.PM || tab == (int)TableType.ADR)
                        return false;
                    break;
                case (int)TableType.PM:
                    if (tab == (int)TableType.PP || tab == (int)TableType.ADR)
                        return false;
                    break;
            }
            return true;
        }



        /// <summary>
        /// Retourne une liste actualisée des descid visibles parmis la liste transmise en paramètre
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="dal">EudoDAL pour la connexion à la base de données</param>
        /// <param name="DescIds">Liste des descid à tester</param>
        /// <param name="bFiles">Test sur table ou champs</param>
        /// <returns>LIste actualisée des descids</returns>
        private List<int> GetViewableDescids(ePref pref, eudoDAL dal, List<int> DescIds, Boolean bFiles)
        {
            //Droits de visu
            String _err = String.Empty;
            List<int> authorizedDescIds = new List<int>();

            using (DataTableReaderTuned dtrRights = eLibDataTools.GetRqViewRight(pref.User.UserId, pref.User.UserLevel, pref.User.UserGroupId, pref.User.UserLang, DescIds, dal))
            {
                if (dtrRights == null || !String.IsNullOrEmpty(_err))
                    throw new Exception(String.Concat("eReportWizard.CheckRights() : ", _err));

                while (dtrRights.Read())
                {
                    Boolean bVisible = false;
                    // TODO - utiliser la fonction eTools.GetTabViewRight ?
                    bVisible = dtrRights.GetInt32("VIEW_P") == 1 && (!bFiles || !dtrRights.GetBoolean("Disabled"));

                    if (bVisible)
                        AddListValue(dtrRights.GetEudoNumeric("DESCID"), authorizedDescIds);
                }
            }

            return authorizedDescIds;
        }

        /// <summary>
        /// Retourne le nombre d'étape nécessaire à l'assistant reporting en fonction du type de rapport demandé.
        /// </summary>
        /// <param name="reportType">Type de rapport</param>
        /// <returns>Nombre d'étape</returns>
        public static Int32 GetTotalSteps(TypeReport reportType)
        {
            switch (reportType)
            {
                case TypeReport.PRINT:
                case TypeReport.EXPORT:
                case TypeReport.MERGE:
                    return 5;
                case TypeReport.PRINT_FILE:
                    return 3;
                case TypeReport.CHARTS:
                    return 4;
                default:
                    return 5;

            }
        }

        /// <summary>
        /// Retourne la valeur du format d'index sélectionné dans l'interface, en fonction du type de rapport
        /// </summary>
        /// <param name="reportType">Type de rapport (impression, publipostage, export...)</param>
        /// <param name="formatIndex">Index de format sélectionné dans l'interface</param>
        /// <returns>Entier correspondant au format sélectionné dans le type de rapport</returns>
        public static Int32 MapFormatValue(TypeReport reportType, Int32 formatIndex)
        {
            switch (reportType)
            {
                //impression
                case TypeReport.PRINT:
                    return 0; //Impression uniquement au format HTML, le paramètre de format n'est pas impactant, on retourne donc 0

                //Export
                case TypeReport.EXPORT:
                    return formatIndex; //la liste indexé des radio de l'interface à été conçu avec comme schéma par défaut le schéma de format d'export. les valeurs correspondent.

                //Publipostage
                case TypeReport.MERGE:
                    switch (formatIndex)
                    {
                        case 1:
                            return 0; //si Text Return valeur par défaut : WORD (ne devrait pas arriver)
                        case 2:
                            return 0; //si Text Return valeur par défaut : WORD (ne devrait pas arriver)
                        case 3:
                            return 0; //si Text Return valeur par défaut : WORD (ne devrait pas arriver)
                        case 4:
                            return 1; //HTML
                        case 5:
                            return 5; //Open Office fonctionne comme word en dehors des paramètres de modèles qui sont différents et gérés dans les champs prévu à cet effet
                        case 6:
                            return 2; //PDF
                        default:
                            return 0;
                    }

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Retourne la liste des onglet disponible pour la liaison.
        /// </summary>
        /// <param name="dal">EudoDal pour la connexion à la base de données</param>
        /// <param name="activeTab">Onglet sélectionné</param>
        /// <param name="pref">Préférences de l'utilisateur en cours</param>
        /// <returns>Liste des onglets sous la forme  libellé / DescId </returns>
        public static List<KeyValuePair<string, int>> GetLinkedFileList(eudoDAL dal, int activeTab, ePref pref)
        {
            int nTab = activeTab;
            string error = String.Empty;
            List<KeyValuePair<string, int>> LinkedTabList = new List<KeyValuePair<string, int>>();
            StringBuilder sbSqlQuery = new StringBuilder();
            string errorMessage = String.Empty;
            ISet<int> linkedTabs = new HashSet<int>();
            DataTableReaderTuned dtrFiles = null;
            RqParam rqRights = new RqParam();


            TableLite currentTab = new TableLite(nTab);
            currentTab.ExternalLoadInfo(dal, out error);
            if (error.Length > 0)
                throw new Exception(error);

            string sWhereNotBkmWeb = "AND NOT Exists (SELECT Descid FROM [DESC] WHERE Descid = RelationFIleDescid AND Type = @BkmType)";

            switch (currentTab.TabType)
            {
                case TableType.PP:
                case TableType.PM:
                    sbSqlQuery.Append("select res.").Append(pref.Lang).Append(" as Libelle, RelationFileDescId from cfc_getLinked(@nTab) left join res on RelationFileDescId = res.resid where isnull(isrelation,0) = 1 ").Append(sWhereNotBkmWeb).Append(" order by  Libelle");
                    break;
                case TableType.EVENT:
                case TableType.ADR:
                    // Table de départ EVENT
                    sbSqlQuery.Append("SELECT Libelle, RelationFileDescId From(select res.").Append(pref.Lang).Append(" as Libelle, RelationFileDescId from cfc_getLinked(@nTab) left join res on RelationFileDescId = res.resid where isnull(isrelation,0) = 1 ").Append(sWhereNotBkmWeb).Append(" ")
                                        .AppendLine(" UNION ")
                                        .Append("select res.").Append(pref.Lang).Append(" as Libelle, RelationFileDescId from cfc_getLiaison(@nTab) left join res on RelationFileDescId = res.resid where isnull(isrelation,0) = 1 ").Append(sWhereNotBkmWeb).Append(") AS LNK  order by  Libelle");
                    break;
                default:
                    // Table de départ : Template
                    // La liaison doit être sur la table de départ
                    sbSqlQuery.Append("select res.").Append(pref.Lang).Append(" as Libelle, RelationFileDescId from cfc_getLiaison(@nTab) left join res on RelationFileDescId = res.resid where isnull(isrelation,0) = 1  ").Append(sWhereNotBkmWeb).Append("  order by  Libelle");
                    break;
            }

            try
            {
                RqParam rqLinkedFiles = new RqParam(sbSqlQuery.ToString());
                rqLinkedFiles.AddInputParameter("@nTab", SqlDbType.Int, nTab);
                rqLinkedFiles.AddInputParameter("@BkmType", SqlDbType.Int, EdnType.FILE_BKMWEB.GetHashCode());

                dal.OpenDatabase();
                dtrFiles = dal.Execute(rqLinkedFiles, out errorMessage);
                try
                {
                    if (!String.IsNullOrEmpty(errorMessage))
                    {
                        throw new Exception(errorMessage);
                    }

                    //chargement de la liste des fichier dans une chaine avec séparateur ";" pour transmission à la xsp de vérification des droits
                    //On ajoute pas
                    if (dtrFiles != null)
                        while (dtrFiles.Read())
                            if (IsFileLinkable(dtrFiles.GetEudoNumeric("RELATIONFILEDESCID"), activeTab))
                                linkedTabs.Add(dtrFiles.GetEudoNumeric("RELATIONFILEDESCID"));
                }
                finally
                {
                    dtrFiles?.Dispose();
                }

                linkedTabs.Add(nTab);

                // Ajout adr, pp et pm si nécessaire
                if (nTab == (int)TableType.PP || nTab == (int)TableType.PM || nTab == (int)TableType.ADR)
                {
                    linkedTabs.Add((int)TableType.PP);
                    linkedTabs.Add((int)TableType.PM);
                    linkedTabs.Add((int)TableType.ADR);
                }

                //Droits de visu                



                using (DataTableReaderTuned dtrRights = eLibDataTools.GetRqViewRight(pref.User.UserId, pref.User.UserLevel, pref.User.UserGroupId, pref.User.UserLang, linkedTabs, dal))
                {
                    while (dtrRights.Read())
                        if (eLibDataTools.GetTabViewRight(dtrRights))
                            LinkedTabList.Add(new KeyValuePair<string, int>(dtrRights.GetString("LIBELLE"), dtrRights.GetEudoNumeric("DESCID")));
                }


            }
            catch (Exception ex)
            {
                throw new Exception(String.Concat("GetLinkedFiles : ", ex.Message.Length == 0 ? "Erreur imprévue." : ex.Message));
            }
            finally
            {
                dal.CloseDatabase();
            }

            return LinkedTabList;
        }

        /// <summary>
        /// Retourne la liste des champs 
        /// </summary>
        /// <param name="pref">Préférences de l'utilisateur en cours</param>
        /// <param name="selectedTab">Fichier lié</param>
        /// <param name="linkedFromTab">Fichier depuis lequel le selectedTab est lié (options lié depuis)</param>
        /// <returns>List de ReportWizardField</returns>
        public static List<eReportWizardField> GetLinkedFields(ePref pref, Int32 selectedTab, Int32 linkedFromTab)
        {

            eudoDAL dal = eLibTools.GetEudoDAL(pref);
            try
            {
                dal.OpenDatabase();
                return eReportWizard.GetLinkedFields(pref, dal, selectedTab, linkedFromTab);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Concat("eReportWizard.GetLinkedFields() : ", ex.Message));
            }
            finally
            {
                dal.CloseDatabase();
            }

        }

        /// <summary>
        /// Retourne la liste des champs affichable pour le tab donné
        /// </summary>
        /// <param name="pref">Préférences de l'utilisateur</param>
        /// <param name="dal">EudoDAL pour la connexion</param>
        /// <param name="selectedTab">Fichier cible</param>
        /// <param name="linkedFromTab">Fichier depuis lequel ce fichier est lié</param>
        /// <returns>Liste des champs</returns>
        public static List<eReportWizardField> GetLinkedFields(ePref pref, eudoDAL dal, Int32 selectedTab, Int32 linkedFromTab)
        {
            List<eReportWizardField> fieldList = new List<eReportWizardField>();
            List<int> fileDescIds = new List<int>();
            String errorMessage = String.Empty;

            StringBuilder sbQuery = new StringBuilder();
            sbQuery.Append("SELECT [DESC].[DESCID], [DESC].[FORMAT], [DESC].[TOOLTIPTEXT], ISNULL([DESC].[POPUP],0) [POPUP], ISNULL([DESC].[POPUPDESCID],0) [POPUPDESCID],")
            .Append(" CASE WHEN ([DESC].[POPUP] = 2 AND [DESC].[POPUPDESCID] <> [DESC].[DESCID]) THEN ")
            .Append(" (SELECT R2.[LANG_00] FROM RES R2 WHERE R2.RESID = ([DESC].[POPUPDESCID] - [DESC].[POPUPDESCID] % 100)) ELSE '' END [FILELABEL], ")
            .Append(" [RES].[").Append(pref.Lang).Append("] AS LABEL, ")
            .Append(" ISNULL([DESC].[PROSPECTENABLED], 0) PROSPECTENABLED, ")
            .Append(" ISNULL([DESC].[Multiple], 0) AS MULTIPLE ")
            .Append(" FROM [DESC] INNER JOIN [RES] ON [DESC].[DESCID] = [RES].[RESID] WHERE ")
            .Append(" [DESCID] > @ntab AND [DESCID] < @ntab + 100 AND ")
            .Append(" ( [DESC].[FORMAT] NOT IN (0, 15)  OR [DESC].[DESCID] = @ntab + 91) ")
            .Append(" AND [DESC].[DESCID] % 100 NOT IN (76,77,78,79,80) ORDER BY LABEL");
            RqParam rqFields = new RqParam(sbQuery.ToString());
            rqFields.AddInputParameter("@ntab", SqlDbType.Int, selectedTab);
            DataTableReaderTuned dtr = dal.Execute(rqFields, out errorMessage);
            try
            {
                if (!String.IsNullOrEmpty(errorMessage))
                    throw new Exception(String.Concat("eReportWizard.AddFields()  : ", errorMessage));

                if (dtr.HasRows)
                {
                    eReportWizardField field;
                    while (dtr.Read())
                    {
                        field = new eReportWizardField(dtr.GetString("DESCID"),
                            (FieldFormat)dtr.GetEudoNumeric("FORMAT"),
                            dtr.GetString("TOOLTIPTEXT"),
                            dtr.GetString("LABEL"),
                            (PopupType)Enum.Parse(typeof(PopupType),
                            dtr.GetString("POPUP")),
                            dtr.GetEudoNumeric("POPUPDESCID"),
                            dtr.GetString("FILELABEL"),
                            dtr.GetBoolean("MULTIPLE")  // 42068 CRU : récupération de l'info "Multiple"
                            );
                        field.LinkedFromTab = linkedFromTab;
                        field.TargetDescdId = dtr.GetEudoNumeric("prospectenabled");

                        fieldList.Add(field);
                        int nF;
                        if (int.TryParse(dtr.GetString("DESCID"), out nF))
                            fileDescIds.Add(nF);
                    }

                    #region droits

                    //Droits de visu
                    String _err = String.Empty;
                    List<int> authorizedDescIds = new List<int>();

                    using (DataTableReaderTuned dtrRights = eLibDataTools.GetRqViewRight(pref.User.UserId, pref.User.UserLevel, pref.User.UserGroupId, pref.User.UserLang, fileDescIds, dal))
                    {

                        if (dtrRights == null || !String.IsNullOrEmpty(_err))
                            throw new Exception(String.Concat("eReportWizard.CheckRights() : ", _err));

                        while (dtrRights.Read())
                        {
                            if (dtrRights.GetInt32("VIEW_P") == 1)
                                AddListValue(dtrRights.GetEudoNumeric("DESCID"), authorizedDescIds);
                        }
                    }


                    #endregion
                }
            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();
            }

            return fieldList;
        }

        /// <summary>
        /// Ajoute une valeur chaine dans la liste en vérifiant qu'elle n'y est pas déjà.
        /// </summary>
        /// <param name="Value">Valeur à ajouter</param>
        /// <param name="lst">Liste dans laquelle ajouter la valeur</param>
        private static void AddListValue(int Value, List<int> lst)
        {
            if (!lst.Contains(Value))
                lst.Add(Value);
        }

        /// <summary>
        /// Construit et retourne le libellé à afficher pour un fichier lié depuis un autre fichier
        /// ex : Contact (lié depuis Affaire).Nom
        /// </summary>
        /// <param name="tabRes"></param>
        /// <param name="fromTabRes"></param>
        /// <returns></returns>
        public static String GetLinkedFileLabel(String tabRes, String fromTabRes)
        {
            return String.Concat(tabRes, " (", "depuis ", fromTabRes, ")");
        }

    }

    /// <summary>
    /// Classe représentant un champ eudonet dans l'assistant Reporting XRM
    /// Utilisé pour représenter les champs disponibles pour le rapport.
    /// </summary>
    public class eReportWizardField
    {
        private Int32 _nDescId;
        private String _strToolTipText;
        private String _strLabel;
        private EudoQuery.FieldFormat _format;
        private Boolean _bIsDrawable = true;
        private PopupType _popup = PopupType.NONE;
        private Int32 _nPopupDescId = 0;
        private String _strLinkedFile = String.Empty;
        private Int32 _iLinkedFromTab = 0;

        public Boolean IsMultiple { get; private set; }


        #region Accesseurs


        /// <summary>
        /// Fichier lié
        /// </summary>
        public String LinkedFile
        {
            get { return this._strLinkedFile; }
        }

        /// <summary>
        /// Type de catalogue
        /// </summary>
        public PopupType PopupType
        {
            get { return _popup; }
        }

        /// <summary>
        /// PopupDescId dans le cas ou le champs est un catalogue lié
        /// </summary>
        public Int32 PopupDescId
        {
            get { return _nPopupDescId; }
        }

        /// <summary>
        /// Texte de l'infobulle du champ
        /// </summary>
        public String ToolTipText
        {
            get { return this._strToolTipText; }
        }

        /// <summary>
        /// DescId du champ
        /// </summary>
        public Int32 DescId
        {
            get { return this._nDescId; }
        }

        /// <summary>
        /// Format du champ
        /// </summary>
        public FieldFormat Format
        {
            get { return this._format; }
        }

        /// <summary>
        /// Libellé du champ
        /// </summary>
        public String Label
        {
            get { return this._strLabel; }
        }

        /// <summary>
        /// Définit le fait que le champs soit affichable ou pas dans la liste des champs du rapport
        /// </summary>
        public Boolean IsDrawable
        {
            get { return this._bIsDrawable; }
            internal set { this._bIsDrawable = value; }
        }
        /// <summary>
        /// DescId Du Fichier depuis lequel le fichier contenant ce champ est lié (lié depuis)
        /// </summary>
        public Int32 LinkedFromTab
        {
            get { return this._iLinkedFromTab; }
            set { this._iLinkedFromTab = value; }
        }


        /// <summary>
        /// Cible étendues
        /// </summary>
        public Int32 TargetDescdId { get; set; } = 0;

        #endregion

        /// <summary>
        /// Fichier parent du champ
        /// </summary>
        public Int32 Tab
        {
            get { return (this._nDescId - this._nDescId % 100); }
        }

        /// <summary>
        /// Constructeur de l'objet
        /// </summary>
        /// <param name="strDescId">DescId du champ</param>
        /// <param name="format">Format du champ</param>
        /// <param name="strToolTipText">Infobulle du champ</param>
        /// <param name="strLabel">Libellé du champ</param>
        /// <param name="Popup">Type de catalogue</param>
        /// <param name="PopupDescId">PopupDescId (utile uniquement si le champ est  d'un type catalogue)</param>
        /// <param name="LinkedFile">Fichier lié</param>
        public eReportWizardField(String strDescId, FieldFormat format, String strToolTipText, String strLabel, PopupType Popup, Int32 PopupDescId, String LinkedFile, Boolean multiple = false)
        {
            Int32.TryParse(strDescId, out _nDescId);
            this._format = format;
            this._strToolTipText = strToolTipText;
            this._strLabel = strLabel;
            this._nPopupDescId = PopupDescId;
            this._popup = Popup;
            this._strLinkedFile = LinkedFile;
            this.IsMultiple = multiple;
        }


    }
}