using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.DataFields
{
    /// <summary>
    /// retourne les champs de type Fichier
    /// </summary>
    public class FileDataFieldModel : DataFieldWithValueModel
    {
        public string WorkDirectory;

        internal FileDataFieldModel(eFieldRecord f) : base(f)
        {
            WorkDirectory = f.BoundFieldValue;
        }

    }
}