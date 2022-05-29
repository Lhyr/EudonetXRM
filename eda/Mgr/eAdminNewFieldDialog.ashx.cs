using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminNewFieldDialog
    /// </summary>
    public class eAdminNewFieldDialog : eAdminManager
    {
        int _nTab = 0;
        string _sError = "";
        protected override void ProcessManager()
        {
            AddHeadAndBody = true;

            #region ajout des css et js

            PageRegisters.RegisterFromRoot = true;
            PageRegisters.RegisterAdminIncludeScript("eAdminNewFieldDialog");
            PageRegisters.RegisterAdminIncludeScript("eAdmin");
            PageRegisters.AddScript("eTools");

            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eAdminMenu");
            PageRegisters.AddCss("eAdminNewFieldDialog");

            BodyCssClass = "adminModal";

            #endregion


            #region récupération des variables
            _nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            TableType tt = _requestTools.GetRequestFormEnum<TableType>("tt");
            #endregion
            #region recherche des id disponibles
            HashSet<int> hsFreeDescIds = new HashSet<int>();
            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            dal.OpenDatabase();
            try
            {
                hsFreeDescIds = eSqlDesc.GetFreeDescIds(dal, _nTab, out _sError);
                if (_sError.Length > 0)
                    LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 7657), title: " ", devMsg: _sError));

            }
            catch (Exception e)
            {
                _sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 7657), title: " ", devMsg: _sError));

            }
            finally
            {
                dal?.CloseDatabase();
            }

            #endregion



            Panel pgContainer = new Panel();
            pgContainer.ID = "NewTabModalContent";
            pgContainer.CssClass = "adminModalContent";

            Panel field = new Panel();
            pgContainer.Controls.Add(field);
            field.CssClass = "field";
            field.ID = "fltLabel";

            HtmlGenericControl label = new HtmlGenericControl("label");
            label.InnerText = eResApp.GetRes(_pref, 7610);
            field.Controls.Add(label);

            HtmlInputText input = new HtmlInputText();
            field.Controls.Add(input);
            input.ID = "txtNewFieldLabel";
            input.Attributes.Add("class", "selectionField");
            input.Value = eResApp.GetRes(_pref, 6929); 

            field = new Panel();
            pgContainer.Controls.Add(field);
            field.CssClass = "field";

            label = new HtmlGenericControl("label");
            label.InnerText = eResApp.GetRes(_pref, 7884);
            field.Controls.Add(label);

            DropDownList ddl = new DropDownList();
            field.Controls.Add(ddl);
            ddl.ID = "ddlNewFieldDid";
            ddl.CssClass = "selectionField";
            string sShortField = "";
            switch (tt)
            {
                case TableType.PP:
                    sShortField = "PP";
                    break;
                case TableType.PM:
                    sShortField = "PM";
                    break;
                case TableType.ADR:
                    sShortField = "ADR";
                    break;
                case TableType.EVENT:
                    sShortField = "EVT";
                    break;
                case TableType.TEMPLATE:
                    sShortField = "TPL";
                    break;
            }

            foreach (int did in hsFreeDescIds)
            {
                ListItem li = new ListItem(String.Format("{0}{1}", sShortField, (did % 100).ToString("00")), did.ToString());
                ddl.Items.Add(li);
            }

            field = new Panel();
            pgContainer.Controls.Add(field);
            field.CssClass = "field";

            label = new HtmlGenericControl("label");
            label.InnerText = eResApp.GetRes(_pref, 7885);
            field.Controls.Add(label);

            ddl = new DropDownList();
            field.Controls.Add(ddl);
            ddl.ID = "ddlNewFieldType";
            ddl.CssClass = "selectionField";

            ddl.Items.AddRange(eAdminFieldsParametersRenderer.GetFieldsTypesListItems(_pref, hsetNotTheseFormats: new HashSet<FieldFormat>() { FieldFormat.TYP_ALIASRELATION }, bDisplayLogin: true).ToArray());
            ddl.Attributes.Add("dsc", String.Format("{0}|{1}", (int)eAdminUpdateProperty.CATEGORY.DESC, (int)eLibConst.DESC.FORMAT));

            ddl.SelectedValue = ((int)FieldFormat.TYP_CHAR).ToString();

            RenderResultHTML(pgContainer);
        }
    }
}