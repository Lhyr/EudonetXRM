using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminNewTabDialogRenderer : eAdminModuleRenderer
    {
        private eAdminNewTabDialogRenderer(ePref pref) : base(pref)
        {
            Pref = pref;
        }

        public static eAdminNewTabDialogRenderer CreateAdminNewTabDialogRenderer(ePref pref)
        {
            return new eAdminNewTabDialogRenderer(pref);
        }


        protected override bool Build()
        {
            _pgContainer.ID = "NewTabModalContent";
            _pgContainer.Attributes.Add("class", "adminModalContent");

            HtmlInputText input = new HtmlInputText();
            input.ID = "txtNewTabName";
            input.Attributes.Add("class", "selectionField");
            input.Attributes.Add("autofocus", "true");
            input.Value = eResApp.GetRes(Pref, 7203); // Nouvel onglet
            _pgContainer.Controls.Add(GetLabelField(eResApp.GetRes(Pref, 7201), input)); // Titre de l'onglet :

            DropDownList ddl = new DropDownList();
            ddl.ID = "ddlNewTabType";
            ddl.CssClass = "selectionField";
            Array tabTypes = Enum.GetValues(typeof(EdnType));
            ListItem li;
            List<ListItem> list = new List<ListItem>();

            bool bGridEnabled = Pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[]
                { eLibConst.CONFIG_DEFAULT.GRIDENABLED })[eLibConst.CONFIG_DEFAULT.GRIDENABLED] == "1";

            foreach (EdnType ednType in tabTypes)
            {
                switch (ednType)
                {
                    case EdnType.FILE_HISTO:
                    case EdnType.FILE_USER:
                    case EdnType.FILE_ADR:
                    case EdnType.FILE_PJ:
                    case EdnType.FILE_FILTER:
                    case EdnType.FILE_REPORT:
                    case EdnType.FILE_MAILTEMPLATE:
                    case EdnType.FILE_BKMWEB:
                    case EdnType.FILE_FORMULARXRM:
                    // case EdnType.FILE_WEBTAB:
                    case EdnType.FILE_GRID:
                    case EdnType.FILE_SUBQUERY:
                    case EdnType.FILE_SYSTEM:
                    case EdnType.FILE_DISCUSSION:
                    case EdnType.FILE_HOMEPAGE:
                    case EdnType.FILE_WIDGET:
                    case EdnType.FILE_OBSOLETE:
                        continue;
                        break;
                    default:
                        break;
                }

                String tabTooltip = String.Empty;
                String tabType = eAdminTools.GetTabTypeName(ednType, Pref, out tabTooltip);
                if (tabType.Length > 0 && (ednType != EdnType.FILE_GRID || (ednType == EdnType.FILE_GRID && bGridEnabled)))
                {
                    li = new ListItem(tabType, ((int)ednType).ToString());
                    list.Add(li);
                    li.Attributes.Add("title", tabTooltip);
                }
            }

            //rajout d'Onglet Web Externe
            //li = new ListItem(eResApp.GetRes(Pref, 7969), ((int)EdnType.FILE_WEBTAB).ToString());
            //li.Attributes.Add("specType", ((int)eLibConst.SPECIF_TYPE.TYP_WEBTAB_EXTERNAL).ToString());
            //list.Add(li);
            //li.Attributes.Add("title", eResApp.GetRes(Pref, 7970));

            ddl.Items.AddRange(list.OrderBy(item => item.Text).ToArray());
            ddl.SelectedValue = ((int)EdnType.FILE_MAIN).ToString();
            _pgContainer.Controls.Add(GetLabelField(eResApp.GetRes(Pref, 7202), ddl)); // Type de l'onglet :

            return base.Build();
        }


        #region old code non used à supprimer kha 26/01/2017
        ///// <summary>Création de la ligne de Checkbox</summary>
        ///// <param name="idCheckbox">The identifier checkbox.</param>
        ///// <param name="label">The label.</param>
        ///// <param name="bChecked">if set to <c>true</c> [b checked].</param>
        ///// <param name="bDisabled">if set to <c>true</c> [b disabled].</param>
        ///// <param name="control">Contrôle à ajouter</param>
        ///// <returns></returns>
        //private Panel BuildCheckBox(String idCheckbox, String label, Boolean bChecked, Boolean bDisabled, Control control)
        //{
        //    Panel panel = new Panel();
        //    panel.CssClass = "field";

        //    Panel chkField = new Panel();
        //    chkField.CssClass = "checkboxField";

        //    chkField.Attributes.Add("onclick", "top.nsAdmin.activeTraitField(this);");

        //    eCheckBoxCtrl chkCtrl = new eCheckBoxCtrl(bChecked, bDisabled);
        //    chkCtrl.ID = idCheckbox;
        //    chkCtrl.AddClick();
        //    chkCtrl.AddText(label);

        //    chkField.Controls.Add(chkCtrl);

        //    panel.Controls.Add(chkField);
        //    panel.Controls.Add(control);

        //    return panel;
        //}

        #endregion
    }
}