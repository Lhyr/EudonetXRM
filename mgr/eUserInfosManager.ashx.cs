using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static Com.Eudonet.Xrm.eConst;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Description résumée de eUserInfosManager
    /// </summary>
    public class eUserInfosManager : eEudoManager
    {

        private USERS_INFO_ACTION _eAction = USERS_INFO_ACTION.UPDATE;



        protected override void ProcessManager()
        {



            _eAction = eLibTools.GetEnumFromCode<USERS_INFO_ACTION>(_requestTools.GetRequestFormKeyI("action") ?? 0);

            switch (_eAction)
            {

                case USERS_INFO_ACTION.RENDER_POPUP_COPYPREF:
                    RendererPopupCopyPref();
                    break;
                case USERS_INFO_ACTION.RENDER_POPUP_CHANGEP_WD:
                    RenderPwdChg();
                    break;
                case USERS_INFO_ACTION.RENDER_POPUP_CHANGE_MEMO:
                    RenderMemoChg();
                    break;
                case USERS_INFO_ACTION.RENDER_POPUP_CHANGE_SIG:
                    RenderSigChg();
                    break;
                case USERS_INFO_ACTION.UPDATE:
                    UpdateAction();
                    break;
                default:
                    throw new EudoException("Action non prévue", "Action non prévue");
            }
        }

        private void UpdateAction()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// retourne le contenu d'une popup de choix de replication de préférence, à destination d'une modal
        /// </summary>
        private void RendererPopupCopyPref()
        {

            if (_pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                throw new EudoAdminInvalidRightException();

            Int32 nFileId = _requestTools.GetRequestFormKeyI("fileid") ?? 0;
            if (nFileId == 0)
                throw new EudoException("Id Utilisateur non fourni");

            eRenderer rdr = eAdminAccessPrefRenderer.CreateAdminAccessPrefRenderer(_pref, 400, 500, true, nFileId);

            AddHeadAndBody = true;



            PageRegisters.RegisterFromRoot = true;
            PageRegisters.RegisterAdminIncludeScript("eAdminPref");
            PageRegisters.RegisterAdminIncludeScript("eAdminUsers");
            PageRegisters.RegisterAdminIncludeScript("eAdmin");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eCatalog");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eList");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eUserOptions");
                     
            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eList");
            PageRegisters.AddCss("eIcon");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eAdmin");
            PageRegisters.AddCss("eAdminMenu");
            PageRegisters.AddCss("eAdminTranslations");
            PageRegisters.AddScript("eEvent");
            PageRegisters.AddScript("ckeditor/ckeditor");
            BodyCssClass = "adminModal bodyWithScroll";
            RenderResultHTML(rdr.PgContainer);
        }

        private void RenderSigChg()
        {
            Int32 nFileId = _requestTools.GetRequestFormKeyI("fileid") ?? 0;
            if (nFileId == 0)
                throw new EudoException("Id Utilisateur non fourni");


            //interdit à un nom admin de changer le mdp d'un autre user
            if (_pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN && nFileId != _pref.UserId)
                throw new EudoAdminInvalidRightException();

            eRenderer er = eRendererFactory.CreateUserOptionsPrefSignAdminRenderer(_pref, nFileId);

            AddHeadAndBody = true;



            PageRegisters.RegisterFromRoot = true;

            PageRegisters.RegisterAdminIncludeScript("eAdmin");
            PageRegisters.RegisterAdminIncludeScript("eAdminUsers");
            PageRegisters.RegisterAdminIncludeScript("eAdminPref");

            PageRegisters.AddScript("ckeditor/ckeditor");
            PageRegisters.AddScript("ePopup");
            PageRegisters.AddScript("eUserOptions");
            PageRegisters.AddScript("eFieldEditor");
            PageRegisters.AddScript("eGrapesJSEditor");
            PageRegisters.AddScript("eMemoEditor");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eAdminHomePageCkInstance");

            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eAdminMenu");
            PageRegisters.AddCss("eCatalog");
            PageRegisters.AddCss("eMain", "all");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eMemoEditor");
            PageRegisters.AddCss("eButtons");

            BodyCssClass = "adminModal bodyWithScroll";


            RenderResultHTML(er.PgContainer);

        }

        private void RenderMemoChg()
        {
            Int32 nFileId = _requestTools.GetRequestFormKeyI("fileid") ?? 0;
            if (nFileId == 0)
                throw new EudoException("Id Utilisateur non fourni");


            //interdit à un nom admin de changer le mdp d'un autre user
            if (_pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN && nFileId != _pref.UserId)
                throw new EudoAdminInvalidRightException();


            AddHeadAndBody = true;



            PageRegisters.RegisterFromRoot = true;

            PageRegisters.RegisterAdminIncludeScript("eAdmin");
            PageRegisters.RegisterAdminIncludeScript("eAdminUsers");
            PageRegisters.RegisterAdminIncludeScript("eAdminPref");

            PageRegisters.AddScript("ckeditor/ckeditor");
            PageRegisters.AddScript("ePopup");
            PageRegisters.AddScript("eFieldEditor");
            PageRegisters.AddScript("eGrapesJSEditor");
            PageRegisters.AddScript("eMemoEditor");
            PageRegisters.AddScript("eUserOptions");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eAdminHomePageCkInstance");

            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eAdminMenu");
            PageRegisters.AddCss("eCatalog");
            PageRegisters.AddCss("eMain", "all");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eMemoEditor");
            PageRegisters.AddCss("eButtons");

            BodyCssClass = "adminModal bodyWithScroll";

            eRenderer er = eRendererFactory.CreateUserOptionsPrefAdminMemoRenderer(_pref, nFileId);
            if (er.ErrorMsg.Length > 0)
            {
                LaunchErrorHTML(true);
            }
            else
                RenderResultHTML(er.PgContainer);

        }

        private void RenderPwdChg()
        {
            Int32 nFileId = _requestTools.GetRequestFormKeyI("fileid") ?? 0;
            if (nFileId == 0)
                throw new EudoException("Id Utilisateur non fourni");


            //interdit à un nom admin de changer le mdp d'un autre user
            if (_pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN && nFileId != _pref.UserId)
                throw new EudoAdminInvalidRightException();


            eRenderer rdr = eRendererFactory.CreateUserOptionsPrefPwdRenderer(_pref, nFileId, eUserOptionsModules.PREFERENCES_PASSWORD_CONTEXT.ADMIN_USERS);
            AddHeadAndBody = true;


            #region css& js
            PageRegisters.RegisterFromRoot = true;
            PageRegisters.RegisterAdminIncludeScript("eAdminPref");
            PageRegisters.RegisterAdminIncludeScript("eAdminUsers");
            PageRegisters.RegisterAdminIncludeScript("eAdmin");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eCatalog");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eList");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");

            PageRegisters.AddScript("eUserOptions");


            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eList");
            PageRegisters.AddCss("eIcon");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eAdmin");
            PageRegisters.AddCss("eAdminMenu");
            PageRegisters.AddCss("eAdminTranslations");

            BodyCssClass = "adminModal bodyWithScroll";

            #endregion

            RenderResultHTML(rdr.PgContainer);

        }
    }


}

