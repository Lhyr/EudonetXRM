using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Description résumée de eHomepageManager
    /// GCH le 19/11/2012
    /// </summary>
    public class eHomepageManager : eEudoManager
    {



        /// <summary>Type d'Action demandée</summary>
        private string _sAction = string.Empty;

        Int32 height = eConst.DEFAULT_WINDOW_HEIGHT;
        Int32 width = eConst.DEFAULT_WINDOW_WIDTH;

        /// <summary>Gestion des actions asynchrones du champ de liaison (principalement : recherche MRU, affichage fenetre, recherche depuis fenêtre)</summary>
        protected override void ProcessManager()
        {

            string sEudoPartCustom = string.Empty;
            Boolean bReadOnly = false;

            #region old
            //if (_allKeys.Contains("action") && !string.IsNullOrEmpty(_context.Request.Form["action"]))
            //    _sAction = _context.Request.Form["action"];
            //if (_allKeys.Contains("readonly") && !string.IsNullOrEmpty(_context.Request.Form["readonly"]))
            //    bReadOnly = (_context.Request.Form["readonly"] == "1");
            //if (_allKeys.Contains("divW") && !string.IsNullOrEmpty(_context.Request.Form["divW"]))
            //    Int32.TryParse(_context.Request.Form["divW"].ToString(), out width);
            //if (_allKeys.Contains("divH") && !string.IsNullOrEmpty(_context.Request.Form["divH"]))
            //    Int32.TryParse(_context.Request.Form["divH"].ToString(), out height);
            //if (_allKeys.Contains("EudoPartCustom") && !string.IsNullOrEmpty(_context.Request.Form["EudoPartCustom"]))
            //    sEudoPartCustom = _context.Request.Form["EudoPartCustom"];
            #endregion

            _sAction = _requestTools.GetRequestFormKeyS("action");
            bReadOnly = _requestTools.GetRequestFormKeyB("readonly") ?? false;
            width = _requestTools.GetRequestFormKeyI("divW") ?? eConst.DEFAULT_WINDOW_WIDTH;
            height = _requestTools.GetRequestFormKeyI("divH") ?? eConst.DEFAULT_WINDOW_HEIGHT;
            sEudoPartCustom = _requestTools.GetRequestFormKeyS("EudoPartCustom");

            switch (_sAction)
            {
                case "homepage":

                    // La page d'accueil sera affecté au démarrage de l'appli                      
                    if (_pref.XrmHomePageId > 0)
                    {
                        var pageId = _requestTools.GetRequestFormKeyI("pageId") ?? 0;

                        bool onlySubMenu = _requestTools.GetRequestFormKeyS("onlysubmenu") == "1";
                        if (onlySubMenu)
                        {
                            eXrmHomePageSubMenuRenderer subMenuRenderer = new eXrmHomePageSubMenuRenderer(_pref, _pref.XrmHomePageId, (int)EdnType.FILE_GRID);
                            if (subMenuRenderer.Init())
                                RenderResultHTML(subMenuRenderer.BuildSubMenu());
                        }
                        else
                        {
                            eRenderer rend = eRendererFactory.CreateXrmHomePageRenderer(_pref, _pref.XrmHomePageId, width, height);
                            RenderResultHTML(rend.PgContainer);
                        }

                        break;

                    }


                    #region Fenetre principale
                    Panel HomePageMaindiv = new Panel();


                    //HtmlGenericControl subMenu = new HtmlGenericControl("div");
                    //subMenu.ID = "SubTabMenuCtnr";
                    //HomePageMaindiv.Controls.Add(subMenu);
                    //DoSubMenuTab(subMenu);


                    eHomePage hpg;
                    eudoDAL dal = eLibTools.GetEudoDAL(_pref);
                    try
                    {
                        dal.OpenDatabase();

                        HtmlGenericControl PageRender = null;
                        hpg = new eHomePage(dal, _pref, width, height, bReadOnly);
                        PageRender = hpg.GetPageRender();
                        if (PageRender == null || PageRender.Controls.Count <= 0 || hpg.NbEudoParts <= 0)
                        {
                            Image image = new Image();
                            image.CssClass = "DefaultHomePage";
                            image.ImageUrl = eConst.GHOST_IMG;

                            if (PageRender == null)
                            {
                                PageRender = new HtmlGenericControl("CENTER");
                            }
                            PageRender.Controls.Add(image);
                            image = null;
                        }

                        HtmlGenericControl innerContainer = new HtmlGenericControl("div");
                        innerContainer.ID = "innerContainer";
                        innerContainer.Controls.Add(PageRender);
                        innerContainer.Style.Add(System.Web.UI.HtmlTextWriterStyle.Width, width + "px");
                        innerContainer.Style.Add(System.Web.UI.HtmlTextWriterStyle.Height, height + "px");

                        HomePageMaindiv.Controls.Add(innerContainer);
                        PageRender = null;
                    }
                    catch (Exception err)
                    {
                        string sDevMsg = string.Concat("Exception Message : ", err.Message, Environment.NewLine, "Exception StackTrace :", err.StackTrace);

                        ErrorContainer = eErrorContainer.GetDevUserError(
                           eLibConst.MSG_TYPE.CRITICAL,
                           eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                           string.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                           eResApp.GetRes(_pref, 72),  //   titre
                           sDevMsg);
                        sDevMsg = null;
                        LaunchError();
                    }
                    finally
                    {
                        dal.CloseDatabase();
                        dal = null;
                        hpg = null;
                    }
                    try
                    {
                        RenderResultHTML(HomePageMaindiv);
                    }
                    finally
                    {
                        HomePageMaindiv = null;
                    }
                    break;
                #endregion

                case "savepref":
                    #region SAUVEGARDE MODIFICATIONS de pref
                    Boolean success = false;
                    try
                    {
                        List<SetParam<eLibConst.PREF_CONFIG>> param = new List<SetParam<eLibConst.PREF_CONFIG>>();
                        param.Add(new SetParam<eLibConst.PREF_CONFIG>(eLibConst.PREF_CONFIG.EUDOPARTCUSTOM, sEudoPartCustom));
                        success = _pref.SetConfig(param);
                        param.Clear();
                        param = null;
                    }
                    catch (Exception ex)
                    {
                        string sDevMsg = string.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                        success = false;

                        sDevMsg = string.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", ex.Message, Environment.NewLine, "Exception StackTrace :", ex.StackTrace);

                        ErrorContainer = eErrorContainer.GetDevUserError(
                           eLibConst.MSG_TYPE.CRITICAL,
                           eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                           string.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                           eResApp.GetRes(_pref, 72),  //   titre
                           sDevMsg);
                        LaunchError();
                    }

                    string sError = string.Empty;
                    if (!success)
                    {
                        sError = "Erreur à l'enregistrement de la modification.";    //TODO GCH : message plus jojo
                        this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 1713), sError, eResApp.GetRes(_pref, 72), null);
                        LaunchError();
                    }

                    XmlDocument xmlResult = GetErrorXmlDocument(sError);
                    try
                    {
                        //RETOUR du XML
                        RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
                    }
                    finally
                    {
                        xmlResult = null;
                        sError = null;
                    }

                    break;
                    #endregion
            }
        }

        /// <summary>
        /// Fait un rendu de l'onglet web/ des sous-onglet
        /// </summary>
        /// <param name="nType"></param>
        private void DoSubMenuTab(HtmlGenericControl subMenuContainer)
        {
            // Dev 38096 demandes parante: onglet web
            var subMenuRenderer = new eWebTabSubMenuRenderer(_pref, 0, EdnType.FILE_MAIN.GetHashCode());
            if (subMenuRenderer.Init())
            {
                if (subMenuRenderer.HasItems())
                {
                    subMenuRenderer.Build(subMenuContainer);
                    subMenuContainer.Attributes.Add("class", "homeSubMenuDiv");
                    height -= 32; // on retire la hauteur de sous menu
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


        private XmlDocument GetErrorXmlDocument(string ErrorMsg)
        {
            XmlDocument xmlResult = new XmlDocument();

            XmlNode detailsNode = xmlResult.CreateElement("elementlist");

            //Déclaration de l'objet XML principal
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            XmlNode resultNode = xmlResult.CreateElement("return");
            xmlResult.AppendChild(resultNode);

            // Erreur ?
            XmlNode successNode = xmlResult.CreateElement("result");
            successNode.InnerText = (ErrorMsg.Length <= 0) ? "SUCCESS" : "ERROR";
            resultNode.AppendChild(successNode);

            // Msg Erreur
            XmlNode errDesc = xmlResult.CreateElement("errordescription");
            errDesc.InnerText = ErrorMsg;
            resultNode.AppendChild(errDesc);

            //Valeurs de recherche à retourner
            if (detailsNode != null)
            {
                resultNode.AppendChild(detailsNode);
            }

            // RETOUR du XML
            return xmlResult;
        }
    }
}