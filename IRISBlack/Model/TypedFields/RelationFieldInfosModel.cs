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
    /// retourne les champs de type catalogue
    /// </summary>
    public class RelationFieldInfos : FldTypedInfosModel, IRelation
    {
        /// <summary>
        /// Table vers laquelle pointe la relation
        /// </summary>
        public int TargetTab { get; set; }
        /// <summary>
        /// DescID de la rubrique correspondante
        /// </summary>
        public int PopupDescId { get; set; }        
        /// <summary>
        /// Type de catalogue correspondant (SPECIAL)
        /// </summary>
        public int PopupType { get; set; }                
        /// <summary>
        /// Libellé de la Table vers laquelle pointe la relation
        /// </summary>
        public string TargetTabLabel { get; set; }

        /// <summary>
        /// Indique si une minifiche est disponible
        /// </summary>
        public bool IsMiniFileEnabled { get; set; }

        public List<eMiniFileParam> LiParam { get; set; }

        /// <summary>
        /// Constructeur pour les relations.
        /// ⚠ Ce constructeur appelle un méthode virtuelle (setTargetTab).
        /// </summary>
        /// <param name="f"></param>
        /// <param name="pref"></param>
        /// <param name="getResAdv"></param>
        internal RelationFieldInfos(Field f, ePref pref, eResInternal getResAdv) : base(f)
        {
            Format = FieldType.Relation;
            setTargetTab(f);

            TargetTabLabel = getResAdv.GetRes(TargetTab, noFoundDefaultRes: this.Label);
            if (TargetTab == (int)TableType.PP)
                IsMiniFileEnabled = true;
            else
            {
                LiParam = eMiniFileParam.GetParams(pref, MiniFileType.File, TargetTab);
                IsMiniFileEnabled = LiParam.Count > 0;
            }

            PopupDescId = f.PopupDescId > 0 ? f.PopupDescId : f.Descid;
            PopupType = (int)f.Popup; // SPECIAL
        }

        /// <summary>
        /// Affecte la table de liaison
        /// </summary>
        /// <param name="f"></param>
        protected virtual void setTargetTab(Field f)
        {
            TargetTab = eLibTools.GetTabFromDescId(f.PopupDescId);

        }
    }
}