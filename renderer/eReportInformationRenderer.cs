using Com.Eudonet.Internal;
using EudoProcessInterfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /******************************************************
    'DEVELOPPEUR  : Maxime Abbey
    'DATE	:  10/04/2018
    'VERSION	:  XRM 10.403.000
    'DESCRIPTION  : Classe de rendu de la fenêtre d'informations détaillées sur certains rapports de publipostage (Power BI notamment)
    'DEMANDE : #64 282
    '******************************************************/

    /// <summary>
    /// Classe de rendu de la fenêtre d'informations détaillées sur certains rapports de publipostage (Power BI notamment)
    /// </summary>
    public class eReportInformationRenderer : eRenderer
    {
        private eReport _report;
        private ePowerBIReport _powerBIReport;
        private int _reportId;
        private bool _forReportWizard = true;

        /// <summary>
        /// Constructeur
        /// </summary>
        public eReportInformationRenderer(ePref pref, int reportId, bool forReportWizard)
        {
            Pref = pref;
            _reportId = reportId;
            _forReportWizard = forReportWizard;
        }

        /// <summary>
        /// Retourne un renderer contenant les informations
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="reportId">Identifiant du rapport pour lequel renvoyer les informations</param>
        /// <param name="forReportWizard">Indique si le rendu des informations doit se faire en lecture seule (pour la fenêtre détachée d'information au double-clic sur le nom du rapport sur la liste) ou en écriture (pour l'assistant Reporting)</param>
        /// <returns></returns>
        public static eReportInformationRenderer GetReportInformationRenderer(ePref pref, int reportId, bool forReportWizard)
        {
            eReportInformationRenderer er = new eReportInformationRenderer(pref, reportId, forReportWizard);
            if (er.Generate())
                return er;
            else
            {
                if (er.InnerException != null)
                    throw er.InnerException;
                else
                    throw new Exception(er._sErrorMsg);
            }

        }

        /// <summary>
        /// Charge le rapport correspondant au reportId passé en paramètre
        /// </summary>
        /// <returns>true si chargé correctement, false sinon</returns>
        protected override Boolean Init()
        {
            _report = new eReport(Pref, _reportId);
            if (_report == null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Effectue le rendu HTML
        /// </summary>
        /// <returns>true si effectué, false sinon</returns>
        protected override Boolean Build()
        {
            HtmlGenericControl ul = new HtmlGenericControl("ul");
            HtmlGenericControl li = new HtmlGenericControl("li");

            if (_report != null)
            {
                // #63 665 - Power BI
                try
                {

                    _powerBIReport = new ePowerBIReport(Pref, _report.Id);

                    //On ne tente pas de load le rapport s'il n'existe pas
                    if (_report.Id > 0)
                    {
                        if (!_powerBIReport.LoadFromDB())
                        {
                            throw new Exception(String.Concat("eReportWizardRenderer.BuildPowerBIMenu.Load() :", _powerBIReport.ErrorMessage), _powerBIReport.InnerException);
                        }
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    eModelTools.EudoTraceLog(String.Concat(ex.Message, " | ", ex.InnerException), Pref, "PowerBI");
#endif
                }
            }

            if (_forReportWizard)
            {
                // Titre "Source de données pour Power BI" (sur l'assistant Reporting uniquement)
                li = new HtmlGenericControl("li");
                ul.ID = "editor_powerbi";
                li.Attributes.Add("class", "reportnameli");
                li.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 8703))); // Source de données pour Power BI
                ul.Controls.Add(li);
            }

            #region URL de la source de données

            string sPowerBIURL = this._reportId > 0 ? _powerBIReport.GetRewrittenURL() : "";

            li = new HtmlGenericControl("li");
            if (_forReportWizard)
                li.Attributes.Add("class", "reportinputli");
            ul.Controls.Add(li);

            // Libellé affiché en face du champ de saisie sur la fenêtre d'informations uniquement
            // (Sur l'assistant, il est affiché en tant que titre de section ci-dessus)
            if (!_forReportWizard)
            {
                HtmlGenericControl powerBIUrlLabelContainer = new HtmlGenericControl("span");
                HtmlGenericControl powerBIUrlLabel = new HtmlGenericControl("label");
                powerBIUrlLabelContainer.Attributes.Add("class", String.Concat("editor_powerbi_url_label editor_powerbi_label", _forReportWizard ? " editor_powerbi_wizard_label" : " editor_powerbi_reportinformation_label"));
                powerBIUrlLabelContainer.Controls.Add(powerBIUrlLabel);
                powerBIUrlLabel.InnerText = eResApp.GetRes(Pref, 8703); // Source de données pour Power BI
                li.Controls.Add(powerBIUrlLabelContainer);
            }

            HtmlGenericControl powerBIUrlInput = new HtmlGenericControl("input");
            powerBIUrlInput.ID = "editor_powerbi_url_input";
            powerBIUrlInput.Attributes.Add("readonly", "readonly");
            powerBIUrlInput.Attributes.Add("onclick", _forReportWizard ? "oReport.SelectPowerBIUrl(this);" : String.Concat("copyControlTextToClipboard(document.getElementById('", powerBIUrlInput.ID, "'));"));
            powerBIUrlInput.Attributes.Add("class", String.Concat("editor_powerbi_url_input editor_powerbi_control", _forReportWizard ? " editor_powerbi_wizard_control" : " editor_powerbi_reportinformation_control"));
            powerBIUrlInput.Attributes.Add("title", eResApp.GetRes(Pref, 8727)); // Utilisez cette URL comme source de données dans votre assistant Power BI
            powerBIUrlInput.Attributes.Add("value", sPowerBIURL);
            li.Controls.Add(powerBIUrlInput);

            // Copier
            // #64 282, #60 560 : uniquement si une valeur est présente
            if (sPowerBIURL.Length > 0)
            {
                HtmlGenericControl powerBIUrlButton = new HtmlGenericControl();
                powerBIUrlButton.ID = "editor_powerbi_url_button";
                powerBIUrlButton.Attributes.Add("class", "icon-duplicate");
                powerBIUrlButton.Attributes.Add("data-for", powerBIUrlInput.ID);
                powerBIUrlButton.Attributes.Add("onclick", String.Concat("copyControlTextToClipboard(document.getElementById('", powerBIUrlInput.ID, "'));"));
                li.Controls.Add(powerBIUrlButton);
            }

            #endregion

            #region Date de validité

            li = new HtmlGenericControl("li");
            if (_forReportWizard)
                li.Attributes.Add("class", "reportinputli");
            ul.Controls.Add(li);

            HtmlGenericControl powerBIDateLabelContainer = new HtmlGenericControl("span");
            HtmlGenericControl powerBIDateLabel = new HtmlGenericControl("label");
            powerBIDateLabelContainer.Attributes.Add("class", String.Concat("editor_powerbi_date_label editor_powerbi_label", _forReportWizard ? " editor_powerbi_wizard_label" : " editor_powerbi_reportinformation_label"));
            powerBIDateLabelContainer.Controls.Add(powerBIDateLabel);
            powerBIDateLabel.InnerText = eResApp.GetRes(_ePref, 8704);
            li.Controls.Add(powerBIDateLabelContainer);

            string powerBIDateString = String.Empty;
            HtmlGenericControl powerBIDateInput = new HtmlGenericControl("input");
            powerBIDateInput.ID = "editor_powerbi_date_input";
            if (!_forReportWizard)
                powerBIDateInput.Attributes.Add("readonly", "readonly");
            powerBIDateInput.Attributes.Add("onclick", _forReportWizard ? "oReport.SelectPowerBIExpirationDate(this);" : String.Empty);
            powerBIDateInput.Attributes.Add("onchange", _forReportWizard ? "oReport.SetPowerBIExpirationDate(getAttributeValue(this, 'dbv'));" : String.Empty);
            powerBIDateInput.Attributes.Add("class", String.Concat("editor_powerbi_date_input editor_powerbi_control", _forReportWizard ? " editor_powerbi_wizard_control" : " editor_powerbi_reportinformation_control"));
            powerBIDateInput.Attributes.Add("title", eResApp.GetRes(Pref, 8728)); // L’actualisation des données sera inactive après cette date
            if (_powerBIReport.ExpirationDate != DateTime.MinValue)
            {
                powerBIDateString = eDate.ConvertBddToDisplay(Pref.CultureInfo, _powerBIReport.ExpirationDate, false, true, true, true);
                powerBIDateInput.Attributes.Add("value", powerBIDateString);
                powerBIDateInput.Attributes.Add("ednvalue", eDate.ConvertDisplayToBdd(Pref.CultureInfo, powerBIDateString));
            }
            li.Controls.Add(powerBIDateInput);

            if (_forReportWizard)
            {
                HtmlGenericControl powerBIDateButton = new HtmlGenericControl();
                powerBIDateButton.ID = "editor_powerbi_date_button";
                powerBIDateButton.Attributes.Add("class", "icon-agenda");
                powerBIDateButton.Attributes.Add("data-for", powerBIDateInput.ID);
                powerBIDateButton.Attributes.Add("onclick", String.Concat("oReport.SelectPowerBIExpirationDate(document.getElementById('", powerBIDateInput.ID, "'));"));
                li.Controls.Add(powerBIDateButton);
            }

            #endregion

            #region Utilisateur d'exécution

            li = new HtmlGenericControl("li");
            if (_forReportWizard)
                li.Attributes.Add("class", "reportinputli");
            ul.Controls.Add(li);

            HtmlGenericControl powerBIUserLabelContainer = new HtmlGenericControl("span");
            HtmlGenericControl powerBIUserLabel = new HtmlGenericControl("label");
            powerBIUserLabelContainer.Attributes.Add("class", String.Concat("editor_powerbi_user_label editor_powerbi_label", _forReportWizard ? " editor_powerbi_wizard_label" : " editor_powerbi_reportinformation_label"));
            powerBIUserLabelContainer.Controls.Add(powerBIUserLabel);
            powerBIUserLabel.InnerText = eResApp.GetRes(_ePref, 8705);
            li.Controls.Add(powerBIUserLabelContainer);

            HtmlGenericControl powerBIUserInput = new HtmlGenericControl("input");
            powerBIUserInput.ID = "editor_powerbi_user_input";
            powerBIUserInput.Attributes.Add("readonly", "readonly");
            powerBIUserInput.Attributes.Add("onclick", _forReportWizard ? "oReport.SelectPowerBIUser(this);" : String.Empty);
            powerBIUserInput.Attributes.Add("class", String.Concat("editor_powerbi_user_input editor_powerbi_control", _forReportWizard ? " editor_powerbi_wizard_control" : " editor_powerbi_reportinformation_control"));
            IDictionary<String, String> dicoUser = eLibDataTools.GetUserLogin(Pref, _powerBIReport.UserId.ToString());
            string powerBIUserLogin = String.Empty;
            int powerBIUserID = 0;
            if (dicoUser != null && dicoUser.Count > 0)
            {
                powerBIUserID = eLibTools.GetNum(dicoUser.FirstOrDefault().Key);
                powerBIUserLogin = dicoUser.FirstOrDefault().Value;
            }
            if (powerBIUserLogin != String.Empty && powerBIUserID > 0)
            {
                powerBIUserInput.Attributes.Add("value", powerBIUserLogin.ToString());
                powerBIUserInput.Attributes.Add("ednvalue", powerBIUserID.ToString());
            }
            li.Controls.Add(powerBIUserInput);

            if (_forReportWizard)
            {
                HtmlGenericControl powerBIUserButton = new HtmlGenericControl();
                powerBIUserButton.ID = "editor_powerbi_user_button";
                powerBIUserButton.Attributes.Add("class", "icon-catalog");
                powerBIUserButton.Attributes.Add("data-for", powerBIUserInput.ID);
                powerBIUserButton.Attributes.Add("onclick", String.Concat("oReport.SelectPowerBIUser(document.getElementById('", powerBIUserInput.ID, "'));"));
                li.Controls.Add(powerBIUserButton);
            }

            #endregion

            #region Mode d'exécution

            li = new HtmlGenericControl("li");
            if (_forReportWizard)
                li.Attributes.Add("class", "reportinputli");
            ul.Controls.Add(li);

            HtmlGenericControl powerBIExecutionModeLabelContainer = new HtmlGenericControl("span");
            HtmlGenericControl powerBIExecutionModeLabel = new HtmlGenericControl("label");
            powerBIExecutionModeLabelContainer.Attributes.Add("class", String.Concat("editor_powerbi_executionmode_label editor_powerbi_label", _forReportWizard ? " editor_powerbi_wizard_label" : " editor_powerbi_reportinformation_label"));
            powerBIExecutionModeLabelContainer.Controls.Add(powerBIExecutionModeLabel);
            powerBIExecutionModeLabel.InnerText = eResApp.GetRes(_ePref, 8706);
            li.Controls.Add(powerBIExecutionModeLabelContainer);

            HtmlGenericControl powerBIExecutionModeControl = null;
            if (_forReportWizard)
            {
                powerBIExecutionModeControl = new HtmlGenericControl("select");
                powerBIExecutionModeControl.ID = "editor_powerbi_executionmode_select";
                powerBIExecutionModeControl.Attributes.Add("class", "editor_powerbi_executionmode_select editor_powerbi_control editor_powerbi_wizard_control");
                powerBIExecutionModeControl.Attributes.Add("onchange", "oReport.SetPowerBIExecutionMode(this);");
                li.Controls.Add(powerBIExecutionModeControl);
                HtmlGenericControl powerBIExecutionModeSelectOption = new HtmlGenericControl("option");
                powerBIExecutionModeSelectOption.InnerText = eResApp.GetRes(_ePref, 8710); // Manuel (à la demande)
                powerBIExecutionModeSelectOption.Attributes.Add("value", eLibConst.POWERBI_EXECUTIONMODE.MANUAL.GetHashCode().ToString());
                if (_powerBIReport.ExecutionMode == eLibConst.POWERBI_EXECUTIONMODE.MANUAL)
                    powerBIExecutionModeSelectOption.Attributes.Add("selected", "selected");
                powerBIExecutionModeControl.Controls.Add(powerBIExecutionModeSelectOption);
                powerBIExecutionModeSelectOption = new HtmlGenericControl("option");
                powerBIExecutionModeSelectOption.InnerText = eResApp.GetRes(_ePref, 8711); // Echantillon
                powerBIExecutionModeSelectOption.Attributes.Add("value", eLibConst.POWERBI_EXECUTIONMODE.SAMPLE.GetHashCode().ToString());
                if (_powerBIReport.ExecutionMode == eLibConst.POWERBI_EXECUTIONMODE.SAMPLE)
                    powerBIExecutionModeSelectOption.Attributes.Add("selected", "selected");
                powerBIExecutionModeControl.Controls.Add(powerBIExecutionModeSelectOption);
                /* Specs du 08/06/2018 : on ne propose pas l'option Planifié, dont le sens peut prêter à confusion */
                /*
                powerBIExecutionModeSelectOption = new HtmlGenericControl("option");
                powerBIExecutionModeSelectOption.InnerText = eResApp.GetRes(_ePref, 8712); // Planifié
                powerBIExecutionModeSelectOption.Attributes.Add("value", eLibConst.POWERBI_EXECUTIONMODE.SCHEDULED.GetHashCode().ToString());
                if (_powerBIReport.ExecutionMode == eLibConst.POWERBI_EXECUTIONMODE.SCHEDULED)
                    powerBIExecutionModeSelectOption.Attributes.Add("selected", "selected");
                powerBIExecutionModeControl.Controls.Add(powerBIExecutionModeSelectOption);
                */
            }
            else
            {
                powerBIExecutionModeControl = new HtmlGenericControl("input");
                powerBIExecutionModeControl.ID = "editor_powerbi_executionmode_input";
                powerBIExecutionModeControl.Attributes.Add("readonly", "readonly");
                powerBIExecutionModeControl.Attributes.Add("class", "editor_powerbi_executionmode_input editor_powerbi_control editor_powerbi_reportinformation_control");
                powerBIExecutionModeControl.Attributes.Add("title", eResApp.GetRes(Pref, 411)); // Utilisateur - TOCHECK RES
                string powerBIExecutionModeString = String.Empty;
                switch (_powerBIReport.ExecutionMode)
                {
                    case eLibConst.POWERBI_EXECUTIONMODE.MANUAL: powerBIExecutionModeString = eResApp.GetRes(_ePref, 8710); break; // Manuel (à la demande)
                    case eLibConst.POWERBI_EXECUTIONMODE.SAMPLE: powerBIExecutionModeString = eResApp.GetRes(_ePref, 8711); break; // Echantillon
                    case eLibConst.POWERBI_EXECUTIONMODE.SCHEDULED: powerBIExecutionModeString = eResApp.GetRes(_ePref, 8712); break; // Planifié
                }
                powerBIExecutionModeControl.Attributes.Add("value", powerBIExecutionModeString);
                powerBIExecutionModeControl.Attributes.Add("ednvalue", _powerBIReport.ExecutionMode.GetHashCode().ToString());
                li.Controls.Add(powerBIExecutionModeControl);
            }

            #endregion

            #region Format d'export

            li = new HtmlGenericControl("li");
            if (_forReportWizard)
                li.Attributes.Add("class", "reportinputli");
            ul.Controls.Add(li);

            HtmlGenericControl powerBIFormatLabelContainer = new HtmlGenericControl("span");
            HtmlGenericControl powerBIFormatLabel = new HtmlGenericControl("label");
            powerBIFormatLabelContainer.Attributes.Add("class", String.Concat("editor_powerbi_format_label editor_powerbi_label", _forReportWizard ? " editor_powerbi_wizard_label" : " editor_powerbi_reportinformation_label"));
            powerBIFormatLabelContainer.Controls.Add(powerBIFormatLabel);
            powerBIFormatLabel.InnerText = eResApp.GetRes(_ePref, 1304); // Format
            li.Controls.Add(powerBIFormatLabelContainer);

            HtmlGenericControl powerBIFormatControl = null;
            if (_forReportWizard)
            {
                powerBIFormatControl = new HtmlGenericControl("select");
                powerBIFormatControl.ID = "editor_powerbi_format_select";
                powerBIFormatControl.Attributes.Add("class", "editor_powerbi_format_select editor_powerbi_control editor_powerbi_wizard_control");
                powerBIFormatControl.Attributes.Add("onchange", "oReport.SetPowerBIFormat(this);");
                li.Controls.Add(powerBIFormatControl);
                string[] powerBIFormats = { "csv", "xml" };
                foreach (string powerBIFormat in powerBIFormats)
                {
                    HtmlGenericControl powerBIFormatSelectOption = new HtmlGenericControl("option");
                    powerBIFormatSelectOption.InnerText = powerBIFormat;
                    powerBIFormatSelectOption.Attributes.Add("value", powerBIFormat.ToLower());
                    if (_powerBIReport.Format == powerBIFormat.ToLower())
                        powerBIFormatSelectOption.Attributes.Add("selected", "selected");
                    powerBIFormatControl.Controls.Add(powerBIFormatSelectOption);
                }
            }
            else
            {
                powerBIFormatControl = new HtmlGenericControl("input");
                powerBIFormatControl.ID = "editor_powerbi_format_input";
                powerBIFormatControl.Attributes.Add("readonly", "readonly");
                powerBIFormatControl.Attributes.Add("class", "editor_powerbi_format_input editor_powerbi_control editor_powerbi_reportinformation_control");
                powerBIFormatControl.Attributes.Add("title", eResApp.GetRes(Pref, 1304));
                powerBIFormatControl.Attributes.Add("value", _powerBIReport.Format);
                powerBIFormatControl.Attributes.Add("ednvalue", _powerBIReport.Format);
                li.Controls.Add(powerBIFormatControl);
            }

            #endregion

            #region Séparateur pour le format CSV

            li = new HtmlGenericControl("li");
            if (_forReportWizard)
                li.Attributes.Add("class", "reportinputli");
            ul.Controls.Add(li);

            HtmlGenericControl powerBIColumnSeparatorControlsContainer = new HtmlGenericControl("span");
            HtmlGenericControl powerBIColumnSeparatorLabelContainer = new HtmlGenericControl("span");
            HtmlGenericControl powerBIColumnSeparatorLabel = new HtmlGenericControl("label");
            powerBIColumnSeparatorControlsContainer.ID = "editor_powerbi_columnseparator_container";
            powerBIColumnSeparatorControlsContainer.Style.Add("display", _powerBIReport.Format != "xml" ? "block" : "none");
            powerBIColumnSeparatorLabelContainer.Attributes.Add("class", String.Concat("editor_powerbi_columnseparator_label editor_powerbi_label", _forReportWizard ? " editor_powerbi_wizard_label" : " editor_powerbi_reportinformation_label"));
            powerBIColumnSeparatorLabelContainer.Controls.Add(powerBIColumnSeparatorLabel);
            powerBIColumnSeparatorLabel.InnerText = eResApp.GetRes(_ePref, 6723); // Veuillez indiquer le type de séparateur de champs - TOCHECK RES
            powerBIColumnSeparatorControlsContainer.Controls.Add(powerBIColumnSeparatorLabelContainer);
            li.Controls.Add(powerBIColumnSeparatorControlsContainer);

            HtmlGenericControl powerBIColumnSeparatorControl = null;
            if (_forReportWizard)
            {
                powerBIColumnSeparatorControl = new HtmlGenericControl("select");
                powerBIColumnSeparatorControl.ID = "editor_powerbi_columnseparator_select";
                powerBIColumnSeparatorControl.Attributes.Add("class", "editor_powerbi_columnseparator_select editor_powerbi_control editor_powerbi_wizard_control");
                powerBIColumnSeparatorControl.Attributes.Add("onchange", "oReport.SetPowerBIColumnSeparator(this);");
                powerBIColumnSeparatorControlsContainer.Controls.Add(powerBIColumnSeparatorControl);
                string[] powerBIColumnSeparators = { "comma", "semicolon", "colon", "equals", "space", "tab", "custom" };
                foreach (string powerBIColumnSeparator in powerBIColumnSeparators)
                {
                    string powerBIColumnSeparatorOptionLabel = eResApp.GetRes(Pref, 1859); // <Personnalisé>
                    switch (powerBIColumnSeparator)
                    {
                        case "comma": powerBIColumnSeparatorOptionLabel = eResApp.GetRes(Pref, 1853); break; // Virgule
                        case "semicolon": powerBIColumnSeparatorOptionLabel = eResApp.GetRes(Pref, 1854); break; // Point-virgule
                        case "colon": powerBIColumnSeparatorOptionLabel = eResApp.GetRes(Pref, 1855); break; // Deux points
                        case "equals": powerBIColumnSeparatorOptionLabel = eResApp.GetRes(Pref, 1856); break; // Signe égal
                        case "space": powerBIColumnSeparatorOptionLabel = eResApp.GetRes(Pref, 1857); break; // Espace
                        case "tab": powerBIColumnSeparatorOptionLabel = eResApp.GetRes(Pref, 1858); break; // Tabulation
                        case "custom": powerBIColumnSeparatorOptionLabel = eResApp.GetRes(Pref, 1859); break; // <Personnalisé>
                    }

                    HtmlGenericControl powerBIColumnSeparatorSelectOption = new HtmlGenericControl("option");
                    powerBIColumnSeparatorSelectOption.InnerText = powerBIColumnSeparatorOptionLabel;
                    powerBIColumnSeparatorSelectOption.Attributes.Add("value", powerBIColumnSeparator);
                    if (_powerBIReport.ColumnSeparator == powerBIColumnSeparator)
                        powerBIColumnSeparatorSelectOption.Attributes.Add("selected", "selected");
                    powerBIColumnSeparatorControl.Controls.Add(powerBIColumnSeparatorSelectOption);
                }

                // Personnalisé
                HtmlGenericControl powerBIColumnSeparatorCustomValueControl = new HtmlGenericControl("input");
                powerBIColumnSeparatorCustomValueControl.ID = "editor_powerbi_columnseparatorcustomvalue_input";
                powerBIColumnSeparatorCustomValueControl.Attributes.Add("onchange", "oReport.SetPowerBIColumnSeparatorCustomValue(this);");
                powerBIColumnSeparatorCustomValueControl.Attributes.Add("class", "editor_powerbi_columnseparatorcustomvalue_input editor_powerbi_control editor_powerbi_reportinformation_control");
                powerBIColumnSeparatorCustomValueControl.Attributes.Add("title", eResApp.GetRes(Pref, 6723)); // Indiquer le type de séparateur de champs - TOCHECK RES
                powerBIColumnSeparatorCustomValueControl.Attributes.Add("value", (_powerBIReport.ColumnSeparator == "custom") ? _powerBIReport.ColumnSeparatorCustomValue : String.Empty);
                powerBIColumnSeparatorCustomValueControl.Attributes.Add("ednvalue", _powerBIReport.ColumnSeparatorCustomValue);
                powerBIColumnSeparatorCustomValueControl.Attributes.Add("style", String.Concat("display: ", _powerBIReport.ColumnSeparator == "custom" ? "block" : "none"));
                powerBIColumnSeparatorControlsContainer.Controls.Add(powerBIColumnSeparatorCustomValueControl);
            }
            else
            {
                string sColumnSeparatorLabel = _powerBIReport.GetColumnSeparatorLabel();
                powerBIColumnSeparatorControl = new HtmlGenericControl("input");
                powerBIColumnSeparatorControl.ID = "editor_powerbi_columnseparator_input";
                powerBIColumnSeparatorControl.Attributes.Add("readonly", "readonly");
                powerBIColumnSeparatorControl.Attributes.Add("class", "editor_powerbi_columnseparator_input editor_powerbi_control editor_powerbi_reportinformation_control");
                powerBIColumnSeparatorControl.Attributes.Add("title", eResApp.GetRes(Pref, 6723)); // Indiquer le type de séparateur de champs - TOCHECK RES
                powerBIColumnSeparatorControl.Attributes.Add("value", _powerBIReport.ColumnSeparator == "custom" ? String.Concat(sColumnSeparatorLabel, " - ", _powerBIReport.ColumnSeparatorCustomValue) : sColumnSeparatorLabel);
                powerBIColumnSeparatorControl.Attributes.Add("ednvalue", _powerBIReport.ColumnSeparator);
                powerBIColumnSeparatorControlsContainer.Controls.Add(powerBIColumnSeparatorControl);
            }

            #endregion

            #region Contrôles cachés - Paramètres de jours ouvrés pour le mode Echantillon

            HtmlInputHidden powerBICalendarWorkHourBegin = new HtmlInputHidden();
            powerBICalendarWorkHourBegin.ID = "editor_powerbi_calendarworkhourbegin";
            powerBICalendarWorkHourBegin.Value = _powerBIReport.CalendarWorkHourBegin;
            li.Controls.Add(powerBICalendarWorkHourBegin);

            HtmlInputHidden powerBICalendarWorkHourEnd = new HtmlInputHidden();
            powerBICalendarWorkHourEnd.ID = "editor_powerbi_calendarworkhourend";
            powerBICalendarWorkHourEnd.Value = _powerBIReport.CalendarWorkHourEnd;
            li.Controls.Add(powerBICalendarWorkHourEnd);

            #endregion

            // Si affichage sur l'assistant Reporting : Options affichées seulement pour Power BI - Donc masquées par défaut dans tous les autres cas
            if (_forReportWizard)
                ul.Style.Add("display", "none");

            _pgContainer.Controls.Add(ul);

            return true;
        }

    }
}