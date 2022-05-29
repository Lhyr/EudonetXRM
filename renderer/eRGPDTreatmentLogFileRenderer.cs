using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.renderer
{
    /// <summary>
    /// Rendu d'une fiche RGPDTreatmentLog
    /// </summary>
    public class eRGPDTreatmentLogFileRenderer : eMainFileRenderer
    {

        /// <summary>
        /// Affichage pour la création et la modification
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nFileId"></param>
        public eRGPDTreatmentLogFileRenderer(ePref pref, Int32 nFileId)
            : base(pref, (int)TableType.RGPDTREATMENTSLOGS, nFileId)
        {

            if (pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                throw new EudoAdminInvalidRightException();

            _rType = RENDERERTYPE.EditFile;
        }


        /// <summary>
        /// Construction des objets HTML
        /// </summary>
        /// <returns></returns>
        protected override Boolean Build()
        {
            return base.Build();
        }

        /// <summary>
        /// remplit le web control avec le contenu souhaité
        /// </summary>
        /// <param name="ednWebCtrl"></param>
        /// <param name="sValue"></param>
        protected override void GetRawMemoControl(EdnWebControl ednWebCtrl, String sValue)
        {
            GetHTMLMemoControl(ednWebCtrl, sValue);
        }

        /// <summary>
        /// Field de type char : gestion des cas particulier de User
        /// </summary>
        /// <param name="row"></param>
        /// <param name="fieldRow"></param>
        /// <param name="ednWebControl"></param>
        /// <param name="sbClass"></param>
        /// <param name="sClassAction"></param>
        /// <returns></returns>
        protected override bool RenderCharFieldFormat(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref string sClassAction)
        {
            int descid = fieldRow.FldInfo.Descid;

            if (
                descid == (int)RGPDTreatmentsLogsField.TabsID
               || descid == (int)WorkflowScenarioField.WFTTARGETDESCID 
               || descid == (int)WorkflowScenarioField.WFTEVENTDESCID
                )
                return RenderTabsIdsField(row, fieldRow, ednWebControl, sbClass, ref sClassAction);
            else if (descid == (int)RGPDTreatmentsLogsField.FieldsID)
                return RenderFieldsIdsField(row, fieldRow, ednWebControl, sbClass, ref sClassAction);
            else if (descid == (int)RGPDTreatmentsLogsField.PersonnalDataCategoriesID)
                return RenderPersonnalDataCategoriesIdsField(row, fieldRow, ednWebControl, sbClass, ref sClassAction);
            else if (descid == (int)RGPDTreatmentsLogsField.SensibleDataID)
                return RenderSensibleDataCategoriesIdsField(row, fieldRow, ednWebControl, sbClass, ref sClassAction);
           

            return base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);
        }

        /// <summary>
        /// Rendu du champ numérique
        /// </summary>
        /// <param name="row">eRecord</param>
        /// <param name="fieldRow">eFieldRecord</param>
        /// <param name="ednWebControl">ednWebControl</param>
        /// <param name="format">FieldFormat</param>
        /// <param name="sbClass">StringBuilder de classe</param>
        /// <param name="sClassAction">Classe action</param>
        /// <returns></returns>
        protected override bool RenderNumericFieldFormat(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, FieldFormat format, StringBuilder sbClass, ref string sClassAction)
        {
            if (fieldRow.FldInfo.Descid == (int)RGPDTreatmentsLogsField.Type || fieldRow.FldInfo.Descid == (int)RGPDTreatmentsLogsField.Status)
                return RenderSpecialNumericField(row, fieldRow, ednWebControl, sbClass, ref sClassAction);

            return base.RenderNumericFieldFormat(row, fieldRow, ednWebControl, format, sbClass, ref sClassAction);
        }

        private bool RenderFieldsIdsField(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref string sClassAction)
        {
            ednWebControl.WebCtrl.Attributes.Add("data-desct", ((int)eCatalogDesc.DescType.Field).ToString());
            return base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);
        }

        private bool RenderTabsIdsField(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref string sClassAction)
        {
            ednWebControl.WebCtrl.Attributes.Add("data-desct", ((int)eCatalogDesc.DescType.Table).ToString());
            return base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);
        }

        private bool RenderSpecialNumericField(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref string sClassAction)
        {
            bool bAllowEmptyString = false;
            Dictionary<string, string> dicValues = new Dictionary<string, string>();

            if (fieldRow.FldInfo.Descid == (int)RGPDTreatmentsLogsField.Type)
            {
                dicValues[((int)RGPDRuleType.Undefined).ToString()] = String.Empty;

                eEnumTools<RGPDRuleType> rgpdTypes = new eEnumTools<RGPDRuleType>();
                foreach (RGPDRuleType rgpdType in rgpdTypes.GetList)
                {
                    int? val = Outils.EnumToResId.GetRGPDTypeResID(rgpdType);
                    if (val == null)
                        continue;

                    string lib = eResApp.GetRes(Pref.LangServId, val.Value);
                    dicValues[((int)rgpdType).ToString()] = lib;
                }
            }
            else if (fieldRow.FldInfo.Descid == (int)RGPDTreatmentsLogsField.Status)
            {
                bAllowEmptyString = true;
                dicValues[((int)RGPDTreatmentLogStatus.Undefined).ToString()] = String.Empty;

                eEnumTools<RGPDTreatmentLogStatus> treatmentStatus = new eEnumTools<RGPDTreatmentLogStatus>();
                foreach (RGPDTreatmentLogStatus status in treatmentStatus.GetList)
                {
                    int? val = Outils.EnumToResId.GetRGPDStatusResID(status);
                    if (val == null)
                        continue;

                    string lib = eResApp.GetRes(Pref.LangServId, val.Value);
                    dicValues[((int)status).ToString()] = lib;
                }
            }

            string function = (row.RightIsUpdatable && fieldRow.RightIsUpdatable) ? String.Concat("(function(obj){ nsEfileJS.UpdateSelect(obj, null,  ", PopupDisplay ? "0" : "1", ")})") : "";
            string value = (String.IsNullOrEmpty(fieldRow.Value) ? "0" : fieldRow.Value);

            HtmlGenericControl sel = eTools.GetSelectCombo(
                String.Concat("SEL_", ednWebControl.WebCtrl.ID),
                dicValues,
                (!(row.RightIsUpdatable && fieldRow.RightIsUpdatable)),
                "selectadmin treatmentLogs",
                function,
                value,
                allowEmptyString: bAllowEmptyString);

            ednWebControl.AdditionalWebCtrl = sel;
            sel.Attributes.Add("inptID", ednWebControl.WebCtrl.ID);

            sClassAction = "LNKNUM ";

            ednWebControl.WebCtrl.Attributes.Add("dbv", value);
            ednWebControl.WebCtrl.Attributes.Add("value", value);
            ednWebControl.WebCtrl.Attributes.Add("type", "hidden");

            return true;
        }

        private bool RenderPersonnalDataCategoriesIdsField(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref string sClassAction)
        {
            ednWebControl.WebCtrl.Attributes.Add("data-enumt", ((int)eCatalogEnum.EnumType.RGPDTreatmentLogPersonnalDataCategory).ToString());
            return base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);
        }

        private bool RenderSensibleDataCategoriesIdsField(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref string sClassAction)
        {
            ednWebControl.WebCtrl.Attributes.Add("data-enumt", ((int)eCatalogEnum.EnumType.RGPDTreatmentLogSensibleDataCategory).ToString());
            return base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);
        }
    }
}