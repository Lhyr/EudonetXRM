using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public static class eAdminAccessSecurityIP
    {
        public static List<eAdminAccessSecurityIPData> GetIPAddresses(ePref pref, out string strError)
        {
            List<eAdminAccessSecurityIPData> ipList = new List<eAdminAccessSecurityIPData>();
            strError = String.Empty;
            eudoDAL dal = eLibTools.GetEudoDAL(pref);

            try
            {
                String sql = String.Concat(
                    "SELECT [IPADDR].*, ISNULL( [Mode], 0 ) AS [Mode], ISNULL( [Level], 1 ) AS [Level], ISNULL( [User], '' ) AS [User] ",
                    "FROM [IPADDR] LEFT JOIN [PERMISSION] ON [IPADDR].[PermId] = [PERMISSION].[PermissionId] ",
                    "ORDER BY [Libelle]"
                );

                RqParam rq = new RqParam(sql);

                dal.OpenDatabase();

                DataTableReaderTuned dtr = dal.Execute(rq, out strError);

                while (dtr.Read())
                {
                    decimal dIPId = dtr.GetDecimal("IPId"); // IPId est un decimal (numeric(18,0)) dans [IPADDR]...
                    string strLabel = dtr.GetString("Libelle");
                    string strIPAddress = dtr.GetString("IPAddress");
                    string strMask = dtr.GetString("SubnetMask");
                    decimal dLevel = dtr.GetDecimal("Level"); // idem
                    string strUser = dtr.GetString("User");
                    decimal dPermId = dtr.GetDecimal("PermId"); // idem
                    decimal dMode = dtr.GetDecimal("Mode"); // idem

                    eAdminAccessSecurityIPData ip = new eAdminAccessSecurityIPData(dIPId, strLabel, strIPAddress, strMask, strUser, dLevel, dPermId, (PermissionMode)dMode);
                    ipList.Add(ip);
                }
            }
            finally
            {
                if (dal != null && dal.IsOpen)
                    dal.CloseDatabase();
            }

            return ipList;
        }

        /// <summary>
        /// Ajoute ou met à jour une entrée d'adresse IP dans la table [IPADDR] avec sa permission associée dans [PERMISSION]
        /// </summary>
        /// <param name="pref">Objet Pref</param>
        /// <param name="ipData">Objet eAdminAccessSecurityIPData contenant les données de l'IP à ajouter/mettre à jour. Si ipData.IpId est inférieur à 0 alors AJOUT, sinon MAJ</param>
        /// <param name="strError">Erreur éventuellement survenue lors de l'opération</param>
        /// <returns>Dans le cas d'un AJOUT, renvoie l'ID (IpId) de l'IP insérée dans IPADDR. Sinon, renvoie le nombre de lignes MAJ</returns>
        public static int UpdateOrInsertIPAddress(ePref pref, eAdminAccessSecurityIPData ipData, out string strError)
        {
            int nUpdateCountOrAddedId = 0;
            strError = String.Empty;
            eudoDAL dal = eLibTools.GetEudoDAL(pref);

            try
            {
                dal.OpenDatabase();

                ePermission permission = null;
                ePermission.PermissionMode pMode = ePermission.PermissionMode.MODE_NONE;
                if (Enum.TryParse(ipData.PermissionMode.ToString(), out pMode))
                    permission = new ePermission((int)ipData.PermissionId, pMode, (int)ipData.Level, ipData.User);
                if (permission != null)
                    // Si la MAJ de la permission se fait correctement, on l'affecte à l'IP que l'on met à jour
                    if (permission.Save(dal ))
                    {
                        ipData.PermissionId = permission.PermId;
                        ipData.PermissionMode = (PermissionMode)permission.PermMode;
                    }

                // Requête par défaut : MAJ
                String sql = String.Concat(
                    "UPDATE [IPADDR] SET ",
                    "[Libelle] = @libelle, [IPAddress] = @ipAddress, [SubnetMask] = @ipMask, [PermId] = @permId ",
                    "WHERE [IPId] = @ipId"
                );
                // Cas de l'ajout : IPId < 0
                if (ipData.IpId < 0)
                    sql = String.Concat(
                    "INSERT INTO [IPADDR] ([Libelle], [IPAddress], [SubnetMask], [Desc], [PermId]) VALUES (",
                        "@libelle, @ipAddress, @ipMask, @desc, @permId",
                    "); SELECT @ipId = scope_Identity()"
                );

                RqParam rq = new RqParam(sql);

                rq.AddInputParameter("@libelle", System.Data.SqlDbType.VarChar, ipData.Label);
                rq.AddInputParameter("@ipAddress", System.Data.SqlDbType.VarChar, ipData.IpAddress);
                rq.AddInputParameter("@ipMask", System.Data.SqlDbType.VarChar, ipData.Mask);
                rq.AddInputParameter("@libelle", System.Data.SqlDbType.VarChar, ipData.Label);
                rq.AddInputParameter("@Desc", System.Data.SqlDbType.Text, /*ipData.Desc*/ String.Empty); // TODO: paramètre non repris ?
                rq.AddInputParameter("@permId", System.Data.SqlDbType.Decimal, ipData.PermissionId);

                // Ajout : renvoi de l'ID de l'IP nouvellement insérée
                if (ipData.IpId < 0) {
                    rq.AddOutputParameter("@ipId", System.Data.SqlDbType.Int, 0);
                }
                // MAJ : passage de l'ID de l'IP à mettre à jour
                else
                    rq.AddInputParameter("@ipId", System.Data.SqlDbType.Decimal, ipData.IpId);

                int updatedRows = dal.ExecuteNonQuery(rq, out strError);

                // Ajout : renvoi de l'ID de l'IP nouvellement insérée
                if (ipData.IpId < 0)
                    int.TryParse(rq.GetParamValue("@ipId").ToString(), out nUpdateCountOrAddedId);
                // MAJ : renvoi du nombre de lignes mises à jour
                else
                    nUpdateCountOrAddedId = updatedRows;
            }
            finally
            {
                if (dal != null && dal.IsOpen)
                    dal.CloseDatabase();
            }

            return nUpdateCountOrAddedId;
        }

        /// <summary>
        /// Supprime une entrée d'adresse IP dans la table [IPADDR] avec sa permission associée dans [PERMISSION]
        /// </summary>
        /// <param name="pref">Objet Pref</param>
        /// <param name="ipData">Objet eAdminAccessSecurityIPData contenant les données de l'IP à supprimer</param>
        /// <param name="strError">Erreur éventuellement survenue lors de l'opération</param>
        /// <returns>true si la suppression a été effectuée sans erreur, false sinon</returns>
        public static bool DeleteIPAddress(ePref pref, eAdminAccessSecurityIPData ipData, out string strError)
        {
            bool bSuccess = false;
            strError = String.Empty;
            eudoDAL dal = eLibTools.GetEudoDAL(pref);

            try
            {
                dal.OpenDatabase();

                ePermission permission = null;
                ePermission.PermissionMode pMode = ePermission.PermissionMode.MODE_NONE;
                if (Enum.TryParse(ipData.PermissionId.ToString(), out pMode))
                    permission = new ePermission((int)ipData.PermissionId, pMode, (int)ipData.Level, ipData.User);
                if (permission != null)
                    permission.Delete(dal);

                String sql = "DELETE FROM [IPADDR] WHERE [IPId] = @ipId";

                RqParam rq = new RqParam(sql);

                rq.AddInputParameter("@ipId", System.Data.SqlDbType.Int, ipData.IpId);

                int updatedRows = dal.ExecuteNonQuery(rq, out strError);

                bSuccess = updatedRows > 0 && strError.Length == 0;
            }
            finally
            {
                if (dal != null && dal.IsOpen)
                    dal.CloseDatabase();
            }

            return bSuccess;
        }

        /// <summary>
        /// Indique si l'accès par adresses IP est activé sur la base (CONFIG.IPAccessEnabled = 1)
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static bool GetIPAccessEnabled(ePref pref)
        {
            try {
                return pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[] { eLibConst.CONFIG_DEFAULT.IPAccessEnabled })[eLibConst.CONFIG_DEFAULT.IPAccessEnabled] == "1";
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Indique si la restriction d'adresses IP est activée sur la base (CONFIG.IPAccessEnabled = 1)
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static bool SetIPAccessEnabled(ePref pref, bool enabled)
        {
            try
            {
                return pref.SetConfigDefault(new List<SetParam<String>>() { new SetParam<String>(eLibConst.CONFIG_DEFAULT.IPAccessEnabled.ToString(), enabled ? "1" : "0") });
            }
            catch
            {
                return false;
            }
        }
    }


    /// <summary>
    /// Classe représentant une adresse IP stockée dans une base Eudonet
    /// TODO: à déplacer dans un fichier de classe dédié ?
    /// </summary>
    public class eAdminAccessSecurityIPData
    {
        decimal ipId;  // IPId est un decimal (numeric(18,0)) dans [IPADDR]...
        string label;
        string ipAddress;
        string mask;
        string user;
        decimal level;
        decimal permissionId;
        PermissionMode permissionMode;

        public eAdminAccessSecurityIPData(decimal ipId, string label, string ipAddress, string mask, string user, decimal level, decimal permissionId, PermissionMode permissionMode)
        {
            this.IpId = ipId;
            this.label = label;
            this.ipAddress = ipAddress;
            this.mask = mask;
            this.user = user;
            this.level = level;
            this.PermissionId = permissionId;
            this.permissionMode = permissionMode;
        }

        public decimal IpId
        {
            get
            {
                return ipId;
            }

            set
            {
                ipId = value;
            }
        }

        public string Label
        {
            get
            {
                return label;
            }

            set
            {
                label = value;
            }
        }

        public string IpAddress
        {
            get
            {
                return ipAddress;
            }

            set
            {
                ipAddress = value;
            }
        }

        public string Mask
        {
            get
            {
                return mask;
            }

            set
            {
                mask = value;
            }
        }

        public string User
        {
            get
            {
                return user;
            }

            set
            {
                user = value;
            }
        }

        public decimal Level
        {
            get
            {
                return level;
            }

            set
            {
                level = value;
            }
        }


        public decimal PermissionId
        {
            get
            {
                return permissionId;
            }

            set
            {
                permissionId = value;
            }
        }

        public PermissionMode PermissionMode
        {
            get
            {
                return permissionMode;
            }

            set
            {
                permissionMode = value;
            }
        }

        /// <summary>
        /// Renvoie la liste des utilisateurs et groupes concernés par l'IP, calculée en fonction des propriétés de la permission insérée dans IPADDR
        /// </summary>
        /// <param name="pref">Objet Pref</param>
        /// <returns>Libellé affichable des utilisateurs et groupes concernés, séparés par ;</returns>
        public string GetUserLabel(ePref pref)
        {
            string strUserLabel = String.Empty;
            string strUserName = String.Empty;

            if (this.User.Length > 0)
            {
                IDictionary<String, String> dicoUser = eLibDataTools.GetUserLogin(pref, this.User);
                strUserName = String.Join(";", dicoUser.Values);
            }

            if (ShowUser())
                strUserLabel = strUserName;
            else {
                if (strUserName.Trim().Length == 0)
                    strUserLabel = eResApp.GetRes(pref, 513); // Aucun
                else {
                    if (ShowLevel())
                        strUserLabel = eResApp.GetRes(pref, 347); // Tous
                    else
                        strUserLabel = String.Empty;
                }
            }

            return strUserLabel;
        }

        /// <summary>
        /// Renvoie le libellé correspondant au niveau appliqué à l'IP
        /// </summary>
        /// <param name="pref">Objet Pref</param>
        /// <returns>Libellé correspondant au niveau passé en paramètre</returns>
        public string GetLevelLabel(ePref pref)
        {
            if (ShowLevel())
                return eLibTools.GetUserLevelLabel(pref, (UserLevel)this.level);
            else
                return String.Empty;
        }

        /// <summary>
        /// Indique si, en fonction de la permission paramétrée dans [IPADDR] pour cette IP, le niveau de l'utilisateur doit être affiché à l'écran
        /// </summary>
        /// <returns>true ou false selon si la donnée doit être affichée ou non</returns>
        public bool ShowLevel()
        {
            switch (permissionMode)
            {
                default:
                case PermissionMode.MODE_NONE:
                case PermissionMode.MODE_USER_ONLY:
                    return false;

                case PermissionMode.MODE_LEVEL_ONLY:
                case PermissionMode.MODE_USER_OR_LEVEL:
                case PermissionMode.MODE_USER_AND_LEVEL:
                    return true;
            }
        }

        /// <summary>
        /// Indique si, en fonction de la permission paramétrée dans [IPADDR] pour cette IP, la liste des utilisateurs et groupes concernés doit être affichée à l'écran
        /// </summary>
        /// <returns>true ou false selon si la donnée doit être affichée ou non</returns>
        public bool ShowUser()
        {
            switch (permissionMode)
            {
                default:
                case PermissionMode.MODE_NONE:
                case PermissionMode.MODE_LEVEL_ONLY:
                    return false;

                case PermissionMode.MODE_USER_ONLY:
                case PermissionMode.MODE_USER_OR_LEVEL:
                case PermissionMode.MODE_USER_AND_LEVEL:
                    return true;
            }
        }
    }
}