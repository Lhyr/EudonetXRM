using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Factories;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type catalogue
    /// </summary>
    public class AliasFieldInfos : FldTypedInfosModel
    {
        /// <summary>
        /// propriété du champs source de l'alias
        /// </summary>
        public IFldTypedInfosModel AliasSourceField;

        internal AliasFieldInfos(Field f, ePref pref, eResInternal oRes, DescAdvDataSet oDescAdv, IDictionary<int, RGPDModel> rgpdData) : base(f)
        {

            if (f.AliasSourceField == null)
                throw new EudoException($"Le champ de type Alias {f.Libelle} ({f.Descid}) n'a pas de champ source correctement défini");

            if (f.AliasSourceField.Format == FieldFormat.TYP_ALIAS)
                throw new EudoException($"Le champ source {f.AliasSourceField.Descid} de l'alias {f.Descid} est aussi un champ de type Alias, ce qui risque de provoquer une boucle infinie");

            Format = FieldType.Alias;

            AliasSourceField = FldTypedInfosFactory.InitFldTypedInfosFactory(
                f.AliasSourceField,
                pref,
                rgpdM: rgpdData.ContainsKey(f.AliasSourceField.Descid) ? rgpdData[f.AliasSourceField.Descid] : null
            ).GetFldTypedInfos(oRes, oDescAdv, rgpdData);
        }

    }
}