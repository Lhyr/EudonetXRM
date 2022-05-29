using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Champ de type boutton
    /// </summary>
    public class eAdminButtonField : eAdminField
    {


        private eAdminButtonParams _params = new eAdminButtonParams();

        /// <summary>
        /// Retourne un eAdminButtonField
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static eAdminButtonField GetEAdminButtonField(eAdminButtonParams param)
        {
            return new eAdminButtonField(param);
        }


        private eAdminButtonField(eAdminButtonParams param) : base(0, param.Label, param.ToolTip)
        {
            _params = param;

            this.FieldControlID = _params.ID;
            this.ReadOnly = _params.Disabled;
        }

        /// <summary>
        /// Constructeur globle
        /// </summary>
        /// <param name="label">Libellé btn</param>
        /// <param name="idButton">Id du bouton</param>
        /// <param name="tooltiptext">tooltip</param>
        /// <param name="onclick">action onclick</param>
        /// <param name="href">url</param>
        /// <param name="iconClass">Icon bouton</param>
        /// <param name="readOnly">readonly</param>
        [Obsolete("Utiliser GetEAdminButtonField")]
        public eAdminButtonField(String label, String idButton, String tooltiptext = "", String onclick = "", String href = "", String iconClass = "", bool readOnly = false)
            : base(0, label, tooltiptext)
        {
            _params = new eAdminButtonParams()
            {
                Label = label,
                ID = idButton,
                ToolTip = tooltiptext,
                Href = href,
                Disabled = readOnly,
                IconClass = iconClass,
                OnClick = onclick

            };

            this.FieldControlID = _params.ID;
            this.ReadOnly = _params.Disabled;
        }

        HyperLink _link = new HyperLink();

        /// <summary>
        /// Liste d'attribut supplémentaire
        /// </summary>
        public List<Tuple<string, string>> LnkAttributes = new List<Tuple<string, string>>();

        /// <summary>
        /// override constuction - ajout l'hyperlien / onclick
        /// </summary>
        /// <param name="panel">paneau conteneur</param>
        /// <returns>réussite/echec opération</returns>
        protected override Boolean Build(Panel panel)
        {


            if (base.Build(panel))
            {


                if (!String.IsNullOrEmpty(_params.IconClass))
                {
                    HtmlGenericControl icon = new HtmlGenericControl();
                    icon.Attributes.Add("class", _params.IconClass);
                    _link.Controls.Add(icon);
                }
                LiteralControl lit = new LiteralControl(this.Label);
                _link.Controls.Add(lit);



                _link.ID = this.FieldControlID;
                if (!String.IsNullOrEmpty(_params.Href))
                {
                    _link.NavigateUrl = _params.Href;
                    _link.Target = "_blank";
                }
                else
                {
                    _link.NavigateUrl = "#";
                }
                if (!String.IsNullOrEmpty(_params.OnClick) && !this.ReadOnly)
                {
                    _link.Attributes.Add("onclick", _params.OnClick);
                }
                if (this.ReadOnly)
                    _link.Attributes.Add("data-ro", "1");

                if (_params.Attributes?.Count > 0)
                {
                    foreach(var p in _params.Attributes)
                    {

                        _link.Attributes.Add(p.Item1, p.Item2);
                    }
                }

                this.PanelField.Controls.Add(_link);

                panel.Controls.Add(this.PanelField);

                this.FieldControl = _link;

                return true;
            }

            return false;

        }
    }


    /// <summary>
    /// Classe d'attributs possible pour un bouton
    /// </summary>
    public class eAdminButtonParams
    {
        /// <summary>
        /// Label du bouton (localisé)
        /// </summary>
        public string Label { get; set; } = "";

        /// <summary>
        /// Id du bouton
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        /// Tooltip du bouton (localisé)
        /// </summary>
        public string ToolTip { get; set; } = "";

        /// <summary>
        /// Action JS sur le onclick
        /// </summary>
        public string OnClick { get; set; } = "";

        /// <summary>
        /// Action Href
        /// </summary>
        public string Href { get; set; } = "";

        /// <summary>
        /// Class pour l'icone
        /// </summary>
        public string IconClass { get; set; } = "";

        /// <summary>
        /// Bouton désactivé
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Liste des attributs sur le bouton (positionnés sur le lien)
        /// </summary>
        public List<Tuple<string, string>> Attributes = new List<Tuple<string, string>>();
    }
}