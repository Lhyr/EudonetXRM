using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{


    /// <summary>
    /// Classe de rendu pour les fiche de type relation
    /// </summary>
    public class eAdminWebTabFileRenderer : eAdminTemplateFileRenderer
    {



        private eAdminWebTabFileRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos)
        {
        }


        /// <summary>
        /// Création d'une instance de eAdminRelationFileRenderer
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tab"></param>
        /// <returns></returns>
        public static eAdminWebTabFileRenderer CreateAdminWebTabFileRenderer(ePref pref, eAdminTableInfos tabInfos)
        {

            eAdminWebTabFileRenderer adminFileRenderer = new eAdminWebTabFileRenderer(pref, tabInfos);
            return adminFileRenderer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            return true;
        }

        /// <summary>
        /// Construit le sous menu
        /// </summary>
        /// <param name="htmlGenericControl"></param>
        protected override Boolean Build()
        {
            _backBoneRdr = GetFileBackBone();
            this._pgContainer = _backBoneRdr.PgContainer;
            this._pgContainer.Attributes.Add("edntype", ((int)EdnType.FILE_GRID).ToString());
            AddAdminDropWebTab();

            return true;
        }
        protected override eAdminWebTabNavBarRenderer GetAdminWebTabNavBar()
        {
            return eAdminWebTabNavBarForWebTabFileRenderer.GetAdminWebTabNavBarForWebTabRenderer(Pref, _tab, 0,
                (_tabInfos.TabType == TableType.TEMPLATE) ? eXrmWidgetContext.eGridLocation.Bkm : eXrmWidgetContext.eGridLocation.Default);
        }


        /// <summary>
        /// Ne pas ajouter le champ mémo
        /// </summary>
        protected override void AddMemoField() { }

    }
}