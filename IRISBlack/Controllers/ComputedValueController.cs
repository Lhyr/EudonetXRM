using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Factories;
using Com.Eudonet.Xrm.IRISBlack.Model;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Com.Eudonet.Xrm.IRISBlack.Controllers
{
    /// <summary>
    /// Contrôleur de calcul de somme de colonnes
    /// </summary>
    public class ComputedValueController : BaseController
    {
        /// <summary>
        /// Méthode interne pour le calcul des valeurs depuis les points d'entrée Post/Get
        /// </summary>
        /// <param name="r">Informations permettant de lancer le processus</param>
        /// <returns>Retour HTTP attendu par GET, POST...</returns>
        private IHttpActionResult GetValue(ComputedValueRequestModel r)
        {
            List<ComputedValueModel> computedValues = new List<ComputedValueModel>();

            try
            {
                computedValues = ComputedValueFactory.GetValues(_pref, r);
            }
            catch (EudoException e)
            {
                return InternalServerError(e);
            }
            catch (Exception e)
            {
                //TODO préciser l'erreur
                return InternalServerError(e);
            }
            finally
            {

            }

            return Ok(JsonConvert.SerializeObject(computedValues));
        }

        /// <summary>
        /// GET api/[controller] - Prend les paramètres en entrée pour les convertir en ComputedValueRequestModel, tel que l'attend le point d'entrée POST
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{ListCol:string=''}/{Tab:int=0}/{ParentTab:int=0}/{ParentFileId:int=0}/{ParentPMFileId:int=0}")]
        public IHttpActionResult Get(string ListCol, int Tab, int ParentTab, int ParentFileId, int ParentPMFileId)
        {
            ComputedValueRequestModel r = new ComputedValueRequestModel();
            r.Tab = Tab;
            r.ParentTab = ParentTab;
            r.ParentFileId = ParentFileId;
            r.ParentPMFileId = ParentPMFileId;
            HashSet<int> lCol = new HashSet<int>();
            foreach (string col in ListCol.Replace("%3B", ";").Split(';'))
            {
                int nDescId = 0;
                if (int.TryParse(col, out nDescId))
                    lCol.Add(nDescId);
            }
            r.ListCol = lCol.ToArray();

            return GetValue(r);
        }

        /// <summary>
        /// Récupère les informations
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST", "PUT")]
        [HttpPost]
        public IHttpActionResult Post(ComputedValueRequestModel r)
        {
            return GetValue(r);
        }

        /// <summary>
        /// PUT api/[controller]/5
        /// </summary>
        public IHttpActionResult Put(int id, object value)
        {
            return InternalServerError(new NotImplementedException());
        }

        /// <summary>
        /// DELETE api/[controller]/5
        /// </summary>
        public override IHttpActionResult Delete(int id)
        {
            return InternalServerError(new NotImplementedException());
        }
    }
}
