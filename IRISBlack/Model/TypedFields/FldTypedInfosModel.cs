using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;


namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// Retourne des objets représentant les champs typés (catalogue numéric etc.)
    /// </summary>
    public abstract class FldTypedInfosModel : IFldTypedInfosModel
    {
        [DefaultValue(0)]
        public int DescId { get; set; }
        [DefaultValue(0)]
        public int PosX { get; set; }
        [DefaultValue(0)]
        public int PosY { get; set; }
        [DefaultValue(0)]
        public int DispOrder { get; set; }
        [DefaultValue(0)]
        public int Colspan { get; set; }
        public int Rowspan { get; set; }
        [DefaultValue("")]
        public string Label { get; set; }
        public FieldType Format { get; set; }
        [DefaultValue("")]
        public string ToolTipText { get; set; }
        [DefaultValue("")]
        public string ValueColor { get; set; }
        [DefaultValue("")]
        public string StyleForeColor { get; set; }
        [DefaultValue("")]
        public string BackgroundColor { get; set; }
        [DefaultValue(false)]
        public bool Bold { get; set; }
        [DefaultValue(false)]
        public bool Italic { get; set; }
        [DefaultValue(false)]
        public bool Flat { get; set; }
        [DefaultValue(false)]
        public bool Underline { get; set; }
        [DefaultValue(false)]
        public bool LabelHidden { get; set; }
        [DefaultValue(0)]
        public long Width { get; set; }
        public ExpressFilterModel ExpressFilterActived { get; set; }
        [DefaultValue(false)]
        public bool IsSortable { get; set; }
        [DefaultValue(false)]
        public int SortOrder { get; set; }
        [DefaultValue(false)]
        public bool IsFiltrable { get; set; }
        [DefaultValue(false)]
        /// <summary>Ce champ est-il compatible avec la fonction Somme ?</summary>
        public bool IsComputable { get; set; }
        [DefaultValue(false)]
        public bool IsInRules { get; set; }
        [DefaultValue(false)]
        public string Watermark { get; set; }
        /// <summary>Rubrique associée</summary>
        [DefaultValue("")]
        public string AssociateField { get; set; }
        /// <summary>
        /// Informations de gestion RGPD
        /// </summary>
        public RGPDModel RGPD { get; set; }
        /// <summary>
        /// SAvoir si on affiche dans la barre d'action l'élément.
        /// </summary>
        public bool DISPLAYINACTIONBAR { get; set; } = false;

        /// <summary>
        /// Type de Champs.
        /// </summary>
        [DefaultValue(0)]
        public FieldFormat FldFormat { get; set; }

        /// <summary>
        /// La longueur du champs.
        /// </summary>
        [DefaultValue(0)]
        public int MaxLength { get; set; }

        internal FldTypedInfosModel(Field f)
        {
            DescId = f.Descid;
            PosX = f.PosX;
            PosY = f.PosY;
            DispOrder = f.PosDisporder;
            Colspan = f.PosColSpan;
            Rowspan = f.PosRowSpan;
            Label = f.Libelle;
            ValueColor = f.ValueColor;
            StyleForeColor = f.StyleForeColor;
            ToolTipText = f.ToolTipText;
            Bold = f.StyleBold;
            Italic = f.StyleItalic;
            Flat = f.StyleFlat;
            Underline = f.StyleUnderline;
            LabelHidden = f.LabelHidden;
            BackgroundColor = f.ButtonColor;
            Width = f.Width;
            if (!string.IsNullOrEmpty(f.ExpressFilterActived))
                ExpressFilterActived = new ExpressFilterModel(f.ExpressFilterActived);
            IsSortable = f.IsSortable;
            IsFiltrable = f.IsFiltrable;
            IsComputable = f.IsComputable;
            IsInRules = f.IsInRules;
            SortOrder = (int)f.SortInfo;// paramètre Order :- 1 pour NONE , 1 pour DESCendant, 0 pour ASCendant
            if (!String.IsNullOrEmpty(f.AssociateField))
                AssociateField = f.AssociateField;
            RGPD = null;
            FldFormat = f.Format;
            MaxLength = f.Length;
        }
    }


    /// <summary>
    /// Types de champs disponibles
    /// </summary>
    public enum FieldType
    {
        /// <summary>Aucune correspondance </summary>
        Undefined = 0,
        /// <summary>Caractère</summary>
        Character = 1,
        /// <summary>Catalogue </summary>
        Catalog = 2,
        /// <summary>Addresse email </summary>
        MailAddress = 3,
        /// <summary>Téléphone </summary>
        Phone = 4,
        /// <summary>Relation </summary>
        Relation = 5,
        /// <summary>Réseau sociaux </summary>
        SocialNetwork = 6,
        /// <summary>Géolocalisation </summary>
        Geolocation = 7,
        /// <summary>Alias </summary>
        Alias = 8,
        /// <summary>Alias d'en-tête (Champs de liaison système placé dans le corps de la page) </summary>
        AliasRelation = 9,
        /// <summary>Bouton </summary>
        Button = 10,
        /// <summary>Case à cocher </summary>
        Logic = 11,

        // Il y un 12 manquant ici car valeur supprimée. A saisir :)

        /// <summary>Compteur automatique </summary>
        AutoCount = 13,
        /// <summary>Date </summary>
        Date = 14,
        /// <summary>Etiquette </summary>
        Label = 15,
        /// <summary>Séparateur de page </summary>
        Separator = 16,
        /// <summary>Memo </summary>
        Memo = 17,
        /// <summary>Numérique </summary>
        Numeric = 18,
        /// <summary>Monetaire </summary>
        Money = 19,
        /// <summary>Image </summary>
        Image = 20,
        /// <summary>Graphique </summary>
        Chart = 21,
        /// <summary>Page Web (Iframe) </summary>
        WebPage = 22,
        /// <summary>Lien web </summary>
        HyperLink = 23,
        /// <summary>Fichier </summary>
        File = 24,
        /// <summary>Utilisateur </summary>
        User = 25,
        /// <summary>Mot de passe </summary>
        Password = 26,
        /// <summary>PJ</summary>
        PJ = 27,
        /// <summary>Type caché.</summary>
        Hidden = 28,
        /// <summary>varbinary et toutes ces sortes de choses.</summary>
        Binary = 29,
        /// <summary>Champs système dont la valeur en base diffère de la valeur affichée</summary>
        System = 30
    }


    public enum BtnSpecificAction
    {
        /// <summary>
        /// Bouton normal
        /// </summary>
        Undefined = 0,
        /// <summary>
        /// création / mise à jour de l'evenement sur teams
        /// </summary>
        CreateSaveTeamsEvent = 1,
        /// <summary>
        /// suppression de l'évèneemnt sur teams
        /// </summary>
        DeleteTeamsEvent = 2


    }
    public class ExpressFilterModel
    {
        public int Operator { get; set; }
        public string Value { get; set; }

        public ExpressFilterModel(string sSerialized)
        {
            if (string.IsNullOrEmpty(sSerialized))
                return;

            string[] aExpressInfos = sSerialized.Split("$%$");

            int filterOP = eLibTools.GetNum(aExpressInfos[0]);
            Operator = filterOP;
            Value = aExpressInfos[1];
        }
    }
}