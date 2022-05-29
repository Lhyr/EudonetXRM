using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Model;
using EudoQuery;
using System;
using System.IO;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Factory pour la gestion des formulaires.
    /// </summary>
    public class FormularFactory
    {
        /// <summary>
        /// Construit un objet eFormular à partir des données json envoyées 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pref"></param>
        /// <param name="formularModel"></param>
        /// <returns></returns>
        public static eFormular GetFormularFromModel(int id, ePref pref, FormularModel formularModel)
        {
            return eFormular.RetrieveParams(id, pref, formularModel);
        }

        /// <summary>
        /// Construit un modèle qui sera envoyé côté client en json à partir de l'objet eFormular
        /// </summary>
        /// <param name="_pref"></param>
        /// <param name="formularId"></param>
        /// <param name="parentFileId"></param>
        /// <returns></returns>
        public static FormularResponseModel GetFormularModel(ePref _pref, int formularId, int parentFileId)
        {
            //on crée un objet de type eFormular pour récupérer les données du formulaire
            var _oFormular = new eFormular(formularId, _pref, parentFileId);
            _oFormular.Init();

            //on remplit l'objet réponse 
            FormularResponseModel formular = new FormularResponseModel();
            formular.FormularName = _oFormular.Label;
            formular.FormularId = _oFormular.FormularId;
            formular.Body = _oFormular.Body;
            formular.Css = _oFormular.BodyCss;
            formular.FormularLang = _oFormular.Lang;
            formular.FormularStatus = _oFormular.Status;//Tâche #2 868: on récupère le status du formulaire
                                                        //Tâche #2 847: chargement des params de la page de remerciement
            formular.SubmissionBody = _oFormular.BodySubmission;
            formular.SubmissionBodyCss = _oFormular.BodySubmissionCss;
            formular.SubmissionRedirectUrl = _oFormular.SubmissionRedirectUrl;
            formular.FormularExtendedParam = SerializerTools.JsonDeserialize<FormularExtendedParam>(_oFormular.ExtendedParam);
            formular.RewrittenURL = _oFormular.GetRewrittenURL(true, "");
            formular.ScriptIntegration = _oFormular.GetIntegrationScript(true, "");

            //Tâche #3 342 le chargement des droits de visualisation
            #region Permissions de visualisation
            formular.IsPublic = _oFormular.IsPublic;

            eudoDAL eDal = null;
            try
            {
                eDal = eLibTools.GetEudoDAL(_pref);
                eDal.OpenDatabase();
                _oFormular.ViewPerm.LoadUserPermLabel(eDal);
                formular.ViewPerm = new AdvFormularPermission(_oFormular.ViewPerm);

                _oFormular.UpdatePerm.LoadUserPermLabel(eDal);
                formular.UpdatePerm = new AdvFormularPermission(_oFormular.UpdatePerm);
            }
            catch
            {
                throw;
            }
            finally
            {
                eDal?.CloseDatabase();
            }
            #endregion Permissions de visualisation

            //Tâche #3 367 récupération de date de début de d'expiration d'un formulaire
            if (_oFormular.ExpireDate != null)
                formular.ExpireDate = DateTime.Compare(_oFormular.ExpireDate.Value, DateTime.MaxValue) == 0 ? null : (DateTime?)_oFormular.ExpireDate;
            if (_oFormular.StartDate != null)
                formular.StartDate = DateTime.Compare(_oFormular.StartDate.Value, DateTime.MaxValue) == 0 ? null : (DateTime?)_oFormular.StartDate;

            formular.MsgDateStart = _oFormular.MsgDateStart;
            formular.MsgDateEnd = _oFormular.MsgDateEnd;
            formular.MetaTitle = _oFormular.MetaTitle;
            formular.MetaDescription = _oFormular.MetaDescription;
            formular.MetaImgURL = _oFormular.MetaImgURL;
            return formular;
        }

        /// <summary>
        /// Ajout d'une image pour les réseaux sociaux
        /// </summary>
        /// <param name="_pref"></param>
        /// <param name="Name"></param>
        /// <param name="imageFile"></param>
        /// <returns></returns>
        public static string AddImageFormular(ePref _pref, string Name, HttpPostedFileBase imageFile)
        {
            Boolean uploadOk = false;
            string imagePostedfile = Name;

            if (!string.IsNullOrEmpty(Name))
            {
                eLibConst.IMAGE_TYPE imageType = eLibConst.IMAGE_TYPE.TXT_URL;
                eLibConst.FOLDER_TYPE datasFolderType = eLibConst.FOLDER_TYPE.FILES;
                ImageStorage imageStorageType = ImageStorage.STORE_IN_FILE;
                String filePath = string.Empty;
                byte[] data = null;
                String fileName = "";
                int fileSize = 0;
                String contentType = "";

                //Indique dans quel dossier doit être stocké le type d'image passé en paramètre
                datasFolderType = eLibTools.GetFolderTypeFromImageType(imageType);

                //Retourne le chemin physique du dossier, le crée s'il n'existe pas
                filePath = String.Concat(eModelTools.GetPhysicalDatasPath(datasFolderType, _pref), @"\");
                eAbstractImage image = eAbstractImage.GetImage(_pref, imageType, 0, 0, 0, "sharringimage");
                fileName = Path.GetFileName(Name);
                fileSize = imageFile.ContentLength;
                contentType = imageFile.ContentType;

                image.SetFileInfos(fileName, fileSize, contentType);

                if (imageFile != null)
                {
                    data = new byte[fileSize];
                    imageFile.InputStream.Read(data, 0, fileSize);
                }

                if (data != null && data.Length > 0)
                {
                    uploadOk = image.Save(data);
                }

                if (uploadOk)
                    imagePostedfile = image.ImageURL;
            }

            return imagePostedfile;
        }

    }
}
