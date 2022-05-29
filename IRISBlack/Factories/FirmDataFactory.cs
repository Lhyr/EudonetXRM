using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Model;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// 
    /// </summary>
    public class FirmDataFactory
    {

        int nTab { get; set; }
        ePref preference { get; set; }
        IDictionary<eLibConst.CONFIGADV, string> dicConfigAdv { get; set; }
        List<int> liSireneTriggers { get; set; } =  new List<int>();

        #region constructeurs
        /// <summary>
        /// Constructeur Pour la factory
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="pref"></param>
        private FirmDataFactory(int desc, ePref pref)
        {
            nTab = desc;
            preference = pref;
        }
        #endregion

        #region Initialisation statique de l'objet
        /// <summary>
        /// Initialisation statique de la classe FirmDataFactory.
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static FirmDataFactory InitFirmDataFactory(int desc, ePref pref)
        {
            return new FirmDataFactory(desc, pref);
        }
        #endregion

        #region public
        /// <summary>
        /// Autocompletion pour Sirene.
        /// </summary>
        public AutoCompletionMappingModel SireneFactory()
        {

            string sError;
            string sSireneProviderUrl = "";
            AutoCompletionMappingModel acmSireneMapping = null;
            List<eFilemapPartner> liFmpSireneMapping = null;


            dicConfigAdv = eLibTools.GetConfigAdvValues(preference,
                new HashSet<eLibConst.CONFIGADV> {
                          eLibConst.CONFIGADV.SIRENE_API_URL
                          , eLibConst.CONFIGADV.AUTO_COMPLETION_ADR_PROVIDER
                          , eLibConst.CONFIGADV.PREDICTIVEADDRESSESREF

                    }
                );



            #region sirene
            try
            {
                sSireneProviderUrl = dicConfigAdv[eLibConst.CONFIGADV.SIRENE_API_URL];

                liFmpSireneMapping = eAutoCompletionTools.LoadFromSirene(preference,
                      tab: nTab,
                      mapType: FILEMAP_TYPE.AUTOCOMPLETE,
                      autoCplType: AutoCompletionType.AUTO_COMPLETION_ADDRESS,
                      sireneFields: null,
                      error: out sError)
                      .Where(fmp => fmp.DescId > 0).ToList();

                if (sError.Length > 0)
                    throw new EudoException(String.Concat("Une erreur est survenue lors de la récupération du mapping Sirene dans FileMapPartner ", Environment.NewLine, sError));

                liSireneTriggers.AddRange(eSireneMapping.GetSireneEnabledFields(preference, nTab).Select(s => int.Parse(s)));

                if (liSireneTriggers.Count > 0 && liFmpSireneMapping.Count > 0)
                    acmSireneMapping = new AutoCompletionMappingModel(sSireneProviderUrl, liFmpSireneMapping, liSireneTriggers);


                return acmSireneMapping;
            }
            catch (InvalidCastException e)
            {
                throw new EudoException("Une erreur est survenue lors de la récupération du mapping Sirene, il est probable que les déclencheurs soient mal enregistrés", innerExcp: e);
            }
            catch (Exception e)
            {
                throw new EudoException("Une erreur est survenue lors de la récupération du mapping Sirene", innerExcp: e);
            }
            #endregion

        }

        /// <summary>
        /// Autocompletion pour datagouv et Bing
        /// </summary>
        /// <returns></returns>
        public AutoCompletionMappingModel DataGouvFactory()
        {

            #region addresse predictive bingmap/datagouv
            try
            {
                string sPredictAddrProviderUrl = "";
                List<int> liPredictAddrTriggers = new List<int>();
                List<eFilemapPartner> liFmpPredictAddrMapping = null;
                AutoCompletionMappingModel acmPredictAddrMapping = null;
                string sError = "";


                string sProvider = dicConfigAdv[eLibConst.CONFIGADV.PREDICTIVEADDRESSESREF];
                eConst.PredictiveAddressesRef eProvider = (eConst.PredictiveAddressesRef)eLibTools.GetNum(sProvider);

                //l'url de référence bingmap est en dur coté front
                if (eProvider != eConst.PredictiveAddressesRef.BingMapsV8)
                    sPredictAddrProviderUrl = dicConfigAdv[eLibConst.CONFIGADV.AUTO_COMPLETION_ADR_PROVIDER].Trim();

                liFmpPredictAddrMapping = eFilemapPartner.LoadAutoCompletion(preference,
                        tab: nTab,
                        mapType: FILEMAP_TYPE.AUTOCOMPLETE,
                        autoCplType: AutoCompletionType.AUTO_COMPLETION_ADDRESS,
                        error: out sError).Where(fmp => fmp.DescId > 0).ToList();

                if (sError.Length > 0)
                    throw new EudoException(String.Concat("Une erreur est survenue lors de la récupération du mapping d'adresse predictive dans FileMapPartner ", Environment.NewLine, sError));

                // Récupération du premier descid défini avec autocomplete activé, mais uniquement parmi ceux qui ne sont PAS utilisés par Sirene
                Dictionary<int, int> dicoAutocompleteAddress = Internal.eda.eSqlDesc.LoadAutocompleteAddressFields(preference, nTab);
                KeyValuePair<int, int> mappingAutoCompletion = dicoAutocompleteAddress.FirstOrDefault(
                    mp => (
                        EudoQuery.Field.AutoCompletionEnabledStatic((EudoQuery.AutoCompletion)mp.Value) &&
                        !liSireneTriggers.Contains(mp.Key)
                    )
                );


                liPredictAddrTriggers.Add(mappingAutoCompletion.Key);

                if (liPredictAddrTriggers.Count > 0 && liFmpPredictAddrMapping.Count > 0)
                    acmPredictAddrMapping = new AutoCompletionMappingModel(sPredictAddrProviderUrl, liFmpPredictAddrMapping, liPredictAddrTriggers);


                return acmPredictAddrMapping;
            }
            catch (Exception e)
            {
                throw new EudoException("Une erreur est survenue lors de la récupération d'adresse predictive", innerExcp: e);
            }

            #endregion

        }
        #endregion

    }
}