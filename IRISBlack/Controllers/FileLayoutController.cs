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
    [RoutePrefix("api/FileLayout")]
    public class FileLayoutController : BaseController
    {

        // GET: api/Default/5
        /// <summary>
        /// Permet de récupérer le JSON de la structure de la page tel que rentré par le client.
        /// Permet également de récupérer les éléments de la page, qui vont servir à peupler la page.
        /// Pour garder la compatibilité avec le front, dans le cas où on n'a qu'un élément, on retourne
        /// un objet simple et non un tableau.
        /// </summary>
        /// <param name="flpm"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{flpm}")]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetFileLayout([FromUri] FileLayoutParamsModel flpm)
        {

            if(!ModelState.IsValid)
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));


            IEnumerable<FileLayoutModel> ModPage;

            try
            {
                ePref pref = _pref;
                ModPage = await System.Threading.Tasks.Task.Run(() => flpm.nTab.Select(nTab => FileLayoutFactory.InitFileLayoutFactory(new eAdminTableInfos(pref, nTab), pref).GetJSONFileLayout()));

                if(flpm.nTab.Length < 2)
                    return Ok(JsonConvert.SerializeObject(ModPage.FirstOrDefault()));

                return Ok(JsonConvert.SerializeObject(ModPage));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }



        }

        [eAuthorization(UserLevel.LEV_USR_ADMIN)]
        ///<summary>
        ///On n'enregistre que si les éléments globaux changent.
        ///<paramref name="flm"/>
        ///</summary>
        public IHttpActionResult Post(FileLayoutModel flm)
        {

            if (!ModelState.IsValid)
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));

            try
            {
                ePref pref = _pref;
                var flFactory = FileLayoutFactory.InitFileLayoutFactory(new eAdminTableInfos(pref, flm.Tab), pref);
                var oldFlm = flFactory.GetJSONFileLayout();
                var res = flFactory.SetJSONFileLayout(oldFlm, flm);

                if (res.Success)
                    return Ok(JsonConvert.SerializeObject(res.ReturnObject));

                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotModified, JsonConvert.SerializeObject(res)));
            }
            catch (Exception ex) { 

                return InternalServerError(ex);
            }
        }

        // PUT: api/Default/5
        public IHttpActionResult Put(int id, object value)
        {
            return InternalServerError(new NotImplementedException());
        }

        // DELETE: api/Default/5
        public override IHttpActionResult Delete(int id)
        {
            return InternalServerError(new NotImplementedException());
        }
    }
}
