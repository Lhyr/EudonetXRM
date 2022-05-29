using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;

namespace Com.Eudonet.Xrm
{
    /// <className>eMailTemplate</className>
    /// <summary>Classe permettant de gérer certains aspects des fichiers de type E-mail unitaire ou SMS, hors processus d'envoi de mail (EngineMail.cs) et hors méthodes de rendu (eMailRendererTools.cs)
    /// Gère, entre autres, la récupération et la mise à jour des modèles de mail (eMailTemplate)</summary>
    /// <purpose>Permet d'accéder aux informations contenues dans la table CATALOG</purpose>
    /// <authors>MAB</authors>
    /// <date>2012-10-15</date>
    public class eMailTemplate
    {
        #region Propriétés

        /// <summary>Composant d'accès aux données</summary>
        private eudoDAL _dal;
        /// <summary>Requête paramétrée utilisée pour les accès à la base de données</summary>
        private RqParam _rqRequest;
        /// <summary>Objet contenant les caractéristiques de l'utilisateur en cours</summary>
        private eUserInfo _userInfo;

        /// <summary>DescId du champ contenant les modèles de mail ([DescId] dans [CATALOG])</summary>
        private int _nDescId;
        /// <summary>Id du modèle de mail à gérer ([CatId] dans [CATALOG])</summary>
        private int _nMailTemplateId;
        /// <summary>Id de filtrage pour les fichiers de type Cible étendue ([TargetModel] dans [CATALOG])</summary>
        private int _nTargetId;
        /// <summary>Nom du modèle de mail à gérer ([Value] dans [CATALOG])</summary>
        private string _strMailTemplateName;
        /// <summary>Corps du modèle de mail à gérer ([Memo] dans [CATALOG])</summary>
        private string _strMailTemplateBody;
        /// <summary>Indique si le modèle de mail est au format HTML ([MemoFormat] dans [CATALOG])</summary>
        private bool _bMailTemplateBodyIsHTML;
        /// <summary>Contenu de la feuille de style CSS associé au modèle de mail à gérer ([MemoCss] dans [CATALOG])</summary>
        private string _strMailTemplateBodyCSS;

        // Droits sur les catalogues
        /// <summary>Droits d'ajout</summary>
        private Boolean _bAddAllowed = true;
        /// <summary>Droit de modification</summary>
        private Boolean _bUpdateAllowed = true;
        /// <summary>Droit de suppression</summary>
        private Boolean _bDeleteAllowed = true;

        #endregion

        #region Accesseurs

        /// <summary>DescId du champ contenant les modèles de mail ([DescId] dans [CATALOG])</summary>
        public int DescId
        {
            get { return _nDescId; }
        }
        /// <summary>DescId du champ contenant les modèles de mail ([DescId] dans [CATALOG])</summary>
        public int TargetId
        {
            get { return _nTargetId; }
        }
        /// <summary>DescId du champ contenant les modèles de mail ([DescId] dans [CATALOG])</summary>
        public int MailTemplateId
        {
            get { return _nMailTemplateId; }
        }
        /// <summary>Nom du modèle de mail</summary>
        public string MailTemplateName
        {
            get { return _strMailTemplateName; }
        }
        /// <summary>Corps du modèle de mail</summary>
        public string MailTemplateBody
        {
            get { return _strMailTemplateBody; }
        }
        /// <summary>Corps du modèle de mail</summary>
        public bool MailTemplateBodyIsHTML
        {
            get { return _bMailTemplateBodyIsHTML; }
        }
        /// <summary>Corps du modèle de mail</summary>
        public string MailTemplateBodyCSS
        {
            get { return _strMailTemplateBodyCSS; }
        }

        /// <summary>Droit d'ajout d'un modèle de mail</summary>
        public Boolean AddAllowed
        {
            get { return _bAddAllowed; }
        }

        /// <summary>Droit de suppression d'un modèle de mail</summary>
        public Boolean DeleteAllowed
        {
            get { return _bDeleteAllowed; }
        }

        /// <summary>Droit de mise à jour d'un modèle de mail</summary>
        public Boolean UpdateAllowed
        {
            get { return _bUpdateAllowed; }
        }

        #endregion

        #region Constructeurs

        /// <summary>
        /// Constructeur sans argument 
        /// Permet de simuler une assignation
        /// </summary>
        public eMailTemplate()
        { }

        /// <summary>
        /// Permet de gérer un modèle de mail v7 depuis la base de données
        /// </summary>
        /// <param name="eDal">Couche d'accès à la base de données</param>
        /// <param name="userInfo">Informations sur l'utilisateur actuellement connecté</param>
        /// <param name="descId">DescID du champ contenant les modèles de mail à gérer ([CATALOG].[DescId])</param>
        /// <param name="mailTemplateId">ID du champ contenant les modèles de mail à gérer ([CATALOG].[CatId])</param>
        /// <param name="targetId">Id du modèle à gérer ([CATALOG].[targetmodel]) </param>
        ///  <param name="strError">Message d'erreur eventuel</param>
        public eMailTemplate(eudoDAL eDal, eUserInfo userInfo, int descId, int targetId, int mailTemplateId, out string strError)
        {
            String error = String.Empty;

            _dal = eDal;
            _userInfo = userInfo;
            _nDescId = descId;
            _nTargetId = targetId;
            _nMailTemplateId = mailTemplateId;
            strError = String.Empty;

            try
            {
                // Récupération des permissions sur les modèles de mail
                StringBuilder sqlCatInfo = new StringBuilder();
                sqlCatInfo.AppendLine("SELECT  ")
                    .AppendLine(" ( select P from cfc_getPermInfo(@userid, @userlevel, @groupid) where permissionid =  IsNull([AddPermission],0) )  as    AddPermission, ")
                    .AppendLine("( select P from cfc_getPermInfo(@userid, @userlevel, @groupid) where permissionid =  IsNull([UpdatePermission],0) ) as  UpdatePermission, ")
                    .AppendLine("( select P from cfc_getPermInfo(@userid, @userlevel, @groupid) where permissionid =  IsNull([DeletePermission],0) ) as  DeletePermission, ")
                    .AppendLine("( select P from cfc_getPermInfo(@userid, @userlevel, @groupid) where permissionid =  IsNull([SynchroPermission],0) ) as  SynchroPermission ");

                sqlCatInfo.AppendLine(" FROM [FILEDATAPARAM] WHERE [DESCID] = @DESCID");

                RqParam rqRequestCatInfo = new RqParam();
                rqRequestCatInfo.SetQuery(sqlCatInfo.ToString());
                rqRequestCatInfo.AddInputParameter("@DESCID", SqlDbType.Int, descId);
                rqRequestCatInfo.AddInputParameter("@USERID", SqlDbType.Int, userInfo.UserId);
                rqRequestCatInfo.AddInputParameter("@USERLEVEL", SqlDbType.Int, userInfo.UserLevel);
                rqRequestCatInfo.AddInputParameter("@GROUPID", SqlDbType.Int, userInfo.UserGroupId);

                DataTableReaderTuned dtrCatalogInfo = eDal.Execute(rqRequestCatInfo, out error);
                try
                {
                    if (String.IsNullOrEmpty(error) && dtrCatalogInfo.HasRows && dtrCatalogInfo.Read())
                    {
                        _bAddAllowed = dtrCatalogInfo.IsDBNull("AddPermission") || dtrCatalogInfo.GetString("AddPermission") == "1";
                        _bUpdateAllowed = dtrCatalogInfo.GetString("UpdatePermission") == "1" || dtrCatalogInfo.IsDBNull("UpdatePermission");
                        _bDeleteAllowed = dtrCatalogInfo.GetString("DeletePermission") == "1" || dtrCatalogInfo.IsDBNull("DeletePermission");
                    }
                }
                finally
                {
                    if (dtrCatalogInfo != null)
                        dtrCatalogInfo.Dispose();
                }

                #region Récupération des informations en base de données
                _rqRequest = new RqParam();
                StringBuilder sbRequest = new StringBuilder();
                _rqRequest.AddInputParameter("@CatId", SqlDbType.Int, _nMailTemplateId);
                _rqRequest.AddInputParameter("@DescId", SqlDbType.Int, _nDescId);
                sbRequest.Append("SELECT [Catalog].[Value], [Catalog].[Memo], [Catalog].[MemoFormat], [Catalog].[MemoCss], ");
                sbRequest.Append("[Catalog].[DescId], [Catalog].[CatId], [Catalog].[TargetModel] ");
                sbRequest.Append("FROM [Catalog] WHERE [Catalog].[DescId] = @DescId AND [Catalog].[CatId] = @CatId");
                if (_nTargetId > 0)
                {
                    _rqRequest.AddInputParameter("@TargetId", SqlDbType.Int, _nTargetId);
                    sbRequest.Append(" AND IsNull([Catalog].[TargetModel],0) = @TargetId");
                }
                _rqRequest.SetQuery(sbRequest.ToString());

                if (_dal.HasActiveTransaction)
                    _dal.AddToTransaction(_rqRequest);

                DataTableReaderTuned dtrMailTemplate = eDal.Execute(_rqRequest, out error);
                try
                {
                    if (dtrMailTemplate != null)
                    {
                        while (dtrMailTemplate.Read())
                        {
                            _nDescId = dtrMailTemplate.GetEudoNumeric("DescId");
                            _nMailTemplateId = dtrMailTemplate.GetEudoNumeric("CatId");
                            _nTargetId = dtrMailTemplate.GetEudoNumeric("TargetModel");

                            _strMailTemplateName = dtrMailTemplate.GetString("Value");
                            _strMailTemplateBody = dtrMailTemplate.GetString("Memo");
                            _strMailTemplateBodyCSS = dtrMailTemplate.GetString("Memo");
                            _bMailTemplateBodyIsHTML = dtrMailTemplate.GetString("MemoFormat").Equals("1");
                        }
                    }
                }
                finally
                {
                    if (dtrMailTemplate != null)
                        dtrMailTemplate.Dispose();
                }
            }
            catch (Exception ex)
            {
                strError = "Erreur lors de la lecture du modèle de mail : " + ex.Message;
            }
            #endregion
        }
        #endregion

        #region Méthodes internes
        /// <summary>
        /// Met à jour le flux XML de rapport, si le message d'erreur n'est pas vide, le noeud success est ajusté à 0
        /// </summary>
        /// <param name="xmlReportDocument">Flux XML de rapport d'opération</param>
        /// <param name="error">Message rapport d'erreur</param>
        public bool UpdateXmlReport(XmlDocument xmlReportDocument, String error)
        {
            XmlNode xmlNodeSuccess = xmlReportDocument.SelectSingleNode("//success");
            XmlNode xmlNodeMessage = xmlReportDocument.SelectSingleNode("//message");

            Boolean operationSuccess = String.IsNullOrEmpty(error);
            xmlNodeSuccess.InnerText = (operationSuccess ? "1" : "0");
            xmlNodeMessage.InnerText = error.ToString();

            return operationSuccess;
        }
        /// <summary>
        /// Crée et retourne le flux XML de rapport de l'intervention sur le modèle de mail
        /// </summary>
        /// <param name="operation">Type d'opération (Insert, Update, Delete) </param>
        /// <param name="error">Message d'erreur post-opération</param>
        /// <returns>Flux XML de rapport d'opération</returns>
        public XmlDocument GetXmlReportDocument(eMailTemplate.Operation operation, String error)
        {
            return GetXmlReportDocument(operation, error, 0);
        }
        /// <summary>
        /// Créé et retourne le flux XML de rapport de l'intervention sur le modèle de mail
        /// </summary>
        /// <param name="operation">Type d'opération (Insert, Update, Delete) </param>
        /// <param name="error">Message d'erreur post opération</param>
        /// <param name="operationElement">Object CatalogValue impacté (dans le cas d'un d'un delete notamment)</param>
        /// <returns>Flux XML de rapport d'opération</returns>
        public XmlDocument GetXmlReportDocument(eMailTemplate.Operation operation, StringBuilder error, int operationElement)
        {
            return GetXmlReportDocument(operation, error.ToString(), operationElement);
        }
        /// <summary>
        /// Créé et retourne le flux XML de rapport de l'intervention sur le modèle de mail
        /// </summary>
        /// <param name="operation">Type d'opération (Insert, Update, Delete) </param>
        /// <param name="error">Message d'erreur post opération</param>
        /// <param name="operationElement">Object CatalogValue impacté </param>
        /// /// <param name="changedElement">Object CatalogValue d'origine(dans le cas d'un rename) </param>
        /// <returns>Flux XML de rapport d'opération</returns>
        public XmlDocument GetXmlReportDocument(eMailTemplate.Operation operation, String error, int operationElement, int changedElement = 0)
        {
            Boolean OperationSuccess = false;

            #region initialisation du retour XML (structure)
            XmlDocument xmlDocReturn = new XmlDocument();

            XmlNode xmlNodeReport = xmlDocReturn.CreateElement("ednOperationReport");

            XmlNode xmlNodeEdnResult = xmlDocReturn.CreateElement("ednResult");
            XmlNode xmlNodeSuccess = xmlDocReturn.CreateElement("success");
            XmlNode xmlNodeMessage = xmlDocReturn.CreateElement("message");
            XmlNode xmlNodeOperation = xmlDocReturn.CreateElement("operation");

            XmlNode xmlNodeMailTemplate = xmlDocReturn.CreateElement("mailTemplate");
            XmlAttribute xmlAttrDescId = xmlDocReturn.CreateAttribute("descId");
            XmlAttribute xmlAttrMailTemplateId = xmlDocReturn.CreateAttribute("mailTemplateId");
            XmlAttribute xmlAttrTargetId = xmlDocReturn.CreateAttribute("targetId");
            XmlNode xmlMailTemplateBody = xmlDocReturn.CreateElement("body");
            XmlAttribute xmlAttrMailTemplateBodyIsHTML = xmlDocReturn.CreateAttribute("html");
            XmlNode xmlMailTemplateBodyCSS = xmlDocReturn.CreateElement("bodyCSS");
            XmlNode xmlMailTemplateName = xmlDocReturn.CreateElement("name");
            #endregion

            #region Remplissage du rapport
            OperationSuccess = String.IsNullOrEmpty(error);
            xmlNodeSuccess.InnerText = (OperationSuccess ? "1" : "0");
            xmlNodeMessage.InnerText = error.ToString();

            xmlAttrDescId.InnerText = this._nDescId.ToString();
            xmlAttrMailTemplateId.InnerText = this._nMailTemplateId.ToString();
            xmlAttrTargetId.InnerText = this._nTargetId.ToString();
            xmlMailTemplateBody.InnerText = this._strMailTemplateBody;
            xmlMailTemplateBodyCSS.InnerText = this._strMailTemplateBodyCSS;
            xmlMailTemplateName.InnerText = this._strMailTemplateName;
            xmlAttrMailTemplateBodyIsHTML.InnerText = (this._bMailTemplateBodyIsHTML ? "1" : "0");

            xmlNodeOperation.AppendChild(xmlNodeReport);
            xmlNodeOperation.AppendChild(xmlNodeMailTemplate);

            xmlNodeEdnResult.AppendChild(xmlNodeSuccess);
            xmlNodeEdnResult.AppendChild(xmlNodeMessage);
            xmlNodeEdnResult.AppendChild(xmlNodeOperation);

            xmlDocReturn.AppendChild(xmlNodeEdnResult);

            xmlNodeMailTemplate.Attributes.Append(xmlAttrDescId);
            xmlNodeMailTemplate.Attributes.Append(xmlAttrMailTemplateId);
            xmlNodeMailTemplate.Attributes.Append(xmlAttrTargetId);
            xmlMailTemplateBody.Attributes.Append(xmlAttrMailTemplateBodyIsHTML);
            xmlNodeMailTemplate.AppendChild(xmlMailTemplateBody);
            xmlNodeMailTemplate.AppendChild(xmlMailTemplateBodyCSS);

            xmlNodeReport.AppendChild(xmlNodeMailTemplate);
            #endregion

            return xmlDocReturn;
        }
        #endregion

        #region actions utilisateur sur le catalogue
        /// <summary>
        /// Permet d'ajouter un nouveau modèle de mail
        /// </summary>
        /// <param name="error">Message d'erreur</param>
        /// <param name="diLang">Dictionnaire contenant les libellés pour chacune des langues utilisées</param>
        /// <returns>Valeur Insérée.</returns>
        public bool Insert(out String error, Dictionary<String, String> diLang = null)
        {
            error = String.Empty;
            StringBuilder sbError = new StringBuilder();

            if (!AddAllowed)
                throw new CatalogException("Ajout non autorisé", DescId);

            #region mise à jour du catalogue
            RqParam rqInsert = null;
            StringBuilder sbSQLInsert = new StringBuilder();

            #region Gestion du rapport XML
            XmlDocument xmlDoc = null;
            bool operationSuccess = false;
            #endregion

            eCatalog ec = new eCatalog();


            try
            {
                sbSQLInsert.Append("INSERT INTO [CATALOG] ([DescId], [TargetModel], [Value], [Memo], [MemoFormat], [MemoCss]) VALUES (@DescId, @TargetModel, @Value, @NewBody, @NewBodyIsHTML, @NewBodyCSS);");
                sbSQLInsert.Append("SELECT @NewCatId = SCOPE_IDENTITY()");
                rqInsert = new RqParam();
                rqInsert.AddInputParameter("@DescId", SqlDbType.Int, _nDescId);
                rqInsert.AddInputParameter("@TargetModel", SqlDbType.Int, _nTargetId);
                rqInsert.AddInputParameter("@Value", SqlDbType.VarChar, _strMailTemplateName);
                rqInsert.AddInputParameter("@NewBody", SqlDbType.VarChar, _strMailTemplateBody);
                rqInsert.AddInputParameter("@NewBodyIsHTML", SqlDbType.Bit, (_bMailTemplateBodyIsHTML ? 1 : 0));
                rqInsert.AddInputParameter("@NewBodyCSS", SqlDbType.VarChar, _strMailTemplateBodyCSS);
                rqInsert.AddOutputParameter("@NewCatId", SqlDbType.Int, 18);
                rqInsert.SetQuery(sbSQLInsert.ToString());
                _dal.AddToTransaction(rqInsert);
                _dal.ExecuteNonQuery(rqInsert, out error);
                if (!String.IsNullOrEmpty(error))
                    sbError.AppendLine(error);
                else
                {
                    //Préparation du Retour XML
                    xmlDoc = this.GetXmlReportDocument(eMailTemplate.Operation.Insert, sbError.ToString());
                    operationSuccess = UpdateXmlReport(xmlDoc, error);
                    if (!String.IsNullOrEmpty(error))
                    {
                        // TODO?
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateXmlReport(xmlDoc, ex.Message);
            }
            if (!String.IsNullOrEmpty(error)) { sbError.AppendLine(error); }

            #endregion

            return operationSuccess;
        }

        /// <summary>
        /// Renomme un modèle de mail existant
        /// </summary>
        /// <param name="error">Message d'erreur</param>        
        /// <returns>Flux XML de rapport d'opération</returns>
        /// <returns></returns>
        public XmlDocument Update(out String error)
        {
            if (!UpdateAllowed)
                throw new CatalogException("Mise à jour non autorisée", DescId);

            error = String.Empty;
            StringBuilder sbError = new StringBuilder();

            #region Gestion du rapport XML
            XmlDocument xmlDoc = null;
            #endregion

            #region mise à jour du catalogue
            RqParam rqUpdate = null;
            StringBuilder sbSQLUpdate = new StringBuilder();

            try
            {
                sbSQLUpdate.Append("UPDATE [CATALOG] SET [Value] = @Value, [Memo] = @NewBody, [MemoFormat] = @NewBodyIsHTML, [MemoCss] = @NewBodyCSS WHERE [CatId] = @CatId");
                rqUpdate = new RqParam();
                rqUpdate.AddInputParameter("@CatId", SqlDbType.Int, _nMailTemplateId);
                rqUpdate.AddInputParameter("@TargetModel", SqlDbType.Int, _nTargetId);
                rqUpdate.AddInputParameter("@DescId", SqlDbType.Int, _nDescId);
                rqUpdate.AddInputParameter("@Value", SqlDbType.VarChar, _strMailTemplateName);
                rqUpdate.AddInputParameter("@NewBody", SqlDbType.VarChar, _strMailTemplateBody);
                rqUpdate.AddInputParameter("@NewBodyIsHTML", SqlDbType.Bit, (_bMailTemplateBodyIsHTML ? 1 : 0));
                rqUpdate.AddInputParameter("@NewBodyCSS", SqlDbType.VarChar, _strMailTemplateBodyCSS);
                rqUpdate.SetQuery(sbSQLUpdate.ToString());
                _dal.AddToTransaction(rqUpdate);
                _dal.ExecuteNonQuery(rqUpdate, out error);
                if (!String.IsNullOrEmpty(error))
                    sbError.AppendLine(error);
                else
                {
                    //Préparation du Retour XML
                    xmlDoc = this.GetXmlReportDocument(eMailTemplate.Operation.Update, sbError.ToString());
                    UpdateXmlReport(xmlDoc, error);
                    if (!String.IsNullOrEmpty(error))
                    {
                        // TODO?
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateXmlReport(xmlDoc, ex.Message);
            }
            if (!String.IsNullOrEmpty(error)) { sbError.AppendLine(error); }

            #endregion

            error = sbError.ToString();

            return xmlDoc;
        }
        #endregion

        #region Sous-objets de la classe eMailTemplate

        /// <summary>
        /// Enum listant les différentes opérations possible sur un eMailTemplate
        /// Afin d'effectuer un traitement via eMailTemplateManager
        /// </summary>
        public enum Operation
        {
            /// <summary>Insertion</summary>
            Insert,
            /// <summary>Mise à jour</summary>
            Update,
            /// <summary>Suppression</summary>
            Delete
        }

        #endregion
    }
}