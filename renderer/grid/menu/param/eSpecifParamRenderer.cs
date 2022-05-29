using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Xrm.eda;
using Com.Eudonet.Internal.eda;
using System.Collections.Generic;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Paramètres des spécif widget (menu droit)
    /// </summary>
    public class eSpecifParamRenderer : eWidgetSpecificParamRenderer
    {
        eSpecif _specif = null;
        string _url = string.Empty;
        string _urlParam = string.Empty;

        /// <summary>
        /// TODO Besoin d'un objet metier de la table
        /// </summary>    
        public eSpecifParamRenderer(ePref pref, bool isVisible, eFile file, eXrmWidgetParam param, eXrmWidgetContext context = null) : base(pref, isVisible, file, param, true, context)
        {

        }

        /// <summary>
        /// Initialisation
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                int specifID = eLibTools.GetNum(_file.GetField((int)XrmWidgetField.ContentSource).Value);
                if (specifID > 0)
                {
                    _specif = eSpecif.GetSpecif(Pref, specifID);
                    if (_specif != null)
                    {
                        _url = _specif.Url;
                        _urlParam = _specif.UrlParam;
                    }
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Construit un block du contenu specifique au widget
        /// </summary>
        /// <returns></returns>
        protected override void BuildWidgetContentPart()
        {
            #region Hidden
            _divHidden = new HtmlGenericControl("div");
            _divHidden.Style.Add("visibility", "hidden");
            _divHidden.Style.Add("display", "none");
            _pgContainer.Controls.Add(_divHidden);
            HtmlInputHidden hidden = new HtmlInputHidden();
            hidden.Value = (_specif != null) ? _specif.SpecifId.ToString() : "0";
            hidden.ID = "hidSpecifID";
            _divHidden.Controls.Add(hidden);
            #endregion

            // SpecifID
            HtmlGenericControl inputID = (HtmlGenericControl)(BuildInputField("ID", _file.GetField((int)XrmWidgetField.ContentSource), bDisabled: true));
            inputID.Style.Add("display", "none");
            _pgContainer.Controls.Add(inputID);
            // URL
            _pgContainer.Controls.Add(BuildInput(eResApp.GetRes(Pref, 5143), _url, "specifURL", "oAdminGridMenu.updateSpecifParam(" + _file.FileId + ")"));
            // Paramètres pour l'URL

            _pgContainer.Controls.Add(

                BuildInput(eResApp.GetRes(Pref, 8019), _urlParam, "specifURLParam", "oAdminGridMenu.updateSpecifParam(" + _file.FileId + ")", eResApp.GetRes(Pref, 8053).Replace(@"\n", Environment.NewLine))


                );


            // Type spécif Produits/Classique
            var items = new Dictionary<string, string>();
            items.Add(eLibConst.SPECIF_SOURCE.SRC_XRM.GetHashCode().ToString(), "Classique"); // XRM
            items.Add(eLibConst.SPECIF_SOURCE.SRC_EXT.GetHashCode().ToString(), "Produits"); // PRODUITD

            //les widgets ont un system de maj différent du reste des options d'admin
            // il n'y a pas de méthodes centralisées des maj des propriétés mais des méthodes
            // par "type". Pour les spécif, il s'agit de updateSpecifParam qui met à jour
            // globalement toutes les propriétés
            Dictionary<string, string> attr = new Dictionary<string, string>();
            attr.Add("fid", _specif?.SpecifId.ToString() ?? "");
            attr.Add("onclick", "oAdminGridMenu.updateSpecifParam(" + _file.FileId + ")");

            eAdminRadioButtonField field = new eAdminRadioButtonField(
                descid: _tab,
                label: "Type de spécif",
                propCat: eAdminUpdateProperty.CATEGORY.SPECIFS,
                propCode: (int)eLibConst.SPECIFS.SOURCE,
                groupName: "rbSpecifSource",
                items: items,
                value: ((int)(_specif?.Source ?? eLibConst.SPECIF_SOURCE.SRC_XRM)).ToString(),
                customRadioButtonAttributes: attr

                );

            field.IsLabelBefore = true;

            field.Generate(_pgContainer);




            // Type spécif Produits/Classique
            var itemTokens = new Dictionary<string, string>();
            itemTokens.Add("0", "POST"); // POST
            itemTokens.Add("1", "GET"); // GET

            //les widgets ont un system de maj différent du reste des options d'admin
            // il n'y a pas de méthodes centralisées des maj des propriétés mais des méthodes
            // par "type". Pour les spécif, il s'agit de updateSpecifParam qui met à jour
            // globalement toutes les propriétés
            Dictionary<string, string> attrToken = new Dictionary<string, string>();
            attrToken.Add("fid", _specif?.SpecifId.ToString() ?? "");
            attrToken.Add("onclick", "oAdminGridMenu.updateSpecifParam(" + _file.FileId + ")");

            eAdminRadioButtonField fieldToken = new eAdminRadioButtonField(
                descid: _tab,
                label: "Type de lancement",
                propCat: eAdminUpdateProperty.CATEGORY.SPECIFS,
                propCode: (int)eLibConst.SPECIFS.ISSTATIC,
                groupName: "rbSpecifStatic",
                items: itemTokens,
                value: ((_specif?.IsStatic ?? false) ? "1" : "0"),
                customRadioButtonAttributes: attrToken

                );

            fieldToken.IsLabelBefore = true;

            if (this.Pref.User.UserLevel > (int)UserLevel.LEV_USR_ADMIN)
                fieldToken.Generate(_pgContainer);
        }
    }
}