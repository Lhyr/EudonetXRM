using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// Modele de présentation des MRU
    /// </summary>
    public class FileMenuModel
    {
        /// <summary>
        /// Représente la liste des options
        /// </summary>
        public List<FileMenuOption> Options { get; set; } = new List<FileMenuOption>();

        /// <summary>
        /// Représente la liste des menus
        /// </summary>
        public List<FileMenuItem> Items { get; set; } = new List<FileMenuItem>();

        /// <summary>
        /// Représente une option de menu
        /// </summary>
        public class FileMenuOption
        {
            // TODO A COMPLETER (SPECS ?)
        }

        /// <summary>
        /// Représente la liste des éléments de menu
        /// </summary>
        public class FileMenuItem
        {
            /// <summary>
            /// Nom du menu
            /// </summary>
            public string Name { get; set; } = "";
            /// <summary>
            /// Actions du menu
            /// </summary>
            public List<FileMenuItemAction> Actions { get; set; } = new List<FileMenuItemAction>();

            /// <summary>
            /// Constructeur par défaut
            /// </summary>
            public FileMenuItem() { }

            /// <summary>
            /// Constructeur
            /// </summary>
            /// <param name="name">Libellé</param>
            /// <param name="actions">Liste des actions proposées</param>
            public FileMenuItem(string name, List<FileMenuItemAction> actions)
            {
                Name = name;
                Actions = actions;
            }
        }

        /// <summary>
        /// Représente une action d'un élément de menu
        /// </summary>
        public class FileMenuItemAction
        {
            /// <summary>
            /// Libellé de l'action
            /// </summary>
            public string Name { get; set; } = "";
            /// <summary>
            /// Identifiant CSS de l'icône de l'action dans le menu
            /// </summary>
            public string Icon { get; set; } = "";
            /// <summary>
            /// Mot-clé correspondant à l'action souhaitée au clic
            /// </summary>
            public string Action { get; set; } = "";
            /// <summary>
            /// Texte à afficher en infobulle au survol de l'action (si souhaité)
            /// </summary>
            public string Tooltip { get; set; } = "";

            /// <summary>
            /// Constructeur par défaut
            /// </summary>
            public FileMenuItemAction() { }

            /// <summary>
            /// Constructeur
            /// </summary>
            /// <param name="name">Libellé</param>
            /// <param name="icon">Icône</param>
            /// <param name="action">Action (JS)</param>
            /// <param name="tooltip">Action (JS)</param>
            public FileMenuItemAction(string name, string icon, string action, string tooltip)
            {
                Name = name;
                Icon = icon;
                Action = action;
                Tooltip = tooltip;
            }
        }
    }
}