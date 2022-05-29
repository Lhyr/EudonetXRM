using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type Date
    /// </summary>
    public class DateFieldInfos : FldTypedInfosModel
    {

        /// <summary>
        /// format d'affichage de la date
        /// </summary>
        /// <example>dd/MM/yyyy</example>
        /// <example>DD d MM YYYY</example>
        /// <example>D dd/mm/yyyy</example>
        public string DisplayFormat { get; set; }

        /// <summary>
        /// Date de début pour la saisie d'un intervalle
        /// </summary>
        public int DateStartDescId { get; set; }

        /// <summary>
        /// date de fin pour la saisie d'un intervalle
        /// </summary>
        public int DateEndDescId { get; set; }

        internal DateFieldInfos(Field f, DescAdvDataSet descAdv) : base(f)
        {
            Format = FieldType.Date;
            DisplayFormat = eLibConst.FieldDateFormat[(eLibConst.DESCADV_CULTUREINFO)eLibTools.GetNum(descAdv.GetAdvInfoValue(f.Descid, DESCADV_PARAMETER.DISPLAY_FORMAT, ""))].Item1;
            DateEndDescId = eLibTools.GetNum(descAdv.GetAdvInfoValue(f.Descid, DESCADV_PARAMETER.DATE_END_DESCID));
        }

    }
}