using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm
{
    /// <className>eWebUiPageRegisters</className>
    /// <summary>Classe de gestion des register JS et CSS des pages .NET XRM</summary>
    /// <purpose></purpose>
    /// <authors>HLA</authors>
    /// <date>2014-08-22</date>
    public class eWebUiPageRegisters
    {
        // Demande 82234 (Affichage - Lenteur ouverture catalogue arborescent)
        public bool DisplayTheme2019 = true;

        private HttpServerUtility _server = null;
        private Control _control = null;

        private ePrefLite.Theme _theme = null;

        //private string _sJSVersion = DateTime.Now.Ticks.ToString();
        private string _sJSVersion = "01";
        private string _sCSSVersion = "01";

        private Boolean _bRegisterFromRoot = false;


        //Enregistre les js avec le chemin depuis la racine du site
        public Boolean RegisterFromRoot
        {
            get { return _bRegisterFromRoot; }
            set { _bRegisterFromRoot = value; }
        }

        public string RelativePath = string.Empty;

        /// <summary>
        /// Liste des nom (sans le js ni le chemin) des script js a ajouter dans le head
        /// </summary>
        private List<string> _lstScriptToRegister = null;
        /// <summary>
        /// Liste des nom (sans le css ni le chemin) des css a ajouter dans le head
        /// </summary>
        private ISet<CssFile> _lstCssToRegister = null;
        /// <summary>
        /// Instruction Javascript a ajouter le head
        /// </summary>
        private StringBuilder _rawScript = null;

        /// <summary>
        /// styles css
        /// </summary>
        private string _rawCss = string.Empty;


        /// <className>CssFile</className>
        /// <summary>Classe qui représente un fichier CSS avec ces différents attibutes spécifiques</summary>
        /// <purpose></purpose>
        /// <authors>HLA</authors>
        /// <date>2014-08-22</date>
        public class CssFile
        {
            /// <summary>
            /// Chemin à partir du dossier "CSS" si defaultPath a true, le chemin complet  nom du fichier sinon et sans extention. Exemple : mobile/mobile-styles-portrait
            /// </summary>
            public string PathName { get; set; }

            /// <summary>
            /// Savoir si on utilise le chemin par defaut
            /// </summary>
            public Boolean UseDefaultPath = true;

            /// <summary>
            /// Attributes complémentaires
            /// </summary>
            public IDictionary<string, string> CpltAttributes { get; set; }

            /// <summary>
            /// Construction
            /// </summary>
            /// <param name="pathName"></param>
            public CssFile(string pathName, Boolean defaultPath = true)
            {
                this.PathName = pathName;
                this.UseDefaultPath = defaultPath;
                this.CpltAttributes = new Dictionary<string, string>();
            }

            /// <summary>
            /// Redefini pour la clé de dédoublonnage dans le HashSet
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override Boolean Equals(object obj)
            {
                string objStr = obj as string;
                if (objStr == null)
                    return false;
                return PathName.Equals(objStr);
            }

            /// <summary>
            /// Redefini pour la clé de dédoublonnage dans le HashSet
            /// </summary>
            /// <returns></returns>
            public override Int32 GetHashCode()
            {
                return PathName.GetHashCode();
            }
        }

        internal eWebUiPageRegisters(HttpServerUtility server, Control control = null)
        {
            this._server = server;
            this._control = control;
            this._lstScriptToRegister = new List<string>();
            this._lstCssToRegister = new HashSet<CssFile>();
            this._rawScript = new StringBuilder();

            _sJSVersion = string.Concat(eConst.VERSION, ".", eConst.REVISION);
            _sCSSVersion = string.Concat(eConst.VERSION, ".", eConst.REVISION);
        }

        internal void SetTheme(ePrefLite.Theme theme)
        {
            this._theme = theme;
        }

        internal void AddScript(string script)
        {
            _lstScriptToRegister.Add(script);
        }

        internal void AddRangeScript(IEnumerable<string> script)
        {
            _lstScriptToRegister.AddRange(script);
        }

        /// <summary>
        /// Ajout l'include d'un fichier CSS
        /// </summary>
        /// <param name="css">fichier CSS</param>
        internal void AddCss(CssFile css)
        {
            _lstCssToRegister.Add(css);
        }

        /// <summary>
        /// Ajout l'include d'un fichier CSS
        /// </summary>
        /// <param name="cssPath">Chemin à partir du dossier "CSS" et nom du fichier sans extention. Exemple : mobile/mobile-styles-portrait</param>
        /// <param name="media">Ajout d'un media particulier</param>
        internal void AddCss(string cssPath, string media = null)
        {
            if (cssPath.Length > 0)
            {
                eWebUiPageRegisters.CssFile cssFile = new eWebUiPageRegisters.CssFile(cssPath);
                if (media != null)
                    cssFile.CpltAttributes.Add("media", media);

                AddCss(cssFile);
            }
        }

        /// <summary>
        /// Ajout d'un collection d'include de CSS
        /// </summary>
        /// <param name="css">Liste des CSS à ajouter</param>
        internal void AddRangeCss(IEnumerable<string> css)
        {
            foreach (string file in css)
                AddCss(new CssFile(file));
        }

        /// <summary>
        /// Ajoute des css stoquées dans les répertoire autre que celui par defaut
        /// </summary>
        /// <param name="listOfCssFiles"> les des chemins avec les noms de ficher sans extension</param>
        public void AddRangeCssWithPath(List<string> listOfCssFiles)
        {
            foreach (string cssName in listOfCssFiles)
                AddCss(new eWebUiPageRegisters.CssFile(cssName, false));
        }


        internal StringBuilder RawScrip { get { return _rawScript; } }


        /// <summary>
        /// Enregistre les scripts d'administrations
        /// </summary>
        internal void RegisterAdminIncludeScript(string file = "eAdmin")
        {
            if (_control == null)
            {
                return;
            }


            string sJStoInclude = string.Concat(@"eda/scripts/", file, ".js");

            if (File.Exists(_server.MapPath((RegisterFromRoot ? @"~/" : RelativePath) + sJStoInclude)))
            {

                if (RegisterFromRoot)
                    sJStoInclude = string.Concat(HttpContext.Current.Request.ApplicationPath, "//", sJStoInclude);

                HtmlGenericControl myInclude = new HtmlGenericControl("script");
                myInclude.Attributes.Add("type", "text/javascript");

                //Defer pose finalement des problèmes de synchronisation de lancement de script
                //     myInclude.Attributes.Add("defer", "");

                myInclude.Attributes.Add("src", string.Concat(sJStoInclude, "?ver=", _sJSVersion));


                _control.Controls.Add(myInclude);

            }
        }

        /// <summary>
        /// permert d'enregistrer un script include dans le header de la page
        /// Le header doit être en runat server
        /// Ajoute un flag de versionning pour forcer le cache navigateur
        /// </summary>
        internal void RegisterIncludeScript()
        {
            if (_control == null)
            {
                return;
            }

            HtmlGenericControl myIncludeVer = new HtmlGenericControl("script");
            myIncludeVer.Attributes.Add("type", "text/javascript");
            myIncludeVer.Attributes.Add("language", "javascript");


            myIncludeVer.InnerHtml = string.Concat(Environment.NewLine, "var _jsVer = '", _sJSVersion, "';", Environment.NewLine, "var _CssVer = '", _sCSSVersion, "';");


            _control.Controls.Add(myIncludeVer);



            foreach (string sInclude in _lstScriptToRegister.Distinct())
            {

                string sJStoInclude;
                if (!sInclude.StartsWith(@"~/"))
                    sJStoInclude = string.Concat(@"scripts/", sInclude, ".js");
                else
                    sJStoInclude = string.Concat(HttpContext.Current.Request.ApplicationPath, sInclude.Substring(1), ".js");



                // #68 13x - Editeur de templates HTML avancé (grapesjs) - Inclusion de scripts distants
                if (sInclude.StartsWith("http://") || sInclude.StartsWith("https://") || sInclude.StartsWith("//"))
                {
                    HtmlGenericControl myInclude = new HtmlGenericControl("script");
                    myInclude.Attributes.Add("type", "text/javascript");

                    myInclude.Attributes.Add("src", sInclude); // pas de rajout de .js, car certaines URL n'en comportent pas

                    _control.Controls.Add(myInclude);
                }
                // Scripts locaux
                else
                {
                    if (File.Exists(_server.MapPath((RegisterFromRoot ? @"~/" : RelativePath) + sJStoInclude)))
                    {

                        if (RegisterFromRoot)
                            sJStoInclude = string.Concat(HttpContext.Current.Request.ApplicationPath, "//", sJStoInclude);

                        HtmlGenericControl myInclude = new HtmlGenericControl("script");
                        myInclude.Attributes.Add("type", "text/javascript");

                        //Defer pose finalement des problèmes de synchronisation de lancement de script
                        //     myInclude.Attributes.Add("defer", "");

                        myInclude.Attributes.Add("src", string.Concat(sJStoInclude, "?ver=", _sJSVersion));

                        _control.Controls.Add(myInclude);

                    }
                }
            }
        }

        /// <summary>
        /// permert d'enregistrer un script dans le header de la page
        /// Le header doit être en runat server
        /// Ajoute un flag de versionning pour forcer le cache navigateur
        /// </summary>
        internal void RegisterScript()
        {




            HtmlGenericControl myInclude = new HtmlGenericControl("script");
            myInclude.Attributes.Add("type", "text/javascript");
            myInclude.Attributes.Add("language", "javascript");
            myInclude.InnerHtml = " top.eTools.UpdateDocCss(document); ";

            //  _control.Controls.Add(myInclude);

            if (_rawScript.Length == 0)
                return;

            myInclude = new HtmlGenericControl("script");
            myInclude.Attributes.Add("type", "text/javascript");
            myInclude.Attributes.Add("language", "javascript");
            myInclude.InnerHtml = _rawScript.ToString();
            _control.Controls.Add(myInclude);
        }

        /// <summary>
        /// permert d'enregistrer un script dans le header de la page
        /// Le header doit être en runat server
        /// Ajoute un flag de versionning pour forcer le cache navigateur
        /// </summary>
        internal void RegisterCSS()
        {
            string fileName = string.Empty;
            string fileWebName = string.Empty;


            // EudoFont d'abord
            fileWebName = eTools.WebPathCombine("themes", "default", "css", "eudoFont.css");
            if (RegisterFromRoot)
                fileWebName = eTools.WebPathCombine(HttpContext.Current.Request.ApplicationPath, fileWebName);
            HtmlGenericControl myBaseInclude = new HtmlGenericControl("link");
            myBaseInclude.Attributes.Add("rel", "stylesheet");
            myBaseInclude.Attributes.Add("type", "text/css");
            myBaseInclude.Attributes.Add("href", string.Concat(fileWebName, "?ver=", _sCSSVersion));
            _control.Controls.Add(myBaseInclude);

            foreach (CssFile cssInclude in _lstCssToRegister)
            {
                // #68 13x - Editeur de templates HTML avancé (grapesjs) - Inclusion de CSS distantes
                if (cssInclude.PathName.StartsWith("http://") || cssInclude.PathName.StartsWith("https://") || cssInclude.PathName.StartsWith("//"))
                {
                    HtmlGenericControl myInclude = new HtmlGenericControl("link");
                    myInclude.Attributes.Add("rel", "stylesheet");
                    myInclude.Attributes.Add("type", "text/css");

                    myInclude.Attributes.Add("href", cssInclude.PathName); // pas de rajout de .css, car certaines URL n'en comportent pas

                    _control.Controls.Add(myInclude);
                }
                // CSS locales
                else
                {
                    fileName = string.Concat(cssInclude.PathName, ".css");
                    fileWebName = cssInclude.UseDefaultPath ? eTools.WebPathCombine("themes", "default", "css", fileName) : fileName;

                    if (RegisterFromRoot)
                        fileWebName = eTools.WebPathCombine(HttpContext.Current.Request.ApplicationPath, fileWebName);

                    HtmlGenericControl myInclude = new HtmlGenericControl("link");
                    myInclude.Attributes.Add("rel", "stylesheet");
                    myInclude.Attributes.Add("type", "text/css");
                    foreach (KeyValuePair<string, string> keyValue in cssInclude.CpltAttributes)
                        myInclude.Attributes.Add(keyValue.Key, keyValue.Value);


                    myInclude.Attributes.Add("href", string.Concat(fileWebName, "?ver=", _sCSSVersion));

                    _control.Controls.Add(myInclude);
                }
            }

            /** Si le thème est en version 2 ou + on met une feuille de style générique. G.L */
            // et que les valeurs de catalogue ne depasse pas TREEVIEW_HIGH_THRESHOLD (1000)  "Demande 82234 (Affichage - Lenteur ouverture catalogue arborescent)" QBO
            if (_theme.Version > 1 && DisplayTheme2019)
            {
                HtmlGenericControl myBaseCSSHome = new HtmlGenericControl("link");
                myBaseCSSHome.Attributes.Add("rel", "stylesheet");
                myBaseCSSHome.Attributes.Add("type", "text/css");
                myBaseCSSHome.Attributes.Add("href", string.Concat(eTools.WebPathCombine("themes", "Theme2019", "css", "theme.css"), "?ver=", _sCSSVersion));
                myBaseCSSHome.Attributes.Add("eType", "THEMEBASE");
                _control.Controls.Add(myBaseCSSHome);
            }


            fileWebName = eTools.WebPathCombine("themes", _theme.Folder, "css", "theme.css");
            if (RegisterFromRoot)
                fileWebName = eTools.WebPathCombine(HttpContext.Current.Request.ApplicationPath, fileWebName);
            myBaseInclude = new HtmlGenericControl("link");
            myBaseInclude.Attributes.Add("rel", "stylesheet");
            myBaseInclude.Attributes.Add("type", "text/css");
            myBaseInclude.Attributes.Add("href", string.Concat(fileWebName, "?ver=", _sCSSVersion));
            myBaseInclude.Attributes.Add("eType", "THEME");
            _control.Controls.Add(myBaseInclude);



            // CNA - MOU [#38642]:  AJout des styles css pour la page  body de formulaire
            // Attention : les styles redefinies dans le corps de formulaire ecrasent les styles existantes.
            if (!string.IsNullOrEmpty(_rawCss))
            {
                HtmlGenericControl rawCss = new HtmlGenericControl("style");
                rawCss.Attributes.Add("type", "text/css");
                rawCss.InnerHtml = _rawCss;
                _control.Controls.Add(rawCss);
            }
        }

        /// <summary>
        /// Enregistre les css de la page pprincipale du formulaire.
        /// </summary>
        /// <param name="rawCss"></param>
        internal void SetRawCss(string rawCss)
        {
            _rawCss = rawCss;
        }
    }
}