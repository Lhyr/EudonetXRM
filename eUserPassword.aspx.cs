using System;
using System.Web.UI;
using Com.Eudonet.Internal;
using System.Text;
using System.IO;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// classe de gestion de mot de passe
    /// </summary>
    public partial class eUserPassword : eEudoPage
    {
        public bool _renew = false;

        /// <summary>
        /// Contexte depuis lequel est appelée la page de changement de mot de passe
        /// </summary>
        public eUserOptionsModules.PREFERENCES_PASSWORD_CONTEXT _context;
        /// <summary>
        /// id du user pour lequel doit être modifié le mot de passe
        /// </summary>
        public int _userId;

        /// <summary>
        /// Contenu de la page
        /// </summary>
        public string strPageContents = String.Empty;

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return null;
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region ajout des css

            PageRegisters.AddCss("eLogin");

            #endregion

            _userId = _pref.User.UserId;

            //on affiche uniquement deux textbox pour le changement de mot de passe
            eUserOptionsModules.PREFERENCES_PASSWORD_CONTEXT _context = eUserOptionsModules.PREFERENCES_PASSWORD_CONTEXT.LOGIN_EXPIRED;
            if (_allKeys.Contains("context") && Request.Form["context"] != null)
                Enum.TryParse(Request.Form["context"].ToString(), out _context);

            eAdminUsrOptPwdRenderer er = (eAdminUsrOptPwdRenderer)eRendererFactory.CreateUserOptionsPrefPwdRenderer(_pref, _userId, _context);
            if (er != null)
                strPageContents = GetResultHTML(er.PgContainer);
        }

        /// <summary>
        /// Renvoie le contenu HTML du contrôle passé en paramètre
        /// Similaire à eEudoPage.RenderResultHTML, mais renvoie le code généré plutôt que de l'ajouter directement à la page
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public string GetResultHTML(Control control)
        {
            string strResult = String.Empty;

            if (control != null)
            {
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                HtmlTextWriter tw = new HtmlTextWriter(sw);
                control.RenderControl(tw);
                strResult = sb.ToString();
            }

            return strResult;
        }
    }
}