using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Interface des méthodes de rendu
    /// </summary>
    public interface iRendererXMLHTML
    {

        /// <summary>
        /// Gestion d'erreur pour les appel hors eUpdater
        /// </summary>
        /// <param name="bFullPage">indique si le flux de retour doit être une page HTML complète (avec header html, ect…)</param>        
        /// <param name="err">Conteneur d'erreur</param>
        /// <param name="sCallBack">Js de callback</param>
        void LaunchErrorHTML(bool bFullPage, eErrorContainer err = null, string sCallBack = "");


        /// <summary>
        /// Lance la gestion d'erreur si besoin
        /// </summary>
        /// <param name="err"></param>
        /// <param name="rqCt"></param>
        void LaunchError(eErrorContainer err = null, RequestContentType rqCt = RequestContentType.XML);


        /// <summary>
        /// Retourne le flux de retour
        /// </summary>
        /// <param name="contentTyp">Type de retour HTML, Text, XML, etc.</param>
        /// <param name="func">Fonction retournant le contenu en string du rendu à retourner</param>
        void RenderResult(RequestContentType contentTyp, Func<String> func);

        /// <summary>
        /// Retourne le résultat sous forme de HTML a partir d'un control Web.UI
        /// </summary>
        /// <typeparam name="T">Objet venant de la classe System.Web.UI.Control</typeparam>
        /// <param name="monPanel">Control représentant le rendu à retourner</param>
        /// <param name="bStripTop">Indique si le conteneur parent (monPanel doit être rendu</param>
        void RenderResultHTML<T>(T monPanel, Boolean bStripTop = false) where T : Control;
    }


    /// <summary>
    /// Classe implémentant les méthodes de rendu des ASHX/ASPX
    /// Pour les aspx, le rendu utilisé est généralement le rendu nati
    /// gestion d'erreur
    /// </summary>
    public class eRendererXMLHTML : iRendererXMLHTML
    {

        #region PROPRIETES
        private HttpContext _context;
        private eError _error;

        private eError EudoError
        {
            get { return _error; }

        }


        /// <summary>
        /// Error container a rendre
        /// </summary>
        private eErrorContainer ErrorContainer
        {
            get { return _error.Container; }

        }


        #endregion


        #region CONSTRUCTEURS

        /// <summary>
        /// Méthode statique permettant d'obtenir une instance de la classe de rendu
        /// </summary>
        /// <param name="err"></param>
        /// <returns></returns>
        public static eRendererXMLHTML GetRenderXMLHTML(eError err)
        {
            return new eRendererXMLHTML(err);
        }



        /// <summary>
        /// Constructeur par défaut
        /// Initialise le contexte
        /// </summary>
        private eRendererXMLHTML(eError err)
        {
            _error = err;
            _context = HttpContext.Current;
        }



        #endregion


        #region INTERFACE PUBLIC



        /// <summary>
        /// Retourne le flux de retour
        /// </summary>
        /// <param name="contentTyp">Type de retour HTML, Text, XML, etc.</param>
        /// <param name="func">Fonction retournant le contenu en string du rendu à retourner</param>
        public void RenderResult(RequestContentType contentTyp, Func<string> func)
        {

            _context.Response.Clear();


            if (contentTyp == RequestContentType.XML)
                _context.Response.AppendHeader("Access-Control-Allow-Origin", "*");

            //Dans le cas de la gestion d'erreur, ajout d'un header 

            if (this.ErrorContainer.IsSessionLost)
                _context.Response.Headers.Add("X-EDN-SESSION", "1");
            else if (this.ErrorContainer.IsSet)
                _context.Response.Headers.Add("X-EDN-ERRCODE", "1");

            SetContentType(contentTyp);
            
            _context.Response.Write(func());
            EndReponse();
        }

        /// <summary>
        /// Retourne le résultat sous forme de HTML a partir d'un control Web.UI
        /// </summary>
        /// <typeparam name="T">Objet venant de la classe System.Web.UI.Control</typeparam>
        /// <param name="monPanel">Control représentant le rendu à retourner</param>
        /// <param name="bStripTop">Indique si le conteneur parent (monPanel doit être rendu</param>
        public void RenderResultHTML<T>(T monPanel, Boolean bStripTop = false) where T : Control
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter tw = new HtmlTextWriter(sw);

            if (!bStripTop)
                monPanel.RenderControl(tw);
            else
            {
                foreach (Control ctr in monPanel.Controls)
                {
                    ctr.RenderControl(tw);
                }
            }

            RenderResult(RequestContentType.HTML, delegate() { return sb.ToString(); });
        }

 







        /// <summary>
        /// Gestion d'erreur depuis les eUpdater.JS
        /// </summary>
        /// <param name="rqCt">ContentType de l'erreur - Si non défini XML</param>
        /// <param name="err">Error container - Si non défini, utilise celui défini précédement </param>
        public void LaunchError(eErrorContainer err = null, RequestContentType rqCt = RequestContentType.XML)
        {

            if (err != null)
                _error.Container = err;

            if (this.ErrorContainer.IsSet || this.ErrorContainer.IsSessionLost)
                RenderErrorIfAny(rqCt);

        }


        /// <summary>
        /// Gestion d'erreur pour les appel hors eUpdater
        /// </summary>
        public void LaunchErrorHTML(bool bWithHeader, eErrorContainer err = null, string sCallBack = "")
        {
            if (err != null)
                _error.Container = err;

            if (this.ErrorContainer.IsSet || this.ErrorContainer.IsSessionLost)
                RenderErrorIfAnyHTML(bWithHeader, sCallBack);
        }


        #endregion


        #region Méthodes de rendu Interne

        /// <summary>
        /// Indique le ContentType de la reponse serveur
        /// </summary>
        /// <param name="enumTyp"></param>
        private void SetContentType(RequestContentType enumTyp)
        {
            switch (enumTyp)
            {
                case RequestContentType.HTML:
                    _context.Response.ContentType = "text/html";
                    break;
                case RequestContentType.TEXT:
                    _context.Response.ContentType = "text/plain";
                    break;
                case RequestContentType.XML:
                    _context.Response.ContentType = "text/xml";
                    break;
                case RequestContentType.SCRIPT:
                    _context.Response.ContentType = "text/javascript";
                    break;
            }
        }



        /// <summary>
        /// Retour le message d'erreur au format RequestContentType
        /// </summary>
        private void RenderErrorIfAny(RequestContentType contentTyp)
        {
            if (this.ErrorContainer.IsSet || this.ErrorContainer.IsSessionLost)
            {
                EudoError.LaunchFeedBack();
                RenderResult(contentTyp, delegate() { return EudoError.RenderError(contentTyp); });
            }
        }

        /// <summary>
        /// Retour le message d'erreur au format RequestContentType
        /// </summary>
        private void RenderErrorIfAnyHTML(Boolean bWithHeader, string sCallBack = "")
        {
            if (this.ErrorContainer.IsSet || this.ErrorContainer.IsSessionLost)
            {
                EudoError.LaunchFeedBack();
                RenderResult(RequestContentType.HTML, delegate() { return EudoError.RenderErrorHTML(bWithHeader, sCallBack); });
            }
        }


        /// <summary>
        /// Met fin à l'objet reponse en évitant les System.Threading.ThreadAbortException
        /// </summary>
        private void EndReponse()
        {

            throw new eEndResponseException();
            //_context.Response.End();
        }


        #endregion

    }
}