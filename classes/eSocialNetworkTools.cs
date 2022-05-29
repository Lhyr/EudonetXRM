using Com.Eudonet.Internal;
using System;
using System.Web;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public static class eSocialNetworkTools
    {
        /// <summary>
        /// Type de réseau social
        /// </summary>
        private enum SOCIALNETWORK
        {
            TWITTER,
            FACEBOOK,
            LINKEDIN
        }

        #region Share URL

        /// <summary>
        /// Renvoie l'url complète de partage de lien d'un reseau social
        /// </summary>
        /// <param name="socialNetwork">Type de réseau social</param>
        /// <param name="sharedUrl">url à partager</param>
        /// <param name="title">titre</param>
        /// <param name="summary">description</param>
        /// <returns></returns>
        private static string GetSharedUrl(SOCIALNETWORK socialNetwork, string sharedUrl, string title = "", string summary = "")
        {
            switch (socialNetwork)
            {
                case SOCIALNETWORK.TWITTER:
                    return GetTwitterShareUrl(sharedUrl, title);
                case SOCIALNETWORK.FACEBOOK:
                    return GetFacebookShareUrl(sharedUrl);
                case SOCIALNETWORK.LINKEDIN:
                    return GetLinkedInShareUrl(sharedUrl, title, summary);
                default:
                    return String.Empty;
            }
        }

        /// <summary>
        /// Renvoie l'url complète de partage de lien de Twitter
        /// </summary>
        /// <param name="sharedUrl">url à partager</param>
        /// <param name="title">titre</param>
        /// <returns></returns>
        public static string GetTwitterShareUrl(string sharedUrl, string title = "")
        {
            string url = "https://twitter.com/share";
            url = String.Concat(url, "?url=", HttpUtility.UrlEncode(sharedUrl));
            if (!String.IsNullOrEmpty(title))
            {
                url = String.Concat(url, "&text=", HttpUtility.UrlEncode(title));
            }

            return url;
        }

        /// <summary>
        /// Renvoie l'url complète de partage de lien de Facebook
        /// </summary>
        /// <param name="sharedUrl">url à partager</param>
        /// <returns></returns>
        public static string GetFacebookShareUrl(string sharedUrl)
        {
            string url = "https://www.facebook.com/sharer/sharer.php";
            url = String.Concat(url, "?u=", HttpUtility.UrlEncode(sharedUrl));

            return url;
        }

        /// <summary>
        /// Renvoie l'url complète de partage de lien de LinkedIn
        /// </summary>
        /// <param name="sharedUrl">url à partager</param>
        /// <param name="title">titre (max 200 caractères)</param>
        /// <param name="summary">description (max 256 caractères)</param>
        /// <returns></returns>
        public static string GetLinkedInShareUrl(string sharedUrl, string title = "", string summary = "")
        {
            string url = String.Concat("https://www.linkedin.com/shareArticle", "?mini=true");
            url = String.Concat(url, "&url=", HttpUtility.UrlEncode(sharedUrl));

            if (!String.IsNullOrEmpty(title))
            {
                url = String.Concat(url, "&title=", HttpUtility.UrlEncode(title.Length > 200 ? title.Substring(0, 200) : title));
            }

            if (!String.IsNullOrEmpty(summary))
            {
                url = String.Concat(url, "&summary=", HttpUtility.UrlEncode(summary.Length > 256 ? summary.Substring(0, 256) : summary));
            }

            return url;
        }
        #endregion

        #region HTML

        /// <summary>
        /// Renvoie la class pour la div génerale du bouton
        /// </summary>
        /// <param name="socialNetwork">type de réseau social</param>
        /// <returns></returns>
        private static string GetContainerClass(SOCIALNETWORK socialNetwork)
        {
            switch (socialNetwork)
            {
                case SOCIALNETWORK.TWITTER:
                    return "twitterButtonContainer";
                case SOCIALNETWORK.FACEBOOK:
                    return "facebookButtonContainer";
                case SOCIALNETWORK.LINKEDIN:
                    return "linkedinButtonContainer";
                default:
                    return String.Empty;
            }
        }

        /// <summary>
        /// Renvoie la class ico-moon correspondant au logo du réseau social
        /// </summary>
        /// <param name="socialNetwork">type de réseau social</param>
        /// <returns></returns>
        private static string GetIconClass(SOCIALNETWORK socialNetwork)
        {
            switch (socialNetwork)
            {
                case SOCIALNETWORK.TWITTER:
                    return "icon-twitter";
                case SOCIALNETWORK.FACEBOOK:
                    return "icon-facebook";
                case SOCIALNETWORK.LINKEDIN:
                    return "icon-linkedin";
                default:
                    return String.Empty;
            }
        }

        /// <summary>
        /// Renvoie le texte du bouton de partage
        /// </summary>
        /// <param name="Pref">pref</param>
        /// <param name="socialNetwork">type de réseau social</param>
        /// <returns></returns>
        private static string GetText(ePref Pref, SOCIALNETWORK socialNetwork)
        {
            switch (socialNetwork)
            {
                case SOCIALNETWORK.TWITTER:
                    return eResApp.GetRes(Pref, 8026); //Tweet
                case SOCIALNETWORK.FACEBOOK:
                case SOCIALNETWORK.LINKEDIN:
                    return eResApp.GetRes(Pref, 8027); //Partager
                default:
                    return String.Empty;
            }
        }

        /// <summary>
        /// Renvoie le texte de l'infobulle du bouton de partage
        /// </summary>
        /// <param name="Pref">pref</param>
        /// <param name="socialNetwork">type de réseau social</param>
        /// <returns></returns>
        private static string GetTooltip(ePref Pref, SOCIALNETWORK socialNetwork)
        {
            string common = "<SOCIALNETWORK>";
            string res = eResApp.GetRes(Pref, 8028); //Partager sur <>
            switch (socialNetwork)
            {
                case SOCIALNETWORK.TWITTER:
                    return res.Replace(common, "Twitter");
                case SOCIALNETWORK.FACEBOOK:
                    return res.Replace(common, "Facebook");
                case SOCIALNETWORK.LINKEDIN:
                    return res.Replace(common, "LinkedIn");
                default:
                    return String.Empty;
            }
        }

        /// <summary>
        /// Renvoie le rendu html d'un bouton "Partager" sur un réseau social
        /// </summary>
        /// <param name="Pref">pref</param>
        /// <param name="socialNetwork">Type de réseau social</param>
        /// <param name="sharedUrl">url à partager</param>
        /// <param name="title">titre</param>
        /// <param name="summary">description</param>
        /// <returns></returns>
        private static HtmlGenericControl GetGenericShareButton(ePref Pref, SOCIALNETWORK socialNetwork, string sharedUrl, string title = "", string summary = "", string divId = "", string linkId = "")
        {
            HtmlGenericControl buttonContainer = new HtmlGenericControl("div");
            buttonContainer.Attributes.Add("class", GetContainerClass(socialNetwork));
            buttonContainer.Attributes.Add("title", GetTooltip(Pref, socialNetwork));
            if(!String.IsNullOrEmpty(divId))
                buttonContainer.Attributes.Add("id", divId);

            HtmlGenericControl link = new HtmlGenericControl("a");
            link.Attributes.Add("target", "_blank");
            link.Attributes.Add("href", GetSharedUrl(socialNetwork, sharedUrl, title, summary));
            if (!String.IsNullOrEmpty(linkId))
                link.Attributes.Add("id", linkId);
            buttonContainer.Controls.Add(link);

            HtmlGenericControl icon = new HtmlGenericControl("span");
            icon.Attributes.Add("class", GetIconClass(socialNetwork));
            link.Controls.Add(icon);

            HtmlGenericControl text = new HtmlGenericControl("span");
            text.InnerText = GetText(Pref, socialNetwork);
            link.Controls.Add(text);

            return buttonContainer;
        }

        /// <summary>
        /// Renvoie le rendu html d'un bouton "Partager sur Twitter"
        /// </summary>
        /// <param name="Pref">pref</param>
        /// <param name="sharedUrl">url à partager</param>
        /// <param name="title">titre</param>
        /// <returns></returns>
        public static HtmlGenericControl GetTwitterShareButton(ePref Pref, string sharedUrl, string title = "", string divId = "", string linkId = "")
        {
            return GetGenericShareButton(Pref, SOCIALNETWORK.TWITTER, sharedUrl, title, divId: divId, linkId: linkId);
        }

        /// <summary>
        /// Renvoie le rendu html d'un bouton "Partager sur Facebook"
        /// </summary>
        /// <param name="Pref">pref</param>
        /// <param name="sharedUrl">url à partager</param>
        /// <returns></returns>
        public static HtmlGenericControl GetFacebookShareButton(ePref Pref, string sharedUrl, string divId = "", string linkId = "")
        {
            return GetGenericShareButton(Pref, SOCIALNETWORK.FACEBOOK, sharedUrl, divId: divId, linkId: linkId);
        }

        /// <summary>
        /// Renvoie le rendu html d'un bouton "Partager sur LinkedIn"
        /// </summary>
        /// <param name="Pref">pref</param>
        /// <param name="sharedUrl">url à partager</param>
        /// <param name="title">titre</param>
        /// <param name="summary">description</param>
        /// <returns></returns>
        public static HtmlGenericControl GetLinkedInShareButton(ePref Pref, string sharedUrl, string title = "", string summary = "", string divId = "", string linkId = "")
        {
            return GetGenericShareButton(Pref, SOCIALNETWORK.LINKEDIN, sharedUrl, title, summary, divId: divId, linkId: linkId);
        }
        #endregion
    }
}