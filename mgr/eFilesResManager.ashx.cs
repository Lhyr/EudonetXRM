using Com.Eudonet.Internal;
using EudoQuery;
using System;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// eFilesResManager : mise à jour de RES_FILES
    /// </summary>
    public class eFilesResManager : eEudoManager
    {
        struct Result
        {
            public Boolean Success;
            public String UserError;
            public String DebugError;
        }

        enum ResAction
        {
            UNDEFINED,
            UPDATE,
            DELETE
        }

        /// <summary>
        /// Processes the manager.
        /// </summary>
        protected override void ProcessManager()
        {
            Result r = new Xrm.eFilesResManager.Result();
            r.Success = false;
            r.UserError = "";
            r.DebugError = "";

            #region Paramètres
            int tab, descid, fileid, langid, action;
            String value;

            ResAction resAction = ResAction.UNDEFINED;

            action = _requestTools.GetRequestFormKeyI("action") ?? 0;
            resAction = (ResAction)action;
            descid = _requestTools.GetRequestFormKeyI("descid") ?? 0;
            tab = _requestTools.GetRequestFormKeyI("tab") ?? descid - descid % 100;
            fileid = _requestTools.GetRequestFormKeyI("fileid") ?? 0;
            langid = _requestTools.GetRequestFormKeyI("langid") ?? _pref.LangId;
            value = _requestTools.GetRequestFormKeyS("value");
            #endregion


            if (resAction == ResAction.UPDATE)
            {
                #region MISE A JOUR
                eSqlResFiles res = new eSqlResFiles(tab, descid, fileid, langid, value);

                r.Success = eSqlResFiles.UpdateFileResList(_pref, new System.Collections.Generic.List<eSqlResFiles> { res }, out _sMsgError);
                #endregion
            }
            else if (resAction == ResAction.DELETE)
            {
                #region SUPPRESSION
                r.Success = eSqlResFiles.DeleteFileResList(_pref, tab, fileid, out _sMsgError);
                #endregion
            }

            if (!r.Success)
            {
                r.UserError = eResApp.GetRes(_pref, 8200);
                r.DebugError = _sMsgError;
                ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), r.UserError, "", r.DebugError);
                LaunchError();
            }

            RenderResult(RequestContentType.TEXT, delegate () { return SerializerTools.JsonSerialize(r); });
        }


    }
}