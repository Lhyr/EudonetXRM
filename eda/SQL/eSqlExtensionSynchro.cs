using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using EudoExtendedClasses;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Traitements SQL liés aux modules EudoSync
    /// </summary>
    public class eSqlExtensionSynchro
    {
        /// <summary>
        /// Détermine si au moins un module de synchro est activé en base ou non
        /// </summary>
        /// <param name="pref">Objet pref</param>
        /// <param name="eDal">Objet eudoDAL</param>
        /// <param name="strError">Erreur éventuellement rencontrée</param>
        /// <returns>true si tous les tests permettent de considérer que l'extension est activée, false sinon</returns>
        public static bool IsEnabled(ePrefLite pref, eudoDAL eDal, out string strError)
        {
            return
                IsEudoSyncExchangeAgendasEnabled(pref, eDal, out strError) ||
                IsEudoSyncExchangeContactsEnabled(pref, eDal, out strError);
        }

        /// <summary>
        /// Détermine si le module EudoSync Exchange Agendas est activé en base ou non
        /// </summary>
        /// <param name="pref">Objet pref</param>
        /// <param name="eDal">Objet eudoDAL</param>
        /// <param name="sError">Erreur éventuellement rencontrée</param>
        /// <returns>true si tous les tests permettent de considérer que l'extension est activée, false sinon</returns>
        public static bool IsEudoSyncExchangeAgendasEnabled(ePrefLite pref, eudoDAL eDal, out string strError)
        {
            int partiallyEnabledUserCount = -1;
            int fullyEnabledUserCount = GetEudoSyncExchangeAgendasUserCount(pref, eDal, out partiallyEnabledUserCount, out strError);
            // On considère que l'extension est activée à partir du moment où au moins un utilisateur est partiellement paramétré.
            // Ce qui permet de ne pas bloquer l'accès à la configuration de l'extension pour justement permettre à l'administrateur de finaliser la configuration
            return fullyEnabledUserCount > 0 || partiallyEnabledUserCount > 0;
        }

        /// <summary>
        /// Détermine si le module EudoSync Exchange Contacts est activé en base ou non
        /// </summary>
        /// <param name="pref">Objet pref</param>
        /// <param name="eDal">Objet eudoDAL</param>
        /// <param name="sError">Erreur éventuellement rencontrée</param>
        /// <returns>true si tous les tests permettent de considérer que l'extension est activée, false sinon</returns>
        public static bool IsEudoSyncExchangeContactsEnabled(ePrefLite pref, eudoDAL eDal, out string strError)
        {
            int partiallyEnabledUserCount = -1;
            int fullyEnabledUserCount = GetEudoSyncExchangeContactsUserCount(pref, eDal, out partiallyEnabledUserCount, out strError);
            // On considère que l'extension est activée à partir du moment où au moins un utilisateur est partiellement paramétré.
            // Ce qui permet de ne pas bloquer l'accès à la configuration de l'extension pour justement permettre à l'administrateur de finaliser la configuration
            return fullyEnabledUserCount > 0 || partiallyEnabledUserCount > 0;
        }

        /// <summary>
        /// Détermine si le module EudoSync Exchange Agendas est activé en base ou non
        /// </summary>
        /// <param name="pref">Objet pref</param>
        /// <param name="eDal">Objet eudoDAL</param>
        /// <param name="sError">Erreur éventuellement rencontrée</param>
        /// <returns>true si tous les tests permettent de considérer que l'extension est activée, false sinon</returns>
        public static bool IsEudoSyncExchangeAgendasEnabled(ePrefLite pref, eudoDAL eDal, out int partiallyEnabledUserCount, out string strError)
        {
            int fullyEnabledUserCount = GetEudoSyncExchangeAgendasUserCount(pref, eDal, out partiallyEnabledUserCount, out strError);
            // On considère que l'extension est activée à partir du moment où au moins un utilisateur est partiellement paramétré.
            // Ce qui permet de ne pas bloquer l'accès à la configuration de l'extension pour justement permettre à l'administrateur de finaliser la configuration
            return fullyEnabledUserCount > 0 || partiallyEnabledUserCount > 0;
        }

        /// <summary>
        /// Détermine si le module EudoSync Exchange Contacts est activé en base ou non
        /// </summary>
        /// <param name="pref">Objet pref</param>
        /// <param name="eDal">Objet eudoDAL</param>
        /// <param name="sError">Erreur éventuellement rencontrée</param>
        /// <returns>true si tous les tests permettent de considérer que l'extension est activée, false sinon</returns>
        public static bool IsEudoSyncExchangeContactsEnabled(ePrefLite pref, eudoDAL eDal, out int partiallyEnabledUserCount, out string strError)
        {
            int fullyEnabledUserCount = GetEudoSyncExchangeContactsUserCount(pref, eDal, out partiallyEnabledUserCount, out strError);
            // On considère que l'extension est activée à partir du moment où au moins un utilisateur est partiellement paramétré.
            // Ce qui permet de ne pas bloquer l'accès à la configuration de l'extension pour justement permettre à l'administrateur de finaliser la configuration
            return fullyEnabledUserCount > 0 || partiallyEnabledUserCount > 0;
        }

        /// <summary>
        /// Retourne le nombre d'utilisateurs pour lesquels la synchronisation Agendas est activée
        /// </summary>
        /// <param name="pref">Objet pref</param>
        /// <param name="eDal">Objet eudoDAL</param>
        /// <param name="sError">Erreur éventuellement rencontrée</param>
        /// <returns>Nombre d'utilisateurs pour lesquels la synchronisation Agendas est activée (-1 si erreur)</returns>
        public static int GetEudoSyncExchangeAgendasUserCount(ePrefLite pref, eudoDAL eDal, out int partiallyEnabledUserCount, out string sError)
        {
            sError = String.Empty;
            List<eEudoSyncExchangeAgendasUser> partiallyEnabledUserList = null;
            List<eEudoSyncExchangeAgendasUser> fullyEnabledUserList = eSqlExtensionSynchro.GetEudoSyncExchangeAgendasUserList(pref, eDal, out partiallyEnabledUserList, out sError);
            partiallyEnabledUserCount = partiallyEnabledUserList == null ? -1 : partiallyEnabledUserList.Count;
            return fullyEnabledUserList == null ? -1 : fullyEnabledUserList.Count;
        }

        /// <summary>
        /// Retourne le nombre d'utilisateurs pour lesquels la synchronisation Contacts est activée
        /// </summary>
        /// <param name="pref">Objet pref</param>
        /// <param name="eDal">Objet eudoDAL</param>
        /// <param name="sError">Erreur éventuellement rencontrée</param>
        /// <returns>Nombre d'utilisateurs pour lesquels la synchronisation Contacts est activée (-1 si erreur)</returns>
        public static int GetEudoSyncExchangeContactsUserCount(ePrefLite pref, eudoDAL eDal, out int partiallyEnabledUserCount, out string sError)
        {
            sError = String.Empty;
            List<eEudoSyncExchangeContactsUser> partiallyEnabledUserList = null;
            List<eEudoSyncExchangeContactsUser> fullyEnabledUserList = eSqlExtensionSynchro.GetEudoSyncExchangeContactsUserList(pref, eDal, out partiallyEnabledUserList, out sError);
            partiallyEnabledUserCount = partiallyEnabledUserList == null ? -1 : partiallyEnabledUserList.Count;
            return fullyEnabledUserList == null ? -1 : fullyEnabledUserList.Count;
        }

        /// <summary>
        /// Retourne la liste des utilisateurs pour lesquels la synchronisation Agendas est activée
        /// </summary>
        /// <param name="pref">Objet pref</param>
        /// <param name="eDal">Objet eudoDAL</param>
        /// <param name="sError">Erreur éventuellement rencontrée</param>
        /// <returns>Nombre d'utilisateurs pour lesquels la synchronisation Agendas est activée (-1 si erreur)</returns>
        public static List<eEudoSyncExchangeAgendasUser> GetEudoSyncExchangeAgendasUserList(ePrefLite pref, eudoDAL eDal, out List<eEudoSyncExchangeAgendasUser> partiallyEnabledUserList, out string sError)
        {
            String sSQL = String.Empty;
            DataTableReaderTuned dtr = null;
            sError = String.Empty;
            List<eEudoSyncExchangeAgendasUser> fullyEnabledUserList = new List<eEudoSyncExchangeAgendasUser>();
            partiallyEnabledUserList = new List<eEudoSyncExchangeAgendasUser>();

            try
            {
                eDal.OpenDatabase();

                if (!eDal.IsFieldExists("USER", "SynchroExchange2Enabled"))
                {

                    return new List<eEudoSyncExchangeAgendasUser>();
                }

                sSQL = String.Concat(
                    "SELECT [UserId], [UserLogin], [UserDisplayName], ",
                    "ISNULL([SynchroExchange2Enabled], 0) AS [SynchroExchange2Enabled], ",
                    "ISNULL([UserMail], '') AS [UserMail] ",
                    "FROM [USER] ",
                    "WHERE ISNULL([SynchroExchange2Enabled], 0) = 1"
                    );
                RqParam rq = new RqParam(sSQL);
                dtr = eDal.Execute(rq, out sError);
                while (dtr.Read())
                {
                    eEudoSyncExchangeAgendasUser user = new eEudoSyncExchangeAgendasUser();
                    user.UserId = dtr.GetEudoNumeric("UserId");
                    user.UserLogin = dtr.GetString("UserLogin");
                    user.UserDisplayName = dtr.GetString("UserDisplayName");
                    user.UserMail = dtr.GetString("UserMail");
                    user.EudoSyncExchangeAgendasEnabled = dtr.GetBoolean("SynchroExchange2Enabled");

                    if (user.EudoSyncExchangeAgendasEnabled)
                    {
                        // TOCHECK : Sur EudoSync Exchange Agendas, on ne vérifie pas systématiquement qu'un utilisateur soit rattaché à une adresse e-mail valide
                        // pour le considérer comme synchronisé
                        if (user.UserMail.Trim().Length > 0)
                            fullyEnabledUserList.Add(user);
                        else
                            partiallyEnabledUserList.Add(user);
                    }
                }
                if (sError.Length > 0)
                {
                    // En cas d'erreur, on remonte l'information comme "inconnue" sur l'interface
                    fullyEnabledUserList = null;
                    partiallyEnabledUserList = null;
                }
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, sSQL, Environment.NewLine, e.StackTrace);
                // En cas d'erreur, on remonte l'information comme "inconnue" sur l'interface
                fullyEnabledUserList = null;
                partiallyEnabledUserList = null;
            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();

                eDal.CloseDatabase();
            }

            return fullyEnabledUserList;
        }

        /// <summary>
        /// Retourne la liste des utilisateurs pour lesquels la synchronisation Contacts est activée
        /// </summary>
        /// <param name="pref">Objet pref</param>
        /// <param name="eDal">Objet eudoDAL</param>
        /// <param name="sError">Erreur éventuellement rencontrée</param>
        /// <returns>Nombre d'utilisateurs pour lesquels la synchronisation Agendas est activée (-1 si erreur)</returns>
        public static List<eEudoSyncExchangeContactsUser> GetEudoSyncExchangeContactsUserList(ePrefLite pref, eudoDAL eDal, out List<eEudoSyncExchangeContactsUser> partiallyEnabledUserList, out string sError)
        {
            String sSQL = String.Empty;
            DataTableReaderTuned dtr = null;
            sError = String.Empty;
            List<eEudoSyncExchangeContactsUser> fullyEnabledUserList = new List<eEudoSyncExchangeContactsUser>();
            partiallyEnabledUserList = new List<eEudoSyncExchangeContactsUser>();

            try
            {
                eDal.OpenDatabase();

                if (!eDal.IsFieldExists("USER", "SynchroContactExchangeEnabled") 
                    || !eDal.IsFieldExists("USER", "UserContactFolderName")
                    || !eDal.IsFieldExists("USER", "UserExchangeServer")
                    )
                {

                    return new List<eEudoSyncExchangeContactsUser>();
                }

                // On respecte la règle utilisée sur EudoSync Exchange Contacts, où un utilisateur est considéré comme activé s'il est flaggé ainsi en base
                // ET s'il dispose d'une adresse mail et d'un lien vers un serveur Exchange référencé dans le mapping.
                sSQL = String.Concat(
                "SELECT [UserId], [UserLogin], [UserDisplayName], ",
                "ISNULL([SynchroContactExchangeEnabled], 0) AS [SynchroContactExchangeEnabled], ",
                "ISNULL([UserMail], '') AS [UserMail], ",
                "ISNULL([UserContactFolderName], '') AS [UserContactFolderName], ",
                "ISNULL([UserExchangeServer], '') AS [UserExchangeServer] ",
                "FROM [USER] ",
                "WHERE ISNULL([SynchroContactExchangeEnabled], 0) = 1"
                );
                RqParam rq = new RqParam(sSQL);
                dtr = eDal.Execute(rq, out sError);
                while (dtr.Read())
                {
                    eEudoSyncExchangeContactsUser user = new eEudoSyncExchangeContactsUser();
                    user.UserId = dtr.GetEudoNumeric("UserId");
                    user.UserLogin = dtr.GetString("UserLogin");
                    user.UserDisplayName = dtr.GetString("UserDisplayName");
                    user.UserMail = dtr.GetString("UserMail");
                    user.UserExchangeServer = dtr.GetString("UserExchangeServer");
                    user.EudoSyncExchangeContactsEnabled = dtr.GetBoolean("SynchroContactExchangeEnabled");

                    if (user.EudoSyncExchangeContactsEnabled)
                    {
                        // TOCHECK : Sur les versions plus récentes d'EudoSync Exchange Contacts, le champ UserContactFolderName n'est plus obligatoire.
                        // S'il n'est pas renseigné, la synchro utilisera le dossier Contacts par défaut.
                        // Vérifier, de ce fait, s'il est pertinent d'effectuer un test sur ce champ dans USER
                        if (user.UserMail.Trim().Length > 0 &&
                            //user.UserContactFolderName.Trim().Length > 0 &&
                            user.UserExchangeServer.Trim().Length > 0
                        )
                            fullyEnabledUserList.Add(user);
                        else
                            partiallyEnabledUserList.Add(user);
                    }
                }
                if (sError.Length > 0)
                {
                    // En cas d'erreur, on remonte l'information comme "inconnue" sur l'interface
                    fullyEnabledUserList = null;
                    partiallyEnabledUserList = null;
                }
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, sSQL, Environment.NewLine, e.StackTrace);
                // En cas d'erreur, on remonte l'information comme "inconnue" sur l'interface
                fullyEnabledUserList = null;
                partiallyEnabledUserList = null;
            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();

                eDal.CloseDatabase();
            }
            return fullyEnabledUserList;
        }

        /// <summary>
        /// Active ou désactive EudoSync Exchange Agendas
        /// </summary>
        /// <param name="pref">Objet pref</param>
        /// <param name="eDal">Objet eudoDAL</param>
        /// <param name="bEnable">true pour activer l'extension, false pour la désactiver</param>
        /// <param name="sError">Erreur éventuellement rencontrée</param>
        /// <returns>true si le processus (activation ou désactivation) a été correctement réalisé, false sinon</returns>
        public static bool EnableEudoSyncExchangeAgendas(ePrefLite pref, eudoDAL eDal, bool bEnable, out string sError)
        {
            String sSQL = String.Empty;
            DataTableReaderTuned dtr = null;
            sError = String.Empty;

            try
            {
                eDal.OpenDatabase();

                if (eDal.IsFieldExists("USER", "SynchroExchange2Enabled"))
                {
                    // TOCHECK/TOSPEC - Activation arbitraire de la synchro sur TOUS les utilisateurs de la base remplissant les prérequis
                    // Dans le cas d'EudoSync Exchange Agendas : le champ UserMail doit être rempli
                    if (bEnable)
                        sSQL = "UPDATE [USER] SET [SynchroExchange2Enabled] = 1 WHERE ISNULL([UserMail], '') <> ''";
                    else
                        sSQL = "UPDATE [USER] SET [SynchroExchange2Enabled] = 0 WHERE [SynchroExchange2Enabled] = 1";
                    RqParam rq = new RqParam(sSQL);
                    dtr = eDal.Execute(rq, out sError);
                    if (sError.Length > 0) { throw new Exception(sError); }
                }
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, sSQL, Environment.NewLine, e.StackTrace);
            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();

                eDal.CloseDatabase();
            }
            return sError.Trim().Length == 0;
        }

        /// <summary>
        /// Active ou désactive EudoSync Exchange Contacts
        /// </summary>
        /// <param name="pref">Objet pref</param>
        /// <param name="eDal">Objet eudoDAL</param>
        /// <param name="bEnable">true pour activer l'extension, false pour la désactiver</param>
        /// <param name="sError">Erreur éventuellement rencontrée</param>
        /// <returns>true si le processus (activation ou désactivation) a été correctement réalisé, false sinon</returns>
        public static bool EnableEudoSyncExchangeContacts(ePrefLite pref, eudoDAL eDal, bool bEnable, out String sError)
        {
            String sSQL = String.Empty;
            DataTableReaderTuned dtr = null;
            sError = String.Empty;

            try
            {
                eDal.OpenDatabase();

                if (eDal.IsFieldExists("USER", "SynchroContactExchangeEnabled"))
                {
                    // TOCHECK/TOSPEC - Activation arbitraire de la synchro sur TOUS les utilisateurs de la base remplissant les prérequis
                    // Dans le cas d'EudoSync Exchange Contacts : le champ UserMail doit être rempli, ainsi que l'alias du serveur Exchange paramétré sur EudoSync côté Exchange
                    // Sur les versions plus récentes d'EudoSync Exchange Contacts, le champ UserContactFolderName n'est plus obligatoire.
                    // On n'effectue donc pas de vérification par rapport à ce champ
                    if (bEnable)
                        sSQL = "UPDATE [USER] SET [SynchroContactExchangeEnabled] = 1 WHERE ISNULL([UserMail], '') <> '' AND ISNULL([UserExchangeServer], '') <> ''";
                    else
                        sSQL = "UPDATE [USER] SET [SynchroContactExchangeEnabled] = 0 WHERE [SynchroContactExchangeEnabled] = 1";
                    RqParam rq = new RqParam(sSQL);
                    dtr = eDal.Execute(rq, out sError);
                    if (sError.Length > 0) { throw new Exception(sError); }
                }
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, sSQL, Environment.NewLine, e.StackTrace);
            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();

                eDal.CloseDatabase();
            }
            return sError.Trim().Length == 0;
        }
    }
}