using System;
using System.Web.UI;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm.eda
{
    public partial class eAdminPage : eEudoPage
    {
        /// <summary>
        /// Vérificatin des droits et css par défaut
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void PageLoad(object sender, EventArgs e)
        {

            /*
            #if !DEBUG
                        Response.End();
            #endif

            */


            base.PageLoad(sender, e);

            if (_pref == null || !_pref.IsAdminEnabled)
                Response.End();

            #region Ajout CSS et scripts
            PageRegisters.RegisterFromRoot = true;
            // CSS
            // EudoFont.css et theme.css sont automatiquement inclus
            // JS
            PageRegisters.AddCss("eMain");
            PageRegisters.AddScript("eTools");
            PageRegisters.RegisterAdminIncludeScript("eAdminEnum");

            #endregion


            #region Vérification niveau de l'utilisateur

            if (_pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
            {
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(0, 6441), eResApp.GetRes(0, 6342));
                LaunchErrorHTML(true, ErrorContainer);
            }

            //_pref.AdminMode = true;

            #endregion


            //Id du process - Check Anti XSRF - 
            // L'id est par session, il faudrait améliorer cela
            string _sIdProcess = _requestTools.GetRequestFormKeyS("_processid");
            string sVerif = Session["_uidupdater"]?.ToString() ?? "";
            if (sVerif != _sIdProcess)
            {
                //ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(0, 6441), eResApp.GetRes(0, 6342));
                //LaunchErrorHTML(true, ErrorContainer);

            }
        }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            throw new Exception("");
        }
    }
}