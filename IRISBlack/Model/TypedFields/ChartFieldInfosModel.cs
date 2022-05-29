using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type graphique
    /// </summary>
    public class ChartFieldInfos : FldTypedInfosModel, IMultiLine
    {
        public int Rowspan { get; set; }

        internal ChartFieldInfos(Field f) : base(f)
        {
            Format = FieldType.Chart;
            Rowspan = f.PosRowSpan;

        }

    }
}