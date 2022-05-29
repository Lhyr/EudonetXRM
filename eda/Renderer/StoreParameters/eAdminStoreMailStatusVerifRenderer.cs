using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using Com.Eudonet.Internal.eda;
using EudoCommonInterface.mailchecker;
using Com.Eudonet.Internal.mailchecker;
using System.Web.UI;
using Com.Eudonet.Common.Enumerations;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer du module d'administration de IP Dédiée
    /// </summary>
    public class eAdminStoreMailStatusVerifRenderer : eAdminStoreFileRenderer
    {
        //eDedicatedIpSetting _dedicatedIpSetting = new eDedicatedIpSetting();

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminStoreMailStatusVerifRenderer(ePref pref, eAdminExtension extension) : base(pref, extension)
        {

        }

        /// <summary>
        /// Création du rendu mode Fiche de l'extension IP Dédiée
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static eAdminStoreMailStatusVerifRenderer CreateAdminStoreMailStatusVerifRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminStoreMailStatusVerifRenderer rdr = new eAdminStoreMailStatusVerifRenderer(pref, ext);

            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Build des params
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (base.Build())
            {
                AddCallBackScript("nsAdmin.addScript('eMailStatusVerifAdmin', 'ADMIN_STORE');", true);
                return true;
            }
            return false;
        }


        private bool IsLaunchAvailable(MailCheckerAPISVerificationStatusEnum status)
        {

            return (status == MailCheckerAPISVerificationStatusEnum.NO_JOB
                      || status == MailCheckerAPISVerificationStatusEnum.DONE
                       || status == MailCheckerAPISVerificationStatusEnum.ERROR
                      );
        }

        /// <summary>
        /// écran de paramétrage
        /// </summary>
        /// <returns></returns>
        protected override void CreateSettingsPanel()
        {
            base.CreateSettingsPanel();

            eMailCheckerJobStatus jobStatus = eMailCheckerTools.GetVerificationJobStatus(Pref, _eRegisteredExt.Param);
            HtmlGenericControl sectionInfoMailVerifExtensionMesssage = new HtmlGenericControl();
            sectionInfoMailVerifExtensionMesssage.Attributes.Add("class", "mailVerifInfoText divStep");
            sectionInfoMailVerifExtensionMesssage.InnerHtml = eResApp.GetRes(_ePref, 2977);
            ExtensionParametersContainer.Controls.Add(sectionInfoMailVerifExtensionMesssage);

            Panel pnlIncludeOldMailAdress = new Panel();
            pnlIncludeOldMailAdress.ID = "includeOldMailAdress";
            ExtensionParametersContainer.Controls.Add(pnlIncludeOldMailAdress);
            AddCheckboxOptionField(pnlIncludeOldMailAdress, "chkIncludeOldMailAdress", eResApp.GetRes(Pref, 2978), "",
                eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.INCLUDE_OLD_MAIL_ADRESS.GetHashCode(),
                typeof(eLibConst.CONFIG_DEFAULT), jobStatus.DateMax == DateTime.MaxValue ? false : true, null, "", null, "", null, "", "", false, eLibConst.CONFIGADV_CATEGORY.UNDEFINED, 
                IsLaunchAvailable(jobStatus.MailCheckerJobStatus) ? false : true);

            Panel pnlLaunchVerificationMailAdress = new Panel();
            pnlLaunchVerificationMailAdress.ID = "launchVerificationMailAdress";
            ExtensionParametersContainer.Controls.Add(pnlLaunchVerificationMailAdress);
            AddButtonOptionField(pnlLaunchVerificationMailAdress, "btnLaunchVerificationMailAdress", eResApp.GetRes(Pref, 2976), eResApp.GetRes(Pref, 2976), 
                IsLaunchAvailable(jobStatus.MailCheckerJobStatus) ? "nsMailStatusVerifAdmin.LaunchVerificationMailAdress();" : "", "", 
                !IsLaunchAvailable(jobStatus.MailCheckerJobStatus) ? true : false);

            Panel pnlMailStatusVerifPlannified = new Panel();
            pnlMailStatusVerifPlannified.ID = "mailStatusVerifPlannified";
            ExtensionParametersContainer.Controls.Add(pnlMailStatusVerifPlannified);
            HtmlGenericControl mailStatusVerifPlannifiedMesssage = new HtmlGenericControl();
            mailStatusVerifPlannifiedMesssage.ID = "dvMailVerifInfoText";
            mailStatusVerifPlannifiedMesssage.Attributes.Add("class", "mailVerifInfoText divStep");
            mailStatusVerifPlannifiedMesssage.InnerHtml = string.Format(eResApp.GetRes(_ePref, 2979), jobStatus.DateSchedule.ToString("dd/MM/yyyy HH:mm"));

            if (IsLaunchAvailable(jobStatus.MailCheckerJobStatus))
                mailStatusVerifPlannifiedMesssage.Style.Add(HtmlTextWriterStyle.Display, "none");

            ExtensionParametersContainer.Controls.Add(mailStatusVerifPlannifiedMesssage);
        }

    }
}