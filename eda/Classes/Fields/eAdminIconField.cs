using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Affichage d'une icône avec un onclick
    /// </summary>
    public class eAdminIconField : eAdminField
    {
        String _onClick;
        String _iconClass;
        String _otherClasses;

        public eAdminIconField(String idButton, String iconClass, String otherClasses = "", String onclick = "", String tooltiptext = "")
            : base(0, "", tooltiptext)
        {
            this.FieldControlID = idButton;
            _onClick = onclick;
            _iconClass = iconClass;
            _otherClasses = otherClasses;
        }

        protected override Boolean Build(Panel panel)
        {
            HtmlGenericControl span = new HtmlGenericControl();
            span.ID = FieldControlID;
            span.Attributes.Add("class", String.Concat(_iconClass, " ", _otherClasses));
            span.Attributes.Add("onclick", _onClick);

            this.FieldControl = span;

            panel.Controls.Add(this.FieldControl);

            return true;
        }

        protected override bool End()
        {
            return true;
        }
    }
}