using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Com.Eudonet.Xrm.IRISBlack.Model
{/// <summary>
/// Model de champs des données
/// </summary>
    public class DataFieldsModel
    {
        public int Descid { get; set; }
        //public int FileId { get; set; }
        public string DisplayValue { get; set; }
        //public string DisplayValuePPName { get; set; }
        //public string BoundFieldValue { get; set; }
        public string Value { get; set; }

        //Déplacé sur l'objet RecordModel
        //public bool RightIsTableVisible { get; set; }
        //public bool RightIsUpdatable { get; set; }
        //public bool RightIsVisible { get; set; }
        //public bool NameOnly { get; set; }
        public bool IsLink { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsCaseOnThisFormat { get; set; }
        public object ObjectValue { get; set; }

    }
}