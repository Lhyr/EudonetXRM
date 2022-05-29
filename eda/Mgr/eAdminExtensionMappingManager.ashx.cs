using System;
using System.Collections.Generic;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Internal;
using System.Web;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Xrm.eda.Mgr;
using EudoQuery;
using static Com.Eudonet.Internal.eOutlookAddinSetting;
using static Com.Eudonet.Internal.eLibConst;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Manager de gestion des interfaces de mapping de champ
    /// </summary>
    public class eAdminExtensionMappingManager : eAdminManager
    {

        ReturnMappingResult result = new ReturnMappingResult();

        /// <summary>
        /// Aiguillage
        /// </summary>
        protected override void ProcessManager()
        {
            result.Success = false;

            // Récupération de la demande
            StreamReader sr = new StreamReader(_context.Request.InputStream);

            string s = sr.ReadToEnd();
            AdminExtensionMapping mapps;

            try
            {
                mapps = JsonConvert.DeserializeObject<AdminExtensionMapping>(s);
                result = ProcessMaps(mapps);

            }
            catch (JsonReaderException)
            {
                result.ErrorTitle = eResApp.GetRes(_pref, 92);
                result.ErrorCode = (int)MAPPING_ERROR.INVALID_JSON;
            }
            catch (Exception e)
            {

                result.ErrorTitle = eResApp.GetRes(_pref, 92);
                result.ErrorMsg = e.Message;
                result.ErrorCode = (int)MAPPING_ERROR.UNHANDLED;
            }


            //Aiguillage du type de mapping


            RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(result); });
        }


        private ReturnMappingResult ProcessMaps(AdminExtensionMapping mapps)
        {
            result.Success = false;

            if (mapps.mappingid == "outlookAddin" || mapps.mappingid == "linkedincontact")
            {
                string codeExtension = (mapps.mappingid == "outlookAddin") ? "OUTLOOKADDIN" : "LINKEDINCONTACT";
                //Validation de la demande
                eExtension eRegisteredExt = eExtension.GetExtensionByCode(_pref, codeExtension);

                //Mise à jour possible si extension installée
                if (eRegisteredExt != null &&
                    (
                        eRegisteredExt.Status == EXTENSION_STATUS.STATUS_READY
                       || eRegisteredExt.Status == EXTENSION_STATUS.STATUS_NEED_CONFIGURING
                    )
                 )
                {
                    eOutlookAddinSetting outlookAddinSettings = new eOutlookAddinSetting();
                    eLinkedinSetting linkedinSettings = new eLinkedinSetting();
                    if (codeExtension =="OUTLOOKADDIN")
                    {
                        outlookAddinSettings = string.IsNullOrEmpty(eRegisteredExt.Param)  ? new eOutlookAddinSetting() : eOutlookAddinSetting.FromJsonString(eRegisteredExt.Param); 
                        outlookAddinSettings.LoadMapDesc(_pref);
                    }
                    else
                    {
                        linkedinSettings = string.IsNullOrEmpty(eRegisteredExt.Param)  ? new eLinkedinSetting() : eLinkedinSetting.FromJsonString(eRegisteredExt.Param); 
                        linkedinSettings.LoadMapDesc(_pref);
                    }

                    bool bMaj = false;
                    foreach (var fld in mapps.mappingflds)
                    {
                        int nKey = 0;
                        int nDescId = 0;
                        VALUES_CHECK_RESULT res = VALUES_CHECK_RESULT.UNDEFINED;
                        if (Int32.TryParse(fld.key, out nKey) && Int32.TryParse(fld.value, out nDescId))
                        {                           
                            if (codeExtension == "OUTLOOKADDIN")
                            {
                                eLibConst.OUTLOOK_ADDIN_KEY key = eLibTools.GetEnumFromCode<eLibConst.OUTLOOK_ADDIN_KEY>(nKey);
                                res = (outlookAddinSettings.CheckKeyValue(key, nDescId, true));
                            }                                                          
                            else
                            {
                                eLibConst.LINKEDIN_KEY key = eLibTools.GetEnumFromCode<eLibConst.LINKEDIN_KEY>(nKey);

                                res = (linkedinSettings.CheckKeyValue(key, nDescId, true));
                            }
                               
                            if (res == VALUES_CHECK_RESULT.VALID)
                            {
                                //Une maj est nécesaire
                                bMaj = true;
                            }
                            else
                            {
                                result.ErrorTitle = eResApp.GetRes(_pref, 92);

                                if (res == VALUES_CHECK_RESULT.MANDATORY)
                                {
                                    result.ErrorMsg = eResApp.GetRes(_pref, 2363);//"La rubrique '{RUB}' est obligatoire.";
                                    result.ErrorCode = (int)MAPPING_ERROR.MANDATORY_PARAMETER;
                                }
                                else if (res == VALUES_CHECK_RESULT.EXIST)
                                {
                                   result.ErrorMsg = eResApp.GetRes(_pref, 8861);//"La rubrique '{RUB}' est obligatoire.";
                                    result.ErrorCode = (int)MAPPING_ERROR.DESCID_EXIST;
                                }
                                else
                                {
                                    result.ErrorCode = (int)MAPPING_ERROR.INVALID_PARAMETER;
                                    result.ErrorMsg = eResApp.GetRes(_pref, 2364); //"La valeur '{VAL}' choisie n'est pas compatible avec la rubrique '{RUB}'.";
                                }
                                return result;
                            }
                        }
                        else
                        {
                            result.ErrorTitle = eResApp.GetRes(_pref, 92);
                            result.ErrorCode = (int)MAPPING_ERROR.INVALID_PARAMETER;
                            result.ErrorMsg = "Paramètres invalides.";
                            return result;
                        }
                    }

                    if (bMaj)
                    {
                        bool resultSaveOutlook = false, resultSaveLinkedin = false;
                        if (codeExtension == "OUTLOOKADDIN")
                        {
                            resultSaveOutlook = outlookAddinSettings.SaveMapping(_pref, eModelTools.GetRootPhysicalDatasPath());
                        }
                        else
                        {
                            resultSaveLinkedin = linkedinSettings.SaveMapping(_pref, eModelTools.GetRootPhysicalDatasPath());
                        }
                        
                        if (resultSaveOutlook)
                        {

                            bool isMappingValid = false;
                            var res = outlookAddinSettings.CheckAllValues(out isMappingValid);

                            result.MappingComplete = isMappingValid;

                            if (!isMappingValid)
                                result.MappingFieldsError = res.Where(a => a.Value != VALUES_CHECK_RESULT.VALID).ToDictionary(a => a.Key, b => b.Value);

                            result.Success = true;
                        }
                        else if (resultSaveLinkedin )
                        {
                            bool isMappingValid = false;
                            var res = linkedinSettings.CheckAllValues(out isMappingValid);

                            result.MappingComplete = isMappingValid;

                            if (!isMappingValid)
                                result.MappingLinkedinFieldsError = res.Where(a => a.Value != VALUES_CHECK_RESULT.VALID).ToDictionary(a => a.Key, b => b.Value);

                            result.Success = true;
                        }
                        else
                        {
                            result.ErrorTitle = eResApp.GetRes(_pref, 92);
                            result.ErrorCode = (int)MAPPING_ERROR.UPDATE_FAILED;
                            result.ErrorMsg = "La mise à jour a échouée.";
                            return result;
                        }
                    }
                    else
                    {
                        result.ErrorTitle = eResApp.GetRes(_pref, 92);
                        result.ErrorCode = (int)MAPPING_ERROR.NO_VALUE;
                        return result;
                    }
                }
            }
            else
            {
                result.ErrorTitle = eResApp.GetRes(_pref, 92);
                result.ErrorCode = (int)MAPPING_ERROR.INVALID_MAPPING_TYPE;
                return result;
            }



            return result;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Représentation d'une demande de modif de planning
    /// </summary>
    public class AdminExtensionMapping
    {
        public string mappingid { get; set; } = "";

        public string action { get; set; } = "";

        public List<AdminExtensionMappingFld> mappingflds { get; set; } = new List<AdminExtensionMappingFld>();

    }

    /// <summary>
    /// Représentation des champs de mapping à modifier
    /// </summary>
    public class AdminExtensionMappingFld
    {
        public string key { get; set; } = "";
        public string value { get; set; } = "";
    }


    /// <summary>
    /// type de retour JSON pour le manager
    /// </summary>
    public class ReturnMappingResult : JSONReturnGeneric
    {

        public bool MappingComplete = false;
        public Dictionary<OUTLOOK_ADDIN_KEY, VALUES_CHECK_RESULT> MappingFieldsError = new Dictionary<OUTLOOK_ADDIN_KEY, VALUES_CHECK_RESULT>();
        public Dictionary<LINKEDIN_KEY, VALUES_CHECK_RESULT> MappingLinkedinFieldsError = new Dictionary<LINKEDIN_KEY, VALUES_CHECK_RESULT>();
    }


    /// <summary>
    /// Code d'erreur de retour 
    /// </summary>
    public enum MAPPING_ERROR
    {
        /// <summary>
        /// Json envoyé invalide
        /// </summary>
        INVALID_JSON = 0,

        /// <summary>
        /// erreur non géré
        /// </summary>
        UNHANDLED = 1,

        /// <summary>
        /// La valeur est invalide pour la rubrique
        /// </summary>
        INVALID_PARAMETER = 2,

        /// <summary>
        /// Aucune valeur valide
        /// </summary>
        NO_VALUE = 3,

        /// <summary>
        /// Echéc de mise à jour
        /// </summary>
        UPDATE_FAILED = 4,

        /// <summary>
        /// Type de mapping non géré
        /// </summary>
        INVALID_MAPPING_TYPE = 5,

        /// <summary>
        /// Une valeur vide a été envoyée pour une rubrique obligatoire
        /// </summary>
        MANDATORY_PARAMETER = 6,

        /// <summary>
        /// DescId déjà existe dans le mapping
        /// </summary>
        DESCID_EXIST = 7
    }
}