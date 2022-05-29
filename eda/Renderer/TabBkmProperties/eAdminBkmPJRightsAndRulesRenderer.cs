using Com.Eudonet.Internal.eda;
using EudoQuery;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{

    /// <summary>
    /// Comportement conditionnel signet PH
    /// </summary>
    public class eAdminBkmPJRightsAndRulesRenderer : eAdminBkmRightsAndRulesRenderer
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tabInfos"></param>
        /// <param name="parentInfos"></param>
        internal eAdminBkmPJRightsAndRulesRenderer(ePref pref, eAdminTableInfos tabInfos, eAdminTableInfos parentInfos) : base(pref, tabInfos, parentInfos)
        {
        }

        /// <summary>
        /// Signet pj affichable
        /// </summary>
        protected override bool DisplayConditionalBehaviors
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Liste des bloc pour les doublons
        /// </summary>
        /// <param name="panelRulesSub"></param>
        protected override void AddBlock(Panel panelRulesSub)
        {

            //Condition de visu uniquement dispo depus PP/PM
            //if (ParentInfos.TabType != TableType.EVENT)
            AddConditionalBehavior(panelRulesSub, TypeTraitConditionnal.BkmView, 7395, 7640);

            //Modification
            AddConditionalBehavior(panelRulesSub, TypeTraitConditionnal.BkmUpdate, 7397);

            //Suppression
            AddConditionalBehavior(panelRulesSub, TypeTraitConditionnal.BkmDelete, 7398);

            /*
            //Liste des comportements conditionnels - Non pertinent sur les pj, le systeme de filtre rend peut lisible les différents
            // type
            string sLabel = String.Concat(eResApp.GetRes(Pref, 7412));
            eAdminButtonField button = new eAdminButtonField(sLabel, "buttonAdminConditionsList", onclick: "nsAdmin.listConditions(" + _tabInfos.DescId + "," + ParentInfos.DescId + ")", readOnly: _readOnly);
            button.Generate(panelRulesSub);
            */
        }

    }
}