using Com.Eudonet.Internal.eda;
using EudoQuery;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer de fichier de type E-mail pour l'admin
    /// </summary>
    public class eAdminMailFileRenderer : eAdminTemplateFileRenderer
    {     
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tabInfos"></param>
        public eAdminMailFileRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos) {}


        /// <summary>
        /// Tous les champs de table système Mail ne sont pas supprimable
        /// </summary>
        /// <param name="ul"></param>
        protected override void RenderDeleteOption(HtmlGenericControl ul, Field field)  {}           

    }
}