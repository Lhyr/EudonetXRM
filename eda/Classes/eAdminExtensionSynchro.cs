using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminExtensionSynchro : eAdminExtension
    {
        private List<eEudoSyncExchangeAgendasUser> _partiallyEnabledAgendasUserList = null;
        private List<eEudoSyncExchangeContactsUser> _partiallyEnabledContactsUserList = null;
        private List<eEudoSyncExchangeAgendasUser> _fullyEnabledAgendasUserList = null;
        private List<eEudoSyncExchangeContactsUser> _fullyEnabledContactsUserList = null;
        private int _fullyEnabledAgendasUserCount = -1;
        private int _partiallyEnabledAgendasUserCount = -1;
        private int _fullyEnabledContactsUserCount = -1;
        private int _partiallyEnabledContactsUserCount = -1;

        [JsonConstructor]
        public eAdminExtensionSynchro(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }

        public eAdminExtensionSynchro(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO)
        {
            string strError = String.Empty;
            _fullyEnabledAgendasUserList = eSqlExtensionSynchro.GetEudoSyncExchangeAgendasUserList(Pref, DAL, out _partiallyEnabledAgendasUserList, out strError);
            _fullyEnabledContactsUserList = eSqlExtensionSynchro.GetEudoSyncExchangeContactsUserList(Pref, DAL, out _partiallyEnabledContactsUserList, out strError);
            _fullyEnabledAgendasUserCount = _fullyEnabledAgendasUserList == null ? -1 : _fullyEnabledAgendasUserList.Count;
            _partiallyEnabledAgendasUserCount = _partiallyEnabledAgendasUserList == null ? -1 : _partiallyEnabledAgendasUserList.Count;
            _fullyEnabledContactsUserCount = _fullyEnabledContactsUserList == null ? -1 : _fullyEnabledContactsUserList.Count;
            _partiallyEnabledContactsUserCount = _partiallyEnabledContactsUserList == null ? -1 : _partiallyEnabledContactsUserList.Count;
        }

        /// <summary>
        /// Indique si l'intégralité du mode Fiche de l'extension (notamment l'onglet Paramètres) doit être rafraîchi après activation/désactivation
        /// </summary>
        public override bool NeedsFullRefreshAfterEnable
        {
            get
            {
                return true; // true car l'extension peut être activée ou désactivée par une combinaison de deux paramètres
            }
        }

        /// <summary>
        /// Indique si l'extension comporte des paramètres à afficher dans l'onglet Paramètres
        /// </summary>
        public override bool ShowParametersTab
        {
            get
            {
                return true;
            }
        }

        public List<eEudoSyncExchangeAgendasUser> PartiallyEnabledAgendasUserList
        {
            get
            {
                return _partiallyEnabledAgendasUserList;
            }

            set
            {
                _partiallyEnabledAgendasUserList = value;
            }
        }

        public List<eEudoSyncExchangeContactsUser> PartiallyEnabledContactsUserList
        {
            get
            {
                return _partiallyEnabledContactsUserList;
            }

            set
            {
                _partiallyEnabledContactsUserList = value;
            }
        }

        public List<eEudoSyncExchangeAgendasUser> FullyEnabledAgendasUserList
        {
            get
            {
                return _fullyEnabledAgendasUserList;
            }

            set
            {
                _fullyEnabledAgendasUserList = value;
            }
        }

        public List<eEudoSyncExchangeContactsUser> FullyEnabledContactsUserList
        {
            get
            {
                return _fullyEnabledContactsUserList;
            }

            set
            {
                _fullyEnabledContactsUserList = value;
            }
        }

        public int FullyEnabledAgendasUserCount
        {
            get
            {
                return _fullyEnabledAgendasUserCount;
            }

            set
            {
                _fullyEnabledAgendasUserCount = value;
            }
        }

        public int PartiallyEnabledAgendasUserCount
        {
            get
            {
                return _partiallyEnabledAgendasUserCount;
            }

            set
            {
                _partiallyEnabledAgendasUserCount = value;
            }
        }

        public int FullyEnabledContactsUserCount
        {
            get
            {
                return _fullyEnabledContactsUserCount;
            }

            set
            {
                _fullyEnabledContactsUserCount = value;
            }
        }

        public int PartiallyEnabledContactsUserCount
        {
            get
            {
                return _partiallyEnabledContactsUserCount;
            }

            set
            {
                _partiallyEnabledContactsUserCount = value;
            }
        }

        public bool IsEudoSyncExchangeAgendasEnabled()
        {
            bool bEnabled = false;
            IsEudoSyncExchangeAgendasEnabled(out bEnabled);
            return bEnabled;
        }
        public bool IsEudoSyncExchangeContactsEnabled()
        {
            bool bEnabled = false;
            IsEudoSyncExchangeContactsEnabled(out bEnabled);
            return bEnabled;
        }

        /// <summary>
        /// Vérifie si l'extension est activée en base, et renvoie le résultat dans la variable passée en paramètre
        /// </summary>
        /// <returns>Objet eResult permettant de vérifier si le processus s'est déroulé correctement, ou non</returns>
        public eAdminResult IsEudoSyncExchangeAgendasEnabled(out bool bEnabled)
        {
            return IsEudoSyncEnabled("eudosyncexchangeagendas", out bEnabled);
        }

        public eAdminResult IsEudoSyncExchangeContactsEnabled(out bool bEnabled)
        {
            return IsEudoSyncEnabled("eudosyncexchangecontacts", out bEnabled);
        }

        private eAdminResult IsEudoSyncEnabled(string syncType, out bool bEnabled)
        {
            bEnabled = false;

            eAdminResult result = new eAdminResult();
            string strException = eResApp.GetRes(Pref, 7841).Replace("<EXTENSION>", Title); // Une erreur est survenue durant la vérification d'activation de l'extension <EXTENSION>

            if (_dal == null)
                _dal = eLibTools.GetEudoDAL(_pref);

            StringBuilder sbError = new StringBuilder();
            try
            {
                _dal.OpenDatabase();

                try
                {
                    string strError = String.Empty;
                    int partiallyEnabledUserCount = 0;
                    if (syncType == "eudosyncexchangeagendas")
                        bEnabled = IsEudoSyncExchangeAgendasEnabledProcess(out partiallyEnabledUserCount, out strError);
                    else
                        bEnabled = IsEudoSyncExchangeContactsEnabledProcess(out partiallyEnabledUserCount, out strError);
                    sbError.Append(strError);
                }
                catch (eSqlConfigAdvException e)
                {
                    sbError.AppendLine("****").AppendLine(e.Message).AppendLine(e.StackTrace);
                }

                if (sbError.Length == 0)
                {
                    result.Success = true;
                }
                else
                {
                    Exception = new eAdminExtensionException(sbError.ToString());
                    result.Success = false;
                    result.InnerException = Exception;
                    result.UserErrorMessage = strException;
                    result.DebugErrorMessage = sbError.ToString();
                }
            }
            catch (Exception e)
            {
                Exception = new eAdminExtensionException(strException, e);
                result.Success = false;
                result.InnerException = Exception;
                result.UserErrorMessage = strException;
                result.DebugErrorMessage = e.Message;
            }
            finally
            {
                _dal.CloseDatabase();
            }

            return result;
        }

        /// <summary>
        /// Effectue le traitement pour vérifier si l'extension est activée
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="dal"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public override bool IsEnabledProcess(out string strError)
        {
            return eSqlExtensionSynchro.IsEnabled(Pref, DAL, out strError);
        }

        /// <summary>
        /// Effectue le traitement pour vérifier si le module EudoSync Exchange Agendas est activé
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="dal"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public bool IsEudoSyncExchangeAgendasEnabledProcess(out int partiallyEnabledUserCount, out string strError)
        {
            return eSqlExtensionSynchro.IsEudoSyncExchangeAgendasEnabled(Pref, DAL, out partiallyEnabledUserCount, out strError);
        }

        /// <summary>
        /// Effectue le traitement pour vérifier si le module EudoSync Exchange Contacts est activé
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="dal"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public bool IsEudoSyncExchangeContactsEnabledProcess(out int partiallyEnabledUserCount, out string strError)
        {
            return eSqlExtensionSynchro.IsEudoSyncExchangeContactsEnabled(Pref, DAL, out partiallyEnabledUserCount, out strError);
        }

        /// <summary>
        /// Effectue le traitement pour activer ou désactiver le module
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="dal"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public override bool EnableProcess(bool bEnable, out string strError)
        {
            string syncType = "";
            AdditionalParameters.TryGetValue("synctype", out syncType);

            switch (syncType)
            {
                case "eudosyncexchangeagendas":
                    return EnableEudoSyncExchangeAgendasProcess(bEnable, out strError);
                case "eudosyncexchangecontacts":
                    return EnableEudoSyncExchangeContactsProcess(bEnable, out strError);
                default:
                    return
                        EnableEudoSyncExchangeAgendasProcess(bEnable, out strError) &&
                        EnableEudoSyncExchangeContactsProcess(bEnable, out strError);
            }
        }

        /// <summary>
        /// Effectue le traitement pour activer ou désactiver le module EudoSync Exchange Agendas
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="dal"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public bool EnableEudoSyncExchangeAgendasProcess(bool bEnable, out string strError)
        {
            return eSqlExtensionSynchro.EnableEudoSyncExchangeAgendas(Pref, DAL, bEnable, out strError);
        }

        /// <summary>
        /// Effectue le traitement pour activer ou désactiver le module EudoSync Exchange Contacts
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="dal"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public bool EnableEudoSyncExchangeContactsProcess(bool bEnable, out string strError)
        {
            return eSqlExtensionSynchro.EnableEudoSyncExchangeContacts(Pref, DAL, bEnable, out strError);
        }

        public override bool AfterEnableProcess(bool bEnable, out string sError)
        {
            sError = String.Empty;
            return true;
        }

        public override List<HtmlGenericControl> GetModuleInfo()
        {
            List<HtmlGenericControl> infoList = new List<HtmlGenericControl>();
            List<Tuple<string, string>> infoData = new List<Tuple<string, string>>();

            #region EudoSync Exchange Agendas
            HtmlGenericControl spanInfo = new HtmlGenericControl("span");
            HtmlGenericControl spanInfoLabel = new HtmlGenericControl("span");
            HtmlGenericControl spanInfoValue = new HtmlGenericControl("span");
            spanInfoLabel.Attributes.Add("class", "extensionInfoLabel");
            spanInfoValue.Attributes.Add("class", "extensionInfoValue");
            spanInfoLabel.InnerHtml = String.Concat(eResApp.GetRes(Pref, 7843), "&nbsp;"); // EudoSync Exchange Agendas est
            spanInfoValue.InnerHtml = _fullyEnabledAgendasUserCount > 0 ? eResApp.GetRes(Pref, 7845) : _partiallyEnabledAgendasUserCount > 0 ? eResApp.GetRes(Pref, 7846) : eResApp.GetRes(Pref, 7847); // activée / partiellement activée / désactivée
            spanInfo.Controls.Add(spanInfoLabel);
            spanInfo.Controls.Add(spanInfoValue);

            HtmlGenericControl ulSubInfo = new HtmlGenericControl("ul");

            HtmlGenericControl liSubInfo = new HtmlGenericControl("li");
            HtmlGenericControl spanSubInfoLabel = new HtmlGenericControl("span");
            HtmlGenericControl spanSubInfoValue = new HtmlGenericControl("span");
            ulSubInfo.Attributes.Add("class", "extensionInfoList");
            spanSubInfoLabel.Attributes.Add("class", "extensionInfoLabel");
            spanSubInfoValue.Attributes.Add("class", "extensionInfoValue");
            spanSubInfoLabel.InnerHtml = _fullyEnabledAgendasUserCount < 0 ? eResApp.GetRes(Pref, 7848) : _fullyEnabledAgendasUserCount < 2 ? String.Concat("&nbsp;", eResApp.GetRes(Pref, 7849)) : String.Concat("&nbsp;", eResApp.GetRes(Pref, 7850)); // Nombre inconnu d'utilisateurs actifs / utilisateur actif / utilisateurs actifs
            spanSubInfoValue.InnerHtml =
                _fullyEnabledAgendasUserCount < 0 ? String.Empty :
                _fullyEnabledAgendasUserCount == 0 ? eResApp.GetRes(Pref, 436) : // Aucun
                _fullyEnabledAgendasUserCount.ToString();
            liSubInfo.Controls.Add(spanSubInfoValue);
            liSubInfo.Controls.Add(spanSubInfoLabel);
            ulSubInfo.Controls.Add(liSubInfo);

            if (_partiallyEnabledAgendasUserCount != 0)
            {
                liSubInfo = new HtmlGenericControl("li");
                spanSubInfoLabel = new HtmlGenericControl("span");
                spanSubInfoValue = new HtmlGenericControl("span");
                ulSubInfo.Attributes.Add("class", "extensionInfoList");
                spanSubInfoLabel.Attributes.Add("class", "extensionInfoLabel");
                spanSubInfoValue.Attributes.Add("class", "extensionInfoValue");
                spanSubInfoLabel.InnerHtml = _partiallyEnabledAgendasUserCount < 0 ? eResApp.GetRes(Pref, 7851) : _partiallyEnabledAgendasUserCount < 2 ? String.Concat("&nbsp;", eResApp.GetRes(Pref, 7852)) : String.Concat("&nbsp;", eResApp.GetRes(Pref, 7853)); // Nombre inconnu d'utilisateurs partiellement paramétrés / utilisateur partiellement paramétré / utilisateurs partiellement paramétrés
                spanSubInfoValue.InnerHtml =
                    _partiallyEnabledAgendasUserCount < 0 ? String.Empty :
                    _partiallyEnabledAgendasUserCount == 0 ? eResApp.GetRes(Pref, 436) : // Aucun
                    _partiallyEnabledAgendasUserCount.ToString();
                liSubInfo.Controls.Add(spanSubInfoValue);
                liSubInfo.Controls.Add(spanSubInfoLabel);
                ulSubInfo.Controls.Add(liSubInfo);
            }

            spanInfo.Controls.Add(ulSubInfo);
            infoList.Add(spanInfo);
            #endregion

            #region EudoSync Exchange Contacts
            spanInfo = new HtmlGenericControl("span");
            spanInfoLabel = new HtmlGenericControl("span");
            spanInfoValue = new HtmlGenericControl("span");
            spanInfoLabel.Attributes.Add("class", "extensionInfoLabel");
            spanInfoValue.Attributes.Add("class", "extensionInfoValue");
            spanInfoLabel.InnerHtml = String.Concat(eResApp.GetRes(Pref, 7844), "&nbsp;"); // EudoSync Exchange Contacts est
            spanInfoValue.InnerHtml = _fullyEnabledContactsUserCount > 0 ? eResApp.GetRes(Pref, 7845) : _partiallyEnabledContactsUserCount > 0 ? eResApp.GetRes(Pref, 7846) : eResApp.GetRes(Pref, 7847); // activée / partiellement activée / désactivée
            spanInfo.Controls.Add(spanInfoLabel);
            spanInfo.Controls.Add(spanInfoValue);

            ulSubInfo = new HtmlGenericControl("ul");

            liSubInfo = new HtmlGenericControl("li");
            spanSubInfoLabel = new HtmlGenericControl("span");
            spanSubInfoValue = new HtmlGenericControl("span");
            ulSubInfo.Attributes.Add("class", "extensionInfoList");
            spanSubInfoLabel.Attributes.Add("class", "extensionInfoLabel");
            spanSubInfoValue.Attributes.Add("class", "extensionInfoValue");
            spanSubInfoLabel.InnerHtml = _fullyEnabledContactsUserCount < 0 ? eResApp.GetRes(Pref, 7848) : _fullyEnabledContactsUserCount < 2 ? String.Concat("&nbsp;", eResApp.GetRes(Pref, 7849)) : String.Concat("&nbsp;", eResApp.GetRes(Pref, 7850)); // Nombre inconnu d'utilisateurs actifs / utilisateur actif / utilisateurs actifs
            spanSubInfoValue.InnerHtml =
                _fullyEnabledContactsUserCount < 0 ? String.Empty :
                _fullyEnabledContactsUserCount == 0 ? eResApp.GetRes(Pref, 436) : // Aucun
                _fullyEnabledContactsUserCount.ToString();
            liSubInfo.Controls.Add(spanSubInfoValue);
            liSubInfo.Controls.Add(spanSubInfoLabel);
            ulSubInfo.Controls.Add(liSubInfo);

            if (_partiallyEnabledContactsUserCount != 0)
            {
                liSubInfo = new HtmlGenericControl("li");
                spanSubInfoLabel = new HtmlGenericControl("span");
                spanSubInfoValue = new HtmlGenericControl("span");
                ulSubInfo.Attributes.Add("class", "extensionInfoList");
                spanSubInfoLabel.Attributes.Add("class", "extensionInfoLabel");
                spanSubInfoValue.Attributes.Add("class", "extensionInfoValue");
                spanSubInfoLabel.InnerHtml = _partiallyEnabledContactsUserCount < 0 ? eResApp.GetRes(Pref, 7851) : _partiallyEnabledContactsUserCount < 2 ? String.Concat("&nbsp;", eResApp.GetRes(Pref, 7852)) : String.Concat("&nbsp;", eResApp.GetRes(Pref, 7853)); // Nombre inconnu d'utilisateurs partiellement paramétrés / utilisateur partiellement paramétré / utilisateurs partiellement paramétrés
                spanSubInfoValue.InnerHtml =
                    _partiallyEnabledContactsUserCount < 0 ? String.Empty :
                    _partiallyEnabledContactsUserCount == 0 ? eResApp.GetRes(Pref, 436) : // Aucun
                    _partiallyEnabledContactsUserCount.ToString();
                liSubInfo.Controls.Add(spanSubInfoValue);
                liSubInfo.Controls.Add(spanSubInfoLabel);
                ulSubInfo.Controls.Add(liSubInfo);
            }

            spanInfo.Controls.Add(ulSubInfo);
            infoList.Add(spanInfo);
            #endregion

            return infoList;
        }
    }

    public class eEudoSyncExchangeUser
    {
        public int UserId = 0;
        public string UserLogin = String.Empty;
        public string UserDisplayName = String.Empty;
        public string UserMail = String.Empty;
    }

    public class eEudoSyncExchangeAgendasUser : eEudoSyncExchangeUser
    {
        public bool EudoSyncExchangeAgendasEnabled = false;
    }

    public class eEudoSyncExchangeContactsUser : eEudoSyncExchangeUser
    {
        public bool EudoSyncExchangeContactsEnabled = false;
        public string UserExchangeServer = String.Empty;
    }
}