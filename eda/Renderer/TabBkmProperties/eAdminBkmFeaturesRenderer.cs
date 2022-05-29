using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe de rendu des "Caractéristiques" de la partie "Paramètres du signet"
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eda.eAdminBlockRenderer" />
    public class eAdminBkmFeaturesRenderer : eAdminBlockRenderer
    {
        private eAdminTableInfos _bkmTabInfos;
        private int _bkmTab;

        private eAdminBkmFeaturesRenderer(ePref pref, eAdminTableInfos tabInfos, eAdminTableInfos bkmTabInfos, string title, string titleInfo, string tooltip)
            : base(pref, tabInfos, title, titleInfo)
        {
            _tab = tabInfos.DescId;
            this.BlockTitleTooltip = tooltip;

            _bkmTabInfos = bkmTabInfos;
            _bkmTab = _bkmTabInfos.DescId;
        }

        /// <summary>
        /// Retourne un objet eAdminBkmFeaturesRenderer
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="tabInfos">Infos sur la table</param>
        /// <param name="bkmTabInfos">Infos sur la table du signet</param>
        /// <param name="title">Titre</param>
        /// <param name="titleInfo">Sous-titre</param>
        /// <param name="tooltip">Infobulle</param>
        /// <returns></returns>
        public static eAdminBkmFeaturesRenderer CreateAdminBkmFeaturesRenderer(ePref pref, eAdminTableInfos tabInfos, eAdminTableInfos bkmTabInfos, string title, string titleInfo, string tooltip)
        {
            eAdminBkmFeaturesRenderer features = new eAdminBkmFeaturesRenderer(pref, tabInfos, bkmTabInfos, title, titleInfo, tooltip);
            return features;
        }

        /// <summary>Construction du bloc Caractéristiques</summary>
        /// <returns></returns>
        protected override bool Build()
        {
            this.OpenedBlock = true;

            base.Build();

            eAdminTabLinkField field = new eAdminTabLinkField(_bkmTabInfos, eResApp.GetRes(Pref, 7390).Replace("<TAB>", _bkmTabInfos.TableLabel));
            field.Generate(_panelContent);

            //doublon seulement pour les fichiers principaux
            if (_tabInfos.EdnType == EdnType.FILE_MAIN && _bkmTabInfos.DescId == (int)TableType.DOUBLONS)
            {
                eAdminButtonField filtBtn = new eAdminButtonField(eResApp.GetRes(Pref, 7053), "eAdmnBtnDup", eResApp.GetRes(Pref, 8161), String.Concat("nsAdminFile.openSpecFilter(", (int)TypeFilter.DBL, ")"));
                filtBtn.Generate(_panelContent);
            }

            //Historique 
            if (_bkmTabInfos.TabType == TableType.HISTO)
            {
                eAdminCheckboxField histoActiveBtn = new eAdminCheckboxField(_bkmTabInfos.DescId, eResApp.GetRes(Pref, 265), eAdminUpdateProperty.CATEGORY.DESC, (int)eLibConst.DESC.ACTIVETAB, value: _bkmTabInfos.ActiveTab);
                histoActiveBtn.Generate(_panelContent);

                eRes resLib = new eRes(Pref, ((int)EudoQuery.TableType.HISTO).ToString());
                eAdminField histoDelBtn = new eAdminCheckboxField(_tabInfos.DescId, eResApp.GetRes(Pref, 7261), eAdminUpdateProperty.CATEGORY.DESC, (int)eLibConst.DESC.HISTORIC, eResApp.GetRes(Pref, 7262).Replace("<HISTOTAB>", resLib.GetRes((int)EudoQuery.TableType.HISTO)), _tabInfos.Historic, "chkDeletionHistory");
                histoDelBtn.Generate(_panelContent);
            }

            #region Compteur personnalisé
            if (_bkmTab % 100 != AllField.ATTACHMENT.GetHashCode())
            {
                // Sélection du filtre pour le compteur
                DESCADV_PARAMETER descadvParam = DESCADV_PARAMETER.BKMCOUNTFILTER100;
                int value = _bkmTabInfos.BkmCountFilter_100;

                if (_tabInfos.DescId == TableType.PP.GetHashCode())
                {
                    descadvParam = DESCADV_PARAMETER.BKMCOUNTFILTER200;
                    value = _bkmTabInfos.BkmCountFilter_200;
                }
                else if (_tabInfos.DescId == TableType.PM.GetHashCode())
                {
                    descadvParam = DESCADV_PARAMETER.BKMCOUNTFILTER300;
                    value = _bkmTabInfos.BkmCountFilter_300;
                }

                String btnLabel = eResApp.GetRes(Pref, 8163);
                if (value > 0)
                    btnLabel = String.Concat(btnLabel, " (1)");

                eAdminButtonField filterBtn = new eAdminButtonField(btnLabel, "btnCountFilter", eResApp.GetRes(Pref, 8162), String.Concat("nsAdmin.openBkmCountFilterModal(this, ", _bkmTab, ")"));
                filterBtn.Generate(_panelContent);
                filterBtn.SetControlAttribute("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.DESCADV.GetHashCode(), "|", descadvParam.GetHashCode()));
                filterBtn.SetControlAttribute("value", value.ToString());
            }

            #endregion
            return true;
        }
    }
}