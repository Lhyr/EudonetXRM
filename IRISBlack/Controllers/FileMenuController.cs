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
using Newtonsoft.Json.Serialization;

namespace Com.Eudonet.Xrm.IRISBlack.Controllers
{
    /// <summary>
    /// Contrôleur permettant de gérer les entrées affichées sur le menu du nouveau mode Fiche
    /// </summary>
    public class FileMenuController : BaseController
    {
        #region Private 
        /// <summary>
        /// Permet au POST et au GET d'avoir les informations du menu
        /// </summary>
        /// <param name="nDescId">DescID de l'onglet</param>
        /// <param name="nFileId">ID de la fiche</param>
        /// <returns></returns>
        private IHttpActionResult GetResultModel(int nDescId, int nFileId)
        {
            if (!ModelState.IsValid)
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));

            try
            {
                // Le JavaScript attend les propriétés avec un nom en lower/camelCase. On les convertit donc ainsi en sortie
                DefaultContractResolver contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };
                JsonSerializerSettings serializeOptions = new JsonSerializerSettings
                {
                    ContractResolver = contractResolver,
                    Formatting = Formatting.Indented
                };
                // Puis on retourne l'objet ainsi formaté
                return Ok(JsonConvert.SerializeObject(FileMenuFactory.InitFileMenuFactory(_pref, nDescId, nFileId).GetFileMenuModel(), serializeOptions));
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
        #endregion

        /// <summary>
        /// Renvoie les données à afficher dans le menu
        /// </summary>
        /// <param name="DescId">DescID de l'onglet</param>
        /// <param name="FileId">ID de la fiche</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{DescId:int}/{FileId:int}")]        
        public IHttpActionResult Get(int DescId, int FileId)
        {
            return GetResultModel(DescId, FileId);
        }

        /// <summary>
        /// Renvoie les données à afficher dans le menu
        /// </summary>
        /// <param name="DescId">DescID de l'onglet</param>
        /// <param name="FileId">ID de la fiche</param>
        /// <returns></returns>
        [HttpPost]
        [AcceptVerbs("POST", "PUT")]
        public IHttpActionResult Post(int DescId, int FileId)
        {
            return GetResultModel(DescId, FileId);
        }

        /// PUT api/<controller>
        public IHttpActionResult Put()
        {
            return InternalServerError(new NotImplementedException());
        }

        /// DELETE api/<controller>
        public override IHttpActionResult Delete(int id)
        {
            return InternalServerError(new NotImplementedException());
        }
    }
}
