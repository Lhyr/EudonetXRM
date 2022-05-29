using Com.Eudonet.Engine.Result;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Fenêtre chargée de gérer l'assistant d'ajout d'annexes
    /// </summary>
    public partial class ePJAdd : eEudoPage
    {
        #region variables membres

        /// <summary>Message d'erreur</summary>
        private string _error = string.Empty;
        private string _sUserError = String.Empty;
        /// <summary>
        /// Tableau contenant les extentions à autoriser (à déterminer) Todo
        /// </summary>
        private List<string> _lstExtClient = new List<string>();

        /// <summary>Objet représentant une PJ</summary>
        protected ePJToAdd _myPj = new ePJToAdd();



        /// <summary>
        /// Définit l'action de l'utilisateur
        /// "init" ==> Ouverture de la fenetre d'ajout de PJ 
        /// "AddPj" ==> Retour de eMain.js pour valider l'ajout de la PJ
        /// </summary>
        protected string _sAction = string.Empty;

        #endregion

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Chargement de la page d'ajout d'annexe
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            Server.ScriptTimeout = 1000;

            #region Recuperation des variables de la fiche

            try
            {
                // Récupération des variables passées en POST
                if (Request.Form["action"] != null)
                    _sAction = Request.Form["action"];

                if (Request.Form["memoid"] != null)
                    hfMemoId.Value = Request.Form["memoid"];

                if (_sAction.Length == 0)
                    _sAction = "init";

                if (Request.Form["nfileID"] != null)
                    _myPj.FileId = eLibTools.GetNum(Request.Form["nFileID"].ToString());

                if (Request.Form["nTab"] != null)
                    _myPj.Tab = eLibTools.GetNum(Request.Form["nTab"].ToString());

                if (Request.Form["lstTypeLink"] != null)
                {
                    bool type = Request.Form["lstTypeLink"] == "1" ? true : false;
                    if (type)
                        _myPj.TypePj = 0;
                    else
                        _myPj.TypePj = 3;
                }

                if (Request.Form["SaveAs"] != null)
                {
                    _myPj.SaveAs = HttpUtility.UrlDecode(Request.Form["SaveAs"]);

                    if (!_pref.IsFullUnicode && eLibTools.ContainsNonUtf8(_myPj.SaveAs))
                        _myPj.SaveAs = eLibTools.RemoveDiacritics(_myPj.SaveAs);

                    _myPj.IsSaveAsChecked = !String.IsNullOrEmpty(_myPj.SaveAs);
                }
                if (Request.Form["txtDescLink"] != null)
                    _myPj.Description = HttpUtility.UrlDecode(Request.Form["txtDescLink"]);

                if (Request.Form["txtToolTipLink"] != null)
                    _myPj.ToolTipText = HttpUtility.UrlDecode(Request.Form["txtToolTipLink"]);

                if (Request.Form["UploadLink"] != null)
                    _myPj.UploadLink = HttpUtility.UrlDecode(Request.Form["UploadLink"]);

                string uploadMode = _requestTools.GetRequestFormKeyS("hUploadMode") ?? "0";
                _myPj.PjUploadInfo = new PJUploadInfo(_myPj.SaveAs, _myPj.SaveAs, eLibTools.GetNum(uploadMode));

                /* AJOUT DES JS & CSS */

                if (_sAction == "init")
                {
                    PageRegisters.AddScript("ePj");
                    PageRegisters.AddScript("eTools");
                    PageRegisters.AddScript("eUpdater");
                    PageRegisters.AddScript("eModalDialog");
                    PageRegisters.AddScript("eMain");
                    PageRegisters.AddScript("eList");
                }

                PageRegisters.AddCss("ePJ");
                PageRegisters.AddCss("eList");
                PageRegisters.AddCss("eTitle");
                PageRegisters.AddCss("eMain");
                PageRegisters.AddCss("eMemoEditor");
            }
            catch (Exception ex)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),
                    eResApp.GetRes(_pref, 6342),
                    eResApp.GetRes(_pref, 6522),
                    string.Concat("Récupération des variables ePjAdd.aspx - StackTrace : ", ex.Message));

                eFeedbackXrm.LaunchFeedbackXrm(ErrorContainer, _pref);
            }

            #endregion

            #region Ajout d'input à la page

            HtmlInputHidden _hInput = new HtmlInputHidden();
            _hInput.ID = "nTab";
            _hInput.Name = "nTab";
            _hInput.Value = _myPj.Tab.ToString();
            divHidden.Controls.Add(_hInput);

            _hInput = new HtmlInputHidden();
            _hInput.ID = "nfileID";
            _hInput.Name = "nfileID";
            _hInput.Value = _myPj.FileId.ToString();
            divHidden.Controls.Add(_hInput);

            #endregion

            #region MODE UPLOAD DU FICHIER EN BASE
            try
            {
                try
                {
                    if (_sAction == "AddPJ")
                        AddPjTraitments();
                }
                catch (eEndResponseException) { Response.End(); }
                catch (ThreadAbortException) { }    // Laisse passer le response.end du RenderResult
                catch (EudoException ex)
                {
                    ErrorContainer = eErrorContainer.GetErrorContainerFromEudoException(eLibConst.MSG_TYPE.CRITICAL, ex);
                    if (_bFromeUpdater)
                        LaunchError(ErrorContainer);
                    else
                        LaunchErrorHTML(true, ErrorContainer, "top.setWait(false);top.oModalPJAdd.hide();");  //
                }
                catch (Exception ex)
                {

                    ErrorContainer = eErrorContainer.GetDevUserError(
                     eLibConst.MSG_TYPE.CRITICAL,
                     eResApp.GetRes(_pref, 72),
                      eResApp.GetRes(_pref, 6342),
                      eResApp.GetRes(_pref, 122),
                     string.Concat("Ajoute une annexe ou un lien en base - AddPjTraitments(); dans ePjAdd.aspx - StackTrace : ", ex.Message));

                    //BSE : #36 346 
                    if (_bFromeUpdater)
                        LaunchError();
                    else
                    {
                        LaunchErrorHTML(true, null, "top.setWait(false);top.oModalPJAdd.hide();");
                    }
                }
            }
            catch (eEndResponseException)
            { }

            #endregion
        }

        /// <summary>
        /// Ajoute une annexe ou un lien en base
        /// Sauvegarde le fichier dans le repertoire des annexes
        /// </summary>
        private void AddPjTraitments()
        {
            try
            {
                eRightAttachment rightManager = new eRightAttachment(_pref, _myPj.Tab);
                
                if (!rightManager.CanAddNewItem())
                {
                    ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 149));
                    Response.Write(eTools.GetCompletAlert(ErrorContainer, fnctReturn: " top.setWait(false); top.oModalPJAdd.hide();"));
                    Response.End();
                }

                //Recuperation du nombre de jour par defaut expiration. Si rien dans param pour la table on met illimité par defaut
                eudoDAL dal = eLibTools.GetEudoDAL(_pref);
                
                try
                {
                    if (!dal.IsOpen)
                        dal.OpenDatabase();

                    _myPj.DayExpire = ePJToAddLite.GetDefaultDayExpire(dal, _myPj.Tab);
                }
                finally
                {
                    if (dal.IsOpen)
                        dal.CloseDatabase();
                }

                // Indique le type de fichier selon la selection
                switch (_myPj.TypePj)
                {
                    case 0: _myPj.FileType = eResApp.GetRes(_pref, 103); break;
                    case 1: _myPj.FileType = eResApp.GetRes(_pref, 127); break;
                    case 2: _myPj.FileType = eResApp.GetRes(_pref, 110); break;
                    case 3: _myPj.FileType = eResApp.GetRes(_pref, 127); break;
                    case 4: _myPj.FileType = eResApp.GetRes(_pref, 112); break;
                    case 5: _myPj.FileType = eResApp.GetRes(_pref, 1438); break;
                }

                // Message alert Javascript en cas d'erreur ou pour un retour d'infos
                string _alert = string.Empty;

                // Type d'annexe
                switch (_myPj.TypePj)
                {
                    case 0: // Type Fichier
                        {
                            if (!rightManager.CanUploadItem())
                            {
                                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 149));
                                Response.Write(eTools.GetCompletAlert(ErrorContainer, fnctReturn: " top.setWait(false); top.oModalPJAdd.hide();"));
                                Response.End();
                            }


                            _error = string.Empty;
                            // Taille maximum du fichier
                            int maxLength = 0;


                            if (eTools.CheckFileToUpload(FileToUpload, maxLength, _lstExtClient, out _error))
                            {

                                HttpPostedFile myFile = FileToUpload.PostedFile;

                                #region Echappements des caractères spéciaux pour le "myPj.StrSaveAs"

                                // On va remplacer les caractères qui peuvent poser des problèmes d'enregistrements
                                if (_myPj.IsSaveAsChecked && _myPj.SaveAs.Length > 0 && _myPj.PjUploadInfo?.Action == 0)
                                {
                                    _myPj.SaveAs = HttpUtility.UrlDecode(_myPj.SaveAs);
                                    _myPj.SaveAs = _myPj.SaveAs.Replace("/", "\\");
                                    _myPj.SaveAs = _myPj.SaveAs.Replace("'", " ");
                                    _myPj.SaveAs = _myPj.SaveAs.Replace("%", " ");
                                    _myPj.SaveAs = _myPj.SaveAs.Replace("-", " ");
                                    _myPj.SaveAs = _myPj.SaveAs.Replace("+", " ");

                                    //retire les accents si contient utf8 et non fulll unicode
                                    if (!_pref.IsFullUnicode && eLibTools.ContainsNonUtf8(_myPj.SaveAs))
                                        _myPj.SaveAs = eLibTools.RemoveDiacritics(_myPj.SaveAs);

                                    // Compatibilité CHROME 
                                    // Chrome renvoi un chemin erroné ("c:\fakepath\nomdufichier.jpg")
                                    if (_myPj.SaveAs.Contains("\\"))
                                    {
                                        string[] _tabSaveAs = _myPj.SaveAs.Split('\\');
                                        //JBE 07/07 problème de "'" dans les pièces jointes BUG#2989
                                        _myPj.SaveAs = _tabSaveAs[_tabSaveAs.Length - 1];
                                    }
                                }
                                else
                                {
                                    _myPj.SaveAs = Path.GetFileName(myFile.FileName);

                                    //JBE 07/07 problème de "'" dans les pièces jointes BUG#2989
                                    _myPj.SaveAs = _myPj.SaveAs.Replace("'", " ");
                                    _myPj.SaveAs = _myPj.SaveAs.Replace("%", " ");
                                }

                                #endregion

                                // chemin physique des annexes
                                String _physicDatasPath = String.Concat(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ANNEXES, _pref));

                                string _strCopyMsg = string.Empty;
                                string _strFileType = string.Empty;
                                string _fileName = _myPj.SaveAs;

                                //BSE : #36 346 
                                if (String.Concat(_physicDatasPath, _fileName).Length > eLibConst.MAX_PATH_LENGTH)
                                {

                                    ErrorContainer = eErrorContainer.GetDevUserError(
                                    eLibConst.MSG_TYPE.CRITICAL,
                                    eResApp.GetRes(_pref, 72),
                                    eResApp.GetRes(_pref, 7782).Replace("<NBC>", (eLibConst.MAX_PATH_LENGTH - _physicDatasPath.Length).ToString()),
                                    eResApp.GetRes(_pref, 122),
                                     "");

                                    LaunchErrorHTML(true, ErrorContainer, "top.setWait(false);top.oModalPJAdd.hide();");
                                }

                                if (_myPj.PjUploadInfo?.Action == 1)
                                {
                                    _myPj.OverWrite = true;

                                    _error = ePJTraitements.DeletePjFromDisk(_pref.GetBaseName, _fileName);

                                    if (_error.Length > 0)
                                    {
                                        throw new Exception(string.Concat(String.Format(eResApp.GetRes(_pref, 8695), _fileName), Environment.NewLine, _error));
                                    }
                                }

                                //Copie le fichier sur le serveur
                                bool bCannotUploadFile = ePJTraitements.CopyFile(_pref, myFile, _physicDatasPath, ref _fileName, out _strCopyMsg, _myPj.OverWrite, true, out _strFileType);
                                _myPj.FileType = _strFileType;
                                _myPj.SaveAs = _fileName;

                                //--IMPOSSIBLE D'UPLOADER LE FICHIER (NOM DE FICHIER INCORRECT)
                                if (!bCannotUploadFile)
                                {
                                    _myPj.SaveAs = _myPj.SaveAs.Replace("'", " ");
                                    _myPj.SaveAs = _myPj.SaveAs.Replace("\"", " ");

                                    throw new Exception(string.Concat("IMPOSSIBLE D'UPLOADER LE FICHIER (NOM DE FICHIER INCORRECT) methode ePJTraitements.CopyFile()", Environment.NewLine, _strCopyMsg));

                                }

                                #region  Ajout de la taille et du Type dans la tables des annexes
                                // Taille du fichier
                                _myPj.Size = myFile.ContentLength;

                                // Type de fichier (image, word, PDF ...)
                                _strFileType = _strFileType.Replace("\\", "\\\\");
                                _strFileType = _strFileType.Replace("'", "\\'");

                                #endregion

                                #region Echappement des champs description, ToolTip et Libellé

                                //      myPj.StrDesc = HttpUtility.UrlDecode(Request.Form["txtDesc"]).Replace("'", "''");
                                //           myPj.StrToolTip = HttpUtility.UrlDecode(Request.Form["txtToolTip"]).Replace("'", "''");
                                _myPj.Label = _myPj.SaveAs;

                                #endregion

                                #region INSERT EN BASE DANS LA TABLE PJ

                                StringBuilder formulaScripts = new StringBuilder();

                                //On insere en base que si le fichier a été Uploadé, et que si le fichier n'est pas présent pour un écrasement
                                int nbInsert = -1;
                                if (bCannotUploadFile && (File.Exists(_physicDatasPath + "\\" + _myPj.SaveAs)))
                                {
                                    _error = string.Empty;

                                    String bodyHtmlColId = String.Concat("edtCOL_", TableType.CAMPAIGN.GetHashCode(), "_", CampaignField.BODY.GetHashCode());

                                    // Cas des campagne
                                    if (_myPj.Tab == TableType.CAMPAIGN.GetHashCode() && hfMemoId.Value.Length > 0 && hfMemoId.Value.StartsWith(bodyHtmlColId))
                                        _myPj.TypePj = PjType.CAMPAIGN.GetHashCode();

                                    EngineResult formulaResult = ePJTraitements.InsertIntoPJ(_pref, _myPj, out nbInsert, out _sUserError, out _error);

                                    // Si il y a une mise a jour des champs de la fiche, celle-ci est rafraichie entièrement
                                    if (formulaResult != null)
                                        AppendRefreshStatements(formulaResult, formulaScripts);
                                }

                                if (_error.Length > 1)
                                {
                                    _alert = eTools.GetCompletAlert(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), _error, eResApp.GetRes(_pref, 416)), 0, 0, " parent.setWait(false);");
                                    Response.Write(_alert);
                                }
                                #endregion


                                string returnFnct = "top.reloadBKMPJ();  top.setWait(false); top.oModalPJAdd.hide();";
                                //GCH dans le cas d'une campagne en mode fiche : hfMemoId.Value est vide
                                if (_myPj.Tab == TableType.CAMPAIGN.GetHashCode() && hfMemoId.Value.Length > 0)
                                    returnFnct = string.Concat(
                                        "var pj = {id:", nbInsert, ",", "src:'", _myPj.GetDownloadUrl(_pref, HttpContext.Current), "', name:'", _myPj.SaveAs, "'}",
                                        Environment.NewLine,
                                        " top.window['_medt']['", hfMemoId.Value, "'].insertPJLink(pj);",
                                        Environment.NewLine,
                                        " top.setWait(false);",
                                        Environment.NewLine,
                                        " top['_md']['oModalPJAdd'].hide();");

                                // Création de l'alert du renommage du fichier
                                if (_strCopyMsg.Length > 1)
                                {
                                    _alert = eTools.GetCompletAlert(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.INFOS, eResApp.GetRes(_pref, 6733), _strCopyMsg, eResApp.GetRes(_pref, 5042)), 0, 0, returnFnct);
                                    Response.Write(_alert);
                                }
                                else
                                    Response.Write(string.Concat("<script type=\"text/javascript\" language=\"javascript\">", returnFnct, Environment.NewLine, formulaScripts.ToString(), Environment.NewLine, " top.oModalPJAdd.hide();</script>"));

                                //_alert = string.Concat(_alert, " alert('", _strCopyMsg.Replace("'", "\\'"), "');");

                                // Affichage du message en cas de renommage du fichier et demande de fermeture de la modaldialog
                                //  Response.Write(string.Concat("<script type=\"text/javascript\" language=\"javascript\">", Environment.NewLine, Environment.NewLine, " parent.reloadBKMPJ();", Environment.NewLine, " parent.setWait(false); ", Environment.NewLine, " parent.oModalPJAdd.hide();", Environment.NewLine, "</script>"));

                            }
                            else // Pas de fichier selectionné obsolète car la vérification est faite en js coté client
                            {
                                _error = _error.Replace("'", "\\'");

                                _alert = eTools.GetCompletAlert(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), _error, eResApp.GetRes(_pref, 416)), 0, 0, " parent.setWait(false);");
                                Response.Write(_alert);
                            }

                            break;
                        }
                    case 5: //type repertoire deprecated !!!
                        {
                            break;
                        }
                    case 3: // Type lien
                        {
                            _error = string.Empty;
                            eLibConst.MSG_TYPE msgType = eLibConst.MSG_TYPE.INFOS;
                            string strErrorTitle = String.Empty;
                            string strErrorMsg = String.Empty;
                            string strErrorDetailedMsg = String.Empty;

                            #region Echappement des champs description, ToolTip et Libellé
                            //         myPj.StrDesc = HttpUtility.UrlDecode(Request.Form["txtDesc"]).Replace("'", "''");
                            //          myPj.StrToolTip = HttpUtility.UrlDecode(Request.Form["txtToolTip"]).Replace("'", "''");
                            _myPj.Label = _myPj.UploadLink;
                            #endregion

                            if (_myPj.Label.Length > 0)
                            {

                                int nbInsert = -1;
                                Engine.Result.EngineResult formulaResult = ePJTraitements.InsertIntoPJ(_pref, _myPj, out nbInsert, out _sUserError, out _error);

                                if (_error.Length > 1)
                                {
                                    msgType = eLibConst.MSG_TYPE.CRITICAL;
                                    strErrorTitle = eResApp.GetRes(_pref, 416);
                                    strErrorMsg = eResApp.GetRes(_pref, 72);
                                    strErrorDetailedMsg = _error;
                                }
                            }
                            else
                            {
                                _error = eResApp.GetRes(_pref, 114);
                                _alert = eTools.GetCompletAlert(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 6304), _error, eResApp.GetRes(_pref, 6564)), 0, 0, " parent.setWait(false);");
                                Response.Write(_alert);
                                return;
                            }

                            // Affichage du message et demande de fermeture de la modaldialog
                            string strReturnFct = String.Empty;
                            if (_sAction == "AddPJ")
                                strReturnFct = "parent.reloadBKMPJ();  parent.setWait(false); parent.oModalPJAdd.hide();";

                            if (_error.Length > 0)
                            {
                                _alert = eTools.GetCompletAlert(eErrorContainer.GetUserError(msgType, strErrorMsg, strErrorDetailedMsg, strErrorTitle), 0, 0, strReturnFct);
                                Response.Write(_alert);
                            }
                            else
                                Response.Write(string.Concat("<script type=\"text/javascript\" language=\"javascript\">", strReturnFct, " </script>"));
                            break;
                        }
                }

                //todo
                #region SavePJ2Text
                #endregion
            }
            catch (eEndResponseException) { Response.End(); }
            catch (ThreadAbortException) { }    // Laisse passer le response.end du RenderResult
            catch (Exception ex)
            {

                throw;
            }


        }

        /// <summary>
        ///  Ajout des appels js pour rafraichir une partie ou toute la fiche
        /// </summary>
        /// <param name="formulaResult"></param>
        /// <param name="javascript"></param>
        private void AppendRefreshStatements(EngineResult formulaResult, StringBuilder javascript)
        {
            //Reload la fiche
            if (formulaResult.ListRefreshFields != null || formulaResult.ListDescidRuleUpdated != null ||
                formulaResult.ReloadDetail || formulaResult.ReloadFileHeader || formulaResult.ReloadHeader)
                javascript.Append(" top.RefreshFile();").AppendLine();


            // rafraichissement de la bkm source
            if (formulaResult.RefreshBkm != null && formulaResult.RefreshBkm.Count > 0)
                foreach (int tab in formulaResult.RefreshBkm)
                    javascript.Append(" top.RefreshBkm(").Append(tab).Append(");").AppendLine();
        }

    }
}