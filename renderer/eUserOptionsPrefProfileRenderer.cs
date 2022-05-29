using System;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model.prefs;
using EudoQuery;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Xrm.eda;
using System.Web.UI.HtmlControls;
using System.Web.UI;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Renderer d'options utilisateur - Choix de préférence de profile utilisateur
    /// </summary>
    public class eUserOptionsPrefProfileRenderer : eUserOptionsRenderer
    {

        protected string _sTitle;
        protected string _sSubTitle;
        protected string _sActionOk;
        protected string _sActionCancel;
        protected string _sUserSrc;
        protected string _sUserSrcProfile;
        
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eUserOptionsPrefProfileRenderer(ePref pref)
            : base(pref, eUserOptionsModules.USROPT_MODULE.PREFERENCES_PROFILE)
        {
            _nUserId = pref.UserId;
        }

        /// <summary>
        /// Construction de la zone de titre
        /// </summary>
        protected virtual void BuildTitle(Panel p)
        {
            #region Les titres
            HtmlGenericControl pnlTitle = new HtmlGenericControl("div"); // pas de InnerText sur un contrôle Panel...
            pnlTitle.Attributes.Add("class", "adminModalMiddleTitle ");
            pnlTitle.InnerText = _sTitle;
            p.Controls.Add(pnlTitle);
        }

        /// <summary>
        /// Construction de la zone de sous-titre
        /// </summary>
        protected virtual void BuildSubTitle(Panel pnl)
        {
            HtmlGenericControl pnlSubTitle = new HtmlGenericControl("div"); // pas de InnerText sur un contrôle Panel...
            pnlSubTitle.ID = "memoSubTitle";
            pnlSubTitle.Attributes.Add("class", "adminCntntTtl ");
            pnlSubTitle.InnerText = _sSubTitle;
            #endregion

            pnl.Controls.Add(pnlSubTitle);
        }


        /// <summary>
        /// Construction des boutons si non construit en js
        /// </summary>
        protected virtual void BuildButton(Panel pnl)
        {
            #region Les boutons
            Panel btnPart = new Panel();
            btnPart.CssClass = "adminBtnPart";

            eButtonCtrl btnValidate = new eButtonCtrl(eResApp.GetRes(Pref, 28), eButtonCtrl.ButtonType.GREEN, _sActionOk); // Valider
            btnPart.Controls.Add(btnValidate);
            #endregion

            pnl.Controls.Add(btnPart);
        }

        /// <summary>
        /// Ajout de champ complémentaire
        /// </summary>
        /// <param name="p"></param>
        private void BuildMainCplt(Panel panel)
        {
            HtmlGenericControl p = new HtmlGenericControl("p");
            p.Attributes.Add("class", "copyPrefP");
            panel.Controls.Add(p);

            HtmlGenericControl infoUL = new HtmlGenericControl("ul");
            infoUL.Attributes.Add("class", "copyPrefLabelUl");
            panel.Controls.Add(infoUL);

            HtmlGenericControl liCatDstSrc = new HtmlGenericControl("li");
            infoUL.Controls.Add(liCatDstSrc);
            HtmlGenericControl ulCatDstSrc = new HtmlGenericControl("ul");
            liCatDstSrc.Controls.Add(ulCatDstSrc);
            ulCatDstSrc.Attributes.Add("class", "usrOptionPrefSrcDslUL");
            //ulCatDstSrc.Attributes.Add("id", "EDA_CPREF_DST");
            //ulCatDstSrc.Attributes.Add("ednvalue", _nUserId.ToString());

            HtmlGenericControl infoSrc = new HtmlGenericControl("li");
            infoSrc.ID = "userProfilePref";
            ulCatDstSrc.Controls.Add(infoSrc);
            infoSrc.Attributes.Add("class", "copyPref");


            //Barre de texte où le nom de profil utilisateur source est affiché
            //Textbar where user's source profile is displayed
            HtmlGenericControl txtSource = new HtmlGenericControl("input");
            infoSrc.Controls.Add(txtSource);
            txtSource.Attributes.Add("readonly", "1");
            txtSource.Attributes.Add("id", "EDA_CPREF_SRC");
            txtSource.Attributes.Add("type", "text");
            txtSource.Attributes.Add("eaction", "LNKCATUSER");
            txtSource.Attributes.Add("class", " readonly LNKCATUSER edit edaPrefInputField");
            txtSource.Attributes.Add("ednformat", ((int)FieldFormat.TYP_USER).ToString());

            txtSource.Attributes.Add("ednvalue", _sUserSrc);
            txtSource.Attributes.Add("value", _sUserSrcProfile);

            //Bouton catalogue pour choisir le profil utilisateur source de préférence
            //Catalog button to choose the user's profile source preference
            HtmlGenericControl btnSrc = new HtmlGenericControl("span");
            btnSrc.Attributes.Add("class", "rUsr icon-catalog btn");
            
            btnSrc.Attributes.Add("onclick", "userProfilePrefCat(this, 'EDA_CPREF_SRC', 0, 1, 1, 1); ");
            infoSrc.Controls.Add(btnSrc);

            if (_nUserId <= 0)
            {
                eAdminButtonField btn = new eAdminButtonField(eResApp.GetRes(Pref, 7880), "btnResetPref", onclick: "nsAdminPref.copyPref()");
                btn.Generate(panel);
            }
        }
    

        /// <summary>
        /// initialise les var propre au renderer
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (!eUser.GetFieldValue<String>(Pref, Pref.UserId, "USER_PROFILE", out _sUserSrc))
                _sUserSrc = String.Empty;

            int userSrcId = 0;

            Int32.TryParse(_sUserSrc, out userSrcId);

            if(userSrcId > 0) {
                if (!eUser.GetFieldValue<String>(Pref, userSrcId, "UserLogin", out _sUserSrcProfile))
                    _sUserSrcProfile = String.Empty;
            }

            _sTitle = eUserOptionsModules.GetModuleMiddleTitleLabel(eUserOptionsModules.USROPT_MODULE.PREFERENCES_PROFILE, Pref);
            _sSubTitle = _sSubTitle = eResApp.GetRes(Pref, 8934);
            _sActionOk = "setUserProfilePref(); ";
            //_sActionCancel = String.Concat("loadUserOption('", eUserOptionsModules.USROPT_MODULE.PREFERENCES.ToString(), "');");
            return true;
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {

            Panel pnlContents = new Panel();
            pnlContents.ID = "admntCntnt";
            pnlContents.CssClass = "adminCntnt";


            BuildTitle(_pgContainer);

            BuildSubTitle(pnlContents);

            BuildMainCplt(pnlContents);

            BuildButton(pnlContents);

            _pgContainer.Controls.Add(pnlContents);

            return true;
        }
    }
}