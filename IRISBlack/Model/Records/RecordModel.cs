using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Factories;
using Com.Eudonet.Xrm.IRISBlack.Model.DataFields;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// Objet présentant les données d'une ligne d'un jeu de données
    /// Potentiellement il faudra faire des dérivations pour différencier le mode fiche et le mode liste
    /// </summary>
    public class RecordModel 
        : IRecordModel
    {
        //private eRecord _rec;
        /// <summary>Icone en début de ligne</summary>
        public string Icon { get; set; }
        /// <summary>Couleur d'écriture</summary>
        public string Color { get; set; }
        /// <summary>Couleur d'arrière plan</summary>
        public string BGColor { get; set; }
        /// <summary>Id de la fiche de départ</summary>
        public int MainFileId { get; set; }
        /// <summary>Libellé de la fiche (XXX01)</summary>
        public string MainFileLabel { get; set; }
        /// <summary>Indique si on a les droits de mise à jour</summary>
        public bool RightIsUpdatable { get; set; }
        /// <summary>Indique si on a les droits de suppression</summary>
        public bool RightIsDeletable { get; set; }

        /// <summary>
        /// Dans le cas d'un enregistrement de PJ (eRecordPj), informations concernant celle-ci
        /// </summary>
        public PJUploadInfoModel PJInfo { get; set; }
        /// <summary>Liste des champs contenant les données</summary>
        public IEnumerable<IDataFieldModel> LstDataFields { get; set; } = new List<IDataFieldModel>();
        /// <summary>Eléments pour le raccourci</summary>
        public FieldsMenuShortcutModel MenuShortcut { get; set; }

        /// <summary>
        /// US #4315 - Droit de modification de la zone Assistant du nouveau mode Fiche Eudonet X
        /// </summary>
        public bool CanUpdateWizardBar { get; set; }
    }
}