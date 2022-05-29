using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type Case à cocher
    /// </summary>
    public class LogicFieldInfos : FldTypedInfosModel
    {
        /// <summary>
        /// Type d'affichage, Switch ou Checkbox
        /// </summary>
        public LOGIC_DISPLAY_TYPE DisplayType { get; set; }

        internal LogicFieldInfos(Field f, DescAdvDataSet descAdv) : base(f) {
            Format = FieldType.Logic;
            DisplayType = (LOGIC_DISPLAY_TYPE)eLibTools.GetNum(descAdv.GetAdvInfoValue(f.Descid, DESCADV_PARAMETER.LOGIC_DISPLAY_TYPE));
        }

    }
}