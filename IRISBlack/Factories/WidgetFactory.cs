using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.IRISBlack
{
    public class WidgetFactory
    {
        /// <summary>Id de la grille</summary>
        internal int _gridId { get; set; } = 0;
        internal ePref Pref { get; set; }
        internal int _width { get; set; }
        internal int _height { get; set; }
        internal int _tab { get; set; }

        private eFile _file;
        private eList _listWidget;
        private eXrmWidgetPrefCollection _WidgetPrefCollection;
        private StringBuilder _dynamicScript = new StringBuilder();
        private StringBuilder _dynamicStyle = new StringBuilder();
        /// <summary>
        /// Dans le cas où on est sur un signet Grille 
        /// </summary>
        private eXrmWidgetContext _context;
        /// <summary>Div global rendu par le renderer</summary>
        protected Panel _pgContainer = new Panel();        /// <summary>Message d'erreur</summary>
        protected string _sErrorMsg = string.Empty;

        /// <summary>numéro de l'erreur</summary>
        protected QueryErrorType _nErrorNumber = QueryErrorType.ERROR_NONE;
        /// <summary>Objet exception</summary>
        protected Exception _eException;


        /// <summary>
        /// Initialisation du builder -> instanciation des objets metiers
        /// </summary>
        /// <returns></returns>
        protected bool Init()
        {
            _file = eFileMain.CreateMainFile(Pref, (int)TableType.XRMGRID, _gridId, -2);
            _listWidget = eListFactory.GetWidgetList(Pref, _gridId);
            _WidgetPrefCollection = new eXrmWidgetPrefCollection(Pref, _gridId);


            return true;
        }

        /// <summary>
        /// Ici est créé l'ensemble des Widgets.
        /// </summary>
        /// <returns></returns>
        protected bool Build()
        {
            _pgContainer.Controls.Add(BuildContainer());

            return true;
        }

        /// <summary>
        /// Construit le grille et le contenu
        /// </summary>
        /// <returns></returns>
        private Control BuildContainer()
        {
            Panel panel = new Panel();
            panel.CssClass = "gridContainer";

            HtmlGenericControl container = new HtmlGenericControl("div");
            container.ID = "widget-grid-container";
            container.Attributes.Add("class", "widget-grid-container");// TODO
            container.Style.Add("height", _height + "px");// on retire la taille de la navbar + titre + bas de page
            container.Style.Add("width", _width + "px");// on retire la taille de la navbar + titre + bas de page



            container.Attributes.Add("gid", _gridId.ToString());// TODO
            container.Attributes.Add("config", Pref.AdminMode && _tab > 0 ? "1" : "0");

            // Affichage de la grille et des widgets
            BuildWidgets(container);

            panel.Controls.Add(container);

            return panel;
        }

        /// <summary>
        /// Rendu des widgets
        /// </summary>
        /// <returns></returns>
        private void BuildWidgets(HtmlControl container)
        {
            // Pas de widgets pour la grille actuelle
            if (_listWidget.ListRecords == null)
                return;

            IXrmWidgetUI IWidget;
            eXrmWidgetPref widgetPref;
            eXrmWidgetParam widgetParam;


            foreach (eRecord record in _listWidget.ListRecords)
            {
                // pref des widgets
                widgetPref = _WidgetPrefCollection[record];

                widgetParam = new eXrmWidgetParam(Pref, record.MainFileid);

                // le widget n'est visibe sauf si explicite ou admin
                if (!Pref.AdminMode && !widgetPref.Visible)
                    continue;

                // rendu du contenu de la div 
                IWidget = new eXrmWidgetContent(eXrmWidgetFactory.GetWidgetUI(Pref, record, true), Pref);

                // ajout de la div pour le déplacement
                // Ajout de la div pour redimensionner
                // ajout de container
                IWidget = new eXrmWidgetWrapper(new eXrmWidgetWithResize(new eXrmWidgetWithToolbar(Pref, IWidget, (Pref.AdminMode && _tab > 0 && widgetParam.GetParamValue("noAdmin") != "1")), Pref), Pref, true);

                // Initilisation
                IWidget.Init(record, widgetPref, widgetParam, _context);

                // Construction
                IWidget.Build(container);

                // ajout des script js
                IWidget.AppendScript(_dynamicScript);

                //ajout des style css
                IWidget.AppendStyle(_dynamicStyle);
            }
        }

        /// <summary>
        /// On ajoute les class css générées dynamiquement
        /// </summary>
        /// <returns></returns>
        protected bool End()
        {
            HtmlGenericControl style = new HtmlGenericControl("style");
            style.ID = "grid-cell-" + _gridId;
            style.Attributes.Add("type", "text/css");
            style.InnerHtml = _dynamicStyle.ToString();
            _pgContainer.Controls.Add(style);


            HtmlGenericControl script = new HtmlGenericControl("script");
            script.Attributes.Add("type", "text/javascript");
            script.InnerHtml = _dynamicScript.ToString();
            _pgContainer.Controls.Add(script);

            return true;
        }

        /// <summary>
        /// appel la séquence de génération basique du renderer
        /// Dans la majorité des renderer, la sequence est : init->build->end 
        /// </summary>
        /// <returns></returns>
        public bool Generate()
        {

            try
            {

                if (!Init())
                    return false;


                if (!Build())
                    return false;


                if (!End())
                    return false;


                return true;
            }
            catch (eFileLayout.eFileLayoutException e)
            {
                throw;
            }
            catch (EudoException e)
            {
                throw;
            }
            catch (Exception e)
            {
                //Interception des erreur
                _eException = e;
                _sErrorMsg = e.ToString();
                _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;

                return false;
            }
            finally
            {

            }
        }
    }
}