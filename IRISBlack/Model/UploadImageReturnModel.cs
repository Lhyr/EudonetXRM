using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Result;
using Com.Eudonet.Engine.Result.Data;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// Objet retourné par UploadImageController
    /// </summary>
    public class UploadImageReturnModel : IUploadImageReturnModel
    {
        /// <summary>
        /// URL générée pour l'image
        /// </summary>
        public string ImageURL { get; set; }

        /// <summary>
        /// Indique à l'appelant si l'image lui a été retournée, plutôt qu'enregistrée sur le serveur
        /// </summary>
        public bool SessionImageSent { get; set; }

        /// <summary>
        /// Image temporaire générée dans le cas d'une création de fiche
        /// </summary>
        public byte[] SessionImageDataBytes { get; set; }

        /// <summary>
        /// URI temporaire générée dans le cas d'une création de fiche
        /// </summary>
        public Uri SessionImageUri { get; set; }

        /// <summary>
        /// Nom de fichier de l'image temporaire générée dans le cas d'une création de fiche
        /// </summary>
        public string SessionImageFileName { get; set; }

        /// <summary>
        /// Content-Type / Type MIME de l'image temporaire générée dans le cas d'une création de fiche
        /// </summary>
        public string SessionImageContentType { get; set; }
    }
}