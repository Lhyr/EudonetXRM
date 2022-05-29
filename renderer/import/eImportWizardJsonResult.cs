using Com.Eudonet.Internal.Import;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.import
{
    /// <summary>
    /// Json pour le retour client
    /// </summary>
    public class eImportWizardJsonResult : JSONReturnHTMLContent
    {
        /// <summary>
        /// Numéro de l'etape dans l'assistant
        /// </summary>
        public int Step { get; set; }

        /// <summary>
        /// Progression de l'excution [0 - 100]
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// Paramètres de retour
        /// </summary>
        public ImportTemplate Params { get; set; }

        /// <summary>
        /// Détail du message d'erreur
        /// </summary>
        public string ErrorDetail { get; set; }
    }
}