using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Engine.ORM;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe de rendu du bloc "Catalogue associé" des paramètres de la rubrique
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eda.eAdminBlockRenderer" />
    public class eAdminFieldCatalogRenderer : eAdminBlockRenderer
    {
        private Int32 _descid;
        private eAdminFieldInfos _field;

        private eAdminFieldCatalogRenderer(ePref pref, eAdminFieldInfos field)
            : base(pref, null, eResApp.GetRes(pref, 7370), idBlock: "blockCatalog")
        {
            _descid = field.DescId;
            _field = field;
        }


        /// <summary>
        /// Instancie un objet eAdminFieldCatalogRenderer
        /// </summary>
        /// <param name="pref">pref user</param>
        /// <param name="field">champ dont on veut le rendu du bloc admin</param>
        /// <returns></returns>
        public static eAdminFieldCatalogRenderer CreateAdminFieldCatalogRenderer(ePref pref, eAdminFieldInfos field)
        {
            eAdminFieldCatalogRenderer features = new eAdminFieldCatalogRenderer(pref, field);
            return features;
        }

        /// <summary>Construction du bloc "Catalogue associé"</summary>
        /// <returns></returns>
        protected override bool Build()
        {
            base.Build();


            OrmMappingInfo ormInfos = eLibTools.OrmLoadAndGetMapWeb(_ePref);


            Boolean userAllowed = _field.IsUserAllowedToUpdate()
                && !eAdminTools.IsSpecialField(_field)
                ;


            eAdminField adminField;

          

            // Saisie par catalogue
            if (_field.Format != FieldFormat.TYP_USER)
            {
                adminField = new eAdminCheckboxField(_descid, eResApp.GetRes(Pref, 242), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.POPUP.GetHashCode(), value: _field.PopupType != PopupType.NONE || _field.Format == FieldFormat.TYP_USER);
                adminField.ReadOnly = !userAllowed;
                adminField.Generate(_panelContent);
            }

            // Catalogue avancé
            //adminField = new eAdminCheckboxField(_descid, "Catalogue avancé", eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC..GetHashCode(), value: _field.PopupType != PopupType.NONE);
            //adminField.Generate(_panelContent);

            // Choix multiple
            Dictionary<String, String> dic = new Dictionary<String, String>();
            dic.Add("0", eResApp.GetRes(Pref, 7329));
            dic.Add("1", eResApp.GetRes(Pref, 7372));
            adminField = new eAdminRadioButtonField(_descid, eResApp.GetRes(Pref, 7371), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.MULTIPLE.GetHashCode(), "catIsMulti", dic,
                value: _field.Multiple ? "1" : "0");
            adminField.IsLabelBefore = true;
            adminField.ReadOnly = !userAllowed || ormInfos.GetAllMappedDescid.Contains(_field.DescId);
            adminField.Generate(_panelContent);

            // Arborescent
            if (_field.Multiple || _field.Format == FieldFormat.TYP_USER)
            {
                String value = "";
                dic = new Dictionary<String, String>();
                dic.Add("0", eResApp.GetRes(Pref, 179));
                dic.Add("1", eResApp.GetRes(Pref, 7331));

                if (_field.Format == FieldFormat.TYP_USER)
                {
                    //value = _field.TreeViewUserList ? "1" : "0";
                    //adminField = new eAdminRadioButtonField(_descid, eResApp.GetRes(Pref, 7374), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.TREEVIEWUSERLIST.GetHashCode(), "catIsArbo", dic, value: value);
                }
                else
                {
                    eAdminFiledataParam fdParam = _field.GetFileDataParam(Pref);
                    value = fdParam.TreeView ? "1" : "0";
                    adminField = new eAdminRadioButtonField(_descid, eResApp.GetRes(Pref, 7374), eAdminUpdateProperty.CATEGORY.FILEDATAPARAM, eLibConst.FILEDATAPARAM.TreeView.GetHashCode(), "catIsArbo", dic, value: value);
                    adminField.ReadOnly = !userAllowed;
                    adminField.IsLabelBefore = true;
                    adminField.Generate(_panelContent);
                }

            }

            // Source catalogue
            if (_field.Format != FieldFormat.TYP_USER)
            {
                List<ListItem> list = new List<ListItem>();

                List<eFieldRes> fields = _field.GetLinkedCatFields();

                foreach (eFieldRes f in fields)
                {
                    list.Add(f.ToListItem());
                }
                // Ajout de <Vide> qui sera le popupdescid
                list.Insert(0, new ListItem($"<{ eResApp.GetRes(_ePref, 141) }>", _descid.ToString()));


                string sAddedCss = "";

                if (_field.PopupDescId != _field.DescId && !list.Exists(ff => ff.Value == _field.PopupDescId.ToString()))
                {

                    var a = new HtmlGenericControl("span");
                    a.InnerText = (eResApp.GetRes(Pref, 1484));
                    a.Attributes.Add("class", "warnmsg");

                    /*var z = list.Find(zz => zz.Value == "0");
                    z.Attributes.CssStyle.Add("background-color", "red");
                    */
                    sAddedCss = "select_red";
                    _panelContent.Controls.Add(a);
                }

                adminField = new eAdminDropdownField(
                    _ePref,
                    _descid,
                    eResApp.GetResWithColon(Pref, 7373),
                    eAdminUpdateProperty.CATEGORY.DESC,
                    eLibConst.DESC.POPUPDESCID.GetHashCode())
                {
                    Items = list.ToArray(),
                    TooltipText = eResApp.GetRes(Pref, 7375),
                    Value = _field.PopupDescId.ToString(),
                    RenderType = eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE,
                    CustomLabelCSSClasses = String.Concat("info", " ", sAddedCss),
                    IsOptional = true,
                    ReadOnly = !userAllowed
                };
                adminField.Generate(_panelContent);

                //Lien Paramètres Catalogue avancé
                adminField = new eAdminButtonField(eResApp.GetRes(Pref, 7286), "btnRightRule", String.Concat(eResApp.GetRes(Pref, 7286), " « ", _field.Labels[Pref.LangId], " »."),
                        String.Concat("nsAdmin.confAdvancedCatalog(this, ", _field.DescId, ", '", HttpUtility.JavaScriptStringEncode(_field.Labels[Pref.LangId]), "')")
                    );
                adminField.Generate(_panelContent);
            }

            return true;
        }
    }
}