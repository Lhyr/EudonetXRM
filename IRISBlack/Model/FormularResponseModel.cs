using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Engine;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    public class FormularResponseModel
    {
        /// <summary>
        /// Id du formulaire avancé
        /// </summary>
        public int FormularId { get; set; }
        /// <summary>
        /// Nom du formulaire avancé
        /// </summary>
        public string FormularName { get; set; }
        /// <summary>
        /// Body du fromulaire
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// CSS du formulaire
        /// </summary>
        public string Css { get; set; }
        /// <summary>
        /// Réponse de l'appel Ajax Axios
        /// </summary>
        /// 
        public bool Success { get; set; }

        #region Gestion des erreurs / popup
        /// <summary>
        /// Si on ferme ou non l'éditeur du formulaire
        /// </summary>
        public bool CloseWizard { get; set; }

        /// <summary>
        /// Si on désactive la publication du formulaire
        /// </summary>
        public bool DisableIfPublished { get; set; }

        /// <summary>
        /// Message dans la popup d'alerte
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Détails de la popup d'alerte
        /// </summary>
        public string Detail { get; set; }
        /// <summary>
        /// Exception levée
        /// </summary>
        public string Exception { get; set; }

        #endregion

        //Noeuds du modèle
        /// <summary>
        /// Type d'opération : ajout, modif, renommage, prévisualisation, suppression
        /// </summary>
        public string Operation { get; set; }

        /// <summary>Le modèle de formulaire est-il public ?</summary>
        public bool IsPublic { get; set; }

        //Permissions
        /// <summary>
        /// Id de la permission de visualisation
        /// </summary>
        public AdvFormularPermission ViewPerm { get; set; }
        /// <summary>
        /// permission de modification
        /// </summary>
        public AdvFormularPermission UpdatePerm { get; set; }


        /// <summary>
        /// Langue du formulaire
        /// </summary>
        public int FormularLang { get; set; } = 0;


        /// <summary>
        /// Statut du formulaire (
        /// </summary>
        public int FormularStatus { get; set; } = 0;

        /// <summary>
        /// Message de remerciement
        /// </summary>
        public string SubmissionBody { get; set; }

        /// <summary>
        /// CSS de Message de remerciement
        /// </summary>
        public string SubmissionBodyCss { get; set; }

        /// <summary>
        /// Url de redirection du formulaire avancée
        /// </summary>
        public string SubmissionRedirectUrl { get; set; }

        /// <summary>
        /// Extended Param
        /// </summary>
        public FormularExtendedParam FormularExtendedParam { get; set; }

        /// <summary>
        /// Lien du formulaire
        /// </summary>
        public string RewrittenURL { get; set; }


        /// <summary>
        /// Script js pour intégration formulaire
        /// </summary>
        public string ScriptIntegration { get; set; }

        /// <summary>Date d'expiration du formulaire</summary>
        public DateTime? ExpireDate { get; set; }

        /// <summary>Date de début du formulaire</summary>
        public DateTime? StartDate { get; set; }

        /// <summary> Message de date de début /// </summary>
        public string MsgDateStart { get; set; }
        /// <summary>Message de date de fin</summary>
        public string MsgDateEnd { get; set; }
        /// <summary>MetaTags - Title</summary>
        public string MetaTitle { get; set; }
        /// <summary>MetaTags - Description</summary>
        public String MetaDescription { get; set; }

        /// <summary> Url d'image pour les réseaux sociaux </summary>
        public string MetaImgURL { get; set; }

        /// <summary> Image de formulaire pour les réseaux sociaux </summary>
        public HttpPostedFileBase imageFileFormular { get; set; }

    }

}