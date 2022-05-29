
using System.ComponentModel;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    public interface IFldTypedInfosModel
    {
        int DescId { get; }
        FieldType Format { get; }
        [DefaultValue("")]
        string Label { get; }
        [DefaultValue("")]
        string ToolTipText { get; set; }
        int PosX { get; set; }
        int PosY { get; set; }
		int DispOrder { get; }
        int Colspan { get; }
        int Rowspan { get; }
        [DefaultValue("")]
        string StyleForeColor { get; }
        [DefaultValue("")]
        string ValueColor { get; }
        bool Bold { get; }
        bool Italic { get; }
        bool Flat { get; }
        bool Underline { get; }
        bool LabelHidden { get; }
        long Width { get; }
        ExpressFilterModel ExpressFilterActived { get; }
        bool IsSortable { get; }
        int SortOrder { get; set; }
        bool IsFiltrable { get; }

        /// <summary>
        /// Le champ est-il compatible avec la fonction Somme des colonnes ?
        /// </summary>
        bool IsComputable { get; }
        bool IsInRules { get; }
        string Watermark { get; set; }  
        /// <summary>
        /// Données RGPD du champ
        /// </summary>
        RGPDModel RGPD { get; set; }
        /// <summary>
        /// SAvoir si on affiche dans la barre d'action l'élément.
        /// </summary>
        bool DISPLAYINACTIONBAR { get; set; }
    }
}