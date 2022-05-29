using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Result;
using Com.Eudonet.Engine.Result.Xrm;
using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal.mailchecker;
using System.Threading.Tasks;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Common.CommonDTO;
using Com.Eudonet.Common.DatasDirectory;
using Com.Eudonet.Xrm.eda;

namespace Com.Eudonet.Xrm
{
    /// <className>eUpdateFieldManager</className>
    /// <summary>Manager de liaison entre le moteur javascript Engine.js et le moteur d'enregistrement Engine.cs</summary>
    /// <purpose>Cette clase s'occupe de récupèrer et préparer les informations qui lui sont transmises, de lancer l'enregistrement par le biais de la classe Engine.cs et ensuite de construire le retour XML des actions mené par le moteur de formule</purpose>
    /// <authors>HLA</authors>
    /// <date>2012-05-03</date>
    public class eUpdateFieldManager : eEngineMgr
    {

        private EdnUpdContext _updContext = null;
        private eudoDAL _dal = null;


        /// <summary>
        /// Mise à cjour / création
        /// </summary>
        protected override void ProcessManager()
        {
            try
            {
                _dal = eLibTools.GetEudoDAL(_pref);
                _dal.OpenDatabase();

                #region Récupération des paramètres

                DoLoadContext(_dal, out _localError);
                if (_localError.Length > 0)
                    return;

                #endregion

                #region renommage de pièces jointes
                bool bPJRenamed = false;
                DoRenamePJ(out bPJRenamed);
                if (!bPJRenamed)
                    return;

                #endregion



                #region Gestion de catalogue - catalogEditor

                if (_updContext.FieldEditorType.Equals("catalogEditor") || _updContext.FieldEditorType.Equals("stepCatalogEditor"))
                {

                    foreach (EdnUpdFldContext fldContext in _updContext.LstUpdFld)
                    {
                        eUpdateField updFld = fldContext.Field;

                        // En création de fiche ou modification pour les formules du milieu, nous avons la liste complète des rubriques de la fiches. On ignore donc les rubriques sans PopupDescId
                        // sph - 37647 : suppression de la condition sur fileid=0
                        if (_updContext.CruAction == XrmCruAction.CHECK_ONLY_MIDDLE && updFld.PopupDescId == 0)
                            continue;

                        if (string.IsNullOrEmpty(updFld.NewDisplay) || !_updContext.InsertCatalogValue
                            || (updFld.Popup != PopupType.DATA && updFld.Popup != PopupType.ONLY))
                            continue;


                        #region Ajout dans le catalogue

                        eTools.AddNewValueInCatalog(_dal, _pref, updFld, _updContext.AutoComplete, false, out _localError);

                        if (_localError.Length > 0)
                            return;

                        #endregion
                    }
                }

                #endregion

                try
                {
                    // Lance l'engine
                    if (_updContext.EdnUpdTyp == EdnUpdContext.UpdType.CLONE_PLANNING)
                    {
                        DoCloneCal();
                    }
                    else
                    {
                        if (!DoUpdate())
                        {
                            return;
                        }

                        int iOldFileId = eLibTools.GetNum(_updContext.CloneParams.GetParam("fileid"));
                        if (iOldFileId > 0)
                        {

                            if (_updContext.EdnUpdTyp == EdnUpdContext.UpdType.CLONE_BKM)
                                DoCloneBkm(iOldFileId);
                        }
                    }
                }
                catch (Exception e)
                {
                    #region Feedback

                    string err = string.Concat("Message:", e.Message, Environment.NewLine, "StackTrace:", e.StackTrace, Environment.NewLine);
                    eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, err), _pref);

                    #endregion

                    _localError = e.ToString();
                    return;
                }
                finally
                {

                }



                // Recalcul les MRU
                DoMruFields(_dal);


                // Recalcul les MRU dse champs de liaisons
                DoMruTabs(_dal);
            }
            catch (eEndResponseException ex)
            {
            }
            catch (Exception ex)
            {
                _localError = ex.ToString();
                return;
            }
            finally
            {

                if (_dal != null)
                    _dal.CloseDatabase();


                DoResponse();

            }
        }

        /// <summary>
        /// Chargement des params du context de la page
        /// </summary>
        /// <param name="dal"></param>
        /// <param name="error"></param>
        private void DoLoadContext(eudoDAL dal, out string error)
        {
            error = string.Empty;

            int reqIntVal;
            eUpdateField fldUpd = null;
            _updContext = new EdnUpdContext();

            try
            {
                // Paramètres obligatoires
                _updContext.FileId = _requestTools.GetRequestFormKeyI("fileId") ?? 0;

                // Recup des informations des fields
                foreach (string key in _requestTools.AllKeys)
                {
                    if (!key.StartsWith("fld_"))
                        continue;

                    fldUpd = eUpdateField.GetUpdateFieldFromDesc(_requestTools.GetRequestFormKeyS(key));
                    //Ne pas mettre à jour les rubriques mot de passe, pour cela il nous faut le format du field
                    FieldLite f = new FieldLite(fldUpd.Descid);
                    f.ExternalLoadInfo(dal, out error);
                    if (error.Length > 0)
                        throw new Exception(error);

                    if (fldUpd == null)
                        continue;

                    fldUpd.SetParam(f);

                    if (f.Format == FieldFormat.TYP_PASSWORD)
                        continue;
                    else if (f.Format == FieldFormat.TYP_NUMERIC)
                    {
                        //A ce stade le newvalue devrait etre au format fr (séparateur décimale ",")
                        // Cependant, le front end accepete les valeurs  avec "." et la transmet telle quelle
                        // cela pose pb a l'ORM, ci dessous un traitement pour forcer la ','

                        if (!string.IsNullOrEmpty(fldUpd.NewValue))
                            fldUpd.NewValue = fldUpd.NewValue.Replace('.', ',');
                    }

                    _updContext.LstUpdFld.Add(new EdnUpdFldContext(fldUpd));
                }

                if (_requestTools.GetRequestFormKey("tab", out reqIntVal))
                    _updContext.TabDescId = reqIntVal;
                else if (_updContext.LstUpdFld.Count >= 1)
                    _updContext.TabDescId = _updContext.LstUpdFld[0].Field.Descid - _updContext.LstUpdFld[0].Field.Descid % 100;
                else
                {
                    _localError = "Aucun paramètres fournis.";
                    return;
                }

                _updContext.Params.AddParam("crtldescid", _requestTools.GetRequestFormKeyS("crtldescid") ?? string.Empty);

                _updContext.TabFromDescId = _requestTools.GetRequestFormKeyI("parenttab") ?? 0;
                _updContext.FileIdFrom = _requestTools.GetRequestFormKeyI("parentfileid") ?? 0;

                eTableLiteUpdate mainTab = new eTableLiteUpdate(_updContext.TabDescId);
                mainTab.ExternalLoadInfo(dal, out error);
                if (error.Length > 0)
                    return;

                int fldTableDid = 0;
                bool correctFld = false;
                foreach (EdnUpdFldContext fldContext in _updContext.LstUpdFld)
                {
                    //liaisons custom (Planning.Projet par ex.)
                    if (mainTab.DicCustomLinks.ContainsKey(fldContext.Field.Descid))
                    {
                        fldContext.Field.Descid = mainTab.DicCustomLinks[fldContext.Field.Descid].Descid; // on remplace le descid de la table par le descid du champ par l'intermédiaire duquel la liaison est faite
                    }

                    eUpdateField updFld = fldContext.Field;

                    fldTableDid = updFld.Descid - updFld.Descid % 100;

                    // Rubrique de la main table
                    correctFld = _updContext.TabDescId == fldTableDid;
                    // Rubrique 200 (PPID)
                    correctFld = correctFld || ((mainTab.InterPP || mainTab.TabType == TableType.ADR) && updFld.Descid == TableType.PP.GetHashCode());
                    // Rubrique 300 (PMID)
                    correctFld = correctFld || ((mainTab.InterPM || mainTab.TabType == TableType.ADR) && updFld.Descid == TableType.PM.GetHashCode());
                    // Rubrique 400 (ADRID)
                    correctFld = correctFld || (mainTab.AdrJoin && updFld.Descid == TableType.ADR.GetHashCode());
                    // Rubrique X00 (EVTID / PARENTEVTID)
                    correctFld = correctFld || (mainTab.InterEVT && updFld.Descid == mainTab.InterEVTDescid);
                    // Rubrique de 400 (ADRXX) et 300 (PMID) depuis une création de PP
                    correctFld = correctFld || (0 == _updContext.FileId && TableType.PP == mainTab.TabType
                        && (TableType.ADR.GetHashCode() == fldTableDid || TableType.PM.GetHashCode() == updFld.Descid));

                    if (!correctFld)
                        throw new Exception("Toutes les rubriques doivent être de la même table.");


                    //traitement sur le corps du mail pour respecter la norme rfc
                    if (fldContext.Field.Table.EdnType == EdnType.FILE_MAIL && fldContext.Field.Descid % 100 == (int)MailField.DESCID_MAIL_HTML)
                    {
                        if (_updContext.LstUpdFld.Find(f => f.Field.Descid % 100 == (int)MailField.DESCID_MAIL_ISHTML)?.Field?.NewValue == "1")
                            try
                            {
                                bool _MailBodyNcharPerLine = false;
                                var dicConfigAdv = eLibTools.GetConfigAdvValues(_pref, new System.Collections.Generic.List<eLibConst.CONFIGADV>() { eLibConst.CONFIGADV.SPLITTING_MAIL_BODY_NCHAR_PER_LINE });
                                if (dicConfigAdv.ContainsKey(eLibConst.CONFIGADV.SPLITTING_MAIL_BODY_NCHAR_PER_LINE) && !string.IsNullOrEmpty(dicConfigAdv[eLibConst.CONFIGADV.SPLITTING_MAIL_BODY_NCHAR_PER_LINE]))
                                    _MailBodyNcharPerLine = dicConfigAdv[eLibConst.CONFIGADV.SPLITTING_MAIL_BODY_NCHAR_PER_LINE] == "1";
                                fldContext.Field.NewValue = _MailBodyNcharPerLine ? eLibTools.GetMailBodyAbout70CharPerLine(fldContext.Field.NewValue) : fldContext.Field.NewValue;
                            }
                            catch (Exception e)
                            {

                            }

                    }



                }

                // Paramèters complémentaires
                if (_requestTools.GetRequestFormKey("engAction", out reqIntVal))
                    _updContext.CruAction = (XrmCruAction)reqIntVal;

                // Recherche prédictive
                _updContext.AutoComplete = _requestTools.GetRequestFormKeyB("autoComplete") ?? false;
                // en creation autoComplete n'est pas toujours transmis
                if (_updContext.FileId == 0)
                    _updContext.AutoComplete = eAutoCompletionTools.IsEnabled(_pref, AutoCompletionType.AUTO_COMPLETION_ADDRESS, _updContext.TabDescId);

                _updContext.FieldEditorType = _requestTools.GetRequestFormKeyS("fldEditorType") ?? string.Empty;
                _updContext.InsertCatalogValue = _requestTools.GetRequestFormKeyB("catNewVal") ?? false;

                // Paramètres additionnels
                _updContext.Params.AddParam("fieldTrigger", _requestTools.GetRequestFormKeyS("fieldTrigger") ?? string.Empty);
                _updContext.Params.AddParam("adrtoupd", _requestTools.GetRequestFormKeyS("adrtoupd") ?? string.Empty);
                _updContext.Params.AddParam("refresh", _requestTools.GetRequestFormKeyS("refresh") ?? string.Empty);

                // Paramètres spécifiques template E-mail
                _updContext.Params.AddParam("mailDN", _requestTools.GetRequestFormKeyS("mailDN") ?? string.Empty);
                _updContext.Params.AddParam("mailRT", _requestTools.GetRequestFormKeyS("mailRT") ?? string.Empty);
                _updContext.Params.AddParam("mailCSS", _requestTools.GetRequestFormKeyS("mailCSS") ?? string.Empty);
                _updContext.Params.AddParam("mailPJ", _requestTools.GetRequestFormKeyS("mailPJ") ?? string.Empty);
                _updContext.Params.AddParam("mailSaveAsDraft", (_requestTools.GetRequestFormKeyB("mailSaveAsDraft") ?? false) ? "1" : "0");
                _updContext.Params.AddParam("mailIsDraft", (_requestTools.GetRequestFormKeyB("mailIsDraft") ?? false) ? "1" : "0");
                _updContext.Params.AddParam("mailIsTest", (_requestTools.GetRequestFormKeyB("mailIsTest") ?? false) ? "1" : "0");

                // Information de la source de l'enregistrement sur XRM
                _updContext.Params.AddParam("onValideFileAction", (_requestTools.GetRequestFormKeyB("onValideFileAction") ?? false) ? "1" : "0");
                _updContext.Params.AddParam("onBlurAction", (_requestTools.GetRequestFormKeyB("onBlurAction") ?? false) ? "1" : "0");

                // Information de la fenêtre de confirmation des formules du milieu de l'ORM
                _updContext.Params.AddParam("ormId", (_requestTools.GetRequestFormKeyI("ormId") ?? 0).ToString());
                _updContext.Params.AddParam("ormUpdates", _requestTools.GetRequestFormKeyS("ormUpdates") ?? string.Empty);
                _updContext.Params.AddParam("ormResponseObj", _requestTools.GetRequestFormKeyS("ormResponseObj") ?? string.Empty);

                #region Paramètres spécifiques pour la duplication de fiche

                if (_requestTools.GetRequestFormKey("fileid0", out reqIntVal) && reqIntVal > 0)
                {
                    _updContext.CloneParams.AddParam("fileid", reqIntVal.ToString());

                    string sBkmIds = string.Empty;
                    if (_requestTools.GetRequestFormKey("bkmids", out sBkmIds) && sBkmIds.Length > 0)
                    {
                        _updContext.CloneParams.AddParam("bkmids", sBkmIds);
                        _updContext.EdnUpdTyp = EdnUpdContext.UpdType.CLONE_BKM;
                    }
                }


                #endregion

                #region Paramètres spécifiques pour la duplication de fiche planning

                string dtFormStr = GetFormatDateTime(_requestTools.GetRequestFormKeyS("newDateBegin") ?? string.Empty);
                if (dtFormStr.Length > 0)
                {
                    _updContext.CloneParams.AddParam("newDateBegin", Convert.ToDateTime(dtFormStr).ToString());
                    _updContext.EdnUpdTyp = EdnUpdContext.UpdType.CLONE_PLANNING;
                }

                dtFormStr = GetFormatDateTime(_requestTools.GetRequestFormKeyS("newDateEnd") ?? string.Empty);
                if (dtFormStr.Length > 0)
                    _updContext.CloneParams.AddParam("newDateEnd", Convert.ToDateTime(dtFormStr).ToString());

                if (_requestTools.GetRequestFormKey("oldFileid", out reqIntVal))
                    _updContext.CloneParams.AddParam("oldFileid", reqIntVal.ToString());

                #endregion

            }
            catch (Exception ex)
            {
                error = ex.ToString();
            }
        }

        /// <summary>
        /// Duplication de fiche planning (copier/coller)
        /// </summary>
        private void DoCloneCal()
        {


            Engine.Engine engClonePlanning = eModelTools.GetEngine(_pref, _updContext.TabDescId, eEngineCallContext.GetCallContext(EngineContext.APPLI));

            engClonePlanning.OldFileId = eLibTools.GetNum(_updContext.CloneParams.GetParam("oldFileid"));
            engClonePlanning.AddParam("newDateBegin", _updContext.CloneParams.GetParam("newDateBegin"));
            engClonePlanning.AddParam("newDateEnd", _updContext.CloneParams.GetParam("newDateEnd"));

            engClonePlanning.EngineProcess(new ClonePlanningRqParam());

            _engResult = engClonePlanning.Result;
        }


        /// <summary>
        /// Duplication unitaire : Duplication des signets
        /// </summary>
        private void DoCloneBkm(int nOldFileId)
        {
            string oBkmsIds = _updContext.CloneParams.GetParam("bkmids");

            if (!ResultAnError(_engResult) && !_engResult.NewRecord.Empty && nOldFileId > 0)
            {
                Engine.Engine engCloneBkms = eModelTools.GetEngine(_pref, _updContext.TabDescId, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                engCloneBkms.FileId = _engResult.NewRecord.FilesId[0];
                engCloneBkms.OldFileId = nOldFileId;
                engCloneBkms.AddParam("bkmsids", oBkmsIds);
                engCloneBkms.EngineProcess(new CloneBookmarksRqParam());
                _engResultBkm = engCloneBkms.Result;    //GCH - SPRINT 2014.16 - 33267 - Gestion d'erreur lors de la duplication des signets depuis une duplication de fiche
            }
        }


        //procède au renommage de la pj
        private void DoRenamePJ(out bool bSuccess)
        {
            bSuccess = true;

            EdnUpdFldContext fPJName = _updContext.LstUpdFld.Find(f => f.Field.Descid == (int)PJField.LIBELLE);
            if (fPJName == null)
            {
                return;
            }

            string sDatasPath = DatasUtility.GetPhysicalDatasPath(DatasFolderType.ROOT, _context, _pref.GetBaseName);
            string sUsrError = string.Empty;
            eFilePJ filePJ = (eFilePJ)eFileMain.CreateMainFile(_pref, (int)TableType.PJ, _updContext.FileId, 0);

            string sError = ePJTraitements.RenamePJ(_pref,
                _pref.User,
                sDatasPath,
                filePJ.GetLinkedTab,
                filePJ.GetLinkedFileId,
                _updContext.FileId,
                filePJ.GetName,
                fPJName.Field.NewValue,
                out sUsrError);

            if (sError.Length > 0)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6650).Replace("<FILE>", filePJ.GetName), sUsrError, eResApp.GetRes(_pref, 72), sError);
                bSuccess = false;
                return;
            }
            else if (sUsrError.Length > 0)
            {
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6650).Replace("<FILE>", filePJ.GetName), sUsrError, eResApp.GetRes(_pref, 72));
                bSuccess = false;
                return;
            }

        }

        private bool DoUpdate()
        {
            List<eUpdateField> updMainFld = null;           // Enregistrement sur la table main
            List<eUpdateField> updAdrFld = null;            // Enregistrement sur la table ADR à la création d'une fiche PP

            #region Sépare les nouvelles valeurs en fonction des différentes listes créées ci-dessus

            updMainFld = new List<eUpdateField>();

            // Création d'un PP avec ADDRESS
            if (_updContext.FileId == 0 && _updContext.TabDescId == TableType.PP.GetHashCode())
            {
                updAdrFld = new List<eUpdateField>();

                foreach (EdnUpdFldContext fldContext in _updContext.LstUpdFld)
                {
                    if (TableType.PP.GetHashCode() == fldContext.Field.Descid - fldContext.Field.Descid % 100)
                        updMainFld.Add(fldContext.Field);
                    else
                        updAdrFld.Add(fldContext.Field);
                }
            }
            else
            {
                foreach (EdnUpdFldContext fldContext in _updContext.LstUpdFld)
                {
                    updMainFld.Add(fldContext.Field);
                }
            }

            #endregion



            Engine.Engine eng = eModelTools.GetEngine(_pref, _updContext.TabDescId, eEngineCallContext.GetCallContext(EngineContext.APPLI));

            eng.TabFrom = _updContext.TabFromDescId;
            eng.FileIdFrom = _updContext.FileIdFrom;
            eng.FileId = _updContext.FileId;
            eng.AddParams(_updContext.Params.GetValues);

            eng.AddParam("ExternalUrl", _pref.AppExternalUrl);
            eng.AddParam("DatabaseUid", _pref.DatabaseUid);

            //Les rubriques de type 'ReadOnly', sont mis a jour avec leurs valeurs par defaut qui sont sauvegardées dan.
            // HLA - Gestion de l'autobuildname en mode création en popup - Dev #33529
            bool updateFld;
            foreach (eUpdateField updFld in updMainFld)
            {
                updateFld = updFld.ChangedValue == null || updFld.ChangedValue.Value;

                if (updateFld)
                {
                    AddValue(eng, updFld);

                }
            }


            ResultStrategy resStrategy = new ResultXrmCru();
            if (_updContext.CruAction == XrmCruAction.CHECK_ONLY_MIDDLE)        // Mode RecordBeingCreated
            {
                if (_updContext.FileId == 0)
                    resStrategy = new ResultXrmCruBeingCreated();
                else
                    resStrategy = new ResultXrmCruBeingUpdated();
            }

            if (_updContext.TabDescId == (int)TableType.XRMWIDGET ||
                _updContext.TabDescId == (int)TableType.XRMGRID ||
                _updContext.TabDescId == (int)TableType.XRMHOMEPAGE)
                eng.EngineProcess(new StrategyCruSimple(), resStrategy);
            else if (_updContext.TabDescId == (int)TableType.USER)
                eng.EngineProcess(new StrategyCruUser(), resStrategy);
            else if (_updContext.TabDescId == (int)TableType.RGPDTREATMENTSLOGS)
                eng.EngineProcess(new StrategyCruRGPDTreatmentLog(), resStrategy);
            else if (_updContext.AutoComplete)
                eng.EngineProcess(new StrategyCruAutoCompletionXrm(_updContext.CruAction), resStrategy);
            else
                eng.EngineProcess(new StrategyCruXrm(_updContext.CruAction), resStrategy);


            _engResult = eng.Result;
            if (_updContext.Params.GetParam("refresh").Equals("1"))
            {
                _engResult.ReloadHeader = true;
                _engResult.ReloadDetail = true;
            }
            if (_engResult.Confirm.Mode != EngineConfirmMode.NONE)
                return false;

            if (_engResult.Success && _updContext.TabDescId == (int)TableType.USER && _engResult.NewRecord.FilesId.Count > 0)
            {
                //Quand l'on crée un utilisateur / When we create an user
                foreach (EdnUpdFldContext updFld in _updContext.LstUpdFld)
                {
                    if (updFld.Field.Descid == (int)UserField.USER_PROFILE)
                    {
                        List<string> lstUserGroup = new List<string>();
                        for (int i = 0; i < _engResult.NewRecord.FilesId.Count; i++)
                        {
                            lstUserGroup.Add(_engResult.NewRecord.FilesId[i].ToString());
                        }

                        int userSrc = 0;
                        int.TryParse(updFld.Field.NewValue, out userSrc);

                        if (userSrc != 0) //Dans le cas où il n'y a pas d'utilisateur / In case where there is no user
                            eAdminAccessPref.CopyUserAllPrefs(_pref, userSrc, lstUserGroup);
                    }
                }
            }

            //Demande #60 616: lors d'une duplication, on fait une copie physique l'image 
            if (_engResult.Success && _engResult.NewRecord.FilesId.Count == 1 && _updContext.FileId == 0)
                foreach (eUpdateField updFld in updMainFld)
                {
                    //on vérifie si le champ image est renseigné
                    updateFld = updFld.ChangedValue == null || updFld.ChangedValue.Value;
                    if (_updContext.FileId == 0 && !string.IsNullOrEmpty(updFld.NewValue))
                    {
                        //on récupère les infos de la rubrique
                        FieldLite fld = eLibTools.GetFieldInfo(_pref, updFld.Descid, FieldLite.Factory());
                        //on vérifie si c'est champ de type image
                        if (fld.Format == FieldFormat.TYP_IMAGE)
                        {
                            //on récupère l'image à partir du DescId
                            eImageField image = eImageField.GetImageField(_pref, _updContext.TabFromDescId, updFld.Descid, _engResult.NewRecord.FilesId[0]);
                            //on vérifie si il s'agit bein d'une sauvegarde physique de l'image
                            if (image.StorageType == ImageStorage.STORE_IN_FILE)
                                //on fait la copie
                                image.Copy();
                        }


                    }

                }

            if (updAdrFld != null && updAdrFld.Count > 0 && !ResultAnError(_engResult) && !_engResult.NewRecord.Empty && _updContext.EdnUpdTyp == EdnUpdContext.UpdType.CLASSIC)
            {
                #region création ADDRESS

                // Attention, ne prend pas en compte les formules du milieu.
                // Sinon il faut le faire en même temps que la verif des formules du milieu à la creation de PP et faire l'enregistrement de PP et ADR après
                Engine.Engine engAdr = eModelTools.GetEngine(_pref, TableType.ADR.GetHashCode(), eEngineCallContext.GetCallContext(EngineContext.APPLI));

                // eng.ModeDebug = modeDebug;
                // eng.Tab = _updContext.TabDescId;
                // eng.TabFrom = _updContext.TabFromDescId;
                // eng.FileIdFrom = _updContext.FileIdFrom;
                //eng.FileId = _updContext.FileId;

                // Ajout du nouveau ID de PP
                engAdr.AddTabValue(TableType.PP.GetHashCode(), _engResult.NewRecord.FilesId[0]);
                // Nouvelles valeurs d'adr
                foreach (eUpdateField updFld in updAdrFld)
                    if (updFld.ChangedValue == null || updFld.ChangedValue.Value)
                        AddValue(engAdr, updFld);

                if (_updContext.FileId == 0)
                    resStrategy = new ResultXrmCruBeingCreatedAdr();

                // Inutile de calculer les ResultXrm car les valeurs de l'engAdr ne sont pas retourner dans le XRM
                // Pour ne pas prendre ne compte les formule du milieu comme évoqué plus haut, on passe directement à l'état CHECK_MIDDLE_OK
                // TODO - Si l'on veux gérer les formules du milieu sur l'adresse
                if (_updContext.AutoComplete)
                    engAdr.EngineProcess(new StrategyCruAutoCompletionXrm(_updContext.CruAction), resStrategy);
                else
                    engAdr.EngineProcess(new StrategyCruXrm(XrmCruAction.CHECK_MIDDLE_OK), resStrategy);

                // On reporte l'erreur de ADR sur le result de PP
                if (engAdr.Result == null)
                    _engResult = null;
                else if (!engAdr.Result.Success)
                {
                    _engResult.Success = false;
                    _engResult.SetError(engAdr.Result.Error);
                }
                else
                {
                    _engResult.AddExternalResult(engAdr.Result);
                }

                // SHA : #73 334
                if (engAdr.Result == null)
                    _engResult = null;

                #endregion
            }



            if (eMailCheckerTools.CheckIsVerifyEnabled(_pref))
            {
                foreach (eUpdateField updFld in updMainFld)
                {
                    if (!string.IsNullOrEmpty(updFld.NewValue))
                    {
                        FieldLite fld = eLibTools.GetFieldInfo(_pref, updFld.Descid, FieldLite.Factory());
                        //Vérification l'adresse email à la saisie
                        if (fld.Format == FieldFormat.TYP_EMAIL)
                        {

                            Task t = eMailCheckerTools.UpdateEmail(_pref, updFld.NewValue);
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>Init des MRU de type catalogues</summary>
        /// <param name="dal"></param>
        private void DoMruFields(eudoDAL dal)
        {
            string error = string.Empty;
            StringBuilder sbDescid = new StringBuilder();

            bool bFromUserCat = (_requestTools.GetRequestFormKeyS("fldEditorType") ?? "").ToLower() == "catalogUserEditor".ToLower();

            foreach (EdnUpdFldContext fldContext in _updContext.LstUpdFld)
            {
                eUpdateField updFld = fldContext.Field;

                //Pas de load MRU si champ de liaison vers table
                if (updFld.Descid.ToString().EndsWith("00"))
                    continue;

                // Pas de MRU
                //  - si valeur vide
                //  - pour les rubriques autres que catalogue ou catalogue USER
                //  - pour les catalogues mutiples et liés
                if (string.IsNullOrEmpty(updFld.NewDisplay)
                    || (updFld.Popup != PopupType.DATA && updFld.Popup != PopupType.ONLY && !bFromUserCat)
                    || updFld.BoundDescid != 0)
                    continue;

                if (sbDescid.Length != 0)
                    sbDescid.Append(";");
                sbDescid.Append(updFld.Descid);
            }

            if (sbDescid.Length == 0)
                return;

            // Ouverture des param
            eParam param = new eParam(_pref);
            param.LoadFieldMru(dal, sbDescid.ToString(), out error);
            if (error.Length > 0)
            {
                eFeedbackXrm.LaunchFeedbackXrm(
                    eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, string.Concat("Chargements des nouvelles MRU FIELD en échec. ", error)), _pref);
                return;
            }

            if (param.ParamMruField.Count == 0)
                return;

            // Liste de CatalogueValue
            eParam.ParamMruFieldItem mruItem = null;
            foreach (EdnUpdFldContext fldContext in _updContext.LstUpdFld)
            {
                eUpdateField updFld = fldContext.Field;

                //Pas de load MRU si champ de liaison vers table
                if (updFld.Descid.ToString().EndsWith("00"))
                    continue;

                if (!param.ParamMruField.TryGetValue(updFld.Descid, out mruItem))
                    continue;   //Un champ peu ne pas avoir de MRU, exemple : le champ note

                int nItemId = 0;
                string itemId = string.Empty, itemDisplay = string.Empty;
                string[] tmp = null;
                string[] mruItemValues = mruItem.Values.Split("$|$");

                // Recup le catalogue avec les valeurs du MRU demandée
                int cnt = 0;
                StringBuilder mruDbNewValue = new StringBuilder();
                StringBuilder mruParamIFrameNewValue = new StringBuilder();

                foreach (string value in mruItemValues)
                {
                    tmp = value.Split(";");
                    if (tmp.Length != 2)
                        continue;

                    itemId = tmp[0];
                    itemDisplay = tmp[1];

                    if (int.TryParse(itemId, out nItemId) && !itemDisplay.Equals(updFld.NewDisplay))
                    {
                        mruDbNewValue.Append(nItemId).Append(";");
                        mruParamIFrameNewValue.Append(nItemId).Append(";").Append(itemDisplay).Append("$|$");
                        cnt++;
                    }

                    // MRU limité à 7 valeurs
                    if (cnt == 6)
                        break;
                }

                // Ajout de la première valeur
                if (mruItem.FldFormat == FieldFormat.TYP_USER)
                {
                    mruDbNewValue.Insert(0, string.Concat(updFld.NewValue, ";"));
                    mruParamIFrameNewValue.Insert(0, string.Concat(updFld.NewValue, ";", updFld.NewDisplay, "$|$"));
                }
                else
                {
                    eCatalog.CatalogValue newCatValue = new eCatalog.CatalogValue(updFld.NewDisplay);
                    //BSE #51 493 : dans le cas d'un catalogue avancé, on se base sur l'id pour l'insertion dans la table MRU
                    int catValueId;
                    if ((updFld.Popup == PopupType.DATA || updFld.Popup == PopupType.SPECIAL) && int.TryParse(updFld.NewValue, out catValueId))
                    {
                        newCatValue.Label = "";
                        newCatValue.Id = catValueId;
                    }

                    // #55418 : Pas de recherche dans le catalogue pour un catalogue de type SPECIAL
                    if (updFld.Popup != PopupType.SPECIAL)
                    {
                        List<eCatalog.CatalogValue> lstcat = new List<eCatalog.CatalogValue>();
                        foreach (string s in updFld.NewDisplay.Split(';'))
                        {
                            lstcat.Add(new eCatalog.CatalogValue(s));
                        }

                        eCatalog wholeCatalog = new eCatalog(dal, _pref, updFld.Popup, _pref.User, updFld.PopupDescId, false, lstcat);
                        newCatValue = wholeCatalog.Find(newCatValue);
                    }

                    if (newCatValue != null)
                    {
                        mruDbNewValue.Insert(0, string.Concat(newCatValue.Id, ";"));
                        mruParamIFrameNewValue.Insert(0, string.Concat(newCatValue.Id, ";", newCatValue.DisplayValue, "$|$"));
                    }
                }

                // Supprime le dernier séparateur de la chaine
                if (mruDbNewValue.Length > 0)
                    mruDbNewValue.Length = mruDbNewValue.Length - 1;
                if (mruParamIFrameNewValue.Length > 0)
                    mruParamIFrameNewValue.Length = mruParamIFrameNewValue.Length - 3;

                int mruId = mruItem == null ? 0 : mruItem.MruId;

                bool bDoCloseDal = false;
                if (dal == null)
                {
                    dal = eLibTools.GetEudoDAL(_pref);
                    dal.OpenDatabase();
                    bDoCloseDal = true;
                }
                try
                {
                    if (eMru.UpdateMRU(dal, _pref.User.UserId, _updContext.TabDescId, updFld.Descid, mruDbNewValue.ToString(), mruId))
                    {
                        if (_mruResult == null)
                            _mruResult = new Dictionary<int, string>();

                        _mruResult.Add(updFld.Descid, mruParamIFrameNewValue.ToString());
                    }
                }
                finally
                {
                    if (bDoCloseDal)
                        dal.CloseDatabase();
                }
            }
        }

        /// <summary>Init des MRU de type tables</summary>
        /// <param name="dal"></param>
        private void DoMruTabs(eudoDAL dal)
        {
            string error = string.Empty;
            StringBuilder sbTabid = new StringBuilder();

            foreach (EdnUpdFldContext fldContext in _updContext.LstUpdFld)
            {
                eUpdateField updFld = fldContext.Field;

                // Pas de MRU
                //  - si valeur vide
                //  - si champ de liaison vers table
                if (string.IsNullOrEmpty(updFld.NewDisplay))
                    continue;
                int nTabId = 0;
                if (updFld.Descid.ToString().EndsWith("00"))
                    nTabId = updFld.Descid;
                else if (updFld.PopupDescId > 0 && updFld.PopupDescId.ToString().EndsWith("01") && (updFld.Popup == PopupType.SPECIAL))
                    nTabId = updFld.PopupDescId - 1;
                else
                    continue;

                if (sbTabid.Length != 0)
                    sbTabid.Append(";");
                sbTabid.Append(nTabId);
            }

            if (sbTabid.Length == 0)
                return;

            // Ouverture des param
            eParam param = new eParam(_pref);

            // Liste de CatalogueValue
            foreach (EdnUpdFldContext fldContext in _updContext.LstUpdFld)
            {
                eUpdateField updFld = fldContext.Field;
                int nTabId = 0;

                //MRU si champ de liaison vers table seulement
                if (updFld.Descid.ToString().EndsWith("00"))
                    nTabId = updFld.Descid;
                else if (updFld.PopupDescId > 0 && updFld.PopupDescId.ToString().EndsWith("01"))
                    nTabId = updFld.PopupDescId - 1;
                else
                    continue;

                /*Récup des nouvelles valeurs*/
                int newValue = 0;
                int.TryParse(updFld.NewValue, out newValue);
                string newDisplayValue = updFld.NewDisplay;

                /*MAJ des MRUs*/
                param.SetTableMru(dal, nTabId, newValue, out error);
                if (error.Length > 0)
                {
                    eFeedbackXrm.LaunchFeedbackXrm(
                        eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, string.Concat("Mise à jours des nouvelles MRU TAB en échec. ", error)), _pref);
                    continue;
                }

                /*Refresh des MRU*/
                if (newValue > 0)
                {
                    if (param.ParamMruTable != null)
                        param.ParamMruTable.Remove(nTabId);

                    if (_mruResult == null)
                        _mruResult = new Dictionary<int, string>();

                    _mruResult[nTabId] = newValue + "$;$" + updFld.NewDisplay + "$;$99$|UPDATE|$";
                }

            }
        }

        private void AddValue(Engine.Engine eng, eUpdateField updFld)
        {

            // Valeur de id de table - liaison directe
            if (updFld.Descid % 100 == 0)
                eng.AddTabValue(updFld.Descid, eLibTools.GetNum(updFld.NewValue), updFld.ReadOnly);
            else
            {
                if (updFld.IsB64)
                {
                    string sVal64 = updFld.NewValue.Replace("data:image/png;base64,", "");
                    eng.AddBinaryValue(updFld.Descid, Convert.FromBase64String(sVal64));
                }
                else
                {
                    eng.AddNewValue(updFld.Descid, updFld.NewValue, updFld.ForceUpdate, updFld.ReadOnly);
                }


                eng.AddPreviousValue(updFld.Descid, updFld.PreviousValue);
            }
        }

        private static bool ResultAnError(EngineResult result)
        {
            return result == null || !result.Success;
        }

        private static string GetFormatDateTime(string date)
        {
            try
            {
                if (date.Length == 0)
                    return string.Empty;

                string[] aDate = date.Split('-');
                int nDay = int.Parse(aDate[0]);
                int nMonth = int.Parse(aDate[1]);
                int nYear = int.Parse(aDate[2]);
                int nHour = int.Parse(aDate[3]);
                int nMn = int.Parse(aDate[4]);

                return date = string.Concat(nDay, "/", nMonth, "/", nYear, " ", nHour, ":", nMn);
            }
            catch
            {
                return date = string.Empty;
            }
        }



        /// <summary>
        /// INNER CLASSES
        /// </summary>
        internal class EdnUpdFldContext
        {
            internal eUpdateField Field { get; private set; }

            internal EdnUpdFldContext(eUpdateField fld)
            {
                Field = fld;
            }

            public override string ToString()
            {
                return string.Concat(Field.ToString());
            }
        }

        internal class EdnUpdContext
        {

            #region propriétés

            internal UpdType EdnUpdTyp { get; set; }

            internal XrmCruAction CruAction { set; get; }

            internal int FileId { set; get; }
            internal int TabDescId { set; get; }
            internal int TabFromDescId { set; get; }
            internal int FileIdFrom { set; get; }

            internal string FieldEditorType { set; get; }
            internal bool InsertCatalogValue { set; get; }

            internal List<EdnUpdFldContext> LstUpdFld { get; private set; }
            internal eParameters Params { get; private set; }
            internal eParameters CloneParams { set; get; }

            internal bool AutoComplete;
            #endregion

            internal EdnUpdContext()
            {
                CruAction = XrmCruAction.UPDATE;
                EdnUpdTyp = UpdType.CLASSIC;
                LstUpdFld = new List<EdnUpdFldContext>();
                Params = new eParameters();
                CloneParams = new eParameters();
                AutoComplete = false;
            }

            internal enum UpdType
            {
                CLASSIC,
                CLONE_BKM,
                CLONE_PLANNING
            }
        }
    }
}