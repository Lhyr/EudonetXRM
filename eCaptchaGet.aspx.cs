using System;
using System.Drawing;
using System.Drawing.Imaging;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm
{
    public partial class eCaptchaGet : System.Web.UI.Page
    {
        /// <summary>
        /// page affichant un captcha
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["Pref"] == null || !Session["Pref"].GetType().Equals(typeof(ePref)))
            //Response.End();
            //L'appel de cette page ne nécessiite pas de session en cours.
            //Mais pour des raisons de sécurité, nous ajoutons ci-dessous un tst sur la page de provenance.
            
            Bitmap _btmp = null;            
            Random _random = new Random();
            String _captchaText = _random.Next(1000000, 9999999).ToString();
            eCaptcha _cap = new eCaptcha(200, 100, _captchaText, out _btmp);
            Session["Captcha"] = _captchaText;
            Response.Clear(); Response.ClearContent(); Response.ClearHeaders();

            Response.ContentType = "image/jpeg";
            _btmp.Save(Response.OutputStream, ImageFormat.Jpeg);
        }
    }
}