using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type Image
    /// </summary>
    public class ImageFieldInfos : FldTypedInfosModel, IMultiLine
    {
        public int Rowspan { get; set; }
        /// <summary>
        /// Type de stockage de l'image
        /// 0 : Fichier sur le serveur
        /// 1 : En base de données / binary blob (deprecated)
        /// 2 : URL
        /// </summary>
        public ImageStorage ImgStorage { get; set; }

        /// <summary>
        /// Taille, en octets, autorisée pour le stockage d'une image dans le champ
        /// </summary>
        public int SizeLimit { get; set; }

        /// <summary>
        /// Formats d'image autorisés dans le champ
        /// </summary>
        public string[] AllowedExtensions { get; set; }

        //KHA sera peut etre necessaire pour les images chargées sur une fiche en cours de créa non encore enregistrée
        //public bool IsB64 { get; set; }
        //public int Width { get; set; }
        //public int Height { get; set; }
        internal ImageFieldInfos(Field f) : base(f)
        {
            Format = FieldType.Image;
            Rowspan = f.PosRowSpan;
            ImgStorage = f.ImgStorage;
            SizeLimit = eLibConst.AVATAR_SIZE * 1024 * 1024; // SIZELIMIT dans DESC Non exposé - f.SizeLimit;
            AllowedExtensions = eLibConst.ALLOWED_IMAGE_EXTENSIONS;
        }

    }
}