using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Com.Eudonet.Xrm.mgr
{
    /// <className>ePrefAdvConfigAdvManager</className>
    /// <summary>Manager de mise à jour de preférence utilisateur avancée (PREFADV) et paramètre de configuration avancé (CONFIGADV)</summary>
    /// <purpose></purpose>
    /// <authors>MAB</authors>
    /// <date>2014-06-10</date>
    public class ePrefAdvConfigAdvManager : eEudoManager
    {
        private Int32 _tab = 0;
        private IDictionary<String, String> _prefDic = new Dictionary<String, String>();
        // Catégorie de la préférence à insérer
        private Int32 _category;
        // UserID à insérer dans la table PREFADV
        private Int32 _parameterUserId = 0;

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

            String prefTyp = String.Empty;
            String errorDescription = String.Empty;
            Boolean updateSuccess = false;

            // Param
            _tab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            _parameterUserId = _requestTools.GetRequestFormKeyI("userid") ?? 0;
            _category = _requestTools.GetRequestFormKeyI("category") ?? 0;

            if (_parameterUserId != _pref.UserId && _pref.User.UserLevel < 99)
                _parameterUserId = _pref.UserId;

            XmlDocument xmlResult = new XmlDocument();

            // Init le document XML
            XmlNode mainNode = xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlResult.AppendChild(mainNode);

            XmlNode detailsNode = xmlResult.CreateElement("prefadvconfigadvmanager");
            xmlResult.AppendChild(detailsNode);

            XmlNode _paramNode = xmlResult.CreateElement("param");
            detailsNode.AppendChild(_paramNode);

            try
            {
                // Chargement de la collection du form
                String _prefValue = String.Empty;
                foreach (String prefFld in _allKeys)
                {
                    _prefValue = _context.Request.Form[prefFld].ToString();

                    // Filtrage des paramètres globaux
                    if (prefFld != "tab" && prefFld != "typ" && prefFld != "userid" && prefFld != "category" && prefFld != "_processid")
                        if (!_prefDic.ContainsKey(prefFld.ToUpper()))
                            _prefDic.Add(prefFld.ToUpper(), _prefValue);
                }

                // Lance la mise à jour
                updateSuccess = UpdateParameter(out errorDescription);
            }
            catch (Exception e)
            {
                updateSuccess = false;
                errorDescription = e.ToString();
            }
            finally
            {
            }

            XmlNode resultNode = xmlResult.CreateElement("result");
            if (updateSuccess)
            {
                resultNode.InnerText = "SUCCESS";
                detailsNode.AppendChild(resultNode);
            }
            else
            {
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), string.Empty, eResApp.GetRes(_pref, 72), errorDescription));
            }

            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
        }

        private Boolean UpdateParameter(out String error)
        {
            error = String.Empty;

            Boolean success = true;

            foreach (KeyValuePair<String, String> keyValue in _prefDic)
            {
                String value = keyValue.Value;

                if (_parameterUserId == 0)
                {
                    if (_pref.User.UserLevel < 99)
                        throw new EudoAdminInvalidRightException();

                    UpdateConfigAdv(keyValue);
                }
                else
                {
                    UpdatePrefAdv(keyValue);
                }
            }

            return success;
        }

        private void UpdateConfigAdv(KeyValuePair<String, String> keyValue)
        {
            eLibConst.CONFIGADV parameter = eLibConst.CONFIGADV.UNDEFINED;
            eLibConst.CONFIGADV_CATEGORY category = eLibConst.CONFIGADV_CATEGORY.UNDEFINED;
            try
            {
                Enum.TryParse(keyValue.Key, out parameter);
                category = (eLibConst.CONFIGADV_CATEGORY)_category;
            }
            catch
            {
                parameter = eLibConst.CONFIGADV.UNDEFINED;
                category = eLibConst.CONFIGADV_CATEGORY.UNDEFINED;
            }

            eLibTools.AddOrUpdateConfigAdv(_pref, parameter, keyValue.Value, category);
        }

        private void UpdatePrefAdv(KeyValuePair<String, String> keyValue)
        {
            eLibConst.PREFADV parameter = eLibConst.PREFADV.UNDEFINED;
            eLibConst.PREFADV_CATEGORY category = eLibConst.PREFADV_CATEGORY.UNDEFINED;
            try
            {
                Enum.TryParse(keyValue.Key, out parameter);
                category = (eLibConst.PREFADV_CATEGORY)_category;
            }
            catch
            {
                parameter = eLibConst.PREFADV.UNDEFINED;
                category = eLibConst.PREFADV_CATEGORY.UNDEFINED;
            }

            bool setTab = false;
            string value = keyValue.Value;

            switch (parameter)
            {
                case eLibConst.PREFADV.THEME:
                    // Met à jour la session et on verif si c'est bien un INT
                    _pref.ReloadTheme(eLibTools.GetNum(value));
                    if(_pref.ThemeXRM.FontSizeMax != null && _pref.ThemeXRM.FontSizeMax.Count>0)
                    {
                        int nMax = _pref.ThemeXRM.FontSizeMax.Max();
                        int nCurrent;
                        if (int.TryParse(_pref.FontSize, out nCurrent))
                        {

                            if (nCurrent > nMax)
                                _pref.FontSize = nMax.ToString();
                        }

                    }

                    value = _pref.ThemeXRM.Id.ToString();
                    break;

                case eLibConst.PREFADV.RIGHTMENUPINNED:
                    // Mise à jour du statut du menu pour la session en cours
                    _pref.RightMenuPinned = (value == "1");
                    break;

                case eLibConst.PREFADV.FONT_SIZE:
                    _pref.FontSize = value;
                    break;

                case eLibConst.PREFADV.SEARCHCOLWIDTH:
                    //CNA - #46232 - Mise à jour taille des colonnes dans les recherches avancés

                    setTab = true;

                    Dictionary<int, int> dicoNewSearchColWidth = new Dictionary<int, int>();

                    IDictionary<eLibConst.PREFADV, string> dicoPrefAdvValues =
                        eLibTools.GetPrefAdvValues(_pref, new List<eLibConst.PREFADV>() { eLibConst.PREFADV.SEARCHCOLWIDTH }, _parameterUserId, _tab);
                    if (dicoPrefAdvValues.ContainsKey(eLibConst.PREFADV.SEARCHCOLWIDTH))
                    {
                        String sSearchColWidth = String.Empty;
                        dicoPrefAdvValues.TryGetValue(eLibConst.PREFADV.SEARCHCOLWIDTH, out sSearchColWidth);
                        if (sSearchColWidth.Length > 0)
                            sSearchColWidth.ConvertToListKeyValInt(";", ":").ForEach(delegate (KeyValuePair<int, int> kvp) { if (!dicoNewSearchColWidth.ContainsKey(kvp.Key)) { dicoNewSearchColWidth.Add(kvp.Key, kvp.Value); } });
                    }

                    Int32 descid = 0;
                    Int32 width = 0;

                    string[] valueArray = value.Split(";");
                    if (valueArray.Length > 1 && Int32.TryParse(valueArray[0], out descid) && Int32.TryParse(valueArray[1], out width))
                    {
                        if (width == 0 && dicoNewSearchColWidth.ContainsKey(descid))
                            dicoNewSearchColWidth.Remove(descid);
                        else
                        {
                            if (!dicoNewSearchColWidth.ContainsKey(descid))
                                dicoNewSearchColWidth.Add(descid, width);
                            else
                                dicoNewSearchColWidth[descid] = width;
                        }
                    }

                    ICollection<string> listValues = new List<string>();
                    foreach (KeyValuePair<int, int> kvp in dicoNewSearchColWidth)
                    {
                        string kvpStr = String.Concat(kvp.Key.ToString(), ":", kvp.Value.ToString());
                        listValues.Add(kvpStr);
                    }
                    value = String.Join(";", listValues);
                    break;

            }

            if (setTab)
                eLibTools.AddOrUpdatePrefAdv(_pref, parameter, value, category, _parameterUserId, _tab);
            else
                eLibTools.AddOrUpdatePrefAdv(_pref, parameter, value, category, _parameterUserId);
        }
    }
}