using Com.Eudonet.Core.Model;
using Com.Eudonet.Core.Model.Teams;
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
using System.Web.Http;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.IRISBlack.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class DetailController : BaseController
    {
        private int Tab { get; set; }
        private int FileId { get; set; }


        /// <summary>
        /// Méthode pour obtenir les données, avec seulement l'onglet et la fiche
        /// </summary>
        /// <returns></returns>
        private async System.Threading.Tasks.Task<FileDetailModel> GetFileDetailModel()
        {
            ePref pref = _pref;
            eFile objFile = await System.Threading.Tasks.Task.Run(() => eFileLite.CreateFileLite(pref, Tab, FileId));

            return await DetailFactory.InitDetailFactory(objFile, _pref, Tab).CreateFileDetailModelFromFile();
        }

        /// <summary>
        /// Methode pour obtenir les données, avec en plus les paramètres nécessaires pour le contexte (fiche parent....)
        /// </summary>
        /// <param name="oCtxInfos"></param>
        /// <returns></returns>
        private async System.Threading.Tasks.Task<FileDetailModel> GetFileDetailModel(FileDetailContextInfosModel oCtxInfos)
        {

            ePref pref = _pref;

            ExtendedDictionary<String, Object> param = new ExtendedDictionary<String, Object>();

            if (oCtxInfos != null && oCtxInfos?.values?.Count > 0 && FileId == 0)
            {
                eFileTools.eFileContext ef = null;
                ExtendedDictionary<Int32, String> dicvalues = new ExtendedDictionary<Int32, String>();
                // Paramètres supplémentaires transmis au constructeur de rendu

                //Création de l'objet contexte (liaisons systèmes)
                int iPrtPPId = 0, iPrtPMId = 0, iPrtADRId = 0, iPrtEVTId = 0;

                var dicCtxInfos = oCtxInfos.values
                    ?.Where(v => v.spclnk <= 0)
                    ?.Select(v => new { v.descid, v.value })
                    ?.ToDictionary(key => key.descid, val => val.value);


                if (dicCtxInfos != null)
                {
                    if (dicCtxInfos.ContainsKey((int)TableType.PP))
                    {
                        int.TryParse(dicCtxInfos[(int)TableType.PP], out iPrtPPId);
                        dicCtxInfos.Remove((int)TableType.PP);
                    }

                    if (dicCtxInfos.ContainsKey((int)TableType.PM))
                    {
                        int.TryParse(dicCtxInfos[(int)TableType.PM], out iPrtPMId);
                        dicCtxInfos.Remove((int)TableType.PM);
                    }

                    if (dicCtxInfos.ContainsKey((int)TableType.ADR))
                    {
                        int.TryParse(dicCtxInfos[(int)TableType.ADR], out iPrtADRId);
                        dicCtxInfos.Remove((int)TableType.ADR);
                    }

                    if (dicCtxInfos.Count > 0)
                    {
                        int.TryParse(dicCtxInfos.FirstOrDefault(kvp => kvp.Key % 100 == 0).Value, out iPrtEVTId);
                    }
                }

                eFileTools.eParentFileId efPrtId = new eFileTools.eParentFileId(
                    parentPpId: iPrtPPId,
                    parentPmId: iPrtPMId,
                    parentAdrid: iPrtADRId,
                    parentEvtId: iPrtEVTId);


                ef = new eFileTools.eFileContext(efPrtId, _pref.User, Tab, 0);
                param.Add("filecontext", ef);
                //relation custom

                //Si la valeur du champ principale doit déjà être affectée
                FileDetailContextInfosModel.ValueModel value = oCtxInfos.values.Find(v => v.spclnk > 0);
                if (value != null)
                {
                    dicvalues.AddContainsKey(value.spclnk, value.value);
                }


                //valeurs préremplies (mainfield 01, et les rubriques associées ça se passera ici également)
                foreach (var kvp in dicCtxInfos.Where(kvp => kvp.Key % 100 > 0))
                {
                    dicvalues.Add(kvp.Key, kvp.Value);
                }

                //Dictionnaires de valeurs de la fiche
                if (dicvalues.Count > 0)
                    param.Add("dicvalues", dicvalues);
            }

            eFile objFile = await System.Threading.Tasks.Task.Run(() => eFileMain.CreateEditMainFile(pref, Tab, FileId, param));

            return await DetailFactory.InitDetailFactory(objFile, _pref, Tab).CreateFileDetailModelFromFile();
        }



        /// <summary>
        /// retourrne la liste des éléments pour la structure d'un détail.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{nTab:int=0}/{nFileId:int}")]
        public async System.Threading.Tasks.Task<IHttpActionResult> Get(int nTab, int nFileId)
        {
            Tab = nTab;
            FileId = nFileId;

            var greeterClientOptions = Grpc.Client.GrpcClientOptions
                .initGrpcClientOptions()
                .AddHandlerDangerousCertificat()
                .AddGrpcWebHandler()
                .AddHandlerVersionHTTP(1, 1)
                .AddWebHandler();

            var greeterClient = Grpc.Client.GreeterClient.InitGreeterClient("https://localhost:7159", greeterClientOptions.GetGrpcChannelOptions());
            var greetings = await greeterClient?.GetServerResponse();
            System.Diagnostics.Trace.WriteLine(greetings?.Message);

            try
            {
                FileDetailModel fdm = await GetFileDetailModel();
                return Ok(JsonConvert.SerializeObject(fdm));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

        }

        /// <summary>
        /// retourne une liste.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{nTab:int}")]
        public async System.Threading.Tasks.Task<IHttpActionResult> Get(int nTab)
        {
            Tab = nTab;

            try
            {

                //TODO voir si on peut annuler la pagination et lancer tous les enregistrements
                /** TODO : Gros goulet d'étranglement. 30s en debug. A voir s'il est impératif d'implémenter cet objet ou une version light.
                 * L'autre point est qu'on récupère le libelle de la table et le descid, qui seront nécessaires ultérieurement, mais qui actuellement
                 * ne sont pas transmis au front. A voir si on met ca dans un model qui encapsule le modele des structures. GLA.*/
                ePref pref = _pref;
                eListMain objList = await System.Threading.Tasks.Task.Run(() => eListFactory.CreateMainList(pref, Tab, nPage: 1, nRows: 50, bDoCount: false, bFullList: true)) as eListMain;


                PagingInfoModel pgim = PagingInfoModelFactory.InitPagingInfoModelFactory(objList).GetPagingInfoModel();


                //TODO: message adapté
                if (objList == null)
                    return InternalServerError(new EudoException("Une Erreur non identifiée s'est produite durant la génération du jeu de données. Merci de contacter notre support."));

                if (!String.IsNullOrEmpty(objList.ErrorMsg) || objList.ErrorType != QueryErrorType.ERROR_NONE || objList.InnerException != null)
                    return InternalServerError(new EudoException(objList.ErrorMsg, sUserMessage: "Une erreur s'est produite lors de la génération du signet", innerExcp: objList.InnerException));


                ListDetailModel.StructureModel sm = await StructureModelForListFactory.InitListDetailModelFactory(objList, _pref)
                    .GetListStructureModel();

                ListDetailModel ListDetail = ListDetailModelFactory.InitListDetailModelFactory(_pref, sm, objList, pgim)
                    .GetListDetailModel();

                /** Pour le mode liste, on ne renvoie pas les valeurs qui correspondent aux valeurs par défaut. */
                return Ok(JsonConvert.SerializeObject(ListDetail, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }));

            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

        }

        /// <summary>
        /// Retourne les données avec un contexte (fiche parent...)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async System.Threading.Tasks.Task<IHttpActionResult> Post(FileDetailContextInfosModel oCtxInfos)
        {
            try
            {
                Tab = oCtxInfos.tab;
                FileId = oCtxInfos.fileid;


                FileDetailModel fdm = await GetFileDetailModel(oCtxInfos);
                return Ok(JsonConvert.SerializeObject(fdm));
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
        [HttpPut]
        [Route("")]
        public IHttpActionResult Put(UpdateFieldsModel value)
        {
            return Ok();
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
