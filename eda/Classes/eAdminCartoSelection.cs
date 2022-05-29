using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Class permettant de gérer le widget de type selection cartographique
    /// </summary>
    public class eAdminCartoSelection
    {
        /// <summary>  noms des Params widgets utilisé pour la sélection cartographique </summary>
        public static string WIDGET_PARAM_CONFIG = "config";
        /// <summary> list des descid table/champs </summary>
        public static string WIDGET_PARAM_DEPENDENCY = "dependency";

        /// <summary>
        /// Format de champ supportés dans la "v1"
        /// </summary>
        public readonly static List<FieldFormat> supportedFormat = new List<FieldFormat>()
        {
            FieldFormat.TYP_GEOGRAPHY_V2,
            FieldFormat.TYP_MONEY,
            FieldFormat.TYP_CHAR,
            FieldFormat.TYP_DATE,
            FieldFormat.TYP_BIT,
            FieldFormat.TYP_NUMERIC
        };

        /// <summary>
        /// Type de catalogue supporté
        /// </summary>
        public readonly static List<PopupType> supportedPopup = new List<PopupType>()
        {
            PopupType.NONE,
            PopupType.FREE,
            PopupType.ONLY,
            PopupType.DATA
        };

        /// <summary>
        /// Ne fait rien par defaut
        /// </summary>
        private Action<string> Log = (s) => { };

        /// <summary>
        /// Id du widget
        /// </summary>
        private int _wid;

        /// <summary>
        /// Pref utilisateur
        /// </summary>
        private ePref _pref;


        /// <summary>
        /// Objet permettant de gérer les demandes clients
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="wid"></param>     
        private eAdminCartoSelection(ePref pref, int wid)
        {
            this._pref = pref;
            this._wid = wid;

            if (_pref.ModeDebug)
                Log = eModelTools.EudoTraceLogger(pref, "[Admin][Sélection cartographique]");
        }

        /// <summary>
        /// Methode de création de gestionnaire du widget
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="widgetId"></param>
        /// <returns></returns>
        public static eAdminCartoSelection Create(ePref pref, int widgetId)
        {
            return new eAdminCartoSelection(pref, widgetId);
        }

        /// <summary>
        /// Methode utilitaire permettant d'ajouter un try catch pour chaque execution
        /// </summary>
        /// <typeparam name="T"> Resultat intermediaire</typeparam>
        /// <typeparam name="R">Résultat fibnale</typeparam>
        /// <param name="func">la fonction a exucuter</param>
        /// <param name="onSuccess">en cas de success</param>
        /// <param name="onError">en cas d'erreur</param>
        private R TryGet<T, R>(Func<T> func, Func<T, R> onSuccess, Func<R> onError)
        {
            try
            {
                T result = func();
                return onSuccess(result);
            }
            catch (Exception ex)
            {
                Log($"{ex.Message} - {Environment.NewLine} - {ex.StackTrace}");

                return onError();
            }
        }

        /// <summary>
        /// Retourne la liste des table sources disponibles
        /// </summary>
        /// <returns></returns>
        public eAdminCartoSelectionItems<eAdminCartoSelectionTable> LoadTabSource()
        {
            return TryGet(() => eAdminTools.GetListTabs(_pref),
                   (sources) =>
                   {

                       return new eAdminCartoSelectionItems<eAdminCartoSelectionTable>()
                       {
                           Items = sources.ConvertAll(e => new eAdminCartoSelectionTable() { Tab = e.Item1, Label = e.Item2 }).OrderBy(e => e.Tab).ToList()
                       };
                   },
                   () => 
                   {
                       return GetError<eAdminCartoSelectionItems<eAdminCartoSelectionTable>>(eResApp.GetRes(_pref, 2188), eResApp.GetRes(_pref, 2182));
                   });
        }

        /// <summary>
        /// Retourne la liste des signets associé à la table principale
        /// </summary>
        /// <param name="selectionTab"></param>
        /// <returns></returns>
        public eAdminCartoSelectionItems<eAdminCartoSelectionTable> LoadTabSelection(int selectionTab)
        {
            if (selectionTab <= 0)
                return GetError<eAdminCartoSelectionItems<eAdminCartoSelectionTable>>(eResApp.GetRes(_pref, 2188), eResApp.GetRes(_pref, 2183));

            return TryGet(() => eSqlDesc.GetBkm(_pref, selectionTab),
            (bkms) => new eAdminCartoSelectionItems<eAdminCartoSelectionTable>() { Items = bkms.Select(e => new eAdminCartoSelectionTable() { Tab = e.Key, Label = e.Value }).ToList() },
            () =>
            {               
                return GetError<eAdminCartoSelectionItems<eAdminCartoSelectionTable>>(eResApp.GetRes(_pref, 2188), $"{eResApp.GetRes(_pref, 2184)} - {selectionTab}");
            });
        }

        /// <summary>
        /// Récupère la liste des champs autorisés pour le filtrage
        /// </summary>
        /// <param name="sourceTab"></param>
        /// <returns></returns>
        public eAdminCartoSelectionItems<eAdminCartoSelectionField> LoadFields(int sourceTab)
        {
            if (sourceTab <= 0)
                return GetError<eAdminCartoSelectionItems<eAdminCartoSelectionField>>(eResApp.GetRes(_pref, 2188), eResApp.GetRes(_pref, 2185));

            // Champs de la table sources
            var fields = RetrieveFields.GetDefault(_pref)
                .AddOnlyThisTabs(new int[] { sourceTab })
                .AddOnlyThisFormats(supportedFormat)
                .AddOnlyThisPopupType(supportedPopup)
                .ResultFieldsInfo(eFieldLiteWithLib.Factory(_pref))
                .OrderBy(f => f.Libelle)
                .ToList();

            return new eAdminCartoSelectionItems<eAdminCartoSelectionField>()
            {
                Items = fields.ConvertAll(e => new eAdminCartoSelectionField()
                {
                    DescId = e.Descid,
                    Format = (int)e.Format,
                    CatalogMultiple = e.Multiple,
                    CatalogType = (int)e.Popup,
                    Label = e.Libelle
                })
            };
        }



        /// <summary>
        /// Sauvegarde la configuration du widget sous forme JSON
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public JSONReturnGeneric SaveConfig(string data)
        {
            string dependency;
            if (!ValidateConfig(data, out dependency))
                return GetError<eAdminCartoSelectionItems<eAdminCartoSelectionTable>>(eResApp.GetRes(_pref, 2188), eResApp.GetRes(_pref, 2186));

            return TryGet(() => new eXrmWidgetParam(_pref, _wid),
            (Param) =>
            {
                Param.SetParam(WIDGET_PARAM_CONFIG, data);
                Param.SetParam(WIDGET_PARAM_DEPENDENCY, dependency);

                string err;
                Param.Save(out err);
                if (!string.IsNullOrEmpty(err))
                    throw new Exception(err);

                return new JSONReturnGeneric() { Success = true };
            },
            () =>
            {
                return GetError<eAdminCartoSelectionItems<eAdminCartoSelectionTable>>(eResApp.GetRes(_pref, 2188), eResApp.GetRes(_pref, 2187));
            });
        }

        /// <summary>
        /// Permet de valider la config et retourner les dependence avec les tables et champs
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dependency"></param>      
        /// <returns></returns>
        private bool ValidateConfig(string data, out string dependency)
        {
            dependency = string.Empty;
          
            try
            {
                CartoSelectionConfigReturn CartoSelection = ParseConfig(data);
                if (!CartoSelection.Success)                                 
                    return false;                

                HashSet<int> used = new HashSet<int>();

                // Sauvegarde les dépendance avec les table/rubriques. Important lors de la modification/suppression de la structure
                CartoSelection.Config.AddDependency(used);

                used.Remove(0);
                used.Remove(-1);

                dependency = eLibTools.Join(";", used);

                return true;
            }
            catch (Exception ex)
            {              
                Log($"{ex.Message} \n {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Récupère la config du widget
        /// </summary>
        /// <returns></returns>
        public CartoSelectionConfigReturn GetConfig()
        {
            return TryGet(() => new eXrmWidgetParam(_pref, _wid),
               (Param) =>
               {                
                   return ParseConfig(Param.GetParamValue(WIDGET_PARAM_CONFIG));
               },
               () =>
               {     
                   return GetError<CartoSelectionConfigReturn>(eResApp.GetRes(_pref, 2188), $"{eResApp.GetRes(_pref, 2052)} - {_wid}");
               });

        }

        /// <summary>
        /// Permet de valider la config depuis la base
        /// </summary>
        /// <param name="data"></param>

        /// <returns></returns>
        private CartoSelectionConfigReturn ParseConfig(string data)
        {
            try
            {
                return new CartoSelectionConfigReturn()
                {
                    Success = true,
                    Config = SerializerTools.JsonDeserialize<eAdminCartoSelectionConfigDefinition>(data)
                };
            }
            catch (Exception ex)
            {
                Log($"{ex.Message} \n {ex.StackTrace}");
                return GetError<CartoSelectionConfigReturn>(eResApp.GetRes(_pref, 2188), eResApp.GetRes(_pref, 2189));
            }
        }

        /// <summary>
        /// Permet de  créer uen instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="title"></param>
        /// <param name="msg"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        private T GetError<T>(string title, string msg, string debug = "") where T : JSONReturnGeneric
        {
            T JSONReturn = Activator.CreateInstance<T>();
            JSONReturn.Success = false;
            JSONReturn.ErrorTitle = title;
            JSONReturn.ErrorMsg = msg;
            return JSONReturn;
        }
    }

    /// <summary>
    /// L'objet qui sera serialisé
    /// </summary>
    public class CartoSelectionConfigReturn : JSONReturnGeneric
    {
        /// <summary> Config du widget </summary>
        public eAdminCartoSelectionConfigDefinition Config = new eAdminCartoSelectionConfigDefinition();

        /// <summary>
        /// Construire un exemple de config
        /// </summary>
        /// <returns></returns>
        public static CartoSelectionConfigReturn GetDefaultAsExample(ePref pref)
        {
            string error;
            var mapping = eFilemapPartner.LoadCartoMapping(pref, 6100, out error);
            if (!string.IsNullOrEmpty(error))
            {
                return new CartoSelectionConfigReturn() { ErrorMsg = error, Success = false };
            }

            var geo = mapping?.Where(e => e.SsType == (int)CartographySsType.GEOGRAPHY).FirstOrDefault();
            var title = mapping?.Where(e => e.SsType == (int)CartographySsType.TITLE).FirstOrDefault();
            var subTitle = mapping?.Where(e => e.SsType == (int)CartographySsType.SUBTITLE).FirstOrDefault();
            var image = mapping?.Where(e => e.SsType == (int)CartographySsType.IMAGE).FirstOrDefault();
            var fields = mapping?.Where(e => e.SsType == (int)CartographySsType.FIELD).Select(e => new eAdminFieldMappingDefinition() { DescId = e.DescId, ShowLabel = e.SourceType == 1 }).ToList() ?? new List<eAdminFieldMappingDefinition>();

            eAdminInfoboxDefinition infobox = new eAdminInfoboxDefinition()
            {
                Geoloc = new eAdminFieldMappingDefinition() { DescId = geo?.DescId ?? 0, ShowLabel = geo == null ? false : geo.SourceType == 1 },
                Title = new eAdminFieldMappingDefinition() { DescId = title?.DescId ?? 0, ShowLabel = title == null ? false : title.SourceType == 1 },
                SubTitle = new eAdminFieldMappingDefinition() { DescId = subTitle?.DescId ?? 0, ShowLabel = subTitle == null ? false : subTitle.SourceType == 1 },
                Image = new eAdminFieldMappingDefinition() { DescId = image?.DescId ?? 0, ShowLabel = image == null ? false : image.SourceType == 1 },
                Fields = fields
            };


            return new CartoSelectionConfigReturn()
            {
                Success = true,
                Config = new eAdminCartoSelectionConfigDefinition()
                {
                    TableSelectionSection = new eAdminTableSelectionSectionDefinition()
                    {
                        SourceTab = 0,
                        DestinationTab = 0,
                        SelectionTab = 0,
                        RelationDescId = 0,
                        Options = new eAdminOptionsDefinition()
                        {
                            DefaultMapLocationName = "Paris",
                            FilterResumeBreakline = "<br/>",
                            FilterResumeDescId = 0
                        }
                    },
                    MappingSection = new eAdminMappingSectionDefinition()
                    {
                        Infobox = infobox,
                        Card = new eAdminCardDefinition()
                    },
                    FilterSection = new eAdminFilterSectionDefinition()
                    {
                        Title = new eAdminTextDefinition() { Text = "Critères" },
                        Groups = new List<eAdminGroupDefinition>()
                        {
                            new eAdminGroupDefinition() {
                                Name = new eAdminTextDefinition() { Text ="Localisation" },
                                Description = new eAdminTextDefinition() {  Text = "Indiquer la ville" },
                                Filters = new List<eAdminFilterDefinition>()
                                {
                                    new eAdminFilterDefinition()
                                    {
                                        CustomLabel =  new eAdminTextDefinition() {  Text = "Indiquer un libellé personnalisé" },
                                        UseFieldLabel =true,
                                        SourceDescId = 253
                                    }
                                }
                            },
                             new eAdminGroupDefinition() {
                                Name = new eAdminTextDefinition() { Text ="Fonction" },
                                Description = new eAdminTextDefinition() {  Text = "Indiquer une description sous forme d'une tooltip" },
                                Filters = new List<eAdminFilterDefinition>()
                                {
                                    new eAdminFilterDefinition()
                                    {
                                        CustomLabel =  new eAdminTextDefinition() {  Text = "Indiquer un libellé personnalisé" },
                                        UseFieldLabel =false,
                                        SourceDescId = 206
                                    },
                                     new eAdminFilterDefinition()
                                    {
                                        CustomLabel =  new eAdminTextDefinition() {  Text =  "Indiquer un libellé personnalisé"},
                                        UseFieldLabel =false,
                                        View="interval",
                                        SourceDescId = 221
                                    }
                                }
                            },
                             new eAdminGroupDefinition() {
                                Name = new eAdminTextDefinition() { Text ="NPAI" },
                                Description = new eAdminTextDefinition() {  Text = "Email NPAI" },
                                Filters = new List<eAdminFilterDefinition>()
                                {
                                    new eAdminFilterDefinition()
                                    {
                                        CustomLabel =  new eAdminTextDefinition() {  Text = "Indiquer un libellé personnalisé" },
                                        UseFieldLabel=true,
                                        SourceDescId = 231
                                    }
                                }
                            },
                            new eAdminGroupDefinition() {
                                Name = new eAdminTextDefinition() { Text ="Consentement CAN" },
                                Description = new eAdminTextDefinition() {  Text = "Indiquer le consentement CAN" },
                                Filters = new List<eAdminFilterDefinition>()
                                {
                                    new eAdminFilterDefinition()
                                    {
                                        CustomLabel =  new eAdminTextDefinition() {  Text = "Indiquer un libellé personnalisé" },
                                        UseFieldLabel=false,
                                        SourceDescId = 265
                                    }
                                }
                            },
                        }
                    }
                }
            };
        }
    }



    /// <summary>
    /// Config du widgets
    /// </summary>
    public class eAdminCartoSelectionConfigDefinition
    {
        /// <summary> Table de données concernées </summary>
        public eAdminTableSelectionSectionDefinition TableSelectionSection = new eAdminTableSelectionSectionDefinition();

        /// <summary> définition du mapping source et destination</summary>
        public eAdminMappingSectionDefinition MappingSection = new eAdminMappingSectionDefinition();

        /// <summary> définition des critères de filtres</summary>
        public eAdminFilterSectionDefinition FilterSection = new eAdminFilterSectionDefinition();

        /// <summary>
        /// retourne la liste des descid utilisé dans la config
        /// </summary>
        /// <returns></returns>
        public void AddDependency(HashSet<int> dependency)
        {
            TableSelectionSection.AddDependency(dependency);
            MappingSection.AddDependency(dependency);
            FilterSection.AddDependency(dependency);
        }
    }

    /// <summary>
    /// Defnition des tables concernées
    /// </summary>
    public class eAdminTableSelectionSectionDefinition
    {
        /// <summary> Table source </summary>
        public int SourceTab = 0;

        /// <summary> Table de selection </summary>
        public int SelectionTab = 0;

        /// <summary> Table destination </summary>
        public int DestinationTab = 0;

        /// <summary>DescId de la table si liaions native, ou descid du champ relation</summary>
        public int RelationDescId = 0;

        /// <summary>Options de la carto</summary>
        public eAdminOptionsDefinition Options = new eAdminOptionsDefinition();

        /// <summary>
        /// retourne la liste des descid utilisé dans la config
        /// </summary>
        /// <returns></returns>
        public void AddDependency(HashSet<int> dependency)
        {
            dependency.Add(SourceTab);
            dependency.Add(DestinationTab);
            dependency.Add(SelectionTab);
            dependency.Add(RelationDescId);
            Options.AddDependency(dependency);
        }
    }

    /// <summary>
    /// Defnition des options de la carto
    /// </summary>
    public class eAdminOptionsDefinition
    {
        /// <summary> Table/Champ </summary>
        public string DefaultMapLocationName = "";

        /// <summary> DescId du champ ou sauvegarder le résumé du filtre </summary>
        public int FilterResumeDescId = 0;

        /// <summary> </summary>
        public string FilterResumeBreakline = ",";

        /// <summary>
        /// retourne la liste des descid utilisé dans la config
        /// </summary>
        /// <returns></returns>
        public void AddDependency(HashSet<int> dependency)
        {
            dependency.Add(FilterResumeDescId);
        }
    }

    /// <summary>
    /// Defnition des champs mappés
    /// </summary>
    public class eAdminMappingSectionDefinition
    {
        /// <summary>
        /// Infobulle de position
        /// </summary>
        public eAdminInfoboxDefinition Infobox = new eAdminInfoboxDefinition();

        /// <summary>
        /// Mini-fiche
        /// </summary>
        public eAdminCardDefinition Card = new eAdminCardDefinition();

        /// <summary>
        /// retourne la liste des descid utilisé dans la config
        /// </summary>
        /// <returns></returns>
        public void AddDependency(HashSet<int> dependency)
        {
            Infobox.AddDependency(dependency);
            Card.AddDependency(dependency);
        }
    }

    /// <summary>
    /// Contient la definition de l'infobulle de position
    /// </summary>
    public class eAdminInfoboxDefinition
    {
        /// <summary> Champ geolocalisation </summary>
        public eAdminFieldMappingDefinition Geoloc = new eAdminFieldMappingDefinition();

        /// <summary> Champ de type image </summary>
        public eAdminFieldMappingDefinition Image = new eAdminFieldMappingDefinition();

        /// <summary>Titre de l'infobulle </summary>
        public eAdminFieldMappingDefinition Title = new eAdminFieldMappingDefinition();

        /// <summary> Sous-titre de l'infobulle </summary>
        public eAdminFieldMappingDefinition SubTitle = new eAdminFieldMappingDefinition();

        /// <summary> List des champs aditionnels dans l'infobulle </summary>
        public List<eAdminFieldMappingDefinition> Fields = new List<eAdminFieldMappingDefinition>();

        /// <summary>
        /// Ajoute a la liste des dependences les descid utilisés ici
        /// </summary>
        /// <returns></returns>
        public void AddDependency(HashSet<int> dependency)
        {
            dependency.Add(Geoloc.DescId);
            dependency.Add(Image.DescId);
            dependency.Add(Title.DescId);
            dependency.Add(SubTitle.DescId);

            foreach (var f in Fields)
                dependency.Add(f.DescId);
        }
    }

    /// <summary>
    /// Contient la definition de la mini-fiche
    /// </summary>
    public class eAdminCardDefinition
    {
        /// <summary> Source d'images</summary>
        public eAdminImageStoreDefinition ImageStore = new eAdminImageStoreDefinition();


        /// <summary> Tables relations</summary>
        public List<eAdminTabDefinition> Tabs = new List<eAdminTabDefinition>();

        /// <summary>
        /// Ajoute a la liste des dependences les descid utilisés ici
        /// </summary>
        /// <returns></returns>
        public void AddDependency(HashSet<int> dependency)
        {
            foreach (var t in Tabs)
                t.AddDependency(dependency);

            ImageStore.AddDependency(dependency);
        }
    }

    /// <summary>
    /// Définition des tables a afficher
    /// </summary>
    public class eAdminTabDefinition
    {

        /// <summary>
        /// Liaisons vers la table parente
        /// </summary>
        public int RelationDescId = 0;

        /// <summary>Titre de l'infobulle </summary>
        public eAdminFieldMappingDefinition Title = new eAdminFieldMappingDefinition();

        /// <summary> List des champs aditionnels dans l'infobulle </summary>
        public List<eAdminFieldMappingDefinition> Fields = new List<eAdminFieldMappingDefinition>();

        /// <summary>
        /// Ajoute a la liste des dependences les descid utilisés ici
        /// </summary>
        /// <returns></returns>
        public void AddDependency(HashSet<int> dependency)
        {
            dependency.Add(Title.DescId);

            foreach (var f in Fields)
                dependency.Add(f.DescId);
        }
    }

    /// <summary>
    /// Contient la definition de la source des images
    /// </summary>
    public class eAdminImageStoreDefinition
    {
        /// <summary>Source d'image : 91 ou champs de type Image, ou liason native ou liasion relationnelle</summary>
        public eAdminFieldMappingDefinition Source = new eAdminFieldMappingDefinition();

        /// <summary> Si liaisons native ou relationnnelle, rubriques de la table parente</summary>
        public List<eAdminFieldMappingDefinition> Fields = new List<eAdminFieldMappingDefinition>();

        /// <summary>
        ///ajoute a la liste des descid utilisé 
        /// </summary>
        /// <returns></returns>
        public void AddDependency(HashSet<int> dependency)
        {
            dependency.Add(Source.DescId);

            foreach (var f in Fields)
                dependency.Add(f.DescId);
        }
    }

    /// <summary>
    /// Définit si on affiche le labelen plus
    /// </summary>
    public class eAdminFieldMappingDefinition
    {
        /// <summary> DescId mappé</summary>
        public int DescId = 0;

        /// <summary>Affiche ou pas le libellé du champ </summary>
        public bool ShowLabel = false;
    }

    /// <summary>
    /// Defnition des filtres
    /// </summary>
    public class eAdminFilterSectionDefinition
    {
        /// <summary> titre global </summary>
        public eAdminTextDefinition Title = new eAdminTextDefinition();

        /// <summary> Correspondance entre descid source et destination</summary>
        public List<eAdminGroupDefinition> Groups = new List<eAdminGroupDefinition>();

        /// <summary>
        ///ajoute a la liste des descid utilisé 
        /// </summary>
        /// <returns></returns>
        public void AddDependency(HashSet<int> dependency)
        {
            foreach (eAdminGroupDefinition g in Groups)
                g.AddDependency(dependency);
        }
    }

    /// <summary>
    /// Defnition de la res
    /// </summary>
    public class eAdminTextDefinition
    {
        /// <summary>Label par défaut</summary>
        public string Text = string.Empty;

        ///// <summary> Id de la res si on accepte de gérer des res par ResFileId comme eRessource.xml mais dans une table a part</summary>
        //public int ResFileId = -1;
    }

    /// <summary>
    /// Defnition des groupes
    /// </summary>
    public class eAdminGroupDefinition
    {
        /// <summary>Nom du groupe </summary>
        public eAdminTextDefinition Name = new eAdminTextDefinition();

        /// <summary>Description du groupe </summary>
        public eAdminTextDefinition Description = new eAdminTextDefinition();

        /// <summary> Définition des champ du filtre</summary>
        public List<eAdminFilterDefinition> Filters = new List<eAdminFilterDefinition>();

        /// <summary>
        ///ajoute a la liste des descid utilisé 
        /// </summary>
        /// <returns></returns>
        public void AddDependency(HashSet<int> dependency)
        {
            foreach (eAdminFilterDefinition f in Filters)
                f.AddDependency(dependency);
        }
    }

    /// <summary>
    /// Defnition des filtres
    /// </summary>
    public class eAdminFilterDefinition
    {
        /// <summary>Nom du groupe </summary>
        public eAdminTextDefinition CustomLabel = new eAdminTextDefinition();

        /// <summary>affiché le libellé du champs</summary>
        public bool UseFieldLabel = true;

        /// <summary>Champ source utilisé</summary>
        public int SourceDescId = 0;

        /// <summary>
        /// "default", on affiche les operateurs
        /// "interval" on affiche minimum-maximum
        /// </summary>
        public string View = "default";

        /// <summary>Unité de mesure</summary>
        public string Unit = string.Empty;

        /// <summary>
        ///ajoute a la liste des descid utilisé 
        /// </summary>
        /// <returns></returns>
        public void AddDependency(HashSet<int> dependency)
        {
            dependency.Add(SourceDescId);
        }
    }


    /// <summary>
    /// Config des données source a utiliser
    /// </summary>
    public class eAdminCartoSelectionConfigSource
    {
        /// <summary>
        /// Tables source
        /// </summary>
        public List<eAdminCartoSelectionTable> Sources = new List<eAdminCartoSelectionTable>();

        /// <summary>
        /// Opérateurs de comparaisons
        /// </summary>
        public List<eAdminCartoSelectionOp> Operators = new List<eAdminCartoSelectionOp>();

        /// <summary>
        /// Champs de la première table
        /// </summary>
        public List<eAdminCartoSelectionField> Fields = new List<eAdminCartoSelectionField>();
    }


    /// <summary>
    /// Le résultat de la demande sous forme JSON
    /// </summary>
    public class eAdminCartoSelectionItems<T> : JSONReturnGeneric
    {
        /// <summary>
        /// Données à envoyer au client
        /// </summary>
        public List<T> Items = new List<T>();

        /// <summary>
        /// Objet regroupant la liste des élements demandé
        /// </summary>
        public eAdminCartoSelectionItems() { Success = true; }
    }

    /// <summary>
    /// représente un element : table ou champ
    /// </summary>
    public class eAdminCartoSelectionField
    {
        /// <summary>
        /// Identifiant de l'element
        /// </summary>
        public int DescId = 0;

        /// <summary>
        /// Identifiant de l'element
        /// </summary>
        public int Format = 0;

        /// <summary>
        /// Type de catalogue 
        /// </summary>
        public int CatalogType = 0;

        /// <summary>
        /// Type de catalogue 
        /// </summary>
        public bool CatalogMultiple = false;

        /// <summary>
        /// Libellé de l'element
        /// </summary>
        public string Label = string.Empty;
    }

    /// <summary>
    /// représente un element : table ou champ
    /// </summary>
    public class eAdminCartoSelectionTable
    {
        /// <summary>
        /// Identifiant de l'element
        /// </summary>
        public int Tab = 0;

        /// <summary>
        /// Libellé de l'element
        /// </summary>
        public string Label = string.Empty;

    }
    /// <summary>
    /// représente un element : table ou champ
    /// </summary>
    public class eAdminCartoSelectionOp
    {
        /// <summary>
        /// Identifiant de l'element
        /// </summary>
        public string Value = string.Empty;

        /// <summary>
        /// Libellé de l'element
        /// </summary>
        public string Label = string.Empty;
    }


}