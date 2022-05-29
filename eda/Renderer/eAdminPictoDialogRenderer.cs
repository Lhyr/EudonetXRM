using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminPictoDialogRenderer : eAdminRenderer
    {
        private const Int32 NB_PICTO_PER_LINE = 20;
        private Int32 _descid;

        // La font et la coleur sélectionné dans la fenetre
        private eFontIcons.FontIcons _currentIcon;
        private string _currentColor;


        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        private eAdminPictoDialogRenderer(ePref pref, Int32 nTab, Int32 nDescId, eFontIcons.FontIcons icon, string color)
        {
            Pref = pref;
            _tab = nTab;
            _descid = nDescId;
            _currentIcon = icon;
            // couleur par défaut
            _currentColor = color != null && color.Length > 0 ? color : "#636363";

            // Si le descid est rensigné, on prend l'icon ou/et la couleur de la rubrique, sinon celle de la table, sinon celles passés en param
            if (_descid > 0 && _descid != _tab)
            {
                eAdminFieldInfos _fieldInfos = eAdminFieldInfos.GetAdminFieldInfos(pref, _descid);
                _currentColor = _fieldInfos.IconColor.Length > 0 ? _fieldInfos.IconColor : _currentColor;
                _currentIcon = eFontIcons.GetFontIcon(_fieldInfos.Icon);
            }
            else if (_tab > 0)
            {
                eAdminTableInfos _tabInfos = new eAdminTableInfos(pref, _tab);
                _currentColor = _tabInfos.IconColor.Length > 0 ? _tabInfos.IconColor : _currentColor;
                _currentIcon = eFontIcons.GetFontIcon(_tabInfos.Icon);
            }
        }

        public static eAdminPictoDialogRenderer CreateAdminPictoDialogRenderer(ePref pref, Int32 nTab, Int32 nDescId, eFontIcons.FontIcons icon, string color)
        {
            return new eAdminPictoDialogRenderer(pref, nTab, nDescId, icon, color);
        }

        protected override bool Build()
        {
            if (base.Build())
            {
                #region Couleur du picto
                HtmlGenericControl p = new HtmlGenericControl("p");
                p.InnerText = String.Concat(eResApp.GetRes(Pref, 7287), " :");
                _pgContainer.Controls.Add(p);

                eRenderer rdr = eRendererFactory.CreateColorPickerRenderer(Pref, _currentColor);
                _pgContainer.Controls.Add(rdr.PgContainer);

                #endregion

                #region Construction du tableau de pictogrammes
                p = new HtmlGenericControl("p");
                p.InnerText = eResApp.GetRes(Pref, 8131);

                HtmlInputHidden hidSelectedPicto = new HtmlInputHidden();
                hidSelectedPicto.ID = "hidSelectedPicto";


                HtmlGenericControl icon;
                Int32 count = 0;

                HtmlTable table = new HtmlTable();
                table.ID = "tablePicto";
                //table.Style.Add("color", _currentColor);


                HtmlTableRow tr = new HtmlTableRow();
                HtmlTableCell tc;

                //String cssSelected = String.Empty;
                eFontIcons.FontIcons fontIcon;

                foreach (KeyValuePair<String, eFontIcons.FontIcons> font in eFontIcons.GetFonts())
                {
                    count++;

                    icon = new HtmlGenericControl();

                    fontIcon = font.Value;

                    icon.Attributes.Add("class", fontIcon.CssName);
                    icon.Attributes.Add("pictoKey", font.Key);
                    icon.Attributes.Add("title", fontIcon.Libelle);


                    tc = new HtmlTableCell();
                    tc.Controls.Add(icon);
                    tr.Cells.Add(tc);

                    if (_currentIcon.Address.ToLower() == font.Value.Address.ToLower())
                    //  _currentIconKey.ToLower() == font.Key.ToLower())
                    {
                        //cssSelected = " selected";
                        tc.Attributes.Add("data-selected", "1");
                        icon.Style.Add("color", _currentColor);
                        hidSelectedPicto.Value = font.Key;
                    }

                    if (count == NB_PICTO_PER_LINE)
                    {
                        count = 0;
                        table.Rows.Add(tr);
                        tr = new HtmlTableRow();
                    }

                }

                table.Rows.Add(tr);

                _pgContainer.Controls.Add(p);
                _pgContainer.Controls.Add(hidSelectedPicto);
                _pgContainer.Controls.Add(table);

                return true;

                #endregion
            }
            return false;
        }

    }
}