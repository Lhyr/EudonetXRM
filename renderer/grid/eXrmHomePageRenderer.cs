using EudoQuery;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet permettant d'afficher la page d'accueil
    /// </summary>
    public class eXrmHomePageRenderer : eRenderer
    {
        eXrmHomePageSubMenuRenderer subMenuRenderer;
        int _fileId;
        int _firstGrid;

        public eXrmHomePageRenderer(ePref pref, int id, int nWidth, int nHeight)
        {
            Pref = pref;
            _width = nWidth;
            _height = nHeight;
            _tab = (int)TableType.XRMHOMEPAGE;
            _fileId = id;
        }

        /// <summary>
        /// Initialise 
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {

            subMenuRenderer = new eXrmHomePageSubMenuRenderer(Pref, _fileId, (int)EdnType.FILE_GRID);
            if (subMenuRenderer.Init())
            {
                _firstGrid = subMenuRenderer.FirstGridItemId();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Lance la construction de la grille
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {

            _pgContainer.Controls.Add(subMenuRenderer.BuildSubMenu());
            _pgContainer.Controls.Add(BuildGridContent());

            return true;
        }

        private Control BuildGridContent()
        {
            Panel listheader = new Panel();
            listheader.ID = "listheader";

            listheader.Controls.Add(eRendererFactory.CreateXrmGridRenderer(Pref, _firstGrid, _width, _height).PgContainer);

            return listheader;
        }

        /// <summary>
        /// On ajoute les class css générées dynamiquement
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            return true;
        }
    }
}