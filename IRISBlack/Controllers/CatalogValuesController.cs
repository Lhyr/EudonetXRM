using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Factories;
using Com.Eudonet.Xrm.IRISBlack.Model;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Com.Eudonet.Xrm.IRISBlack.Controllers
{
    public class CatalogValuesController : BaseController
    {
        /// <summary>
        /// renvoie les valeurs d'un catalogue
        /// </summary>
        /// <param name="descid"></param>
        /// <param name="popupType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{descid:int}/{popupType:int=3}/")]
        public async System.Threading.Tasks.Task<IHttpActionResult> Get(int descid, int popupType = 3)
        {
            CatalogValuesModel catModel;
            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            try
            {
                ePref pref = _pref;
                catModel = await System.Threading.Tasks.Task.Run(() => {
                    dal.OpenDatabase();
                    //TODO : ne pas mettre showHidden en dur ?
                    eCatalog catObj = new eCatalog(dal, pref, (PopupType)popupType, pref.User, descid, showHiddenValues: true);
                    return CatalogValuesFactory.GetModel(catObj);
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
            finally
            {
                dal.CloseDatabase();
            }
            return Ok(JsonConvert.SerializeObject(catModel));

        }


        /// <summary>
        /// Met à jour une information
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST", "PUT")]
        public IHttpActionResult GetCatalogValues(CatalogValuesRequestModel request)
        {
            if (!ModelState.IsValid)
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));


            CatalogValuesModel catModel;
            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            try
            {
                dal.OpenDatabase();
                //TODO : ne pas mettre showHidden en dur ?
                eCatalog catObj = new eCatalog(dal, _pref, (PopupType)request.PopupType, _pref.User, request.DescId
                    , showHiddenValues: request.ShowHiddenValue
                    , treeView: request.Treeview
                    , parentId: request.ParentId
                    , searchpattern: request.SearchPattern);


                catModel = CatalogValuesFactory.GetModel(catObj);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
            finally
            {
                dal.CloseDatabase();
            }
            return Ok(JsonConvert.SerializeObject(catModel));

        }

        // PUT api/<controller>/5
        public IHttpActionResult Put(int id, object value)
        {
            return InternalServerError(new NotImplementedException());
        }

        // DELETE api/<controller>/5
        public override IHttpActionResult Delete(int id)
        {
            return InternalServerError(new NotImplementedException());
        }
    }
}