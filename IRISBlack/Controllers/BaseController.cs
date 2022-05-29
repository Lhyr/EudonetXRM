using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Com.Eudonet.Xrm.IRISBlack.Controllers
{
    /// <summary>
    /// Controller qui sert de base aux autres controleurs
    /// avec notamment toutes les méthodes communes.
    /// </summary>
    public abstract class BaseController : ApiController
    {
        #region variables
        private eParam _param;
        #endregion
        #region Accesseurs
        /// <summary>
        /// Classe de chargement des param
        /// </summary>
        protected eParam param {
            get {
                if (_param == null)
                    _param = new eParam(_pref);

                return _param;
            }
        }
        /// <summary>
        /// retourne les préférences utilisateurs.
        /// Et un paquet d'autres choses.
        /// </summary>
        protected ePref _pref
        {
            get
            {
#if DEBUG
                /** Pour le debug, on permet une connexion bidon à une base de données fixe avec un utilisateur fixe.
                 * On peut ainsi utiliser des appels VueJs sans se soucier de l'interface de connexion Webform. 
                 */
                if(HttpContext.Current?.Session["Pref"] == null)
                {
                    ePrefSQL prefSqlClient = ePrefTools.GetDefaultPrefSql("EUDO_GRC_LIGHT_PF");
                    eudoDAL dalClient = eLibTools.GetEudoDAL(prefSqlClient);


                    RqParam rqParam = new RqParam("SELECT [userid], [Lang] FROM [user] WHERE [userlogin] LIKE @UserName");
                    rqParam.AddInputParameter("@UserName", SqlDbType.VarChar, "Administrateur");
                    int nUserId = -1;
                    string sLang = "Lang_00";

                    dalClient.OpenDatabase();

                    using (DataTableReaderTuned dtr = dalClient.Execute(rqParam))
                    {
                        if (dtr == null || !dtr.Read())
                            throw new Exception("L'utilisateur Administrateur n'existe pas dans la base de données. ");

                        nUserId = dtr.GetEudoNumeric(0);
                        sLang = dtr.GetString(1);
                    }

                    ePref pref = new ePref(prefSqlClient.GetSqlInstance, prefSqlClient.GetBaseName, prefSqlClient.GetSqlUser, prefSqlClient.GetSqlPassword, nUserId, sLang);
                    pref.DatabaseUid = prefSqlClient.DatabaseUid;

                    if (!pref.LoadConfig())
                        throw new Exception("Erreur de chargement de la configuration");

                    HttpContext.Current.Session.Add("Pref", pref);
                }
                    
#endif

                return HttpContext.Current.Session["Pref"] as ePref;
            }
        }
        #endregion

        #region methodes controleur abstraites

        /// <summary>
        /// liste des éléments.
        /// </summary>
        /// <returns></returns>
        //public abstract IHttpActionResult Get();

        /// <summary>
        /// récupère un élément.
        /// </summary>
        /// <param name="nTab"></param>
        /// <param name="nFileId"></param>
        /// <returns></returns>
        //public abstract IHttpActionResult Get(int nTab, int nFileId);

        /// <summary>
        /// insertion d'un élément.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        //public abstract IHttpActionResult Post([FromBody]string value);

        /// <summary>
        /// mise à jour d'un élément.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        //public abstract IHttpActionResult Put(int id, [FromBody]string value);

        /// <summary>
        /// Suppression d'un élément.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract IHttpActionResult Delete(int id);

        #endregion
    }
}
