using EudoQuery;
using System;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu d'un mode fiche en signet
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eRenderer" />
    public class eBookmarkFileRenderer : eRenderer
    {
        protected eBookmark _bkm;
        protected String _error = String.Empty;
        protected QueryErrorType _errErrorType = QueryErrorType.ERROR_NONE;
        protected Exception _innerException;

        /// <summary>
        /// Initializes a new instance of the <see cref="eBookmarkFileRenderer"/> class.
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="bkm">eBookmark</param>
        public eBookmarkFileRenderer(ePref pref, eBookmark bkm)
        {
            Pref = pref;
            _bkm = bkm;
        }

        /// <summary>
        /// Appel l'objet métier
        /// eList/eFiche (l'appel a EudoQuery est fait dans cet appel ainsi que l'appel et le parcours du dataset)
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (_error.Length > 0 || _innerException != null)
                return false;


            return true;


        }

        /// <summary>
        /// Construit le html de l'objet demandé
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            //Id de la fiche à afficher (post-création)
            if (_bkm.GetIdsList().Count == 0)
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

            try
            {
                _pgContainer.Controls.Add(eBookmarkRenderer.CreateTitleBar(Pref, bkm: _bkm, bDisplayButtons: true));
            }
            catch (Exception e)
            {
                _eException = e;
                _sErrorMsg = String.Concat("eBookmarkFileRenderer.Build()>eBookmarkRenderer.CreateTitleBar : ", Environment.NewLine,
                    "bkm: ", _bkm?.CalledTabDescId.ToString() ?? "null", ", bDisplayButtons: true", Environment.NewLine,
                    e.Message, Environment.NewLine,
                    e.StackTrace, Environment.NewLine);
                return false;
            }

            if (_bkm.GetIdsList().Count > 0)
            {
                ExtendedDictionary<String, Object> param = new ExtendedDictionary<String, Object>();
                param.Add("fileid", _bkm.BkmFileId);
                param.Add("popup", true);
                param.Add("bkmfile", true);
                Panel pnFile = eRendererFactory.CreateFileRenderer(eConst.eFileType.FILE_MODIF, _bkm.ViewMainTable, Pref, param).PgContainer;
                pnFile.CssClass = "BkmFileDiv";
                _pgContainer.Controls.Add(pnFile);
            }
            else
            {
                eBookmarkRenderer.SetEmptyBkmPanel(Pref, _bkm, _pgContainer);
            }
            return true;
        }
    }
}