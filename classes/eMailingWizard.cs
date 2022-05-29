using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// 
    /// </summary>
    public class eMailingWizard
    {
        private eMailing _mailing;
        private TypeMailing _mailingType;
        private Int32 _nTab = 0;
        private Int32 _nCurrentListCount = 0;
        private Int32 _nCheckedListCount = 0;
        private Boolean _bCurrentMultipleAddress = false;
        private Boolean _bMarkedMultipleAddress = false;



        /// <summary>préférences de l'utilisateur.</summary>
        private ePref _pref;

        //Parametres de base de connexion 
        private eudoDAL _dal;
        private Boolean _bIsLocalDal = true;
        private Boolean _bIsLocalDalOpened = false;

        private List<KeyValuePair<String, String>> _lstFiles;
        private List<eMailingWizardField> _lstFields;
        private List<String> _lstWorkingDescids;

        private EdnType _FileType;
        private Boolean _bIsEvent;
        private Boolean _bInterPP;
        private Boolean _bInterPM;
        private Boolean _bInterEvent;
        private Boolean _bAdrJoin;
        private Boolean _bProspectEnabled;
        private Int32 _nInterEventNum;
        private Boolean _bFromBkm;
        private String _strTabName = String.Empty;
        private String _strWizardError = String.Empty;
        private Dictionary<Int32, String> _lstRelatedFilters;


        private String _strError = String.Empty;

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
        public List<eMailingWizardField> Fields
        {
            get { return this._lstFields; }
        }

        /// <summary>
        /// Mode list : Le nombre de fiche en cours
        /// </summary>
        public int CurrentListCount
        {
            get { return this._nCurrentListCount; }
        }

        /// <summary>
        /// Mode list : Le nombre de fiche marquées
        /// </summary>
        public int CurrentCheckedListCount
        {
            get { return this._nCheckedListCount; }

        }

        /// <summary>
        /// Savoir si au moins une fiche de la liste en cours  a des adresses multiples 
        /// </summary>
        public Boolean HasCurrentListMultipleAddress
        {
            get { return this._bCurrentMultipleAddress; }
        }

        /// <summary>
        /// Savoir si au moins une fiche de la liste marquées a des adresses multiples 
        /// </summary>
        public Boolean HasCheckedListMultipleAddress
        {
            get { return this._bMarkedMultipleAddress; }
        }

        /// <summary>
        /// Liste des rubriques de type adresse mail
        /// </summary>
        public String Error
        {
            get { return this._strError; }
        }

        /// <summary>
        /// Savoir si c'est un invit++
        /// </summary>
        public Boolean bIsInvit
        {
            get { return this._bInterPP && this._bAdrJoin; }
        }

        /// <summary>
        /// Savoir si c'est cible etendue
        /// </summary>
        public Boolean bProspectEnabled
        {
            get { return this._bProspectEnabled; }
        }

        /// <summary>
        /// DescId de la table en cours
        /// </summary>
        public Int32 Tab
        {
            get { return this._nTab; }
        }

        /// <summary>
        /// DescId (et pas eventNum, allez savoir pourquoi !?) de l'event lié de la table en cours
        /// </summary>
        public Int32 InterEventNum
        {
            get { return this._nInterEventNum; }
        }


        #endregion

        /// <summary>
        /// Constructeur de la classe utilisé dans le cas de l'édition ou l'on a déjà un objet eMailing existant.
        /// </summary>
        /// <param name="ePref">Préférences de l'utilisateur</param>
        /// <param name="nTab">Tab en cours</param>
        /// <param name="mailing">Objet eMailing</param>
        /// <param name="bFromBkm">Provenance d'un signet (probablement à revoir)</param>
        public eMailingWizard(ePref ePref, Int32 nTab, eMailing mailing, TypeMailing mailingType, Boolean bFromBkm = false)
            : this(ePref, nTab, mailingType, bFromBkm)
        {
            this._mailing = mailing;

            if (_mailing != null)
                this._nInterEventNum = _mailing.ParentTab;

            if (mailingType != TypeMailing.MAILING_FROM_CAMPAIGN)
            {
                ComputeCounters();

                if (_mailing != null)//par defaut la lignes cochées

                    //Par defaut s'il y a des fiches marquées on preselection dans l'assistant cette option sur l'asssitant donc on met markedFile a 1                
                    this._mailing.SetParamValue("markedFiles", (_nCheckedListCount > 0) ? "1" : "0");
                this._mailing.MailingParams.UpdateContainsValue("nMailCount", (_nCheckedListCount > 0) ? _nCheckedListCount.ToString() : _nCurrentListCount.ToString());

                //Par defaut, le premier mail dans le dico est preselectionné dans l'assistant  
                IEnumerator enumerator = _mailing.EmailFields.Keys.GetEnumerator();
                if (enumerator.MoveNext())
                    _mailing.MailingParams.AddOrUpdateValue("mailFieldDescId", enumerator.Current.ToString(), true);
            }
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="ePref">Préférences de l'utilisateur</param>
        /// <param name="nTab">Tab en cours</param>
        /// <param name="mailingType">Type de Mailing</param>
        /// <param name="bFromBkm">Provenance d'un signet (probablement à revoir)</param>
        public eMailingWizard(ePref ePref, Int32 nTab, TypeMailing mailingType, Boolean bFromBkm = false)
        {

            this._mailingType = mailingType;
            this._pref = ePref;
            this._nTab = nTab;
            this._bFromBkm = bFromBkm;
            this._lstFiles = new List<KeyValuePair<String, String>>();
            this._lstFields = new List<eMailingWizardField>();
            this._lstWorkingDescids = new List<string>();
            this._lstRelatedFilters = new Dictionary<Int32, String>();


            LoadTabInfo();

        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="ePref"></param>
        /// <param name="mailing"></param>
        public eMailingWizard(ePref ePref, eMailing mailing)
        {
            this._mailing = mailing;
            this._pref = ePref;
        }

        /// <summary>
        /// On compte le nombre de fiches (liste en cours/marquées)
        /// </summary>
        public void ComputeCounters()
        {

            Boolean bPPorPMorEVT = false;
            String sErr = string.Empty;

            try
            {
                OpenDal();

                bPPorPMorEVT = _nTab == TableType.PP.GetHashCode() || _nTab == TableType.PM.GetHashCode() || _bIsEvent;

                FilterSel FilterSel = null;
                this._pref.Context.Filters.TryGetValue(_nTab, out FilterSel);
                Int32 nFilterId = (FilterSel != null && FilterSel.FilterSelId > 0) ? FilterSel.FilterSelId : 0;

                String strSQLCurrentCountQ = string.Empty;

                if (bPPorPMorEVT)
                {
                    String strSQLCurrentMultipleAdressQ = string.Empty;

                    String strSQLMarkedCountQ = string.Empty;
                    String strSQLMarkedMultipleAdressQ = string.Empty;

                    MarkedFilesSelection ms = null;
                    _pref.Context.MarkedFiles.TryGetValue(_nTab, out ms);
                    Boolean msActiv = ms != null && ms.Enabled;
                    Boolean drawFicheCoche = ms != null && ms.NbFiles > 0;

                    //Requete du nombre de fiche en cours
                    strSQLCurrentCountQ = eDataTools.BuildCountQuery(_pref, _nTab, nFilterId, msActiv, false, _dal);

                    //Adresse multiple
                    if (_nTab == TableType.PP.GetHashCode() || _nTab == TableType.PM.GetHashCode())
                        strSQLCurrentMultipleAdressQ = eDataTools.BuildCountQuery(_pref, _nTab, nFilterId, msActiv, true, _dal);

                    //Requete du nombre de fiche marquées en cours
                    if (drawFicheCoche)
                    {
                        strSQLMarkedCountQ = eDataTools.BuildCountQuery(_pref, _nTab, nFilterId, true, false, _dal);
                        if (_nTab == TableType.PP.GetHashCode() || _nTab == TableType.PM.GetHashCode())
                            strSQLMarkedMultipleAdressQ = eDataTools.BuildCountQuery(_pref, _nTab, nFilterId, true, true, _dal);
                    }
                    if (!msActiv) // si "Afficher uniquement les fiches cochées" n'est pas activé on compte la liste en cours
                        _nCurrentListCount = eDataTools.GetCountByQuery(String.Concat(strSQLCurrentCountQ, ";", strSQLCurrentMultipleAdressQ), _dal, out this._bCurrentMultipleAddress, out sErr);

                    if (sErr.Length > 0)
                        throw (new Exception(string.Concat("eTools.GetCountByQuery._nSelectedCount : ", Environment.NewLine, sErr)));

                    if (drawFicheCoche)
                        _nCheckedListCount = eDataTools.GetCountByQuery(String.Concat(strSQLMarkedCountQ, ";", strSQLMarkedMultipleAdressQ), _dal, out this._bMarkedMultipleAddress, out sErr);

                    if (sErr.Length > 0)
                        throw (new Exception(string.Concat("eTools.GetCountByQuery._nCheckedCount : ", Environment.NewLine, sErr)));

                }
                else
                {//cible etendu ou invitations

                    //Requete du nombre de fiche en cours
                    strSQLCurrentCountQ = eDataTools.BuildCountQuery(_pref, _nTab, nFilterId, false, false, _dal, this._mailing.ParentTab, _mailing.ParentFileId);

                    _nCurrentListCount = eDataTools.GetCountByQuery(strSQLCurrentCountQ, _dal, out sErr);

                    if (sErr.Length > 0)
                        throw (new Exception(string.Concat("eTools.GetCountByQuery._nCheckedCount : ", Environment.NewLine, sErr)));


                }
            }
            catch (Exception ex)
            {
                _strError = String.Concat("eMailingWizard.cs :: ComputeCounters ", Environment.NewLine, ex.Message, Environment.NewLine, ex.StackTrace);
            }
            finally
            {
                CloseDal();
            }
        }

        /// <summary>
        /// Charge les informations relative à la table en cours (type, nom, liaisons...)
        /// </summary>
        private void LoadTabInfo()
        {
            string sErr = String.Empty;

            try
            {
                OpenDal();

                eTableLiteMailing tabMain = eLibTools.GetTableInfo(this._dal, _nTab, eTableLiteMailing.Factory(_pref));

                this._FileType = tabMain.EdnType;
                this._bAdrJoin = tabMain.AdrJoin;

                this._bInterPP = tabMain.InterPP;
                this._bInterPM = tabMain.InterPM;




                //Il faut le descid de la table de départ du mailing, pas la liaison principale des fiches de destinataire
                // en effet, si le mailing n'est pas faite depuis cet event principal (liaison template.evtid) mais depuis un catalogue relation,
                // alors, on va rechercher une mauvaise liaison               
                //this._bInterEvent = tabMain.InterEVT; // Semble inutile
                //this._nInterEventNum = _mailing.ParentTab; //tabMain.InterEVTDescid;


                this._bIsEvent = tabMain.TabType == TableType.EVENT;
                this._bProspectEnabled = tabMain.ProspectEnabled;
            }
            catch (Exception exp)
            {
                throw new Exception(String.Concat("LoadTabInfo: ", exp.Message));
            }
            finally
            {
                CloseDal();
            }
        }

        /// <summary>
        /// Peuple la liste interne avec les descid/libellés des fichiers liés/dépendants de la table
        /// </summary>
        private void AddFiles()
        {
            /*
            Boolean bAddTemplate = _mailingType.Equals(TypeMailing.PRINT) || _mailingType.Equals(TypeMailing.EXPORT) || _mailingType.Equals(TypeMailing.MERGE);

            List<String> fileDescIds = new List<String>();

            AddListValue(_nTab.ToString(), fileDescIds);
            switch (_nTab)
            {
                case 200:
                    AddListValue(TableType.PM.GetHashCode().ToString(), fileDescIds);
                    AddListValue(TableType.ADR.GetHashCode().ToString(), fileDescIds);
                    break;
                case 300:
                    AddListValue(TableType.PP.GetHashCode().ToString(), fileDescIds);
                    AddListValue(TableType.ADR.GetHashCode().ToString(), fileDescIds);
                    break;
                default:
                    if (_bIsEvent)
                    {
                        if (_bInterPP)
                            AddListValue(TableType.PP.GetHashCode().ToString(), fileDescIds);
                        if (_bInterPM)
                            AddListValue(TableType.PM.GetHashCode().ToString(), fileDescIds);
                        if (_bInterPP && _bInterPM)
                            AddListValue(TableType.ADR.GetHashCode().ToString(), fileDescIds);
                    }
                    break;
            }
            if (!_FileType.Equals(EdnType.FILE_MAIN))
            {
                if (_bInterPP)
                {
                    AddListValue(TableType.PP.GetHashCode().ToString(), fileDescIds);
                    if (this._bAdrJoin || _bFromBkm)
                        AddListValue(TableType.ADR.GetHashCode().ToString(), fileDescIds);
                }
                if (this._bInterPM || (this._bFromBkm || this._bAdrJoin))
                    AddListValue(TableType.PM.GetHashCode().ToString(), fileDescIds);

                if (this._bInterEvent)
                    if (this._nInterEventNum == 0)
                        AddListValue("100", fileDescIds);
                    else
                        AddListValue(((this._nInterEventNum + 10) * 100).ToString(), fileDescIds);

            }

            AddSpecialLink(fileDescIds);
            AddDependentFiles(fileDescIds);

            // une fois tous les descid de tables liées chargés on actualise la liste
            // En retirant les tables pour lesquelles l'utilisateur en cours
            // n'a pas les droits de visu.
            fileDescIds = GetViewableDescids(_pref, _dal, fileDescIds, true);

            String strRes = String.Join(",", fileDescIds.ToArray());
            Boolean bFound = false;
            String strFileRes = String.Empty;
            Int32 nFileDescId = 0;
            eRes res = new eRes(_pref.GetSqlInstance, _pref.GetBaseName, _pref.Lang, strRes);

            foreach (String DescId in fileDescIds)
            {
                Int32.TryParse(DescId, out nFileDescId);
                strFileRes = res.GetRes(nFileDescId, out bFound);
                if (bFound)
                {
                    this._lstFiles.Add(new KeyValuePair<String, String>(DescId, strFileRes));
                    bFound = false;
                }
            }
            /*this._lstFiles.Sort(
                delegate(KeyValuePair<String, String> firstPair,
                KeyValuePair<String, String> nextPair)
                {
                    return firstPair.Value.CompareTo(nextPair.Value);
                }
            );
            */
        }

        /// <summary>
        /// Ajout dans la liste passée en paramètre les descid des champs liés par des champs de liaison
        /// </summary>
        /// <param name="fileDescIds">Liste de DescId</param>
        private void AddSpecialLink(List<String> fileDescIds)
        {
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
                    throw new Exception(String.Concat("eMailingWizard.GetSpecialLink() : ", _strWizardError));
                if (dtr.HasRows)
                {
                    Int32 nPopupDescId = 0;
                    while (dtr.Read())
                    {
                        nPopupDescId = dtr.GetEudoNumeric("POPUPDESCID");
                        nPopupDescId = nPopupDescId - (nPopupDescId % 100);
                        if (!fileDescIds.Contains(nPopupDescId.ToString()))
                            fileDescIds.Add(nPopupDescId.ToString());
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
        private void AddDependentFiles(List<String> fileDescIds)
        {
            StringBuilder sbQuery = new StringBuilder();
            StringBuilder sbWhere = new StringBuilder();

            sbQuery.Append("SELECT [DESC].[DESCID] ,[DESC].[FILE],[RES].[").Append(_pref.Lang).Append("] AS LIBELLE ")
                .Append(" FROM [DESC] LEFT JOIN [RES] ON [DESC].[DESCID] = [RES].[RESID] WHERE CAST( [DESC].[DESCID]  AS INT ) % 100 = 0 ");

            switch (_nTab)
            {
                case 200:
                    sbWhere.Append(" ISNULL([DESC].[INTERPP],0) <> 0 ");
                    break;
                case 300:
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
                .Append(" OR [DESC].[DESCID] IN (SELECT CAST(([DESCID] / 100) AS INT) * 100 FROM [DESC] WHERE [POPUP] = 2 AND ISNULL([RELATION],0) <> 0 AND [POPUPDESCID] = (@ntab +1)) ")
                .Append(" OR [DESC].[DESCID] IN (SELECT POPUPDESCID - 1 FROM [DESC] WHERE [POPUP] = 2 AND ISNULL([RELATION],0) <> 0 AND CAST(([DESCID] / 100) AS INT) * 100 = @ntab)  ) ");

            sbQuery.Append(" ORDER BY LIBELLE");

            RqParam rqDependantFile = new RqParam(sbQuery.ToString());
            rqDependantFile.AddInputParameter("@ntab", SqlDbType.Int, _nTab);
            rqDependantFile.AddInputParameter("@intereventnum", SqlDbType.Int, this._nTab == TableType.EVENT.GetHashCode() ? 0 : ((_nTab / 100) - 10));

            DataTableReaderTuned dtr = _dal.Execute(rqDependantFile, out _strWizardError);
            try
            {
                if (!String.IsNullOrEmpty(_strWizardError))
                    throw new Exception(String.Concat("eMailingWizard.AddDependentFiles() : ", _strWizardError));
                if (dtr.HasRows)
                {
                    Int32 nDescId = 0;
                    while (dtr.Read())
                    {
                        nDescId = dtr.GetEudoNumeric("DESCID");
                        nDescId = nDescId - (nDescId % 100);
                        if (!fileDescIds.Contains(nDescId.ToString()))
                            fileDescIds.Add(nDescId.ToString());
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

            if (this._mailing == null)
                return null;
            List<Int32[]> linkedTabs = new List<Int32[]>();
            Int32 tabDescId = 0;
            Int32 fromTabDescId = 0;
            Int32 selectedField = 0;
            eRes res;
            List<int> resList = new List<int>();
            String[] fields = _mailing.GetParamValue("field").Split(';');
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
            List<int> viewableDescIds = GetViewableDescids(_dal, resList, true);


            //parcours des fichiers liés pour comparer avec les descid autorisés en visu afin de les ajouter dans la liste retournée.
            foreach (Int32[] tabInfos in linkedTabs)
            {
                //Si le fichier depuis lequel ce fichier est lié n'est plus visible, on masque le fichier lié depuis également ?
                //TODO check si c'est un bon fonctionnement
                if (!viewableDescIds.Contains(tabInfos[0]) || !viewableDescIds.Contains(tabInfos[1]))
                    continue;
                String tabRes = res.GetRes(tabInfos[0], out bFound);
                if (!bFound)
                    throw new Exception("eMailingWizard.AddlinkedFromFiles() : Libellé du fichier \"lié\" non trouvé.");
                String fromTabRes = res.GetRes(tabInfos[1], out bFound);
                if (!bFound)
                    throw new Exception("eMailingWizard.AddlinkedFromFiles() : Libellé du fichier \"lié depuis\" non trouvé.");

                fileList.Add(new KeyValuePair<String, String>(String.Concat(tabInfos[0], "_", tabInfos[1]), eMailingWizard.GetLinkedFileLabel(tabRes, fromTabRes)));
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
        /// <returns>List de eMailingWizardFields</returns>
        private List<eMailingWizardField> AddFields(Int32 workingTab)
        {
            return eMailingWizard.AddFields(this._pref, this._dal, workingTab, out this._strWizardError);


        }

        /// <summary>
        /// Retourne la liste des fichiers visible et exploitable pour le fichier passé en paramètres
        /// </summary>
        /// <param name="pref">préférences de l'utilisateur en cours</param>
        /// <param name="dal">EudoDal pouet la connexion à la base de données</param>
        /// <param name="workingTab">Fichier cible</param>
        /// <param name="error">Message d'Erreur</param>
        /// <returns>List de eMailingWizardFields</returns>
        public static List<eMailingWizardField> AddFields(ePref pref, eudoDAL dal, Int32 workingTab, out String error)
        {
            List<eMailingWizardField> fieldList = new List<eMailingWizardField>();
            List<String> workingDescIds = new List<String>();

            StringBuilder sbQuery = new StringBuilder();
            sbQuery.Append("SELECT [DESC].[DESCID], [DESC].[FORMAT], [DESC].[TOOLTIPTEXT], ISNULL([DESC].[POPUP],0) [POPUP], ISNULL([DESC].[POPUPDESCID],0) [POPUPDESCID],")
            .Append(" CASE WHEN ([DESC].[POPUP] = 2 AND [DESC].[POPUPDESCID] <> [DESC].[DESCID]) THEN ")
            .Append(" (SELECT R2.[LANG_00] FROM RES R2 WHERE R2.RESID = ([DESC].[POPUPDESCID] - [DESC].[POPUPDESCID] % 100)) ELSE '' END [FILELABEL], ")
            .Append(" [RES].[").Append(pref.Lang).Append("] AS LABEL, ISNULL(P,1) AS P ")
            .Append(" FROM [DESC] INNER JOIN [RES] ON [DESC].[DESCID] = [RES].[RESID] ")
            .Append(" left join cfc_getPermInfo(").Append(pref.User.UserId).Append(",").Append(pref.User.UserLevel).Append(",").Append(pref.User.UserGroupId).Append(") A on A.permissionid=[DESC].viewpermid ")
            .Append(" WHERE ")

            .Append(" [DESCID] > @ntab AND [DESCID] < @ntab + 100 AND ")
            .Append(" ( [DESC].[FORMAT] NOT IN (0, 15)  OR [DESC].[DESCID] = @ntab + 91) ")
            .Append(" AND [DESC].[DESCID] % 100 NOT IN (76,77,78,79,80) ");
            RqParam rqFields = new RqParam(sbQuery.ToString());
            rqFields.AddInputParameter("@ntab", SqlDbType.Int, workingTab);
            DataTableReaderTuned dtr = dal.Execute(rqFields, out error);
            try
            {
                if (!String.IsNullOrEmpty(error))
                    throw new Exception(String.Concat("eMailingWizard.AddFields()  : ", error));

                if (dtr.HasRows)
                {
                    eMailingWizardField field;
                    while (dtr.Read())
                    {
                        if (dtr.GetString("P") == "1")
                        {

                            field = new eMailingWizardField(dtr.GetString("DESCID"),
                                (FieldFormat)dtr.GetEudoNumeric("FORMAT"),
                                dtr.GetString("TOOLTIPTEXT"),
                                dtr.GetString("LABEL"),
                                (PopupType)Enum.Parse(typeof(PopupType), dtr.GetString("POPUP")),
                               dtr.GetEudoNumeric("POPUPDESCID"),
                               dtr.GetString("FILELABEL"));


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

                    throw new Exception(String.Concat("eMailingWizard.LoadAvailableFilters List.cs", " ", List.ErrorMsg));
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
                case 200:
                    if (tab == 300 || tab == 400)
                        return false;
                    break;
                case 300:
                    if (tab == 200 || tab == 400)
                        return false;
                    break;
            }
            return true;
        }

        /// <summary>
        /// Retourne une liste actualisée des descid visibles parmis la liste transmise en paramètre
        /// </summary>
        /// <param name="dal">EudoDAL pour la connexion à la base de données</param>
        /// <param name="DescIds">Liste des descid à tester</param>
        /// <param name="bFiles">Test sur table ou champs</param>
        /// <returns>LIste actualisée des descids</returns>
        private List<int> GetViewableDescids(eudoDAL dal, IEnumerable<int> DescIds, bool bFiles)
        {
            //Droits de visu
            string error = String.Empty;
            List<int> authorizedDescIds = new List<int>();




            using (DataTableReaderTuned dtrRights = eLibDataTools.GetRqViewRight(_pref.User.UserId, _pref.User.UserLevel, _pref.User.UserGroupId, _pref.User.UserLang, DescIds, dal))
            {
                if (dtrRights == null || !String.IsNullOrEmpty(error))
                    throw new Exception(String.Concat("eMailingWizard.CheckRights() : ", error));

                while (dtrRights.Read())
                {
                    Boolean bVisible = false;
                    // TODO - utiliser la fonction eTools.GetTabViewRight ?
                    bVisible = dtrRights.GetInt32("VIEW_P") == 1 && (!bFiles || dtrRights.GetBoolean("Disabled"));

                    if (bVisible)
                        AddListValue(dtrRights.GetEudoNumeric("DESCID"), authorizedDescIds);
                }
            }


            return authorizedDescIds;
        }


        /// <summary>
        /// retourne la liste des étapes
        /// </summary>
        /// <param name="mt"></param>
        /// <returns></returns>
        public List<eWizardStep> GetWizardStep(eMailingWizardRenderer mt)
        {
            List<eWizardStep> res = new List<eWizardStep>();
            switch (mt.Mailing.MailingType)
            {

                case TypeMailing.SMS_MAILING_FROM_BKM:
                    //Liste des étapes sms
                    bool consentEnabled = false;

                    string useNewUnsubscribeMethodValue = string.Empty;
                    if (eLibTools.GetConfigAdvValues(_pref, new HashSet<eLibConst.CONFIGADV> { eLibConst.CONFIGADV.USE_NEW_UNSUBSCRIBE_METHOD }).TryGetValue(eLibConst.CONFIGADV.USE_NEW_UNSUBSCRIBE_METHOD, out useNewUnsubscribeMethodValue) && useNewUnsubscribeMethodValue == "1")
                        consentEnabled = true;


                    //Etape 1 - Corps du sms
                    res.Add(new eWizardStep()
                    {
                        IsActive = true,
                        Label = eResApp.GetRes(_pref, 383),
                        ID = "smsbody",
                        Order = 1
                    }
                    ) ;


                    //Pour l'instant on n'affiche pas l'étape 2
                    //if (consentEnabled)
                    //{
                    //    //Etape 2 - Type de campagne
                    //    res.Add(new eWizardStep()
                    //    {
                    //        IsActive = false,                          
                    //        Label = eResApp.GetRes(_pref, 8713),
                    //        ID = "cpgtyp",
                    //        Order = res.Count + 1
                    //    }
                    //    ); ;
                    //}

                    //Etape 3 - Options d'envoi
                    res.Add(new eWizardStep()
                    {
                        IsActive = false,
                        Label = eResApp.GetRes(_pref, 6403),
                        ID = "sendingoption",
                        Order = res.Count + 1
                    }
                    );

                    break;
            }

            return res;
        }

        /// <summary>
        /// Retourne le nombre d'étape nécessaire à l'assistant mailing en fonction du type de rapport demandé.
        /// </summary>
        /// <param name="mailingType">Type de rapport</param>
        /// <returns>Nombre d'étape</returns>
        public Int32 GetTotalSteps(TypeMailing mailingType)
        {
            switch (mailingType)
            {
                case TypeMailing.MAILING_FROM_LIST:

                    //depuis liste de pp/pm on affiche tjrs l'etape destinatires (y a un choix sur l'adresse active ou principale)
                    if (this._nTab == TableType.PP.GetHashCode() || this._nTab == TableType.PM.GetHashCode())
                        return 7;

                    //Depuis une liste d'event
                    if (this._nCheckedListCount > 0 || _mailing.EmailFields.Count > 1)
                        return 7;

                    return 6;

                case TypeMailing.MAILING_FROM_BKM:
                case TypeMailing.MAILING_FOR_MARKETING_AUTOMATION:

                    //Affiche l etape choix de destinataire si on a plusieures adresses mail
                    if (_mailing.EmailFields.Count > 1)
                        return 7;

                    return 6;
                // Pour le sms on a un seul ecran
                case TypeMailing.SMS_MAILING_FROM_BKM:
                    return 3;

                case TypeMailing.MAILING_FROM_CAMPAIGN:
                    //Affiche l etape choix de destinataire si on a plusieures adresses mail
                    if (_mailing.EmailFields.Count > 1)
                        return 7;

                    return 6;



                default:
                    return 7;
            }
        }

        /// <summary>
        /// Retourne la liste des champs 
        /// </summary>
        /// <param name="pref">Préférences de l'utilisateur en cours</param>
        /// <param name="selectedTab">Fichier lié</param>
        /// <param name="linkedFromTab">Fichier depuis lequel le selectedTab est lié (options lié depuis)</param>
        /// <returns>List de MailingWizardField</returns>
        public static List<eMailingWizardField> GetLinkedFields(ePref pref, Int32 selectedTab, Int32 linkedFromTab)
        {

            eudoDAL dal = eLibTools.GetEudoDAL(pref);
            try
            {
                dal.OpenDatabase();
                return eMailingWizard.GetLinkedFields(pref, dal, selectedTab, linkedFromTab);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Concat("eMailingWizard.GetLinkedFields() : ", ex.Message));
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
        public static List<eMailingWizardField> GetLinkedFields(ePref pref, eudoDAL dal, Int32 selectedTab, Int32 linkedFromTab)
        {
            List<eMailingWizardField> fieldList = new List<eMailingWizardField>();
            List<String> fileDescIds = new List<String>();
            String errorMessage = String.Empty;
            StringBuilder sbQuery = new StringBuilder();
            sbQuery.Append("SELECT [DESC].[DESCID], [DESC].[FORMAT], [DESC].[TOOLTIPTEXT], ISNULL([DESC].[POPUP],0) [POPUP], ISNULL([DESC].[POPUPDESCID],0) [POPUPDESCID],")
            .Append(" CASE WHEN ([DESC].[POPUP] = 2 AND [DESC].[POPUPDESCID] <> [DESC].[DESCID]) THEN ")
            .Append(" (SELECT R2.[LANG_00] FROM RES R2 WHERE R2.RESID = ([DESC].[POPUPDESCID] - [DESC].[POPUPDESCID] % 100)) ELSE '' END [FILELABEL], ")
            .Append(" [RES].[").Append(pref.Lang).Append("] AS LABEL")
            .Append(" FROM [DESC] INNER JOIN [RES] ON [DESC].[DESCID] = [RES].[RESID] WHERE ")
            .Append(" [DESCID] > @ntab AND [DESCID] < @ntab + 100 AND ")
            .Append(" ( [DESC].[FORMAT] NOT IN (0, 15)  OR [DESC].[DESCID] = @ntab + 91) ")
            .Append(" AND [DESC].[DESCID] % 100 NOT IN (76,77,78,79,80) ");
            RqParam rqFields = new RqParam(sbQuery.ToString());
            rqFields.AddInputParameter("@ntab", SqlDbType.Int, selectedTab);
            DataTableReaderTuned dtr = dal.Execute(rqFields, out errorMessage);
            try
            {
                if (!String.IsNullOrEmpty(errorMessage))
                    throw new Exception(String.Concat("eMailingWizard.AddFields()  : ", errorMessage));

                if (dtr.HasRows)
                {
                    eMailingWizardField field;
                    while (dtr.Read())
                    {
                        field = new eMailingWizardField(dtr.GetString("DESCID"),
                            (FieldFormat)dtr.GetEudoNumeric("FORMAT"),
                            dtr.GetString("TOOLTIPTEXT"),
                            dtr.GetString("LABEL"),
                            (PopupType)Enum.Parse(typeof(PopupType), dtr.GetString("POPUP")),
                            dtr.GetEudoNumeric("POPUPDESCID"), dtr.GetString("FILELABEL"));
                        field.LinkedFromTab = linkedFromTab;
                        fieldList.Add(field);
                        fileDescIds.Add(dtr.GetString("DESCID"));
                    }

                    #region droits

                    //Droits de visu
                    String _err = String.Empty;
                    List<int> authorizedDescIds = new List<int>();


                    using (DataTableReaderTuned dtrRights = eLibDataTools.GetRqViewRight(
                        pref.User.UserId, pref.User.UserLevel, pref.User.UserGroupId, pref.User.UserLang, fileDescIds.Select(zz => int.Parse(zz)), dal))
                    {
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
                dtr?.Dispose();
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

        /// <summary>
        /// Ouvre le dal
        /// </summary>
        private void OpenDal()
        {
            if (_bIsLocalDal && !_bIsLocalDalOpened)
            {
                _dal = eLibTools.GetEudoDAL(_pref);
                this._dal.OpenDatabase();
                this._bIsLocalDalOpened = true;
            }
        }

        /// <summary>
        /// Ferme le dal
        /// </summary>
        private void CloseDal()
        {
            if (_bIsLocalDal && !_bIsLocalDalOpened)
            {
                this._dal.CloseDatabase();
                this._bIsLocalDalOpened = false;
            }
        }

    }

    /// <summary>
    /// Classe représentant un champ eudonet dans l'assistant Mailing XRM
    /// Utilisé pour représenter les champs disponibles pour le rapport.
    /// </summary>
    public class eMailingWizardField
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
        public eMailingWizardField(String strDescId, FieldFormat format, String strToolTipText, String strLabel, PopupType Popup, Int32 PopupDescId, String LinkedFile)
        {
            Int32.TryParse(strDescId, out _nDescId);
            this._format = format;
            this._strToolTipText = strToolTipText;
            this._strLabel = strLabel;
            this._nPopupDescId = PopupDescId;
            this._popup = Popup;
            this._strLinkedFile = LinkedFile;
        }


    }


    /// <summary>
    /// Etaptes d'un wizqrd
    /// </summary>
    public class eWizardStep
    {
        /// <summary>
        /// Ordre de l'étapes (commence à 1)
        /// </summary>
        public int Order;

        /// <summary>
        /// Libelle de l'étape
        /// </summary>
        public string Label = "";


        /// <summary>
        /// Id de l'étape (pour manipulation JS)
        /// </summary>
        public string ID ="";

        /// <summary>
        /// Etape active
        /// </summary>
        public bool IsActive;

        /// <summary>
        /// Etapes désactivée
        /// </summary>
        public bool IsDisabled;

    }
}