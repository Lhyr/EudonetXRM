using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Renderer d'options utilisateur - Choix de la langue
    /// </summary>
    public class eUserOptionsAdvExportRenderer : eUserOptionsRenderer
    {
        static string IMG_EXPORT_PATH = @"xadminst/img";

        Panel _pnlContents;
        HtmlGenericControl _pnlSubTitle;

        OfficeRelease _currentOffice = OfficeRelease.OFFICE_97;
        eConst.ExportMode _currentExportMode = eConst.ExportMode.STANDARD;

        Dictionary<Int32, String> _officeVersions;
        Dictionary<Int32, String> _exportModes;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eUserOptionsAdvExportRenderer(ePref pref)
            : base(pref, eUserOptionsModules.USROPT_MODULE.ADVANCED_EXPORT)
        {



        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {

            if (!Enum.TryParse(Pref.GetConfig(eLibConst.PREF_CONFIG.OFFICERELEASE), out _currentOffice))
                _currentOffice = OfficeRelease.OFFICE_2000;

            if (!Enum.TryParse(Pref.GetConfig(eLibConst.PREF_CONFIG.EXPORTMODE), out _currentExportMode))
                _currentExportMode = eConst.ExportMode.STANDARD;

            _officeVersions = new Dictionary<int, string>();

          //  _officeVersions.Add(OfficeRelease.OFFICE_97.GetHashCode(), eResApp.GetRes(Pref, 539));
          //  _officeVersions.Add(OfficeRelease.OFFICE_2000.GetHashCode(), eResApp.GetRes(Pref, 540));
            _officeVersions.Add(OfficeRelease.OFFICE_2007.GetHashCode(), eResApp.GetRes(Pref, 6069));
            _officeVersions.Add(OfficeRelease.OFFICE_2010_64.GetHashCode(), eResApp.GetRes(Pref, 6157));
            _officeVersions.Add(OfficeRelease.OFFICE_2013_32.GetHashCode(), eResApp.GetRes(Pref, 6357));
            _officeVersions.Add(OfficeRelease.OFFICE_2013_64.GetHashCode(), eResApp.GetRes(Pref, 6356));
            _officeVersions.Add(OfficeRelease.OFFICE_2016_32.GetHashCode(), eResApp.GetRes(Pref, 6803));
            _officeVersions.Add(OfficeRelease.OFFICE_2016_64.GetHashCode(), eResApp.GetRes(Pref, 6802));
            _officeVersions.Add(OfficeRelease.OFFICE_MAC_2011.GetHashCode(), eResApp.GetRes(Pref, 6156));
            _officeVersions.Add(OfficeRelease.OFFICE_OPEN_OFFICE.GetHashCode(), eResApp.GetRes(Pref, 6066));

            _officeVersions.Add(OfficeRelease.OFFICE_2019_32.GetHashCode(), eResApp.GetRes(Pref, 2947));
            _officeVersions.Add(OfficeRelease.OFFICE_2019_64.GetHashCode(), eResApp.GetRes(Pref, 2946));

            _officeVersions.Add(OfficeRelease.OFFICE_LIBRE_OFFICE_5_7.GetHashCode(), eResApp.GetRes(Pref, 2948));

            /*
             * 
             * Microsoft Office 2019 - 64 bits (Repartir de office 2016)
                Microsoft Office 2019 - 32 bits (Repartir de office 2016)
                LibreOffice 5, 6 et 7 (cf image en annexes)
                OpenOffice 3 et 4 (cf image en annexes)
             * */

            _exportModes = new Dictionary<int, string>();
            _exportModes.Add(eConst.ExportMode.STANDARD.GetHashCode(), eResApp.GetRes(Pref, 6256));
            _exportModes.Add(eConst.ExportMode.MAIL_ONLY.GetHashCode(), eResApp.GetRes(Pref, 6257));
            _exportModes.Add(eConst.ExportMode.EXPORT_CHOICE.GetHashCode(), eResApp.GetRes(Pref, 6258));

            return base.Init();
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {

            //Version d'office
            BuildTitle(eResApp.GetRes(Pref, 538));

            InitInnerContainer();

            BuildSubTitle(eResApp.GetRes(Pref, 6786));

            BuildOfficeVersionsSelection();

            //Mode export
            BuildTitle(eResApp.GetRes(Pref, 6255));

            InitInnerContainer();

            BuildSubTitle(eResApp.GetRes(Pref, 6787));

            BuildExportModeSelection();

            BuildBtns("setExportProp();", String.Concat("loadUserOption('", eUserOptionsModules.USROPT_MODULE.ADVANCED.ToString(), "');"));

            BuildHiddenDiv(eConst.eFileType.FILE_MODIF);

            return true;
        }

        /// <summary>
        /// On instancie le container 
        /// </summary>
        private void InitInnerContainer()
        {
            _pnlContents = new Panel();
            _pnlContents.CssClass = "adminCntnt";
            _pgContainer.Controls.Add(_pnlContents);
        }

        /// <summary>
        /// Construit un sous titre
        /// </summary>
        /// <param name="subTitle"></param>
        private void BuildSubTitle(String subTitle)
        {
            _pnlSubTitle = new HtmlGenericControl("div");
            _pnlSubTitle.Attributes.Add("class", "adminCntntTtl ");
            _pnlSubTitle.InnerText = subTitle;
            _pnlContents.Controls.Add(_pnlSubTitle);
        }

        /// <summary>
        /// Construit les blocks de chaque version d'office dispo
        /// </summary>
        private void BuildOfficeVersionsSelection()
        {
            Panel versionChoice = new Panel();
            versionChoice.ID = "version-choice";
            versionChoice.Attributes.Add("curr-office", _currentOffice.GetHashCode().ToString());
            versionChoice.Attributes.Add("class", "versionChoice");

            //#65612 - Classer les versions par ordre décroissant
            versionChoice.Controls.Add(BuildOfficeVersion(OfficeRelease.OFFICE_2019_64, "office2013.png"));
            versionChoice.Controls.Add(BuildOfficeVersion(OfficeRelease.OFFICE_2019_32, "office2013.png"));
            versionChoice.Controls.Add(BuildOfficeVersion(OfficeRelease.OFFICE_2016_64, "office2013.png"));
            versionChoice.Controls.Add(BuildOfficeVersion(OfficeRelease.OFFICE_2016_32, "office2013.png"));
            versionChoice.Controls.Add(BuildOfficeVersion(OfficeRelease.OFFICE_2013_64, "office2013.png"));
            versionChoice.Controls.Add(BuildOfficeVersion(OfficeRelease.OFFICE_2013_32, "office2013.png"));            
            versionChoice.Controls.Add(BuildOfficeVersion(OfficeRelease.OFFICE_2010_64, "office2010.png"));
            versionChoice.Controls.Add(BuildOfficeVersion(OfficeRelease.OFFICE_2007, "office2010.png"));
        //    versionChoice.Controls.Add(BuildOfficeVersion(OfficeRelease.OFFICE_2000, "windowsXp.png"));
        //    versionChoice.Controls.Add(BuildOfficeVersion(OfficeRelease.OFFICE_97, "office97.png"));

            versionChoice.Controls.Add(BuildOfficeVersion(OfficeRelease.OFFICE_MAC_2011, "officeMAC.png"));


            versionChoice.Controls.Add(BuildOfficeVersion(OfficeRelease.OFFICE_LIBRE_OFFICE_5_7, "LibreOffice.png"));
            versionChoice.Controls.Add(BuildOfficeVersion(OfficeRelease.OFFICE_OPEN_OFFICE, "OpenOffice.png"));

            _pnlContents.Controls.Add(versionChoice);
        }

        /// <summary>
        /// Construit un block de choix de version d'office
        /// </summary>
        /// <param name="officeRelease">La version a construire</param>
        /// <param name="imageName"></param>
        /// <returns>un panel</returns>
        private Control BuildOfficeVersion(OfficeRelease officeRelease, string imageName)
        {
            Panel versionChoiceElm = new Panel();
            versionChoiceElm.ID = "version-" + officeRelease.GetHashCode().ToString();
            versionChoiceElm.Attributes.Add("class", "versionChoiceElmnt" + (officeRelease == _currentOffice ? " actived" : String.Empty));
            versionChoiceElm.Attributes.Add("onclick", "setOfficeVer(this, '" + officeRelease.GetHashCode().ToString() + "');");

            Panel versionChoiceElmCtn = new Panel();
            versionChoiceElmCtn.Attributes.Add("class", "versionChoiceElemntCtnt");
            versionChoiceElm.Controls.Add(versionChoiceElmCtn);

            HtmlImage img = new HtmlImage();
            img.Src = IMG_EXPORT_PATH + "/" + imageName;
            versionChoiceElmCtn.Controls.Add(img);

            HtmlGenericControl versionTitleCtn = new HtmlGenericControl("div");
            versionTitleCtn.Attributes.Add("class", "versionTitle");
            versionChoiceElm.Controls.Add(versionTitleCtn);

            HtmlGenericControl versionTitle = new HtmlGenericControl("p");
            versionTitle.InnerHtml = _officeVersions.ContainsKey(officeRelease.GetHashCode()) ?
                _officeVersions[officeRelease.GetHashCode()] : eResApp.GetRes(Pref, 538) + " : " + eResApp.GetRes(Pref, 108);
            versionTitleCtn.Controls.Add(versionTitle);

            return versionChoiceElm;
        }

        /// <summary>
        /// Le choix de mode d'export
        /// </summary>
        private void BuildExportModeSelection()
        {
            HtmlSelect select = new HtmlSelect();
            select.ID = "export-mode";

            select.DataSource = _exportModes;
            select.DataTextField = "Value";
            select.DataValueField = "Key";
            select.DataBind();

            ListItem item = select.Items.FindByValue(_currentExportMode.GetHashCode().ToString());
            if (item != null)
                item.Selected = true;

            _pnlSubTitle.Controls.Add(select);
        }
    }
}