using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.DataFields
{
    /// <summary>
    /// Retourne des objets représentant les champs typés dont les valeurs affiché
    /// Catalogues avancés, Relation, Utilisateurs
    /// </summary>
    public abstract class DataFieldWithDisplayModel : DataFieldWithValueModel
    {
        [DefaultValue("")]
        public override string DisplayValue { get; set; }
        protected DataFieldWithDisplayModel(eFieldRecord f) : base(f)
        {
            DisplayValue = f.DisplayValue;
        }



    }

}