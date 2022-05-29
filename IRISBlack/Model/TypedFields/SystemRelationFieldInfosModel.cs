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
    public class SystemRelationFieldInfos : FldTypedInfosModel, IRelation
    {
        public int TargetTab { get; set; }

        /// <summary>
        /// Libellé de la Table vers laquelle pointe la relation
        /// </summary>
        public string TargetTabLabel { get; set; }

        internal SystemRelationFieldInfos(Field f, ePref pref, eResInternal getResAdv) : base(f)
        {
            Format = FieldType.AliasRelation;
            //TargetTab = eLibTools.GetTabFromDescId(f.Descid);
            //eResInternal res = new eResInternal(pref, TargetTab.ToString());
            TargetTabLabel = getResAdv.GetRes(TargetTab, noFoundDefaultRes: this.Label);

        }

    }
}