using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{

    /// <summary>
    /// Bloc droit et comporportement conditionnel d'un signet
    /// </summary>
    public class eAdminBkmRightsAndRulesRenderer : eAdminRightsAndRulesRenderer
    {

        #region propriété spécififique

        /// <summary>
        /// Table parente du signet web
        /// </summary>
        private eAdminTableInfos parentInfos;


        /// <summary>
        /// Table parente du signet
        /// </summary>
        protected eAdminTableInfos ParentInfos
        {
            get
            {
                return parentInfos;
            }

            set
            {
                parentInfos = value;
            }
        }


        #endregion


        #region constructeur


        protected eAdminBkmRightsAndRulesRenderer(ePref pref, eAdminTableInfos tabInfos, eAdminTableInfos parentInfos)
            : base(pref, tabInfos)
        {

            //eResApp.GetRes(pref, 7389)
            ParentInfos = parentInfos;
        }


        /// <summary>
        /// Création du bloc admin "Comportement conditionnel"
        /// </summary>
        /// <param name="pref">préférence user en cours</param>
        /// <param name="tabInfos">information signet</param>       
        /// <param name="parentInfos">information table parente</param>
        /// <returns></returns>
        public static eAdminBkmRightsAndRulesRenderer CreateAdminRightsAndRulesBkmRenderer(ePref pref, eAdminTableInfos tabInfos, eAdminTableInfos parentInfos)
        {
            eAdminBkmRightsAndRulesRenderer rdr = null;


            if (tabInfos.DescId == (int)TableType.DOUBLONS) // le "tabInfos.TabType" de doublon n'est pas doublons ! On doit tester le descid :(
                rdr = new eAdminBkmDblRightsAndRulesRenderer(pref, tabInfos, parentInfos);
            else if (tabInfos.DescId == (tabInfos.DescId - tabInfos.DescId % 100) + (int)AllField.ATTACHMENT)
                rdr = new eAdminBkmPJRightsAndRulesRenderer(pref, tabInfos, parentInfos);
            else if (tabInfos.TabType == TableType.HISTO)
                rdr = new eAdminBkmHistoRightsAndRulesRenderer(pref, tabInfos, parentInfos);
            else
                rdr = new eAdminBkmRightsAndRulesRenderer(pref, tabInfos, parentInfos);



            rdr._tab = tabInfos.DescId;
            rdr._descid = tabInfos.DescId;

            return rdr;

        }


        #endregion


        #region Override spécifique eAdminRightsAndRulesRenderer

        /// <summary>
        /// Création du bouton des droits
        /// </summary>
        protected override void BuildAdminRights()
        {
            bool bIsSpecialBkm = _descid % 100 == 91 || _descid % 100 == 87;
            eAdminField button = new eAdminButtonField(eResApp.GetRes(Pref, 7406), "buttonAdminRights", onclick: "nsAdmin.confRights(" + _descid + (bIsSpecialBkm ? "" : "," + ParentInfos.DescId) + ")", readOnly: _readOnly);
            button.Generate(_panelContent);
        }


        /// <summary>
        /// Indicateur de possibilité de paramétrer des comportement conditionnels
        /// </summary>
        protected virtual bool DisplayConditionalBehaviors
        {
            get
            {
                // si le bkm est affiché depuis 300 via adresse, pas de condition
                if (_tab == 300 && _tabInfos.AdrJoin && !_tabInfos.InterPM)
                    return false;
                else if (_tabInfos.DescId == (parentInfos.DescId + (int)AllField.ATTACHMENT))
                    return true;
                else if (_tabInfos.DescId == (int)TableType.DOUBLONS)
                    return true;
                else if ((ParentInfos.DescId == 200 && _tabInfos.InterPP
                       || ParentInfos.DescId == 200 && _tabInfos.DescId == 400
                       || ParentInfos.DescId == 300 && _tabInfos.InterPM
                       || ParentInfos.DescId == 300 && _tab == 400 // #65808
                       || (ParentInfos.DescId == _tabInfos.InterEVTDescid && _tabInfos.InterEVT))
                    && (_tabInfos.TabType == TableType.PP
                        || _tabInfos.TabType == TableType.PM
                        || _tabInfos.TabType == TableType.EVENT
                        || _tabInfos.TabType == TableType.TEMPLATE
                        || _tabInfos.TabType == TableType.ADR))
                    return true;
                else
                    return false;
            }
        }




        /// <summary>
        /// Condition d'un bloc de comportemet conditionnel
        /// </summary>
        /// <param name="panelRulesSub"></param>
        /// <param name="cnd">Type de comportement conditionnel</param>
        /// <param name="resLabel">id de la ressource du label</param>
        /// <param name="resTooltip">id de la ressource du tooltip</param>
        protected virtual void AddConditionalBehavior(Panel panelRulesSub, TypeTraitConditionnal cnd, int resLabel, int resTooltip = 0)
        {

            string sLabel = String.Concat(eResApp.GetRes(Pref, resLabel), " ", GetRulesInfos(cnd));

            eAdminButtonField button = new eAdminButtonField(

                label: sLabel,
                idButton: string.Concat("buttonAdminConditions_", ((int)cnd).ToString()),
                onclick: string.Concat("nsAdmin.confConditions(", (int)cnd, ",", _tabInfos.DescId, ",", ParentInfos.DescId, ")"),
                tooltiptext: resTooltip > 0 ? "" : eResApp.GetRes(Pref, resTooltip),
                readOnly: _readOnly);

            button.Generate(panelRulesSub);
        }


        /// <summary>
        /// Ajoute les block disponibles suivant le type
        /// </summary>
        /// <param name="panelRulesSub"></param>
        protected virtual void AddBlock(Panel panelRulesSub)
        {

            // Visu signet - seulement pour les type main
            //  if (_tabInfos.EdnType == EdnType.FILE_MAIN && _tabInfos.TabType != TableType.ADR)
            AddConditionalBehavior(panelRulesSub, TypeTraitConditionnal.BkmView, 7395, 7640);

            //Ajout
            AddConditionalBehavior(panelRulesSub, TypeTraitConditionnal.BkmAdd, 7396);

            //Modification
            AddConditionalBehavior(panelRulesSub, TypeTraitConditionnal.BkmUpdate, 7397);

            //Suppression
            AddConditionalBehavior(panelRulesSub, TypeTraitConditionnal.BkmDelete, 7398);


            //Liste des comportements conditionnels
            string sLabel = String.Concat(eResApp.GetRes(Pref, 7412));
            eAdminButtonField button = new eAdminButtonField(sLabel, "buttonAdminConditionsList", onclick: "nsAdmin.listConditions(" + _tabInfos.DescId + "," + ParentInfos.DescId + ")", readOnly: _readOnly);
            button.Generate(panelRulesSub);
        }

        /// <summary>
        /// Comportement conditionnels -  pour signet web
        /// </summary>
        protected override void BuildConditionalBehaviors()
        {

            if (!DisplayConditionalBehaviors)
                return;


            Panel panelRules = new Panel();
            Panel panelRulesSub = new Panel();
            CreateCollapsibleMenu(out panelRules, out panelRulesSub, false, eResApp.GetRes(Pref, 7355), bRights: true);
            panelRules.CssClass = "btnLink";
            panelRules.ID = "adminRules";
            _panelContent.Controls.Add(panelRules);


            AddBlock(panelRulesSub);


            //Attachement - cas particulier
            if (_tabInfos.DescId % 100 == AllField.ATTACHMENT.GetHashCode())
            {
                //Condition de suppression automatiques des annexes
                string sLabel = eResApp.GetRes(Pref, 8565);
                eAdminButtonField button = new eAdminButtonField(sLabel, "buttonAdminPJDeleting", onclick: String.Concat("nsAdmin.confRGPDConditions(", (int)RGPDRuleType.PJDeletion, ");"), readOnly: _readOnly);
                button.Generate(panelRulesSub);
            }


        }






        #endregion


        #region Override 
        // <summary>Construction du bloc Droits et comportements conditionnels</summary>
        /// <returns></returns>
        protected override bool Build()
        {

            //Conteneur principal
            BuildMainContent();


            // administrer les droits
            BuildAdminRights();

            // Comportement conditionel
            BuildConditionalBehaviors();

            return true;


        }


        #endregion

    }
}