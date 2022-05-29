using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web;
using System.Xml;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Description résumée de eOptionManager
    /// </summary>
    public class eUserOptionsManager : eEudoManager
    {


        int _nUserId;

        /// <summary>
        /// traitement lié au opt
        /// </summary>
        protected override void ProcessManager()
        {

            eConst.OPTIONS_TYPE eOptionType = eConst.OPTIONS_TYPE.UNDEFINED;

            Int32 nOption;
            String sOptionValue = String.Empty;


            #region Récupération des infos postées



            if (_allKeys.Contains("optiontype"))
            {
                if (!Enum.TryParse<eConst.OPTIONS_TYPE>(_context.Request.Form["optiontype"], out eOptionType) || eOptionType == eConst.OPTIONS_TYPE.UNDEFINED)
                    throw new Exception("Type d'option non reconnue : >" + _context.Request.Form["optiontype"] + "<");
            }
            else
                throw new Exception("Type d'option non fournie.");


            if (_allKeys.Contains("option"))
            {
                if (!Int32.TryParse(_context.Request.Form["option"], out nOption))
                    throw new Exception("Option non reconnue : >" + _context.Request.Form["option"] + "<");
            }
            else
                throw new Exception("Option non fournie.");

            if (_allKeys.Contains("optionvalue"))
                sOptionValue = _context.Request.Form["optionvalue"];


            _nUserId = _requestTools.GetRequestFormKeyI("userid") ?? _pref.UserId;
            if (_pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                _nUserId = _pref.UserId;

            #endregion


            InitXMLResult();

            var result = _xmlResult.SelectSingleNode("/result");
            XmlNode successNode = _xmlResult.CreateElement("success");
            result.AppendChild(successNode);

            switch (eOptionType)
            {
                case eConst.OPTIONS_TYPE.CONFIG:
                    successNode.InnerText = ManageConfigOption(nOption, sOptionValue) ? "1" : "0";
                    break;
                case eConst.OPTIONS_TYPE.PREF:
                    throw new NotImplementedException("Gestion des options de PREF non implémentée.");
                //break;
                case eConst.OPTIONS_TYPE.PREF_ADV:
                    successNode.InnerText = ManagePrefAdvOption(nOption, sOptionValue) ? "1" : "0";
                    break;
                case eConst.OPTIONS_TYPE.CONFIG_ADV:
                    throw new NotImplementedException("Gestion des options de CONFIG_ADV non implémentée.");
                //break;
                case eConst.OPTIONS_TYPE.USER:
                    successNode.InnerText = ManageUserOption(nOption, sOptionValue) ? "1" : "0";
                    break;
                case eConst.OPTIONS_TYPE.UNDEFINED:
                    throw new Exception("Type d'Option non reconnue : >" + _context.Request.Form["option"] + "<");
                // break;
                default:
                    break;
            }

            RenderResult(RequestContentType.XML, delegate () { return _xmlResult.OuterXml; });
        }

        private void InitXMLResult()
        {
            #region INIT XML
            XmlNode baseResultNode;
            _xmlResult = new XmlDocument();
            // BASE DU XML DE RETOUR            
            _xmlResult.AppendChild(_xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            baseResultNode = _xmlResult.CreateElement("result");
            _xmlResult.AppendChild(baseResultNode);



            #endregion
        }

        /// <summary>
        /// Gestion des options de type USER
        /// </summary>
        /// <param name="sOption">Option</param>
        /// <param name="sOptionValue">Valeur de l'option</param>
        /// <returns></returns>InnerException
        private bool ManageUserOption(Int32 nOption, String sOptionValue)
        {
            eConst.OPTIONS_USER option;
            Enum.TryParse(nOption.ToString(), out option);

            //Que pour les options de user
            switch (option)
            {
                case eConst.OPTIONS_USER.MEMO:
                    return eUser.SetFieldValue<String>(_pref, _nUserId, "UserMessage", HttpUtility.HtmlDecode(sOptionValue), System.Data.SqlDbType.NText);

                case eConst.OPTIONS_USER.LANG:
                    eUserOptionsUser o = new eUserOptionsUser(_pref);
                    var byolo = o.Update(eConst.OPTIONS_USER.LANG, sOptionValue);

                    if (byolo)
                        return true;
                    else
                    {
                        //gestion erreur 
                        if (o != null)
                        {
                            _eInnerException = o.InnerException;
                            _sMsgError = o.ErrorMsg;
                        }

                        return false;
                    }
                case eConst.OPTIONS_USER.USER_PROFILE:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gère les options de Config
        /// </summary>
        /// <param name="nOption">Option</param>
        /// <param name="sOptionValue">Valeur de l option</param>
        /// <returns></returns>
        private bool ManageConfigOption(int nOption, string sOptionValue)
        {
            eConst.OPTIONS_USER option;
            Enum.TryParse(nOption.ToString(), out option);
            eRequestTools tools = new eRequestTools(_context);

            //Que pour les options de config
            switch (option)
            {
                case eConst.OPTIONS_USER.SIGNATURE:

                    #region corps signature
                    String body = tools.GetRequestFormKeyS("bodysign");

                    if (!eUser.SetFieldValue<String>(_pref, _nUserId, "UserSignature", body, System.Data.SqlDbType.NText))
                        return false;

                    #endregion

                    #region auto add signature

                    bool bAutoAddSign;
                    tools.GetRequestFormKey("autoaddsign", out bAutoAddSign);

                    List<SetParam<eLibConst.PREF_CONFIG>> param = new List<SetParam<eLibConst.PREF_CONFIG>>();
                    param.Add(new SetParam<eLibConst.PREF_CONFIG>(eLibConst.PREF_CONFIG.EMAILAUTOADDSIGN, bAutoAddSign ? "1" : "0"));
                    #endregion

                    return _pref.SetConfig(param, new List<int>() { _nUserId });
                case eConst.OPTIONS_USER.STOPNEWS:

                    List<SetParam<eLibConst.PREF_CONFIG>> cfgsNews = new List<SetParam<eLibConst.PREF_CONFIG>>();

                    var t = eTools.LoadNewsLetterInfos();



                    cfgsNews.Add(new SetParam<eLibConst.PREF_CONFIG>(
                        eLibConst.PREF_CONFIG.DISPLAYVERMSG,
                       _pref.User.UserLevel >= 99 ? t.adminmsg.num.ToString() : t.usermsg.num.ToString()

                        ));

                    return _pref.SetConfig(cfgsNews, new List<int>() { _nUserId });


                case eConst.OPTIONS_USER.EXPORT:

                    #region office release and export mode
                    //Version office 
                    OfficeRelease office = OfficeRelease.OFFICE_97;
                    Int32? officeId = tools.GetRequestFormKeyI("officerelease");
                    if (officeId.HasValue)
                        Enum.TryParse(officeId.Value.ToString(), out office);

                    //Export mode
                    eConst.ExportMode exportMode = eConst.ExportMode.STANDARD;
                    Int32? exportModeId = tools.GetRequestFormKeyI("exportmode");
                    if (exportModeId.HasValue)
                        Enum.TryParse(exportModeId.Value.ToString(), out exportMode);

                    List<SetParam<eLibConst.PREF_CONFIG>> cfgs = new List<SetParam<eLibConst.PREF_CONFIG>>();
                    cfgs.Add(new SetParam<eLibConst.PREF_CONFIG>(eLibConst.PREF_CONFIG.OFFICERELEASE, office.GetHashCode().ToString()));
                    cfgs.Add(new SetParam<eLibConst.PREF_CONFIG>(eLibConst.PREF_CONFIG.EXPORTMODE, exportMode.GetHashCode().ToString()));
                    #endregion

                    return _pref.SetConfig(cfgs, new List<int>() { _nUserId });

                case eConst.OPTIONS_USER.MRUMODE:

                    List<SetParam<eLibConst.PREF_CONFIG>> config = new List<SetParam<eLibConst.PREF_CONFIG>>();
                    config.Add(new SetParam<eLibConst.PREF_CONFIG>(eLibConst.PREF_CONFIG.MRUMODE, sOptionValue));

                    return _pref.SetConfig(config, new List<int>() { _nUserId });
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gère les options de Pref Adv
        /// </summary>
        /// <param name="nOption">Option</param>
        /// <param name="sOptionValue">Valeur de l option</param>
        /// <returns></returns>
        private bool ManagePrefAdvOption(int nOption, string sOptionValue)
        {
            eConst.OPTIONS_USER option;
            Enum.TryParse(nOption.ToString(), out option);
            eRequestTools tools = new eRequestTools(_context);

            //Que pour les options de config
            switch (option)
            {
                case eConst.OPTIONS_USER.FONT_SIZE:
                    //Mise a jour en base et mise a jour de la pref en direct
                    eLibTools.AddOrUpdatePrefAdv(_pref, eLibConst.PREFADV.FONT_SIZE, sOptionValue, eLibConst.PREFADV_CATEGORY.MAIN);
                    _pref.FontSize = sOptionValue;
                    return true;
                default:
                    return false;
            }
        }
    }
}