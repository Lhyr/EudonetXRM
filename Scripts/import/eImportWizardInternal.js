/// <summary>
/// Objet interne de l'assistant d'import, gère et controle les diffréntes étapes
/// </summary>
var oImportWizardInternal = (function () {

    /// <summary>
    /// Sauvegarde l'etape actuelle
    /// </summary>
    var _currentStep = 1;

    /// <summary>
    /// Paramètre provisoire pour sauvegarder les informations de la rubrique sélectionnée
    /// </summary>
    var _currentFieldSelector = null;


    var _obligFields = true;

    var _wizardInternal = null;


    /// <summary>
    /// Récupérer le nom du fichier posté 
    /// </summary>
    var _fileName = '';

    var _fileSize = 0;

    var _browser = new getBrowser();

    /// <summary>
    /// Paramètres de l'assistant d'import par défaut
    /// ImportTab = 0 est autorisé pour signaler une erreur
    /// </summary>
    var _wizardParam = {
        'ImportTab': 0,
        'ImportId': 0,
        'ImportTemplateParams': {
            'ImportTemplateId': 0
        },
        'Action': 0,
        'ParentTab': 0,
        'ParentFileId': 0
    };


    /// <summary>
    /// Paramètres de l'import 
    /// </summary>
    var _importGlobalTabParam = {
        ImportTab: _wizardParam.ImportTab,
        ParentTab: _wizardParam.ParentTab,
        ParentFieldId: _wizardParam.ParentFileId,
        Tables: [],
        ParsingOption: {
            WithHead: true,
            TextQualifier: '',
            TextSeparator: '',
            FirstDataRow: true
        },
        GetTableMainParams: function () {
            var tabMainParams = null;
            var tabMain = this.ImportTab;
            if (this.Tables != null && typeof this.Tables != 'undefined' && this.Tables.length > 0)
                this.Tables.forEach(function (_table, index) {
                    if (_table.TabInfo.TabInfoId == tabMain) {
                        //oRes.Existe = true;
                        //oRes.Index = index;
                        tabMainParams = _table;
                    }
                });
            return tabMainParams;
        },
        /// <summary>
        /// Reset tables params
        /// </summary>
        ClearTablesParams: function () {
            this.Tables.length = 0;
        },
        /// <summary>
        /// Affecter une clé de dédoublonnage pour une table passé en paramètre
        /// </summary>
        SetTableKey: function (tabId, Key, removeIndex) {
            var existTable = false;
            var tabIndex = -1;
            this.Tables.forEach(function (_table, index) {
                if (_table.TabInfo.TabInfoId == tabId) {
                    existTable = true;
                    tabIndex = index;
                }
            });

            if (existTable && tabIndex != -1) {

                var _table = this.Tables[tabIndex];

                if (_table.Keys.length > 0) {
                    var KeyExist = false;
                    var keyIndex = -1;
                    _table.Keys.forEach(function (item, i) {

                        if (item === Key) {
                            KeyExist = true;
                            keyIndex = i;
                        }

                    });

                    if (KeyExist && keyIndex != -1) {
                        if (removeIndex)
                            this.RemoveKey(tabIndex, keyIndex)
                        else
                            _table.Keys[keyIndex] = Key;

                    } else
                        _table.Keys.push(Key);
                }
                else
                    _table.Keys.push(Key);

                if (_table.Keys.length == 0)
                    _table.IsKey = false;
            }
            else
                top.eAlert(0, top._res_8365, top._res_416, top._res_8478, null, null);

        },

        /// <summary>
        /// Définir une table relationnelle comme clé de dédoublonnage pour de la table principale
        /// </summary>
        SetTableAsKey: function (tabId, isChecked) {
            var existTable = false;
            var tabIndex = -1;
            this.Tables.forEach(function (_table, index) {
                if (_table.TabInfo.TabInfoId == tabId) {
                    existTable = true;
                    tabIndex = index;
                }
            });

            if (existTable && tabIndex != -1) {
                var _table = this.Tables[tabIndex];
                _table.IsKey = (isChecked == 1);
            }
            //else
            //    top.eAlert(0, top._res_8365, top._res_416, top._res_8478, null, null);
        },
        /// <summary>
        /// Supprimer une clé de dédoublonnage
        /// </summary>
        RemoveKey: function (tabIndex, keyIndex) {
            if (tabIndex != -1 && keyIndex != -1)
                this.Tables[tabIndex].Keys.splice(keyIndex, 1);
        },

        /// <summary>
        /// Supprimer une table/paramètres du tableau des params
        /// </summary>
        DeleteTablesParams: function (IdTable) {

            if (this.Tables.length == 0)
                return;
            else {
                var controlTab = this.CheckExistTabParams(IdTable);
                if (controlTab.Existe && controlTab.Index != -1) {
                    this.Tables.splice(controlTab.Index, 1);
                }
            }
        },

        /// <summary>
        /// Ajouter une table/paramètres au tableau des params
        /// </summary>
        AddTablesParams: function (params) {

            if (this.Tables.length == 0)
                this.Tables.push(params);
            else {
                var controlTab = this.CheckExistTabParams(params.TabInfo.TabInfoId);
                if (controlTab.Existe && controlTab.Index != -1) {
                    if (this.Tables[controlTab.Index].Mapp.length > 0) {
                        var controlMapping = this.CheckExistMapping(controlTab.Index, params.Mapp[0].Field);
                        if (controlMapping.Existe && controlMapping.Index != -1) {
                            this.Tables[controlTab.Index].Mapp[controlMapping.Index].Col = params.Mapp[0].Col;
                            this.Tables[controlTab.Index].Mapp[controlMapping.Index].ColName = params.Mapp[0].ColName;
                        }
                        else
                            this.Tables[controlTab.Index].Mapp.push(params.Mapp[0]);
                    }
                    else
                        this.Tables[controlTab.Index].Mapp.push(params.Mapp[0]);

                }
                else
                    _importGlobalTabParam.Tables.push(params);
            }
        },
        /// <summary>
        /// Vérification de l'existance d'une table dans la collection
        /// </summary>
        CheckExistTabParams: function (tableId) {
            var oRes = {};
            oRes.Existe = false;
            oRes.Index = -1;

            this.Tables.forEach(function (_table, index) {
                if (_table.TabInfo.TabInfoId == tableId) {
                    oRes.Existe = true;
                    oRes.Index = index;
                }
            });
            return oRes;
        },
        /// <summary>
        /// Vérification de l'existance d'un mapping
        /// </summary>
        CheckExistMapping: function (tabIndex, fieldDescid) {
            var oRes = {};
            oRes.Existe = false;
            oRes.Index = -1;

            this.Tables[tabIndex].Mapp.forEach(function (item, i) {

                if (item.Field == fieldDescid) {
                    oRes.Existe = true;
                    oRes.Index = i;
                }

            });

            return oRes;
        },
        /// <summary>
        /// Vérification de l'existance d'une clé de dédoublonnage
        /// </summary>
        CheckExistKey: function (tabIndex, fieldDescid) {
            var oRes = {};
            oRes.Existe = false;
            oRes.Index = -1;

            this.Tables[tabIndex].Keys.forEach(function (item, i) {

                if (item === fieldDescid) {
                    oRes.Existe = true;
                    oRes.Index = i;
                }

            });

            return oRes;
        },
        /// <summary>
        /// Permet d'ajouter une option à un field
        /// </summary>
        SetOptionField: function (tableId, fieldDescid, option) {
            var tabExiste = this.CheckExistTabParams(tableId);
            if (tabExiste.Existe) {
                var mapping = this.CheckExistMapping(tabExiste.Index, fieldDescid);
                if (mapping.Existe) {
                    var mapp = this.Tables[tabExiste.Index].Mapp[mapping.Index];
                    if (option.Value === false) {
                        var mapIndex = 0;

                        mapp.Options.forEach(function (m, index) {
                            if (m.Name === option.Name) {
                                mapIndex = index;
                            }
                        });

                        this.Tables[tabExiste.Index].Mapp[mapping.Index].Options.splice(mapIndex, 1);
                        if (this.Tables[tabExiste.Index].Mapp[mapping.Index].Options.length === 0)
                            this.Tables[tabExiste.Index].Mapp[mapping.Index].Options = null;

                    } else {
                        if (!this.Tables[tabExiste.Index].Mapp[mapping.Index].Options)
                            this.Tables[tabExiste.Index].Mapp[mapping.Index].Options = [];
                        this.Tables[tabExiste.Index].Mapp[mapping.Index].Options.push(option);
                    }
                }
            }
        },
        /// <summary>
        /// Supprime le mapping d'une rubrique pour une table
        /// </summary>
        RemoveTabMapping: function (tabId, fieldDescid) {
            //On vérifie que la table existe 
            var oResTab = this.CheckExistTabParams(tabId);
            var emptyTabMapping = true;
            //on vérifie que la table à un mapping
            if (oResTab.Existe) {
                emptyTabMapping = false;
                var oResMapping = this.CheckExistMapping(oResTab.Index, fieldDescid);

                //On supprime le mapping s'il eiste
                if (oResMapping.Existe)
                    this.Tables[oResTab.Index].Mapp.splice(oResMapping.Index, 1);

                //on supprime la table si elle n'a aucune rubrique mappée 
                if (this.Tables[oResTab.Index].Mapp.length == 0) {
                    this.DeleteTablesParams(tabId);
                    emptyTabMapping = true;
                }
                else {
                    //sinon on supprime la clé si elle existe
                    var oResKey = this.CheckExistKey(oResTab.Index, fieldDescid);
                    if (oResKey.Existe)
                        this.RemoveKey(oResTab.Index, oResKey.Index);
                }

            }

            return emptyTabMapping;

        },
        /// <summary>
        /// Retourne le modèle d'import utilisé s'il existe
        /// </summary>
        GetImportTemplate: function () {
            return _wizardParam.ImportTemplateParams;
        },

        GetImportTemplateTab: function (tabId) {
            var importTemplateTapParams = null;
            if (_wizardParam.ImportTemplateParams != null && _wizardParam.ImportTemplateParams.ImportParams != null)
                [].map.call(_wizardParam.ImportTemplateParams.ImportParams.Tables, function (tab) {
                    if (tab.TabInfo.TabInfoId == tabId)
                        importTemplateTapParams = tab;
                });

            return importTemplateTapParams;
        },
        /// <summary>
        /// Les options possibles sur les rubriques
        /// </summary>
        IMPORTFIELDOPTIONSTYPE:
        {
            /// <summary>
            /// L'option définie est inconnue
            /// </summary>
            UNDEFINED: 0,
            /// <summary>
            /// Cumuler avec les valeurs du catalogue existantes
            /// </summary>
            MERGEDATA: 1,
            /// <summary>
            /// Conserver la valeur existante si vide
            /// </summary>
            KEEPDATA: 2
        },
        DisableAutomatismsORM: false

    };


    /// <summary>
    /// Représente un proxy du serveur
    /// </summary>
    var _server = {

        /// <summary>
        /// Envoi de la requete au serveur
        /// option :
        /// step : numero de l'etape
        /// Action : Action a executer coté serveur
        /// addAdditionalParam : function pour ajouter de nouveaux paramètres
        /// onSuccess : function après le succès d'excution de la requête
        /// </summary>
        send: function (option) {
            var oUpdater = new eUpdater("mgr/eImportProcessManager.ashx", 1);
            oUpdater.asyncFlag = true;

            // identification de la fiche parente
            oUpdater.addParam("ParentTab", _wizardParam.ParentTab, "post");
            oUpdater.addParam("ParentFileId", _wizardParam.ParentFileId, "post");

            // Table cible de l'import
            oUpdater.addParam("ImportTab", _wizardParam.ImportTab, "post");

            // Identifiant de l'import
            oUpdater.addParam("ImportId", _wizardParam.ImportId, "post");

            // Identifiant du modèle d'import
            oUpdater.addParam("ImportTemplateParams", JSON.stringify(_wizardParam.ImportTemplateParams), "post");

            // Action a excuter coté serveur
            oUpdater.addParam("Action", option.Action, "post");

            // l'etape concernée de l'Action
            oUpdater.addParam("Step", option.step, "post");

            if (typeof (_wizardParam.ImportGlobalTabParam) != 'undefined')
                oUpdater.addParam("importGlobalTabParam", JSON.stringify(_wizardParam.ImportGlobalTabParam), "post");


            // taille du rendu            
            var winSize = getWindowSize();
            oUpdater.addParam("height", winSize.h, "post");
            oUpdater.addParam("width", winSize.w, "post");

            // ajout de paramètres personalisés
            if (typeof (option.addAdditionalParam) == 'function')
                option.addAdditionalParam(oUpdater);

            // En cas d'erreur
            oUpdater.ErrorCallBack = option.onError;

            oUpdater.send(option.onSuccess);
        },

        /// <summary>
        /// Transfert du fichier vers le serveur
        /// option :
        /// file : fichier à transferer
        /// transferComplete : Action a executer en cas de succès
        /// transferFailed : Action a excuter en cas d'echec
        /// </summary>
        upload: function (option) {

            // Préparation de l'objet formulaire
            var formData = new this.ieFormData(); //new FormData(); 
            formData.append("Action", option.Action);
            formData.append("file", option.file);
            if (typeof option.Params != 'undefined' && option.Params != null)
                formData.append("ImportGlobalTabParam", option.Params);
            // fenetre d'attente de transfert : 
            var oWaitDialog = showWaitDialog(top._res_8197, top._res_8466);

            var oRequest = this.createXMLHTTPObject();
            oRequest.open("POST", "mgr/eImportProcessManager.ashx", true);
            //Retour serveur
            setEventListener(oRequest, "load", function transferComplete(evt) {
                src = evt.srcElement || evt.target;

                oWaitDialog.hide();

                // en cas de succès
                if (src.status == 200)

                    option.transferComplete(src.responseText);
                else
                    option.transferFailed("Server response " + src.status + " : " + src.statusText);


            });

            //En cas d'echec
            setEventListener(oRequest, "error", function transferFailed(evt) {
                oWaitDialog.hide();
                option.transferFailed("Erreur");


            });

            try { oRequest.send(formData); } catch (ex) { console.log(ex); }
        },

        delete: function (option) {

            var oRequest = this.createXMLHTTPObject();
            oRequest.open("POST", "mgr/eImportProcessManager.ashx", false);
            var data = new this.ieFormData();
            data.append('Action', option.Action);

            // fenetre d'attente de transfert : 
            var oWaitDialog = showWaitDialog(top._res_8197, top._res_8467);

            //Retour serveur
            setEventListener(oRequest, "load", function transferComplete(evt) {
                src = evt.srcElement || evt.target;

                // en cas de succès
                if (src.status == 200)
                    option.transferComplete(src.responseText);
                else
                    option.transferFailed("Server response " + src.status + " : " + src.statusText);
                //console.log(src.status);
                oWaitDialog.hide();
            });

            //En cas d'echec
            setEventListener(oRequest, "error", function transferFailed(evt) {
                src = evt.srcElement || evt.target;
                option.transferFailed(src.statusText);
                oWaitDialog.hide();
            });

            try { oRequest.send(data); } catch (ex) { }
        }
        ,
        ieFormData: function ieFormData() {
            if (window.FormData == undefined) {
                this.processData = true;
                this.contentType = 'application/json';
                this.append = function (name, value) {
                    this[name] = value == undefined ? "" : value;
                    return true;
                }
            }
            else {
                return new FormData();
            }
        },
        createXMLHTTPObject: function () {
            var xmlhttp = false;

            var XMLHttpFactories = [
                function () { return new XMLHttpRequest() },
                function () { return new ActiveXObject("Msxml2.XMLHTTP") },
                function () { return new ActiveXObject("Msxml3.XMLHTTP") },
                function () { return new ActiveXObject("Microsoft.XMLHTTP") }
            ];
            var len = XMLHttpFactories.length;

            for (var i = 0; i < len; i++) {
                try { xmlhttp = XMLHttpFactories[i](); }
                catch (e) { continue; }
                break;
            }
            return xmlhttp;
        }

    };



    return {
        ///permId, Mode, Level, User
        _aViewPerm: {
            "id": "0",
            "mode": "-1",
            "level": "0",
            "user": ""
        },
        _aUpdatePerm: {
            "id": "0",
            "mode": "-1",
            "level": "0",
            "user": ""
        },
        MODE: {
            VIEW: 0,
            UPDATE: 1
        },

        ImportTemplateInternal: null,

        _HideMappedFields: false,
        _HideFreeFields: false,

        STEP: {
            DATASOURCE: 1,
            MAPPING: 2,
            OPTIONS: 3,
            END: 4,
            PROGESS: 5
        },

        /// <summary>
        /// Initialisation de l'assistant
        /// </summary>
        Init: function () { /*console.log("Init CurrentStep " + _currentStep); */ },


        /// <summary>
        /// Initialise les paramètres du wizrad
        /// </summary>
        SetWizardParam: function (wizardParam) {
            _wizardParam = wizardParam;
            _importGlobalTabParam.ParentFieldId = wizardParam.ParentFileId;
            _importGlobalTabParam.ImportTab = wizardParam.ImportTab;
            _importGlobalTabParam.ParentTab = wizardParam.ParentTab;
        },

        /// <summary>
        /// Définit le type de l'assistant
        /// </summary>
        GetType: function () { return "import"; },
        /// <summary>
        /// Affecte les paramètres du template d'import 
        /// </summary>
        SetImportTemplateParams: function (importTemplateParams) {
            _wizardParam.ImportTemplateParams = importTemplateParams;
        },
        /// <summary>
        /// Affecte l'id du template d'import 
        /// </summary>
        SetImportTemplateId: function (importTemplateId) {
            _wizardParam.ImportTemplateParams.ImportTemplateId = importTemplateId;
        },
        /// <summary>
        /// Retourne la table main d'import
        /// </summary>
        GetImportTab: function () {
            return _importGlobalTabParam.ImportTab;
        },
        /// <summary>
        /// Retourne l'id du modèle d'import
        /// </summary>
        GetImportTemplateId: function () {
            return _wizardParam.ImportTemplateParams.ImportTemplateId;
        },
        /// <summary>
        /// Retourne les paramètres du modèle d'import
        /// </summary>
        GetImportTemplate: function () {
            return _wizardParam.ImportTemplateParams;
        },
        /// <summary>
        /// Retourne le mapping d'import
        /// </summary>
        GetImportParams: function () {
            var currentStep = oImportWizardInternal.GetCurrentStep();
            if (currentStep == oImportWizardInternal.STEP.OPTIONS && document.getElementById('trigger-automations') != null)
                _importGlobalTabParam.DisableAutomatismsORM = document.getElementById('trigger-automations').checked;
            return _importGlobalTabParam;
        },
        /// <summary>
        /// Passe à l'etape suivante
        /// </summary>
        MoveStep: function (bNext, saveTemplateImport) {
            var currentStep = oImportWizardInternal.GetCurrentStep();
            //si on est au mins à l'étape du mapping, on enregistre le template d'import
            if (!isNaN(parseInt(currentStep)) && currentStep >= oImportWizardInternal.STEP.MAPPING) {
                var importSavedTemplate = _wizardParam.ImportTemplateParams;
                var importCurrentParams = oImportWizardInternal.GetImportParams();
                if (currentStep == oImportWizardInternal.STEP.OPTIONS && document.getElementById('trigger-automations') != null)
                    importCurrentParams.DisableAutomatismsORM = document.getElementById('trigger-automations').checked;
                if ((importSavedTemplate && importSavedTemplate.ImportParams.Tables.length > 0 && (importSavedTemplate.IsNotSavedImportTemplate || importSavedTemplate.ImportTemplateId == 0)) || importCurrentParams.Tables.length > 0)
                    this.SaveImportTemplate(false, true, saveTemplateImport);
            }

            //depuis eWizard.js
            MoveStep(bNext, 'import');
        },

        GetCurrentStep: function () {
            return _currentStep;
        }
        ,
        ReloadCurrentStep: function (step, imporTemplatetId) {
            _currentStep = (step >= 1 ? step - 1 : 1);
            this.SetImportTemplateId(imporTemplatetId);
            this.SwitchStep(step);
        },
        /// <summary>
        /// Controle l'étape actuelle si elle est valide 
        /// </summary>
        ControlStep: function (currentStep) {

            if (currentStep == this.STEP.DATASOURCE) {
                // pour revenir à la première étape
                if (_currentStep == this.STEP.MAPPING) {
                    _wizardInternal = this;
                    top.eConfirm(eMsgBoxCriticity.MSG_INFOS, top._res_6343, top._res_8399, '', 600, 200, _wizardInternal.ResetMapping, _wizardInternal.Cancell, false, true);


                    // passer à l'étape du mapping
                } else {
                    var oRes = this.CheckDataSource();
                    if (!oRes.success)
                        top.eAlert(0, top._res_8474, top._res_8474, top._res_8475, null, null);
                    else {
                        this.SetParsingOption(false);
                        _wizardParam.ImportGlobalTabParam = _importGlobalTabParam;
                        _wizardInternal = this;
                        setTimeout(_wizardInternal.SaveFile(oRes, function (t) {
                            _wizardInternal.SetParsingOption(true);
                            if (t.Success)
                                SwitchStep(_wizardInternal.STEP.MAPPING);

                        }), 100);
                    }
                }
            }

            if (currentStep == this.STEP.MAPPING) {
                if (_currentStep == this.STEP.OPTIONS) {
                    this.ShowFieldsCurrentTable(_importGlobalTabParam.ImportTab);
                    SwitchStep(_wizardInternal.STEP.MAPPING);
                    this.SwitchActivePage(1);
                } else {
                    _wizardParam.ImportGlobalTabParam = _importGlobalTabParam;

                    var oRes = this.ValidImportMappingParams();
                    if (!oRes.Success) {
                        oRes.FctErr();
                        return oRes.Success;
                    }

                }
                return true;
            }

            if (currentStep == this.STEP.OPTIONS) {
                var tableMainParam = _importGlobalTabParam.GetTableMainParams();
                if (tableMainParam && !tableMainParam.Create && !tableMainParam.Update) {
                    eAlert(0, top._res_329, top._res_1941);
                    return false;
                } else if (_importGlobalTabParam.nAllawedTable == 0) {
                    eAlert(0, top._res_329, top._res_1834);
                    return false;
                }
                return true;
            }


            if (currentStep == this.STEP.END) {
                return true;
            }
        },

        /// <summary>
        /// Passe à l'etape dont le numero en paramètre
        /// </summary>
        SwitchStep: function (nextStep) {
            if (_currentStep < nextStep) {
                // fenetre d'attente de transfert : 
                var oWaitDialog = showWaitDialog(top._res_8197, top._res_644);
                //showDebugWindow();

                // TODO verifier si l'etape n'est pas déjà chargée
                _server.send({
                    'step': nextStep,
                    'Action': 1,
                    'onSuccess': function (result) {
                        var res = JSON.parse(result);
                        if (!res.Success) {
                            eAlert(2, res.ErrorTitle, res.ErrorMsg, res.ErrorDetail);
                        } else {

                            if (res.Step == _wizardInternal.STEP.PROGESS) {
                                _wizardInternal.DrawProgression(res.Html);
                            }
                            else {
                                if (typeof res.Params == 'object' && typeof res.Params.ImportParams.Tables == 'object') {
                                    if (res.Step != _wizardInternal.STEP.MAPPING) {
                                        _wizardInternal.AfterAffectImportTemplate(res);
                                        //Affectation du renderer
                                        document.getElementById('editor_' + res.Step).innerHTML = res.Html;
                                    }
                                    else {
                                        if (res.Params.ImportTemplateOriginNotFoundList != null && res.Params.ImportTemplateOriginNotFoundList.length > 0) {
                                            var strCol = _wizardInternal.GetFormatedInformationString(res.Params.ImportTemplateOriginNotFoundList, 9, false);
                                            if (strCol != '')
                                                strCol += '[[BR]]<span class="text-alert-info">' + top._res_8732 + '</span>';
                                            top.eConfirm(eMsgBoxCriticity.MSG_INFOS, top._res_6343, top._res_8731, strCol, 600, 200, function () {
                                                _wizardInternal.AfterAffectImportTemplate(res);
                                            }, _wizardInternal.Cancell, false, true);
                                        }

                                        else {
                                            _wizardInternal.AfterAffectImportTemplate(res);
                                            document.getElementById('editor_' + res.Step).innerHTML = res.Html;
                                        }

                                    }
                                } else
                                    //Affectation du renderer
                                    document.getElementById('editor_' + res.Step).innerHTML = res.Html;
                            }
                        }

                        if (oWaitDialog != null)
                            oWaitDialog.hide();

                        //Initialiser les listner
                        _wizardInternal.InitListener();

                    },
                    'onError': function (result) {
                        var res = JSON.parse(result);
                        eAlert(1, top._res_72, top._res_1806, res.ErrorDetail);

                        if (oWaitDialog != null)
                            oWaitDialog.hide();
                    }
                });
            }

            // Sauvegarde de l'étape 
            _currentStep = nextStep;

        },

        /// <summary>
        /// Transfert le fichier vers le serveur
        /// </summary>
        SaveFile: function (oRes, callback) {

            var src = oRes.file;
            var res = {};
            // Pas de fichier
            if (src == '')
                return;
            _fileName = src;
            _server.upload({
                'file': (oRes.dataSource == 1) ? src : encode(src),
                'Action': 3,
                'Params': JSON.stringify(_wizardParam.ImportGlobalTabParam),
                'transferComplete': function (result) {
                    res = JSON.parse(result);
                    if (!res.Success)
                        eAlert(2, res.ErrorTitle, res.ErrorMsg, res.ErrorDetail);

                    console.log('transferComplete');

                    callback(res);
                },
                'transferFailed': function (errMsg) {
                    eAlert(2, top._res_416, errMsg, '');
                    res.Success = false;
                    callback(res);
                    //console.log(errMsg);                  
                }
            });


        },


        /// <summary>
        /// On supprime le fichier si on revient à la première étape ou si on ferme le wizard
        /// </summary>
        DeleteFile: function (callBack) {
            if (_currentStep > 1) {
                _server.delete({
                    'Action': 6,
                    'transferComplete': function (result) {
                        var res = JSON.parse(result);
                        if (!res.Success)
                            eAlert(2, res.ErrorTitle, res.ErrorMsg, res.ErrorDetail);
                        if (typeof callBack == 'function')
                            callBack();
                    },
                    'transferFailed': function (errMsg) {
                        console.log(errMsg);
                    }
                });
            }

            if (typeof callBack == 'function')
                callBack();
        },
        /// <summary>
        /// Sélectionner un modèlele d'import
        /// </summary>
        selectModel: function (elem) {
            if (addClass != null)
                addClass(elem, "eSel");
        },
        /// <summary>
        /// appliquer un modèle d'import
        /// </summary>
        applayModel: function (elem) {
            alert('applayModel');
        },
        renameImportTpl: function (id, elem) {
            alert('renameImportTpl')
        },
        /// <summary>
        /// On Vérifie la validitée des données
        /// </summary>
        CheckDataSource: function () {
            var input = document.getElementById('btnFileUpload');
            var txtPasteArea = document.getElementById('txtPasteArea');

            var oRes = {};
            oRes.success = false;
            oRes.dataSource = 1;
            oRes.file = '';
            oRes.element = '';
            if (input.files != null && input.files.length != 0) {
                oRes.file = input.files[0];
                oRes.success = true;
                oRes.element = input;
            }


            if (txtPasteArea.value.trim() != '' && (input.files == null || input.files.length == 0) && !oRes.success) {
                oRes.file = txtPasteArea.value.trim();
                oRes.success = true;
                oRes.dataSource = 2;
                oRes.element = txtPasteArea;
            }

            return oRes;


        },

        AfterAffectImportTemplate: function (res) {
            try {
                _wizardParam.ImportTemplateParams = res.Params;
                if (res.Params.ImportParams.Tables.length > 0)
                    _importGlobalTabParam.Tables = res.Params.ImportParams.Tables;
                var divButton = top.oImportWizard.Wizard.Modal.getDivButton();

                if (res.Step == _wizardInternal.STEP.END && res.Params.ImportParams.NbTabImportAllowed == 0) {
                    divButton.querySelector('div[id="savemodel_btn"]').style.display = 'none';
                    divButton.querySelector('div[id="savemodelas_btn"]').style.display = 'none';
                    divButton.querySelector('div[id="import_btn"]').style.display = 'none';
                    divButton.querySelector('div[id="cancel_btn"]').style.display = 'inline';
                    //Cacher l'etape de l'import
                    this.SwitchVisibilityStep(_currentStep + 1, 'none');
                } else {
                    if (res.Params.ImportTemplateId > 0) {
                        divButton.querySelector('div[id="savemodelas_btn"]').style.display = 'inline';
                        if (res.Params.IsUpdatable == false || res.Params.IsNotSavedImportTemplate == true)
                            divButton.querySelector('div[id="savemodel_btn"]').style.display = 'none';
                        else
                            divButton.querySelector('div[id="savemodel_btn"]').style.display = 'inline';
                    } else
                        divButton.querySelector('div[id="savemodel_btn"]').style.display = 'inline';

                    document.getElementById('editor_' + res.Step).innerHTML = res.Html;
                    //Afficher toutes les rubriques
                    _wizardInternal._HideMappedFields = false;
                }
                _wizardInternal.InitListener();

            } catch (e) {
                eAlert(0, top._res_6524, top._res_8709);
                console.log(e);
            }
        }
        ,
        //Permet d'afficher ou de masquer une étape dasn l'assistant
        SwitchVisibilityStep: function (step, visibility) {
            SwitchVisibilityStep(step, visibility);
        },

        //Affiche et affecte une autre valeur de séparateur
        ShowHideOtherSeparator: function () {
            var separator = document.getElementById('separatorList').value;
            if (separator == '0')
                document.getElementById('otherSeparator').style.display = '';
            else {
                document.getElementById('otherSeparator').style.display = 'none';
                document.getElementById('otherSeparator').value = '';
            }

        },

        /// <summary>
        /// On récupère les paramètres du fichier
        /// </summary>
        SetParsingOption: function (getFirstDataRow) {
            var separator = document.getElementById('separatorList');
            var firstLine = document.getElementById('firstLineHeader');
            var indicator = document.getElementById('textQualifier');
            var otherOperator = document.getElementById('otherSeparator');
            if (typeof separator != 'undefined')
                separator = document.getElementById('separatorList').value;
            else
                separator = ';';

            if (separator == '0' && otherOperator.value != '')
                separator = otherOperator.value;

            if (typeof firstLine != 'undefined')
                firstLine = (document.getElementById('firstLineHeader').checked);
            else
                firstLine = true;

            if (typeof indicator != 'undefined')
                indicator = document.getElementById('textQualifier').value;
            else
                indicator = ' ';

            _importGlobalTabParam.ParsingOption.FirstDataRow = (typeof getFirstDataRow != 'undefined') ? getFirstDataRow : true;
            _importGlobalTabParam.ParsingOption.Delimiter = encode(separator);
            _importGlobalTabParam.ParsingOption.WithHead = firstLine;
            _importGlobalTabParam.ParsingOption.TextQualifier = indicator;

            _wizardParam.ImportGlobalTabParam = _importGlobalTabParam;
        },

        /// <summary>
        /// Nétoyage avant fermeture de l'assistant
        /// </summary>
        Dispose: function (bDeleteFile, fn) {
            if (bDeleteFile == true)
                this.DeleteFile(fn);
            else if (typeof fn == 'function')
                fn();
        },

        // Set la valeur de l'attribute en fournissant l'id de l'element 
        SetAttributeValueById: function (NodeId, attrName, attrValue) {

            if (attrValue == 'paste') {
                document.getElementById('btnFileUpload').disabled = true;
                document.getElementById('btnFileUpload').value = '';
            }
            else {
                document.getElementById('btnFileUpload').disabled = false;
                document.getElementById('rbPasteOption').value = '';
            }



            if (typeof (NodeId) != 'string')
                return false;

            return setAttributeValue(document.getElementById(NodeId), attrName, attrValue);


        },

        ShowFieldsCurrentTable: function (tab) {

            var info = document.getElementById('mappingRappelText');
            if (info != null && typeof info != 'undefined')
                info.innerHTML = top._res_8398;

            var fields = document.querySelectorAll(".fieldItem div[class*='subFieldHeadImport']");

            [].map.call(fields, function (ele) {

                if (getAttributeValue(ele.parentElement, 'tEdndescid') == tab) {
                    if (_wizardInternal._HideFreeFields) {
                        if (isNaN(parseInt(getAttributeValue(ele, 'ednorig')))) {
                            //setAttributeValue(ele.parentElement, 'fActive', '0');
                            addClass(ele.parentElement, 'fieldItemDisplayNone');
                        }

                        else {
                            //setAttributeValue(ele.parentElement, 'fActive', '1');
                            removeClass(ele.parentElement, 'fieldItemDisplayNone');
                        }

                    } else {
                        if (isNaN(parseInt(getAttributeValue(ele, 'ednorig')))
                            || (!isNaN(parseInt(getAttributeValue(ele, 'ednorig'))) && !_wizardInternal._HideMappedFields)) {

                            //setAttributeValue(ele.parentElement, 'fActive', '1');
                            removeClass(ele.parentElement, 'fieldItemDisplayNone');
                        }

                        else {
                            //setAttributeValue(ele.parentElement, 'fActive', '0');
                            addClass(ele.parentElement, 'fieldItemDisplayNone');
                        }

                    }
                } else {
                    //setAttributeValue(ele.parentElement, 'fActive', '0');
                    addClass(ele.parentElement, 'fieldItemDisplayNone');
                }

            });

            var txtTabs = document.querySelectorAll(".txtTabHeadImport");
            if (txtTabs.length > 0) {
                [].map.call(txtTabs, function (ele) {
                    if (getAttributeValue(ele, 'tEdndescid') == tab) {
                        //setAttributeValue(ele, 'fActive', '1');
                        removeClass(ele, 'fieldItemDisplayNone');
                    }

                    else {
                        //setAttributeValue(ele, 'fActive', '0');
                        addClass(ele, 'fieldItemDisplayNone');
                    }


                });
            }

            var tabs = document.querySelectorAll(".tabls_destination_container div[class*='subTableHead']");
            [].map.call(tabs, function (ele) {
                if (getAttributeValue(ele, 'edndescid') == tab) {
                    setAttributeValue(ele, 'tActive', '1');
                    addClass(ele, 'navImportTitleActive navTitleActiveLight');
                }
                else {
                    setAttributeValue(ele, 'tActive', '0');
                    removeClass(ele, 'navImportTitleActive');
                }
            });

        },

        ShowMapElement: function () {

            var activeTab = document.querySelector('div[tactive="1"]');
            var activeTabId = getAttributeValue(activeTab, 'edndescid');
            if (!isNaN(parseInt(activeTabId))) {
                var fieldCibleItemLists = document.querySelectorAll(".field_destination_container .fieldItem[tedndescid='" + activeTabId + "']");

                [].map.call(fieldCibleItemLists, function (ele) {
                    //setAttributeValue(ele, 'fActive', '1');
                    removeClass(ele, 'fieldItemDisplayNone');
                });

                var fieldOriginItemLists = document.querySelectorAll(".field_source_container .fieldItem");
                [].map.call(fieldOriginItemLists, function (ele) {
                    //setAttributeValue(ele, 'fActive', '1');
                    removeClass(ele, 'fieldItemDisplayNone');
                });
                this._HideMappedFields = false;
                this._HideFreeFields = false;
                /*ELAIZ - régression 76 033*/
                _wizardInternal.putToRight();
            }
        },

        HideMapElement: function () {

            var activeTab = document.querySelector('div[tactive="1"]');
            var activeTabId = getAttributeValue(activeTab, 'edndescid');
            if (!isNaN(parseInt(activeTabId))) {
                var fieldCibleItemLists = document.querySelectorAll(".field_destination_container .fieldItem[tedndescid='" + activeTabId + "'] div[class*='subFieldHeadImport'");
                [].map.call(fieldCibleItemLists, function (ele) {
                    if (!isNaN(parseInt(getAttributeValue(ele, 'ednorig')))) {
                        //setAttributeValue(ele.parentNode, 'fActive', '0');
                        addClass(ele.parentNode, 'fieldItemDisplayNone');
                    }

                    else {
                        //setAttributeValue(ele.parentNode, 'fActive', '1');
                        removeClass(ele.parentNode, 'fieldItemDisplayNone');
                    }

                });

                var fieldOriginItemLists = document.querySelectorAll(".field_source_container .fieldItem .subFieldHead");

                //if (this._HideFreeFields && fieldOriginItemLists.length == 0)
                //    fieldOriginItemLists = document.querySelectorAll(".field_source_container .fieldItem div[fActive='0'");
                [].map.call(fieldOriginItemLists, function (ele) {
                    if (isNaN(parseInt(getAttributeValue(ele, 'edndescid')))) {
                        //setAttributeValue(ele.parentNode, 'fActive', '1');
                        removeClass(ele.parentNode, 'fieldItemDisplayNone');
                    } else {
                        //setAttributeValue(ele.parentNode, 'fActive', '0');
                        addClass(ele.parentNode, 'fieldItemDisplayNone');
                    }

                });

                this._HideMappedFields = true;
                this._HideFreeFields = false;
                /*ELAIZ - régression 76 033*/
                _wizardInternal.putToRight();
            }
        },

        HideFreeElement: function () {

            var activeTab = document.querySelector('div[tactive="1"]');
            var activeTabId = getAttributeValue(activeTab, 'edndescid');
            if (!isNaN(parseInt(activeTabId))) {
                var fieldCibleItemLists = document.querySelectorAll(".field_destination_container .fieldItem[tedndescid='" + activeTabId + "'] div[class*='subFieldHeadImport'");
                [].map.call(fieldCibleItemLists, function (ele) {
                    if (isNaN(parseInt(getAttributeValue(ele, 'ednorig')))) {
                        //setAttributeValue(ele.parentNode, 'fActive', '0');
                        addClass(ele.parentNode, 'fieldItemDisplayNone');
                    }
                    else {
                        //setAttributeValue(ele.parentNode, 'fActive', '1');
                        removeClass(ele.parentNode, 'fieldItemDisplayNone');
                    }

                });

                var fieldOriginItemLists = document.querySelectorAll(".field_source_container .fieldItem div[class*='subFieldHead'");
                [].map.call(fieldOriginItemLists, function (ele) {
                    if (hasClass(ele, 'ednMappedOrg')) {
                        // setAttributeValue(ele.parentNode, 'fActive', '1');
                        removeClass(ele.parentNode, 'fieldItemDisplayNone');
                    } else {
                        //setAttributeValue(ele.parentNode, 'fActive', '0');
                        addClass(ele.parentNode, 'fieldItemDisplayNone');
                    }

                });

                this._HideMappedFields = false;
                this._HideFreeFields = true;
                /*ELAIZ - régression 76 033*/
                _wizardInternal.putToRight();
            }
        },

        /// <summary>
        /// Affecter une clé de dédoublonnage
        /// </summary>
        KeyChange: function (fieldId, tabId) {

            var remove = false;
            var existTable = false;
            var tabIndex = -1;
            var ednDescid = 0;
            var field = document.getElementById(fieldId);
            if (typeof field != 'undefined' && field != null && getAttributeValue(field, 'ednorig') != '' && !isNaN(parseInt(getAttributeValue(field, 'edndescid'))))
                ednDescid = parseInt(getAttributeValue(field, 'edndescid'));

            if (ednDescid != 0) {
                if (hasClass(field, 'ednKeyImportField') && getAttributeValue(field, 'ednkey') == '1') {
                    setAttributeValue(field, 'ednkey', '');
                    removeClass(field, "ednKeyImportField");
                    remove = true;
                } else {

                    setAttributeValue(field, 'ednkey', '1');
                    addClass(field, "ednKeyImportField");

                }

                _importGlobalTabParam.SetTableKey(tabId, ednDescid, remove);
            }
        },
        /// <summary>
        /// Affecter une clé de dédoublonnage à une table
        /// </summary>
        TabKeyChange: function (tabElement, force) {

            var ednDescid = '';
            if (typeof tabElement != 'undefined' && tabElement != null
                && !isNaN(parseInt(getAttributeValue(tabElement, 'edndescid'))))
                ednDescid = getAttributeValue(tabElement, 'edndescid');
            var oRes = _importGlobalTabParam.CheckExistTabParams(ednDescid);
            if (ednDescid != '' && getAttributeValue(tabElement, 'tmaintab') != '1' && getAttributeValue(tabElement, 'tactive') == '1') {
                if (hasClass(tabElement, 'ednKeyImportField') && getAttributeValue(tabElement, 'ednTabkey') == '1' || (force && !oRes.Existe)) {
                    setAttributeValue(tabElement, 'ednTabkey', '');
                    removeClass(tabElement, "ednKeyImportField");
                    this.SetRelationFieldAsKey(ednDescid, 0);
                } else {

                    if (oRes.Existe) {
                        setAttributeValue(tabElement, 'ednTabkey', '1');
                        addClass(tabElement, "ednKeyImportField");
                        this.SetRelationFieldAsKey(ednDescid, 1);
                    }
                }
            }
        },

        /// <summary>
        /// Supprimer les clés de dédoublonnage pour les tables 
        /// </summary>
        RemoveTabsKey: function () {
            var tabs = document.querySelectorAll('.tabls_destination_container div[canbekey="1"]');
            [].map.call(tabs, function (ele) {
                setAttributeValue(ele, 'edntabkey', '0');
                removeClass(ele, 'ednKeyImportField');
            });
        },
        /// <summary>
        /// Supprimer la clé de dédoublonnage pour une table 
        /// </summary>
        RemoveTabKey: function (tabId) {
            var tabs = document.querySelectorAll('.tabls_destination_container div[canbekey="1"]');
            [].map.call(tabs, function (ele) {
                if (getAttributeValue(ele, 'edndescid') == tabId) {
                    setAttributeValue(ele, 'edntabkey', '0');
                    removeClass(ele, 'ednKeyImportField');
                }
            });
        },

        SetOriFieldId: function (id) {
            _currentFieldSelector = id;
        },


        GetInfosField: function (id, event) {
            var divSource = document.getElementById(id);
            var divCible = document.getElementById("mappingRappelText");
            var ednLabel = getAttributeValue(divSource, "ednlabel");
            var ednType = getAttributeValue(divSource, "edntype");
            var ednInfos = getAttributeValue(divSource, "edntypeinfos");
            var txt = top._res_8374.replace("<FIELD>", ednLabel).replace("<FIELDTYPE>", ednType) + ' ' + ednInfos;
            divCible.innerHTML = txt;

            //Cacher les menus des options 
            if (!event.target.parentElement.classList.contains('rChk'))
                this.HideFieldOption(event.target.nextElementSibling);
        },
        HideFieldOption: function (target) {
            var openDropdown = document.querySelectorAll(".dropdown-content");
            [].map.call(openDropdown, function (ele) {
                if (ele !== target)
                    ele.classList.remove('show');
            });
        }
        ,

        //Vider le mapping d'une rubrique
        SetEmptyField: function (cibleField) {
            var activeTab = document.querySelector('.tabls_destination_container div[tactive="1"]');
            var activeTabId = getAttributeValue(activeTab, 'edndescid');

            if (cibleField != null && typeof cibleField != 'undefined') {
                var originField = document.getElementById(getAttributeValue(cibleField, 'ednorig'));
                var table = getAttributeValue(cibleField, 'edntabdescid');
                var endmulti = getAttributeValue(cibleField, 'endmulti') === '1';

                removeClass(cibleField.nextSibling, 'ednMappedTgt');
                if (getAttributeValue(cibleField, 'ednkey') == 1)
                    removeClass(cibleField, 'ednKeyImportField');
                cibleField.nextSibling.innerHTML = '';
                var origEdnDecid = getAttributeValue(originField, 'edndescid');
                var cibleEdnDescid = getAttributeValue(cibleField, 'edndescid');

                if (origEdnDecid != null && typeof origEdnDecid != 'undefined' && originField != null) {
                    if ((';' + origEdnDecid).indexOf(';' + cibleEdnDescid) != -1)
                        origEdnDecid = origEdnDecid.replace(cibleEdnDescid, '').replace(';;', ';');

                    if (origEdnDecid.replace(';', '') == '') {
                        origEdnDecid = '';
                        removeClass(originField, 'ednMappedOrg');
                        //setAttributeValue(originField.parentNode, 'fActive', '1');
                        removeClass(originField.parentNode, 'fieldItemDisplayNone');
                    }

                    // setAttributeValue(cibleField.parentNode, 'fActive', activeTabId == table ? '1' : '0');
                    if (activeTabId == table)
                        removeClass(cibleField.parentNode, 'fieldItemDisplayNone');
                    else
                        addClass(cibleField.parentNode, 'fieldItemDisplayNone');

                    setAttributeValue(originField, 'edndescid', origEdnDecid);
                }

                setAttributeValue(cibleField, 'ednorig', '');

                //Si le bouton radio Masquer les rubriques non sélectionnées est activé, on masque les 2 rubriques 'cible' et 'origine'(si vide)
                if (_wizardInternal._HideFreeFields) {
                    if (origEdnDecid == '') {
                        //setAttributeValue(originField.parentNode, 'fActive', '0');
                        addClass(originField.parentNode, 'fieldItemDisplayNone');
                    }

                    //setAttributeValue(cibleField.parentNode, 'fActive', '0');
                    addClass(cibleField.parentNode, 'fieldItemDisplayNone');
                }
                //Suppression du Mapping de la rubrique pour la table
                var emptyTabMapping = _importGlobalTabParam.RemoveTabMapping(table, parseInt(cibleEdnDescid));
                //Si aucune rubrique n'est mapée, on supprime la clée de dédoublonnage de la table
                if (emptyTabMapping)
                    this.RemoveTabKey(table);
                //Masquer le menu pour les rubriques type catalogue multiple et décoché les options
                if (endmulti) {
                    var optienChecked = cibleField.firstChild.lastChild.getElementsByClassName('icon-square-o');
                    if (optienChecked.length > 0)
                        [].map.call(optienChecked, function (option) {
                            option.classList.value = "icon-square-o";
                        });
                    cibleField.firstChild.lastChild.style.display = "none";
                }
            }

        },

        //Vider le mapping d'une colonne 
        SetEmptyOrigineField: function (id, cibleEdnDescid) {

            var originField = document.getElementById(id);
            var origEdnDecid = getAttributeValue(originField, 'edndescid');

            if (origEdnDecid != null && typeof origEdnDecid != 'undefined') {
                if ((';' + origEdnDecid).indexOf(';' + cibleEdnDescid) != -1)
                    origEdnDecid = origEdnDecid.replace(cibleEdnDescid, '').replace(';;', ';');

                if (origEdnDecid.replace(';', '') == '') {
                    origEdnDecid = '';
                    removeClass(originField, 'ednMappedOrg');

                    if (document.querySelector('input[id="rdbHide"]:checked') != null) {
                        //setAttributeValue(originField.parentNode, 'fActive', '1');
                        removeClass(originField.parentNode, 'fieldItemDisplayNone');
                    }

                }


                setAttributeValue(originField, 'edndescid', origEdnDecid)
            }

        },

        //Permet de définir une rebrique relationnelle comme clé de dédoublonnage
        SetRelationFieldAsKey: function (tabId, isChecked) {
            _importGlobalTabParam.SetTableAsKey(tabId, isChecked);
        },

        //Exécuter un Mapping
        GetCibleFieldId: function (id) {
            if (_currentFieldSelector != null && typeof _currentFieldSelector != 'undefined') {
                var originField = document.getElementById(_currentFieldSelector);
                if (typeof originField != 'undefined') {
                    var cibleField = document.getElementById(id);

                    if (cibleField != null && typeof cibleField != 'undefined') {
                        var endmulti = getAttributeValue(cibleField, 'endmulti') === '1';
                        var table = getAttributeValue(cibleField, 'edntabdescid');
                        var cibleEdnDescid = getAttributeValue(cibleField, 'edndescid');
                        var origEdndescid = getAttributeValue(originField, 'edndescid');


                        var cibleEdnFormat = getAttributeValue(cibleField, 'ednformat');

                        origEdndescid = ((origEdndescid == '') ? '' : (origEdndescid + ';')) + cibleEdnDescid;
                        setAttributeValue(originField, 'edndescid', origEdndescid);

                        cibleField.nextSibling.innerHTML = originField.innerHTML;
                        cibleField.firstChild.lastChild.style.display = "block";

                        removeClass(cibleField.nextSibling.firstChild, 'fieldcibleSpan');

                        setAttributeValue(cibleField, 'ednorig', _currentFieldSelector);
                        setAttributeValue(cibleField.nextSibling, 'ednorig', _currentFieldSelector);
                        addClass(cibleField.nextSibling, 'ednMappedTgt');
                        addClass(originField, 'ednMappedOrg');

                        //Ajouter un listener pour pouvoir démapper une rubrique
                        if (getAttributeValue(cibleField.nextSibling, 'bListner') != '1') {
                            cibleField.nextSibling.addEventListener('dblclick', function () {
                                _wizardInternal.SetEmptyField(cibleField);
                                setAttributeValue(cibleField.nextSibling, 'bListner', '1');
                                if (endmulti) {
                                    //Masquer le menu pour les rubriques type catalogue multiple
                                    cibleField.firstChild.lastChild.style.display = "none";
                                }
                                var oRes = _importGlobalTabParam.CheckExistTabParams(table);
                                if (!oRes.Existe)
                                    _wizardInternal.TabKeyChange(document.getElementById('t_' + table), true);
                            }, false);
                        }

                        //Si le bouton radio Masquer les rubriques sélectionnées est activé, on masque les 2 rubriques 'cible' et 'origine'
                        if (_wizardInternal._HideMappedFields) {
                            //setAttributeValue(originField.parentNode, 'fActive', '0');
                            //setAttributeValue(cibleField.parentNode, 'fActive', '0');
                            addClass(originField.parentNode, 'fieldItemDisplayNone');
                            addClass(cibleField.parentNode, 'fieldItemDisplayNone');
                        }


                        removeClass(cibleField.parentNode, 'fieldItemHover');




                        //Ajouter au dictionnaire des rubriques mappées
                        var oRes = {};
                        oRes.Table = table;
                        oRes.Value = _currentFieldSelector;
                        oRes.Cible = parseInt(cibleEdnDescid);
                        oRes.ColName = getAttributeValue(cibleField.nextSibling.firstChild, 'title');
                        this.UpdateSelectedParams(oRes);


                        if (cibleEdnFormat == "4")
                            _wizardInternal.KeyChange(id, table);


                        if (cibleEdnDescid == "101001") {
                            //Force clé
                            _wizardInternal.KeyChange(id, table);
                        }
                    }
                }
            }

            //Libérer le selector
            _currentFieldSelector = null;

        },

        ///Mise à jour des fields mappés sur chaque table
        UpdateSelectedParams: function (params) {
            //console.clear();
            var table = params.Table;
            var tabIndex = -1;
            var isRelation = (table.split("_").length == 2);
            var _mapParam = { Field: params.Cible, Col: parseInt(params.Value), ColName: params.ColName };
            var _ImportTabParam = {
                TabInfo: {
                    TabInfoId: table,
                    TabDescId: (isRelation ? parseInt(table.split("_")[1]) : parseInt(table)),
                    IsRelation: isRelation,
                    IsRequiered: (getAttributeValue(document.querySelector('div[edndescid="' + table + '"]'), 'oblig') == '1'),
                    RelationField: (isRelation ? parseInt(table.split("_")[0]) : 0),
                    ImportRight: (getAttributeValue(document.querySelector('div[edndescid="' + table + '"]'), 'eImportRight') == '1')
                },
                ParentTab: 0,
                Keys: [],
                Mapp: [_mapParam],
                Create: true,
                Update: true,
                IsKey: false

            }
            //On ajoute le paramètre de la table à l'object globale 
            _importGlobalTabParam.AddTablesParams(_ImportTabParam);

            //console.clear();
            // console.log(_importGlobalTabParam);

        },

        ValidImportMappingParams: function () {
            return this.CheckObligFields();
        },

        /// <summary>
        /// Construire un dictionay des options pour chaque table mappée
        /// </summary>
        ValidImportOptionsParams: function () {
            var tabsLine = document.querySelectorAll(".data-source-step .headerOptionsLine");
            var nAllawoedTable = 0;
            for (var i = 0; i < tabsLine.length; i++) {
                var tabOption = { Create: (getAttributeValue(tabsLine[i], 'ednCreate') == '1'), Update: 0 };
                var descid = getAttributeValue(tabsLine[i], 'edntabid');
                var existeFile = document.getElementById('exist_' + descid);
                var nExisteFile = document.getElementById('nexist_' + descid);


                if (typeof existeFile != 'undefined' && existeFile != null)
                    tabOption.Update = (existeFile.options[existeFile.selectedIndex].value == '1');

                if (typeof nExisteFile != 'undefined' && nExisteFile != null)
                    tabOption.Create = (nExisteFile.options[nExisteFile.selectedIndex].value == '1');

                var existTable = false;
                var tabIndex = -1;

                var verifTab = _importGlobalTabParam.CheckExistTabParams(descid);

                if (verifTab.Existe == true && verifTab.Index != -1) {
                    _importGlobalTabParam.Tables[verifTab.Index].Create =  tabOption.Create,
                        _importGlobalTabParam.Tables[verifTab.Index].Update =  tabOption.Update;

                } else if (descid == TAB_ADR) {
                    //Si la table est Adresse (présente dans l'étape OPTIONS) et qu'elle n'est pas mappée, on l'ajoute pour l'afficher dans l'étape de récap
                    var _ImportTabParam = {
                        TabInfo: {
                            TabInfoId: descid,
                            TabDescId: TAB_ADR,
                            IsRelation: false,
                            IsRequiered: false,
                            RelationField: 0,
                            ImportRight: true
                        },
                        ParentTab: 0,
                        Keys: [],
                        Mapp: [],
                        Create: true,
                        Update: true,
                        IsKey: false

                    };

                    _importGlobalTabParam.AddTablesParams(_ImportTabParam);
                }

                if (tabOption.Create || tabOption.Update)
                    nAllawoedTable++;
            }

            _importGlobalTabParam.nAllawedTable = nAllawoedTable;
            return true;
        },

        /// <summary>
        /// Vérif si toutes les rubriques obligatoires sont mappées
        /// </summary>
        CheckObligFields: function (fct) {
            //Check si au moin une rubrique est mappée
            var valide = this.CheckMappingMainTable();
            var oRes = {};
            oRes.LstObligatEmpty = valide.LstObligatEmpty;
            oRes.Success = valide.Success;
            oRes.FctErr = valide.FctErr;

            if (valide.LstObligatEmpty.length > 0)
                oRes.Success = false;

            if (!oRes.Success) {

                var strObligatFields = _wizardInternal.GetFormatedInformationString(oRes.LstObligatEmpty, 5, true);
                var txtError = (oRes.LstObligatEmpty.length == 0 ? top._res_8366 : top._res_8497);
                if (typeof (fct) != "function")
                    fct = function () { };

                oRes.FctErr = function () { top.eAlert(0, top._res_8365, txtError.replace('<TABLE>', valide.MainLibelle).replace('<FIELDS>', strObligatFields), '', null, null, fct) };
            } else {
                var oResObligFileds = this.CheckMappingTable();

                if (!oResObligFileds.Success) {

                    var _bRequieredFieldsForTab = (oResObligFileds.LstRequiredFieldsForTable != null && typeof oResObligFileds.LstRequiredFieldsForTable != 'undefined' && oResObligFileds.LstRequiredFieldsForTable.length > 0);

                    var _tab = new Array();
                    var errorTxt = '';


                    if (_bRequieredFieldsForTab == true) {
                        _tab = oResObligFileds.LstRequiredFieldsForTable;
                        errorTxt = top._res_8663;
                    }


                    if (_tab.length > 0) {
                        var nbTab = _tab.length;
                        var errorRes = (nbTab == 1) ? top._res_8440 : top._res_8441;
                        var strTab = _wizardInternal.GetFormatedInformationString(_tab, 5, false);

                        if (typeof (fct) != "function")
                            fct = function () { };

                        oRes.Success = oResObligFileds.Success;

                        _wizardInternal = this;
                        oRes.FctErr = function () {

                            top.eConfirm(eMsgBoxCriticity.MSG_INFOS, top._res_8479, errorTxt, strTab, 600, 200, function () {
                                SwitchStep(_wizardInternal.STEP.OPTIONS);
                            }, _wizardInternal.Cancell, false, true);
                        }

                    }

                }

            }

            return oRes;
        },

        /// <summary>
        /// Check si au moin une rubrique est mappée
        /// </summary>
        CheckMappingMainTable: function (fct) {

            var oRes = {};
            oRes.Success = false;
            oRes.MainTable = 0;
            oRes.MainLibelle = '';
            oRes.FctErr = new function () { };
            oRes.LstObligatEmpty = new Array();

            var mainTab = document.querySelectorAll(".tabls_destination_container div[class*='subTableHead'][tMainTab='1']")[0];
            if (mainTab != null && typeof mainTab != 'undefined') {
                oRes.MainTable = getAttributeValue(mainTab, 'edndescid');
                oRes.MainLibelle = getAttributeValue(mainTab, 'ednTabLabelle');
                if (oRes.MainTable != '') {
                    var lstFieldMainTable = document.querySelectorAll(".field_destination_container [edntabdescid='" + oRes.MainTable + "']");
                    for (var i = 0; i < lstFieldMainTable.length; i++) {
                        if (getAttributeValue(lstFieldMainTable[i], 'ednorig') != '') {
                            oRes.Success = true;
                            break;
                        }
                    }

                    //On check plus les rubriques obligatoires
                    /*
                    if (oRes.Success) {
                        var lstObligat = document.querySelectorAll(".field_destination_container [edntabdescid='" + oRes.MainTable + "'][ednoblig='1']");
                        for (var ii = 0; ii < lstObligat.length; ii++) {
         
                            var node = lstObligat[ii];
                            //var oCheck = node.parentNode;
                            var nIdx = getAttributeValue(node, "id");
                            var sLabel = getAttributeValue(node, "ednlabel");
         
                            if (getAttributeValue(node, "ednorig") == "") {
                                oRes.LstObligatEmpty.push(sLabel);
                                oRes.Success = false;
                            } else
                                oRes.Success = true;
                        }
                    }
                    */
                }
            }

            return oRes;
        },


        CheckMappingTable: function (fct) {
            return this.CheckObligFieldsMappingForMappedTables();
        },

        CheckRequiredTable: function () {
            var oRes = {};
            oRes.LstRequiredTable = new Array();
            oRes.Success = true;
            oRes.FctErr = function () { };
            var requiredTable = document.querySelectorAll('.tabls_destination_container .subTableHead[oblig="1"]');

            for (var i = 0; i < requiredTable.length; i++) {
                var _tabId = getAttributeValue(requiredTable[i], 'edndescid');
                var _table = _importGlobalTabParam.CheckExistTabParams(_tabId);
                var _exist = _table.Existe;
                var _tabLibelle = getAttributeValue(requiredTable[i], 'ednTabLabelle');

                if (_exist) {
                    var mapp = _importGlobalTabParam.Tables[_table.Index].Mapp;

                    var requiredFields = document.querySelectorAll(".field_destination_container [edntabdescid='" + _tabId + "'][ednoblig='1'][ednorig='']");
                    if (mapp.length == 0 || requiredFields.length > 0) {
                        oRes.LstRequiredTable.push(_tabLibelle);
                        oRes.Success = false;
                    }

                } else {
                    oRes.LstRequiredTable.push(_tabLibelle);
                    oRes.Success = false;
                }
            }

            return oRes;
        },

        //Check si toutes les rubriques obligatoires sont mappées de Toutes les tables qui contiennent un Mapping 
        CheckObligFieldsMappingForMappedTables: function () {

            var oRes = {};
            oRes.LstRequiredFieldsForTable = new Array();
            oRes.Success = true;
            oRes.FctErr = function () { };

            var indexMainTable = 0;
            var mainTabCreate = true;
            var mainTemplateTab = null;

            _importGlobalTabParam.Tables.forEach(function (_table, index) {

                var _bMainTab = (_table.TabInfo.TabInfoId == _importGlobalTabParam.ImportTab);

                if (_bMainTab)
                    indexMainTable = index;



                var _tabLibelle = getAttributeValue(document.querySelector('div[edndescid="' + _table.TabInfo.TabInfoId + '"]'), 'ednTabLabelle');

                var requiredFields = document.querySelectorAll(".field_destination_container [edntabdescid='" + _table.TabInfo.TabInfoId + "'][ednoblig='1'][ednorig='']");

                if (!_bMainTab && _table.TabInfo.IsRequiered && _table.Mapp.length == 0)
                    mainTabCreate = false;

                var templateTab = _importGlobalTabParam.GetImportTemplateTab(_table.TabInfo.TabInfoId);

                if (_bMainTab)
                    mainTemplateTab = templateTab;

                if (requiredFields.length > 0) {
                    oRes.LstRequiredFieldsForTable.push(_tabLibelle);
                    _table.Create = false;
                    //Si c'est la table main, alors la création n'est pas possible
                    if (_bMainTab)
                        mainTabCreate = false;
                    else
                        mainTabCreate = (mainTabCreate == true && _table.Keys.length > 0);

                } else {


                    if (templateTab != null && typeof templateTab != 'undefined') {
                        _table.Create = templateTab.Create;
                    } else
                        _table.Create = true;

                }

                _table.Update = ((templateTab != null && typeof templateTab != 'undefined') ? templateTab.Update : _table.Keys.length > 0);
            });


            //Parcourir les tables obligatoires
            Array.prototype.slice.apply(document.querySelectorAll(".tabls_destination_container div[oblig='1']")).forEach(
                function (arrElem) {
                    var _oRes = _importGlobalTabParam.CheckExistTabParams(getAttributeValue(arrElem, 'edndescid'));
                    var _tabLibelle = getAttributeValue(arrElem, 'ednTabLabelle');
                    if (!_oRes.Existe) {
                        mainTabCreate = false;
                        oRes.LstRequiredFieldsForTable.push(_tabLibelle);
                    }
                }
            );

            _importGlobalTabParam.Tables[indexMainTable].Create = (mainTemplateTab != null ? mainTemplateTab.Create : mainTabCreate);

            if (oRes.LstRequiredFieldsForTable.length > 0)
                oRes.Success = false;

            return oRes;
        },

        GetFormatedInformationString: function (Array, nb, addSpace) {
            var str = '';
            if (Array != null && typeof Array != 'undefined' && Array.length > 0 && nb > 0) {
                var char1 = '* ';
                var char2 = ' ...';
                var char3 = '[[BR]]';
                for (var i = 0; i < Array.length; i++) {
                    str += (addSpace ? (i == 0 ? char3 : '') : '') + char1 + Array[i];
                    if (i == nb) {
                        str += char2 + char3;
                        break;
                    } else
                        str += char3;

                }
            }
            return str;
        },

        SwitchActivePage: function (nbPage) {

            var lstPaging = document.querySelectorAll("span[id*='swImportPg']");
            var lstTab = document.querySelectorAll(".tabls_destination_container .subTableHead");
            for (var i = 0; i < lstTab.length; i++) {

                var tpage = getAttributeValue(lstTab[i], 'tpage')
                if (parseInt(tpage) == parseInt(nbPage))
                    setAttributeValue(lstTab[i], "tactivetabpaging", 1);
                else
                    setAttributeValue(lstTab[i], "tactivetabpaging", 0);
            }


            for (var i = 0; i < lstPaging.length; i++) {

                var tpage = getAttributeValue(lstPaging[i], 'tpage')
                if (parseInt(tpage) == parseInt(nbPage)) {
                    addClass(lstPaging[i], 'icon-circle imgActn');
                    removeClass(lstPaging[i], 'icon-circle-thin imgInact');
                }

                else {
                    addClass(lstPaging[i], 'icon-circle-thin imgInact');
                    removeClass(lstPaging[i], 'icon-circle imgActn ');
                }
            }
        },

        SetEmptyMapping: function () {
            var activeTab = document.querySelector('div[tactive="1"]');
            var activeTabId = getAttributeValue(activeTab, 'edndescid');
            if (!isNaN(parseInt(activeTabId))) {
                var fieldCibleItemLists = document.querySelectorAll(".subFieldHeadImport:not([ednorig=''])");
                [].map.call(fieldCibleItemLists, function (ele) {
                    oImportWizardInternal.SetEmptyField(ele);
                });

                oImportWizardInternal.RemoveTabsKey();
            }
            _wizardInternal.ClearMapping();
        },

        ClearMapping: function () {
            _importGlobalTabParam.ClearTablesParams();
        },

        ResetMapping: function () {
            _wizardInternal.SetEmptyMapping();
            _wizardParam.ImportTemplateParams.ImportTemplateId = 0;
            _wizardInternal.DeleteFile();
            _wizardInternal.MoveStep(false, false);
        },
        Cancell: function () {
            return;
        },

        ResetImportTemplate: function (elem) {

            if (_wizardParam.ImportGlobalTabParam.Tables.length > 0) {
                top.eConfirm(eMsgBoxCriticity.MSG_INFOS, top._res_6343, top._res_8722, '', 600, 200, function () {
                    var divButton = top.oImportWizard.Wizard.Modal.getDivButton();
                    divButton.querySelector('div[id="savemodelas_btn"]').style.display = 'none';
                    setAttributeValue(elem, 'class', 'mapping_header_title');
                    elem.removeAttribute('style');
                    elem.removeAttribute('onclick');
                    elem.innerText = top._res_1111;
                    setAttributeValue(elem, 'title', top._res_1111);
                    _wizardInternal.SetEmptyMapping();
                    _wizardParam.ImportTemplateParams.ImportTemplateId = 0;
                }, '', false, true);
            }

        },
        DragStart: function (event) {
            if (!_browser.isIE && !_browser.isEdge)
                event.dataTransfer.setData('text/plain', null);

            var id = getAttributeValue(event.target, "field");
            this.SetOriFieldId(id);

        },
        Dragging: function (event) {
        },
        DragEnd: function (event) {
            console.log('dragEnd');
        },
        AllowDrop: function (event) {
            try {
                stopEvent(event);
                var element = event.target.parentNode;
                var id = getAttributeValue(element, "field");
                if (getAttributeValue(element, 'edndescid') == '') {
                    element = document.getElementById(id);
                }

                addClass(element.parentNode, 'fieldItemHover');
            } catch (e) {
                console.log('Erreur AllowDrop:' + e.message);
            }

        },
        LeaveDrop: function (event) {
            try {
                stopEvent(event);
                var element = event.target.parentNode;
                var id = getAttributeValue(element, "field");
                if (getAttributeValue(element, 'edndescid') == '') {
                    element = document.getElementById(id);
                }

                removeClass(element.parentNode, 'fieldItemHover');
            } catch (e) {
                console.log('Erreur LeaveDrop:' + e.message);
            }

        },

        Drop: function (event) {

            try {
                stopEvent(event);
                //event.preventDefault();
                //var data = event.dataTransfer.getData("Text");

                var element = event.target.parentNode;
                var id = getAttributeValue(element, "field");

                if (getAttributeValue(element, 'edndescid') == '') {
                    element = document.getElementById(id);
                }


                if (getAttributeValue(element, 'ednformat') == '4') {

                }

                var ednOrig = getAttributeValue(element, 'ednorig');
                if (ednOrig != '')
                    this.SetEmptyOrigineField(ednOrig, getAttributeValue(element, 'edndescid'));

                this.GetCibleFieldId(element.id);
                //#72 188 KJE, Dans le même cadre, on affiche les informations de l'élément après le Drop
                _wizardInternal.GetInfosField(element.id, event);

            } catch (e) {
                console.log('Erreur Drop:' + e.message);
            }
        },

        DrawProgression: function (html) {

            try {
                if (_currentStep >= 3) {
                    var beginPercent = 0;
                    var maxPercent = 100;
                    var duration = 5000;
                    var stepDuration = duration / (maxPercent - beginPercent);
                    var stepPercent = Math.round((maxPercent / (maxPercent - beginPercent)) * 1000) / 1000;

                    if (typeof top.oImportWizard != undefined)
                        top.oImportWizard.Wizard.Modal.romovebtnClose();

                    var content = document.getElementById('content');
                    content.style.height = (document.documentElement.clientHeight * 90 / 100) + 'px';
                    content.innerHTML = html;

                    var progression = beginPercent;

                    var progress = setInterval(function () {

                        if (progression >= maxPercent)
                            progression = maxPercent;

                        setAttributeValue(document.getElementById('pathCircle'), 'stroke-dasharray', progression + ', 100');
                        document.getElementById('percent').textContent = progression + '%';



                        if (progression == maxPercent) {

                            //top.oImportWizard.Wizard.Modal.addButton(top._res_30, this.Dispose, "button-green", null, "cancel_btn");//Fermer
                            document.getElementById('importInfos').style.display = 'block';
                            window.clearInterval(progress);
                        } else
                            progression += Math.round((stepPercent * 1000) / 1000);
                    }, stepDuration);
                }

            } catch (e) {
                console.log(e);
            }


        },

        /*ELAIZ - régression 76 033 : rajout de la classe CSS Right sur les éléments situés sur la droite pour changer la position du dropdown*/

        addRightCSSClass: function (delta) {

            var fieldItems = [].slice.call(document.querySelectorAll("#editor_2 .global_field_destination_container #field_destination_container .fieldItem:not(.fieldItemDisplayNone)"));

            for (i = (delta - 1); i < fieldItems.length; i = i + delta) {
                var rightFieldItems = fieldItems[i];
                rightFieldItems.classList.add('right');

            }
        },

        putToRight: function () {

            paramWin = top.getParamWindow();
            objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());

            var fieldItems = [].slice.call(document.querySelectorAll("#editor_2 .global_field_destination_container #field_destination_container .fieldItem:not(.fieldItemDisplayNone)"));

            for (i = 0; i < fieldItems.length; i++) {
                if (fieldItems[i].classList.contains('right'))
                    fieldItems[i].classList.remove('right');

            }

            if (window.innerWidth < 1300) {
                if (objThm.Version != 1) {
                    _wizardInternal.addRightCSSClass(2);
                } else {
                    _wizardInternal.addRightCSSClass(6);
                }
            } else {
                if (objThm.Version != 1) {
                    _wizardInternal.addRightCSSClass(4);
                } else {
                    _wizardInternal.addRightCSSClass(9);
                }
            }

        },


        //Initier un listner 
        InitListener: function () {
            //Si l'etape du mapping, on ajoute un listener sur les rubriques pour démapper sur double click
            switch (_currentStep) {
                case _wizardInternal.STEP.MAPPING:
                    //Ajout un listner click/dblclick/dragover sur l'entête des tables
                    var tableList = document.querySelectorAll(".tabls_destination_container .navEntry");
                    [].map.call(tableList, function (ele) {
                        var tabDescid = getAttributeValue(ele, 'edndescid');
                        var canBeKey = (getAttributeValue(ele, 'canbekey') == '1');
                        //Ne pas affecter une clé de dédoublonnage si la rubrique est multiple
                        if (tabDescid != '') {
                            ele.addEventListener('click', function (e) {
                                stopEvent(e);
                                _wizardInternal.ShowFieldsCurrentTable(tabDescid);
                                _wizardInternal.HideFieldOption(null);
                            }, false);
                            if (canBeKey)
                                ele.addEventListener('dblclick', function (e) {
                                    stopEvent(e);
                                    _wizardInternal.TabKeyChange(ele);
                                }, false);

                            ele.addEventListener("dragover", function (e) {
                                _wizardInternal.ShowFieldsCurrentTable(tabDescid);

                            }, false);
                        }
                    });

                    var fieldItemLists = document.querySelectorAll(".field_destination_container .fieldItem");
                    [].map.call(fieldItemLists, function (ele) {
                        var field = getAttributeValue(ele, 'field');
                        ele.addEventListener("click", function (e) {
                            stopEvent(e);
                            _wizardInternal.GetInfosField(field, e);
                        }, false);

                        ele.addEventListener("dragover", function (e) {
                            _wizardInternal.AllowDrop(e);
                            _wizardInternal.HideFieldOption(null);

                        }, false);

                        ele.addEventListener("dragleave", function (e) {
                            _wizardInternal.LeaveDrop(e);

                        }, false);

                        ele.addEventListener("drop", function (e) {
                            _wizardInternal.Drop(e);

                        }, false);
                    });

                    var fieldList = document.querySelectorAll(".field_destination_container .subFieldHeadImport");

                    [].map.call(fieldList, function (ele) {
                        var id = ele.id;
                        var tabDescid = getAttributeValue(ele, 'edntabdescid');
                        var bMulti = getAttributeValue(ele, 'endmulti');


                        //Ne pas affecter une clé de dédoublonnage si la rubrique est multiple
                        if (tabDescid != '' && bMulti != '1' && tabDescid != "101000")
                            ele.addEventListener('dblclick', function (e) {
                                stopEvent(e);
                                _wizardInternal.KeyChange(id, tabDescid);
                            }, false);
                    });

                    var fieldMappedItemLists = document.querySelectorAll('div[class="subFieldBottom ednMappedTgt"]');
                    if (fieldMappedItemLists.length > 0) {
                        [].map.call(fieldMappedItemLists, function (ele) {

                            //Ajouter un listener pour pouvoir démapper une rubrique
                            if (getAttributeValue(ele, 'bListner') != '1') {
                                ele.addEventListener('dblclick', function () {
                                    var cibleFieldOrigIndex = getAttributeValue(ele, 'ednorig');
                                    var cibleField = ele.parentElement.querySelector('div .subFieldHeadImport[ednorig="' + cibleFieldOrigIndex + '"]');
                                    _wizardInternal.SetEmptyField(cibleField);
                                    setAttributeValue(ele, 'bListner', '1');
                                }, false);
                            }
                        });
                    }

                    var options = document.querySelectorAll('.options_header .optionsMapping .divOptions input');
                    if (options.length > 0) {
                        [].map.call(options, function (ele) {
                            //Ajouter un listener pour pouvoir afficher/cacher des rubriques
                            ele.onclick = function () {
                                var id = ele.id;
                                switch (id) {
                                    case 'rdbShow':
                                        _wizardInternal.ShowMapElement();
                                        break;
                                    case 'rdbHide':
                                        _wizardInternal.HideMapElement();
                                        break;
                                    case 'rdbHideFreeElement':
                                        _wizardInternal.HideFreeElement();
                                        break;
                                    default:
                                        break;
                                }
                            };
                        });
                    }

                    var fieldCatMulti = document.querySelectorAll('[class="dropbtn"]');
                    if (fieldCatMulti.length > 0) {
                        [].map.call(fieldCatMulti, function (ele) {
                            //Ajouter un listener pour pouvoir afficher ou masquer le menu options catalogue
                            if (getAttributeValue(ele, 'bListner') !== '1') {
                                ele.addEventListener('click', function () {
                                    var dropdowns = document.getElementsByClassName("dropdown-content");
                                    var i;
                                    for (i = 0; i < dropdowns.length; i++) {
                                        var openDropdown = dropdowns[i];
                                        if (!ele.nextElementSibling.classList.contains('show')) {
                                            openDropdown.classList.remove('show');
                                        }
                                    }
                                    /*ELAIZ - régression 76 033*/
                                    _wizardInternal.putToRight();
                                    ele.nextElementSibling.classList.toggle("show");

                                    setAttributeValue(ele, 'bListner', '1');
                                }, false);
                            }

                        });
                    }


                    var fieldOptions = document.querySelectorAll('a[class="rChk"]');
                    if (fieldOptions.length > 0) {
                        [].map.call(fieldOptions, function (ele) {
                            //Ajouter un listener pour pouvoir mettre à jour la valeur de l'option du catalogue
                            if (getAttributeValue(ele, 'bListner') !== '1') {
                                ele.addEventListener('click', function (e) {
                                    var checked = ele.children[0].classList.value === "icon-check-square";
                                    ele.children[0].classList.value = checked === true ? "icon-square-o" : "icon-check-square";
                                    var fieldDescId = getAttributeValue(ele, 'edndescid');
                                    var tabDescId = getAttributeValue(ele, 'edntabdescid');
                                    var opt = getAttributeValue(ele, 'opt');
                                    _importGlobalTabParam.SetOptionField(tabDescId, fieldDescId, {
                                        Name: opt === "merge" ?
                                            _importGlobalTabParam.IMPORTFIELDOPTIONSTYPE.MERGEDATA :
                                            _importGlobalTabParam.IMPORTFIELDOPTIONSTYPE.KEEPDATA, Value: !checked
                                    });
                                    setAttributeValue(ele, 'bListner', '1');
                                }, false);
                            }

                        });
                    }

                    //Ajouter un listener sur document pour masquer tous les menus options catalogue
                    document.addEventListener('click', function (event) {
                        if (typeof event.target !== 'undefined' &&
                            !event.target.matches('.dropbtn')
                            && !event.target.matches('.dropdown-content')
                            && !event.target.matches('.chk')
                            && !event.target.matches('span')) {
                            _wizardInternal.HideFieldOption(null);
                        }

                    }, false);

                    break;
                default:
                    break;
            }
        },

        SaveImportTemplate: function (saveAs, bEmptyName, saveTemplateImport) {
            var currentStep = this.GetCurrentStep();
            if (currentStep <= 1 || !saveTemplateImport)
                return;
            var templateName = '';
            var doc = null;
            if (top.oImportWizard.Wizard.ImportTemplateNewInternal != null && typeof top.oImportWizard.Wizard.ImportTemplateNewInternal != 'undefined' && top.oImportWizard.Wizard.ImportTemplateNewInternal.getIframe() != null)
                doc = top.oImportWizard.Wizard.ImportTemplateNewInternal.getIframe().document;

            if (doc != null && typeof doc != 'undefined') {
                var templateDiv = doc.querySelector('#PermName');
                if (templateDiv.tagName == 'SPAN')
                    templateName = doc.querySelector('#PermName').innerText;
                else
                    templateName = doc.querySelector('#PermName').value;
            }

            if (!bEmptyName && (typeof templateName == 'undefined' || templateName == null || trim(templateName) == ''))
                eAlert(0, top._res_6524, top._res_8725);
            else {
                if (this.GetCurrentStep() == this.STEP.OPTIONS) {
                    if (this.ValidImportOptionsParams())
                        oImportTemplateWizardManager.Wizard.SaveImportTemplate(templateName, saveAs, bEmptyName);
                }
                else
                    oImportTemplateWizardManager.Wizard.SaveImportTemplate(templateName, saveAs, bEmptyName);
            }

        },

        SetPermParams: function (permMode, param, value) {
            if (permMode == this.MODE.VIEW)
                this._aViewPerm[param] = value;
            else if (permMode == this.MODE.UPDATE)
                this._aUpdatePerm[param] = value;

            this.UpdateMode(permMode);

        },
        UpdateMode: function (permCode) {
            if (permCode == this.MODE.VIEW) {
                if (parseInt(this._aViewPerm.level) > 0
                    && this._aViewPerm.user != "") {
                    this._aViewPerm.mode = "2";
                }
                else if (parseInt(this._aViewPerm.level) > 0 && this._aViewPerm.user == "") {
                    this._aViewPerm.mode = "0";
                }
                else if ((parseInt(this._aViewPerm.level) <= 0 || isNaN(parseInt(this._aViewPerm.level))) && this._aViewPerm.user != "") {
                    this._aViewPerm.mode = "1"
                }
                else {
                    this._aViewPerm.mode = "-1";
                }
            }
            else if (permCode == this.MODE.UPDATE) {
                if (parseInt(this._aUpdatePerm.level) > 0
                    && this._aUpdatePerm.user != "") {
                    this._aUpdatePerm.mode = "2";
                }
                else if (parseInt(this._aUpdatePerm.level) > 0 && this._aUpdatePerm.user == "") {
                    this._aUpdatePerm.mode = "0";
                }
                else if ((parseInt(this._aUpdatePerm.level) <= 0 || isNaN(parseInt(this._aUpdatePerm.level))) && this._aUpdatePerm.user != "") {
                    this._aUpdatePerm.mode = "1"
                }
                else {
                    this._aUpdatePerm.mode = "-1";
                }
            }
        },
        //Demande #79 020: mettre ajour le nom de fichier sélectionné
        UpdateSelcetedFileLabel: function (input) {
            if (input.files && input.files[0]) {
                document.getElementById('lbl_import_file').innerHTML = input.files[0].name;
            }
        }
    };
})();

function showDebugWindow() {

    var Modal = new eModalDialog("DEBUG Window", 0, "eImportProgressDebug.aspx", 1200, 800);
    Modal.ErrorCallBack = function () {
        setWait(false);
        Modal.hide();
    };
    Modal.hideMaximizeButton = true;
    Modal.show();
    Modal.AddCancelMethode(function () {
        setWait(false);
        Modal.hide();
    });

}