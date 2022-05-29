using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Xml;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Xrm.eda;
using Com.Eudonet.Internal.eda;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Com.Eudonet.Xrm
{
    /// <className>eAdminGetFieldManager</className>
    /// <summary>Récupère les infos des champs d'une base</summary>
    /// <purpose>A appeler depuis un JavaScript par ex.</purpose>
    /// <authors>MAB</authors>
    /// <date>2021-05-12</date>
    public class eAdminGetFieldManager : eAdminManager
    {
        #region Classes pour serialisation et deserialisation

        [DataContract]
        private class eAdminGetFieldManagerResult
        {
            [DataMember]
            public bool Success;
            [DataMember]
            public string Error;
            [DataMember]
            public object Data;
        }
        #endregion

        /// <summary>
        /// Types d'actions disponibles pour ce manager
        /// </summary>
        public enum AdminGetFieldManagerAction
        {
            /// <summary>
            /// récupération d'une liste de champs (action par défaut)
            /// </summary>
            FIELD_LIST,
            /// <summary>
            /// Récupération des données d'un catalogue
            /// </summary>
            CATALOG_DATA
        }

        /// <summary>
        ///  Objet permettant de charger le contexte (paramètres d'exécution) pour l'exécution du processus de récupération de données
        /// </summary>
        public class AdminGetFieldManagerContext
        {
            #region Propriétés

            private AdminGetFieldManagerAction _action = AdminGetFieldManagerAction.FIELD_LIST;
            private int _tabDescId = 0;
            private int _descId = 0;

            #endregion

            #region Accesseurs

            /// <summary>
            /// Action à effectuer
            /// A passer en paramètre depuis l'appelant, sous forme de String
            /// FIELD_LIST : récupérer une liste de champs pour le TabID spécifié (par défaut)
            /// CATALOG_DATA : récupère les informations du catalogue correspondant au DescId indiqué
            /// </summary>
            public AdminGetFieldManagerAction Action
            {
                get { return _action; }
            }
            /// <summary>
            /// ID de l'onglet concerné
            /// </summary>
            public int TabDescId
            {
                get { return _tabDescId; }
            }
            /// <summary>
            /// DescID du champ concerné
            /// </summary>
            public int DescId
            {
                get { return _descId; }
            }

            #endregion

            /// <summary>
            /// Objet permettant de charger le contexte (paramètres d'exécution) pour l'exécution du processus de récupération de données
            /// </summary>
            public AdminGetFieldManagerContext()
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
                        case "CATALOG_DATA":
                            _action = AdminGetFieldManagerAction.CATALOG_DATA;
                            break;
                        case "FIELD_LIST":
                        default:
                            _action = AdminGetFieldManagerAction.FIELD_LIST;
                            break;
                    }
                }
                catch
                {
                    _action = AdminGetFieldManagerAction.FIELD_LIST;
                }
                #endregion

                #region Paramètres obligatoires
                try
                {
                    if (_action == AdminGetFieldManagerAction.FIELD_LIST)
                    {
                        _tabDescId = int.Parse(parameters["tabDescId"].ToString());
                    }
                    else if (_action == AdminGetFieldManagerAction.CATALOG_DATA)
                    {
                        _descId = int.Parse(parameters["descId"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
                #endregion
            }
        }

        private AdminGetFieldManagerContext _getContext = null;

        #region Accesseurs pour les propriétés renvoyées en sortie

        /// <summary>
        /// Liste des champs liés au TabID indiqué
        /// </summary>
        public List<eAdminFieldInfos> FieldList { get; set; }

        /// <summary>
        /// Données du catalogue liées au DescId indiqué
        /// </summary>
        public eCatalog CatalogData { get; set; }

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

            _getContext = new AdminGetFieldManagerContext();
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
            _getContext = new AdminGetFieldManagerContext();
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
        public bool DoGet(eudoDAL dal)
        {
            EudoQuery.EudoQuery subQuery = null;
            bool returnValue = false;

            try
            {
                #region Récupération de la liste des champs
                if (_getContext.Action == AdminGetFieldManagerAction.FIELD_LIST)
                {
                    if (_getContext.TabDescId > 0)
                    {
                        string sErrorMsg = String.Empty;
                        FieldList = eAdminFieldInfos.GetFieldsList(_pref, dal, _getContext.TabDescId, out sErrorMsg);
                        if (!String.IsNullOrEmpty(sErrorMsg))
                            throw new Exception(sErrorMsg);
                    }

                    returnValue = FieldList != null;
                }
                #endregion

                #region Récupération des informations d'un catalogue
                if (_getContext.Action == AdminGetFieldManagerAction.CATALOG_DATA)
                {
                    if (_getContext.DescId > 0)
                    {
                        string sErrorMsg = String.Empty;
                        CatalogData = new eCatalog(dal, _pref, PopupType.DATA, _pref.User, _getContext.DescId);
                        if (!String.IsNullOrEmpty(sErrorMsg))
                            throw new Exception(sErrorMsg);
                    }

                    returnValue = CatalogData != null;
                }
                #endregion
            }
            catch (Exception e)
            {
                ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, e.ToString());
                FieldList = null;
                CatalogData = null;
                returnValue = false;
            }
            finally
            {
                if (subQuery != null)
                    subQuery.CloseQuery();
            }

            return returnValue;
        }

        private void DoResponse()
        {
            eAdminGetFieldManagerResult result = new eAdminGetFieldManagerResult()
            {
                Error = ErrorContainer?.Msg,
            };

            if (_getContext.Action == AdminGetFieldManagerAction.FIELD_LIST)
            {
                result.Success = FieldList != null;
                result.Data = FieldList;
            }
            else if (_getContext.Action == AdminGetFieldManagerAction.CATALOG_DATA)
            {
                result.Success = CatalogData != null;
                result.Data = CatalogData;
            }

            RenderResult(RequestContentType.TEXT, delegate ()
            {
                return JsonConvert.SerializeObject(result);
            });
        }
    }
}