using System;
using EudoQuery;
using System.Collections.Generic;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.Sms;
using Newtonsoft.Json;
using Com.Eudonet.Internal.Payment;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminConfigManager
    /// </summary>
    public class eAdminConfigManager : eAdminManager
    {

        // Vérification du fichier à Uploader
        protected override void ProcessManager()
        {
            eAdminCapsule<eAdminUpdateProperty> caps = null;
            try
            {
                caps = eAdminTools.GetAdminCapsule<eAdminCapsule<eAdminUpdateProperty>, eAdminUpdateProperty>(_context.Request.InputStream);
            }
            catch (Exception e)
            {
                // TODO: gestion d'erreur
                throw e;
            }

            // Parcours des propriétés CONFIG
            List<SetParam<eLibConst.PREF_CONFIG>> listConfig = new List<SetParam<eLibConst.PREF_CONFIG>>();
            List<SetParam<String>> listConfigDefault = new List<SetParam<String>>();

            //Résultat
            eAdminResult res = new eAdminResult();


            foreach (eAdminUpdateProperty pty in caps.ListProperties)
            {
                eAdminUpdateProperty.CATEGORY cat = (eAdminUpdateProperty.CATEGORY)pty.Category;

                switch (cat)
                {
                    case eAdminUpdateProperty.CATEGORY.CONFIG:
                        listConfig.Add(new SetParam<eLibConst.PREF_CONFIG>((eLibConst.PREF_CONFIG)pty.Property, pty.Value));
                        break;
                    case eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT:
                        listConfigDefault.Add(new SetParam<String>(((eLibConst.CONFIG_DEFAULT)pty.Property).ToString(), pty.Value));
                        break;
                    case eAdminUpdateProperty.CATEGORY.LDAPPARAMS:
                        LdapLib.MajParam(_pref, (eLibConst.CONFIG_DEFAULT)pty.Property, pty.Value);
                        break;
                    case eAdminUpdateProperty.CATEGORY.SMSNETMESSAGE_SETTINGS:
                        SmsNetMessageSettingsClient.MajParam(_pref, (SmsNetMessageSettingsClient.SMS_PARAMS)pty.Property, pty.Value);
                        break;
                    case eAdminUpdateProperty.CATEGORY.WORLDLINE_PAYMENT_SETTINGS:
                        eWorldlinePaymentSetting.MajParam(_pref, (eWorldlinePaymentSetting.WORLDLINE_PARAMS)pty.Property, pty.Value);
                        break;
                }
            }

            res.Success = true;

            // Mise à jour CONFIG
            if (listConfig.Count > 0)
                res.Success = _pref.SetConfig(listConfig);

            // Maj ConfigADV
            if(res.Success && listConfigDefault.Count > 0)
                res.Success = _pref.SetConfigDefault(listConfigDefault);

            //ALISTER Demande #64 862, Ajoute une pop up, montrant que le paramètre sera pris en compte
            //à la prochaine connexion
            if (!res.Success)
            {
                res.UserErrorTitle = eResApp.GetRes(_pref.LangId, 416);
                res.UserErrorMessage = eResApp.GetRes(_pref.LangId, 2702);
            }
            else
            {
                res.NeedConfirm = false;
                res.UserMessage = eResApp.GetRes(_pref.LangId, 2701);
            }

            RenderResult(RequestContentType.TEXT, delegate ()
            {
                return JsonConvert.SerializeObject(res);
            });

        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }

}