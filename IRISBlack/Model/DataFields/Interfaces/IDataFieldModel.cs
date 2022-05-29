using System.ComponentModel;

namespace Com.Eudonet.Xrm.IRISBlack.Model.DataFields
{
    public interface IDataFieldModel
    {

        /// <summary>
        /// DescID du champ
        /// </summary>
        [DefaultValue(0)]
        int DescId { get; set; }

        [DefaultValue("")]
        string Value { get; set; }
        [DefaultValue("")]
        string DisplayValue { get; set; }
        /// <summary>
        /// Le champ est-il visible ?
        /// </summary>
        [DefaultValue(false)]
        bool IsVisible { get; set; }

        /// <summary>
        /// Indique si le champ est lié à une formule du milieu
        /// </summary>
        [DefaultValue(false)]
        bool HasMidFormula { get; }

        /// <summary>
        /// Indique si le champ est lié à un automatisme ORM
        /// </summary>
        [DefaultValue(false)]
        bool HasORMFormula { get; set; }

        /// <summary>
        /// Représente la colonne [DESC].[Formula], soit la formule du bas éventuellement liée au champ
        /// </summary>
        [DefaultValue("")]
        string Formula { get; }
        string Icon { get; set; }
    }
}