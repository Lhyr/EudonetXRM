using Com.Eudonet.Engine.Notif;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{

    /// <summary>
    /// Gestion du bloc "Droits, règles, comportement conditionnels et automatisme"
    ///  -> Pour mode "Onglet"
    /// </summary>
    public class eAdminRightsAndRulesRenderer : eAdminBlockRenderer
    {

        /// <summary>
        /// Descid de l'élément concerné (tab/field)
        /// </summary>
        protected Int32 _descid;

        /// <summary>
        /// BLock en lecture  seule (champ syteme, produit...)
        /// </summary>
        protected bool _readOnly = false;


        #region Constructeur

        /// <summary>
        /// Constructeur interne
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tabInfos"></param>
        /// <param name="idBlock"></param>
        protected eAdminRightsAndRulesRenderer(ePref pref, eAdminTableInfos tabInfos, string idBlock = "RulesPart")
            : base(pref, tabInfos, eResApp.GetRes(pref, 6812), "", idBlock: idBlock)
        {

        }


        /// <summary>
        /// Instanciation de l'objet
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tabInfos"></param>
        /// <returns></returns>
        public static eAdminRightsAndRulesRenderer CreateAdminRightsAndRulesRenderer(ePref pref, eAdminTableInfos tabInfos)
        {

            eAdminRightsAndRulesRenderer features = null;

            switch (tabInfos.TabType)
            {

                default:
                    features = new eAdminRightsAndRulesRenderer(pref, tabInfos);
                    break;
            }


            features._tab = tabInfos.DescId; // todo : vérifier pourquoi ce n'est pas fait dans le cas général
            features._descid = tabInfos.DescId; // le cas générale est la table
            return features;
        }


        #endregion

        #region Overide Renderer

        /// <summary>Construction du bloc Performances</summary>
        /// <returns></returns>
        protected override bool Build()
        {
            _readOnly = !eAdminTools.IsUserAllowedForProduct(this.Pref, this.Pref.User, _tabInfos.ProductID);

            //Construction du conteneur 
            BuildMainContent();


            #region Construction des Liens

            // administrer les droits
            BuildAdminRights();

            //Comportements conditionnels
            BuildConditionalBehaviors();

            //Condition de communication des fiches
            BuildCommunicationPart();

            //Automatismes
            BuildAutomatismPart();

            // administrer les règles
            BuildAdminRules();


            #endregion

            return true;
        }



        #endregion

        #region Construction des sous blocs

        /// <summary>
        /// Bloc admin rights
        /// </summary>
        protected virtual void BuildAdminRights()
        {
            eAdminField button = new eAdminButtonField(eResApp.GetRes(Pref, 7406), "buttonAdminRights", onclick: "nsAdmin.confRights(" + _descid + ")");
            button.Generate(_panelContent);

        }

        /// <summary>
        /// Gestion des règles
        /// </summary>
        protected virtual void BuildAdminRules()
        {
            eAdminButtonField button = new eAdminButtonField(eResApp.GetRes(Pref, 858), "buttonAdminRules", onclick: "nsAdmin.confRules(" + _tab + ")", readOnly: _readOnly);
            button.Generate(_panelContent);
        }


        /// <summary>
        /// Comportement conditionnels
        /// </summary>
        protected virtual void BuildConditionalBehaviors()
        {
            //TODO : dériver la classe
            if (_tabInfos.TabType == TableType.HISTO)
                return;

            Panel panelRules = new Panel();
            Panel panelRulesSub = new Panel();
            CreateCollapsibleMenu(out panelRules, out panelRulesSub, false, eResApp.GetRes(Pref, 7355), bRights: true);
            panelRules.CssClass = "btnLink";
            panelRules.ID = "adminRules";
            _panelContent.Controls.Add(panelRules);

            //Conditions de modification de la fiche en cours            
            string sLabel = String.Concat(eResApp.GetRes(Pref, 7353), " ", GetRulesInfos(TypeTraitConditionnal.Update));
            eAdminButtonField button = new eAdminButtonField(sLabel, "buttonAdminConditions" + (int)TypeTraitConditionnal.Update, onclick: "nsAdmin.confConditions(" + (int)TypeTraitConditionnal.Update + ")", tooltiptext: eResApp.GetRes(Pref, 7640), readOnly: _readOnly);
            button.Generate(panelRulesSub);


            //Condition de suppression de la fiche en cours
            sLabel = String.Concat(eResApp.GetRes(Pref, 7352), " ", GetRulesInfos(TypeTraitConditionnal.Delete));
            button = new eAdminButtonField(sLabel, "buttonAdminConditions" + (int)TypeTraitConditionnal.Delete, onclick: "nsAdmin.confConditions(" + (int)TypeTraitConditionnal.Delete + ")", tooltiptext: eResApp.GetRes(Pref, 7641), readOnly: _readOnly);
            button.Generate(panelRulesSub);


            //Choix conditionnel des pictogrammes et de couleurs
            sLabel = String.Concat(eResApp.GetRes(Pref, 7351), " ", GetRulesInfos(TypeTraitConditionnal.Color));
            button = new eAdminButtonField(sLabel, "buttonAdminConditions" + (int)TypeTraitConditionnal.Color, onclick: "nsAdmin.confConditions(" + (int)TypeTraitConditionnal.Color + ")", tooltiptext: eResApp.GetRes(Pref, 7644), readOnly: _readOnly);
            button.Generate(panelRulesSub);


            //Visu des entêtes de la fiche
            sLabel = String.Concat(eResApp.GetRes(Pref, 7350), " ", GetRulesInfos(TypeTraitConditionnal.Header_View));
            button = new eAdminButtonField(sLabel, "buttonAdminConditions" + (int)TypeTraitConditionnal.Header_View, onclick: "nsAdmin.confConditions(" + (int)TypeTraitConditionnal.Header_View + ")", tooltiptext: eResApp.GetRes(Pref, 7642), readOnly: _readOnly);
            button.Generate(panelRulesSub);


            //Condition de modification des entêtes de la fiche
            sLabel = String.Concat(eResApp.GetRes(Pref, 7349), " ", GetRulesInfos(TypeTraitConditionnal.Header_Update));
            button = new eAdminButtonField(sLabel, "buttonAdminConditions" + (int)TypeTraitConditionnal.Header_Update, onclick: "nsAdmin.confConditions(" + (int)TypeTraitConditionnal.Header_Update + ")", tooltiptext: eResApp.GetRes(Pref, 7643), readOnly: _readOnly);
            button.Generate(panelRulesSub);

            //Liste des comportements conditionnels
            sLabel = String.Concat(eResApp.GetRes(Pref, 7412));
            button = new eAdminButtonField(sLabel, "buttonAdminConditionsList", onclick: "nsAdmin.listConditions(" + _tab + "," + 0 + ")", readOnly: _readOnly);
            button.Generate(panelRulesSub);

        }


        /// <summary>
        /// Bloc automatisme
        /// </summary>
        protected virtual Panel BuildAutomatismPart()
        {
            // Pas de paramètrage si l'extension est désactivée
            if (!NotifConst.NotifEnabled(_ePref, _ePref.User, eModelTools.GetRootPhysicalDatasPath()))
                return new Panel();

            Panel panelRulesSecond = new Panel();
            Panel panelRulesSecondSub = new Panel();
            CreateCollapsibleMenu(out panelRulesSecond, out panelRulesSecondSub, false, eResApp.GetRes(Pref, 7344), bRights: true);
            panelRulesSecond.CssClass = "btnLink";
            panelRulesSecond.ID = "adminAutomatismes";
            _panelContent.Controls.Add(panelRulesSecond);

            //Ajouter une notification pour la table
            eAdminButtonField button = new eAdminButtonField(eResApp.GetRes(Pref, 7356), "buttonAddAdminAutomations", eResApp.GetRes(Pref, 7492), onclick: "nsAdmin.createNotif(" + _tab + ", 0, " + AutomationType.NOTIFICATION.GetHashCode() + ")");
            button.Generate(panelRulesSecondSub);

            //Liste des notifications pour toute la table
            button = new eAdminButtonField(eResApp.GetRes(Pref, 7485).Replace("<COUNT>", GetNotificationNumber(_tab, 0)), "btnListAutomation_" + _tab, eResApp.GetRes(Pref, 7493), onclick: "nsAdmin.confShowAutomationList(" + _tab + ", 0, " + AutomationType.NOTIFICATION.GetHashCode() + ")");
            button.Generate(panelRulesSecondSub);


            return panelRulesSecond;
        }

        /// <summary>
        /// Block Communication
        /// </summary>
        protected virtual void BuildCommunicationPart()
        {
            Panel panelRulesThird = new Panel();
            Panel panelRulesThirdSub = new Panel();
            CreateCollapsibleMenu(out panelRulesThird, out panelRulesThirdSub, false, eResApp.GetRes(Pref, 7347), bRights: true);
            panelRulesThird.CssClass = "btnLink";
            panelRulesThird.ID = "adminFileRules";
            _panelContent.Controls.Add(panelRulesThird);


            //Condition d'export
            string sLabel = String.Concat(eResApp.GetRes(Pref, 7367), " ", GetRulesInfos(TypeTraitConditionnal.Export));
            eAdminButtonField button = new eAdminButtonField(sLabel, "buttonAdminAutomations" + (int)TypeTraitConditionnal.Export, onclick: "nsAdmin.confConditions(" + (int)TypeTraitConditionnal.Export + ")", tooltiptext: eResApp.GetRes(Pref, 7645));
            button.Generate(panelRulesThirdSub);

            //Condition de publipostage
            sLabel = String.Concat(eResApp.GetRes(Pref, 7368), " ", GetRulesInfos(TypeTraitConditionnal.Merge));
            button = new eAdminButtonField(sLabel, "buttonAdminAutomations" + (int)TypeTraitConditionnal.Merge, onclick: "nsAdmin.confConditions(" + (int)TypeTraitConditionnal.Merge + ")", tooltiptext: eResApp.GetRes(Pref, 7646));
            button.Generate(panelRulesThirdSub);

            //Condition d'emailing
            sLabel = String.Concat(eResApp.GetRes(Pref, 7369), " ", GetRulesInfos(TypeTraitConditionnal.Mailing));
            button = new eAdminButtonField(sLabel, "buttonAdminAutomations" + (int)TypeTraitConditionnal.Mailing, onclick: "nsAdmin.confConditions(" + (int)TypeTraitConditionnal.Mailing + ")", tooltiptext: eResApp.GetRes(Pref, 7647));
            button.Generate(panelRulesThirdSub);


        }


        #endregion



        /// <summary>
        /// Retourne le bloc d'info sur la règles
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        protected string GetRulesInfos(TypeTraitConditionnal t)
        {
            eRules er = eRules.GetRules(t, _descid, Pref);

            string sTitle = er.GetShortDesc();
            string sSpan = "";

            sSpan = String.Concat(" <span ednnb='", er.AllRules.Count, "' id='RULES_INFO_", (int)t, "' class='spanAdminRulesInfo' title='", sTitle.Replace("'", "\\'"), "' > (", er.AllRules.Count, ") </span>");

            return sSpan;
        }
    }



    /// <summary>
    /// Bloc menu des signets web
    /// </summary>
    public class eAdminRightsAndRulesBkmWebRenderer : eAdminBkmRightsAndRulesRenderer
    {


        #region Override

        /// <summary>
        /// Comportement conditionnels - uniquement visu pour signet web
        /// </summary>
        protected override void BuildConditionalBehaviors()
        {


            Panel panelRules = new Panel();
            Panel panelRulesSub = new Panel();
            CreateCollapsibleMenu(out panelRules, out panelRulesSub, false, eResApp.GetRes(Pref, 7355), bRights: true);
            panelRules.CssClass = "btnLink";
            panelRules.ID = "adminRules";
            _panelContent.Controls.Add(panelRules);

            //Conditions de visu en signet de la fiche en cours            
            string sLabel = String.Concat(eResApp.GetRes(Pref, 7395), " ", GetRulesInfos(TypeTraitConditionnal.BkmView));
            eAdminButtonField button = new eAdminButtonField(sLabel, "buttonAdminConditions" + (int)TypeTraitConditionnal.BkmView, onclick: String.Concat("nsAdmin.confConditions(", (int)TypeTraitConditionnal.BkmView, ",", _tab, ",", ParentInfos.DescId, " )"), tooltiptext: eResApp.GetRes(Pref, 7640));
            button.Generate(panelRulesSub);


            //Liste des comportements conditionnels
            sLabel = String.Concat(eResApp.GetRes(Pref, 7412));
            button = new eAdminButtonField(sLabel, "buttonAdminConditionsList", onclick: "nsAdmin.listConditions(" + _tab + "," + ParentInfos.DescId + ")");
            button.Generate(panelRulesSub);

        }





        #endregion


        #region Constructeur


        private eAdminRightsAndRulesBkmWebRenderer(ePref pref, eAdminTableInfos tabInfos, eAdminTableInfos parentInfos)
            : base(pref, tabInfos, parentInfos)
        {

        }



        /// <summary>
        /// Droit, règle et comportemenent conditionnels
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tabInfos"></param>
        /// <param name="parentInfos"></param>
        /// <returns></returns>
        public static eAdminRightsAndRulesBkmWebRenderer CreateAdminRightsAndRulesBkmWebRenderer(ePref pref, eAdminTableInfos tabInfos, eAdminTableInfos parentInfos)
        {
            eAdminRightsAndRulesBkmWebRenderer rdr = new eAdminRightsAndRulesBkmWebRenderer(pref, tabInfos, parentInfos);
            rdr._tab = tabInfos.DescId;
            rdr._descid = tabInfos.DescId;

            return rdr;

        }

        #endregion
    }

    /// <summary>
    /// Classe de rendu du bloc "Droits, règles, comportements conditionnels et automatismes" pour un signet relationnel
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eda.eAdminRightsAndRulesRenderer" />
    public class eAdminRightsAndRulesRelationTabRenderer : eAdminRightsAndRulesRenderer
    {



        #region Constructeur


        private eAdminRightsAndRulesRelationTabRenderer(ePref pref, eAdminTableInfos tabInfos)
            : base(pref, tabInfos)
        {

        }



        /// <summary>
        /// Droit, règle et comportemenent conditionnels
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="tabInfos">The tab infos.</param>
        /// <returns></returns>
        public static eAdminRightsAndRulesRelationTabRenderer CreateAdminRightsAndRulesRelationTabRenderer(ePref pref, eAdminTableInfos tabInfos)
        {
            eAdminRightsAndRulesRelationTabRenderer rdr = new eAdminRightsAndRulesRelationTabRenderer(pref, tabInfos);
            rdr._tab = tabInfos.DescId;
            rdr._descid = tabInfos.DescId;

            return rdr;

        }

        #endregion

        /// <summary>
        /// Bloc automatisme
        /// </summary>
        /// <returns></returns>
        protected override Panel BuildAutomatismPart()
        {
            return new Panel();
        }

        /// <summary>
        /// Gestion des règles
        /// </summary>
        protected override void BuildAdminRules()
        {


        }

    }


}