using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Factories;
using Com.Eudonet.Xrm.IRISBlack.Model;
using Com.Eudonet.Xrm.IRISBlack.Model.TypedFields;
using EudoExtendedClasses;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static Com.Eudonet.Core.Model.eParam;

namespace Com.Eudonet.Xrm.IRISBlack.Controllers
{
    /// <summary>
    /// Gestion des MRU
    /// </summary>
    public class MruController : BaseController
    {
        #region private 
        /// <summary>
        /// Permet au post et au get d'avoir les informations des MRU depuis des valeurs déterminées.
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        private IHttpActionResult GetResultModel(MRUGetSearchResultModel search)
        {

            if (!ModelState.IsValid)
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));

            try
            {
                IEnumerable<UserValueField> ieUVF = null;
                if (!string.IsNullOrEmpty(search.lstUserVal))
                {
                    UserValueFieldModel[] uvf = JsonConvert.DeserializeObject<UserValueFieldModel[]>(search.lstUserVal);

                    ieUVF = uvf?.Select(uf => new UserValueField
                    {
                        IsFound = uf.IsFound,
                        Label = uf.DisplayDesc,
                        Parameter = uf.NumDesc.ToString(),
                        Value = uf.ValDesc
                    });
                }

                // US #3798 - TK #6239 - Saisie guidée - Filtre Relations selon le contexte, à la création d'une fiche
                // Récupération des IDs des fiches à afficher en MRU si aucune recherche n'est effectuée
                List<int> mruIds = new List<int>();

                if (search.lstDispVal != null)
                    mruIds = new List<int>(eLibTools.GetMruIdsFromParam(search.lstDispVal));

                return Ok(JsonConvert.SerializeObject(MruFactory.InitMruFactoryDescId(_pref, search.nDesc).GetMRUFromFile(search.TargetTab, search.sSearch,
                    ieUVF,
                    mruIds,
                    search.lstDescDeduplicate,
                    search.bSearchAllUserDefined,
                    search.nTabFrom, search.nFileId)));
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
        #endregion

        /// <summary>
        /// renvoie les valeurs des MRU
        /// </summary>
        /// <param name="descid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{descid:int}")]
        public IHttpActionResult Get(int descid)
        {
            try
            {
                return Ok(JsonConvert.SerializeObject(MruFactory.InitMruFactoryDescId(_pref, descid).GetBaseMRUModel()));
            }
            catch (Exception ex)
            {

                return InternalServerError(ex);
            }

        }

        /// <summary>
        /// renvoie les valeurs d'une recherche depuis les MRU
        /// </summary>
        /// <param name="descid"></param>
        /// <param name="search"></param>
        /// <param name="fieldType">format du champ</param>
        /// <param name="popupType">Type de Catalogue</param>
        /// <param name="parentValue">valeur du champ parent pour les catalogues liés</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{descid:int}/{search:string}/{fieldType:int}/{popupType:int}/{parentValue:string}")]
        public IHttpActionResult Get(int descid, string search, int fieldType, int popupType = 3, string parentValue = "")
        {
            eParam param = new eParam(_pref);
            MRUModel mRUModel = new MRUModel();
            FieldType fieldType1 = (FieldType)fieldType;
            eudoDAL dal = eLibTools.GetEudoDAL(_pref);

            PopupType ePopupType = (PopupType)popupType;

            try
            {
                dal.OpenDatabase();

                switch (fieldType1)
                {
                    case FieldType.Catalog:
                        int iParentId = -1;
                        if (!string.IsNullOrEmpty(parentValue) && !int.TryParse(parentValue, out iParentId))
                            iParentId = eCatalog.GetCatalogValueID(_pref, ePopupType, descid, search, dal, _pref.User);
                        eCatalog.CatalogValue searchvalue = new eCatalog.CatalogValue() { Label = search, ParentId = iParentId };
                        eCatalog catObj = new eCatalog(dal, _pref, ePopupType, _pref.User, descid, treeView: false, searchedValue: searchvalue);
                        CatalogValuesModel catModel = CatalogValuesFactory.GetModel(catObj);
                        mRUModel = MruFactory.InitMruFactoryCatalog(_pref, catModel).GetMRUFromModel();

                        break;
                    case FieldType.User:
                        eUser user = new eUser(dal, descid, _pref.User, eUser.ListMode.USERS_AND_GROUPS, _pref.GroupMode);
                        UserValuesModel userValues = new UserValuesModel();
                        userValues.Values = UserValuesFactory.GetMRUValues(user, search);
                        mRUModel = MruFactory.InitMruFactoryUser(_pref, userValues).GetMRUFromModel();

                        break;
                    default:
                        return InternalServerError(new NotImplementedException());
                        break;
                }

            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
            finally
            {
                dal.CloseDatabase();
            }

            return Ok(JsonConvert.SerializeObject(mRUModel));

        }

        /// <summary>
        /// renvoie les valeurs d'une recherche depuis les MRU
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public IHttpActionResult Get([FromUri]MRUGetSearchResultModel search)
        {
            return GetResultModel(search);
        }


        /// <summary>
        /// renvoie les valeurs d'une recherche depuis les MRU
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult Post(MRUGetSearchResultModel search)
        {
            return GetResultModel(search);
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
