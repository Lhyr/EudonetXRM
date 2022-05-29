using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Factories;
using Com.Eudonet.Xrm.IRISBlack.Model;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Com.Eudonet.Xrm.IRISBlack.Controllers
{
    /// <summary>
    /// Contrôleur permettant de récupérer la structure du formulaire avancé
    /// </summary>
    public class FormularManagerController : BaseController
    {
        private enum Operation
        {
            /// <summary>
            /// Aucune action, comportement par défaut si aucune action n'est transmise au Manager
            /// Ce cas ne devrait jamais arriver sans provoquer une erreur de paramètre incorrect.
            /// </summary>
            NONE = 0,
            /// <summary>Sauvegarder le formulaire</summary>
            SAVE = 1,
            /// <summary>Renommer le formulaire</summary>
            RENAME = 2,
            /// <summary>Supprimer le formulaire</summary>           
            DELETE = 5,
            /// <summary>Afficher la prévisualisation du rendu</summary>           
            PREVIEW = 6,
            /// <summary>
            /// Supprimer l'image de diffusion du formulaire
            /// </summary>
            DELETE_SHARINGIMAGE = 7,
            /// <summary>
            /// Retourne le javascript d'intégration du formulaire demandes
            /// </summary>
            INTEGRATION_SCRIPT = 8
        }

        //le tagname du boutton wordline paimenet
        private const string _btnWorldlinePaimentTagname = "extended-worldline-btn";
        //Gestion des erreurs
        eFormularException formException = null;

        //Objet eFormular à traiter
        eFormular _oFormular = null;

        //l'opération demandée
        Operation _operation = Operation.NONE;

        /// <summary>Le modèle de formulaire est-il public ?</summary>
        bool _bPublic = false;


        [HttpPost]
        public IHttpActionResult Post([FromBody] FormularAdvancedModel FormularAdvancedModel)
        {
            bool success = false;

            FormularModel formularModel = JsonConvert.DeserializeObject<FormularModel>(FormularAdvancedModel.formulardata);
            formularModel.ImageFormular = FormularAdvancedModel.imageFile;

            try
            {
                if(!CheckWordlinePaimentExtensionIsOk(formularModel))
                    throw new eFormularException(eFormularException.ErrorCode.WORLDLINE_EXTENSION_NOT_ACTIVATED,
                        eResApp.GetRes(_pref, 8789),
                       "Dev : Le formulaire contient un boutton 'wordlinepayment' or l'extension est désactivée");

                if (!string.IsNullOrEmpty(formularModel.MetaImgURL) && formularModel.ImageFormular != null)
                {
                    formularModel.MetaImgURL = FormularFactory.AddImageFormular(_pref, formularModel.MetaImgURL, formularModel.ImageFormular);
                }

                RetrieveParams(formularModel);
                success = RunOperation(formularModel.saveAs);

            }
            catch (eFormularException ex)
            {
                formException = ex;
            }

            return Ok(JsonConvert.SerializeObject(RenderResponse(success)));
        }
        // DELETE api/<controller>/5
        public override IHttpActionResult Delete(int id)
        {
            return InternalServerError(new NotImplementedException());
        }

        /// <summary>
        /// renvoie le script d'intégration d'un formulaire
        /// Génère une iframe avec l'url du formulaire
        /// </summary>
        /// <param name="tok">UID de la base</param>
        /// <param name="cs"></param>
        /// /// <param name="p"></param>
        /// <param name="tr">target de creation de la frame</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{tok:string}/{cs:string}/{p:string}/{tr:string=''}")]

        public IHttpActionResult Get(string tok, string cs, string p, string tr = "")
        {
            string sScript = "";
            try
            {
                string sURL = "";
                try
                {
                    if (string.IsNullOrEmpty(tok))
                        throw new EudoException("Token de Base UID introuvable", "Votre lien formulaire est invalide (tok).");

                    if (string.IsNullOrEmpty(cs))
                        throw new EudoException("Token de Base cs introuvable", "Votre lien formulaire est invalide (cs).");


                    if (string.IsNullOrEmpty(p))
                        throw new EudoException("Token de Base p introuvable", "Votre lien formulaire est invalide (p).");

                    LoadQueryStringForm DataParam = new LoadQueryStringForm(tok, cs, p);


                    ePref prefExternal = eExternal.GetExternalPref(eExternal.ExternalPageType.FORMULAR, tok);

                    FormularResponseModel oFormular = FormularFactory.GetFormularModel(
                            prefExternal,
                            DataParam.ParamData.FormularId,
                            DataParam.ParamData.ParentFileId);

                    sURL = oFormular.RewrittenURL;
                }
                catch (EudoException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new EudoException(ex.Message, "Impossible de générer votre formulaire", ex);
                }
                finally
                {

                }
                sScript = @"
							function displayMessage (evt) {
									switch (evt.data.action) { 
									case 'setHeight': iframe.style.height = evt.data.value+ 'px'; break; 
									default:break;
										}
							}
							window.addEventListener('message', displayMessage, false);

							var iframe = document.createElement('iframe');
								  
							iframe.src ='" + sURL + @"';
							iframe.onload = 'window.parent.scrollTo(0,0)';
							iframe.allowtransparency = 'true';
							iframe.allowfullscreen = 'true';                          
							iframe.frameborder = '0'
							iframe.style = 'min-width: 100%;min-height:580px;height:100%;border: none;';
							iframe.scrolling = 'no';";

                if (string.IsNullOrEmpty(tr))
                {
                    sScript += Environment.NewLine + @"
							var _scrptElem = document.currentScript;
							if(_scrptElem && _scrptElem.parentElement){
								_scrptElem.parentElement.appendChild(iframe)
								}
							else
								document.body.appendChild(iframe); 
					";
                }
                else
                {
                    sScript += Environment.NewLine
                            + " var tr = document.getElementById('" + tr + "')" + Environment.NewLine
                            + "if(tr) tr.appendChild(iframe)";

                }
            }
            catch (Exception e)
            {
                //TODO : 
                //En cas d'erreur affichage d'un message spécifique
                sScript = "alert('Impossible de générer votre formulaire')";

            }
            //retour en mode text brute
            return base.ResponseMessage(
                    new HttpResponseMessage()
                    {
                        Content = new StringContent(sScript)
                    }
                 );
        }


        private void RetrieveParams(FormularModel formularModel)
        {

            #region Type d'operation
            _operation = (Operation)formularModel.Operation;
            #endregion

            #region Paramètres du formulaire

            Int32 id = 0;
            id = formularModel.FormularId;

            _oFormular = FormularFactory.GetFormularFromModel(id, _pref, formularModel);

            #endregion

            #region Permissions

            #region View
            Int32 viewPermId = 0;
            viewPermId = formularModel.viewPermId;

            _oFormular.ViewPerm = new ePermission(viewPermId, _pref);

            _oFormular.ViewPerm.PermMode = (ePermission.PermissionMode)formularModel.PermMode;

            if (_oFormular.ViewPerm.PermMode == ePermission.PermissionMode.MODE_NONE)
            {
                _oFormular.ViewPerm.PermLevel = 0;
                _oFormular.ViewPerm.PermUser = string.Empty;
            }
            else
            {
                _oFormular.ViewPerm.PermUser = /*(_oFormular.ViewPerm.HasPerm && _oFormular.ViewPerm.PermUser == "") ? _pref.User.UserId.ToString() :*/ formularModel.PermUser;
                _oFormular.ViewPerm.PermLevel = formularModel.PermLevel;
            }

            #endregion

            #region update

            Int32 updatePermId = 0;
            updatePermId = formularModel.UpdatePermId;

            //Avant toute operation on vérifie si l'utilisateur en cours a la permission de mise à jour
            if (_oFormular.UpdatePerm != null && !_oFormular.UpdatePerm.IsAllowed(_pref.User))
                throw new eFormularException(eFormularException.ErrorCode.UPDATE_NOT_ALLOWED,
                         eResApp.GetRes(_pref, 6714),
                        "Dev : On devrait pas tomber sur cas sauf si on change l'id du formulaire depuis javascript");

            _oFormular.UpdatePerm = new ePermission(updatePermId, _pref);

            _oFormular.UpdatePerm.PermMode = (ePermission.PermissionMode)formularModel.UpdatePermMode;

            _oFormular.UpdatePerm.PermUser = formularModel.UpdatePermUser;

            if (_oFormular.UpdatePerm.PermMode == ePermission.PermissionMode.MODE_NONE)
            {
                _oFormular.UpdatePerm.PermLevel = 0;
                _oFormular.UpdatePerm.PermUser = string.Empty;
            }
            else
            {
                _oFormular.UpdatePerm.PermUser = /*(_oFormular.UpdatePerm.HasPerm && _oFormular.UpdatePerm.PermUser == "") ? _pref.User.UserId.ToString() :*/ formularModel.UpdatePermUser;
                _oFormular.UpdatePerm.PermLevel = formularModel.UpdatePermLevel;
            }

            #endregion

            _oFormular.FormularType = EudoQuery.FORMULAR_TYPE.TYP_ADVANCED;

            #endregion
        }

        /// <summary>
        /// Execute l'action demandée 
        /// </summary>
        private bool RunOperation(bool saveAs)
        {
            bool oprationSuccess = false;
            try
            {
                switch (_operation)
                {
                    case Operation.SAVE:

                        _oFormular.CheckValid();

                        if (saveAs)
                            _oFormular.Reset();

                        if (_oFormular.Save())
                        {

                            oprationSuccess = true;
                        }

                        break;
                    case Operation.RENAME:
                        //if (_context.Request.Form.AllKeys.Contains("label") && _context.Request.Form["label"] != null)
                        //{
                        //    if (_oFormular.Rename(_context.Request.Form["label"].ToString()))
                        //        oprationSuccess = true; ;
                        //}
                        //else
                        //{
                        //    ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, "", "Vous devez saisir un nom de formulaire.");  //TODO RES
                        //    LaunchError();
                        //}
                        break;
                    case Operation.DELETE:
                        if (_oFormular.Delete())
                            oprationSuccess = true;
                        break;
                    case Operation.PREVIEW:
                        //Preview();
                        break;
                    case Operation.DELETE_SHARINGIMAGE:
                        if (_oFormular.DeleteSharingImage(false))
                            oprationSuccess = true;
                        break;
                    default:
                        break;
                }
                return oprationSuccess;
            }
            catch
            {
                throw;
            }

        }


        /// <summary>
        /// Construit et renvoit le JSON de réponse
        /// </summary>
        private FormularResponseModel RenderResponse(bool success)
        {
            #region Initialisation du retour JSON, remplissage et rendu

            FormularResponseModel response = new FormularResponseModel();

            response.Success = success;
            //On récupére le lien du formulaire.NB: le lien est construit à partir des params du formulaire
            if (success)
            {
                response.RewrittenURL = _oFormular.GetRewrittenURL(true, "");
                response.ScriptIntegration = _oFormular.GetIntegrationScript(true, "");
            }

            bool bError = formException != null || !success;
            response.Success = !bError;
            response.Operation = this._operation.ToString();

            if (bError)
            {
                // Pas de droits de visu, on envoie la commande de fermeture du wizard
                response.CloseWizard = formException.Code == eFormularException.ErrorCode.VIEW_NOT_ALLOWED;
                response.DisableIfPublished = formException.Code == eFormularException.ErrorCode.WORLDLINE_EXTENSION_NOT_ACTIVATED;
                response.Message = formException.UserMessage;
                response.Detail = formException.UserMessageDetails ?? "";// eResApp.GetRes(_pref, 6721);

                if (_pref.User.UserLevel >= EudoQuery.UserLevel.LEV_USR_ADMIN.GetHashCode())
                    response.Exception = formException.Message + "<br />" + formException.StackTrace;
            }
            else
            {
                response.CloseWizard = false;
                response.Message = eResApp.GetRes(_pref, 2772);
                response.Detail = eResApp.GetRes(_pref, 2773);
                response.FormularId = this._oFormular.FormularId;
                //TODO: ajouter une factory pour les formulaires avancés
                response.ViewPerm = new AdvFormularPermission(_oFormular.ViewPerm);
                response.UpdatePerm = new AdvFormularPermission(_oFormular.UpdatePerm);
                response.FormularName = _oFormular.Label;
                response.FormularLang = _oFormular.Lang;
            }

            #endregion
            return response;
        }

        /// <summary>
        /// retourne les données d'un formulaire à partir de son Id
        /// </summary>
        /// <param name="formularId"></param>
        /// <param name="parentFileId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{formularId}")]
        public IHttpActionResult Get(int formularId, int parentFileId)
        {
            try
            {
                //#tâche #2 752: KJE, on ajoute un test sur les droits de modif pour s'assurer que l'utilisateur a bien les droits de modif sur les formulaires
                if (formularId != 0)
                {
                    eRightFormular RightManager = new eRightFormular(_pref);
                    if (RightManager != null && !RightManager.HasRight(eLibConst.TREATID.UPDATE_FORMULAR))
                    {
                        return Ok(JsonConvert.SerializeObject(new FormularResponseModel
                        {
                            FormularId = formularId,
                            Success = false
                        }));
                    }
                }


                //on retourne l'objet en format json
                return Ok(JsonConvert.SerializeObject(FormularFactory.GetFormularModel(_pref, formularId, parentFileId)));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

        }


        [HttpGet]
        [Route("{Name}")]
        public HttpResponseMessage Get(string Name)
        {
            eLibConst.FOLDER_TYPE datasFolderType = eLibConst.FOLDER_TYPE.FILES;
            String filePath = "";
            MemoryStream ms = null;
            HttpResponseMessage httpResponseMessage = null;

            try
            {
                filePath = String.Concat(eModelTools.GetPhysicalDatasPath(datasFolderType, _pref), @"\");
                Byte[] binaryImage = System.IO.File.ReadAllBytes(filePath + Name);
                ms = new MemoryStream(binaryImage);

                httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
                httpResponseMessage.Content = new StreamContent(ms);
                httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/*");

            }
            catch (Exception ex)
            {
                throw new Exception();
            }

            return httpResponseMessage;

        }

        /// <summary>
        /// check si le formulaire contient un boutton wordline paiment et l'extension est désactivée
        /// </summary>
        /// <param name="formularModel"></param>
        /// <returns></returns>
        private bool CheckWordlinePaimentExtensionIsOk(FormularModel formularModel)
        {
            if (formularModel.Body.Contains(_btnWorldlinePaimentTagname) && !eExtension.IsReadyStrict(_pref, "WORLDLINECORE", true))
                return false;
            else
                return true;
        }
    }
}
