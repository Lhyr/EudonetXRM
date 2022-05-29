using System;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Widget cartographie de type sélection par onglet
    /// </summary>
    public class eCartographyXrmWidgetUI : eAbstractXrmWidgetUI
    {
        string themeColorStyles = string.Empty;

        /// <summary>
        /// Fait un rendu du widget dans le container
        /// </summary>
        /// <param name="widgetContainer">Container du widget</param>
        public override void Build(HtmlControl widgetContainer)
        {
            int wid = this._widgetParam.WidgetId;

            string color = string.IsNullOrWhiteSpace(_widgetParam._pref.ThemeXRM.Color2) ? "white" : _widgetParam._pref.ThemeXRM.Color2;
            string backColor = string.IsNullOrWhiteSpace(_widgetParam._pref.ThemeXRM.Color) ? "white" : _widgetParam._pref.ThemeXRM.Color;
            themeColorStyles = $"color:{color};background-color:{backColor};";

            HtmlBuilder
                .Create("div")
                .Id("cartoSelection")
                .Attr("class", "carto-selection")
                .Attr("fid", $"{wid}")
                .AddBloc(e => BuildLeft(e)) // menu gauche filtres
                .AddBloc(e => BuildMiddle(e)) // conteneur carte Bing Map
                .AddBloc(e => BuildRight(e, wid)) // menu droite mini-fiche sélection
                .AppendTo(widgetContainer);

            base.Build(widgetContainer);
        }

        /// <summary>
        /// Créer la partie filtre gauche
        /// </summary>
        /// <param name="cartoSelection"></param>
        private void BuildLeft(HtmlControl cartoSelection)
        {
            bool pinned = true;

            // h4 menu de la sélection quand c'est masqué           
            HtmlBuilder
                .Create("h4")
                .Id("menuCartoLeft")
                .Attr("class", "menu-carto-Left static-background-theme")
                .Add("i", builder => builder.Attr("class", "icon-bars"))
                .Add("div", builder => builder.Text("Critères"))
                .AppendTo(cartoSelection);

            // Critères
            HtmlBuilder
                .Create("div").AppendTo(cartoSelection)
                .Id("cartoLeft")
                .Attr("pinned", pinned ? "1" : "0")
                .Attr("class", "carto-left")
                .Add("h4", cartoTitle => // Titre de la séction gauche
                    cartoTitle
                   .Id("cartoTitle")
                   .Attr("class", "carto-title static-background-theme")                  
                   .Text(eResApp.GetRes(_widgetParam._pref, 2044))//Critères
                   .Add("i", thumbtack => thumbtack.Attr("class", "icon-thumb-tack").Attr("pinned", pinned ? "1" : "0")))
               .Add("div", (contentParamCarto) => // Emplacement des filtres                    
                   contentParamCarto
                  .Attr("class", "content-param-carto")
                  .Add("div", cartoFiltersContainer =>
                        cartoFiltersContainer
                       .Id("cartoFilters")
                       .Attr("class", "carto-filters")
                       .Html($"<p>{eResApp.GetRes(_widgetParam._pref, 1233)}</p>"))) //contenu des filtres sera chargé depuis le js
               .Add("div", buttonBuilder => // Partie Appliquer le filtre
                   buttonBuilder
                  .Id("applyFilters")
                  .Attr("class", "apply-filters")
                  .Add("button", (button) =>
                       button
                      .Id("applyButton")
                      .Attr("class", "apply-button background-theme")
                      .Text(eResApp.GetRes(_widgetParam._pref, 866))));
        }

        /// <summary>
        /// Créer la partie conteneur de la carte
        /// </summary>
        /// <param name="cartoSelection"></param>
        private void BuildMiddle(HtmlControl cartoSelection)
        {
            // Conteneur de la carte          
            HtmlBuilder
               .Create("div")
               .Id("mapContainer")
               .Attr("class", "map-container")
               .AppendTo(cartoSelection);
        }

        /// <summary>
        /// Construit la partie droite : sélecton des mini-fiches
        /// </summary>
        /// <param name="cartoSelection"></param>
        /// <param name="wid">id du widget</param>
        private void BuildRight(HtmlControl cartoSelection, int wid)
        {
            bool pinned = true;
            // h4 menu de la sélection quand c'est masqué           
            HtmlBuilder
                .Create("h4")
                .Id("menuCartoRight")
                .Attr("class", "menu-carto-right static-background-theme")               
                .Add("i", builder => builder.Attr("class", "icon-bars"))
                .Add("i", builder => builder.Attr("class", "icon-heart"))
                .Add("div", builder => builder.Text(eResApp.GetRes(_widgetParam._pref, 2045))) // Résultat(s)
                .AppendTo(cartoSelection);

            // Conteneur des mini-fiches
            HtmlBuilder
                .Create("div").AppendTo(cartoSelection)
                .Id("cartoRight")
                .Attr("pinned", pinned ? "1" : "0")
                .Attr("class", "carto-right content-block all-list")
                .Add("h4", cartoTitle =>
                    cartoTitle
                   .Id("cartoTitle")
                   .Attr("class", "carto-title static-background-theme")
                   .Add("div", displayResult =>
                         displayResult
                         .Id("displayResultNumber")
                         .Html($"<i class='icon-thumb-tack' pinned='{(pinned ? "1" : "0")}'></i><div>{eResApp.GetRes(_widgetParam._pref, 2045)}</div>"))// Résultat(s)
                   .Add("div", displaySelection =>
                         displaySelection
                         .Id("displaySelectionNumber")
                         .Html($"<i class='icon-thumb-tack' pinned='{(pinned ? "1" : "0")}'></i><div>{eResApp.GetRes(_widgetParam._pref, 6214)} (<span sid='selectedCardCount_{wid}'>0</span>)</div>")))
               .Add("div", (contentParamCarto) =>
                    contentParamCarto
                   .Attr("class", "content-param-carto")
                   .AddBloc(e => BuildRightContainer(e, wid)))               
               .Add("div", buttonBuilder =>// Enregistrer la sélection
                    buttonBuilder
                   .Id("applySelection")
                   .Attr("class", "apply-selection")
                   .Add("button", (button) =>
                        button
                       .Id("selectionButton")
                       .Attr("class", "selection-button")
                       .Attr("style", "background: #79BE0B;")
                       .Text(eResApp.GetRes(_widgetParam._pref, 2046))));//Enregistrer ma sélection
        }

        /// <summary>
        /// Structure globale de la partie qui heberge les mini-fiches
        /// </summary>
        /// <param name="paramCarto"></param>
        /// <param name="wid"></param>
        private void BuildRightContainer(HtmlControl paramCarto, int wid)
        {
            HtmlBuilder
                .Create("div").AppendTo(paramCarto)
                .Id("cartoResult")
                .Attr("style", "height: 100%;")
                .Add("div", contentSet =>
                     contentSet
                    .Id("contentSet")
                    .Add("div", backAll =>
                         backAll
                        .Id("backAll")
                        .Add("button", button => button.Html($"<i class='icon-arrow-left'></i>{eResApp.GetRes(_widgetParam._pref, 2047)}")))//Retour aux résultats
                    .Add("div", inputAll =>
                         inputAll
                        .Id("inputAll")
                        .Add("input", input =>
                             input
                            .Id($"allSelectedCard_{wid}")
                            .Attr("type", "checkbox")
                            .Attr("name", "allSelectedCard"))
                        .Add("label", lbo =>
                             lbo
                             .Attr("for", $"allSelectedCard_{wid}")
                             .Text(eResApp.GetRes(_widgetParam._pref, 431))))                   
                    .Add("div", btnSelection =>
                        btnSelection
                        .Id("btnSelection")
                        .Add("button", button =>
                              button
                             .Html($"<i class='icon-heart'></i>{eResApp.GetRes(_widgetParam._pref, 6214)} (<span sid='selectedCardCount_{wid}'>0</span>)"))))
               .Add("div", rightContent => rightContent.Id("contentResultat"));
        }


        /// <summary>
        /// Factory permet de générer du HTML en utilisant la technique FluentInterface
        /// https://martinfowler.com/bliki/FluentInterface.html
        /// </summary>
        public class HtmlBuilder
        {
            /// <summary>
            /// Element en cours de creation
            /// </summary>
            private HtmlGenericControl elm;

            /// <summary>
            /// Constructeur de l'element, en définissant son Tag
            /// </summary>
            /// <param name="tag"></param>
            private HtmlBuilder(string tag)
            {
                elm = new HtmlGenericControl(tag);
            }

            /// <summary>
            /// Methode factory de création
            /// </summary>
            /// <param name="tag"></param>
            /// <returns></returns>
            public static HtmlBuilder Create(string tag)
            {
                return new HtmlBuilder(tag);
            }

            /// <summary>
            /// Definir l'id
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            public HtmlBuilder Id(string id)
            {
                elm.ID = id;
                return this;
            }

            /// <summary>
            /// Ajout un attribut
            /// </summary>
            /// <param name="id"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public HtmlBuilder Attr(string id, string value)
            {
                elm.Attributes.Add(id, value);
                return this;
            }

            /// <summary>
            /// Définit le contenu HTML
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public HtmlBuilder Html(string value)
            {
                elm.InnerHtml = value;
                return this;
            }

            /// <summary>
            /// Définit le text de l'element
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public HtmlBuilder Text(string value)
            {
                elm.InnerText = value;
                return this;
            }

            /// <summary>
            /// Ajout un element enfant a l'element en cours de création
            /// </summary>
            /// <param name="tag"></param>
            /// <param name="func"></param>
            /// <returns></returns>
            public HtmlBuilder Add(string tag, Action<HtmlBuilder> func)
            {
                HtmlBuilder child = new HtmlBuilder(tag);
                child.AppendTo(this.elm);
                func(child);
                return this;
            }

            /// <summary>
            /// Ajout un block html a l'element
            /// </summary>
            /// <param name="func"></param>
            /// <returns></returns>
            public HtmlBuilder AddBloc(Action<HtmlControl> func)
            {
                func(this.elm);
                return this;
            }

            /// <summary>
            /// Attache l'element en cours de creation a son parent
            /// </summary>
            /// <param name="parent"></param>
            /// <returns></returns>
            public HtmlBuilder AppendTo(HtmlControl parent)
            {
                parent.Controls.Add(this.elm);
                return this;
            }
        }
    }
}