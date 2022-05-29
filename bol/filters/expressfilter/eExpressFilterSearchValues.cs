using System;
using System.Xml;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm
{



    /// <summary>
    /// Classe de gestion de récupération des valeurs de filtres express
    /// </summary>
    public abstract class eExpressFilterSearchValues
    {
        #region propriétés

        /// <summary>
        /// pref sql de la base
        /// </summary>
        protected ePrefUser _prefUser = null;


        /// <summary>
        /// Information utilisateur
        /// </summary>
        protected eUserInfo _uInfos
        {
            get
            {
                return _prefUser.User;
            }
        }

        /// <summary>
        /// Dal commun ouvert
        /// </summary>
        protected eudoDAL _dal = null;

        /// <summary>
        /// Représentation de la demande de filtre express
        /// </summary>
        protected ExpressFilterQuery _ExpressQuery;
        
        /// <summary>
        /// Conteneur des erreurs/exception
        /// </summary>
        protected eErrorContainer ErrorContainer = null;

        /// <summary>
        /// document xml de retour contenant les valeurs disponible pour le filtre express
        /// </summary>
        protected XmlDocument _xmlResult = new XmlDocument();

        /// <summary>
        /// Noeud xml principale de <seealso cref="_xmlResult"/>
        /// </summary>
        protected XmlNode _detailsNode = null;

        #endregion

        /// <summary>
        /// Retourne le flux xml des valeurs de filtre express
        /// </summary>
        /// <returns></returns>
        protected abstract void generateValues();

        /// <summary>
        /// intitialise le xml de retour
        /// </summary>
        protected virtual void initXML()
        {
            XmlNode mainNode = _xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
            _xmlResult.AppendChild(mainNode);

            _detailsNode = _xmlResult.CreateElement("expressfilterresult");
            _xmlResult.AppendChild(_detailsNode);
        }

        /// <summary>
        /// Retourne les valeurs du filtres express au format XML
        /// </summary>
        /// <param name="pref">pref Usert</param>
        /// 
        /// <param name="nDescId">Descid du champ</param>
        /// <param name="sSearchValue">Filtre sur les valeurs</param>
        /// 
        /// <param name="nTab">Table principale de la recherche</param>
        /// <param name="nParentTabDescId">Mode signet : descid de la table parente</param>
        /// <param name="nParentTabFileId">Mode signet : fileid de la fiche parente</param>
        /// <param name="bSearchALl">recherche sans tenir compte des parent</param>
        /// <returns></returns>
        public static XmlDocument GetValuesXML(ePrefUser pref, int nDescId, int nTab, string sSearchValue = "", int nParentTabDescId = 0, int nParentTabFileId = 0, bool bSearchALl = false)
        {

            eudoDAL dal = eLibTools.GetEudoDAL(pref);

            try
            {
                string sError = "";
                dal.OpenDatabase();

                //Génération du fieldlite du champ du filtre express
                FieldLite fldSrch = new FieldLite(nDescId);
                fldSrch.ExternalLoadInfo(dal, out sError);
                if (!string.IsNullOrEmpty(sError))
                {
                    throw EudoInternalException.GetEudoInternalException(
                        sTitle: "",
                        sDebugError: dal.InnerException?.Message ?? sError,
                        sShortUserMessage: "Impossible de rechercher les informations sur le filtre express",
                        sDetailUserMessage: "",
                        ex: dal.InnerException
                        );
                }


                ExpressFilterQuery exp = ExpressFilterQuery.GetExpressFilterQuery(fldSrch, sSearchValue: sSearchValue);



                if (fldSrch.Format == FieldFormat.TYP_USER)
                    return GetXmlResult<eExpressFilterSearchUsersValues>(pref, exp, dal);
                else if (fldSrch.Format == FieldFormat.TYP_CHAR && fldSrch.Popup != PopupType.NONE && fldSrch.Popup != PopupType.SPECIAL)
                    return GetXmlResult<eExpressFilterSearchCatalogValues>(pref, exp, dal);
                else
                {
                    //catalogue spéciaux
                    exp.StartTab = nTab;
                    exp.ParentTabDescId = nParentTabDescId;
                    exp.ParentTabFileId = nParentTabFileId;
                    exp.SearchAll = bSearchALl;

                    return GetXmlResult<eExpressFilterSearchFilesValues>(pref, exp, dal);
                }
            }
            finally
            {
                dal.CloseDatabase();
            }
        }

        /// <summary>
        /// Retourne le flux xml des valeurs de filtre express
        /// </summary>        
        /// <param name="pref">pref de l'utilisateur</param>
        /// <param name="exFq">ExpressFilterQuery du champ de recherche</param>
        /// <param name="dal">EudoDal ouvert sur la base</param>
        private static XmlDocument GetXmlResult<T>(
            ePrefUser pref,
            ExpressFilterQuery exFq,
            eudoDAL dal
            ) where T : eExpressFilterSearchValues, new()
        { 
            try
            {

                eExpressFilterSearchValues t = new T();
                t._prefUser = pref;
                t._dal = dal;
                t._ExpressQuery = exFq;

                t.initXML();
                t.generateValues();

                return t._xmlResult;
            }
            catch (EudoException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new EudoException(e.Message, "Une erreur est survenue", e, true);
            }
            finally
            {

            }
        }
    }


    /// <summary>
    /// Critère de recherche des valeurs de filtres express
    /// </summary>
    public class ExpressFilterQuery
    {

        #region properties

        int _nStartTab = 0;

        int _nParentTabDescId = 0;
        int _nParentTabFileId = 0;

        FieldLite _fldSearch;

        string _sSearchValue = "";

        bool _bSearchAll = false;
        #endregion

        #region accesseurs
        /// <summary>
        /// Table de départ du filtre
        /// </summary>
        public int StartTab
        {
            get
            {
                return _nStartTab;
            }

            set
            {
                _nStartTab = value;
            }
        }

        /// <summary>
        /// DescId de la table du champ filtre express
        /// </summary>
        public int SearchFieldTab
        {
            get
            {
                return _fldSearch?.Table.DescId ?? 0;
            }
        }


        /// <summary>
        /// descid du champ de recherch
        /// </summary>
        public int SearchDescId
        {
            get
            {
                return _fldSearch.Descid;
            }
        }


        /// <summary>
        /// FieldLite du champ de recherche
        /// </summary>
        public FieldLite FldSearch
        {
            get
            {
                return _fldSearch;
            }
        }


        /// <summary>
        /// DescId de la table parente de celle de départ de la recherche
        /// </summary>
        public int ParentTabDescId
        {
            get
            {
                return _nParentTabDescId;
            }

            set
            {
                _nParentTabDescId = value;
            }
        }


        /// <summary>
        /// Id de la fiche du <seealso cref="ParentTabDescId"/>
        /// </summary>
        public int ParentTabFileId
        {
            get
            {
                return _nParentTabFileId;

            }

            set
            {
                _nParentTabFileId = value;
            }
        }


        /// <summary>
        /// Valeur de filtre pour les valeurs du filtre express
        /// </summary>
        public string SearchValue
        {
            get
            {
                return _sSearchValue;
            }
        }


        /// <summary>
        /// Recherche sur toutes les fiches
        /// </summary>
        public bool SearchAll
        {
            get
            {
                return _bSearchAll;
            }

            set
            {
                _bSearchAll = value;
            }
        }


        #endregion

        /// <summary>
        /// Retourne un ExpressFilterQuery
        /// </summary>
        /// <param name="fld">Champ du filtre express</param>        
        /// <param name="sSearchValue">Valeur de fltre des valeurs du filtre express</param>
        /// <returns>ExpressFilterQuery construit</returns>
        public static ExpressFilterQuery GetExpressFilterQuery(FieldLite fld, string sSearchValue = "")
        {

            var z = new ExpressFilterQuery();
            z._fldSearch = fld;
            z._sSearchValue = sSearchValue;


            return z;
        }


        /// <summary>
        /// constructeur privé
        /// </summary>
        private ExpressFilterQuery()
        {

        }

    }


}