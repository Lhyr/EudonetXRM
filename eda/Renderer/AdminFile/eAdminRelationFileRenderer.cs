using Com.Eudonet.Internal.eda;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{


    /// <summary>
    /// Classe de rendu pour les fiche de type relation
    /// </summary>
    public class eAdminRelationFileRenderer : eAdminTemplateFileRenderer
    {

      

        private eAdminRelationFileRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos)
        {



        }


        /// <summary>
        /// Création d'une instance de eAdminRelationFileRenderer
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tab"></param>
        /// <returns></returns>
        public static eAdminRelationFileRenderer CreateAdminRelationFileRenderer(ePref pref, eAdminTableInfos tabInfo)
        {

            eAdminRelationFileRenderer adminFileRenderer = new eAdminRelationFileRenderer(pref, tabInfo);         
            return adminFileRenderer;
        }


        /// <summary>
        /// Pas de header
        /// </summary>
        /// <param name="bHasButtons"></param>
        /// <returns></returns>
        protected override System.Web.UI.WebControls.Table GetHeader(bool bHasButtons = true)
        {
            return new System.Web.UI.WebControls.Table();
        }

        /// <summary>
        /// Ne pas ajouter le champ mémo
        /// </summary>
        protected override void AddMemoField() { }

    }
}