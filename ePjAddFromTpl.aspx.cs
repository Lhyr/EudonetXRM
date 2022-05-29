using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Page affichant la liste des annexes pour les templates et qui permet d'en ajouter ou en supprimer
    /// </summary>
    public partial class ePjAddFromTpl : eEudoPage
    {
        #region variables membres

        private const int MaxLength = 0;

        /// <summary>Table sur laquelle les pjs vont être ratachées</summary>
        public int _nTab;

        /// <summary>les pjIds</summary>
        public int _pjId;

        /// <summary>Message d'erreur</summary>
        private string _error = string.Empty;
        private string _sUserError = string.Empty;

        //hauteur/largeur 
        private int _nWidth = eConst.DEFAULT_WINDOW_WIDTH;
        private int _nHeight = eConst.DEFAULT_WINDOW_HEIGHT;

        /// <summary>Tableau contenant les extentions à exclure ou autoriser (à déterminer) Todo</summary>
        private List<string> _LstExtClient = new List<string>();

        private string _sViewType = "checkedonly";


        /// <summary>Objet représentant une PJ</summary>
        protected ePJToAdd _myPj = new ePJToAdd();

        ///<summary>Output JS </summary>
        protected string _outputJs = string.Empty;

        /// <summary>
        /// Output JS pour le saitwaitfalse
        /// </summary>
        protected string _outputJsSetWait = string.Empty;
        /// <summary>
        /// Définit l'action de l'utilisateur
        /// "init" ==> Ouverture de la fenetre d'ajout de PJ 
        /// "AddPj" ==> Retour de eMain.js pour valider l'ajout de la PJ
        /// </summary>
        protected string _sAction = string.Empty;

        /// <summary>
        /// Liste des noms de fichiers (utilisée pour les templates de type Mails
        /// </summary>
        private List<string> _lstFiles = new List<string>();


        private bool _bFromTpl = false;
        private bool _bFromMailTemplate = false;
        #endregion

        /// <summary>indique si l'ajout d'annexe est autorisé </summary>
        public bool IsAddAllowed = false;
        public bool IsAddLinkAllowed = false;

        /// <summary>
        /// Type de PJ (fichier, lien...)
        /// </summary>
        private int _nPjType = 0;


        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                #region ajout des css

                PageRegisters.AddCss("ePJ", "all");
                PageRegisters.AddCss("eMain");
                PageRegisters.AddCss("eList");
                PageRegisters.AddCss("eIcon");
                PageRegisters.AddCss("eControl");
                PageRegisters.AddCss("eTitle");
                PageRegisters.AddCss("eActions");
                PageRegisters.AddCss("eContextMenu");
                PageRegisters.AddCss("eCatalog");
                PageRegisters.AddCss("eActionList");

                #endregion


                #region ajout des js

                PageRegisters.AddScript("eTools");
                PageRegisters.AddScript("ePj");
                PageRegisters.AddScript("eMain");
                PageRegisters.AddScript("eButtons");
                PageRegisters.AddScript("eList");
                PageRegisters.AddScript("eUpdater");
                PageRegisters.AddScript("eModalDialog");

                #endregion
                #region Recuperation des variables de la fiche

                List<PJUploadInfo> files = null;
                try
                {
                    // Récupération des variables passées en POST
                    if (Request.Form["UploadInfo"] != null)
                    {
                        files = JsonConvert.DeserializeObject<List<PJUploadInfo>>(Request.Form["UploadInfo"]);
                    }

                    _myPj.SaveAs = _requestTools.GetRequestFormKeyS("SaveAs") == null ? string.Empty : HttpUtility.UrlDecode(_requestTools.GetRequestFormKeyS("SaveAs"));

                    if (!_pref.IsFullUnicode && eLibTools.ContainsNonUtf8(_myPj.SaveAs))
                        _myPj.SaveAs = eLibTools.RemoveDiacritics(_myPj.SaveAs);

                    if (Request.Form["nFileID"] != null)
                    {
                        _myPj.FileId = eLibTools.GetNum(Request.Form["nFileID"].ToString());
                        nFileID.Value = _myPj.FileId.ToString();
                    }
                    if (Request.Form["nTab"] != null)
                    {
                        _myPj.Tab = eLibTools.GetNum(Request.Form["nTab"].ToString());
                        nTab.Value = _myPj.Tab.ToString();
                        _nTab = _myPj.Tab;
                    }

                    if (Request.Form["iDsOfPj"] != null)
                        idspj.Value = Request.Form["iDsOfPj"].ToString();

                    if (_requestTools.AllKeys.Contains("width") && Request.Form["width"] != null)
                        int.TryParse(Request.Form["width"].ToString(), out _nWidth);

                    if (_requestTools.AllKeys.Contains("height") && Request.Form["height"] != null)
                        int.TryParse(Request.Form["height"].ToString(), out _nHeight);

                    //Cas d'ajout de pj depuis l'assiatnt d'emailing en signet ou modif
                    if (_requestTools.AllKeys.Contains("parentEvtFileId") && Request.Form["parentEvtFileId"] != null)
                    {
                        _myPj.ParentEvtFileId = eLibTools.GetNum(Request.Form["parentEvtFileId"].ToString());
                        parentEvtFileId.Value = _myPj.ParentEvtFileId.ToString();
                    }
                    if (_requestTools.AllKeys.Contains("parentEvtTabId") && Request.Form["parentEvtTabId"] != null)
                    {
                        _myPj.ParentEvtTab = eLibTools.GetNum(Request.Form["parentEvtTabId"].ToString());
                        parentEvtTabId.Value = _myPj.ParentEvtTab.ToString();
                    }
                    if (_requestTools.AllKeys.Contains("fromtpl") && Request.Form["fromtpl"] != null)
                    {
                        _bFromTpl = Request.Form["fromtpl"].ToString() == "1";
                        fromtpl.Value = Request.Form["fromtpl"].ToString();
                        if (_requestTools.AllKeys.Contains("viewtype") && Request.Form["viewtype"] != null)
                        {
                            _sViewType = Request.Form["viewtype"].ToString();
                            viewtype.Value = _sViewType.Length > 0 ? _sViewType : "checkedonly";
                        }
                    }
                    if (_requestTools.AllKeys.Contains("frommailtemplate") && Request.Form["frommailtemplate"] != null)
                    {
                        _bFromMailTemplate = Request.Form["frommailtemplate"].ToString() == "1";
                        if (_requestTools.AllKeys.Contains("viewtype") && Request.Form["viewtype"] != null)
                        {
                            _sViewType = Request.Form["viewtype"].ToString();
                            viewtype.Value = _sViewType.Length > 0 ? _sViewType : "all";
                        }
                    }
                    if (_requestTools.AllKeys.Contains("ppid") && Request.Form["ppid"] != null)
                    {
                        _myPj.PPID = eLibTools.GetNum(Request.Form["ppid"].ToString());
                        ppid.Value = _myPj.PPID.ToString();
                    }
                    if (_requestTools.AllKeys.Contains("pmid") && Request.Form["pmid"] != null)
                    {
                        _myPj.PMID = eLibTools.GetNum(Request.Form["pmid"].ToString());
                        pmid.Value = _myPj.PMID.ToString();
                    }
                    if (_requestTools.AllKeys.Contains("mailForward") && Request.Form["mailForward"] != null)
                    {
                        _myPj.MailForwarded = Request.Form["mailForward"].Equals("1");
                        mailForward.Value = _myPj.MailForwarded.GetHashCode().ToString();// bool convertit en 0 ou 1;
                    }
                    if (_requestTools.AllKeys.Contains("adrid") && Request.Form["adrid"] != null)
                    {
                        _myPj.ADRID = eLibTools.GetNum(Request.Form["adrid"].ToString());
                    }

                    if (_requestTools.AllKeys.Contains("radioPJ") && Request.Form["radioPJ"] != null)
                    {
                        _nPjType = eLibTools.GetNum(Request.Form["radioPJ"].ToString());
                        pjType.Value = _nPjType.ToString();
                    }

                    if (_requestTools.AllKeys.Contains("uploadvalue") && Request.Form["uploadvalue"] != null)
                        _myPj.UploadLink = Request.Form["uploadvalue"].ToString();


                    #endregion
                }
                catch (Exception ex)
                {
                    lbl_erreur.Text = string.Concat(eResApp.GetRes(_pref, 72), ", ", eResApp.GetRes(_pref, 6342));
                    eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, ex.Message), _pref);
                }
                #region Traitement de l'ajout de PJ par drag & drop sur la page

                if (Request.ContentType.Contains("multipart/form-data") && !IsPostBack)
                {
                    int nFileId = 0;
                    int nTabId = 0;
                    string sidspj = string.Empty;

                    // Charge les valeurs de request TODO : les centraliser plus haut.
                    HashSet<string> allKeys = new HashSet<string>(Request.Form.AllKeys, StringComparer.OrdinalIgnoreCase);
                    if (allKeys.Contains("nFileId") && Request.Form["nFileId"] != null)
                        int.TryParse(Request.Form["nFileId"].ToString(), out nFileId);
                    _myPj.FileId = nFileId;

                    if (allKeys.Contains("nTab") && Request.Form["nTab"] != null)
                        int.TryParse(Request.Form["nTab"].ToString(), out nTabId);

                    _myPj.Tab = nTabId;
                    _nTab = _myPj.Tab;

                    if (allKeys.Contains("iDsOfPj") && Request.Form["iDsOfPj"] != null)
                        sidspj = Request.Form["iDsOfPj"].ToString();


                    if (_nPjType == 0 && files != null)
                    {
                        int nbFileUp = 0;
                        HttpPostedFile myFile = null;
                        //string[] sShortFilesName = _myPj.SaveAs.Split('|');

                        //Dictionary<string, string> dicRename = new Dictionary<string, string>();
                        //foreach (string s in sShortFilesName)
                        //{
                        //    string[] aS = s.Split(':');
                        //    dicRename.Add(aS[0], aS.Length == 2 ? aS[1] : aS[0]);
                        //}
                        for (int j = 0; j < Request.Files.Count; j++)
                        {
                            myFile = Request.Files[j];
                            int nFileLen = myFile.ContentLength;

                            if (eTools.CheckFileToUpload(myFile, MaxLength, _LstExtClient, out _error))
                            {
                                try
                                {
                                    _pjId = 0;

                                    // #57 013 - Le tableau dicRename étant alimenté à partir de noms de fichiers stockés dans une variable ayant subi un UrlDecode(),
                                    // il faut donc effectuer le même traitement sur le FileName de chaque fichier pour retrouver, dans le tableau des noms de fichier,
                                    // les fichiers dont le nom a subi une transformation après passage via UrlDecode
                                    // Cas des fichiers comportant, par ex., le signe + dans le nom
                                    _myPj.PjUploadInfo = files.Find(f => f.FileName == HttpUtility.UrlDecode(Path.GetFileName(myFile.FileName)));
                                    _myPj.SaveAs = _myPj.PjUploadInfo?.SaveAs;

                                    //Sauvegarde du fichier
                                    bool retourARddPj = AddPjTraitments(myFile, string.Empty, out _pjId);

                                    if (_error.Length > 0)
                                    {
                                        _outputJsSetWait = string.Concat(_outputJsSetWait, Environment.NewLine, _error);
                                    }
                                    if (retourARddPj)
                                    {
                                        // récuperation de la PjId
                                        if (sidspj.Length > 0)
                                            sidspj = string.Concat(sidspj, ";", _pjId.ToString());
                                        else
                                            sidspj = _pjId.ToString();
                                    }
                                    else
                                    {
                                        // [MOU 17/11/2014 #34686] 
                                        // erreur sur l'ajout,                              
                                        throw new Exception(_error);
                                    }
                                }
                                catch (eEndResponseException)
                                {
                                    Response.End();
                                }
                                catch (EudoException ex)
                                {
                                    LaunchError(eErrorContainer.GetErrorContainerFromEudoException(eLibConst.MSG_TYPE.CRITICAL, ex));
                                }
                                catch (Exception ex)
                                {


                                    lbl_erreur.Text = string.Concat(eResApp.GetRes(_pref, 72), ", ", eResApp.GetRes(_pref, 6342));
                                    eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, ex.Message), _pref);
                                    _outputJsSetWait = string.Concat(_outputJsSetWait, Environment.NewLine, lbl_erreur.Text);


                                }
                                finally
                                {
                                    if (_pjId > 0)
                                        nbFileUp++;
                                }
                            }
                        }


                        if (nbFileUp <= 0)
                        {

                            try
                            {
                                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.INFOS, eResApp.GetRes(_pref, 6724), " ", eResApp.GetRes(_pref, 6724));
                                LaunchError();
                            }
                            catch (eEndResponseException)
                            {
                                Response.End();
                            }
                        }
                    }

                    string sErr = string.Empty;

                    #region On rafraîchit la liste des pj
                    ePjListRenderer pjRend = GetRendPjList(sidspj, _sViewType, out sErr);
                    if (pjRend == null || sErr.Length != 0)
                        return;     // TODO - RENVOYER UNE ERREUR ?

                    Panel mainDiv = pjRend.PgContainer;
                    mainDiv.Attributes.Add("nbpj", pjRend.DicoPj.Count.ToString());
                    mainDiv.Attributes.Add("idspj", eLibTools.Join(";", pjRend.DicoPj.Keys));

                    try
                    {
                        RenderResultHTML(mainDiv);
                    }
                    catch (eEndResponseException)
                    {

                        Response.End();

                    }
                    #endregion
                }

                #endregion

                #region Affichage de la page ou traitement du formulaire soumis par l'utilisateur

                #region Ajout d'input à la page

                //HtmlInputHidden _hInput = new HtmlInputHidden();
                //_hInput.ID = "nTab";
                //_hInput.Name = "nTab";
                //_hInput.Value = _myPj.Tab.ToString();
                //divHidden.Controls.Add(_hInput);

                //_hInput = new HtmlInputHidden();
                //_hInput.ID = "nfileID";
                //_hInput.Name = "nfileID";
                //_hInput.Value = _myPj.FileId.ToString();
                //divHidden.Controls.Add(_hInput);

                labelViewType1.InnerText = eResApp.GetRes(_pref.LangId, 6769);
                labelViewType2.InnerText = eResApp.GetRes(_pref.LangId, 6770);


                #endregion


                // AFFICHAGE
                // Si on est en mode création de template (donc fileid=0) on ne cherche pas à afficher d'annexe
                if (!IsPostBack)
                {
                    //  Recherche des Annexes dispo pour cette fiche
                    RefreshPjList();
                }

                // OU TRAITEMENT DU FORMULAIRE (BOUTON TELECHARGER UN FICHIER OU AJOUTER UN LIEN)
                else
                {
                    //Ajout de PJ
                    Add_Click(sender, e);
                }
                #endregion
            }
            catch (eEndResponseException)
            {
            }

        }

        /// <summary>
        /// Ajoute une piece une annexe en base
        /// </summary>
        /// <param name="myFile">Fichier à uploader</param>
        /// <param name="strToolTip">Tooltip du fichier</param>
        /// <param name="pjId">Id de la pj ajoutée</param>
        private bool AddPjTraitments(HttpPostedFile myFile, string strToolTip, out int pjId)
        {
            string alertTitle = eResApp.GetRes(_pref, 5042);
            string alertMsg = eResApp.GetRes(_pref, 6316);

            _error = string.Empty;
            pjId = 0;
            eudoDAL dal = eLibTools.GetEudoDAL(_pref);

            try
            {
                eRightAttachment rightManager = new eRightAttachment(_pref, _myPj.Tab);

                if (!rightManager.CanAddNewItem())
                {
                    ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 149));
                    _error = new eAlert(ErrorContainer).GetJs();
                    return false;
                }

                // Récupération du toolTip
                if (strToolTip.Length > 0)
                    _myPj.ToolTipText = strToolTip.Replace("'", "''");

                // Indique le type de fichier
                _myPj.FileType = eResApp.GetRes(_pref, 103);

                //Si on fait un emailing, le type de pj doit etre campaign/rempte_campaign, car dans l'emailing on ne ajoute une url dans le corps.
                if (_myPj.Tab == TableType.CAMPAIGN.GetHashCode())
                    _myPj.TypePj = PjType.CAMPAIGN.GetHashCode();
                else
                {
                    if (_nPjType == 0)
                        _myPj.TypePj = 0; // fichier
                    else
                        _myPj.TypePj = 3; // lien
                }

                #region Echappements des caractères spéciaux pour le "__myPj.StrSaveAs"

                if (string.IsNullOrEmpty(_myPj.SaveAs) || _myPj.PjUploadInfo?.Action == 1)
                    _myPj.SaveAs = Path.GetFileName(myFile.FileName);

                //JBE 07/07 problème de "'" dans les pièces jointes BUG#2989
                _myPj.SaveAs = ePJTraitements.EscapeSpecialCharactersInFilename(_myPj.SaveAs);

                #endregion

                // chemin physique des annexes
                string physicDatasPath = string.Concat(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ANNEXES, _pref));

                string strCopyMsg = string.Empty;
                string strFileType = string.Empty;
                string fileName = _myPj.SaveAs;

                //vérification des droits d'upload d'annexe.
                if (!rightManager.CanUploadItem())
                {
                    ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 149));
                    _error = new eAlert(ErrorContainer).GetJs();
                    return false;
                }

                // Si remplacement du fichier, on supprime d'abord le fichier
                if (_myPj.PjUploadInfo?.Action == 1)
                {
                    _myPj.OverWrite = true;

                    _error = ePJTraitements.DeletePjFromDisk(_pref.GetBaseName, fileName);

                    if (_error.Length > 0)
                    {
                        eErrorContainer err = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.EXCLAMATION,
                            alertMsg, string.Format(eResApp.GetRes(_pref, 8695), fileName), alertTitle,
                            string.Concat("ePJTraitements.DeletePjFromDisk - Impossible de supprimer le fichier existant : ", fileName));
                        _error = new eAlert(err).GetJs();

                        eFeedbackXrm.LaunchFeedbackXrm(err, _pref);
                        return false;
                    }
                }

                //Copie le fichier sur le serveur
                bool bCannotUploadFile = ePJTraitements.CopyFile(_pref, myFile, physicDatasPath, ref fileName, out strCopyMsg, _myPj.OverWrite, true, out strFileType);
                _myPj.FileType = strFileType;
                _myPj.SaveAs = fileName;

                //Recuperation du nombre de jour par defaut expiration. Si rien dans param pour la table on met illimité par defaut

                if (!dal.IsOpen)
                    dal.OpenDatabase();

                _myPj.DayExpire = ePJToAddLite.GetDefaultDayExpire(dal, _myPj.Tab);

                //--IMPOSSIBLE D'UPLOADER LE FICHIER (NOM DE FICHIER INCORRECT)
                if (!bCannotUploadFile)
                {
                    _myPj.SaveAs = _myPj.SaveAs.Replace("'", " ");
                    _myPj.SaveAs = _myPj.SaveAs.Replace("\"", " ");

                    eErrorContainer err = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.EXCLAMATION,
                        alertMsg, eResApp.GetRes(_pref, 124).Replace("<FILE>", _myPj.SaveAs), alertTitle, string.Concat("ePJTraitements.CopyFile --IMPOSSIBLE D'UPLOADER LE FICHIER (NOM DE FICHIER INCORRECT) : ", strCopyMsg));
                    _error = new eAlert(err).GetJs();

                    eFeedbackXrm.LaunchFeedbackXrm(err, _pref);
                    return false;
                }

                #region  Ajout de la taille et du Type dans la tables des annexes
                // Taille du fichier
                _myPj.Size = myFile.ContentLength;

                // Type de fichier (image, word, PDF ...)
                strFileType = strFileType.Replace("\\", "\\\\");
                strFileType = strFileType.Replace("'", "\\'");

                #endregion

                _myPj.Label = _myPj.SaveAs;

                // Pour le mail tranféré, la pj qu'on rajoute sera pas attaché au mail d'origine
                if (_myPj.MailForwarded)
                    _myPj.FileId = 0;

                #region INSERT EN BASE DANS LA TABLE PJ

                //On insere en base que si le fichier a été Uploadé, et que si le fichier n'est pas présent pour un écrasement
                if (bCannotUploadFile && (File.Exists(physicDatasPath + "\\" + _myPj.SaveAs)))
                {
                    if (_myPj.PjUploadInfo?.Action == 1)
                    {
                        pjId = _myPj.PjId = _myPj.PjUploadInfo.PjId;
                        ePJTraitements.UpdateReplacedPJ(_pref.User, dal, _myPj);
                    }
                    else
                    {
                        Engine.Result.EngineResult formulaResult = ePJTraitements.InsertIntoPJ(_pref, _myPj, out pjId, out _sUserError, out _error);
                    }

                }
                if (_error.Length > 1)
                {
                    eErrorContainer err = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION,
                        alertMsg, _error, alertTitle);
                    _error = new eAlert(err).GetJs();
                    return false;
                }
                #endregion

                // Création de l'alert du renommage du fichier
                if (strCopyMsg.Length > 1)
                {
                    eErrorContainer err = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.INFOS,
                        alertMsg, strCopyMsg, alertTitle);
                    _error = new eAlert(err).GetJs();
                }



                return true;
            }
            catch (EudoException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                eErrorContainer err = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL,
                        ex.Source, ex.Message, alertTitle);
                _error = new eAlert(err).GetJs();
                return false;
            }
            finally
            {
                if (dal.IsOpen)
                    dal.CloseDatabase();
            }
        }

        /// <summary>
        /// Savoir si la pj est externalisée (on crée une copie sur le serveur distant)
        /// </summary>
        /// <returns></returns>
        private bool PjExternalEnabled()
        {
            IDictionary<eLibConst.CONFIGADV, string> dic = eLibTools.GetConfigAdvValues(_pref,
                new HashSet<eLibConst.CONFIGADV>(){
                    eLibConst.CONFIGADV.EXTERNAL_PJ_ENABLED
                });

            return dic.ContainsKey(eLibConst.CONFIGADV.EXTERNAL_PJ_ENABLED) && dic[eLibConst.CONFIGADV.EXTERNAL_PJ_ENABLED] == "1";
        }

        /// <summary>
        /// Au clic sur le bouton ajouter la pièce jointe 
        /// Insert en base et ajoute à la liste en dessous
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Add_Click(object sender, EventArgs e)
        {

            lbl_erreur.Text = string.Empty;
            lbl_erreur_link.Text = string.Empty;
            _error = string.Empty;
            _outputJsSetWait = string.Concat(_outputJsSetWait, Environment.NewLine, "top.setWait(false);");

            #region Ajout de fichier par téléchargement
            if (_nPjType == 0)
            {
                // Vérification du fichier à Uploader
                if (eTools.CheckFileToUpload(FileToUpload, MaxLength, _LstExtClient, out _error))
                {
                    try
                    {

                        //Sauvegarde du fichier
                        bool retourARddPj = AddPjTraitments(FileToUpload.PostedFile, string.Empty, out _pjId);

                        if (_error.Length > 0)
                            _outputJsSetWait = string.Concat(_outputJsSetWait, Environment.NewLine, _error);

                        if (retourARddPj)
                        {
                            // récuperation de la PjId
                            if (idspj.Value.Length > 0)
                                idspj.Value = string.Concat(idspj.Value, ";", _pjId.ToString());
                            else
                                idspj.Value = _pjId.ToString();

                        }
                        // On rafraîchit la liste des pj
                        RefreshPjList();
                    }
                    catch (EudoException ex)
                    {
                        LaunchErrorHTML(true, eErrorContainer.GetErrorContainerFromEudoException(eLibConst.MSG_TYPE.CRITICAL, ex), "top.oModalPJAdd.hide();");
                    }
                    catch (Exception ex)
                    {
                        lbl_erreur.Text = string.Concat(eResApp.GetRes(_pref, 72), ", ", eResApp.GetRes(_pref, 6342));
                        eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, ex.Message), _pref);
                    }

                }
                else
                {

                    lbl_erreur.Text = _error;
                    // On raffraichit la liste des pj
                    RefreshPjList();
                }
            }
            #endregion

            #region Ajout de lien
            else if (_nPjType == 1)
            {
                _error = string.Empty;

                #region Echappement des champs description, ToolTip et Libellé
                //         myPj.StrDesc = HttpUtility.UrlDecode(Request.Form["txtDesc"]).Replace("'", "''");
                //          myPj.StrToolTip = HttpUtility.UrlDecode(Request.Form["txtToolTip"]).Replace("'", "''");
                _myPj.Label = _myPj.UploadLink;
                #endregion

                _myPj.TypePj = 3; // type : lien

                if (_myPj.Label.Length > 0)
                {
                    int nbInsert = -1;
                    Engine.Result.EngineResult formulaResult = ePJTraitements.InsertIntoPJ(_pref, _myPj, out nbInsert, out _sUserError, out _error);

                    // récuperation de la PjId
                    if (idspj.Value.Length > 0)
                        idspj.Value = string.Concat(idspj.Value, ";", nbInsert.ToString());
                    else
                        idspj.Value = nbInsert.ToString();
                }
                else
                {
                    _error = eResApp.GetRes(_pref, 114);
                }

                if (_error.Length > 0)
                {
                    lbl_erreur_link.Text = _error;
                }

                // On rafraîchit la liste des pj
                RefreshPjList();
            }
            #endregion

        }


        /// <summary>
        /// Rafraichit la liste des Pj lors de l'ajout 
        /// </summary>
        private void RefreshPjList()
        {
            try
            {
                string sErr = string.Empty;

                ePjListRenderer pjRend = GetRendPjList(idspj.Value, _sViewType, out sErr);
                IsAddAllowed = pjRend.IsAddAllowed;
                IsAddLinkAllowed = _myPj.ParentTab.EdnType != EdnType.FILE_MAIL; // l'ajout de liens n'est pas proposé en mail unitaire

                if (sErr.Length != 0)
                {
                    lbl_erreur.Text = sErr;
                }
                else if (pjRend == null)
                {
                    lbl_erreur.Text = "Rendu vide";
                }
                else
                {
                    Panel monPanel = pjRend.PgContainer;

                    if (monPanel != null)
                    {

                        // [MOU 17/11/2014 #34686] 
                        // On remplace l'ancien container par le nouveau 
                        divlstPJ.Controls.Clear();
                        divlstPJ.Controls.Add(monPanel);
                        divlstPJ.Visible = true;
                    }

                    idspj.Value = eLibTools.Join(";", pjRend.DicoPj.Keys);

                    // Récupération du nombre de PJ
                    nbpj.Value = pjRend.DicoPj.Count.ToString();

                    // Récupération des noms de fichiers pour les TPL Mails
                    // GCH - 17/06/2014 - #31390 - Blocage lors du téléchargement d'annexe dans un mail (image) :
                    //      => retour sur un correctif de la révision 22202 qui préférait l'utilisation de _sRawScript appelé dans le eudopage, mais le problème c'est qu'il n'est jamais appelé dans ce cas car il y a un Page.Header et que le _sRawScript n'est chargé que s'il n'y en a pas.
                    //              Bref ne pas utiliser _sRawScript mais bien _outputJs !
                    _outputJs = "var arrayLstPj = new Array(); this.document.ListePj = arrayLstPj; ";

                    foreach (var item in pjRend.DicoPj)
                    {
                        _outputJs = string.Concat(_outputJs, " arrayLstPj['", item.Key, "'] = '", item.Value.Replace("'", "\\\'"), "';", Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                lbl_erreur.Text = string.Concat(eResApp.GetRes(_pref, 72), ", ", eResApp.GetRes(_pref, 6342));
                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, ex.Message), _pref);
            }
        }

        /// <summary>
        /// Rafraichit la liste des Pj lors de l'ajout 
        /// </summary>
        private ePjListRenderer GetRendPjList(string initialePjIds, string sView, out string error)
        {
            error = string.Empty;
            ePjListRenderer myRend = null;
            if (string.IsNullOrEmpty(sView))
                sView = "checkedonly";

            try
            {
                if (initialePjIds.Length == 0)
                    initialePjIds = "0"; // 

                _myPj.LoadParentTabInfos(_pref);

                if (_bFromTpl && _myPj.ParentTab.TabName.ToUpper().StartsWith("TEMPLATE_") && (_myPj.ParentTab.InterPP || _myPj.ParentTab.InterPM || _myPj.ParentTab.InterEVT))
                {

                    ePjListFromTplRenderer myRendPJFromTpl = (ePjListFromTplRenderer)eRendererFactory.CreatePjListFromTplRenderer(_pref, _myPj, initialePjIds, sView, _nHeight, _nWidth);
                    if (myRendPJFromTpl.ErrorMsg.Length > 0)
                        throw myRendPJFromTpl.InnerException ?? new Exception(myRendPJFromTpl.ErrorMsg);

                    myRend = myRendPJFromTpl;

                    if (eLibTools.GetNum(ppid.Value) == 0 && _myPj.PPID > 0)
                        ppid.Value = _myPj.PPID.ToString();
                    if (eLibTools.GetNum(pmid.Value) == 0 && _myPj.PMID > 0)
                        pmid.Value = _myPj.PMID.ToString();
                    if (eLibTools.GetNum(parentEvtFileId.Value) == 0 && _myPj.ParentEvtFileId > 0)
                        parentEvtFileId.Value = _myPj.ParentEvtFileId.ToString();

                    viewtype.Value = myRendPJFromTpl.ViewType;
                    if ((_myPj.ParentTab.EdnType == EdnType.FILE_MAIL || _myPj.ParentTab.EdnType == EdnType.FILE_SMS) && _myPj.FileId > 0 && _myPj.IsMailFixed && !_myPj.MailForwarded)
                    {
                        setViewType.Style.Add(HtmlTextWriterStyle.Display, "none");
                    }
                    else
                    {
                        rbViewType1.Attributes.Add("onclick", "changePjView('checkedonly');");
                        rbViewType2.Attributes.Add("onclick", "changePjView('all');");

                        switch (myRendPJFromTpl.ViewType)
                        {
                            case "all":
                                setViewType.Attributes.Add("class", "dspchk");
                                rbViewType2.Attributes.Add("checked", "checked");
                                break;
                            case "checkedonly":
                                setViewType.Attributes.Add("class", "dspall");
                                rbViewType1.Attributes.Add("checked", "checked");
                                break;
                            default:
                                break;
                        }
                    }
                }
                else
                {
                    setViewType.Style.Add(HtmlTextWriterStyle.Display, "none");
                    myRend = (ePjListRenderer)eRendererFactory.CreatePjListRenderer(_pref, _myPj, initialePjIds, _nHeight, _nWidth);

                }
                if (myRend.ErrorMsg.Length != 0)
                {
                    error = myRend.ErrorMsg;
                    return null;
                }

                myRend.PgContainer.ID = "subdivlstPJ";
            }
            catch (Exception ex)
            {
                lbl_erreur.Text = string.Concat(eResApp.GetRes(_pref, 72), ", ", eResApp.GetRes(_pref, 6342));
                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, ex.Message), _pref);
            }

            return myRend;
        }
        /// <summary>
        /// Raffraichissement de la liste de Pj suite à une suppression
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void RefreshListPj(object sender, EventArgs e)
        {
            try
            {
                lbl_erreur.Text = string.Empty;

                // On rafraîchit la liste des pj
                RefreshPjList();
            }
            catch (Exception ex)
            {
                lbl_erreur.Text = string.Concat(eResApp.GetRes(_pref, 72), ", ", eResApp.GetRes(_pref, 6342));
                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, ex.Message), _pref);
            }
        }

    }
}