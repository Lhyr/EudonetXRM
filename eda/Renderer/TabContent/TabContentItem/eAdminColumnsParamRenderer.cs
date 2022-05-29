using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Linq;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe de rendu du bloc "Colonnes" dans "Contenu de l'onglet"
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eda.eAdminBlockRenderer" />
    public class eAdminColumnsParamRenderer : eAdminBlockRenderer
    {
        private eAdminColumnsParamRenderer(ePref pref, eAdminTableInfos tabInfos, String title, String titleInfo, string[] openedBlocks = null)
            : base(pref, tabInfos, title, titleInfo, "partNbColumns", bOpenedBlock: openedBlocks != null && openedBlocks.Contains("partNbColumns"))
        {

        }
        /// <summary>
        /// Retourne un objet eAdminColumnsParamRenderer
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="tabInfos">Infos de la table</param>
        /// <param name="title">Titre du bloc</param>
        /// <param name="titleInfo">Sous-titre du bloc</param>
        /// <param name="openedBlocks">Blocs ouverts</param>
        /// <returns></returns>
        public static eAdminColumnsParamRenderer CreateAdminColumnsParamRenderer(ePref pref, eAdminTableInfos tabInfos, String title, String titleInfo, string[] openedBlocks = null)
        {
            eAdminColumnsParamRenderer features = new eAdminColumnsParamRenderer(pref, tabInfos, title, titleInfo, openedBlocks);
            return features;
        }

        /// <summary>Construction du bloc Caractéristiques</summary>
        /// <returns></returns>
        protected override bool Build()
        {
            this.OpenedBlock = true;

            base.Build();

            // Colonnes
            eAdminField adminField = new eAdminTextboxField(_tabInfos.DescId, eResApp.GetResWithColon(Pref, 769), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.NBCOLS.GetHashCode(), AdminFieldType.ADM_TYPE_NUM, eResApp.GetRes(_ePref, 1840), _tabInfos.ColPerLine.ToString());
            adminField.SetFieldControlID("admNbCols");

            adminField.Generate(_panelContent);
            adminField.SetControlAttribute("erngmin", "1");
            adminField.SetControlAttribute("erngmax", "10");
            return true;
        }
    }
}