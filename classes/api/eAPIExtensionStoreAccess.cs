using System;
using System.Collections.Generic;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.eda;
using EudoQuery;
using Newtonsoft.Json;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe d'accès à l'API pour récupérer les informations de HotCom
    /// </summary>
    public class eAPIExtensionStoreAccess
    {
        private eAPI _api = null;
        private eAPIExtensionStoreParam _param = null;
        private Action<string> _apiActionOnFailure = null;

        /// <summary>
        /// Erreurs
        /// </summary>
        public string ApiErrors { get; private set; } = string.Empty;

        /// <summary>
        /// Résultats retournés
        /// </summary>
        public string ApiResults { get; private set; } = string.Empty;


        /// <summary>
        /// valeur de catalogue de prix.offer
        /// </summary>
        public List<APIExtensionStoreCatalogValue> _lstOfferPrice = new List<APIExtensionStoreCatalogValue>();

        /// <summary>
        /// valeur de catalogue de produit.offer
        /// </summary>
        public List<APIExtensionStoreCatalogValue> _lstOffer = new List<APIExtensionStoreCatalogValue>();


        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        public eAPIExtensionStoreAccess(ePref pref)
        {
            _param = new eAPIExtensionStoreParam(pref);
            _api = new eAPI(_param.ApiBaseUrl, _param.ApiDebug);

            _apiActionOnFailure =
                delegate (string response)
                {
                    ApiErrors = string.Concat(ApiErrors, Environment.NewLine, response);
                };

            //_lstOfferPrice = GetProductOfferPrice();
            //_lstOffer = GetProductOffer();
        }

        #region Critères

        public ApiWhereCustom GetSearchWhereCustomDefault()
        {
            // Conditions
            ApiWhereCustom apiWhere = new ApiWhereCustom();
            apiWhere.WhereCustoms = new List<ApiWhereCustom>();

            // Critère sur les produits de type "Modules"
            apiWhere.WhereCustoms.Add(
                new ApiWhereCustom(
                    ((int)ExtEnum.FIELDS_PRODUCT.TYPE).ToString(),
                    Operator.OP_EQUAL,
                    ((int)ExtEnum.DATA_PRODUCT_TYPE.EXTENSION).ToString(),
                    InterOperator.OP_AND));

            // Critère sur les produits dont la case à cocher "Publié sur le store" est cochée
            ApiWhereCustom wc = new ApiWhereCustom() { InterOperator = (int)InterOperator.OP_AND };
            wc.WhereCustoms = new List<ApiWhereCustom>();
            wc.WhereCustoms.Add(
                new ApiWhereCustom(((int)ExtEnum.FIELDS_PRODUCT.AVAILABLE_ON_STORE).ToString(), Operator.OP_IS_TRUE, "1", InterOperator.OP_OR));
            if (eLibTools.GetServerConfig("displaydebugextension") == "1")
            {
                wc.WhereCustoms.Add(
                    new ApiWhereCustom(((int)ExtEnum.FIELDS_PRODUCT.AVAILABLE_DEBUG).ToString(), Operator.OP_IS_TRUE, "1", InterOperator.OP_OR));
            }
            apiWhere.WhereCustoms.Add(wc);

            // Code API Non vide
            apiWhere.WhereCustoms.Add(
                new ApiWhereCustom(((int)ExtEnum.FIELDS_PRODUCT.NATIVE_ID).ToString(), Operator.OP_IS_NOT_EMPTY, "1", InterOperator.OP_AND));

            return apiWhere;
        }

        public ApiWhereCustom GetSearchWhereCustom_Old(string categoryFilter, string searchFilter, int extensionIdFilter)
        {
            // Conditions
            ApiWhereCustom apiProductsWhereCustom = new ApiWhereCustom();
            apiProductsWhereCustom.WhereCustoms = new List<ApiWhereCustom>();

            // Critère sur les produits de type "Modules"
            apiProductsWhereCustom.WhereCustoms.Add(
                new ApiWhereCustom(
                    ((int)ExtEnum.FIELDS_PRODUCT.TYPE).ToString(),
                    Operator.OP_EQUAL,
                    ((int)ExtEnum.DATA_PRODUCT_TYPE.EXTENSION).ToString(),
                    InterOperator.OP_AND));

            // Critère sur les produits dont la case à cocher "Publié sur le store" est cochée
            ApiWhereCustom wc = new ApiWhereCustom();
            wc.WhereCustoms = new List<ApiWhereCustom>();
            wc.InterOperator = (int)InterOperator.OP_AND;
            wc.WhereCustoms.Add(new ApiWhereCustom(((int)ExtEnum.FIELDS_PRODUCT.AVAILABLE_ON_STORE).ToString(), Operator.OP_IS_TRUE, "1", InterOperator.OP_OR));
            if (eLibTools.GetServerConfig("displaydebugextension") == "1")
                wc.WhereCustoms.Add(new ApiWhereCustom(((int)ExtEnum.FIELDS_PRODUCT.AVAILABLE_DEBUG).ToString(), Operator.OP_IS_TRUE, "1", InterOperator.OP_OR));

            apiProductsWhereCustom.WhereCustoms.Add(wc);

            // Code API Non vide
            apiProductsWhereCustom.WhereCustoms.Add(
                new ApiWhereCustom(((int)ExtEnum.FIELDS_PRODUCT.NATIVE_ID).ToString(), Operator.OP_IS_NOT_EMPTY, "1", InterOperator.OP_AND));

            // Critère sur le FileId
            if (extensionIdFilter > 0)
                apiProductsWhereCustom.WhereCustoms.Add(new ApiWhereCustom("EVTID", Operator.OP_EQUAL, extensionIdFilter.ToString(), InterOperator.OP_AND));

            bool categoryFilterEnabled = categoryFilter != null && categoryFilter.Trim().Length > 0;
            bool searchFilterEnabled = searchFilter != null && searchFilter.Trim().Length > 0;

            // Ajout d'un critère de filtre sur les catégories si défini, et différent de <TOUS>
            if (categoryFilterEnabled)
            {
                if (categoryFilter != "-1")
                    apiProductsWhereCustom.WhereCustoms.Add(new ApiWhereCustom(((int)ExtEnum.FIELDS_PRODUCT.CATEGORIES).ToString(), Operator.OP_CONTAIN, categoryFilter, InterOperator.OP_AND));
                else
                    apiProductsWhereCustom.WhereCustoms.Add(new ApiWhereCustom(((int)ExtEnum.FIELDS_PRODUCT.CATEGORIES).ToString(), Operator.OP_IS_EMPTY, "", InterOperator.OP_AND));
            }

            // Ajout d'un critère sur les champs texte pour la recherche, si définie
            if (searchFilterEnabled)
            {
                // Ajout des sous-critères sur chaque champ texte, en OR
                ApiWhereCustom apiProductsSearchWhereCustom = new ApiWhereCustom();
                // Il est nécessaire de repréciser l'InterOperator qui est à NONE par défaut, afin que notre série de critères s'additionne aux critères de base
                // sur le type de produits (WhereCustom ci-dessus)
                apiProductsSearchWhereCustom.InterOperator = InterOperator.OP_AND.GetHashCode();
                apiProductsSearchWhereCustom.WhereCustoms = new List<ApiWhereCustom>();
                apiProductsSearchWhereCustom.WhereCustoms.Add(
                    new ApiWhereCustom(((int)ExtEnum.FIELDS_PRODUCT.TITLE).ToString(), Operator.OP_CONTAIN, searchFilter, InterOperator.OP_OR));
                apiProductsSearchWhereCustom.WhereCustoms.Add(
                    new ApiWhereCustom(((int)ExtEnum.FIELDS_PRODUCT.AUTHOR).ToString(), Operator.OP_CONTAIN, searchFilter, InterOperator.OP_OR));
                apiProductsSearchWhereCustom.WhereCustoms.Add(
                    new ApiWhereCustom(((int)ExtEnum.FIELDS_DESCRIPION.SUMMARY).ToString(), Operator.OP_CONTAIN, searchFilter, InterOperator.OP_OR));
                apiProductsSearchWhereCustom.WhereCustoms.Add(
                    new ApiWhereCustom(((int)ExtEnum.FIELDS_DESCRIPION.DESCRIPTION).ToString(), Operator.OP_CONTAIN, searchFilter, InterOperator.OP_OR));
                // Ajout des sous-critères au WhereCustom global
                apiProductsWhereCustom.WhereCustoms.Add(apiProductsSearchWhereCustom);
            }

            return apiProductsWhereCustom;
        }

        #endregion

        #region Méthodes de recupe des données

        private void GetAuthenticateToken()
        {
            if (!string.IsNullOrEmpty(_api.Token))
                return;

            Action<ApiResponseAuthenticateToken> apiActionSuccess =
                delegate (ApiResponseAuthenticateToken response)
                {

                };

            // Connexion et récupération du token
            _api.AuthenticateToken(
                _param.ApiSubscriberLogin,
                _param.ApiSubscriberPassword,
                _param.ApiBaseName,
                _param.ApiUserLogin,
                _param.ApiUserPassword,
                _param.ApiUserLang,
                _param.ApiProductName,
                GlobalSuccess(apiActionSuccess),
                _apiActionOnFailure);
        }

        public DateTime GetRemoteServerDate()
        {
            DateTime date = new DateTime();

            Action<ApiResponseServerDateTime> apiActionSuccess =
                delegate (ApiResponseServerDateTime response)
                {
                    if (response.ResultInfos.Success)
                        date = response.ResultData.ServerDateTime;
                };

            _api.GetServerDateTime(GlobalSuccess(apiActionSuccess), _apiActionOnFailure);

            return date;
        }

        public List<eAdminExtension> GetExtensionList(int currentPage, int extensionsPerPage, StoreListCriteres criteres, out int totalExtensionsCount, out int totalExtensionsPages)
        {
            List<eAdminExtension> result = new List<eAdminExtension>();

            // Uniquement pour la page 1 pour obtenir le nombre d'enregistrement total sur la recherche
            bool showMetaData = currentPage == 1;

            ApiWhereCustom apiWhere = GetSearchWhereCustomDefault();
            criteres.AddWhereCustom(apiWhere, _param.Pref);

            List<ExtensionGlobalInfo> list = GetExtensionSearch(currentPage, extensionsPerPage, showMetaData, apiWhere, out totalExtensionsCount, out totalExtensionsPages);

            foreach (var o in list)
            {
                eAdminExtension newExtensionFromStore = GetAdminExtension(o);

                if (newExtensionFromStore == null)
                    continue;
                try
                {
                    o.DescriptionInfos = GetProductDescription(o.ExtensionFileId);
                }
                catch
                {
                    // Le chargement des descriptions ne doit pas bloquer l'affichage de l'extension
                }

                // TODO - Pkoi on a besoin de Screenshots sur le setinfo ? A deplacer
                newExtensionFromStore.SetInfos(_param.Pref, o, null);
                result.Add(newExtensionFromStore);
            }

            return result;
        }

        //SHA
        /// <summary>
        /// Retourne la liste des docs d'une extension de l'onglet "Produit Description"(sur Hotcom)
        /// </summary>
        /// <param name="productDescriptionId"></param>
        /// <param name="totalDocsCount"></param>
        /// <returns></returns>
        public List<ProductDescriptionDoc> GetProductDescDocsList(int productDescriptionId, out int totalDocsCount)
        {
            List<ProductDescriptionDoc> productDescDocs = new List<ProductDescriptionDoc>();
            int localTotalDocsCount = 0;
            int numPage = 1;
            int rowsPerPage = 4;
            bool showMetaData = true;

            List<int> listCol = new List<int>()
            {
                (int)ExtEnum.FIELDS_DOC_PRODUCT_DESCRIPTION.TAB,
                (int)ExtEnum.FIELDS_DOC_PRODUCT_DESCRIPTION.TITLE,
                (int)ExtEnum.FIELDS_DOC_PRODUCT_DESCRIPTION.PRODUCT_DESCRIPTION,
                (int)ExtEnum.FIELDS_DOC_PRODUCT_DESCRIPTION.MANUAL_TUTO
            };

            List<ApiOrderBy> orderBy = new List<ApiOrderBy>() {
                new ApiOrderBy() { DescId = (int)ExtEnum.FIELDS_DOC_PRODUCT_DESCRIPTION.TITLE, Order = 0 }
            };

            ApiWhereCustom apiWhere = new ApiWhereCustom("2511", Operator.OP_EQUAL, productDescriptionId.ToString(), InterOperator.OP_AND);
            ApiWhereCustom apiWhereHasPJ = new ApiWhereCustom("2591", Operator.OP_GREATER_OR_EQUAL, "1", InterOperator.OP_AND);

            List<ApiWhereCustom> lst = new List<ApiWhereCustom>();

            lst.Add(apiWhere);
            lst.Add(apiWhereHasPJ);

            ApiWhereCustom apiWhereList = new ApiWhereCustom() { WhereCustoms = lst };


            Action<ApiResponseSearch> apiActionSuccess =
                delegate (ApiResponseSearch response)
                {
                    if (response.ResultData.Rows.Count <= 0)
                        return;

                    foreach (ApiContainerRecordsRow row in response.ResultData.Rows)
                    {
                        ProductDescriptionDoc productDescDoc = new ProductDescriptionDoc();
                        if (row.Fields == null)
                            continue;
                        productDescDoc.DocFileId = row.FileId;

                        foreach (ApiContainerRecordsField fld in row.Fields)
                        {
                            switch (fld.DescId)
                            {
                                case (int)ExtEnum.FIELDS_DOC_PRODUCT_DESCRIPTION.TITLE: productDescDoc.DocTitle = fld.Value; break;
                                case (int)ExtEnum.FIELDS_DOC_PRODUCT_DESCRIPTION.PRODUCT_DESCRIPTION: productDescDoc.ProductDescription = fld.Value; break;
                                case (int)ExtEnum.FIELDS_DOC_PRODUCT_DESCRIPTION.MANUAL_TUTO: productDescDoc.ManualTuto = Convert.ToInt32(fld.DbValue); break;
                            }
                        }

                        productDescDocs.Add(productDescDoc);

                    }
                    localTotalDocsCount = response.ResultMetaData.TotalRows;
                };

            GetAuthenticateToken();

            _api.Search((int)ExtEnum.FIELDS_DOC_PRODUCT_DESCRIPTION.TAB, showMetaData, listCol, numPage, rowsPerPage, apiWhereList, orderBy, GlobalSuccess(apiActionSuccess), _apiActionOnFailure);

            totalDocsCount = localTotalDocsCount;

            return productDescDocs;
        }

        //SHA
        /// <summary>
        /// Retourne la liste des annexes d'une doc de l'onglet "Produit Description"
        /// </summary>
        /// <param name="fileIds"></param>
        /// <param name="totalPJsCount"></param>
        /// <returns></returns>
        public List<Tuple<string, string, int>> GetPJsDico(List<int> fileIds, out int totalPJsCount)
        {
            List<Tuple<string, string, int>> pjsDico = new List<Tuple<string, string, int>>();
            int localTotalPJsCount = 0;
            int numPage = 1;
            int rowsPerPage = 4;
            bool showMetaData = true;

            List<int> listCol = new List<int>()
            {
                (int)PJField.LIBELLE,
                //(int)PJField.DESCRIPTION,
                //(int)PJField.FILE,
                (int)PJField.FILEID
            };

            List<ApiOrderBy> orderBy = new List<ApiOrderBy>() {
                new ApiOrderBy() { DescId = (int)PJField.PJ_DATE, Order = 1 }
            };

            ApiWhereCustom apiWhereFile = new ApiWhereCustom("102011", Operator.OP_EQUAL, "EVENT_15", InterOperator.OP_AND); //Doc.
            ApiWhereCustom apiWhereFileId = new ApiWhereCustom("102012", Operator.OP_IN_LIST, eLibTools.Join(";", fileIds), InterOperator.OP_AND); //Annexe

            List<ApiWhereCustom> lst = new List<ApiWhereCustom>();
            lst.Add(apiWhereFile);
            lst.Add(apiWhereFileId);

            ApiWhereCustom apiWhereList = new ApiWhereCustom() { WhereCustoms = lst };

            Action<ApiResponseSearch> apiActionSuccess =
                delegate (ApiResponseSearch response)
                {
                    Tuple<string, string, int> pjs;
                    if (response.ResultData.Rows.Count <= 0)
                        return;

                    foreach (ApiContainerRecordsRow row in response.ResultData.Rows)
                    {
                        string pjLibelle = "", pjSecuredlink = "";
                        int pjFileId = 0;

                        if (row.Fields == null)
                            continue;

                        //Récupérer le libellé et l'annexe du pj
                        if (!string.IsNullOrEmpty(row.Fields[0].DbValue) && !string.IsNullOrEmpty((row.Fields[0].Value)))
                        {
                            pjLibelle = row.Fields[0].DbValue;
                            pjSecuredlink = row.Fields[0].Value;
                        }

                        //récupérer le fileID du pj
                        foreach (var fld in row.Fields)
                        {
                            if (!string.IsNullOrEmpty(fld.DbValue) && !string.IsNullOrEmpty(fld.Value) && eLibTools.IsDigitsOnly(fld.Value))
                                pjFileId = Convert.ToInt32(fld.Value);
                        }

                        pjs = new Tuple<string, string, int>(pjLibelle, pjSecuredlink, pjFileId);

                        pjsDico.Add(pjs);
                    }

                    localTotalPJsCount = response.ResultMetaData.TotalRows;
                };

            GetAuthenticateToken();

            _api.Search(102000, showMetaData, listCol, numPage, rowsPerPage, apiWhereList, orderBy, GlobalSuccess(apiActionSuccess), _apiActionOnFailure);

            totalPJsCount = localTotalPJsCount;
            return pjsDico;
        }





        /// <summary>
        /// retourne un object eAdminExtension avec toutes les informations additionnelles (capture d'écran, description....)
        /// Cette méthode lance plusieurs appel a l'API, si toutes les informations ne sont pas utiles, utiliser la version
        /// avec extensionQueryParam
        /// </summary>
        /// <param name="extensionId"></param>
        /// <returns></returns>
        public eAdminExtension GetExtensionFile(int extensionId)
        {
            return GetExtensionFile(extensionId, new extensionQueryParam()
            {
                ScreenShots = true,
                VersionInfos = true,
                DescriptionInfos = true,
                PriceInfos = true,
            });
        }


        /// <summary>
        /// retourne l'extension fourni en paramètre
        /// Ne lance que les appel appi demandée dans <paramref name="paramsQuery"/>
        /// </summary>
        /// <param name="extensionId"></param>
        /// <param name="paramsQuery">Paramètre optionnels a charger</param>
        /// <returns></returns>
        public eAdminExtension GetExtensionFile(int extensionId, extensionQueryParam paramsQuery)
        {
            int totalExtensionsCount;
            int totalExtensionsPages;

            int numPage = 1;
            int rowsPerPage = 1;
            bool showMetaData = false;

            ApiWhereCustom apiWhere = GetSearchWhereCustomDefault();
            apiWhere.WhereCustoms.Add(new ApiWhereCustom("EVTID", Operator.OP_EQUAL, extensionId.ToString(), InterOperator.OP_AND));

            List<ExtensionGlobalInfo> list = GetExtensionSearch(numPage, rowsPerPage, showMetaData, apiWhere, out totalExtensionsCount, out totalExtensionsPages);

            if (list == null || list.Count <= 0)
                return null;

            ExtensionGlobalInfo oExtensionGlobalInfo = list[0];

            List<eAPIProductScreenshot> Screenshots = new List<eAPIProductScreenshot>();

            try
            {
                // TODO - Revoir si on ne peut pas recup le tout en une requete !
                Screenshots = paramsQuery.ScreenShots ? GetScreenshots(oExtensionGlobalInfo.ExtensionFileId) : new List<eAPIProductScreenshot>();
                oExtensionGlobalInfo.VersionInfos = paramsQuery.VersionInfos ? GetVersionsInfos(oExtensionGlobalInfo.ExtensionFileId) : new ExtensionVersionInfo();
                oExtensionGlobalInfo.DescriptionInfos = paramsQuery.DescriptionInfos ? GetProductDescription(oExtensionGlobalInfo.ExtensionFileId) : new ExtensionProductDescription();
                oExtensionGlobalInfo.PriceInfos = paramsQuery.PriceInfos ? GetProductExtensionPrice(oExtensionGlobalInfo.ExtensionFileId) : new ExtensionPrice();
            }
            catch { }

            eAdminExtension newExtensionFromStore = GetAdminExtension(oExtensionGlobalInfo);

            if (newExtensionFromStore != null)
            {
                newExtensionFromStore.SetInfos(
                    _param.Pref,
                    oExtensionGlobalInfo,
                    Screenshots
                );
            }

            return newExtensionFromStore;
        }

        //SHA private to public
        public eAdminExtension GetAdminExtension(ExtensionGlobalInfo oExtensionGlobalInfo)
        {
            eUserOptionsModules.USROPT_MODULE? module = null;

            // Puis, en fonction de l'ID interne de l'extension déclaré sur HotCom, on instancie l'objet Extension, soit à partir d'une classe interne
            // correspondant à l'ID, soit à partir de l'objet eAdminExtensionFromStore "à remplir soi-même"
            switch (oExtensionGlobalInfo.ExtensionNativeId)
            {
                case "MOBILE": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_MOBILE; break;
                case "OUTLOOKADDIN": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN; break;
                case "LINKEDINCONTACT": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_LINKEDIN; break;
                case "SYNCHRO": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO; break;
                case "SYNCHROEXCHANGE": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_OFFICE365; break;
                case "SMS": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SMS; break;
                case "CTI": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_CTI; break;
                case "API": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_API; break;
                case "EXTERNALMAILING": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING; break;
                case "VCARD": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_VCARD; break;
                case "SNAPSHOT": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SNAPSHOT; break;
                case "EMAILING": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING; break;
                case "GRID": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_GRID; break;
                case "NOTIFICATIONS": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS; break;
                case "CARTO": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_CARTO; break;
                case "SIRENE": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE; break;
                case "POWERBI": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI; break;
                case "COMPTA_BUSINESSSOFT": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT; break;
                case "COMPTA_CEGID": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_CEGID; break;
                case "COMPTA_SAGE": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SAGE; break;
                case "COMPTA_EBP": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_EBP; break;
                case "COMPTA_SIGMA": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SIGMA; break;
                case "UBIFLOW_IN": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_IN_UBIFLOW; break;
                case "HBS_IN": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_IN_HBS; break;
                case "SIGN_ELEC_DOCUSIGN": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_DOCUSIGN; break;
                case "SMS_NETMESSAGE": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SMS_NETMESSAGE; break;
                case "SYNCHROEXCHANGE2016_ONPREMISE": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_DOCUSIGN; break;
                case "ZAPIER": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ZAPIER; break;
                case "EXTRANET": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EXTRANET; break;
                //SHA : tâche #1 873
                case "ADVANCED_FORM": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ADVANCED_FORM; break;
                case "DEDICATEDIP": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_DEDICATED_IP; break;
                case "VERIFY": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_MAIL_STATUS_VERIFICATION; break;
                case "WORLDLINECORE": module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_WORLDLINE_PAYMENT; break;
                default:
                    module = eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_FROMSTORE;
                    oExtensionGlobalInfo.IsNativeExtension = false;
                    break;
            }

            if (module != null)
                return eAdminExtension.InitFromModule(module.Value, _param.Pref);

            return null;
        }

        /// <summary>
        /// Renvoie la liste complète des extensions référencées sur le Store, via l'API, correspondant aux paramètres indiqués
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="extensionsPerPage"></param>
        /// <param name="showMetaData"></param>
        /// <param name="apiWhere"></param>
        /// <param name="totalExtensionsCount"></param>
        /// <param name="totalExtensionsPages"></param>
        /// <returns></returns>
        private List<ExtensionGlobalInfo> GetExtensionSearch(int currentPage, int extensionsPerPage, bool showMetaData, ApiWhereCustom apiWhere,
            out int totalExtensionsCount, out int totalExtensionsPages)
        {
            int localTotalExtensionsCount = 0;
            int localTotalExtensionsPages = 0;
            List<ExtensionGlobalInfo> extensionList = new List<ExtensionGlobalInfo>();

            List<int> listCol = new List<int>() {
                (int)ExtEnum.FIELDS_PRODUCT.TITLE,
                (int)ExtEnum.FIELDS_PRODUCT.LOGO,
                (int)ExtEnum.FIELDS_PRODUCT.CATEGORIES,
                (int)ExtEnum.FIELDS_PRODUCT.AUTHOR,
                (int)ExtEnum.FIELDS_PRODUCT.AUTHOR_URL,
                (int)ExtEnum.FIELDS_PRODUCT.INSTALL_COUNT,
                (int)ExtEnum.FIELDS_PRODUCT.NATIVE_ID,
                (int)ExtEnum.FIELDS_PRODUCT.OFFERS,
                (int)ExtEnum.FIELDS_PRODUCT.ISNEW,
               (int)ExtEnum.FIELDS_PRODUCT.HASCUSTOMPARAM,
            };

            List<ApiOrderBy> orderBy = new List<ApiOrderBy>() { new ApiOrderBy() { DescId = (int)ExtEnum.FIELDS_PRODUCT.TITLE, Order = 0 } };

            Action<ApiResponseSearch> apiActionOnSearchListSuccess =
                delegate (ApiResponseSearch response)
                {
                    ExtensionGlobalInfo oExtensionGlobalInfo;
                    IDictionary<string, string> categories;

                    foreach (var rowExtension in response.ResultData.Rows)
                    {
                        oExtensionGlobalInfo = new ExtensionGlobalInfo();
                        categories = new Dictionary<string, string>();

                        extensionList.Add(oExtensionGlobalInfo);
                        oExtensionGlobalInfo.ExtensionFileId = rowExtension.FileId;

                        #region Charge les valeurs des rubriques

                        foreach (ApiContainerRecordsField field in rowExtension.Fields)
                        {
                            switch (field.DescId)
                            {
                                case (int)ExtEnum.FIELDS_PRODUCT.TITLE:
                                    if (!string.IsNullOrEmpty(field.Value)) oExtensionGlobalInfo.Title = field.Value.Trim(); break;

                                case (int)ExtEnum.FIELDS_PRODUCT.LOGO:
                                    if (!string.IsNullOrEmpty(field.Value)) oExtensionGlobalInfo.Icon = field.Value.Trim(); break;

                                case (int)ExtEnum.FIELDS_PRODUCT.CATEGORIES:
                                    if (!string.IsNullOrEmpty(field.DbValue) && !string.IsNullOrEmpty(field.Value))
                                    {
                                        string[] categoriesIds = field.DbValue.Split(';');
                                        string[] categoriesLabels = field.Value.Split(';');
                                        if (categoriesIds.Length == categoriesLabels.Length)
                                        {
                                            for (int i = 0; i < categoriesIds.Length; i++)
                                                categories.Add(categoriesIds[i], categoriesLabels[i]);
                                        }
                                        oExtensionGlobalInfo.Categories = categories;
                                    }
                                    break;

                                case (int)ExtEnum.FIELDS_PRODUCT.AUTHOR:
                                    if (!string.IsNullOrEmpty(field.Value)) oExtensionGlobalInfo.Author = field.Value.Trim(); break;

                                case (int)ExtEnum.FIELDS_PRODUCT.AUTHOR_URL:
                                    if (!string.IsNullOrEmpty(field.Value)) oExtensionGlobalInfo.AuthorUrl = field.Value.Trim(); break;

                                case (int)ExtEnum.FIELDS_PRODUCT.INSTALL_COUNT:
                                    oExtensionGlobalInfo.InstallCount = eLibTools.GetNum(field.Value.Trim()); break;

                                case (int)ExtEnum.FIELDS_PRODUCT.NATIVE_ID:
                                    if (!string.IsNullOrEmpty(field.Value)) oExtensionGlobalInfo.ExtensionNativeId = field.Value.Trim(); break;
                                case (int)ExtEnum.FIELDS_PRODUCT.ISNEW:
                                    if (!string.IsNullOrEmpty(field.Value)) oExtensionGlobalInfo.IsNew = field.Value == "1"; break;

                                case (int)ExtEnum.FIELDS_PRODUCT.HASCUSTOMPARAM:
                                    if (!string.IsNullOrEmpty(field.Value)) oExtensionGlobalInfo.HasCustomParam = field.Value == "1"; break;

                                case (int)ExtEnum.FIELDS_PRODUCT.OFFERS:
                                    oExtensionGlobalInfo.Offers = field.Value;
                                    oExtensionGlobalInfo.IdOffers = field.DbValue;
                                    break;
                            }
                        }

                        #endregion
                    }

                    localTotalExtensionsCount = response.ResultMetaData.TotalRows;
                    localTotalExtensionsPages = response.ResultMetaData.TotalPages;
                };

            GetAuthenticateToken();

            _api.Search((int)ExtEnum.FIELDS_PRODUCT.TAB, showMetaData, listCol, currentPage, extensionsPerPage, apiWhere, orderBy, GlobalSuccess(apiActionOnSearchListSuccess), _apiActionOnFailure);

            totalExtensionsCount = localTotalExtensionsCount;
            totalExtensionsPages = localTotalExtensionsPages;
            return extensionList;
        }

        [Obsolete]
        public List<eAdminExtension> GetExtensionList_Old(int currentPage, int extensionsPerPage, string categoryFilter, string searchFilter, out int totalExtensionsCount)
        {
            return GetExtensionSearch_Old(currentPage, extensionsPerPage, categoryFilter, searchFilter, 0, out totalExtensionsCount);
        }

        [Obsolete]
        public eAdminExtension GetExtensionFile_Old(int extensionId)
        {
            int totalExtensionsCount = 0;
            List<eAdminExtension> extensionList = GetExtensionSearch_Old(1, 50, String.Empty, String.Empty, extensionId, out totalExtensionsCount);

            if (extensionList != null && extensionList.Count > 0)
                return extensionList[0];
            else
                return null;
        }

        /// <summary>
        /// Renvoie la liste complète des extensions référencées sur le Store, via l'API, correspondant aux paramètres indiqués
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="extensionsPerPage"></param>
        /// <param name="categoryFilter"></param>
        /// <param name="searchFilter"></param>
        /// <param name="totalExtensionsCount"></param>
        /// <returns></returns>
        [Obsolete]
        private List<eAdminExtension> GetExtensionSearch_Old(int currentPage, int extensionsPerPage, string categoryFilter, string searchFilter, int extensionIdFilter, out int totalExtensionsCount)
        {
            int localTotalExtensionsCount = 0;
            List<eAdminExtension> extensionList = new List<eAdminExtension>();

            Action<ApiResponseSearch> apiActionOnSearchListSuccess =
            delegate (ApiResponseSearch response)
            {
                ApiResults = string.Concat(JsonConvert.SerializeObject(response));

                foreach (var rowExtension in response.ResultData.Rows)
                {
                    #region Récupération des champs depuis le retour API

                    ApiContainerRecordsField titleField = null;
                    ApiContainerRecordsField logoField = null;
                    IDictionary<string, string> categories = new Dictionary<string, string>();
                    ApiContainerRecordsField categoriesField = null;
                    List<eAPIProductScreenshot> screenshots = new List<eAPIProductScreenshot>();
                    ApiContainerRecordsField authorField = null;
                    ApiContainerRecordsField authorUrlField = null;
                    ApiContainerRecordsField installCountField = null;
                    ApiContainerRecordsField offersField = null;
                    ApiContainerRecordsField extensionNativeIdField = null;

                    foreach (ApiContainerRecordsField field in rowExtension.Fields)
                    {
                        if (field.DescId == (int)ExtEnum.FIELDS_PRODUCT.TITLE) { titleField = field; }
                        else if (field.DescId == (int)ExtEnum.FIELDS_PRODUCT.LOGO) { logoField = field; }
                        else if (field.DescId == (int)ExtEnum.FIELDS_PRODUCT.CATEGORIES) { categoriesField = field; }

                        else if (field.DescId == (int)ExtEnum.FIELDS_PRODUCT.AUTHOR) { authorField = field; }
                        else if (field.DescId == (int)ExtEnum.FIELDS_PRODUCT.AUTHOR_URL) { authorUrlField = field; }

                        else if (field.DescId == (int)ExtEnum.FIELDS_PRODUCT.INSTALL_COUNT) { installCountField = field; }
                        else if (field.DescId == (int)ExtEnum.FIELDS_PRODUCT.NATIVE_ID) { extensionNativeIdField = field; }

                        else if (field.DescId == (int)ExtEnum.FIELDS_PRODUCT.OFFERS) { offersField = field; }
                    }

                    List<eAPIProductScreenshot> Screenshots = new List<eAPIProductScreenshot>();
                    ExtensionGlobalInfo oExtensionGlobalInfo = new ExtensionGlobalInfo();

                    try
                    {
                        if (!string.IsNullOrEmpty(titleField.Value))
                            oExtensionGlobalInfo.Title = titleField.Value.Trim();

                        if (!string.IsNullOrEmpty(logoField.Value))
                            oExtensionGlobalInfo.Icon = logoField.Value.Trim();

                        if (!string.IsNullOrEmpty(categoriesField.DbValue) && !string.IsNullOrEmpty(categoriesField.Value))
                        {
                            categories = new Dictionary<string, string>();
                            string[] categoriesIds = categoriesField.DbValue.Split(';');
                            string[] categoriesLabels = categoriesField.Value.Split(';');
                            if (categoriesIds.Length == categoriesLabels.Length)
                            {
                                for (int i = 0; i < categoriesIds.Length; i++)
                                    categories.Add(categoriesIds[i], categoriesLabels[i]);
                            }

                            oExtensionGlobalInfo.Categories = categories;
                        }

                        if (!string.IsNullOrEmpty(authorField.Value))
                            oExtensionGlobalInfo.Author = authorField.Value.Trim();

                        if (!string.IsNullOrEmpty(authorUrlField.Value))
                            oExtensionGlobalInfo.AuthorUrl = authorUrlField.Value.Trim();

                        int.TryParse(installCountField.Value, out oExtensionGlobalInfo.InstallCount);

                        if (!string.IsNullOrEmpty(extensionNativeIdField.Value))
                            oExtensionGlobalInfo.ExtensionNativeId = extensionNativeIdField.Value.Trim();


                        oExtensionGlobalInfo.ExtensionFileId = rowExtension.FileId;

                        // TODO - Revoir si on ne peut pas recup le tout en une requete !
                        Screenshots = GetScreenshots_Old(oExtensionGlobalInfo.ExtensionFileId);

                        oExtensionGlobalInfo.VersionInfos = GetVersionsInfos(oExtensionGlobalInfo.ExtensionFileId);
                        oExtensionGlobalInfo.DescriptionInfos = GetProductDescription_Old(oExtensionGlobalInfo.ExtensionFileId);
                        oExtensionGlobalInfo.PriceInfos = GetProductExtensionPrice(oExtensionGlobalInfo.ExtensionFileId);

                        oExtensionGlobalInfo.Offers = offersField.Value;
                        oExtensionGlobalInfo.IdOffers = offersField.DbValue;
                    }
                    catch { }

                    #endregion

                    #region initialisation

                    // Puis, en fonction de l'ID interne de l'extension déclaré sur HotCom, on instancie l'objet Extension, soit à partir d'une classe interne
                    // correspondant à l'ID, soit à partir de l'objet eAdminExtensionFromStore "à remplir soi-même"
                    eAdminExtension newExtensionFromStore = null;
                    switch (oExtensionGlobalInfo.ExtensionNativeId)
                    {
                        case "MOBILE":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_MOBILE, _param.Pref);
                            break;
                        case "OUTLOOKADDIN":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN, _param.Pref);
                            break;
                        case "LINKEDINCONTACT":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_LINKEDIN, _param.Pref);
                            break;
                        case "SYNCHRO":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO, _param.Pref);
                            break;
                        case "SYNCHROEXCHANGE":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_OFFICE365, _param.Pref);
                            break;
                        case "SMS":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SMS, _param.Pref);
                            break;
                        case "CTI":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_CTI, _param.Pref);
                            break;
                        case "API":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_API, _param.Pref);
                            break;
                        case "EXTERNALMAILING":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING, _param.Pref);
                            break;
                        case "VCARD":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_VCARD, _param.Pref);
                            break;
                        case "SNAPSHOT":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SNAPSHOT, _param.Pref);
                            break;
                        case "EMAILING":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING, _param.Pref);
                            break;
                        case "GRID":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_GRID, _param.Pref);
                            break;
                        case "NOTIFICATIONS":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS, _param.Pref);
                            break;
                        case "CARTO":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_CARTO, _param.Pref);
                            break;
                        case "SIRENE":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE, _param.Pref);
                            break;
                        case "POWERBI":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI, _param.Pref);
                            break;
                        case "COMPTA_BUSINESSSOFT":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT, _param.Pref);
                            break;
                        case "COMPTA_CEGID":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_CEGID, _param.Pref);
                            break;
                        case "COMPTA_SAGE":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SAGE, _param.Pref);
                            break;
                        case "COMPTA_EBP":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_EBP, _param.Pref);
                            break;
                        case "COMPTA_SIGMA":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SIGMA, _param.Pref);
                            break;
                        case "UBIFLOW_IN":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_IN_UBIFLOW, _param.Pref);
                            break;
                        case "HBS_IN":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_IN_HBS, _param.Pref);
                            break;
                        case "SIGN_ELEC_DOCUSIGN":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_DOCUSIGN, _param.Pref);
                            break;
                        case "SMS_NETMESSAGE":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SMS_NETMESSAGE, _param.Pref);
                            break;
                        case "SYNCHROEXCHANGE2016_ONPREMISE":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE, _param.Pref);
                            break;
                        case "EXTRANET":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EXTRANET, _param.Pref);
                            break;
                        //SHA : tâche #1 873
                        case "ADVANCED_FORM":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ADVANCED_FORM, _param.Pref);
                            break;
                        case "DEDICATEDIP":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_DEDICATED_IP, _param.Pref);
                            break;
                        case "WORLDLINECORE":
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_WORLDLINE_PAYMENT, _param.Pref);
                            break;
                        default:
                            newExtensionFromStore = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_FROMSTORE, _param.Pref);
                            oExtensionGlobalInfo.IsNativeExtension = false;
                            break;
                    }

                    if (newExtensionFromStore != null)
                    {
                        newExtensionFromStore.SetInfos(
                            _param.Pref,
                            oExtensionGlobalInfo,
                            Screenshots
                        );


                        extensionList.Add(newExtensionFromStore);
                    }
                    #endregion
                }

                localTotalExtensionsCount = response.ResultMetaData.TotalRows;
            };

            GetAuthenticateToken();

            // Récupération de la liste des extensions disponibles
            List<ApiOrderBy> apiProductsOrderByList = new List<ApiOrderBy>() { new ApiOrderBy() { DescId = (int)ExtEnum.FIELDS_PRODUCT.TITLE, Order = 0 } }; ;
            ApiWhereCustom apiProductsWhereCustomList = GetSearchWhereCustom_Old(categoryFilter, searchFilter, extensionIdFilter);

            List<int> apiProductsListCols = new List<int>()
            {
                (int)ExtEnum.FIELDS_PRODUCT.TITLE,
                (int)ExtEnum.FIELDS_PRODUCT.LOGO,
                (int)ExtEnum.FIELDS_PRODUCT.CATEGORIES,

                (int)ExtEnum.FIELDS_PRODUCT.AUTHOR,
                (int)ExtEnum.FIELDS_PRODUCT.AUTHOR_URL,

                (int)ExtEnum.FIELDS_PRODUCT.INSTALL_COUNT,
                (int)ExtEnum.FIELDS_PRODUCT.NATIVE_ID,
                (int)ExtEnum.FIELDS_PRODUCT.OFFERS,

                 (int)ExtEnum.FIELDS_PRODUCT.ISNEW,
                 (int)ExtEnum.FIELDS_PRODUCT.HASCUSTOMPARAM,
            };

            _api.Search((int)ExtEnum.FIELDS_PRODUCT.TAB, true, apiProductsListCols, currentPage, extensionsPerPage, apiProductsWhereCustomList, apiProductsOrderByList, apiActionOnSearchListSuccess, _apiActionOnFailure);

            totalExtensionsCount = localTotalExtensionsCount;

            return extensionList;
        }

        /// <summary>
        /// Retourne la liste des visuels correspondant au produit
        /// </summary>
        /// <param name="extensionId">ID de la fiche Produit</param>
        /// <returns></returns>
        private List<eAPIProductScreenshot> GetScreenshots(int extensionId)
        {
            List<eAPIProductScreenshot> imgList = new List<eAPIProductScreenshot>();

            int numPage = 1;
            int rowsPerPage = 0;
            bool showMetaData = false;

            List<int> listCol = new List<int>() {
                (int)ExtEnum.FIELDS_VISU.IMAGE,
                (int)ExtEnum.FIELDS_VISU.LABEL,
                (int)ExtEnum.FIELDS_VISU.ORDER,
                (int)ExtEnum.FIELDS_VISU.VIDEOLINK
            };

            List<ApiOrderBy> orderBy = new List<ApiOrderBy>() { new ApiOrderBy() { DescId = (int)ExtEnum.FIELDS_VISU.ORDER, Order = 0 } };

            int userIdCatLang = (int)ExtEnum.DATA_VISU_LANG.LANG_00;
            switch (_param.Pref.User.UserLangId)
            {
                case 0: userIdCatLang = (int)ExtEnum.DATA_VISU_LANG.LANG_00; break;
                case 1: userIdCatLang = (int)ExtEnum.DATA_VISU_LANG.LANG_01; break;
                case 2: userIdCatLang = (int)ExtEnum.DATA_VISU_LANG.LANG_02; break;
                case 3: userIdCatLang = (int)ExtEnum.DATA_VISU_LANG.LANG_03; break;
                case 4: userIdCatLang = (int)ExtEnum.DATA_VISU_LANG.LANG_04; break;
                case 5: userIdCatLang = (int)ExtEnum.DATA_VISU_LANG.LANG_05; break;
            }

            ApiWhereCustom apiWhere = new ApiWhereCustom()
            {
                WhereCustoms = new List<ApiWhereCustom>()
                {
                    new ApiWhereCustom("PARENTEVTID", Operator.OP_EQUAL, extensionId.ToString(), InterOperator.OP_AND),
                    new ApiWhereCustom(((int)ExtEnum.FIELDS_VISU.LANG).ToString(), Operator.OP_EQUAL, userIdCatLang.ToString(), InterOperator.OP_AND)
                }
            };

            Action<ApiResponseSearch> apiActionSuccess =
                delegate (ApiResponseSearch response)
                {
                    string imgURL, imgLabel, sVideoUrl;
                    int imgOrder;

                    foreach (ApiContainerRecordsRow row in response.ResultData.Rows)
                    {
                        imgURL = string.Empty;
                        imgLabel = string.Empty;
                        sVideoUrl = string.Empty;
                        imgOrder = 0;

                        foreach (ApiContainerRecordsField field in row.Fields)
                        {
                            switch (field.DescId)
                            {
                                case (int)ExtEnum.FIELDS_VISU.IMAGE: imgURL = field.Value; break;
                                case (int)ExtEnum.FIELDS_VISU.LABEL: imgLabel = field.Value; break;
                                case (int)ExtEnum.FIELDS_VISU.ORDER: imgOrder = eLibTools.GetNum(field.Value); break;
                                case (int)ExtEnum.FIELDS_VISU.VIDEOLINK: sVideoUrl = field.Value; break;
                            }
                        }

                        imgList.Add(new eAPIProductScreenshot(imgURL, imgLabel, imgOrder) { VideoURL = sVideoUrl });
                    }
                };

            GetAuthenticateToken();

            _api.Search((int)ExtEnum.FIELDS_VISU.TAB, showMetaData, listCol, numPage, rowsPerPage, apiWhere, orderBy, GlobalSuccess(apiActionSuccess), _apiActionOnFailure);

            return imgList;
        }
        /// <summary>
        /// Retourne la liste des visuels correspondant au produit
        /// </summary>
        /// <param name="extensionId">ID de la fiche Produit</param>
        /// <returns></returns>
        [Obsolete]
        private List<eAPIProductScreenshot> GetScreenshots_Old(int extensionId)
        {
            List<eAPIProductScreenshot> imgList = new List<eAPIProductScreenshot>();

            int numPage = 1;
            int rowsPerPage = 0;
            bool showMetaData = false;

            List<int> listCol = new List<int>() {
                (int)ExtEnum.FIELDS_VISU.IMAGE,
                (int)ExtEnum.FIELDS_VISU.LABEL,
                (int)ExtEnum.FIELDS_VISU.ORDER,
                (int)ExtEnum.FIELDS_VISU.LANG
            };

            List<ApiOrderBy> orderBy = new List<ApiOrderBy>() { new ApiOrderBy() { DescId = (int)ExtEnum.FIELDS_VISU.ORDER, Order = 0 } };

            ApiWhereCustom apiWhere = new ApiWhereCustom("PARENTEVTID", Operator.OP_EQUAL, extensionId.ToString());

            Action<ApiResponseSearch> apiActionSuccess =
                delegate (ApiResponseSearch response)
                {
                    // TODO Ajouter cela filtre en tant que conditions => modifier la méthodes de recherche pour prendre une liste de where
                    int nIdCatLang = (int)ExtEnum.DATA_VISU_LANG.LANG_00;
                    switch (_param.Pref.User.UserLangId)
                    {
                        case 0: nIdCatLang = (int)ExtEnum.DATA_VISU_LANG.LANG_00; break;
                        case 1: nIdCatLang = (int)ExtEnum.DATA_VISU_LANG.LANG_01; break;
                        case 2: nIdCatLang = (int)ExtEnum.DATA_VISU_LANG.LANG_02; break;
                        case 3: nIdCatLang = (int)ExtEnum.DATA_VISU_LANG.LANG_03; break;
                        case 4: nIdCatLang = (int)ExtEnum.DATA_VISU_LANG.LANG_04; break;
                        case 5: nIdCatLang = (int)ExtEnum.DATA_VISU_LANG.LANG_05; break;
                    }

                    string imgURL, imgLabel;
                    int imgOrder;

                    foreach (ApiContainerRecordsRow row in response.ResultData.Rows)
                    {
                        imgURL = string.Empty;
                        imgLabel = string.Empty;
                        imgOrder = 0;

                        int nLang = (int)ExtEnum.DATA_VISU_LANG.LANG_00;
                        foreach (ApiContainerRecordsField field in row.Fields)
                        {
                            switch (field.DescId)
                            {
                                case (int)ExtEnum.FIELDS_VISU.IMAGE: imgURL = field.Value; break;
                                case (int)ExtEnum.FIELDS_VISU.LABEL: imgLabel = field.Value; break;
                                case (int)ExtEnum.FIELDS_VISU.ORDER: imgOrder = eLibTools.GetNum(field.Value); break;
                                case (int)ExtEnum.FIELDS_VISU.LANG: nLang = eLibTools.GetNum(field.DbValue); break;
                            }
                        }

                        if (nLang == nIdCatLang)
                            imgList.Add(new eAPIProductScreenshot(imgURL, imgLabel, imgOrder));
                    }
                };

            GetAuthenticateToken();

            _api.Search((int)ExtEnum.FIELDS_VISU.TAB, showMetaData, listCol, numPage, rowsPerPage, apiWhere, orderBy, GlobalSuccess(apiActionSuccess), _apiActionOnFailure);

            return imgList;
        }

        /// <summary>
        /// Retourne les informations de descriptions d'un produit
        /// </summary>
        /// <param name="extensionId"></param>
        /// <returns></returns>
        //SHA private to public
        public ExtensionProductDescription GetProductDescription(int extensionId)
        {
            ExtensionProductDescription productDesc = new ExtensionProductDescription();

            int numPage = 1;
            int rowsPerPage = 2;
            bool showMetaData = false;

            List<int> listCol = new List<int>() {
                (int)ExtEnum.FIELDS_DESCRIPION.INSTALLATION,
                (int)ExtEnum.FIELDS_DESCRIPION.SUMMARY,
                (int)ExtEnum.FIELDS_DESCRIPION.DESCRIPTION,
                (int)ExtEnum.FIELDS_DESCRIPION.TEXTE_LOGO
            };

            List<ApiOrderBy> orderBy = new List<ApiOrderBy>() {
                new ApiOrderBy() { DescId = (int)ExtEnum.FIELDS_DESCRIPION.LANG, Order = 0 },
                new ApiOrderBy() { DescId = (int)ExtEnum.FIELDS_DESCRIPION.CREATE_AT, Order = 0 }
            };

            int userIdCatLang = (int)ExtEnum.DATA_VISU_LANG.LANG_00;
            switch (_param.Pref.User.UserLangId)
            {
                case 0: userIdCatLang = (int)ExtEnum.DATA_DESCRIPION_LANG.LANG_00; break;
                case 1: userIdCatLang = (int)ExtEnum.DATA_DESCRIPION_LANG.LANG_01; break;
                case 2: userIdCatLang = (int)ExtEnum.DATA_DESCRIPION_LANG.LANG_02; break;
                case 3: userIdCatLang = (int)ExtEnum.DATA_DESCRIPION_LANG.LANG_03; break;
                case 4: userIdCatLang = (int)ExtEnum.DATA_DESCRIPION_LANG.LANG_04; break;
                case 5: userIdCatLang = (int)ExtEnum.DATA_DESCRIPION_LANG.LANG_05; break;
            }

            ISet<int> lstIdCatLang = new HashSet<int>();
            lstIdCatLang.Add(userIdCatLang);
            lstIdCatLang.Add((int)ExtEnum.DATA_DESCRIPION_LANG.LANG_01);
            lstIdCatLang.Add((int)ExtEnum.DATA_DESCRIPION_LANG.LANG_00);

            ApiWhereCustom apiWhere = new ApiWhereCustom()
            {
                WhereCustoms = new List<ApiWhereCustom>()
                {
                    new ApiWhereCustom("PARENTEVTID", Operator.OP_EQUAL, extensionId.ToString(), InterOperator.OP_AND),
                    new ApiWhereCustom(((int)ExtEnum.FIELDS_DESCRIPION.LANG).ToString(), Operator.OP_IN_LIST, eLibTools.Join(";", lstIdCatLang), InterOperator.OP_AND)
                }
            };

            Action<ApiResponseSearch> apiActionSuccess =
                delegate (ApiResponseSearch response)
                {
                    if (response.ResultData.Rows.Count <= 0)
                        return;

                    ApiContainerRecordsRow row;
                    // Dans le cas du user en FR, on prend la derniere ligne
                    if (_param.Pref.User.UserLangId == 0)
                        row = response.ResultData.Rows[response.ResultData.Rows.Count - 1];
                    else
                        row = response.ResultData.Rows[0];

                    if (row.Fields == null)
                        return;
                    //SHA
                    productDesc.FileId = row.FileId;

                    foreach (var fld in row.Fields)
                    {
                        switch (fld.DescId)
                        {
                            case (int)ExtEnum.FIELDS_DESCRIPION.INSTALLATION: productDesc.InstallationInformation = fld.Value; break;
                            case (int)ExtEnum.FIELDS_DESCRIPION.SUMMARY: productDesc.Summary = fld.Value; break;
                            case (int)ExtEnum.FIELDS_DESCRIPION.DESCRIPTION: productDesc.Description = fld.Value; break;
                            case (int)ExtEnum.FIELDS_DESCRIPION.TEXTE_LOGO: productDesc.TexteLogo = fld.Value; break;

                        }
                    }
                };

            GetAuthenticateToken();

            _api.Search((int)ExtEnum.FIELDS_DESCRIPION.TAB, showMetaData, listCol, numPage, rowsPerPage, apiWhere, orderBy, GlobalSuccess(apiActionSuccess), _apiActionOnFailure);

            return productDesc;
        }

        /// <summary>
        /// Retourne les informations de descriptions d'un produit
        /// </summary>
        /// <param name="extensionId"></param>
        /// <returns></returns>
        [Obsolete]
        private ExtensionProductDescription GetProductDescription_Old(int extensionId)
        {
            ExtensionProductDescription productDesc = new ExtensionProductDescription();

            int numPage = 1;
            int rowsPerPage = 25;
            bool showMetaData = false;

            List<int> listCol = new List<int>() {
                (int)ExtEnum.FIELDS_DESCRIPION.INSTALLATION,
                (int)ExtEnum.FIELDS_DESCRIPION.SUMMARY,
                (int)ExtEnum.FIELDS_DESCRIPION.DESCRIPTION,
                (int)ExtEnum.FIELDS_DESCRIPION.LANG
            };

            List<ApiOrderBy> orderBy = new List<ApiOrderBy>() { new ApiOrderBy() { DescId = (int)ExtEnum.FIELDS_DESCRIPION.CREATE_AT, Order = 0 } };

            ApiWhereCustom apiWhere = new ApiWhereCustom("PARENTEVTID", Operator.OP_EQUAL, extensionId.ToString());

            Action<ApiResponseSearch> apiActionSuccess =
                delegate (ApiResponseSearch response)
                {
                    if (response.ResultData.Rows.Count <= 0)
                        return;

                    ApiContainerRecordsRow lg_d = response.ResultData.Rows[0];
                    ApiContainerRecordsRow lg_00 = response.ResultData.Rows.Find(zz => zz.Fields.Exists(
                        aa => aa.DescId == (int)ExtEnum.FIELDS_DESCRIPION.LANG && aa.DbValue == ((int)ExtEnum.DATA_CHANGE_LOG_LANG.LANG_00).ToString())) ?? lg_d;
                    ApiContainerRecordsRow lg_01 = response.ResultData.Rows.Find(zz => zz.Fields.Exists(
                        aa => aa.DescId == (int)ExtEnum.FIELDS_DESCRIPION.LANG && aa.DbValue == ((int)ExtEnum.DATA_CHANGE_LOG_LANG.LANG_01).ToString())) ?? lg_00;
                    ApiContainerRecordsRow lg_02 = response.ResultData.Rows.Find(zz => zz.Fields.Exists(
                        aa => aa.DescId == (int)ExtEnum.FIELDS_DESCRIPION.LANG && aa.DbValue == ((int)ExtEnum.DATA_CHANGE_LOG_LANG.LANG_02).ToString())) ?? lg_01;
                    ApiContainerRecordsRow lg_03 = response.ResultData.Rows.Find(zz => zz.Fields.Exists(
                        aa => aa.DescId == (int)ExtEnum.FIELDS_DESCRIPION.LANG && aa.DbValue == ((int)ExtEnum.DATA_CHANGE_LOG_LANG.LANG_03).ToString())) ?? lg_01;
                    ApiContainerRecordsRow lg_04 = response.ResultData.Rows.Find(zz => zz.Fields.Exists(
                        aa => aa.DescId == (int)ExtEnum.FIELDS_DESCRIPION.LANG && aa.DbValue == ((int)ExtEnum.DATA_CHANGE_LOG_LANG.LANG_04).ToString())) ?? lg_01;
                    ApiContainerRecordsRow lg_05 = response.ResultData.Rows.Find(zz => zz.Fields.Exists(
                        aa => aa.DescId == (int)ExtEnum.FIELDS_DESCRIPION.LANG && aa.DbValue == ((int)ExtEnum.DATA_CHANGE_LOG_LANG.LANG_05).ToString())) ?? lg_01;

                    switch (_param.Pref.User.UserId)
                    {
                        case 0: lg_d = lg_00; break;
                        case 1: lg_d = lg_01; break;
                        case 2: lg_d = lg_02; break;
                        case 3: lg_d = lg_03; break;
                        case 4: lg_d = lg_04; break;
                        case 5: lg_d = lg_05; break;
                    }

                    foreach (var fld in lg_d.Fields)
                    {
                        switch (fld.DescId)
                        {
                            case (int)ExtEnum.FIELDS_DESCRIPION.INSTALLATION: productDesc.InstallationInformation = fld.Value; break;
                            case (int)ExtEnum.FIELDS_DESCRIPION.SUMMARY: productDesc.Summary = fld.Value; break;
                            case (int)ExtEnum.FIELDS_DESCRIPION.DESCRIPTION: productDesc.Description = fld.Value; break;
                        }
                    }
                };

            GetAuthenticateToken();

            _api.Search((int)ExtEnum.FIELDS_DESCRIPION.TAB, showMetaData, listCol, numPage, rowsPerPage, apiWhere, orderBy, GlobalSuccess(apiActionSuccess), _apiActionOnFailure);

            return productDesc;
        }

        /// <summary>
        /// Retourne le prix de l'extension correspondant au produit et à l'offre du client (table "Prix extension")
        /// </summary>
        private ExtensionPrice GetProductExtensionPrice(int extensionId)
        {
            ExtensionPrice price = new ExtensionPrice();

            int numPage = 1;
            int rowsPerPage = 1;
            bool showMetaData = false;

            List<int> listCol = new List<int>() {
                (int)ExtEnum.FIELDS_PRICE.OFFER,
                (int)ExtEnum.FIELDS_PRICE.PRICE,
                (int)ExtEnum.FIELDS_PRICE.UNIT,
                (int)ExtEnum.FIELDS_PRICE.INCLUDED
            };

            List<ApiOrderBy> orderBy = new List<ApiOrderBy>() { new ApiOrderBy() { DescId = (int)ExtEnum.FIELDS_PRICE.CREATE_AT, Order = 0 } };

            // Récupération de l'équivalent DataId de l'offre du client
            int offerDataId;
            switch (_param.Pref.ClientInfos.ClientOffer)
            {
                case eLibConst.ClientOffer.ACCES: offerDataId = (int)ExtEnum.DATA_PRICE_OFFER.ACCES; break;
                case eLibConst.ClientOffer.STANDARD: offerDataId = (int)ExtEnum.DATA_PRICE_OFFER.STANDARD; break;
                case eLibConst.ClientOffer.PREMIER: offerDataId = (int)ExtEnum.DATA_PRICE_OFFER.PREMIER; break;
                case eLibConst.ClientOffer.PRO: offerDataId = (int)ExtEnum.DATA_PRICE_OFFER.PRO; break;
                case eLibConst.ClientOffer.ESSENTIEL: offerDataId = (int)ExtEnum.DATA_PRICE_OFFER.ESSENTIEL; break;

                default: offerDataId = (int)ExtEnum.DATA_PRICE_OFFER.XRM; break;
            }

            ApiWhereCustom apiWhere = new ApiWhereCustom()
            {
                WhereCustoms = new List<ApiWhereCustom>()
                {
                    new ApiWhereCustom("EVTID", Operator.OP_EQUAL, extensionId.ToString(), InterOperator.OP_AND),
                    new ApiWhereCustom(((int)ExtEnum.FIELDS_PRICE.OFFER).ToString(), Operator.OP_EQUAL, offerDataId.ToString(), InterOperator.OP_AND)
                }
            };

            Action<ApiResponseSearch> apiActionSuccess =
                delegate (ApiResponseSearch response)
                {
                    if (response.ResultData.Rows.Count <= 0)
                        return;

                    var firstRow = response.ResultData.Rows[0];
                    if (firstRow.Fields == null)
                        return;

                    foreach (ApiContainerRecordsField field in firstRow.Fields)
                    {
                        switch (field.DescId)
                        {
                            case (int)ExtEnum.FIELDS_PRICE.PRICE: price.Price = field.DbValue; break;
                            case (int)ExtEnum.FIELDS_PRICE.OFFER: price.Offer = (eLibConst.ClientOffer)(eLibTools.GetNum(field.DbValue)); break;
                            case (int)ExtEnum.FIELDS_PRICE.UNIT: price.Unit = field.DbValue; break;
                            case (int)ExtEnum.FIELDS_PRICE.INCLUDED: price.Included = field.DbValue == "1"; break;
                        }
                    }
                };

            GetAuthenticateToken();

            _api.Search((int)ExtEnum.FIELDS_PRICE.TAB, showMetaData, listCol, numPage, rowsPerPage, apiWhere, orderBy, GlobalSuccess(apiActionSuccess), _apiActionOnFailure);

            return price;
        }

        /// <summary>
        /// Retourne la liste des informations sur les versions de l'extensions
        /// </summary>
        /// <param name="extensionId">ID eudo de l'extension</param>
        /// <returns></returns>
        private ExtensionVersionInfo GetVersionsInfos(int extensionId)
        {
            ExtensionVersionInfo versInfo = new ExtensionVersionInfo();

            int numPage = 1;
            int rowsPerPage = 25;
            bool showMetaData = false;

            List<int> listCol = new List<int>() {
                (int)ExtEnum.FIELDS_VERSION.VERSION,
                (int)ExtEnum.FIELDS_VERSION.VERSION_COMPATIBLE,
                (int)ExtEnum.FIELDS_VERSION.ENVIRONEMENT,
                (int)ExtEnum.FIELDS_VERSION.DEV_FINISH,
                (int)ExtEnum.FIELDS_VERSION.UPDATE,
                (int)ExtEnum.FIELDS_VERSION.STATUS,
                (int)ExtEnum.FIELDS_VERSION.NUM_VERSION,
                (int)ExtEnum.FIELDS_VERSION.IS_NEW,
                (int)ExtEnum.FIELDS_VERSION.DESCRIPTION,
                (int)ExtEnum.FIELDS_VERSION.NOTES,

                (int)ExtEnum.FIELDS_CHANGE_LOG.LANG,
                (int)ExtEnum.FIELDS_CHANGE_LOG.NEW,
            };

            List<ApiOrderBy> orderBy = new List<ApiOrderBy>() { new ApiOrderBy() { DescId = (int)ExtEnum.FIELDS_CHANGE_LOG.CREATE_AT, Order = 0 } };

            ApiWhereCustom apiWhere = new ApiWhereCustom("PARENTEVTID", Operator.OP_EQUAL, extensionId.ToString());

            Action<ApiResponseSearch> apiActionSuccess =
                delegate (ApiResponseSearch response)
                {
                    if (response.ResultData.Rows.Count <= 0)
                        return;

                    ApiContainerRecordsField fDateHomol = response.ResultData.Rows[0].Fields.Find(zz => zz.DescId == 14506);

                    if (string.IsNullOrEmpty(fDateHomol?.Value))
                    {
                        versInfo.DateUpdate = DateTime.MinValue;
                    }
                    else
                    {
                        try
                        {
                            versInfo.DateUpdate = DateTime.Parse(fDateHomol.Value);
                        }
                        catch
                        {
                            versInfo.DateUpdate = DateTime.MinValue;
                        }
                    }

                    ApiContainerRecordsRow lg_d = response.ResultData.Rows[0];
                    ApiContainerRecordsRow lg_00 = response.ResultData.Rows.Find(zz => zz.Fields.Exists(
                        aa => aa.DescId == (int)ExtEnum.FIELDS_CHANGE_LOG.LANG && aa.DbValue == ((int)ExtEnum.DATA_CHANGE_LOG_LANG.LANG_00).ToString())) ?? lg_d;
                    ApiContainerRecordsRow lg_01 = response.ResultData.Rows.Find(zz => zz.Fields.Exists(
                        aa => aa.DescId == (int)ExtEnum.FIELDS_CHANGE_LOG.LANG && aa.DbValue == ((int)ExtEnum.DATA_CHANGE_LOG_LANG.LANG_01).ToString())) ?? lg_00;
                    ApiContainerRecordsRow lg_02 = response.ResultData.Rows.Find(zz => zz.Fields.Exists(
                        aa => aa.DescId == (int)ExtEnum.FIELDS_CHANGE_LOG.LANG && aa.DbValue == ((int)ExtEnum.DATA_CHANGE_LOG_LANG.LANG_02).ToString())) ?? lg_01;
                    ApiContainerRecordsRow lg_03 = response.ResultData.Rows.Find(zz => zz.Fields.Exists(
                        aa => aa.DescId == (int)ExtEnum.FIELDS_CHANGE_LOG.LANG && aa.DbValue == ((int)ExtEnum.DATA_CHANGE_LOG_LANG.LANG_03).ToString())) ?? lg_01;
                    ApiContainerRecordsRow lg_04 = response.ResultData.Rows.Find(zz => zz.Fields.Exists(
                        aa => aa.DescId == (int)ExtEnum.FIELDS_CHANGE_LOG.LANG && aa.DbValue == ((int)ExtEnum.DATA_CHANGE_LOG_LANG.LANG_04).ToString())) ?? lg_01;
                    ApiContainerRecordsRow lg_05 = response.ResultData.Rows.Find(zz => zz.Fields.Exists(
                        aa => aa.DescId == (int)ExtEnum.FIELDS_CHANGE_LOG.LANG && aa.DbValue == ((int)ExtEnum.DATA_CHANGE_LOG_LANG.LANG_05).ToString())) ?? lg_01;

                    switch (_param.Pref.User.UserId)
                    {
                        case 0: lg_d = lg_00; break;
                        case 1: lg_d = lg_01; break;
                        case 2: lg_d = lg_02; break;
                        case 3: lg_d = lg_03; break;
                        case 4: lg_d = lg_04; break;
                        case 5: lg_d = lg_05; break;
                    }

                    foreach (var fld in lg_d.Fields)
                    {
                        switch (fld.DescId)
                        {
                            case (int)ExtEnum.FIELDS_CHANGE_LOG.NEW: versInfo.ChangeLog = fld.Value; break;
                            case (int)ExtEnum.FIELDS_VERSION.IS_NEW: versInfo.IsNew = fld.Value == "1"; break;
                            case (int)ExtEnum.FIELDS_VERSION.VERSION_COMPATIBLE: versInfo.MinEudoVersion = fld.Value; break;
                        }
                    }

                    // Valeur par défaut
                    if (versInfo.MinEudoVersion == null)
                        versInfo.MinEudoVersion = "10.301.000";
                };

            GetAuthenticateToken();

            _api.Search((int)ExtEnum.FIELDS_VERSION.TAB, showMetaData, listCol, numPage, rowsPerPage, apiWhere, orderBy, GlobalSuccess(apiActionSuccess), _apiActionOnFailure);

            return versInfo;
        }

        /// <summary>
        /// Retourne la liste des catégorie des produits
        /// </summary>
        /// <returns></returns>
        public List<APIExtensionStoreCatalogValue> GetExtensionCategories()
        {
            return GetExtensionCatalogueValues((int)ExtEnum.FIELDS_PRODUCT.CATEGORIES);
        }

        /// <summary>
        /// Retourne la liste des offres
        /// </summary>
        /// <returns></returns>
        public List<APIExtensionStoreCatalogValue> GetProductOffer()
        {
            return GetExtensionCatalogueValues((int)ExtEnum.FIELDS_PRODUCT.OFFERS);
        }


        /// <summary>
        /// Retourne la liste des offres - sur la table prix
        /// </summary>
        /// <returns></returns>
        public List<APIExtensionStoreCatalogValue> GetProductOfferPrice()
        {
            return GetExtensionCatalogueValues((int)ExtEnum.FIELDS_PRICE.OFFER);
        }

        /// <summary>
        /// Retourne la liste des catégorie des produits
        /// </summary>
        /// <returns></returns>
        public List<APIExtensionStoreCatalogValue> GetExtensionCatalogueValues(int fieldDid)
        {
            List<APIExtensionStoreCatalogValue> values = new List<APIExtensionStoreCatalogValue>();

            Action<ApiResponseCatalogValues> apiActionSuccess =
                delegate (ApiResponseCatalogValues response)
                {
                    APIExtensionStoreCatalogValue status;
                    string catalogLabel = response.ResultData.Label;

                    foreach (ApiContainerCatalog catalogValue in response.ResultData.CatalogValues)
                    {
                        status = new APIExtensionStoreCatalogValue();
                        status.Id = catalogValue.Id;
                        status.Label = catalogValue.Label.Trim();
                        status.Data = catalogValue.Data.Trim();
                        values.Add(status);
                    }
                };

            GetAuthenticateToken();

            // Récupération des valeurs de filtre (Catégories et Notes)
            _api.Catalog(fieldDid, GlobalSuccess(apiActionSuccess), _apiActionOnFailure);

            return values;
        }






        /// <summary>
        /// Permet de toujours avoir le ApiResults rempli
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actionSuccess">action a lancé après traitement</param>
        /// <returns></returns>
        private Action<T> GlobalSuccess<T>(Action<T> actionSuccess) where T : ApiResponse
        {
            return delegate (T response)
            {
                ApiResults = JsonConvert.SerializeObject(response);

                actionSuccess(response);
            };
        }

        #endregion

        #region Gestion Hotcom - Bases liées, Extensions liées

        public List<APIExtensionStoreCatalogValue> GetExtensionLieeStatus()
        {
            return GetExtensionCatalogueValues((int)ExtEnum.FIELDS_LINKED_EXTENSION.STATUT);
        }

        /// <summary>
        /// Recherche la fiche Base liée de la base en cours sur Hotcom
        /// </summary>
        /// <returns>Id de la fiche Base liée</returns>
        public APIExtensionStoreLinkedBase SearchBaseLiee()
        {
            APIExtensionStoreLinkedBase linkedBase = new APIExtensionStoreLinkedBase();

            int numPage = 1;
            int rowsPerPage = 0;
            bool showMetaData = false;

            List<int> listCol = new List<int>() {
                (int)ExtEnum.FIELDS_BASE_LIEE.TITLE,
                (int)ExtEnum.FIELDS_BASE_LIEE.SQL_NAME,
                (int)ExtEnum.FIELDS_PM.TITLE,
                (int)ExtEnum.FIELDS_CONTRAT.TITLE
            };

            List<ApiOrderBy> orderBy = new List<ApiOrderBy>() { new ApiOrderBy() { DescId = (int)ExtEnum.FIELDS_BASE_LIEE.SQL_NAME, Order = 0 } };

            ApiWhereCustom apiWhere = new ApiWhereCustom(((int)ExtEnum.FIELDS_BASE_LIEE.SQL_NAME).ToString(), Operator.OP_EQUAL, _param.Pref.GetBaseName);

            Action<ApiResponseSearch> apiActionSuccess =
                delegate (ApiResponseSearch response)
                {
                    if (response.ResultData.Rows.Count <= 0)
                        return;

                    var firstRow = response.ResultData.Rows[0];
                    if (firstRow.Fields == null)
                        return;

                    linkedBase.Id = firstRow.FileId;
                    foreach (ApiContainerRecordsField field in firstRow.Fields)
                    {
                        switch (field.DescId)
                        {
                            case (int)ExtEnum.FIELDS_BASE_LIEE.TITLE: linkedBase.Title = field.Value; break;
                            case (int)ExtEnum.FIELDS_BASE_LIEE.SQL_NAME: linkedBase.SQLName = field.Value; break;
                            case (int)ExtEnum.FIELDS_PM.TITLE: linkedBase.CompanyId = field.FileId; break;
                            case (int)ExtEnum.FIELDS_CONTRAT.TITLE: linkedBase.ContractId = field.FileId; break;
                        }
                    }
                };

            GetAuthenticateToken();

            _api.Search((int)ExtEnum.FIELDS_BASE_LIEE.TAB, showMetaData, listCol, numPage, rowsPerPage, apiWhere, orderBy, GlobalSuccess(apiActionSuccess), _apiActionOnFailure);

            return linkedBase;
        }

        /// <summary>
        /// Création d'une fiche "Base Liée" dans HOTCOM
        /// </summary>
        /// <param name="title">Intitulée</param>
        /// <param name="SQLName">Nom SQL de la base</param>
        /// <param name="offerId">Offre (id valeur catalogue)</param>
        /// <param name="productId">Produit (id valeur catalogue)</param>
        /// <param name="nbPaidSubscriptions">Nombre de licences payantes</param>
        /// <param name="nbFreeSubscriptions">Nombre de licences gratuites</param>
        /// <param name="nbLightSubscriptions">Nombre de licences lights</param>
        /// <returns>Id de la fiche Base liée créée</returns>
        public int CreateBaseLiee(string title, string SQLName, int offerId, int productId, int nbPaidSubscriptions = 0, int nbFreeSubscriptions = 0, int nbLightSubscriptions = 0, string licenseKey = "")
        {
            int newId = 0;

            List<object> listFields = new List<object>()
            {
                new { DescId = (int)ExtEnum.FIELDS_BASE_LIEE.TITLE, Value = title }, // Intitulé
                new { DescId = (int)ExtEnum.FIELDS_BASE_LIEE.SQL_NAME, Value = SQLName }, // Nom SQL
                new { DescId = (int)ExtEnum.FIELDS_BASE_LIEE.NB_PAID_SUBSCRIPTIONS, Value = nbPaidSubscriptions.ToString() }, // Licences payantes
                new { DescId = (int)ExtEnum.FIELDS_BASE_LIEE.NB_FREE_SUBSCRIPTIONS, Value = nbFreeSubscriptions.ToString() }, // Licences gratuites
                new { DescId = (int)ExtEnum.FIELDS_BASE_LIEE.NB_LIGHT_SUBSCRIPTIONS, Value = nbLightSubscriptions.ToString() }, // Licences lights
            };

            if (offerId > 0)
                listFields.Add(new { DescId = (int)ExtEnum.FIELDS_BASE_LIEE.OFFER, Value = offerId.ToString() }); // Offre

            if (productId > 0)
                listFields.Add(new { DescId = (int)ExtEnum.FIELDS_BASE_LIEE.PRODUCT, Value = productId.ToString() }); // Produit

            if (!string.IsNullOrEmpty(licenseKey))
                listFields.Add(new { DescId = (int)ExtEnum.FIELDS_BASE_LIEE.LICENSE_KEY, Value = licenseKey }); // Clé de license

            var CreaLinkedExt = new {
                Fields = listFields.ToArray()
            };

            Action<ApiResponseCUD> apiActionSuccess =
                delegate (ApiResponseCUD response)
                {
                    if (response.ResultInfos.Success && response.ResultData != null)
                        newId = response.ResultData.FileId;
                };

            GetAuthenticateToken();

            _api.Create((int)ExtEnum.FIELDS_BASE_LIEE.TAB, CreaLinkedExt, GlobalSuccess(apiActionSuccess), _apiActionOnFailure);

            return newId;
        }

        /// <summary>
        /// Recherche la fiche Extension liée sur Hotcom en fonction de l'extension et la base liée
        /// </summary>
        /// <param name="linkedBaseId">id de la fiche Base liée</param>
        /// <param name="extensionId">id de l'extension</param>
        /// <returns>Id de la fiche Extension liée</returns>
        public APIExtensionStoreLinkedExtension SearchExtensionLiee(int linkedBaseId, int extensionId)
        {
            APIExtensionStoreLinkedExtension linkedExtension = new APIExtensionStoreLinkedExtension();

            int numPage = 1;
            int rowsPerPage = 0;
            bool showMetaData = false;

            List<int> listCol = new List<int>() {
                (int)ExtEnum.FIELDS_LINKED_EXTENSION.TITLE,
                (int)ExtEnum.FIELDS_LINKED_EXTENSION.BASE,
                (int)ExtEnum.FIELDS_LINKED_EXTENSION.PRODUCT,
                (int)ExtEnum.FIELDS_LINKED_EXTENSION.STATUT,
                (int)ExtEnum.FIELDS_CONTRAT.TITLE
            };

            List<ApiOrderBy> orderBy = new List<ApiOrderBy>() { new ApiOrderBy() { DescId = (int)ExtEnum.FIELDS_LINKED_EXTENSION.TITLE, Order = 0 } };

            ApiWhereCustom apiWhere = new ApiWhereCustom()
            {
                WhereCustoms = new List<ApiWhereCustom>()
                {
                    new ApiWhereCustom(((int)ExtEnum.FIELDS_LINKED_EXTENSION.BASE).ToString(), Operator.OP_EQUAL, linkedBaseId.ToString(), InterOperator.OP_AND),
                    new ApiWhereCustom(((int)ExtEnum.FIELDS_LINKED_EXTENSION.PRODUCT).ToString(), Operator.OP_EQUAL, extensionId.ToString(), InterOperator.OP_AND)
                }
            };

            Action<ApiResponseSearch> apiActionSuccess =
                delegate (ApiResponseSearch response)
                {
                    if (response.ResultData.Rows.Count <= 0)
                        return;

                    var firstRow = response.ResultData.Rows[0];
                    if (firstRow.Fields == null)
                        return;

                    linkedExtension.Id = firstRow.FileId;
                    foreach (ApiContainerRecordsField field in firstRow.Fields)
                    {
                        switch (field.DescId)
                        {
                            case (int)ExtEnum.FIELDS_LINKED_EXTENSION.TITLE: linkedExtension.Title = field.Value; break;
                            case (int)ExtEnum.FIELDS_LINKED_EXTENSION.BASE: linkedExtension.BaseId = eLibTools.GetNum(field.Value); break;
                            case (int)ExtEnum.FIELDS_LINKED_EXTENSION.PRODUCT: linkedExtension.ExtensionId = eLibTools.GetNum(field.Value); break;
                            case (int)ExtEnum.FIELDS_LINKED_EXTENSION.STATUT: linkedExtension.StatutId = eLibTools.GetNum(field.Value); break;
                            case (int)ExtEnum.FIELDS_CONTRAT.TITLE: linkedExtension.ContractId = eLibTools.GetNum(field.Value); break;
                        }
                    }
                };

            GetAuthenticateToken();

            _api.Search((int)ExtEnum.FIELDS_LINKED_EXTENSION.TAB, showMetaData, listCol, numPage, rowsPerPage, apiWhere, orderBy, GlobalSuccess(apiActionSuccess), _apiActionOnFailure);

            return linkedExtension;
        }

        /// <summary>
        /// Met à jour une fiche "Extension liée" dans HOTCOM
        /// </summary>
        /// <param name="linkedExtId">Id de la fiche "Extension liée" à mettre à jour</param>
        /// <param name="extensionStatus">Statut de l'extension (id valeur catalogue)</param>
        /// <returns></returns>
        public bool UpdateExtensionLiee(int linkedExtId, int extensionStatus)
        {
            bool success = false;

            object UpdLinkedExt = new {
                Fields = new[]
                    {
                        new { DescId = (int)ExtEnum.FIELDS_LINKED_EXTENSION.DATE, Value = eAPI.FormatDate(GetRemoteServerDate()) },   // Date
                        new { DescId = (int)ExtEnum.FIELDS_LINKED_EXTENSION.STATUT, Value = extensionStatus.ToString() },   // Statut
                        new { DescId = (int)ExtEnum.FIELDS_LINKED_EXTENSION.REQUEST_USER, Value = _param.Pref.User.UserDisplayName },   // Demandé par
                        new { DescId = (int)ExtEnum.FIELDS_LINKED_EXTENSION.EMAIL, Value = _param.Pref.User.UserMail },   // Email demandeur
                        new { DescId = (int)ExtEnum.FIELDS_LINKED_EXTENSION.PHONE, Value = _param.Pref.User.UserMobile },   // Téléphone demandeur
                        new { DescId = (int)ExtEnum.FIELDS_LINKED_EXTENSION.VERSION, Value = eConst.VERSION },   // Version
                    }
            };

            Action<ApiResponse> apiActionSuccess =
                delegate (ApiResponse response)
                {
                    success = response.ResultInfos.Success;
                };

            GetAuthenticateToken();

            _api.Update((int)ExtEnum.FIELDS_LINKED_EXTENSION.TAB, linkedExtId, UpdLinkedExt, GlobalSuccess(apiActionSuccess), _apiActionOnFailure);

            return success;
        }

        /// <summary>
        /// Crée une fiche "Extension liée" dans HOTCOM
        /// </summary>
        /// <param name="title">Intitulé</param>
        /// <param name="extensionStatus">Statut de l'extension (id valeur catalogue)</param>
        /// <param name="companyId">Id société</param>
        /// <param name="contratId">Id contrat</param>
        /// <param name="extensionId">Id extension (table Produit)</param>
        /// <param name="baseId">Id base liée</param>
        /// <returns></returns>
        public int CreateExtensionLiee(string title, int extensionStatus, int companyId, int contratId, int extensionId, int baseId)
        {
            int newId = 0;

            List<object> listFields = new List<object>()
            {
                new { DescId = (int)ExtEnum.FIELDS_LINKED_EXTENSION.TITLE, Value = title },   // Titre
                new { DescId = (int)ExtEnum.FIELDS_LINKED_EXTENSION.DATE, Value = eAPI.FormatDate(GetRemoteServerDate()) },   // Date
                new { DescId = (int)ExtEnum.FIELDS_LINKED_EXTENSION.REQUEST_USER, Value = _param.Pref.User.UserDisplayName },   // Demandé par
                new { DescId = (int)ExtEnum.FIELDS_LINKED_EXTENSION.EMAIL, Value = _param.Pref.User.UserMail },   // Email demandeur
                new { DescId = (int)ExtEnum.FIELDS_LINKED_EXTENSION.PHONE, Value = _param.Pref.User.UserMobile },   // Téléphone demandeur
                new { DescId = (int)ExtEnum.FIELDS_LINKED_EXTENSION.VERSION, Value = eConst.VERSION },   // Version
            };

            if (companyId > 0)
                listFields.Add(new { DescId = (int)ExtEnum.FIELDS_PM.TAB, Value = companyId.ToString() }); // Société

            if (contratId > 0)
                listFields.Add(new { DescId = (int)ExtEnum.FIELDS_CONTRAT.TAB, Value = contratId.ToString() }); // Contrat

            if (extensionId > 0)
                listFields.Add(new { DescId = (int)ExtEnum.FIELDS_LINKED_EXTENSION.PRODUCT, Value = extensionId.ToString() }); // Extension

            if (baseId > 0)
                listFields.Add(new { DescId = (int)ExtEnum.FIELDS_LINKED_EXTENSION.BASE, Value = baseId.ToString() }); // Base liée

            if (extensionStatus > 0)
                listFields.Add(new { DescId = (int)ExtEnum.FIELDS_LINKED_EXTENSION.STATUT, Value = extensionStatus.ToString() }); // Statut

            var CreaLinkedExt = new {
                Fields = listFields.ToArray()
            };

            Action<ApiResponseCUD> apiActionSuccess =
                delegate (ApiResponseCUD response)
                {
                    if (response.ResultInfos.Success && response.ResultData != null)
                        newId = response.ResultData.FileId;
                };

            GetAuthenticateToken();

            _api.Create((int)ExtEnum.FIELDS_LINKED_EXTENSION.TAB, CreaLinkedExt, GlobalSuccess(apiActionSuccess), _apiActionOnFailure);

            return newId;
        }

        /// <summary>
        /// Indique si il y a besoin de mettre à jour le statut de l'extension liee en fonction de son statut actuel
        /// </summary>
        /// <param name="oldStatus"></param>
        /// <param name="newStatus"></param>
        /// <returns></returns>
        public bool IsNeededUpdateExtensionLieeStatus(APIExtensionStoreCatalogValue oldStatus, APIExtensionStoreCatalogValue newStatus)
        {
            List<string> listEnableRequest = new List<string>()
                {
                    eAPIExtensionStoreParam.LinkedExtensionStatutsCodes.EnableRequest,
                    eAPIExtensionStoreParam.LinkedExtensionStatutsCodes.EnableRequestTransmittedToIC,
                    eAPIExtensionStoreParam.LinkedExtensionStatutsCodes.CommercialProposalSent
                };

            List<string> listEnabled = new List<string>()
                {
                    eAPIExtensionStoreParam.LinkedExtensionStatutsCodes.Enabled,
                };

            List<string> listDisableRequest = new List<string>()
                {
                    eAPIExtensionStoreParam.LinkedExtensionStatutsCodes.DisableRequest,
                };

            List<string> listDisabled = new List<string>()
                {
                    eAPIExtensionStoreParam.LinkedExtensionStatutsCodes.Disabled,
                };

            if (oldStatus != null && newStatus != null &&
                (
                    (listEnableRequest.Contains(oldStatus.Data) && listEnableRequest.Contains(newStatus.Data))
                    || (listEnabled.Contains(oldStatus.Data) && listEnabled.Contains(newStatus.Data))
                    || (listDisableRequest.Contains(oldStatus.Data) && listDisableRequest.Contains(newStatus.Data))
                    || (listDisabled.Contains(oldStatus.Data) && listDisabled.Contains(newStatus.Data))
                ))
                return false;

            return true;
        }

        #endregion
    }


    /// <summary>
    /// Classe de Paramètres d'interogation
    /// </summary>
    public class extensionQueryParam
    {
        /// <summary>
        /// Charge les informations de captures d'écrans
        /// </summary>
        public bool ScreenShots = false;

        /// <summary>
        /// Charge les informations de version
        /// </summary>
        public bool VersionInfos = false;

        /// <summary>
        /// Charge les informations de descriptions
        /// </summary>
        public bool DescriptionInfos = false;

        /// <summary>
        /// Charge les informations de prix
        /// </summary>
        public bool PriceInfos = false;

    }
}