using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.DataFields
{
    /// <summary>
    /// TYpe Binaire, comme les varBinary. Normalement on n'affiche rien, mais 
    /// ne pas les gérer fait planter le système.
    /// </summary>
    public class BinaryDataFieldModel : DataFieldWithDisplayModel
    {

        #region constructor
        internal BinaryDataFieldModel(eFieldRecord f) : base(f)
        {
        }
        #endregion
    }
}