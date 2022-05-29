using Com.Eudonet.Internal;
using Com.Eudonet.Merge;
using EudoQuery;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Page qui se charge d'ouvre une annexe en lecture 
    /// </summary>
    public partial class ePjDisplay : eEudoPage
    {
        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return null;
        }

        /// <summary>
        /// Load de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Chemin virtuel du fichier
                StringBuilder sbPath = new StringBuilder();
                string srcPj = null;
                // Nom du fichier
                String sFileName = String.Empty;

                String sDispFrom = _requestTools.GetRequestQSKeyS("dispFrom") ?? "pj";

                eLibConst.FOLDER_TYPE folder = eLibConst.FOLDER_TYPE.ANNEXES;
                String sFolderCplt = "";

                Int32 pjId = 0;
                int descId = 0;
                int fileId = 0;
                int pjType = 0;
                int iFolder = 0;

                Boolean bUnsafeFilename = false;

                if (sDispFrom.StartsWith("pj"))
                {
                    if (Request.QueryString["pj"] == null)
                    {
                        LaunchError(eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            eResApp.GetRes(_pref, 6342),
                            eResApp.GetRes(_pref, 416),
                            "Paramètres invalides dans ePjDisplay.aspx : pj null"));
                        return;
                    }

                    if (Request.QueryString["descId"] == null)
                    {
                        LaunchError(eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            eResApp.GetRes(_pref, 6342),
                            eResApp.GetRes(_pref, 416),
                            "Paramètres invalides dans ePjDisplay.aspx : descId null"));
                        return;
                    }

                    if (Request.QueryString["fileId"] == null)
                    {
                        LaunchError(eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            eResApp.GetRes(_pref, 6342),
                            eResApp.GetRes(_pref, 416),
                            "Paramètres invalides dans ePjDisplay.aspx : fileId null"));
                        return;
                    }

                    Int32.TryParse(Request.QueryString["pj"].ToString(), out pjId);
                    int.TryParse(Request.QueryString["descId"].ToString(), out descId);
                    int.TryParse(Request.QueryString["fileId"].ToString(), out fileId);

                    if (Request.QueryString.AllKeys.Contains("pjtype"))
                        int.TryParse(Request.QueryString["pjtype"].ToString(), out pjType);

                    // Le FileID n'est pas vérifié ici, afin d'autoriser la visualisation des PJ de fiches en cours de création (FileID 0).
                    // Si le FileID est passé à 0 dans la Query String, la fonction GetNamePJ() appellée ci-dessous se chargera de vérifier
                    // que ce FileID est bien à 0 pour la PJ en question. Dans le cas contraire, l'accès sera refusé, afin d'éviter qu'une
                    // personne puisse afficher n'importe quelle PJ en précisant uniquement le pjId et le descId avec un fileId à 0.
                    // pour
                    if (pjId == 0 || descId == 0)
                    {
                        LaunchError(eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            eResApp.GetRes(_pref, 6342),
                            eResApp.GetRes(_pref, 416),
                            String.Concat(
                                "Paramètres invalides dans ePjDisplay.aspx",
                                pjId == 0 ? " - pjId à 0 (" + Request.QueryString["pj"].ToString() + ")" : String.Empty,
                                descId == 0 ? " - descId à 0 (" + Request.QueryString["descId"].ToString() + ")" : String.Empty
                            )
                        )
                        );
                        return;
                    }

                    // Recherche du nom de la PJ à partir des ID, et vérifications de sécurité
                    // Avec renvoi du message d'erreur si nécessaire
                    ePJ myPJ = null;
                    sFileName = ePJ.GetNamePJ(_pref, pjId, descId, fileId, out myPJ);
                    if (myPJ != null && myPJ.ErrorContainer != null)
                        LaunchError(myPJ.ErrorContainer);

                    if (pjType == PjType.REPORTS.GetHashCode())
                        folder = eLibConst.FOLDER_TYPE.REPORTS;

                    //Bug #60651 - CNA: on initialise une pj sécurisé uniquement pour la visonneuse de pj
                    PjBuildParam paramPj = new PjBuildParam()
                    {
                        AppExternalUrl = eLibTools.GetAppUrl(Request),
                        Uid = _pref.DatabaseUid,
                        TabDescId = descId,
                        PjId = pjId,
                        UserId = _pref.UserId,
                        UserLangId = _pref.LangId
                    };

                    srcPj = ExternalUrlTools.GetLinkPJ(paramPj, withIframe: false);
                }
                else if (sDispFrom == "fieldfiles")
                {
                    if (!_requestTools.AllKeysQS.Contains("file") || !_requestTools.AllKeysQS.Contains("folder"))
                    {
                        LaunchError(eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            eResApp.GetRes(_pref, 6342),
                            eResApp.GetRes(_pref, 416),
                            String.Concat(
                                "Paramètres invalides dans ePjDisplay.aspx",
                                !_requestTools.AllKeysQS.Contains("file") ? " - file manquant" : String.Empty,
                                !_requestTools.AllKeysQS.Contains("folder") ? " - folder manquant" : String.Empty
                            )
                        )
                        );
                        return;
                    }

                    sFileName = Request.QueryString["file"].ToString();
                    sFolderCplt = Request.QueryString["folder"].ToString();

                    if (String.IsNullOrEmpty(sFileName) || String.IsNullOrEmpty(sFolderCplt))
                    {
                        LaunchError(eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            eResApp.GetRes(_pref, 6342),
                            eResApp.GetRes(_pref, 416),
                            String.Concat(
                                "Paramètres invalides dans ePjDisplay.aspx",
                                String.IsNullOrEmpty(sFileName) ? " - sFileName vide" : String.Empty,
                                String.IsNullOrEmpty(sFolderCplt) ? " - sFolderCplt vide" : String.Empty
                            )
                        )
                        );
                        return;
                    }

                    if (!String.IsNullOrEmpty(sFolderCplt) && Int32.TryParse(sFolderCplt, out iFolder))
                        folder = (eLibConst.FOLDER_TYPE)iFolder;
                    else
                        folder = eLibConst.FOLDER_TYPE.FOLDERS;

                }

                if (sFileName == null || sFileName.Length == 0)
                {
                    LaunchError(eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6342),
                        eResApp.GetRes(_pref, 416),
                        String.Concat(
                            "Paramètres invalides dans ePjDisplay.aspx",
                            sFileName == null ? " - sFileName null" : "- sFileName vide"
                        )
                    )
                    );
                    return;
                }

                bool bUseViewer = _requestTools.GetRequestQSKeyB("useviewer") ?? false;
                bool bIsIE8 = _requestTools.GetRequestFormKeyB("isIE8") ?? false;

                string sObjectID = _requestTools.AllKeys.Contains("objectID") ? Request.Form["objectID"].ToString() : String.Empty;

                bool bIsURLPJ = (pjType == PjType.FTP.GetHashCode() || pjType == PjType.MAIL.GetHashCode() || pjType == PjType.WEB.GetHashCode());
                string sFilePath = String.Empty;

                if (bIsURLPJ)
                {
                    if (pjType == PjType.FTP.GetHashCode())
                    {
                        if (!sFileName.StartsWith("ftp://") && !sFileName.StartsWith("ftps://") && !sFileName.StartsWith("sftp://"))
                            sbPath.Append("ftp://");
                    }
                    if (pjType == PjType.MAIL.GetHashCode())
                    {
                        if (!sFileName.StartsWith("mailto:"))
                            sbPath.Append("mailto:");
                    }
                    if (pjType == PjType.WEB.GetHashCode())
                    {
                        if (!sFileName.StartsWith("http://") && !sFileName.StartsWith("https://") && !sFileName.StartsWith("ftp://") && !sFileName.StartsWith("ftps://") && !sFileName.StartsWith("sftp://") && !sFileName.StartsWith("mailto:"))
                            sbPath.Append("http://");
                    }
                    sbPath.Append(sFileName);
                }
                else
                {
                    sbPath.Append(eLibTools.GetWebDatasPath(folder, _pref.GetBaseName));

                    if (sbPath.ToString().ToCharArray()[sbPath.Length - 1] != '/')
                        sbPath.Append("/");

                    if (sFolderCplt.Length > 0 && !folder.Equals(eLibConst.FOLDER_TYPE.MODELES))
                        sbPath.Append(sFolderCplt).Append("/");

                    // Chemin complet du fichier
                    sFilePath = String.Empty;
                    try
                    {
                        sFilePath = HttpContext.Current.Server.MapPath(String.Concat(sbPath.ToString(), sFileName));

                        if (sFileName.Contains('+'))
                            bUnsafeFilename = true;
                    }
                    // En cas d'erreur de conversion de chemin du fichier (ex : caractères invalides), on renverra le nom de fichier
                    // erroné dans le else { } plus bas avec un message d'erreur 404
                    catch
                    {
                        sFilePath = String.Empty;
                    }

                    //Url encodé (qui n'est utilisable que pour le chemin web)
                    sbPath.Append(Uri.EscapeDataString(sFileName));
                }

                // On va vérifier que le fichier existe physiquement sur le serveur
                if (bIsURLPJ || System.IO.File.Exists(sFilePath))
                {
                    // Si on demande à visionner le fichier...
                    if (bUseViewer || bUnsafeFilename)
                    {
                        string sMimeType = eLibTools.GetMimeTypeFromExtension(Path.GetExtension(sFileName));
                        string sMimeTypeCategory = "application";
                        string sMimeTypeSubCategory = "octet-stream";
                        try
                        {
                            string[] sMimeTypeParts = sMimeType.Split('/');
                            sMimeTypeCategory = sMimeTypeParts[0];
                            sMimeTypeSubCategory = sMimeTypeParts[1];
                        }
                        catch
                        {
                            sMimeTypeCategory = "application";
                            sMimeTypeSubCategory = "octet-stream";
                        }

                        #region Définition de la taille de la visionneuse
                        int nViewerWidth = 0;
                        int nViewerHeight = 0;
                        int.TryParse(Request.Form["Width"], out nViewerWidth);
                        int.TryParse(Request.Form["Height"], out nViewerHeight);
                        if (nViewerWidth == 0)
                            nViewerWidth = 940;
                        else
                            nViewerWidth -= 20;
                        if (nViewerHeight == 0)
                            nViewerHeight = 440;
                        else
                            nViewerHeight -= 100;
                        #endregion

                        #region Définition du contenu HTML à utiliser
                        string strOnFrameSizeChangeScript = String.Concat(
                            // EVENEMENT LORS DU REDIMENSIONNEMENT DE LA MODALDIALOG
                            "<script type=\"text/javascript\" language=\"javascript\">",
                            "function onFrameSizeChange(nNewWidth, nNewHeight) {", Environment.NewLine,
                                "var oViewerContainer = document.getElementById('viewer');", Environment.NewLine,
                                "if (oViewerContainer) {", Environment.NewLine,
                                    "oViewerContainer.style.width = (nNewWidth - 20) + 'px';", Environment.NewLine,
                                    "if (oViewerContainer.getAttribute('filetype') != 'audio') {", Environment.NewLine,
                                        "oViewerContainer.style.height = (nNewHeight - 30) + 'px';", Environment.NewLine,
                                    "}", Environment.NewLine,
                                "}", Environment.NewLine,
                            "}", Environment.NewLine,
                            "</script>"
                        );
                        string strPageHeader = String.Concat(
                            "<!DOCTYPE html><html><head><title></title></head><body>", strOnFrameSizeChangeScript
                        );
                        string strPageFooter = String.Concat(
                            "</body></html>"
                        );
                        string strContainerAttributes = String.Concat(
                             "id=\"viewer\" filetype=\"", sMimeTypeCategory, "\" style=\"width: ", nViewerWidth, "px;",
                             (sMimeTypeCategory != "audio" ? String.Concat("height:", nViewerHeight, "px;\"") : "\"")
                        );
                        #endregion

                        string fileExt = Path.GetExtension(sFileName);

                        string strViewerType = sMimeTypeCategory;

                        // Pour certaines extensions de fichiers, on utilise une visionneuse spécifique sans se fier spécialement
                        // au type MIME
                        string[] strTextExtensions = { ".sql" };
                        string[] strTextMimeSubCats = { "xml" };
                        if (strTextExtensions.Contains(fileExt) || strTextMimeSubCats.Contains(sMimeTypeSubCategory))
                            strViewerType = "text";

                        // Pour les URL vers des fichiers texte ou dangereux (.sql, .aspx, etc.), on affiche un lien permettant de
                        // l'ouvrir via le navigateur, selon ses contraintes de sécurité
                        if (bIsURLPJ && strViewerType == "text")
                            strViewerType = "application";
                        if (bUnsafeFilename)
                            strViewerType = "unsafe";
                        if (bIsIE8 && strViewerType != "text")
                            strViewerType = "application";

                        switch (strViewerType)
                        {
                            case "text":
                                TextReader tr = new StreamReader(sFilePath);
                                string textContents = tr.ReadToEnd();
                                tr.Close();
                                tr.Dispose();

                                // Si le poids du fichier dépasse une certaine taille, on affiche la fenêtre maximisée
                                // A ajuster en fonction de la taille de la modal dialog
                                int nMaxLineCount = 29;
                                int nMaxLineLength = 128;
                                bool bMaximize = textContents.Length > ((nMaxLineLength + 2) * nMaxLineCount) - 2; // +2 pour compter les retours chariot - 1 sur la dernière ligne
                                if (!bMaximize)
                                {
                                    string[] textContentsArray = textContents.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                                    bMaximize = textContentsArray.Length > nMaxLineCount || textContentsArray.Max(s => s.Length) > nMaxLineLength;
                                }
                                if (bMaximize)
                                    strPageHeader = strPageHeader.Replace("<body>", "<body onload=\"top.modalFileViewer.MaxOrMinModal();\">");

                                Response.Write(
                                    String.Concat(
                                        strPageHeader,
                                        "<form><textarea readonly=\"readonly\" ", strContainerAttributes, ">",
                                        textContents,
                                        "</textarea></form>",
                                        strPageFooter
                                    )
                                );
                                break;
                            case "audio":
                            case "video":
                                if (sMimeTypeCategory == "audio")
                                    strPageHeader = strPageHeader.Replace("<body>", "<body onload=\"top.modalFileViewer.resizeTo(960, 275);\">");
                                Response.Write(
                                    String.Concat(
                                        strPageHeader,
                                        "<", sMimeTypeCategory, " ", strContainerAttributes, " controls>",
                                            "<source src=\"", String.IsNullOrEmpty(srcPj) ? sbPath.ToString() : srcPj, "\" type=\"", sMimeType, "\">",
                                            "<a href=\"", String.IsNullOrEmpty(srcPj) ? sbPath.ToString() : srcPj, "\" target=\"_blank\">", sFileName, "</a>",
                                        "</", sMimeTypeCategory, ">",
                                        strPageFooter
                                    )
                                );
                                break;
                            case "image":

                                PjBuildParam pjBuildParam = new PjBuildParam()
                                {
                                    AppExternalUrl = _pref.AppExternalUrl,
                                    Uid = _pref.DatabaseUid,
                                    TabDescId = descId,
                                    PjId = pjId,
                                    UserId = _pref.UserId,
                                    UserLangId = _pref.LangId

                                };
                                string sLink = ExternalUrlTools.GetLinkPJ(pjBuildParam, withIframe: false);

                                Response.Write(
                                        String.Concat(
                                            strPageHeader,
                                            "<script> window.location = '" + sLink + "'</script>",
                                            strPageFooter
                                        )
                                    );
           
                                break;
                            // #53 256 : depuis la suppression de viewerJS, on redirige vers le fichier directement pour les types "application"
                            // précédemment pris en charge, ex : PDF, DOC, XLS... qu'on affiche donc dans un nouvel onglet en repassant la main à
                            // la fonction SetFldPj sur la page appelante, puis en fermant automatiquement la fenêtre.
                            // En cas d'échec, on propose un lien permettant d'accéder à la PJ
                            case "application":
                            default:
                                if (sObjectID.Length > 0)
                                    Response.Write(
                                        String.Concat(
                                            strPageHeader.Replace("<body>", String.Concat("<body style=\"font-family: Verdana; font-size: 8pt;\" onload=\"try { top.SetFldPj(top.document.getElementById('", sObjectID, "')); top.modalFileViewer.hide(); } catch (ex) { top.modalFileViewer.resizeTo(960, 150); document.getElementById('innerLink').style.display = 'block'; }\">")),
                                            "<a id=\"innerLink\" href=\"", String.IsNullOrEmpty(srcPj) ? sbPath.ToString() : srcPj, "\" target=\"_blank\" style=\"display: none\">", eResApp.GetRes(_pref, 587), " ", sFileName, "</a>",
                                            strPageFooter
                                        )
                                    );
                                else
                                {
                                    if (!bIsURLPJ)
                                    {
                                        Response.Clear();
                                        Response.ClearContent();
                                        Response.ClearHeaders();
                                        Response.Buffer = true;

                                        FileStream myFileStream = new FileStream(sFilePath, FileMode.Open);
                                        long fileSize = myFileStream.Length;
                                        byte[] buffer = new byte[(int)fileSize];
                                        myFileStream.Read(buffer, 0, (int)fileSize);
                                        myFileStream.Close();

                                        Response.AddHeader("Content-Length", fileSize.ToString());
                                        Response.AddHeader("Content-Disposition", "inline; filename=" + sFileName.Replace(" ", "_"));
                                        Response.ContentType = sMimeType;

                                        Response.BinaryWrite(buffer);
                                        //Response.End();
                                        Context.ApplicationInstance.CompleteRequest();
                                    }
                                    else
                                        Response.Redirect(String.IsNullOrEmpty(srcPj) ? sbPath.ToString() : srcPj);
                                }
                                break;
                        }
                        /*}*/
                    }
                    // Sinon, redirection vers le fichier
                    else
                    {
                        Response.Redirect(String.IsNullOrEmpty(srcPj) ? sbPath.ToString() : srcPj);
                    }
                }
                else
                {
                    HtmlImage img404 = new HtmlImage();
                    img404.Src = @".\themes\default\images\404.png";
                    divImg.Controls.Add(img404);

                    sFileName = String.Concat("\"", sFileName, "\"");
                    String message = Eudonet.Internal.eResApp.GetRes(_pref, 1317).Replace("<TAB>", sFileName);
                    spTxtPj.InnerText = message;
                }
            }
            catch (eEndResponseException) { Response.End(); }
            catch (ThreadAbortException) { }    // Laisse passer le response.end du RenderResult
            catch (Exception ex)
            {
                try
                {
                    LaunchErrorHTML(true, eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, "Ouverture d'annexe", eResApp.GetRes(_pref, 6342), eResApp.GetRes(_pref, 72), ex.ToString()));
                }
                catch (eEndResponseException)
                {

                }
                catch (ThreadAbortException) { }
            }
        }
    }
}
