using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// Classe pour les champs de type hidden (obsolètes, v7, toussa, toussa, mais on en a encore)
    /// </summary>
    public class HiddenFieldInfosModel : FldTypedInfosModel
    {
        #region constructor
        internal HiddenFieldInfosModel(Field f) : base(f)
        {
            Format = FieldType.Hidden;
        }
        #endregion

    }
}