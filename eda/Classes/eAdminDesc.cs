using System;
using System.Collections.Generic;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;

namespace Com.Eudonet.Xrm.eda
{



    /// <summary>
    /// Classe métier eAdminDesc pour gérer la table DESC
    /// </summary>
    /// <author>CRU</author>
    /// <remarks>12/11/2015</remarks>
    public class eAdminDesc : eAdminDescInternal, IPermSaver
    {

        /// <summary>
        /// Constructeur avec argument DescId
        /// </summary>
        public eAdminDesc(Int32 descid, Int32 parentTab = 0) : base(descid, parentTab)
        {
        }






        /// <summary>
        /// On retire les rubriques appartenant à la table "tabToRemove"
        /// </summary>
        /// <param name="tabToRemove">Table à retirer</param>
        /// <param name="arrHeader300">Rubriques entête gauche</param>
        /// <param name="arrHeader200">Rubriques entête droite</param>
        private void RemoveFieldsFromSelection(int tabToRemove, ref List<String> arrHeader300, ref List<String> arrHeader200)
        {
            arrHeader300.RemoveAll(item => eLibTools.GetNum(item) / 100 * 100 == tabToRemove);
            arrHeader200.RemoveAll(item => eLibTools.GetNum(item) / 100 * 100 == tabToRemove);
        }


    }


}