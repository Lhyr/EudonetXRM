using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// Model de structure de type du champs
    /// </summary>
    public class FieldsModel
    {
        public int DescId { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        /// <summary>
        /// Ordre d'affichage (historique) du champ à l'écran
        /// </summary>
        public int DispOrder { get; set; }
        public long Width { get; set; }
        public string Label { get; set; }
        public FieldFormat Format { get; set; }
        public PopupType PopUp { get; set; }
        public PopupDataRender PopupDataRend { get; set; }
        public int PopupDescId { get; set; }
        public bool ReadOnly { get; set; }
        public string ToolTipText { get; set; }
        public string ValueColor { get; set; }
        public string StyleForeColor { get; set; }
        public string Icon { get; set; }
        public bool Multiple { get; set; }
        public string ValueAlias { get; set; }
    }
}