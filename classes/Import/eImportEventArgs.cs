using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.Import
{
    /// <summary>
    /// Informations transmis lors d'un evenement
    /// </summary>
    public class eImportEventArgs : EventArgs
    {
        public Int32 TotalLine { get; set; }
        public Int32 TotalSucessLine { get; set; }
        public Int32 TotalErrorLine { get; set; }
        public Int32 TotalLineProcessed { get; set; }

        public eErrorContainer ErrorContainer { get; set; }
    }

    /// <summary>
    /// Evenement déclenché avant et après un traitement d'import
    /// </summary>
    /// <param name="Sender">déclancheur</param>
    /// <param name="args">arguments</param>
    public delegate void ImportProcessHandler(object Sender, eImportEventArgs args);
}