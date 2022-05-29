using Com.Eudonet.Internal.eda;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminAddressFileRenderer : eAdminFileRenderer
    {
        public eAdminAddressFileRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos)
        {
        }


        /// <summary>
        /// Pas de Drag and Drop 
        /// </summary>
        //protected override void AddDragAndDropAttributes() { }
        //temporaire
        protected override void GetBookMarkBlock()
        {
            base.GetBookMarkBlock();
        }


    }
}