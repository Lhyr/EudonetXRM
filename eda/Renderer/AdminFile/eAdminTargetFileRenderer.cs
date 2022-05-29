using System.Collections.Generic;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminTargetFileRenderer : eAdminTemplateFileRenderer
    {

        public eAdminTargetFileRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos)
        {
            Dictionary<int, int> alreadyMapped = new eAdminTableInfos(pref, tabInfos.DescId).GetExtendedTargetMappingDescId(pref);

            SpecialFields.UnionWith(alreadyMapped.Keys);
        }


    }
}