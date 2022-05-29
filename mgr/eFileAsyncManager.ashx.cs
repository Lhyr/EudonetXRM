using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.eda;
using EudoQuery;
using System;
using System.Text;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Description résumée de eFileAsyncManager
    /// </summary>
    public class eFileAsyncManager : eFileManager
    {
        /// <summary>représente la partie de la fiche demandée</summary>
        private enum REQ_PART
        {
            //A savoir : des variables équivalentes ont été créées dans eFile.js

            /// <summary>Pas d'info (dans ce cas on arretera la page)</summary>
            NONE = 0,

            /// <summary>Partie haute de la fiche avec le squelette</summary>
            PART1_WITHBACKBONE = 1,

            /// <summary>Partie de la fiche reportée dans les signets (détails)</summary>
            PART2 = 2,

            /// <summary>Barre des signets</summary>
            BKMBAR = 3,

            /// <summary>Signet individuel</summary>
            BKM = 4,

            /// <summary>Tous les signets de la fiche sauf 1</summary>
            BKMALLBUT = 5,

            /// <summary>Partie haute de la fiche sans le squelette</summary>
            PART1_ONLY = 6,

            /// <summary>Propriété de la fiche</summary>
            PROPERTIES = 7,

            /// <summary>Tous les signets + la barre des signets</summary>
            BKMBLOCK = 8,

            /// <summary>Un seul commentaire</summary>
            DISC_COMM = 9,

            /// <summary> Rendu sous forme d'une grille des pages d'accueils</summary>
            XRM_HOMEPAGE = 10,

            /// <summary> Rendu sous forme d'une grille des pages d'accueils</summary>
            XRM_HOMEPAGE_GRID = 11

        }
        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            Int32 nTab = 0;
            Int32 nFileId = 0;
            Int32 nBkmDescId = 0;
            Int32 iReqPart = 0;
            REQ_PART reqPart;

            Boolean bDisplayAll = false;
            Boolean bFileTabInBkm = false;
            Int32 nBkmRows = 0, nBkmPage = 1, nBkmFilePos = 0, nBkmFileId = 0;


            if (!_requestTools.AllKeys.Contains("part") ||
                String.IsNullOrEmpty(_context.Request.Form["part"]) ||
                !Int32.TryParse(_context.Request.Form["part"].ToString(), out iReqPart) ||
                iReqPart <= 0)
                ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, "eFileAsyncManager : pas de part");

            LaunchError();

            reqPart = (REQ_PART)iReqPart;

            if (!_requestTools.AllKeys.Contains("fileid") ||
                String.IsNullOrEmpty(_context.Request.Form["fileid"]) ||
                !Int32.TryParse(_context.Request.Form["fileid"].ToString(), out nFileId) ||
                (nFileId <= 0 && reqPart != REQ_PART.XRM_HOMEPAGE_GRID))
                ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, "eFileAsyncManager : pas de fileid");

            nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            if (reqPart != REQ_PART.XRM_HOMEPAGE_GRID && nTab <= 0)
                ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, "eFileAsyncManager : pas de tab");

            if (_requestTools.AllKeys.Contains("bkm") && !String.IsNullOrEmpty(_context.Request.Form["bkm"]))
                Int32.TryParse(_context.Request.Form["bkm"].ToString(), out nBkmDescId);

            if (_requestTools.AllKeys.Contains("displayall") && !String.IsNullOrEmpty(_context.Request.Form["displayall"]))
                bDisplayAll = (_context.Request.Form["displayall"].ToString() == "1");

            if (_requestTools.AllKeys.Contains("bftbkm") && !String.IsNullOrEmpty(_context.Request.Form["bftbkm"]))
                bFileTabInBkm = (_context.Request.Form["bftbkm"].ToString() == "1");
            if (_requestTools.AllKeys.Contains("width") && !String.IsNullOrEmpty(_context.Request.Form["width"]))
                _pref.Context.FileWidth = eLibTools.GetNum(_context.Request.Form["width"].ToString());

            if (_requestTools.AllKeys.Contains("rows") && !String.IsNullOrEmpty(_context.Request.Form["rows"]))
                nBkmRows = eLibTools.GetNum(_context.Request.Form["rows"].ToString());

            if (_requestTools.AllKeys.Contains("bkmPage") && !String.IsNullOrEmpty(_context.Request.Form["bkmPage"]))
                nBkmPage = eLibTools.GetNum(_context.Request.Form["bkmPage"].ToString());

            if (_requestTools.AllKeys.Contains("bkmfilepos") && !String.IsNullOrEmpty(_context.Request.Form["bkmfilepos"]))
                nBkmFilePos = eLibTools.GetNum(_context.Request.Form["bkmfilepos"].ToString());

            if (_requestTools.AllKeys.Contains("bkmfileid") && !String.IsNullOrEmpty(_context.Request.Form["bkmfileid"]))
                nBkmFileId = eLibTools.GetNum(_context.Request.Form["bkmfileid"].ToString());

            if (reqPart == REQ_PART.PART1_WITHBACKBONE)
            {
                eudoDAL eDal = eLibTools.GetEudoDAL(_pref);
                eDal.OpenDatabase();
                try
                {
                    UpdateMRU(eDal, nTab, nFileId);
                }
                finally
                {
                    eDal.CloseDatabase();
                }
            }

            eRenderer renderer = null;

            ExtendedDictionary<String, Object> param = new ExtendedDictionary<String, Object>();


            try
            {
                switch (reqPart)
                {
                    case REQ_PART.NONE:
                        ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, "eFileAsyncManager : pas de part");
                        break;
                    case REQ_PART.PART1_WITHBACKBONE:
                        renderer = eRendererFactory.CreateFilePart1WithBackBoneRenderer(_pref, nTab, nFileId, param);
                        break;
                    case REQ_PART.PART1_ONLY:
                        renderer = eRendererFactory.CreateFilePart1Renderer(_pref, nTab, nFileId, param);
                        break;
                    case REQ_PART.PART2:
                        renderer = eRendererFactory.CreateFilePart2Renderer(_pref, nTab, nFileId, param);
                        break;
                    case REQ_PART.BKMBAR:
                        renderer = eRendererFactory.CreateBookmarkBarRenderer(_pref, nTab, nFileId, bFileTabInBkm);
                        break;
                    case REQ_PART.BKM:
                        //Retourne un bookmark
                        // #92 393 : on récupère désormais le numéro de page à afficher en paramètre depuis le JavaScript ("bkmPage"), plutôt que de toujours afficher la page 1
                        renderer = eRendererFactory.CreateBookmarkRenderer(_pref, nTab, nFileId, nBkmDescId, nBkmPage, nBkmRows, false, nBkmFileId: nBkmFileId, nBkmFilePos: nBkmFilePos);
                        break;
                    case REQ_PART.BKMALLBUT:
                        renderer = eRendererFactory.CreateBookmarkListAllButOneRenderer(_pref, nTab, nFileId, nBkmDescId);
                        break;
                    case REQ_PART.PROPERTIES:
                        renderer = eRendererFactory.CreatePropertiesRenderer(_pref, nTab, nFileId);
                        break;
                    case REQ_PART.BKMBLOCK:
                        eFile ef = eFileForBkm.CreateFileForBookmark(_pref, nTab, nFileId);//eFileLiteWithBkmBarInfos.CreateFileLiteWithBkmBarInfos(_pref, nTab, nFileId);
                        renderer = eRendererFactory.CreateBookmarkWithHeaderRenderer(_pref, ef, ef.ActiveBkm == ActiveBkm.DISPLAYALL.GetHashCode(), null, bFileTabInBkm);
                        break;
                    case REQ_PART.DISC_COMM:
                        eBkmDiscComm bkm = eBkmDiscComm.CreateBkmDiscComm(_pref, nTab, nFileId);
                        renderer = eRendererFactory.CreateBkmDiscCommRenderer(_pref, bkm);
                        break;
                    case REQ_PART.XRM_HOMEPAGE:
                        int height = _requestTools.GetRequestFormKeyI("height") ?? 0;
                        renderer = eAdminRendererFactory.CreateAdminXrmHomePageRenderer(_pref, nFileId, _pref.Context.FileWidth, height);
                        break;
                    case REQ_PART.XRM_HOMEPAGE_GRID:
                        Int32.TryParse(_context.Request.Form["tab"].ToString(), out nTab);
                        height = _requestTools.GetRequestFormKeyI("height") ?? 0;
                        int parentTab = _requestTools.GetRequestFormKeyI("parenttab") ?? nTab;
                        int parentFid = _requestTools.GetRequestFormKeyI("parentfid") ?? 0;

                        eXrmWidgetContext context = new eXrmWidgetContext(nFileId, parentTab, parentFid);                      

                        renderer = eRendererFactory.CreateXrmGridRenderer(_pref, nFileId, _pref.Context.FileWidth, height, context);
                        break;

                    default:
                        ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, "eFileAsyncManager : pas de part");
                        break;
                }

                // On laisse passer les erreurs bookmark non disponible pour des raisons de droits ou de liaison non trouvée
                if (renderer != null && renderer.ErrorMsg.Length > 0 && renderer.ErrorNumber != EudoQuery.QueryErrorType.ERROR_NUM_BKM_NOT_LINKED)
                {

                    if (renderer.ErrorNumber == QueryErrorType.ERROR_NUM_FILE_NOT_FOUND)
                    {
                        ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 6695), eResApp.GetRes(_pref, 6696), eResApp.GetRes(_pref, 6695));
                    }
                    else
                    {
                        StringBuilder sDevMsg = new StringBuilder("Erreur sur la page : ");
                        sDevMsg.AppendLine(System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1])
                                .Append("Partie de la fiche demandée : ").AppendLine(reqPart.ToString())
                                .Append("Tab : ").AppendLine(nTab.ToString())
                                .Append("FileId : ").AppendLine(nFileId.ToString())
                                .Append("Erreur :").AppendLine(renderer.ErrorMsg)
                                .Append("ErrorNumber :").AppendLine(renderer.ErrorNumber.ToString());


                        if (renderer.InnerException != null)
                            sDevMsg.AppendLine(renderer.InnerException.Message).AppendLine(renderer.InnerException.StackTrace);

                        String sUserMsg = String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544));

                        if (renderer.InnerException != null)
                        {
                            sDevMsg.Append("Exception Message : ").AppendLine(renderer.InnerException.Message);
                            sDevMsg.Append("Exception StackTrace :").Append(renderer.InnerException.StackTrace);
                        }

                        ErrorContainer = eErrorContainer.GetDevUserError(
                           eLibConst.MSG_TYPE.CRITICAL,
                           eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                           sUserMsg,  //  Détail : pour améliorer...
                           eResApp.GetRes(_pref, 72),  //   titre
                           sDevMsg.ToString());
                    }
                }


            }
            catch (eFileLayout.eFileLayoutException flex)
            {
                ErrorContainer = flex.ErrorContainer;
            }
            catch (Exception ex)
            {

                StringBuilder sDevMsg = new StringBuilder("Erreur sur la page : ");
                sDevMsg.AppendLine(System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1])
                        .Append("Partie de la fiche demandée : ").AppendLine(reqPart.ToString())
                        .Append("Exception Message : ").AppendLine(ex.Message)
                        .Append("Exception StackTrace :").Append(ex.StackTrace);

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   sDevMsg.ToString());

            }



            LaunchError();

            RenderResultHTML(renderer.PgContainer);






        }
    }
}