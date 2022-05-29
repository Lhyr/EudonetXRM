using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type Fichier
    /// </summary>
    public class FileFieldInfos : FldTypedInfosModel
    {
       

        internal FileFieldInfos(Field f) : base(f)
        {
            Format = FieldType.File;
         
        }

    }
}