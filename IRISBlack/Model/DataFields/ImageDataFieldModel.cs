using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.DataFields
{
    /// <summary>
    /// retourne les champs de type Image
    /// </summary>
    public class ImageDataFieldModel : DataFieldWithValueModel
    {
        /// <summary>
        /// Constructeur interne
        /// </summary>
        /// <param name="f">Champ à traiter</param>
        /// <param name="path">Dossier de stockage des images. Utilisé pour vérifier l'existence de l'image côté serveur, et renvoyer une réponse adaptée en fonction</param>
        internal ImageDataFieldModel(eFieldRecord f, string path) : base(f)
        {
            // Si le nom de fichier pointe vers un fichier inexistant sur le serveur, on renvoie une valeur vide pour indiquer au Front d'afficher l'image par défaut
            if (!String.IsNullOrEmpty(f.Value) && !File.Exists(Path.Combine(path, f.Value)))
                Value = String.Empty; // attention iic, il faut bien modifier Value (de la classe en cours) et non f.Value
        }

    }
}