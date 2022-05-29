using Com.Eudonet.Internal;
using System;
using System.Xml;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Description résumée de eOrganigramme
    /// </summary>
    public class eOrganigrammeManager : eEudoManager
    {
        /// <summary>Opération de manger demandée</summary>
        private Operation _operation = 0;
        /// <summary>descid de la table ou l'on souhaite voir l'organigramme</summary>
        private Int32 _nTab = 0;
        /// <summary>Id de la fiche de départ</summary>
        private Int32 _nFileId = 0;
        /// <summary>
        /// Liste des actions possibles du manager
        /// </summary>
        private enum Operation
        {
            NONE = -1,
            ShowDialog = 0,
        }
        /// <summary>
        /// Load de la page
        /// </summary>
        protected override void ProcessManager()
        {
            RetrieveParams();
            RunOperation();
            RenderXmlResponse();
        }
        /// <summary>
        /// Récupération des infos postées
        /// </summary>
        private void RetrieveParams()
        {
            Enum.TryParse<Operation>(_context.Request.Form["operation"], out _operation);
            //Table sur laquelle on recherche
            if (_allKeys.Contains("tab"))
                _nTab = eLibTools.GetNum(_context.Request.Form["tab"]);
            //id de la fiche de départ
            if (_allKeys.Contains("fileid"))
                _nFileId = eLibTools.GetNum(_context.Request.Form["fileid"]);
        }

        /// <summary>
        /// Execute l'action demandée 
        /// </summary>
        private void RunOperation()
        {
            switch (_operation)
            {
                case Operation.ShowDialog:
                    ShowDialog();
                    break;
                default:
                    break;
            }
        }
        /// <summary>Rendu de la fenêtre</summary>
        public void ShowDialog()
        {
            String sError = String.Empty;
            String sUserError = String.Empty;
            eOrganigrammeRenderer renOrg = new eOrganigrammeRenderer(_pref, _nTab, _nFileId);
            if (!renOrg.Load(out sUserError, out sError))
                LaunchError(sUserError, sError);
            AddHeadAndBody = true;
            RenderResultHTML(renOrg.GetDialogContent());
        }
        /// <summary>Affiche un message d'erreur</summary>
        /// <param name="error">message d'erreur à afficher</param>
        public void LaunchError(String userError, String error)
        {

            String sDevMsg = string.Concat("Erreur : ", error);

            ErrorContainer = eErrorContainer.GetDevUserError(
               eLibConst.MSG_TYPE.CRITICAL,
               eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
               userError,
               eResApp.GetRes(_pref, 72),  //   titre
               string.Concat(sDevMsg));
            LaunchError();
        }

        /// <summary>
        /// Construit et renvois le xml de réponse
        /// </summary>
        private void RenderXmlResponse()
        {
            XmlDocument _xmlDocReturn = new XmlDocument();

            #region XML Declartion, UTF8 ..etc

            XmlNode baseResultNode;
            _xmlDocReturn.AppendChild(_xmlDocReturn.CreateXmlDeclaration("1.0", "UTF-8", null));
            baseResultNode = NewNode("result", _xmlDocReturn, _xmlDocReturn);

            #endregion

            #region initialisation du retour XML (structure)

            XmlNode xmlNodeSuccessNode = NewNode("success", baseResultNode, _xmlDocReturn);

            //Gestion des erreurs
            XmlNode xmlNodeErrorCodeNode = NewNode("ErrorCode", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeErrorDescription = NewNode("ErrorDescription", baseResultNode, _xmlDocReturn);

            //Noeuds du modèle
            XmlNode xmlNodeOperation = NewNode("operation", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeMailTemplateId = NewNode("iMailTemplateId", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeSuBject = NewNode("subject", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeBody = NewNode("body", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeBody_css = NewNode("body_css", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeBody_html = NewNode("body_html", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeName = NewNode("mailTplname", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeType = NewNode("mailTplType", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeMailTemplateType = NewNode("mailTplTypeDb", baseResultNode, _xmlDocReturn);

            //Permissions
            XmlNode xmlNodeViewPermId = NewNode("ViewPermId", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeUpdatePermId = NewNode("UpdatePermId", baseResultNode, _xmlDocReturn);

            #endregion

            #region remplit le xml et fait le rendu
            /*
            Boolean bError = CheckError();

            xmlNodeSuccessNode.InnerText = bError ? "0" : "1";

            if (bError)
            {
                //erreur
                xmlNodeName.InnerText = _sLabel;
                xmlNodeErrorCodeNode.InnerText = _errCode.GetHashCode().ToString();
                xmlNodeErrorDescription.InnerText = this._sErr;
            }
            else
            {
                //Succes
                xmlNodeMailTemplateId.InnerText = this._oMailTemplate.Id.ToString();
                xmlNodeSuBject.InnerText = this._oMailTemplate.Subject;
                xmlNodeBody.InnerText = this._oMailTemplate.Body;
                xmlNodeName.InnerText = this._oMailTemplate.Label;
                xmlNodeBody_css.InnerText = this._oMailTemplate.Css;
                xmlNodeBody_html.InnerText = (this._oMailTemplate.Body_HTML ? "1" : "0");
                xmlNodeType.InnerText = _iTypeTemplate.ToString();

                //retourne les ids des permissions
                xmlNodeViewPermId.InnerText = this._oMailTemplate.ViewPerm.PermId.ToString();
                xmlNodeUpdatePermId.InnerText = this._oMailTemplate.UpdatePerm.PermId.ToString();

                //ajout l'attribut descid pour body_html
                XmlAttribute xmlDescId = _xmlDocReturn.CreateAttribute("id");
                xmlDescId.Value = String.Concat("COL_", TableType.CAMPAIGN.GetHashCode(), "_@descid_0_0_0");
                xmlDescId.Value = xmlDescId.Value.Replace("@descid", CampaignField.ISHTML.GetHashCode().ToString());
                xmlNodeBody_html.Attributes.Append(xmlDescId);

                //ajout l'attribut descid pour subject
                xmlDescId = _xmlDocReturn.CreateAttribute("id");
                xmlDescId.Value = String.Concat("COL_", TableType.CAMPAIGN.GetHashCode(), "_@descid_0_0_0");
                xmlDescId.Value = xmlDescId.Value.Replace("@descid", CampaignField.SUBJECT.GetHashCode().ToString());
                xmlNodeSuBject.Attributes.Append(xmlDescId);

                //ajout l'attribut descid pour body
                xmlDescId = _xmlDocReturn.CreateAttribute("id");
                xmlDescId.Value = String.Concat("edtCOL_", TableType.CAMPAIGN.GetHashCode(), "_@descid_0_0_0");
                xmlDescId.Value = xmlDescId.Value.Replace("@descid", CampaignField.BODY.GetHashCode().ToString());
                xmlNodeBody.Attributes.Append(xmlDescId);

            }
            */

            RenderResult(RequestContentType.XML, delegate () { return _xmlDocReturn.OuterXml; });

            #endregion

        }
        /// <summary>
        /// Créer un nouveau noeud xml et l'ajoute au noeud parent
        /// </summary>
        /// <param name="Name">nom du noeud</param>
        /// <param name="ParentNode">Noeud parent</param>
        /// <returns></returns>
        private XmlNode NewNode(String Name, XmlNode ParentNode, XmlDocument Creator)
        {
            XmlNode child = Creator.CreateElement(Name);
            ParentNode.AppendChild(child);
            return child;
        }

    }
}