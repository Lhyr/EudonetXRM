using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.import
{
    /// <summary>
    /// Class pour faire un rendu vide d'une etape
    /// </summary>
    public class eEmptyStepRenderer : IWizardStepRenderer
    {


        /// <summary>
        /// Initialisation de l'etape
        /// </summary>
        /// <returns></returns>
        public IWizardStepRenderer Init() { return this; }

        /// <summary>
        /// Execute l'opération du rendu
        /// </summary>
        /// <returns></returns>
        public Panel Render()
        {
            Panel ctn = new Panel();
#if DEBUG
            ctn.Controls.Add(new LiteralControl(""));
#endif
            return ctn;
        }
    }
}
