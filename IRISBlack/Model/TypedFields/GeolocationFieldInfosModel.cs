using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type Géolocalisation
    /// </summary>
    public class GeolocationFieldInfos : FldTypedInfosModel
    {

        internal GeolocationFieldInfos(Field f) : base(f)
        {
            Format = FieldType.Geolocation;
            DISPLAYINACTIONBAR = f.DisplayInActionBar;
        }

    }
}