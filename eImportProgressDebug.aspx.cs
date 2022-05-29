using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Form appeler pour selectionner la date de début et la date de fin pour le filtre express
    /// </summary>
    public partial class eImportProgressDebug : eEudoPage
    {
#if DEBUG
        public const bool bDebug = true;
#else
        public const bool bDebug = false;
#endif

        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        protected void Page_Load(object sender, EventArgs e)
        {           
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eTools");
        }
    }
}