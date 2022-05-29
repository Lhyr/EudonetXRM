using System;
using System.Collections.Generic;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Newtonsoft.Json;
using EudoEnum = Com.Eudonet.Common.Enumerations;


namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Gestionnaire des demandes de maj de confiadv pour les type contenu JSON en admin
    /// </summary>
    public class eAdminConfigAdvJSONManager : eAdminManager
    {
        /// <summary>
        /// traitement de la demande
        /// </summary>
        protected override void ProcessManager()
        {


            eAdminCapsule<eAdminCAdvUpdateProperty> caps = null;
            try
            {
                caps = eAdminTools.GetAdminCapsule<eAdminCapsule<eAdminCAdvUpdateProperty>, eAdminCAdvUpdateProperty>(_context.Request.InputStream);
            }
            catch (Exception e)
            {
                // TODO: gestion d'erreur
                throw e;
            }

            // Parcours des propriétés CONFIG
            List<SetCAdvParam> listConfig = new List<SetCAdvParam>();
            foreach (eAdminCAdvUpdateProperty pty in caps.ListProperties)
            {
                if (pty.Category != eAdminUpdateProperty.CATEGORY.CONFIGADV.GetHashCode())
                    continue;

                EudoEnum.CONFIGADV param = EudoEnum.CONFIGADV.UNDEFINED;
                eLibConst.CONFIGADV_CATEGORY cat = eLibConst.CONFIGADV_CATEGORY.UNDEFINED;

                if (!Enum.TryParse<EudoEnum.CONFIGADV>(pty.Property.ToString(), out param) || param == EudoEnum.CONFIGADV.UNDEFINED)
                    continue;
                if (!Enum.TryParse<eLibConst.CONFIGADV_CATEGORY>(pty.CAdvCategory.ToString(), out cat) || cat == eLibConst.CONFIGADV_CATEGORY.UNDEFINED)
                    continue;

                listConfig.Add(new SetCAdvParam(param, pty.Value, cat));

            }

            if (listConfig.Count > 0)
            {
                eAdminConfigAdv adminCfgAdv = new eAdminConfigAdv(_pref);
                eAdminResult res = adminCfgAdv.SetParam(listConfig);

                if (!res.Success)
                {
                    LaunchError(
                        eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            res.UserErrorMessage,
                            "",
                            res.DebugErrorMessage
                        //adminCfgAdv.Exception.Message,
                        //adminCfgAdv.Exception.InnerException != null ?
                        //    String.Concat(adminCfgAdv.Exception.InnerException.Message, Environment.NewLine, adminCfgAdv.Exception.InnerException.StackTrace)
                        //    : String.Concat(adminCfgAdv.Exception.StackTrace)
                        )
                    );
                }
                else
                {
                    //Rechargement des préférences
                    _pref.LoadConfigAdv();
                }

                RenderResult(RequestContentType.TEXT, delegate ()
                {
                    return JsonConvert.SerializeObject(res);
                });
            }


        }

    }
}