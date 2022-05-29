using System;
using System.Web.UI.HtmlControls;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// 
    /// </summary>
    public class eAPIProductScreenshot
    {
        /// <summary>URL de l'image </summary>
        public String ImageURL { get; private set; }

        /// <summary>Libellé </summary>
        public String Label { get; private set; }

        /// <summary>Ordre d'affichage </summary>
        public int DisplayOrder { get; private set; }

        /// <summary>URL Youtube / Dailymotion </summary>
        public string VideoURL { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imgURL"></param>
        /// <param name="label"></param>
        /// <param name="order"></param>
        public eAPIProductScreenshot(String imgURL, String label = "", int order = 0)
        {
            this.ImageURL = imgURL;
            this.Label = label;
            this.DisplayOrder = order;
        }

        /// <summary>
        /// Création d'une balise "li" contenant l'image
        /// </summary>
        /// <returns></returns>
        public HtmlGenericControl CreateImageItem()
        {
            HtmlGenericControl li = new HtmlGenericControl("li");

            HtmlImage image = new HtmlImage();
            image.ID = String.Concat("productScreenshot", this.DisplayOrder);
            image.Src = this.ImageURL;
            image.Alt = this.Label;
            image.Attributes.Add("title", this.Label);

            HtmlGenericControl label = new HtmlGenericControl("p");
            label.Attributes.Add("class", "productScreenshotLabel");
            label.InnerText = this.Label;

            li.Controls.Add(image);
            li.Controls.Add(label);

            return li;
        }
    }
}