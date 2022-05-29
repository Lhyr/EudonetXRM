using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Threading;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eWebLinkManager
    /// </summary>
    public class eWebLinkManager : eAdminManager
    {
        /// <summary>
        /// Type d'action pour le manager
        /// ! Ne pas oublier de mettre à jour eAdmin.js !
        /// </summary>
        public enum WebLinkManagerAction
        {
            /// <summary>Action indéfinie </summary>
            UNDEFINED = 0,
            /// <summary>Rendu html de la table d'édition des propriétés d'un lien web</summary>
            GETINFOS = 1,
            /// <summary>
            /// Création d'un nouveau lien web
            /// </summary>
            CREATE = 2,
            /// <summary>
            /// Rafraichit le bloc de menu "Liens web et traitements spécifiques"
            /// </summary>
            UPDATEMENU = 3,
            /// <summary>
            /// Suppression d'un lien web
            /// </summary>
            DELETE = 4
        }

        /// <summary>
        /// Action
        /// </summary>
        private WebLinkManagerAction action = WebLinkManagerAction.UNDEFINED;
        private int _tab;
        private int _specifId;

        protected override void ProcessManager()
        {
            try
            {
                // Action - paramètre obligatoire
                if (_requestTools.AllKeys.Contains("action") && !String.IsNullOrEmpty(_context.Request.Form["action"]))
                {
                    if (!Enum.TryParse(_context.Request.Form["action"], out action))
                        action = WebLinkManagerAction.UNDEFINED;
                }

                // Table
                if (_requestTools.AllKeys.Contains("tab") && !String.IsNullOrEmpty(_context.Request.Form["tab"]))
                    Int32.TryParse(_context.Request.Form["tab"], out _tab);

                if (_tab == 0)
                    throw new EudoException("Le paramètre 'tab' est obligatoire.");


                switch (action)
                {
                    case WebLinkManagerAction.UNDEFINED:
                        throw new EudoException("Aucune action définie.");
                    case WebLinkManagerAction.GETINFOS:
                        if (_requestTools.AllKeys.Contains("id") && !String.IsNullOrEmpty(_context.Request.Form["id"]))
                            Int32.TryParse(_context.Request.Form["id"], out _specifId);

                        // Spécif obligatoire
                        if (_specifId == 0)
                        {
                            throw new EudoException("Paramètres non renseignés.");
                        }

                        // Création du rendu
                        eAdminRenderer rdr = eAdminRendererFactory.CreateAdminWebLinkPropertiesRenderer(_pref, _tab, _specifId);
                        RenderResultHTML(rdr.PgContainer);

                        break;
                    case WebLinkManagerAction.CREATE:

                        //int nPos = 1;
                        //if (_requestTools.AllKeys.Contains("createat") && !String.IsNullOrEmpty(_context.Request.Form["createat"]))
                        //    Int32.TryParse(_context.Request.Form["createat"], out nPos);
                        //else
                        //    throw new EudoException("Paramètre de position non renseigné.");


                        //eLibConst.SPECIF_TYPE specType = _requestTools.GetRequestFormEnum<eLibConst.SPECIF_TYPE>("type");

                        SpecifTreatmentResult result = eSpecif.CreateWebLink(_pref, _tab);

                        //retourne le flux json de l'objet de retour
                        RenderResult(RequestContentType.TEXT, delegate () { return JsonConvert.SerializeObject(result); });
                        break;
                    case WebLinkManagerAction.UPDATEMENU:


                        eAdminRenderer rdrBlock = eAdminRendererFactory.CreateAdminWebLinksAndTreatments(_pref, _tab, eResApp.GetRes(_pref, 6815));

                        RenderResultHTML(rdrBlock.PgContainer);

                        break;

                    case WebLinkManagerAction.DELETE:

                        _specifId = _requestTools.GetRequestFormKeyI("id") ?? 0;

                        // Spécif obligatoire
                        if (_specifId == 0) { throw new EudoException("Paramètres non renseignés."); }

                        SpecifTreatmentResult deleteResult = eSpecif.DeleteSpecif(_pref, _tab, _specifId);

                        //retourne le flux json de l'objet de retour
                        RenderResult(RequestContentType.TEXT, delegate () { return JsonConvert.SerializeObject(deleteResult); });

                        break;

                    default:
                        throw new NotImplementedException(String.Concat("Action non reconnue : ", action.ToString()));

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
    }
}