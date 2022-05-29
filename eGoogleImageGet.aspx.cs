using Com.Eudonet.Engine;
using Com.Eudonet.Internal;
//using Google.API.Search;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Common.CommonDTO;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Pop up apparraissant au clique sur l'image de la VCARD du contact
    /// </summary>
    public partial class eGoogleImageGet : eEudoPage
    {
        /// <summary>Javascript de retour à afficher dans le rendu</summary>
        public StringBuilder sbJsInput = new StringBuilder();

        private String _sCurrentFileName = String.Empty;

        #region Variables de post original
        /// <summary>
        /// Nom de l'objet pop up duquel est issu la popup
        /// </summary>
        public string _modalVarName;
        private Int32 _nFileId = 0;
        private Int32 _nTab = 0;
        /// <summary>
        /// Est-ce un upload via un drag n drop ?
        /// </summary>
        Boolean uploadByDragAndDrop = false;
        #endregion

        /// <summary>
        /// Appelée au chargement de la page
        /// </summary>
        /// <param name="e"></param>
        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!String.IsNullOrEmpty(Request["modalVarName"]))
                _modalVarName = Request["modalVarName"].ToString();

            if (!String.IsNullOrEmpty(Request["fileId"]))
                Int32.TryParse(Request["fileId"].ToString(), out _nFileId);

            if (!String.IsNullOrEmpty(Request["nTab"]))
                Int32.TryParse(Request["nTab"].ToString(), out _nTab);

            if (!String.IsNullOrEmpty(Request["fromDragAndDrop"]))
                uploadByDragAndDrop = Request["fromDragAndDrop"].ToString() == "1";
        }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Retourne le nom du fichier avatar de la fiche en cours
        /// </summary>
        /// <returns></returns>
        private String GetAvatarName()
        {
            String sFileName = String.Empty;
            eDataFillerGeneric dtf = new eDataFillerGeneric(_pref, _nTab, ViewQuery.CUSTOM);
            dtf.EudoqueryComplementaryOptions =
                delegate (EudoQuery.EudoQuery eqDelegate)
                {
                    eqDelegate.SetListCol = (_nTab + EudoQuery.AllField.AVATAR.GetHashCode()).ToString();
                    eqDelegate.SetFileId = _nFileId;

                };

            dtf.Generate();

            if (dtf.ErrorMsg.Length != 0 || dtf.InnerException != null)
            {
                if (dtf.ErrorType != QueryErrorType.ERROR_NUM_PREF_NOT_FOUND)
                    throw new Exception(String.Concat(dtf.ErrorMsg, dtf.InnerException == null ? String.Empty : dtf.InnerException.Message));
            }


            if (dtf.ListRecords.Count == 1)
            {
                eFieldRecord f = dtf.GetFirstRow().GetFields.Find(fld => fld.FldInfo.Descid == (_nTab + EudoQuery.AllField.AVATAR.GetHashCode()));
                if (f != null)
                    sFileName = f.DisplayValue;
            }

            return sFileName;
        }

        private string DeleteAvatarFiles(String sFileName)
        {
            string strReturn = "";

            string sFilePath = String.Concat(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.FILES, _pref), @"\", sFileName);

            try
            {
                if (File.Exists(sFilePath))
                    File.Delete(sFilePath);
                if (File.Exists(sFilePath.Replace(".jpg", String.Concat(eLibConst.MOBILE_SUFFIX, ".jpg"))))
                    File.Delete(sFilePath.Replace(".jpg", String.Concat(eLibConst.MOBILE_SUFFIX, ".jpg")));
                if (File.Exists(sFilePath.Replace(".jpg", String.Concat(eLibConst.THUMB_SUFFIX, ".jpg"))))
                    File.Delete(sFilePath.Replace(".jpg", String.Concat(eLibConst.THUMB_SUFFIX, ".jpg")));
            }
            catch (Exception ex)
            {
                strReturn = ex.Message;
            }

            return strReturn;
        }

        /// <summary>
        /// Met à jour le nom du fichier avatar
        /// </summary>
        /// <param name="sFileName"></param>
        /// <returns></returns>
        private bool UpdateAvatarName(string sFileName)
        {
            if (_nFileId == 0)
                return true;

            Engine.Engine eng = eModelTools.GetEngine(_pref, _nTab, eEngineCallContext.GetCallContext(EngineContext.APPLI));
            eng.FileId = _nFileId;

            eng.AddNewValue(_nTab + AllField.AVATAR.GetHashCode(), sFileName, false, true);

            if (_nTab == (int)TableType.USER)
                eng.EngineProcess(new StrategyCruUser());
            else
                eng.EngineProcess(new StrategyCruSimple());

            if (!eng.Result.Success)
            {
                throw new Exception(eng.Result.Error.DebugMsg);
            }

            return true;

        }

        /// <summary>
        /// PageLoad
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (_nTab > 0 && _nFileId > 0)
                _sCurrentFileName = GetAvatarName();

            if (uploadByDragAndDrop)
            {
                HttpPostedFile file;
                String url;
                String sPhyName;
                String error;
                Dictionary<String, String> returnValues = new Dictionary<string, string>();

                if (Request.Files["file"] != null)
                {
                    file = Request.Files["file"];
                    Upload(file, out url, out error, out sPhyName);

                    if (!String.IsNullOrEmpty(error))
                    {
                        ErrorContainer = eErrorContainer.GetDevUserError(
                             eLibConst.MSG_TYPE.CRITICAL,
                             eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                             String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  
                             eResApp.GetRes(_pref, 72),  //   titre
                             error
                        );
                        returnValues.Add("title", eResApp.GetRes(_pref, 72));
                        returnValues.Add("msg", String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)));
                        returnValues.Add("detail", error);
                    }
                    else
                    {
                        returnValues.Add("imageURL", url);
                        returnValues.Add("imageName", sPhyName);
                    }

                    try
                    {
                        DoResponse(returnValues);
                    }
                    catch (eEndResponseException)
                    {

                        Response.End();

                    }


                }
            }
            else
            {
                #region ajout des css

                PageRegisters.AddCss("eMain");
                PageRegisters.AddCss("eImage");
                PageRegisters.AddCss("eButtons");

                #endregion

                #region add js
                PageRegisters.AddScript("eTools");
                PageRegisters.AddScript("eUpdater");
                PageRegisters.AddScript("eModalDialog");
                PageRegisters.AddScript("eMain");
                #endregion

                string keyword = ""; // mot-clé utilisé pour la recherche Google Image
                string resultcount = "10"; // nombre de résultat affiché sur la page
                string pageRequest = string.Empty;

                sbJsInput.Append("  nTab = ").Append(_nTab).AppendLine(";");
                sbJsInput.Append("  nFileId = ").Append(_nFileId).AppendLine(";");

                sbJsInput.Append(" sEmptyIconPath = '").Append("themes/").Append(_pref.ThemePaths.GetImageWebPath("/images/ui/picture.png")).AppendLine("';");
                sbJsInput.Append(" sFullIconPath = '").Append("themes/").Append(_pref.ThemePaths.GetDefaultImageWebPath()).AppendLine("';");



                try
                {
                    // Demande #33 532 - Message d'avertissement si on tombe sur l'erreur explicite "[response status:503]qps rate exceeded"
                    // Pour debugger le message d'erreur 503 "qps rate exceeded" sans assaillir Google de requêtes, décommenter la ligne ci-dessous
                    //throw new Exception("503: qps rate exceeded");

                    if (_sCurrentFileName.Length > 0)
                    {
                        // Initialisation de la photo de la VCARD
                        Panel vcPhoto = new Panel();
                        eTools.SetAvatar(vcPhoto, _pref, _nTab, false, _sCurrentFileName, _nFileId);

                        DivCurrentPicture.Attributes.Add("tab", _nTab.ToString());
                        DivCurrentPicture.Attributes.Add("fid", _nFileId.ToString());
                        DivCurrentPicture.Attributes.Add("did", (_nTab + AllField.AVATAR.GetHashCode()).ToString());

                        Label labCurrentPic = new Label();
                        labCurrentPic.Text = eResApp.GetRes(_pref, 6346);

                        DivCurrentPicture.Controls.Add(labCurrentPic);
                        HtmlGenericControl ulCadreEtBtn = new HtmlGenericControl("ul");
                        DivCurrentPicture.Controls.Add(ulCadreEtBtn);

                        HtmlGenericControl liCadreEtBtn = new HtmlGenericControl("li");
                        ulCadreEtBtn.Controls.Add(liCadreEtBtn);

                        Panel vcCadre = new Panel();
                        vcCadre.CssClass = "vcCadreFile";
                        vcCadre.Controls.Add(vcPhoto);
                        liCadreEtBtn.Controls.Add(vcCadre);

                        #region Bouton Supprimer
                        liCadreEtBtn = new HtmlGenericControl("li");
                        ulCadreEtBtn.Controls.Add(liCadreEtBtn);
                        #endregion
                    }
                    else
                        DivCurrentPicture.Visible = false;


                }
                catch (eEndResponseException)
                {

                }
                catch (Exception e1)
                {
                    // Demande #33 532 - Message d'avertissement si on tombe sur l'erreur explicite "[response status:503]qps rate exceeded"
                    if (e1.Message.Contains("503") || e1.Message.ToLower().Contains("qps rate exceeded"))
                    {
                        //string onOkFct = "function() { top.modalImage.hide(); };";
                        string onOkFct = "null";

                        sbJsInput
                            .Append("top.setWait(false);")
                            .Append("top.eAlert(2, ")
                                .Append("'").Append(eResApp.GetRes(_pref, 6735).Replace("'", "\\'")).Append("', ")
                                .Append("'").Append(eResApp.GetRes(_pref, 6735).Replace("'", "\\'")).Append("', ")
                                .Append("'").Append(eResApp.GetRes(_pref, 6734).Replace("'", "\\'")).Append("', ")
                                .Append("null, null, ").Append(onOkFct)
                            .Append(");");
                        return;
                    }

                    //Avec exception
                    String sDevMsg = String.Concat("Erreur sur eGoogleImageGet.aspx -PAGE_LOAD = -> : ", Environment.NewLine);
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Message Exception : ", e1.Message,
                        Environment.NewLine, "Exception StackTrace :", e1.StackTrace
                        );

                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                        String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  
                        eResApp.GetRes(_pref, 72),  //   titre
                        String.Concat(sDevMsg)

                        );

                    try
                    {
                        if (_bFromeUpdater)
                            LaunchError();
                        else
                            LaunchErrorHTML(true, null, "top.setWait(false);top.modalImage.hide();");
                    }
                    catch (eEndResponseException)
                    {

                    }
                }
            }

        }


        /// <summary>
        /// Action à la validation, permettant l'ajout de l'image sélectionnée
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void cmdSend_Click(object sender, EventArgs e)
        {

            try
            {
                String urlImageFullFileName = String.Empty;
                String sPhysImageFullFileName = String.Empty;

                String error = String.Empty;

                if (FromLocal.Checked == true)
                {
                    #region Si récupération d'un fichier sur le poste directement de choisi
                    Upload(filMyFile.PostedFile, out urlImageFullFileName, out error, out sPhysImageFullFileName);
                    if (!String.IsNullOrEmpty(error))
                        throw new Exception("Erreur lors de l'upload du fichier : " + error);

                    #endregion
                }
                else
                {
                    #region Sinon c'est que l'on souhaite récupérer une image en ligne
                    if (!String.IsNullOrEmpty(tbImageGoogle.Value))
                    {
                        //Lecture du contenu de l'url de l'image choisie
                        System.Net.WebClient fileReader = new System.Net.WebClient();
                        byte[] myData = fileReader.DownloadData(tbImageGoogle.Value.ToString());

                        urlImageFullFileName = CreateFinalPicture(myData, out sPhysImageFullFileName, out error);
                        if (!String.IsNullOrEmpty(error))
                            throw new Exception("Erreur lors de l'enregistrement du fichier : " + error);
                    }
                    #endregion
                }

                if (!String.IsNullOrEmpty(urlImageFullFileName))
                {
                    urlImageFullFileName.Insert(0, "./");
                }

                //Action JS de sortie suite à la validation de la popup
                sbJsInput.Append("onImageSubmit('")
                        .Append(urlImageFullFileName.Replace("'", "\\'"))
                        .Append("','")
                        .Append(sPhysImageFullFileName.Replace("'", "\\'"))
                        .Append("');");
            }
            catch (eEndResponseException)
            {

            }
            catch (Exception e1)
            {

                //Avec exception
                String sDevMsg = String.Concat("Erreur sur eGoogleImageGet.aspx - cmdSend_Click = -> : ", Environment.NewLine);
                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Message Exception : ", e1.Message,
                    Environment.NewLine, "Exception StackTrace :", e1.StackTrace
                    );

                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                        String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  
                    eResApp.GetRes(_pref, 72),  //   titre
                    String.Concat(sDevMsg)

                    );
                try
                {
                    if (_bFromeUpdater)
                        LaunchError();
                    else
                        LaunchErrorHTML(true, null, "top.setWait(false);top.modalImage.hide();");
                }
                catch (eEndResponseException)
                {

                }
            }
        }

        /// <summary>
        /// Suppression de l'image sélectionnée
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void cmdDelete_Click(object sender, EventArgs e)
        {
            try
            {
                #region Si aucune image n'a été spécifié, on supprime l'image actuel

                DeleteAvatarFiles(_sCurrentFileName);

                UpdateAvatarName("");

                sbJsInput.Append("onImageSubmit('','');");
                DivCurrentPicture.Visible = false;



                #endregion
            }
            catch (eEndResponseException)
            {

            }
            catch (Exception e1)
            {

                //Avec exception
                String sDevMsg = String.Concat("Erreur sur eGoogleImageGet.aspx - cmdDelete_Click = -> : ", Environment.NewLine);
                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Message Exception : ", e1.Message,
                    Environment.NewLine, "Exception StackTrace :", e1.StackTrace
                    );

                ErrorContainer = eErrorContainer.GetDevUserError(
    eLibConst.MSG_TYPE.CRITICAL,
    eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
        String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  
    eResApp.GetRes(_pref, 72),  //   titre
    String.Concat(sDevMsg)

    );

                try
                {

                    if (_bFromeUpdater)
                        LaunchError();
                    else
                        LaunchErrorHTML(true, null, "top.setWait(false);top.modalImage.hide();");
                }
                catch (eEndResponseException)
                {

                }
            }
        }

        /// <summary>
        /// Upload du fichier image
        /// </summary>
        /// <param name="file">Fichier</param>
        /// <param name="url">URL</param>
        /// <param name="error">Erreur</param>
        /// <returns>Upload réussi ou non</returns>
        Boolean Upload(HttpPostedFile file, out String url, out String error, out string sPhyName)
        {
            url = String.Empty;
            sPhyName = "";
            error = String.Empty;

            if (file != null)
            {
                if (file.ContentLength > 0)
                {
                    if (
                        !file.FileName.ToLower().EndsWith(".jpg") &&
                        !file.FileName.ToLower().EndsWith(".png") &&
                        !file.FileName.ToLower().EndsWith(".gif")
                        )
                    {
                        error = eResApp.GetRes(_pref, 1545);
                    }
                    int nFileLen = file.ContentLength;


                    if (nFileLen > eLibConst.AVATAR_SIZE * 1024 * 1024) // 5 Mo
                    {
                        error = eResApp.GetRes(_pref, 1537).Replace("<CURRENT>", String.Concat(nFileLen / 1024 / 1024, " M")).Replace("<MAX>", String.Concat(eLibConst.AVATAR_SIZE, " M"));
                    }

                    if (nFileLen > 0)
                    {
                        byte[] myData = new byte[nFileLen];
                        file.InputStream.Read(myData, 0, nFileLen);

                        url = CreateFinalPicture(myData, out sPhyName, out error);

                        if (error.Length == 0)
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Crée les images de l'avatar en 3 versions (vignette, mobile, original) à partir du flux d'image récupéré
        /// </summary>
        /// <param name="myData">Contenu de l'image à convertir en Vignette</param>
        /// <returns>Url de l'image</returns>
        private String CreateFinalPicture(byte[] myData, out string sBaseNameFull, out string error)
        {
            error = String.Empty;

            //Recherche de l'avatar existant.
            string sBaseName;
            if (_nFileId == 0)
                sBaseName = String.Concat(_nTab, "_", eLibTools.GetToken(5), "_", DateTime.Now.ToString("ddMMyyyyhhmmss"));
            else if (_nTab == (int)TableType.USER)
                sBaseName = String.Concat(_nTab, "_", _nFileId);
            else
                sBaseName = String.Concat(_nTab, "_", _nFileId, "_", DateTime.Now.ToString("ddMMyyyyhhmmss"));


            eLibTools.PictureWebFullFileName webFullFileName = null;
            String baseFilePath = String.Concat(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.FILES, _pref), @"\");

            String originalFileName = String.Concat(sBaseName, ".jpg");
            sBaseNameFull = originalFileName;

            String mobileFileName = String.Concat(sBaseName, eLibConst.MOBILE_SUFFIX, ".jpg");
            String thumbnailFileName = String.Concat(sBaseName, eLibConst.THUMB_SUFFIX, ".jpg");

            String originalFilePath = String.Concat(baseFilePath, originalFileName);
            String mobileFilePath = String.Concat(baseFilePath, mobileFileName);
            String thumbnailFilePath = String.Concat(baseFilePath, thumbnailFileName);

            try
            {
                MemoryStream msImg = new MemoryStream(myData);
                System.Drawing.Image image = System.Drawing.Image.FromStream(msImg);
                lblError.Text = String.Empty;
                eLibTools.RotateImageFromEXIF(ref image);

                #region suppression du fichier de destination s'il existe déjà
                DeleteAvatarFiles(_sCurrentFileName);
                #endregion

                eLibTools.CreatePictureVersions(image, sBaseName, baseFilePath, _pref.AppExternalUrl, _pref.GetBaseName, out webFullFileName);

                UpdateAvatarName(originalFileName);
            }
            catch (Exception e1)
            {
                error = e1.Message;
            }
            finally
            {

            }

            return webFullFileName?.thumbnailFileName ?? String.Empty;
        }

        private void DoResponse(Dictionary<string, string> returnValues)
        {
            XmlDocument xmlDocReturn = new XmlDocument();

            XmlNode xmlNodeEdnResult = xmlDocReturn.CreateElement("ednResult");
            xmlDocReturn.AppendChild(xmlNodeEdnResult);

            XmlNode xmlNodeSuccess = xmlDocReturn.CreateElement("success");
            xmlNodeEdnResult.AppendChild(xmlNodeSuccess);

            XmlNode xmlNode = null;

            #region Gestion d'erreur standard du Manager (ErrorContainer)

            if (ErrorContainer.IsSet)
            {
                xmlNode = xmlDocReturn.CreateElement("error");
                if (ErrorContainer.DebugMsg.Length > 0)
                    xmlNode.InnerText = ErrorContainer.DebugMsg;
                xmlNodeEdnResult.AppendChild(xmlNode);
            }

            #endregion

            #region Valeurs retournées, dont erreurs additionnelles spécifiques si définies
            foreach (KeyValuePair<string, string> kvp in returnValues)
            {
                xmlNode = xmlDocReturn.CreateElement(kvp.Key);
                xmlNode.InnerText = kvp.Value;
                xmlNodeEdnResult.AppendChild(xmlNode);
            }
            #endregion

            xmlNodeSuccess.InnerText = (returnValues.ContainsKey("msg") == false ? "1" : "0");

            LaunchError();
            RenderResult(RequestContentType.XML, delegate () { return xmlDocReturn.OuterXml; });
        }
    }
}
