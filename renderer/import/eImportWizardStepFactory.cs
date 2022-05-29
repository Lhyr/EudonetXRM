using Com.Eudonet.Internal;
using Com.Eudonet.Internal.Import;
using Com.Eudonet.Internal.tools.WCF;
using EudoExtendedClasses;
using EudoProcessInterfaces;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.SessionState;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal.wcfs.data.import;
using Com.Eudonet.Internal.import;
using Com.Eudonet.Common.Cryptography;
using Com.Eudonet.Common.Import;
using Newtonsoft.Json;
using Com.Eudonet.Common.Enumerations;
using EudoCommonHelper;
using System.Data;

namespace Com.Eudonet.Xrm.import
{

    /// <summary>
    /// Factory des renderer des etapes de l'assistant d'import
    /// </summary>
    public class eImportWizardStepFactory
    {

        /// <summary>
        /// Retourne l'objet qui va faire le rendu l'etape
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="step"></param>
        /// <param name="wizardParam"></param>
        /// <returns></returns>
        public static IWizardStepRenderer GetStep(ePref pref, int step, eImportWizardParam wizardParam)
        {
            eImportSourceInfosCallReturn resultCall;

            switch (step)
            {
                case 1:

                    // Initialisation
                    SessionStorage().Clear("ImportSourceInfos");

                    return new eDataSourceSelectionStepRenderer(pref, wizardParam, new ImportSpecification());
                case 2:
                    resultCall = GetInfos(pref, wizardParam);

                    if (resultCall.ResultCode != ImportResultCode.Success)
                        throw new ImportException(resultCall.ResultCode, resultCall.Error);

                    return new eMappingStepRenderer(pref, wizardParam, resultCall);
                case 3:

                    return new eResumeStepRenderer(pref, wizardParam);
                case 4:

                    resultCall = GetInfos(pref, wizardParam);

                    if (resultCall.ResultCode != ImportResultCode.Success)
                        throw new ImportException(resultCall.ResultCode, resultCall.Error);


                    return new eRecapStepRenderer(pref, wizardParam, resultCall);
                case 5:

                    // Néttoyage
                    SessionStorage().Clear("ImportSourceInfos");

                    //Appel au wcf
                    resultCall = ImportServiceFactory(
                    ImportService => ImportService.RunImport(
                        new eImportCall()
                        {
                            FileName = wizardParam.FileName,
                            PrefSQL = pref.GetNewPrefSql(),
                            DatasPath = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ROOT),
                            WebApp = pref.AppExternalUrl,
                            Lang = pref.Lang,
                            SecurityGroup = (int)pref.GroupMode,
                            UserId = pref.UserId,
                            Params = wizardParam.ImportParams,
                            EudoBaseName = pref.EudoBaseName
                        }));

                    if (resultCall.ResultCode != ImportResultCode.Success)
                        throw new ImportException(resultCall.ResultCode, resultCall.Error);

                    return new eResultStepRenderer(pref, wizardParam, resultCall);


                default: return new eErrorStepRenderer(eResApp.GetRes(pref, 72));
            }

        }

        /// <summary>
        /// L'etape vide correspond à un rend sans contenu
        /// </summary>
        /// <returns></returns>
        public static IWizardStepRenderer GetEmptyStep()
        {
            return new eEmptyStepRenderer();
        }


        /// <summary>
        /// Appel eudoprocess et réupere les informations d'import du Fichier posté
        /// </summary>
        /// <param name="pref">Préférences utilisateur</param>
        /// <param name="wizardParam">Paramètres d'import</param>
        /// <param name="invalidate">Savoir si on demande à wcf un nouceau, ne pas utiliser celui de la session</param>
        /// <returns></returns>
        public static eImportSourceInfosCallReturn GetInfos(ePref pref, eImportWizardParam wizardParam, bool invalidate = false)
        {
            eImportSourceInfosCallReturn result = null;
            var session = SessionStorage();

            // On regarde en session s'il n'est pas invalidé par le client
            if (!invalidate)
                result = session.Get<eImportSourceInfosCallReturn>("ImportSourceInfos");

            // On le retourne
            if (result != null)
                return result;

            // On appel wcf pour un nouveau
            result = ImportServiceFactory(ImportService => ImportService.GetSourceInfos(
                  new eImportCall()
                  {
                      FileName = wizardParam.FileName,
                      PrefSQL = pref.GetNewPrefSql(),
                      DatasPath = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ROOT),
                      WebApp = pref.AppExternalUrl,
                      Lang = pref.Lang,
                      SecurityGroup = (int)pref.GroupMode,
                      UserId = pref.UserId,
                      Params = wizardParam.ImportParams
                  }), 5);


            // On le mémorise en session
            session.Set("ImportSourceInfos", result);

            return result;
        }




        /// <summary>
        /// Appel eudoprocess et réupere les informations d'import du Fichier posté
        /// </summary>
        /// <param name="pref">Préférences utilisateur</param>
        /// <param name="importparams"></param>
        /// <param name="invalidate">Savoir si on demande à wcf un nouceau, ne pas utiliser celui de la session</param>
        /// <returns></returns>
        public static eImportSourceInfosCallReturn GetInfos(ePref pref, eImportByCodeParams importparams, bool invalidate = false)
        {
            eImportSourceInfosCallReturn result = null;
            var session = SessionStorage();

            // On regarde en session s'il n'est pas invalidé par le client
            if (!invalidate)
                result = session.Get<eImportSourceInfosCallReturn>("ImportSourceInfos");

            // On le retourne
            if (result != null)
                return result;

            // On appel wcf pour un nouveau
            result = ImportServiceFactory(ImportService => ImportService.GetSourceInfos(
                  new eImportCall()
                  {
                      FileName = importparams.PathFile,
                      PrefSQL = pref.GetNewPrefSql(),
                      DatasPath = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ROOT),
                      WebApp = pref.AppExternalUrl,
                      Lang = pref.Lang,
                      SecurityGroup = (int)pref.GroupMode,
                      UserId = pref.UserId,
                      Params = JsonConvert.DeserializeObject<ImportParams>(importparams.ImportParams)
                  }), 5); ;


            // On le mémorise en session
            session.Set("ImportSourceInfos", result);

            return result;
        }


        /// <summary>
        /// Sauvegarder un fichier posté
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur</param>
        /// <param name="file">Source de données postée</param>
        /// <param name="wizardParam">Paramètres de l'import</param>
        /// <param name="fileName">Nom du fichier stoquer sur le disc</param>
        /// <returns></returns>
        public static eImportWizardJsonResult SaveFile(ePref pref, HttpPostedFile file, eImportWizardParam wizardParam, out string fileName)
        {
            eImportWizardJsonResult result = new eImportWizardJsonResult();
            result.Step = (int)eImportWizardParam.WIZARD_STEP.IMPORTPARAMS;
            fileName = string.Empty;
            List<FileContentError> erreur = new List<FileContentError>();
            try
            {
                fileName = Path.GetFileName(file.FileName);

                if (file == null || file.ContentLength == 0)
                {
                    result.Success = false;
                    result.ErrorTitle = eResApp.GetRes(pref, 8459);
                    result.ErrorMsg = string.Format(eResApp.GetRes(pref, 8463), file != null ? fileName : eResApp.GetRes(pref, 8647));
                    result.ErrorDetail = eResApp.GetRes(pref, 8461);

                    return result;
                }

                // Extensions supportées              
                IEnumerable<string> extList = ImportSpecification.FileFormats;



                if (!extList.Contains(Path.GetExtension(fileName), StringComparison.OrdinalIgnoreCase))
                {
                    result.Success = false;
                    result.ErrorTitle = eResApp.GetRes(pref, 8459);
                    result.ErrorMsg = string.Format(eResApp.GetRes(pref, 8463), fileName);
                    result.ErrorDetail = string.Concat(eResApp.GetRes(pref, 8462), extList.Join(",")); //Format de fichiers supportés :

                    return result;
                }

                //// Taille limite
                if (file.ContentLength > ImportSpecification.MaxFileSize) //10Mo  
                {
                    StringBuilder sr = new StringBuilder();
                    sr.Append(eResApp.GetRes(pref, 8460));
                    sr.Append(Environment.NewLine);
                    sr.AppendLine(string.Concat(eResApp.GetRes(pref, 639), ": ", ImportSpecification.MaxFileSize / 1024 / 1024, " Mo"));
                    result.Success = false;
                    result.ErrorTitle = eResApp.GetRes(pref, 8459);
                    result.ErrorMsg = string.Format(eResApp.GetRes(pref, 8646), fileName);
                    result.ErrorDetail = sr.ToString();

                    return result;
                }

                try
                {
                    String sFolderPath = String.Concat(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.IMPORT, pref.GetBaseName), @"\");

                    if (File.Exists(Path.Combine(sFolderPath, fileName)))
                        fileName = ePJTraitements.GetValidFileName(sFolderPath, fileName);

                    file.SaveAs(Path.Combine(sFolderPath, fileName));


                    wizardParam.FileName = fileName;

                    eImportSourceInfosCallReturn resultWcfCall = GetInfos(pref, wizardParam, true);

                    //Contrôle du fihcier d'import
                    CheckFile(pref, fileName, resultWcfCall, erreur, ref result, wizardParam.ImportParams.ImportTab);

                }
                catch (ImportException ex)
                {
                    result.ErrorTitle = eResApp.GetRes(pref, 8459);
                    result.ErrorMsg = ex.Message;
                    result.Success = false;

                }
                catch (Exception)
                {
                    result.Success = false;
                    throw;

                }
            }
            catch (Exception)
            {

                result.Success = false;
                result.ErrorTitle = eResApp.GetRes(pref, 8459);
                result.ErrorMsg = string.Format(eResApp.GetRes(pref, 8463), file != null ? fileName : eResApp.GetRes(pref, 8647));
                result.ErrorDetail = eResApp.GetRes(pref, 8461);

                return result;
            }



            //result.Html = input.ToString();

            return result;
        }



        /// <summary>
        /// Sauvegarder un contenu posté
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur</param>
        /// <param name="file">Source de données postée</param>
        /// <param name="wizardParam">Paramètres de l'import</param>
        /// <param name="fileName">Nom du fichier stoquer sur le disc</param>
        /// <returns></returns>
        public static eImportWizardJsonResult SaveFile(ePref pref, string file, eImportWizardParam wizardParam, out string fileName)
        {
            eImportWizardJsonResult result = new eImportWizardJsonResult();
            result.Step = (int)eImportWizardParam.WIZARD_STEP.IMPORTPARAMS;
            fileName = string.Empty;
            List<FileContentError> erreur = new List<FileContentError>();
            try
            {
                fileName = string.Concat(pref.User.UserDisplayName, "_", pref.UserId, "_FileName.csv");

                if (file == null || file.Length == 0)
                {
                    result.Success = false;
                    result.ErrorTitle = eResApp.GetRes(pref, 8459);
                    result.ErrorMsg = string.Format(eResApp.GetRes(pref, 8463), file != null ? fileName : eResApp.GetRes(pref, 8647));
                    result.ErrorDetail = eResApp.GetRes(pref, 8461);

                    return result;
                }

                // Extensions supportées
                ImportSpecification importParams = new ImportSpecification();
                IEnumerable<string> extList = ImportSpecification.FileFormats;



                if (!extList.Contains(Path.GetExtension(fileName), StringComparison.OrdinalIgnoreCase))
                {
                    result.Success = false;
                    result.ErrorTitle = eResApp.GetRes(pref, 8459);
                    result.ErrorMsg = string.Format(eResApp.GetRes(pref, 8463), fileName);
                    result.ErrorDetail = string.Concat(eResApp.GetRes(pref, 8462), extList.Join(",")); //Format de fichiers supportés :

                    return result;
                }

                //// Taille limite
                if (file.Length > ImportSpecification.MaxFileSize) //5Mo = 5 242 880 octects
                {
                    StringBuilder sr = new StringBuilder();
                    sr.Append(eResApp.GetRes(pref, 8460));
                    sr.Append(Environment.NewLine);
                    sr.AppendLine(string.Concat(eResApp.GetRes(pref, 639), ": ", ImportSpecification.MaxFileSize / 1024 / 1024, " Mo"));
                    result.Success = false;
                    result.ErrorTitle = eResApp.GetRes(pref, 8459);
                    result.ErrorMsg = eResApp.GetRes(pref, 8476);
                    result.ErrorDetail = sr.ToString();

                    return result;
                }

                try
                {
                    String sFolderPath = String.Concat(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.IMPORT, pref.GetBaseName), @"\");

                    if (File.Exists(Path.Combine(sFolderPath, fileName)))
                        fileName = ePJTraitements.GetValidFileName(sFolderPath, fileName);

                    if (!File.Exists(Path.Combine(sFolderPath, fileName)))
                    {
                        try
                        {
                            File.WriteAllText(Path.Combine(sFolderPath, fileName), file, wizardParam.WizardEncoding);
                        }
                        catch (Exception ex)
                        {
                            result.Success = false;
                            throw;

                        }
                    }

                    wizardParam.FileName = fileName;

                    eImportSourceInfosCallReturn resultWcfCall = GetInfos(pref, wizardParam, true);

                    //Contrôle du fihcier d'import
                    CheckFile(pref, fileName, resultWcfCall, erreur, ref result, wizardParam.ImportParams.ImportTab);
                }
                catch (ImportException ex)
                {
                    result.ErrorTitle = eResApp.GetRes(pref, 8459);
                    result.ErrorMsg = ex.Message;
                    result.Success = false;

                }
                catch (Exception ex)
                {
                    result.Success = false;
                    throw;

                }
            }
            catch (Exception)
            {

                result.Success = false;
                result.ErrorTitle = eResApp.GetRes(pref, 8459);
                result.ErrorMsg = string.Format(eResApp.GetRes(pref, 8463), file != null ? fileName : eResApp.GetRes(pref, 8647));
                result.ErrorDetail = eResApp.GetRes(pref, 8461);

                return result;
            }



            //result.Html = input.ToString();

            return result;
        }


        /// <summary>
        /// Supprimer le fichier du disc
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="fileName"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool DeleteFile(ePref pref, string fileName, out string error)
        {
            error = string.Empty;
            bool success = false;
            try
            {
                string file = Path.Combine(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.IMPORT, pref.GetBaseName), fileName);
                File.Delete(file);
                success = true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            return success;
        }

        /// <summary>
        /// Crée un objet de communication avec EudoImport en WCF et excute la fonction en param
        /// </summary>
        /// <param name="work">L'objet à envoyer</param>
        /// <param name="timeout">Timout sur Send et receiave</param>
        public static T ImportServiceFactory<T>(Func<IEudoImportWCF, T> work, int timeout = 1)
        {
            eWCFBasic<IEudoImportWCF> wcfAccess = null;
            string err;
            try
            {
                string wcfUrl = ConfigurationManager.AppSettings.Get("EudoImportURL");
                // TODOMOU check default params
                return eWCFTools.WCFEudoProcessCaller(wcfUrl, work);
            }
            catch (EndpointNotFoundException ExWS)
            {
                err = string.Concat("Module d'import injoignable : ", Environment.NewLine, ExWS.ToString());
                throw new ImportException(ImportResultCode.ErrorOnCallingWCF, err);
            }
            catch (ImportException)
            {
                throw;
            }
            catch (EudoException)
            {
                throw;
            }
            catch (Exception ex)
            {
                err = string.Concat("Une erreur est survenue ", ex.ToString());
                throw new ImportException(ImportResultCode.ErrorOnCallingWCF, err);
            }
            finally
            {
                wcfAccess?.Dispose();
            }
        }

        /// <summary>
        /// Vérification que le nombre de lignes et le nombre de colonne dans le fichier sont réspéctés
        /// </summary>
        /// <param name="pref">Préférences utilisateur</param>
        /// <param name="fileName">Nom de la source de données</param>
        /// <param name="resultWcfCall">Les informations retournées par eudoprocess</param>
        /// <param name="erreur">Liste d'erreur détecté sur la source de données</param>
        /// <param name="result">Référence vers le résultat du contrôle de la source de données</param>
        /// <returns></returns>
        public static void CheckFile(ePref pref, string fileName, eImportSourceInfosCallReturn resultWcfCall, List<FileContentError> erreur, ref eImportWizardJsonResult result, int nStartTab)
        {
            //On récupère les limites de l'import
            eImportLimits limits = null;
            if (!eImportTools.CheckFileValidity(resultWcfCall, erreur) || !eImportTools.CheckFileSize(pref, resultWcfCall, erreur, nStartTab, out limits) || !eImportTools.CheckContentFile(resultWcfCall, erreur))
            {
                string error = string.Empty;
                if (DeleteFile(pref, fileName, out error))
                    result = GetErrorControlFile(pref, resultWcfCall, erreur, nStartTab, limits);
                else
                    throw new Exception(error);
            }
            else
                result.Success = true;
        }

        /// <summary>
        /// Objet prxy permettant de dialoguer avec la session
        /// </summary>       
        public static IStateStorage SessionStorage()
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
                return new SessionProxy(HttpContext.Current.Session);

            return new SessionProxy(null);
        }

        /// <summary>
        /// Permet de contrôler , la taille , le nombre de lignes et de colonnes dans le fichier
        /// </summary>
        /// <param name="pref">Préférences utilisateur</param>
        ///  <param name="resultWcfCall">Les informations retournées par eudoprocess</param>
        /// <param name="erreur">Les paramètres de l'import</param>
        /// <param name="nStartTab">table de lancement de l'import</param>
        /// <param name="limits">limites de l'import</param>
        /// <returns></returns>
        private static eImportWizardJsonResult GetErrorControlFile(ePref pref, eImportSourceInfosCallReturn resultWcfCall, IList<FileContentError> erreur, int nStartTab, eImportLimits limits)
        {
            eImportWizardJsonResult result = new eImportWizardJsonResult();
            if (erreur.Count > 0)
            {
                result.Success = false;
                result.ErrorTitle = eResApp.GetRes(pref, 8459);
                result.ErrorMsg = eResApp.GetRes(pref, 8460);
                StringBuilder str = new StringBuilder();

                int maxRow = ImportSpecification.MaxFileNbLine(pref, nStartTab);


                foreach (FileContentError item in erreur)
                {
                    if (str.Length > 0)
                        str.Append(Environment.NewLine);

                    switch (item)
                    {
                        case FileContentError.FILE_NB_COL:
                            result.ErrorCode = (int)FileContentError.FILE_NB_COL;
                            result.ErrorMsg = string.Format(result.ErrorMsg, resultWcfCall.SourceInfos.LineCount, resultWcfCall.SourceInfos.ColumnCount);
                            str.Append(string.Concat(eResApp.GetRes(pref, 7377), ": ", limits != null ? limits.MaxCols : ImportSpecification.MaxFileNbCol));
                            break;
                        case FileContentError.FILE_NB_LINE:
                            result.ErrorCode = (int)FileContentError.FILE_NB_LINE;
                            result.ErrorMsg = string.Format(result.ErrorMsg, resultWcfCall.SourceInfos.LineCount, resultWcfCall.SourceInfos.ColumnCount);
                            str.Append(string.Concat(eResApp.GetRes(pref, 6373), ": ", limits != null ? limits.MaxLines : maxRow));
                            break;
                        case FileContentError.FILE_SIZE:
                            result.ErrorCode = (int)FileContentError.FILE_SIZE;
                            result.ErrorMsg = string.Format(result.ErrorMsg, resultWcfCall.SourceInfos.LineCount, resultWcfCall.SourceInfos.ColumnCount);
                            str.Append(string.Concat(eResApp.GetRes(pref, 639), ": ", limits != null ? limits.MaxSize / 1024 / 1024 : ImportSpecification.MaxFileSize / 1024 / 1024, " Mo"));
                            break;
                        case FileContentError.INVALID_FILE:
                            result.ErrorCode = (int)FileContentError.INVALID_FILE;
                            result.ErrorMsg = eResApp.GetRes(pref, 7050);
                            str.Append(eResApp.GetRes(pref, 8461));
                            break;
                        case FileContentError.EMPTY_FILE:
                            result.ErrorCode = (int)FileContentError.EMPTY_FILE;
                            result.ErrorMsg = eResApp.GetRes(pref, 8475);
                            str.Append(eResApp.GetRes(pref, 8461));
                            break;
                        case FileContentError.INVALID_CONTENT_FILE:
                            result.ErrorCode = (int)FileContentError.INVALID_CONTENT_FILE;
                            result.ErrorMsg = eResApp.GetRes(pref, 8475);
                            str.Append(eResApp.GetRes(pref, 8733));
                            break;
                        default:
                            result.ErrorMsg = eResApp.GetRes(pref, 8475);
                            break;
                    }
                }

                result.ErrorDetail = str.ToString();

            }
            return result;
        }



        /// <summary>
        /// Interface permettant de géré l'etat
        /// peut etre remplacé par un cache
        /// </summary>
        public interface IStateStorage
        {
            /// <summary>
            /// Ajout d'une clé
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="key"></param>
            /// <param name="obj"></param>
            void Set<T>(string key, T obj);

            /// <summary>
            /// Retourne la valeur d'une clé
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="key"></param>
            /// <returns></returns>
            T Get<T>(string key);

            /// <summary>
            /// Retire la clé et sa valeur
            /// </summary>
            /// <param name="key"></param>
            void Clear(string key);
        }

        /// <summary>
        /// Objet permettant d'interagir avec la session
        /// </summary>
        public class SessionProxy : IStateStorage
        {
            private HttpSessionState session;

            /// <summary>
            /// Session actuelle dans le context web
            /// </summary>
            /// <param name="session"></param>
            public SessionProxy(HttpSessionState session)
            {
                this.session = session;
            }

            /// <summary>
            /// Ajout une clé et la valeur en json 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="key"></param>
            /// <param name="obj"></param>
            public void Set<T>(string key, T obj)
            {
                if (session != null)
                    session[key] = SerializerTools.JsonSerialize(obj);
            }

            /// <summary>
            /// Retourne la valeur de la clé de la session
            /// </summary>
            /// <typeparam name="T">Chaine vers l'objet</typeparam>
            /// <param name="key">clé</param>
            /// <returns></returns>
            public T Get<T>(string key)
            {
                if (session != null)
                {
                    string value = session[key] as string;
                    if (!string.IsNullOrEmpty(value))
                        return SerializerTools.JsonDeserialize<T>(value);
                }

                return default(T);
            }

            /// <summary>
            /// Retire la clé et sa valeur de la session
            /// </summary>
            /// <param name="key"></param>
            public void Clear(string key)
            {
                if (session != null)
                {
                    session.Remove(key);
                }
            }
        }




        /// <summary>
        /// lance un import
        /// </summary>
        /// <param name="pref">pref utilisateur</param>
        /// <param name="importparams">parametrage de l'import</param>
        /// <param name="invalidate"></param>
        /// <returns></returns>
        public static eImportSourceInfosCallReturn Run(ePref pref, eImportByCodeParams importparams, bool invalidate = false)
        {
            eImportSourceInfosCallReturn result = null;
            var session = SessionStorage();

            // On regarde en session s'il n'est pas invalidé par le client
            if (!invalidate)
                result = session.Get<eImportSourceInfosCallReturn>("ImportSourceInfos");

            // On le retourne
            if (result != null)
                return result;

            // On appel wcf pour un nouveau
            result = ImportServiceFactory(ImportService => ImportService.RunImport(
                  new eImportCall()
                  {
                      FileName = importparams.PathFile,
                      PrefSQL = pref.GetNewPrefSql(),
                      DatasPath = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ROOT),
                      WebApp = pref.AppExternalUrl,
                      Lang = pref.Lang,
                      SecurityGroup = (int)pref.GroupMode,
                      UserId = pref.UserId,
                      Params = JsonConvert.DeserializeObject<ImportParams>(importparams.ImportParams)
                  }), 5);


            // On le mémorise en session
            session.Set("ImportSourceInfos", result);

            return result;
        }


        /// <summary>
        /// Retourne le status d'un import
        /// </summary>
        /// <param name="sToken">jeton authentif</param>
        /// <param name="nretportId"></param> 
        /// <returns></returns>
        public static eImportInfos CheckImportStatus(string sToken, int nretportId)
        {


            CnxToken t = null;
            try
            {

                t = eLoginTools.GetCnxTokenFromKey(sToken);
            }
            catch
            {
                return new eImportInfos()
                {

                };
            }

            APPKEY myKey;
            try
            {
                myKey = APPKEY.AppKeyFromToken(sToken, CryptographyConst.TokenType.ORM_WS);
                if (myKey.Rights == null || !myKey.Rights.Contains(TokenRight.IMPORT))
                {
                    return new eImportInfos()
                    {
                         
                    };
                }
            }
            catch
            {
                return new eImportInfos()
                {

                };
            }

            //Création de l'objet epref
            ePref pref = eExternal.GetExternalPref(myKey.UserId, t.DBUID);


            // On appel wcf pour un nouveau
            var result = ImportServiceFactory(ImportService => ImportService.CheckImport(
                  new eImportCall()
                  {
                      PrefSQL = pref.GetNewPrefSql(),
                      DatasPath = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ROOT),
                      Lang = pref.Lang,
                      UserId = pref.UserId,
                      ServerImportId = nretportId
                  }), 5);

            return result;



        }

        /// <summary>
        /// Result import
        /// </summary>
        /// <param name="pref">Préférences utilisateur</param>
        /// <param name="resultWcfCall">Les informations retournées par eudoprocess</param>
        /// <param name="erreur">Liste d'erreur détecté sur la source de données</param>
        /// <returns></returns>
        public static void CheckImport(ePref pref, eImportSourceInfosCallReturn resultWcfCall, ref eImportWizardJsonResult result)
        {
            if (resultWcfCall.ResultCode != ImportResultCode.Success)
            {
                result = GetErrorParamsData(pref, resultWcfCall.ResultCode);
                result.Success = false;
            }
            else
                result.Success = true;
        }

        /// <summary>
        /// Permet de contrôler le param del'import
        /// </summary>
        /// <param name="pref">Préférences utilisateur</param>
        /// <param name="erreur">Les paramètres de l'import</param>        
        /// <returns></returns>
        private static eImportWizardJsonResult GetErrorParamsData(ePref pref, ImportResultCode erreur)
        {
            eImportWizardJsonResult result = new eImportWizardJsonResult();

            result.Success = false;
            StringBuilder str = new StringBuilder();


            if (str.Length > 0)
                str.Append(Environment.NewLine);

            switch (erreur)
            {
                case ImportResultCode.IdTemplateNotFound:
                    result.ErrorCode = 204;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 2969));
                    break;
                case ImportResultCode.InvalidDataSource:
                    result.ErrorCode = 300;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 2970));
                    break;
                case ImportResultCode.InvalidToken:
                    result.ErrorCode = 101;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 2465));
                    break;
                case ImportResultCode.InstanceAlreadyRunning:
                    result.ErrorCode = 303;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 2971));
                    break;
                case ImportResultCode.ErrorWhileAddingNewJob:
                    result.ErrorCode = 302;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8615));
                    break;
                case ImportResultCode.InvalidImportParams:
                    result.ErrorCode = 401;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    result.ErrorMsg = eResApp.GetRes(pref, 8616);
                    break;
                case ImportResultCode.ErrorWhileRunningNewInstance:
                    result.ErrorCode = 304;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8617));
                    break;
                case ImportResultCode.EmptyDataSource:
                    result.ErrorCode = 501;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8619));
                    break;
                case ImportResultCode.ErrorOnCallingWCF:
                    result.ErrorCode = 404;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8620));
                    break;
                case ImportResultCode.UnauthorizedFileAccess:
                    result.ErrorCode = 508;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8621));
                    break;
                case ImportResultCode.FilePathTooLong:
                    result.ErrorCode = 509;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8622));
                    break;
                case ImportResultCode.DirectoryNotFound:
                    result.ErrorCode = 510;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8623));
                    break;
                case ImportResultCode.FileNotFound:
                    result.ErrorCode = 501;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8624));
                    break;
                case ImportResultCode.FileNotSupported:
                    result.ErrorCode = 510;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8625));
                    break;
                case ImportResultCode.CanNotOpenFile:
                    result.ErrorCode = 511;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8626));
                    break;
                case ImportResultCode.LogAccessNotAllowed:
                    result.ErrorCode = 600;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8627));
                    break;
                case ImportResultCode.LogFileError:
                    result.ErrorCode = 601;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8628));
                    break;
                case ImportResultCode.DatabaseQueryError:
                    result.ErrorCode = 700;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8629));
                    break;
                case ImportResultCode.ErrorWhileProcessingLine:
                    result.ErrorCode = 701;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8630));
                    break;
                case ImportResultCode.TooManyFilesForKeyIdentity:
                    result.ErrorCode = 702;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8631));
                    break;
                case ImportResultCode.InvalidColumnDataFormat:
                    result.ErrorCode = 512;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append("Donnée(s) invalide(s)");// eResApp.GetRes(pref, 8632);
                    break;
                case ImportResultCode.MappingInvalid:
                    result.ErrorCode = 513;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8633));
                    break;
                case ImportResultCode.CanNotCopyFile:
                    result.ErrorCode = 514;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8634));
                    break;
                case ImportResultCode.CanNotWriteIntoFile:
                    result.ErrorCode = 515;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8635));
                    break;
                case ImportResultCode.EmptyKeyException:
                    result.ErrorCode = 516;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append("Clé dédoublonnage est vide"); //eResApp.GetRes(pref, 8636);
                    break;
                case ImportResultCode.GroupPolicyException:
                    result.ErrorCode = 517;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8637));
                    break;
                case ImportResultCode.MainTabImportRightRequired:
                    result.ErrorCode = 518;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8638));
                    break;
                case ImportResultCode.NoFieldsToUpdate:
                    result.ErrorCode = 506;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8639));
                    break;
                case ImportResultCode.SendMailSmtpError:
                    result.ErrorCode = 800;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8640));
                    break;
                case ImportResultCode.SendMailError:
                    result.ErrorCode = 801;
                    result.ErrorMsg = eResApp.GetRes(pref, 7050);
                    str.Append(eResApp.GetRes(pref, 8641));
                    break;
                default:
                    result.ErrorMsg = eResApp.GetRes(pref, 8642);
                    break;
            }


            result.ErrorDetail = str.ToString();

            return result;
        }




        /// <summary>
        /// appel de wcf pour vérification de limites et lancement de l'import
        /// </summary>
        /// <param name="eImportByCodeParams">params pour l'import</param>
        /// <returns></returns>
        public static ImportLaunchResult Import(eImportByCodeParams eImportByCodeParams)
        {
            try
            {

                if (String.IsNullOrEmpty(eImportByCodeParams.Token))
                {
                    return new ImportLaunchResult()
                    {
                        Success = false,
                        ErrorWSCode = ImportWSErrorCode.TOKEN_MISSING,
                        ErrorDescription = "Token non trouvé"
                    };
                }

                CnxToken t = null;
                try
                {

                    t = eLoginTools.GetCnxTokenFromKey(eImportByCodeParams.Token);
                }
                catch
                {
                    return new ImportLaunchResult()
                    {
                        ErrorWSCode = ImportWSErrorCode.TOKEN_INVALID,
                        ErrorDescription = "Token Invalide"
                    };
                }

                APPKEY myKey;
                try
                {
                    myKey = APPKEY.AppKeyFromToken(eImportByCodeParams.Token, CryptographyConst.TokenType.ORM_WS);

                    if (myKey.Rights == null || !myKey.Rights.Contains(TokenRight.IMPORT))
                    {
                        return new ImportLaunchResult()
                        {
                            Success = false,
                            ErrorWSCode = ImportWSErrorCode.TEMPLATE_DENIED,
                            ErrorDescription = "Token sans droits d'import"
                        };
                    }
                }
                catch
                {
                    return new ImportLaunchResult()
                    {
                        ErrorWSCode = ImportWSErrorCode.TOKEN_INVALID,
                        ErrorDescription = "Token Invalide"
                    };
                }


                if (String.IsNullOrEmpty(eImportByCodeParams.PathFile) || !File.Exists(eImportByCodeParams.PathFile))
                {
                    return new ImportLaunchResult()
                    {
                        Success = false,
                        ErrorWSCode = ImportWSErrorCode.SOURCE_MISSING,
                        ErrorDescription = "Fichier non existant"
                    };
                }


                if (eImportByCodeParams.IdTemplate <= 0)
                {
                    return new ImportLaunchResult()
                    {
                        ErrorImportCode = ImportResultCode.IdTemplateNotFound,
                        ErrorDescription = "Id template non valide"
                    };
                }



                List<FileContentError> erreur = new List<FileContentError>();
                ImportResultCode erreurImport = new ImportResultCode();
                eImportWizardJsonResult result = new eImportWizardJsonResult();
                eImportByCodeResult resultimport = new eImportByCodeResult();

                //Création de l'objet epref
                ePref pref = eExternal.GetExternalPref(myKey.UserId, t.DBUID);


                //Chargement de eImportByCodeParams.ImportParams
                string sErr = "";
                var importTemplate = new ImportTemplateWizard(pref, eImportByCodeParams.IdTemplate);

                try
                {
                    if (!ImportTemplateWizard.Load(pref, importTemplate, out sErr))
                        return new ImportLaunchResult()
                        {
                            ErrorWSCode = ImportWSErrorCode.TEMPLATE_INVALID,
                            ErrorDescription = "Impossible de charger les paramètres - vérifier l'id du template"
                        };
                }
                catch (Exception e)
                {

                    return new ImportLaunchResult()
                    {
                        ErrorWSCode = ImportWSErrorCode.TEMPLATE_INVALID,
                        ErrorDescription = "Impossible de charger les paramètres du template"
                    };
                }

                if (importTemplate.ImportTemplateLine == null || importTemplate.ImportTemplateLine?.ImportParams == null)
                {
                    return new ImportLaunchResult()
                    {
                        ErrorWSCode = ImportWSErrorCode.TEMPLATE_INVALID,
                        ErrorDescription = "Impossible de charger les paramètres du template"
                    };
                }



                if (!importTemplate.ImportTemplateLine.IsViewable)
                {
                    return new ImportLaunchResult()
                    {
                        ErrorWSCode = ImportWSErrorCode.TEMPLATE_DENIED,
                        ErrorDescription = "Impossible de charger les paramètres"
                    };
                }

                eImportByCodeParams.ImportParams = JsonConvert.SerializeObject(importTemplate.ImportTemplateLine.ImportParams);



                //recopie du rapport dans le repertoire data de la base
                string sOriginalPath = eImportByCodeParams.PathFile;

                string sDataPath = Path.Combine(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.IMPORT, pref));

                if (!Directory.Exists(sDataPath))
                {
                    try
                    {
                        sDataPath = Path.Combine(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ROOT, pref),
                                DatasFolderType.IMPORT.ToString().ToLower());



                        Directory.CreateDirectory(sDataPath);

                        if (!Directory.Exists(sDataPath))
                        {
                            return new ImportLaunchResult()
                            {
                                ErrorWSCode = ImportWSErrorCode.TEMPLATE_INVALID,
                                ErrorDescription = "Répertoire datas inaccessible"
                            };
                        }


                    }
                    catch
                    {
                        return new ImportLaunchResult()
                        {
                            ErrorWSCode = ImportWSErrorCode.TEMPLATE_INVALID,
                            ErrorDescription = "Répertoire datas inaccessible"
                        };
                    }
                }

                FileInfo fo = new FileInfo(sOriginalPath);


                String strFileTemp = Path.Combine(sDataPath, ePJTraitements.GetValidFileName(sDataPath, fo.Name));

                try
                {
                    File.Copy(sOriginalPath, strFileTemp);
                }
                catch
                {
                    return new ImportLaunchResult()
                    {
                        ErrorWSCode = ImportWSErrorCode.DIRECTORY_NOT_FOUND,
                        ErrorDescription = "Acces impossible a " + strFileTemp
                    };
                }


                eImportByCodeParams.PathFile = strFileTemp;

                int nDescId = importTemplate.ImportTemplateLine.ImportParams.ImportTab;
                int userId = pref.User.UserId;
                int groupId = (int)pref.User.UserGroupId;
                int userlevel = pref.User.UserLevel;
                string lang = pref.Lang;

                //get RightImport
                if (!GetRightImportForUser(pref, nDescId, userId, groupId, userlevel, lang))
                {
                    return new ImportLaunchResult()
                    {
                        Success = false,
                        ErrorWSCode = ImportWSErrorCode.MAIN_TAB_IMPORT_RIGHT_REQUIRED,
                        ErrorDescription = eResApp.GetRes(pref, 2992)
                    };
                }

                eImportSourceInfosCallReturn resultCall = GetInfos(pref, eImportByCodeParams, true);

                // gestion d'erreur
                //Contrôle du fihcier d'import
                CheckFile(pref, eImportByCodeParams.PathFile, resultCall, erreur, ref result,

                    JsonConvert.DeserializeObject<ImportParams>(eImportByCodeParams.ImportParams).ImportTab


                    );

                if (erreur.Count() == 0)
                {
                    eImportSourceInfosCallReturn resultCallImport = eImportWizardStepFactory.Run(pref, eImportByCodeParams, true);



                    //si envoie ok; on move
                    if (resultCallImport.ResultCode == ImportResultCode.Success)
                    {
                        try
                        {
                            File.Move(sOriginalPath, sOriginalPath + ".done");
                        }
                        catch
                        {

                            resultCallImport.Error = "Impossible de renommer " + sOriginalPath;

                        }
                    }
                    else
                    {
                        //Controle de retour d'import
                        CheckImport(pref, resultCallImport, ref result);
                        return new ImportLaunchResult()
                        {
                            ErrorWSCode = (ImportWSErrorCode)result.ErrorCode,
                            ErrorDescription = string.Concat(result.ErrorMsg, " ", result.ErrorDetail),
                            Success = result.Success ? true : false
                        };
                    }


                    //gestion de retour d'import
                    return ResultReturn(resultCallImport);

                }
                else
                {
                    return ResultError(result);
                }


            }
            catch (EudoException ee)
            {
                //erreur eudoexecption
                return new ImportLaunchResult()
                {
                    ErrorWSCode = ImportWSErrorCode.UNSPECIFIED,
                    ErrorDescription = ee.UserMessage,
                    InnerException = ee

                };

            }
            catch (Exception e)
            {
                //erreur non gérée
                return new ImportLaunchResult()
                {
                    ErrorWSCode = ImportWSErrorCode.UNSPECIFIED,
                    ErrorDescription = e.Message,
                    InnerException = new EudoException(e.Message, "", e)

                };
            }


        }



        /// <summary>
        /// gestion de retour aprés import
        /// </summary>
        /// <param name="resultcall">resultat de web service d'import</param>
        /// <returns></returns>
        public static ImportLaunchResult ResultReturn(eImportSourceInfosCallReturn resultcall)
        {
            ImportLaunchResult result = new ImportLaunchResult();

            result.ErrorWSCode = resultcall.ResultCode == ImportResultCode.Success ? ImportWSErrorCode.NONE : ImportWSErrorCode.EUDOPROCESS_ERROR;
            result.Success = resultcall.ResultCode == ImportResultCode.Success ? true : false;

            result.IdImport = resultcall.ServerImportId;
            result.ErrorImportCode = resultcall.ResultCode;
            result.ErrorDescription = resultcall.Error;

            return result;
        }


        /// <summary>
        /// gestion d'erreur
        /// </summary>
        /// <param name="resultFile"></param>
        /// <param name="resultParams"></param>
        /// <returns></returns>
        public static ImportLaunchResult ResultError(eImportWizardJsonResult resultFile)
        {
            ImportLaunchResult result = new ImportLaunchResult();
            result.Success = false;

            if (resultFile != null)
            {
                switch ((FileContentError)resultFile.ErrorCode)
                {
                    case FileContentError.UNDEFINED:
                        result.ErrorWSCode = ImportWSErrorCode.UNSPECIFIED;
                        break;
                    case FileContentError.FILE_SIZE:
                        result.ErrorWSCode = ImportWSErrorCode.FILE_SIZE;
                        result.ErrorDescription = resultFile.ErrorMsg + " " + resultFile.ErrorDetail;
                        break;
                    case FileContentError.FILE_NB_COL:
                        result.ErrorWSCode = ImportWSErrorCode.FILE_SIZE;
                        result.ErrorDescription = resultFile.ErrorMsg + " " + resultFile.ErrorDetail;
                        break;
                    case FileContentError.FILE_NB_LINE:
                        result.ErrorWSCode = ImportWSErrorCode.FILE_NB_LINE;
                        result.ErrorDescription = resultFile.ErrorMsg + " " + resultFile.ErrorDetail;
                        break;
                    case FileContentError.INVALID_FILE:
                        result.ErrorWSCode = ImportWSErrorCode.INVALID_FILE;
                        result.ErrorDescription = resultFile.ErrorMsg + " " + resultFile.ErrorDetail;
                        break;
                    case FileContentError.EMPTY_FILE:
                        result.ErrorWSCode = ImportWSErrorCode.EMPTY_FILE;
                        result.ErrorDescription = resultFile.ErrorMsg + " " + resultFile.ErrorDetail;
                        break;
                    case FileContentError.INVALID_CONTENT_FILE:
                        result.ErrorWSCode = ImportWSErrorCode.INVALID_CONTENT_FILE;
                        result.ErrorDescription = resultFile.ErrorMsg + " " + resultFile.ErrorDetail;
                        break;
                    default:
                        result.ErrorWSCode = ImportWSErrorCode.UNSPECIFIED;
                        result.ErrorDescription = resultFile.ErrorMsg;
                        break;
                }

            }

            return result;
        }


        /// <summary>
        /// get import right for current user
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nDescId"></param>
        /// <param name="userId"></param>
        /// <param name="groupId"></param>
        /// <param name="userLevel"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static Boolean GetRightImportForUser(ePref pref, int nDescId, int userId, int groupId, int userLevel, string lang)
        {
            DataTableReaderTuned dtrTabTreatmentRight = null;
            eudoDAL dal = eLibTools.GetEudoDAL(pref);
            RqParam infoTab;
            string sError;
            bool importAllowed = false;
            int langId = EudoHelpers.GetLangId(lang);
            try
            {
                dal.OpenDatabase();

                /*  Droits de traitements sur la table */
                infoTab = new RqParam();
                infoTab.SetProcedure("xsp_getTableDescFromDescIdV2");
                infoTab.AddInputParameter("@DescId", SqlDbType.Int, nDescId);
                infoTab.AddInputParameter("@UserId", SqlDbType.Int, userId);
                infoTab.AddInputParameter("@GroupId", SqlDbType.Int, groupId);
                infoTab.AddInputParameter("@UserLevel", SqlDbType.Int, userLevel);
                infoTab.AddInputParameter("@Lang", SqlDbType.VarChar, lang);

                dtrTabTreatmentRight = dal.Execute(infoTab, out sError);

                if (!string.IsNullOrEmpty(sError) || !dtrTabTreatmentRight.HasRows || !dtrTabTreatmentRight.Read())
                {
                    if (!string.IsNullOrEmpty(sError))
                    {
                        throw new EudoException(sError);
                    }
                    else
                    {
                        throw new EudoException("Erreur lors la récupération de droit d'import .");
                    }
                }

                importAllowed = dtrTabTreatmentRight.GetString("IMPORT_TAB_P") == "1";

            }
            catch (Exception ex)
            {
                throw new EudoException(ex.Message);
            }
            finally
            {
                dal.CloseDatabase();
            }


            return importAllowed;
        }
    }

}