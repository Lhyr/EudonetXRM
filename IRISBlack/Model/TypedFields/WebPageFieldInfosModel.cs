using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type page web
    /// </summary>
    public class WebPageFieldInfos : FldTypedInfosModel, IMultiLine
    {
        public int Rowspan { get; set; }

        internal WebPageFieldInfos(Field f) : base(f)
        {
            Format = FieldType.WebPage;
            Rowspan = f.PosRowSpan;

        }

    }
}