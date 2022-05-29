using Com.Eudonet.Xrm.IRISBlack.Model.DataFields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    public class EmailShortcutModel : MenuShortcutModel
    {
        public IEnumerable<MailDataFieldModel> Fields { get; set; }
    }
}