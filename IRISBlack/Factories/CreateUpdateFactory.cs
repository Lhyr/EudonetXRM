using Com.Eudonet.Common.CommonDTO;
using Com.Eudonet.Common.Cryptography;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Result;
using Com.Eudonet.Engine.Result.Xrm;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.mailchecker;
using Com.Eudonet.Internal.tools.filler;
using Com.Eudonet.Xrm.IRISBlack.Model;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Classe permettant de gérer la création et la mise à jour de fiches/champs
    /// Equivalent de eUpdateFieldManager
    /// </summary>
    public class CreateUpdateFactory
    {
        /// <summary>
        /// Objet Pref
        /// </summary>
        private ePref _pref;

        /// <summary>
        /// DAO
        /// </summary>
        private eudoDAL _dal = null;

        /// <summary>
        /// Contexte de MAJ
        /// </summary>
        private EdnUpdContext _updContext = null;

        /// <summary>
        /// Erreur retournée par les sous-processus
        /// </summary>
        private string _localError = "";

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref">Objet Pref pour l'accès au contexte et à la base de données</param>
        public CreateUpdateFactory(ePref pref)
        {
            _pref = pref;
        }

        /// <summary>
        /// Met à jour ou crée un ensemble de valeurs pour les champs d'une fiche
        /// </summary>
        /// <param name="id">Identifiant (FileID) de la fiche à mettre à jour (0 pour création)</param>
        /// <param name="ufm">Objet contenant les données à mettre à jour</param>
        /// <returns>Retour d'Engine concernant la mise à jour</returns>
        public EngineResult CreateUpdateFields(int id, UpdateFieldsModel ufm)
        {
            EngineResult result = null;

            try
            {
                _dal = eLibTools.GetEudoDAL(_pref);
                _dal.OpenDatabase();

                #region Récupération des paramètres

                DoLoadContext(_dal, id, ufm, out _localError);
                if (_localError.Length > 0)
                {
                    result = new EngineResult();
                    result.Success = false;
                    result.SetError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), detailMsg: "", devMsg: _localError));
                    return result;
                }
                #endregion

                #region Catalogue

                if (_updContext.FieldEditorType == "catalogEditor" || _updContext.FieldEditorType == "stepCatalogEditor")
                {

                    foreach (EdnUpdFldContext fldContext in _updContext.LstUpdFld)
                    {
                        eUpdateField updFld = fldContext.Field;

                        // En création de fiche ou modification pour les formules du milieu, nous avons la liste complète des rubriques de la fiches. On ignore donc les rubriques sans PopupDescId
                        // sph - 37647 : suppression de la condition sur fileid=0
                        if ((_updContext.CruAction == XrmCruAction.CHECK_ONLY_MIDDLE
                                && updFld.PopupDescId == 0)
                            || (string.IsNullOrEmpty(updFld.NewDisplay)
                                    || !_updContext.InsertCatalogValue
                                    || (updFld.Popup != PopupType.DATA
                                        && updFld.Popup != PopupType.ONLY)
                               )
                           )
                            continue;


                        #region Ajout dans le catalogue

                        eTools.AddNewValueInCatalog(_dal, _pref, updFld, _updContext.AutoComplete, false, out _localError);

                        if (_localError.Length > 0)
                            return null;

                        #endregion
                    }
                }

                #endregion

                // Lance l'engine
                if (_updContext.EdnUpdTyp == EdnUpdContext.UpdType.CLONE_PLANNING)
                {
                    // TOCHECK: Appel à un autre contrôleur dans ce cas ?
                    return DoCloneCal();
                }
                else
                {
                    result = DoUpdate(result);
                    if (result == null || !result.Success)
                    {
                        return result;
                    }

                    int iOldFileId = eLibTools.GetNum(_updContext.CloneParams.GetParam("fileid"));
                    if (iOldFileId > 0)
                    {
                        EngineResult resultCloneGeo = DoCloneGeo(result, iOldFileId);
                        EngineResult resultCloneBkm = null;

                        if (_updContext.EdnUpdTyp == EdnUpdContext.UpdType.CLONE_BKM)
                            resultCloneBkm = DoCloneBkm(result, iOldFileId);
                    }
                }


                if (eMailCheckerTools.CheckIsVerifyEnabled(_pref))
                {
                    foreach (EdnUpdFldContext fldContext in _updContext.LstUpdFld)
                    {
                        FieldLite fld = eLibTools.GetFieldInfo(_pref, fldContext.Field.Descid, FieldLite.Factory());
                        if (fld.Format == FieldFormat.TYP_EMAIL)
                        {
                            Task t = eMailCheckerTools.UpdateEmail(_pref, fldContext.Field.NewValue);
                            var lstRefreshNewValue = result.ListRefreshFields.Where(e => e.Field.Descid == fldContext.Field.Descid)?.FirstOrDefault()?.List;

                            if (lstRefreshNewValue != null)
                            {
                                foreach (var r in lstRefreshNewValue)
                                {
                                    if (r.ExtendedProperties is ExtendedMailStatus)
                                    {
                                        var ep = (ExtendedMailStatus)r.ExtendedProperties;
                                        if (ep.MailStatusEudo == (int)EmailValidationEudoStatus.UNCHECKED)
                                            ep.MailStatusEudo = (int)EmailValidationEudoStatus.VERIFICATION_IN_PROGRESS;

                                    }
                                    else
                                    {
                                        r.ExtendedProperties = new ExtendedMailStatus() { MailStatusEudo = (int)EmailValidationEudoStatus.VERIFICATION_IN_PROGRESS };
                                    }
                                }
                            }
                        }
                    }
                }

                // Recalcul les MRU
                DoMruFields(_dal, ufm);

                // Recalcul les MRU dse champs de liaisons
                DoMruTabs(_dal);

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_dal != null)
                    _dal.CloseDatabase();
            }
        }

        /// <summary>
        /// Chargement des params du context de la page
        /// </summary>
        /// <param name="dal"></param>
        /// <param name="id">Identifiant du fichier à mettre à jour</param>
        /// <param name="error"></param>
        private void DoLoadContext(eudoDAL dal, int id, UpdateFieldsModel ufm, out string error)
        {
            error = string.Empty;

            int reqIntVal;
            _updContext = new EdnUpdContext();

            try
            {
                // Paramètres obligatoires
                _updContext.FileId = id;

                // Recup des informations des fields
                List<EdnUpdFldContext> fieldsToRemove = new List<EdnUpdFldContext>(); // Tâche #2408 - Recensement d'éventuels champs sur lesquels la MAJ doit être interdite après vérification
                if (ufm.Fields.Count > 0)
                    _updContext.LstUpdFld.AddRange(ufm.Fields.Select(fldUpd => new EdnUpdFldContext(fldUpd)));

                if (ufm.TabDescId > 0)
                    _updContext.TabDescId = ufm.TabDescId;
                else if (_updContext.LstUpdFld.Count >= 1)
                    _updContext.TabDescId = _updContext.LstUpdFld.Select(updfld => eLibTools.GetTabFromDescId(updfld.Field.Descid)).First();
                else
                {
                    _localError = "Aucun paramètre fourni.";
                    return;
                }

                /// ATTENTION : côté JS, la variable se nomme "crtldescid" et non "ctrldescid")
                if (!string.IsNullOrEmpty(ufm.CtrlDescId))
                {
                    var encCtrlDescid = CryptoAESRijndael.Encrypt(ufm.CtrlDescId, CryptographyConst.KEY_CRYPT_LINK6, CryptographyConst.KEY_CRYPT_LINK1.Substring(0, 16));
                    _updContext.Params.AddParam("crtldescid", encCtrlDescid ?? string.Empty);
                }

                _updContext.TabFromDescId = ufm.ParentTabDescId;
                _updContext.FileIdFrom = ufm.ParentFileId;

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

                    fldTableDid = eLibTools.GetTabFromDescId(updFld.Descid);

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

                    // US #1495 - Autocomplétion/MAJ de champs - Tâche #2408 - MAJ de valeurs de catalogues dans le cas où le front ne passe pas l'ID de la nouvelle valeur, mais uniquement le code/libellé
                    // Code repris de eAutoCompletionManager.ashx.cs, avec une différence majeure : ici, on recherche à partir du libellé de la valeur (passé dans la propriété NewDisplay) ET à partir du
                    // DataID de la valeur s'il est précisé dans NewValue
                    //KHA 15062020 correctif au chatterton pour permettre la mise à jour des catalogue multiple depuis le nouveau mode fiche
                    //TODO Faire au propre
                    if (updFld.Popup == PopupType.DATA && !updFld.Multiple && ((updFld.NewValue != null && updFld.NewValue.Trim().Length != 0) || (updFld.NewDisplay != null && updFld.NewDisplay.Trim().Length != 0)))
                    {
                        /* Recherche de la valeur dans le catalogue selon 2 axes
                        Cas (1) - Tâche #2408 :
                            - On recherche d'abord la valeur par DataID s'il est passé en paramètre via NewValue. Evite l'insertion en base de DataIDs ne correspondant à aucune valeur dans FILEDATA
                        Cas (2) - Demande #62 782 : Puis on effectue ensuite une recherche de la valeur sur "Code" puis "Libellé". (2)
                            - Si la valeur renvoyée par le référentiel correspond à un code du catalogue sans ambiguité, on sélectionne cette valeur
                            - Si la valeur ne correspond pas à un code mais correspond à un des libellés des valeurs du catalogue, on sélectionne la première valeur détectée
                            - Si aucune correspondance, créer la valeur avec Code = Valeur si le catalogue est à code, et si la création de valeurs est autorisée par le contexte JavaScript
                         */
                        int dataId = -1;
                        bool searchByDataId = int.TryParse(updFld.NewValue, out dataId) && dataId > -1;
                        eCatalog catalog = new eCatalog(_dal, _pref, updFld.Popup, _pref.User, (updFld.PopupDescId > 0 && updFld.Descid != updFld.PopupDescId) ? updFld.PopupDescId : updFld.Descid);
                        eCatalog.CatalogValue cv = null;
                        if (catalog != null && catalog.Values != null)
                        {
                            if (searchByDataId)
                                cv = catalog.Values.Find(
                                    cVal => (
                                        cVal.Id == dataId // (1)
                                    )
                                );
                            else
                                cv = catalog.Values.Find(
                                    cVal => (
                                        cVal.Data == updFld.NewDisplay || // (2)
                                        cVal.DbValue == updFld.NewDisplay || // (2)
                                        cVal.DisplayValue == updFld.NewDisplay // (2)
                                    )
                                );
                        }
                        if (cv == null)
                            cv = catalog.Find(new eCatalog.CatalogValue(updFld.NewDisplay));

                        if (cv == null)
                        {
                            updFld.NewValue = updFld.NewDisplay; // la fonction AddNewValueInCatalog considèrera qu'elle devra aller chercher le libellé de la valeur à ajouter dans NewValue (paramètre updFldValueIsLabel à true)

                            // Si la valeur n'existe pas, on la crée ou pas selon l'option définie en admin 
                            //BSE:#64 182 : on ajoute une valeur au catalogue seulement si on a les droits
                            if (updFld.AddValueInCatalog && catalog.AddAllowed)
                            {
                                string addNewValueInCatalogError = String.Empty;
                                updFld.NewValue = eTools.AddNewValueInCatalog(_dal, _pref, updFld, true, true, out addNewValueInCatalogError);

                                if (!String.IsNullOrEmpty(addNewValueInCatalogError))
                                {
                                    throw new Exception($"Erreur d'ajout de nouvelle valeur {updFld.NewValue} ({updFld.NewDisplay}) dans le catalogue dont le DescId est {updFld.PopupDescId}");
                                }

                            }
                            else
                                fieldsToRemove.Add(fldContext); // Le champ sera retiré de la liste des champs à MAJ après sortie de la boucle

                        }
                        else
                        {
                            // Si la valeur existe
                            updFld.NewValue = cv.DbValue;
                            updFld.NewDisplay = cv.DisplayValue;
                        }

                    }
                }
                // #2408 - Si un champ doit être retiré suite aux vérifications ci-dessus
                foreach (EdnUpdFldContext fieldToRemove in fieldsToRemove)
                    _updContext.LstUpdFld.RemoveAll(f => f.Field == fieldToRemove.Field);

                // Paramèters complémentaires
                _updContext.CruAction = ufm.EngineAction;

                // Recherche prédictive
                _updContext.AutoComplete = ufm.AutoComplete;
                // en creation autoComplete n'est pas toujours transmis
                if (_updContext.FileId == 0)
                    _updContext.AutoComplete = eAutoCompletionTools.IsEnabled(_pref, AutoCompletionType.AUTO_COMPLETION_ADDRESS, _updContext.TabDescId);

                _updContext.FieldEditorType = ufm.FieldEditorType;
                _updContext.InsertCatalogValue = ufm.InsertCatalogValue;

                // Paramètres additionnels
                _updContext.Params.AddParam("fieldTrigger", ufm.FieldTrigger ?? "");
                _updContext.Params.AddParam("adrtoupd", ufm.AddressToUpdate ?? "");
                _updContext.Params.AddParam("refresh", ufm.Refresh ?? "");

                // Paramètres spécifiques template E-mail
                _updContext.Params.AddParam("mailDN", ufm.MailDisplayName ?? "");
                _updContext.Params.AddParam("mailRT", ufm.MailReplyTo ?? "");
                _updContext.Params.AddParam("mailCSS", ufm.MailCSS ?? "");
                _updContext.Params.AddParam("mailPJ", ufm.MailPJ ?? "");
                _updContext.Params.AddParam("mailSaveAsDraft", ufm.MailSaveAsDraft ? "1" : "0");
                _updContext.Params.AddParam("mailIsDraft", ufm.MailIsDraft ? "1" : "0");
                _updContext.Params.AddParam("mailIsTest", ufm.MailIsText ? "1" : "0");

                // Information de la source de l'enregistrement sur XRM
                _updContext.Params.AddParam("onValideFileAction", ufm.TriggerOnValidFileAction ? "1" : "0");
                _updContext.Params.AddParam("onBlurAction", ufm.TriggerOnBlurAction ? "1" : "0");

                // Information de la fenêtre de confirmation des formules du milieu de l'ORM
                _updContext.Params.AddParam("ormId", ufm.OrmId.ToString() ?? "0");
                _updContext.Params.AddParam("ormUpdates", ufm.OrmUpdates ?? "");
                _updContext.Params.AddParam("ormResponseObj", ufm.OrmResponseObj ?? "");

                #region Paramètres spécifiques pour la duplication de fiche

                if (ufm.CloneFileId > 0)
                {
                    _updContext.CloneParams.AddParam("fileid", ufm.CloneFileId.ToString());

                    if (ufm.BkmIds != null && ufm.BkmIds.Length > 0)
                    {
                        _updContext.CloneParams.AddParam("bkmids", ufm.BkmIds);
                        _updContext.EdnUpdTyp = EdnUpdContext.UpdType.CLONE_BKM;
                    }
                }


                #endregion

                #region Paramètres spécifiques pour la duplication de fiche planning

                string dtFormStr = ufm.NewDateBegin;
                if (dtFormStr?.Length > 0)
                {
                    _updContext.CloneParams.AddParam("newDateBegin", Convert.ToDateTime(dtFormStr).ToString());
                    _updContext.EdnUpdTyp = EdnUpdContext.UpdType.CLONE_PLANNING;
                }

                dtFormStr = ufm.NewDateEnd;
                if (dtFormStr?.Length > 0)
                    _updContext.CloneParams.AddParam("newDateEnd", Convert.ToDateTime(dtFormStr).ToString());

                _updContext.CloneParams.AddParam("oldFileid", ufm.CloneFileId.ToString());

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
        private EngineResult DoCloneCal()
        {
            Engine.Engine engClonePlanning = eModelTools.GetEngine(_pref, _updContext.TabDescId, eEngineCallContext.GetCallContext(EngineContext.APPLI));

            engClonePlanning.OldFileId = eLibTools.GetNum(_updContext.CloneParams.GetParam("oldFileid"));
            engClonePlanning.AddParam("newDateBegin", _updContext.CloneParams.GetParam("newDateBegin"));
            engClonePlanning.AddParam("newDateEnd", _updContext.CloneParams.GetParam("newDateEnd"));

            engClonePlanning.EngineProcess(new ClonePlanningRqParam());

            return engClonePlanning.Result;
        }

        /// <summary>
        /// Duplication unitaire : Duplication des champs Géo
        /// </summary>
        private EngineResult DoCloneGeo(EngineResult lastResult, int nOldFileId)
        {
            if (!ResultAnError(lastResult) && lastResult.NewRecord.Empty && nOldFileId > 0)
            {
                // Bizarre de reprendre toutes les rubriques GEO alors qu'en dehors de la rub système l'autres sont recopié pour le système de duplication de fiche classique
                IEnumerable<int> listFieldsGeo = RetrieveFields.GetDefault(_pref)
                    .AddOnlyThisTabs(new int[] { _updContext.TabDescId })
                    .AddOnlyThisFormats(new FieldFormat[] { FieldFormat.TYP_GEOGRAPHY_V2 })
                    .ResultFieldsDid();

                // Si pas de rubrique, on ne fait rien
                if (listFieldsGeo.Count() == 0)
                    return null;

                Engine.Engine engCloneGeo = eModelTools.GetEngine(_pref, _updContext.TabDescId, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                engCloneGeo.FileId = lastResult.NewRecord.FilesId[0];
                engCloneGeo.OldFileId = nOldFileId;
                engCloneGeo.AddParam("fieldsids", listFieldsGeo.Join(";"));
                engCloneGeo.EngineProcess(new CloneGeoRqParam());
                return engCloneGeo.Result; //CNA - Gestion d'erreur lors de la duplication des champs type Geography
            }

            return null;
        }

        /// <summary>
        /// Duplication unitaire : Duplication des signets
        /// </summary>
        private EngineResult DoCloneBkm(EngineResult lastResult, int nOldFileId)
        {
            EngineResult result = null;

            string oBkmsIds = _updContext.CloneParams.GetParam("bkmids");

            if (!ResultAnError(lastResult) && !lastResult.NewRecord.Empty && nOldFileId > 0)
            {
                Engine.Engine engCloneBkms = eModelTools.GetEngine(_pref, _updContext.TabDescId, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                engCloneBkms.FileId = lastResult.NewRecord.FilesId[0];
                engCloneBkms.OldFileId = nOldFileId;
                engCloneBkms.AddParam("bkmsids", oBkmsIds);
                engCloneBkms.EngineProcess(new CloneBookmarksRqParam());
                result = engCloneBkms.Result;    //GCH - SPRINT 2014.16 - 33267 - Gestion d'erreur lors de la duplication des signets depuis une duplication de fiche
            }

            return result;
        }

        private EngineResult DoUpdate(EngineResult lastResult)
        {


            List<eUpdateField> updMainFld = null;           // Enregistrement sur la table main
            List<eUpdateField> updAdrFld = null;            // Enregistrement sur la table ADR à la création d'une fiche PP

            #region Sépare les nouvelles valeurs en fonction des différentes listes créées ci-dessus

            updMainFld = new List<eUpdateField>();


            // Création d'un PP avec ADDRESS
            if (_updContext.FileId == 0 && _updContext.TabDescId == TableType.PP.GetHashCode())
            {
                /** ATTENTION, je n'ai pas pu tester. Le cas est spécifique et j'ai l'impression que ce n'est pas encore utilisé. G.L */
                var lstUpd = _updContext.LstUpdFld.GroupBy(fldContext => new {
                    isPP = (TableType.PP.GetHashCode() == eLibTools.GetTabFromDescId(fldContext.Field.Descid)),
                    fldContext.Field
                }).Select(fldContext => new { fldContext.Key.isPP, fldContext.Key.Field });

                var tmpUpdAdrFld = lstUpd.Where(up => !up.isPP);
                var tmpUpdMainFld = lstUpd.Where(up => up.isPP);

                if (tmpUpdAdrFld?.Count() > 0)
                    updAdrFld = tmpUpdAdrFld.Select(fld => fld?.Field).ToList();

                if (tmpUpdMainFld?.Count() > 0)
                    updMainFld = tmpUpdMainFld.Select(fld => fld?.Field).ToList();
            }
            else
            {
                if (_updContext.LstUpdFld.Count > 0)
                    updMainFld = _updContext.LstUpdFld.Select(fldContext => fldContext.Field).ToList();
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
                    AddValue(eng, updFld);
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


            lastResult = eng.Result;
            if (_updContext.Params.GetParam("refresh") == "1")
            {
                lastResult.ReloadHeader = true;
                lastResult.ReloadDetail = true;
            }
            if (lastResult.Confirm.Mode != EngineConfirmMode.NONE)
                return lastResult; // TOCHECK: false dans eUpdateFieldManager

            if (updAdrFld != null && updAdrFld.Count > 0 && !ResultAnError(lastResult) && !lastResult.NewRecord.Empty && _updContext.EdnUpdTyp == EdnUpdContext.UpdType.CLASSIC)
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
                engAdr.AddTabValue(TableType.PP.GetHashCode(), lastResult.NewRecord.FilesId[0]);
                // Nouvelles valeurs d'adr
                foreach (eUpdateField updFld in updAdrFld)
                    if (updFld != null && (updFld.ChangedValue == null || updFld.ChangedValue.Value))
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
                    lastResult = null;
                else if (!engAdr.Result.Success)
                {
                    lastResult.Success = false;
                    lastResult.SetError(engAdr.Result.Error);
                }
                else
                {
                    lastResult.AddExternalResult(engAdr.Result);
                }

                // SHA : #73 334
                if (engAdr.Result == null)
                    lastResult = null;

                #endregion
            }



            return lastResult;
        }

        /// <summary>Init des MRU de type catalogues</summary>
        /// <param name="dal"></param>
        private Dictionary<int, string> DoMruFields(eudoDAL dal, UpdateFieldsModel ufm)
        {
            Dictionary<int, string> result = null;
            string error = string.Empty;
            StringBuilder sbDescid = new StringBuilder();

            bool bFromUserCat = ufm.FieldEditorType?.ToLower() == "catalogUserEditor".ToLower();

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
                return null;

            // Ouverture des param
            eParam param = new eParam(_pref);
            param.LoadFieldMru(dal, sbDescid.ToString(), out error);
            if (error.Length > 0)
            {
                eFeedbackXrm.LaunchFeedbackXrm(
                    eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, string.Concat("Chargements des nouvelles MRU FIELD en échec. ", error)), _pref);
                return null;
            }

            if (param.ParamMruField.Count == 0)
                return null;

            // Liste de CatalogueValue
            eParam.ParamMruFieldItem mruItem = null;
            foreach (EdnUpdFldContext fldContext in _updContext.LstUpdFld)
            {
                eUpdateField updFld = fldContext.Field;

                //Pas de load MRU si champ de liaison vers table
                if (updFld.Descid.ToString().EndsWith("00"))
                    continue;

                if (!param.ParamMruField.TryGetValue(updFld.Descid, out mruItem))
                    continue;   //Un champ peut ne pas avoir de MRU, exemple : le champ note

                int nItemId = 0;
                string itemId = string.Empty, itemDisplay = string.Empty;
                string[] tmp = null;
                string[] mruItemValues = mruItem.Values.Split("$|$");

                // Recup le catalogue avec les valeurs du MRU demandées
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

                    // MRU limitée à 7 valeurs
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
                        eCatalog wholeCatalog = new eCatalog(dal, _pref, updFld.Popup, _pref.User, updFld.PopupDescId, false, newCatValue);
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
                        if (result == null)
                            result = new Dictionary<int, string>();

                        result.Add(updFld.Descid, mruParamIFrameNewValue.ToString());
                    }
                }
                finally
                {
                    if (bDoCloseDal)
                        dal.CloseDatabase();
                }
            }

            return result;
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

                    /*
                    if (_mruResult == null)
                        _mruResult = new Dictionary<int, string>();

                    _mruResult[nTabId] = newValue + "$;$" + updFld.NewDisplay + "$;$99$|UPDATE|$";
                    */
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

        /// <summary>
        /// Renvoie true si l'objet Engine passé en paramètre est en Succès, false sinon
        /// </summary>
        /// <param name="result">Résultat d'Engine</param>
        /// <returns>true si l'objet Engine passé en paramètre est en Succès, false sinon</returns>
        private static bool ResultAnError(EngineResult result)
        {
            return result == null || !result.Success;
        }

    }
}