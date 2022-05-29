using Com.Eudonet.Internal;
using EudoQuery;
using System;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminTraceabilityRenderer : eAdminBlockRenderer
    {
        public eAdminTraceabilityRenderer(ePref pref, eAdminTableInfos tabInfos, String title)
            : base(pref, tabInfos, title, "", idBlock:"TraceabilityPart")
        {

        }

        public static eAdminTraceabilityRenderer CreateAdminTraceabilityRenderer(ePref pref, eAdminTableInfos tabInfos, String title)
        {
            eAdminTraceabilityRenderer features = new eAdminTraceabilityRenderer(pref, tabInfos, title);
            return features;
        }

        /// <summary>Construction du bloc Traçabilité</summary>
        /// <returns></returns>
        protected override bool Build()
        {
            base.Build();

            eAdminField btnSystemFields = new eAdminButtonField(eResApp.GetRes(Pref, 7360), "btnSystemFields", eResApp.GetRes(Pref, 7362), "nsAdmin.ToggleAdminFileProp();");
            btnSystemFields.Generate(_panelContent);

            if (_tabInfos.TabType != TableType.HISTO)
            {
                eAdminField btnOwnershipDelegation = new eAdminButtonField(eResApp.GetRes(Pref, 7361), "btnOwnershipDelegation", eResApp.GetRes(Pref, 7363), "nsAdmin.showBelongingPopup();");
                btnOwnershipDelegation.Generate(_panelContent);
            }


            if (_tabInfos.TabType != TableType.HISTO)
            {
                eRes resLib = new eRes(Pref, ((int)EudoQuery.TableType.HISTO).ToString());

                eAdminField checkbox = new eAdminCheckboxField(this._tabInfos.DescId, eResApp.GetRes(Pref, 7261), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.HISTORIC.GetHashCode(), eResApp.GetRes(Pref, 7262).Replace("<HISTOTAB>", resLib.GetRes((int)EudoQuery.TableType.HISTO)), _tabInfos.Historic, "chkDeletionHistory");
                checkbox.Generate(_panelContent);
            }

            return true;
        }
    }
}