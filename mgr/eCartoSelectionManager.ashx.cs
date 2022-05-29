using System;
using System.Collections.Generic;
using EudoQuery;
using Com.Eudonet.Engine;
using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Génère une réponse JSON aux differents type de requetes
    /// </summary>
    public class eCartoSelectionManager : eEudoManagerReadOnly
    {
        Func<string> result = () => SerializerTools.JsonSerialize(new JSONReturnGeneric() { Success = true });

        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessManager()
        {                         

            CartoRequest request = SerializerTools.JsonDeserialize<CartoRequest>(_context.Request.InputStream) ?? new CartoRequest();
            var CartoSelection = eCartoSelection.Create(_pref, request.WidgetId);

            switch (request.Action)
            {
                // On charge la config
                case "get-config":
                    result = () => SerializerTools.JsonSerialize(CartoSelection.LoadConfig(), true);
                    break;
                // On applique le filtre
                case "apply-filter":
                    result = () => SerializerTools.JsonSerialize(CartoSelection.ApplyCriteria(request.Criteria), true);
                    break;
                // On enregistre la sélection
                case "save-selection":
                    result = () => SerializerTools.JsonSerialize(CartoSelection.SaveSelection(request.Selection), true);
                    break;
                // On récupère le nombre de fiche insérée pour la fiche parente
                case "get-count":
                    result = () => SerializerTools.JsonSerialize(CartoSelection.GetSelectionCount(request.Selection), true);
                    break;
                default:
                    break;
            }

            // Renvoi du flux JSON à l'appelant
            RenderResult(RequestContentType.SCRIPT, () => result());
        }
    }

    /// <summary>
    /// Demande js sous format JSON
    /// </summary>
    public class CartoRequest
    {

        /// <summary>
        /// Id du widget cartographique
        /// </summary>
        public int WidgetId = 0;

        /// <summary>
        /// Action a executer
        /// </summary>
        public string Action = string.Empty;

        /// <summary>
        /// Liste des critère de recherche
        /// </summary>
        public Criteria Criteria = new Criteria();

        /// <summary>
        ///Liste des sélection
        /// </summary>
        public CardSelection Selection = new CardSelection();
    }

    /// <summary>
    /// Résultats de la recherche
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CartoResponse<T> : JSONReturnGeneric
    {
        /// <summary>
        /// Limite de nombre de fiches 
        /// </summary>
        public int MaxRecord = eCartoSelection.MAX_RECORD;
        
        /// <summary>
        /// Liste des lignes retournées
        /// </summary>
        public List<T> Results = new List<T>();
    }

   
    /// <summary> Infobulle de position</summary>
    public class CartoRecord
    {
        /// <summary> Infobulle de la position </summary>
        public int FileId = 0;

        /// <summary> Table source </summary>
        public int SourceTab = 0;

        /// <summary> Infobulle de la position </summary>
        public CartoInfobox Infobox = new CartoInfobox();

        /// <summary> Mini-fiche</summary>
        public CartoCard Card = new CartoCard();
    }

    /// <summary> Infobulle de position</summary>
    public class CartoInfobox
    {
        /// <summary> Point geoloc </summary>     
        public string Geography = string.Empty;
        /// <summary> Titre de l'infobulle </summary>
        public string Title = string.Empty;
        /// <summary> Sous-titre</summary>
        public string SubTitle = string.Empty;
        /// <summary> Image de l'infobulle </summary>
        public string ImageSource = string.Empty;
        /// <summary> Titre de l'image </summary>
        public string ImageTitle = string.Empty;
        /// <summary> Champs additionnels </summary>
        public List<string> Fields = new List<string>();
    }

    /// <summary>
    /// mini-fiche
    /// </summary>
    public class CartoCard
    {
        /// <summary> Champs additionnels </summary>
        public List<string> Images = new List<string>();

        /// <summary>Table principale + Tables parentes</summary>
        public List<CartoTab> Tabs = new List<CartoTab>();
    }

    /// <summary>
    /// Table mappée
    /// </summary>
    public class CartoTab {

        /// <summary> Titre  de l'onglet </summary>
        public string Title = string.Empty;
      
        /// <summary> Champs mappé </summary>
        public List<string> Fields = new List<string>();
    }

    /// <summary>
    /// L'ensemble des critère de sélection
    /// </summary>
    public class Criteria
    {

        /// <summary>
        /// Liste des critères de sélection
        /// </summary>
        public List<FieldLine> Filters = new List<FieldLine>();

        /// <summary>
        /// rectangle de la carte
        /// </summary>
        public string MapBounds = string.Empty;

        /// <summary>
        /// centre de la carte
        /// </summary>
        public string MapCenter = string.Empty;
    }

    /// <summary>
    /// La sélection a enregistrer
    /// </summary>
    public class CardSelection
    {
        /// <summary>
        /// Id de la fiche sélection parente
        /// </summary>
        public int ParentSelectionId = 0;

        /// <summary>
        /// Liste des id fiches sources
        /// </summary>
        public List<int> Selections = new List<int>();
    }

    /// <summary>
    /// Valeur choisie
    /// </summary>
    public class FieldLine
    {

        /// <summary>
        /// descId de la rubrique filtre
        /// </summary>
        public int DescId = 0;

        /// <summary>
        /// Type de filtre : interval (date, numeric), catalog, logic, text
        /// </summary>
        public string Type = "";

        /// <summary>
        /// Valeurs du catalog, logic, text
        /// </summary>
        public string Value = "";

        /// <summary>
        /// Valeur min de l'interval 
        /// </summary>
        public string MinValue = "";

        /// <summary>
        /// Valeur max de l'interval 
        /// </summary>
        public string MaxValue = "";
    }
}
