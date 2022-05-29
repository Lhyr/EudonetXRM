using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.import
{
    /// <summary>
    /// Interface pour faire un rendu d'une étape de l'assistant
    /// </summary>
    public interface IWizardStepRenderer
    {


        /// <summary>
        /// Initialisation de l'etape
        /// </summary>
        /// <returns></returns>
        IWizardStepRenderer Init();

        /// <summary>
        /// Execute l'opération du rendu
        /// </summary>
        /// <returns></returns>
        Panel Render();
    }
}
