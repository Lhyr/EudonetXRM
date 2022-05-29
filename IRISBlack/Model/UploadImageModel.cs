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
    /// Modèle utilisé pour transmettre les paramètres d'envoi d'images à la méthode POST de UploadImageController
    /// </summary>
    public class UploadImageModel : UploadFileModel
    {
        /// <summary>
        /// DescId du champ concerné
        /// </summary>
        public int DescId { get; set; }

        /// <summary>
        /// Contexte d'ajout d'image : en popup ou non ?
        /// </summary>
        public bool ParentIsPopup { get; set; }

        /// <summary>
        /// Contexte d'ajout d'image : se met-elle à jour à la sortie du coamp ?
        /// </summary>
        public bool UpdateOnBlur { get; set; }

        /// <summary>
        /// L'image doit-elle conservée en session si l'utilisateur crée une nouvelle fiche et ne l'enregistre pas ?
        /// </summary>
        public bool SaveInSession { get; set; }

        /// <summary>
        /// Indique si une miniature de l'image doit être générée pour les modes Liste et autres
        /// </summary>
        public bool ComputeRealThumbnail { get; set; }

        /// <summary>
        /// Type d'image à gérer
        /// </summary>
        public eLibConst.IMAGE_TYPE ImageType { get; set; }

        /// <summary>
        /// Largeur finalement retenue pour l'image
        /// </summary>
        public int ImageWidth { get; set; }

        /// <summary>
        /// Hauteur finalement retenue pour l'image
        /// </summary>
        public int ImageHeight { get; set; }
    }
}