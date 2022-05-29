using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm
{
    public class eMultiSelectItem
    {
        /// <summary>
        /// Identifiant de l'element
        /// </summary>
        public int Id;

        /// <summary>
        /// Titre afficher
        /// </summary>
        public string Title;

        /// <summary>
        /// Titre afficher
        /// </summary>
        public string Tooltip;

        /// <summary>
        /// Savoir si il est sélectionné
        /// </summary>
        public bool Selected;

        /// <summary>
        /// Désactivé, l'element peut etre afficher mais pas déplaçable
        /// </summary>
        public bool Disabled;
    }
}