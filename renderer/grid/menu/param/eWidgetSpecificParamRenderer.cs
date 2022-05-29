using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Web.UI;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu de la partie "Propriétés" des "Paramètres du widget"
    /// </summary>
    public class eWidgetSpecificParamRenderer : eAbstractMenuRenderer
    {
        /// <summary>
        /// The widget parameters
        /// </summary>
        protected eXrmWidgetParam _widgetParams;

        /// <summary>
        /// Construction de la partie "Actualisation" 
        /// </summary>
        bool _buildRefreshProperty = false;
        /// <summary>
        /// eFieldRecord de la colonne XrmWidget.ContentParam
        /// </summary>
        protected eFieldRecord _contentParamField = null;
        /// <summary>
        /// Emplacement (utilisé dans les traductions)
        /// </summary>
        protected eResLocation _resLoc;


        /// <summary>
        /// Constructeur parent de la partie "Propriétés" des "Paramètres du widget"
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="isVisible">Visibilité de la partie "Paramètres du widget"</param>
        /// <param name="file">eFile correspondant à la fiche</param>
        /// <param name="param">Objet contenant les paramètres spécifiques du widget</param>
        /// <param name="buildRefreshProp">Construction de la partie "Actualisation" du widget</param>
        /// <param name="context">The context.</param>
        public eWidgetSpecificParamRenderer(ePref pref, bool isVisible, eFile file, eXrmWidgetParam param, bool buildRefreshProp = false, eXrmWidgetContext context = null) : base(isVisible, file, context)
        {
            this.Pref = pref;
            _widgetParams = param;
            _buildRefreshProperty = buildRefreshProp;
        }
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            _contentParamField = _file.GetField((int)XrmWidgetField.ContentParam);

            _resLoc = new eResLocation(eModelConst.ResCodeNature.WidgetParam, eResLocation.GetPathFromWidgetContext(_context));

            return true;
        }

        /// <summary>
        /// Build
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            this._pgContainer.CssClass = "field-container";
            this._pgContainer.ID = "specificParamsContainer";

            try
            {
                // Partie "Actualisation"
                if (_buildRefreshProperty)
                {
                    if (_context != null && _context.GridLocation != eXrmWidgetContext.eGridLocation.Bkm)
                        _pgContainer.Controls.Add(BuildRefreshPropPart());
                }

                // Paramètres spécifiques
                BuildWidgetContentPart();
            }
            catch (Exception e)
            {
                throw e;
            }


            return true;
        }


        /// <summary>
        /// Builds the widget content part.
        /// </summary>
        protected virtual void BuildWidgetContentPart()
        {

        }

        /// <summary>
        /// Construction des propriétés d'actualisation du widget
        /// </summary>
        /// <returns></returns>
        private Control BuildRefreshPropPart()
        {
            return BuildYesNoOptionField(
                    eResApp.GetRes(Pref, 8002),
                    _file.GetField((int)XrmWidgetField.ManuelRefresh),
                    null,
                    eResApp.GetRes(Pref, 8008),
                    eResApp.GetRes(Pref, 8009),
                    eResApp.GetRes(Pref, 8010),
                    eResApp.GetRes(Pref, 8011),
                    true
                    );
        }

    }
}