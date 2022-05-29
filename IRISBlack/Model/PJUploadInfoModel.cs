using EudoQuery;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// Infos concernant l'upload d'une annexe
    /// </summary>
    public class PJUploadInfoModel
    {
        /// <summary>
        /// ID dans la table PJ
        /// </summary>
        [DisplayName("pjid")]
        public int PjId { get; set; }
        /// <summary>
        /// Nom du fichier
        /// </summary>
        [DisplayName("filename")]
        public string FileName { get; set; }
        /// <summary>
        /// Nom du fichier renommé
        /// </summary>
        [DisplayName("saveas")]
        public string SaveAs { get; set; }
        /// <summary>
        /// Action : 0 pour renommage / 1 pour remplacement
        /// </summary>
        [DisplayName("action")]
        public int Action { get; set; }
        /// <summary>
        /// L'option "Remplacer et supprimer le fichier existant" est-elle affichée ?
        /// </summary>
        [DisplayName("replaceoptdisplayed")]
        public bool ReplaceOptDisplayed { get; set; }
        /// <summary>
        /// URL d'une PJ existante (SrcPJ côté E17)
        /// </summary>
        [DisplayName("pjsrcurl")]
        public string PJSrcUrl { get; set; }
        /// <summary>
        /// Type d'annexe
        /// NONE = -1 : Non chargé
        /// FILE = 0 : Fichier
        /// LOCAL_FILE = 1 : Fichier Local
        /// MAIL = 2 : Adresse E-Mail
        /// WEB = 3 : Site Web
        /// FTP = 4 : Site FTP
        /// PATH = 5 : Chemin (UNC
        /// CAMPAIGN = 6 : pour une PJ dans les e-mails issus d'une campagne
        /// REPORTS = 7 : pour les rapports d'une campagne mail
        /// REMOTE_CAMPAIGN = 8 : pour les PJ externalisées dans les mails de campagne
        /// IMPORT = 9 : pour les rapports d'import
        /// IMPORT_REPORTS = 10
        /// </summary>
        [DisplayName("pjtype")]
        public PjType PJType { get; set; }
        /// <summary>
        /// DescId de la table
        /// </summary>
        [DisplayName("pjtabdescid")]
        public int PJTabDescID { get; set; }
        /// <summary>
        /// Id de la fiche de la table
        /// </summary>
        [DisplayName("pjfileid")]
        public int PJFileID { get; set; }
        /// <summary>
        /// Type de la table
        /// </summary>
        [DisplayName("pjtabtype")]
        public EdnType PJTabType { get; set; }
        /// <summary>
        /// Nombre d'annexes rattachées à l'enregistrement
        /// </summary>
        [DisplayName("pjcnt")]
        public int PJCnt { get; set; }
        /// <summary>
        /// ToolTip de la ligne
        /// </summary>
        [DisplayName("pjtooltip")]
        public string PJToolTip { get; set; }
        /// <summary>
        /// Règles et droits de suppression sur les PJ sur la fiche
        /// </summary>
        [DisplayName("ispjdeletable")]
        public bool IsPJDeletable { get; set; }
        /// <summary>
        /// Règles et droits de modification sur les PJ sur la fiche
        /// </summary>
        [DisplayName("ispjupdatable")]
        public bool IsPJUpdatable { get; set; }
        /// <summary>
        /// Règles et droits de visu sur les PJ sur la fiche
        /// </summary>
        [DisplayName("ispjviewable")]
        public bool IsPJViewable { get; set; }
    }
}