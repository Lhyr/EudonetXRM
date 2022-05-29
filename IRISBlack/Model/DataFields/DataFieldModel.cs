using Com.Eudonet.Core.Model;
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
    /// Retourne des objets représentant les champs typés ne contenant pas de valeur
    /// Titre separateur, graphique, Page web.
    /// </summary>
    public abstract class DataFieldModel : IDataFieldModel
    {
        //protected eFieldRecord _dataField;

        /// <summary>
        /// DescID du champ
        /// </summary>
        [DefaultValue(0)]
        public int DescId { get; set; }

        [DefaultValue("")]
        public string Value { get; set; }
        [DefaultValue("")]
        public virtual string DisplayValue { get; set; }

        /// <summary>
        /// Le champ est-il visible ?
        /// </summary>
        [DefaultValue(false)]
        public bool IsVisible { get; set; }

        /// <summary>
        /// Indique si le champ est lié à une formule du milieu
        /// </summary>
        [DefaultValue(false)]
        public bool HasMidFormula { get; }

        /// <summary>
        /// Indique si le champ est lié à un automatisme ORM
        /// </summary>
        [DefaultValue(false)]
        public bool HasORMFormula { get; set; }

        /// <summary>
        /// Représente la colonne [DESC].[Formula], soit la formule du bas éventuellement liée au champ
        /// </summary>
        [DefaultValue("")]
        public string Formula { get; }

        public string Icon { get; set; }

        internal DataFieldModel(eFieldRecord f)
        {
            //_dataField = f;

            DescId = f.FldInfo.Descid;
            IsVisible = f.RightIsVisible;
			HasMidFormula = f.FldInfo.HasMidFormula;
			Formula = f.FldInfo.Formula;
            Icon = f.FldInfo.Icon;
        }
    }




}