using System;
using System.Text;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eListXrmWidgetRenderer : eListMainRenderer
    {
        int _gridId = 0;

        /// <summary>
        /// Constructeur par défaut avec uniquement pref
        /// Base des classe dérivées
        /// </summary>
        /// <param name="pref"></param>
        private eListXrmWidgetRenderer(ePref ePref, Int32 nTab, int fileId, Int32 page, Int32 row, Int32 height, Int32 width, Boolean bFullList = true) : base(ePref)
        {           
            _gridId = fileId;
            _rType = RENDERERTYPE.ListRendererMain;
            _height = height;
            _width = width;
            _page = page;
            _rows = row;         
            _tab = nTab;
            _bFullList = bFullList;
        }

        /// <summary>
        /// sans test de droits et retourne un GetListXrmWidgetRenderer
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static eListXrmWidgetRenderer GetListXrmWidgetRenderer(ePref ePref, Int32 nTab, int fileId, Int32 page, Int32 row, Int32 height, Int32 width, Boolean bFullList = true)
        {
           return new eListXrmWidgetRenderer( ePref,  nTab,  fileId,  page,  row,  height,  width,  bFullList );
        }

        /// <summary>
        /// Paging pour l'instant désactivé ici
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                if (_list.ErrorMsg.Length == 0 && _list.ListRecords != null)
                {
                    _rows = _list.ListRecords.Count;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Génère l'objet _list du renderer
        /// </summary>
        /// <returns></returns>
        protected override void GenerateList()
        {
            if (_gridId > 0) // on retire les widget liée
                _list = eListFactory.GetWidgetList(Pref, _gridId, false);
            else
                base.Generate();
        }

        /// <summary>
        /// On affiche la recherche
        /// </summary>
        public override Boolean DrawSearchField
        {
            get { return (_list != null); }
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


        protected override bool End()
        {
            if (!base.End())
                return false;

            _tblMainList.Attributes.Add("onclick", "top.oAdminGridMenu.selectFromList(this, event);");


            return true;
        }
    }
}