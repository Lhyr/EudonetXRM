using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// TYpe Binaire, comme les varBinary. Normalement on n'affiche rien, mais 
    /// ne pas les gérer fait planter le système.
    /// </summary>
    public class BinaryFieldInfosModel : FldTypedInfosModel
    {

        #region constructor
        internal BinaryFieldInfosModel(Field f) : base(f)
        {
            Format = FieldType.Hidden;
        }
        #endregion

    }
}