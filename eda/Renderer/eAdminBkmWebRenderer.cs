using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminBkmWebParameterRenderer : eAdminWebTabParameterRenderer
    {
        Int32 iBkm = 0;
        eAdminBkmWeb adminBkmWeb;

        protected eAdminBkmWebParameterRenderer(ePref pref, Int32 nTab, Int32 bkm)
            : base(pref, nTab, 0)
        {
            iBkm = bkm;
            _blockTitle = eResApp.GetRes(Pref, 7388);
        }

        /// <summary>
        /// Retourne un eAdminBkmWebParameterRenderer
        /// </summary>
        /// <param name="pref">préférence user</param>
        /// <param name="nTab">Table de la spécif</param>
        /// <param name="nBkmTab">Descid du signet de la spécif</param>
        /// <returns></returns>
        public static eAdminBkmWebParameterRenderer GetAdminBkmWebParameterRenderer(ePref pref, Int32 nTab, Int32 nBkmTab)
        {
            eAdminBkmWebParameterRenderer a = new eAdminBkmWebParameterRenderer(pref, nTab, nBkmTab);

            return a;
        }



        eAdminTableInfos TabInfos;
        eAdminTableInfos ParentTabInfos;

        protected override Boolean InitProperties()
        {

            TabInfos = new eAdminTableInfos(Pref, iBkm);
            ParentTabInfos = new eAdminTableInfos(Pref, _tab);

            adminBkmWeb = new eAdminBkmWeb(Pref, iBkm);
            adminBkmWeb.Generate();
            _specif = adminBkmWeb.Spec;

            return true;
        }

        protected override void SetLabels()
        {
            base.SetLabels();

            _labelName = eResApp.GetRes(Pref, 7724);
            _labelURL = eResApp.GetRes(Pref, 7725);
            _labelAdminURL = eResApp.GetRes(Pref, 7723);

            _tooltipURL = eResApp.GetRes(Pref, 7726);
        }


        protected override void BuildTitle()
        {
            base.BuildTitle();

            String tabTooltip = String.Empty;
            String tabType = eAdminTools.GetTabTypeName(TabInfos, Pref, out tabTooltip);

            HtmlGenericControl panelTab = new HtmlGenericControl("p");
            panelTab.Attributes.Add("class", "info");
            panelTab.InnerText = tabType;
            panelTab.Attributes.Add("title", tabTooltip);

            _pgContainer.Controls.Add(panelTab);
        }

        protected override void BuildPanelContent()
        {

            _panelContent = new Panel();
            _panelContent.CssClass = "paramBlockContent " + _idPart;
            _panelContent.ID = _idPart;
            _panelContent.Attributes.Add("data-active", "1");
            _panelContent.Attributes.Add("eactive", "1");

            if (_specif != null)
                _panelContent.Attributes.Add("fid", _specif.SpecifId.ToString());

            _pgContainer.Controls.Add(_panelContent);
        }



        /// <summary>
        /// Construction du menu de paramètre des signets web
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {

            BuildTitle();

            BuildPanelContent();

            //Bloc caractéristiques -> dns un sous bloc
            eAdminRenderer renderer = eAdminRendererFactory.CreateAdminBlockRenderer(Pref, TabInfos, eResApp.GetRes(Pref, 6809), "", "", true);
            _panelContent.Controls.Add(renderer.PgContainer);
            BuildParametersContent(((eAdminBlockRenderer)renderer).PanelContent);

            //Bloc droits
            renderer = eAdminRendererFactory.CreateAdminRightsAndRulesRenderer(Pref, TabInfos, ParentTabInfos);
            _panelContent.Controls.Add(renderer.PgContainer);

            //Langues
            renderer = eAdminRendererFactory.CreateAdminBlockTranslationsRenderer(Pref, TabInfos);
            _panelContent.Controls.Add(renderer.PgContainer);


            return true;
        }



        ///// <summary>
        ///// Retourne le panel des caractèristiques
        ///// </summary>
        ///// <returns></returns>
        //protected override Panel GetPanelContent()
        //{
        //    Panel panelContent = base.GetPanelContent();
        //    panelContent.CssClass = "paramBlockContent ";
        //    return panelContent;
        //}


        protected override eAdminField CreateLabelField()
        {
            eAdminField field = new eAdminTextboxField(_tab, _labelName, eAdminUpdateProperty.CATEGORY.RES, Pref.LangId, EudoQuery.AdminFieldType.ADM_TYPE_CHAR, value: adminBkmWeb.Labels[Pref.LangId]);
            field.Generate(_panelParametersContent);
            field.SetControlAttribute("did", iBkm.ToString());
            ((WebControl)field.FieldControl).ID = "admWebTabLabel";
            return field;
        }

        protected override eAdminField CreateUrlField()
        {
            eAdminField field;
            if (adminBkmWeb.IsSpecif)
            {

                field = new eAdminTextboxField(_tab, _labelURL, _updateCat, eLibConst.SPECIFS.URL.GetHashCode(), EudoQuery.AdminFieldType.ADM_TYPE_CHAR, value: _specif.Url.ToString());
                field.Generate(_panelParametersContent);
                AddAttributeSpecifID(field);
            }
            else
            {
                field = new eAdminTextboxField(_tab, _labelURL, eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.DEFAULT.GetHashCode(), EudoQuery.AdminFieldType.ADM_TYPE_CHAR,
                    value: adminBkmWeb.Url.ToString());
                field.Generate(_panelParametersContent);
                field.SetControlAttribute("did", iBkm.ToString());


            }
            return field;
        }

        protected override void AddAttributeSpecifID(eAdminField field)
        {
            if (adminBkmWeb.IsSpecif)
                base.AddAttributeSpecifID(field);
        }


        protected override eAdminField CreateUrlParamField()
        {
            if (!adminBkmWeb.IsSpecif)
                return null;

            return base.CreateUrlParamField();

        }

        protected override eAdminField CreateAdminUrlField()
        {
            if (!adminBkmWeb.IsSpecif)
                return null;


            eAdminIconField icon = new eAdminIconField("urladminbtn", "icon-cloud-upload", "adminFieldParamBtn",
                string.Concat("nsAdmin.openCloudLink(this,1)")

                );


            eAdminField field = new eAdminTextboxField(_tab, _labelAdminURL, _updateCat, eLibConst.SPECIFS.ADMINURL.GetHashCode(), EudoQuery.AdminFieldType.ADM_TYPE_CHAR,
                value: _specif.UrlAdmin, icon: icon);
            field.Generate(_panelParametersContent);


            AddAttributeSpecifID(field);
            return field;
        }

        protected override eAdminField CreatePositionField()
        {
            return null;
        }


        //protected override Control GetIsSpecifParam(Panel panelContent)
        //{
        //    if (adminBkmWeb.IsSpecif)
        //        return null;

        //    eCheckBoxCtrl cb = new eCheckBoxCtrl(adminBkmWeb.IsSpecif, adminBkmWeb.IsSpecif);
        //    panelContent.Controls.Add(cb);
        //    cb.AddText(eResApp.GetRes(_ePref, 653));
        //    cb.AddClick("nsAdminBkmWeb.switchIsSpecif(this);");
        //    cb.Attributes.Add("bkm", iBkm.ToString());
        //    return cb;
        //}

        protected override void CreateTranslationsField()
        {
            return;
        }

    }
}