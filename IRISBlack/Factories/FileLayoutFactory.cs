using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Xrm.IRISBlack.Model;
using EudoQuery;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Factory qui permet de créer des modèles pour les 
    /// le canevas des fiches.
    /// </summary>
    public class FileLayoutFactory
    {

        #region Properties
        eAdminTableInfos TableInfos { get; set; }
        ePref Pref { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="_tabInfos"></param>
        /// <param name="_pref"></param>
        private FileLayoutFactory(eAdminTableInfos _tabInfos, ePref _pref)
        {
            TableInfos = _tabInfos;
            Pref = _pref;
        }

        #endregion

        #region static initializers
        /// <summary>
        /// Initialiseur statique de la classe avec tous les paramètres
        /// </summary>
        /// <param name="_tabInfos"></param>
        /// <param name="_pref"></param>
        /// <returns></returns>
        public static FileLayoutFactory InitFileLayoutFactory(eAdminTableInfos _tabInfos, ePref _pref)
        {
            return new FileLayoutFactory(_tabInfos, _pref);
        }


        #endregion

        #region Public

        /// <summary>
        /// Permet depuis un Json récupéré en back d'avoir
        /// un objet FileLayoutModel.
        /// </summary>
        /// <returns></returns>
        public FileLayoutModel GetJSONFileLayout()
        {
            FileLayoutModel flm = null;
            JObject jObj = null;
            JObject jObjGuided = null;
            JToken JPage = null;
            string exMessages = String.Empty;

            try
            {
                if (!string.IsNullOrEmpty(TableInfos.sJSonPageIrisBlack))
                {
                    jObj = JObject.Parse(TableInfos.sJSonPageIrisBlack);
                    JPage = (jObj.ContainsKey("Page")) ? jObj["Page"] : null;
                }

                if (!string.IsNullOrEmpty(TableInfos.sJSonGuidedIrisPurple))
                    jObjGuided = JObject.Parse(TableInfos.sJSonGuidedIrisPurple);

            }
            catch (Exception ex)
            {
                jObj = null;
                JPage = null;
                jObjGuided = null;
                exMessages = ex.Message;
            }

            try
            {
                flm = new FileLayoutModel
                {
                    Tab = TableInfos.DescId,
                    DetailArea = (JPage != null && JPage["DetailArea"] != null) ? JPage["DetailArea"].ToObject<AreaModel>() : null,
                    WizardBarArea = (JPage != null && JPage["WizardBarArea"] != null) ? JPage["WizardBarArea"].ToObject<AreaModel>() : null,
                    ActivityArea = (JPage != null && JPage["ActivityArea"] != null) ? JPage["ActivityArea"].ToObject<AreaModel>() : null,
                    Activity = (JPage != null && JPage["Activity"] != null) ? (bool)JPage["Activity"] : true,
                    JsonSummary = (JPage != null && (JPage["Summary"] ?? JPage["JsonSummary"]) != null) ? (JPage["Summary"] ?? JPage["JsonSummary"]).ToObject<FileLayoutSummaryModel>() : null,
                    JsonWizardBar = (JPage != null && (JPage["WizardBar"] ?? JPage["JsonWizardBar"]) != null) ? (JPage["WizardBar"] ?? JPage["JsonWizardBar"]).ToObject<FileLayoutWizardModel>() : null,
                    JsonGuidedMode = jObjGuided?.ToObject<FileLayoutGuidedModel>(),
                    NbCols = TableInfos.ColPerLine,
                    CanUpdateWizardBar = TableInfos.CanUpdateWizardBar
                };
            }
            catch (JsonReaderException jsonRE)
            {
                exMessages = String.Concat(exMessages, " - ", jsonRE.Message);
                throw new EudoException(exMessages, eResApp.GetRes(Pref, 72), jsonRE);
            }
            catch (Exception ex)
            {
                exMessages = String.Concat(exMessages, " - ", ex.Message);
                throw new EudoException(exMessages, eResApp.GetRes(Pref, 72), ex);
            }

            return flm;
        }

        /// <summary>
        /// Permet depuis un Json d'enregistrer en back
        /// un objet FileLayoutModel.
        /// </summary>
        /// <param name="oldFlm"></param>
        /// <param name="newFlm"></param>
        /// <returns></returns>
        public eAdminResult SetJSONFileLayout(FileLayoutModel oldFlm, FileLayoutModel newFlm)
        {

            FileLayoutModel flm = oldFlm;

            if (flm.JsonSummary == null || (newFlm.JsonSummary != null && !flm.JsonSummary.Equals(newFlm.JsonSummary)))
            {
                flm.JsonSummary = newFlm.JsonSummary;
            }

            if (flm.JsonWizardBar?.FieldsById == null || flm.JsonWizardBar.DescId != newFlm.JsonWizardBar?.DescId || flm.JsonWizardBar.FieldsById.Length == 0)
            {
                flm.JsonWizardBar = newFlm.JsonWizardBar;
            }

            flm.DetailArea = newFlm.DetailArea;
            flm.ActivityArea = newFlm.ActivityArea;
            flm.WizardBarArea = newFlm.WizardBarArea;

            flm.JsonGuidedMode = null;

            FileLayouPageModel pgFlm = new FileLayouPageModel
            {
                Page = flm
            };

            DescAdvObj descAdvObj = DescAdvObj.GetSingle(flm.Tab, DESCADV_PARAMETER.JSON_STRUCTURE_IRIS_BLACK, JsonConvert.SerializeObject(pgFlm));
            eAdminDescAdv descAdv = new eAdminDescAdv(Pref, new List<DescAdvObj>() { descAdvObj });
            var adminRes = descAdv.SaveDescAdv();
            adminRes.ReturnObject = flm;

            return adminRes;
        }
        #endregion
    }
}