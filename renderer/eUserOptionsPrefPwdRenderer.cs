using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Renderer d'options utilisateur - Mot de passe
    /// </summary>
    public class eAdminUsrOptPwdRenderer : eUserOptionsRenderer
    {
        /// <summary>
        /// id du user auquel appartient le mot de passe
        /// </summary>
        public int _userId;
        /// <summary>
        /// contexte depuis lequel est appelé le module de changement de mot de passe
        /// </summary>
        public eUserOptionsModules.PREFERENCES_PASSWORD_CONTEXT _context;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminUsrOptPwdRenderer(ePref pref, int userId, eUserOptionsModules.PREFERENCES_PASSWORD_CONTEXT context)
            : base(pref, eUserOptionsModules.USROPT_MODULE.PREFERENCES_PASSWORD)
        {
            _userId = userId;
            _context = context;
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {
            #region Affichage

            Panel pnlContents = new Panel();
            pnlContents.ID = "admntCntnt";
            pnlContents.CssClass = "adminCntnt";

            #region Les titres
            if (_context == eUserOptionsModules.PREFERENCES_PASSWORD_CONTEXT.USEROPTIONS_PASSWORD)
            {
                HtmlGenericControl pnlTitle = new HtmlGenericControl("div"); // pas de InnerText sur un contrôle Panel...
                pnlTitle.Attributes.Add("class", "adminModalMiddleTitle ");
                pnlTitle.InnerText = eUserOptionsModules.GetModuleMiddleTitleLabel(eUserOptionsModules.USROPT_MODULE.PREFERENCES_PASSWORD, Pref);

                HtmlGenericControl pnlSubTitle = new HtmlGenericControl("div"); // pas de InnerText sur un contrôle Panel...
                pnlSubTitle.Attributes.Add("class", "adminCntntTtl ");
                pnlSubTitle.InnerText = eResApp.GetRes(Pref, 6782); // Modifier votre mot de passe :

                _pgContainer.Controls.Add(pnlTitle);
                pnlContents.Controls.Add(pnlSubTitle);
            }
            #endregion

            #region Partie principale - Champs de saisie

            // Création des listes
            HtmlGenericControl ulLabels = new HtmlGenericControl("ul");
            ulLabels.Attributes.Add("class", "labelMdpCntnt");
            pnlContents.Controls.Add(ulLabels);
            HtmlGenericControl ulInputs = new HtmlGenericControl("ul");
            ulInputs.Attributes.Add("class", "valueMdpCntnt");
            pnlContents.Controls.Add(ulInputs);

            // Définition des attributs de chaque champ
            List<string> labels = new List<string>() { eResApp.GetRes(Pref, 31), eResApp.GetRes(Pref, 201) };
            List<string> ids = new List<string>() { "NewPassword", "ConfirmNewPassword" };
            List<string> names = new List<string>() { "NewPassword", "ConfirmNewPassword" };
            if (_context == eUserOptionsModules.PREFERENCES_PASSWORD_CONTEXT.USEROPTIONS_PASSWORD)
            {
                labels.Insert(0, eResApp.GetRes(Pref, 524));
                ids.Insert(0, "OldPassword");
                names.Insert(0, "OldPassword");
            }

            // Création des libellés
            foreach (string label in labels)
            {
                HtmlGenericControl li = new HtmlGenericControl("li");
                li.InnerText = label;
                ulLabels.Controls.Add(li);
            }
            // Création des champs de saisie
            for (int i = 0; i < ids.Count; i++)
            {
                HtmlGenericControl li = new HtmlGenericControl("li");
                HtmlInputPassword input = new HtmlInputPassword();
                input.ID = ids[i];
                if (i < names.Count)
                    input.Name = names[i];
                input.Attributes.Add("class", "IZ");
                input.Attributes.Add("autocomplete", "new-password"); // #83 857 - Ne pas proposer de MDP enregistré dans les paramètres du navigateur de l'utilisateur - https://developer.mozilla.org/en-US/docs/Web/Security/Securing_your_site/Turning_off_form_autocompletion
                input.MaxLength = 50;
                li.Controls.Add(input);
                HtmlGenericControl h1 = new HtmlGenericControl("i");               
                h1.ID = String.Concat("pass-status","_",ids[i]);
                h1.Attributes.Add("class", "icon-edn-eye");
                h1.Attributes.Add("onclick", String.Concat("viewPassword(", ids[i] , ")"));
                li.Controls.Add(h1);
                ulInputs.Controls.Add(li);
            }

            #endregion

            #region Cases à cocher optionnelles

            Panel divCheckPart = new Panel();
            HtmlGenericControl ul = new HtmlGenericControl("ul");
            divCheckPart.CssClass = "checkPart";
            divCheckPart.Controls.Add(ul);
            List<string> chkLabels = new List<string>();
            List<string> chkIds = new List<string>();
            List<string> chkOnClicks = new List<string>();
            bool bUseConfirmChk = false;

            // Définition des cases à ajouter
            switch (_context)
            {
                case eUserOptionsModules.PREFERENCES_PASSWORD_CONTEXT.USEROPTIONS_PASSWORD:
                    if (bUseConfirmChk)
                    {
                        chkLabels.Add(eResApp.GetRes(Pref, 6783));
                        chkIds.Add("ChkConfirmPwdChange");
                        chkOnClicks.Add("togglePwdValid();");
                    }
                    break;
                case eUserOptionsModules.PREFERENCES_PASSWORD_CONTEXT.ADMIN_USERS:
                    chkLabels.Add(eResApp.GetRes(Pref, 5027));
                    chkLabels.Add(eResApp.GetRes(Pref, 6067));
                    chkIds.Add("ChkNotExpire");
                    chkIds.Add("ChkMustChangePwd");
                    chkOnClicks.Add(String.Empty);
                    chkOnClicks.Add(String.Empty);
                    break;
            }

            // Création des contrôles
            for (int i = 0; i < chkLabels.Count; i++)
            {
                HtmlGenericControl li = new HtmlGenericControl("li");
                eCheckBoxCtrl chkBox = new eCheckBoxCtrl(false, false);
                if (i < chkIds.Count) chkBox.ID = chkIds[i];
                chkBox.AddText(chkLabels[i]);
                if (i < chkOnClicks.Count) chkBox.AddClick(chkOnClicks[i]);
                li.Controls.Add(chkBox);
                ul.Controls.Add(li);
            }

            // Ajout du contenu
            if (ul.Controls.Count > 0)
                pnlContents.Controls.Add(divCheckPart);

            #endregion

            #region Les boutons
            if (_context == eUserOptionsModules.PREFERENCES_PASSWORD_CONTEXT.USEROPTIONS_PASSWORD)
            {
                Panel btnPart = new Panel();
                btnPart.CssClass = "adminBtnPart";
                eButtonCtrl btnCancel = new eButtonCtrl(eResApp.GetRes(Pref, 29), eButtonCtrl.ButtonType.GRAY, "onPwdCancel();"); // Annuler
                eButtonCtrl btnValidate = new eButtonCtrl(eResApp.GetRes(Pref, 28), eButtonCtrl.ButtonType.GREEN, "onPwdValid();"); // Valider
                btnValidate.ID = "btnPwdValid";
                if (bUseConfirmChk)
                    btnValidate.Style.Add("visibility", "hidden"); // le bouton n'est visible que lorsque la case de confirmation est cochée
                btnPart.Controls.Add(btnValidate);
                btnPart.Controls.Add(btnCancel);
                pnlContents.Controls.Add(btnPart);
            }
            #endregion

            #region Champs cachés
            HtmlInputHidden inputHiddenUserId = new HtmlInputHidden();
            inputHiddenUserId.Name = "userid";
            inputHiddenUserId.Value = _userId.ToString();
            #endregion

            // Ajout du contenu
            _pgContainer.Controls.Add(inputHiddenUserId);
            _pgContainer.Controls.Add(pnlContents);

            #endregion

            return true;
        }
    }
}