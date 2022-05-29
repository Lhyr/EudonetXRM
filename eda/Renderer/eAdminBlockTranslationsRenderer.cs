using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using System;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminBlockTranslationsRenderer : eAdminBlockRenderer
    {
        /// <summary>
        /// Descid de l'élément concerné (tab/field)
        /// </summary>
        protected Int32 _descid;


        protected eAdminBlockTranslationsRenderer(ePref pref, eAdminTableInfos tabInfos)
            : base(pref, tabInfos, title: eResApp.GetRes(pref, 6816), titleInfo:"", idBlock: "TranslationsPart")
        {
            _descid = tabInfos.DescId;
        }

        protected eAdminBlockTranslationsRenderer(ePref pref, eAdminFieldInfos fieldInfos)
             : base(pref, fieldInfos.Table, title: eResApp.GetRes(pref, 6816), titleInfo: "", idBlock: "TranslationsPart")
        {
            _descid = fieldInfos.DescId;
        }



        public static eAdminBlockTranslationsRenderer CreateAdminBlockTranslationsRenderer(ePref pref, eAdminTableInfos tabInfos)
        {
            eAdminBlockTranslationsRenderer features = new eAdminBlockTranslationsRenderer(pref, tabInfos);
            return features;
        }
        public static eAdminBlockTranslationsRenderer CreateAdminBlockTranslationsRenderer(ePref pref, eAdminFieldInfos fieldInfos)
        {
            eAdminBlockTranslationsRenderer features = new eAdminBlockTranslationsRenderer(pref, fieldInfos);
            return features;
        }


        protected override bool Build()
        {
            base.Build();

            eAdminField btnTranslations = new eAdminButtonField(eResApp.GetRes(Pref, 7716), "btnTranslations", eResApp.GetRes(Pref, 7362), String.Format("nsAdmin.openTranslations({0});", _descid));
            btnTranslations.Generate(_panelContent);

            return true;
        }
    }

}