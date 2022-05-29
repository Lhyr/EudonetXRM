using Com.Eudonet.Internal;
using EudoGraphTeams.Authentication;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    ///vérifie l'état de la connection Azure 
    /// </summary>
    public partial class AzureCheckAuthentication : eEudoPage
    {

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {

            List<string> lstCss = new List<string> {
                    "../IRISBlack/Front/Assets/CSS/lato",
            };

            PageRegisters.AddRangeCssWithPath(lstCss);
            if (eLibTools.IsLocalOrEudoMachine())
                if (_requestTools.AllKeysQS.Contains("message")
                    || _requestTools.AllKeysQS.Contains("debug")
                    )
                {
                    ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.EXCLAMATION,
                        msg: "Azure Authentication has failed",
                        detailMsg: "",
                        devMsg: $"{_requestTools.GetRequestQSKeyS("message")}, {_requestTools.GetRequestQSKeyS("debug")}"
                        );
                    LaunchError();
                }

            SessionTokenStore tokenStore = new SessionTokenStore(null, HttpContext.Current, ClaimsPrincipal.Current);

            CachedUser user = null;

            if (tokenStore.HasUser())
                user = user = tokenStore.GetUserDetails();


            AzureCheckAuthResponse result = new AzureCheckAuthResponse()
            {
                IsAuthenticated = tokenStore.HasUser(),
                AzureName = user?.DisplayName,
                AzureMail = user?.Email,
                EudoName = _pref?.User?.UserDisplayName,
                EudoMail = _pref?.User?.UserMail
            };

            resultInput.Value = SerializerTools.JsonSerialize(result);



            if (tokenStore.HasUser())
            {
                GlobalContentDiv.Attributes.Add("class", "authok");
                TitleH3.InnerText = eResApp.GetRes(_pref, 3051);//  Vous êtes connecté !
                MessageDiv.InnerHtml = eResApp.GetRes(_pref, 3061); //Vous pouvez désormais créer et mettre à jour vos rendez-vous Microsoft Teams depuis vos fiches Eudonet et à partir des comptes suivants :
                AzureDiv.InnerHtml = string.Format(eResApp.GetRes(_pref, 3052), HttpUtility.HtmlEncode($"{result.AzureName} <{result.AzureMail}>"));

                EudoDiv.InnerHtml = string.Format(eResApp.GetRes(_pref, 3053), HttpUtility.HtmlEncode($"{result.EudoName} <{result.EudoMail}>"));

                AddonInformationDiv.InnerHtml = eResApp.GetRes(_pref, 3054);

                HyperLink a = new HyperLink()
                {
                    Text = eResApp.GetRes(_pref, 3060),                  //fermer cette fenêtre 
                    NavigateUrl = "#",
                };
                a.Attributes.Add("onclick", "window.close();return false;");
                LinkDiv.Controls.Add(a);

            }
            else
            {
                GlobalContentDiv.Attributes.Add("class", "authdown");
                TitleH3.InnerText = eResApp.GetRes(_pref, 3055); // "Zut j'ai perdu le fil"; 
                MessageDiv.InnerHtml= eResApp.GetRes(_pref, 3062); //Eudonet n'a pu établir la connexion avec Microsoft Teams et récupérer la liste de vos comptes.
                AddonInformationDiv.Controls.Add(new LiteralControl(eResApp.GetRes(_pref, 3059))); //Eudonet n'a pu établir la connexion avec Microsoft Teams et récupérer la liste de vos comptes.
                LinkDiv.Controls.Add(
                    new HyperLink()
                    {
                        Text = eResApp.GetRes(_pref, 3056),                  //Je fais une nouvelle tentative 
                        NavigateUrl = "AzureLogin.ashx"
                    }
                );

                HyperLink a = new HyperLink()
                {
                    Text = eResApp.GetRes(_pref, 3060),                  //fermer cette fenêtre 
                    NavigateUrl = "#",
                };
                a.Attributes.Add("onclick", "window.close();return false;");
                LinkDiv.Controls.Add(a);

            }
        }

        protected class AzureCheckAuthResponse
        {
            public bool IsAuthenticated { get; set; } = false;
            public string AzureName { get; set; }
            public string AzureMail { get; set; }
            public string EudoName { get; set; }
            public string EudoMail { get; set; }


        }

    }
}