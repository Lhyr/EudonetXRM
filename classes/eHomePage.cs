using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.list;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Merge;
using Com.Eudonet.Common.Cryptography;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// classe de gestion de page d'accueil
    /// GCH le 19/11/2012
    /// </summary>
    public class eHomePage
    {
        /// <summary>Objet d'accès à la BDD</summary>
        private eudoDAL _dal;
        /// <summary>Identifiant de la page d'accueil sélectionnée</summary>
        private Int32 _nHomePageId = 0;
        /// <summary>Type d'Eudopart</summary>
        private eConst.HOMEPAGE_DISPO _config;
        /// <summary>Titre de la page d'accueil</summary>
        private string _sTitle = string.Empty;
        /// <summary>Ordre d'affichage des id des eudopart</summary>
        private string _strEudoPartOrder;
        /// <summary>Liste des identifiants des eudoparts sélectionnées</summary>
        private List<Int32> _partsId = new List<Int32>();
        private List<Int32> _defPartsId = new List<Int32>();
        private List<Int32> _customPartsId = new List<int>();
        /// <summary>Indique que les eudoparts doivent être figées</summary>
        private Boolean _bLocked = false;
        /// <summary>Indique que les eudoparts doivent être en lecture seule</summary>
        private Boolean _bReadOnly = false;
        /// <summary>Liste des EudoParts</summary>
        private List<eEudoPart> _eudoParts = new List<eEudoPart>();
        /// <summary>Largeur de la zone d'affichage de la HomePage</summary>
        private Int32 _nWidth = eConst.DEFAULT_WINDOW_WIDTH;
        /// <summary>Hauteur de la zone d'affichage de la HomePage</summary>
        private Int32 _nHeight = eConst.DEFAULT_WINDOW_HEIGHT;
        /// <summary>Hauteur de la zone d'affichage des eudopart</summary>
        private Int32 _nHeightForEudoParts = eConst.DEFAULT_WINDOW_HEIGHT;

        private String _sExprMsg = String.Empty;
        private ePref _pref = null;

        /// <summary>Position min de haut d'eudopart</summary>
        private const Int32 TOP_PAGE = 5;
        /// <summary>Position min de gauche d'eudopart</summary>
        private const Int32 LEFT_PAGE = 5;
        /// <summary>Position min de bas d'eudopart</summary>
        private const Int32 BOTTOM_PAGE = 5;
        /// <summary>Position min de droite d'eudopart</summary>
        private const Int32 RIGHT_PAGE = 5;
        /// <summary>Espace vertical entre les eudoparts</summary>
        private const Int32 SPACE_BETWEEN_PART_HEIGHT = 5;
        /// <summary>Espace Horizontal entre les eudoparts</summary>
        private const Int32 SPACE_BETWEEN_PART_WIDTH = 5;

        /// <summary>hauteur du messg express</summary>
        private const Int32 EXPR_MSG_HEIGHT = 40;

        private Int32 _nbEudoParts = 0;
        /// <summary>
        /// Nombre d'Eudoparts
        /// </summary>
        public int NbEudoParts
        {
            get
            {
                return _nbEudoParts;
            }

            set
            {
                _nbEudoParts = value;
            }
        }




        /// <summary>
        /// constructeur
        /// </summary>
        /// <param name="dal">Objet d'accès à la BDD</param>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="width">LARGEUR de la zone d'affichage de la HOMEPAGE</param>
        /// <param name="height">LARGEUR de la zone d'affichage de la HOMEPAGE</param>
        /// <param name="bReadOnly">fenêtre non modifiable</param>
        /// <param name="homePageId">Id de la homepage affichée</param>
        public eHomePage(eudoDAL dal, ePref pref, Int32 width, Int32 height, bool bReadOnly, Int32 homePageId = 0)
        {
            _dal = dal;

            _nWidth = width;
            _nHeight = height;
            _nHeightForEudoParts = height;
            _bReadOnly = bReadOnly;
            _pref = pref;
            string sqlError = string.Empty;
            string eudoPartOrder = string.Empty;
            string eudoPartCustom = string.Empty;
            string err = string.Empty;


            #region Message Express

            Boolean bHideHomePageFooter = _pref.GetConfigDefault(new HashSet<eLibConst.CONFIG_DEFAULT> { eLibConst.CONFIG_DEFAULT.HIDEHOMEPAGEFOOTER })[eLibConst.CONFIG_DEFAULT.HIDEHOMEPAGEFOOTER] == "1";

            if (!bHideHomePageFooter)
            {
                string sId = _pref.GetConfig(eLibConst.PREF_CONFIG.EXPRESSMSG);


                String sExprMsgSql = "SELECT TOP 1 [HOMEPAGE].[Value]" + Environment.NewLine +
                    "FROM [HOMEPAGE] INNER JOIN [CONFIG] ON [CONFIG].[ExpressMsg] = [HOMEPAGE].[HpgId] " + Environment.NewLine +
                    "WHERE [HOMEPAGE].Type = @exprMsg AND [CONFIG].[UserId] IN (@userid,0)" + Environment.NewLine +
                    "ORDER BY [CONFIG].UserId DESC";


                RqParam rqExprMsg = new RqParam(sExprMsgSql);

                rqExprMsg.AddInputParameter("@exprMsg", SqlDbType.Int, eConst.HOMEPAGE_TYPE.HPG_MSG_EXPRESS.GetHashCode());
                rqExprMsg.AddInputParameter("@userid", SqlDbType.Int, pref.User.UserId);

                var dtr = dal.Execute(rqExprMsg, out err);
                if (err.Length > 0)
                    throw dal.InnerException ?? new Exception(err);

                if (dtr.HasRows && dtr.Read())
                    _sExprMsg = dtr.GetString(0);
                else
                    _sExprMsg = "";
            }

            #endregion

            if (_bReadOnly)
            {
                _nHomePageId = homePageId;
                _strEudoPartOrder = string.Empty;
            }
            else
            {
                _nHomePageId = eLibTools.GetNum(pref.GetConfig(eLibConst.PREF_CONFIG.ADVANCEDHOMEPAGEID));
                _strEudoPartOrder = pref.GetConfig(eLibConst.PREF_CONFIG.EUDOPARTORDER);


                Int32 nDefaultHomePageId = eLibTools.GetNum(pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[] { eLibConst.CONFIG_DEFAULT.ADVANCEDHOMEPAGEID })[eLibConst.CONFIG_DEFAULT.ADVANCEDHOMEPAGEID]);


                eudoPartCustom = pref.GetConfig(eLibConst.PREF_CONFIG.EUDOPARTCUSTOM);

                //Page d'accueuil de groupe

                if (_nHomePageId == 0 && nDefaultHomePageId == 0)
                {
                    RqParam rqGroupHpg = new RqParam("Select [AdvancedHomepageId], [GroupLevel] from [user] left join [Group] on [user].[groupid] = [group].[groupid]  where userid = @UserId ");
                    rqGroupHpg.AddInputParameter("@UserId", SqlDbType.Int, pref.User.UserId);

                    DataTableReaderTuned dtr = _dal.Execute(rqGroupHpg, out sqlError);
                    try
                    {

                        if (dtr.Read())
                        {
                            _nHomePageId = dtr.GetEudoNumeric("AdvancedHomepageId");
                        }
                    }
                    finally
                    {
                        if (dtr != null)
                            dtr.Dispose();
                    }
                }


                if (_nHomePageId == 0)
                {
                    _nHomePageId = nDefaultHomePageId;
                }
            }

            string sqlHp = "Select * From [HomePageAdvanced] where [HomePageId] =@HomePageId";



            RqParam rq = new RqParam(sqlHp);
            rq.AddInputParameter("@HomePageId", SqlDbType.Int, _nHomePageId);

            bool bHideHeader = false;
            DataTableReaderTuned dtrPg = _dal.Execute(rq, out err);
            try
            {
                if (dtrPg == null || !dtrPg.Read())
                {
                    //Pas de pages Web de définies pour l'utilisateur en cours
                    return;
                }

                _sTitle = dtrPg.GetString("HomePageTitle");
                _bLocked = (dtrPg.GetString("Locked") == "1");
                _config = (eConst.HOMEPAGE_DISPO)dtrPg.GetEudoNumeric("HomePageConfig");

                bHideHeader = (dtrPg.GetString("HideHeaderEnabled") == "1");

                //Récupère les Eudoparts de la page d'accueil définie par l'administrateur (sans les customs)
            }
            finally
            {
                if (dtrPg != null)
                    dtrPg.Dispose();
            }
            string sql = string.Concat("Select [EudoPart].[EudoPartId],[HomePageAdvanced].[Locked] From [HomePageAdvanced] left Join",
                  " [EudoPartRelation] on [HomePageAdvanced].[HomePageId] = [EudoPartRelation].[HomePageId] left Join",
                  " [EudoPart] on [EudoPartRelation].[EudoPartId] = [EudoPart].[EudoPartId]  where",
                  " [HomePageAdvanced].[HomePageId] = @HomePageId Order By [EudoPartRelation].[EudoPartOrder]");
            rq = new RqParam(sql);
            rq.AddInputParameter("@HomePageId", SqlDbType.Int, _nHomePageId);

            dtrPg = _dal.Execute(rq, out err);
            try
            {
                if (dtrPg == null || !dtrPg.HasRows)
                {
                    //Pas de pages Web de définies pour l'utilisateur en cours
                    return;
                }
                Int32 partId = 0;
                while (dtrPg.Read())
                {
                    partId = dtrPg.GetEudoNumeric("EudoPartId");
                    _defPartsId.Add(partId);
                }
            }
            finally
            {
                if (dtrPg != null)
                    dtrPg.Dispose();
            }

            //  CRU: On ne prend plus en compte la colonne EUDOPARTCUSTOM de la table CONFIG(retiré en V7)
            if (!string.IsNullOrEmpty(eudoPartCustom) && !_bReadOnly)
            {

                _partsId = _defPartsId;


                try
                {
                    string[] aParts = eudoPartCustom.Split(';');
                    var custPar = new List<Int32>(aParts.Select<string, Int32>(q => eLibTools.GetNum(q)));



                    for (var i = 0; i < _partsId.Count; i++)
                    {
                        if (_partsId[i] == 0)
                        {
                            for (var j = 0; j < custPar.Count; j++)
                            {
                                if (custPar[j] != 0 && !_partsId.Contains(custPar[j]))
                                    _partsId[i] = custPar[j];
                            }
                        }
                    }

                    _partsId = _partsId.OrderBy(a => (";" + eudoPartCustom + ";").IndexOf(";" + a.ToString() + ";")).ToList();
                }
                catch (Exception e)
                {
                    string s = e.Message;
                }
            }
            else
            {
                _partsId = _defPartsId;
            }
            /*

            if (!String.IsNullOrEmpty(eudoPartCustom))
            {
                string[] aParts = eudoPartCustom.Split(';');
                _customPartsId = new List<Int32>(aParts.Select<string, Int32>(q => eLibTools.GetNum(q)));
            }

            _partsId = _defPartsId;*/




            if (!String.IsNullOrEmpty(_sExprMsg))
                _nHeightForEudoParts -= EXPR_MSG_HEIGHT + SPACE_BETWEEN_PART_HEIGHT;

            Int32 nParts = 0;

            Int32 Pos1Top = TOP_PAGE;
            Int32 Pos1Left = LEFT_PAGE;
            Int32 nWidth_EudoPart_1 = 0;
            Int32 nHeight_EudoPart_1 = 0;

            Int32 Pos2Top = TOP_PAGE;
            Int32 Pos2Left = (_nWidth / 2) + LEFT_PAGE + SPACE_BETWEEN_PART_WIDTH;
            Int32 nWidth_EudoPart_2 = 0;
            Int32 nHeight_EudoPart_2 = 0;

            Int32 Pos3Top = (_nHeightForEudoParts / 2) + TOP_PAGE;
            Int32 Pos3Left = LEFT_PAGE;
            Int32 nWidth_EudoPart_3 = 0;
            Int32 nHeight_EudoPart_3 = 0;

            Int32 Pos4Top = (_nHeightForEudoParts / 2) + TOP_PAGE + SPACE_BETWEEN_PART_HEIGHT;
            Int32 Pos4Left = (_nWidth / 2) + LEFT_PAGE + SPACE_BETWEEN_PART_WIDTH;
            Int32 nWidth_EudoPart_4 = 0;
            Int32 nHeight_EudoPart_4 = 0;



            switch (this._config)
            {
                case eConst.HOMEPAGE_DISPO.HPG_DISPO_1: //UNE SEULE GRANDE
                    nParts = 1;


                    nWidth_EudoPart_1 = _nWidth - LEFT_PAGE - RIGHT_PAGE;
                    nHeight_EudoPart_1 = _nHeightForEudoParts - TOP_PAGE - BOTTOM_PAGE;
                    break;
                case eConst.HOMEPAGE_DISPO.HPG_DISPO_2H2V:  //4
                    nParts = 4;

                    nWidth_EudoPart_1 = (_nWidth / 2) - (SPACE_BETWEEN_PART_WIDTH / 2) - LEFT_PAGE;
                    nHeight_EudoPart_1 = (_nHeightForEudoParts / 2) - (SPACE_BETWEEN_PART_HEIGHT / 2) - TOP_PAGE;

                    nWidth_EudoPart_2 = (_nWidth / 2) - (SPACE_BETWEEN_PART_WIDTH / 2) - RIGHT_PAGE;
                    nHeight_EudoPart_2 = (_nHeightForEudoParts / 2) - (SPACE_BETWEEN_PART_HEIGHT / 2) - TOP_PAGE;

                    Pos2Top = TOP_PAGE;
                    Pos2Left = nWidth_EudoPart_1 + SPACE_BETWEEN_PART_WIDTH + LEFT_PAGE;

                    nWidth_EudoPart_3 = (_nWidth / 2) - (SPACE_BETWEEN_PART_WIDTH / 2) - LEFT_PAGE;
                    nHeight_EudoPart_3 = (_nHeightForEudoParts / 2) - (SPACE_BETWEEN_PART_HEIGHT / 2) - BOTTOM_PAGE;

                    Pos3Top = nHeight_EudoPart_2 + SPACE_BETWEEN_PART_HEIGHT + TOP_PAGE;
                    Pos3Left = LEFT_PAGE;

                    nWidth_EudoPart_4 = (_nWidth / 2) - (SPACE_BETWEEN_PART_WIDTH / 2) - RIGHT_PAGE;
                    nHeight_EudoPart_4 = (_nHeightForEudoParts / 2) - (SPACE_BETWEEN_PART_HEIGHT / 2) - BOTTOM_PAGE;

                    Pos4Top = nHeight_EudoPart_2 + SPACE_BETWEEN_PART_HEIGHT + TOP_PAGE;
                    Pos4Left = nWidth_EudoPart_1 + SPACE_BETWEEN_PART_WIDTH + LEFT_PAGE;


                    // dans le cas de l'affichage en deux par deux, tous les eudoparts on les memes dimensions
                    nWidth_EudoPart_2 = nWidth_EudoPart_1;
                    nHeight_EudoPart_2 = nHeight_EudoPart_1;

                    nWidth_EudoPart_3 = nWidth_EudoPart_1;
                    nHeight_EudoPart_3 = nHeight_EudoPart_1;

                    nWidth_EudoPart_4 = nWidth_EudoPart_1;
                    nHeight_EudoPart_4 = nHeight_EudoPart_1;


                    break;
                case eConst.HOMEPAGE_DISPO.HPG_DISPO_2H:    //2 HORIZONTALES
                    nParts = 2;

                    nWidth_EudoPart_1 = _nWidth - LEFT_PAGE - RIGHT_PAGE;
                    nHeight_EudoPart_1 = (_nHeightForEudoParts / 2) - (SPACE_BETWEEN_PART_HEIGHT / 2) - TOP_PAGE;

                    nWidth_EudoPart_2 = _nWidth - LEFT_PAGE - RIGHT_PAGE;
                    nHeight_EudoPart_2 = (_nHeightForEudoParts / 2) - (SPACE_BETWEEN_PART_HEIGHT / 2) - BOTTOM_PAGE;


                    Pos2Top = nHeight_EudoPart_1 + SPACE_BETWEEN_PART_HEIGHT + TOP_PAGE;
                    Pos2Left = LEFT_PAGE;
                    break;
                case eConst.HOMEPAGE_DISPO.HPG_DISPO_2V:    //2 VERTICALES
                    nParts = 2;

                    nWidth_EudoPart_1 = (_nWidth / 2) - (SPACE_BETWEEN_PART_WIDTH / 2) - LEFT_PAGE;
                    nHeight_EudoPart_1 = _nHeightForEudoParts - TOP_PAGE - BOTTOM_PAGE;

                    nWidth_EudoPart_2 = (_nWidth / 2) - (SPACE_BETWEEN_PART_WIDTH / 2) - RIGHT_PAGE;
                    nHeight_EudoPart_2 = _nHeightForEudoParts - TOP_PAGE - BOTTOM_PAGE;
                    Pos2Top = TOP_PAGE;
                    Pos2Left = nWidth_EudoPart_1 + SPACE_BETWEEN_PART_WIDTH + LEFT_PAGE;
                    break;
                case eConst.HOMEPAGE_DISPO.HPG_DISPO_12V:   //1 HORIZONTALE et 2 VERTICALE
                    nParts = 3;

                    //HORIZONTALE
                    nWidth_EudoPart_1 = _nWidth - LEFT_PAGE - (SPACE_BETWEEN_PART_WIDTH / 2) - RIGHT_PAGE;
                    nHeight_EudoPart_1 = (_nHeightForEudoParts / 2) - (SPACE_BETWEEN_PART_HEIGHT / 2) - TOP_PAGE;

                    //2 petites
                    nWidth_EudoPart_2 = (_nWidth / 2) - (SPACE_BETWEEN_PART_WIDTH / 2) - LEFT_PAGE;
                    nHeight_EudoPart_2 = (_nHeightForEudoParts / 2) - (SPACE_BETWEEN_PART_HEIGHT / 2) - BOTTOM_PAGE;
                    Pos2Top = nHeight_EudoPart_1 + SPACE_BETWEEN_PART_HEIGHT + TOP_PAGE;
                    Pos2Left = LEFT_PAGE;

                    nWidth_EudoPart_3 = (_nWidth / 2) - (SPACE_BETWEEN_PART_WIDTH / 2) - RIGHT_PAGE;
                    nHeight_EudoPart_3 = (_nHeightForEudoParts / 2) - (SPACE_BETWEEN_PART_HEIGHT / 2) - BOTTOM_PAGE;
                    Pos3Top = nHeight_EudoPart_1 + SPACE_BETWEEN_PART_HEIGHT + TOP_PAGE;
                    Pos3Left = nWidth_EudoPart_2 + SPACE_BETWEEN_PART_WIDTH + LEFT_PAGE;
                    break;
                case eConst.HOMEPAGE_DISPO.HPG_DISPO_2H1:   //2 HORIZONTALES et 1 VERTICALE
                    nParts = 3;
                    //VERTICALE
                    nWidth_EudoPart_1 = (_nWidth / 2) - (SPACE_BETWEEN_PART_WIDTH / 2) - LEFT_PAGE;
                    nHeight_EudoPart_1 = _nHeightForEudoParts - TOP_PAGE - BOTTOM_PAGE;

                    //2 petites
                    nWidth_EudoPart_2 = (_nWidth / 2) - (SPACE_BETWEEN_PART_WIDTH / 2) - RIGHT_PAGE;
                    nHeight_EudoPart_2 = (_nHeightForEudoParts / 2) - (SPACE_BETWEEN_PART_HEIGHT / 2) - TOP_PAGE;
                    Pos2Top = TOP_PAGE;
                    Pos2Left = nWidth_EudoPart_1 + SPACE_BETWEEN_PART_WIDTH + LEFT_PAGE;

                    nWidth_EudoPart_3 = (_nWidth / 2) - (SPACE_BETWEEN_PART_WIDTH / 2) - RIGHT_PAGE;
                    nHeight_EudoPart_3 = (_nHeightForEudoParts / 2) - (SPACE_BETWEEN_PART_HEIGHT / 2) - BOTTOM_PAGE;
                    Pos3Top = nHeight_EudoPart_2 + SPACE_BETWEEN_PART_HEIGHT + TOP_PAGE;
                    Pos3Left = nWidth_EudoPart_1 + SPACE_BETWEEN_PART_WIDTH + LEFT_PAGE;
                    break;
                default:
                    nParts = 0;
                    break;
            }

            Int32 nCurrentPartid = 0;
            for (Int32 i = 0; i < nParts; i++)
            {
                nCurrentPartid = (_partsId.Count > i) ? _partsId[i] : 0;

                // CRU : Reprise du système de la V7
                /*
                try
                {
                    if (nCurrentPartid == 0)
                        nCurrentPartid = _customPartsId[i];
                }
                catch (Exception) { } */

                //L'eudopart est configurable si le partid = 0 ou si l'eudopartid ne fait pas partie du contenu par défaut de la page
                bool bCustomizable = ((nCurrentPartid == 0) || !_defPartsId.Contains(nCurrentPartid));

                bCustomizable = bCustomizable && !_bReadOnly;

                Int32 nCurrentWidth = 0;
                Int32 nCurrentHeight = 0;
                Int32 nCurrentPosTop = 0;
                Int32 nCurrentPosLeft = 0;
                switch (i)
                {
                    case 0:
                        nCurrentWidth = nWidth_EudoPart_1;
                        nCurrentHeight = nHeight_EudoPart_1;
                        nCurrentPosTop = Pos1Top;
                        nCurrentPosLeft = Pos1Left;
                        break;
                    case 1:
                        nCurrentWidth = nWidth_EudoPart_2;
                        nCurrentHeight = nHeight_EudoPart_2;
                        nCurrentPosTop = Pos2Top;
                        nCurrentPosLeft = Pos2Left;
                        break;
                    case 2:
                        nCurrentWidth = nWidth_EudoPart_3;
                        nCurrentHeight = nHeight_EudoPart_3;
                        nCurrentPosTop = Pos3Top;
                        nCurrentPosLeft = Pos3Left;
                        break;
                    case 3:
                        nCurrentWidth = nWidth_EudoPart_4;
                        nCurrentHeight = nHeight_EudoPart_4;
                        nCurrentPosTop = Pos4Top;
                        nCurrentPosLeft = Pos4Left;
                        break;
                }
                _eudoParts.Add(new eEudoPart(dal, nCurrentWidth, nCurrentHeight, nCurrentPosTop, nCurrentPosLeft, nCurrentPartid, i + 1, bCustomizable, pref, bHideHeader, this._config));
            }

        }
        /// <summary>Retourne le rendu global des eudoparts</summary>
        /// <returns>rendu global des eudoparts</returns>
        public HtmlGenericControl GetPageRender()
        {
            _nbEudoParts = _eudoParts.Count;

            HtmlGenericControl hpg = new HtmlGenericControl("div");
            hpg.ID = "HpContainer";
            hpg.Attributes.Add("class", "HpContainer");
            hpg.Style.Add(HtmlTextWriterStyle.Position, "relative");
            hpg.Attributes.Add("ednIsLocked", (_bLocked) ? "1" : "0");
            hpg.Attributes.Add("ednIsReadonly", (_bReadOnly) ? "1" : "0");
            hpg.Attributes.Add("nBox", _nbEudoParts.ToString());    //Nombre d'eudopart
            if (_nbEudoParts > 0)
            {

                HttpContext context = HttpContext.Current;

                string sBaseSiteV7URL =
                    string.Concat(context.Request.Url.Scheme, "://",

                        context.Request.Url.Authority, "/",
                            eLibTools.GetServerConfig("v7dir").TrimEnd('/'), "/app/specif/",


                        _pref.GetBaseName
                        );
                foreach (eEudoPart ep in _eudoParts)
                {
                    HtmlGenericControl hpr = null;
                    hpr = ep.GetHtmlPartRender();
                    hpg.Controls.Add(hpr);
                    hpr = null;
                }

                #region pointillé affiché en fond lors du déplacement d'eudopart
                Panel divDashedBox = new Panel();
                hpg.Controls.Add(divDashedBox);
                divDashedBox.Attributes.Add("ednDivType", "BackgroundDiv");
                divDashedBox.ID = "DashedBox";
                divDashedBox.Style.Add(HtmlTextWriterStyle.Display, "none");
                divDashedBox.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                divDashedBox.Style.Add(HtmlTextWriterStyle.Top, "0px");
                divDashedBox.Style.Add(HtmlTextWriterStyle.Position, "relative");
                divDashedBox.Style.Add(HtmlTextWriterStyle.Width, "0px");
                divDashedBox.CssClass = "dashedbox";
                divDashedBox = null;
                #endregion

                #region TextBox contenant l'ordre d'affichage des eudopart
                HtmlInputHidden tbCustomEudoPartsId = new HtmlInputHidden();
                hpg.Controls.Add(tbCustomEudoPartsId);
                tbCustomEudoPartsId.ID = "CustomEudoPartsId";
                tbCustomEudoPartsId.Value = "";
                tbCustomEudoPartsId = null;
                #endregion

                context = null;
            }

            if (!String.IsNullOrEmpty(_sExprMsg))
            {
                String sRegSpec = String.Concat("specif/", _pref.GetBaseName, "/(.*)$");

                Regex regExp = new Regex(sRegSpec, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                MatchCollection mc;

                mc = regExp.Matches(_sExprMsg);
                // Url de type Spécif 
                if (mc.Count == 1)
                {
                    // Obtention d'une version "nettoyée" de la Query String
                    // On remplace le premier ? de l'URL par &, puis on insèrera ensuite l'URL vers la page de pont V7 en début de la ligne
                    // avec ses paramètres, pour terminer avec le code original

                    NameValueCollection nvcQS = HttpUtility.ParseQueryString(mc[0].Value);
                    StringBuilder sbNewQS = new StringBuilder();
                    int index = 0;
                    foreach (string key in nvcQS)
                    {
                        if (index == 0)
                            sbNewQS.Append(key.Replace("?", "&")).Append("=").Append(nvcQS[key]);
                        else
                            sbNewQS.Append(key).Append("=").Append(nvcQS[key]);
                        if (index < nvcQS.AllKeys.Length - 1)
                            sbNewQS.Append("&");
                        index++;
                    }

                    _sExprMsg = _sExprMsg.Replace(
                        mc[0].Value,
                        String.Concat(
                            "eExportToV7.aspx?",
                            "type=", eConst.HOMEPAGE_TYPE.HPG_MSG_EXPRESS.GetHashCode(),
                            "&tab=", 0,
                            "&fileid=", 0,
                            "&id=", sbNewQS.ToString()
                        )
                    );

                }

                Int32 nExprMsgPosTop = _nHeight - BOTTOM_PAGE - EXPR_MSG_HEIGHT;
                Int32 nExprMsgPosLeft = LEFT_PAGE;

                HtmlGenericControl divExpressMsg = new HtmlGenericControl("div");
                divExpressMsg.Attributes.Add("class", "ExprMsg");
                divExpressMsg.Style.Add(HtmlTextWriterStyle.Top, string.Concat(nExprMsgPosTop, "px"));
                divExpressMsg.Style.Add(HtmlTextWriterStyle.Left, string.Concat(nExprMsgPosLeft, "px"));

                hpg.Controls.Add(divExpressMsg);
                divExpressMsg.InnerHtml = _sExprMsg;
            }
            return hpg;

        }
    }
    /// <summary>
    /// Objet représentant un EudoPart
    /// </summary>
    public class eEudoPart
    {
        /// <summary>Objet d'accès à la BDD</summary>
        private eudoDAL _dal;
        /// <summary>Objet contenant les préférences de l'utilisateurs en cours)</summary>
        private ePref _pref;

        /// <summary>Identifiant de l'eudopart</summary>
        private Int32 _nId = 0;
        /// <summary>Indique si l'onglet est personnalisable</summary>
        private bool _bCustomizable = false;
        /// <summary>Retourne l'emplacement ou se trouve l'eudopart</summary>
        private Int32 _nOrder = 0;
        /// <summary>Contenu de l'eudopart</summary>
        private string _sContent = string.Empty;
        /// <summary>Type d'eudopart affichée (RSS, graph...)</summary>
        private eConst.EUDOPART_CONTENT_TYPE _type;
        /// <summary>Titre de l'eudopart apparraissant en entête</summary>
        private string _sTitle = string.Empty;
        /// <summary>Indique si l'entete de l'eudopart doit être caché</summary>
        bool _bHideHeader = false;
        /// <summary>Type d'eudopart demandée</summary>
        private eConst.HOMEPAGE_DISPO _pageConfig;

        /// <summary>Largeur de la zone d'affichage de la HomePage</summary>
        private Int32 _nWidth = eConst.DEFAULT_WINDOW_WIDTH;
        /// <summary>Hauteur de la zone d'affichage de l'eudopart</summary>
        private Int32 _nHeight = eConst.DEFAULT_WINDOW_HEIGHT;
        /// <summary>Hauteur de la zone d'affichage du contenu de l'eudopart</summary>
        private Int32 _nHeightBody = eConst.DEFAULT_WINDOW_HEIGHT;
        /// <summary>Position dans le cadre à partir de la bordure haute</summary>
        private Int32 _nPosTop = 0;
        /// <summary>Position dans le cadre à partir de la bordure gauche</summary>
        private Int32 _nPosLeft = 0;

        private const Int32 BORDERBODY = 2;

        private const Int32 TITLE_HEIGHT = 19;
        private Dictionary<string, string> JsParams = new Dictionary<string, string>();

        /// <summary>
        /// constructeur
        /// </summary>
        public eEudoPart(eudoDAL dal, Int32 width, Int32 height, Int32 posTop, Int32 posLeft, int partId, Int32 order, bool bCustomizable, ePref pref, bool bHideHeader, eConst.HOMEPAGE_DISPO pageConfig)
        {
            _pref = pref;
            _nWidth = width;
            _nHeight = height;
            _nPosTop = posTop;
            _nPosLeft = posLeft;

            _nOrder = order;

            _pageConfig = pageConfig;
            // GMA 20140313 : vu avec Raphaël et Bertrand : affichage permanent de la barre de titre
            //_bHideHeader = bHideHeader;
            _dal = dal;
            string err = string.Empty;
            RqParam rq = new RqParam("Select * From [EudoPart] where [EudoPartId] = @EudoPartId");
            rq.AddInputParameter("@EudoPartId", SqlDbType.Int, partId);
            DataTableReaderTuned dtrEp = null;
            try
            {
                dtrEp = _dal.Execute(rq, out err);

                if (!dtrEp.Read())
                {
                    _sTitle = eResApp.GetRes(_pref, 141);
                    _type = eConst.EUDOPART_CONTENT_TYPE.TYP_CONTENT_EMPTY;
                    _sContent = "";
                }
                else
                {
                    _type = (eConst.EUDOPART_CONTENT_TYPE)dtrEp.GetEudoNumeric("EudoPartContentType");
                    _nId = dtrEp.GetEudoNumeric("EudoPartId");
                    _sContent = dtrEp.GetString("EudoPartContent");

                    // dans le cas des mrus le titre est une res (demande 40 176)
                    if (_type == eConst.EUDOPART_CONTENT_TYPE.TYP_CONTENT_MRU)
                    {
                        eRes res = new eRes(pref, _sContent);
                        Int32 iTab = 0;
                        Int32.TryParse(_sContent, out iTab);
                        _sTitle = eResApp.GetRes(pref, 1621).Replace("<TAB>", res.GetRes(iTab));
                    }
                    else
                    {
                        _sTitle = dtrEp.GetString("EudoPartTitle");
                    }
                }
            }
            finally
            {
                if (dtrEp != null)
                {
                    dtrEp.Dispose();
                    dtrEp = null;
                }
                rq = null;
            }
        }



        /// <summary>
        /// rendu Html des Eudoparts selon leurs types
        /// </summary>
        /// <returns></returns>
        public HtmlGenericControl GetHtmlPartRender()
        {
            HtmlGenericControl htmlDiv = new HtmlGenericControl("div");
            Panel divBody = new Panel();
            int _chartId = 0;
            try
            {

                htmlDiv.ID = string.Concat("box", _nOrder);
                htmlDiv.Attributes.Add("ednDivType", "HomePart");
                htmlDiv.Attributes.Add("ednCustom", (_bCustomizable ? "1" : "0"));
                htmlDiv.Attributes.Add("ednExpanded", "0");
                htmlDiv.Attributes.Add("ednOrder", _nOrder.ToString());
                htmlDiv.Attributes.Add("ednPartId", _nId.ToString());
                htmlDiv.Style.Add(HtmlTextWriterStyle.Position, "absolute");
                htmlDiv.Style.Add(HtmlTextWriterStyle.Top, string.Concat(_nPosTop, "px"));
                htmlDiv.Style.Add(HtmlTextWriterStyle.Left, string.Concat(_nPosLeft, "px"));
                htmlDiv.Style.Add(HtmlTextWriterStyle.Width, string.Concat(_nWidth, "px"));
                htmlDiv.Style.Add(HtmlTextWriterStyle.Height, string.Concat(_nHeight, "px"));
                htmlDiv.Style.Add(HtmlTextWriterStyle.ZIndex, "2");
                htmlDiv.Attributes.Add("class", "box");
                htmlDiv.Attributes.Add("onclick", "try{setEudopartSelected(this.id,1,true);}catch(e){}");


                //Ajout du contenu de l'eudopart
                htmlDiv.Controls.Add(divBody);
                divBody.Attributes.Add("endVisible", "1");
                divBody.Attributes.Add("ednPartType", _type.GetHashCode().ToString());
                divBody.Attributes.Add("ednPartId", _nId.ToString());
                divBody.Attributes.Add("ednDivType", "BoxBodyDiv");
                _nHeightBody = _nHeight - ((!_bHideHeader && _pageConfig.GetHashCode() > 1) ? TITLE_HEIGHT : 0) - BORDERBODY;
                //_nHeightBody = _nHeight - TITLE_HEIGHT - BORDERBODY;
                divBody.Style.Add(HtmlTextWriterStyle.Height, string.Concat(_nHeightBody, "px"));
                divBody.ID = string.Concat("BodyPart", _nOrder);
                divBody.CssClass = "boxbody";
                //if ((_pageConfig.GetHashCode() <= 1) || _bHideHeader)
                //    divBody.CssClass = string.Concat(divBody.CssClass, " boxbodyFULL");
                //GMA 20140304 : commenté car les coins supérieurs doivent être carrés (vu avec Marianne)

                HtmlGenericControl divContent = null;
                Literal ControlContent = null;
                string err = string.Empty;
                try
                {
                    switch (this._type)
                    {
                        case eConst.EUDOPART_CONTENT_TYPE.TYP_CONTENT_HTML:
                            ControlContent = new Literal();
                            ControlContent.Text = GetHtmlRender(_sContent, _nId);
                            divBody.Controls.Add(ControlContent);
                            break;
                        case eConst.EUDOPART_CONTENT_TYPE.TYP_CONTENT_CHART:
                            _chartId = eLibTools.GetNum(_sContent.Replace(";", ""));
                            eReport report = new eReport(_pref, _chartId);
                            if (report != null && report.LoadFromDB())
                            {
                                divContent = new HtmlGenericControl();
                                Panel Control = new Panel();
                                Control = GetChartReportPanel(_chartId);
                                Control.Controls.Add(renderer.eSyncFusionChartRenderer.GetHtmlChart(_pref, _chartId.ToString()));
                                divContent.Controls.Add(Control);
                            }
                            else
                            {
                                eErrorContainer eErrCont = eErrorContainer.GetDevUserError(
                                    eLibConst.MSG_TYPE.CRITICAL,
                                    eResApp.GetRes(_pref, 72),
                                    report.ErrorMessage,
                                    eResApp.GetRes(_pref, 72),
                                    report.ErrorMessage);

                                eFeedbackXrm.LaunchFeedbackXrm(eErrCont, _pref);
                            }



                            break;
                        case eConst.EUDOPART_CONTENT_TYPE.TYP_CONTENT_SPECIF:
                        case eConst.EUDOPART_CONTENT_TYPE.TYP_CONTENT_WEBPAGE:
                            if (_type == eConst.EUDOPART_CONTENT_TYPE.TYP_CONTENT_SPECIF)
                            {
                                Int32 nSpecId = 0;
                                Int32.TryParse(_sContent, out nSpecId);
                                eSpecif spec = eSpecif.GetSpecif(_pref, nSpecId);
                                if (spec == null)
                                {
                                    throw new Exception(eResApp.GetRes(_pref, 6524));
                                }
                                if (!spec.IsViewable)
                                {
                                    ControlContent = new Literal();
                                    ControlContent.Text = string.Concat("<font size='2' face='Verdana'>", eResApp.GetRes(_pref, 141), "</font>");
                                    divBody.Controls.Add(ControlContent);
                                    break;
                                }

                                string sEncode = ExternalUrlTools.GetCryptEncode("sid=" + spec.SpecifId )  ;

                                _sContent = String.Concat("eSubmitTokenXRM.aspx?t=", sEncode );

                            }
                            else if (!_sContent.ToUpper().StartsWith("HTTP"))
                                _sContent = string.Concat("http://", _sContent);

                            if (_type == eConst.EUDOPART_CONTENT_TYPE.TYP_CONTENT_SPECIF)
                            {
                            }
                            //Acces à Nomination - GCH : voir v7 si demande d'umplémentation, le code n'est ici pas terminé
                            else if (_sContent.Contains("nomination.fr/modules/partenaires/eudonet"))
                            {
                                string sPPPKeyNomination = string.Empty;
                                string sUserMail = _pref.User.UserMail;
                                if (!string.IsNullOrEmpty(sUserMail))
                                {
                                    //yyyymmddXXXX@DDDD.COMeudonet
                                    string sNominationUserMD5 = string.Concat(DateTime.Now.ToString("yyyyMMdd"), sUserMail, "eudonet");
                                    //sNominationUserMD5 = eMD5.EncryptMd5(sNominationUserMD5);
                                    sNominationUserMD5 = HashMD5.GetHash(sNominationUserMD5);

                                    string sErreur = string.Empty;
                                    bool NomiEnabled = eTools.GetNominationAuthenticate(sUserMail, sNominationUserMD5, out sErreur);

                                    //TODO NOMINATION à vérifier et compléter
                                }
                            }
                            else if (_sContent.EndsWith("AdminXRM") && _pref.User.UserLevel >= UserLevel.LEV_USR_ADMIN.GetHashCode())
                            {
                                //ADMIN
                                _sContent = string.Concat("eExportToV7.aspx?id=", _nId, "&type=", eLibConst.SPECIF_TYPE.TYP_ADMIN.GetHashCode());
                            }
                            else
                            {
                                //RegExp de la spécif
                                string sRegSpec = string.Concat("(^https?://([^/]*/)+app/specif/", _pref.GetBaseName, "/)(.*)$");

                                Regex regExp = new Regex(sRegSpec, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                MatchCollection mc;

                                mc = regExp.Matches(_sContent);
                                // Url de type Spécif 
                                if (mc.Count == 1)
                                    _sContent = string.Concat("eExportToV7.aspx?id=", _nId, "&type=", eLibConst.SPECIF_TYPE.TYP_EUDOPART.GetHashCode());
                            }
                            divContent = new HtmlGenericControl("iframe");
                            divContent.Style.Add(HtmlTextWriterStyle.Width, "100%");
                            divContent.Style.Add(HtmlTextWriterStyle.Height, "99%");
                            divContent.Style.Add(HtmlTextWriterStyle.BorderWidth, "0px");
                            divContent.Attributes.Add("src", _sContent);
                            divBody.Controls.Add(divContent);
                            break;
                        case eConst.EUDOPART_CONTENT_TYPE.TYP_CONTENT_RSS:
                            string[] aParams = _sContent.Split(new string[] { "&||&" }, StringSplitOptions.None);
                            string rssUrl = aParams[0];
                            bool withTitle = (aParams[1] == "1");
                            ControlContent = new Literal();
                            ControlContent.Text = GetRssFlow(rssUrl, withTitle);
                            divBody.Controls.Add(ControlContent);
                            aParams = null;
                            break;
                        case eConst.EUDOPART_CONTENT_TYPE.TYP_CONTENT_MRU:
                            Int32 nTab = eLibTools.GetNum(_sContent);
                            divBody.Attributes.Add("ednPartTab", nTab.ToString());
                            ControlContent = new Literal();
                            ControlContent.Text = GetMruList(_pref, nTab, out err);
                            divBody.Controls.Add(ControlContent);

                            if (err.Length > 0)
                            {
                                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, err), _pref);
                            }
                            break;
                        default:
                            ControlContent = new Literal();
                            ControlContent.Text = string.Concat("<font size='2' face='Verdana'>", eResApp.GetRes(_pref, 141), "</font>");
                            divBody.Controls.Add(ControlContent);
                            break;
                    }
                }
                catch (NotImplementedException) //En construction
                {
                    ControlContent = null;
                    HtmlImage img = new HtmlImage();

                    divContent = new HtmlGenericControl("CENTER");
                    img.Src = String.Concat("themes/", _pref.ThemePaths.GetImageWebPath("/images/underConstruction.png")); //LOGO
                    divContent.Controls.Add(img);
                    img = null;
                }
                if (ControlContent != null)
                    divBody.Controls.Add(ControlContent);
                ControlContent = null;
                if (divContent != null)
                    divBody.Controls.Add(divContent);
                divContent = null;
                //*******************************

                //divBody = null;
            }
            catch (Exception ex)
            {
                string sDevMsg = string.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);
                sDevMsg = string.Concat(sDevMsg, Environment.NewLine, "Exception : ", ex.ToString());
                string sUsrMsg = string.Concat("<br>", eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544));

                eErrorContainer eErrCont = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   sUsrMsg,  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   sDevMsg);
                eFeedbackXrm.LaunchFeedbackXrm(eErrCont, _pref);
                htmlDiv.Controls.Add(new LiteralControl(string.Concat(eResApp.GetRes(_pref, 72), "<br/>", sUsrMsg)));
                sDevMsg = null;
                sUsrMsg = null;
            }

            #region Barre de titre en footer (titre, gestion plein écran, drag 'n drop...)
            if (_pageConfig.GetHashCode() > 1 || _bCustomizable)
            {
                HtmlTable tabTitle = null;
                HtmlTableRow trTitle = null;
                HtmlTableCell tdTitle = null;
                if (_bHideHeader)
                {
                    tabTitle = new HtmlTable();
                    htmlDiv.Controls.Add(tabTitle);
                    tabTitle.Style.Add(HtmlTextWriterStyle.Position, "absolute");
                    tabTitle.Style.Add(HtmlTextWriterStyle.Top, String.Concat(_nHeight - TITLE_HEIGHT, "px"));
                    tabTitle.Style.Add(HtmlTextWriterStyle.Width, "100%");
                    tabTitle.Style.Add(HtmlTextWriterStyle.Height, "10px");
                    tabTitle.Attributes.Add("onmouseover", string.Concat("document.getElementById('tr_", _nId, "').style.display='block';document.getElementById('", divBody.ID, "').style.height='", (_nHeightBody - TITLE_HEIGHT), "px';"));
                    tabTitle.Attributes.Add("onmouseout", string.Concat("document.getElementById('tr_", _nId, "').style.display='none';document.getElementById('", divBody.ID, "').style.height='", _nHeightBody, "px';"));
                    //tabTitle.Attributes.Add("onmouseover", string.Concat("document.getElementById('tr_", _nId, "').style.display='block';"));
                    //tabTitle.Attributes.Add("onmouseout", string.Concat("document.getElementById('tr_", _nId, "').style.display='none';"));
                    trTitle = new HtmlTableRow();
                    tabTitle.Controls.Add(trTitle);
                    tdTitle = new HtmlTableCell();
                    trTitle.Controls.Add(tdTitle);
                    tdTitle.InnerHtml = "&nbsp;";
                }

                HtmlGenericControl divTitle = new HtmlGenericControl("div");
                htmlDiv.Controls.Add(divTitle);

                tabTitle = new HtmlTable();
                divTitle.Controls.Add(tabTitle);
                tabTitle.Border = 0;
                tabTitle.CellPadding = 0;
                tabTitle.CellSpacing = 0;
                tabTitle.Width = "100%";
                trTitle = new HtmlTableRow();
                tabTitle.Controls.Add(trTitle);
                trTitle.ID = string.Concat("tr_", _nId);

                //Si titre caché
                if (_bHideHeader)
                {
                    trTitle.Style.Add(HtmlTextWriterStyle.Display, "none");
                    trTitle.Attributes.Add("onmouseover", string.Concat("document.getElementById('tr_", _nId, "').style.display='block';document.getElementById('", divBody.ID, "').style.height='", (_nHeightBody - TITLE_HEIGHT), "px';"));
                    trTitle.Attributes.Add("onmouseout", string.Concat("document.getElementById('tr_", _nId, "').style.display='none';document.getElementById('", divBody.ID, "').style.height='", _nHeightBody, "px';"));
                    //trTitle.Attributes.Add("onmouseover", string.Concat("document.getElementById('tr_", _nId, "').style.display='block';"));
                    //trTitle.Attributes.Add("onmouseout", string.Concat("document.getElementById('tr_", _nId, "').style.display='none';"));
                }
                #region Coin gauche
                tdTitle = new HtmlTableCell();
                trTitle.Controls.Add(tdTitle);
                tdTitle.Attributes.Add("class", "CornerTdLeft");
                tdTitle.ID = string.Concat("BoxLeftCorner", _nOrder);
                tdTitle.InnerHtml = "&nbsp;";
                #endregion
                #region Milieu - Logo type d'eudopart
                tdTitle = new HtmlTableCell();
                trTitle.Controls.Add(tdTitle);
                tdTitle.Attributes.Add("class", "TypePart");
                tdTitle.ID = string.Concat("BoxType", _nOrder);
                tdTitle.Style.Add(HtmlTextWriterStyle.Width, string.Concat(TITLE_HEIGHT, "px"));
                //tdTitle.Style.Add(HtmlTextWriterStyle.VerticalAlign, "top");
                //HtmlImage imgTtlLogo = new HtmlImage();
                HtmlGenericControl imgTtlLogo = new HtmlGenericControl();
                tdTitle.Controls.Add(imgTtlLogo);
                String eudopartIcon = "";
                switch (_type.GetHashCode())
                {
                    case 1: eudopartIcon = "icon-desktop"; break;
                    case 2: eudopartIcon = "icon-file-text"; break;
                    case 3: eudopartIcon = "icon-bar-chart"; break;
                    case 4: eudopartIcon = "icon-globe"; break;
                    case 5: eudopartIcon = "icon-rss"; break;
                    case 6: eudopartIcon = "icon-file-text"; break;
                }
                imgTtlLogo.Attributes.Add("class", eudopartIcon);
                #endregion
                #region Milieu - Titre
                tdTitle = new HtmlTableCell();
                trTitle.Controls.Add(tdTitle);
                tdTitle.Attributes.Add("class", "ClEudopartTitle");
                //tdTitle.Attributes.Add("ondblclick", string.Concat("showEudoPart(", _nId, ");")); //agrandit l'eudopart
                tdTitle.Attributes.Add("ondblclick", string.Concat("if(" + _nOrder + "!=0){expandPart('box", _nOrder, "','BodyPart", _nOrder, "');}")); //agrandit l'eudopart
                tdTitle.ID = string.Concat("BoxTitle", _nOrder);
                //tdTitle.Style.Add(HtmlTextWriterStyle.Width, "75%");
                tdTitle.InnerHtml = string.Concat(_sTitle, "&nbsp;");
                #endregion
                #region Milieu - Redimensionnement
                tdTitle = new HtmlTableCell();
                trTitle.Controls.Add(tdTitle);
                if (_bCustomizable)
                    tdTitle.Style.Add(HtmlTextWriterStyle.Width, "60px");
                tdTitle.Style.Add(HtmlTextWriterStyle.TextAlign, "right");
                tdTitle.Style.Add(HtmlTextWriterStyle.VerticalAlign, "top");
                Panel divAction = new Panel();
                tdTitle.Controls.Add(divAction);
                divAction.ID = string.Concat("ControlBox", _nOrder);
                divAction.CssClass = "controlbox";
                HtmlGenericControl imgTitleAction;
                if (_bCustomizable)
                {
                    #region ACTION MODIFIER EUDOPART
                    imgTitleAction = new HtmlGenericControl();
                    divAction.Controls.Add(imgTitleAction);
                    imgTitleAction.Attributes.Add("onclick", "EudoPartConfig(this)");   //TODO HOMEPAGE : sera à traiter lors de l'administration
                    //imgTitleAction.Src = string.Concat("themes/", _pref.ThemePaths.GetImageWebPath("/images/iHomePage/action_config_out.png"));
                    imgTitleAction.Attributes.Add("class", "icon-wrench");
                    #endregion
                }
                #region ACTION REDIMENSIONNEMENT
                imgTitleAction = new HtmlGenericControl();
                divAction.Controls.Add(imgTitleAction);
                imgTitleAction.Attributes.Add("onclick", string.Concat("if(" + _nOrder + "!=0){expandPart('box", _nOrder, "','BodyPart", _nOrder, "');}"));
                //imgTitleAction.Src = string.Concat("themes/", _pref.ThemePaths.GetImageWebPath("/images/iHomePage/action_minus_out.png"));
                imgTitleAction.Attributes.Add("class", "icon-clone");
                #endregion

                #region ACTION MODE LISTE
                if (_type == eConst.EUDOPART_CONTENT_TYPE.TYP_CONTENT_CHART)
                {
                    imgTitleAction = new HtmlGenericControl();
                    divAction.Controls.Add(imgTitleAction);
                    imgTitleAction.Attributes.Add("onclick", String.Concat("goNav(", _chartId, ",'')"));
                    imgTitleAction.Attributes.Add("class", "icon-edn-list");
                    imgTitleAction.Attributes.Add("title", eResApp.GetRes(_pref, 23));
                }
                #endregion

                #endregion
                #region Coin droit
                tdTitle = new HtmlTableCell();
                trTitle.Controls.Add(tdTitle);
                tdTitle.Attributes.Add("class", "CornerTdRight");
                tdTitle.ID = string.Concat("BoxRightCorner", _nOrder);
                tdTitle.InnerHtml = "&nbsp;";
                #endregion

                tdTitle = null;
                trTitle = null;
                tabTitle = null;
                divTitle = null;
            }
            #endregion
            return htmlDiv;

        }

        /// <summary>
        /// Retourne une Eudopart de type MRU
        /// </summary>
        /// <param name="pref">objet preference de l'utilisateur en cours</param>
        /// <param name="_tab">desc de la table de liste à afficher</param>
        /// <param name="err"> si une erreur se produit elle se trouve là</param>
        /// <returns>Contenu HTML de l'eudopart</returns>
        private string GetMruList(ePref pref, Int32 _tab, out string err)
        {
            RqParam rqTableMRU = new RqParam();
            StringBuilder sbQuery = new StringBuilder();



            err = string.Empty;
            StringBuilder _sbRender = new StringBuilder();

            try
            {


                #region Init de la liste de MRU
                sbQuery.Append("SELECT [PREF].[Tab], [PREF].[MruFileId], [DESC].[Type], ")
                    .Append("   (SELECT TOP 1 [PTab].[CalendarEnabled] FROM [PREF] AS [PTab]")
                    .Append("   WHERE [PREF].[Tab] = [PTab].[Tab] AND [PTab].[UserId] = 0 ORDER BY [PTab].[PrefId]) AS [CalendarEnabled]")
                    .Append(" FROM [PREF]")
                    .Append("   INNER JOIN [DESC] ON [PREF].[Tab] = [DESC].[DescId]")
                    .Append(" WHERE [PREF].[UserId] = @userid AND [PREF].[Tab] = ").Append(_tab);

                rqTableMRU.SetQuery(sbQuery.ToString());
                rqTableMRU.AddInputParameter("@userid", SqlDbType.Int, pref.User.UserId);

                DataTableReaderTuned dtrTableMRU = _dal.Execute(rqTableMRU, out err);
                string sMruList = string.Empty;
                try
                {
                    // Erreur
                    if (!string.IsNullOrEmpty(err))
                    {
                        err = string.Concat("eEudoPart.GetMRUList.DataTableReader dtrTableMRU  : ", Environment.NewLine, err);
                        return eResApp.GetRes(pref, 72);
                    }

                    if (dtrTableMRU == null || !dtrTableMRU.Read())
                    {
                        sMruList = "0";
                    }
                    else
                    {

                        Int32 tabType;
                        if (!Int32.TryParse(dtrTableMRU.GetString(2), out tabType))
                        {
                            err = "eEudoPart.GetMRUList.DataTableReader dtrTableMRU : Impossible de récupérer le type de Fichier.";
                            return eResApp.GetRes(pref, 72);
                        }

                        EdnType tabEdnType = (EdnType)tabType;

                        if (tabEdnType != EdnType.FILE_MAIN)
                        {
                            err = "eEudoPart.GetMRUList.DataTableReader dtrTableMRU : Fichier non principal !";
                            return eResApp.GetRes(pref, 72);
                        }

                        sMruList = dtrTableMRU.GetString(1);

                    }
                }
                finally
                {
                    if (dtrTableMRU != null)
                        dtrTableMRU.Dispose();
                }
                List<Int32> mruIds = new List<int>();
                string[] tabMru = sMruList.Split(";");
                Int32 currentid = 0;
                for (int i = 0; i < tabMru.Length; i++)
                    if (Int32.TryParse(tabMru[i], out currentid))
                        mruIds.Add(currentid);
                #endregion

                #region Récupération de la liste de type HomePageList

                eRenderer mainList = null;
                #region Init de la liste
                eHomePageListRenderer hpgRend = new eHomePageListRenderer(_dal, _pref, _tab, mruIds, _nWidth, _nHeightBody, _nOrder);
                if (!string.IsNullOrEmpty(hpgRend.ErrorMsg))
                {
                    err = string.Concat("eEudoPart.GetMRUList.new eHomePageListRenderer : ", Environment.NewLine, hpgRend.ErrorMsg);
                    return eResApp.GetRes(pref, 72);
                }

                mainList = eRendererFactory.CreateHomePageRenderer(hpgRend);
                if (!string.IsNullOrEmpty(hpgRend.ErrorMsg))
                {
                    err = string.Concat("eEudoPart.GetMRUList.eRenderer.CreateHomePageRenderer : ", Environment.NewLine, hpgRend.ErrorMsg);
                    return eResApp.GetRes(pref, 72);
                }
                #endregion
                //Si pas d'erreur on retourne la liste sinon on retourne une textbox cachée contenant l'erreur
                if (mainList.ErrorMsg.Length == 0)
                {
                    Panel mainDivMRU = mainList.PgContainer;

                    mainDivMRU.Attributes.Add("onClick", string.Concat("goMruFile(event);"));  //ocf : evennement, Si click

                    //RENDU dans la div
                    _sbRender = new StringBuilder();
                    StringWriter sw = new StringWriter(_sbRender);
                    HtmlTextWriter tw = new HtmlTextWriter(sw);

                    mainDivMRU.RenderControl(tw);
                    return _sbRender.ToString();
                }
                else
                {
                    err = string.Concat("eEudoPart.GetMRUList.mainList.ErrorMsg : ", Environment.NewLine, mainList.ErrorMsg);
                    return eResApp.GetRes(pref, 72);
                }

                #endregion

            }
            catch (Exception esseption)
            {
                err = string.Concat("eEudoPart.GetMRUList : ", Environment.NewLine, esseption.ToString());

                return eResApp.GetRes(pref, 72);
            }
            finally
            {

            }
        }


        private string GetRssFlow(string _rssUrl, bool _withTitle)
        {
            if (_rssUrl.Trim().Equals(string.Empty))
            {
                return "<Div class='Rss'>Aucun flux Rss!</div>";
            }
            else
            {
                try
                {

                    //GCH : bug #35582 Certains Flux n'étaient pas lisible alors qu'en v7 oui
                    /*Nouvelle méthode*/
                    XmlDocument doc = eLibTools.GetWebData(_rssUrl);
                    /***************/
                    /*Ancienne Méthode*/
                    //XmlDocument doc = GetXmlDataOld(_rssUrl);
                    /******************/

                    string strBase = string.Empty;
                    string strChannelTitle = string.Empty;
                    string strChannelLink = string.Empty;
                    string strChannelDescription = string.Empty;
                    if (_withTitle)
                    {
                        foreach (XmlNode strEntry in doc.SelectNodes("//channel/*"))
                        {
                            if (strEntry.Name.ToLower() == "title")
                                strChannelTitle = strEntry.InnerText;
                            else
                            {
                                if (strEntry.Name == "link")
                                    strChannelLink = strEntry.InnerText;
                                else
                                {
                                    if (strEntry.Name == "description")
                                    {
                                        strChannelDescription = strEntry.InnerText;
                                        strBase = string.Concat(strBase, "<p><a href='", strChannelLink, "' class='h1a' target='new'>", strChannelTitle, "</a></p>");
                                        strBase = string.Concat(strBase, "<p>", strChannelDescription, "</p>");
                                        strBase = string.Concat(strBase, "<hr>");
                                    }
                                }
                            }
                        }
                    }

                    string strNews = string.Empty;
                    string strItemTitle = string.Empty;
                    string strItemLink = string.Empty;
                    string strItempubDate = string.Empty;
                    string strItemDescription = string.Empty;

                    foreach (XmlNode oItem in doc.SelectNodes("//item/*"))
                    {
                        if (oItem.Name == "title")
                            strItemTitle = oItem.InnerText;
                        else
                            if (oItem.Name == "link")
                            strItemLink = oItem.InnerText;
                        else
                                if (oItem.Name == "pubDate")
                        {
                            DateTime dt;
                            if (DateTime.TryParse(oItem.InnerText, out dt))
                            {
                                if (dt.Date == DateTime.Now.Date)
                                    strItempubDate = dt.ToString("hh:mm");
                                else
                                    strItempubDate = eDate.ConvertBddToDisplay(_pref.CultureInfo, dt);
                            }
                            else
                                strItempubDate = oItem.InnerText;
                        }
                        else
                                    if (oItem.Name == "description")
                        {
                            strItemDescription = oItem.InnerText;
                            strNews = string.Concat(strNews, "<span class='RssDate'>", strItempubDate, "</span>");
                            strNews = string.Concat(strNews, "<p><a target='new' href='", strItemLink, "' class='h2a'>", strItemTitle, "</a></p>");
                            strNews = string.Concat(strNews, strItemDescription, "<br>");
                            strNews = string.Concat(strNews, "<hr>");
                        }
                    }
                    return String.Concat("<Div class='Rss'>", strBase, strNews, "</div>");
                }
                catch (System.Security.SecurityException ex)
                {
                    return String.Concat("<Div class='Rss'>", eResApp.GetRes(_pref, 6459), _rssUrl, Environment.NewLine, ex.Message, "</div>");   //Aucun flux Rss, impossible d'accéder à :
                }
                catch (WebException ex)
                {
                    return String.Concat("<Div class='Rss'>", eResApp.GetRes(_pref, 6459), _rssUrl, Environment.NewLine, ex.Message, "</div>");   //Aucun flux Rss, impossible d'accéder à :
                }
                catch (UriFormatException ex)
                {
                    return String.Concat("<Div class='Rss'>", eResApp.GetRes(_pref, 6376), _rssUrl, Environment.NewLine, ex.Message, "</div>");   //Aucun flux Rss, l'url suivante n'est pas un flux RSS valide :
                }
                catch (Exception ex)
                {
                    string sDevMsg = string.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine
                        , " GetRssFLow (rssUrl : ", _rssUrl, ")", Environment.NewLine);
                    sDevMsg = string.Concat(sDevMsg, Environment.NewLine, "Exception : ", ex.ToString());
                    string sUsrMsg = string.Concat("<br>", eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544));

                    eErrorContainer eErrCont = eErrorContainer.GetDevUserError(
                       eLibConst.MSG_TYPE.CRITICAL,
                       eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                       sUsrMsg,  //  Détail : pour améliorer...
                       eResApp.GetRes(_pref, 72),  //   titre
                       sDevMsg);
                    eFeedbackXrm.LaunchFeedbackXrm(eErrCont, _pref);
                    return string.Concat(eResApp.GetRes(_pref, 72), "<br/>", sUsrMsg);
                }
            }
        }

        ///// <summary>
        ///// Lecture d'un flux rss web en utilisant un HttpWebRequest
        ///// Méthode abandonnée, dans certains flux elle renvoyait l'erreur Les tentatives de redirection automatique ont été trop nombreuses.
        ///// </summary>
        ///// <param name="url">url du flux à lire</param>
        ///// <returns>Format XML</returns>
        /*private static XmlDocument GetXmlDataOld(string url)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);

                // execute the request
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException(String.Concat("XML error: ", response.StatusCode, response.StatusDescription));
                }
                else
                {
                    Stream stream = response.GetResponseStream();
                    try
                    {
                        return eLibTools.StreamToXml(stream);
                    }
                    finally
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                }
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
                request = null;
            }
        }*/
        /// <summary>
        /// Renvoi le Rendu d'un eudopart de type graphique
        /// GCH : bug #35582 Certains Flux n'étaient pas lisible alors qu'en v7 oui
        /// </summary>
        /// <param name="_reportId">Id du graphique demandé</param>
        /// <returns>Rendu généré</returns>
        private string GetChartReport(int _reportId, out int nTab)
        {
            eRenderer chart = eRendererFactory.CreateChartRenderer(_pref, _reportId, true);
            nTab = (((eChartRenderer)chart).ChartReport != null) ? ((eChartRenderer)chart).ChartReport.Tab : 0;
            chart.PgContainer.Style.Add("width", "100%");
            chart.PgContainer.Style.Add("height", "80%");

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter tw = new HtmlTextWriter(sw);
            chart.PgContainer.RenderControl(tw);

            return sb.ToString();
        }

        /// <summary>
        /// Renvoi le Rendu d'un eudopart de type graphique
        /// </summary>
        /// <param name="_reportId">Id du graphique demandé</param>
        /// <param name="nTab"></param>
        /// <returns>Rendu généré</returns>
        private Panel GetChartReportPanel(int _reportId)
        {
            eRenderer chart = eRendererFactory.CreateChartRenderer(_pref, _reportId, bFullSize: true);
            chart.PgContainer.Style.Add("width", "100%");
            chart.PgContainer.Style.Add("height", "95%");
            return chart.PgContainer;
        }


        /// <summary>
        /// Si des balises de fusion sont présentes remplace les balises de fusions par les champs de recherches correspondants. Sinon retourne simplement le même contenu que fournit
        /// </summary>
        /// <param name="content">Contenu HTML qui sera fusionné si des balises sont présentes</param>
        /// <param name="partId">Id de l'eudopart</param>
        /// <returns>Html fusionné si champs de fusions ou même contenu que content si pas de balises de fusion</returns>
        private string GetHtmlRender(string content, int partId)
        {
            //Gestion des formulaires de recherche depuis la page d'accueil
            if (content.ToUpper().Contains(eConst.HTM_TAG_BEGIN))
            {
                List<int> listDescId = new List<int>();
                List<string> listOperator = new List<string>();
                int lFieldCount = 0;
                content = FormMerge(content, ref lFieldCount, ref listDescId, ref listOperator, true, partId);

                content = string.Concat(
                     "<input type=\"hidden\" name=\"formula\" value=\"\"/>"
                    , "<input type=\"hidden\" name=\"countfields_", partId, "\" id=\"countfields_", partId, "\" value=\"", listDescId.Count, "\"/>"
                    , content
                    );
            }
            else
            {
                //Dans le cas d'un champs Html sans fusion aucune modification, il reste tel quel.
            }

            return content;
        }
        /// <summary>
        /// Gestion des formulaires de recherches
        /// </summary>
        /// <param name="content"></param>
        /// <param name="lFieldCount"></param>
        /// <param name="strListDescId"></param>
        /// <param name="strListOperator"></param>
        /// <param name="bMerge"></param>
        /// <param name="strPartId"></param>
        /// <returns>Contenu fusionné</returns>
        private string FormMerge(string content, ref Int32 lFieldCount, ref List<Int32> strListDescId, ref List<string> strListOperator, Boolean bMerge, Int32 strPartId)
        {
            strListDescId = new List<int>();
            strListOperator = new List<string>();
            Int32 nPosBegin, nPosEnd;
            string strTagInnerText, strHTMLValue, strTag, strNewTag, strStyle, strFieldFormat, strDescId;
            Boolean bShowOp;
            lFieldCount = 0;

            nPosBegin = content.ToLower().IndexOf(eConst.HTM_TAG_BEGIN.ToLower());

            if (nPosBegin >= 0)
                bMerge = true;
            else
                bMerge = false;

            // Filtre fictif - Le did de la table est set à chaque rubrique
            AdvFilter filter = AdvFilter.GetNewFilter(_pref, TypeFilter.OTHER, 0);
            AdvFilterContext filterContext = new AdvFilterContext(_pref, _dal, filter);
            // Le filtre fictif se composera uniquement d'un seul critere à chaque fois, donc index 0,0
            AdvFilterLineIndex lineIndex = new AdvFilterLineIndex(0, 0);

            while (nPosBegin >= 0)
            {
                nPosEnd = content.ToUpper().IndexOf(eConst.HTM_TAG_END, nPosBegin);

                if (nPosEnd >= 0)
                {
                    nPosEnd = nPosEnd + eConst.HTM_TAG_END.Length;
                    strTagInnerText = string.Empty;
                    strHTMLValue = string.Empty;
                    strTag = content.Substring(nPosBegin, nPosEnd - nPosBegin);

                    //Récupere la valeur de la propriété pour le champ de fusion
                    string strFieldType = eTools.GetTagPropValue(strTag, eConst.HTM_TAG_FIELDTYPE).ToLower();
                    string strFieldName = eTools.GetTagPropValue(strTag, eConst.HTM_TAG_FIELDNAME).ToLower();
                    strDescId = eTools.GetTagPropValue(strTag, eConst.HTM_TAG_FIELDDESCID).ToLower();
                    int nDescId = eLibTools.GetNum(strDescId);
                    strStyle = eTools.GetTagPropValue(strTag, "style").ToLower();
                    bShowOp = (eTools.GetTagPropValue(strTag, "ednshowoperator") == "1");
                    string strDefaultOp = eTools.GetTagPropValue(strTag, "ednoperator").ToLower();
                    strFieldFormat = eTools.GetTagPropValue(strTag, "ednfieldformat").ToLower();
                    Int32 nDefaultOp = 0;
                    if (!Int32.TryParse(strDefaultOp, out nDefaultOp))
                        nDefaultOp = Operator.OP_CONTAIN.GetHashCode();

                    if (nDescId > 0)
                    {
                        //Champs de fusion
                        strListDescId.Add(nDescId);
                        strListOperator.Add(strDefaultOp);

                        AdvFilterTab filterTab = AdvFilterTab.GetNewEmpty(strPartId, nDescId.ToString());
                        AdvFilterLine filterLine = filterTab.Lines[0];
                        filterLine.LineIndex = lFieldCount;
                        filterLine.DescId = nDescId;
                        filterLine.Operator = (Operator)nDefaultOp;
                        filterLine.LineOperator = InterOperator.OP_AND; //Toujours AND pour les eudopart de recherche

                        // Le filtre fictif se composera uniquement d'un seul critere à chaque fois
                        filter.FilterTab = eLibTools.GetTabFromDescId(nDescId);
                        filter.FilterTabs.Clear();
                        filter.FilterTabs.Add(filterTab);

                        //string dispVal = string.Empty;
                        Control ectrl = null;

                        HtmlGenericControl ctrlField = new HtmlGenericControl("span");
                        ctrlField.Attributes.Add("ednfiltertype", TypeFilter.OTHER.GetHashCode().ToString());
                        ctrlField.Attributes.Add("edntab", eLibTools.GetTabFromDescId(nDescId).ToString());
                        ctrlField.Attributes.Add("edndescid", nDescId.ToString());
                        ctrlField.ID = string.Concat("header_", strPartId, "_", lFieldCount);

                        if (bMerge)
                        {
                            //ComboBox d'Opérateurs
                            HtmlGenericControl eOpCtrl = eFilterLineRenderer.GetOperatorsList(filterContext, lineIndex);

                            if (!bShowOp)
                                eOpCtrl.Style.Add(HtmlTextWriterStyle.Display, "none");
                            ctrlField.Controls.Add(eOpCtrl);

                            //Rendu du champ de saisie
                            ectrl = eFilterLineRenderer.GetValuesList(filterContext, lineIndex);
                            ctrlField.Controls.Add(ectrl);
                        }
                        else
                        {
                            //TODO : Gestion de l'aperçu (en administration)
                            ectrl = eFilterLineRenderer.GetValuesList(filterContext, lineIndex); //TODO : Gestion de l'aperçu
                            ctrlField.Controls.Add(ectrl);
                            /*
                            strNewTag = string.Concat("<INPUT TYPE=text readonly style='", strStyle, ";height:21px' value='", strFieldName.Replace("'", "\\'"), "'>");
                            */

                        }
                        strNewTag = eTools.GetHtmlRender(ctrlField);
                        ectrl = null;
                        ctrlField = null;


                        lFieldCount = lFieldCount + 1;
                    }
                    else
                    {
                        switch (strDescId.ToUpper())
                        {
                            case "BUTTON_SEARCH":
                            case "LINK_SEARCH": //En v7 les deux boutons ont été au final rendu identique =)
                                strNewTag = SearchButton(strTag, strStyle, strPartId);
                                break;
                            default:
                                strNewTag = string.Empty;
                                break;
                        }
                    }
                    content = content.Replace(strTag, strNewTag);
                    nPosBegin = content.ToLower().IndexOf(eConst.HTM_TAG_BEGIN.ToLower(), nPosBegin + 1);
                }
                else
                    nPosBegin = 0;
            }
            return content;
        }

        /// <summary>
        /// Retourne le code pour le bouton de recherche
        /// </summary>
        /// <param name="strTag"></param>
        /// <param name="strStyle"></param>
        /// <param name="nPartId"></param>
        /// <returns></returns>
        private string SearchButton(string strTag, string strStyle, Int32 nPartId)
        {
            string strReturnValue;
            string strStartStyleTag, strEndStyleTag;
            Int32 lPosStart, lPosEnd;
            Boolean bShowTabList;
            string strFormula;

            //Ex : on récupère le ou les tags de mise en page
            //<LABEL><FONT size=2>[Bouton de recherche]</FONT></LABEL>
            //strStartStyleTag = "<FONT size=2>"
            //strEndStyleTag = "</FONT>"

            strStartStyleTag = string.Empty;
            strEndStyleTag = string.Empty;
            lPosStart = strTag.IndexOf(">") + 1;
            lPosEnd = strTag.IndexOf("[") + 1;
            if (lPosStart < lPosEnd)
            {

                strStartStyleTag = strTag.Substring(lPosStart, lPosEnd - lPosStart);

                lPosStart = strTag.LastIndexOf("]") + 1;
                lPosEnd = strTag.LastIndexOf("<");
                if (lPosStart < lPosEnd)
                {

                    strEndStyleTag = strTag.Substring(lPosStart, lPosEnd - lPosStart);

                }
                else
                {
                    strStartStyleTag = "";
                    strEndStyleTag = "";
                }
            }

            //Affiche la liste des onglets de destination
            Boolean.TryParse(eTools.GetTagPropValue(strTag, "ednshowtablist"), out bShowTabList);
            //Formule de calcul //TODO : deprecated ou pas ? Attention ce sytème est dangereux, injections possibles...
            strFormula = eTools.GetTagPropValue(strTag, "ednformula");

            strReturnValue = string.Concat(
                "<div id=\"FormValidation\" class=\"button-green\" style=\"text-align:right;float:none;", strStyle, "\" onclick=\"onOkEudoPartFilter(", nPartId, ");\">"
                , "<div class=\"button-green-left\"></div>"
                , "<div class=\"button-green-mid\">"
                , strStartStyleTag
                , eResApp.GetRes(_pref, 924)
                , strEndStyleTag
                , "</div>"
                , "<div class=\"button-green-right\"></div>"
                , "</div>"
                );
            /*GCH : Deprecated, ne fct pas en v7*/
            /*if (bShowTabList)
            {
                strReturnValue = string.Concat(strReturnValue, "<td style='width:21px;cursor:default' nowrap align=center><a href='#' onclick='showTabList();'><img class='rIco DropDown' border='0' src='", eConst.GHOST_IMG, "'></a></td>");
            }
            */

            return strReturnValue;
        }
    }

}