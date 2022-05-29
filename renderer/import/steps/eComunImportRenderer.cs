using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Cache;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.Import;
using Com.Eudonet.Internal.wcfs.data.import;
using EudoQuery;

namespace Com.Eudonet.Xrm.import
{
    /// <summary>
    /// Class qui gère les informations de la table principale d'import et les tables liées
    /// </summary>
    public class eComunImportRenderer : IWizardStepRenderer
    {
        #region Propriétées
        /// <summary>
        /// Préférences utilisateur
        /// </summary>
        protected ePref _pref;
        private eImportWizardParam _wizardParam;
        private eImportSourceInfosCallReturn _result;
        /// <summary>
        /// Liste des Descid des tables liées à la tables principale
        /// </summary>
        protected IEnumerable<int> ListTab { get { return this._list.Select(s => s.Info.Table.DescId); } }

        /// <summary>
        /// Liste des Descid des tables liées à la table principale mais non affichables
        /// </summary>
        private ICollection<int> _excludedTab { get; set; }
        /// <summary>
        /// Liste des Descid des tables liées à la table principale mais non affichables
        /// </summary>
        protected ICollection<int> ExcludedTab
        {
            get { return this._excludedTab; }
            private set { _excludedTab = value; }
        }
        private List<ImportInfo> _list { get; set; }
        /// <summary>
        /// Informations d'import pour toutes les tables liées à la table principale
        /// </summary>
        protected List<ImportInfo> ListImportInfo
        {
            get { return this._list; }
            set { _list = value; }
        }

        /// <summary>
        /// Liste des tables pour lesquelles aucune action n'est possible => mise à jour/création
        /// </summary>
        protected List<string> BlamedTab { get; set; } = new List<string>();

        /// <summary>
        /// Paramètres d'import de la table principale
        /// </summary>
        protected ImportInfo MainTabImportInfo
        {
            get
            {
                if (this._list != null && this._list.Count() > 0 && _wizardParam != null && _wizardParam.ImportTab > 0)
                {
                    IEnumerable<ImportInfo> lst = this._list.Where(s => s.Info.Table.DescId.Equals(_wizardParam.ImportTab));
                    if (lst.Count() > 0)
                        return lst.First();
                    else
                        return new ImportInfo();
                }
                else
                    return new ImportInfo();
            }
        }

        private eTableLiteWithLib _mainTabLite { get; set; }

        private CacheImportInfo _mainTab { get; set; }
        /// <summary>
        /// Retourne la table main
        /// </summary>
        protected CacheImportInfo MainTab
        {
            get { return this._mainTab; }
        }


        private CacheImportInfo _parentTab { get; set; }
        /// <summary>
        /// Retourne la table parente de la table main en mode signet
        /// </summary>
        protected CacheImportInfo ParentTab
        {
            get { return this._parentTab; }
        }
        private bool _bSpecialTab { get; set; }

        /// <summary>
        /// Résultat d'appel à eudoprocess
        /// </summary>
        protected eImportSourceInfosCallReturn ResulWWcf
        {
            get { return this._result; }
        }

        #endregion

        #region accesseur

        /// <summary>
        /// 
        /// </summary>
        public ePref Pref
        {
            get { return _pref; }
        }

        /// <summary>
        /// 
        /// </summary>
        public eImportWizardParam WizardParam
        {
            get { return _wizardParam; }
        }

        /// <summary>
        /// Si la Main table est PP,PM ou ADDR
        /// </summary>
        protected bool IsSpecialTab
        {
            get { return _bSpecialTab; }
        }



        #endregion


        #region Constructeur
        /// <summary>
        /// Constructeur 
        /// </summary>
        /// <param name="pref">Préférences utilisateur</param>
        /// <param name="wizardParam">Paramètres de l'import</param>
        public eComunImportRenderer(ePref pref, eImportWizardParam wizardParam)
        {
            this._pref = pref;
            this._wizardParam = wizardParam;
            this._mainTabLite = this.GetTabLibelle();
            this._bSpecialTab = (_mainTabLite.TabType == TableType.ADR || _mainTabLite.TabType == TableType.PM || _mainTabLite.TabType == TableType.PP);
            this._list = this.GetTabImportInfos();
            if (wizardParam != null && wizardParam.ImportTab > 0)
                SetTabsInfos();
        }

        /// <summary>
        /// Constructeur 
        /// </summary>
        /// <param name="pref">Préférences utilisateur</param>
        /// <param name="wizardParam">Paramètres de l'import</param>
        /// <param name="resultCalWcf">Résultat de l'appel à eudoprocess</param>
        public eComunImportRenderer(ePref pref, eImportWizardParam wizardParam, eImportSourceInfosCallReturn resultCalWcf)
            : this(pref, wizardParam)
        {
            this._result = resultCalWcf;
        }

        /// <summary>
        /// Retourne table main et table parente si elle existe
        /// </summary>
        private void SetTabsInfos()
        {
            if (WizardParam != null)
            {
                EudoImportCache cache = new EudoImportCache(_pref);
                using (eudoDAL dal = eLibTools.GetEudoDAL(_pref))
                {
                    dal.OpenDatabase();
                    cache.SetDal = dal;
                    if (WizardParam.ImportTab > 0)
                        this._mainTab = cache.Get(WizardParam.ImportTab);
                    if (WizardParam.ParentTab > 0)
                        this._parentTab = cache.Get(WizardParam.ParentTab);

                    dal?.CloseDatabase();
                }

                SetExcludedTab();
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected bool IsPPLinkFromParent()
        {

            return MainTab != null
                    && MainTab.Table != null
                    && WizardParam != null
                    && WizardParam.ParentTab != 0
                    && MainTab.Table.InterPP
                    && (int)TableType.PP != WizardParam.ParentTab
                    && (int)TableType.ADR != WizardParam.ImportTab
                    && ParentTab.Table != null
                    && ParentTab.Table.InterPP
                    && !MainTab.Table.NoDefaultLink200;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected bool IsPMLinkFromParent()
        {
            return MainTab != null
                    && MainTab.Table != null
                    && WizardParam != null
                    && WizardParam.ParentTab != 0
                    && MainTab.Table.InterPM
                    && (int)TableType.PM != WizardParam.ParentTab
                    && ParentTab.Table != null && ParentTab.Table.InterPM
                    && !MainTab.Table.NoDefaultLink300;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected bool AddAdrTab() { return IsPPLinkFromParent() && IsPMLinkFromParent(); }


        /// <summary>
        /// Indique si on a mappé la table PM
        /// </summary>
        /// <returns></returns>
        protected bool BpmMapped()
        {
            return WizardParam.ImportParams.Tables.Where(t => t.TabInfo.TabDescId == (int)TableType.PP && t.TabInfo.TabInfoId.Equals(t.TabInfo.TabDescId.ToString()) && t.Mapp.Count() > 0).Count() > 0;
        }

        /// <summary>
        /// Indique si on a mappé la table PP
        /// </summary>
        /// <returns></returns>
        protected bool BppMapped()
        {
            return WizardParam.ImportParams.Tables.Where(t => t.TabInfo.TabDescId == (int)TableType.PM && t.TabInfo.TabInfoId.Equals(t.TabInfo.TabDescId.ToString()) && t.Mapp.Count() > 0).Count() > 0;
        }


        /// <summary>
        /// Indique si on doit ajouter la table Adresse à l'import
        /// </summary>
        /// <returns></returns>
        protected bool BaddAdrTAb()
        {
            //BSE:#66 310 + #67 965 => Aficher l'onglet adresse si 
            //On a mappé adresse
            //On a mappé ou pas adresse et: On hérite du PP ou PM ou les 2 de la table parente 
            //On a mappé ou pas adresse et: On hérite ni de PP ni de PM de la table parente et on a mappé PP et PM liés au signet
            return
                ((IsPMLinkFromParent() || IsPPLinkFromParent()) && (BppMapped() || BpmMapped())
                ||
                (!IsPMLinkFromParent() && !IsPPLinkFromParent() && BppMapped() && BpmMapped()));
        }
        #endregion

        /// <summary>
        /// Appel la méthode init() de base
        /// </summary>
        /// <returns>Retourne un objet eComunImportRenderer</returns>
        public virtual IWizardStepRenderer Init()
        {
            return this;
        }

        /// <summary>
        /// Rendu d'une étape d'import
        /// </summary>
        /// <returns></returns>
        public virtual Panel Render()
        {
            return new Panel();
        }


        /// <summary>
        /// Retourne un htmlGenericControl
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="cssName"></param>
        /// <returns></returns>
        public virtual HtmlGenericControl GetOptionsLine(Cache.ImportInfo tab, string cssName)
        {
            return new HtmlGenericControl();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tab"></param>
        /// <returns></returns>
        protected HtmlGenericControl GetOptionsTabLine(Cache.ImportInfo tab)
        {
            eFontIcons.FontIcons font = eFontIcons.GetFontIcon(tab.Info.Table.GetIcon);
            return GetOptionsLine(tab, font.CssName);
        }


        /// <summary>
        /// Retourne un contenu html avec un message 
        /// </summary>
        /// <param name="message">Message à afficher</param>
        /// <returns></returns>
        protected HtmlGenericControl GetEmptyTabLine(string message)
        {
            HtmlGenericControl headerTab = new HtmlGenericControl("div");
            headerTab.Attributes.Add("class", "headerOptionsLine");
            headerTab.Style.Add(HtmlTextWriterStyle.Color, Pref.ThemeXRM.Color);
            headerTab.InnerHtml = message;
            return headerTab;
        }


        /// <summary>
        /// Retourne une liste de tables liées
        /// </summary>
        /// <returns></returns>
        protected ISet<int> GetLinkedTableListe()
        {
            string sql = string.Empty;
            string error = String.Empty;
            ISet<int> listTab = new HashSet<int>();

            if (!_bSpecialTab)
            {
                //listTab.Add(_wizardParam.ImportTab);


                switch (_mainTabLite.TabType)
                {
                    case TableType.EVENT:
                    case TableType.ADR:
                        // Table de départ EVENT
                        sql = string.Concat("select res.", _pref.Lang,
                            " as Libelle, RelationFileDescId from cfc_getLiaison(",
                            _wizardParam.ImportTab
                            , ") left join res on RelationFileDescId = res.resid ",
                            " left join [desc] on RelationFileDescId =[desc].descid ",
                            " left join(SELECT PERMISSIONID, P FROM[DBO].[cfc_getPermInfo](", Pref.UserId, ",", Pref.User.UserLevel, ",", Pref.User.UserGroupId, "))viewp on viewp.permissionid = ViewPermId",
                            " left join(SELECT PERMISSIONID, P FROM [DBO].[cfc_getPermInfo](", Pref.UserId, ",", Pref.User.UserLevel, ",", Pref.User.UserGroupId, ")) updatep on updatep.permissionid = UpdatePermId ",
                            "where isnull(viewp.P,1) = 1 And isnull(isrelation,0) = 1 ")
                                            //, " UNION "
                                            //, "select res.", _pref.Lang, " as Libelle, RelationFileDescId from cfc_getLiaison(", _wizardParam.ImportTab, ") left join res on RelationFileDescId = res.resid where isnull(isrelation,0) = 1 order by res.", _pref.Lang)
                                            ;
                        break;
                    default:
                        // Table de départ : Template
                        // La liaison doit être sur la table de départ
                        sql = string.Concat("select res.", _pref.Lang,
                            " as Libelle, RelationFileDescId from cfc_getLiaison(",
                            _wizardParam.ImportTab
                            , ") left join res on RelationFileDescId = res.resid ",
                            " left join [desc] on RelationFileDescId =[desc].descid ",
                            " left join(SELECT PERMISSIONID, P FROM[DBO].[cfc_getPermInfo](", Pref.UserId, ",", Pref.User.UserLevel, ",", Pref.User.UserGroupId, "))viewp on viewp.permissionid = ViewPermId",
                            " left join(SELECT PERMISSIONID, P FROM [DBO].[cfc_getPermInfo](", Pref.UserId, ",", Pref.User.UserLevel, ",", Pref.User.UserGroupId, ")) updatep on updatep.permissionid = UpdatePermId ",
                            "where isnull(viewp.P,1) = 1 And isnull(isrelation,0) = 1 ");
                        break;
                }




                DataTableReaderTuned dtr = null;
                eudoDAL dal = eLibTools.GetEudoDAL(_pref);
                if (!String.IsNullOrEmpty(sql))
                {
                    try
                    {
                        RqParam rq = new RqParam(sql);
                        dal.OpenDatabase();
                        dtr = dal.Execute(rq, out error);
                        if (!string.IsNullOrEmpty(error))
                            throw new Exception(String.Concat("FillMainFileList : ", error));

                        if (dtr != null)
                        {
                            while (dtr.Read())
                            {
                                int tabDescId = eLibTools.GetNum(dtr.GetString("RelationFileDescId"));
                                if (tabDescId != 0)
                                    listTab.Add(tabDescId);
                            }
                        }

                        Dictionary<int, bool> dicPerm = new Dictionary<int, bool>();


                        ePermission.LoadPermInfo(dal, dicPerm, Pref.User.UserId, Pref.User.UserLevel, Pref.User.UserGroupId);

                    }
                    finally
                    {
                        dtr?.Dispose();
                        if (dal.IsOpen)
                            dal?.CloseDatabase();
                    }
                }

            }

            if (!listTab.Contains((int)TableType.PP))
                listTab.Add((int)TableType.PP);
            if (!listTab.Contains((int)TableType.PM))
                listTab.Add((int)TableType.PM);
            if (!listTab.Contains((int)TableType.ADR) && _bSpecialTab)
                listTab.Add((int)TableType.ADR);


            return listTab;

        }


        /// <summary>
        /// Retourne une liste d'importInfo de toutes les tables liées à la table principale
        /// </summary>
        /// <returns></returns>
        protected List<ImportInfo> GetTabImportInfos()
        {
            return eLibTools.GetTabImportInfos(Pref, _wizardParam.ParentTab, _wizardParam.ImportTab, _bSpecialTab);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="txtHeader"></param>
        /// <returns></returns>
        protected HtmlGenericControl GetHeaderTabLine(string txtHeader)
        {
            HtmlGenericControl header = new HtmlGenericControl("div");
            header.Attributes.Add("class", "headerClass");
            header.InnerHtml = txtHeader;
            return header;
        }

        /// <summary>
        /// Retourne le libélle d'une table
        /// </summary>
        /// <returns></returns>
        private eTableLiteWithLib GetTabLibelle()
        {
            return new eTableLiteWithLib(_wizardParam.ImportTab, _pref.Lang);
        }

        /// <summary>
        /// Retourne objet de importtemplate
        /// </summary>
        protected ImportTemplateWizard GetImportTemplate(int idTemplate)
        {
            ImportTemplateWizard template = null;
            if (!WizardParam.ImportTemplateParams.IsNotSavedImportTemplate)
            {
                template = ImportTemplateWizard.GetImportTemplate(Pref, idTemplate);
            }

            return template;
        }

        /// <summary>
        /// Retourne le nom du modèle d'import
        /// </summary>
        /// <param name="container">Composant HTML</param>
        protected virtual void GetTemplateName(HtmlGenericControl container)
        {
            if (!WizardParam.ImportTemplateParams.IsNotSavedImportTemplate)
            {
                ImportTemplateWizard template = ImportTemplateWizard.GetImportTemplate(Pref, WizardParam.ImportTemplateParams.ImportTemplateId);
                HtmlGenericControl modelName = new HtmlGenericControl("div");
                modelName.InnerHtml = template != null ? template.ImportTemplateLine.ImportTemplateName : eResApp.GetRes(Pref, 1111);
                modelName.Attributes.Add("class", "resume_import_template_name");
                container.Controls.Add(modelName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void SetExcludedTab()
        {
            this._excludedTab = new List<int>();
            if (WizardParam.ParentTab != 0)
            {
                foreach (Cache.ImportInfo tab in this.ListImportInfo)
                {
                    if (IsPPLinkFromParent() && tab.Info.Table.DescId == (int)TableType.PP && tab.GetJsKey().Equals(((int)TableType.PP).ToString()))
                    {
                        this._excludedTab.Add((int)TableType.PP);
                        tab.HideTab = true;
                    }


                    if (IsPMLinkFromParent() && tab.Info.Table.DescId == (int)TableType.PM && tab.GetJsKey().Equals(((int)TableType.PM).ToString()))
                    {
                        this._excludedTab.Add((int)TableType.PM);
                        tab.HideTab = true;
                    }


                    if (WizardParam.ParentTab != 0 && AddAdrTab() && tab.Info.Table.DescId == (int)TableType.ADR)
                    {
                        this._excludedTab.Add((int)TableType.ADR);
                        tab.HideTab = true;
                    }

                }
            }
        }
    }
}