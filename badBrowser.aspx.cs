using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm
{
    public partial class badBrowser : System.Web.UI.Page
    {

        public int _nLangServ = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            HashSet<String> lstQS = new HashSet<String>(Request.QueryString.AllKeys);

            if (lstQS.Contains("lng"))
            {
                string sLng = Request.QueryString["lng"].ToString();
                Int32.TryParse(sLng, out _nLangServ);
                if (_nLangServ < 0 || _nLangServ > 10)
                    _nLangServ = 1; // langue par défaut : Anglais

              
            }

			// Si langue = Français, URLs spécifiques de téléchargement en français
            if (_nLangServ == 0)
            {
                lnkChrome.HRef = "https://www.google.com/intl/fr_fr/chrome";
                lnkFF.HRef = "https://www.mozilla.org/fr/firefox/new/";
                lnkSafari.HRef = "https://support.apple.com/fr_FR/downloads/internet";
                lnkEdge.HRef = "https://www.microsoft.com/fr-fr/edge";
            }

            title.InnerText = eResApp.GetRes(_nLangServ, 2878);
            titlebody.InnerHtml = eResApp.GetRes(_nLangServ, 2878);
            msgbody.InnerHtml = eResApp.GetRes(_nLangServ, 2879);
        }
    }
}