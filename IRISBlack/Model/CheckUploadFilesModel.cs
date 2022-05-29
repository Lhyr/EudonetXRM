using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    public class CheckUploadFilesModel
    {
        /// <summary>
        /// Liste des fichiers à checker
        /// </summary>
        public IList<CheckFileModel> CheckFile { get; set; }
        /// <summary>
        /// Le succès ou l'échec de l'opération.
        /// </summary>
        public bool Success { get; set; }
    }
}