using System;
using System.Text;
using System.Web;
using Com.Eudonet.Internal;
using EudoQuery;
using System.Configuration;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    class LoadEudoLog
    {
        public string Directory { get; private set; }

        /// <summary>
        /// Url du serveur visible depuis Internet
        /// </summary>
        public string AppExternalUrl { get; private set; }

        /*
        /// <summary>
        /// URL du serveur Interne
        /// </summary>
        public string AppInternalUrl { get; private set; }*/

        /// <summary>
        /// Indique si l'url recu par le serveur est différente de celle dans external url
        /// Dans ce cas, il faut rediriger.
        /// </summary>
        public bool RedirecUrl { get; private set; }

        /// <summary>
        /// erreur évenetulles
        /// </summary>
        public string Error { get; private set; }



        /// <summary>
        /// Chargement des informations sur une base depuis Eudolog
        /// </summary>
        /// <param name="request"></param>
        /// <param name="uid"></param>
        public LoadEudoLog(HttpRequest request, string uid)
        {
            Error = string.Empty;

            eudoDAL dalEdnLog = eLibTools.GetEudoDAL(ePrefTools.GetDefaultPrefSql("EUDOLOG"));
            DataTableReaderTuned dtr = null;

            try
            {
                string error = "";

                dalEdnLog.OpenDatabase();
                string sql = new StringBuilder()
                    .Append("SELECT [directory], case when isnull([appexternalurl], '') = '' then isnull([appurl], '') else [appexternalurl] end [AppExtUrl]")
                    .Append("FROM [DATABASES] WHERE [UID] = @uid")
                    .ToString();

                RqParam rqEndLog = new RqParam(sql);
                rqEndLog.AddInputParameter("@uid", System.Data.SqlDbType.VarChar, uid);

                dtr = dalEdnLog.Execute(rqEndLog, out error);

                if (error.Length != 0 || dtr == null || !dtr.Read())
                {
                    Error = (error.Length != 0) ? error : string.Concat("Base non trouvé pour l'UID : ", uid);
                    return;
                }

                Directory = dtr.GetString(0);
                AppExternalUrl = dtr.GetString(1);

                if (AppExternalUrl.Length == 0)
                    return;

                AppExternalUrl = AppExternalUrl.TrimEnd('/') + "/";

                // Chemin en cours
                string pageAppUrl = eLibTools.GetAppUrl(request);
                pageAppUrl = pageAppUrl.TrimEnd('/') + "/";

                // "tentative" de gestion des redirection "interne" par reverseproxy
                //  le pb arrive quand un reverseproxy est utilisé et que celui-ci ré-écrit le host-header :
                //   si l'url externe est xrm.eudonet.com et qu'un reverse-proxy redirige vers "interne.serveur.eudo"
                //  alors il se produit une bouble de redirection :
                //    le client externe appel "xrm.eudonet.com", le reverse-proxy redirige en changeant le host-header vers "interne.serveur.eudo"
                //  cette url est détecté comme différente de "xrm.eudonet.com" et rebelotte.
                // pour eviter cela, on indique de ne pas rediriger si l'url "recu" (pageAppUrl) est l'url de reverseproxy déclaré dans web.config

                string sReverseProxyInternalUrl = AppExternalUrl;


                /* 
                 * - correctif non activé, en attente d'un environement de test "officiel". test ok via ngrok avec réécriture de host-header
                 *    -> a bien vérifié dans les différents use case (envoie de mail avec l'url externe/interne, liens de déinscription dans les 2 cas depuis une
                 *    ip interne/externe, etc...    
                 */
                sReverseProxyInternalUrl = eLibTools.GetServerConfig("reverseproxyinternalurl", AppExternalUrl);
                if (string.IsNullOrEmpty(sReverseProxyInternalUrl))
                    sReverseProxyInternalUrl = AppExternalUrl;

                sReverseProxyInternalUrl = sReverseProxyInternalUrl.TrimEnd('/') + "/";


                if (!AppExternalUrl.ToLower().Equals(pageAppUrl.ToLower()) && !sReverseProxyInternalUrl.ToLower().Equals(pageAppUrl.ToLower()))
                    RedirecUrl = true;
            }
            finally
            {
                dtr?.Dispose();
                dalEdnLog.CloseDatabase();
            }
        }
    }
}