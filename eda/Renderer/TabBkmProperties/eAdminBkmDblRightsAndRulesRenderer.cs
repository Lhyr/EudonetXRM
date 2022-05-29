using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{


    /// <summary>
    /// Propriété des signets doublons
    /// </summary>
    public class eAdminBkmDblRightsAndRulesRenderer : eAdminBkmRightsAndRulesRenderer
    {
        /// <summary>
        /// Constructeur par défuat
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tabInfos"></param>
        /// <param name="parentInfos"></param>
        internal eAdminBkmDblRightsAndRulesRenderer(ePref pref, eAdminTableInfos tabInfos, eAdminTableInfos parentInfos) : base(pref, tabInfos, parentInfos)
        {
        }

        /// <summary>
        /// Doublon seulement pour type principaux
        /// </summary>
        protected override bool DisplayConditionalBehaviors
        {
            get
            {
                return (ParentInfos.EdnType == EdnType.FILE_MAIN && ParentInfos.TabType != TableType.ADR);
            }
        }

        /// <summary>
        /// Pas de comportement conditionnel pour les doublons
        /// </summary>
        protected override void BuildConditionalBehaviors()
        {
            //Du fait du mode stockage, les  comportement conditionnels ne sont dispo pour les signets que depuis PP/PM 
            // cf [desc].[BkmViewPermId_XXX] ... Seul 200/300 sont différentiant, pour tous les autres EVENT, la liaison est la même
            if (ParentInfos != null && (

                (ParentInfos.TabType == TableType.PP)
                ||
                (ParentInfos.TabType == TableType.PM)

                ))
            {
                base.BuildConditionalBehaviors();
            }
        }

        /// <summary>
        /// Liste des bloc pour les doublons
        /// </summary>
        /// <param name="panelRulesSub"></param>
        protected override void AddBlock(Panel panelRulesSub)
        {
            // Visu signet - seulement pour les type main, pour les doublon, voir BuildConditionalBehaviors
            AddConditionalBehavior(panelRulesSub, TypeTraitConditionnal.BkmView, 7395, 7640);

            /* le doublon étant un "pseudo" template, représenantat */
            
               

            //Liste des comportements conditionnels - Non pertinent sur les pj, le systeme de filtre rend peut lisible les différents
            // type
        }

        /// <summary>
        /// Création du bouton des droits
        /// </summary>
        protected override void BuildAdminRights()
        {
            eAdminField button = new eAdminButtonField(eResApp.GetRes(Pref, 7406), "buttonAdminRights", onclick: "nsAdmin.confRights(" + (int)TableType.DOUBLONS + ")", readOnly: _readOnly);
            button.Generate(_panelContent);
        }

    }
}