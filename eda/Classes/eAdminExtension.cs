using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Newtonsoft.Json;
using static Com.Eudonet.Internal.eLibConst;
using static Com.Eudonet.Xrm.eConst;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe regroupant les informations relatives à une extension optionnelle de l'application, administrable depuis la tuile Extension de la zone d'administration
    /// </summary>
    public abstract class eAdminExtension
    {
        /// <summary>
        /// Indique si on doit afficher l'ancien EudoStore (antérieur à la 10.504) ou le nouvel EudoStore (10.504 et >)
        /// Variable prévue pour être mise à false en cas d'anomalie constatée avec la nouvelle version
        /// </summary>
        public static bool IsNewStore { get; set; } = true;

        /// <summary>
        /// Disponibilité de l'extension
        /// </summary>
        public enum ExtensionAvailability
        {
            /// <summary>Inclus</summary>
            Included,
            /// <summary>Extension : activable</summary>
            Extension,
            /// <summary>Non disponible</summary>
            Unavailable
        }

        /// <summary>Connexion bdd</summary>
        protected eudoDAL _dal = null;
        /// <summary>Preferences</summary>
        protected ePref _pref = null;
        /// <summary>Module/Extension en cours</summary>
        protected eUserOptionsModules.USROPT_MODULE _module = eUserOptionsModules.USROPT_MODULE.UNDEFINED;
        /// <summary>Libellé du module</summary>
        protected string _title = String.Empty;
        /// <summary>Exception</summary>
        protected eAdminExtensionException _exception = null;
        /// <summary>Indique si l'onglet Paramètres doit être affiché pour cette extension</summary>
        protected bool _showParametersTab = false;
        /// <summary>Indique si la page de l'extension doit être entièrement rafraîchie après activation/désactivation</summary>
        protected bool _needsFullRefreshAfterEnable = false;
        /// <summary>Tableau de paramètres additionnels</summary>
        protected IDictionary<string, string> _additionalParameters = null;
        /// <summary>
        /// Le bouton "Activer" est-il modifiable ou en lecture seule ?
        /// </summary>
        protected bool _IsUninstallable = true;
        /// <summary>
        /// Disponibilité de l'extension suivant l'offre du client
        /// </summary>
        protected ExtensionAvailability availability = ExtensionAvailability.Extension;
        /// <summary>
        /// Fonctionnalité XrmExtension correspondante
        /// </summary>
        protected XrmExtension _feature = XrmExtension.Undefined;

        /// <summary>
        /// Infos de l'extension
        /// </summary>
        public eAdminExtensionInfo Infos { get; protected set; }

        /// <summary>
        /// Indique s'il s'agit d'une extension native
        /// </summary>
        public bool IsNativeExtension { get; set; }

        /// <summary>
        /// Préferences
        /// </summary>
        [JsonIgnore]
        public ePref Pref
        {
            get
            {
                return _pref;
            }
            set
            {
                _pref = value;
            }
        }

        /// <summary>
        /// Connexion bdd
        /// </summary>
        [JsonIgnore]
        public eudoDAL DAL
        {
            get
            {
                return _dal;
            }
            set
            {
                _dal = value;
            }
        }

        /// <summary>
        /// Module/Extension en cours
        /// </summary>
        public eUserOptionsModules.USROPT_MODULE Module
        {
            get
            {
                return _module;
            }
            set
            {
                _module = value;
            }
        }

        /// <summary>
        /// Libellé du module
        /// </summary>
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }

        /// <summary>
        /// Exception
        /// </summary>
        public eAdminExtensionException Exception
        {
            get
            {
                return _exception;
            }
            set
            {
                _exception = value;
            }
        }

        /// <summary>
        /// Tableau de paramètres additionnels passés au manager de l'extension, afin d'effectuer certains traitements spécifiques par extension
        /// - passage de paramètres à eAdmin.enableExtension pour certaines extensions
        /// - récupération de ces paramètres par eAdminExtensionManager sur l'objet eAdminExtension
        /// - l'objet eAdminExtension comporte ainsi des paramètres qui peuvent être utilisés en interne selon les besoin
        /// Exemple : Synchros - Activation d'un seul type de synchro (Agendas ou Contacts)
        /// </summary>
        public IDictionary<string, string> AdditionalParameters
        {
            get
            {
                return _additionalParameters;
            }

            set
            {
                _additionalParameters = value;
            }
        }

        /// <summary>
        /// Indique si la page de l'extension doit être entièrement rafraîchie après activation/désactivation
        /// Si ce booléen est à false, l'onglet Paramètres sera simplement affiché/masqué sans A/R serveur pour actualiser les paramètres, et seul l'encart
        /// d'informations en haut de fiche sera mis à jour
        /// </summary>
        public virtual bool NeedsFullRefreshAfterEnable
        {
            get
            {
                return _needsFullRefreshAfterEnable;
            }
            set
            {
                _needsFullRefreshAfterEnable = value;
            }
        }

        /// <summary>
        /// Indique si l'onglet Paramètres doit être affiché pour cette extension
        /// A mettre à true si l'extension dispose d'un renderer ajoutant des contrôles de paramétrage spécifique
        /// A mettre à false si l'extension ne dispose pas d'options spécifiques, mais nécessite uniquement de pouvoir être activée/désactivée
        /// </summary>
        public virtual bool ShowParametersTab
        {
            get
            {
                return _showParametersTab || (Infos != null ? Infos.HasCustomParameter && Pref.User.UserLevel >= UserLevel.LEV_USR_SUPERADMIN.GetHashCode() : false);
            }
        }
        /// <summary>
        /// Indique si 'extension peut être déinstallée
        /// </summary>
        public bool IsUnInstallable
        {
            get
            {
                return _IsUninstallable /*&& Infos.IsCompatible*/;
            }
        }
        /// <summary>
        /// Disponibilité de l'extension suivant l'offre du client
        /// </summary>
        public ExtensionAvailability Availability
        {
            get
            {
                return availability;
            }

            set
            {
                this.availability = value;
            }
        }

        /// <summary>
        /// Fonctionnalité correspondante
        /// </summary>
        /// <value>
        /// The feature.
        /// </value>
        public XrmExtension Feature
        {
            get
            {
                return _feature;
            }

            set
            {
                this._feature = value;
            }
        }

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        [JsonConstructor]
        public eAdminExtension()
        {

        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref">Preferences</param>
        /// <param name="module">Module/Extension à initialiser</param>
        public eAdminExtension(ePref pref, eUserOptionsModules.USROPT_MODULE module)
        {
            _pref = pref;
            _module = module;
            if (_pref != null)
            {
                _title = eUserOptionsModules.GetModuleLabel(module, _pref);
                _dal = eLibTools.GetEudoDAL(_pref);
            }
            this.Init();
        }

        /// <summary>
        /// Initialise l'extension
        /// </summary>
        protected virtual void Init()
        {
            return;
        }

        /// <summary>
        /// Initialise l'extension
        /// </summary>
        /// <param name="pref">Preferences</param>
        /// <param name="sError">erreur</param>
        /// <returns></returns>
        protected virtual bool InitConfig(ePref pref, out string sError)
        {
            sError = String.Empty;
            return false;
        }

        /// <summary>
        /// Vérifie si l'extension est activée en base et renvoie directement true ou false, sans renvoyer d'objet permettant de vérifier si le processus s'est déroulé correctement
        /// </summary>
        /// <returns>true si l'extension est activée, false sinon</returns>
        public virtual bool IsEnabled()
        {
            bool bEnabled = false;
            IsEnabled(out bEnabled);
            return bEnabled;
        }

        /// <summary>
        /// Vérifie si l'extension est activée en base, et renvoie le résultat dans la variable passée en paramètre
        /// </summary>
        /// <returns>Objet eResult permettant de vérifier si le processus s'est déroulé correctement, ou non</returns>
        public eAdminResult IsEnabled(out bool bEnabled)
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

                    LoadStatus();

                    if (Infos != null)
                    {
                        bEnabled = IsEnabledProcess(out strError);

                        //Insert l'extension en status actif si elle est enabled mais en statu not installed
                        if (bEnabled
                                && Infos.IsNativeExtension
                                && Infos.IsCompatible
                                && Infos.Status == EXTENSION_STATUS.STATUS_NON_INSTALLED)
                        {

                            UpdateExtension(true);
                        }



                        if (Infos.IsNativeExtension && (!Infos.DelayedActivation || !_IsUninstallable))
                        {
                            Infos.IsEnabled = bEnabled;
                            if (bEnabled)
                            {
                                Infos.Status = EXTENSION_STATUS.STATUS_READY;
                            }
                            else
                            {
                                Infos.Status = EXTENSION_STATUS.STATUS_DISABLED;
                            }
                        }
                    }

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
        /// Active ou désactive l'extension sur la base
        /// </summary>
        /// <param name="bEnable">true pour activer l'extension, false pour la désactiver</param>
        /// <returns>Objet eAdminResult indiquant si la mise à jour s'est correctement déroulée, ou non</returns>
        public virtual eAdminResult SetEnabled(bool bEnable)
        {
            eAdminResult result = new eAdminResult();
            string strException = eResApp.GetRes(Pref, 7842).Replace("<EXTENSION>", Title); // Une erreur est survenue durant l'activation de l'extension <EXTENSION>

            if (_dal == null)
                _dal = eLibTools.GetEudoDAL(_pref);

            StringBuilder sbError = new StringBuilder();
            try
            {
                _dal.OpenDatabase();

                try
                {

                    string strError = "";
                    UpdateExtension(bEnable);

                    if (!Infos.DelayedActivation || Pref.User.UserLevel > (int)UserLevel.LEV_USR_ADMIN)
                    {
                        if (EnableProcess(bEnable, out strError))
                        {
                            AfterEnableProcess(bEnable, out strError);
                        }
                    }
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

            // Remise à jour des méta-infos de l'extension après exécution du processus pour rafraîchissement de l'affichage
            SetInfos(Pref);

            return result;
        }


        /// <summary>
        /// Charge le status de l'extension depuis la table EXTENSION
        /// </summary>
        private void LoadStatus()
        {

            if (Infos != null)
            {

                List<eExtension> lst = eExtension.GetExtensionsByCode(_pref, Infos.ExtensionNativeId);
                if (lst.Count == 0)
                {
                    Infos.IsEnabled = false;
                    Infos.Status = EXTENSION_STATUS.STATUS_NON_INSTALLED;
                }
                else
                {
                    var userDef = lst.Find(aa => aa.UserId == 0);



                    if (userDef != null)
                    {

                        //status global
                        if (userDef.Status == EXTENSION_STATUS.STATUS_DISABLED
                            || userDef.Status == EXTENSION_STATUS.STATUS_DESACTIVATION_ASKED
                            || userDef.Status == EXTENSION_STATUS.STATUS_NON_INSTALLED
                            || userDef.Status == EXTENSION_STATUS.STATUS_UNDEFINED
                            )
                            Infos.IsEnabled = false;
                        else
                        {

                            if (eExtension.NeedStrictActivation(userDef.Code) && !userDef.IsBaseValid(_pref.GetBaseName))
                            {

                                userDef.Status = EXTENSION_STATUS.STATUS_DISABLED;
                                Infos.Status = EXTENSION_STATUS.STATUS_DISABLED;
                                Infos.IsEnabled = false;

                                UpdateExtension(false);
                            }
                            else
                            {

                                Infos.IsEnabled = true;


                            }
                        }


                        Infos.Status = userDef.Status;
                    }
                    else
                    {
                        Infos.IsEnabled = false;
                        Infos.Status = EXTENSION_STATUS.STATUS_NON_INSTALLED;
                    }


                }

            }
            // #57 208 Cas où on interroge Enabled immédiatement après InitFromModule - On initialise l'objet afin qu'il ne soit pas null et puisse être complété
            else
            {
                SetInfos(_pref, false); // false pour éviter de réinterroger justement IsEnabled, qui interrogera LoadStatus, qui interrogera IsEnabled, ...


                LoadStatus();
            }

        }

        /// <summary>
        /// Effectue le traitement spécifique à chaque extension pour vérifier si elle est activée
        /// </summary>
        public abstract bool IsEnabledProcess(out string sError);


        /// <summary>
        /// Active l'extension - Actions system hors extensions
        /// </summary>
        /// <param name="bEnable"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public abstract bool EnableProcess(bool bEnable, out string sError);


        /// <summary>
        /// retourne le status que doit avoir l'extensions en fonction de la demande activation/désactivation
        /// </summary>
        /// <param name="bEnable"></param>
        /// <returns></returns>
        protected virtual EXTENSION_STATUS GetExtensionStatus(bool bEnable)
        {
            //en fonction du code extension, voir quel statut utiliser
            if (bEnable)
            {
                if (!Infos.DelayedActivation || Pref.User.UserLevel > (int)UserLevel.LEV_USR_ADMIN || Infos.Status == EXTENSION_STATUS.STATUS_DESACTIVATION_ASKED)
                {
                    return EXTENSION_STATUS.STATUS_READY;
                }
                else
                {
                    return EXTENSION_STATUS.STATUS_ACTIVATION_ASKED;
                }
            }
            else
            {
                if (Pref.User.UserLevel > (int)UserLevel.LEV_USR_ADMIN || Infos.Status == EXTENSION_STATUS.STATUS_ACTIVATION_ASKED)
                {
                    return EXTENSION_STATUS.STATUS_DISABLED;
                }
                else
                {
                    return EXTENSION_STATUS.STATUS_DESACTIVATION_ASKED;
                }
            }
        }


        /// <summary>
        /// met à jour les paramètres d'une extension en fonction de son paramétrage
        /// Utilisable si le champ param contient des données à parser par exemple
        /// </summary>
        /// <param name="newExt"></param>
        public virtual void SetDefaultParam(eExtension newExt)
        {

        }

        /// <summary>
        /// Mis à jour des param de l'extension en fonction d'un changement de status
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="oldStatus"></param>
        protected virtual void UpdateParamChangeStatus(eExtension ext, EXTENSION_STATUS oldStatus)
        {

        }

        /// <summary>
        /// Met à jour le statut dans la table extension / Créé l'entrée dans la table extension
        /// </summary>
        /// <param name="bEnable">true pour activer l'extension, false pour la désactiver</param>
        protected virtual bool UpdateExtension(bool bEnable)
        {


            if (Infos == null || string.IsNullOrEmpty(Infos.ExtensionNativeId))
                throw new EudoException("Inforamtions sur l'extensions non valide (Infos ou Infos.ExtensionNativeId) vide", "Impossible de traiter votre demande");

            List<eExtension> a = eExtension.GetExtensionsByCode(_pref, Infos.ExtensionNativeId);
            eExtension newExt;
            if (a.Count == 0)
            {
                newExt = eExtension.GetNewExtension(Infos.ExtensionNativeId);

            }
            else
            {
                newExt = a.Find(zz => zz.UserId == 0);
                if (newExt == null)
                    newExt = eExtension.GetNewExtension(Infos.ExtensionNativeId);


            }

            EXTENSION_STATUS oldStatus = newExt.Status;

            newExt.Status = GetExtensionStatus(bEnable);

            SetDefaultParam(newExt);

            if (newExt.Status != oldStatus)
                UpdateParamChangeStatus(newExt, oldStatus);


            Infos.Status = newExt.Status;

            string rootPhysicalDatasPath = eModelTools.GetRootPhysicalDatasPath();
            if (eExtension.UpdateExtension(newExt, _pref, _pref.User, rootPhysicalDatasPath, _pref.ModeDebug))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Méthode à appelér après avoir effectué le traitement spécifique à chaque extension pour l'activer ou la désactiver,
        /// afin de remettre à jour les infos internes de l'objet eAdminExtension sans avoir à le réinstancier complètement
        /// </summary>
        /// <param name="bEnable">true pour activer l'extension, false pour la désactiver</param>
        public abstract bool AfterEnableProcess(bool bEnable, out string sError);

        /// <summary>
        /// Renvoie une liste de contrôles HTML à afficher en tant qu'informations détaillées de l'extension sur la liste des extensions en administration
        /// </summary>
        /// <returns>Liste de contrôles HTML à afficher en tant qu'informations détaillées de l'extension sur la liste des extensions en administration</returns>
        public abstract List<HtmlGenericControl> GetModuleInfo();

        /// <summary>
        /// Instancie un objet eAdminExtension à partir de la référence correspondante dans l'objet eUserOptionsModules.USROPT_MODULE
        /// sans effectuer d'appel à l'API en vue de récupérer les informations marketing relatives à l'extension
        /// L'objet renvoyé comportera donc uniquement les informations natives issues du code d'XRM
        /// </summary>
        /// <param name="module">Objet eUserOptionsModules.USROPT_MODULE correspondant à l'extension souhaitée</param>
        /// <param name="pref">Objet Pref</param>
        /// <returns></returns>
        public static eAdminExtension InitFromModule(eUserOptionsModules.USROPT_MODULE module, ePref pref)
        {
            return InitFromModule(module, pref, 0);
        }

        /// <summary>
        /// Instancie un objet eAdminExtension à partir de la référence correspondante dans l'objet eUserOptionsModules.USROPT_MODULE
        /// Si le FileID correspondant à cette extension sur le store (HotCom) est précisé, on effectue un appel à la base de référence du store via l'API
        /// pour récupérer toutes les informations marketing relatives à l'extension
        /// </summary>
        /// <param name="module">Objet eUserOptionsModules.USROPT_MODULE correspondant à l'extension souhaitée</param>
        /// <param name="pref">Objet Pref</param>
        /// <param name="extensionFileIdFromStore">FileID correspondant à cette extension sur la base de référence du store (HotCom, table Produits)</param>
        /// <returns></returns>
        public static eAdminExtension InitFromModule(eUserOptionsModules.USROPT_MODULE module, ePref pref, int extensionFileIdFromStore)
        {
            eAdminExtension extension = null;

            // Si on reçoit un FileId, on instancie et paramètre l'extension à partir des informations disponibles sur le store (appel API vers HotCom)
            if (extensionFileIdFromStore > 0)
            {
                eAPIExtensionStoreAccess storeAccess = new eAPIExtensionStoreAccess(pref);
                extension = storeAccess.GetExtensionFile(extensionFileIdFromStore);

            }
            // Paramétrage à partir des informations disponibles nativement
            else
            {
                switch (module)
                {
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO:
                        extension = new eAdminExtensionSynchro(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_OFFICE365:
                        extension = new eAdminExtensionSynchroExchange(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_MOBILE:
                        extension = new eAdminExtensionMobile(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN:
                        extension = new eAdminExtensionOutlookAddin(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_LINKEDIN:
                        extension = new eAdminExtensionLinkedin(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SMS:
                        extension = new eAdminExtensionSMS(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_CTI:
                        extension = new eAdminExtensionCTI(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_CARTO:
                        extension = new eAdminExtensionCarto(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_API:
                        extension = new eAdminExtensionAPI(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING:
                        extension = new eAdminExtensionExternalMailing(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_VCARD:
                        extension = new eAdminExtensionVCard(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SNAPSHOT:
                        extension = new eAdminExtensionSnapshot(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING:
                        extension = new eAdminExtensionEmailing(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_GRID:
                        extension = new eAdminExtensionGrid(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS:
                        extension = new eAdminExtensionNotifications(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE:
                        extension = new eAdminExtensionSirene(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI:
                        extension = new eAdminExtensionPowerBI(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT:
                        extension = new eAdminExtensionAccountingBusinessSoft(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_CEGID:
                        extension = new eAdminExtensionAccountingCegid(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SAGE:
                        extension = new eAdminExtensionAccountingSage(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_EBP:
                        extension = new eAdminExtensionAccountingEBP(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SIGMA:
                        extension = new eAdminExtensionAccountingSigma(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_IN_UBIFLOW:
                        extension = new eAdminExtensionInUbiflow(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_IN_HBS:
                        extension = new eAdminExtensionInHBS(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_DOCUSIGN:
                        extension = new eAdminExtensionDocuSign(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SMS_NETMESSAGE:
                        extension = new eAdminExtensionSMSNetMessage(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE:
                        extension = new eAdminExtensionSynchroExchange2016OnPremise(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ZAPIER:
                        extension = new eAdminExtensionZapier(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EXTRANET:
                        extension = new eAdminExtensionExtranet(pref);
                        break;
                    //SHA : tâche #1 873
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ADVANCED_FORM:
                        extension = new eAdminExtensionAdvancedForm(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_DEDICATED_IP:
                        extension = new eAdminExtensionDedicatedIp(pref);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_WORLDLINE_PAYMENT:
                        extension = new eAdminExtensionWorldlinePayment(pref);
                        break;
                    //Ce cas ne devrait pas être utilisé, sauf si on souhaite réellement instancier une extension à vide(sans FileID correspondant)
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_FROMSTORE:
                        extension = new eAdminExtensionFromStore(pref);
                        extension._needsFullRefreshAfterEnable = false;

                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_MAIL_STATUS_VERIFICATION:
                        extension = new eAdminExtensionMailVerification(pref);
                        break;
                }

                extension.SetInfos(pref);
            }

            return extension;
        }

        /// <summary>
        /// Retourne la liste des extensions depuis le json
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static List<eAdminExtension> initListExtensionFromJson(ePref pref)
        {
            List<eAdminExtension> extensionFromJson = new List<eAdminExtension>();
            using (StreamReader r = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\res\\ExtensionList.json"))
            {
                string json = r.ReadToEnd();
                extensionFromJson = JsonConvert.DeserializeObject<List<eAdminExtension>>(json, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });

                foreach (eAdminExtension ext in extensionFromJson)
                {
                    string strError = String.Empty;
                    ext.Pref = pref;
                    ext.DAL = eLibTools.GetEudoDAL(pref);
                    ext.InitConfig(pref, out strError);

                    // Temporairement ici pour le chargement du json
                    ext.Infos.InitOfferList();
                }
            }
            return extensionFromJson;
        }

        /// <summary>
        /// Retourne l extension voulu dans le Json
        /// </summary>
        /// <param name="module">Module selectionné</param>
        /// <param name="pref">Pref</param>
        /// <returns></returns>
        public static eAdminExtension initExtensionFromJson(eUserOptionsModules.USROPT_MODULE module, ePref pref, int extensionFileIdFromStore = 0)
        {
            List<eAdminExtension> extensionFromJson = initListExtensionFromJson(pref);

            return extensionFileIdFromStore == 0 ? extensionFromJson.Where(item => item.Module == module).FirstOrDefault() :
                extensionFromJson.Where(item => item.Infos.ExtensionFileId == extensionFileIdFromStore).FirstOrDefault();
        }


        /// <summary>
        /// Intialisation d'une extension à partir d'un ExtensionGlobalInfo
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="oExtensionGlobalInfo"></param>
        /// <param name="screenshots"></param>
        internal void SetInfos(ePref pref, ExtensionGlobalInfo oExtensionGlobalInfo, List<eAPIProductScreenshot> screenshots)
        {
            this.Title = oExtensionGlobalInfo.Title;


            this.Infos = new eAdminExtensionInfo(this, oExtensionGlobalInfo, true, screenshots);



            this.Infos.IsEnabled = this.IsEnabled();
        }



        /// <summary>
        /// Paramètre les informations de base de l'extension à partir des préférences uniquement
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="bIsEnabled">status activé pour l'extension</param>
        public void SetInfos(ePrefLite pref, bool? bIsEnabled = null)
        {
            // Mise à jour du statut d'activation via requête en base, sauf si forcé en paramètre
            Boolean enabled = bIsEnabled ?? this.IsEnabled();
            string title = this.Title;
            string icon = String.Empty;
            string summary = String.Empty;
            string description = String.Empty;
            IDictionary<string, string> categories = new Dictionary<string, string>();
            List<eAPIProductScreenshot> screenshots = new List<eAPIProductScreenshot>();
            string author = String.Empty;
            string authorUrl = String.Empty;
            string tooltip = String.Empty;
            KeyValuePair<string, double?> notation = new KeyValuePair<string, double?>();
            int nbInstall = 0;
            DateTime lastUpdate = default(DateTime);
            string version = "10.301";
            bool isNew = true;
            string extensionNativeId = String.Empty;
            int extensionFileId = 0;
            //SHA
            int productDescriptionFileId = 0;
            string sMinVersionInfo = "10.303.000";

            bool bHasCustomParameter = false;

            // Reprise des informations précédemment paramétrées si existantes
            // Permet de ne pas les réécraser et requêter de nouveau l'API lorsqu'on appelle SetInfos() pour mettre à jour Enabled
            if (this.Infos != null)
            {
                icon = this.Infos.Icon;
                summary = this.Infos.Summary;
                description = this.Infos.Description;
                categories = this.Infos.Categories;
                screenshots = this.Infos.Screenshots;
                author = this.Infos.Author;
                authorUrl = this.Infos.AuthorUrl;
                tooltip = this.Infos.Tooltip;
                notation = this.Infos.Notation;
                nbInstall = this.Infos.NbInstallations;
                lastUpdate = this.Infos.LastUpdate;
                version = this.Infos.Version;
                isNew = this.Infos.IsNewExtension;
                extensionNativeId = this.Infos.ExtensionNativeId;
                extensionFileId = this.Infos.ExtensionFileId;
                sMinVersionInfo = this.Infos.MinEudoVersion;
                //SHA
                productDescriptionFileId = this.Infos.ProductDescriptionFileId;
                bHasCustomParameter = this.Infos.HasCustomParameter;

            }
            // Si on instancie une nouvelle extension, ajout de valeurs par défaut
            else
            {
                // TODO TEMPORAIRE - Notes en dur pour tester les filtres et l'affichage
                switch (this.Module)
                {
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_MOBILE:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE:
                        notation = new KeyValuePair<string, double?>("5", 5);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_OFFICE365:
                        notation = new KeyValuePair<string, double?>("4", 4);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SMS:
                        notation = new KeyValuePair<string, double?>("3", 3);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING:
                        notation = new KeyValuePair<string, double?>("2", 2);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SNAPSHOT:
                        notation = new KeyValuePair<string, double?>("1", 1);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_CTI:
                        notation = new KeyValuePair<string, double?>("0", 0); // TOCHECK: voir si les notes de zéro seront acceptées. Ajouté pour tester la distinction pas de note / note à zéro
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_GRID:
                        notation = new KeyValuePair<string, double?>("1", 1); // TOCHECK: voir si les notes de zéro seront acceptées. Ajouté pour tester la distinction pas de note / note à zéro
                        break;
                }
            }

            this.Infos = new eAdminExtensionInfo(
                this,
                title,
                icon,
                categories,
                screenshots,
                summary,
                description,
                enabled,
                tooltip,
                author,
                authorUrl,
                notation,
                nbInstall,
                lastUpdate,
                version,
                isNew,
                extensionNativeId,
                extensionFileId,
                sMinVersionInfo,
                productDescriptionFileId,
                bHasCustomParameter
            );


        }



        /// <summary>
        /// Construit un tableau de paramètres additionnels passés au manager de l'extension, afin d'effectuer certains traitements spécifiques par extension
        /// - passage de paramètres à eAdmin.enableExtension pour certaines extensions
        /// - récupération de ces paramètres par eAdminExtensionManager sur l'objet eAdminExtension
        /// - l'objet eAdminExtension comporte ainsi des paramètres qui peuvent être utilisés en interne selon les besoins
        /// Exemple : Synchros - Activation d'un seul type de synchro (Agendas ou Contacts)
        /// </summary>
        /// <param name="requestTools"></param>
        public void SetAdditionalParameters(eRequestTools requestTools)
        {
            this.AdditionalParameters = new Dictionary<string, string>();
            string paramPrefix = String.Concat("additionalparam_", this.Module.ToString().Replace("ADMIN_EXTENSIONS_", String.Empty).ToLower(), "_");

            foreach (string key in requestTools.AllKeys)
            {
                if (key.StartsWith(paramPrefix))
                    this.AdditionalParameters.Add(key.Replace(paramPrefix, String.Empty), requestTools.GetRequestFormKeyS(key));
            }
        }
    }

    /// <summary>
    /// Classe contenant les infos d'une extension
    /// </summary>
    public class eAdminExtensionInfo
    {

        /// <summary>
        /// Indque que l'extension nécessite une activation à la demande
        /// </summary>
        public bool DelayedActivation = true;



        private ExtensionGlobalInfo oExtensionGlobalInfo;

        #region accesseurs


        private List<eAPIProductScreenshot> _Screenshots = new List<eAPIProductScreenshot>();

        /// <summary>
        /// Liste des visuels et du code html de rendu
        /// </summary>
        public List<eAPIProductScreenshot> Screenshots { get { return _Screenshots; } private set { _Screenshots = value; } }

        /// <summary>
        /// Titre
        /// </summary>
        public String Title { get { return oExtensionGlobalInfo.Title; } }

        /// <summary>
        /// Tarifs
        /// </summary>
        //public List<ExtensionPrice> Tarifs { get { return oExtensionGlobalInfo.DescriptionInfos.Tarifs; } }

        /// <summary>
        /// Tarifs
        /// </summary>
        public ExtensionPrice Price { get { return oExtensionGlobalInfo.PriceInfos; } }

        /// <summary>
        /// Icon de l'extension
        /// </summary>
        public String Icon { get { return oExtensionGlobalInfo.Icon; } }

        /// <summary>
        /// Liste des catégories
        /// </summary>
        public IDictionary<String, String> Categories { get { return oExtensionGlobalInfo.Categories; } }

        /// <summary>
        /// Résumé du descriptif
        /// </summary>
        public String Summary { get { return oExtensionGlobalInfo.DescriptionInfos.Summary; } }

        /// <summary>
        /// Descriptif
        /// </summary>
        public String Description { get { return oExtensionGlobalInfo.DescriptionInfos.Description; } }

        /// <summary>
        /// Texte logo
        /// </summary>
        public String TexteLogo { get { return oExtensionGlobalInfo.DescriptionInfos.TexteLogo; } }

        /// <summary>
        /// Auteur de l'extenion
        /// </summary>
        public String Author { get { return oExtensionGlobalInfo.Author; } }

        /// <summary>
        /// Site de l'éditeur de l'extension
        /// </summary>
        public String AuthorUrl { get { return oExtensionGlobalInfo.AuthorUrl; } }

        /// <summary>
        /// Note
        /// </summary>
        public KeyValuePair<String, Double?> Notation { get { return oExtensionGlobalInfo.Note; } }

        /// <summary>
        /// Nombdre d'install
        /// </summary>
        public Int32 NbInstallations { get { return oExtensionGlobalInfo.InstallCount; } }

        /// <summary>
        /// Date de dernière mise à jour
        /// </summary>
        public DateTime LastUpdate { get { return oExtensionGlobalInfo.VersionInfos.DateUpdate; } }

        /// <summary>
        /// Description des nouveauté de la version
        /// </summary>
        public String Version { get { return oExtensionGlobalInfo.VersionInfos.ChangeLog; } }

        /// <summary>
        /// Indicateur du bandeau nouveau
        /// </summary>
        public Boolean IsNewExtension { get { return oExtensionGlobalInfo.IsNew; } }

        /// <summary>
        /// Tooltip
        /// </summary>
        public String Tooltip { get { return oExtensionGlobalInfo.DescriptionInfos.Tooltip; } }

        /// <summary>
        /// Information sur la procédure d'installation
        /// </summary>
        public string InstallationInfos { get { return oExtensionGlobalInfo.DescriptionInfos.InstallationInformation; } }

        /// <summary>
        /// Code de l'extension
        /// </summary>
        public String ExtensionNativeId { get { return oExtensionGlobalInfo.ExtensionNativeId; } }


        /// <summary>
        /// Indicateur du bandeau nouveau
        /// </summary>
        public Boolean HasCustomParameter { get { return oExtensionGlobalInfo?.HasCustomParam ?? false; } }

        /// <summary>
        /// Id de la fiche produit sur HOTCOM 
        /// </summary>
        public int ExtensionFileId { get { return oExtensionGlobalInfo.ExtensionFileId; } }

        //SHA
        /// <summary>
        /// Id de la fiche produit description sur HOTCOM
        /// </summary>
        public int ProductDescriptionFileId { get { return oExtensionGlobalInfo.DescriptionInfos.FileId; } }


        /// <summary>
        /// Indique si l'extension est activé sur la base
        /// </summary>
        public Boolean IsEnabled { get; internal set; }


        /// <summary>
        /// Indique si l'extension est native
        /// </summary>
        public bool IsNativeExtension { get { return oExtensionGlobalInfo.IsNativeExtension; } }


        /// <summary>
        /// Version minimale d'eudo
        /// </summary>
        public string MinEudoVersion { get { return oExtensionGlobalInfo.VersionInfos.MinEudoVersion; } }

        /// <summary>
        /// Liste des offres pour lesquelles l'extension est dispo
        /// </summary>
        [Obsolete("use OffersLst")]
        public string Offers { get { return oExtensionGlobalInfo.Offers; } }


        /// <summary>
        /// Liste des identifiants offres pour lesquelles l'extension est dispo
        /// </summary>
        [Obsolete("use OffersLst")]
        public string IdOffers { get { return oExtensionGlobalInfo.IdOffers; } }

        /// <summary>
        /// Liste des offres pour lesquelles l'extension est dispo
        /// </summary>
        public IEnumerable<ExtensionOffer> OffersLst { get; set; }

        //SHA
        /// <summary>
        /// Liste des docs de l'extension
        /// </summary>
        public List<ProductDescriptionDoc> DocsManTuto { get { return oExtensionGlobalInfo.DescriptionInfos.DocsManTuto; } }

        #endregion

        /// <summary>
        /// Indique si l'extension est compatible avec la version d'eudo
        /// </summary>
        public bool IsCompatible
        {
            get
            {
                return string.Compare(MinEudoVersion, eConst.VERSION) <= 0;
            }
        }

        /// <summary>
        /// Status de l'extension
        /// </summary>
        public EXTENSION_STATUS Status = EXTENSION_STATUS.STATUS_UNDEFINED;

        /// <summary>
        /// Création d'un eAdminExtensionInfo à partir d'un ExtensionGlobalInfo
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="o">ExtensionGlobalInfo</param>
        /// <param name="enabled"></param>
        /// <param name="screenshots">Copies d'écran</param>
        public eAdminExtensionInfo(eAdminExtension extension, ExtensionGlobalInfo o, bool enabled, List<eAPIProductScreenshot> screenshots)
        {
            oExtensionGlobalInfo = o;

            IsEnabled = enabled;
            Screenshots = screenshots;

            #region Correction des informations

            // Icône - doit correspondre à une classe CSS
            if (String.IsNullOrEmpty(oExtensionGlobalInfo.Icon))
                oExtensionGlobalInfo.Icon = "icon-" + eUserOptionsModules.GetModuleIcon(extension.Module);

            // Info-bulle - Utilisation d'un texte par défaut si non définie
            if (String.IsNullOrEmpty(oExtensionGlobalInfo.DescriptionInfos.Tooltip))
            {
                oExtensionGlobalInfo.DescriptionInfos.Tooltip = enabled ?
                    eResApp.GetRes(extension.Pref, 7863).Replace("<EXTENSION>", extension.Title) :
                     eResApp.GetRes(extension.Pref, 7864).Replace("<EXTENSION>", extension.Title); // Cliquez ici pour administrer les paramètres de l'extension <EXTENSION> - L'extension <EXTENSION> n'est pas activée sur votre base. Cliquez sur le bouton ci-contre pour l'activer.
            }

            // Catégories - Permet d'ajouter le filtre "Aucune" dans la liste des catégories disponibles
            if (oExtensionGlobalInfo.Categories == null || oExtensionGlobalInfo.Categories.Count == 0)
            {
                oExtensionGlobalInfo.Categories = new Dictionary<String, String>();
                oExtensionGlobalInfo.Categories.Add("-1", "-1");
            }

            // Offres
            InitOfferList();

            //SHA
            oExtensionGlobalInfo.DescriptionInfos.FileId = extension.Infos.ProductDescriptionFileId;

            #endregion
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="title"></param>
        /// <param name="icon"></param>
        /// <param name="categories"></param>
        /// <param name="screenshots"></param>
        /// <param name="summary"></param>
        /// <param name="description"></param>
        /// <param name="enabled"></param>
        /// <param name="tooltip"></param>
        /// <param name="author"></param>
        /// <param name="authorUrl"></param>
        /// <param name="note"></param>
        /// <param name="nbInstall"></param>
        /// <param name="lastUpdate"></param>
        /// <param name="version"></param>
        /// <param name="isNew"></param>
        /// <param name="extensionNativeId"></param>
        /// <param name="extensionFileId"></param>
        /// <param name="sInstallationInfos"></param>
        /// <param name="productDescriptionFileId"></param>
        /// <param name="bHasCustomParam">Indique si l'extension dispose de parametre librement personalisable</param>

        [JsonConstructor]
        public eAdminExtensionInfo(
            eAdminExtension extension,
            String title, String icon,
            IDictionary<String, String> categories, List<eAPIProductScreenshot> screenshots, String summary, String description,
            Boolean enabled,
            String tooltip = "",
            String author = "Eudonet",
            String authorUrl = "",
            KeyValuePair<String, double?> note = new KeyValuePair<String, double?>(), Int32 nbInstall = 0,
            DateTime lastUpdate = new DateTime(), String version = "", Boolean isNew = true, string extensionNativeId = "", int extensionFileId = 0, string sInstallationInfos = ""
            //SHA
            , int productDescriptionFileId = 0,
            bool bHasCustomParam = false


        )
        {

            oExtensionGlobalInfo = new ExtensionGlobalInfo()
            {
                Title = title,
                Categories = categories,
                Icon = icon,
                Author = author,
                AuthorUrl = authorUrl,
                Note = note,
                InstallCount = nbInstall,
                ExtensionFileId = extensionFileId,
                ExtensionNativeId = extensionNativeId,
                HasCustomParam = bHasCustomParam
            };

            oExtensionGlobalInfo.DescriptionInfos = new ExtensionProductDescription()
            {
                Summary = summary,
                Description = description,
                Tooltip = tooltip,
                InstallationInformation = sInstallationInfos,
                //SHA
                FileId = productDescriptionFileId
            };

            oExtensionGlobalInfo.VersionInfos = new ExtensionVersionInfo()
            {
                DateUpdate = lastUpdate,
                ChangeLog = version,
                IsNew = isNew,
                MinEudoVersion = "10.303.000"
            };

            IsEnabled = enabled;
            Screenshots = screenshots;
        }

        internal void InitOfferList()
        {
            string[] offerId = oExtensionGlobalInfo.IdOffers.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            OffersLst = offerId.Select(ofId =>
            {
                ClientOffer offre = ClientOffer.XRM;
                int idCat = eLibTools.GetNum(ofId);

                switch (idCat)
                {
                    case (int)ExtEnum.DATA_PRODUCT_OFFER.ACCES:
                        offre = ClientOffer.ACCES;
                        break;
                    case (int)ExtEnum.DATA_PRODUCT_OFFER.STANDARD:
                        offre = ClientOffer.STANDARD;
                        break;
                    case (int)ExtEnum.DATA_PRODUCT_OFFER.PREMIER:
                        offre = ClientOffer.PREMIER;
                        break;
                    case (int)ExtEnum.DATA_PRODUCT_OFFER.PRO:
                        offre = ClientOffer.PRO;
                        break;
                    case (int)ExtEnum.DATA_PRODUCT_OFFER.ESSENTIEL:
                        offre = ClientOffer.ESSENTIEL;
                        break;
                }
                return new ExtensionOffer
                {
                    Offer = offre,
                    DbId = idCat
                };
            }).OrderBy(c => (int)c.Offer);
        }



        /// <summary>
        /// indique si l'extension est disponible pour l'offre indiquée
        /// </summary>
        /// <param name="offer"></param>
        /// <returns></returns>
        public bool IsAvailableInOffer(ClientOffer offer)
        {
            return (OffersLst != null && OffersLst.Count() > 0) && OffersLst.Any(e => e.Offer == offer);
        }


    }

    /// <summary>
    /// Classe d'exception pour les extensions
    /// </summary>
    public class eAdminExtensionException : Exception
    {
        /// <summary>
        /// Construteur
        /// </summary>
        /// <param name="msg">message</param>
        /// <param name="e">exception interne</param>
        public eAdminExtensionException(String msg, Exception e) : base(msg, e) { }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="msg">message</param>
        public eAdminExtensionException(String msg) : base(msg) { }

    }
}