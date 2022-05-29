using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Result;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.Import.Exceptions;
using EudoQuery;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.CommonDTO;
using Com.Eudonet.Internal.eda;
using System.Linq;

namespace Com.Eudonet.Xrm.Import
{
    /// <summary>
    /// Traitement d'import de cible étendu
    /// </summary>
    public class eTargetImport
    {
        private ePref _pref;

        private Dictionary<int, string> _resDico;
        /// <summary>
        /// Liste des colonnes disponible pour l'import et leur label
        /// </summary>
        public IEnumerable<KeyValuePair<int, string>> FieldsTargetInfos { get { return _resDico; } }

        private Engine.Engine _engine;

        /// <summary>
        /// Evenement lancé au début d'import
        /// </summary>
        public ImportProcessHandler StartImport;

        /// <summary>
        /// Evenement lancé a la fin de l'import
        /// </summary>
        public ImportProcessHandler EndImport;

        /// <summary>
        /// Evenement lancé au début d'import de chaque ligne
        /// </summary>
        public ImportProcessHandler StartLineImport;

        /// <summary>
        /// Evenement lancé à la fin d'import de chaque ligne
        /// </summary>
        public ImportProcessHandler EndLineImport;

        /// <summary>
        /// Construit un objet metier sur l'import cible etendu
        /// </summary>
        /// <param name="pref">pref utilisateur</param>
        public eTargetImport(ePref pref)
        {
            this._pref = pref;

            this._resDico = new Dictionary<int, string>();
        }

        /// <summary>
        /// Construit la liste des rubriques disponibles pour la destinations des données
        /// </summary>
        /// <param name="tab">table cible</param>
        /// <param name="tabFrom">table de départ</param>
        /// <param name="evtId">id de la fiche de la table de départ</param>
        public void LoadTargetInfos(int tab, int tabFrom, int evtId)
        {
            // Format permis sur les rubriques non mappé
            StringBuilder formatSupported = new StringBuilder()
                .Append(FieldFormat.TYP_CHAR.GetHashCode())
                .Append(",").Append(FieldFormat.TYP_EMAIL.GetHashCode())
                .Append(",").Append(FieldFormat.TYP_PHONE.GetHashCode())
                .Append(",").Append(FieldFormat.TYP_MEMO.GetHashCode());

            // Rub syst permis
            StringBuilder shortDescidSupported = new StringBuilder()
                .Append(AllField.MEMO_NOTES.GetHashCode())
                .Append(",").Append(AllField.MEMO_INFOS.GetHashCode())
                .Append(",").Append(AllField.MEMO_DESCRIPTION.GetHashCode());

            StringBuilder sb = new StringBuilder().AppendFormat(
                "SELECT rub.[DescId], [RES].[LANG_{0}] AS RES, ", _pref.LangId.ToString("00")).AppendLine().Append(
                "   case isnull(rub.[ProspectEnabled], 0) when 0 then 1 else 0 end Sort1, ").AppendLine().Append(
                "   isnull(case rub.[ProspectEnabled] when 301 then 1 end, 0)+ ").AppendLine().Append(
                "   isnull(case rub.[ProspectEnabled] when 302 then 2 end, 0)+ ").AppendLine().Append(
                "   isnull(case rub.[ProspectEnabled] when 304 then 3 end, 0)+ ").AppendLine().Append(
                "   isnull(case rub.[ProspectEnabled] when 307 then 4 end, 0)+ ").AppendLine().Append(
                "   isnull(case rub.[ProspectEnabled] when 309 then 5 end, 0)+ ").AppendLine().Append(
                "   isnull(case rub.[ProspectEnabled] when 310 then 6 end, 0)+ ").AppendLine().Append(
                "   isnull(case rub.[ProspectEnabled] when 303 then 7 end, 0)+ ").AppendLine().Append(
                "   isnull(case rub.[ProspectEnabled] when 402 then 2 end, 0)+ ").AppendLine().Append(
                "   isnull(case rub.[ProspectEnabled] when 404 then 3 end, 0)+ ").AppendLine().Append(
                "   isnull(case rub.[ProspectEnabled] when 407 then 4 end, 0)+ ").AppendLine().Append(
                "   isnull(case rub.[ProspectEnabled] when 409 then 5 end, 0)+ ").AppendLine().Append(
                "   isnull(case rub.[ProspectEnabled] when 410 then 6 end, 0)+ ").AppendLine().Append(
                "   isnull(case rub.[ProspectEnabled] when 403 then 7 end, 0) as Sort2 ").Append(
                "FROM [DESC] rub ").AppendLine().Append(
                "   inner join [RES] on [RES].[ResId] = rub.[DescId] ").AppendLine().Append(
                "WHERE rub.[DescId] - rub.[DescId] % 100 = @tabDid ").AppendLine().Append(
                "   and isnull(rub.[ProspectEnabled], 0) <> 1 ").AppendLine().Append(
                "   and (rub.[DescId] <= @tabMaxField or rub.[DescId] % 100 in (").Append(shortDescidSupported).Append(")) ").AppendLine().Append(
                "   and rub.[Format] in (").Append(formatSupported).Append(") ").AppendLine().Append(
                "   and rub.[Popup] <> ").Append(PopupType.DATA.GetHashCode()).Append( //Desactivation des catalogues avancés dans le mappage
                "ORDER BY [Sort1], [Sort2], rub.[DispOrder]");

            RqParam rq = new RqParam(sb.ToString());
            rq.AddInputParameter("@tabDid", SqlDbType.Int, tab);
            rq.AddInputParameter("@tabMaxField", SqlDbType.Int, tab + eLibConst.MAX_NBRE_FIELD);

            string error = string.Empty;
            DataTableReaderTuned dtr = null;
            eudoDAL dal = eLibTools.GetEudoDAL(_pref);

            try
            {
                dal.OpenDatabase();

                dtr = dal.Execute(rq, out error);

                if (error.Length > 0)
                    throw new Exception(error);

                int descid = 0;
                while (dtr.Read())
                {
                    descid = dtr.GetEudoNumeric(0);
                    if (descid != 0)
                        _resDico.Add(descid, dtr.GetString(1));
                }
            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();

                if (dal != null)
                    dal.CloseDatabase();
            }
        }

        /// <summary>
        /// Lance l'inport a partir des infos de importInfos
        /// </summary>
        /// <param name="impInfos">Informations de paramètre de l'import</param>
        /// <param name="impContent">Manager de contenu</param>
        public void ImportData(eImportParams impInfos, eImportContent impContent)
        {
            string error;
            string initialError = string.Empty;

            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            eImportProgression progression = new eImportProgression();

            _engine = eModelTools.GetEngine(_pref, impInfos.TabMainDid, eEngineCallContext.GetCallContext(Common.Enumerations.EngineContext.APPLI));

            progression.TotalLine = impContent.NbLines;

            // Notification de début d'import
            Notify(StartImport, progression.GetEventArgs(_pref));

            try
            {
                int nbLineDuplicates;
                int newFileId;

                dal.OpenDatabase();

                // Si pas de données à importer on termine
                if (progression.TotalLine <= 0)
                    initialError = eResApp.GetRes(_pref.LangId, 1670);      // 1670 - vos données sont vides

                // Si pas d'informations sur la table cible
                TableLite mainTab = new TableLite(impInfos.TabMainDid);
                if (!mainTab.ExternalLoadInfo(dal, out error))
                {
                    // 1819 - Informations de la table <TAB> non trouvées
                    initialError = eResApp.GetRes(_pref, 1819).Replace("<TAB>", impInfos.TabMainDid.ToString());
                }

                if (initialError.Length != 0)
                {
                    eImportEventArgs eventArgs = progression.GetEventArgs(_pref,
                        eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, initialError, ""));

                    // Notification de fin d'import
                    Notify(EndImport, eventArgs);

                    return;
                }


                //recherche de la liaison parente si applicable
                if (impInfos.TabFromDid > 0 && impInfos.EvtId > 0)
                {
                    eAdminTableInfos tabMainInfos = eAdminTableInfos.GetAdminTableInfos(_pref, impInfos.TabMainDid);

                    tabMainInfos.LoadAllFieldsInfos(_pref);
                    //si liaison principale, on ne fait pas de traitement suplémentaire
                    if (tabMainInfos.InterEVTDescid != impInfos.TabFromDid && tabMainInfos.FieldsInfos != null)
                    {
                        //sinon on ajoute l'info du champ de liaison
                        var fldParent = tabMainInfos.FieldsInfos.FirstOrDefault(fld => fld.PopupDescId == impInfos.TabFromDid + 1 && fld.Relation);
                        if (fldParent != null)
                        {

                            impInfos.RelationParentDescId = fldParent.DescId;
                        }

                    }
                }

                // Construit la requete de vérification si la cible existe ou pas selon les  colonnes de dédoublonage définies
                string dedupeQuery = BuildCheckRequest(impContent, mainTab, impInfos.TabFromDid > 0 ? impInfos.EvtId : 0);

                eImportContentLine line;
                while (impContent.LinesIteratorMoveNext())
                {
                    // Raccourci
                    line = impContent.LinesIteratorCurrent;

                    progression.LineInProgress = line;

                    // Notification avant  l'import de la ligne
                    Notify(StartLineImport, progression.GetEventArgs(_pref));

                    try
                    {
                        // Vérifie si la ligne est vide on la zappe
                        if (line.Values.Count == 0)
                        {
                            progression.LinesErrorMsg.Add(new ImportLineErrorMsgEmpty() { Line = line });
                            continue;
                        }

                        // Vérifie si la cible existe en fonction des colonnes de dédoublonage
                        if (dedupeQuery.Length != 0)
                        {
                            nbLineDuplicates = TargetExists(dal, impContent, dedupeQuery, out error);

                            if (error.Length != 0)
                            {
                                progression.LinesErrorMsg.Add(new ImportLineErrorMsgCheckDouble() { Line = line, Error = error });
                                continue;
                            }

                            // Vérifie si la cible existe en fonction des colonnes de dédoublonage
                            if (nbLineDuplicates > 0)
                            {
                                progression.LinesErrorMsg.Add(new ImportLineErrorMsgDouble()
                                {
                                    Line = line,
                                    NbLineDuplicates = nbLineDuplicates,
                                    Columns = impContent.Columns
                                });
                                continue;
                            }
                        }

                        // Import de la ligne
                        eErrorContainer resultErr = ImportLine(dal, impContent, impInfos, out newFileId);
                        if (resultErr != null)
                        {
                            progression.LinesErrorMsg.Add(new ImportLineErrorMsgEngine() { Line = line, ErrContainer = resultErr });
                            continue;
                        }

                        if (newFileId != 0)
                            progression.FilesCreatedId.Add(newFileId);

                    }
                    catch (Exception exc)
                    {
                        progression.LinesErrorMsg.Add(new ImportLineErrorMsgEngine()
                        {
                            Line = line,
                            ErrContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "Erreur inattendue sur l'import de la ligne " + line.Index, string.Concat(exc.Message, Environment.NewLine, exc.StackTrace))
                        });
                    }
                    finally
                    {
                        // Notification après l'import de la ligne
                        Notify(EndLineImport, progression.GetEventArgs(_pref));
                    }
                }

                // Notification de fin d'import
                Notify(EndImport, progression.GetEventArgs(_pref));
            }
            catch (Exception ex)
            {
                eImportEventArgs eventArgs = progression.GetEventArgs(_pref,
                    eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, string.Concat(ex.Message, Environment.NewLine, ex.StackTrace)));
                Notify(EndImport, eventArgs);
            }
            finally
            {
                if (dal != null)
                    dal.CloseDatabase();
            }
        }

        /// <summary>
        /// Construit la requete de vérification
        /// </summary>
        /// <param name="impContent">Manager de contenu</param>
        /// <param name="mainTab">Informations de la principale</param>
        /// <returns>Requête sql</returns>
        private string BuildCheckRequest(eImportContent impContent, TableLite mainTab, int fromFileId)
        {
            string error = string.Empty;

            string fldName = string.Empty;
            StringBuilder rqSql = new StringBuilder();
            foreach (eImportContentColumn col in impContent.Columns)
            {
                if (!col.IsKey)
                    continue;

                // On génère le filtre sous form ex : [TEMPLATE_2].[TPL12] = @Param_<DESCID> AND ....
                fldName = string.Concat(mainTab.ShortField, (col.DescId % 100).ToString().PadLeft(2, '0'));
                if (rqSql.Length != 0)
                    rqSql.Append(" AND ");
                rqSql.Append("isnull([").Append(mainTab.TabName).Append("].[").Append(fldName).Append("], '') = @Param_").Append(col.DescId);
            }

            // On compte le nombre de cibles qui satisfait le critère des clés
            if (rqSql.Length > 0)
            {
                // Ajout du critère de ratachement à la fiche parente
                if (fromFileId > 0)
                    rqSql.Append(" AND [").Append(mainTab.TabName).Append("].[EvtId] = ").Append(fromFileId);

                return string.Concat("SELECT COUNT(1) FROM [", mainTab.TabName, "] WHERE ", rqSql);
            }

            return string.Empty;
        }

        /// <summary>
        /// Fait une recherche sur la base pour trouver une cible corespondant aux colonnes de dédoublonage definit dans l'assistant
        /// </summary>
        /// <param name="dal">Connexion ouverte à la base</param>
        /// <param name="impContent">Manager de contenu</param>
        /// <param name="dedupeQuery">Requête de dédoublonnage</param>
        /// <param name="error">Description d'erreur</param>
        /// <returns>Nombre de fiche trouvées</returns>
        private int TargetExists(eudoDAL dal, eImportContent impContent, string dedupeQuery, out string error)
        {
            error = string.Empty;

            if (dedupeQuery.Length == 0)
                return 0;

            // Requete paramêtrée
            RqParam rq = new RqParam(dedupeQuery);

            eImportContentLine line = impContent.LinesIteratorCurrent;
            foreach (eImportContentColumn column in impContent.Columns)
                if (column.IsKey)
                    rq.AddInputParameter("@Param_" + column.DescId, SqlDbType.VarChar, line.Values[column.Index]);

            DataTableReaderTuned dtr = dal.Execute(rq, out error);
            if (dtr != null && dtr.Read() && error.Length == 0)
                return dtr.GetInt32UnSafe(0);

            return 0;
        }

        /// <summary>
        /// Importe la ligne d'index lineIndex depuis importInfos,
        /// Renseigne importEvent du resultat
        /// </summary>
        /// <param name="dal">Connexion ouverte à la base</param>
        /// <param name="impContent">Manager de contenu</param>
        /// <param name="impInfos">Informations de paramètre de l'import</param>
        /// <param name="newFileId">Id de fiche crée</param>
        /// <returns>Description d'erreur Si pas d'erreur, retour null</returns>
        private eErrorContainer ImportLine(eudoDAL dal, eImportContent impContent, eImportParams importInfos, out int newFileId)
        {
            newFileId = 0;
            eErrorContainer resultError = null;

            _engine.ResetContextAndValues();

            // Création
            _engine.FileId = 0;

            // Fields
            eImportContentLine line = impContent.LinesIteratorCurrent;
            foreach (eImportContentColumn column in impContent.Columns)
            {
                if (column.DescId != 0)
                    _engine.AddNewValue(column.DescId, line.Values[column.Index], true);
            }

            // Ratachement à la fiche parente
            if (importInfos.TabFromDid > 0 && importInfos.EvtId > 0)
            {
                if (importInfos.RelationParentDescId == 0)
                    _engine.AddTabValue(importInfos.TabFromDid, importInfos.EvtId);
                else
                    _engine.AddNewValue(importInfos.RelationParentDescId, importInfos.EvtId.ToString(), true);
            }


            _engine.AddParam("noorm", "1");

            // Process
            _engine.EngineProcess(new StrategyCruSimple(), null, dal);

            EngineResult engResult = _engine.Result;

            if (EngineResultHasNewRecordFile(engResult))
            {
                // Creation réussie
                newFileId = engResult.NewRecord.FilesId[0];
            }
            else
            {
                // Création echouée
                if (engResult.Error != null && (engResult.Error.DebugMsg.Length != 0 || engResult.Error.Msg.Length != 0))
                {
                    resultError = engResult.Error;
                }
                else
                {
                    // 6340 - Import cible étendue
                    // 1801 - Une erreur est survenue lors de la création
                    resultError = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 1801), "", eResApp.GetRes(_pref, 6568), "Retour d'erreur engine inconnue.");
                }
            }

            return resultError;
        }

        /// <summary>
        /// Savoir si engine Result a pu créer un enregistrement ou pas
        /// </summary>
        /// <param name="engResult">retour d'engine</param>
        /// <returns>indique si le retour d'engine correspond bien à une création et qu'il n'a pas rencontré d'erreur</returns>
        private Boolean EngineResultHasNewRecordFile(EngineResult engResult)
        {
            return engResult != null && engResult.Success
                && engResult.NewRecord != null && engResult.NewRecord.FilesId != null && engResult.NewRecord.FilesId.Count > 0;
        }

        /// <summary>
        /// Lance l'evenment handler avec des l'arguement args
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        private void Notify(ImportProcessHandler handler, eImportEventArgs args)
        {
            if (handler != null)
                handler(this, args);
        }
    }
}
