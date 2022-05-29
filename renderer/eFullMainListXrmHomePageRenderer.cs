using EudoQuery;
using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Retourne le bloc complet d'une main liste (cad, la barre d'info, de pagination, d'action et la mainlist) des pages d'accueil XRM
    ///  > peut retourner les blocs de façons indépendante
    /// 
    /// </summary>
    public class eFullMainListXrmHomePageRenderer : eFullMainListRenderer
    {

        /// <summary>
        /// Retourne un renderer eListRendererMain
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="height">Hauteur du bloc de rendu</param>
        /// <param name="width">Largeur du bloc de rendu</param>
        /// <param name="nPage">Page</param>
        /// <param name="nRow">Nombdre de ligne par page</param>
        /// <param name="tab">Table lite chargé</param>
        /// <returns></returns>
        internal static eFullMainListRenderer GetFullXrmHomePageRenderer(ePref pref, Int32 height, Int32 width, Int32 nPage, Int32 nRow, TableLite tab)
        {
            eFullMainListXrmHomePageRenderer rdr = new eFullMainListXrmHomePageRenderer(pref, tab.DescId, height, width, nPage, nRow); ;
            rdr._ednTab = tab;
            return rdr;
        }


        /// <summary>
        /// Constructeur "passe plat"
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="nPage"></param>
        /// <param name="nRow"></param>

        private eFullMainListXrmHomePageRenderer(ePref pref, Int32 nTab, Int32 height, Int32 width, Int32 nPage, Int32 nRow)
            : base(pref, nTab, height, width, nPage, nRow)
        {
        }


        /// <summary>
        /// Initialisation du renderer
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {

            _bHideActionSelection = true;

            return base.Init();



        }

        /// <summary>
        /// Construction du bloc MainList qui contient le menu du haut de liste
        ///  (nombre de fiche, filtres avancés...)
        /// </summary>
        /// <param name="pMainListContent"></param>
        protected override void BuildTopMenu(Panel pMainListContent)
        {

            Panel pInfos = new Panel();

            //Div Infos
            pMainListContent.Controls.Add(pInfos);
            pInfos.ID = "infos";
            BuildDivInfos(pInfos);

        }

        /// <summary>
        /// Fait un rendu de l'onglet web/ des sous-onglet
        /// </summary>
        /// <param name="cpnl"></param>
        protected override void DoWebTab(HtmlGenericControl cpnl)
        {

        }
    }
}