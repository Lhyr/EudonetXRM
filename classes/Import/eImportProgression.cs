using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.Import.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Com.Eudonet.Xrm.Import
{
    /// <summary>
    /// Progression de l'import
    /// </summary>
    public class eImportProgression
    {
        /// <summary>Ligne en cours de traitement</summary>
        public eImportContentLine LineInProgress { get; set; }

        /// <summary>Lignes à traiter</summary>
        public Int32 TotalLine { get; set; }

        /// <summary>Identifiant des lignes importées (tplid)</summary>
        public ICollection<Int32> FilesCreatedId { get; private set; }

        /// <summary>Messages utilisateurs des lignes en échec</summary>
        public ICollection<ImportLineErrorMsg> LinesErrorMsg { get; private set; }

        /// <summary>
        /// Constructeur
        /// </summary>
        public eImportProgression()
        {
            this.FilesCreatedId = new List<Int32>();
            this.LinesErrorMsg = new List<ImportLineErrorMsg>();
        }

        /// <summary>
        /// Retourne les informations necessaires à la levé d'un evenement
        /// </summary>
        /// <param name="pref">pref utilisateur</param>
        /// <param name="errContainer">permet de forcer le message d'erreur mais si une ou plusieurs lignes ont rencontrées une erreur</param>
        /// <returns>informations d'evenement</returns>
        public eImportEventArgs GetEventArgs(ePrefLite pref, eErrorContainer errContainer = null)
        {
            // On donnée la priorité au message d'erreur passé, car dans ce cas il ne concerne pas une lignes en particulier
            eErrorContainer localErrContainer = errContainer;
            if (localErrContainer == null && LinesErrorMsg.Count > 0)
            {
                String message;
                StringBuilder sbUser = new StringBuilder();
                StringBuilder sbDev = new StringBuilder();

                foreach (ImportLineErrorMsg lineErrMsg in LinesErrorMsg)
                {
                    message = lineErrMsg.GetUserMsg(pref);
                    if (message.Length != 0)
                        sbUser.Append(message).AppendLine();

                    message = lineErrMsg.GetDevMsg(pref);
                    if (message.Length != 0)
                        sbDev.Append(message).AppendLine();
                }

                // 6340 - Import cible étendue
                // 1686 - Les lignes suivantes sont incorrectes : 
                localErrContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.INFOS,
                    eResApp.GetRes(pref, 1686), sbUser.ToString(), eResApp.GetRes(pref, 6568), sbDev.ToString());
            }

            return new eImportEventArgs()
            {
                TotalLine = this.TotalLine,
                TotalSucessLine = this.FilesCreatedId.Count,
                TotalErrorLine = this.LinesErrorMsg.Count,
                TotalLineProcessed = this.LineInProgress != null ? (this.LineInProgress.Index + 1) : 0,
                ErrorContainer = localErrContainer
            };
        }
    }
}