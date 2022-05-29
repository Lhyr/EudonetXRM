using Com.Eudonet.Internal;
using System;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Rendu de RSS
    /// </summary>
    public class eRSSRenderer : eRenderer
    {
        int _wid = 0;
        eXrmWidgetParam _params;

        private eRSSRenderer(ePref pref, int wid)
        {
            _ePref = pref;
            _wid = wid;
        }

        /// <summary>
        /// Appel l'objet métier
        /// eList/eFiche (l'appel a EudoQuery est fait dans cet appel ainsi que l'appel et le parcours du dataset)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected override bool Init()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
| SecurityProtocolType.Tls11
| SecurityProtocolType.Tls12
| SecurityProtocolType.Ssl3;

                _params = new eXrmWidgetParam(_ePref, _wid);
                return true;
            }
            catch (Exception exc)
            {
                _eException = exc;
                _sErrorMsg = exc.Message;
                return false;
            }

        }

        /// <summary>
        /// Build
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            Panel panel, content;
            HtmlGenericControl title, p, intro, cat;
            string authors = string.Empty;
            string categories = string.Empty;
            string description = string.Empty;
            string link = string.Empty;
            XmlReader reader = null;
            StringBuilder sbIntro = new StringBuilder();
            HyperLink a;

            _pgContainer.ID = "widgetRSS";

            //#region Erreur
            //error = new HtmlGenericControl();
            //_pgContainer.Controls.Add(error);
            //#endregion

            string url = _params.GetParamValue("url");

            if (!String.IsNullOrEmpty(url) && Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                ServicePointManager.ServerCertificateValidationCallback = (s, ce, ch, ssl) => true;
                try
                {
        

            

                    reader = XmlReader.Create(url);

                    SyndicationFeed feed = SyndicationFeed.Load(reader);
                    reader.Close();

                    foreach (SyndicationItem item in feed.Items)
                    {
                        panel = new Panel();
                        panel.CssClass = "widgetRSSItem";
                        _pgContainer.Controls.Add(panel);

                        link = GetLink(item);

                        description = GetDescription(item);

                        eLibTools.TransformLinksOpenMode(ref description);

                        // Image
                        string uri = GetImageURL(item, ref description);
                        if (!String.IsNullOrEmpty(uri))
                        {
                            Panel imgWrapper = new Panel();
                            imgWrapper.CssClass = "itemImage";

                            a = new HyperLink();
                            a.NavigateUrl = link;
                            a.Target = "_blank";

                            HtmlImage img = new HtmlImage();
                            img.Src = uri;
                            a.Controls.Add(img);

                            imgWrapper.Controls.Add(a);

                            panel.Controls.Add(imgWrapper);
                        }

                        content = new Panel();
                        content.CssClass = "itemContent";

                        // Titre
                        title = new HtmlGenericControl("h2");
                        a = new HyperLink();
                        a.NavigateUrl = link;
                        a.Text = item.Title.Text;
                        a.Target = "_blank";
                        title.Controls.Add(a);
                        content.Controls.Add(title);

                        // Catégories
                        categories = String.Join(" | ", item.Categories.Select(c => c.Name));
                        if (!String.IsNullOrEmpty(categories))
                        {
                            cat = new HtmlGenericControl();
                            cat.Attributes.Add("class", "categories");
                            cat.Attributes.Add("title", categories);
                            cat.InnerHtml = categories;
                            content.Controls.Add(cat);
                        }

                        // Auteur - Date de publication
                        intro = new HtmlGenericControl();
                        intro.Attributes.Add("class", "intro");
                        authors = String.Join("/", item.Authors.Select(au => String.Concat(au.Name, " ", au.Email)));
                        sbIntro = new StringBuilder();
                        if (!String.IsNullOrEmpty(authors))
                            sbIntro.Append(eResApp.GetRes(_ePref, 60)).Append(" ").Append(authors);
                        if (sbIntro.Length > 0)
                            sbIntro.Append(" - ");


                        sbIntro.Append(eLibTools.GetTimeSince(_ePref, item.PublishDate.LocalDateTime));

                        //string format = eDate.GetFormat(_ePref.CultureInfo, true);
                        intro.Attributes.Add("title", item.PublishDate.ToString());
                        intro.InnerText = sbIntro.ToString();
                        content.Controls.Add(intro);

                        // Résumé
                        p = new HtmlGenericControl("div");
                        p.Attributes.Add("class", "description");
                        p.Attributes.Add("title", eLibTools.RemoveHTML(description));
                        if (description.Length > 280)
                        {
                            description = eLibTools.GetFirstCharacters(description, 275) + "[...]";

                        }



                        p.InnerHtml = description;

                        content.Controls.Add(p);

                        panel.Controls.Add(content);
                    }



                }
                catch (Exception exc)
                {
                    _eException = exc;
                    _sErrorMsg = eResApp.GetRes(_ePref, 8542);
                    return false;
                }
                finally
                {
                    if (reader != null && reader.ReadState != ReadState.Closed)
                        reader.Close();
                }

            }


            return true;
        }

        /// <summary>
        /// Renvoie l'URL de l'image trouvée
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="summary">The summary.</param>
        /// <returns></returns>
        private string GetImageURL(SyndicationItem item, ref string summary)
        {
            string value = string.Empty;
            string imgURL = string.Empty;

            string enclosureImg = GetEnclosureUri(item);
            if (enclosureImg != "")
                imgURL = enclosureImg;
            else if (item.ElementExtensions.Count > 0)
            {
                foreach (SyndicationElementExtension extension in item.ElementExtensions)
                {
                    value = GetExtensionImage(extension);
                    if (!String.IsNullOrEmpty(value))
                    {
                        imgURL = value;
                        break;
                    }


                }

            }

            if (String.IsNullOrEmpty(imgURL))
            {
                // Si aucune image trouvée, on récupère la première image définie dans la description, s'il y en a une
                imgURL = eLibTools.RemoveFirstImageElementInHtmlContent(ref summary);
            }

            if (!String.IsNullOrEmpty(imgURL))
            {
                // Si l'image est dans la description, on retire l'image de la description
                eLibTools.RemoveImageElementWithSrc(ref summary, imgURL);
            }


            return imgURL;
        }

        /// <summary>
        /// Récupère 
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private string GetLink(SyndicationItem item)
        {
            if (item.Links.Count > 0)
            {
                for (int i = 0; i < item.Links.Count; i++)
                {
                    if (String.IsNullOrEmpty(item.Links[i].MediaType) || !item.Links[i].MediaType.Contains("image"))
                    {
                        return item.Links[i].Uri.AbsoluteUri;
                    }
                }
            }
            return "#";
        }

        /// <summary>
        /// Retourne l'image contenue dans le tag "enclosure"
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private string GetEnclosureUri(SyndicationItem item)
        {
            for (int i = 0; i < item.Links.Count; i++)
            {
                if (item.Links[i].RelationshipType == "enclosure" && item.Links[i].MediaType.Contains("image"))
                {
                    return item.Links[i].Uri.AbsoluteUri;
                }
            }
            return "";
        }

        /// <summary>
        /// Retourne l'URL de l'image trouvée dans une balise spécifique
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        private string GetExtensionImage(SyndicationElementExtension ext)
        {
            XElement elem = ext.GetObject<XElement>();
            string value = elem.Value.ToLower().Trim();
            if (value.EndsWith(".jpg") || value.EndsWith(".png") || value.EndsWith(".gif"))
            {
                return value;
            }
            else if (elem.HasAttributes)
            {
                foreach (XAttribute attribute in elem.Attributes())
                {
                    value = attribute.Value.ToLower().Trim();
                    if ((value.EndsWith(".jpg") || value.EndsWith(".png") || value.EndsWith(".gif")))
                    {
                        return value;
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the description from the SyndicationItem object
        /// </summary>
        /// <param name="item">SyndicationItem</param>
        /// <returns></returns>
        string GetDescription(SyndicationItem item)
        {
            string description = string.Empty;


            if (item.Content != null)
            {
                try
                {
                    TextSyndicationContent tsc = (TextSyndicationContent)item.Content;
                    description = tsc.Text;
                }
                catch (Exception)
                {
                    description = string.Empty;
                }
            }

            if (String.IsNullOrEmpty(description) && !String.IsNullOrEmpty(item.Summary.Text))
                description = item.Summary.Text;

            return description;
        }

        /// <summary>
        /// Création du rendu d'un feed RSS
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="wid"></param>
        /// <returns></returns>
        public static eRSSRenderer CreateRSSRenderer(ePref pref, int wid)
        {
            eRSSRenderer rdr = new eRSSRenderer(pref, wid);
            return rdr;
        }
    }
}