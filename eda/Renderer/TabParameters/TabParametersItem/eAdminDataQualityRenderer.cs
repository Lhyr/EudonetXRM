using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Gestion du bloc "Qualité des données et RGPD"
    ///  -> Pour mode "Onglet"
    /// </summary>
    public class eAdminDataQualityRenderer : eAdminBlockRenderer
    {
        /// <summary>
        /// BLock en lecture  seule (champ syteme, produit...)
        /// </summary>
        protected bool _readOnly = false;
        /// <summary>
        /// The person category field
        /// </summary>
        string _personCategoryField = "0";

        #region Constructeur

        /// <summary>
        /// Constructeur interne
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tabInfos"></param>
        /// <param name="idBlock"></param>
        protected eAdminDataQualityRenderer(ePref pref, eAdminTableInfos tabInfos, string idBlock = "DataQuality")
            : base(pref, tabInfos, eResApp.GetRes(pref, 8284), "", idBlock: idBlock)
        {

        }

        /// <summary>
        /// Instanciation de l'objet
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tabInfos"></param>
        /// <returns></returns>
        public static eAdminDataQualityRenderer CreateAdminDataQualityRenderer(ePref pref, eAdminTableInfos tabInfos)
        {
            eAdminDataQualityRenderer features = null;
            features = new eAdminDataQualityRenderer(pref, tabInfos);
            features._tab = tabInfos.DescId; // todo : vérifier pourquoi ce n'est pas fait dans le cas général
            //features._descid = tabInfos.DescId; // le cas générale est la table            
            return features;

        }

        #endregion

        #region Overide Renderer

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (!base.Init())
                return false;

            eudoDAL dal = eLibTools.GetEudoDAL(this.Pref);
            try
            {
                dal.OpenDatabase();

                _personCategoryField = DescAdvDataSet.LoadAndGetAdvParam(dal, _tab, DESCADV_PARAMETER.RGPD_PERSON_CATEGORY, "0");
            }
            finally
            {
                dal.CloseDatabase();
            }

            return true;
        }

        /// <summary>Construction du bloc Performances</summary>
        /// <returns></returns>
        protected override bool Build()
        {
            _readOnly = !eAdminTools.IsUserAllowedForProduct(this.Pref, this.Pref.User, _tabInfos.ProductID);

            //Construction du conteneur 
            BuildMainContent();

            #region Construction des Liens

            // Liens RGPD
            BuildRGPDLinks();

            #endregion

            return true;
        }

        #endregion


        #region Construction des sous blocs

        /// <summary>
        /// Liens RGPD
        /// </summary>
        private void BuildRGPDLinks()
        {
            if (eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminRGPD))
            {
                string sLabel;
                eAdminField field;

                // Condition d'archivage de la fiche
                if (_tabInfos.HistoDescId != 0)
                {
                    sLabel = eResApp.GetRes(Pref, 8351);
                    field = new eAdminButtonField(sLabel, "buttonAdminArchiving", onclick: String.Concat("nsAdmin.confRGPDConditions(", (int)RGPDRuleType.Archiving, ");"), readOnly: _readOnly);
                    field.Generate(_panelContent);
                }

                // Condition de suppression de la fiche
                sLabel = eResApp.GetRes(Pref, 8352);
                field = new eAdminButtonField(sLabel, "buttonAdminDeleting", onclick: String.Concat("nsAdmin.confRGPDConditions(", (int)RGPDRuleType.Deleting, ");"), readOnly: _readOnly);
                field.Generate(_panelContent);

                // Condition de pseudonymisation de la fiche
                sLabel = eResApp.GetRes(Pref, 8513);
                field = new eAdminButtonField(sLabel, "buttonAdminPseudonym", onclick: String.Concat("nsAdmin.confRGPDConditions(", (int)RGPDRuleType.Pseudonym, ");"), readOnly: _readOnly);
                field.Generate(_panelContent);

                // Catégorie de personne
                if (_tab == (int)TableType.PP)
                {
                    List<eFieldLiteWithLib> fields = RetrieveFields.GetDefault(_ePref)
                    .AddOnlyThisTabs(new int[] { _tab })
                    .AddOnlyThisFormats(new FieldFormat[] { FieldFormat.TYP_CHAR })
                    .AddOnlyThisPopupType(new PopupType[] { PopupType.DATA, PopupType.FREE, PopupType.ONLY })
                    .ResultFieldsInfo(eFieldLiteWithLib.Factory(_ePref)).ToList();
                    List<ListItem> li = fields.Where(f => f.PopupDescId == f.Descid || f.PopupDescId % 100 > 1)
                                        .Select(f => new ListItem(f.Libelle, f.Descid.ToString())).ToList();
                    li.Insert(0, new ListItem(eResApp.GetRes(_ePref, 375), "0"));
                    sLabel = eResApp.GetRes(_ePref, 8726);
                    field = new eAdminDropdownField(_tab, sLabel, eAdminUpdateProperty.CATEGORY.DESCADV, (int)DESCADV_PARAMETER.RGPD_PERSON_CATEGORY, li.ToArray(), value: _personCategoryField,
                        renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE);
                    field.Generate(_panelContent);
                }

            }
        }

        #endregion
    }
}