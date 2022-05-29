using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.import
{
    /// <summary>
    /// Action demandée par le js
    /// </summary>
    public enum eImportWizardAction
    {
        /// <summary>
        /// Demande à charger une étape de l'assistant
        /// </summary>
        NO_ACTION = 0,

        /// <summary>
        /// Demande à charger une étape de l'assistant
        /// </summary>
        LOAD_STEP = 1,

        /// <summary>
        /// Progression de l'excution de l'etape
        /// </summary>
        EXEC_PROGRESS = 2,

        /// <summary>
        /// Upload file
        /// </summary>
        UPLOAD_FILE = 3,

        /// <summary>
        /// Lance l'execution d'import via WCF
        /// </summary>
        EXEC = 4,

        /// <summary>
        /// Verifiier la validité des paramètres 
        /// </summary>       
        CHECK_PARAMS = 5,

        /// <summary>
        /// Supprimer le fichier 
        /// </summary>       
        DELETE_FILE = 6,

        /// <summary>
        /// Verifie si un import est en cours  
        /// </summary>       
        CHECK_RUNNING = 7

    }
}