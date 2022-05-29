using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Result.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// Interface correspondant à l'objet retourné par UploadImageController
    /// </summary>
    public interface IUploadImageReturnModel
    {
        /// <summary>
        /// URL générée pour l'image dans tous les cas, à réutiliser pour l'affichage sur le Front
        /// </summary>
        string ImageURL { get; set; }

        /// <summary>
        /// Image temporaire générée dans le cas d'une création de fiche
        /// </summary>
        byte[] SessionImageDataBytes { get; set; }

        /// <summary>
        /// URI temporaire générée dans le cas d'une création de fiche
        /// </summary>
        Uri SessionImageUri { get; set; }

        /// <summary>
        /// Nom de fichier de l'image temporaire générée dans le cas d'une création de fiche
        /// </summary>
        string SessionImageFileName { get; set; }

        /// <summary>
        /// Content-Type / Type MIME de l'image temporaire générée dans le cas d'une création de fiche
        /// </summary>
        string SessionImageContentType { get; set; }
    }
}