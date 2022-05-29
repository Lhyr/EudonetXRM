using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eXrmWidgetFactory
    {
        /// <summary>
        /// Construit Un objet responsable du rendu du widget
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static IXrmWidgetUI GetWidgetUI(ePref pref, eRecord record, bool gridFirstLoad = false)
        {

            if (record == null)
                return new eErrorXrmWidgetUI();

            eFieldRecord fld = record.GetFieldByAlias((int)TableType.XRMWIDGET + "_" + (int)XrmWidgetField.Type);
            XrmWidgetType type = (XrmWidgetType)eLibTools.GetNum(fld.Value);

            switch (type)
            {
                case XrmWidgetType.Image:
                    return new eImageXrmWidgetUI(pref);

                case XrmWidgetType.WebPage:
                    return new eWebPageXrmWidgetUI()
                    {
                        Pref = pref,
                    };

                case XrmWidgetType.Weather:
                    return new eWeatherXrmWidgetUI();

                case XrmWidgetType.Chart:
                    //if (gridFirstLoad && !pref.AdminMode)
                    //    return new eWaiterXrmWidgetUI();
                    // else
                    return new eChartXrmWidgetUI(pref);

                case XrmWidgetType.List:
                    if (gridFirstLoad && !pref.AdminMode)
                        return new eWaiterXrmWidgetUI();
                    else
                        return new eListXrmWidgetUI();

                case XrmWidgetType.Specif:
                    return new eEudoSpecifXrmWidgetUI(pref);

                case XrmWidgetType.Editor:
                    return new eHtmlEditorXrmWidgetUI();

                case XrmWidgetType.Indicator:
                    if (gridFirstLoad && !pref.AdminMode)
                        return new eWaiterXrmWidgetUI();
                    else
                        return new eIndicatorXrmWidgetUI(pref);

                case XrmWidgetType.ExpressMessage:
                    return new eExpressMessageXrmWidgetUI(pref);

                case XrmWidgetType.Tuile:
                    return new eTileXrmWidgetUI(pref);

                case XrmWidgetType.Kanban:
                    return new eKanbanXrmWidgetUI();

                case XrmWidgetType.RSS:
                    return new eRSSXrmWidgetUI();
                case XrmWidgetType.Carto_Selection:
                    if (eExtension.IsReady(pref, ExtensionCode.CARTOGRAPHY))
                        return new eCartographyXrmWidgetUI();
                    return new eErrorXrmWidgetUI();

                default:
                    return new eErrorXrmWidgetUI();

            }
        }

        /// <summary>
        /// Retourn le renderer du contenu
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="tab">The tab.</param>
        /// <param name="file">The file.</param>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static eAbstractMenuRenderer GetMenuContentRenderer(ePref pref, int tab, eFile file, bool isVisible, eXrmWidgetContext context)
        {
            if (tab == (int)TableType.XRMHOMEPAGE)
                return new eMenuHomePageContentRenderer(pref, isVisible, file, context);

            return new eMenuGridContentRenderer(pref, isVisible, file, context);

        }

        /// <summary>
        /// Retourn le renderer des params
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="tab">The tab.</param>
        /// <param name="file">The file.</param>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        /// <returns></returns>
        public static eAbstractMenuRenderer GetMenuParamRenderer(ePref pref, int tab, eFile file, bool isVisible, eXrmWidgetContext context)
        {
            if (tab == (int)TableType.XRMHOMEPAGE)
                return new eMenuHomePageParamRenderer(pref, isVisible, file, context);

            return new eMenuGridParamRenderer(pref, isVisible, file, context);

        }

        /// <summary>
        /// Construit Un objet responsable du rendu du paramètrage spécifique du widget widegt
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static eAbstractMenuWidgetParamRenderer GetMenuWidgetParamRenderer(ePref pref, int fileId, bool isVisible, eXrmWidgetContext context)
        {
            eXrmWidgetParam widgetParam = new eXrmWidgetParam(pref, fileId);

            if (fileId == 0)
                return new eGenericParamRenderer(pref, isVisible, null, widgetParam);

            eFile file = eFileMain.CreateMainFile(pref, (int)TableType.XRMWIDGET, fileId, -2);

            eFieldRecord fld = file.GetField((int)XrmWidgetField.Type);
            int nType = eLibTools.GetNum(fld.Value);

            return new eGenericParamRenderer(pref, isVisible, file, widgetParam, nType, context);

            //switch (nType)
            //{
            //    case (int)XrmWidgetType.Image:
            //        return new eImageParamRenderer(pref, isVisible, file, widgetParam);
            //    case (int)XrmWidgetType.WebPage:
            //        return new eWebPageParamRenderer(pref, isVisible, file, widgetParam);
            //    case (int)XrmWidgetType.Weather:
            //        return new eWeatherParamRenderer(pref, isVisible, file, widgetParam);
            //    case (int)XrmWidgetType.Chart:
            //        return new eChartParamRenderer(pref, isVisible, file, widgetParam);
            //    case (int)XrmWidgetType.List:
            //        return new eListParamRenderer(pref, isVisible, file, widgetParam);
            //    case (int)XrmWidgetType.Specif:
            //        return new eSpecifParamRenderer(pref, isVisible, file, widgetParam);
            //    case (int)XrmWidgetType.Indicator:
            //        return new eIndicatorParamRenderer(pref, isVisible, file, widgetParam);
            //    case (int)XrmWidgetType.Tuile:
            //        return new eTileParamRenderer(pref, isVisible, file, widgetParam);
            //    case (int)XrmWidgetType.Kanban:
            //        return new eKanbanParamRenderer(pref, isVisible, file, widgetParam);
            //    default:
            //        return new eErrorParamRenderer(pref, isVisible, file, widgetParam);

            //}
        }
    }


    public class eXrmHomeTools
    {
#if DEBUG
        // Débug des pages d'accueil
        public static bool DEBUG = true;
#else
          // Débug des pages d'accueil
        public static bool DEBUG = false;
#endif
    }

}
