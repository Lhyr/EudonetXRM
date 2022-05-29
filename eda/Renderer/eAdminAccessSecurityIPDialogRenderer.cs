using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminAccessSecurityIPDialogRenderer : eAdminModuleRenderer
    {
        int _ipId = -1;
        string _fieldToFocus = String.Empty;
        eAdminAccessSecurityIPData _ip = null;

        public int IpId
        {
            get
            {
                return _ipId;
            }

            set
            {
                _ipId = value;
            }
        }

        public string FieldToFocus
        {
            get
            {
                return _fieldToFocus;
            }

            set
            {
                _fieldToFocus = value;
            }
        }

        private eAdminAccessSecurityIPDialogRenderer(ePref pref, int ipId, string fieldToFocus) : base(pref)
        {
            Pref = pref;
            IpId = ipId;
            FieldToFocus = fieldToFocus;
        }

        public static eAdminAccessSecurityIPDialogRenderer CreateAdminAccessSecurityIPDialogRenderer(ePref pref, int ipId, string fieldToFocus)
        {
            return new eAdminAccessSecurityIPDialogRenderer(pref, ipId, fieldToFocus);
        }

        protected override bool Init()
        {
            string strError = String.Empty;
            List<eAdminAccessSecurityIPData> ipList = eAdminAccessSecurityIP.GetIPAddresses(Pref, out strError);

            if (IpId > -1)
                _ip = ipList.Find(delegate (eAdminAccessSecurityIPData ip) { return ip.IpId == IpId; });
            else
                _ip = new eAdminAccessSecurityIPData(-1, String.Empty, String.Empty, String.Empty, String.Empty, -1, -1, PermissionMode.MODE_NONE);

            return base.Init();
        }

        protected override bool Build()
        {
            _pgContainer.ID = "AccessSecurityIPModalContent";
            _pgContainer.Attributes.Add("class", "adminModalContent");

            Panel dialogContents = GetModuleSection("accessSecurityIPEdit", IpId < 0 ? eResApp.GetRes(Pref, 1486) : eResApp.GetRes(Pref, 1487), 0); // Ajouter une valeur / Modifier une valeur
            Panel targetContainer = null;
            if (dialogContents.Controls.Count > 0 && dialogContents.Controls[dialogContents.Controls.Count - 1] is Panel)
                targetContainer = (Panel)dialogContents.Controls[dialogContents.Controls.Count - 1];
            if (targetContainer == null)
                targetContainer = _pgContainer;
            else
                _pgContainer.Controls.Add(dialogContents);

            HtmlInputHidden hiddenId = new HtmlInputHidden();
            hiddenId.ID = "ipId";
            hiddenId.Value = IpId.ToString();
            targetContainer.Controls.Add(hiddenId);

            HtmlInputText input = new HtmlInputText();
            input.ID = "inputIPLabel";
            input.Attributes.Add("class", "selectionField");
            input.MaxLength = 100;
            input.Value = _ip.Label.Trim();
            targetContainer.Controls.Add(GetLabelField(eResApp.GetRes(Pref, 223), input)); // Libellé

            input = new HtmlInputText();
            input.ID = "inputIPAddress";
            input.Attributes.Add("class", "selectionField");
            input.Value = _ip.IpAddress.Trim();
            input.MaxLength = 15;
            targetContainer.Controls.Add(GetLabelField(eResApp.GetRes(Pref, 509), input)); // Adresse IP

            input = new HtmlInputText();
            input.ID = "inputIPMask";
            input.Attributes.Add("class", "selectionField");
            input.Value = _ip.Mask.Trim();
            input.MaxLength = 15;
            targetContainer.Controls.Add(GetLabelField(eResApp.GetRes(Pref, 1261), input)); // Masque de sous-réseau

            Boolean bChecked = _ip.ShowLevel();
            Boolean bDisabled = false;
            String chkLink = "nsAdminRights.activeTraitField(this);";

            DropDownList ddl = new DropDownList();
            ddl.ID = "ddlIPLevel";
            ddl.CssClass = "selectionField";
            ddl.Items.Add(new ListItem(eLibTools.GetUserLevelLabel(Pref, UserLevel.LEV_USR_1, false), UserLevel.LEV_USR_1.GetHashCode().ToString()));
            ddl.Items.Add(new ListItem(eLibTools.GetUserLevelLabel(Pref, UserLevel.LEV_USR_2, false), UserLevel.LEV_USR_2.GetHashCode().ToString()));
            ddl.Items.Add(new ListItem(eLibTools.GetUserLevelLabel(Pref, UserLevel.LEV_USR_3, false), UserLevel.LEV_USR_3.GetHashCode().ToString()));
            ddl.Items.Add(new ListItem(eLibTools.GetUserLevelLabel(Pref, UserLevel.LEV_USR_4, false), UserLevel.LEV_USR_4.GetHashCode().ToString()));
            ddl.Items.Add(new ListItem(eLibTools.GetUserLevelLabel(Pref, UserLevel.LEV_USR_5, false), UserLevel.LEV_USR_5.GetHashCode().ToString()));
            ddl.Items.Add(new ListItem(eLibTools.GetUserLevelLabel(Pref, UserLevel.LEV_USR_ADMIN, false), UserLevel.LEV_USR_ADMIN.GetHashCode().ToString()));
            ddl.Items.Add(new ListItem(eLibTools.GetUserLevelLabel(Pref, UserLevel.LEV_USR_NONE, false), UserLevel.LEV_USR_NONE.GetHashCode().ToString()));
            ddl.SelectedIndex = ddl.Items.IndexOf(ddl.Items.FindByValue(_ip.Level.ToString()));
            ddl.Enabled = bChecked;
            ddl.Attributes.Add("active", ddl.Enabled ? "1" : "0");
            targetContainer.Controls.Add(GetCheckBox("chkLevel", eResApp.GetRes(Pref, 8658), chkLink, bChecked, bDisabled, ddl)); //TODO RES

            bChecked = _ip.ShowUser();
            bDisabled = false;
            TextBox lbUsers = new TextBox();
            lbUsers.ID = "lbIPUsers";
            lbUsers.Attributes.Add("class", "selectionField");
            lbUsers.Attributes.Add("active", bChecked ? "1" : "0");
            lbUsers.Attributes.Add("onclick", "top.nsAdmin.showUsersCatInIP(this);");
            lbUsers.Attributes.Add("ednvalue", _ip.User);
            lbUsers.Text = _ip.GetUserLabel(Pref);
            lbUsers.ReadOnly = true;
            targetContainer.Controls.Add(GetCheckBox("chkGroupsAndUsers", eResApp.GetRes(Pref, 8657) , chkLink, _ip.ShowUser(),  bDisabled, lbUsers)); // TODORES

            //HtmlGenericControl icon = new HtmlGenericControl();
            //icon.Attributes.Add("class", "icon-catalog");
            //icon.Attributes.Add("onclick", "top.nsAdmin.showUsersCatInIP(this);");
            //targetContainer.Controls.Add(icon);

            HtmlInputHidden hiddenPermId = new HtmlInputHidden();
            hiddenPermId.ID = "permId";
            hiddenPermId.Value = _ip.PermissionId.ToString();
            targetContainer.Controls.Add(hiddenPermId);

            switch (FieldToFocus)
            {
                case "label":
                    FieldToFocus = "inputIPLabel";
                    break;
                case "address":
                    FieldToFocus = "inputIPAddress";
                    break;
                case "mask":
                    FieldToFocus = "inputIPMask";
                    break;
                case "usergroups":
                    FieldToFocus = "lbIPUsers";
                    break;
                case "level":
                    FieldToFocus = "ddlIPLevel";
                    break;
                default:
                    FieldToFocus = String.Empty;
                    break;
            }
            if (FieldToFocus.Length > 0)
            {
                HtmlGenericControl javaScript = new HtmlGenericControl("script");
                javaScript.Attributes.Add("type", "text/javascript");
                javaScript.Attributes.Add("language", "javascript");
                javaScript.InnerHtml = String.Concat(
                    "if (document.getElementById('", FieldToFocus, "')) { ",
                        "document.getElementById('", FieldToFocus, "').focus();",
                        "document.getElementById('", FieldToFocus, "').select();",
                     "}");
                _pgContainer.Controls.Add(javaScript);
            }

            return base.Build();
        }
    }
}