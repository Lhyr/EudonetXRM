using System;
using System.Web.Http;
using Com.Eudonet.Core.Model;
using Newtonsoft.Json;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Model;
using System.Text;
using System.Collections.Generic;
using EudoQuery;
using System.Text.RegularExpressions;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Controllers
{
    /// <summary>
    /// Contrôleur permettant de récupérer la structure du formulaire avancé
    /// </summary>
    public class FormularParamController : BaseController
    {

        //Gestion des erreurs
        eFormularException formExeption = null;


        /// <summary>
        /// Récupère la liste de champs de fusion à partir de l'id de la table
        /// </summary>
        /// <param name="nTab"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{nTab:int=0}")]
        public IHttpActionResult Get(int nTab)
        {
            try
            {
                Dictionary<String, String> _AvailableLanguages;
                bool isWordLineExtensionIsActivated = false;
                using (eudoDAL dal = eLibTools.GetEudoDAL(_pref))
                {
                    dal.OpenDatabase();
                    //On récupére la liste des langues
                    _AvailableLanguages = eLibTools.GetUsersLangWithFilter(dal, new eLibTools.FilterLang());
                    dal.CloseDatabase();
                    isWordLineExtensionIsActivated = eExtension.IsReadyStrict(_pref, "WORLDLINECORE", true);

                }
                return Ok(JsonConvert.SerializeObject(new FormularParamsModel()
                {
                    AvailableLanguages = _AvailableLanguages,
                    MergeFields = GetMergeAndTrackFields(_pref, nTab),
                    MergeFieldsWithoutExtendedFields = GetMergeAndTrackFields(_pref, nTab, false, true),
                    HyperLinkMergeFields = GetHyperLinkMergeFields(_pref, nTab),
                    IsWorldLineExtensionIsActivated = isWordLineExtensionIsActivated,
                    UserInfos = new IRISBlack.AdvFormularUserInfoModel()
                    {
                        Login = _pref.User.UserLogin,
                        LangId = _pref.User.UserLangId
                    }
                }));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);

            }
        }

        /// <summary>
        /// On récupère les res selon l'id de la langue
        /// </summary>
        /// <param name="nTab"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{nLangId:int=0}/{nRes:int}")]
        public IHttpActionResult Get(int nLangId, int nRes)
        {
            try
            {
                string btnRes = eResApp.GetRes(nLangId, nRes);
                return Ok(JsonConvert.SerializeObject(btnRes));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);

            }
        }



        [HttpPost]
        public IHttpActionResult Post(FormularModel formularModel)
        {
            return InternalServerError(new NotImplementedException());
        }

        // DELETE  
        public override IHttpActionResult Delete(int id)
        {
            return InternalServerError(new NotImplementedException());
        }

        /// <summary>
        /// Récupère la liste de champs de fusion à partir de l'id de la table
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTabFrom"></param>
        /// <param name="bGetOnlyTxtMergedField"></param>
        /// <returns></returns>
        private String GetMergeAndTrackFields(ePref pref, int nTabFrom, bool bGetOnlyTxtMergedField = false, bool bGetWithoutExtendedCible = false)
        {
            StringBuilder strScriptBuilder = new StringBuilder();

            #region Champs de fusion
            string strJavaScript = String.Empty;
            string error = String.Empty;

            eTableLiteMailing table;
            IEnumerable<eFieldLiteWithLib> fields = null;

            eudoDAL dal = eLibTools.GetEudoDAL(pref);

            try
            {
                dal.OpenDatabase();

                if (nTabFrom == 0)
                    throw new Exception("Invalide nTabFrom = " + nTabFrom);

                table = new eTableLiteMailing(nTabFrom, pref.Lang);
                table.ExternalLoadInfo(dal, out error);
                if (error.Length > 0)
                    throw new Exception(error);

                //Tous les champs de fusion
                List<int> AllMergeFields = eLibTools.GetMergeFieldsMailingList(dal, pref, table, bGetWithoutExtendedCible, bGetOnlyTxtMergedField: bGetOnlyTxtMergedField);

                //On filtre la  liste par rapport aux droits de visu
                List<int> AllowedMergeFields = new List<int>(eLibTools.GetAllowedFieldsFromDescIds(pref, pref.User, String.Join(";", AllMergeFields.ToArray()), false).Keys);

                //on construit la liste des champs
                eLibTools.GetMergeFieldsData(dal, pref, pref.User, AllowedMergeFields, null, null, null, null, null, null, out strJavaScript);

            }
            catch (Exception ex)
            {
                throw new Exception("MergeFieldController::GetMergeAndTrackFields:", ex);
            }
            finally
            {
                dal.CloseDatabase();
            }

            strScriptBuilder.Append(String.IsNullOrEmpty(strJavaScript) ? "{}" : strJavaScript).AppendLine();

            #endregion


            return strScriptBuilder.ToString();
        }

        /// <summary>
        /// Récupère la liste de champs de fusion de type siteweb à partir de l'id de la table
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTabFrom"></param>
        /// <param name="bGetOnlyTxtMergedField"></param>
        /// <param name="bGetWithoutExtendedCible"></param>
        /// <returns></returns>
        private String GetHyperLinkMergeFields(ePref pref, int nTabFrom, bool bGetOnlyTxtMergedField = false, bool bGetWithoutExtendedCible = false)
        {
            StringBuilder strScriptBuilder = new StringBuilder();

            #region Champs de fusion
            string strWebsiteFieldsJavaScript = String.Empty;
            string error = String.Empty;

            eTableLiteMailing table;

            eudoDAL dal = eLibTools.GetEudoDAL(pref);

            try
            {
                dal.OpenDatabase();

                if (nTabFrom == 0)
                    throw new Exception("Invalide nTabFrom = " + nTabFrom);

                table = new eTableLiteMailing(nTabFrom, pref.Lang);
                table.ExternalLoadInfo(dal, out error);
                if (error.Length > 0)
                    throw new Exception(error);

                //Tous les champs de fusion de type site web
                List<int> AllWebsiteMergeFields = eLibTools.GetSpecificMergeFieldsMailingList(dal, pref, table, table.ProspectEnabled, false, false, FieldFormat.TYP_WEB);

                //On filtre la liste des champs de type site web
                List<int> WebsiteMergeFields = new List<int>(eLibTools.GetAllowedFieldsFromDescIds(pref, pref.User, String.Join(";", AllWebsiteMergeFields.ToArray()), false).Keys);

                //on construit la liste des champs de type site web
                eLibTools.GetMergeFieldsData(dal, pref, pref.User, WebsiteMergeFields, null, null, null, null, null, null, out strWebsiteFieldsJavaScript);

                strScriptBuilder.Append("{ \"link\" :{\"href\":\"\", \"ednc\":\"lnk\", \"ednt\":\"on\", \"ednd\":\"0\", \"ednn\":\"\", \"ednl\":\"0\", \"title\":\"\", \"target\":\"_blank\"}, \"fields\" : [ ")
                .Append("[\"<").Append(eResApp.GetRes(pref, 141)).Append(">\", \"0\", \"\"]");     // option vide

                strWebsiteFieldsJavaScript = strWebsiteFieldsJavaScript.Replace(System.Environment.NewLine, string.Empty);

                if (String.IsNullOrEmpty(strWebsiteFieldsJavaScript))
                    strScriptBuilder.Append("");
                else
                {
                    string[] websiteFields = strWebsiteFieldsJavaScript.Split(',');
                    foreach (String wbF in websiteFields)
                    {
                        string[] strlist1 = wbF.Split(':');
                        if (strlist1.Length > 1)
                        {
                            string fldLibelle = Regex.Replace(strlist1[0], "(\\r|\\n|\\|\u0022|{)*", String.Empty);
                            string[] strlist2 = strlist1[1].Split(';');
                            string fldDescId = String.Concat("\"", strlist2[0].Replace("\"", String.Empty), "\"").Replace(" ", String.Empty);
                            string fldFormat = String.Concat("\"", strlist2[3], "\"").Replace(" ", String.Empty);
                            string wsField = String.Concat(", [", HttpUtility.HtmlDecode(fldLibelle), ",", fldDescId, ",", fldFormat, "]");
                            strScriptBuilder.Append(wsField);
                        }
                    }
                }

                strScriptBuilder.Append(" ]} ");
            }
            catch (Exception ex)
            {
                throw new Exception("MergeFieldController::GetMergeAndTrackFields:", ex);
            }
            finally
            {
                dal.CloseDatabase();
            }

            

            #endregion
            return strScriptBuilder.ToString();
        }

    }
}
