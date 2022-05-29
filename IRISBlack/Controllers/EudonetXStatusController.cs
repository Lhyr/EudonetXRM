using System;
using System.Web.Http;
using Com.Eudonet.Xrm.IRISBlack.Model;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Core.Model;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using Com.Eudonet.Xrm.IRISBlack.Factories;
using Com.Eudonet.Xrm.IRISBlack.Attributs;
using System.Web;
using EudoQuery;

namespace Com.Eudonet.Xrm.IRISBlack.Controllers
{
    /// <summary>
    /// Controleur permettant de récupérer la structure d'une page d'un onglet.
    /// </summary>
    [RoutePrefix("api/EudonetXStatus")]
    public class EudonetXStatusController : BaseController
    {

        // GET: api/EudonetXStatus
        /// <summary>
        /// Permet de lire le statut d'activation des fonctionnalités Eudonet X d'un onglet
        /// </summary>
        /// <param name="Tab">TabID (DescID) de l'onglet pour lequel on souhaite récupérer les informations</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{exsm}")]
        public async System.Threading.Tasks.Task<IHttpActionResult> Get(int Tab)
        {

            if(!ModelState.IsValid)
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));

            try
            {
                ePref pref = _pref;
                EudonetXStatusModel exsm = await System.Threading.Tasks.Task.Run(() => EudonetXStatusFactory.InitEudonetXStatusFactory(new eAdminTableInfos(pref, Tab), pref).GetEudonetXStatus());

                return Ok(JsonConvert.SerializeObject(exsm));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }



        }

        [eAuthorization(UserLevel.LEV_USR_ADMIN)]
        // POST: api/EudonetXStatus
        /// <summary>
        /// Permet de lire le statut d'activation des fonctionnalités Eudonet X d'un onglet
        /// </summary>
        /// <param name="exsm">Objet contenant les infos à mettre à jour</param>
        /// <returns>Le résultat de la mise à jour</returns>
        [HttpPost]
        [Route("{exsm}")]
        public IHttpActionResult Post(EudonetXStatusModel exsm)
        {

            if (!ModelState.IsValid)
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));

            try
            {
                // On récupère d'abord les valeurs actuelles en base
                ePref pref = _pref;
                EudonetXStatusFactory exsf = EudonetXStatusFactory.InitEudonetXStatusFactory(new eAdminTableInfos(pref, exsm.Tab), pref);
                EudonetXStatusModel currentEXSM = exsf.GetEudonetXStatus();

                // Puis on met à jour avec celles passées en paramètres (uniquement pour celles qui ne sont pas en UNDEFINEd)
                eAdminResult res = exsf.SetEudonetXStatus(currentEXSM, exsm);

                if (res.Success)
                    return Ok(JsonConvert.SerializeObject(res.ReturnObject));

                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotModified, JsonConvert.SerializeObject(res)));
            }
            catch (Exception ex) { 

                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Mise à jour d'une information via PUT : NON IMPLEMENTE, UTILISER POST
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IHttpActionResult Put(int id, object value)
        {
            return InternalServerError(new NotImplementedException());
        }

        /// <summary>
        /// Suppression d'une information via DELETE : NON IMPLEMENTE, UTILISER POST
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override IHttpActionResult Delete(int id)
        {
            return InternalServerError(new NotImplementedException());
        }
    }
}
