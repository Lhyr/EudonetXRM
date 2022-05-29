using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{


    /// <summary>
    /// Informations contextuelles pour le chargement d'une nouvelle fiche
    /// </summary>
    public class FileDetailContextInfosModel
    {
        public int tab;
        public int fileid;
        public List<ValueModel> values;


        public class ValueModel
        {
            public int descid;
            public string value;
            public int spclnk;
        }
    }
}