using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Xml;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.mgr
{

    /// <summary>
    /// Manager qui traite le formulaire
    /// </summary>
    public class eFormularManager : eEudoManager
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
            /// dupliquer le formulaire
            /// </summary>
            CLONE = 8
        }



        //Gestion des erreurs
        eFormularException formExeption = null;


        //Objet eFormular à traiter
        eFormular _oFormular = null;

        //l'operation demandée
        Operation _operation = Operation.NONE;

        /// <summary>Le modèle de formulaire est-il public ?</summary>
        bool _bPublic = false;

        /// <summary>
        /// Gestion des traitements
        /// </summary>
        protected override void ProcessManager()
        {
            try
            {
                RetrieveParams();
                RunOperation();
            }
            catch (eFormularException ex)
            {
                formExeption = ex;
            }

            RenderXmlResponse();
        }

        /// <summary>
        /// Récuperer les params de QueryString
        /// </summary>
        private void RetrieveParams()
        {

            #region Type d'operation

            if (_context.Request.Form.AllKeys.Contains("operation") && _context.Request.Form["operation"] != null)
                Enum.TryParse(_context.Request.Form["operation"].ToString().ToUpper(), out _operation);
            #endregion

            #region Paramètres du formulaire

            Int32 id = 0;
            if (_context.Request.Form.AllKeys.Contains("id") && _context.Request.Form["id"] != null)
                id = eLibTools.GetNum(_context.Request.Form["id"].ToString());


            _oFormular = eFormular.RetrieveParams(id, _pref, _context.Request.Form);

            #endregion

            #region Permissions

            #region View

            //  && _context.Request.Form["viewperm"].ToString().Equals("1")
            if (_context.Request.Form.AllKeys.Contains("viewperm"))
            {
                Int32 viewPermId = 0;
                if (_context.Request.Form.AllKeys.Contains("viewpermid") && _context.Request.Form["viewpermid"] != null)
                    viewPermId = eLibTools.GetNum(_context.Request.Form["viewpermid"].ToString());

                _oFormular.ViewPerm = new ePermission(viewPermId, _pref);

                if (_context.Request.Form.AllKeys.Contains("viewpermmode") && _context.Request.Form["viewpermmode"] != null)
                    _oFormular.ViewPerm.PermMode = (ePermission.PermissionMode)eLibTools.GetNum(_context.Request.Form["viewpermmode"].ToString());

                if (_context.Request.Form.AllKeys.Contains("viewpermusersid") && _context.Request.Form["viewpermusersid"] != null)
                    _oFormular.ViewPerm.PermUser = _context.Request.Form["viewpermusersid"].ToString();

                if (_context.Request.Form.AllKeys.Contains("viewpermlevel") && _context.Request.Form["viewpermlevel"] != null)
                    _oFormular.ViewPerm.PermLevel = eLibTools.GetNum(_context.Request.Form["viewpermlevel"].ToString());
            }
            #endregion

            #region update

            if (_context.Request.Form.AllKeys.Contains("updateperm"))
            {
                Int32 updatePermId = 0;
                if (_context.Request.Form.AllKeys.Contains("updatepermid") && _context.Request.Form["updatepermid"] != null)
                    updatePermId = eLibTools.GetNum(_context.Request.Form["updatepermid"].ToString());

                //Avant toute operation on vérifie si l'utilisateur en cours a la permission de mise à jour
                if (_oFormular.UpdatePerm != null && !_oFormular.UpdatePerm.IsAllowed(_pref.User))
                    throw new eFormularException(eFormularException.ErrorCode.UPDATE_NOT_ALLOWED,
                             eResApp.GetRes(_pref, 6714),
                            "Dev : On devrait pas tomber sur cas sauf si on change l'id du formulaire depuis javascript");

                _oFormular.UpdatePerm = new ePermission(updatePermId, _pref);

                if (_context.Request.Form.AllKeys.Contains("updatepermmode") && _context.Request.Form["updatepermmode"] != null)
                    _oFormular.UpdatePerm.PermMode = (ePermission.PermissionMode)eLibTools.GetNum(_context.Request.Form["updatepermmode"].ToString());

                if (_context.Request.Form.AllKeys.Contains("updatepermusersid") && _context.Request.Form["updatepermusersid"] != null)
                    _oFormular.UpdatePerm.PermUser = _context.Request.Form["updatepermusersid"].ToString();

                if (_context.Request.Form.AllKeys.Contains("updatepermlevel") && _context.Request.Form["updatepermlevel"] != null)
                    _oFormular.UpdatePerm.PermLevel = eLibTools.GetNum(_context.Request.Form["updatepermlevel"].ToString());

            }
            #endregion


            #endregion
        }

        /// <summary>
        /// Execute l'action demandée 
        /// </summary>
        private void RunOperation()
        {
            switch (_operation)
            {
                case Operation.CLONE:
                    //Vérification de droit de modif coté back 
                    if (_oFormular.ViewPerm != null && !_oFormular.ViewPerm.IsAllowed(_pref.User))
                        throw new eFormularException(eFormularException.ErrorCode.VIEW_NOT_ALLOWED,
                                 eResApp.GetRes(_pref, 6714),
                                "Dev : On devrait pas tomber sur cas sauf si on change l'id du formulaire depuis javascript");
                    _oFormular.Reset(true, eLibTools.GetValidFormularName(_oFormular.Label, _pref), true);
                    if (!_oFormular.Save())
                        SendError("_oFormular.Duplicate", _oFormular.ErrorMessage);
                    break;
                case Operation.SAVE:

                    bool saveAs = _requestTools.GetRequestFormKeyB("saveas") ?? false;
                    if (saveAs)
                        _oFormular.Reset();

                    if (!_oFormular.Save())
                        SendError("_oFormular.Save", _oFormular.ErrorMessage);
                    break;
                case Operation.RENAME:
                    if (_context.Request.Form.AllKeys.Contains("label") && _context.Request.Form["label"] != null)
                    {
                        if (!_oFormular.Rename(_context.Request.Form["label"].ToString()))
                            SendError("_oFormular.Rename", _oFormular.ErrorMessage);
                    }
                    else
                    {
                        ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, "", "Vous devez saisir un nom de formulaire.");  //TODO RES
                        LaunchError();
                    }
                    break;
                case Operation.DELETE:
                    if (!_oFormular.Delete())
                        SendError("_oFormular.Delete", _oFormular.ErrorMessage);
                    break;
                case Operation.PREVIEW:
                    Preview();
                    break;
                case Operation.DELETE_SHARINGIMAGE:
                    if (!_oFormular.DeleteSharingImage(false))
                        SendError("_oFormular.DeleteSharingImage", _oFormular.ErrorMessage);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Affiche un message d'erreur stabndard à l'utilisateur et envoi le feedback avec les informations passées en paramètre
        /// </summary>
        /// <param name="sOrig">Zone ou survient l'erreur</param>
        /// <param name="sError">erreur détaillée</param>
        private void SendError(string sOrig, string sError)
        {
            ErrorContainer = eErrorContainer.GetDevUserError(
                eLibConst.MSG_TYPE.CRITICAL,
                eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                eResApp.GetRes(_pref, 422),  //  Détail : pour améliorer...
                eResApp.GetRes(_pref, 72),  //   titre
                string.Concat(sOrig, sError)
                );
            LaunchError();
        }


        /// <summary>
        /// Prévisualisation
        /// </summary>
        private void Preview()
        {
            //RendType = ExternalPageRendType.FORM_VISU;
            bool bFirstPage = true;  //Page 1 ou 2
            if (_context.Request.Form.AllKeys.Contains("step"))
                if (eLibTools.GetNum(_context.Request.Form["step"].ToString()) == 2)
                    bFirstPage = false;

            var rendu = new HtmlGenericControl("FORM");

            if (_context.Request.Form.AllKeys.Contains("redirecturl") && !string.IsNullOrEmpty(_context.Request.Form["redirecturl"].ToString()))
            {
                // Cas d'une redirection vers une page : on affiche la page dans une iframe
                HtmlGenericControl iframe = new HtmlGenericControl("iframe");
                iframe.ID = "";
                iframe.Style.Add("width", "99%");
                iframe.Style.Add("height", "95%");

                string url = _context.Request.Form["redirectUrl"].ToString();
                if (!url.ToLower().StartsWith("http"))
                {
                    url = "http://" + url;
                }
                iframe.Attributes.Add("src", url);
                rendu.Controls.Add(iframe);

                iframe = null;
            }
            else
            {
                var rend = eFormularFileRenderer.LoadPreview(eLibTools.GetAppUrl(_context.Request), _oFormular);

                rendu.Attributes.Add("onsubmit", "return false;");
                //Fait le rendu du formulaire
                rendu.Controls.Add(rend.DivGlobalParam);

                //Pour les ashx on remonte d'un cran car le repertoire actuel est mgr/            
                PageRegisters.RelativePath = "../";

                HtmlGenericControl content = new HtmlGenericControl("div");
                if (bFirstPage)
                {
                    content.InnerHtml = rend.BodyMerge;
                    if (rend.BodyCss != null)
                        PageRegisters.SetRawCss(rend.BodyCss);
                }
                else
                {
                    content.InnerHtml = rend.BodySubmissionMerge;
                    if (rend.BodySubmissionCss != null)
                        PageRegisters.SetRawCss(rend.BodySubmissionCss);
                }
                rendu.Controls.Add(content);

                if (rend.RawScript != null && rend.RawScript.Length > 0)
                    PageRegisters.RawScrip.Append(rend.RawScript.ToString());

                HtmlGenericControl scriptContainer = new HtmlGenericControl("div");
                rendu.Controls.Add(scriptContainer);
                scriptContainer.InnerHtml = rend.AppendInitScript();

                PageRegisters.AddRangeScript(rend.ListScriptToRegister);
                PageRegisters.AddRangeCss(rend.ListCssToRegister);
                PageRegisters.AddRangeCssWithPath(rend.ListCssWithPathToRegister);

                //Ajout des infos a la page du rendu pour le js
                //Param de tok... à null car en preview pas de validation.
                foreach (var ctrl in rend.GetClientContextInfo(null))
                    rendu.Controls.Add(ctrl);

                AddHeadAndBody = true;
                BodyCssClass = "bodyFormular";

            }

            RenderResultHTML(rendu);
        }


        /// <summary>
        /// Construit et renvois le xml de réponse
        /// </summary>
        private void RenderXmlResponse()
        {
            XmlDocument _xmlDocReturn = new XmlDocument();

            #region XML Declartion, UTF8 ..etc

            XmlNode baseResultNode;
            _xmlDocReturn.AppendChild(_xmlDocReturn.CreateXmlDeclaration("1.0", "UTF-8", null));
            baseResultNode = NewNode("result", _xmlDocReturn, _xmlDocReturn);

            #endregion

            #region initialisation du retour XML (structure)

            XmlNode xmlNodeSuccessNode = NewNode("success", baseResultNode, _xmlDocReturn);

            //Gestion des erreurs
            XmlNode xmlNodeCloseWizard = NewNode("closewizard", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeMessage = NewNode("message", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeDetail = NewNode("detail", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeException = NewNode("exception", baseResultNode, _xmlDocReturn);

            //Noeuds du modèle
            XmlNode xmlNodeLabel = NewNode("label", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeOperation = NewNode("operation", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeFormularId = NewNode("formularid", baseResultNode, _xmlDocReturn);

            //Permissions
            XmlNode xmlNodeViewPermId = NewNode("ViewPermId", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeUpdatePermId = NewNode("UpdatePermId", baseResultNode, _xmlDocReturn);

            #endregion

            #region remplit le xml et fait le rendu

            bool bError = formExeption != null;
            xmlNodeSuccessNode.InnerText = bError ? "0" : "1";
            xmlNodeOperation.InnerText = this._operation.ToString();

            if (bError)
            {
                // Pas de droits de visu, on envoie la commande de fermeture du wizard
                xmlNodeCloseWizard.InnerText = formExeption.Code == eFormularException.ErrorCode.VIEW_NOT_ALLOWED ? "1" : "0";
                xmlNodeMessage.InnerText = formExeption.UserMessage;
                xmlNodeDetail.InnerText = eResApp.GetRes(_pref, 6721);
                if (_pref.User.UserLevel >= UserLevel.LEV_USR_ADMIN.GetHashCode())
                    xmlNodeException.InnerText = formExeption.Message + "<br />" + formExeption.StackTrace;

            }
            else
            {
                xmlNodeCloseWizard.InnerText = "0";
                xmlNodeMessage.InnerText = eResApp.GetRes(_pref, 6381);
                xmlNodeDetail.InnerText = eResApp.GetRes(_pref, 6720);
                xmlNodeFormularId.InnerText = this._oFormular.FormularId.ToString();
                xmlNodeLabel.InnerText = _oFormular.Label;

                if (_operation == Operation.SAVE)
                {
                    XmlNode xmlNodeRewrittenURL = NewNode("rewrittenurl", baseResultNode, _xmlDocReturn);
                    XmlNode xmlNodeHiddenRewrittenURL = NewNode("hiddenrewrittenurl", baseResultNode, _xmlDocReturn);
                    XmlNode xmlNodeTwitterSharerLink = NewNode("twittersharerlink", baseResultNode, _xmlDocReturn);
                    XmlNode xmlNodeFacebookSharerLink = NewNode("facebooksharerlink", baseResultNode, _xmlDocReturn);
                    XmlNode xmlNodelinkedinSharerLink = NewNode("linkedinsharerlink", baseResultNode, _xmlDocReturn);
                    XmlNode xmlNodeSharingImage = NewNode("sharingimage", baseResultNode, _xmlDocReturn);

                    string rewrittenURL = _oFormular.GetRewrittenURL(true);
                    xmlNodeRewrittenURL.InnerText = rewrittenURL;
                    xmlNodeHiddenRewrittenURL.InnerText = _oFormular.GetRewrittenURL(false);
                    xmlNodeTwitterSharerLink.InnerText = HttpUtility.UrlEncode(eSocialNetworkTools.GetTwitterShareUrl(rewrittenURL, _oFormular.MetaTitle));
                    xmlNodeFacebookSharerLink.InnerText = HttpUtility.UrlEncode(eSocialNetworkTools.GetFacebookShareUrl(rewrittenURL));
                    xmlNodelinkedinSharerLink.InnerText = HttpUtility.UrlEncode(eSocialNetworkTools.GetLinkedInShareUrl(rewrittenURL, _oFormular.MetaTitle, _oFormular.MetaDescription));
                    xmlNodeSharingImage.InnerText = _oFormular.MetaImgURL;
                }
                else if (_operation == Operation.DELETE_SHARINGIMAGE)
                {
                    xmlNodeCloseWizard.InnerText = "1";
                    xmlNodeMessage.InnerText = string.Empty;
                    xmlNodeDetail.InnerText = string.Empty;
                    xmlNodeFormularId.InnerText = this._oFormular.FormularId.ToString();
                    xmlNodeLabel.InnerText = _oFormular.Label;

                    XmlNode xmlNodeSharingImage = NewNode("sharingimage", baseResultNode, _xmlDocReturn);
                    xmlNodeSharingImage.InnerText = _oFormular.MetaImgURL;

                    XmlNode xmlNodeCloseUploadWindow = NewNode("closeuploadwindow", baseResultNode, _xmlDocReturn);
                    if (_context.Request.Form.AllKeys.Contains("closeuploadwindow") && _context.Request.Form["closeuploadwindow"] != null)
                        xmlNodeCloseUploadWindow.InnerText = _context.Request.Form["closeuploadwindow"].ToString();
                }
                else if (_operation == Operation.CLONE)
                {
                    XmlNode xmlNodeFormularTab = NewNode("formularTab", baseResultNode, _xmlDocReturn);
                    XmlNode xmlNodeFormularParentFileId = NewNode("formularParentFileId", baseResultNode, _xmlDocReturn);
                    XmlNode xmlNodeFormularType = NewNode("formularType", baseResultNode, _xmlDocReturn);

                    xmlNodeFormularTab.InnerText = _oFormular.Tab.ToString();
                    xmlNodeFormularParentFileId.InnerText = _oFormular.EvtFileId.ToString();
                    xmlNodeFormularType.InnerText = _oFormular.FormularType == FORMULAR_TYPE.TYP_ADVANCED ? "1" : "0";
                }
            }

            RenderResult(RequestContentType.XML, delegate () { return _xmlDocReturn.OuterXml; });

            #endregion

        }

        /// <summary>
        /// Créer un nouveau noeud xml et l'ajoute au noeud parent
        /// </summary>
        /// <param name="Name">nom du noeud</param>
        /// <param name="ParentNode">Noeud parent</param>
        /// <param name="Creator">Le créateur tout puissant de noeud</param>
        /// <returns></returns>
        private XmlNode NewNode(string Name, XmlNode ParentNode, XmlDocument Creator)
        {
            XmlNode child = Creator.CreateElement(Name);
            ParentNode.AppendChild(child);
            return child;
        }

    }


}