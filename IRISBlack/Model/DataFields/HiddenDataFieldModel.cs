using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.DataFields
{
    /// <summary>
    /// Classe pour les champs de type hidden (obsolètes, v7, toussa, toussa, mais on en a encore)
    /// </summary>
    public class HiddenDataFieldModel : DataFieldWithDisplayModel
    {
        #region constructor
        internal HiddenDataFieldModel(eFieldRecord f) : base(f)
        {
        }
        #endregion
    }
}