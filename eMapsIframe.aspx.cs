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
    /// <className>eMapsIFrame</className>
    /// <summary>Classe de gestion du chargement et de modifications carte BingMaps</summary>
    /// <authors>MOU</authors>
    /// <date>2017-09</date>
    public partial class eMapsIFrame : eEudoPage
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
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("bingmaps/eBingInterface");

            PageRegisters.AddCss("eMain");

            // Clé bing maps
            accessKey.Value = eLibConst.BING_MAPS_KEY;
        }
    }
}
