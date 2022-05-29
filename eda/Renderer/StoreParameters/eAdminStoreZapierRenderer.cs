using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using System.Xml;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// administration de la vcard
    /// </summary>
    public class eAdminStoreZapierRenderer : eAdminStoreFileRenderer
    {
        private Dictionary<Int32, string> dicFields;
        private Dictionary<string, int> dicMappings;
        private bool eudoDropCreatePmAuthorized = false;


        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminStoreZapierRenderer(ePref pref, eAdminExtension extension)
            : base(pref, extension)
        {

            
        }
        /// <summary>
        /// statique qui crée l'écran
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static eAdminStoreZapierRenderer CreateAdminStoreZapierRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminStoreZapierRenderer rdr = new eAdminStoreZapierRenderer(pref, ext);

            rdr.Generate();
            return rdr;
        }
 

        /// <summary>
        /// écran de paramétrage
        /// </summary>
        protected override void CreateSettingsPanel()
        {
            base.CreateSettingsPanel();
      
            List<Tuple<string, string, eAdminField>> lstInfos = new List<Tuple<string, string, eAdminField>>();

            lstInfos.Add(new Tuple<string, string, eAdminField>(eResApp.GetRes(_ePref, 2312), _eRegisteredExt?.ActivationKey ?? "", null));
            lstInfos.Add(new Tuple<string, string, eAdminField>(eResApp.GetRes(_ePref, 2313), _eRegisteredExt?.DateEnabled.ToString("yyyy/MM/dd HH:mm") ?? "", null));

            ExtensionParametersContainer.Controls.Add(GetInfosSection("zappierInfos", eResApp.GetRes(_ePref, 5080), "", lstInfos));
        }
    }

}