using System;
using System.Collections.Generic;
using System.Web.UI;
using Com.Eudonet.Internal;
using System.Text.RegularExpressions;
using System.IO;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// page permettant de gérer les champs de type fichier
    /// </summary>
    public partial class eFieldFiles : eEudoPage
    {
        private Int32 _iDescid = 0;
        private String _sAlertError = "";

        /// <summary>indique si une sélection multiple est possible</summary>
        public Boolean _bMultiple = false;

        private eFieldFilesRenderer rdr;
        private eLibConst.FOLDER_TYPE folderType;
        /// <summary>Type de fichier : "excel", "pdf", ...</summary>
        private String _fileType;

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// méthode de chargment de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
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


            #region add js


            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("ePj");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eButtons");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eFieldEditor");
            PageRegisters.AddScript("ePopup");

            #endregion

            li1.Controls.Add(new LiteralControl(String.Concat(eResApp.GetRes(_pref, 1114), " :")));
            lblAdd.Text = eResApp.GetRes(_pref, 18);

            if (!_requestTools.AllKeys.Contains("folder"))
                return;

            String sFolder = Request.Form["folder"].ToString();


            String sFiles = String.Empty;
            if (_requestTools.AllKeys.Contains("files"))
                sFiles = Request.Form["files"].ToString();

            if (_requestTools.AllKeys.Contains("mult"))
            {
                _bMultiple = Request.Form["mult"].ToString() == "1";
            }

            if (_requestTools.AllKeys.Contains("descid"))
            {
                Int32.TryParse(Request.Form["descid"].ToString(), out _iDescid);
            }

            if (_requestTools.AllKeys.Contains("filetype"))
            {
                _fileType = Request.Form["filetype"].ToString();
            }

            _fileType = !String.IsNullOrEmpty(_fileType) ? _fileType : "excel"; // Par défaut : Excel

            //on met les valeur dans des input masquées pour les récupérer après le postback
            folder.Value = sFolder;
            files.Value = sFiles;
            descid.Value = _iDescid.ToString();
            mult.Value = _bMultiple ? "1" : "0";
            filetype.Value = _fileType;

            Int32 iFolderType = 0;
            folderType = eLibConst.FOLDER_TYPE.FOLDERS;
            Regex reFolders = new Regex(@"^\[[0-9]\]+$");
            if (!reFolders.IsMatch(sFolder) && Int32.TryParse(sFolder, out iFolderType))
            {
                try
                {
                    folderType = (eLibConst.FOLDER_TYPE)iFolderType;
                    if (folderType == eLibConst.FOLDER_TYPE.MODELES)
                        sFolder = "";
                }
                catch (Exception ex)
                {
                    folderType = eLibConst.FOLDER_TYPE.FOLDERS;
                }
            }

            if (folderType == eLibConst.FOLDER_TYPE.FOLDERS && _iDescid == 0)
                return;

            rdr = new eFieldFilesRenderer(_pref, folderType, sFolder, _bMultiple, sFiles);

            String sMask = String.Empty;
            if (folderType == eLibConst.FOLDER_TYPE.MODELES)
            {
                if (filetype.Value == "pdf")
                    sMask = ".pdf";
                else
                    sMask = ".xls;.xlsx";
            }

            if (!IsPostBack)
                rdr.LoadFiles(divLstFiles, sMask);
        }

        /// <summary>
        /// Bouton d'ajout de fichier à la bibliothèque
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lnkBtnAddFile_Click(object sender, EventArgs e)
        {

            // Vérification du fichier à Uploader
            String sError = String.Empty;
            eErrorContainer err;
            String sMask = "";
            Int32 iMaxSize = 0;

            if (folderType == eLibConst.FOLDER_TYPE.MODELES)
            {
                if (filetype.Value == "pdf")
                    sMask = ".pdf";
                else
                    sMask = ".xls;.xlsx";

            }
            else if (!eDataTools.GetFileUploadWhiteList(_iDescid, out sMask, out iMaxSize, out sError, pref: _pref))
            {
                //TODO message de retour au user.
                return;
            }

            sMask = sMask.ToLower().Replace("*.", ".");
            List<String> lstExt = new List<string>();
            lstExt.AddRange(sMask.Split(';'));

            if (!FileToUpload.HasFile || !eTools.CheckFileToUpload(FileToUpload, iMaxSize, lstExt, out sError))
            {
                rdr.LoadFiles(divLstFiles);

                if (!FileToUpload.HasFile && String.IsNullOrEmpty(sError))
                    sError = eResApp.GetRes(_pref, 6855);

                err = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, sError, "", eResApp.GetRes(_pref, 1114));
                _sAlertError = new eAlert(err).GetJs(true);

                lbl_erreur.Text = _sAlertError;
                return;
            }


            String sReturnFileName = "";
            String sFileName = FileToUpload.FileName;
            String sMsg = "";
            Boolean bCannotUploadFile = ePJTraitements.CopyFile(_pref, FileToUpload.PostedFile, rdr.FolderPath, ref sFileName, out sMsg, false, false, out sReturnFileName);

            rdr.LoadFiles(divLstFiles);

            //--IMPOSSIBLE D'UPLOADER LE FICHIER
            if (!bCannotUploadFile)
            {
                sFileName = sFileName.Replace("'", " ");
                sFileName = sFileName.Replace("\"", " ");

                err = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.EXCLAMATION,
                   eResApp.GetRes(_pref, 1114), eResApp.GetRes(_pref, 124).Replace("<FILE>", sFileName), eResApp.GetRes(_pref, 5042), String.Concat("ePJTraitements.CopyFile --IMPOSSIBLE D'UPLOADER LE FICHIER (NOM DE FICHIER INCORRECT) : ", sMsg));
                _sAlertError = new eAlert(err).GetJs(true);

                lbl_erreur.Text = _sAlertError;

                eFeedbackXrm.LaunchFeedbackXrm(err, _pref);

                return;
            }

            lbl_erreur.Text = "";

        }


        
    }
}