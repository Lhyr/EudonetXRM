using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using static Com.Eudonet.Internal.eLibConst;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe de gestion du rendu de l'administration des langues
    /// </summary>
    public class eAdminLanguagesDialogRenderer : eAdminRenderer
    {
        private const int NB_LANG_MAX = 10;



        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        private eAdminLanguagesDialogRenderer(ePref pref)
        {
            Pref = pref;
        }

        /// <summary>
        /// Crée l'objet demandé en faisant appel à son constructeur
        /// </summary>
        /// <param name="pref">ePref object</param>
        /// <returns></returns>
        public static eAdminLanguagesDialogRenderer CreateAdminLanguagesDialogRenderer(ePref pref)
        {
            return new eAdminLanguagesDialogRenderer(pref);
        }

        /// <summary>
        /// Construit le html de l'objet demandé
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            TextBox textbox;
            eCheckBoxCtrl checkbox;
            DropDownList ddl;
            eAdminLanguage lang;
            Table table;
            TableRow tr;
            TableCell tc;
            TableHeaderRow thr;
            TableHeaderCell th;
            Panel btnWrapper;
            HtmlGenericControl btn;

            List<eAdminLanguage> languages = eAdminLanguage.Load(Pref);

            table = new Table();
            table.ID = "tableLang";
            PgContainer.Controls.Add(table);

            thr = new TableHeaderRow();
            table.Rows.Add(thr);
            th = new TableHeaderCell();
            th.Text = eResApp.GetRes(Pref, 690);
            thr.Controls.Add(th);
            th = new TableHeaderCell();
            th.Text = eResApp.GetRes(Pref, 7768);
            thr.Controls.Add(th);
            th = new TableHeaderCell();
            th.Text = eResApp.GetRes(Pref, 7769);
            thr.Controls.Add(th);
            th = new TableHeaderCell();
            th.Text = eResApp.GetRes(Pref, 973);
            thr.Controls.Add(th);
            th = new TableHeaderCell();
            th.Text = eResApp.GetRes(Pref, 1417);
            thr.Controls.Add(th);

            for (int i = 0; i <= NB_LANG_MAX; i++)
            {
                lang = languages.Find(l => l.Id == i);
                if (lang != null)
                {
                    tr = new TableRow();
                    tr.ID = String.Concat("lang_", i);
                    tr.Attributes.Add("class", "blockLang");
                    tr.Attributes.Add("langId", i.ToString());
                    table.Rows.Add(tr);

                    tc = new TableCell();
                    tr.Cells.Add(tc);
                    checkbox = new eCheckBoxCtrl(lang.Disabled, false);
                    checkbox.AddClick("nsAdminLang.updateLang(this);");
                    checkbox.ID = String.Concat("chkLang_", i);
                    checkbox.Attributes.Add("dsc", ((int)MAPLANG_FIELD.DISABLED).ToString());
                    tc.Controls.Add(checkbox);

                    tc = new TableCell();
                    tr.Cells.Add(tc);
                    textbox = new TextBox();
                    textbox.ID = String.Concat("txtLang_", i);
                    textbox.CssClass = "txtLang";
                    textbox.Attributes.Add("placeholder", String.Concat(eResApp.GetRes(Pref, 6746), " ", i));
                    textbox.Text = lang.Label;
                    textbox.Attributes.Add("dsc", ((int)MAPLANG_FIELD.LANG_LABEL).ToString());
                    tc.Controls.Add(textbox);

                    tc = new TableCell();
                    tr.Cells.Add(tc);
                    ddl = GetSystemLanguages();
                    ddl.ID = String.Concat("ddlSysLang_", i);
                    ddl.SelectedValue = lang.SysId.ToString();
                    ddl.Attributes.Add("dsc", ((int)MAPLANG_FIELD.LANG_SYSID).ToString());
                    tc.Controls.Add(ddl);

                    tc = new TableCell();
                    tr.Cells.Add(tc);
                    textbox = new TextBox();
                    textbox.ID = String.Concat("txtCode_", i);
                    textbox.CssClass = "txtCode";
                    textbox.ReadOnly = true;
                    textbox.Enabled = false;
                    textbox.Text = lang.Code;
                    tc.Controls.Add(textbox);

                    tc = new TableCell();
                    tr.Cells.Add(tc);
                    checkbox = new eCheckBoxCtrl(lang.IsDefault, lang.Disabled);
                    checkbox.AddClick("nsAdminLang.updateDefault(this);");
                    checkbox.ID = String.Concat("chkLangDefault_", i);
                    checkbox.Attributes.Add("dsc", ((int)MAPLANG_FIELD.LANG_DEFAULT).ToString());
                    tc.Controls.Add(checkbox);

                    // Bouton "Supprimer" seulement pour les ressources a partir de LANG_06
                    //if (i > 5)
                    //{
                    //    btn = new HtmlGenericControl();
                    //    btn.Attributes.Add("class", "icon-delete");
                    //    btn.Attributes.Add("title", eResApp.GetRes(Pref, 7776));
                    //    tc.Controls.Add(btn);
                    //}

                }

            }

            return true;
        }

        /// <summary>
        /// Génére une dropdownlist contenant les langues systèmes
        /// </summary>
        /// <returns></returns>
        private DropDownList GetSystemLanguages()
        {
            DropDownList ddl = new DropDownList();

            ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 293), ""));
            foreach (SYS_LANG lang in Enum.GetValues(typeof(SYS_LANG)))
            {
                ddl.Items.Add(new ListItem(
                    eLibTools.GetSysLangLabel(Pref, lang),
                    ((int)lang).ToString()
                    ));
            }
            return ddl;
        }
    }
}