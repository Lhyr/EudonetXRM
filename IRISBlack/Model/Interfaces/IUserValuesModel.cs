using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    public interface IUserValuesModel
    {
        IList<IUserValue> Values { get; set; }
    }
}
