using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.edm
{
    public partial class TestBingSearch : System.Web.UI.Page
    {
        String _accountKey = "Io8Fv3oVcdpsRX6BVAoS96Ba0akDeqNUUjo7ZOXmJOI=";

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void ButtonSearch_Click(object sender, EventArgs e)
        {

            // Create a Bing container.
            string rootUri = "https://api.datamarket.azure.com/Bing/Search";

            Bing.BingSearchContainer bingContainer = new Bing.BingSearchContainer(new Uri(rootUri));

            // Replace this value with your account key.

            // Configure bingContainer to use your credentials.

            bingContainer.Credentials = new NetworkCredential(_accountKey, _accountKey);

            // Build the query.
            String query = TextSearch.Text;
            string size = DdlSize.SelectedItem.Text;
            string color = DdlColor.SelectedItem.Text;
            string style = DdlStyle.SelectedItem.Text;

            // Parameters
            String market = "fr-FR";

            var imageQuery = bingContainer.Image(query, null, market, "Moderate", null, null, "Size:" + size + "+Color:" + color+"+Style:"+style);
            imageQuery = imageQuery.AddQueryOption("$top", eLibTools.GetNum(TextTop.Text));

            var imageResults = imageQuery.Execute();

            HtmlGenericControl ul = new HtmlGenericControl("ul");

            foreach (var result in imageResults)
            {
                ul.Controls.Add(BuildResult(result.Title, result.MediaUrl));
            }

            PanelResults.Controls.Add(ul);
        }

        /// <summary>Construction du bloc résultat</summary>
        /// <param name="title">Titre</param>
        /// <param name="url">URL de l'image</param>
        /// <returns></returns>
        private HtmlGenericControl BuildResult(String title, String url)
        {
            HtmlGenericControl li = new HtmlGenericControl("li");
            HtmlImage image = new HtmlImage();
            image.Src = url;
            image.Attributes.Add("title", title);
            li.Controls.Add(image);
            return li;
        }
    }
}