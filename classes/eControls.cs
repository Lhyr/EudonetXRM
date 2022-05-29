using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm
{
    /// <classname>eCheckBoxCtrl</classname>
    /// <summary>Classe qui permet d'afficher une case à cocher</summary>
    /// <purpose>Classe qui permet de générer une case à cocher depuis plusieurs images.
    /// Appelée depuis eList.cs</purpose>
    /// <authors>HLA</authors>
    /// <date>2011-10-17</date>
    public class eCheckBoxCtrl : HyperLink
    {
        #region VARIABLES
        //Bouton Image dans l'élément
        private HtmlGenericControl _imgChk = null;
        bool _disabled = false;

        #endregion
        #region ACCESSEURS
        /// <summary>ToolTip sur la balise image de checkbox</summary>
        public String ToolTipChkBox
        {
            get { return (_imgChk != null) ? _imgChk.Attributes["title"] : string.Empty; }
            set
            {
                if (_imgChk != null)
                    _imgChk.Attributes.Add("title", value);
            }
        }
        #endregion

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="sFct">Fonction additionnelle JavaScript à déclencher lors du clic sur la case à cocher</param>
        public void AddClick(String sFct = "")
        {
            this.Attributes.Add("Onclick", String.Concat(" chgChk(this); ", sFct, "; return false; "));
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="_checked">true pour créer une case cochée, false pour créer une case non cochée</param>
        /// <param name="disabled">true pour créer une case grisée (désactivée), false sinon</param>
        public eCheckBoxCtrl(Boolean _checked, Boolean disabled)
        {
            this.NavigateUrl = "#";
            _disabled = disabled;

            String iconClass = "icon-square-o";
            if (_checked)
            {
                iconClass = (disabled) ? "icon-check-square-o" : "icon-check-square";
            }
            else if (disabled)
            {
                iconClass = "icon-square";
            }
            _imgChk = new HtmlGenericControl();
            _imgChk.Attributes.Add("class", iconClass);
            this.Controls.Add(_imgChk);
            //_imgChk.ImageUrl = eConst.GHOST_IMG;
            this.CssClass = "rChk chk";

            this.Attributes.Add("dis", disabled ? "1" : "0");
            this.Attributes.Add("chk", _checked ? "1" : "0");
            this.Attributes.Add("align", "top");
        }

        /// <summary>
        /// Ajoute une classe CSS au contrôle
        /// </summary>
        /// <param name="sSubClass">Classe CSS à ajouter</param>
        public void AddClass(String sSubClass)
        {

            if (!String.IsNullOrEmpty(sSubClass))
                this.CssClass = String.Concat(this.CssClass, " ", sSubClass);
        }
        /// <summary>
        /// Ajoute du texte après le bouton (le texte est ainsi cliquable vace le même event que l'image)
        /// </summary>
        /// <param name="sText">Texte à ajouter</param>
        public void AddText(String sText, String sTooltip = "")
        {
            if (!String.IsNullOrEmpty(sText))
            {
                this.Attributes.Add("title", (!String.IsNullOrEmpty(sTooltip)) ? sTooltip : sText); // #44549 : Ajout tooltip
                //Anti XSS
                sText = HttpUtility.HtmlEncode(sText);

                HtmlGenericControl spanText = new HtmlGenericControl();
                spanText.InnerHtml = sText;
                this.Controls.Add(spanText);

            }
        }

        /// <summary>
        /// Coche ou décoche le contrôle case à cocher
        /// </summary>
        /// <param name="_checked">true pour cocher la case, false pour la décocher</param>
        public void SetChecked(Boolean _checked)
        {
            string iconClass = "icon-square-o";

            this.Attributes.Add("chk", _checked ? "1" : "0");

            if (_checked)
            {
                iconClass = (_disabled) ? "icon-check-square-o" : "icon-check-square";
            }
            _imgChk.Attributes["class"] = iconClass;
        }

        /// <summary>
        /// Active ou désactive le contrôle case à cocher
        /// </summary>
        /// <param name="disabled">true pour désactiver la case, false pour l'activer</param>
        public void SetDisabled(Boolean disabled)
        {
            _disabled = disabled;

            this.Attributes.Add("dis", disabled ? "1" : "0");

            // Gestion de l'apparence de la case à cocher
            String iconClass = "icon-square-o";
            if (this.Attributes["chk"] == "1")
            {
                iconClass = (disabled) ? "icon-check-square-o" : "icon-check-square";
            }
            else if (disabled)
            {
                iconClass = "icon-square";
            }
            _imgChk.Attributes["class"] = _imgChk.Attributes["class"].Replace("icon-square-o", "");
            _imgChk.Attributes["class"] = _imgChk.Attributes["class"].Replace("icon-check-square-o", "");
            _imgChk.Attributes["class"] = _imgChk.Attributes["class"].Replace("icon-check-square", "");
            _imgChk.Attributes["class"] = _imgChk.Attributes["class"].Replace("icon-square", "");

            _imgChk.Attributes["class"] += " " + iconClass;
        }

    }


    /// <classname>eIconCtrl</classname>
    /// <summary>Classe qui permet d'afficher une icône depuis une clé CSS extraite d'une image</summary>
    /// <purpose>Classe qui permet d'afficher une icône depuis une clé CSS.
    /// Appelée depuis eList.cs</purpose>
    /// <authors>HLA</authors>
    /// <date>2011-10-17</date>
    public class eIconCtrl : System.Web.UI.WebControls.Image
    {
        /// <summary>
        /// Ajoute une classe CSS sur le contrôle
        /// </summary>
        /// <param name="sSubClass">Classe CSS à ajouter</param>
        public void AddClass(String sSubClass)
        {
            if (!String.IsNullOrEmpty(sSubClass))
                this.CssClass = String.Concat(this.CssClass, " ", sSubClass);
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        public eIconCtrl()
            : this(String.Empty)
        {
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="sSubClass">Classe CSS à appliquer sur le contrôle</param>
        public eIconCtrl(String sSubClass)
        {

            this.ImageUrl = eConst.GHOST_IMG;
            this.CssClass = "rIco";

            if (!String.IsNullOrEmpty(sSubClass))
                AddClass(sSubClass);
        }
    }

    /// <classname>eFontIconCtrl</classname>
    /// <summary>Classe qui permet d'afficher une icône police depuis une clé CSS extraite d'une image</summary>
    /// <purpose>Classe qui permet d'afficher une icône police depuis une clé CSS.
    /// Appelée depuis eList.cs</purpose>
    /// <authors>MAB, d'après eIconCtrl</authors>
    /// <date>2014-12-127</date>
    public class eFontIconCtrl : HtmlGenericControl
    {
        /// <summary>
        /// Ajoute une classe CSS sur le contrôle
        /// </summary>
        /// <param name="sSubClass">Classe CSS à ajouter</param>
        public void AddClass(String sSubClass)
        {
            if (!String.IsNullOrEmpty(sSubClass))
                this.Attributes["class"] = String.Concat(this.Attributes["class"], sSubClass);
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        public eFontIconCtrl()
            : this(String.Empty)
        {
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="sSubClass">Classe CSS à appliquer sur le contrôle</param>
        public eFontIconCtrl(String sSubClass)
        {

            this.Attributes.Add("class", "icon-");

            if (!String.IsNullOrEmpty(sSubClass))
                AddClass(sSubClass);
        }
    }

    /// <summary>
    /// Contrôle de type "Bouton" avec style personnalisable
    /// </summary>
    /// <authors>MAB</authors>
    /// <date>2012-09-17</date>
    public class eButtonCtrl : System.Web.UI.WebControls.Panel
    {
        /// <summary>
        /// Classes CSS prédéfinies à utiliser pour définir le style du bouton
        /// </summary>
        public struct ButtonType
        {
            /// <summary>
            /// Bouton vert (type Valider)
            /// </summary>
            public const string GREEN = "button-green";
            /// <summary>
            /// Bouton gris
            /// </summary>
            public const string GRAY = "button-gray";
            /// <summary>
            /// Bouton gris (type précédent)
            /// </summary>
            public const string GRAY_LEFT_ARROW = "button-gray-left-arrow";
            /// <summary>
            /// Bouton gris (type suivant)
            /// </summary>
            public const string GRAY_RIGHT_ARROW = "button-gray-right-arrow";
            /// <summary>
            /// Bouton vert (type précédent)
            /// </summary>
            public const string GREEN_LEFT_ARROW = "button-green-left-arrow";
            /// <summary>
            /// Bouton vert (type suivant)
            /// </summary>
            public const string GREEN_RIGHT_ARROW = "button-green-right-arrow";


            /// <summary>
            /// Bouton dans l'admin des menu
            /// </summary>
            public const string ADMIN_MENU = "button-gray";
        }

        /// <summary>
        /// Renvoie ou définit l'action JavaScript à exécuter lors du clic sur le bouton (attribut onclick)
        /// </summary>
        public string OnClick
        {
            get { return this.Attributes["onclick"]; }
            set { this.Attributes["onclick"] = value; }
        }

        /// <summary>
        /// Ajoute une classe CSS sur le bouton
        /// </summary>
        /// <param name="strSubClass">Classe CSS à ajouter</param>
        public void AddClass(String strSubClass)
        {
            if (!String.IsNullOrEmpty(strSubClass))
                this.CssClass = String.Concat(this.CssClass, " ", strSubClass);
        }

        /// <summary>
        /// Constructeur de eButtonCtrl
        /// </summary>
        /// <param name="strLabel">Libellé du bouton</param>
        public eButtonCtrl(String strLabel)
            : this(strLabel, String.Empty)
        {
        }

        /// <summary>
        /// Constructeur de eButtonCtrl
        /// </summary>
        /// <param name="strLabel">Libellé du bouton</param>
        /// <param name="strSubClass">Classe CSS à appliquer sur le bouton</param>
        public eButtonCtrl(String strLabel, String strSubClass)
            : this(strLabel, strSubClass, String.Empty)
        {
        }

        /// <summary>
        /// Constructeur de eButtonCtrl
        /// </summary>
        /// <param name="strLabel">Libellé du bouton</param>
        /// <param name="strSubClass">Classe CSS à appliquer sur le bouton</param>
        /// <param name="strOnClick">Action JavaScript à effectuer lors du clic sur le bouton (attribut onclick)</param>
        public eButtonCtrl(String strLabel, String strSubClass, String strOnClick)
        {
            // Structure de base
            if (!String.IsNullOrEmpty(strSubClass))
            {
                AddClass(strSubClass);
                strSubClass = String.Concat(strSubClass, "-"); // séparateur pour les parties gauche/milieu/droite
            }
            this.AddClass("eButtonCtrl");

            // Partie gauche
            HtmlGenericControl divLeft = new HtmlGenericControl("div");
            divLeft.Attributes.Add("class", String.Concat(strSubClass, "left"));
            this.Controls.Add(divLeft);

            // Partie milieu avec libellé
            HtmlGenericControl divMid = new HtmlGenericControl("div");
            divMid.Attributes.Add("class", String.Concat(strSubClass, "mid"));
            divMid.InnerHtml = strLabel;
            this.Controls.Add(divMid);

            // Partie droite
            HtmlGenericControl divRight = new HtmlGenericControl("div");
            divRight.Attributes.Add("class", String.Concat(strSubClass, "right"));
            this.Controls.Add(divRight);

            // Action lors du clic
            this.Attributes.Add("onclick", strOnClick);
        }
    }

    /// <summary>
    /// WebControl générique
    /// </summary>
    /// <authors>GCH</authors>
    /// <date>2014-09-05</date>
    public class eGenericWebControl : WebControl
    {

        /// <summary>Balise à afficher</summary>
        private HtmlTextWriterTag _tag = HtmlTextWriterTag.Span;
        /// <summary>Balise à afficher</summary>
        protected override HtmlTextWriterTag TagKey
        {
            get { return _tag; }
        }

        /// <summary>Texte à afficher</summary>
        public String InnerText { get; set; }
        /// <summary>attribut OnClick de la balise</summary>
        public String OnClick { get; set; }
        /// <summary>attribut OnDblClick de la balise</summary>
        public String OnDblClick { get; set; }
        /// <summary>attribut OnMouseDown de la balise</summary>
        public String OnMouseDown { get; set; }
        /// <summary>attribut OnMouseOver de la balise</summary>
        public String OnMouseOver { get; set; }
        /// <summary>attribut OnMouseOut de la balise</summary>
        public String OnMouseOut { get; set; }

        /// <summary>Constructeur générique de web control</summary>
        /// <param name="tag">Tag de la balise à générer</param>
        public eGenericWebControl(HtmlTextWriterTag tag)
        {
            _tag = tag;
            InnerText = String.Empty;
            OnClick = String.Empty;
            OnDblClick = String.Empty;
            OnMouseDown = String.Empty;
            OnMouseOver = String.Empty;
            OnMouseOut = String.Empty;
        }
        /// <summary>
        /// </summary>
        /// <param name="writer"></param>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (InnerText.Length > 0)
                this.Controls.Add(new LiteralControl(InnerText));
            base.RenderContents(writer);
        }
        /// <summary>
        /// </summary>
        /// <param name="writer"></param>
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            if (OnClick.Length > 0)
                this.Attributes.Add("OnClick", OnClick);
            if (OnDblClick.Length > 0)
                this.Attributes.Add("OnDblClick", OnDblClick);
            if (OnMouseDown.Length > 0)
                this.Attributes.Add("OnMouseDown", OnMouseDown);
            if (OnMouseOver.Length > 0)
                this.Attributes.Add("OnMouseOver", OnMouseOver);
            if (OnMouseOut.Length > 0)
                this.Attributes.Add("OnMouseOut", OnMouseOut);
            base.AddAttributesToRender(writer);
        }

        /// <summary>
        /// Permet de créer une nouvelle instance d'un control passé en paramètre et de le
        /// </summary>
        /// <typeparam name="T">Control générique</typeparam>
        /// <returns>control instancié et ajouté à la collection du control appelant</returns>
        public T AddControl<T>()
            where T : WebControl, new()
        {
            return AddControl(new T());
        }
        /// <summary>
        /// Permet d'ajouté control passé en paramètre dans la collection de control du control courant
        /// </summary>
        /// <typeparam name="T">Control générique</typeparam>
        /// <param name="ctrl">control déjà instancié</param>
        /// <returns>control passé en paramètre</returns>
        public T AddControl<T>(T ctrl)
            where T : WebControl
        {
            this.Controls.Add(ctrl);
            return ctrl;
        }

    }

    /// <summary>
    /// Contrôle de type "UL"
    /// </summary>
    /// <authors>GCH</authors>
    /// <date>2014-09-05</date>
    public class eUlCtrl : eGenericWebControl
    {
        /// <summary>Constructeur</summary>
        public eUlCtrl() : base(HtmlTextWriterTag.Ul) { }
        /// <summary>Renvoi un WebControl LI qui est automatiquement ajouté au contrôle UL actuelle</summary>
        /// <returns>WebControl LI</returns>
        public eLiCtrl AddLi()
        {
            return AddControl<eLiCtrl>();
        }
        /*
        /// <summary>Reprend le control LI passé et l'ajoute à la collection de li</summary>
        /// <param name="li">WebControl LI</param>
        public void AddLi(eLiCtrl li)
        {
            Controls.Add(li);
        }
        /// <summary>Reprend le control LI passé et l'ajoute à la collection de li</summary>
        /// <param name="index">Emplacement du tableau auquel ajouter le control li</param>
        /// <param name="li">WebControl LI</param>
        public void AddLiAt(int index, eLiCtrl li)
        {
            Controls.AddAt(index, li);
        }
        /// <summary>Dans un ul on autorise aucun autre control que le li donc utiliser AddLi</summary>
        //private ControlCollection Controls { get { return base.Controls; } }
        */

    }
    /// <summary>
    /// Contrôle de type "LI"
    /// </summary>
    /// <authors>GCH</authors>
    /// <date>2014-09-05</date>
    public class eLiCtrl : eGenericWebControl
    {
        /// <summary>Constructeur</summary>
        public eLiCtrl() : base(HtmlTextWriterTag.Li) { }
    }
    /// <summary>
    /// Contrôle de type "DIV"
    /// </summary>
    /// <authors>GCH</authors>
    /// <date>2014-09-05</date>
    public class eDivCtrl : eGenericWebControl
    {
        /// <summary>Constructeur</summary>
        public eDivCtrl() : base(HtmlTextWriterTag.Div) { }
    }
    /// <summary>
    /// Contrôle de type "LABEL"
    /// </summary>
    /// <authors>GCH</authors>
    /// <date>2014-10-08</date>
    public class eLabelCtrl : eGenericWebControl
    {
        /// <summary>Constructeur</summary>
        public eLabelCtrl()
            : base(HtmlTextWriterTag.Label)
        {
            For = String.Empty;
        }
        /// <summary>Attribut de la balise "For"</summary>
        public String For { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="writer"></param>
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            if (For.Length > 0)
                this.Attributes.Add("For", For);
            base.AddAttributesToRender(writer);
        }
    }
}