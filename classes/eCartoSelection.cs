using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Common.CommonDTO;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Result;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Xrm.eda;
using EudoQuery;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Class permettant de gérer l'utilisation du widget selection cartographique
    /// </summary>
    public class eCartoSelection
    {
        /// <summary>
        /// Logger ne fait rien par défaut
        /// </summary>
        Action<string> Log = (s) => { };

        /// <summary>
        /// Nombre max de pushpin affichée sur la carte
        /// </summary>
        public static int MAX_RECORD = 100;

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
        private eCartoSelection(ePref pref, int wid)
        {
            this._pref = pref;
            this._wid = wid;

            if (_pref.ModeDebug)
                Log = eModelTools.EudoTraceLogger(pref, "[Sélection cartographique]");
        }

        /// <summary>
        /// Applique les critère de recherche 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public CartoResponse<CartoRecord> ApplyCriteria(Criteria criteria)
        {
            var response = new CartoResponse<CartoRecord>();
            response.Success = true;

            try
            {
                // Récupération de la config admin               
                eAdminCartoSelectionConfigDefinition adminConfig = LoadAdminConfig();


                HashSet<int> descidList = new HashSet<int>();

                // Infobulle
                descidList.Add(adminConfig.MappingSection.Infobox.Geoloc.DescId);
                descidList.Add(adminConfig.MappingSection.Infobox.Title.DescId);
                descidList.Add(adminConfig.MappingSection.Infobox.SubTitle.DescId);
                descidList.Add(adminConfig.MappingSection.Infobox.Image.DescId);
                descidList.UnionWith(adminConfig.MappingSection.Infobox.Fields.Select(e => e.DescId));
                descidList.UnionWith(adminConfig.FilterSection.Groups.SelectMany(e => e.Filters.Select(f => f.SourceDescId)));

                var card = adminConfig.MappingSection.Card;

                Dictionary<int, HashSet<int>> relations = new Dictionary<int, HashSet<int>>();

                // 
                relations[adminConfig.TableSelectionSection.SourceTab] = new HashSet<int>(descidList);
                if (!relations.ContainsKey(card.ImageStore.Source.DescId))
                    relations[card.ImageStore.Source.DescId] = new HashSet<int>();

                relations[card.ImageStore.Source.DescId].UnionWith(card.ImageStore.Fields.Select(e => e.DescId));
                card.Tabs.ForEach(tab =>
                {
                    if (!relations.ContainsKey(tab.RelationDescId))
                        relations[tab.RelationDescId] = new HashSet<int>();

                    relations[tab.RelationDescId].Union(tab.Fields.Select(e => e.DescId));
                    relations[tab.RelationDescId].Add(tab.Title.DescId);
                });

                var result = relations.Select(kv =>
                {
                    if (kv.Key == adminConfig.TableSelectionSection.SourceTab || (kv.Key % 100 == 0 && kv.Key != adminConfig.TableSelectionSection.SourceTab))
                    {
                        return eLibTools.Join(";", kv.Value);
                    }
                    else if (eLibTools.GetTabFromDescId(kv.Key) == adminConfig.TableSelectionSection.SourceTab)
                    {
                        return $"{kv.Key},{kv.Value.First()};{eLibTools.Join($";", kv.Value)}";
                    }
                    else
                    {
                        return string.Empty;
                    }
                }).Where(s => !string.IsNullOrEmpty(s));

                if (result.Count() == 0)
                {
                    response.Success = false;
                    response.ErrorTitle = eResApp.GetRes(_pref, 8633);// "La liste des rubriques mappée non compatible";
                    response.ErrorMsg = eResApp.GetRes(_pref, 6342);

                    return response;
                }

                string col = eLibTools.Join(";", result);

                // récup des fields
                EqCaches caches = EqCaches.GetNew();
                EudoQuery.EudoQuery fields = eLibTools.GetEudoQuery(_pref, adminConfig.TableSelectionSection.SourceTab, ViewQuery.CUSTOM, caches);
                fields.SetListCol = eLibTools.Join(";", result);
                fields.LoadRequest();

                eDataFillerGeneric dataFiller = new eDataFillerGeneric(_pref, adminConfig.TableSelectionSection.SourceTab, ViewQuery.CUSTOM);
                dataFiller.EudoqueryCaches = caches;
                dataFiller.EudoqueryComplementaryOptions = (query) =>
               {
                   query.SetTopRecord = MAX_RECORD + 1; // +1 pour savoir s'il y a encore des fiches au dela de la limite
                   query.SetListCol = eLibTools.Join(";", result);
                   if (!string.IsNullOrWhiteSpace(criteria.MapCenter))
                       query.AddParam("geofrom", $"geography::STGeomFromText('{criteria.MapCenter}', 4326)");

                   List<WhereCustom> wheres = new List<WhereCustom>();
                   wheres.Add(new WhereCustom(adminConfig.MappingSection.Infobox.Geoloc.DescId.ToString(), Operator.OP_IS_NOT_EMPTY, ""));

                   if (!string.IsNullOrWhiteSpace(criteria.MapBounds))
                       wheres.Add(new WhereCustom(adminConfig.MappingSection.Infobox.Geoloc.DescId.ToString(), Operator.OP_IN_POLYGON, criteria.MapBounds));

                   ILookup<int, Field> lookup = fields.GetFieldHeaderList.ToLookup(f => f.Descid, f => f);
                   criteria.Filters.ForEach(filter =>
                   {
                       Field fld = lookup[filter.DescId].FirstOrDefault();
                       if (fld == null || !fld.PermViewAll)
                           return;

                       switch (filter.Type)
                       {
                           case "interval":
                               if (fld.Format != FieldFormat.TYP_DATE &&
                                     fld.Format != FieldFormat.TYP_MONEY &&
                                     fld.Format != FieldFormat.TYP_NUMERIC)
                                   return;

                               if (!string.IsNullOrWhiteSpace(filter.MinValue) && !string.IsNullOrWhiteSpace(filter.MaxValue))
                               {
                                   if (fld.Format == FieldFormat.TYP_DATE)
                                   {
                                       wheres.Add(new WhereCustom(filter.DescId.ToString(), Operator.OP_BETWEEN, $"{filter.MinValue}{SEPARATOR.OP_BETWEEN}{filter.MaxValue}"));
                                   }
                                   else
                                   {
                                       wheres.Add(new WhereCustom(filter.DescId.ToString(), Operator.OP_GREATER_OR_EQUAL, filter.MinValue));
                                       wheres.Add(new WhereCustom(filter.DescId.ToString(), Operator.OP_LESS_OR_EQUAL, filter.MaxValue));
                                   }

                               }
                               else if (!string.IsNullOrWhiteSpace(filter.MinValue) && string.IsNullOrWhiteSpace(filter.MaxValue))
                               {
                                   wheres.Add(new WhereCustom(filter.DescId.ToString(), Operator.OP_GREATER_OR_EQUAL, filter.MinValue));

                               }
                               else if (string.IsNullOrWhiteSpace(filter.MinValue) && !string.IsNullOrWhiteSpace(filter.MaxValue))
                               {
                                   wheres.Add(new WhereCustom(filter.DescId.ToString(), Operator.OP_LESS_OR_EQUAL, filter.MaxValue));
                               }
                               break;

                           case "catalog":
                               // fld pas un catalog
                               if (fld.Popup == PopupType.NONE)
                                   return;

                               if (!string.IsNullOrWhiteSpace(filter.Value))
                               {
                                   if (fld.Multiple)
                                       wheres.Add(new WhereCustom(filter.DescId.ToString(), Operator.OP_IN_LIST, filter.Value));
                                   else
                                       wheres.Add(new WhereCustom(filter.DescId.ToString(), Operator.OP_EQUAL, filter.Value));
                               }

                               break;

                           case "text":
                               // fld pas un catalog
                               if (fld.Format != FieldFormat.TYP_CHAR)
                                   return;

                               if (!string.IsNullOrWhiteSpace(filter.Value))
                                   wheres.Add(new WhereCustom(filter.DescId.ToString(), Operator.OP_CONTAIN, filter.Value));
                               break;

                           case "logic":
                               // fld pas un catalog
                               if (fld.Format != FieldFormat.TYP_BIT)
                                   return;

                               if (!string.IsNullOrWhiteSpace(filter.Value))
                               {
                                   bool bVal;
                                   if (bool.TryParse(filter.Value, out bVal))
                                       wheres.Add(new WhereCustom(filter.DescId.ToString(), bVal ? Operator.OP_IS_TRUE : Operator.OP_IS_FALSE, string.Empty));
                               }

                               break;
                       }

                   });

                   query.AddCustomFilter(new WhereCustom(wheres));
               };

                dataFiller.Generate();

                if (dataFiller.ListRecords == null || dataFiller.ListRecords.Count() == 0)
                    return response;

                var adminInfobox = adminConfig.MappingSection.Infobox;
                dataFiller.ListRecords.ForEach(record =>
                {
                    CartoRecord cartoRecord = new CartoRecord();
                    cartoRecord.FileId = record.MainFileid;
                    cartoRecord.SourceTab = adminConfig.TableSelectionSection.SourceTab;

                    SetInfobox(cartoRecord, record, adminConfig);
                    SetCard(cartoRecord, record, adminConfig);

                    response.Results.Add(cartoRecord);
                });
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorTitle = eResApp.GetRes(_pref, 416); 
                response.ErrorMsg = eResApp.GetRes(_pref, 2052);

                Log($"{ex.Message} : {Environment.NewLine} {ex.StackTrace}");
                
            }

            return response;
        }

        /// <summary>
        /// Mis  a jour les info infobulle de position
        /// </summary>
        /// <param name="cartoRecord"></param>
        /// <param name="record"></param>
        /// <param name="adminConfig"></param>
        private void SetInfobox(CartoRecord cartoRecord, eRecord record, eAdminCartoSelectionConfigDefinition adminConfig)
        {
            eAdminInfoboxDefinition adminInfobox = adminConfig.MappingSection.Infobox;
            cartoRecord.Infobox = new CartoInfobox();
            cartoRecord.Infobox.Geography = GetDisplayValue(record, adminConfig.TableSelectionSection.SourceTab, adminInfobox.Geoloc.DescId, false, "");
            cartoRecord.Infobox.Title = GetDisplayValue(record, adminConfig.TableSelectionSection.SourceTab, adminInfobox.Title.DescId, adminInfobox.Title.ShowLabel, "");
            cartoRecord.Infobox.SubTitle = GetDisplayValue(record, adminConfig.TableSelectionSection.SourceTab, adminInfobox.SubTitle.DescId, adminInfobox.SubTitle.ShowLabel, "");
            cartoRecord.Infobox.ImageSource = GetDisplayValue(record, adminConfig.TableSelectionSection.SourceTab, adminInfobox.Image.DescId, adminInfobox.Image.ShowLabel, "themes/default/images/image.png");

            adminConfig.MappingSection.Infobox.Fields.ForEach(field =>
            {
                string value = GetDisplayValue(record, adminConfig.TableSelectionSection.SourceTab, field.DescId, field.ShowLabel, "");
                if (!string.IsNullOrWhiteSpace(value))
                    cartoRecord.Infobox.Fields.Add(value);
            });
        }
        /// <summary>
        /// Mis  a jour les info infobulle de position
        /// </summary>
        /// <param name="cartoRecord"></param>
        /// <param name="record"></param>
        /// <param name="adminConfig"></param>
        private void SetCard(CartoRecord cartoRecord, eRecord record, eAdminCartoSelectionConfigDefinition adminConfig)
        {
            eAdminCardDefinition adminCard = adminConfig.MappingSection.Card;

            cartoRecord.Card = new CartoCard();
            cartoRecord.Card.Images = adminCard.ImageStore.Fields
                .Select(f => GetDisplayValue(record, adminConfig.TableSelectionSection.SourceTab, f.DescId, f.ShowLabel, ""))
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToList();

            // if (cartoRecord.Card.Images.Count == 0)
            //     cartoRecord.Card.Images.Add("themes/default/images/image.png");

            cartoRecord.Card.Tabs = adminCard.Tabs
            .Where(link =>
            {
                // la table principale
                if (adminConfig.TableSelectionSection.SourceTab == link.RelationDescId || link.RelationDescId == 0)
                    return true;

                // liaisons parentes doivent exister
                if (record.TablesFileId.ContainsKey($"{adminConfig.TableSelectionSection.SourceTab}_{link.RelationDescId}"))
                    return record.TablesFileId[$"{adminConfig.TableSelectionSection.SourceTab}_{link.RelationDescId}"] > 0;

                // liaisons inconnu on ignore
                return false;

            })
            .Select(tab =>
            {
                CartoTab ct = new CartoTab()
                {
                    Title = GetDisplayValue(record, adminConfig.TableSelectionSection.SourceTab, tab.Title.DescId, tab.Title.ShowLabel, ""),
                    Fields = tab.Fields
                    .Select(f => GetDisplayValue(record, adminConfig.TableSelectionSection.SourceTab, f.DescId, f.ShowLabel, ""))
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .ToList()
                };

                if (string.IsNullOrWhiteSpace(ct.Title) && ct.Fields.Count == 0)
                    return null;

                return ct;
            })
            .Where(t => t != null)
            .ToList();
        }

        /// <summary>
        /// Récupère la valeur de sélection
        /// </summary>
        /// <param name="record"></param>
        /// <param name="sourceTab"></param>
        /// <param name="descId"></param>
        /// <param name="showLabel"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private string GetDisplayValue(eRecord record, int sourceTab, int descId, bool showLabel, string defaultValue)
        {
            if (descId == 0)
                return defaultValue;

            try
            {
                var fldRecord = record.GetFieldByAlias($"{sourceTab}_{descId}");
                if (fldRecord == null)
                    return defaultValue;

                if (fldRecord.FldInfo.Format == FieldFormat.TYP_IMAGE)
                {
                    if (!String.IsNullOrEmpty(fldRecord.Value) && fldRecord.FldInfo.ImgStorage == ImageStorage.STORE_IN_FILE)
                    {
                        return String.Concat(eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.FILES, _pref.GetBaseName), "/", fldRecord.Value);
                    }

                    return defaultValue;
                }

                if (fldRecord.FldInfo.Format == FieldFormat.TYP_BIT)
                {
                    fldRecord.DisplayValue = fldRecord.Value == "1" ? eResApp.GetRes(_pref, 58) : eResApp.GetRes(_pref, 59);
                }


                if (showLabel)
                    return $"{fldRecord.FldInfo.Libelle} : {fldRecord.DisplayValue}";

                return fldRecord.DisplayValue;
            }
            catch(Exception ex)
            {
                Log($"{ex.Message} : {Environment.NewLine} {ex.StackTrace}");

                return defaultValue;
            }
        }

        /// <summary>
        /// Sélection à enregistrer 
        /// </summary>
        /// <param name="cardSelection"></param>
        /// <returns></returns>
        public CartoResponse<string> SaveSelection(CardSelection cardSelection)
        {
            var response = new CartoResponse<string>();
            response.Success = true;

            try
            {
                // Pas de id de la fiche parente              
                eAdminTableSelectionSectionDefinition adminConfig = LoadAdminConfig().TableSelectionSection;

                if (cardSelection.ParentSelectionId <= 0)
                {
                    response.Success = false;                   
                    response.ErrorTitle = eResApp.GetRes(_pref, 6524);
                    response.ErrorMsg = eResApp.GetRes(_pref, 2192); 

                    Log($"L'id de la fiche parente n'est pas transmis pour le widget {_wid}");

                    return response;
                }

                if (cardSelection.Selections.Count <= 0)
                    return response;

                // le test est fait une fois
                Action<Engine.Engine, int, int> addRelation;
                if (adminConfig.RelationDescId % 100 == 0)
                    addRelation = (__engine, __descId, __id) => { __engine.AddTabValue(__descId, __id); };
                else
                    addRelation = (__engine, __descId, __id) => { __engine.AddNewValue(__descId, __id.ToString()); };

                List<string> errors = new List<string>();
                List<int> success = new List<int>();
                Engine.Engine engine = eModelTools.GetEngine(_pref, adminConfig.DestinationTab, eEngineCallContext.GetCallContext(Common.Enumerations.EngineContext.APPLI));
                cardSelection.Selections.ForEach(fileId =>
                {
                    engine.ResetContextAndValues();                    
        

                    engine.AddTabValue(adminConfig.SelectionTab, cardSelection.ParentSelectionId);



                    addRelation(engine, adminConfig.RelationDescId, fileId);

   
                    engine.EngineProcess(new StrategyCruSimple());
                    if (engine.Result.Success)
                        success.Add(engine.Result.Record.Id);
                    else
                        errors.Add($"{eResApp.GetRes(_pref, 190)} {fileId} : {engine.Result.Error.Msg}");
                });

                // 1 ligne succès
                response.Results.Add(eResApp.GetRes(_pref, 874).Replace("<ITEM>", $"{success.Count()}/{cardSelection.Selections.Count()}"));

                //2 ligne echec
                if (errors.Count() > 0)
                {
                    response.Results.Add(eResApp.GetRes(_pref, 874).Replace("<ITEM>", $"{errors.Count()}/{cardSelection.Selections.Count()}"));

                    // les erreurs engines
                    response.Results.AddRange(errors);
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorTitle = eResApp.GetRes(_pref, 617);
                response.ErrorMsg = eResApp.GetRes(_pref, 2178);

                Log($"{ex.Message} : {Environment.NewLine} {ex.StackTrace}");
            }

            return response;
        }

        /// <summary>
        /// Sélection à enregistrer 
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        public CartoResponse<int> GetSelectionCount(CardSelection selection)
        {
            var response = new CartoResponse<int>();
            response.Success = true;

            try
            {
                // Récupération de la config admin               
                eAdminCartoSelectionConfigDefinition adminConfig = LoadAdminConfig();

                if (selection.ParentSelectionId <= 0)
                {
                    response.Success = false;
                    response.ErrorTitle = "La fiche parente non transmise";
                    response.ErrorMsg = "La fiche parente non transmise";

                    Log($"L'id de la fiche parente n'est pas transmis pour le widget {_wid}");

                    return response;
                }

                var config = adminConfig.TableSelectionSection;

                EudoQuery.EudoQuery query = eLibTools.GetEudoQuery(_pref, config.DestinationTab, ViewQuery.CUSTOM);
                query.SetParentDescid = config.SelectionTab;
                query.SetParentFileId = selection.ParentSelectionId;

                // Construction de la requete
                query.LoadRequest();
                query.BuildRequest();

                RqParam sqlCount = new RqParam(query.EqCountQuery);
                using (var dal = eLibTools.GetEudoDAL(_pref))
                {

                    dal.OpenDatabase();
                    string error;

                    int nbResults = dal.ExecuteScalar<int>(sqlCount, out error);
                    if (error.Length != 0)
                    {
                        response.Success = false;
                        response.ErrorTitle = "Impossible le comptage des sélection";
                        response.ErrorMsg = "Impossible le comptage des sélection";

                        Log($"execution du dal.ExecuteScalar: {error}");
                        return response;
                    }

                    response.Results.Add(nbResults);
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorTitle = "Impossible de récupérer le nombre de sélection";
                response.ErrorMsg = "Impossible de récupérer le nombre de sélection";

                Log($"{ex.Message} : {Environment.NewLine} {ex.StackTrace}");
            }

            return response;
        }

        /// <summary>
        /// Methode de création de gestionnaire du widget
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="widgetId"></param>
        /// <returns></returns>
        public static eCartoSelection Create(ePref pref, int widgetId)
        {
            return new eCartoSelection(pref, widgetId);
        }

     
        /// <summary>
        /// Retourne la liste des table sources disponibles
        /// </summary>
        /// <returns></returns>
        public CartoSelectionConfigReturn LoadConfig()
        {
            try
            {
                // Récupération de la config admin               
                eAdminCartoSelectionConfigDefinition adminConfig = LoadAdminConfig();

                CartoSelectionConfigReturn userConfig = new CartoSelectionConfigReturn();

                // Transformation de la config admin en config utilisable
                Transform(adminConfig, userConfig);

                return userConfig;
            }
            catch (Exception ex)
            {
                Log($"{ex.Message} : {Environment.NewLine} {ex.StackTrace}");

                return new CartoSelectionConfigReturn()
                {
                    Success = false,
                    ErrorTitle = "Impossible de récupérer la configuration du widget",
                    ErrorMsg = "Impossible de récupérer la configuration du widget"
                };               
            }
        }

        /// <summary>
        /// Chargement de la config admins
        /// </summary>
        /// <returns></returns>
        private eAdminCartoSelectionConfigDefinition LoadAdminConfig()
        {
            // Récupération de la config admin
            eXrmWidgetParam param = new eXrmWidgetParam(_pref, _wid);
            string data = param.GetParamValue(eAdminCartoSelection.WIDGET_PARAM_CONFIG);
            eAdminCartoSelectionConfigDefinition adminConfig = SerializerTools.JsonDeserialize<eAdminCartoSelectionConfigDefinition>(data);

            return adminConfig;
        }

        /// <summary>
        /// Charge les champs des sources et 
        /// </summary>
        /// <param name="adminConfig"></param>
        /// <param name="userConfig"></param>
        private void Transform(eAdminCartoSelectionConfigDefinition adminConfig, CartoSelectionConfigReturn userConfig)
        {
            userConfig.Success = true;

            // Pas de config en base
            if (adminConfig == null)
            {
                userConfig.Success = false;
                userConfig.ErrorTitle = "Le widget n'est pas paramétré";
                userConfig.ErrorMsg = "Le widget n'est pas paramétré";

                Log($"Pas de config sauvegardée pour le widget {_wid}");
                return;
            }

            HashSet<int> descidList = new HashSet<int>();

            descidList.UnionWith(adminConfig.FilterSection.Groups.SelectMany(e => e.Filters.Select(f => f.SourceDescId)));
            string listCol = eLibTools.Join(";", descidList);

            // Pas de liste de champ source à récupérer
            if (string.IsNullOrEmpty(listCol))
            {
                userConfig.Success = false;
                userConfig.ErrorTitle = "Les critères de recherche ne sont pas définit";
                userConfig.ErrorMsg = "Les critères de recherche ne sont pas définit";

                Log($"Aucune rubrique de critère n'est définit pour le widget {_wid} !");

                return;
            }

            EudoQuery.EudoQuery query = eLibTools.GetEudoQuery(_pref, adminConfig.TableSelectionSection.SourceTab, ViewQuery.CUSTOM);
            query.SetListCol = listCol;
            query.LoadRequest();

            var fields = query.GetFieldHeaderList.ToDictionary(e => e.Descid, e => e);

            // Pas de droit de visu sur la table source
            TableMain source = query.GetMainTable;
            if (!source.PermViewAll)
            {
                userConfig.Success = false;
                userConfig.ErrorTitle = $"Vous n'avez pas le droit de visualisation sur la table {source.Libelle}";
                userConfig.ErrorMsg = $"Vous n'avez pas le droit de visualisation sur la table {source.Libelle}";
                return;
            }

            userConfig.Selection.SourceTab = new DescIdLabelDefinition() { DescId = source.DescId, Label = source.Libelle };
            userConfig.Selection.DestinationTab = new DescIdLabelDefinition() { DescId = adminConfig.TableSelectionSection.DestinationTab };
            userConfig.Selection.SelectionTab = new DescIdLabelDefinition() { DescId = adminConfig.TableSelectionSection.SelectionTab };
            userConfig.Selection.RelationDescId = new DescIdLabelDefinition() { DescId = adminConfig.TableSelectionSection.RelationDescId };

            userConfig.Selection.Options.DefaultMapLocationName = adminConfig.TableSelectionSection.Options.DefaultMapLocationName;
            userConfig.Selection.Options.FilterResumeDescId = adminConfig.TableSelectionSection.Options.FilterResumeDescId;
            userConfig.Selection.Options.FilterResumeBreakline = adminConfig.TableSelectionSection.Options.FilterResumeBreakline;

            // On peut faire any, mais dans le cas de plusieurs fois le descid ?
            adminConfig.FilterSection.Groups.ForEach(adminGroup =>
            {
                // groupe
                GroupDefinition usageGroup = new GroupDefinition();
                usageGroup.Name = adminGroup.Name.Text;
                usageGroup.Description = adminGroup.Description.Text;

                // Filters
                adminGroup.Filters.ForEach(adminFilter =>
                {
                    // Si le champ n'existe pas on ignore le filtre
                    if (!fields.ContainsKey(adminFilter.SourceDescId))
                        return;

                    // Pas de droit de visu, on igonre le filtre
                    Field field = fields[adminFilter.SourceDescId];
                    if (!field.PermViewAll)
                        return;

                    // Pas de champ non supporté
                    if (!eAdminCartoSelection.supportedFormat.Contains(field.Format))
                        return;

                    // Pas de catalog non supporté
                    if (!eAdminCartoSelection.supportedPopup.Contains(field.Popup))
                        return;

                    FilterDefinition usageFilter = new FilterDefinition();
                    usageFilter.SourceDescId = adminFilter.SourceDescId;
                    usageFilter.FilterName = $"{{{field.Table.Libelle}}}.{{{field.Libelle}}}";
                    usageFilter.Label = adminFilter.UseFieldLabel ? field.Libelle : usageFilter.Label = adminFilter.CustomLabel.Text;
                    usageFilter.Unit = adminFilter.Unit;
                    usageGroup.Filters.Add(usageFilter);

                    SetMetadata(field, usageFilter);
                    SetClientAction(field, usageFilter);

                });

                if (usageGroup.Filters.Count() > 0)
                    userConfig.CriteriaConfig.Groups.Add(usageGroup);
            });
        }

        /// <summary>
        /// Mise a jour des infos de config
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="adminInfoboxConfig"></param>
        /// <param name="usageInfoboxConfig"></param>
        private void SetMappingInfobox(Dictionary<int, Field> fields, eAdminInfoboxDefinition adminInfoboxConfig, eInfoboxDefinition usageInfoboxConfig)
        {
            UpdateMappingField(fields, adminInfoboxConfig.Geoloc, usageInfoboxConfig.Geoloc);
            UpdateMappingField(fields, adminInfoboxConfig.Title, usageInfoboxConfig.Title);
            UpdateMappingField(fields, adminInfoboxConfig.SubTitle, usageInfoboxConfig.SubTitle);
            UpdateMappingField(fields, adminInfoboxConfig.Image, usageInfoboxConfig.Image);

            usageInfoboxConfig.Fields.Clear();
            adminInfoboxConfig.Fields.ForEach(admFld =>
            {
                eFieldMappingDefinition usageField = new eFieldMappingDefinition();
                usageInfoboxConfig.Fields.Add(usageField);
                UpdateMappingField(fields, admFld, usageField);
            });
        }

        /// <summary>
        /// Récup des infos d'admin
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="adminField"></param>
        /// <param name="usageField"></param>
        private void UpdateMappingField(Dictionary<int, Field> fields, eAdminFieldMappingDefinition adminField, eFieldMappingDefinition usageField)
        {
            usageField.DescId = adminField.DescId;
            usageField.ShowLabel = adminField.ShowLabel;

            if (adminField.ShowLabel && fields.ContainsKey(adminField.DescId))
                usageField.Label = fields[adminField.DescId].Libelle;
        }

        /// <summary>
        /// Ajoute les méta description de la rubrique
        /// </summary>
        /// <param name="field"></param>
        /// <param name="filter"></param>
        private void SetMetadata(Field field, FilterDefinition filter)
        {
            filter.Meta.Add("did", field.Descid.ToString());
            filter.Meta.Add("ename", $"fld_{field.Table.DescId}_{field.Descid}_{_wid}");
            filter.Meta.Add("popid", field.PopupDescId.ToString());
            filter.Meta.Add("mult", (field.Multiple ? "1" : "0"));
            filter.Meta.Add("tree", (field.bTreeView ? "1" : "0"));
            filter.Meta.Add("pop", ((int)field.Popup).ToString());
            filter.Meta.Add("bndid", field.BoundDescid.ToString());
            filter.Meta.Add("special", (field.Popup == PopupType.SPECIAL) ? "1" : "0");
        }

        /// <summary>
        /// En fonction du champ, on retourne l'action JS correspondante
        /// </summary>
        /// <param name="field"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private void SetClientAction(Field field, FilterDefinition filter)
        {

            switch (field.Format)
            {
                case FieldFormat.TYP_CHAR:
                    // pas pris en compte
                    if (field.bTreeView)
                        goto default;

                    SetCharAction(field, filter);
                    break;
                case FieldFormat.TYP_DATE:
                    filter.Action = "LNKDATE";
                    break;
                case FieldFormat.TYP_BIT:
                    filter.Action = "LNKCHECK";
                    break;
                case FieldFormat.TYP_NUMERIC:
                case FieldFormat.TYP_MONEY:
                    filter.Action = "LNKNUM";
                    break;
                default:
                    filter.Action = "NOSUPPORT";
                    break;
            }
        }

        /// <summary>
        /// En fonction si c'est un catalog ou pas
        /// </summary>
        /// <returns></returns>
        private void SetCharAction(Field field, FilterDefinition filter)
        {
            switch (field.Popup)
            {
                // Pas de catalogue
                case PopupType.NONE:
                    filter.Action = "LNKFREETEXT";
                    break;

                case PopupType.FREE:
                    filter.Action = "LNKFREECAT";
                    break;

                case PopupType.ONLY:
                    filter.Action = "LNKCAT";
                    break;

                case PopupType.DATA:
                    filter.Action = "LNKADVCAT";
                    break;

                default:
                    // les
                    filter.Action = "NOSUPPORT";
                    break;
            }
        }
    }

    /// <summary>
    /// L'objet qui sera serialisé
    /// </summary>
    public class CartoSelectionConfigReturn : JSONReturnGeneric
    {

        /// <summary> Config de la sélection </summary>
        public SelectionSectionDefinition Selection = new SelectionSectionDefinition();

        /// <summary> Config du widget </summary>
        public CriteriaSectionDefinition CriteriaConfig = new CriteriaSectionDefinition();
    }

    /// <summary>
    /// Defnition des tables concernées
    /// </summary>
    public class SelectionSectionDefinition
    {
        /// <summary> Table source </summary>
        public DescIdLabelDefinition SourceTab = new DescIdLabelDefinition();

        /// <summary> Table destination </summary>
        public DescIdLabelDefinition DestinationTab = new DescIdLabelDefinition();

        /// <summary> Table de selection </summary>
        public DescIdLabelDefinition SelectionTab = new DescIdLabelDefinition();

        /// <summary>DescId de la table si liaions native, ou descid du champ relation</summary>
        public DescIdLabelDefinition RelationDescId = new DescIdLabelDefinition();

        /// <summary>Option supplémentaire de la carto</summary>
        public OptionsDefinition Options = new OptionsDefinition();
    }

    /// <summary>
    /// Defnition des options de la carto
    /// </summary>
    public class OptionsDefinition
    {
        /// <summary> Table/Champ </summary>
        public string DefaultMapLocationName = "";

        /// <summary> Descid du champ ou stoqué le résume </summary>
        public int FilterResumeDescId = 0;

        /// <summary> Retour à la ligne</summary>
        public string FilterResumeBreakline = ", ";
    }

    /// <summary>
    /// Defnition des tables concernées
    /// </summary>
    public class DescIdLabelDefinition
    {
        /// <summary> Table/Champ </summary>
        public int DescId = 0;

        /// <summary> Libellé </summary>
        public string Label = string.Empty;
    }

    /// <summary>
    /// Defnition des champs mappés
    /// </summary>
    public class MappingSectionDefinition
    {
        /// <summary>
        /// Correspondance entre descid source et destination
        /// </summary>
        public List<MapFieldDefinition> Fields = new List<MapFieldDefinition>();

        /// <summary>
        /// Correspondance entre descid source et destination
        /// </summary>
        public eInfoboxDefinition Infobox = new eInfoboxDefinition();

        /// <summary>
        /// Correspondance entre descid source et destination
        /// </summary>
        public eCardDefinition Card = new eCardDefinition();
    }

    /// <summary>
    /// Contient la definition de l'infobulle de position
    /// </summary>
    public class eInfoboxDefinition
    {
        /// <summary> Champ geolocalisation </summary>
        public eFieldMappingDefinition Geoloc = new eFieldMappingDefinition();

        /// <summary> Champ de type image </summary>
        public eFieldMappingDefinition Image = new eFieldMappingDefinition();

        /// <summary>Titre de l'infobulle </summary>
        public eFieldMappingDefinition Title = new eFieldMappingDefinition();

        /// <summary> Sous-titre de l'infobulle </summary>
        public eFieldMappingDefinition SubTitle = new eFieldMappingDefinition();

        /// <summary> List des champs aditionnels dans l'infobulle </summary>
        public List<eFieldMappingDefinition> Fields = new List<eFieldMappingDefinition>();
    }

    /// <summary>
    /// Contient la definition de la mini-fiche
    /// </summary>
    public class eCardDefinition
    {
        /// <summary> Source d'images</summary>
        public eImageStoreDefinition ImageStore = new eImageStoreDefinition();

        /// <summary> Si liaisons native ou relationnnelle, rubriques de la table parente</summary>
        public List<eTabDefinition> Tabs = new List<eTabDefinition>();
    }

    /// <summary>
    /// Définition des tables a afficher
    /// </summary>
    public class eTabDefinition
    {
        /// <summary>Titre de l'infobulle </summary>
        public eFieldMappingDefinition Title = new eFieldMappingDefinition();

        /// <summary> List des champs aditionnels dans l'infobulle </summary>
        public List<eFieldMappingDefinition> Fields = new List<eFieldMappingDefinition>();
    }

    /// <summary>
    /// Contient la definition de la source des images
    /// </summary>
    public class eImageStoreDefinition
    {
        /// <summary>Source d'image : 91 ou champs de type Image, ou liason native ou liasion relationnelle</summary>
        public eFieldMappingDefinition Source = new eFieldMappingDefinition();

        /// <summary> Si liaisons native ou relationnnelle, rubriques de la table parente</summary>
        public List<eFieldMappingDefinition> Fields = new List<eFieldMappingDefinition>();
    }

    /// <summary>
    /// Définit si on affiche le labelen plus
    /// </summary>
    public class eFieldMappingDefinition
    {
        /// <summary> DescId mappé</summary>
        public int DescId = 0;

        /// <summary>Affiche ou pas le libellé du champ </summary>
        public bool ShowLabel = false;

        /// <summary>Libellé du champ </summary>
        public string Label = string.Empty;
    }

    /// <summary>
    /// Defnition des tables concernées
    /// </summary>
    public class MapFieldDefinition
    {
        /// <summary> champ source </summary>
        public int SourceDescId = 0;

        /// <summary> champ destination par defaut ou la valeur min si intervalle</summary>
        public int DestinationDescId = 0;

        /// <summary> champ destination pour la valeur min si intervalle</summary>
        public int DestinationDescIdMax = 0;
    }

    /// <summary>
    /// Defnition des filtres
    /// </summary>
    public class CriteriaSectionDefinition
    {
        /// <summary> titre global </summary>
        public string Title = string.Empty;

        /// <summary> Correspondance entre descid source et destination</summary>
        public List<GroupDefinition> Groups = new List<GroupDefinition>();

    }

    /// <summary>
    /// Defnition des groupes
    /// </summary>
    public class GroupDefinition
    {
        /// <summary>Nom du groupe </summary>
        public string Name = string.Empty;

        /// <summary>Description du groupe </summary>
        public string Description = string.Empty;

        /// <summary> Définition des champ du filtre</summary>
        public List<FilterDefinition> Filters = new List<FilterDefinition>();
    }

    /// <summary>
    /// Defnition des filtres
    /// </summary>
    public class FilterDefinition
    {
        /// <summary>Libellé du filtre</summary>
        public string FilterName = string.Empty;

        /// <summary>Libellé du filtre</summary>
        public string Label = string.Empty;

        /// <summary>Champ source utilisé</summary>
        public int SourceDescId = 0;

        /// <summary>Type d'action</summary>
        public string Action = string.Empty;

        /// <summary>
        /// "default", on affiche les operateurs
        /// "interval" on affiche minimum-maximum
        /// </summary>
        public string Dispaly = "default";

        /// <summary>Unité de mesure</summary>
        public string Unit = string.Empty;

        /// <summary>Métadonnées sur la rubrique</summary>
        public Dictionary<string, string> Meta = new Dictionary<string, string>();
    }
}