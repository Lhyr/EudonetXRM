using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Xrm.IRISBlack.Model;
using Com.Eudonet.Xrm.IRISBlack.Model.TypedFields;
using Com.Eudonet.Xrm.IRISBlack.Factories;
using EudoQuery;
using Com.Eudonet.Internal;
using Com.Eudonet.Engine.Result;
using EudoExtendedClasses;
using System.Net.Http;
using System.Net;

namespace Com.Eudonet.Xrm.IRISBlack.Controllers
{
    /// <summary>
    /// Contrôleur permettant la mise à jour ou la création de fiches/champs
    /// </summary>
    public class CreateUpdateController : BaseController
    {

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult Get(int nTab, int nFileId)
        {
            return Ok();
        }

        /// <summary>
        /// Met à jour une information
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST", "PUT")]
        public IHttpActionResult SaveField(UpdateFieldsModel value)
        {
            if (!ModelState.IsValid)
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));

            if (value?.Fields == null || value?.Fields?.Count() < 1)
                return StatusCode(HttpStatusCode.BadRequest);

            try
            {
                CreateUpdateFactory factory = new CreateUpdateFactory(_pref);

                EngineResult result = factory.CreateUpdateFields(value.FileId, value);

                IEngineReturnModel resultModel = EngineReturnFactory.initEngineReturnFactory(result).GetEngineResult();

                CreateReturnModel crm = resultModel as CreateReturnModel;
                string sMajMRU = string.Empty;

                if (crm != null)
                    foreach (int nFileId in crm.FilesId)
                        param.SetTableMru(_pref, crm.MainTab, nFileId, out sMajMRU);

                return Ok(JsonConvert.SerializeObject(resultModel));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route("")]
        public override IHttpActionResult Delete(int id)
        {
            return Ok();
        }
    }
}
