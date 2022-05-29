using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Data;
using System.Collections.Generic;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <className>eNav</className>
    /// <summary>Classe des option de navigation (nav bar, menu droite, ...</summary>
    /// <purpose>Retourne les informations permettant de générer les menus de navigation</purpose>
    /// <authors>SPH</authors>
    /// <date>2011-09-11</date>
    public class eNav
    {

        #region Variables locales

        private string _sqlInstance = string.Empty;
        private string _baseName = string.Empty;
        private int _userId = 0;

        private string _lang = string.Empty;

        private ePref _pref;

        private  Dictionary<int, HrefLink> _listFavorite;


        #endregion


        /// <summary>
        /// Menu de navigation
        /// </summary>
        /// <param name="sqlInstance">instance de la base cliente</param>
        /// <param name="baseName">nom de la base cliente</param>
        /// <param name="userId">id de l'utilisateur</param>
        /// <param name="lang">Langue de l'utilisateur</param>
        /// <param name="pref">objet préférence</param>
        public eNav(string sqlInstance, string baseName, int userId, string lang, ePref pref)
        {
            _sqlInstance = sqlInstance;
            _baseName = baseName;
            _userId = userId;
            _lang = lang;
            _pref = pref;
        }
        
        /// <summary>
        /// Recherche si l'utilisateur à des reports(export, publipostage...) disponible en téléchargement 
        /// anisi que les rapports d'import
        /// </summary>
        /// <returns>true/false </returns>
        public bool hasReport()
        {
            return HasServerReport() || HasImportReport();     
        }
        
        /// <summary>
        /// Contient des rapports d'export du serveur 
        /// (Recherche dans ServerReports)
        /// </summary>
        /// <returns></returns>
        private bool HasServerReport()
        {   
            // Recherche si le user à des reports disponibles
            RqParam rqReportCount = new RqParam();
            rqReportCount.SetQuery("SELECT COUNT(1)  FROM SERVERREPORTS  WHERE ISNULL(deleted,0) = 0 AND basename = @BASENAME AND UserId = @USERID");
            rqReportCount.AddInputParameter("@BASENAME", SqlDbType.VarChar, _baseName);
            rqReportCount.AddInputParameter("@USERID",  SqlDbType.Int, _userId);

            // Base EudoTrait
            eudoDAL eDal = ePrefTools.GetDefaultEudoDal("EUDOTRAIT", _sqlInstance);
            return InternalHasReport(rqReportCount, eDal);
        }

        /// <summary>
        /// Contient des rapports d'import sur la base pour l'utilisateur s'il n'est pas admin ou tous les rapport d'import si il est admin 
        /// (rechcerche dans PJ)
        /// </summary>
        /// <returns></returns>
        private bool HasImportReport()
        {
            // Recherche si le user à des reports d'import disponibles
            RqParam rqReportCount = new RqParam();
            rqReportCount.SetQuery("SELECT COUNT(1) FROM PJ  WHERE (AddedBy = @UserId OR @AtLeastAdminLevel = 1) AND Type = @Type");
            rqReportCount.AddInputParameter("@Type", SqlDbType.Int, (int)PjType.IMPORT_REPORTS);
            rqReportCount.AddInputParameter("@UserId", SqlDbType.Int, _userId);
            rqReportCount.AddInputParameter("@AtLeastAdminLevel", SqlDbType.Int, _pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN);

            // Base de l'utilisateur
            eudoDAL eDal = eLibTools.GetEudoDAL(_pref);
            return InternalHasReport(rqReportCount, eDal);
        }

        /// <summary>
        /// Execution de la requete de count
        /// </summary>
        /// <param name="rqReportCount">requete de count</param>
        /// <param name="eDal">couche d'acces aux données</param>
        /// <returns>vrai s'il y a des rapport, sinon faux</returns>
        private static bool InternalHasReport(RqParam rqReportCount, eudoDAL eDal)
        {
            using (eDal)
            {
                eDal.OpenDatabase();
                string sError;
                int nbReport = eDal.ExecuteScalar<int>(rqReportCount, out sError);
                return (nbReport > 0);
            }
        }

        /// <summary>
        /// Retourne la liste des liens favoris de la page d'accueil
        /// </summary>
        /// <returns></returns>
        public  Dictionary<int, HrefLink> getFavoriteLink()
        {

            //Si la liste des favoris n'a pas été initialisée, alors la charger depuis la base
            if (_listFavorite == null)
            {
                _listFavorite = new Dictionary<int, HrefLink>();


                string lstFavoritesUser = string.Empty;
                string lstFavoritesDef = string.Empty;


                lstFavoritesDef = _pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[] { eLibConst.CONFIG_DEFAULT.FAVORITES})[eLibConst.CONFIG_DEFAULT.FAVORITES];
                lstFavoritesUser = _pref.GetConfig(eLibConst.PREF_CONFIG.FAVORITES);

                if (!string.IsNullOrEmpty(lstFavoritesDef))
                {
                    if (!string.IsNullOrEmpty(lstFavoritesUser))
                        lstFavoritesUser += ";";

                    lstFavoritesUser += lstFavoritesDef;
                }

                if (!string.IsNullOrEmpty(lstFavoritesUser))
                    lstFavoritesUser = ";" + lstFavoritesUser + ";";

                eudoDAL _dal = eLibTools.GetEudoDAL(_pref);
                _dal.OpenDatabase();

                // Recherche si le user à des reports disponibles
                RqParam rqReportList = new RqParam();
                rqReportList.SetQuery(@"SELECT [type], [libelle],[value], [HpgId] FROM [HOMEPAGE] 
                                        WHERE CHARINDEX( ';' + CAST(HpgId AS VARCHAR(10)) +';',@IDFAVORITES) > 0 
                                        ORDER BY [libelle] ASC ");
                rqReportList.AddInputParameter("@IDFAVORITES", System.Data.SqlDbType.VarChar, lstFavoritesUser);

                string sError;
                DataTableReaderTuned edtr = _dal.Execute(rqReportList, out sError);
                try
                {
                    if (string.IsNullOrEmpty(sError) && edtr != null && edtr.HasRows)
                    {
                        while (edtr.Read())
                        {
                            int idFavorite;
                            int nType;


                            //
                            if (int.TryParse(edtr.GetString("HpgId"), out idFavorite) && !_listFavorite.ContainsKey(idFavorite))
                            {
                                HrefLink link = new HrefLink();

                                link.href = edtr.GetString("value");
                                link.target = string.Empty;
                                link.label = edtr.GetString("libelle");
                                link.id = idFavorite;

                                nType = edtr.GetEudoNumeric("type");

                                link.type = nType;

                                switch (nType)
                                {
                                    case 0:
                                        break;
                                    case 1: //Liens favoris - Fichier local
                                        if (link.href.StartsWith("file"))
                                            link.prefix = "";
                                        else
                                            link.prefix = "file:///";
                                        break;
                                    case 2: //Liens favoris - E-Mail
                                        link.prefix = "mailto:";
                                        break;
                                    case 3: //Liens favoris - Site web
                                        if (link.href.StartsWith("https") || link.href.StartsWith("http"))
                                            link.prefix = "";
                                        else
                                            link.prefix = "http://";
                                        link.target = "blank";
                                        break;
                                    case 4: //Liens favoris - Site FTP
                                        if (link.href.StartsWith("ftps") || link.href.StartsWith("ftp"))
                                            link.prefix = "";
                                        else
                                            link.prefix = "ftp://";
                                        link.target = "blank";
                                        break;
                                    case 6: //Liens favoris - Page ASP (masquée)
                                        link.prefix = "";
                                        link.target = "blank";
                                        break;
                                    default:
                                        link.prefix = "";
                                        break;
                                }

                                _listFavorite[idFavorite] = link;
                            }

                        }
                    }
                }
                finally
                {
                    if (edtr != null)
                        edtr.Dispose();
                    _dal.CloseDatabase();
                }

            }

            return _listFavorite;
        }
    }


    /// <summary>
    /// Structure de représentation d'un link hypertext
    /// </summary>
    public struct HrefLink
    {
        /// <summary>
        /// Type de lient
        /// </summary>
        public int type;

        /// <summary>
        /// Label du lien
        /// </summary>
        public string label;

        /// <summary>
        /// préfixe de protocole du lien (ftp, http..)
        /// </summary>
        public string prefix;

        /// <summary>
        /// Hyperlien
        /// </summary>
        public string href;

        /// <summary>
        /// Attribut target
        /// </summary>
        public string target;

        /// <summary>
        /// Id du lien
        /// </summary>
        public int id;
    }


 
}