using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Rendu des propriétés de spécif
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eda.eAdminRenderer" />
    public abstract class eAdminSpecifPropertiesRenderer : eAdminRenderer
    {
        /// <summary>
        /// Liste des spécifs
        /// </summary>
        protected List<eSpecif> _listSpecifs;
        /// <summary>
        /// L'objet spécif
        /// </summary>
        protected eSpecif _specif;
        /// <summary>
        /// ID de la spécif
        /// </summary>
        protected Int32 _specifID;
        /// <summary>
        /// Liste des types de spécif
        /// </summary>
        protected List<eLibConst.SPECIF_TYPE> _specifTypes;
        /// <summary>
        /// Panel contenu complet
        /// </summary>
        protected Panel _panelContent;
        /// <summary>
        /// Panel contenant les champs de paramètres
        /// </summary>dw
        protected Panel _panelParametersContent;
        /// <summary>
        /// Catégorie de mise à jour des paramètres
        /// </summary>
        protected eAdminUpdateProperty.CATEGORY _updateCat;
        /// <summary>
        /// Titre du bloc de paramètres
        /// </summary>
        protected String _blockTitle;
        /// <summary>
        /// ID du Panel "paramPartContent"
        /// </summary>
        protected String _idPart;
        /// <summary>
        /// Libellés
        /// </summary>
        protected String _labelName;
        protected String _labelURL;
        protected String _labelURLParam;
        protected String _labelOpenMode;
        protected String _labelAdminURL;
        protected String _labelSpecifPosition;
        protected String _tooltipURL;
        /// <summary>
        /// Modes d'ouverture de spécif disponibles pour le type de spécif
        /// </summary>
        protected List<eLibConst.SPECIF_OPENMODE> _openModes;


        /// <summary>
        /// Création d'un modèle de renderer pour l'affichage des propriétés d'une spécif
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="cat">Catégorie de la mise à jour</param>
        /// <param name="tab">DescId de la table de la spécif</param>
        /// <param name="specifID">ID de la spécif</param>
        /// <param name="specifTypes">The specif types.</param>
        /// <param name="idPart">ID du Panel "paramPartContent"</param>
        /// <exception cref="EudoAdminInvalidRightException"></exception>
        protected eAdminSpecifPropertiesRenderer(ePref pref, eAdminUpdateProperty.CATEGORY cat, int tab, int specifID, List<eLibConst.SPECIF_TYPE> specifTypes, String idPart = "")
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            _specifID = specifID;
            _tab = tab;
            this.Pref = pref;
            _specifTypes = specifTypes;
            _updateCat = cat;
            _blockTitle = eResApp.GetRes(pref, 382);
            _idPart = idPart;
        }

        /// <summary>
        /// Définit les libellés des champs
        /// </summary>
        protected virtual void SetLabels()
        {
            _labelName = eResApp.GetRes(Pref, 223);
            _labelURL = eResApp.GetRes(Pref, 5143);
            _labelURLParam = eResApp.GetRes(Pref, 7600);

            _labelOpenMode = eResApp.GetRes(Pref, 7822);
            _labelAdminURL = eResApp.GetRes(Pref, 7825);
            _labelSpecifPosition = eResApp.GetRes(Pref, 7595);
        }

        /// <summary>
        /// Définit les modes d'ouverture de spécif
        /// </summary>
        protected virtual void SetAvailableOpenModes()
        {
            _openModes = new List<eLibConst.SPECIF_OPENMODE> { eLibConst.SPECIF_OPENMODE.MODAL, eLibConst.SPECIF_OPENMODE.HIDDEN, eLibConst.SPECIF_OPENMODE.IFRAME, eLibConst.SPECIF_OPENMODE.NEW_WINDOW };
        }

        /// <summary>
        /// Initialisation des propriétes specif
        /// </summary>
        protected virtual Boolean InitProperties()
        {
            _listSpecifs = eSpecif.GetSpecifList(Pref, _specifTypes, _tab);
            _specif = _listSpecifs.Find(a => a.SpecifId == _specifID);

            if (_specif == null)
                return false;

            return true;
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                if (!InitProperties())
                    return false;

                // Libellés
                SetLabels();

                SetAvailableOpenModes();

            }
            return true;
        }


        /// <summary>
        /// Construit le html de l'objet demandé
        /// </summary>
        /// <returns></returns>
        protected override Boolean Build()
        {
            if (base.Build())
            {
                // Titre
                BuildTitle();

                // Conteneur
                BuildPanelContent();

                // Contenu
                BuildParametersContent();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Génère le titre de l'onglet
        /// </summary>
        protected virtual void BuildTitle()
        {
            _pgContainer.ID = "paramTab2";
            _pgContainer.Attributes.Add("class", "paramBlock");
            _pgContainer.Style.Add(HtmlTextWriterStyle.Display, "none");
            HtmlGenericControl title = new HtmlGenericControl("h3");
            title.ID = "paramTitleTab2";
            title.InnerHtml = _blockTitle;
            _pgContainer.Controls.Add(title);
        }

        /// <summary>
        /// Génère un Panel conteneur 
        /// </summary>
        /// <returns></returns>
        protected virtual void BuildPanelContent()
        {

            _panelContent = new Panel();
            _panelContent.CssClass = "paramPartContent " + _idPart;
            _panelContent.ID = _idPart;
            _panelContent.Attributes.Add("data-active", "1");
            _panelContent.Attributes.Add("eactive", "1");

            if (_specif != null)
                _panelContent.Attributes.Add("fid", _specif.SpecifId.ToString());

            _pgContainer.Controls.Add(_panelContent);
        }

        /// <summary>
        /// Génère le contenu
        /// </summary>
        protected virtual void BuildParametersContent(Panel panel = null)
        {
            if (panel != null)
                _panelParametersContent = panel;
            else
                _panelParametersContent = _panelContent;

            CreateLabelField();

            CreatePositionField();

            CreateUrlField();

            CreateUrlParamField();

            CreateOpenModeField();

            CreateAdminUrlField();

            CreateAdditionalFields();

            CreateTranslationsField();
        }

        /// <summary>
        /// Creates the additional fields.
        /// </summary>
        protected virtual void CreateAdditionalFields()
        {

        }

        /// <summary>
        /// Creates the translations button
        /// </summary>
        protected virtual void CreateTranslationsField()
        {
            //Traductions
            eAdminTranslation.NATURE nature = eAdminTranslation.NATURE.None;
            switch (_specif.Type)
            {
                case eLibConst.SPECIF_TYPE.TYP_FILE:
                case eLibConst.SPECIF_TYPE.TYP_LIST:
                    nature = eAdminTranslation.NATURE.WebLink;
                    break;
                case eLibConst.SPECIF_TYPE.TYP_WEBTAB_INTERNAL:
                case eLibConst.SPECIF_TYPE.TYP_WEBTAB_EXTERNAL:
                    nature = eAdminTranslation.NATURE.WebTab;
                    break;
                default:
                    break;
            }
            eAdminField button = new eAdminButtonField(eResApp.GetRes(Pref, 7716), "", onclick: String.Format("nsAdmin.openTranslations({0}, {1}, {2});", this._tab, (int)nature, this._specifID));
            button.Generate(_panelContent);
        }

        /// <summary>
        /// Adds the attribute specif identifier.
        /// </summary>
        /// <param name="field">The field.</param>
        protected virtual void AddAttributeSpecifID(eAdminField field)
        {
            ((WebControl)field.FieldControl).Attributes.Add("fid", _specif.SpecifId.ToString());
        }

        /// <summary>
        /// Creates the label field.
        /// </summary>
        /// <returns></returns>
        protected virtual eAdminField CreateLabelField()
        {
            eAdminField field = new eAdminTextboxField(_tab, _labelName, _updateCat, eLibConst.SPECIFS.LABEL.GetHashCode(), EudoQuery.AdminFieldType.ADM_TYPE_CHAR, value: _specif.Label);
            field.Generate(_panelParametersContent);
            AddAttributeSpecifID(field);
            return field;
        }

        /// <summary>
        /// Ordre d'affichage
        /// </summary>
        /// <returns></returns>
        protected virtual eAdminField CreatePositionField()
        {
            eAdminField field = new eAdminTextboxField(_tab, _labelSpecifPosition, _updateCat, eLibConst.SPECIFS.DISPORDER.GetHashCode(), EudoQuery.AdminFieldType.ADM_TYPE_NUM, value: _specif.DispOrder.ToString());
            field.Generate(_panelParametersContent);
            AddAttributeSpecifID(field);
            field.SetControlAttribute("erngmin", "1");
            field.SetControlAttribute("erngmax", _listSpecifs.Count.ToString());
            return field;
        }

        /// <summary>
        /// Creates the URL field.
        /// </summary>
        /// <returns></returns>
        protected virtual eAdminField CreateUrlField()
        {
            eAdminField field = new eAdminTextboxField(_tab, _labelURL, _updateCat, eLibConst.SPECIFS.URL.GetHashCode(), EudoQuery.AdminFieldType.ADM_TYPE_CHAR, _tooltipURL, value: _specif.Url.ToString());
            field.Generate(_panelParametersContent);
            AddAttributeSpecifID(field);
            return field;
        }

        /// <summary>
        /// Creates the URL parameter field.
        /// </summary>
        /// <returns></returns>
        protected virtual eAdminField CreateUrlParamField()
        {
            eAdminField field = new eAdminTextboxField(_tab, 
                _labelURLParam,
                _updateCat, 
                eLibConst.SPECIFS.URLPARAM.GetHashCode(), EudoQuery.AdminFieldType.ADM_TYPE_CHAR,
                value: _specif.UrlParam.ToString(),
                optional:true,
                tooltiptext: eResApp.GetRes(Pref, 8053).Replace(@"\n", Environment.NewLine));


        

            field.Generate(_panelParametersContent);

            AddAttributeSpecifID(field);
            return field;
        }

        /// <summary>
        /// génère l'option du mode d'ouverture de la spécif
        /// </summary>
        /// <returns></returns>
        protected virtual eAdminField CreateOpenModeField()
        {
            if (_specif != null)
            {
                List<ListItem> listItems = new List<ListItem>();

                if (_openModes.Contains(eLibConst.SPECIF_OPENMODE.MODAL))
                    listItems.Add(new ListItem(eResApp.GetRes(Pref, 7823), eLibConst.SPECIF_OPENMODE.MODAL.GetHashCode().ToString()));

                if (_openModes.Contains(eLibConst.SPECIF_OPENMODE.HIDDEN))
                    listItems.Add(new ListItem(eResApp.GetRes(Pref, 7826), eLibConst.SPECIF_OPENMODE.HIDDEN.GetHashCode().ToString()));

                if (_openModes.Contains(eLibConst.SPECIF_OPENMODE.IFRAME))
                    listItems.Add(new ListItem(eResApp.GetRes(Pref, 7827), eLibConst.SPECIF_OPENMODE.IFRAME.GetHashCode().ToString()));

                if (_openModes.Contains(eLibConst.SPECIF_OPENMODE.NEW_WINDOW))
                    listItems.Add(new ListItem(eResApp.GetRes(Pref, 7824), eLibConst.SPECIF_OPENMODE.NEW_WINDOW.GetHashCode().ToString()));

                eAdminField field = new eAdminDropdownField(_tab, _labelOpenMode, _updateCat, eLibConst.SPECIFS.OPENMODE.GetHashCode(), listItems.ToArray(),
                    value: _specif.OpenMode.GetHashCode().ToString(), renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE);
                field.Generate(_panelParametersContent);
                AddAttributeSpecifID(field);
                return field;
            }

            return null;
        }

        /// <summary>
        /// Creates the admin URL field.
        /// </summary>
        /// <returns></returns>
        protected virtual eAdminField CreateAdminUrlField()
        {
            eAdminIconField icon = new eAdminIconField("urladminbtn", "icon-cloud-upload", "adminFieldParamBtn",
                string.Concat("nsAdmin.openCloudLink(this,", _specif.Type == eLibConst.SPECIF_TYPE.TYP_WEBTAB_INTERNAL ? "1" : "0", ")")
                );

            eAdminField field = new eAdminTextboxField(_tab, _labelAdminURL, _updateCat, eLibConst.SPECIFS.ADMINURL.GetHashCode(), EudoQuery.AdminFieldType.ADM_TYPE_CHAR,
                value: _specif.UrlAdmin, icon: icon);
            field.Generate(_panelParametersContent);
            AddAttributeSpecifID(field);
            return field;
        }

    }
}