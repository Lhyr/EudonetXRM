using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Merge;
using Com.Eudonet.Xrm.IRISBlack.Model;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Factory pour la gestion de l'existance des PJ.
    /// </summary>
    public class CheckPJExistsFactory
    {
        #region Properties
        /// <summary>
        /// Le chemin du fichier.
        /// </summary>
        public string sTargetPath { get; set; } = "";
        /// <summary>
        /// La réussite ou l'échec de la copie
        /// </summary>
        public bool IsAllSuccess { get; set; } = true;
        /// <summary>
        /// La liste de tous les noms de fichiers proposés.
        /// </summary>
        public List<string> ListNames { get; set; } = new List<string>();

        /// <summary>
        /// Les préférences utilsateurs.
        /// </summary>
        public ePref pref { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructeur pour la classe CheckPJExistsFactory
        /// </summary>
        /// <param name="_pref"></param>
        /// <param name="sPath"></param>
        private CheckPJExistsFactory(ePref _pref, string sPath)
        {
            pref = _pref;
            sTargetPath = sPath;
        }

        /// <summary>
        /// Constructeur pour la classe CheckPJExistsFactory
        /// </summary>
        /// <param name="_pref"></param>
        /// <param name="sPath"></param>
        /// <param name="bSuccess"></param>
        /// <param name="liName"></param>
        private CheckPJExistsFactory(ePref _pref, string sPath, bool bSuccess, List<string> liName)
            :this(_pref, sPath)
        {
            IsAllSuccess = bSuccess;
            ListNames = liName;
        }
        #endregion

        #region static initializers
        /// <summary>
        /// Appel static du constructeur, permettant de gérer les appels à la classe.
        /// </summary>
        /// <param name="_pref"></param>
        /// <param name="sPath"></param>
        /// <returns></returns>
        public static CheckPJExistsFactory initCheckPJExistsFactory(ePref _pref, string sPath)
        {

            return new CheckPJExistsFactory(_pref, sPath);
        }
        /// <summary>
        /// Appel static du constructeur, permettant de gérer les appels à la classe.
        /// </summary>
        /// <param name="_pref"></param>
        /// <param name="sPath"></param>
        /// <param name="bSuccess"></param>
        /// <param name="liName"></param>
        /// <returns></returns>
        public static CheckPJExistsFactory initCheckPJExistsFactory(ePref _pref, string sPath, bool bSuccess, List<string> liName)
        {
            return new CheckPJExistsFactory(_pref, sPath, bSuccess, liName);
        }
        #endregion

        #region Private
        /// <summary>
        /// Permet d'essayer de supprimer un fichier suivant son 
        /// chemin.
        /// Retourne un booléen. Vrai en cas de suppression,
        /// faux dans les autres cas.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool TryDeleteFile(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
        #endregion

        #region Public
        /// <summary>
        /// Permet de vérifier si un fichier ne posera pas de problème à la copie,
        /// et renvoie ce qui va ou ne va pas.
        /// </summary>
        /// <param name="realFileName"></param>
        /// <param name="suggestFileName"></param>
        public CheckFileModel checkIfFileExists(string realFileName, string suggestFileName)        {

            string sTargetPathWithFileName = string.Concat(sTargetPath, "\\", suggestFileName);
            string sSuggestedName = suggestFileName;

            if (File.Exists(sTargetPathWithFileName))
            {
                suggestFileName = ePJTraitements.GetValidFileName(sTargetPath, suggestFileName, ListNames);

                IsAllSuccess = false;
                ListNames.Add(sSuggestedName);

                return new CheckFileModel
                {
                    Successfull = false,
                    RealName = realFileName,
                    SuggestedName = suggestFileName,
                    WindowsTitle = eResApp.GetRes(pref, 6879), // Les fichiers suivants existent déjà. Veuillez les renommer.
                    WindowsDescription = eResApp.GetRes(pref, 8693), // Humm, il semble que certains fichiers portent le même nom, que souhaitez-vous faire ?
                };
            }

            // #31 762 : on gère également d'autres cas de noms de fichiers invalides (ex : chemin total trop long)
            bool bFileSuccess = true;
            string sWindowsTitle = "";
            string sWindowsDescription = "";

            // Pour tester la longueur du chemin total, on tente d'écrire un fichier vide à cet endroit, et on intercepte PathTooLongException
            try
            {
                using (File.Create(sTargetPathWithFileName))
                {
                    // Un fichier de test a pu être sauvegardé avec ce nom à cet endroit : on valide le nom
                    bFileSuccess = true;
                }
            }
            // Si le fichier ne peut pas être enregistré à cause d'un chemin trop long : on en suggère un autre, et on indique d'afficher la popup
            catch (PathTooLongException)
            {
                Exception innerException = null;
                bFileSuccess = false;
                sSuggestedName = ePJTraitements.GetTruncatedFileName(sTargetPath, suggestFileName, out innerException, ListNames);
                sWindowsTitle = eResApp.GetRes(pref, 1923); // Un ou plusieurs fichiers portent un nom trop long. Veuillez les raccourcir.
                sWindowsDescription = eResApp.GetRes(pref, 1924); // Humm, il semble que certains fichiers portent un nom trop long, que souhaitez-vous faire ?
                IsAllSuccess = false;
            }
            finally
            {
                // Le fichier ayant forcément été créé après vérification de non-existence, on supprime sans vérification ici, car on est, du coup,
                // certains que le fichier a été créé par le StreamWriter ci-dessus
                TryDeleteFile(sTargetPathWithFileName);
            }

            ListNames.Add(sSuggestedName);

            return new CheckFileModel
            {
                Successfull = bFileSuccess,
                RealName = realFileName,
                SuggestedName = sSuggestedName,
                WindowsTitle = sWindowsTitle,
                WindowsDescription = sWindowsDescription,
            };
        }

        /// <summary>
        /// Renvoie le lien URL sécurisé correspondant à une PJ enregistrée dans la table Annexes
        /// </summary>
        /// <param name="rowPj"></param>
        /// <returns></returns>
        public string GetSecuredPJURL(eRecordPJ rowPj)
        {
            string sLink = String.Empty;
            if (rowPj.PjType == PjType.FILE ||
                rowPj.PjType == PjType.REPORTS || rowPj.PjType == PjType.CAMPAIGN ||
                rowPj.PjType == PjType.IMPORT || rowPj.PjType == PjType.IMPORT_REPORTS
                )
            {
                PjBuildParam paramPj = new PjBuildParam()
                {
                    AppExternalUrl = pref.AppExternalUrl,
                    Uid = pref.DatabaseUid,
                    TabDescId = rowPj.CalledTab,
                    PjId = rowPj.MainFileid,
                    UserId = pref.UserId,
                    UserLangId = pref.LangId
                };

                sLink = ExternalUrlTools.GetLinkPJ(paramPj);
            }
            return sLink;
        }
        #endregion
    }
}