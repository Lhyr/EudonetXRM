using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Propriété des onglets/signet web
    /// </summary>
    public class eWebTabManager : eEudoManager
    {

        /// <summary>
        /// type d'action pour le manager
        /// </summary>
        public enum WebTabManagerAction
        {
            /// <summary>action indéfini </summary>
            UNDEFINED = 0,
            /// <summary>Rendu html de la table d'édition des propriétés d'un sigent web </summary>
            GETINFOS = 1,

            /// <summary>MaJ des informations d'un onglet web </summary>
            UPDATEINFO = 2,

            /// <summary>Création d'un onglet web </summary>
            CREATE = 3,

            /// <summary>Suppression d'un onglet web </summary>
            DELETE = 4,

            /// <summary>
            /// Retourne la navbar des spécif d'onglet
            /// </summary>
            GETNAVBAR = 5,
            /// <summary>
            /// Deplace un webtab
            /// </summary>
            MOVETAB = 6,
            /// <summary>
            /// Deplace un webtab grille
            /// </summary>
            MOVEGRID = 7,
            /// <summary>
            /// Retourne la navbar des sous onglet web
            /// </summary>
            GETSUBTAB = 8,
            /// <summary>
            /// Deplace le webtab Liste des fiches
            /// </summary>
            MOVETABLIST = 9
        }


        /// <summary>
        /// table du signet web
        /// </summary>
        private int _nTab = 0;

        /// <summary>
        /// attention n'est différent de EdnType.FILE_MAIN que pour WEBTAB
        /// </summary>
        private EdnType _ednType = EdnType.FILE_MAIN;

        /// <summary>
        /// spécif id
        /// </summary>
        private int _nSpecifId = 0;


        private WebTabManagerAction action = WebTabManagerAction.UNDEFINED;

        /// <summary>
        /// Gestion de l'action a effectuer
        /// </summary>
        protected override void ProcessManager()
        {

            try
            {
                //Action - paramètre obligatoire
                if (_requestTools.AllKeys.Contains("action") && !String.IsNullOrEmpty(_context.Request.Form["action"]))
                {
                    if (!Enum.TryParse(_context.Request.Form["action"], out action))
                        action = WebTabManagerAction.UNDEFINED;
                }


                //table
                if (_requestTools.AllKeys.Contains("tab") && !String.IsNullOrEmpty(_context.Request.Form["tab"]))
                    Int32.TryParse(_context.Request.Form["tab"], out _nTab);

                if (_requestTools.AllKeys.Contains("ednType"))
                    _ednType = _requestTools.GetRequestFormEnum<EdnType>("ednType");

                if (_nTab == 0)
                    throw new EudoException("Le paramètre 'tab' est obligatoire.");

                int iPageId = 0;
                if (_nTab == (int)TableType.XRMHOMEPAGE)
                    iPageId = _requestTools.GetRequestFormKeyI("pageid") ?? 0;

                switch (action)
                {
                    case WebTabManagerAction.UNDEFINED:
                        throw new EudoException("Aucune action définie.");
                    case WebTabManagerAction.GETINFOS:

                        if (_requestTools.AllKeys.Contains("id") && !String.IsNullOrEmpty(_context.Request.Form["id"]))
                            Int32.TryParse(_context.Request.Form["id"], out _nSpecifId);


                        //Spécif obligatoire
                        if (_nSpecifId == 0)
                            throw new EudoException("Paramètres non renseignés.");

                        //création du rendu
                        eAdminRenderer rdr = eAdminRendererFactory.CreateAdminWebTabParameterRenderer(_pref, this._nTab, _nSpecifId);
                        RenderResultHTML(rdr.PgContainer);

                        break;
                    case WebTabManagerAction.UPDATEINFO:
                      
                        RenderWebTabNavBar(this._nTab, iPageId);

                        break;
                    case WebTabManagerAction.MOVETAB:

                        #region parameters
                        int nMoveTo = 0;
                      
                        if (_requestTools.AllKeys.Contains("moveto") && !String.IsNullOrEmpty(_context.Request.Form["moveto"]))
                        {
                            Int32.TryParse(_context.Request.Form["moveto"], out nMoveTo);
                        }



                        if (_requestTools.AllKeys.Contains("id") && !String.IsNullOrEmpty(_context.Request.Form["id"]))
                            Int32.TryParse(_context.Request.Form["id"], out _nSpecifId);


                        if (_nSpecifId == 0)
                        {
                            ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", "Paramètres non renseignés.");
                            LaunchError();
                        }

                        #endregion


                        #region MAJ

                        eSpecif.MajDisporder(_pref, this._nTab, _nSpecifId, nMoveTo, false);

                        #endregion

                        RenderWebTabNavBar(this._nTab, iPageId);

                        break;
                    case WebTabManagerAction.MOVEGRID:

                        string sError = "";
                        int iOldDisporder = _requestTools.GetRequestFormKeyI("oldDO") ?? 0;
                        int iNewDisporder = _requestTools.GetRequestFormKeyI("newDO") ?? 0;
                        int iGridId = _requestTools.GetRequestFormKeyI("gridid") ?? 0;                       
                        if (iOldDisporder == iNewDisporder)
                            return;

                        if (iGridId == 0 || iOldDisporder < 0 || iNewDisporder < 0)
                        {
                            throw new Exception(String.Format("Paramètres insuffisants : iGridId = {0}, iOldDisporder = {1}, iNewDisporder = {2}", iGridId, iOldDisporder, iNewDisporder));
                        }
                        eudoDAL dal = eLibTools.GetEudoDAL(_pref);
                        try
                        {
                            dal.OpenDatabase();

                            RqParam rqMove = eSqlXrmGrid.GetSwitchGridDisporderRqParam(_nTab, iGridId, iOldDisporder, iNewDisporder, iPageId);

                            dal.ExecuteNonQuery(rqMove, out sError);

                            if (sError.Length > 0)
                                throw new Exception(String.Concat("GetSwitchGridDisporderRqParam => ", rqMove.GetSqlCommandText()));
                           

                            RenderWebTabNavBar(_nTab, iPageId);
                        }
                        catch (eEndResponseException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 544), " ", String.Concat(e.Message, Environment.NewLine, e.StackTrace)));
                        }
                        finally
                        {
                            dal?.CloseDatabase();
                        }

                        break;
                    case WebTabManagerAction.CREATE:

                        int nPos = 1;
                        if (_requestTools.AllKeys.Contains("createat") && !String.IsNullOrEmpty(_context.Request.Form["createat"]))
                            Int32.TryParse(_context.Request.Form["createat"], out nPos);
                        else
                            throw new EudoException("Paramètre de position non renseigné.");

                        eLibConst.SPECIF_TYPE spectType = _requestTools.GetRequestFormEnum<eLibConst.SPECIF_TYPE>("type");

                        if (!(spectType == eLibConst.SPECIF_TYPE.TYP_WEBTAB_INTERNAL || spectType == eLibConst.SPECIF_TYPE.TYP_WEBTAB_EXTERNAL))
                            throw new EudoException("Type d'onglet web invalide");

                        SpecifTreatmentResult result = eSpecif.CreateWebTab(_pref, this._nTab, nPos, spectType);

                        //retourne le flux json de l'objet de retour
                        RenderResult(RequestContentType.TEXT, delegate () { return JsonConvert.SerializeObject(result); });

                        break;
                    case WebTabManagerAction.DELETE:

                        _nSpecifId = _requestTools.GetRequestFormKeyI("id") ?? 0;

                        //Spécif obligatoire
                        if (_nSpecifId == 0) { throw new EudoException("Paramètres non renseignés."); }

                        SpecifTreatmentResult deleteResult = eSpecif.DeleteSpecif(_pref, this._nTab, _nSpecifId);

                        //retourne le flux json de l'objet de retour
                        RenderResult(RequestContentType.TEXT, delegate () { return JsonConvert.SerializeObject(deleteResult); });

                        break;
                    case WebTabManagerAction.GETNAVBAR:
                        RenderWebTabNavBar(this._nTab, iPageId);
                        break;
                    case WebTabManagerAction.GETSUBTAB:

                        Panel _pnlTopMenuList = new Panel();
                        Panel PgContainer = new Panel();
                        PgContainer.Controls.Add(_pnlTopMenuList);
                        _pnlTopMenuList.ID = "mainListContent";


                        Panel _pnlMainList = new Panel();
                        _pnlMainList = new Panel();
                        PgContainer.Controls.Add(_pnlMainList);
                        _pnlMainList.ID = "listheader";
                        _pnlMainList.CssClass = "listheader";

                        //Ajout de la fontsize 
                        String sUserFontSize = eTools.GetUserFontSize(_pref);
                        _pnlMainList.CssClass += " fs_" + sUserFontSize + "pt";


                        Panel pInfos = new Panel();

                        //Div Infos
                        _pnlTopMenuList.Controls.Add(pInfos);
                        pInfos.ID = "infos";

                        HtmlGenericControl pSubTabMenuCtnr = new HtmlGenericControl("div");
                        pSubTabMenuCtnr.ID = "SubTabMenuCtnr";
                        pSubTabMenuCtnr.Attributes.Add("class", "subTabDiv");
                        pSubTabMenuCtnr.Attributes.Add("tab", _nTab.ToString());

                        pInfos.Controls.Add(pSubTabMenuCtnr);
                        //Construction du sous menu
                        DoSubMenuTab(pSubTabMenuCtnr, _nTab, _ednType.GetHashCode());

                        Panel toolbarCtn = new Panel();
                        toolbarCtn.CssClass = "grid-toolbar-ctn";
                        toolbarCtn.Attributes.Add("toolbar", "0");
                        toolbarCtn.Controls.Add(eWebTabRenderer.BuildGridToolBar());
                        pInfos.Controls.Add(toolbarCtn);

                        //Retour au format HTML => On s'en servira pour determiner si on charge la liste ou une grille
                        RenderResultHTML(PgContainer);
                        break;
                    default:
                        throw new NotImplementedException(String.Concat("action non reconnue : ", action.ToString()));

                }


            }
            catch (eEndResponseException) { _context.Response.End(); }
            catch (ThreadAbortException)
            {

            }
            catch (EudoException eudoExp)
            {


                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", eudoExp.Message);


                LaunchError();
            }
            catch (Exception e)
            {

#if DEBUG
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", e.Message);
#else
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", "Une erreur est survenue.");
#endif
                LaunchError();

            }



        }


        /// <summary>
        /// génère la navbar des webtab
        /// </summary>
        /// <param name="nTab">Table de la navvar</param>
        private void RenderWebTabNavBar(Int32 nTab, int fileId = 0)
        {

            eAdminWebTabNavBarRenderer rdrNavbar;
            if (_ednType == EdnType.FILE_WEBTAB || _ednType == EdnType.FILE_GRID)
                rdrNavbar = eAdminWebTabNavBarForWebTabFileRenderer.GetAdminWebTabNavBarForWebTabRenderer(_pref, nTab, fileId);
            else
                rdrNavbar = eAdminWebTabNavBarRenderer.GetAdminWebTabNavBarRenderer(_pref, nTab, fileId);

            RenderResultHTML(rdrNavbar.PgContainer);
        }

        private void DoSubMenuTab(HtmlGenericControl subMenuContainer, Int32 nTab, int nType)
        {
            // Dev 38096 demandes parante: onglet web
            var subMenuRenderer = new eWebTabSubMenuRenderer(_pref, nTab, nType, false);
            if (subMenuRenderer.Init())
            {
                if (subMenuRenderer.HasItems())
                {
                    subMenuRenderer.Build(subMenuContainer);
                }
            }
            else
            {
                subMenuRenderer.sError += "Impossible d'initialiser le renderer eWebTabSubMenuRenderer";
            }

            if (subMenuRenderer.sError.Length > 0)
            {
                //Avec exception
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", subMenuRenderer.sError, Environment.NewLine, "Exception StackTrace :", subMenuRenderer.innerException.StackTrace);

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg));
            }
        }

    }
}