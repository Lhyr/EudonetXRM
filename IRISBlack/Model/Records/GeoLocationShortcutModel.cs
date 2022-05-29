using Com.Eudonet.Xrm.IRISBlack.Model.DataFields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    public class GeoLocationShortcutModel: MenuShortcutModel
    {
        public IEnumerable<GeolocationDataFieldModel> Fields { get; set; }
    }
}