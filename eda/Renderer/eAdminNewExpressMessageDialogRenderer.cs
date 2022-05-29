using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminNewExpressMessageDialogRenderer : eAdminModuleRenderer
    {
        private eAdminHomepage _hpExpressMessage { get; set; }
        private eAdminNewExpressMessageDialogRenderer(ePref pref, eAdminHomepage hpExpressMessage) : base(pref)
        {
            this._hpExpressMessage = hpExpressMessage;
            Pref = pref;
        }


        public static eAdminNewExpressMessageDialogRenderer CreateAdminNewExpressMessageDialogRenderer(ePref pref, eAdminHomepage hpExpressMessage)
        {
            return new eAdminNewExpressMessageDialogRenderer(pref, hpExpressMessage);
        }


        protected override bool Build()
        {

            _pgContainer.ID = "NewExpressMessageModalContent";
            _pgContainer.Attributes.Add("class", "adminModalContent");

            Panel innerContentGlobal = new Panel();
            innerContentGlobal.CssClass = "inner-cases-lineGlobaleExM";
            _pgContainer.Controls.Add(innerContentGlobal);


            HtmlGenericControl label = new HtmlGenericControl("div");
            label.InnerHtml = String.Concat(eResApp.GetRes(Pref, 223), " :");// "Libellé :";
            label.Attributes.Add("title", eResApp.GetRes(Pref, 223));
            label.Attributes.Add("class", "labelFieldExM");
            innerContentGlobal.Controls.Add(label);



            HtmlInputText input = new HtmlInputText();
            input.ID = "txtNewExpressMessageName";
            input.Attributes.Add("class", "selectionField");
            input.Attributes.Add("autofocus", "true");
            input.Value = String.IsNullOrEmpty(_hpExpressMessage?.Label) ? eResApp.GetRes(Pref, 7609) : _hpExpressMessage.Label; // Nouveau message expresse

            innerContentGlobal.Controls.Add(input);

            // _pgContainer.Controls.Add(GetLabelField(eResApp.GetRes(_ePref, 223), input)); // Libellé :

            Panel caseContentCKEditor = new Panel();
            caseContentCKEditor.ID = "innerCasesTextArea";
            caseContentCKEditor.Attributes.Add("class", "inner-casesExM innerCasesTextArea");
            _pgContainer.Controls.Add(caseContentCKEditor);

            //Champ mémo
            AddMemoEditorBlock(caseContentCKEditor);

            Panel caseContent = new Panel();
            caseContent.ID = "innerCasesUsers";
            caseContent.Attributes.Add("class", "inner-casesExM");
            _pgContainer.Controls.Add(caseContent);

            //Utilisateurs
            AddUsersBlock(caseContent);

            return base.Build();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        private void AddUsersBlock(Panel content)
        {
            string idUser = "allUserActive_0";
            string userHiden = "userAllControl";
            string idListeUser = "fldExpressUser_0";

            Panel innerContent = new Panel();
            innerContent.CssClass = "inner-cases-lineUserExM";
            content.Controls.Add(innerContent);
            // Block conséquence


            Panel check = new Panel();
            check.CssClass = "value-containerExM";
            check.Attributes.Add("data-active", "1");
            // Champ caché pour obtenir l'attribut "dsc"
            Control homepagefooter = eAdminFieldBuilder.BuildField(check, AdminFieldType.ADM_TYPE_HIDDEN, "", eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.HIDEHOMEPAGEFOOTER.GetHashCode());
            homepagefooter.ID = userHiden;
            ((TextBox)homepagefooter).Attributes.Add("dbv", "0");

            Dictionary<string, string> dicRB = new Dictionary<string, string>();
            dicRB.Add("0", eResApp.GetRes(Pref, 58));
            dicRB.Add("1", eResApp.GetRes(Pref, 59));


            Dictionary<string, string> dicRBAttributes = new Dictionary<string, string>();
            dicRBAttributes.Add("tabfld", "configdefault");

            eAdminField rb = new eAdminRadioButtonField(_tab, eResApp.GetRes(Pref, 7867),
                eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.HIDEHOMEPAGEFOOTER.GetHashCode(), "UserAll", dicRB, value: ((_hpExpressMessage != null && _hpExpressMessage.IDUsers == "0") || _hpExpressMessage == null) ? "0" : "1",
                onclick: "top.nsAdminHomepages.showHideUser(this," + idUser + "," + userHiden + "," + idListeUser + " );");
            rb.SetFieldControlID("userAllControlId");
            rb.IsLabelBefore = true;
            rb.Generate(check);

            innerContent.Controls.Add(check);


            Panel user = new Panel();
            user.Attributes.Add("data-active", ((_hpExpressMessage != null && _hpExpressMessage.IDUsers == "0") || _hpExpressMessage == null) ? "0" : "1");
            user.ID = idUser;
            // Label
            HtmlGenericControl label = new HtmlGenericControl("div");
            label.InnerHtml = eResApp.GetRes(Pref, 7658);// "Affecté à :";
            label.Attributes.Add("title", eResApp.GetRes(Pref, 195));
            label.Attributes.Add("class", "labelFieldExM");
            user.Controls.Add(label);

            HtmlGenericControl valueContainer = new HtmlGenericControl("div");
            valueContainer.Attributes.Add("class", "value-containerExM");
            //valueContainer.Attributes.Add("data-active", "0");
            //valueContainer.Controls.Add(label);
            user.Controls.Add(valueContainer);

            // Valeur            
            HtmlGenericControl value = new HtmlGenericControl("div");
            value.ID = idListeUser;
            value.InnerText = _hpExpressMessage?.DisplayUsers;
            value.Attributes.Add("class", "field-valueExM");
            value.Attributes.Add("dbv", _hpExpressMessage?.IDUsers);
            valueContainer.Controls.Add(value);

            // le btn 
            HtmlGenericControl btn = new HtmlGenericControl("div");
            btn.Attributes.Add("class", "hpUsers icon-catalog field-catalog");
            btn.Attributes.Add("eaction", "LNKCATUSER");
            btn.Attributes.Add("onclick", "top.nsAdminHomepages.openUserCatalog(this,'" + value.ID + "'," + userHiden + ");");
            user.Controls.Add(btn);

            innerContent.Controls.Add(user);
            //valueContainer.Controls.Add(btn);

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        private void AddMemoEditorBlock(Panel content)
        {
            Panel innerContent = new Panel();
            innerContent.CssClass = "inner-cases-lineExM";
            content.Controls.Add(innerContent);

            HtmlGenericControl label = new HtmlGenericControl("div");
            label.InnerHtml = String.Concat(eResApp.GetRes(Pref, 383), " :");// "Message :";
            label.Attributes.Add("title", eResApp.GetRes(Pref, 383));
            label.Attributes.Add("class", "labelFieldExM");
            innerContent.Controls.Add(label);



            Panel innerContentTextArea = new Panel();
            innerContentTextArea.CssClass = "inner-cases-lineExM inner-txtArea";
            content.Controls.Add(innerContentTextArea);

            // Contrôles HTML : valeur des Notes (textarea) et div conteneur
            HtmlTextArea memoEditorValueControl = new HtmlTextArea();
            memoEditorValueControl.ID = String.Concat("eMemoEditorValue");
            memoEditorValueControl.Style.Add("display", "none");
            memoEditorValueControl.InnerText = _hpExpressMessage?.Content;
            HtmlGenericControl memoEditorContainerControl = new HtmlGenericControl("div");
            memoEditorContainerControl.ID = String.Concat("eExpressMessageMemoEditorContainer");

            memoEditorContainerControl.Attributes.Add("ename", String.Concat("ExpressMessageMemoEditorContainer"));
            memoEditorContainerControl.Attributes.Add("did", "0");
            memoEditorContainerControl.Attributes.Add("fid", "0");
            memoEditorContainerControl.Attributes.Add("html", "1");
            memoEditorContainerControl.Attributes.Add("frominnertext", "1");
            memoEditorContainerControl.Attributes.Add("nbrows", "50");

            innerContentTextArea.Controls.Add(memoEditorValueControl);
            innerContentTextArea.Controls.Add(memoEditorContainerControl);
        }

    }
}