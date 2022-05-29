using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using Com.Eudonet.Internal;
using EudoQuery;
using System.Web;

namespace Com.Eudonet.Xrm
{
    /// <className></className>
    /// <summary>Classe de gestion du chargement et de modifications carte BingMaps</summary>
    /// <authors>MOU</authors>
    public partial class eCartography : eEudoPage
    {

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {            
            PageRegisters.AddCss("eXrmHomePage");
            PageRegisters.AddCss("eBingCarto");           
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("bingmaps/eBingCarto");           
        }

        /// <summary>
        /// Retourne la ressource correspondante
        /// </summary>
        /// <param name="resId"></param>
        /// <returns></returns>
        public string GetRes(int resId) {
            return eResApp.GetRes(_pref, resId).Replace("\"", "&#34;");
        }
    }
}
