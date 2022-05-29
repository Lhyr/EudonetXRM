using System.Collections.Generic;
using Com.Eudonet.Internal;
using Newtonsoft.Json;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminExtensionCarto : eAdminExtensionFromStore
    {
        [JsonConstructor]
        public eAdminExtensionCarto(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }

        public eAdminExtensionCarto(ePref pref) : base(pref)
        {
            _module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_CARTO;
        }

        protected override void Init()
        {
            this._feature = eConst.XrmExtension.Cartographie;
            base.Init();
        }

        /// <summary>
        /// Active l'extension
        /// </summary>
        /// <param name="bEnable"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public override bool EnableProcess(bool bEnable, out string sError)
        {
            List<eExtension> a = eExtension.GetExtensionsByCode(_pref, Infos.ExtensionNativeId);
            eExtension newExt;
            if (a.Count == 0)
            {
                newExt = eExtension.GetNewExtension(Infos.ExtensionNativeId);
            }
            else
            {
                newExt = a.Find(zz => zz.UserId == 0);
                if (newExt == null)
                    newExt = eExtension.GetNewExtension(Infos.ExtensionNativeId);
            }
            if (bEnable)
            {
                newExt.Status = EXTENSION_STATUS.STATUS_READY;

                string rootPhysicalDatasPath = eModelTools.GetRootPhysicalDatasPath();
                eExtension.UpdateExtension(newExt, _pref, _pref.User, rootPhysicalDatasPath, _pref.ModeDebug);
            }

            sError = "";
            return true;
        }



    }

}