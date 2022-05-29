using EudoQuery;
using System;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu du mode fiche en asynchrone
    /// N'est pas conçue pour le mode Popup
    /// </summary>
    public class eEditFileLiteRenderer : eEditFileRenderer
    {
        /// <summary>
        /// Affichage pour la modification en asynchrone
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="nFileId"></param>
        public eEditFileLiteRenderer(ePref pref, Int32 nTab, Int32 nFileId)
            : base(pref, nTab, nFileId)
        {
            _rType = RENDERERTYPE.EditFileLite;
        }

        /// <summary>
        /// Création et initialisation de l'objet eFile
        /// </summary>
        /// <returns></returns>
        protected override Boolean Init()
        {
            if (!(_tab > 0 && _nFileId > 0))
                return false;

            try
            {
                _myFile = eFileLiteWithBkmBarInfos.CreateFileLiteWithBkmBarInfos(Pref, _tab, _nFileId);

                if (_myFile.ErrorMsg.Length > 0)
                {
                    _eException = _myFile.InnerException;
                    _sErrorMsg = String.Concat("eEditFileLiteRenderer.Init ", Environment.NewLine, _myFile.ErrorMsg);
                    if (_myFile.InnerException.GetType() == typeof(EudoFileNotFoundException))
                    {
                        _nErrorNumber = QueryErrorType.ERROR_NUM_FILE_NOT_FOUND;
                    }
                    else
                    {
                        _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;
                    }
                    return false;
                }

                HideTeamsButtons();

                return true;
            }
            catch (Exception e)
            {
                _sErrorMsg = String.Concat("eEditFileLiteRenderer.Init ", Environment.NewLine, e.Message);
                _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;
                _eException = e;
                return false;
            }
        }

        /// <summary>
        /// rend le block HTML de tous les signets dans une méthode overridable
        /// </summary>
        /// <returns></returns>
        protected override void GetBookMarkBlock()
        {
            eRenderer bkmBarRdr = eRendererFactory.CreateBookmarkBarRenderer(Pref, _myFile, _bFileTabInBkm);
            if (bkmBarRdr.ErrorMsg.Length > 0)
            {
                _eException = bkmBarRdr.InnerException;
                _sErrorMsg = bkmBarRdr.ErrorMsg;
                _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;
            }
            else
            {
                eTools.TransfertFromTo(bkmBarRdr.PgContainer, _backBoneRdr.PnBkmBar);
            }

        }
    }
}