using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.mgr
{
    /// <className>eTabsManager</className>
    /// <summary>TODO</summary>
    /// <purpose>TODO 
    /// <result>
    ///     <error></error>
    ///     <errorMsg></errorMsg>
    ///     <content></content>
    ///  </result>
    /// </purpose>
    /// <authors>JBE</authors>
    /// <date></date>
    public class eTabsManager : eEudoManager
    {
        private ExtendedDictionary<String, String> _prefDic = new ExtendedDictionary<String, String>();

        XmlNode _detailsNode;

        /// <summary>
        /// Exception interne
        /// </summary>
        private Exception exp = null;
        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            Int32 _groupId = _pref.User.UserGroupId;
            Int32 _userLevel = _pref.User.UserLevel;

            String _lang = _pref.Lang;

            Int32 _userId = _pref.User.UserId;
            String _instance = _pref.GetSqlInstance;
            String _baseName = _pref.GetBaseName;

            String errorDescription = String.Empty;
            Boolean updateSucess = false;

            // Init le document XML
            _xmlResult = new XmlDocument();
            XmlNode _mainNode = _xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
            _xmlResult.AppendChild(_mainNode);
            _detailsNode = _xmlResult.CreateElement("tabsmanager");
            _xmlResult.AppendChild(_detailsNode);

            XmlNode _paramNode = _xmlResult.CreateElement("param");
            _detailsNode.AppendChild(_paramNode);


            try
            {

                // Chargement de la collection du form
                String _prefValue = String.Empty;
                foreach (String prefFld in _allKeys)
                {
                    _prefValue = _context.Request.Form[prefFld].ToString();

                    // On n'ajout pas les param globaux
                    if (!_prefDic.ContainsKey(prefFld.ToLower()))
                        _prefDic.Add(prefFld.ToLower(), _prefValue);
                }

                // Lance la mise à jour
                updateSucess = UpdateTabs(out errorDescription);
            }
            catch (Exception exp1)
            {
                exp = exp1;
                updateSucess = false;
                errorDescription = exp.ToString();
            }



            if (updateSucess)
            {
                XmlNode _resultNode = _xmlResult.CreateElement("result");
                _resultNode.InnerText = "SUCCESS";
                _detailsNode.AppendChild(_resultNode);
            }
            else
            {

                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1]);
                sDevMsg = String.Concat(Environment.NewLine, errorDescription);

                if (exp != null)
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", exp.Message, Environment.NewLine, "Exception StackTrace :", exp.StackTrace);

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg));

                LaunchError();

            }
            RenderResult(RequestContentType.XML, delegate () { return _xmlResult.OuterXml; });
        }

        private Boolean UpdateTabs(out String error)
        {
            error = String.Empty;

            Boolean success = true;


            StringBuilder sbSql = new StringBuilder();

            List<SetParam<ePrefConst.PREF_SELECTION_TAB>> prefSelTab = new List<SetParam<ePrefConst.PREF_SELECTION_TAB>>();
            List<SetParam<eLibConst.PREF_CONFIG>> prefConfig = new List<SetParam<eLibConst.PREF_CONFIG>>();

            foreach (KeyValuePair<String, String> keyValue in _prefDic)
            {
                String formName = keyValue.Key;
                String formValue = keyValue.Value;

                switch (formName)
                {
                    case "taborder":
                        String[] aTabOrder = formValue.Split(';');

                        formValue = String.Empty;
                        foreach (String tabDescId in aTabOrder)
                        {
                            if (String.IsNullOrEmpty(tabDescId))
                                continue;

                            if (!String.IsNullOrEmpty(formValue))
                                formValue = String.Concat(formValue, ";");

                            formValue = String.Concat(formValue, tabDescId);
                        }

                        prefSelTab.Add(new SetParam<ePrefConst.PREF_SELECTION_TAB>(ePrefConst.PREF_SELECTION_TAB.TABORDER, formValue));

                        break;
                    case "taborderid":
                        prefConfig.Add(new SetParam<eLibConst.PREF_CONFIG>(eLibConst.PREF_CONFIG.TABORDERID, formValue));
                        break;
                }
            }

            // MODE DEBUG
            if (true)
            {
                XmlAttribute att = null;
                XmlNode _pNode = _xmlResult.CreateElement("updSelection");

                foreach (SetParam<ePrefConst.PREF_SELECTION_TAB> p in prefSelTab)
                {
                    att = _xmlResult.CreateAttribute(p.Option.ToString());
                    att.Value = p.Value;
                    _pNode.Attributes.Append(att);
                }

                foreach (SetParam<eLibConst.PREF_CONFIG> p in prefConfig)
                {
                    att = _xmlResult.CreateAttribute(p.Option.ToString());
                    att.Value = p.Value;
                    _pNode.Attributes.Append(att);
                }

                if (_pNode.Attributes.Count > 0)
                    _detailsNode.AppendChild(_pNode);
            }

            if (success && prefConfig.Count > 0)
            {
                success = _pref.SetConfig(prefConfig);
                _pref.LoadConfig();
                _pref.LoadPref();
                _pref.LoadTabs();
            }

            if (success && prefSelTab.Count > 0)
            {
                success = _pref.SetTabs(prefSelTab);
                _pref.LoadConfig();
                _pref.LoadPref();
                _pref.LoadTabs();
            }


            return success;
        }

    }
}