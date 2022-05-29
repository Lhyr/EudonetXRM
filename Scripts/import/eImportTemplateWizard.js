/// <summary>
/// Objet permettant de gérer les étapes de l'assistant d'import depuis l'exterieur du wizard
/// </summary>
var oImportTemplateWizardManager = {

    ACTION: {
        'Rename': 1,
        'Delete': 2,
        'GetDesc': 3,
        'Save': 4,
        'ShowSave': 5
    },
    ImportTemplateLine: {
        TemplateId: 0,
        TemplateName: '',
        TemplateParentElement: '',
        GetNewTemplate: function () {
            this.TemplateName = '';
            this.TemplateId = 0;
        },
        SetImportLine: function (importElement) {
            var td = importElement.parentElement.parentElement;
            this.TemplateId = GetFieldFileId(td.id);
            this.TemplateName = td.innerText;
            this.TemplateParentElement = td.parentElement;
        }
    }
    ,
    Wizard: {
        /// <summary>
        /// Référence à la modal
        /// </summary>
        Modal: null,
        Event: null,
        TimeOut: null,
        /// <summary>
        /// Afficher la modal des modèles d'import
        /// </summary>
        Show: function (importTemplateId) {
            var size = getWindowSize();
            var nPopupWidth = 927;
            var bTabletMode = isTablet();
            if (bTabletMode) {
                if (Number(size.w) < 1024)
                    nPopupWidth = 750;
            }
            var that = this;
            this.Modal = new eModalDialog(top._res_8685, 0, 'eFilterReportListDialog.aspx', nPopupWidth, 600, 'ImportTemplateWizard');
            this.Modal.ErrorCallBack = launchInContext(this.Modal, this.Modal.hide);
            this.Modal.addParam("width", nPopupWidth, "post");
            this.Modal.addParam("height", 600, "post");
            this.Modal.addParam("tab", oImportWizardInternal.GetImportTab(), "post");
            this.Modal.addParam("frmId", this.Modal.iframeId, "post");
            this.Modal.addParam("activeImportTemplate", importTemplateId, "post");
            this.Modal.addParam("lstType", 26, "post");
            this.Modal.onIframeLoadComplete = function () { top.oImportWizard.Wizard.Modal.getIframe().oImportWizardInternal.ImportTemplateInternal = oImportTemplateWizardManager; }
            this.Modal.ErrorCallBack = function () { setWait(false); oImportTemplateWizardManager.Wizard.Close(); };
            this.Modal.show();
            //oModalReportList.addButtonFct(top._res_29, onCloseReportList, 'button-gray');
            //oModalReportList.addButtonFct(top._res_219, onApplySelectedReport, 'button-green');
            this.Modal.addButtonFct(top._res_29, that.Close, 'button-gray');
            this.Modal.addButtonFct(top._res_219, that.ApplaySelectedTemplateParams, 'button-green');
            this.Modal.hideMaximizeButton = true;
            // Référence à la modal depuis la page princiaple
            //top.window["_md"]["ImportTemplateWizard"] = this.Modal;
        },

        // <summary>
        /// Férmer la modal en cas d'erreur
        /// </summary>
        Close: function () {
            oImportTemplateWizardManager.Wizard.Modal.hide();
        },

        /// <summary>
        /// Affecte les paramètres du modèle d'import au wizard
        /// </summary>
        ApplaySelectedTemplateParams: function () {
            var tr = oImportTemplateWizardManager.Wizard.Modal.getIframe().document.querySelector('tr[class*="eSel"]');
            var templateId = getAttributeValue(tr, 'mtid');
            oImportTemplateWizardManager.Wizard.Close();
            if (trim(templateId) != '')
                oImportWizardInternal.ReloadCurrentStep(oImportWizardInternal.STEP.MAPPING, templateId);

        },
        /// <summary>
        /// Sélectionner un modèlele d'import
        /// </summary>
        SelectImportTemplate: function (elem) {
            var importTemplateTab = this.Modal.getIframe().document.getElementById("mt_" + TAB_IMPORTTEMPLATE);
            oTrs = importTemplateTab.querySelectorAll("tr[eid^='" + TAB_IMPORTTEMPLATE + "']");
            for (var j = 0; j < oTrs.length; j++) {
                if (oTrs[j] == elem) {
                    addClass(oTrs[j], 'eSel');
                } else
                    removeClass(oTrs[j], 'eSel');
            }
        },
        /// <summary>
        /// Renommer un modèlele d'import
        /// </summary>
        RenameImportTemplate: function (elem, id, value) {
            try {
                var upd = new eUpdater('mgr/import/eImportTemplateWizardManager.ashx', 1);
                upd.addParam("action", oImportTemplateWizardManager.ACTION.Rename, "post");
                upd.addParam("importtemplatename", value, "post");
                upd.addParam("importtemplateid", id, "post");

                upd.ErrorCallBack = function (oRes) { console.log(oRes); };
                upd.send(function (oRes) {
                    var result = JSON.parse(oRes);
                    if (result.Success) {
                        elem.lastChild.nodeValue = result.Html;
                        [].map.call(document.querySelectorAll('.resume_import_template_name'), function (ele) {
                            ele.innerText = result.Html;
                        });
                    }
                    else
                        eAlert(0, top._res_72, result.Html);

                });
            } catch (e) {
                eAlert(0, top._res_72, top._res_6172);
                console.log(e);
            }

        },
        /// <summary>
        /// Afficher la déscription d'un modèlele d'import
        /// </summary>
        ShowDescription: function (event, templateId) {
            try {
                this.Event = event;
                var upd = new eUpdater('mgr/import/eImportTemplateWizardManager.ashx', 1);
                upd.addParam("action", oImportTemplateWizardManager.ACTION.GetDesc, "post");
                upd.addParam("importtemplateid", templateId, "post");
                upd.addParam("importmodelparams", JSON.stringify(oImportWizardInternal.GetImportParams()), "post");
                upd.ErrorCallBack = function (oRes) { console.log(oRes); };
                oImportTemplateWizardManager.Wizard.TimeOut = setTimeout(function () {
                    upd.send(function (oRes) {
                        var doc = oImportTemplateWizardManager.Wizard.Modal.getIframe().document;
                        var templateEvent = oImportTemplateWizardManager.Wizard.Event;
                        var result = JSON.parse(oRes);
                        if (result.Success) {
                            var sDesc = result.Html;
                            if (sDesc.length == 0) {
                                ht(doc);
                                return;
                            }

                            st(templateEvent, sDesc, "importTemplateToolTip", doc);
                        }
                        else
                            ht(doc);
                    })
                }, 100);
            } catch (e) {
                eAlert(0, top._res_72, top._res_6172);
                console.log(e);
            }
        },
        /// <summary>
        /// Afficher la déscription d'un modèlele d'import
        /// </summary>
        SaveImportTemplate: function (templateName, saveAs, bEmptyName) {
            try {

                var templateId = (saveAs || bEmptyName ? 0 : oImportWizardInternal.GetImportTemplateId());
                var upd = new eUpdater('mgr/import/eImportTemplateWizardManager.ashx', 1);
                upd.addParam("action", oImportTemplateWizardManager.ACTION.Save, "post");
                upd.addParam("maintab", oImportWizardInternal.GetImportTab(), "post");
                upd.addParam("importtemplateid", templateId, "post");
                upd.addParam("importtemplatename", templateName, "post");
                upd.addParam("importmodelparams", JSON.stringify(oImportWizardInternal.GetImportParams()), "post");
                upd.addParam("saveas", (saveAs ? '1' : '0'), "post");


                if (!bEmptyName) {
                    var oWaitDialog = showWaitDialog(top._res_8197, top._res_1120);
                    this.UpdatePermission();
                    var bViewPerm = (getAttributeValue(top.oImportWizard.Wizard.ImportTemplateNewInternal.getIframe().document.getElementById("chk_OptViewFilter"), 'chk') == "1");
                    var bUpdatePerm = (getAttributeValue(top.oImportWizard.Wizard.ImportTemplateNewInternal.getIframe().document.getElementById("chk_OptUpdateFilter"), 'chk') == "1");
                    var bpublic = (getAttributeValue(top.oImportWizard.Wizard.ImportTemplateNewInternal.getIframe().document.getElementById("chk_OptPublicFilter"), 'chk') == "1");

                    upd.addParam("bpublic", (bpublic ? '1' : '0'), "post");
                    upd.addParam("viewperm", (bViewPerm ? '1' : '0'), "post");
                    upd.addParam("updateperm", (bUpdatePerm ? '1' : '0'), "post");

                    if (bViewPerm) {
                        upd.addParam("viewpermmode", oImportWizardInternal._aViewPerm.mode, "post");
                        upd.addParam("viewpermusersid", oImportWizardInternal._aViewPerm.user, "post");
                        upd.addParam("viewpermlevel", oImportWizardInternal._aViewPerm.level, "post");
                    }
                    if (bUpdatePerm) {
                        upd.addParam("updatepermmode", oImportWizardInternal._aUpdatePerm.mode, "post");
                        upd.addParam("updatepermusersid", oImportWizardInternal._aUpdatePerm.user, "post");
                        upd.addParam("updatepermlevel", oImportWizardInternal._aUpdatePerm.level, "post");
                    }
                }

                upd.ErrorCallBack = function (oRes) {
                    if (oWaitDialog != null && typeof oWaitDialog != 'undefined')
                        oWaitDialog.hide();
                    console.log(oRes);
                };

                upd.send(function (oRes) {
                    var result = JSON.parse(oRes);
                    if (result.Success) {
                        if (result.Params.ImportTemplateName != '') {
                            var divButton = top.oImportWizard.Wizard.Modal.getDivButton();
                            var importTemplate = oImportWizardInternal.GetImportTemplate();
                            var originId = (importTemplate.ImportTemplateId > 0 ? importTemplate.ImportTemplateId : oImportWizardInternal.GetImportTemplateId());
                            var originLibelle = importTemplate.ImportTemplateName;
                            var addClass = false;

                            if (originId != result.Params.ImportTemplateId) {
                                var templateClass = '.resume_import_template_name';
                                if (isNaN(parseInt(originId)) || originId == 0 || originLibelle == '') {
                                    templateClass = '.import_template_name';
                                    addClass = true;
                                }

                                var templateNameList = document.querySelectorAll(templateClass);
                                oImportWizardInternal.SetImportTemplateParams(result.Params);
                                [].map.call(templateNameList, function (ele) {
                                    ele.innerText = result.Params.ImportTemplateName;
                                    if (addClass) {
                                        setAttributeValue(ele, 'class', 'resume_import_template_name');
                                        setAttributeValue(ele, 'title', top._res_8707);
                                        ele.style.cursor = 'pointer';
                                        ele.addEventListener("click", function (e) {
                                            stopEvent(e);
                                            oImportWizardInternal.ResetImportTemplate(ele);

                                        }, false);
                                    }
                                });
                            }

                            if (result.Params.IsUpdatable == false)
                                divButton.querySelector('div[id="savemodel_btn"]').style.display = 'none';
                            else
                                divButton.querySelector('div[id="savemodel_btn"]').style.display = 'inline';

                            if (result.Params.ImportTemplateId == 0)
                                divButton.querySelector('div[id="savemodelas_btn"]').style.display = 'none';
                            else
                                divButton.querySelector('div[id="savemodelas_btn"]').style.display = 'inline';

                            if (oWaitDialog != null && typeof oWaitDialog != 'undefined')
                                oWaitDialog.hide();
                            top.oImportWizard.Wizard.ImportTemplateNewInternal.hide();
                            eAlert(4, top._res_6381, top._res_221);
                        }

                    } else {
                        if (oWaitDialog != null && typeof oWaitDialog != 'undefined')
                            oWaitDialog.hide();
                        console.log(result);
                        if (top.oImportWizard.Wizard.ImportTemplateNewInternal != null)
                            top.oImportWizard.Wizard.ImportTemplateNewInternal.hide();

                    }


                });
            } catch (e) {
                eAlert(0, top._res_72, top._res_6172);
                console.log(e);
            }
        },
        /// <summary>
        /// Supprimer un modèlele d'import
        /// </summary>
        DeleteImportTemplate: function () {
            try {
                var that = this;
                var templateId = oImportTemplateWizardManager.ImportTemplateLine.TemplateId;
                //var templateId = GetFieldFileId(elem.parentElement.parentElement.id);
                var upd = new eUpdater('mgr/import/eImportTemplateWizardManager.ashx', 1);
                if (typeof templateId != 'undefined' && !isNaN(parseInt(templateId))) {
                    upd.addParam("action", oImportTemplateWizardManager.ACTION.Delete, "post");
                    upd.addParam("importtemplateid", templateId, "post");
                    upd.addParam("maintab", oImportWizardInternal.GetImportTab(), "post");
                    upd.ErrorCallBack = function (oRes) { console.log(oRes); };
                    oImportTemplateWizardManager.Wizard.TimeOut = setTimeout(function () {
                        upd.send(function (oRes) {
                            var doc = oImportTemplateWizardManager.Wizard.Modal.getIframe().document;
                            var templateEvent = oImportTemplateWizardManager.Wizard.Event;
                            var result = JSON.parse(oRes);
                            if (result.Success) {
                                var divButton = top.oImportWizard.Wizard.Modal.getDivButton();
                                var currentImportTemplateId = oImportWizardInternal.GetImportTemplateId();
                                var tr = oImportTemplateWizardManager.ImportTemplateLine.TemplateParentElement;
                                tr.parentElement.removeChild(tr);
                                if (currentImportTemplateId == oImportTemplateWizardManager.ImportTemplateLine.TemplateId) {
                                    oImportWizardInternal.SetImportTemplateId(0);
                                    var wizardElementTemplateName = document.querySelector('label[class="resume_import_template_name"]');
                                    if (typeof (wizardElementTemplateName) != 'undefined' && wizardElementTemplateName != null) {
                                        wizardElementTemplateName.innerText = top._res_1111;
                                        setAttributeValue(wizardElementTemplateName, 'class', 'import_template_name');
                                        wizardElementTemplateName.removeAttribute('style');
                                        wizardElementTemplateName.removeAttribute('onclick');
                                        setAttributeValue(wizardElementTemplateName, 'title', top._res_1111);
                                        divButton.querySelector('div[id="savemodel_btn"]').style.display = 'none';
                                    }

                                }
                                eAlert(4, top._res_6381, top._res_221);
                            }
                            else
                                eAlert(0, top._res_72, result.Html);

                            clearTimeout(oImportTemplateWizardManager.Wizard.TimeOut);
                        })
                    }, 100);
                } else
                    eAlert(0, top._res_72, top._res_6172);

            } catch (e) {
                eAlert(0, top._res_72, top._res_6172);
                console.log(e);
            }
        },
        UpdatePermission: function () {

            var childwindow = top.oImportWizard.Wizard.ImportTemplateNewInternal.getIframe();
            var objRetValue = getPermReturnValue("View", childwindow);
            oImportWizardInternal.SetPermParams(oImportWizardInternal.MODE.VIEW, "level", objRetValue.levels)
            oImportWizardInternal.SetPermParams(oImportWizardInternal.MODE.VIEW, "user", objRetValue.users);
            oImportWizardInternal.SetPermParams(oImportWizardInternal.MODE.VIEW, "mode", objRetValue.perMode);
            var objRetValue = getPermReturnValue("Update", childwindow);
            oImportWizardInternal.SetPermParams(oImportWizardInternal.MODE.UPDATE, "level", objRetValue.levels);
            oImportWizardInternal.SetPermParams(oImportWizardInternal.MODE.UPDATE, "user", objRetValue.users);
            oImportWizardInternal.SetPermParams(oImportWizardInternal.MODE.UPDATE, "mode", objRetValue.perMode);
        }
    },

    /// <summary>
    /// Permet d'ouvrir la liste des modèles d'import
    /// </summary>
    ShowImportTemplateListWizard: function () {
        var importTemplateId = oImportWizardInternal.GetImportTemplateId();
        oImportTemplateWizardManager.Wizard.Show(importTemplateId);
    },
    RenameTemplate: function (elem, id, value) {
        oImportTemplateWizardManager.Wizard.RenameImportTemplate(elem, id, value);
    },
    SelectTemplate: function (elem) {
        oImportTemplateWizardManager.Wizard.SelectImportTemplate(elem);
    },
    ApplayTemplateParams: function () {
        var params = oImportWizardInternal.GetImportParams();
        if (typeof params != 'undefined' && params != null && params.Tables.length > 0) {
            top.eConfirm(eMsgBoxCriticity.MSG_INFOS, top._res_6343, top._res_8722, '', 600, 200, oImportTemplateWizardManager.Wizard.ApplaySelectedTemplateParams, '', false, true);
        } else
            oImportTemplateWizardManager.Wizard.ApplaySelectedTemplateParams();
    },
    ShImportTemplateDesc: function (event, elem) {
        oImportTemplateWizardManager.Wizard.ShowDescription(event, GetFieldFileId(elem.parentElement.parentElement.id));
    },
    DeleteTemplate: function (event, elem) {
        oImportTemplateWizardManager.ImportTemplateLine.SetImportLine(elem);
        top.eConfirm(eMsgBoxCriticity.MSG_CRITICAL, top._res_6343, top._res_8721, '', 600, 200, oImportTemplateWizardManager.Wizard.DeleteImportTemplate, '', false, true);

    },
    ClearTimeOut: function () {
        clearTimeout(oImportTemplateWizardManager.Wizard.TimeOut);
        ht(this.Wizard.Modal.getIframe().document);
    }


}
