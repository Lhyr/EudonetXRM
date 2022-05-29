using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type Bouton
    /// </summary>
    public class ButtonFieldInfos : FldTypedInfosModel
    {
        /// <summary>
        /// action spécifique sur le bouton.
        /// </summary>
        public BtnSpecificAction SpecificAction { get; set; }

        internal ButtonFieldInfos(Field f) : base(f)
        {
            SpecificAction = BtnSpecificAction.Undefined;
            Format = FieldType.Button;
        }

    }
}