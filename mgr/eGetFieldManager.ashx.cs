using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Xml;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <className>eGetFieldManager</className>
    /// <summary>Récupère la valeur d'un champ donné en base.</summary>
    /// <purpose>A appeler depuis un JavaScript par ex.</purpose>
    /// <authors>MAB</authors>
    /// <date>2012-09-03</date>
    public class eGetFieldManager : eEudoManager
    {

        /// <summary>
        /// Type d'action disponible pour ce manager :
        /// </summary>
        public enum GetFieldManagerAction
        {
            /// <summary>
            /// récupération de la valeur d'un champ (action par défaut)
            /// </summary>
            FIELD_VALUE,

            /// <summary>
            /// récupération d'un modèle de mail
            /// </summary>
            MAIL_TEMPLATE, // 
        }


        /// <summary>
        ///  Objet permettant de charger le contexte (paramètres d'exécution) pour l'exécution du processus de récupération de données
        /// </summary>
        public class GetFieldManagerContext
        {
            #region Propriétés

            private GetFieldManagerAction _action = GetFieldManagerAction.FIELD_VALUE;
            private Int32 _fieldDescId = 0;
            private Int32 _fileId = 0;
            private Int32 _tabDescId = 0;
            private Int32 _catId = 0;
            private String _memoId = String.Empty;

            #endregion

            #region Accesseurs

            /// <summary>
            /// Action à effectuer
            /// A passer en paramètre depuis l'appelant, sous forme de String
            /// FIELD_VALUE : récupérer la valeur d'un champ (DescID) pour le FileID spécifié (par défaut)
            /// MAIL_TEMPLATE : récupérer les informations d'un modèle de mail
            /// </summary>
            public GetFieldManagerAction Action
            {
                get { return _action; }
            }
            /// <summary>
            /// DescID du champ à récupérer
            /// </summary>
            public Int32 FieldDescId
            {
                get { return _fieldDescId; }
            }
            /// <summary>
            /// ID de l'enregistrement pour lequel récupérer la valeur du champ
            /// </summary>
            public Int32 FileId
            {
                get { return _fileId; }
            }
            /// <summary>
            /// ID de l'onglet concerné
            /// </summary>
            public Int32 TabDescId
            {
                get { return _tabDescId; }
            }
            /// <summary>
            /// Identifiant d'un modèle de mail dans la table [CATALOG]
            /// </summary>

            public Int32 CatId
            {
                get { return _catId; }
            }

            /// <summary>
            /// Id de aMemoeditors
            /// </summary>
            public String MemoId
            {
                get { return _memoId; }
            }

            #endregion

            /// <summary>
            /// Objet permettant de charger le contexte (paramètres d'exécution) pour l'exécution du processus de récupération de données
            /// </summary>
            public GetFieldManagerContext()
            {

            }

            /// <summary>
            /// Charge le contexte (paramètres d'exécution) à partir de l'objet contexte HTTP (valeurs dans Request.Form)
            /// </summary>
            /// <param name="context">Contexte HTTP contenant les paramètres requis</param>
            /// <param name="error">Message d'erreur si le traitement échoue</param>
            public void Load(HttpContext context, out String error)
            {
                error = String.Empty;
                Load(context.Request.Form, out error);
            }

            /// <summary>
            /// Charge le contexte (paramètres d'exécution) à partir d'un tableau de clés/valeurs
            /// </summary>
            /// <param name="parameters">Tableau de clés/valeurs contenant les paramètres requis</param>
            /// <param name="error">Message d'erreur si le traitement échoue</param>
            public void Load(NameValueCollection parameters, out String error)
            {
                error = String.Empty;

                #region Action à effectuer
                try
                {
                    string strAction = parameters["action"].ToString();
                    switch (strAction)
                    {
                        case "MAIL_TEMPLATE":
                            _action = GetFieldManagerAction.MAIL_TEMPLATE;
                            break;
                        case "FIELD_VALUE":
                        default:
                            _action = GetFieldManagerAction.FIELD_VALUE;
                            break;
                    }
                }
                catch
                {
                    _action = GetFieldManagerAction.FIELD_VALUE;
                }
                #endregion

                #region Paramètres obligatoires
                try
                {
                    if (_action == GetFieldManagerAction.MAIL_TEMPLATE)
                    {
                        _catId = Int32.Parse(parameters["catId"].ToString());
                        _fieldDescId = Int32.Parse(parameters["fieldDescId"].ToString());
                    }
                    else
                    {
                        _fileId = Int32.Parse(parameters["fileId"].ToString());
                        _tabDescId = Int32.Parse(parameters["tabDescId"].ToString());
                        _fieldDescId = Int32.Parse(parameters["fieldDescId"].ToString());
                    }

                    _memoId = parameters["memoId"].ToString();
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
                #endregion
            }
        }

        private GetFieldManagerContext _getContext = null;

        #region Propriétés renvoyées en sortie

        private FieldFormat _eFormat = FieldFormat.TYP_CHAR;
        private String _displayValue = String.Empty;
        private String _dbValue = String.Empty;
        private String _mailTemplateName = String.Empty;
        private String _mailTemplateBody = String.Empty;
        private String _mailTemplateBodyCSS = String.Empty;
        private bool _mailTemplateBodyIsHTML = true;

        #endregion

        #region Accesseurs pour les propriétés renvoyées en sortie


        /// <summary>
        /// Valeur affichable "human readable"
        /// </summary>
        public String DisplayValue
        {
            get { return _displayValue; }
        }

        /// <summary>
        /// Format eudonet du champ
        /// </summary>
        public FieldFormat FormatValue
        {
            get
            {
                return _eFormat;
            }
        }

        /// <summary>
        /// Valeur stockée en base
        /// </summary>
        public String DbValue
        {
            get { return _dbValue; }
        }

        public String MailTemplateName
        {
            get { return _mailTemplateName; }
        }

        public String MailTemplateBody
        {
            get { return _mailTemplateBody; }
        }

        public String MailTemplateBodyCSS
        {
            get { return _mailTemplateBodyCSS; }
        }

        public bool MailTemplateBodyIsHTML
        {
            get { return _mailTemplateBodyIsHTML; }
        }

        #endregion

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            eudoDAL dal = null;

            try
            {
                dal = eLibTools.GetEudoDAL(_pref);
                dal.OpenDatabase();
                Process(dal, _context);
            }
            catch (eEndResponseException) { }
            catch (System.Threading.ThreadAbortException) { }
            catch (Exception exp)
            {
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);
                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", exp.Message, Environment.NewLine, "Exception StackTrace :", exp.StackTrace);

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg));
            }
            finally
            {
                if (dal != null)
                    dal.CloseDatabase();

                DoResponse();
            }
        }

        /// <summary>
        /// Récupère les paramètres d'exécution à partir d'un tableau de clés/valeurs et effectue le traitement
        /// </summary>
        /// <param name="dal">Objet d'accès à la base de données</param>
        /// <param name="context">Contexte HTTP contenant les paramètres nécessaires à l'exécution du processus</param>
        public void Process(eudoDAL dal, HttpContext context)
        {
            #region Récupération des paramètres

            String error = String.Empty;

            _getContext = new GetFieldManagerContext();
            _getContext.Load(context, out error);

            if (error.Length > 0)
                return;

            #endregion

            if (!DoGet(dal))
                return;
        }

        /// <summary>
        /// Récupère les paramètres d'exécution à partir d'un tableau de clés/valeurs et effectue le traitement
        /// Requiert de passer les préférences car cette méthode est destinée à être appelée hors contexte HTTP
        /// </summary>
        /// <param name="pref">Objet Préférences</param>
        /// <param name="dal">Objet d'accès à la base de données</param>
        /// <param name="nvc">Objet contenant les paramètres nécessaires à l'exécution du processus</param>
        public void Process(ePref pref, eudoDAL dal, NameValueCollection nvc)
        {
            #region Récupération des paramètres

            String error = String.Empty;

            _pref = pref;
            _getContext = new GetFieldManagerContext();
            _getContext.Load(nvc, out error);

            if (error.Length > 0)
                return;

            #endregion

            if (!DoGet(dal))
                return;
        }

        /// <summary>
        /// Récupère les informations demandées à partir des paramètres renseignés dans le contexte (lui-même initialisé par Process)
        /// </summary>
        /// <param name="dal">Objet d'accès à la base de données</param>
        /// <returns>true si la valeur a pu être récupérée, false sinon</returns>
        public Boolean DoGet(eudoDAL dal)
        {
            Boolean gotValue = false;
            EudoQuery.EudoQuery subQuery = null;

            try
            {
                #region Récupération de modèle de mail v7
                if (_getContext.Action == GetFieldManagerAction.MAIL_TEMPLATE)
                {
                    eMailTemplate mailTemplate = new eMailTemplate(dal, _pref.User, _getContext.FieldDescId, 0, _getContext.CatId, out _errorMsg);
                    _mailTemplateName = mailTemplate.MailTemplateName;
                    _mailTemplateBody = mailTemplate.MailTemplateBody;
                    _mailTemplateBodyIsHTML = mailTemplate.MailTemplateBodyIsHTML;
                    _mailTemplateBodyCSS = mailTemplate.MailTemplateBodyCSS;
                    gotValue = true;
                }
                #endregion

                #region Récupération de la valeur d'un champ
                else
                {
                    if (_getContext.FieldDescId > 0 && _getContext.TabDescId > 0 && _getContext.FileId > 0)
                    {
                        IDictionary<Int32, eFieldRecord> flds =
                            eDataFillerGeneric.GetFieldsValue(_pref, new HashSet<Int32>() { _getContext.FieldDescId }, _getContext.TabDescId, _getContext.FileId);

                        eFieldRecord eFldRow = null;
                        if (flds.TryGetValue(_getContext.FieldDescId, out eFldRow))
                        {
                            _displayValue = eFldRow.DisplayValue;
                            _dbValue = eFldRow.Value;

                            _eFormat = eFldRow.FldInfo.Format;
                            gotValue = true;
                        }
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, e.ToString());
                gotValue = false;
            }
            finally
            {
                if (subQuery != null)
                    subQuery.CloseQuery();
            }

            return gotValue;
        }

        private void DoResponse()
        {
            XmlDocument xmlDocReturn = new XmlDocument();

            XmlNode xmlNodeEdnResult = xmlDocReturn.CreateElement("ednResult");
            xmlDocReturn.AppendChild(xmlNodeEdnResult);

            XmlNode xmlNodeSuccess = xmlDocReturn.CreateElement("success");
            xmlNodeEdnResult.AppendChild(xmlNodeSuccess);

            XmlNode xmlNode = null;

            #region Gestion d'erreur

            if (ErrorContainer.IsSet)
            {
                xmlNodeSuccess.InnerText = "0";

                xmlNode = xmlDocReturn.CreateElement("error");
                if (ErrorContainer.DebugMsg.Length > 0)
                    xmlNode.InnerText = ErrorContainer.DebugMsg.ToHtml();

                xmlNodeEdnResult.AppendChild(xmlNode);
            }

            #endregion

            else
            {
                xmlNodeSuccess.InnerText = "1";

                #region Valeur retournée

                xmlNode = xmlDocReturn.CreateElement("action");
                xmlNode.InnerText = _getContext.Action.ToString();
                xmlNodeEdnResult.AppendChild(xmlNode);

                if (_getContext.Action == GetFieldManagerAction.MAIL_TEMPLATE)
                {
                    xmlNode = xmlDocReturn.CreateElement("catId");
                    xmlNode.InnerText = _getContext.CatId.ToString();
                    xmlNodeEdnResult.AppendChild(xmlNode);

                    xmlNode = xmlDocReturn.CreateElement("descId");
                    xmlNode.InnerText = _getContext.FieldDescId.ToString();
                    xmlNodeEdnResult.AppendChild(xmlNode);

                    xmlNode = xmlDocReturn.CreateElement("targetId");
                    xmlNode.InnerText = "0".ToString(); // TODO
                    xmlNodeEdnResult.AppendChild(xmlNode);

                    xmlNode = xmlDocReturn.CreateElement("name");
                    xmlNode.InnerText = _mailTemplateName;
                    xmlNodeEdnResult.AppendChild(xmlNode);

                    xmlNode = xmlDocReturn.CreateElement("body");
                    xmlNode.InnerText = _mailTemplateBody;
                    xmlNodeEdnResult.AppendChild(xmlNode);

                    xmlNode = xmlDocReturn.CreateElement("bodyCSS");
                    xmlNode.InnerText = _mailTemplateBodyCSS;
                    xmlNodeEdnResult.AppendChild(xmlNode);

                    xmlNode = xmlDocReturn.CreateElement("bodyIsHTML");
                    xmlNode.InnerText = (_mailTemplateBodyIsHTML ? "1" : "0");
                    xmlNodeEdnResult.AppendChild(xmlNode);
                }
                else
                {
                    xmlNode = xmlDocReturn.CreateElement("dbvalue");
                    xmlNode.InnerText = _dbValue;
                    xmlNodeEdnResult.AppendChild(xmlNode);

                    xmlNode = xmlDocReturn.CreateElement("value");
                    xmlNode.InnerText = _displayValue;
                    xmlNodeEdnResult.AppendChild(xmlNode);
                }

                xmlNode = xmlDocReturn.CreateElement("memoId");
                xmlNode.InnerText = _getContext.MemoId;
                xmlNodeEdnResult.AppendChild(xmlNode);

                #endregion
            }

            LaunchError();
            RenderResult(RequestContentType.XML, delegate() { return xmlDocReturn.OuterXml; });
        }
    }
}