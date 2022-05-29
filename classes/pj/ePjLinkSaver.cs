using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Merge;
using Com.Eudonet.Xrm.mgr;
using EudoExtendedClasses;
using EudoQuery;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Sauvegarde les annexes de l'emailing/modèle d'emailing présent dans le corps de mail sous forme de liens
    /// </summary>
    public class ePjLinkSaver
    {
        /// <summary>
        /// Préférence utilisateur
        /// </summary>
        private ePref _pref;

        /// <summary>
        /// Strategy de creation des pj
        /// </summary>
        Func<string, string, ePJToAddLite> _pjCreator;

        /// <summary>
        /// Code d'erreur sur le traitement de la sauvegarde
        /// </summary>
        public eErrorCode ErrorCode { get; private set; }

        /// <summary>
        /// Titre de message  d'erreur
        /// </summary>
        public string UserTitleMessage { get; private set; }

        /// <summary>
        /// Message d'erreur final affiché à l'utilisateur
        /// </summary>
        public string UserMessage { get; private set; }

        /// <summary>
        /// Créer une instance  avec le createur de pj 
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="pjCreator">Createur de pj : prend en param datasPath et pjName</param>
        public ePjLinkSaver(ePref pref, Func<string, string, ePJToAddLite> pjCreator)
        {
            _pref = pref;
            _pjCreator = pjCreator;
        }

        /// <summary>
        /// Découpe le corps de mail et enregistre les annexes dans la table PJ 
        /// </summary>
        /// <returns></returns>
        public bool Save(eAnalyzerInfos bodyAnalyse, int tab, int fileId, PjType type)
        {
            // Récupèration de la liste des annexes présentent dans le corps de mail pour les enregistrer en base
            ePjLinksMailingBuildParam buildData = null;

            using (eudoDAL eDal = eLibTools.GetEudoDAL(_pref))
            {
                eDal.OpenDatabase();

                buildData = new ePjLinksMailingBuildParam()
                {
                    Pref = _pref,
                    UserInfo = _pref.User,
                    DAL = eDal,
                    AppExternalUrl = _pref.AppExternalUrl,
                    WebPjDatasPath = eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.ANNEXES, _pref.GetBaseName),
                    PhysicalRootDatasPath = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ROOT, _pref),
                    PhysicalPjDatasPath = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ANNEXES, _pref),
                    Tab = tab,
                    FileId = fileId,
                    Type = type,
                    // HLA - [RÉGRESSION 10.408] -  LES LIENS SÉCURISÉS DANS LES E-MAILINGS - #68831
                    SecureLinkTrackGenerator = SecureLinkGen
                };

                eMergeTools.GetBodyMerge_PjLinks(bodyAnalyse, buildData);
            }

            if ((buildData?.PjErrors?.Count() ?? 0) > 0)
            {
#if DEBUG
                string lstUserDevMsg = buildData.PjErrors.Select(d => string.Format("{0} :: {1}", d.UserMsg, d.DevMsg)).Join("<br/>");
                UserMessage = string.Concat(eResApp.GetRes(_pref, 8418), "<br/>", lstUserDevMsg);
#else
                string lstUserMsg = buildData.PjErrors.Select(d => d.UserMsg).Join("<br/>");
                UserMessage = string.Concat(eResApp.GetRes(_pref, 8418), "<br/>", lstUserMsg);
#endif
                ErrorCode = eErrorCode.PJ_NOT_SAVED;
                return false;
            }

            return true;
        }

        private string SecureLinkGen(ePjLinksMailingBuildParam buildParam, string trackName, string pjUrl)
        {
            string pjName = null;

            if (pjUrl.Contains(buildParam.WebPjDatasPath))
            {
                pjName = pjUrl.Substring(pjUrl.LastIndexOf("/") + 1);
            }
            else
            {
                Regex regExpIsPj = new Regex("at\\?", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (regExpIsPj.IsMatch(pjUrl))
                {
                    // Gestion d'un lien PJ sécurisé
                    Regex regExpIsValidSecurePj = new Regex("(?:tok|cs|p)=[^&]+&*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    MatchCollection mc = regExpIsValidSecurePj.Matches(pjUrl);
                    if (mc.Count == 3)
                    {
                        string paramCrypt = null;
                        foreach (Match p in mc)
                            if (p.Value.StartsWith("p="))
                                paramCrypt = p.Value.Substring(2);

                        if (paramCrypt == null)
                        {
                            // TODORES
                            string userMsg = string.Format("L'url sécurisé du lien \"{0}\" est en erreur", trackName);
                            string devMsg = string.Format("Paramètre 'p' de l'Url en erreur. Url : {0}", pjUrl);
                            throw new ePjLinksMailingBuildParam.GeneratorException(userMsg, devMsg);
                        }
                        else
                            paramCrypt = HttpUtility.UrlDecode(paramCrypt);

                        ExternalUrlParamPj paramData = null;
                        try
                        {
                            string param = ExternalUrlTools.GetDecrypt(paramCrypt);
                            paramData = (ExternalUrlParamPj)ExternalUrlTools.LoadParam(param);
                        }
                        catch (Exception e)
                        {
                            // TODORES
                            string userMsg = string.Format("L'url sécurisé du lien \"{0}\" est en erreur", trackName);
                            string devMsg = string.Format("Paramètre 'p' de l'Url en erreur. Url : {0}. Erreur : {1}", pjUrl, e.Message);
                            throw new ePjLinksMailingBuildParam.GeneratorException(userMsg, devMsg);
                        }

                        RqParam rq = new RqParam("select [Libelle] from [PJ] where [PjId] = @pjid");
                        rq.AddInputParameter("@pjid", SqlDbType.Int, paramData.PjId);

                        string error;
                        pjName = buildParam.DAL.ExecuteScalar<string>(rq, out error);
                        if (error.Length != 0)
                        {
                            // TODORES
                            string userMsg = string.Format("Annexe du lien \"{0}\" non trouvé", trackName);
                            string devMsg = string.Format("Pj non trouvé en db. Erreur : {0}", error);
                            throw new ePjLinksMailingBuildParam.GeneratorException(userMsg, devMsg);
                        }
                        if (string.IsNullOrEmpty(pjName))
                        {
                            // TODORES
                            string userMsg = string.Format("Annexe du lien \"{0}\" non trouvé", trackName);
                            string devMsg = string.Format("Nom de la pj vide. Requête : {0}", rq.GetSqlCommandText());
                            throw new ePjLinksMailingBuildParam.GeneratorException(userMsg, devMsg);
                        }
                    }
                }
            }

            // Si pas de pjName, pas de traitement à effectuer
            if (string.IsNullOrEmpty(pjName))
                return null;

            string physicalFullPath = string.Concat(buildParam.PhysicalPjDatasPath.TrimEnd('\\'), "\\", pjName);
            if (!File.Exists(physicalFullPath))
            {
                // TODORES
                string userMsg = string.Format("L'annexe du lien \"{0}\" ({1}) non trouvé", trackName, pjName);
                string devMsg = string.Format("Pj non trouvé sur le serveur. Chemin : {0}", physicalFullPath);
                throw new ePjLinksMailingBuildParam.GeneratorException(userMsg, devMsg);
            }

            int newPjId = 0;
            string sError = string.Empty;
            if (InternalInsertPj(buildParam, pjName, out newPjId, out sError))
            {
                PjBuildParam paramPj = new PjBuildParam()
                {
                    AppExternalUrl = buildParam.AppExternalUrl,
                    Uid = buildParam.Pref.DatabaseUid,
                    TabDescId = buildParam.Tab,
                    PjId = newPjId,
                    UserId = buildParam.UserInfo.UserId,
                    UserLangId = buildParam.UserInfo.UserLangId
                };

                return ExternalUrlTools.GetLinkPJ(paramPj);
            }
            else
            {
                // TODORES
                string userMsg = string.Format("L'enregistrement de l'annexe \"{0}\" ({1}) a échoué", trackName, pjName);
                string devMsg = string.Format("Erreur : {0}", sError);
                throw new ePjLinksMailingBuildParam.GeneratorException(userMsg, devMsg);
            }
        }

        /// <summary>
        /// Ajout une entrée annexe dans la table PJ pour le table en cours
        /// Si le file+fileId+PjName existe on fait rien
        /// Sinon on crée une nouvelle entrée file+fileId+PjName 
        /// </summary>
        /// <param name="buildParam">Information sur les paramètre de merge</param>
        /// <param name="pjName">Nom de l'annexe</param>
        /// <param name="newPjId">Id de la PJ créée/trouvée</param>
        /// <param name="sError">Erreur</param>
        /// <returns>Vrai si tout va bien</returns>
        private bool InternalInsertPj(ePjLinksMailingBuildParam buildParam, string pjName, out int newPjId, out string sError)
        {
            //TODO
            //Ouverture de connextion + fermeture
            //Utiliser meme DAL pour les deux ePJTraitementsLite.PJExistsForFile et ePJTraitementsLite.InsertIntoPJ

            newPjId = 0;
            sError = string.Empty;

            // On créée la pj si elle n'existe pas
            if (!ePJTraitementsLite.PJExistsForFile(buildParam.DAL, buildParam.Tab, buildParam.FileId, pjName, out newPjId))
            {
                ePJToAddLite pjCreator = new ePJToAddLite()
                {
                    FileId = buildParam.FileId,
                    FileType = ePJTraitementsLite.GetUserFriendlyFileType(pjName),
                    Description = string.Empty,
                    Size = (int)new FileInfo(String.Concat(buildParam.PhysicalPjDatasPath, "\\", pjName)).Length,
                    OverWrite = false,
                    DayExpire = null,
                    Tab = buildParam.Tab,
                    TypePj = (int)buildParam.Type,
                    SaveAs = pjName,
                    Label = pjName
                };

                ePJTraitementsLite.InsertIntoPJ(buildParam.DAL, buildParam.Pref, buildParam.UserInfo, buildParam.PhysicalRootDatasPath, pjCreator, out newPjId, out sError);
                if (newPjId == 0 || !String.IsNullOrEmpty(sError))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Purge les pj avec le fileId  = 0 pour la table en paramètre
        /// </summary>
        /// <param name="tab"></param>
        /// <returns></returns>
        public bool RemoveDirtyPj(int tab)
        {
            //TODO supprimer les entrées PJ avec fileId = 0
            return true;
        }
    }
}