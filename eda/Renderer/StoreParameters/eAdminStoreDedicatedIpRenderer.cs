using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer du module d'administration de IP Dédiée
    /// </summary>
    public class eAdminStoreDedicatedIpRenderer : eAdminStoreFileRenderer
    {
        eDedicatedIpSetting _dedicatedIpSetting = new eDedicatedIpSetting();

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminStoreDedicatedIpRenderer(ePref pref, eAdminExtension extension) : base(pref, extension)
        {

        }

        /// <summary>
        /// Création du rendu mode Fiche de l'extension IP Dédiée
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static eAdminStoreDedicatedIpRenderer CreateAdminStoreDedicatedIpRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminStoreDedicatedIpRenderer rdr = new eAdminStoreDedicatedIpRenderer(pref, ext);

            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                if (string.IsNullOrEmpty(_eRegisteredExt?.Param))
                    _dedicatedIpSetting = eDedicatedIpSetting.GetEmptyMapping();
                else
                    _dedicatedIpSetting = eDedicatedIpSetting.FromJsonString(_eRegisteredExt.Param);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Build des params
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (base.Build())
            {
                AddCallBackScript("nsAdmin.addScript('eDedicatedIpAdmin', 'ADMIN_STORE');", true);
                return true;
            }
            return false;
        }


        /// <summary>
        /// écran de paramétrage
        /// </summary>
        /// <returns></returns>
        protected override void CreateSettingsPanel()
        {
            base.CreateSettingsPanel();

            //Informations
            Tuple<string, string, eAdminField> infos = new Tuple<string, string, eAdminField>(eResApp.GetRes(_ePref, 2808), _dedicatedIpSetting.DedicatedIp ?? String.Empty, null); //IP Dédiée
            ExtensionParametersContainer.Controls.Add(GetInfosSectionDedicatedIp("dedicatedIpInfos", eResApp.GetRes(_ePref, 5080), "", infos)); //Informations
        }


        /// <summary>
        /// Retourne un bloc d'information
        /// </summary>
        /// <param name="id">id js</param>
        /// <param name="sectionLabel"></param>
        /// <param name="sectionTabLabel"></param>
        /// <param name="infos"></param>
        /// <returns></returns>
        protected Panel GetInfosSectionDedicatedIp(string id, string sectionLabel, string sectionTabLabel, Tuple<string, string, eAdminField> infos)
        {
            Panel sectionPanel = GetModuleSectionDedicatedIp(String.Concat("section_", id), sectionLabel);
            Panel sectionPanelContainer = (Panel)sectionPanel.Controls[sectionPanel.Controls.Count - 1];

            if (infos != null)
            {
                Panel field = new Panel();
                field.CssClass = "field fieldinfosection";
                field.ID = String.Concat(id, "_fieldinfosection_", 1);
                HtmlGenericControl fieldLabel = new HtmlGenericControl("label");
                fieldLabel.InnerText = infos.Item1;
                field.Controls.Add(fieldLabel);

                if (String.Equals(id, "dedicatedIpInfos"))
                {
                    HtmlInputText fieldValue = new HtmlInputText();
                    fieldValue.Value = infos.Item2;
                    fieldValue.Attributes.Add("title", eResApp.GetRes(_ePref, 2314)); //Double-cliquez pour copier la valeur dans le presse papier
                    fieldValue.Attributes.Add("ondblclick", "nsAdminField.CopyValueToClipBoard(event)");
                    fieldValue.Attributes.Add("onChange", "nsDedicatedIpAdmin.updateParam(event)");
                    if (_ePref.User.UserLevel < 100)
                        fieldValue.Attributes.Add("readonly", "1");
                    if (_ePref.User.UserLevel >= 100)
                        fieldValue.Style.Add("background-color", "white");
                    fieldValue.Size = 60;

                    field.Controls.Add(fieldValue);
                }
                else
                {
                    fieldLabel.Style.Add("width", "100%");
                    fieldLabel.Style.Add("text-align", "center");
                }

                if (infos.Item3 != null)
                    infos.Item3.Generate(field);

                sectionPanelContainer.Controls.Add(field);
            }
            return sectionPanel;
        }


        /// <summary>
        /// Crée un conteneur de section pour module d'administration, repliable, avec un ID, un titre et une ancre correspondant à l'ID.
        /// </summary>
        /// <param name="id">Identifiant du conteneur. Sera également utilisé comme cible pour l'ancre générée près du titre pour être atteint via <a href="#id"/></param>
        /// <param name="label">Libellé de la section à afficher comme titre</param>
        /// <param name="number">Si souhaité, ajoute un numéro à la section. Si la valeur est inférieure à 1, le numéro est ignoré</param>
        /// <param name="bCollapsable">Indique si l'entrée de module doit pouvoir être replié</param>
        /// <returns>Renvoie le contrôle généré, de façon à pouvoir y ajouter des contrôles enfants via Controls.Add</returns>
        protected Panel GetModuleSectionDedicatedIp(string id, string label, int number = 0, bool bCollapsable = true)
        {
            Panel sectionContainer = new Panel();
            HyperLink sectionAnchor = new HyperLink();
            Panel sectionTitleContainer = new Panel();
            HtmlGenericControl sectionTitle = new HtmlGenericControl();
            HtmlGenericControl sectionIcon = new HtmlGenericControl();
            Panel sectionContentsContainer = new Panel();

            sectionContainer.Attributes.Add("class", "divStep");

            sectionAnchor.Attributes.Add("name", id);

            sectionTitleContainer.ID = id;
            sectionTitleContainer.Attributes.Add("class", "paramStep active" + (bCollapsable ? "" : " paramStepDefPointer"));

            sectionIcon.Attributes.Add("class", "icon-unvelop");

            sectionTitle.Attributes.Add("class", "stepTitle");
            sectionTitle.InnerText = label;

            sectionContentsContainer.Attributes.Add("class", "stepContent");
            sectionContentsContainer.ID = "stepContent_" + id;
            sectionContentsContainer.Attributes.Add("data-active", "1");

            if (number > 0)
            {
                HtmlGenericControl sectionNum = new HtmlGenericControl("span");
                sectionTitleContainer.Controls.Add(sectionNum);
            }

            if (bCollapsable)
                sectionTitleContainer.Controls.Add(sectionIcon);

            sectionTitleContainer.Controls.Add(sectionTitle);

            sectionContainer.Controls.Add(sectionAnchor);
            sectionContainer.Controls.Add(sectionTitleContainer);
            if (String.Equals(id, "section_dedicatedIpInfos") && _ePref.User.UserLevel >= 100)
            {
                Panel sectionParamWarningContainer = new Panel();
                sectionParamWarningContainer.Attributes.Add("class", "mandatoryParamWarning");
                HtmlGenericControl sectionTitleParamWarning = new HtmlGenericControl();
                sectionTitleParamWarning.Attributes.Add("class", "mandatoryParamTitle");
                sectionTitleParamWarning.InnerText = eResApp.GetRes(_ePref, 2809);
                sectionParamWarningContainer.Controls.Add(sectionTitleParamWarning);
                sectionContentsContainer.Controls.Add(sectionParamWarningContainer);
            }

            sectionContainer.Controls.Add(sectionContentsContainer);

            return sectionContainer;
        }
    }
}