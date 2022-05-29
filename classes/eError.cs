using Com.Eudonet.Internal;
using EudoExtendedClasses;
using System;
using System.Collections.Generic;
using System.Web;
using System.Xml;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de gestion d'erreur
    /// </summary>
    public class eError
    {
        #region Properties

        /// <summary>
        /// eErrorContainer définit les propriétés d'une remontée d'erreur
        /// </summary>
        private eErrorContainer _container;
        private ePref _pref;
        /// <summary>
        /// Liste de noeuds custom qui sera ajouté dans le XML d'erreur
        /// </summary>
        private Dictionary<String, String> _dicCustomValues = new Dictionary<String, String>();

        #endregion

        #region Accesseur

        /// <summary>
        /// MaJ la référence à perf
        /// </summary>
        /// <param name="pref"></param>
        public void SetPref(ePref pref)
        {
            _pref = pref;
        }

        /// <summary>
        /// Objet message d'erreur
        /// Sur le set, déclenche l'appel a feedback si le container a
        /// sa propriété IsSet à true (automatiquement géré par l'objet container)
        /// </summary>
        public eErrorContainer Container
        {
            get
            {
                if (_container == null)
                    _container = new eErrorContainer();

                return _container;
            }

            set
            {
                _container = value;
            }
        }


        /// <summary>
        /// Indique si une erreur a été définie
        /// </summary>
        public Boolean IsSet
        {
            get
            {
                if (_container == null)
                    return false;

                return _container.IsSet;
            }
        }


        #endregion

        /// <summary>
        /// Constructeur privé
        /// </summary>
        /// <param name="container">Erreur</param>
        /// <param name="pref">Préference utilisateur</param>
        private eError(eErrorContainer container, ePref pref)
        {
            if (container != null)
                _container = container;
            _pref = pref;
        }

        private eError()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static eError getError()
        {
            eError e = new eError();
            e.Container = new eErrorContainer();
            return new eError();
        }

        #region methodes

        /// <summary>
        /// Lance les feedback
        ///  - Si une erreur qui n'est pas une perte de session a été déclarée
        ///  - Si un message de debug a été initialisé
        /// </summary>
        public void LaunchFeedBack()
        {
            if (_container.IsSet && _container.DebugMsg.Length > 0 && (_container.ForceFeedback || !_container.IsSessionLost))
            {
                eFeedbackXrm.LaunchFeedbackXrm(_container, _pref);
            }
        }


        /// <summary>
        /// Retour en XML du message d'erreur.
        /// Si Title vide, msg par defaut : 92 - Mise à jour impossible
        /// Si Msg vide, msg par defaut : 6237 - Une erreur est survenue lors de la mise à jour
        /// Si Detail vide, msg par defaut : 6236 - Merci de réessayer ou de contacter le service support.
        /// </summary>
        private XmlDocument GetErrorXML()
        {
            if (_container == null)
                return null;

            // <root><success>0</success><title>titre de la fenetre</title><msg errTyp="">BLABLA</msg><detail>BLABLA autre</detail><detaildev>BLABLA pour le dev</detaildev></root>

            // Lang par défaut
            Int32 iLangId = 0;
            if (_pref != null)
            {
                iLangId = _pref.LangServId;
            }
            else
                iLangId = EudoCommonHelper.EudoHelpers.GetUserIdLangFromCookie();

            XmlDocument xmlResult = new XmlDocument();

            XmlNode mainNode = xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlResult.AppendChild(mainNode);

            XmlNode xmlNodeRoot = xmlResult.CreateElement("root");
            xmlResult.AppendChild(xmlNodeRoot);

            // Indicateur de non success
            XmlNode xmlNodeSuccess = xmlResult.CreateElement("success");
            xmlNodeRoot.AppendChild(xmlNodeSuccess);
            xmlNodeSuccess.InnerText = "0";

            // Titre de la fenêtre
            XmlNode xmlErrorTitleNode = xmlResult.CreateElement("title");
            xmlNodeRoot.AppendChild(xmlErrorTitleNode);
            // Message par defaut : 92 - Mise à jour impossible
            xmlErrorTitleNode.InnerText = _container.Title.Length == 0 ? eResApp.GetRes(iLangId, 92) : _container.Title.ToHtml();

            // Theme Message
            XmlNode xmlErrorNode = xmlResult.CreateElement("msg");
            xmlNodeRoot.AppendChild(xmlErrorNode);
            // Message par defaut : 6237 - Une erreur est survenue lors de la mise à jour
            xmlErrorNode.InnerText = _container.Msg.Length == 0 ? eResApp.GetRes(iLangId, 6237) : _container.Msg.ToHtml();

            // Attribut indiquant le type d'erreur
            XmlAttribute xmlNodeAttr = xmlResult.CreateAttribute("errTyp");
            xmlErrorNode.Attributes.Append(xmlNodeAttr);
            xmlNodeAttr.Value = _container.TypeCriticity.GetHashCode().ToString();

            // Msg detailé
            XmlNode xmlErrorDetailNode = xmlResult.CreateElement("detail");
            xmlNodeRoot.AppendChild(xmlErrorDetailNode);
            // Message par defaut : 6236 - Merci de réessayer ou de contacter le service support.
            xmlErrorDetailNode.InnerText = _container.Detail.Length == 0 ? eResApp.GetRes(iLangId, 6236) : _container.Detail.ToHtml();

            // Msg detailé
            XmlNode xmlErrorDetailDevNode = xmlResult.CreateElement("detaildev");
            xmlNodeRoot.AppendChild(xmlErrorDetailDevNode);

            // Test debug 
            if (eLibTools.IsLocalOrEudoMachine(HttpContext.Current))
                xmlErrorDetailDevNode.InnerText = _container.DebugMsg.ToHtml();
            else {
#if DEBUG
                xmlErrorDetailDevNode.InnerText = _container.DebugMsg.ToHtml();
#endif
            }
            #region Noeuds custom

            XmlNode xmlCustomNode = null;
            if (_dicCustomValues.Count > 0)
            {
                XmlNode customNodes = xmlResult.CreateElement("custom");
                xmlNodeRoot.AppendChild(customNodes);
                foreach (KeyValuePair<String, String> currentCustomNode in _dicCustomValues)
                {
                    xmlCustomNode = xmlResult.CreateElement(currentCustomNode.Key);
                    customNodes.AppendChild(xmlCustomNode);
                    xmlCustomNode.InnerText = currentCustomNode.Value;
                }
                xmlCustomNode = null;
            }

            #endregion

            return xmlResult;
        }

        /// <summary>
        /// Création du string représentant un eAlert Javascript contenu dans des balises de scripts
        /// Ce message est déstiné à l'utilisateur
        /// Il faut au préalable avoir remplis les differents champs
        /// </summary>
        /// <param name="widtheAlert">Largeur de la fenetre</param>
        /// <param name="heighteAlert">Hauteur de la fenetre</param>
        /// <param name="fnctReturn">Méthode de retour</param>
        /// <returns>String contenant une eAlert Javascript entourée des balises de scripts</returns>
        protected string GetCompletAlert(int widtheAlert = 0, int heighteAlert = 0, string fnctReturn = "")
        {
            return eTools.GetCompletAlert(_container, widtheAlert, heighteAlert, fnctReturn);
        }


        /// <summary>
        /// Création du string représentant un eAlert Javascript
        /// Cette méthode renvoi un code JavaScript sans les balises de scripts
        /// Ce message est déstiné à l'utilisateur
        /// Il faut au préalable avoir remplis les differents champs contenu dans eErrorContainer
        /// </summary>
        /// <param name="widtheAlert">Largeur de la fenetre</param>
        /// <param name="heighteAlert">Hauteur de la fenetre</param>
        /// <param name="fnctReturn">Méthode de retour</param>
        /// <returns>String contenant une eAlert Javascript </returns>
        public String GetJsAlert(int widtheAlert = 0, int heighteAlert = 0, string fnctReturn = "")
        {
            return eTools.GetJsAlert(_container, widtheAlert, heighteAlert, fnctReturn);
        }


        /// <summary>
        /// Retour le message d'erreur au format XML string dans le cadre d'un appel via eUpdater.JS
        /// </summary>
        public String RenderError(RequestContentType contentTyp)
        {
            if (IsSet)
            {
                return GetErrorXML().OuterXml;
            }

            return String.Empty;
        }


        /// <summary>
        /// /// Retour le message d'erreur au format HTML string dans le cadre d'un appel Hors eUpdater.JS
        /// </summary>
        /// <param name="bWithHeader">Avec header HTML</param>
        /// <param name="sCallBack">Fonction JS de callback</param>
        /// <returns></returns>
        public String RenderErrorHTML(Boolean bWithHeader, string sCallBack = "")
        {

            if (Container.IsSet || Container.IsSessionLost)
            {
                if (bWithHeader)
                    return GetCompletAlert(0, 0, sCallBack);
                else
                    return GetJsAlert();
            }

            return String.Empty;
        }

        /// <summary>
        /// Fonction qui permet de rajouter un noeud XML dans le XML d'erreur
        /// </summary>
        /// <param name="sKey">Id du noeud</param>
        /// <param name="sValue">valeur du noeud</param>
        /// <returns>Si le noeud est déjà existant il sera retourné FAUX</returns>
        public Boolean AddCustomXmlNode(String sKey, String sValue)
        {
            Boolean alreadyExist = _dicCustomValues.ContainsKey(sKey);
            if (!alreadyExist)
            {
                _dicCustomValues.Add(sKey, sValue);
                return true;
            }
            else
                return false;

        }


        #endregion

    }

    /// <summary>
    /// Type de retour
    /// </summary>
    public enum RequestContentType
    {
        /// <summary>retour XML</summary>
        XML,
        /// <summary>retour TEXT</summary>
        TEXT,
        /// <summary>retour HTML</summary>
        HTML,
        /// <summary>javascript</summary>
        SCRIPT
    }
}