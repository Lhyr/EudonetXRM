using System;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Class permettant d'afficher le paramétrage des caractéristiques du siget Grille
    /// </summary>
    public class eAdminBkmGridFeaturesRenderer : eAdminBlockRenderer
    {
        private eAdminTableInfos _bkmTabInfos;
        private int _bkmTab;

        private eAdminBkmGridFeaturesRenderer(ePref pref, eAdminTableInfos tabInfos, eAdminTableInfos bkmTabInfos, string title, string titleInfo, string tooltip)
            : base(pref, tabInfos, title, titleInfo)
        {
            _tab = tabInfos.DescId;
            this.BlockTitleTooltip = tooltip;

            _bkmTabInfos = bkmTabInfos;
            _bkmTab = _bkmTabInfos.DescId;
        }

        /// <summary>
        /// Crée l'objet de paramétrage
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tabInfos"></param>
        /// <param name="bkmTabInfos"></param>
        /// <param name="title"></param>
        /// <param name="titleInfo"></param>
        /// <param name="tooltip"></param>
        /// <returns></returns>
        public static eAdminBkmGridFeaturesRenderer GetAdminBkmGridFeaturesRenderer(ePref pref, eAdminTableInfos tabInfos, eAdminTableInfos bkmTabInfos, string title, string titleInfo, string tooltip)
        {
            eAdminBkmGridFeaturesRenderer features = new eAdminBkmGridFeaturesRenderer(pref, tabInfos, bkmTabInfos, title, titleInfo, tooltip);
            return features;
        }

        /// <summary>Construction du bloc Caractéristiques</summary>
        /// <returns></returns>
        protected override bool Build()
        {
            this.OpenedBlock = true;

            base.Build();

            // Libellé du signet grille
            eAdminTextboxField adminField = new eAdminTextboxField(_bkmTabInfos.DescId,"Libellé du signet Grille", eAdminUpdateProperty.CATEGORY.RES, Pref.LangId, value: _bkmTabInfos.TableLabel);
            adminField.SetFieldControlID("txtTabName");
            adminField.Generate(_panelContent);
                    

            return true;
        }
    }
}