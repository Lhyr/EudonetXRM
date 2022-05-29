using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminRightsListRenderer : eListMainRenderer
    {

        public static eAdminRightsListRenderer GetRightsList(ePref pref, Int32 nTab = 0, ProcessRightTypes type = ProcessRightTypes.RIGHT_TYPE_ALL, int level = 0, String groupsAndUsers = "")
        {
            eAdminRightsListRenderer myRenderer = new eAdminRightsListRenderer(pref, nTab, type, level, groupsAndUsers);
            myRenderer._tab = nTab;

            return myRenderer;
        }


        protected eAdminRightsListRenderer(ePref pref, Int32 nTab = 0, ProcessRightTypes type = ProcessRightTypes.RIGHT_TYPE_ALL, int level = 0, String groupsAndUsers = "")
            : base(pref)
        {
            _rType = RENDERERTYPE.TreatmentRights;
        }


        protected override void GenerateList()
        {
            //_list = eListFactory.CreateFilterList(Pref, _tab, _page, FilterType);
        }
    }
}