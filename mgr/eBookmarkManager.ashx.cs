using System;
using System.Collections.Generic;
using Com.Eudonet.Internal;

using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <className>eBookmarkManager</className>
    /// <summary>Permet l'appel au renderer des bkm</summary>
    /// <purpose>Rafraichi  l'affichage d'un/de plusieurs signet(s)</purpose>
    /// <authors>SPH</authors>
    /// <date>2011-05-10</date>
    public class eBookmarkManager : eEudoManagerReadOnly
    {
        /// <summary>
        /// Met à jour le Rendu d'un BookMark ou de la liste des BookMarks
        /// </summary>
        protected override void ProcessManager()
        {
            Int32 nParentFileId = 0;
            Int32 nParentTab = 0;
            Int32 nBkmDescId = 0;
            Int32 nBkmFileId = 0;
            Int32 nBkmFilePos = 0;
            Int32 nPage = 1;
            Int32 nBkmRows = 0;
            Boolean bDisplayAll = false;
            Boolean bDisplayAllRecord = false;
            Boolean bReloadBkmBar = false;
            String sType = String.Empty;
            Boolean bTabInBkm = false;

            if (_requestTools.AllKeys.Contains("parentfileid") && !String.IsNullOrEmpty(_context.Request.Form["parentfileid"]))
                Int32.TryParse(_context.Request.Form["parentfileid"].ToString(), out nParentFileId);

            if (_requestTools.AllKeys.Contains("parenttab") && !String.IsNullOrEmpty(_context.Request.Form["parenttab"]))
                Int32.TryParse(_context.Request.Form["parenttab"].ToString(), out nParentTab);

            if (_requestTools.AllKeys.Contains("bkmfileid") && !String.IsNullOrEmpty(_context.Request.Form["bkmfileid"]))
                Int32.TryParse(_context.Request.Form["bkmfileid"].ToString(), out nBkmFileId);

            if (_requestTools.AllKeys.Contains("bkmfilepos") && !String.IsNullOrEmpty(_context.Request.Form["bkmfilepos"]))
                Int32.TryParse(_context.Request.Form["bkmfilepos"].ToString(), out nBkmFilePos);

            if (_requestTools.AllKeys.Contains("displayall") && !String.IsNullOrEmpty(_context.Request.Form["displayall"]))
                bDisplayAll = _context.Request.Form["displayall"] == "1";

            if (_requestTools.AllKeys.Contains("bkm")
                    && !String.IsNullOrEmpty(_context.Request.Form["bkm"])
                    && Int32.TryParse(_context.Request.Form["bkm"].ToString(), out nBkmDescId)
                    && !bDisplayAll
                )
            {
                List<SetParam<ePrefConst.PREF_PREF>> pref = new List<SetParam<ePrefConst.PREF_PREF>>();
                pref.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.ACTIVEBKM, nBkmDescId.ToString()));
                _pref.SetPref(nParentTab, pref);
            }

            if (_requestTools.AllKeys.Contains("page") && !String.IsNullOrEmpty(_context.Request.Form["page"]))
                Int32.TryParse(_context.Request.Form["page"].ToString(), out nPage);

            if (_requestTools.AllKeys.Contains("rows") && !String.IsNullOrEmpty(_context.Request.Form["rows"]))
                Int32.TryParse(_context.Request.Form["rows"].ToString(), out nBkmRows);


            if (_requestTools.AllKeys.Contains("displayallrecord") && !String.IsNullOrEmpty(_context.Request.Form["displayallrecord"]))
                bDisplayAllRecord = _context.Request.Form["displayallrecord"] == "1";

            // TYPE = 0 : appel via loadbkmlist - reload complet du bloc signet : changement des signets affichés, 
            // TYPE = 1 : appel via loadbkm - reload partiel d'un signet : pour filtre express/paging,...
            if (_requestTools.AllKeys.Contains("type") && !String.IsNullOrEmpty(_context.Request.Form["type"]))
                sType = _context.Request.Form["type"].ToString();

            if (_requestTools.AllKeys.Contains("rldbkmbar") && !String.IsNullOrEmpty(_context.Request.Form["rldbkmbar"]))
                bReloadBkmBar = _context.Request.Form["rldbkmbar"] == "1";

            if (_requestTools.AllKeys.Contains("btabinbkm") && !String.IsNullOrEmpty(_context.Request.Form["btabinbkm"]))
                bTabInBkm = _context.Request.Form["btabinbkm"] == "1";


            if (_requestTools.AllKeys.Contains("bkmfilepos") && !String.IsNullOrEmpty(_context.Request.Form["bkmfilepos"]))
            {
                Int32.TryParse(_context.Request.Form["bkmfilepos"].ToString(), out nBkmFilePos);
                if (nBkmFilePos > 0)
                {
                    nPage = (nBkmFilePos / nBkmRows) + 1;
                    //nBkmFilePos = nBkmFilePos % nBkmRows;
                }
            }


            //
            if (nParentFileId == 0 || nParentTab == 0)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 6237),
                    eResApp.GetRes(_pref, 2024).Replace(" <PARAM> ", " "),
                    eResApp.GetRes(_pref, 72),
                    String.Concat(eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "nParentFileId, nParentTab"), " (nParentFileId = ", nParentFileId, ", nParentTab = ", nParentTab, ")")
                    );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }

            //
            eRenderer efRend;
            if (sType == "0")
            {
                //Reload du bloc signet complet : on passe par efile
                eFile ef = eFileMain.CreateMainFile(_pref, nParentTab, nParentFileId, nBkmDescId);
                //TODO KHA regénérer l'affichage de la fiche pour obtenir la partie de la fiche à afficher en signet
                if (bReloadBkmBar)
                    efRend = eRendererFactory.CreateBookmarkWithHeaderRenderer(_pref, ef, bDisplayAll, null, bTabInBkm);
                else if (bDisplayAll && nBkmDescId > 0)
                {
                    // TODO : A VERIFIER
                    //   CE CAS NE DEVRAIT PLUS EXISTER
                    efRend = null;

                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 6237),
                        "Cas non géré pour le rendu des signets",
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Cas non géré dans eBookmarkManager - sType == 0 && bDisplayAll && nBkmDescId = ", nBkmDescId, " && !bReloadHeader)")
                        );

                    //Arrete le traitement et envoi l'erreur
                    LaunchError();
                }
                else
                {
                    efRend = eRendererFactory.CreateBookmarkListRenderer(_pref, ef, bDisplayAll, nBkmDescId > 0);
                }
            }
            else
            {
                //reload d'un signet unique
                //  paging, filtre express, choix des rubriques..
                efRend = eRendererFactory.CreateBookmarkRenderer(_pref, nParentTab, nParentFileId, nBkmDescId, nPage, nBkmRows, bDisplayAllRecord, nBkmFileId: nBkmFileId, nBkmFilePos: nBkmFilePos);
            }

            // On laisse passer les erreurs bookmark non disponible pour des raisons de droits ou de liaison non trouvée
            if (efRend.ErrorMsg.Length == 0 || efRend.ErrorNumber == EudoQuery.QueryErrorType.ERROR_NUM_BKM_NOT_LINKED)
            {
                RenderResultHTML(efRend.PgContainer);
            }
            else
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 6237),
                    efRend.ErrorMsg,
                    eResApp.GetRes(_pref, 72),
                    String.Concat(efRend.ErrorNumber, " - ", efRend.ErrorMsg)
                    );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }
        }
    }
}