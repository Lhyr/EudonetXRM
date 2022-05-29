using Com.Eudonet.Internal;
using Com.Eudonet.Internal.tools.filler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.DataFields
{
    /// <summary>
    /// Champ 'score'de campagne
    /// </summary>
    public class CampaignScoreFieldModel : NumericDataFieldModel
    {
        /// <summary>
        /// tooltip sur la valeur
        /// </summary>
        [DefaultValue("")]
        public string ValueToolTipText;

        internal CampaignScoreFieldModel(eFieldRecord f) : base(f)
        {

            if (f.ExtendedProperties != null && f.ExtendedProperties is ExtendedScoreDetail)
                ValueToolTipText = ((ExtendedScoreDetail)f.ExtendedProperties).ToolTip.Replace("[[BR]]", "\n");
        }
    }
}