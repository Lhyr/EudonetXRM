using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type catalogue
    /// </summary>
    public class SocialNetworkFieldInfos : FldTypedInfosModel
    {

        public string RootURL = "";
        public string Icon = "";
        public string IconColor = "";

        internal SocialNetworkFieldInfos(Field f) : base(f)
        {
            Format = FieldType.SocialNetwork;
            RootURL = f.RootURL;
            Icon = f.Icon;
            IconColor = f.IconColor;
        }



    }
}