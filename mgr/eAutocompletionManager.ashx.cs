using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Description résumée de eAutocompletionManager
    /// </summary>
    public class eAutoCompletionManager : eEudoManager
    {
        ICompletionSet CompletionSet;
        String _error;
        FieldMappingResult _result;
        int _descid = 0;
        int _fileid = 0;
        string _token = String.Empty;
        string _searchInfo = String.Empty;
        bool _fromSirene = false; // Indique si le chargement du mapping doit être fait depuis FILEMAP_PARTNER (DataGouv, BingMapsV8) ou autre (Sirene)

        /// <summary>
        /// Token de connexion (si utilisé)
        /// </summary>
        public string Token
        {
            get
            {
                return _token;
            }

            set
            {
                _token = value;
            }
        }

        /// <summary>
        /// Informations à afficher concernant la recherche effectuée
        /// </summary>
        public string SearchInfo
        {
            get
            {
                return _searchInfo;
            }

            set
            {
                _searchInfo = value;
            }
        }

        #region Enum        
        /// <summary>
        /// Type d'action sur le manager
        /// Mettre à jour eAutoCompletion.js également
        /// </summary>
        enum ActionType
        {
            Undefined = 0,
            /// <summary>
            /// Renvoie la liste des suggestions de Data.gouv.fr
            /// </summary>
            GetDataGouvSuggestions = 1,
            /// <summary>
            /// Renvoie le mapping des champs pour l'autocomplétion
            /// </summary>
            GetAddressMapping = 2,
            /// <summary>
            /// Renvoie une liste de champs pouvant être mis à jour avec Engine
            /// </summary>
            GetFieldsToUpdate = 3,
            /// <summary>
            /// Renvoie la liste des suggestions de la base Sirene
            /// </summary>
            GetSireneSuggestions = 4,
            /// <summary>
            /// Renvoie le mapping de la base Sirene
            /// </summary>
            GetSireneMapping = 5
        }
        #endregion

        #region Struct
        [DataContract]
        public struct FieldMapping
        {
            [DataMember]
            public int DescID;
            [DataMember]
            public string Source;
            [DataMember]
            public bool AddCatValue;
        }
        [DataContract]
        struct FieldMappingResult
        {
            [DataMember]
            public bool Success;
            [DataMember]
            public string Error;
            [DataMember]
            public string DebugError;
            [DataMember]
            public List<FieldMapping> ListMapping;
        }

        /// <summary>
        /// Structure d'un champ à mettre à jour
        /// </summary>
        [DataContract]
        struct FieldToUpdate
        {
            [DataMember]
            public int DescID;
            [DataMember]
            public string DbValue;
            [DataMember]
            public string DisplayValue;
            [DataMember]
            public int BoundDescid;
            [DataMember]
            public int PopupDescid;
            //[DataMember]
            //public eUpdateField Field;
        }
        #endregion

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            try
            {
                // Transmis depuis le js
                _descid = _requestTools.GetRequestFormKeyI("descid") ?? 0;

                int nAction = _requestTools.GetRequestFormKeyI("action") ?? 0;
                ActionType action = (ActionType)nAction;

                _fileid = _requestTools.GetRequestFormKeyI("fid") ?? 0;

                _fromSirene = _requestTools.GetRequestFormKeyB("fromSirene") ?? false;

                if (action == ActionType.GetDataGouvSuggestions)
                {
                    #region Suggestions data.gouv.fr
                    String searchWord = _requestTools.GetRequestFormKeyS("search");

                    AutoCompletionType autoCompleteType = AutoCompletionType.NONE;
                    autoCompleteType = eFilemapPartner.GetAutoCompletionType(_pref, _descid, out _error);

                    // #67 138 : dans le cas où le champ déclencheur ne figure pas parmi les champs mappés à mettre à jour, on ne déclencherait pas d'autocomplétion
                    // car GetAutoCompletionType() n'aurait rien renvoyé, alors qu'il peut avoir été décidé de déclencher la recherche sur un champ X pour uniquement remplir les champs Y ou Z.
                    // Dans ce cas de figure, on vérifie si l'autocomplétion a été activée sur un des champs de l'onglet, et si tel est le cas, on déclenche une autocomplétion d'adresses à défaut
                    if (autoCompleteType == AutoCompletionType.NONE && String.IsNullOrEmpty(_error))
                    {
                        Dictionary<Int32, Int32> dicoAutocompleteAddress = Internal.eda.eSqlDesc.LoadAutocompleteAddressFields(_pref, eLibTools.GetTabFromDescId(_descid));
                        if (dicoAutocompleteAddress.Keys.Contains(_descid) && dicoAutocompleteAddress[_descid] == 1)
                            autoCompleteType = AutoCompletionType.AUTO_COMPLETION_ADDRESS;
                    }

                    if (String.IsNullOrEmpty(_error))
                    {

                        switch (autoCompleteType)
                        {
                            case AutoCompletionType.AUTO_COMPLETION_ADDRESS:

                                // Récuperation de l'adresse de fournisseur
                                IDictionary<eLibConst.CONFIGADV, String> dico = eLibTools.GetConfigAdvValues(_pref, new HashSet<eLibConst.CONFIGADV> { eLibConst.CONFIGADV.AUTO_COMPLETION_ADR_PROVIDER });

                                //Creation de fournisseur de service
                                IDataProvider<IDataAddressItem> dataProvider = new DataGouvProvider(dico[eLibConst.CONFIGADV.AUTO_COMPLETION_ADR_PROVIDER]);

                                //Affectation du résulat
                                eAutoCompletion<IDataAddressItem> autoCompletion = new eAutoCompletion<IDataAddressItem>(dataProvider);
                                List<IDataAddressItem> resultData = autoCompletion.Find(_pref, searchWord);
                                SearchInfo = autoCompletion.Provider.GetSearchInfo(); // récupération des informations concernant la recherche
                                CompletionSet = new eCompletionAddress(_pref, _descid, dataProvider, resultData);
                                break;

                            default:
                                _error = string.Concat(" Pas d'implémentation pour " + autoCompleteType);
                                break;

                        }
                    }

                    RenderXML();
                    return;
                    #endregion

                }
                else if (action == ActionType.GetSireneSuggestions)
                {
                    #region Suggestions Sirene
                    String searchWord = _requestTools.GetRequestFormKeyS("search");
                    Token = _requestTools.GetRequestFormKeyS("token"); // récupération du token stocké côté JS/navigateur

                    /*
                    Pour le référentiel Sirene, on ne vérifie pas le paramétrage depuis FILEMAP_PARTNER, car le mapping n'y est pas stocké.
                    On saute donc l'étape de vérification ci-dessous

                    AutoCompletionType autoCompleteType = AutoCompletionType.NONE;
                    autoCompleteType = eFilemapPartner.GetAutoCompletionType(_pref, _descid, out _error);

                    if (String.IsNullOrEmpty(_error))
                    {
                        switch (autoCompleteType)
                        {
                            case AutoCompletionType.AUTO_COMPLETION_ADDRESS:
                            */
                    // Récuperation de l'adresse de fournisseur
                    string error = String.Empty;
                    IDictionary<eLibConst.CONFIGADV, String> dicoConfig = new Dictionary<eLibConst.CONFIGADV, String>();

                    try
                    {
                        dicoConfig = eLibTools.GetConfigAdvValues(_pref,
                            new HashSet<eLibConst.CONFIGADV> {
                                                            eLibConst.CONFIGADV.SIRENE_API_URL
                                }
                            );
                    }
                    catch (Exception e)
                    {
                        dicoConfig = null;
                        error = String.Concat("L'URL d'accès au référentiel Sirene n'est pas définie. L'extension a t-elle été activée ?", Environment.NewLine, e.Message, Environment.NewLine, e.StackTrace);
                    }

                    if (dicoConfig != null && dicoConfig.ContainsKey(eLibConst.CONFIGADV.SIRENE_API_URL))
                    {
                        //Creation de fournisseur de service
                        IDataProvider<IDataAddressItem> dataProvider = new SireneProvider(dicoConfig[eLibConst.CONFIGADV.SIRENE_API_URL], _pref, Token);

                        //Affectation du résulat
                        eAutoCompletion<IDataAddressItem> autoCompletion = new eAutoCompletion<IDataAddressItem>(dataProvider);
                        List<IDataAddressItem> resultData = autoCompletion.Find(_pref, searchWord);
                        Token = ((SireneProvider)autoCompletion.Provider).Token; // récupération du token éventuellement regénéré via un appel à Authenticate() par le provider
                        SearchInfo = autoCompletion.Provider.GetSearchInfo(); // récupération des informations concernant la recherche
                        CompletionSet = new eCompletionAddress(_pref, _descid, dataProvider, resultData);
                    }
                    //break;
                    /*
                default:
                    _error = string.Concat(" Pas d'implémentation pour " + autoCompleteType);
                    break;
            }
        }
        */

                    RenderXML();
                    return;
                    #endregion

                }
                else if (action == ActionType.GetAddressMapping)
                {
                    #region Autocomplete mapping
                    List<FieldMapping> listMapping = new List<FieldMapping>();
                    FieldMapping mapping;
                    _result.Error = string.Empty;
                    _result.DebugError = string.Empty;

                    try
                    {
                        List<eFilemapPartner> list = eAutoCompletionTools.Load(_pref, _descid, FILEMAP_TYPE.AUTOCOMPLETE, AutoCompletionType.AUTO_COMPLETION_ADDRESS, false, out _error);
                        if (!String.IsNullOrEmpty(_error))
                            throw new Exception(_error);

                        foreach (eFilemapPartner m in list)
                        {
                            mapping = new FieldMapping { DescID = m.DescId, Source = m.Source, AddCatValue = m.CreateCatalogValue };
                            listMapping.Add(mapping);
                        }

                        _result.Success = true;
                        _result.ListMapping = listMapping;
                    }
                    catch (Exception exc)
                    {
                        _result.Success = false;
                        _result.Error = eResApp.GetRes(_pref, 5171);
                        _result.DebugError = string.Concat(exc.Message, " - ", exc.StackTrace);
                        ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 1760), eResApp.GetRes(_pref, 5171), "", _result.DebugError);
                        LaunchError();
                    }

                    RenderResult(RequestContentType.TEXT, delegate () { return SerializerTools.JsonSerialize(_result); });
                    #endregion

                }
                else if (action == ActionType.GetFieldsToUpdate)
                {
                    #region Liste des champs à mettre à jour, après traitement pour les catalogues

                    eErrorContainer errorContainer = null;
                    List<FieldToUpdate> fields = GetFieldsToUpdate(_fromSirene, out errorContainer);
                    if (errorContainer == null)
                        RenderResult(RequestContentType.TEXT, delegate () { return SerializerTools.JsonSerialize(fields); });
                    else
                    {
                        _result.Success = false;
                        _result.Error = errorContainer.Msg;
                        _result.DebugError = errorContainer.DebugMsg;
                        ErrorContainer = errorContainer;
                        LaunchError();
                    }


                    #endregion
                }
            }
            catch (eEndResponseException) { _context.Response.End(); }
            catch (ThreadAbortException)
            {

            }
            catch (Exception e)
            {

                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", e.Message);
                LaunchError();

            }
        }

        private void RenderXML()
        {
            // Init le document XML
            _xmlResult = new XmlDocument();
            XmlNode _mainNode = _xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
            _xmlResult.AppendChild(_mainNode);

            XmlNode _detailsNode = _xmlResult.CreateElement("autocomplete");
            _xmlResult.AppendChild(_detailsNode);

            XmlNode _successNode = _xmlResult.CreateElement("success");
            _detailsNode.AppendChild(_successNode);

            if (!String.IsNullOrEmpty(_error) || CompletionSet == null)
            {
                _successNode.InnerText = "0";
                XmlNode errorNode = _xmlResult.CreateElement("error");
                errorNode.InnerText = eResApp.GetRes(_pref.LangId, 72);
                _detailsNode.AppendChild(errorNode);
#if DEBUG

                XmlNode debugNode = _xmlResult.CreateElement("debug");
                debugNode.InnerText = _error;
                _detailsNode.AppendChild(debugNode);
#endif

            }
            else
            {
                _successNode.InnerText = "1";

                XmlNode _tokenNode = _xmlResult.CreateElement("token");
                _tokenNode.InnerText = Token;
                _detailsNode.AppendChild(_tokenNode);

                XmlNode _countNode = _xmlResult.CreateElement("count");
                _countNode.InnerText = CompletionSet.CompletionLines().Count.ToString();
                _detailsNode.AppendChild(_countNode);

                XmlNode _noResultsNode = _xmlResult.CreateElement("noResults");
                _noResultsNode.InnerText = CompletionSet.CompletionLines().Count == 0 ? eResApp.GetRes(_pref, 7612) : String.Empty;
                _detailsNode.AppendChild(_noResultsNode);

                XmlNode _searchInfoNode = _xmlResult.CreateElement("searchInfo");
                _searchInfoNode.InnerText = SearchInfo;
                _detailsNode.AppendChild(_searchInfoNode);

                XmlNode _optionsNode = _xmlResult.CreateElement("options");
                _detailsNode.AppendChild(_optionsNode);

                XmlNode _optionNode, _itemNode, _lineNode, _valuesNode, _valueNode, _tooltipNode;
                XmlAttribute _attribute;

                foreach (eCompletionLine completionLine in CompletionSet.CompletionLines())
                {
                    _optionNode = _xmlResult.CreateElement("option");
                    _optionsNode.AppendChild(_optionNode);

                    // La première ligne sera affichée comme option  
                    _lineNode = _xmlResult.CreateElement("line");
                    _optionNode.AppendChild(_lineNode);

                    _attribute = _xmlResult.CreateAttribute("key");
                    _lineNode.Attributes.Append(_attribute);
                    _attribute.Value = completionLine.Key;

                    // La première ligne sera affichée comme option  
                    _valuesNode = _xmlResult.CreateElement("values");
                    _lineNode.AppendChild(_valuesNode);
                    int valueIndex = -1;
                    foreach (string value in completionLine.Values)
                    {
                        valueIndex++;
                        _valueNode = _xmlResult.CreateElement("value");
                        _valuesNode.AppendChild(_valueNode);
                        _valueNode.InnerText = value;
                        if (completionLine.UsedFieldsForValues != null && valueIndex < completionLine.UsedFieldsForValues.Count)
                        {
                            _attribute = _xmlResult.CreateAttribute("usedfields");
                            _attribute.Value = String.Join(";", completionLine.UsedFieldsForValues[valueIndex]);
                            _valueNode.Attributes.Append(_attribute);
                        }
                    }

                    // La première ligne sera affichée comme option  
                    _tooltipNode = _xmlResult.CreateElement("tooltip");
                    _lineNode.AppendChild(_tooltipNode);
                    _tooltipNode.InnerText = completionLine.Tooltip;
                    if (completionLine.UsedFieldsForTooltip != null && completionLine.UsedFieldsForTooltip.Count > 0)
                    {
                        _attribute = _xmlResult.CreateAttribute("usedfields");
                        _attribute.Value = String.Join(";", completionLine.UsedFieldsForTooltip);
                        _tooltipNode.Attributes.Append(_attribute);
                    }

                    _attribute = _xmlResult.CreateAttribute("badge");
                    _lineNode.Attributes.Append(_attribute);
                    _attribute.Value = completionLine.Badge;

                    _attribute = _xmlResult.CreateAttribute("badgeClass");
                    _lineNode.Attributes.Append(_attribute);
                    _attribute.Value = completionLine.BadgeClass;


                    // Quand l'utilisateur clique sur la ligne, les champs suivant seront mis à jour
                    foreach (eCompletionItem completionItem in completionLine.CompletionItems)
                    {
                        _itemNode = _xmlResult.CreateElement(completionLine.Key);
                        _itemNode.InnerText = completionItem.Value;
                        _attribute = _xmlResult.CreateAttribute("descid");
                        _attribute.Value = completionItem.Descid.ToString();
                        _itemNode.Attributes.Append(_attribute);
                        _attribute = _xmlResult.CreateAttribute("label");
                        _attribute.Value = completionItem.Label;
                        _itemNode.Attributes.Append(_attribute);
                        _attribute = _xmlResult.CreateAttribute("source");
                        _attribute.Value = completionItem.Source;
                        _itemNode.Attributes.Append(_attribute);
                        _optionNode.AppendChild(_itemNode);
                    }
                }
            }

            RenderResult(RequestContentType.XML, delegate () { return _xmlResult.OuterXml; });
        }



        /// <summary>
        /// Retourne la liste des champs à mettre à jour
        /// </summary>
        /// <returns></returns>
        private List<FieldToUpdate> GetFieldsToUpdate(bool fromSirene, out eErrorContainer errorContainer)
        {
            errorContainer = null;
            eudoDAL eDal = null;
            List<FieldToUpdate> fieldsListToUpdate = new List<FieldToUpdate>();
            List<eUpdateField> fieldsList = new List<eUpdateField>();
            eUpdateField updField;
            string newValue = string.Empty;
            string displayValue = string.Empty;
            int boundDescid = 0;
            int popupDescid = 0;
            FieldToUpdate fieldToUpdate = new FieldToUpdate()
            {
                DescID = 0,
                DbValue = newValue,
                DisplayValue = displayValue,
                BoundDescid = boundDescid,
                PopupDescid = popupDescid
            };

            #region Récupération des champs depuis les paramètres
            foreach (String key in _requestTools.AllKeys)
            {
                if (!key.StartsWith("fld_"))
                    continue;
                
                updField = eUpdateField.GetUpdateFieldFromDesc(_requestTools.GetRequestFormKeyS(key));
                if (updField != null)
                    fieldsList.Add(updField);
            }
            #endregion

            if (fieldsList.Count > 0)
            {
                eDal = eLibTools.GetEudoDAL(_pref);
                eDal.OpenDatabase();

                try
                {
                    #region Recherche des champs à mettre à jour de type catalogue avancé - Récup de l'option "CreateCatalogValue"
                    List<eUpdateField> catFieldsList = fieldsList.FindAll(f => f.Popup == PopupType.DATA);
                    if (catFieldsList.Count > 0)
                    {
                        // S'il y en a, on vérifie 
                        eUpdateField catField = null;
                        List<eFilemapPartner> list = eAutoCompletionTools.Load(_pref, _descid, FILEMAP_TYPE.AUTOCOMPLETE, AutoCompletionType.AUTO_COMPLETION_ADDRESS, fromSirene, out _sMsgError);
                        foreach (eFilemapPartner filemapPartner in list)
                        {
                            catField = catFieldsList.FirstOrDefault(f => f.Descid == filemapPartner.DescId);
                            if (catField == null)
                                continue;
                            catField.AddValueInCatalog = filemapPartner.CreateCatalogValue;
                        }
                    }
                    #endregion

                    #region Parcours de la liste des champs pour traitement

                    foreach (eUpdateField field in fieldsList)
                    {
                        if (field.ReadOnly)
                            continue;
                        //Vérification des permissions
                        ePermission updatePerm = ePermission.GetPermission(_pref, field.Descid, eDal, eLibConst.TREATMENTLOCATION.UpdatePermId);
                        //Si l'utilisateur n'a pas les permissions
                        if (updatePerm != null && !updatePerm.IsAllowed(_pref.User))
                            continue;

                        bool addField = true;
                        newValue = string.Empty;
                        displayValue = field.NewValue;

                        if (field.Popup == PopupType.DATA && field.NewValue != null && field.NewValue.Trim().Length != 0)
                        {
                            /* Recherche de la valeur dans le catalogue
                            #62 782 :
                            On effectue d'abord une recherche de la valeur sur "Code" puis "Libellé".
                                - Si la valeur renvoyée par le référentiel correspond à un code du catalogue sans ambiguité, on sélectionne cette valeur
                                - Si la valeur ne correspond pas à un code mais correspond à un des libellés des valeurs du catalogue, on sélectionne la première valeur détectée
                                - Si aucune correspondance, créer la valeur avec Code = Valeur si le catalogue est à code.
                                - Si la valeur renvoyée par le référentiel est vide, on ne fait rien
                             */
                            eCatalog catalog = new eCatalog(eDal, _pref, field.Popup, _pref.User, (field.PopupDescId > 0 && field.Descid != field.PopupDescId) ? field.PopupDescId : field.Descid);
                            eCatalog.CatalogValue cv = null;
                            if (catalog != null && catalog.Values != null)
                                cv = catalog.Values.Find(
                                    cVal => (
                                        //cVal.Id == int.Parse(field.NewValue) ||
                                        cVal.Data == field.NewValue ||
                                        cVal.DbValue == field.NewValue ||
                                        cVal.DisplayValue == field.NewValue
                                    )
                                );
                            if (cv == null)
                                cv = catalog.Find(new eCatalog.CatalogValue(field.NewValue));

                            if (cv == null)
                            {
                                displayValue = string.Empty;

                                // Si la valeur n'existe pas, on la crée ou pas selon l'option définie en admin 
                                //BSE:#64 182 : on ajoute une valeur au catalogue seulement si on a les droits
                                if (field.AddValueInCatalog && catalog.AddAllowed)
                                {
                                    newValue = eTools.AddNewValueInCatalog(eDal, _pref, field, true, true, out _sMsgError);

                                    if (!String.IsNullOrEmpty(_sMsgError))
                                    {
                                        errorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL,
                                             eResApp.GetRes(_pref, 92),
                                            $"Erreur d'ajout de nouvelle valeur {field.NewValue} dans le catalogue dont le DescId est {field.PopupDescId}", "", _sMsgError);
                                        //LaunchError(ErrorContainer); // Pas de lancement d'erreur ici, renvoi à l'appelant pour traitement approprié
                                    }
                                    else
                                    {
                                        displayValue = field.NewValue;
                                    }

                                }
                                else
                                    addField = false;

                            }
                            else
                            {
                                // Si la valeur existe
                                newValue = cv.DbValue;
                                displayValue = cv.DisplayValue;
                            }

                        }
                        else
                        {
                            newValue = field.NewValue;
                        }

                        if (addField)
                        {
                            fieldToUpdate = new FieldToUpdate()
                            {
                                DescID = field.Descid,
                                DbValue = newValue,
                                DisplayValue = displayValue,
                                BoundDescid = (field.Popup == PopupType.DATA) ? field.BoundDescid : 0,
                                PopupDescid = field.PopupDescId
                            };

                            fieldsListToUpdate.Add(fieldToUpdate);
                        }

                    }
                    #endregion

                    #region Post-traitement

                    BoundCatalogsPostTreatment(eDal, fieldsListToUpdate);

                    #endregion


                }
                catch (Exception exc)
                {
                    ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 544), "",
                                            String.Concat(exc.Message, " : ", exc.StackTrace));
                    LaunchError(ErrorContainer);
                }
                finally
                {
                    eDal.CloseDatabase();
                }

            }

            return fieldsListToUpdate;
        }

        /// <summary>
        /// Met à jour le ParentDataId de la valeur catalogue
        /// </summary>
        /// <param name="eDal">Connexion ouverte</param>
        /// <param name="descID">DescID du catalogue</param>
        /// <param name="dataID">DataID de la valeur</param>
        /// <param name="parentDataID">ParentDataID à mettre à jour</param>
        /// <param name="error">Erreur retournée</param>
        /// <returns></returns>
        private bool SetParentDataId(eudoDAL eDal, int descID, int dataID, int parentDataID, out string error)
        {
            error = string.Empty;
            string query = "UPDATE [FILEDATA] SET ParentDataId = @parentID WHERE DataId = @id AND Descid = @did";
            RqParam rq = new RqParam(query);
            rq.AddInputParameter("@parentID", System.Data.SqlDbType.Int, parentDataID);
            rq.AddInputParameter("@id", System.Data.SqlDbType.Int, dataID);
            rq.AddInputParameter("@did", System.Data.SqlDbType.Int, descID);

            return eDal.ExecuteNonQuery(rq, out error) > 0;
        }

        /// <summary>
        /// Post-traitement pour affecter les ParentDataID aux valeurs sélectionnées dans le cas des catalogues liés
        /// </summary>
        /// <param name="eDal">Connexion ouverte</param>
        /// <param name="fieldsListToUpdate">Liste des champs à mettre à jour</param>
        private void BoundCatalogsPostTreatment(eudoDAL eDal, List<FieldToUpdate> fieldsListToUpdate)
        {
            int boundValue = 0, dataId = 0;
            FieldToUpdate loopedField;

            // Recherche des rubriques qui sont liées à un catalogue qui est dans le mapping

            List<FieldToUpdate> listWithBound = fieldsListToUpdate.FindAll(f => f.BoundDescid > 0);

            for (int i = 0; i < listWithBound.Count; i++)
            {
                loopedField = listWithBound[i];
                boundValue = 0;
                dataId = 0;

                FieldToUpdate boundField = fieldsListToUpdate.FirstOrDefault(u => u.DescID == loopedField.BoundDescid);

                // Si le catalogue parent comporte une valeur, on va mettre à jour le ParentDataId du DataID

                if (!String.IsNullOrEmpty(boundField.DbValue))
                {
                    boundValue = eLibTools.GetNum(boundField.DbValue);
                    dataId = eLibTools.GetNum(loopedField.DbValue);
                    if (boundValue > 0 && dataId > 0)
                    {
                        if (!SetParentDataId(eDal, loopedField.PopupDescid, dataId, boundValue, out _sMsgError))
                            throw new Exception(_sMsgError);
                    }
                }
                else
                {
                    // Commenté car à priori cela est déjà géré

                    // Si le catalogue n'est pas dans le mapping, donc pas dans les champs à mettre à jour, il faut aller récupérer la valeur pour associer la valeur à la valeur parente

                    //if (_descid % 100 == 0 && _fileid > 0 // Si le descid est bien le descid d'une table et qu'on est sur une fiche existante
                    //    && boundField.DescID == 0 && boundField.BoundDescid == 0
                    //    )
                    //{
                    //    eDataFillerGeneric df = new eDataFillerGeneric(_pref, _descid, ViewQuery.CUSTOM);
                    //    df.EudoqueryComplementaryOptions = delegate (EudoQuery.EudoQuery eq)
                    //    {
                    //        eq.SetListCol = loopedField.BoundDescid.ToString();
                    //        eq.SetFileId = _fileid;
                    //    };
                    //    df.Generate();
                    //    if (String.IsNullOrEmpty(df.ErrorMsg))
                    //    {
                    //        eRecord rec = df.GetFirstRow();
                    //        if (rec != null)
                    //        {
                    //            eFieldRecord fRec = rec.GetFieldByAlias(String.Concat(_descid, "_", loopedField.BoundDescid));
                    //            if (fRec != null)
                    //            {
                    //                boundValue = eLibTools.GetNum(fRec.Value);
                    //            }
                    //        }

                    //    }

                    //}


                }


            }
        }
    }
}