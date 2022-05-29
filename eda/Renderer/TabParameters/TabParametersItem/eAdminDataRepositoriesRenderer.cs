using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Rendu du bloc "Référentiel de données" parmi les paramètres d'un onglet sur le menu de droite
    /// </summary>
    public class eAdminDataRepositoriesRenderer : eAdminBlockRenderer
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref">Objet ePref</param>
        /// <param name="tabInfos">Objet eAdminTableInfos contenant les infos de l'onglet concerné par le paramétrage</param>
        /// <param name="title">Titre du bloc</param>
        /// <param name="titleInfo">Descriptif du bloc</param>
        public eAdminDataRepositoriesRenderer(ePref pref, eAdminTableInfos tabInfos, String title, String titleInfo)
            : base(pref, tabInfos, title, titleInfo, idBlock: "DataRepositoriesPart")
        {

        }

        /// <summary>
        /// Création du renderer
        /// </summary>
        /// <param name="pref">Objet ePref</param>
        /// <param name="tabInfos">Objet eAdminTableInfos contenant les infos de l'onglet concerné par le paramétrage</param>
        /// <param name="title">Titre du bloc</param>
        /// <param name="titleInfo">Descriptif du bloc</param>
        /// <returns></returns>
        public static eAdminDataRepositoriesRenderer CreateAdminDataRepositoriesRenderer(ePref pref, eAdminTableInfos tabInfos, String title, String titleInfo)
        {
            eAdminDataRepositoriesRenderer features = new eAdminDataRepositoriesRenderer(pref, tabInfos, title, titleInfo);
            return features;
        }

        /// <summary>
        /// Construction du renderer
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            base.Build();

            #region Référentiel Sirene
            if (eExtension.IsReady(Pref, ExtensionCode.SIRENE))
            {
                // administrer le référentiel Sirene
                // Récupération du premier descid défini avec autocomplete activé, mais uniquement ceux qui sont utilisés par Sirene
                IEnumerable<KeyValuePair<int, int>> autocompleteSireneEnabledFields = _tabInfos.GetAutocompleteAddressFields(Pref).Where(
                    mp => (
                        EudoQuery.Field.AutoCompletionEnabledStatic((EudoQuery.AutoCompletion)mp.Value) == true &&
                        eSireneMapping.GetSireneEnabledFields(Pref, _tabInfos.DescId).Contains(mp.Key.ToString())
                    )
                );

                string sLib = eResApp.GetRes(Pref, 8544); // Référentiel Sirene

                if (autocompleteSireneEnabledFields.Count() > 0)
                    sLib = String.Concat(sLib, " ", "(", autocompleteSireneEnabledFields.Count(), ")");

                eAdminButtonField btnSirene = new eAdminButtonField(sLib, "eAdmnBtnSirene", eResApp.GetRes(Pref, 7025), "nsAdmin.confSirene();");

                btnSirene.Generate(_panelContent);
            }
            #endregion

            return true;
        }
    }
}