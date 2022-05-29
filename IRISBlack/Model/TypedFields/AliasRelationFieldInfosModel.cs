using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type relation d'en tete (liaison haute dans le corps de la page)
    /// </summary>
    public class AliasRelationFieldInfos : RelationFieldInfos, IRelation
    {

        internal AliasRelationFieldInfos(Field f, ePref pref, eResInternal oRes) : base(f, pref, oRes)
        {
            Format = FieldType.AliasRelation;
        }

        /// <summary>
        /// Attribution de la table cible
        /// </summary>
        /// <param name="f"></param>
        protected override void setTargetTab(Field f)
        {
            TargetTab = f.RelationSource;
        }

    }
}