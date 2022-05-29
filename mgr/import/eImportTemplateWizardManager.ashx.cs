using Com.Eudonet.Internal;
using Com.Eudonet.Internal.Import;
using Com.Eudonet.Xrm.import;
using Com.Eudonet.Xrm.renderer;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web;
using static Com.Eudonet.Internal.Import.ImportTemplateWizard;

namespace Com.Eudonet.Xrm.mgr.import
{
    /// <summary>
    /// Description résumée de eImportTemplateWizardManager
    /// </summary>
    public class eImportTemplateWizardManager : eEudoManager
    {

        /// <summary>
        /// Gestion des modèles d'import
        /// </summary>
        protected override void ProcessManager()
        {
            ImportTemplateWizard importTemplate;
            eImportWizardJsonResult result;
            IMPORT_TEMPLATE_ACTION importAction;
            ImportParams importModelParams = new ImportParams();
            string error = String.Empty;

            #region Variables du post
            string action = _requestTools.GetRequestFormKeyS("action") ?? "0";
            importAction = (IMPORT_TEMPLATE_ACTION)Enum.Parse(typeof(IMPORT_TEMPLATE_ACTION), action);
            int mainTab = _requestTools.GetRequestFormKeyI("maintab") ?? 0;

            int importTemplateId = _requestTools.GetRequestFormKeyI("importtemplateid") ?? 0;
            string importTemplateName = _requestTools.GetRequestFormKeyS("importtemplatename") ?? "";


            #endregion

            #region Informations pour la sauvegarde du modèle d'import
            string sImportModelParams = _requestTools.GetRequestFormKeyS("importmodelparams") ?? String.Empty;
            bool saveAs = _requestTools.GetRequestFormKeyB("saveas") ?? false;

            int importModelUserId = _pref.User.UserId;
            // UserId = 0 : Modèle public
            // UserId = -1 : On reprend le userid courant (modification d'un modèle public vers un modèle non public)
            string reqUserId = _requestTools.GetRequestFormKeyS("userid");
            if (reqUserId != null && reqUserId != "-1")
                importModelUserId = eLibTools.GetNum(reqUserId);
            bool isPublic = _requestTools.GetRequestFormKeyB("bpublic") ?? false;
            if (!string.IsNullOrEmpty(sImportModelParams))
                importModelParams = SerializerTools.JsonDeserialize<ImportParams>(sImportModelParams);
            #endregion


            switch (importAction)
            {
                #region VALID Template Import
                case IMPORT_TEMPLATE_ACTION.SAVE:
                    result = new eImportWizardJsonResult();
                    importTemplate = new ImportTemplateWizard(_pref, importTemplateId);
                    if (mainTab > 0 && importTemplate.ImportTemplateLine.ImportTemplateTab == 0)
                        importTemplate.ImportTemplateLine.ImportTemplateTab = mainTab;
                    if ((importTemplateId > 0 && !string.IsNullOrEmpty(importTemplateName)) || (string.IsNullOrEmpty(importTemplateName) && importTemplateId == 0))
                    {
                        ImportTemplateWizard.Load(_pref, importTemplate, out error);
                        if (!saveAs)
                        {
                            if (_pref.User.UserLevel <= (int)UserLevel.LEV_USR_ADMIN && importTemplate.ImportTemplateLine.ImportTemplateId != 0)
                                importModelParams.DisableAutomatismsORM = importTemplate.ImportTemplateLine.ImportParams.DisableAutomatismsORM;
                            
                            importTemplate.ImportTemplateLine.ImportParams = importModelParams;
                        }                            
                    }
                    else
                    {
                        importTemplate.ImportTemplateLine.ImportTemplateId = 0;
                        importTemplate.ImportTemplateLine.ImportTemplateName = importTemplateName;
                        importTemplate.ImportTemplateLine.ImportParams = importModelParams;
                        importTemplate.ImportTemplateLine.ImportTemplateTab = mainTab;
                        importTemplate.ImportTemplateLine.UserId = 0;
                    }

                    if (string.IsNullOrEmpty(error))
                    {
                        importTemplate.ImportTemplateLine.ImportTemplateLastModified = DateTime.Now;
                        if (saveAs && !string.IsNullOrEmpty(importTemplateName))
                        {
                            importTemplate.ImportTemplateLine.ImportTemplateId = 0;
                            importTemplate.ImportTemplateLine.ImportTemplateName = importTemplateName;
                            importTemplate.ImportTemplateLine.ImportParams = importModelParams;
                        }



                        ValidImportTemplate(importTemplate, isPublic, saveAs, result);

                        RenderResult(RequestContentType.TEXT, delegate () { return SerializerTools.JsonSerialize(result); });
                    }
                    else
                    {
                        this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), eResApp.GetRes(_pref, 8709), devMsg: error);
                        LaunchError();
                    }

                    break;
                #endregion

                #region RENAME
                case IMPORT_TEMPLATE_ACTION.RENAME:
                    result = new eImportWizardJsonResult();
                    if (importTemplateId == 0 || string.IsNullOrEmpty(importTemplateName))
                    {
                        this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), eResApp.GetRes(_pref, 6524));
                        LaunchError();
                    }
                    else
                    {
                        RenameImportTemplate(importTemplateId, importTemplateName, result);
                        //Retourne le flux XML
                        RenderResult(RequestContentType.TEXT, delegate () { return SerializerTools.JsonSerialize(result); });
                    }

                    break;
                #endregion

                #region GETDESCRIPTION
                case IMPORT_TEMPLATE_ACTION.GESTDESCRIPTION:
                    result = new eImportWizardJsonResult();

                    //récupère la description
                    GetDescription(importTemplateId, importModelParams, result);

                    //Retourne le flux Json
                    RenderResult(RequestContentType.TEXT, delegate () { return SerializerTools.JsonSerialize(result); });
                    break;
                #endregion

                #region DELETE
                case IMPORT_TEMPLATE_ACTION.DELETE:
                    result = new eImportWizardJsonResult();

                    //supprime la description
                    DeleteImportTemplate(importTemplateId, mainTab, result);

                    //Retourne le flux Json
                    RenderResult(RequestContentType.TEXT, delegate () { return SerializerTools.JsonSerialize(result); });
                    break;
                #endregion

                case IMPORT_TEMPLATE_ACTION.SHOWSAVEMODAL:
                    AddHeadAndBody = true;
                    PageRegisters.RegisterFromRoot = true;
                    PageRegisters.AddCss("ePerm");
                    PageRegisters.AddCss("eControl");
                    PageRegisters.AddCss("eFilterwizard");
                    PageRegisters.AddScript("eTools");
                    PageRegisters.AddScript("ePerm");
                    PageRegisters.AddScript("eModalDialog");
                    PageRegisters.AddScript("eUpdater");
                    ePermission.PermissionMode viewPermMode = (ePermission.PermissionMode)(_requestTools.GetRequestFormKeyI("viewpermmode") ?? 0);
                    String viewPermUsersId = _requestTools.GetRequestFormKeyS("viewpermusersid") ?? string.Empty;
                    int viewPermLevel = _requestTools.GetRequestFormKeyI("viewpermlevel") ?? 0;
                    bool bViewPerm = _requestTools.GetRequestFormKeyB("viewperm") ?? false;
                    bool bSaveAs = _requestTools.GetRequestFormKeyB("saveas") ?? false;
                    ePermission.PermissionMode updatePermMode = (ePermission.PermissionMode)(_requestTools.GetRequestFormKeyI("updatepermmode") ?? 0);
                    String updatePermUsersId = _requestTools.GetRequestFormKeyS("updatepermusersid") ?? string.Empty;
                    int updatePermLevel = _requestTools.GetRequestFormKeyI("updatepermlevel") ?? 0;
                    bool bPublic = _requestTools.GetRequestFormKeyB("bpublic") ?? false;
                    result = new eImportWizardJsonResult();
                    ImportTemplateWizard template = null;
                    if (importTemplateId > 0)
                    {
                        template = new ImportTemplateWizard(_pref, importTemplateId);
                        ImportTemplateWizard.Load(_pref, template, out result.ErrorMsg);
                        bPublic = (template.ImportTemplateLine.UserId.HasValue && template.ImportTemplateLine.UserId.Value == 0) || template.ImportTemplateLine.UserId == null;

                        ePermission viewPerm = GetPermission(template.ImportTemplateLine.ViewPermId);
                        ePermission updatePerm = GetPermission(template.ImportTemplateLine.UpdatePermId);
                        viewPermMode = viewPerm.PermMode;
                        viewPermUsersId = viewPerm.PermUser;
                        viewPermLevel = viewPerm.PermLevel;
                        updatePermMode = updatePerm.PermMode;
                        updatePermUsersId = updatePerm.PermUser;
                        updatePermLevel = updatePerm.PermLevel;
                    }
                    else
                        template = new ImportTemplateWizard();

                    if (result.ErrorMsg.Length > 0)
                    {

                        this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), eResApp.GetRes(_pref, 8709), devMsg: result.ErrorMsg);
                        LaunchError();
                    }

                    else
                    {
                        ePermissionRenderer premRend = new ePermissionRenderer(_pref, eResApp.GetRes(_pref, 6544), template.ImportTemplateLine.ImportTemplateName, bPublic, template.ImportTemplateLine.ViewPermId, template.ImportTemplateLine.UpdatePermId, viewPermMode, viewPermUsersId, viewPermLevel, updatePermMode, updatePermUsersId, updatePermLevel, bLabelReadOnly: importTemplateId > 0 && !bSaveAs);

                        RenderResultHTML(premRend.GetSaveAsBlock());
                    }
                    break;
                default:
                    this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), eResApp.GetRes(_pref, 2024).Replace("<PARAM>", ""));
                    LaunchError();
                    break;

            }
        }


        /// <summary>
        /// Supprime un modèle d'import
        /// </summary>
        /// <param name="importTemplateId">Id du modèle à supprimer</param>
        /// <param name="tabDid">Table du modèle d'import</param>
        /// <param name="result">Résultat de retour</param>
        /// <returns></returns>
        private void DeleteImportTemplate(int importTemplateId, int tabDid, eImportWizardJsonResult result)
        {

            ImportTemplateCrudInfos delInfos = ImportTemplateWizard.DeleteImportTemplate(_pref, tabDid, importTemplateId);
            switch (delInfos.Code)
            {
                case ImportTemplateCrudInfos.ReturnCode.OK:
                    result.Success = true;
                    break;
                default:
                    // Erreur de supréssion du modèle d'import
                    this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 8689), eResApp.GetRes(_pref, 8653), title: eResApp.GetRes(_pref, 845), devMsg: delInfos.ErrorMsg);
                    this.ErrorContainer.AppendDebug = delInfos.ErrorMsg;
                    LaunchError();
                    break;
            }
        }

        /// <summary>
        /// Récupère la description d'un modèle d'import et renseigne un xml document avec cette description
        /// </summary>
        /// <param name="importTemplateId">Id du modèle d'import</param>
        /// <param name="importModelParams">Paramètres du modèle d'import</param>
        /// <param name="result">Flux de retour en Json</param>
        /// <returns>succès/echec de la récupération</returns>
        private void GetDescription(int importTemplateId, ImportParams importModelParams, eImportWizardJsonResult result)
        {
            string filterName;
            string error;
            string description = ImportTemplateWizard.GetDescription(_pref, importTemplateId, importModelParams, out filterName, out error);

            if (error.Length != 0)
            {
                this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 8697), "");
                this.ErrorContainer.AppendDetail = eResApp.GetRes(_pref, 8687);
                this.ErrorContainer.AppendDebug = error;
                LaunchError();
            }

            result.Success = true;
            result.Html = description;
        }

        /// <summary>
        /// Renomme un modèle d'import 
        ///  - Retourne vrai si le renommage a réussi
        ///  - maj le xmlResult pour renvoi vers l'html
        /// </summary>
        /// <param name="templateId">Id du template d'import</param>
        /// <param name="templateName">Nouveau Nom</param>
        /// <param name="result">flux de retour vers l'html</param>
        /// <returns>Vrai si renommage OK</returns>
        private void RenameImportTemplate(int templateId, string templateName, eImportWizardJsonResult result)
        {

            ImportTemplateCrudInfos cruInfos = ImportTemplateWizard.RenameImportTemplate(_pref, templateId, templateName);
            switch (cruInfos.Code)
            {
                case ImportTemplateCrudInfos.ReturnCode.OK:
                    result.Success = true;
                    result.Html = templateName;
                    break;
                case ImportTemplateCrudInfos.ReturnCode.FOUND:
                    result.Success = false;
                    this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), "");
                    ErrorContainer.AppendDetail = eResApp.GetRes(_pref, 8688).Replace("<NAME>", templateName);
                    LaunchError();
                    break;
                default:
                    result.Success = false;
                    this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), "");
                    ErrorContainer.AppendDetail = eResApp.GetRes(_pref, 6237);
                    LaunchError();
                    break;
            }
        }


        /// <summary>
        /// Renomme un modèle d'import 
        ///  - Retourne vrai si le renommage a réussi
        ///  - maj le xmlResult pour renvoi vers l'html
        /// </summary>
        /// <param name="importTemplate">Modèle d'import à valider</param>
        /// <param name="isPublic">Indique si le mdèle est public</param>
        /// <param name="saveAs">Enregistrer sous</param>
        /// <param name="result">flux de retour vers l'html</param>
        /// <returns>Vrai si renommage OK</returns>
        private void ValidImportTemplate(ImportTemplateWizard importTemplate, bool isPublic, bool saveAs, eImportWizardJsonResult result)
        {
            result.Success = false;
            string loadDescError = String.Empty;

            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            try
            {
                dal.OpenDatabase();

                #region ViewPermId
                int viewPermId = 0;
                bool viewPermActive = _requestTools.GetRequestFormKeyB("viewperm") ?? false;
                if (viewPermActive)
                {
                    int viewPermMode = _requestTools.GetRequestFormKeyI("viewpermmode") ?? 0;
                    if (viewPermMode != -1)
                    {
                        // #58270 : On ré-utilise le même permid que si ce n'est pas un "enregistrer sous"
                        if (!saveAs)
                            viewPermId = importTemplate.ImportTemplateLine.ViewPermId;

                        string viewPermUsersId = _requestTools.GetRequestFormKeyS("viewpermusersid") ?? String.Empty;

                        int viewPermLevel = _requestTools.GetRequestFormKeyI("viewpermlevel") ?? 0;

                        //Enregistrement dans la base
                        ePermission permView = new ePermission(viewPermId, (ePermission.PermissionMode)viewPermMode, viewPermLevel, viewPermUsersId);
                        permView.Save(dal);

                        // On renseigne la permid du modèle pour la visu
                        importTemplate.ImportTemplateLine.ViewPermId = permView.PermId;
                    }
                }
                else
                    importTemplate.ImportTemplateLine.ViewPermId = viewPermId;
                #endregion

                #region UpdatePermId
                int updatePermId = 0;
                bool updatePermActive = _requestTools.GetRequestFormKeyB("updateperm") ?? false;
                if (updatePermActive)
                {
                    int updatePermMode = _requestTools.GetRequestFormKeyI("updatepermmode") ?? 0;
                    if (updatePermMode != -1)
                    {
                        if (!saveAs)
                            updatePermId = importTemplate.ImportTemplateLine.UpdatePermId;

                        string updatePermUsersId = _requestTools.GetRequestFormKeyS("updatepermusersid") ?? String.Empty;

                        int updatePermLevel = _requestTools.GetRequestFormKeyI("updatepermlevel") ?? 0;

                        //Enregistrement dans la base
                        ePermission permUpdate = new ePermission(updatePermId, (ePermission.PermissionMode)updatePermMode, updatePermLevel, updatePermUsersId);
                        permUpdate.Save(dal);

                        // On renseigne la permid du moèle pour la modif
                        importTemplate.ImportTemplateLine.UpdatePermId = permUpdate.PermId;
                    }
                }
                else
                    importTemplate.ImportTemplateLine.UpdatePermId = updatePermId;
                #endregion

                if (saveAs) //Si enregistrer-sous le modèle on force l appartenance 
                {
                    if (!isPublic)
                        importTemplate.ImportTemplateLine.UserId = _pref.User.UserId;

                }

                if (isPublic)
                    importTemplate.ImportTemplateLine.UserId = 0;
                else
                    importTemplate.ImportTemplateLine.UserId = _pref.UserId;

                ImportTemplateCrudInfos resultIfos = importTemplate.Save(dal);
                //Recharger les informations du modèle d'import
                ImportTemplateWizard.Load(_pref, importTemplate, out loadDescError);

                switch (resultIfos.Code)
                {
                    case ImportTemplateCrudInfos.ReturnCode.OK:
                        result.Params = importTemplate.ImportTemplateLine;
                        break;
                    case ImportTemplateCrudInfos.ReturnCode.FOUND:
                        result.Success = false;
                        this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 8688).Replace("<NAME>", importTemplate.ImportTemplateLine.ImportTemplateName));
                        LaunchError();
                        break;
                    default:
                        result.Success = false;
                        this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 8720), devMsg: resultIfos.ErrorMsg);
                        LaunchError();
                        break;
                }
            }
            finally
            {
                dal?.CloseDatabase();
            }


            if (loadDescError.Length == 0)
            {
                result.Html = HttpUtility.HtmlDecode(importTemplate.GetImportModeleDescription(false, "&#13;")).ToString();
                result.Success = true;
            }
            else
            {
                result.ErrorDetail = loadDescError;
                result.ErrorMsg = eResApp.GetRes(_pref, 8687);
                result.ErrorTitle = eResApp.GetRes(_pref, 72);
            }




        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private ePermission GetPermission(int id)
        {
            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            try
            {
                dal.OpenDatabase();
                return new ePermission(id, dal, _pref);
            }
            finally
            {
                dal?.CloseDatabase();
            }
        }
    }
}