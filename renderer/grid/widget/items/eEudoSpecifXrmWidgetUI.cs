using Com.Eudonet.Internal;
using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Merge;

namespace Com.Eudonet.Xrm
{
    public class eEudoSpecifXrmWidgetUI : eAbstractXrmWidgetUI
    {
        ePref _pref;

        public eEudoSpecifXrmWidgetUI(ePref pref)
        {
            _pref = pref;
        }
        /// <summary>
        /// Fait un rendu du widget dans le container
        /// </summary>
        /// <param name="widgetContainer">Container du widget</param>
        public override void Build(HtmlControl widgetContainer)
        {


            String sSpecifId = _widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(EudoQuery.XrmWidgetField.ContentSource)).Value;
            int specifId = eLibTools.GetNum(sSpecifId);
            if (specifId > 0)
            {
                widgetContainer.Attributes.Add("sid", sSpecifId);

                HtmlGenericControl iframe = new HtmlGenericControl("iframe");

                //this._widgetContext.ParentTab

                string sEncode = ExternalUrlTools.GetCryptEncode(string.Concat("sid=", specifId,
                    "&tab=", this._widgetContext?.ParentTab.ToString() ?? "0",
                    "&fid=", this._widgetContext?.ParentFileId.ToString() ?? "0"));

                string url = String.Concat("eSubmitTokenXRM.aspx?t=", sEncode);

                iframe.Attributes.Add("src", url);

                widgetContainer.Controls.Add(iframe);

                // TODO Attributs aditionnal paramètre de site

                base.Build(widgetContainer);
            }
            else
            {
                // Param invalide
                widgetContainer.Controls.Add(new LiteralControl(eResApp.GetRes(_pref, 8018)));
            }

        }

        /// <summary>
        /// Ajoute des scripts js
        /// </summary>
        /// <param name="scriptBuilder">builder de script</param>
        public override void AppendScript(StringBuilder scriptBuilder)
        {
            base.AppendScript(scriptBuilder);
        }

        /// <summary>
        /// Ajoute des style css
        /// </summary>
        /// <param name="scriptBuilder">builder de styles</param>
        public override void AppendStyle(StringBuilder styleBuilder)
        {
            base.AppendStyle(styleBuilder);
        }
    }
}