using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Core.Model;
using EudoQuery;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de gestion des PJ externalisées
    /// </summary>
    public class eExternalPJ
    {
        /// <summary>
        /// Objet d'accès aux données
        /// </summary>
        private eudoDAL _dal = null;

        /// <summary>
        /// 
        /// </summary>
        public eExternalPJ()
        {
            
        }

        ~eExternalPJ()
        {
            CloseDatabase();
        }

        /// <summary>
        /// Récupère et ouvre une connexion à la base EUDOTRAIT
        /// </summary>
        public void OpenDatabase()
        {
            String sCurrentSQLInstance = ePrefTools.GetAppDefaultInstance();
            _dal = ePrefTools.GetDefaultEudoDal("EUDOTRAIT", sCurrentSQLInstance);

            if (_dal != null)
                _dal.OpenDatabase();
        }

        /// <summary>
        /// 
        /// </summary>
        public void CloseDatabase()
        {
            if (_dal != null)
                _dal.CloseDatabase();
        }

        /// <summary>
        /// Récupère ID et FileName de la PJ Externalisée
        /// </summary>
        /// <param name="sUid">Uid de la base</param>
        /// <param name="nPjId">PjId de la PJ Externalisée (correspond au PjId sur le serveur intranet)</param>
        /// <param name="nId">Valeur de retour: id de l'enregistrement dans EXTERNAL_PJ</param>
        /// <param name="sFilename">Valeur de retour: nom du fichier physique de la pj</param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public bool GetPJFileName(string sUid, Int32 nPjId, out Int32 nId, out string sFilename, out string sError)
        {
            nId = 0;
            sFilename = String.Empty;

            string strSQL = "SELECT TOP 1 [Id], [FileName] FROM [EUDOTRAIT].[dbo].[EXTERNAL_PJ] WHERE [uid] = @uid AND [PjId] = @pjid";

            RqParam rqSelectPJ = new RqParam(strSQL);
            rqSelectPJ.AddInputParameter("@uid", System.Data.SqlDbType.VarChar, sUid);
            rqSelectPJ.AddInputParameter("@pjid", System.Data.SqlDbType.Int, nPjId);

            DataTableReaderTuned dtrSelectPJ = _dal.Execute(rqSelectPJ, out sError);
            try
            {
                if (sError.Length != 0 || dtrSelectPJ == null || !dtrSelectPJ.Read())
                {
                    sError = "PJ Invalide.";
                    //throw new Exception(sError);
                    return false;
                }

                nId = dtrSelectPJ.GetInt32UnSafe(0);
                sFilename = dtrSelectPJ.GetString(1);
            }
            finally
            {
                if (dtrSelectPJ != null)
                    dtrSelectPJ.Dispose();
            }

            return true;
        }

        /// <summary>
        /// Incrémente le compteur du nombre de vue et met à jour la date d'accès à la pj
        /// </summary>
        /// <param name="nId">id de l'enregistrement dans EXTERNAL_PJ</param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public bool UpdateAccessDateAndCounter(Int32 nId, out string sError)
        {
            string strSQL = "UPDATE [EUDOTRAIT].[dbo].[EXTERNAL_PJ] SET [LastAccessDate] = getdate(), [AccessCounter] = isnull([AccessCounter], 0) + 1 WHERE [Id] = @id";

            RqParam rqUpdatePJ = new RqParam(strSQL);
            rqUpdatePJ.AddInputParameter("@id", System.Data.SqlDbType.Int, nId);

            _dal.ExecuteNonQuery(rqUpdatePJ, out sError);
            if (sError.Length != 0)
            {
                sError = "Erreur mise à jour compteur.";
                //throw new Exception(sError);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Récupère répertoire racine des PJ externalisées sur le serveur
        /// </summary>
        /// <param name="sDatasPath">Valeur de retour: répertoire racine</param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public bool GetRootDatasPath(out string sDatasPath, out string sError)
        {
            sDatasPath = String.Empty;

            string strSQL = "SELECT TOP 1 [Value] FROM [EUDOTRAIT].[dbo].[SERVERCONFIG] WHERE [Parameter] = @parameter";

            RqParam rqSelectDatasPath = new RqParam(strSQL);
            rqSelectDatasPath.AddInputParameter("@parameter", System.Data.SqlDbType.VarChar, "EXTERNALPJ_DATASPATH");

            DataTableReaderTuned dtrSelectDatasPath = _dal.Execute(rqSelectDatasPath, out sError);
            try
            {
                if (sError.Length != 0 || dtrSelectDatasPath == null || !dtrSelectDatasPath.Read())
                {
                    sError = "DatasPath non configuré sur ce serveur.";
                    //throw new Exception(sError);
                    return false;
                }

                sDatasPath = dtrSelectDatasPath.GetString(0);
            }
            finally
            {
                if (dtrSelectDatasPath != null)
                    dtrSelectDatasPath.Dispose();
            }

            return true;
        }
    }
}