using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Model.Formatter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// Structure de données utilisée pour passer les paramètres d'images à supprimer via la méthode DELETE de UploadImageController
    /// </summary>
    public class UploadImageDeleteModel
    {
        /// <summary>
        /// Objet représentant une image à supprimer
        /// </summary>
        public struct ImageField {
            /// <summary>
            /// DescID du champ concerné par l'image en question
            /// </summary>
            public int DescId;
            /// <summary>
            /// Type d'image
            /// </summary>
            public eLibConst.IMAGE_TYPE ImageType;
            /// <summary>
            /// Nom de fichier de l'image
            /// </summary>
            public string FileName;
        }

        /// <summary>
        /// FileID du fichier concerné (normalement, uniquement en création, donc 0)
        /// </summary>
        public int FileId { get; set; }

        /// <summary>
        /// TabID de l'onglet concerné
        /// </summary>
        public int TabDescId { get; set; }

        /// <summary>
        /// Liste des champs Image à supprimer
        /// </summary>
        public List<ImageField> ImageFields { get; set;  }
    }
}