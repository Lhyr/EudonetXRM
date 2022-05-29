using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminMapRenderer : eAdminBlockRenderer
    {
        public eAdminMapRenderer(ePref pref, eAdminTableInfos tabInfos, String title, String titleInfo)
            : base(pref, tabInfos, title, titleInfo, idBlock: "MapPart")
        {

        }

        public static eAdminMapRenderer CreateAdminMapRenderer(ePref pref, eAdminTableInfos tabInfos, String title, String titleInfo)
        {
            eAdminMapRenderer features = new eAdminMapRenderer(pref, tabInfos, title, titleInfo);
            return features;
        }

        protected override bool Build()
        {
            base.Build();

            #region Infobulle de position

            if (eExtension.IsReady(Pref, ExtensionCode.CARTOGRAPHY))
            {
                eAdminField button = new eAdminButtonField(eResApp.GetRes(Pref, 7106), "buttonAdminCarto", tooltiptext: eResApp.GetRes(Pref, 7107), onclick: "nsAdmin.confMapTooltip()");
                button.Generate(_panelContent);
            }

            #endregion

            #region Recherche d'adresses prédictive
            // administrer la recherche d'adresse prédictive, sauf ceux qui sont utilisés par Sirene
            IEnumerable<KeyValuePair<int, int>> autocompleteAddressEnabledFields = _tabInfos.GetAutocompleteAddressFields(Pref).Where(
                    mp => (
                        EudoQuery.Field.AutoCompletionEnabledStatic((EudoQuery.AutoCompletion)mp.Value) == true &&
                        !eSireneMapping.GetSireneEnabledFields(Pref, _tabInfos.DescId).Contains(mp.Key.ToString())
                    )
                );

            string sLib = eResApp.GetRes(Pref, 7264);

            if (autocompleteAddressEnabledFields.Count() > 0)
                sLib = String.Concat(sLib, " ", "(", autocompleteAddressEnabledFields.Count(), ")");

            eAdminButtonField btnAutoComplete = new eAdminButtonField(sLib, "eAdmnBtnAutoCptl", eResApp.GetRes(Pref, 7025), "nsAdmin.confAutocompleteAddress();");

            btnAutoComplete.Generate(_panelContent);
            #endregion

            return true;
        }
    }
}