using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eMultiSelectFactory
    {

        /// <summary>
        /// Factory qui permet de selectionner le multiselct en fobnction de type
        /// </summary>
        /// <param name="type">type de multi-select</param>
        /// <param name="pref">preferences utilisateur</param>
        /// <param name="param">Paramètres en brut transmis par le js</param>
        /// <returns></returns>
        public static eMultiSelectDataSourceInterface CreateMultiSelectDataSource(eMultiSelectType type, ePref pref, string param)
        {
            switch (type)
            {
                case eMultiSelectType.WIDGET:
                default:
                    return new eMultiSelectWidgetSource(pref, new eMultiSelectParam(param));
            }
        }
    }
}