using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Rendu du mode liste complet pour les users
    ///  -> Le mode liste complet désigne une liste en pleine page avec tous les blocs filtres, icone et co
    /// </summary>
    public class eFullMainListUserRenderer : eFullMainListRenderer
    {

        eUserGroupCatalogRenderer _eRdrGroup;


        Panel _pnlListGroups;


        #region constructeur

        /// <summary>
        /// Rendu du mode liste complet pour les users
        ///  -> liste "pleine page" avec filtre et co
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="nPage"></param>
        /// <param name="nRow"></param>
        /// <param name="Type"></param>
        private eFullMainListUserRenderer(ePref pref, Int32 nTab, Int32 height, Int32 width, Int32 nPage, Int32 nRow) : base(pref, nTab, height, width, nPage, nRow)
        {
            if (pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                throw new EudoAdminInvalidRightException();

        }


        protected override void SetPagingTableWith(System.Web.UI.WebControls.Table tbListAction)
        {
            //ALISTER Demande #64 862
            tbListAction.Style.Add("width", _ePref.GroupMode == SECURITY_GROUP.GROUP_NONE && _ePref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN ? "100%" : "75%");
        }



        /// <summary>
        /// Instancie et retourne un eFullMainListUserRenderer
        /// </summary>
        /// <param name="pref"></param>
        
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="nPage"></param>
        /// <param name="nRow"></param>
        /// <param name="tab"></param>
        /// <returns></returns>
        internal static eFullMainListUserRenderer GetFullMainListUserRenderer(ePref pref,   Int32 height, Int32 width, Int32 nPage, Int32 nRow, TableLite tab)
        {
            eFullMainListUserRenderer eFullListRdr = new eFullMainListUserRenderer(pref, tab.DescId, height, width, nPage, nRow);
            eFullListRdr._ednTab = tab;
            return eFullListRdr;
        }

        #endregion

        #region Overide
        protected override void BuildListContent(Panel pListHeader)
        {

            base.BuildListContent(pListHeader);




        }


        /// <summary>
        /// Initialisation du renderer :
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            /*
            var zz = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO, Pref);
            bool bEudoSyncEnabled = zz?.Infos.IsEnabled ?? false;
            */
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
            pListContent.CssClass = "listAdminUser";
            pListContent.Width = new Unit();
           
            //ALISTER Demande #64 862
            if (_ePref.GroupMode == SECURITY_GROUP.GROUP_NONE && _ePref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN)
            {
                pListContent.CssClass = String.Concat(pListContent.CssClass, " listAdminUserHidden");
                
            }

            //Construction du panel group
            _pnlListGroups = new Panel();
            _pnlListGroups.ID = "listGroup";
            _pnlListGroups.Width = new Unit("25%");

            // a placer entre le  menu et la liste
            _pgContainer.Controls.AddAt(1, _pnlListGroups);

            //
            _pnlListGroups.CssClass = "listAdminGroup";




            _pnlListGroups.Controls.Add(BuildGroupPanel());



            return true;
        }

        #endregion

        private Panel BuildGroupPanel()
        {

            CatalogParamUserGroup p = new Xrm.CatalogParamUserGroup()
            {
                Multiple = true,
                ShowGroupOnly = true,
                UseGroup = true,
                Admin = Pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN,
                WidthSearchBox = 60,
                FullUserList = true,
                ShowRootGroup = true
            };

            _eRdrGroup = eUserGroupCatalogRenderer.GetGroupArboRenderer(Pref, 1, -1, p, "", new List<string>(), new eFileTools.eParentFileId(), false);

            if (_eRdrGroup.ErrorMsg.Length > 0)
                throw _eRdrGroup.InnerException ?? new Exception(_eRdrGroup.ErrorMsg);


            return _eRdrGroup.PgContainer;

        }


        /// <summary>
        /// Retourne le panel de la liste des groupes
        /// </summary>
        public Panel GetGroupPanel
        {
            get
            {
                return _pnlListGroups;

            }

        }


        /// <summary>
        /// Construction des sous-blocs de contenu
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {

            DicContent = new Dictionary<string, Content>();
            DicContent["MenuPanel"] = new Content() { Ctrl = GetMenuPanel, CallBackScript = GetCallBackScript };
            DicContent["GroupPanel"] = new Content() { Ctrl = GetGroupPanel, CallBackScript = _eRdrGroup?.GetCallBackScript }; ;
            DicContent["UserPanel"] = new Content() { Ctrl = GetListPanel, CallBackScript = "" }; ;
            return true;
        }

    }
}