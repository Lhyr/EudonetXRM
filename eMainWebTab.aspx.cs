using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Threading;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Onglet web principal
    /// </summary>
    public partial class eMainWebTab : eEudoPage
    {


        /// <summary>
        /// Table de la liste
        /// </summary>
        public Int32 _nTab = 0;


        Int32 height;
        Int32 width;

        bool _onlySubMenu = false;



        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return null;
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {



            height = eConst.DEFAULT_WINDOW_HEIGHT;
            width = eConst.DEFAULT_WINDOW_WIDTH;

            if (_allKeys.Contains("divW") && !String.IsNullOrEmpty(Request.Form["divW"]))
                Int32.TryParse(Request.Form["divW"].ToString(), out width);

            if (_allKeys.Contains("divH") && !String.IsNullOrEmpty(Request.Form["divH"]))
                Int32.TryParse(Request.Form["divH"].ToString(), out height);


            _onlySubMenu = _requestTools.GetRequestQSKeyB("onlysubmenu") ?? false;

            // Chargement de param de session
            SECURITY_GROUP securityGroup = _pref.GroupMode;

            // par défaut accueil - TODO : VERIFIER SI LA PAGE PAR DEFAUT NE PEUT PAS ETRE PERSONNALISEE


            // Table
            if (!_allKeys.Contains("tab") || !Int32.TryParse(Request.Form["tab"].ToString(), out _nTab))
            {
                //TODO - supprimer cette ligne en fin de tests
                //_nTab = 200;

                ErrorContainer = eErrorContainer.GetDevUserError(
                eLibConst.MSG_TYPE.CRITICAL,
                eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                eResApp.GetRes(_pref, 72),  //   titre
                String.Concat("Erreur sur eMainWebTab.aspx - Page_Load :Paramètre table non fourni =  ")

                );
                if (_bFromeUpdater)
                    LaunchError();
                else
                    LaunchErrorHTML(true);
            }



            //Mode d'affichage
            TableLite tab = new TableLite(_nTab);
            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            dal.OpenDatabase();
            String err = String.Empty;
            tab.ExternalLoadInfo(dal, out err);
            if (err.Length > 0)
            {
                dal.CloseDatabase();

                ErrorContainer = eErrorContainer.GetDevUserError(
                eLibConst.MSG_TYPE.CRITICAL,
                eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                eResApp.GetRes(_pref, 72),  //   titre
                String.Concat("Erreur sur eMainWebTab.aspx - Page_Load : récupération du table lite ntab = ->", _nTab, "<- \n", err)

                );
                if (_bFromeUpdater)
                    LaunchError();
                else
                    LaunchErrorHTML(true);


            }

            //Type eudonet de la table

            try
            {
                if (tab.EdnType == EdnType.FILE_WEBTAB || tab.EdnType == EdnType.FILE_GRID)
                    DoWebTab(tab.EdnType.GetHashCode());
                else
                    throw new Exception("Impossible de charger l'onglet/sous-onglet web");

            }
            catch (eEndResponseException) { Response.End(); }
            catch (ThreadAbortException) { }
            catch (Exception e1)
            {
                //Avec exception
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);


                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", e1.Message, Environment.NewLine, "Exception StackTrace :", e1.StackTrace);

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg));


                if (_bFromeUpdater)
                    LaunchError();
                else
                    LaunchErrorHTML(true);
            }
            finally
            {

                dal.CloseDatabase();
            }
        }

        /// <summary>
        /// Fait un rendu de l'onglet web/ des sous-onglet
        /// </summary>
        /// <param name="nType"></param>
        private void DoWebTab(int nType)
        {
            #region Dev 38096 demandes parante: onglet web

            var subMenuRenderer = new eWebTabSubMenuRenderer(_pref, _nTab, nType);
            if (subMenuRenderer.Init())
            {
                if (subMenuRenderer.HasItems())
                    subMenuRenderer.Build(this.SubTabMenuCtnr);
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


                if (_bFromeUpdater)
                    LaunchError();
            }

            if (_onlySubMenu)
                return;


            HtmlGenericControl divErr = new HtmlGenericControl("div");
            divErr.Style.Add(HtmlTextWriterStyle.TextAlign, "center");
            if (!subMenuRenderer.HasItems())
            {
                divErr.InnerHtml = " Vous n'avez pas de sous-onglets à afficher !<br /> Pour plus d'information, veuillez contacter votre administrateur.";
                listheader.Controls.Add(divErr);
                return;
            }
            else
            {
                // Si on a onglet web on affiche la première specif
                eRenderer renderer = null;
                Int32 iFirstSpecifId = subMenuRenderer.FirstSpecifItemId();
                int iFirstGridId = subMenuRenderer.FirstGridItemId();
                if (iFirstSpecifId > 0)
                {
                    renderer = eRendererFactory.CreateWebTabRenderer(_pref, iFirstSpecifId, width, height, Request.Url.Scheme.ToLower().Equals("https"));
                }
                else if (iFirstGridId > 0)
                {
                    renderer = eRendererFactory.CreateXrmGridRenderer(_pref, iFirstGridId, width, height);

                }


                if (renderer.ErrorMsg.Length > 0)
                {
                    divErr.InnerHtml = " Une erreur s'est produite, impossible d'affihcer les sous-onglets!<br /> Merci de contacter votre administrateur.";
                    listheader.Controls.Add(divErr);
                    return;
                }

                listheader.Controls.Add(renderer.PgContainer);
            }
            #endregion
        }
    }
}