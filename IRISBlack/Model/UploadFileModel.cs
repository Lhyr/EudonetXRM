using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Xrm.IRISBlack.Model.Formatter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    public class UploadFileModel
    {

        /// <summary>Infos concernant l'upload d'une annexe </summary>
        public string[] UploadInfo { get; set; }

        /// <summary>Nom sous lequel le fichier doit être enregistré </summary>
        public string SaveAs { get; set; }

        /// <summary>Id de la fiche parente </summary>
        public int FileId { get; set; }
        /// <summary>DescId de la table parente </summary>
        public int Tab { get; set; }

        /// <summary>
        /// FileId de la fiche evenement parent
        /// </summary>
        public int ParentEvtFileId { get; set; }

        /// <summary>
        /// DescId de la table evenement parent
        /// </summary>
        public int ParentEvtTab { get; set; }


        public bool FromTpl { get; set; }

        public bool MailForwarded { get; set; }


        /// <summary>PPID de la fiche à laquelle on lie l'annexe</summary>
        public int PPID { get; set; }

        /// <summary>PMID de la fiche à laquelle on lie l'annexe </summary>
        public int PMID { get; set; }

        /// <summary>ADRID de la fiche à laquelle on lie l'annexe </summary>
        public int ADRID { get; set; }

        /// <summary>
        /// Type de PJ (fichier, lien...)
        /// </summary>
        public int PJType { get; set; }

        /// <summary>
        /// Lien en cas de selection de site web, FTP ou adresse Email
        /// </summary>
        public string UploadLink { get; set; }


        /// <summary>
        /// Type d'upload (PJ, Mail non remis...)
        /// par défaut PJ
        /// </summary>
        public UploadTypes UploadType { get; set; } = UploadTypes.PJ;

        /// <summary>
        /// Une liste de fichiers envoyés depuis le client.
        /// </summary>
        public HttpPostedFileMultipart[] fileCollection { get; set; }
    }
}