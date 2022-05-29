using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer de l'administration des onglets
    /// </summary>
    public class eAdminAccessRenderer : eAdminModuleRenderer
    {
        Panel _pnlContents;
        HtmlGenericControl _pnlSubTitle;
        eUserOptionsModules.USROPT_MODULE _childModule;

        // Paramètres spécifiques au module Utilisateurs et groupes
        bool _bFullRenderer;
        private int _nPage = 1;
        private int _nRows = 1;



        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminAccessRenderer(ePref pref, eUserOptionsModules.USROPT_MODULE childModule, bool bFull, int nPage, int nRows, int nWidth, int nHeight)
            : base(pref)
        {
            Pref = pref;
            _width = nWidth - 30;
            _height = nHeight - 30;
            _childModule = childModule;
            _bFullRenderer = bFull;
            _nPage = nPage;
            _nRows = nRows;
        }


        public static eAdminAccessRenderer CreateAdminAccessRenderer(ePref pref, eUserOptionsModules.USROPT_MODULE childModule, bool bFull, int nPage, int nRows, int nWidth, int nHeight)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            return new eAdminAccessRenderer(pref, childModule, bFull, nPage, nRows, nWidth, nHeight);
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                return eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminAccess);
            }
            return false;
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {
            InitInnerContainer();

            GetChildModuleRendererContents();

            return true;
        }

        /// <summary>
        /// On instancie le container 
        /// </summary>
        private void InitInnerContainer()
        {
            // Pour un mode Liste, la liste (mainListContent) doit impérativement être ajouté au conteneur racine mainDiv
            // On n'ajoute donc pas de conteneur additionnel pour le module Utilisateurs et groupes
            if (
                _childModule != eUserOptionsModules.USROPT_MODULE.UNDEFINED &&
                _childModule != eUserOptionsModules.USROPT_MODULE.ADMIN_ACCESS &&
                _childModule != eUserOptionsModules.USROPT_MODULE.ADMIN_ACCESS_USERGROUPS
            )
            {
                _pnlContents = new Panel();
                _pnlContents.CssClass = "adminCntnt adminCntntAccess";
                _pnlContents.Style.Add(HtmlTextWriterStyle.Width, String.Concat(_width, "px"));
                _pnlContents.Style.Add(HtmlTextWriterStyle.Height, String.Concat(_height.ToString(), "px"));
                _pgContainer.Controls.Add(_pnlContents);
            }
        }

        private void GetChildModuleRendererContents()
        {
            eAdminRenderer rdr = null;
            Control contents = null;

            switch (_childModule)
            {
                // Page par défaut/première page : liste des utilisateurs et groupes
                case eUserOptionsModules.USROPT_MODULE.ADMIN_ACCESS:
                case eUserOptionsModules.USROPT_MODULE.ADMIN_ACCESS_USERGROUPS:
                case eUserOptionsModules.USROPT_MODULE.UNDEFINED:
                default:
                    // TODO: on ne renvoie que GetPanelUserList pour l'instant. Revenir sur cette partie lorsque les autres parties du renderer
                    // seront terminées et devront être éventuellement affichées ici
                    rdr = eAdminUsersListRenderer.CreateAdminUsersListRenderer(Pref, _bFullRenderer, _nPage, _nRows, _height, _width);
                    if (rdr.ErrorMsg.Length > 0 || rdr.InnerException != null)
                        throw rdr.InnerException ?? new Exception(rdr.ErrorMsg);

                    //All content (dans l'ordre 

                    DicContent = rdr.DicContent;


                    //Main Content
                    contents = (rdr != null ? ((eAdminUsersListRenderer)rdr).PgContainer : null);

                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_ACCESS_SECURITY:
                    rdr = eAdminAccessSecurityRenderer.CreateAdminAccessSecurityRenderer(Pref, _height, _width);
                    contents = (rdr != null ? ((eAdminAccessSecurityRenderer)rdr).GetContents() : null);

                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_ACCESS_PREF:
                    rdr = eAdminAccessPrefRenderer.CreateAdminAccessPrefRenderer(Pref, _height, _width);
                    contents = (rdr != null ? ((eAdminAccessPrefRenderer)rdr).GetContents() : null);
                    break;
            }

            if (rdr == null || contents == null)
            {
                _sErrorMsg = "Renderer de module admin non implémenté";
                _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT; // TODO/TOCHECK
                _eException = null;
            }

            else if (rdr.ErrorMsg.Length > 0)
            {
                _sErrorMsg = rdr.ErrorMsg;
                _nErrorNumber = rdr.ErrorNumber;
                _eException = rdr.InnerException;
            }
            else
            {
                AddCallBackScript(rdr.GetCallBackScript);

                switch (_childModule)
                {
                    // Page par défaut/première page : liste des utilisateurs et groupes
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_ACCESS:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_ACCESS_USERGROUPS:
                    case eUserOptionsModules.USROPT_MODULE.UNDEFINED:
                    default:
                        // pour un mode Liste, la liste (mainListContent) doit impérativement être ajouté au conteneur racine mainDiv
                        _pgContainer.Controls.Add(contents);



                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_ACCESS_SECURITY:
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_ACCESS_PREF:

                        _pnlContents.Controls.Add(contents);
                        DicContent["MainContent"] = new Xrm.eRenderer.Content()
                        {
                            Ctrl = contents,
                            CallBackScript = GetCallBackScript
                        };
                        break;
                }
            }
        }

        /// <summary>
        /// Construit un sous titre
        /// </summary>
        /// <param name="subTitle"></param>
        private void BuildSubTitle(String subTitle)
        {
            _pnlSubTitle = new HtmlGenericControl("div");
            _pnlSubTitle.Attributes.Add("class", "adminCntntTtl ");
            _pnlSubTitle.InnerText = subTitle;
            _pnlContents.Controls.Add(_pnlSubTitle);
        }
    }
}