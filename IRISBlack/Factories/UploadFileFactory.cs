using Com.Eudonet.Common.Cryptography;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.mgr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Factory pour l'upload des fichiers.
    /// </summary>
    public class UploadFileFactory
    {
        #region Properties
        /// <summary>
        /// Le fichier uploadé
        /// </summary>
        HttpPostedFileBase FileUpload { get; set; }
        /// <summary>
        /// liste des extensions à inclure
        /// </summary>
        List<string> LstExtensions { get; set; } = null;
        /// <summary>
        /// Taille max en Ko.
        /// </summary>
        int MaxLength { get; set; } = 0;
        #endregion

        #region Constructor

        /// <summary>
        /// Constructeur uniquement pour le fichier posté.
        /// </summary>
        /// <param name="fu"></param>
        private UploadFileFactory(HttpPostedFileBase fu)
        {
            FileUpload = fu;
        }
        /// <summary>
        /// Constructuer pour le fichier posté, la liste des extensions et la taille en ko.
        /// </summary>
        /// <param name="fu"></param>
        /// <param name="lstExt"></param>
        /// <param name="length"></param>
        private UploadFileFactory(HttpPostedFileBase fu, List<string> lstExt, int length)
            : this(fu)
        {
            LstExtensions = lstExt;
            MaxLength = length;
        }
        #endregion

        #region static initializers
        /// <summary>
        /// Initialiseur statique de la classe avec tous les paramètres
        /// </summary>
        /// <param name="fu"></param>
        /// <param name="lstExt"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static UploadFileFactory initUploadFileFactory(HttpPostedFileBase fu, List<string> lstExt, int length)
        {
            return new UploadFileFactory(fu, lstExt, length);
        }

        /// <summary>
        /// Initialiseur statique de la classe avec seulement le fichier.
        /// </summary>
        /// <param name="fu"></param>
        /// <returns></returns>
        public static UploadFileFactory initUploadFileFactory(HttpPostedFileBase fu)
        {
            return new UploadFileFactory(fu);
        }


        #endregion

        #region Public

        /// <summary>
        /// Vérification d'upload d'un fichier, on vérifie la présence d'un fichier, la taille maximum autorisée et les extensions
        /// </summary>
        /// <param name="messageOut">Massage en cas d'echec</param>
        /// <returns>True ou false</returns>
        public bool CheckFileToUpload(out string messageOut)
        {
            messageOut = string.Empty;

            // Vérification de la présence d'un fichier à uploader
            if (FileUpload.ContentLength == 0
                || (MaxLength > 0 && (FileUpload.ContentLength > MaxLength))
               )
            {
                messageOut = !(FileUpload.ContentLength == 0) ? "Taille du fichier trop volumineuse" : "";
                return false;
            }

            if (string.IsNullOrEmpty(eLibTools.GetServerConfig("allowedextensions")))
                return true;

            // récupération de l'extension du fichier 
            string extFile = Path.GetExtension(FileUpload.FileName);
            //récupération des Extentions niveau serveur
            string sServeurWideAllowed = CryptoTripleDES.Decrypt(eLibTools.GetServerConfig("allowedextensions"), CryptographyConst.KEY_CRYPT_LINK1)
                .ToLower()
                .Replace("*.", ".");

            List<string> lstAllowedExtServer = sServeurWideAllowed.Split(';').ToList();

            // Vérification des extensions
            // Les fichiers doivent répondre au critères serveur et à ceux spécifique
            // L'echec de l'un ou l'autre abouti au blocage du fichier.
            if (extFile.Length > 0 // Les fichiers sans extensions sont autorisés
            && (!(lstAllowedExtServer.Contains(".*") || lstAllowedExtServer.Contains(extFile.ToLower())))   // Si on autorise tout côté serveur et qu'au Niveau serveur  : ne contient l'extension du fichier          
            || (LstExtensions != null && LstExtensions.Count > 0
                && !LstExtensions.Contains(extFile.ToLower()) && !LstExtensions.Contains(".*")))   // Niveau spécifique  : l'extension du fichier n'est pas dans le masque fourni
            {
                //On a pas de pref, on tente de récupérer la langue depuis le cookie

                int idLang = 0;
                try
                {
                    idLang = EudoCommonHelper.EudoHelpers.GetUserIdLangFromCookie();
                }
                catch
                {
                    idLang = 0;

                }

                messageOut = eResApp.GetRes(idLang, 1545) + $" ({sServeurWideAllowed})";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Une méthode récupérée sur stakoverflow, pour contourner le fait que dans l'ancien monde
        /// on utilisait des httppostedfile et dans le nouveau des httppostedfilebase.
        /// Notez la différence. L'un est juste l'implémentation de l'autre.
        /// Mais en C# ca suffit pour être incompatible.
        /// <see href="https://stackoverflow.com/questions/5514715/how-to-instantiate-a-httppostedfile"/>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filename"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public HttpPostedFile ConstructHttpPostedFile(byte[] data, string filename, string contentType)
        {
            // Get the System.Web assembly reference
            System.Reflection.Assembly systemWebAssembly = typeof(HttpPostedFileBase).Assembly;
            // Get the types of the two internal types we need
            Type typeHttpRawUploadedContent = systemWebAssembly.GetType("System.Web.HttpRawUploadedContent");
            Type typeHttpInputStream = systemWebAssembly.GetType("System.Web.HttpInputStream");

            // Prepare the signatures of the constructors we want.
            Type[] uploadedParams = { typeof(int), typeof(int) };
            Type[] streamParams = { typeHttpRawUploadedContent, typeof(int), typeof(int) };
            Type[] parameters = { typeof(string), typeof(string), typeHttpInputStream };

            // Create an HttpRawUploadedContent instance
            object uploadedContent = typeHttpRawUploadedContent
              .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, uploadedParams, null)
              .Invoke(new object[] { data.Length, data.Length });

            // Call the AddBytes method
            typeHttpRawUploadedContent
              .GetMethod("AddBytes", BindingFlags.NonPublic | BindingFlags.Instance)
              .Invoke(uploadedContent, new object[] { data, 0, data.Length });

            // This is necessary if you will be using the returned content (ie to Save)
            typeHttpRawUploadedContent
              .GetMethod("DoneAddingBytes", BindingFlags.NonPublic | BindingFlags.Instance)
              .Invoke(uploadedContent, null);

            // Create an HttpInputStream instance
            object stream = (Stream)typeHttpInputStream
              .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, streamParams, null)
              .Invoke(new object[] { uploadedContent, 0, data.Length });

            // Create an HttpPostedFile instance
            HttpPostedFile postedFile = (HttpPostedFile)typeof(HttpPostedFile)
              .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null)
              .Invoke(new object[] { filename, contentType, stream });

            return postedFile;
        }
        #endregion
    }
}