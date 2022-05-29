using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// 
    /// </summary>
    public class eAdminFieldCheckBoxPresentation
        : eAdminBlockRenderer
    {
        private int _descid;
        private eAdminFieldInfos _field;

        /// <summary>
        /// Initialise la classe avec les éléments à afficher.
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="field"></param>
        private eAdminFieldCheckBoxPresentation(ePref pref, eAdminFieldInfos field)
            : base(pref, null, eResApp.GetRes(pref, 8862), idBlock: "blockRubric")
        {
            _descid = field.DescId;
            _field = field;
        }

        /// <summary>
        /// appelle le constructeur pour initialiser la classe avec les éléments à afficher.
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static eAdminFieldCheckBoxPresentation CreateAdminFieldCheckBoxPresentation(ePref pref, eAdminFieldInfos field)
            => new eAdminFieldCheckBoxPresentation(pref, field);


        /// <summary>Construction du bloc Caractéristiques</summary>
        /// <returns></returns>
        protected override bool Build()
        {
            base.Build();

            Dictionary<string, string> dic = new Dictionary<string, string> {
                { LOGIC_DISPLAY_TYPE.CHECKBOX.ToString("d"), eResApp.GetRes(Pref, 2204)},
                { LOGIC_DISPLAY_TYPE.SWITCH.ToString("d"), eResApp.GetRes(Pref, 8863)}};

            eAdminField rb = new eAdminRadioButtonField(_field.DescId, eResApp.GetRes(Pref, 8862), 
                eAdminUpdateProperty.CATEGORY.DESCADV, DESCADV_PARAMETER.LOGIC_DISPLAY_TYPE.GetHashCode(), "rbLogicDisplayType", dic,
                 tooltiptext: eResApp.GetRes(Pref, 8862), value: _field.LogicFieldTypeDisplay, valueFormat: FieldFormat.TYP_BIT);

            rb.IsLabelBefore = true;
            rb.Generate(_panelContent);

            return true;
        }
    }
}