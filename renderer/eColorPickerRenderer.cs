using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eColorPickerRenderer : eRenderer
    {
        String _currentColor;

        private eColorPickerRenderer(ePref pref, String color)
        {
            Pref = pref;
            _currentColor = color;
        }

        public static eColorPickerRenderer CreateColorPickerRenderer(ePref pref, String color)
        {
            eColorPickerRenderer rdr = new eColorPickerRenderer(pref, color);
            return rdr;
        }

        /// <summary>
        /// Build
        /// </summary>
        /// <returns></returns>
        protected override Boolean Build()
        {
            Panel colorWrapper = new Panel();
            colorWrapper.ID = "colorWrapper";

            //HtmlGenericControl a = new HtmlGenericControl("a");
            //a.InnerText = eResApp.GetRes(Pref, 7975);
            //a.ID = "btnDefaultColor";
            //colorWrapper.Controls.Add(a);

            colorWrapper.Controls.Add(CreateColorPicker());

            Panel panelChosenColor = new Panel();
            panelChosenColor.ID = "pnlPickedColor";

            Panel panelColor = new Panel();
            panelColor.ID = "pnlDisplayedColor";
            panelColor.Style.Add("background-color", _currentColor);
            panelColor.ToolTip = _currentColor;

            panelChosenColor.Controls.Add(panelColor);

            TextBox txtColor = new TextBox();
            txtColor.ID = "txtPictoColor";
            txtColor.Text = _currentColor;
            panelChosenColor.Controls.Add(txtColor);


            colorWrapper.Controls.Add(panelChosenColor);

            _pgContainer.Controls.Add(colorWrapper);

            return true;
        }


        /// <summary>
        /// Génération de la palette de couleurs
        /// </summary>
        /// <param name="wrapper"></param>
        private Panel CreateColorPicker()
        {


            Panel wrapper = new Panel();
            wrapper.ID = "pnlColors";

            List<String> listColors = new List<String>();
            listColors.Add("#1785bf;#2d9662;#f1a504;#910101;#553399;#535151;#a9ddf8;#a7dfa7;#fbd75b;#f09494;#dbadff;#bbb8b8");
            listColors.Add("#42a7dc;#3fa371;#f9b21a;#bb1515;#800080;#6e6c6c;#c1e8fc;#c4e9c4;#ffec86;#f8c4c4;#e9cdff;#d1d0d0");
            listColors.Add("#69c0ee;#69bf69;#f9b931;#dc4646;#954cce;#878585;#ceedfd;#daf2da;#fff1b0;#ffe5e5;#f0ddfe;#e7e4e4");
            listColors.Add("#90d1f3;#8bd48b;#fdce69;#e86c6c;#c48af2;#9a9898;#e4f8f8;#e8f6e8;#fef6d2;#fceded;#f6ebfe;#efefef");

            HtmlGenericControl ul, li;

            String[] arrColors;
            foreach (String colorsLine in listColors)
            {
                arrColors = colorsLine.Split(';');

                ul = new HtmlGenericControl("ul");

                for (int i = 0; i < arrColors.Length; i++)
                {
                    li = new HtmlGenericControl("li");
                    li.Style.Add("background-color", arrColors[i]);
                    li.Attributes.Add("title", arrColors[i]);
                    //li.Attributes.Add("onclick", "nsAdmin.selectColor(this)");
                    ul.Controls.Add(li);
                }

                wrapper.Controls.Add(ul);
            }

            return wrapper;
        }
    }
}