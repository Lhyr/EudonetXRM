
using Com.Eudonet.Internal.Import;
using System.Collections.Generic;

namespace Com.Eudonet.Xrm.import
{
    /// <summary>
    /// Structure de données représentant les pramètres de l'assistant d'import
    /// </summary>
    public class eImportWizardParam
    {
        /// <summary>
        /// Nom du fichier
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Action du wizard demandée
        /// </summary>
        public eImportWizardAction Action { get; private set; }
        /// <summary>
        /// Identifiant d'import
        /// </summary>
        public int ImportId { get; private set; }

        /// <summary>
        /// Identifiant du modèle d'import
        /// </summary>
        public ImportTemplate ImportTemplateParams { get; set; }

        /// <summary>
        /// Table sur de démarage de l'assistant
        /// </summary>
        public int ImportTab { get; private set; }

        /// <summary>
        /// Etape actuelle de l'assistant
        /// </summary>
        public int CurrentWizardStep { get; private set; }

        /// <summary>
        /// Table parente si import depuis un signet 
        /// </summary>
        public int ParentTab { get; private set; }

        /// <summary>
        /// Fiche parente si import depuis un signet
        /// </summary>
        public int ParentFileId { get; private set; }

        /// <summary>
        /// Largeur de l'assistant
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Hauteur de l'assistant
        /// </summary>
        public int Height { get; private set; }


        /// <summary>
        /// les clés définies par table
        /// </summary>
        public Dictionary<string, string> KeysFields { get; private set; }


        /// <summary>
        /// les rubriques définies par colonne d'import
        /// </summary>
        public Dictionary<string, string> FieldsParam { get; private set; }


        /// <summary>
        /// les rubriques définies par table
        /// </summary>
        public ImportParams ImportParams { get; private set; }

        /// <summary>
        /// l'encodage en cours envoyé par le navigateur
        /// </summary>
        public System.Text.Encoding WizardEncoding { get; private set; }


        /// <summary>
        /// Récupèration des paramètres de l'assistant d'import
        /// </summary>
        /// <param name="requestTools"></param>
        public eImportWizardParam(eRequestTools requestTools)
        {
            // fiche parente si on importe depuis un signet
            ParentTab = requestTools.GetRequestFormKeyI("ParentTab") ?? 0;
            ParentFileId = requestTools.GetRequestFormKeyI("ParentFileId") ?? 0;

            // table (en onglet ou en signet) sur laquelle l'import va s'effectuer
            ImportTab = requestTools.GetRequestFormKeyI("ImportTab") ?? 0;

            // identifiant de l'import dans la base
            ImportId = requestTools.GetRequestFormKeyI("ImportTemplateId") ?? 0;

            // identifiant du modèle d'import dans la base
            if (!string.IsNullOrEmpty(requestTools.GetRequestFormKeyS("ImportTemplateParams")))
                ImportTemplateParams = EudoQuery.SerializerTools.JsonDeserialize<ImportTemplate>(requestTools.GetRequestFormKeyS("ImportTemplateParams"));
            // numéro de l'etape
            CurrentWizardStep = requestTools.GetRequestFormKeyI("Step") ?? (int)WIZARD_STEP.IMPORTPARAMS;

            // numéro de l'etape, default eImportWizardAction.NO_ACTION
            Action = requestTools.GetRequestFormEnum<eImportWizardAction>("Action", true);

            // Hauteur et largeur de la fenetre
            Height = requestTools.GetRequestFormKeyI("height") ?? eConst.DEFAULT_WINDOW_HEIGHT;
            Width = requestTools.GetRequestFormKeyI("width") ?? eConst.DEFAULT_WINDOW_WIDTH;

            if (!string.IsNullOrEmpty(requestTools.GetRequestFormKeyS("ImportGlobalTabParam")))
                ImportParams = EudoQuery.SerializerTools.JsonDeserialize<ImportParams>(requestTools.GetRequestFormKeyS("ImportGlobalTabParam"));

            FileName = requestTools.GetSessionKeyS("ImportFile");
            WizardEncoding = requestTools.GetContentEncoding();

        }

        /// <summary>
        /// Affecte des aparmètres à l'import
        /// </summary>
        /// <param name="param"></param>
        public void SetTabParams(IEnumerable<ImportTabParams> param)
        {
            this.ImportParams.SetImportTabParams(param);
        }

        /// <summary>
        /// Présente les différentes étape du wizard
        /// </summary>
        public enum WIZARD_STEP
        {
            /// <summary>
            /// Etape de paramétrage de la source des données
            /// </summary>
            IMPORTPARAMS = 1,
            /// <summary>
            /// Etape du mapping des rubriques
            /// </summary>
            MAPPING = 2,
            /// <summary>
            /// Options d'import : création / mise à jour
            /// </summary>
            OPTIONS = 3,
            /// <summary>
            /// Récapitulatif des paramètres définis
            /// </summary>
            RECAP = 4
        }

    }


}