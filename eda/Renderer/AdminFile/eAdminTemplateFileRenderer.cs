using Com.Eudonet.Internal.eda;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminTemplateFileRenderer : eAdminFileRenderer
    {
      
        public eAdminTemplateFileRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos) {}
                       

        /// <summary>
        /// Pas de Drag and Drop 
        /// </summary>
        protected override void AddDragAndDropAttributes() { }

    }
}