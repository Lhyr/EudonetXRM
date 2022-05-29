using Com.Eudonet.Core.Model;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// classe de représentation de la structure 
    /// </summary>
    public class PJRelationFieldInfosModel : RelationFieldInfos, IRelation
    {
        internal PJRelationFieldInfosModel(Field f, ePref pref, Internal.eResInternal getResAdv) : base(f, pref, getResAdv)
        {
            Format = FieldType.AliasRelation;
        }

        /// <summary>
        /// Attribution de la table cible
        /// </summary>
        /// <param name="f"></param>
        protected override void setTargetTab(Field f)
        {
            TargetTab = 0;
        }


    }
}