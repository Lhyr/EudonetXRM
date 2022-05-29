using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Factories;
using Com.Eudonet.Xrm.IRISBlack.Model;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using static Com.Eudonet.Internal.eLibConst;
using static Com.Eudonet.Xrm.IRISBlack.Model.FileDetailModel;

namespace Com.Eudonet.Xrm.IRISBlack.Controllers
{
    /// <summary>
    /// Controleur spécifique aux Bookmarks
    /// </summary>
    public class BookmarkController : BaseController
    {
        /// <summary>
        /// Id de la fiche à afficher (post-création)
        /// </summary>
        /// <param name="_bkm"></param>
        private void GetFileToDisplay(eBookmark _bkm)
        {
            //Id de la fiche à afficher (post-création)
            if ((_bkm.GetIdsList()?.Count ?? 0) == 0)
            {
                _bkm.BkmFileId = 0;
                _bkm.BkmFilePos = 0;
            }
            else if (_bkm.BkmFileId > 0)
            {
                _bkm.BkmFilePos = _bkm.GetIdsList().IndexOf(_bkm.BkmFileId);
            }
            else
            {
                if (_bkm.BkmFilePos > 0 && _bkm.GetIdsList().Count > _bkm.BkmFilePos)
                {
                    _bkm.BkmFileId = _bkm.GetIdsList()[_bkm.BkmFilePos];
                }
                else
                {
                    _bkm.BkmFilePos = 0;
                    _bkm.BkmFileId = _bkm.GetIdsList()[0];
                }
            }

            if (_bkm.BkmFilePos > -1)
                _bkm.BkmFilePos++;
        }


        /// <summary>
        /// Renvoie les données nécessaires aux méthodes de ce contrôleur : la liste des bookmarks d'un onglet
        /// </summary>
        /// <param name="nTab">TabID de l'onglet concerné</param>
        /// <param name="nFileId">FileID concerné</param>
        /// <returns></returns>
        protected async Task<IEnumerable<StructBkmModel>> GetData(int nTab, int nFileId)
        {
            try
            {
                ePref pref = _pref;

                // eFile objFile = await System.Threading.Tasks.Task.Run(() => eFileForBkm.CreateFileForBkmBar(pref, nTab, nFileId));
                eFile objFile = await System.Threading.Tasks.Task.Run(() => eFileLiteWithBkmBarInfos.CreateFileLiteWithBkmBarInfos(pref, nTab, nFileId));

                // Si les infos du signet ne sont pas renvoyées et qu'une erreur est survenue, on la renvoie, plutôt que de risquer une NullReferenceException plus bas
                if (!String.IsNullOrEmpty(objFile.ErrorMsg))
                    throw new Exception(String.Concat("Erreur de récupération des données des signets (Tab : ", nTab, ", FileID : ", nFileId, ") - ", objFile.ErrorMsg));

                IDictionary<TREATID, bool> globalRight =
                  eLibDataTools.GetTreatmentGlobalRight(pref, new HashSet<eLibConst.TREATID>
                      {
                                        eLibConst.TREATID.FORMULAR,
                                        eLibConst.TREATID.EMAILING,
                                        eLibConst.TREATID.SMSING,
                                        eLibConst.TREATID.THRESHOLD_EMAILING,
                                        eLibConst.TREATID.THRESHOLD_REPORT
                      });

                eRightReport oRightManager = new eRightReport(pref, TypeReport.ALLFORWIZARD);

                List<int> lPinnedBookmarkList = StructureBkmModelFactory.GetPinnedBookmarkList(pref, objFile.CalledTabDescId);

                //on parcourt la liste des signets pour les ajouter au flux
                IEnumerable<System.Threading.Tasks.Task<FileDetailModel.StructBkmModel>> lstTkEBookmarks = objFile.LstBookmark
                    ?.Where(bkm => bkm != null)
                    .AsParallel()
                    .Select(bkm => System.Threading.Tasks.Task.Run(() => StructureBkmModelFactory.InitStructureBkmModelFactory(bkm, pref).GetStructBkmModel(globalRight, oRightManager, lPinnedBookmarkList)));

                FileDetailModel.StructBkmModel[] enumBkm = await System.Threading.Tasks.Task.WhenAll(lstTkEBookmarks);

                return enumBkm.Where(bkm => bkm != null).OrderBy(structBkm => objFile.ListBkmOrder.IndexOf(structBkm?.DescId ?? -1));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// retourrne la liste des bookmarks d'un onglet.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{nTab:int=0}/{nFileId:int}")]
        public async System.Threading.Tasks.Task<IHttpActionResult> Get(int nTab, int nFileId)
        {
            try
            {
                IEnumerable<StructBkmModel> data = await GetData(nTab, nFileId);

                return Ok(JsonConvert.SerializeObject(data));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// retourne le détail d'un signet.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{ParentTab:int/ParentFileId:int/Bkm:int/RowsPerPage:int/Page:int}")]
        public async System.Threading.Tasks.Task<IHttpActionResult> Get([FromUri] BkmRequestModel request)
        {

            if (!ModelState.IsValid)
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));

            //Gestion nRows = 0
            if (request.BkmFilePos > 0)
                request.Page = (request.BkmFilePos / (request.RowsPerPage <= 0 ? eLibConst.MAX_ROWS : request.RowsPerPage)) + 1;

            try
            {
                eBkmPref bkmPref = new eBkmPref(_pref, request.ParentTab, request.Bkm);

                ePref pref = _pref;
                bool bForceLoad = !(bkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.VIEWMODE) == ((int)ePrefConst.BKMVIEWMODE.LIST).ToString() || request.IsPinned);
                eList objBkm = await System.Threading.Tasks.Task.Run(() => eBookmark.CreateBookmark(pref, request.ParentTab, request.ParentFileId, request.Bkm, request.Page, request.RowsPerPage, bDisplayAllRecord: request.DisplayAllRecord, load: eBookmark.LoadMode.LOAD, bTriForceLoad: bForceLoad));

                PagingInfoModel pgim = PagingInfoModelFactory.InitPagingInfoModelFactory(objBkm).GetPagingInfoModel();

                if (objBkm == null)
                    return InternalServerError(new EudoException("Une Erreur non identifiée s'est produite durant la génération du jeu de données. Merci de contacter notre support."));

                if (!String.IsNullOrEmpty(objBkm.ErrorMsg) || objBkm.ErrorType != QueryErrorType.ERROR_NONE || objBkm.InnerException != null)
                    return InternalServerError(new EudoException(objBkm.ErrorMsg, sUserMessage: "Une erreur s'est produite lors de la génération du signet", innerExcp: objBkm.InnerException));

                eBookmark edmBkm = objBkm as eBookmark;


                if (edmBkm == null)
                    return StatusCode(System.Net.HttpStatusCode.NoContent);

                if (edmBkm.BkmEdnType != EdnType.FILE_GRID)
                {
                    edmBkm.BkmFileId = request.BkmFileId;
                    edmBkm.BkmFilePos = request.BkmFilePos;

                    if (edmBkm.BkmFileId != 0) {
                        GetFileToDisplay(edmBkm);
                    }

                    //BSE:US:1 788 Tâche: 2 584 Besoin de retourner la structure même quand il n'a pas de résultat:ça permet de savoir:
                    // si un filtre a été activé et qu'aucun résultat ne correspond
                    // Si on est dans le cas de suppression de la derniere fiche

                    if (bkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.VIEWMODE) == ((int)ePrefConst.BKMVIEWMODE.FILE).ToString() && request.IsPinned)
                    {

                        IEnumerable<Task<FileDetailModel>> lstTkEdmBkm = edmBkm.GetIdsList()
                             .Where((fileId, index) => index >= edmBkm.BkmFilePos)
                             .Take(request.RowsPerPage)
                             .Select(async (fileId) => {
                                 eFile objFile = eFileLite.CreateFileLite(_pref, request.Bkm, fileId);
                                 return await DetailFactory.InitDetailFactory(objFile, _pref, request.Bkm).CreateFileDetailModelFromFile();
                             });

                        var lstEdmBkm = await Task.WhenAll(lstTkEdmBkm);

                        return Ok(JsonConvert.SerializeObject(lstEdmBkm));

                    }

                    ListDetailModel.StructureModel structListMF;
                    BookmarkDetailModel ldMF = null;

                    try
                    {
                        structListMF = await StructureModelForBookmarkFactory.InitListDetailModelFactory(objBkm, _pref, edmBkm.BkmPref)
                            .GetListStructureModel();
                        ldMF = BookmarkDetailModelFactory.InitBookmarkDetailModelFactory(_pref, structListMF, objBkm, pgim)
                            .GetBookmarkDetailModel();
                    }
                    catch (Exception ex)
                    {
                        return InternalServerError(ex);
                    }

                    return Ok(JsonConvert.SerializeObject(ldMF));
                }
                else
                {
                    #region avec les grilles
                    edmBkm.BkmFileId = request.Bkm;
                    edmBkm.BkmFilePos = 0;
                    eRenderer BkmRenderer = new eBookmarkGridSubMenu(_pref, edmBkm);
                    await System.Threading.Tasks.Task.Run(() => BkmRenderer.Generate());

                    BkmRenderer.PgContainer.ID = $"bkm_{edmBkm.CalledTabDescId}";
                    BkmRenderer.PgContainer.CssClass = "bkmdiv";

                    eError _error = eError.getError();
                    eRendererXMLHTML _renderXMLHTML = eRendererXMLHTML.GetRenderXMLHTML(_error);
                    _error.SetPref(_pref);

                    DetailXHTMLFactory dxf = new DetailXHTMLFactory
                    {
                        _error = _error,
                        _renderXMLHTML = _renderXMLHTML
                    };

                    HtmlGenericControl ctrlLinkGrilles = dxf.ChangeAttributeOfCtrlRecursively(BkmRenderer.PgContainer.Controls.GetEnumerator(), "firstSubTabItem") as HtmlGenericControl;

                    if (ctrlLinkGrilles != null)
                    {
                        //ctrlLinkGrilles.Attributes["onclick"] = "setActionOnGrid(event, " + Bkm + ", true)";
                        // A la demande du PO, pour des raisons de simplicité d'usage, on désactive le lien sur "Grille".
                        ctrlLinkGrilles.Attributes.Remove("onclick");
                    }

                    HtmlGenericControl ctrlLinkRefresh = dxf.ChangeAttributeOfCtrlRecursively(BkmRenderer.PgContainer.Controls.GetEnumerator(), "xrmGridRefreshDate") as HtmlGenericControl;

                    WebControl ctrlLinkRefreshFather = ctrlLinkRefresh?.Parent as WebControl;

                    if (ctrlLinkRefreshFather != null)
                    {
                        ctrlLinkRefreshFather.Attributes["onclick"] = "setActionOnGrid(event, " + request.Bkm + ", false)";
                        ctrlLinkRefreshFather.Controls.OfType<HtmlGenericControl>()
                            .Where(node => node.Attributes["action"] != null)
                            .DefaultIfEmpty()
                            .ToList()
                            .ForEach(node =>
                            {
                                if (node.Attributes["action"].ToLower() == "refresh")
                                    node.Attributes["class"] = "fas fa-sync";
                                else if (node.Attributes["action"].ToLower() == "options")
                                    node.Attributes["class"] = "fas fa-cog";
                            });
                    }

                    string sRes = @"<div class=""dispalyGridBkm""> " + dxf.GetResultHTML(BkmRenderer.PgContainer) + "</div>";

                    #endregion


                    return Ok(sRes);

                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

        }


        public override IHttpActionResult Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
