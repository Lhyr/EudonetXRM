using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using System;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminFieldTraceabilityRenderer : eAdminBlockRenderer
    {
        private Int32 _descid;
        private eAdminFieldInfos _field;
        private eAdminFieldTraceabilityRenderer(ePref pref, eAdminFieldInfos field)
            : base(pref, null, eResApp.GetRes(pref, 6915), idBlock: "blockTrace") 
        {
            _descid = field.DescId;
            _field = field;
        }

        public static eAdminFieldTraceabilityRenderer CreateAdminFieldCatalogRenderer(ePref pref, eAdminFieldInfos field)
        {
            eAdminFieldTraceabilityRenderer features = new eAdminFieldTraceabilityRenderer(pref, field);
            return features;
        }


        /// <summary>Construction du bloc "Traçabilité"</summary>
        /// <returns></returns>
        protected override bool Build()
        {
            base.Build();

            eAdminField adminField;

            // Historique
            adminField = new eAdminCheckboxField(_descid, eResApp.GetRes(Pref, 816), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.HISTORIC.GetHashCode(), value: _field.Historic);
            adminField.Generate(_panelContent);

            
            return true;
        }



    }
}