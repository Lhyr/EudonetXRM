using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    interface IRelation
    {
        int TargetTab { get; }

        /// <summary>
        /// Libellé de la Table vers laquelle pointe la relation
        /// </summary>
        string TargetTabLabel { get; }

    }
}
