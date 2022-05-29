using System;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model.prefs;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Renderer d'options utilisateur - Choix de la langue
    /// </summary>
    public class eAdminUsrOptSignRenderer : eAdminUsrOptMemoRenderer
    {


        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminUsrOptSignRenderer(ePref pref)
            : base(pref, eUserOptionsModules.USROPT_MODULE.PREFERENCES_SIGNATURE)
        {

            _nUserId = pref.UserId;

            _sEditorType = "adminusersign";
        }

        /// <summary>
        /// Ajout de champ complémentaire
        /// </summary>
        /// <param name="p"></param>
        protected override void BuildMainCplt(Panel p)
        {

            #region Case à cocher pour la signature
            var dic = Pref.LoadUserConfig(_nUserId);


            string sValue;
            dic.TryGetValue(new KeyConfig(eLibConst.PREF_CONFIG.EMAILAUTOADDSIGN), out sValue);
            bool bAutoSignInsert = sValue == "1";
            eCheckBoxCtrl chkAutoAddSign = new eCheckBoxCtrl(bAutoSignInsert, false);
            chkAutoAddSign.ID = "auto-sign";
            chkAutoAddSign.AddClick(String.Empty);
            chkAutoAddSign.AddText(eResApp.GetRes(Pref, 576));
            p.Controls.Add(chkAutoAddSign);
            #endregion
        }

        /// <summary>
        /// initialise les var propre au renderer
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {

            if (!eUser.GetFieldValue<String>(Pref, Pref.UserId, "UserSignature", out _sMemoContent))
                _sMemoContent = String.Empty;


            _sTitle = eUserOptionsModules.GetModuleMiddleTitleLabel(eUserOptionsModules.USROPT_MODULE.PREFERENCES_SIGNATURE, Pref);
            _sSubTitle = eUserOptionsModules.GetModuleLabel(eUserOptionsModules.USROPT_MODULE.PREFERENCES_SIGNATURE, Pref);
            _sActionOk = "setSign();";
            _sActionCancel = String.Concat("loadUserOption('", eUserOptionsModules.USROPT_MODULE.PREFERENCES.ToString(), "');");
            return true;



        }


    }


    /// <summary>
    /// renderer de modification d'un champ mémo depuis l'administration des users
    /// réserver à l'admin - ouverture en popup
    /// TODO : le renderer est en très grande partie en commun avec celui des mémo.
    /// </summary>
    public class eAdminUsrOptSignAdminRenderer : eAdminUsrOptSignRenderer
    {
        public eAdminUsrOptSignAdminRenderer(ePref pref, Int32 nUserId) : base(pref)
        {

            if (pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                throw new EudoAdminInvalidRightException();

            _nUserId = nUserId;


        }

        protected override void BuildButton(Panel pnl)
        {
            //base.BuildButton(pnl);
        }

        protected override void BuildTitle(Panel p)
        {

        }

        /// <summary>
        /// initialise les variables lié au renderer
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (!eUser.GetFieldValue<String>(Pref, _nUserId, "UserSignature", out _sMemoContent))
                _sMemoContent = String.Empty;



            _sTitle = eResApp.GetRes(Pref, 7965);
            _sSubTitle = string.Empty;

            return true;
        }

        /// <summary>
        /// Ajout de champ complémentaire
        /// </summary>
        /// <param name="p"></param>
        protected override void BuildMainCplt(Panel p)
        {

            #region Case à cocher pour la signature

            var dic = Pref.LoadUserConfig(_nUserId);


            string sValue;
            dic.TryGetValue(new KeyConfig(eLibConst.PREF_CONFIG.EMAILAUTOADDSIGN), out sValue);
            bool bAutoSignInsert = sValue == "1";
            eCheckBoxCtrl chkAutoAddSign = new eCheckBoxCtrl(bAutoSignInsert, false);
            chkAutoAddSign.ID = "auto-sign";
            chkAutoAddSign.AddClick(String.Empty);
            chkAutoAddSign.AddText(eResApp.GetRes(Pref, 7964));
            p.Controls.Add(chkAutoAddSign);
            #endregion
        }
    }
}