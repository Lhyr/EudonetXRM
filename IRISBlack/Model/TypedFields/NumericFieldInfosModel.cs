using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type numerique
    /// </summary>
    public class NumericFieldInfos : FldTypedInfosModel
    {
        /// <summary>
        /// Unité à afficher
        /// </summary>
        public eUnitDisplayLayout Unit = new eUnitDisplayLayout();
        internal NumericFieldInfos(Field f, ePref pref, DescAdvDataSet descAdv, eResInternal resAdv) : base(f)
        {
            Format = FieldType.Numeric;
            Unit.Position = descAdv.GetAdvInfoValue<UNIT_POSITION>(DescId, DESCADV_PARAMETER.UNIT_POSITION, ((int)UNIT_POSITION.RIGHT).ToString());
            bool bFound;
            Unit.Unit = resAdv.GetResAdv(new KeyResADV(eLibConst.RESADV_TYPE.UNIT, f.Descid, pref.LangId), out bFound); ;
        }

    }
}