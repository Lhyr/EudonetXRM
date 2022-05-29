using System;
using System.Text;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eListXrmHomePageRenderer : eListMainRenderer
    {


        /// <summary>
        /// Constructeur par défaut avec uniquement pref
        /// Base des classe dérivées
        /// </summary>
        /// <param name="pref"></param>
        private eListXrmHomePageRenderer(ePref pref) : base(pref)
        {
            _rType = RENDERERTYPE.ListRendererMain;
        }

        /// <summary>
        /// sans test de droits et retourne un eListHomePageRenderer
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static eListXrmHomePageRenderer GetListXrmHomePageRenderer(ePref pref)
        {
            return new eListXrmHomePageRenderer(pref);
        }


        /// <summary>
        /// On affiche la recherche
        /// </summary>
        public override Boolean DrawSearchField
        {
            get { return (_list != null ) ; }
        }

        /// <summary>
        /// ajoute la cellule d'en tete contenant la case à cocher "selectionner tout"
        /// </summary>
        /// <param name="headerRow"></param>
        /// <param name="iWidth">largeur utilisée par la cellule</param>
        protected override void AddSelectCheckBoxHead(TableRow headerRow)
        {

        }      

        /// <summary>
        /// Pas de checkbox pour les pages d'accueil
        /// </summary>
        /// <param name="row"></param>
        /// <param name="trDataRow"></param>
        /// <param name="sAltLineCss"></param>
        protected override void AddSelectCheckBox(eRecord row, TableRow trDataRow, String sAltLineCss)
        {
            return;
        }
    }
}