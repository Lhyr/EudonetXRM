using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.IRISBlack
{
    public class DetailXHTMLFactory
    {
        /// <summary>
        /// Liste du temps consommé par étape
        /// </summary>
        protected double _timeTaken = 0;

        /// <summary>
        /// TimeSpan de la durée de génération
        /// </summary>
        protected TimeSpan ts = new TimeSpan();


        /// <summary>
        /// Date de début de génération
        /// </summary>
        protected DateTime dtStart;

        /// <summary>
        /// Date de fin de génération
        /// </summary>
        protected DateTime dtEnd;

        /// <summary>Si à true rajoute le doctype et les balises HTML, HEAD et BODY dans le rendu</summary>
        public bool AddHeadAndBody { get; set; } = false;

        /// <summary>Classe à ajouter à l'élément BODY </summary>
        public string BodyCssClass { get; set; }

        /// <summary>méthode javascript à éxecuter lors du chargement </summary>
        public string OnLoadBody { get; set; }

        /// <summary>
        /// objet de gestion de rendu
        /// </summary>
        public eRendererXMLHTML _renderXMLHTML { get; set; }

        // Les variables suivantes ne sont pas utilisée au sein de la classe maitre, elles doivent être redéfini dans les filles
        internal eError _error { get; set; } = null;


        /// <summary>
        /// Retourne le résultat sous forme de HTML a partir d'un control Web.UI
        /// </summary>
        /// <typeparam name="T">Objet venant de la classe System.Web.UI.Control</typeparam>
        /// <param name="monPanel">Control représentant le rendu à retourner</param>
        /// <param name="bStripTop">Indique si le conteneur parent (monPanel doit être rendu</param>
        public void RenderResultHTML<T>(T monPanel, Boolean bStripTop = false) where T : Control
        {
            RenderResult(RequestContentType.HTML, delegate () { return GetResultHTML(monPanel, bStripTop); });
        }


        /// <summary>
        /// Retourne le résultat sous forme de HTML a partir d'une liste de control Web.UI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="monPanel"></param>
        /// <param name="bStripTop"></param>
        /// <returns></returns>
        public string GetResultHTML<T>(List<T> monPanel, bool bStripTop = false) where T : Control {
            return string.Join("", monPanel.Select(pan => GetResultHTML(pan, bStripTop)));            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="monPanel"></param>
        /// <param name="bStripTop"></param>
        /// <returns></returns>
        public string GetResultHTML<T>(T monPanel, Boolean bStripTop = false) where T : Control
        {
            IEnumerable<Control> lstControl;
            StringBuilder sb = null;
            lstControl = (!bStripTop) ? new HashSet<Control> { monPanel } : monPanel.Controls.Cast<Control>();

            if (AddHeadAndBody)
            {
                //AJOUT DES BALISES HTML, HEAD, BODY et du doctype
                HtmlGenericControl ctrlHtml = new HtmlGenericControl("HTML");
                HtmlGenericControl ctrlHead = new HtmlGenericControl("HEAD");

                ctrlHtml.Controls.Add(ctrlHead);
                HtmlGenericControl ctrlBody = new HtmlGenericControl("BODY");
                // 41590 CRU : Ajout d'une classe sur Body pour cibler en CSS
                if (!string.IsNullOrEmpty(BodyCssClass))
                {
                    ctrlBody.Attributes.Add("class", BodyCssClass);
                }
                if (!string.IsNullOrEmpty(OnLoadBody))
                {
                    ctrlBody.Attributes.Add("onload", OnLoadBody);
                }
                ctrlHtml.Controls.Add(ctrlBody);
                //---Ajout du contenu de la page
                foreach (Control c in lstControl)
                    ctrlBody.Controls.Add(c);
                //---
                sb = new StringBuilder()
                    .Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 //EN\">")
                    .Append(GetControlRender((Control)ctrlHtml)); //Récupération du contenu de la page au format Chaine
            }
            else
            {
                sb = GetControlRender(lstControl);  //Récupération du contenu de la page au format Chaine
            }

            return sb.ToString();
        }

        /// <summary>
        /// Transformation d'un control en chaine de caractère html
        /// </summary>
        /// <param name="ctrToConvert">control à convertir</param>
        /// <returns>rendu html</returns>
        private static StringBuilder GetControlRender(Control ctrToConvert)
        {
            return GetControlRender(new HashSet<Control> { ctrToConvert });
        }

        /// <summary>
        /// Transformation d'une liste de controls en chaine de caractère html
        /// </summary>
        /// <param name="ctrToConvert">controls à convertir</param>
        /// <returns>rendu html</returns>
        private static StringBuilder GetControlRender(IEnumerable<Control> listCtrToConvert)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter tw = new HtmlTextWriter(sw);
            foreach (Control ctr in listCtrToConvert)
            {
                ctr.RenderControl(tw);
            }
            return sb;
        }

        /// <summary>
        /// Fonction qui va chercher dans les controles, et les sous-controles un ID précis.
        /// Et retourner le controle, ou null.
        /// </summary> 
        /// <typeparam name="T">Un control, pour le moment</typeparam>
        /// <param name="currentCtrl">le controle à rechercher</param>
        /// <param name="strID">l'identifiant du controle à rechercher.</param>
        /// <returns>le controle dont on a besoin.</returns>
        internal Control ChangeAttributeOfCtrlRecursively<T>(T currentCtrl, string strID)
            where T : System.Collections.IEnumerator
        {
            Control ctrlFnd = null;
            bool isCtrlFound = false;

            if (currentCtrl.MoveNext())
            {                
                ctrlFnd = (currentCtrl.Current as Control);
                WebControl wbFnd = (currentCtrl.Current as WebControl);
                HtmlGenericControl htmlFnd = (currentCtrl.Current as HtmlGenericControl);
                isCtrlFound = (htmlFnd ?? wbFnd ?? ctrlFnd)?.ID == strID || (htmlFnd?.Attributes["id"] ?? wbFnd?.Attributes["id"]) == strID;

                if (isCtrlFound)
                    return ctrlFnd;
                else if (ctrlFnd.HasControls())
                    ctrlFnd = ChangeAttributeOfCtrlRecursively(ctrlFnd.Controls.GetEnumerator(), strID);
                else
                    ctrlFnd = ChangeAttributeOfCtrlRecursively(currentCtrl, strID);

                if(ctrlFnd == null)
                    ctrlFnd = ChangeAttributeOfCtrlRecursively(currentCtrl, strID);
            }

            return ctrlFnd;

        }


        #region INTERFACE PUBLIC

        /// <summary>
        /// Gestion d'erreur depuis les eUpdater.JS
        /// </summary>
        /// <param name="rqCt">ContentType de l'erreur - Si non défini XML</param>
        /// <param name="err">Error container - Si non défini, utilise celui défini précédement </param>
        public void LaunchError(eErrorContainer err = null, RequestContentType rqCt = RequestContentType.XML)
        {
            LogResult(err);

            _renderXMLHTML.LaunchError(err, rqCt);
        }

        /// <summary>
        /// Retourne le flux de retour
        /// </summary>
        /// <param name="contentTyp">Type de retour HTML, Text, XML, etc.</param>
        /// <param name="func">Fonction retournant le contenu en string du rendu à retourner</param>
        public void RenderResult(RequestContentType contentTyp, Func<string> func)
        {

            LogResult();

            HttpContext.Current.Response.Headers.Add("X-time", (DateTime.Now - dtStart).TotalMilliseconds.ToString());


            _renderXMLHTML.RenderResult(contentTyp, func);



        }

        /// <summary>
        /// Lance un rendu HTML de l'erreur
        /// Il s'agit d'un appel js a eAlert.
        /// </summary>
        /// <param name="bWithHeader">Indique si les entete html complet doivent être générés</param>
        /// <param name="err">erreurContainer. Par défaut, utilise celui du eError de la page</param>
        /// <param name="sCallBack">Callback sur erreur</param>
        public void LaunchErrorHTML(bool bWithHeader, eErrorContainer err = null, string sCallBack = "")
        {
            _renderXMLHTML.LaunchErrorHTML(bWithHeader, err, sCallBack);
        }

        #endregion


        /// <summary>
        /// Log des informations sur les temps de réponses
        /// </summary>
        /// <param name="err">erreur éventuelle</param>
        protected virtual void LogResult(eErrorContainer err = null)
        {
            _timeTaken = (DateTime.Now - dtStart).TotalMilliseconds;
        }
    }
}