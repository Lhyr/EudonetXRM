using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using static Com.Eudonet.Xrm.eConst;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Rendu des paramètres spécifiques d'un widget tuile
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eWidgetSpecificParamRenderer" />
    public class eTileParamRenderer : eWidgetSpecificParamRenderer
    {
        eSpecif _specif = null;
        eAdminTableInfos _tabInfos = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="eTileParamRenderer"/> class.
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        /// <param name="file">The file.</param>
        /// <param name="param">The parameter.</param>
        public eTileParamRenderer(ePref pref, bool isVisible, eFile file, eXrmWidgetParam param) : base(pref, isVisible, file, param)
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
                int specifID = _widgetParams.GetParamValueInt("specifid");
                if (specifID > 0)
                {
                    _specif = eSpecif.GetSpecif(Pref, specifID);
                }

                _tab = _widgetParams.GetParamValueInt("tab");
                if (_tab > 0)
                {
                    _tabInfos = eAdminTableInfos.GetAdminTableInfos(_ePref, _tab);
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

            //Type de visualisation
            _pgContainer.Controls.Add(
                BuildYesNoOptionField(eResApp.GetRes(Pref, 8269), _contentParamField, "oAdminGridMenu.onVisuTypeChange(this, event);",
                eResApp.GetRes(Pref, 6819) /*Pictogramme*/,
                eResApp.GetRes(Pref, 1216) /*Image*/, null, null, bCarriageReturn: true, paramName: "visuType", value: _widgetParams.GetParamValueInt("visutype").ToString(), sYesValue: "0", sNoValue: "1")
            );

            //Champ Image
            _pgContainer.Controls.Add(BuildImageArea(_file.GetField((int)XrmWidgetField.ContentSource), "tilePictureParamArea", _widgetParams.GetParamValueInt("visutype") == 1));

            _pgContainer.Controls.Add(BuildHiddenField(_contentParamField, "width", _widgetParams.GetParamValue("width")));
            _pgContainer.Controls.Add(BuildHiddenField(_contentParamField, "height", _widgetParams.GetParamValue("height")));

            //Couleur de fond
            _pgContainer.Controls.Add(BuildColorPicker(_contentParamField, eResApp.GetRes(Pref, 1505), String.Empty, paramName: "backgroundColor", value: _widgetParams.GetParamValue("backgroundcolor"), onblur: "oAdminGridMenu.updateParam(this);"));

            //Couleur du titre
            _pgContainer.Controls.Add(BuildColorPicker(_contentParamField, eResApp.GetRes(Pref, 8270), String.Empty, paramName: "titleColor", value: _widgetParams.GetParamValue("titleColor"), onblur: "oAdminGridMenu.updateParam(this);"));

            //Couleur du sous-titre
            _pgContainer.Controls.Add(BuildColorPicker(_contentParamField, eResApp.GetRes(Pref, 8271), String.Empty, paramName: "subtitleColor", value: _widgetParams.GetParamValue("subtitleColor"), onblur: "oAdminGridMenu.updateParam(this);"));


            //Type d'action
            int nTileAction = _widgetParams.GetParamValueInt("tileAction");
            XrmWidgetTileAction tileAction = (XrmWidgetTileAction)nTileAction;
            List<Tuple<string, string>> listItems = new List<Tuple<string, string>>();
            listItems.Add(new Tuple<string, string>(((int)XrmWidgetTileAction.OpenWebpage).ToString(), eResApp.GetRes(Pref, 8272)));
            listItems.Add(new Tuple<string, string>(((int)XrmWidgetTileAction.OpenSpecif).ToString(), eResApp.GetRes(Pref, 8273)));
            listItems.Add(new Tuple<string, string>(((int)XrmWidgetTileAction.OpenTab).ToString(), eResApp.GetRes(Pref, 8274)));
            listItems.Add(new Tuple<string, string>(((int)XrmWidgetTileAction.CreateFile).ToString(), eResApp.GetRes(Pref, 76)));
            listItems.Add(new Tuple<string, string>(((int)XrmWidgetTileAction.GoToFile).ToString(), eResApp.GetRes(Pref, 8746)));

            _pgContainer.Controls.Add(
                BuildSelectOptionField(eResApp.GetRes(Pref, 8275), "", _contentParamField, listItems, "oAdminGridMenu.onTypeActionChange(this);", paramName: "tileAction", value: _widgetParams.GetParamValue("tileaction"))
            );

            BuildWebpageParams(_pgContainer,
                tileAction == XrmWidgetTileAction.OpenWebpage ? eResApp.GetRes(Pref, 8276) : eResApp.GetRes(Pref, 8277),
                tileAction == XrmWidgetTileAction.OpenWebpage
                );

            BuildSpecifParams(_pgContainer, tileAction == XrmWidgetTileAction.OpenSpecif);

            BuildTabParams(_pgContainer, tileAction == XrmWidgetTileAction.OpenTab || tileAction == XrmWidgetTileAction.CreateFile || tileAction == XrmWidgetTileAction.GoToFile, tileAction);

            // Si Action = "Nouvelle fiche" et onglet principal sélectionné, alors on doit proposer le comportement souhaité à la validation
            if (tileAction == XrmWidgetTileAction.CreateFile
                && _tabInfos != null
                && _tabInfos.EdnType == EdnType.FILE_MAIN)
            {
                BuildFileValidationBehaviour(_pgContainer);
            }

            // Si Action = "Ouverture d'une fiche"
            if (tileAction == XrmWidgetTileAction.GoToFile
                && _tab > 0)
            {
                BuildFileSelection();

                // Si onglet principal sélectionné, alors on propose les options d'ouverture de la fiche (popup ou non)
                if (_tabInfos != null && _tabInfos.EdnType == EdnType.FILE_MAIN)
                    BuildGoToFileOptions();
            }


        }

        private void BuildGoToFileOptions()
        {
            string value = XrmWidgetTileFileOpenMode.Default.GetHashCode().ToString();

            if (!String.IsNullOrEmpty(_widgetParams.GetParamValue("fileOpenMode")))
                value = _widgetParams.GetParamValue("fileOpenMode");

            List<Tuple<string, string>> listItems = new List<Tuple<string, string>>();
            listItems.Add(new Tuple<string, string>(((int)XrmWidgetTileFileOpenMode.Popup).ToString(), eResApp.GetRes(Pref, 8748)));
            listItems.Add(new Tuple<string, string>(((int)XrmWidgetTileFileOpenMode.Default).ToString(), eResApp.GetRes(Pref, 8749)));

            _pgContainer.Controls.Add(BuildSelectOptionField(eResApp.GetRes(_ePref, 7822), "", _contentParamField, listItems, "oAdminGridMenu.updateParam(this, true);",
                paramName: "fileOpenMode",
                value: value,
                disableNoValue: true));
        }

        /// <summary>
        /// Création d'un champ permettant de sélectionner une fiche
        /// </summary>
        private void BuildFileSelection()
        {
            string displayValue = string.Empty;

            int fileid = _widgetParams.GetParamValueInt("fileid");
            if (fileid > 0)
            {
                Dictionary<int, eFieldRecord> dic = eDataFillerGeneric.GetFieldsValue(_ePref, new HashSet<int>() { (_tab + 1) }, _tab, fileid);
                if (dic.Count > 0 && dic.ContainsKey(_tab + 1))
                {
                    displayValue = dic[_tab + 1]?.DisplayValue;
                }
            }
            _pgContainer.Controls.Add(
                BuildWidgetParamFileSelect(_widgetParams.WidgetId, eResApp.GetRes(_ePref, 190), "fileid", fileid.ToString(), displayValue, _tab)
            );


        }

        /// <summary>
        /// Création des paramètres pour la page web à ouvrir
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="strLabelUrl">The string label URL.</param>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        private void BuildWebpageParams(Control container, string strLabelUrl, bool isVisible)
        {
            container.Controls.Add(BuildInputField(strLabelUrl, _contentParamField, paramName: "url", value: _widgetParams.GetParamValue("url"), onchange: "oAdminGridMenu.updateParam(this);", containerId: "tileUrlContainer", isVisible: isVisible));
        }

        /// <summary>
        /// Création des paramètres pour la page web à ouvrir
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        private void BuildSpecifParams(Control container, bool isVisible)
        {
            int specifId = 0;
            string specifUrl = String.Empty;
            string specifUrlParam = String.Empty;
            string specifUrlAdmin = String.Empty;
            int specifTab = 0;
            eLibConst.SPECIF_OPENMODE specifOpenMode = eLibConst.SPECIF_OPENMODE.MODAL;

            if (_specif != null)
            {
                specifId = _specif.SpecifId;
                specifUrl = _specif.Url;
                specifUrlParam = _specif.UrlParam;
                specifUrlAdmin = _specif.UrlAdmin;
                specifOpenMode = _specif.OpenMode;
                specifTab = _specif.Tab;
            }

            HtmlGenericControl inputID = (HtmlGenericControl)(BuildInputField("ID", _contentParamField, bDisabled: true, paramName: "specifid", value: specifId.ToString(), containerId: "tileSpecifIdContainer", isVisible: false));
            inputID.Style.Add("display", "none");
            container.Controls.Add(inputID);

            container.Controls.Add(BuildInputField(eResApp.GetRes(Pref, 8277), _contentParamField, paramName: "specifUrl", value: specifUrl, onchange: "oAdminGridMenu.updateTileSpecifParam(this);", containerId: "tileSpecifUrlContainer", isVisible: isVisible));
            container.Controls.Add(BuildInputField(eResApp.GetRes(Pref, 7600), _contentParamField, paramName: "specifURLParam", value: specifUrlParam, onchange: "oAdminGridMenu.updateTileSpecifParam(this);", containerId: "tileSpecifUrlParamContainer", isVisible: isVisible));

            HtmlGenericControl icon = new HtmlGenericControl();
            icon.Attributes.Add("class", "icon-cloud-upload adminFieldParamBtn");
            icon.Attributes.Add("onclick", "nsAdmin.openCloudLink(this, 1)");
            icon.Attributes.Add("ebtnparam", "1");
            Control field = BuildInputField(eResApp.GetRes(Pref, 8278), _contentParamField, paramName: "specifURLAdmin", value: specifUrlAdmin, onchange: "oAdminGridMenu.updateTileSpecifParam(this);", containerId: "tileSpecifUrlAdmContainer", isVisible: isVisible, icon: icon);
            if (field.Controls.Count >= 2)
            {
                ((HtmlGenericControl)field.Controls[1]).Attributes.Add("did", specifTab.ToString());
                ((HtmlGenericControl)field.Controls[1]).Attributes.Add("fid", specifId.ToString());
            }
            container.Controls.Add(field);

            //Type d'action
            List<Tuple<string, string>> listItems = new List<Tuple<string, string>>();
            listItems.Add(new Tuple<string, string>(((int)eLibConst.SPECIF_OPENMODE.MODAL).ToString(), eResApp.GetRes(Pref, 7823)));
            listItems.Add(new Tuple<string, string>(((int)eLibConst.SPECIF_OPENMODE.HIDDEN).ToString(), eResApp.GetRes(Pref, 7826)));
            //listItems.Add(new Tuple<string, string>(((int)eLibConst.SPECIF_OPENMODE.IFRAME).ToString(), eResApp.GetRes(Pref, 7827))); //Mode Iframe non disponible pour une tuile
            listItems.Add(new Tuple<string, string>(((int)eLibConst.SPECIF_OPENMODE.NEW_WINDOW).ToString(), eResApp.GetRes(Pref, 7824)));

            container.Controls.Add(
                BuildSelectOptionField(eResApp.GetRes(Pref, 7822), "", _file.GetField((int)XrmWidgetField.ContentParam), listItems, "oAdminGridMenu.updateTileSpecifParam(this, event);", paramName: "specifOpenMode", value: ((int)specifOpenMode).ToString(), containerId: "tileSpecifOpenModeContainer", isVisible: isVisible, disableNoValue: true)
            );
        }

        /// <summary>
        /// Création des paramètres pour l'ouverture de l'onglet
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        /// <param name="action">Action du widget tuile</param>
        private void BuildTabParams(Control container, bool isVisible, XrmWidgetTileAction action)
        {
            string onChange = "oAdminGridMenu.updateParam(this, true);";
            // Onglet
            List<Tuple<string, string>> listTabs = eAdminTools.GetListTabs(Pref).Select(x => new Tuple<string, string>(x.Item1.ToString(), x.Item2)).ToList();
            container.Controls.Add(BuildSelectOptionField(eResApp.GetRes(Pref, 264), "", _file.GetField((int)XrmWidgetField.ContentParam), listTabs, onChange, "tab", _tab.ToString(), containerId: "tileTabContainer", isVisible: isVisible));

            if (action == XrmWidgetTileAction.OpenTab)
            {
                // Bouton "Filtre à l'ouverture"
                String btnLabel = eResApp.GetRes(Pref, 8266) + ((_widgetParams.GetParamValueInt("filterid") > 0) ? " (1)" : "");
                container.Controls.Add(BuildBtnField(btnLabel, "", _file.GetField((int)XrmWidgetField.ContentParam), "oAdminGridMenu.showFilterEditor(this)", "filterid", _widgetParams.GetParamValue("filterid"), containerId: "tileFilterIdContainer", isVisible: isVisible));
            }

        }

        /// <summary>
        /// Construit le paramètre "Comportement à la validation" pour l'action "Nouvelle fiche"
        /// </summary>
        /// <param name="container">The container.</param>
        private void BuildFileValidationBehaviour(Control container)
        {
            // Comportement à la validation
            List<Tuple<string, string>> listItems = new List<Tuple<string, string>>();
            listItems.Add(new Tuple<string, string>(((int)XrmWidgetTileCreateFileValidationBehaviour.OpenFile).ToString(), eResApp.GetRes(Pref, 8749)));
            listItems.Add(new Tuple<string, string>(((int)XrmWidgetTileCreateFileValidationBehaviour.StayOnGrid).ToString(), eResApp.GetRes(Pref, 8745)));

            container.Controls.Add(
                BuildSelectOptionField(eResApp.GetRes(Pref, 7822), "", _file.GetField((int)XrmWidgetField.ContentParam), listItems, "oAdminGridMenu.updateParam(this, true);",
                paramName: "fileValidationMode", value: _widgetParams.GetParamValue("fileValidationMode") ?? "0", disableNoValue: true)
            );
        }

    }
}