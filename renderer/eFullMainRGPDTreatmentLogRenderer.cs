using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eFullMainRGPDTreatmentLogRenderer : eFullMainListRenderer
    {
        Panel _pnlListGroups;


        #region constructeur

        /// <summary>
        /// Rendu du mode liste complete
        /// -&gt; liste "pleine page" avec filtre et co
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <param name="nPage">The n page.</param>
        /// <param name="nRow">The n row.</param>
        /// <param name="tab">The tab.</param>
        /// <exception cref="EudoAdminInvalidRightException"></exception>
        private eFullMainRGPDTreatmentLogRenderer(ePref pref, Int32 height, Int32 width, Int32 nPage, Int32 nRow, TableLite tab) : base(pref, tab.DescId, height, width, nPage, nRow)
        {

            if (pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                throw new EudoAdminInvalidRightException();

        }



        /// <summary>
        /// Instancie et retourne un eFullMainListUserRenderer
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <param name="nPage">The n page.</param>
        /// <param name="nRow">The n row.</param>
        /// <param name="tab">The tab.</param>
        /// <returns></returns>
        internal static eFullMainRGPDTreatmentLogRenderer GetFullMainListRenderer(ePref pref, Int32 height, Int32 width, Int32 nPage, Int32 nRow, TableLite tab)
        {
            eFullMainRGPDTreatmentLogRenderer eFullListRdr = new eFullMainRGPDTreatmentLogRenderer(pref, height, width, nPage, nRow, tab);
            eFullListRdr._ednTab = tab;
            return eFullListRdr;
        }

        #endregion

        #region Override
        protected override void BuildListContent(Panel pListHeader)
        {

            base.BuildListContent(pListHeader);
        }

        protected override void DoWebTab(HtmlGenericControl cpnl)
        {
            return;
        }

        /// <summary>
        /// Initialisation du renderer :
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {

            _bHideActionSelection = true;
            return base.Init();



        }

        protected override bool Build()
        {

            if (!base.Build())
                return false;


            _pnlTopMenuList.Attributes.Add("ednadmin", "1");


            //affecte la css sur listcontent
            Panel pListContent = (Panel)FindControlRecursive(_pnlMainList, "listContent");
            pListContent.CssClass = "listAdminRGPDTreatmentLog";
            pListContent.Width = new Unit();


            return true;
        }

        #endregion



        /// <summary>
        /// Construction des sous-blocs de contenu
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            DicContent = new Dictionary<string, Content>();
            DicContent["MenuPanel"] = new Content() { Ctrl = GetMenuPanel, CallBackScript = GetCallBackScript };
            DicContent["UserPanel"] = new Content() { Ctrl = GetListPanel, CallBackScript = "" }; ;
            return true;
        }
    }
}