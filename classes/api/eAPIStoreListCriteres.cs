using System.Collections.Generic;
using System.Linq;
using Com.Eudonet.Core.Model;
using EudoQuery;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// 
    /// </summary>
    public enum StoreListTypeRefresh
    {
        FULL,
        LIST,
        ADD_ITEM
    }

    /// <summary>
    ///
    /// </summary>
    public enum StoreListDisplayType
    {
        ALL = 0,
        INSTALLED = 1,
        INSTALL_WAITING = 2,
        SUGGEST = 3
    }

    /// <summary>
    /// 
    /// </summary>
    public enum StoreListOtherFilter
    {
        FREE = 0,
        COMPATIBLE = 1,
        NEW = 2,
    }

    /// <summary>
    /// 
    /// </summary>
    public class StoreListCriteres
    {
        /// <summary>Recherche</summary>
        public string Search { get; set; }
        /// <summary>Categorie</summary>
        public string Category { get; set; }
        /// <summary>Offres choisi</summary>
        public IEnumerable<ExtEnum.DATA_PRODUCT_OFFER> Offres { get; set; }
        /// <summary>Autres critères actif</summary>
        public IEnumerable<StoreListOtherFilter> OtherFilters { get; set; }
        /// <summary>Status d'affichage</summary>
        public StoreListDisplayType DisplayType { get; set; }

        /// <summary>
        /// Indique si un filtre de recherche est activé sur la liste
        /// </summary>
        public bool FilterSearchEnabled
        {
            get
            {
                return Search != null && Search.Trim().Length > 0;
            }
        }

        /// <summary>
        /// Indique si un filtre de catégorie est activé sur la liste
        /// </summary>
        public bool FilterCategoryEnabled
        {
            get
            {
                return Category != null && Category.Trim().Length > 0;
            }
        }

        /// <summary>
        /// TODO - A COMPLETER pour OtherFilters et DisplayType
        /// </summary>
        /// <param name="apiWhere">critères de recherche</param>
        /// <param name="pref">préférences</param>
        public void AddWhereCustom(ApiWhereCustom apiWhere, ePref pref)
        {
            // Ajout d'un critère de filtre sur les catégories si défini, et différent de <TOUS>
            if (FilterCategoryEnabled)
            {
                if (Category != "-1")
                    apiWhere.WhereCustoms.Add(new ApiWhereCustom(((int)ExtEnum.FIELDS_PRODUCT.CATEGORIES).ToString(), Operator.OP_CONTAIN, Category, InterOperator.OP_AND));
                else
                    apiWhere.WhereCustoms.Add(new ApiWhereCustom(((int)ExtEnum.FIELDS_PRODUCT.CATEGORIES).ToString(), Operator.OP_IS_EMPTY, "", InterOperator.OP_AND));
            }

            if (DisplayType != StoreListDisplayType.ALL)
            {
                HashSet<string> lstCode = new HashSet<string>();

                eDataFillerGeneric dtf = new eDataFillerGeneric(pref, (int)TableType.EXTENSION, ViewQuery.CUSTOM);
                dtf.EudoqueryComplementaryOptions = delegate (EudoQuery.EudoQuery eq)
                {
                    eq.SetListCol = ((int)ExtensionField.EXTENSIONCODE).ToString();

                    WhereCustom wc = null;
                    HashSet<EXTENSION_STATUS> lstInstalled = new HashSet<EXTENSION_STATUS> {
                                EXTENSION_STATUS.STATUS_INSTALLED,
                                EXTENSION_STATUS.STATUS_READY,
                                EXTENSION_STATUS.STATUS_DESACTIVATION_ASKED,
                                EXTENSION_STATUS.STATUS_NEED_CONFIGURING,
                            };

                    HashSet<EXTENSION_STATUS> lstInstalledOrAsked = new HashSet<EXTENSION_STATUS>(lstInstalled);
                    lstInstalledOrAsked.Add(EXTENSION_STATUS.STATUS_ACTIVATION_ASKED);

                    switch (DisplayType)
                    {
                        case StoreListDisplayType.INSTALLED:
                            wc = new WhereCustom(((int)ExtensionField.EXTENSIONSTATUS).ToString(), Operator.OP_IN_LIST, eLibTools.Join(";", lstInstalled));
                            break;
                        case StoreListDisplayType.INSTALL_WAITING:
                            wc = new WhereCustom(((int)ExtensionField.EXTENSIONSTATUS).ToString(), Operator.OP_IN_LIST, EXTENSION_STATUS.STATUS_ACTIVATION_ASKED.ToString());
                            break;
                        case StoreListDisplayType.SUGGEST:
                            wc = new WhereCustom(((int)ExtensionField.EXTENSIONSTATUS).ToString(), Operator.OP_IN_LIST, eLibTools.Join(";", lstInstalledOrAsked));
                            break;
                        default:
                            break;
                    }

                    eq.AddCustomFilter(wc);
                };

                if (!dtf.Generate())
                { //TODO gestion d'erreur ?
                    throw dtf.InnerException ?? new EudoException(dtf.ErrorMsg);
                }

                foreach (eRecord rec in dtf.ListRecords)
                {
                    eFieldRecord field = rec.GetFieldByAlias(string.Format("{0}_{1}", (int)TableType.EXTENSION, (int)ExtensionField.EXTENSIONCODE));
                    lstCode.Add(field.Value);
                }


                ApiWhereCustom statusWhereCustom = new ApiWhereCustom() { InterOperator = (int)InterOperator.OP_AND };
                statusWhereCustom.WhereCustoms = new List<ApiWhereCustom>();
                //pour les extensions suggérées, on prend les extension qui n'apparaissent pas dans les extensions activées ou demandées.
                statusWhereCustom.WhereCustoms.Add(new ApiWhereCustom(((int)ExtEnum.FIELDS_PRODUCT.NATIVE_ID).ToString(), DisplayType == StoreListDisplayType.SUGGEST ? Operator.OP_NOT_IN_LIST : Operator.OP_IN_LIST, eLibTools.Join(";", lstCode)));


                // Ajout des sous-critères au WhereCustom global
                apiWhere.WhereCustoms.Add(statusWhereCustom);

            }



            // Ajout d'un critère sur les champs texte pour la recherche, si définie
            if (FilterSearchEnabled)
            {
                // Ajout des sous-critères sur chaque champ texte, en OR
                ApiWhereCustom searchWhereCustom = new ApiWhereCustom() { InterOperator = (int)InterOperator.OP_AND };
                // Il est nécessaire de repréciser l'InterOperator qui est à NONE par défaut, afin que notre série de critères s'additionne aux critères de base
                // sur le type de produits (WhereCustom ci-dessus)
                searchWhereCustom.WhereCustoms = new List<ApiWhereCustom>();

                searchWhereCustom.WhereCustoms.Add(
                    new ApiWhereCustom(((int)ExtEnum.FIELDS_PRODUCT.TITLE).ToString(), Operator.OP_CONTAIN, Search, InterOperator.OP_OR));
                searchWhereCustom.WhereCustoms.Add(
                    new ApiWhereCustom(((int)ExtEnum.FIELDS_PRODUCT.AUTHOR).ToString(), Operator.OP_CONTAIN, Search, InterOperator.OP_OR));
                searchWhereCustom.WhereCustoms.Add(
                    new ApiWhereCustom(((int)ExtEnum.FIELDS_PRODUCT.CATEGORIES).ToString(), Operator.OP_CONTAIN, Search, InterOperator.OP_OR));
                searchWhereCustom.WhereCustoms.Add(
                    new ApiWhereCustom(((int)ExtEnum.FIELDS_DESCRIPION.SUMMARY).ToString(), Operator.OP_CONTAIN, Search, InterOperator.OP_OR));
                searchWhereCustom.WhereCustoms.Add(
                    new ApiWhereCustom(((int)ExtEnum.FIELDS_DESCRIPION.DESCRIPTION).ToString(), Operator.OP_CONTAIN, Search, InterOperator.OP_OR));
                searchWhereCustom.WhereCustoms.Add(
                    new ApiWhereCustom(((int)ExtEnum.FIELDS_DESCRIPION.TEXTE_LOGO).ToString(), Operator.OP_CONTAIN, Search, InterOperator.OP_OR));

                // Ajout des sous-critères au WhereCustom global
                apiWhere.WhereCustoms.Add(searchWhereCustom);
            }

            // Ajout du critere sur l'offre
            if (Offres != null && Offres.Count() > 0)
            {
                apiWhere.WhereCustoms.Add(new ApiWhereCustom(((int)ExtEnum.FIELDS_PRODUCT.OFFERS).ToString(), Operator.OP_IN_LIST, string.Join(";", Offres.Select(it => (int)it)), InterOperator.OP_AND));
            }

            #region OtherFilter

            if (OtherFilters.ToList().Contains(StoreListOtherFilter.FREE))
            {
                ApiWhereCustom freeWhereCustom = new ApiWhereCustom() { InterOperator = (int)InterOperator.OP_AND };
                freeWhereCustom.WhereCustoms = new List<ApiWhereCustom>();

                freeWhereCustom.WhereCustoms.Add(new ApiWhereCustom(((int)ExtEnum.FIELDS_PRICE.OFFER).ToString(), Operator.OP_EQUAL, ((int)ExtEnum.MapPriceClientOffer(pref.ClientInfos.ClientOffer)).ToString(), InterOperator.OP_AND));
                freeWhereCustom.WhereCustoms.Add(new ApiWhereCustom(((int)ExtEnum.FIELDS_PRICE.INCLUDED).ToString(), Operator.OP_IS_TRUE, string.Empty, InterOperator.OP_AND));

                apiWhere.WhereCustoms.Add(freeWhereCustom);
            }

            if (OtherFilters.ToList().Contains(StoreListOtherFilter.COMPATIBLE))
            {
                Dictionary<eLibConst.CONFIG_DEFAULT, string> dicoConfig = pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[] { eLibConst.CONFIG_DEFAULT.VERSION });

                apiWhere.WhereCustoms.Add(new ApiWhereCustom(((int)ExtEnum.FIELDS_VERSION.VERSION_COMPATIBLE).ToString(), Operator.OP_LESS_OR_EQUAL,  dicoConfig[eLibConst.CONFIG_DEFAULT.VERSION], InterOperator.OP_AND));
                //apiWhere.WhereCustoms.Add(new ApiWhereCustom(((int)ExtEnum.FIELDS_VERSION.VERSION_COMPATIBLE).ToString(), Operator.OP_START_WITH, "Eudonet", InterOperator.OP_AND));
            }

            if (OtherFilters.ToList().Contains(StoreListOtherFilter.NEW))
            {
                apiWhere.WhereCustoms.Add(new ApiWhereCustom(((int)ExtEnum.FIELDS_PRODUCT.ISNEW).ToString(), Operator.OP_IS_TRUE, string.Empty, InterOperator.OP_AND));
            }

            #endregion
        }
    }
}