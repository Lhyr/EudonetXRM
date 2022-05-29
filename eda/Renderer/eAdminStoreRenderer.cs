using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using static Com.Eudonet.Internal.eLibConst;
using System.Linq;
using System.Web.UI.HtmlControls;
using System;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>    /// 
    /// Renderer de base des écrans affichant les infos liées aux extensions (liste des extensions, fiche d'une extension)
    /// </summary>
    public class eAdminStoreRenderer : eAdminModuleRenderer
    {
        #region variables
        /// <summary>
        /// Indique si l'accès aux informations d'extensions peut se faire en effectuant une connexion API vers HotCom, ou si on doit passer par un cache
        /// d'extensions local (ExtensionList.json dans /eudonetXRM/Res/). Cette variable est mise à true si la connexion vers HotCom échoue,
        /// ou si la clé ServerWithoutInternet est valorisée à 1 dans le fichier server.config de /eudonetXRM (cf. ci-dessous)
        /// </summary>
        protected bool bInternet = true;
        #endregion

        #region constructeurs
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        protected eAdminStoreRenderer(ePref pref) : base(pref)
        {
            bInternet = eLibTools.GetServerConfig("ServerWithoutInternet", "1") == "0";
        }

        /// <summary>
        /// Retourne le renderer d'une extension en  mode fiche
        /// </summary>
        /// <param name="pref"></param>
        internal static eAdminStoreRenderer GetAdminStoreFileRenderer(ePref pref)
        {
            return new eAdminStoreRenderer(pref);
        }
        #endregion

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (!eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminExtensions))
                return false;

            return base.Init();
        }

        /// <summary>
        /// Construction du renderer
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (!base.Build())
                return false;

            return true;

        }

        /// <summary>
        /// Retourne l'icone et le text en fonction du status de l'extension
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="cssClass"></param>
        /// <param name="tooltip"></param>
        /// <param name="txt"></param>
        /// <returns></returns>
        protected bool AddStatusIcon(eAdminExtension ext, out string cssClass, out string tooltip, out string txt)
        {
            cssClass = string.Empty;
            tooltip = string.Empty;
            txt = string.Empty;

            if (ext.Infos.Status == EXTENSION_STATUS.STATUS_ACTIVATION_ASKED)
            {
                // Extension en attente d'activation
                cssClass = "rond attente-exts icon-clock-o";
                tooltip = eResApp.GetRes(_ePref, 2264);
                txt = eResApp.GetRes(_ePref, 2309);
                return true;
            }
            else if (ext.Infos.Status == EXTENSION_STATUS.STATUS_DESACTIVATION_ASKED)
            {
                // Extension en attente de désactivation
                cssClass = "rond attente-exts icon-clock-o";
                tooltip = eResApp.GetRes(_ePref, 2266);
                txt = eResApp.GetRes(_ePref, 2310);
                return true;
            }
            else if (ext.Infos.IsEnabled)
            {
                // Extension installée
                cssClass = "rond active-exts icon-check-circle";
                tooltip = eResApp.GetRes(_ePref, 2265);
                txt = eResApp.GetRes(_ePref, 2311);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Construit les images d'indication des offres
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="bDescOrder">Tri descendant par offre</param>
        /// <returns></returns>
        protected IEnumerable<Control> GetOfferImg(eAdminExtension ext, bool bDescOrder = true)
        {
            return GetOfferImg(ext.Infos.OffersLst, bDescOrder);
        }



        /// <summary>
        /// Dropdown spécialisé pour les mapping
        /// </summary>
        /// <param name="mapDescid"></param>
        /// <param name="id"></param>
        /// <param name="jsOnChange"></param>
        /// <returns></returns>
        protected Panel GetMappingDropDownList(ParamMappingDescId mapDescid, string id, string jsOnChange)
        {
            Panel field = new Panel();
            field.CssClass = "field DropDownMapping";
            field.ID = String.Concat("List_", id, "_", mapDescid.Key);
            HtmlGenericControl fieldLabel = new HtmlGenericControl("label");
            fieldLabel.InnerText = mapDescid.Label;




            DropDownList ddl = new DropDownList();

            ddl.ID = String.Concat("ListDdl_", id, "_", mapDescid.Key);



            if (mapDescid.List.Exists(z => z.Key == mapDescid.SelectedKey))
                ddl.SelectedValue = mapDescid.SelectedKey.ToString();


            if (!string.IsNullOrEmpty(jsOnChange))
                ddl.Attributes.Add("onchange", jsOnChange);

            if (mapDescid.SelectedKey != 0)
            {
                ddl.Attributes.Add("elastvalid", mapDescid.SelectedKey.ToString());
            }

            foreach (ParamMappingDescId paramD in mapDescid.List)
            {
                ListItem item = new ListItem(paramD.Label, paramD.Key.ToString());

                if (paramD.IsDisabled)
                    item.Attributes.Add("disable", "1");

                if (paramD.IsMandatory)
                    item.Attributes.Add("mandatory", "1");

                if (!String.IsNullOrEmpty(paramD.Css))
                    item.Attributes["class"] = paramD.Css;

                if (mapDescid.SelectedKey != 0 && mapDescid.SelectedKey == paramD.Key)
                    item.Attributes.Add("selected", "1");

                ddl.Items.Add(item);
            }

            /*
            ddl.DataSource = mapDescid.List;

            ddl.DataTextField = "Label";
            ddl.DataValueField = "Key";
            

            ddl.DataBind();
            */


            // Attributs concernant le champ dans la table MOBILE


            field.Attributes.Add("key", mapDescid.Key.ToString());


            field.Controls.Add(fieldLabel);

            if (mapDescid.IsMandatory)
            {
                HtmlGenericControl mandatoryParam = new HtmlGenericControl("span");

                mandatoryParam.Attributes.Add("class", "mandatoryParam");
                mandatoryParam.InnerText = "*";

                field.Controls.Add(mandatoryParam);
            }

            field.Controls.Add(ddl);

            return field;
        }

        /// <summary>
        /// TextBox spécialisé pour les mapping
        /// </summary>
        /// <param name="mapDescid"></param>
        /// <param name="id"></param>
        /// <param name="jsOnChange"></param>
        /// <returns></returns>
        protected Panel GetMappingTextBox(ParamMappingDescId mapDescid, string id, string jsOnChange)
        {
            Panel field = new Panel();
            field.CssClass = "field DropDownMapping";
            field.ID = String.Concat("List_", id, "_", mapDescid.Key);
            HtmlGenericControl fieldLabel = new HtmlGenericControl("label");
            fieldLabel.InnerText = mapDescid.Label;

            DropDownList ddl = new DropDownList();
            ddl.ID = String.Concat("ListDdl_", id, "_", mapDescid.Key);

            if (mapDescid.List.Exists(z => z.Key == mapDescid.SelectedKey))
                ddl.SelectedValue = mapDescid.SelectedKey.ToString();

            if (!string.IsNullOrEmpty(jsOnChange))
                ddl.Attributes.Add("onchange", jsOnChange);

            if (mapDescid.SelectedKey != 0)
                ddl.Attributes.Add("elastvalid", mapDescid.SelectedKey.ToString());

            foreach (ParamMappingDescId paramD in mapDescid.List)
            {
                ListItem item = new ListItem(paramD.Label, paramD.Key.ToString());

                if (paramD.IsDisabled)
                    item.Attributes.Add("disable", "1");

                if (paramD.IsMandatory)
                    item.Attributes.Add("mandatory", "1");

                if (!String.IsNullOrEmpty(paramD.Css))
                    item.Attributes["class"] = paramD.Css;

                if (mapDescid.SelectedKey != 0 && mapDescid.SelectedKey == paramD.Key)
                    item.Attributes.Add("selected", "1");

                ddl.Items.Add(item);
            }

            field.Attributes.Add("key", mapDescid.Key.ToString());


            field.Controls.Add(fieldLabel);

            if (mapDescid.IsMandatory)
            {
                HtmlGenericControl mandatoryParam = new HtmlGenericControl("span");

                mandatoryParam.Attributes.Add("class", "mandatoryParam");
                mandatoryParam.InnerText = "*";

                field.Controls.Add(mandatoryParam);
            }

            field.Controls.Add(ddl);

            return field;
        }



        /// <summary>
        /// Retourne l'url de l'icone d'une offre
        /// </summary>
        /// <param name="offer"></param>
        /// <param name="Pref"></param>
        /// <returns></returns>
        public static string GetOfferImgUrl(ClientOffer offer, ePref Pref)
        {
            switch (offer)
            {
                case ClientOffer.ACCES:
                    return string.Concat("themes/", Pref.ThemePaths.GetImageWebPath("/images/store/medal-Access.png"));
                case ClientOffer.STANDARD:
                    return string.Concat("themes/", Pref.ThemePaths.GetImageWebPath("/images/store/medal-Standard.png"));
                case ClientOffer.PREMIER:
                    return string.Concat("themes/", Pref.ThemePaths.GetImageWebPath("/images/store/medal-Premier.png"));
                case ClientOffer.PRO:
                    return string.Concat("themes/", Pref.ThemePaths.GetImageWebPath("/images/store/medal-Pro.png"));
                case ClientOffer.ESSENTIEL:
                    return string.Concat("themes/", Pref.ThemePaths.GetImageWebPath("/images/store/medal-Essentiel.png"));
                default:
                    return string.Concat("themes/", Pref.ThemePaths.GetImageWebPath("/images/store/medal-Essentiel.png"));
            }
        }



        /// <summary>
        /// Retourne l'url de l'icone d'une offre
        /// </summary>
        /// <param name="offer"></param>
        /// <returns></returns>
        public static string GetOfferCss(ClientOffer offer)
        {
            switch (offer)
            {
                case ClientOffer.ACCES:
                    return "img-view-access";
                case ClientOffer.STANDARD:
                case ClientOffer.PREMIER:
                    return "img-view-" + offer.ToString().ToLower();
                default:
                    return "img-view-premier";
            }
        }

        /// <summary>
        /// retourne un objet image pour l'offre demandé
        /// </summary>
        /// <param name="offer"></param>
        /// <param name="Pref"></param>
        /// <returns></returns>
        public static Image GetOfferImage(ClientOffer offer, ePref Pref)
        {
            if (offer == ClientOffer.XRM)
                return null;

            string sOfferName = offer.ToString();

            return new Image
            {
                CssClass = GetOfferCss(offer),
                ImageUrl = GetOfferImgUrl(offer, Pref),
                ToolTip = string.Format(eResApp.GetRes(Pref, 8214), eResApp.GetRes(Pref, 8215), offer.ToString()),
                AlternateText = string.Format(eResApp.GetRes(Pref, 8214), eResApp.GetRes(Pref, 8215), offer.ToString()),
            };
        }

        /// <summary>
        /// Construit les images d'indication des offres
        /// </summary>
        /// <param name="extensions"></param>
        ///   <param name="DescOrder">Tri descendant par offre</param>
        /// <returns></returns>
        protected IEnumerable<Control> GetOfferImg(IEnumerable<ExtensionOffer> extensions, bool DescOrder = true)
        {
            List<Control> ctrls = new List<Control>();

            if (extensions == null)
                return ctrls;


            // filtre
            IEnumerable<eLibConst.ClientOffer> lstOffers = extensions
                        .Select(zz => zz.Offer)
                        .Where(off => off != eLibConst.ClientOffer.XRM); // pas xrm  ;

            //tri
            lstOffers = DescOrder ? lstOffers.OrderByDescending(off => eLibTools.SortClientOffer(off)) : lstOffers.OrderBy(off => eLibTools.SortClientOffer(off));      //trié par "niveau de l'offre"


            foreach (ClientOffer e in lstOffers)
            {
                Image img = GetOfferImage(e, Pref);

                if (img != null)
                    ctrls.Add(img);
            }
            return ctrls;
        }
    }



    /// <summary>
    /// Classe de représentation d'un élément à mapper
    /// </summary>
    public class ParamMappingDescId
    {
        /// <summary>
        /// List des possibilité de mapping
        /// </summary>
        public List<ParamMappingDescId> List = new List<ParamMappingDescId>();

        /// <summary>
        /// Une valeur doit être renseigné
        /// </summary>
        public bool IsMandatory { get; set; } = false;


        /// <summary>
        /// Valeur désactivé
        /// </summary>
        public bool IsDisabled { get; set; } = false;

        /// <summary>
        /// Nom de la propriété a mappé
        /// </summary>
        public string Label { get; set; } = "";

        /// <summary>
        /// tooltip pour la propriété
        /// </summary>
        public string ToolTip { get; set; } = "";

        /// <summary>
        /// Class CSS 
        /// </summary>
        public string Css { get; set; } = "";

        /// <summary>
        /// Clé de la propriété a mapper
        /// </summary>
        public int Key { get; set; } = 0;


        /// <summary>
        /// Valeur sélectionnée
        /// </summary>
        public int SelectedKey { get; set; } = 0;

        /// <summary>
        /// Retourne un ParamMappingDescId pret a l'utilisation
        /// </summary>
        /// <param name="label"></param>
        /// <param name="Key"></param>
        /// <param name="dicList"></param>
        /// <param name="selectedValue"></param>
        /// <param name="tooltip"></param>
        /// <param name="mandatory">paramètre obligatoire</param>
        /// <returns></returns>
        public static ParamMappingDescId GetParamMapping(string label, int Key, IEnumerable<KeyValuePair<int, string>> dicList, int selectedValue = 0, string tooltip = "", bool mandatory = false)
        {
            ParamMappingDescId d = new ParamMappingDescId() { Label = label, Key = Key, SelectedKey = selectedValue, ToolTip = tooltip, IsMandatory = mandatory };

            foreach (var val in dicList)
            {
                d.List.Add(new ParamMappingDescId() { Label = val.Value, Key = val.Key });
            }

            return d;
        }

        /// <summary>
        /// Retourne un ParamMappingDescId prêt à l'utilisation
        /// </summary>
        /// <param name="label"></param>
        /// <param name="Key"></param>
        /// <param name="dicList"></param>
        /// <param name="selectedValue"></param>
        /// <param name="tooltip"></param>
        /// <param name="mandatory">paramètre obligatoire</param>
        /// <returns></returns>
        public static ParamMappingDescId GetParamsMapping(string label, int Key, int selectedValue = 0, string tooltip = "", bool mandatory = false)
        {
            ParamMappingDescId d = new ParamMappingDescId() { Label = label, Key = Key, SelectedKey = selectedValue, ToolTip = tooltip, IsMandatory = mandatory };
            return d;
        }
    }
}