using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{

    /// <summary>
    /// Droits sur les doublons
    /// Les droits administrable sont bcp plus restreint
    /// </summary>
    public class eAdminDescDblTreatmentRightCollection : eAdminDescTreatmentRightCollection
    {
        /// <summary>
        /// Constructeur standard, appel le base et set les paramètre "const"
        /// </summary>
        /// <param name="pref"></param>
        public eAdminDescDblTreatmentRightCollection(ePref pref) : base(pref)
        {



            this.Tab = (int)TableType.DOUBLONS;
            this.Field = (int)TableType.DOUBLONS;

            //Les droits sur les doublons ne sont pas contextualisable
            this.From = 0;

        }


 
    }
}