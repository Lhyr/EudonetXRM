using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model.prefs;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Xrm.eda;

namespace Com.Eudonet.Xrm.mgr
{
    /// <className>ePrefManager</className>
    /// <summary>Manageur de mise à jour de preférence utilisateur</summary>
    /// <purpose></purpose>
    /// <authors>HLA</authors>
    /// <date>2012-06-27</date>
    public class ePrefManager : eEudoManager
    {
        private int _tab = 0;
        private string _type = string.Empty;
        private int _mainField;
        private int _usrSrcId = 0;

        /// <summary>Informations sur les paramètres de la page</summary>
        private XmlNode _paramNode;
        /// <summary>Informations complémentaires retourné en fin de traitement</summary>
        private XmlNode _infosNode;

        private IDictionary<string, string> _prefDic = new Dictionary<string, string>();

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            if (_pref.User.UserId == 0)
            {
                LaunchError(eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),
                    eResApp.GetRes(_pref, 6562),
                    eResApp.GetRes(_pref, 72),
                    "ePrefManager.ashx : Paramètre UserId manquant"));
            }

            string errorDescription = string.Empty;
            string errorStackTrace = string.Empty;
            bool updateSucess = false;

            // Param
            _type = _requestTools.GetRequestFormKeyS("typ") ?? string.Empty;
            if (_type.Length == 0)
                _type = "USER_PREF";
            _tab = _requestTools.GetRequestFormKeyI("tab") ?? 0;

            if(_type.ToUpper() == "USER_PROFILE_PREF")
            {
                _tab = (int)TableType.USER;
                _usrSrcId = _requestTools.GetRequestFormKeyI("src") ?? 0;

                if (_usrSrcId <= 0)
                {
                    LaunchError(eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),
                    eResApp.GetRes(_pref, 6524),
                    eResApp.GetRes(_pref, 72),
                    string.Concat("ePrefManager.ashx : Paramètre src manquant (", _usrSrcId, ")")));
                }

            }

            if (_tab <= 0)
            {
                LaunchError(eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),
                    eResApp.GetRes(_pref, 6524),
                    eResApp.GetRes(_pref, 72),
                    string.Concat("ePrefManager.ashx : Paramètre tab manquant (", _tab, ")")));
            }

            _mainField = _requestTools.GetRequestFormKeyI("mainfld") ?? 0;

            // Init le document XML
            _xmlResult = new XmlDocument();
            XmlNode mainNode = _xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
            _xmlResult.AppendChild(mainNode);

            XmlNode detailsNode = _xmlResult.CreateElement("prefmanager");
            _xmlResult.AppendChild(detailsNode);

            // Param
            _paramNode = _xmlResult.CreateElement("param");
            detailsNode.AppendChild(_paramNode);
            XmlAttribute attTab = _xmlResult.CreateAttribute("tab");
            attTab.Value = _tab.ToString();
            _paramNode.Attributes.Append(attTab);
            XmlAttribute attTyp = _xmlResult.CreateAttribute("typ");
            attTyp.Value = _type;
            _paramNode.Attributes.Append(attTyp);

            // Infos
            _infosNode = _xmlResult.CreateElement("infos");
            detailsNode.AppendChild(_infosNode);

            // Success (Utile pour le profil utilisateur / Useful for user profile)
            XmlNode successNode = _xmlResult.CreateElement("success");
            detailsNode.AppendChild(successNode);

            try
            {
                // Chargement de la collection du form
                string _prefValue = string.Empty;
                foreach (string prefFld in _requestTools.AllKeys)
                {
                    _prefValue = _context.Request.Form[prefFld].ToString();

                    // On n'ajout pas les param globaux
                    if (prefFld != "tab" && prefFld != "typ")
                        _prefDic[prefFld.ToLower()] = _prefValue;
                }

                eudoDAL dal = eLibTools.GetEudoDAL(_pref);

                try
                {
                    dal.OpenDatabase();

                    // Lance la mise à jour
                    switch (_type.ToUpper())
                    {
                        case "USER_FINDER_PREF":
                        case "USER_LISTWIDGET_PREF":
                        case "USER_COLSPREF":
                            updateSucess = UpdateColsPref(dal, out errorDescription);
                            break;
                        case "USER_PREF":
                            updateSucess = UpdatePref(dal, out errorDescription);
                            break;
                        case "USER_BKM_PREF":
                            updateSucess = UpdateBkmPref(dal, out errorDescription);
                            break;
                        case "USER_SELECTION":
                            updateSucess = UpdateSelection(dal, out errorDescription);
                            break;
                        case "USER_PROFILE_PREF":
                            updateSucess = UpdateUserProfileAllPrefs(_usrSrcId, dal, out errorDescription);
                            successNode.InnerText = updateSucess ? "1" : "0";
                            break;
                    }
                }
                finally
                {
                    dal.CloseDatabase();
                }
            }
            catch (Exception e1)
            {
                updateSucess = false;
                errorDescription = e1.ToString();
                errorStackTrace = e1.StackTrace;
            }

            if (updateSucess)
            {
                XmlNode resultNode = _xmlResult.CreateElement("result");
                resultNode.InnerText = "SUCCESS";
                detailsNode.AppendChild(resultNode);
            }
            else
            {
                //Avec exception
                StringBuilder devMsg = new StringBuilder("Erreur sur la page : ");

                // Nom de page
                devMsg.Append(System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1]);

                devMsg.AppendLine().Append(errorDescription);

                // Ajout de la stacktrace
                if (errorStackTrace.Length != 0)
                    devMsg.AppendLine().Append("Exception StackTrace :").Append(errorStackTrace);

                LaunchError(eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),
                    string.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer..., 
                    eResApp.GetRes(_pref, 72),
                    devMsg.ToString()));
            }

            RenderResult(RequestContentType.XML, delegate () { return _xmlResult.OuterXml; });
        }

        /// <summary>
        /// Met à jour les pref de l'utilisateur
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        private bool UpdatePref(eudoDAL dal, out string error)
        {
            error = string.Empty;

            ColFilterOptions colFiltOpts;

            bool success = true;
            List<SetParam<ePrefConst.PREF_PREF>> pref = new List<SetParam<ePrefConst.PREF_PREF>>();

            foreach (KeyValuePair<string, string> keyValue in _prefDic)
            {
                switch (keyValue.Key)
                {
                    case "histo":
                        if (keyValue.Value == "1" || keyValue.Value == "0")
                        {
                            pref.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.HISTO, keyValue.Value));
                            _pref.Context.Paging.resetInfo();
                        }
                        break;

                    case "listfilterid":
                        //Activation du filtre en mode liste
                        //TODO - Cas des filtres spécifiques
                        int _filterId = 0;
                        if (int.TryParse(keyValue.Value, out _filterId))
                        {
                            _pref.Context.Filters.AddOrUpdateValue(_tab, new FilterSel(_filterId), true);
                            _pref.Context.Paging.resetInfo();
                        }
                        break;

                    case "listfilteridname":
                        //Activation du filtre en mode liste
                        //TODO - Cas des filtres spécifiques
                        string[] aFilter = keyValue.Value.Split("|");
                        int _filterId2 = 0;
                        if (int.TryParse(aFilter[0], out _filterId2))
                        {
                            _pref.Context.Filters.AddOrUpdateValue(_tab, new FilterSel(_filterId2, aFilter[1]), true);
                            _pref.Context.Paging.resetInfo();
                        }
                        break;

                    case "canceladvfilter":
                        //désactivation du filtre en mode liste
                        //TODO - Cas des filtres spécifiques
                        if (_pref.Context.Filters.ContainsKey(_tab))
                        {
                            _pref.Context.Filters.Remove(_tab);
                            _pref.Context.Paging.resetInfo();
                        }
                        break;

                    case "charindex":
                        if (keyValue.Value.Length <= 1)
                        {
                            pref.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.CHARINDEX, keyValue.Value));
                            _pref.Context.Paging.resetInfo();
                        }
                        break;

                    case "selectid":
                        pref.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.SELECTID, keyValue.Value));
                        if (string.IsNullOrEmpty(keyValue.Value))
                        {
                            //initialise col width
                        }
                        _pref.Context.Paging.resetInfo();

                        break;

                    case "filterexpress":
                        // Colonnes qui stockent les filtres express
                        ePrefConst.PREF_PREF prefCol;
                        ePrefConst.PREF_PREF prefOp;
                        ePrefConst.PREF_PREF prefValue;

                        try
                        {
                            // Chargement des nouveaux param
                            ExpressFilterParam newParams = ExpressFilterParam.LoadNewParams(keyValue.Value);

                            if (newParams.Option.Equals("addfromfilter"))
                            {
                                prefCol = ePrefConst.PREF_PREF.BKMFILTERCOL_100;
                                prefOp = ePrefConst.PREF_PREF.BKMFILTEROP_100;
                                prefValue = ePrefConst.PREF_PREF.BKMFILTERVALUE_100;

                                // Inutile de recuperer les anciennes valeurs lors de l'ajout depuis un filtre car les filtres colonnes n'y sont pas cumulables
                                colFiltOpts = new ColFilterOptions();
                            }
                            else
                            {
                                prefCol = ePrefConst.PREF_PREF.BKMFILTERCOL;
                                prefOp = ePrefConst.PREF_PREF.BKMFILTEROP;
                                prefValue = ePrefConst.PREF_PREF.BKMFILTERVALUE;

                                if (newParams.CancelAll)
                                {
                                    // On vide tous les paramètres
                                    colFiltOpts = new ColFilterOptions();
                                }
                                else
                                {
                                    // Récup des anciennes valeurs
                                    ColFilterOptionsOldFormat colOptsOldFormat = ColFilterOptionsOldFormat.GetNew(
                                            _pref.GetPref(_tab, prefCol),
                                            _pref.GetPref(_tab, prefOp),
                                            _pref.GetPref(_tab, prefValue));
                                    colFiltOpts = colOptsOldFormat.GetOptions();

                                    // Fusion des nouveaux avec les anciens
                                    newParams.ApplyNewParam(colFiltOpts);
                                }
                            }

                            ColFilterOptionsOldFormat newColOptsOldFormat = ColFilterOptionsOldFormat.GetNew(colFiltOpts.Options);

                            _pref.Context.Paging.resetInfo();
                            pref.Add(new SetParam<ePrefConst.PREF_PREF>(prefCol, newColOptsOldFormat.FilterCol));
                            pref.Add(new SetParam<ePrefConst.PREF_PREF>(prefOp, newColOptsOldFormat.FilterOp));
                            pref.Add(new SetParam<ePrefConst.PREF_PREF>(prefValue, newColOptsOldFormat.FilterValue));
                        }
                        catch (ArgumentException)
                        {
                            // Dans le cas d'un argument invalide, on n'effectue aucune mise à jour en db
                            // Jusqu'ici on ne remonte pas se genre d'anomalie, du coup je ne remonte pas l'exception
                        }
                        break;

                    case "menuuserid":
                        pref.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.MENUUSERID, keyValue.Value));
                        _pref.Context.Paging.resetInfo();
                        break;

                    case "menuuserenabled":
                        pref.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.MENUUSERENABLED, keyValue.Value));
                        _pref.Context.Paging.resetInfo();
                        break;

                    case "cancelmarkedfile":
                        if (_pref.Context.MarkedFiles.ContainsKey(_tab))
                        {
                            _pref.Context.MarkedFiles.Remove(_tab);
                            _pref.Context.Paging.resetInfo();
                        }
                        break;
                    case "calendardate":
                        pref.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.CALENDARDATE, keyValue.Value));
                        break;

                    case "calendarcol":
                        pref.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.CALENDARCOL, keyValue.Value));
                        break;

                    case "viewmode":
                        pref.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.VIEWMODE, keyValue.Value));
                        break;

                    case "bkmorder":
                        //Si aucun signet n'est sélectionné, il faut passé la valeur "-1".
                        // valeur vide signifie reprendre les valeurs par défaut                  
                        pref.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.BKMORDER, (keyValue.Value.Length == 0 ? "-1" : keyValue.Value)));
                        break;

                    case "activebkm":
                        pref.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.ACTIVEBKM, keyValue.Value));
                        break;

                    case "clearalltabfilter":
                        _pref.ClearTabFilter(_tab);
                        break;

                    case "listselcol":

                        pref.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.LISTSELCOL, keyValue.Value));
                        break;

                    case "listselwidth":



                        try
                        {
                            ListColSizeParam newParams = ListColSizeParam.LoadNewParams(keyValue.Value);

                            HashSet<int> listCol = new HashSet<int>(_pref.GetPref(_tab, ePrefConst.PREF_PREF.LISTSELCOL).ConvertToListInt(";"));

                            // Ajout des colonnes a ne jamais netoyer (liaisons, rubriques 01, etc.)
                            HashSet<int> listColAndColFixed = new HashSet<int>();
                            if (_mainField != 0 && !listCol.Contains(_mainField))
                                listColAndColFixed.Add(_mainField);
                            listColAndColFixed.UnionWith(listCol);

                            // Charge les valeurs actuelles en db
                            ColWidthOptionsOldFormat colWidOptsOldFormat = ColWidthOptionsOldFormat.GetNew(
                                string.Join(";", listColAndColFixed),
                                _pref.GetPref(_tab, ePrefConst.PREF_PREF.LISTSELCOLWIDTH));

                            ColWidthOptions colWidOpts = colWidOptsOldFormat.GetOptions();

                            // Fusion des nouveaux avec les anciens
                            newParams.ChangeWidth(colWidOpts);

                            colWidOptsOldFormat = ColWidthOptionsOldFormat.GetNew(colWidOpts.Options);

                            // Verifie la taille du champ
                            eLibTools.CheckFieldSize(dal, "PREF", ePrefConst.PREF_PREF.LISTSELCOLWIDTH.ToString(), colWidOptsOldFormat.Width.Length, true);

                            pref.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.LISTSELCOLWIDTH, colWidOptsOldFormat.Width));
                        }
                        catch (ArgumentException)
                        {
                            // Dans le cas d'un argument invalide, on n'effectue aucune mise à jour en db
                            // Jusqu'ici on ne remonte pas se genre d'anomalie, du coup je ne remonte pas l'exception
                        }

                        break;

                    case "listselsort":
                        pref.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.LISTSELSORT, keyValue.Value));
                        break;

                    case "listselorder":
                        pref.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.LISTSELORDER, keyValue.Value));
                        break;
                }
            }

            if (pref.Count > 0)
                success = _pref.SetPref(_tab, pref);

            return success;
        }

        /// <summary>
        /// Met à jour les preférences du profile de l'utilisateur
        /// Update user's profile preferences
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        private bool UpdateUserProfileAllPrefs(int _usrSrcId, eudoDAL dal, out string error)
        {
            error = string.Empty;

            try {
                //Si le profil utilisateur est le même, on ne modifie pas
                if (_usrSrcId != _pref.UserId)
                {
                    List<int> usrList = new List<int>();
                    usrList.Add(_pref.UserId);

                    //Copie les préférences sources d'un utilisateur vers l'utilisateur cible
                    //Copy user's source profile preferences to the target user 
                    eAdminAccessPref.LaunchCopyPref(_pref, dal, _usrSrcId, usrList);

                    //Met à jour les préférences de profile utilisateurs
                    //Update user's profile preferences
                    List<string> sUsrList = new List<string>();
                    sUsrList.Add(_pref.UserId.ToString());

                    eAdminAccessPref.UpdateUserProfilePrefs(_pref, _usrSrcId, sUsrList);
                }
            }
            catch (ArgumentException)
            {
                // Dans le cas d'un argument invalide, on n'effectue aucune mise à jour en db
                // Jusqu'ici on ne remonte pas se genre d'anomalie, du coup je ne remonte pas l'exception
            }

            return true;
        }

        /// <summary>
        /// Met à jour les pref de signet
        /// </summary>
        /// <param name="dal">connexion sql ouverte</param>
        /// <param name="error"></param>
        /// <returns></returns>
        private bool UpdateBkmPref(eudoDAL dal, out string error)
        {
            error = string.Empty;

            ISet<int> listCol;
            HashSet<int> listColAndColFixed;

            string dbContent;
            ColFilterOptions colFiltOpts;
            ColFilterOptionsOldFormat colFiltOptsOldFormat;
            ColWidthOptions colWidOpts;
            ColWidthOptionsOldFormat colWidOptsOldFormat;

            // Param spécifique aux signets
            int bkm = _requestTools.GetRequestFormKeyI("bkm") ?? 0;
            // Charge les préférences du signet
            eBkmPref daoBkmPref = null;
            if (bkm % 100 == AllField.ATTACHMENT.GetHashCode())
                daoBkmPref = new eBkmPref(_pref, _tab, TableType.PJ.GetHashCode());
            else
                daoBkmPref = new eBkmPref(_pref, _tab, bkm);

            // On complète les informations de paramétres
            XmlAttribute attBkm = _xmlResult.CreateAttribute("bkm");
            attBkm.Value = bkm.ToString();
            _paramNode.Attributes.Append(attBkm);

            List<SetParam<ePrefConst.PREF_BKMPREF>> prefBkm = new List<SetParam<ePrefConst.PREF_BKMPREF>>();

            foreach (KeyValuePair<string, string> keyValue in _prefDic)
            {
                string formValue = keyValue.Value;

                switch (keyValue.Key)
                {
                    case "bkmhisto":
                        string histoLastValue = daoBkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMHISTO);
                        string histoNewValue = (histoLastValue == "1" ? string.Empty : "1");        // On inverse l'ancienne valeur
                        prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.BKMHISTO, histoNewValue));
                        break;

                    case "listsort":       // colonne triée
                        prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.BKMSORT, keyValue.Value));
                        break;

                    case "listorder":       // sens du tri ASC = 1 et DESC = NULL
                        // On met chaine vide pour le 0 pour avoir NULL en base
                        string newOrder = (keyValue.Value == "0" ? string.Empty : keyValue.Value);
                        prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.BKMORDER, newOrder));
                        break;

                    case "bkmcol":
                        #region colwidth - Chargement des anciennes valeurs
                        listCol = new HashSet<int>(daoBkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMCOL).ConvertToListInt(";"));

                        // Ajout des colonnes a ne jamais netoyer (liaisons, rubriques 01, etc.)
                        listColAndColFixed = new HashSet<int>();
                        if (_mainField != 0 && !listCol.Contains(_mainField))
                            listColAndColFixed.Add(_mainField);
                        listColAndColFixed.UnionWith(listCol);

                        // Charge les valeurs actuelles en db
                        colWidOptsOldFormat = ColWidthOptionsOldFormat.GetNew(
                            string.Join(";", listColAndColFixed),
                            daoBkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMCOLWIDTH));
                        ColWidthOptions colWidOpts_Old = colWidOptsOldFormat.GetOptions();
                        #endregion

                        listCol = new HashSet<int>(formValue.ConvertToListInt(";"));

                        // Ajout des colonnes a ne jamais netoyer (liaisons, rubriques 01, etc.)
                        listColAndColFixed = new HashSet<int>();
                        if (_mainField != 0 && !listCol.Contains(_mainField))
                            listColAndColFixed.Add(_mainField);
                        listColAndColFixed.UnionWith(listCol);

                        #region colwidth
                        colWidOptsOldFormat = ColWidthOptionsOldFormat.GetNew(
                            string.Join(";", listColAndColFixed), string.Empty);
                        colWidOpts = colWidOptsOldFormat.GetOptions();

                        colWidOpts.RestaureWidths(colWidOpts_Old);

                        colWidOptsOldFormat = ColWidthOptionsOldFormat.GetNew(colWidOpts.Options);

                        // Verifie la taille du champ
                        eLibTools.CheckFieldSize(dal, "BKMPREF", ePrefConst.PREF_BKMPREF.BKMCOLWIDTH.ToString(), colWidOptsOldFormat.Width.Length, true);
                        prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.BKMCOLWIDTH, colWidOptsOldFormat.Width));
                        #endregion

                        #region expressfilter
                        // On recupère les valeurs existantes en base
                        colFiltOptsOldFormat = ColFilterOptionsOldFormat.GetNew(
                                    daoBkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMFILTERCOL),
                                    daoBkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMFILTEROP),
                                    daoBkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMFILTERVALUE));
                        colFiltOpts = colFiltOptsOldFormat.GetOptions();

                        // On supprime les valeurs non utile
                        colFiltOpts.ClearNotOptionInList(listColAndColFixed);

                        colFiltOptsOldFormat = ColFilterOptionsOldFormat.GetNew(colFiltOpts.Options);

                        // Filtre express
                        prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.BKMFILTERCOL, colFiltOptsOldFormat.FilterCol));
                        prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.BKMFILTEROP, colFiltOptsOldFormat.FilterOp));
                        prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.BKMFILTERVALUE, colFiltOptsOldFormat.FilterValue));
                        #endregion

                        dbContent = eLibTools.Join(";", listCol);
                        // Verifie la taille du champ
                        eLibTools.CheckFieldSize(dal, "BKMPREF", ePrefConst.PREF_BKMPREF.BKMCOL.ToString(), dbContent.Length, true);
                        prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.BKMCOL, dbContent));
                        break;

                    case "listwidth":
                        try
                        {
                            ListColSizeParam newParams = ListColSizeParam.LoadNewParams(formValue);

                            listCol = new HashSet<int>(daoBkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMCOL).ConvertToListInt(";"));

                            // Ajout des colonnes a ne jamais netoyer (liaisons, rubriques 01, etc.)
                            listColAndColFixed = new HashSet<int>();
                            if (_mainField != 0 && !listCol.Contains(_mainField))
                                listColAndColFixed.Add(_mainField);
                            listColAndColFixed.UnionWith(listCol);

                            // Charge les valeurs actuelles en db
                            colWidOptsOldFormat = ColWidthOptionsOldFormat.GetNew(
                                string.Join(";", listColAndColFixed),
                                daoBkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMCOLWIDTH));
                            colWidOpts = colWidOptsOldFormat.GetOptions();

                            // Fusion des nouveaux avec les anciens
                            newParams.ChangeWidth(colWidOpts);

                            colWidOptsOldFormat = ColWidthOptionsOldFormat.GetNew(colWidOpts.Options);

                            // Verifie la taille du champ
                            eLibTools.CheckFieldSize(dal, "BKMPREF", ePrefConst.PREF_BKMPREF.BKMCOLWIDTH.ToString(), colWidOptsOldFormat.Width.Length, true);
                            prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.BKMCOLWIDTH, colWidOptsOldFormat.Width));
                        }
                        catch (ArgumentException)
                        {
                            // Dans le cas d'un argument invalide, on n'effectue aucune mise à jour en db
                            // Jusqu'ici on ne remonte pas se genre d'anomalie, du coup je ne remonte pas l'exception
                        }
                        break;

                    case "listmove":
                        try
                        {
                            ListColMoveParam newParams = ListColMoveParam.LoadNewParams(formValue);

                            listCol = new HashSet<int>(daoBkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMCOL).ConvertToListInt(";"));

                            // Ajout des colonnes a ne jamais netoyer (liaisons, rubriques 01, etc.)
                            listColAndColFixed = new HashSet<int>();
                            if (_mainField != 0 && !listCol.Contains(_mainField))
                                listColAndColFixed.Add(_mainField);
                            listColAndColFixed.UnionWith(listCol);

                            // Charge les valeurs actuelles en db
                            colWidOptsOldFormat = ColWidthOptionsOldFormat.GetNew(
                                string.Join(";", listColAndColFixed),
                                daoBkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMCOLWIDTH));
                            colWidOpts = colWidOptsOldFormat.GetOptions();

                            // Fusion des nouveaux avec les anciens
                            newParams.ApplyNewParam(colWidOpts);

                            colWidOptsOldFormat = ColWidthOptionsOldFormat.GetNew(colWidOpts.Options);

                            // ListCol
                            eLibTools.CheckFieldSize(dal, "BKMPREF", ePrefConst.PREF_BKMPREF.BKMCOL.ToString(), colWidOptsOldFormat.ListCol.Length, true);
                            prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.BKMCOL, colWidOptsOldFormat.ListCol));

                            // ListColWidth
                            eLibTools.CheckFieldSize(dal, "BKMPREF", ePrefConst.PREF_BKMPREF.BKMCOLWIDTH.ToString(), colWidOptsOldFormat.Width.Length, true);
                            prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.BKMCOLWIDTH, colWidOptsOldFormat.Width));
                        }
                        catch (ArgumentException)
                        {
                            // Dans le cas d'un argument invalide, on n'effectue aucune mise à jour en db
                            // Jusqu'ici on ne remonte pas se genre d'anomalie, du coup je ne remonte pas l'exception
                        }
                        break;

                    case "filterexpress":
                        try
                        {
                            // Chargement des nouveaux param
                            ExpressFilterParam newParams = ExpressFilterParam.LoadNewParams(formValue);

                            if (newParams.CancelAll)
                            {
                                // On vide tous les paramètres
                                colFiltOpts = new ColFilterOptions();
                            }
                            else
                            {
                                // Récup des anciennes valeurs
                                colFiltOptsOldFormat = ColFilterOptionsOldFormat.GetNew(
                                    daoBkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMFILTERCOL),
                                    daoBkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMFILTEROP),
                                    daoBkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMFILTERVALUE));
                                colFiltOpts = colFiltOptsOldFormat.GetOptions();

                                // Fusion des nouveaux avec les anciens
                                newParams.ApplyNewParam(colFiltOpts);
                            }

                            colFiltOptsOldFormat = ColFilterOptionsOldFormat.GetNew(colFiltOpts.Options);

                            prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.BKMFILTERCOL, colFiltOptsOldFormat.FilterCol));
                            prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.BKMFILTEROP, colFiltOptsOldFormat.FilterOp));
                            prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.BKMFILTERVALUE, colFiltOptsOldFormat.FilterValue));
                        }
                        catch (ArgumentException)
                        {
                            // Dans le cas d'un argument invalide, on n'effectue aucune mise à jour en db
                            // Jusqu'ici on ne remonte pas se genre d'anomalie, du coup je ne remonte pas l'exception
                        }
                        break;

                    case "viewmode":
                        prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.VIEWMODE, keyValue.Value));
                        break;
                }
            }

            if (prefBkm.Count > 0)
                daoBkmPref.SetBkmPref(prefBkm);

            return true;
        }

        /// <summary>
        /// Met à jour la selection de l'utilisateur
        /// </summary>
        /// <param name="dal">connexion sql ouverte</param>
        /// <param name="error"></param>
        /// <returns></returns>
        private bool UpdateSelection(eudoDAL dal, out string error)
        {
            error = string.Empty;
            bool bDoResetContext = false;
            bool success = true;

            ISet<int> listCol;
            HashSet<int> listColAndColFixed;

            string dbContent;
            ColFilterOptions colFiltOpts;
            ColFilterOptionsOldFormat colOptsOldFormat;
            ColWidthOptions colWidOpts;
            ColWidthOptionsOldFormat colWidOptsOldFormat;

            string[] formValueTmp;

            Dictionary<ePrefConst.PREF_SELECTION, string> currentPref;
            List<SetParam<ePrefConst.PREF_PREF>> pref = new List<SetParam<ePrefConst.PREF_PREF>>();
            List<SetParam<ePrefConst.PREF_SELECTION>> prefSel = new List<SetParam<ePrefConst.PREF_SELECTION>>();

            foreach (KeyValuePair<string, string> keyValue in _prefDic)
            {
                string formName = keyValue.Key;
                string formValue = keyValue.Value;

                switch (formName)
                {
                    case "histo":
                        bDoResetContext = true;
                        string histoLastValue = _pref.GetPref(_tab, ePrefConst.PREF_PREF.HISTO);
                        string histoNewValue = (histoLastValue == "1" ? string.Empty : "1");        // On inverse l'ancienne valeur
                        pref.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.HISTO, histoNewValue));
                        break;

                    case "listsort":
                        // colonne triée
                        prefSel.Add(new SetParam<ePrefConst.PREF_SELECTION>(ePrefConst.PREF_SELECTION.LISTSORT, formValue));
                        break;

                    case "listorder":
                        // sens du tri ASC = 1 et DESC = NULL
                        // On met chaine vide pour le 0 pour avoir NULL en base
                        string newOrder = (formValue == "0" ? string.Empty : formValue);
                        prefSel.Add(new SetParam<ePrefConst.PREF_SELECTION>(ePrefConst.PREF_SELECTION.LISTORDER, newOrder));
                        break;

                    case "listcol":
                        bDoResetContext = true;

                        // Récupération du SelectId
                        formValueTmp = formValue.Split(";|;");
                        formValue = formValueTmp[1];

                        int selectId = 0;
                        int.TryParse(formValueTmp[0], out selectId);
                        if (!_pref.GetPref(_tab, ePrefConst.PREF_PREF.SELECTID).Equals(selectId.ToString()))
                            _pref.SetPrefScalar(_tab, ePrefConst.PREF_PREF.SELECTID, selectId.ToString());

                        #region colwidth - Chargement des anciennes valeurs
                        // Récup des anciennes valeurs
                        currentPref = _pref.GetSelection(_tab,
                            new ePrefConst.PREF_SELECTION[] { ePrefConst.PREF_SELECTION.LISTCOL, ePrefConst.PREF_SELECTION.LISTCOLWIDTH });

                        // Si pas de pref trouvé, on simule un pref vide pour une création
                        if (currentPref.Count <= 0)
                        {
                            currentPref.Add(ePrefConst.PREF_SELECTION.LISTCOL, string.Empty);
                            currentPref.Add(ePrefConst.PREF_SELECTION.LISTCOLWIDTH, string.Empty);
                        }

                        listCol = new HashSet<int>(currentPref[ePrefConst.PREF_SELECTION.LISTCOL].ConvertToListInt(";"));

                        // Ajout des colonnes a ne jamais netoyer (liaisons, rubriques 01, etc.)
                        listColAndColFixed = new HashSet<int>();
                        if (_mainField != 0 && !listCol.Contains(_mainField))
                            listColAndColFixed.Add(_mainField);
                        listColAndColFixed.UnionWith(listCol);

                        // Charge les valeurs actuelles en db
                        colWidOptsOldFormat = ColWidthOptionsOldFormat.GetNew(
                            string.Join(";", listColAndColFixed),
                            currentPref[ePrefConst.PREF_SELECTION.LISTCOLWIDTH]);
                        ColWidthOptions colWidOpts_Old = colWidOptsOldFormat.GetOptions();
                        #endregion

                        listCol = new HashSet<int>(formValue.ConvertToListInt(";"));

                        // Ajout des colonnes a ne jamais netoyer (liaisons, rubriques 01, etc.)
                        listColAndColFixed = new HashSet<int>();
                        if (_mainField != 0 && !listCol.Contains(_mainField))
                            listColAndColFixed.Add(_mainField);
                        listColAndColFixed.UnionWith(listCol);

                        #region colwidth
                        colWidOptsOldFormat = ColWidthOptionsOldFormat.GetNew(
                            string.Join(";", listColAndColFixed), string.Empty);
                        colWidOpts = colWidOptsOldFormat.GetOptions();

                        colWidOpts.RestaureWidths(colWidOpts_Old);

                        colWidOptsOldFormat = ColWidthOptionsOldFormat.GetNew(colWidOpts.Options);

                        // Verifie la taille du champ
                        eLibTools.CheckFieldSize(dal, "SELECTION", ePrefConst.PREF_SELECTION.LISTCOLWIDTH.ToString(), colWidOptsOldFormat.Width.Length, true);
                        prefSel.Add(new SetParam<ePrefConst.PREF_SELECTION>(ePrefConst.PREF_SELECTION.LISTCOLWIDTH, colWidOptsOldFormat.Width));
                        #endregion

                        #region expressfilter
                        // On recupère les valeurs existantes en base
                        colOptsOldFormat = ColFilterOptionsOldFormat.GetNew(
                                    _pref.GetPref(_tab, ePrefConst.PREF_PREF.BKMFILTERCOL),
                                    _pref.GetPref(_tab, ePrefConst.PREF_PREF.BKMFILTEROP),
                                    _pref.GetPref(_tab, ePrefConst.PREF_PREF.BKMFILTERVALUE));
                        colFiltOpts = colOptsOldFormat.GetOptions();

                        // On supprime les valeurs non utile
                        colFiltOpts.ClearNotOptionInList(listColAndColFixed);

                        colOptsOldFormat = ColFilterOptionsOldFormat.GetNew(colFiltOpts.Options);

                        // Filtre express
                        pref.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.BKMFILTERCOL, colOptsOldFormat.FilterCol));
                        pref.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.BKMFILTEROP, colOptsOldFormat.FilterOp));
                        pref.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.BKMFILTERVALUE, colOptsOldFormat.FilterValue));
                        #endregion

                        dbContent = eLibTools.Join(";", listCol);
                        // Verifie la taille du champ
                        eLibTools.CheckFieldSize(dal, "SELECTION", ePrefConst.PREF_SELECTION.LISTCOL.ToString(), dbContent.Length, true);
                        prefSel.Add(new SetParam<ePrefConst.PREF_SELECTION>(ePrefConst.PREF_SELECTION.LISTCOL, dbContent));
                        break;

                    case "listwidth":
                        try
                        {
                            ListColSizeParam newParams = ListColSizeParam.LoadNewParams(formValue);

                            // Récup des anciennes valeurs
                            currentPref = _pref.GetSelection(_tab,
                                new ePrefConst.PREF_SELECTION[] { ePrefConst.PREF_SELECTION.LISTCOL, ePrefConst.PREF_SELECTION.LISTCOLWIDTH });

                            // Si pas de pref trouvé, on simule un pref vide pour une création
                            if (currentPref.Count <= 0)
                            {
                                currentPref.Add(ePrefConst.PREF_SELECTION.LISTCOL, string.Empty);
                                currentPref.Add(ePrefConst.PREF_SELECTION.LISTCOLWIDTH, string.Empty);
                            }

                            listCol = new HashSet<int>(currentPref[ePrefConst.PREF_SELECTION.LISTCOL].ConvertToListInt(";"));

                            // Ajout des colonnes a ne jamais netoyer (liaisons, rubriques 01, etc.)
                            listColAndColFixed = new HashSet<int>();
                            if (_mainField != 0 && !listCol.Contains(_mainField))
                                listColAndColFixed.Add(_mainField);
                            listColAndColFixed.UnionWith(listCol);

                            // Charge les valeurs actuelles en db
                            colWidOptsOldFormat = ColWidthOptionsOldFormat.GetNew(
                                string.Join(";", listColAndColFixed),
                                currentPref[ePrefConst.PREF_SELECTION.LISTCOLWIDTH]);
                            colWidOpts = colWidOptsOldFormat.GetOptions();

                            // Fusion des nouveaux avec les anciens
                            newParams.ChangeWidth(colWidOpts);

                            colWidOptsOldFormat = ColWidthOptionsOldFormat.GetNew(colWidOpts.Options);

                            // Verifie la taille du champ
                            eLibTools.CheckFieldSize(dal, "SELECTION", ePrefConst.PREF_SELECTION.LISTCOLWIDTH.ToString(), colWidOptsOldFormat.Width.Length, true);
                            prefSel.Add(new SetParam<ePrefConst.PREF_SELECTION>(ePrefConst.PREF_SELECTION.LISTCOLWIDTH, colWidOptsOldFormat.Width));
                        }
                        catch (ArgumentException)
                        {
                            // Dans le cas d'un argument invalide, on n'effectue aucune mise à jour en db
                            // Jusqu'ici on ne remonte pas se genre d'anomalie, du coup je ne remonte pas l'exception
                        }
                        break;

                    case "listmove":
                        try
                        {
                            ListColMoveParam newParams = ListColMoveParam.LoadNewParams(formValue);

                            // Récup des anciennes valeurs
                            currentPref = _pref.GetSelection(_tab,
                                new ePrefConst.PREF_SELECTION[] { ePrefConst.PREF_SELECTION.LISTCOL, ePrefConst.PREF_SELECTION.LISTCOLWIDTH });

                            // Si pas de pref trouvé, on fait rien
                            if (currentPref.Count <= 0)
                                break;

                            listCol = new HashSet<int>(currentPref[ePrefConst.PREF_SELECTION.LISTCOL].ConvertToListInt(";"));

                            // Ajout des colonnes a ne jamais netoyer (liaisons, rubriques 01, etc.)
                            listColAndColFixed = new HashSet<int>();
                            if (_mainField != 0 && !listCol.Contains(_mainField))
                                listColAndColFixed.Add(_mainField);
                            listColAndColFixed.UnionWith(listCol);

                            colWidOptsOldFormat = ColWidthOptionsOldFormat.GetNew(
                                string.Join(";", listColAndColFixed),
                                currentPref[ePrefConst.PREF_SELECTION.LISTCOLWIDTH]);
                            colWidOpts = colWidOptsOldFormat.GetOptions();

                            // Fusion des nouveaux avec les anciens
                            newParams.ApplyNewParam(colWidOpts);

                            colWidOptsOldFormat = ColWidthOptionsOldFormat.GetNew(colWidOpts.Options);

                            // ListCol
                            eLibTools.CheckFieldSize(dal, "SELECTION", ePrefConst.PREF_SELECTION.LISTCOL.ToString(), colWidOptsOldFormat.ListCol.Length, true);
                            prefSel.Add(new SetParam<ePrefConst.PREF_SELECTION>(ePrefConst.PREF_SELECTION.LISTCOL, colWidOptsOldFormat.ListCol));

                            // ListColWidth
                            eLibTools.CheckFieldSize(dal, "SELECTION", ePrefConst.PREF_SELECTION.LISTCOLWIDTH.ToString(), colWidOptsOldFormat.Width.Length, true);
                            prefSel.Add(new SetParam<ePrefConst.PREF_SELECTION>(ePrefConst.PREF_SELECTION.LISTCOLWIDTH, colWidOptsOldFormat.Width));
                        }
                        catch (ArgumentException)
                        {
                            // Dans le cas d'un argument invalide, on n'effectue aucune mise à jour en db
                            // Jusqu'ici on ne remonte pas se genre d'anomalie, du coup je ne remonte pas l'exception
                        }
                        break;
                }
            }

            if (bDoResetContext)
            {
                _pref.Context.Paging.resetInfo();
            }

            // MODE DEBUG
            if (true)
            {
                XmlAttribute att = null;

                foreach (SetParam<ePrefConst.PREF_PREF> p in pref)
                {
                    att = _xmlResult.CreateAttribute(p.Option.ToString());
                    att.Value = p.Value;
                    _infosNode.Attributes.Append(att);
                }

                foreach (SetParam<ePrefConst.PREF_SELECTION> p in prefSel)
                {
                    att = _xmlResult.CreateAttribute(p.Option.ToString());
                    att.Value = p.Value;
                    _infosNode.Attributes.Append(att);
                }
            }

            if (pref.Count > 0)
                success = _pref.SetPref(_tab, pref);

            if (success && prefSel.Count > 0)
                success = _pref.SetSelection(_tab, prefSel);

            return success;
        }

        /// <summary>
        /// Met à jour les pref de l'ecran de recherche
        /// </summary>
        /// <param name="dal">connexion sql ouverte</param>
        /// <param name="error"></param>
        /// <returns></returns>
        private bool UpdateColsPref(eudoDAL dal, out string error)
        {
            error = string.Empty;

            string dbContent;
            ColWidthOptions colWidOpts;
            ColFilterOptions colFiltOpts;

            eColsPref daoColsPref;
            bool defaultPref = false;

            // Lance la mise à jour
            switch (_type.ToUpper())
            {
                case "USER_COLSPREF":
                    bool bDeleteMode = _requestTools.GetRequestFormKeyS("deletemode") == "1" || _requestTools.GetRequestFormKeyS("deletemode").ToLower() == "true";
                    bool bTargetFile = _requestTools.GetRequestFormKeyS("targetmode") == "1";

                    if (bTargetFile)
                        daoColsPref = new eColsPref(_pref, _tab, bDeleteMode ? ColPrefType.TARGETDELETE : ColPrefType.TARGETADD);
                    else
                        daoColsPref = new eColsPref(_pref, _tab, bDeleteMode ? ColPrefType.INVITDELETE : ColPrefType.INVITADD);

                    break;
                case "USER_FINDER_PREF":
                    daoColsPref = new eColsPref(_pref, _tab, ColPrefType.FINDERPREF);
                    break;
                case "USER_LISTWIDGET_PREF":
                    // On passe ici _pref.AdminMode pour définir si on est sur une pref par defaut
                    daoColsPref = new eColsPref(_pref, _tab, ColPrefType.LISTWIDGET);
                    if (_pref.AdminMode)
                        defaultPref = true;
                    break;
                default:
                    throw new NotImplementedException("préférences UpdateColsPref : " + _type + " non gérée ");
            }

            List<SetParam<eLibConst.PREF_COLSPREF>> prefColsPref = new List<SetParam<eLibConst.PREF_COLSPREF>>();

            foreach (KeyValuePair<string, string> keyValue in _prefDic)
            {
                string formValue = keyValue.Value;

                switch (keyValue.Key)
                {

                    case "listsort":       // colonne triée
                    case "listselsort":       // colonne triée
                        prefColsPref.Add(new SetParam<eLibConst.PREF_COLSPREF>(eLibConst.PREF_COLSPREF.Sort, keyValue.Value));
                        break;

                    case "listselorder":       // sens du tri ASC = 1 et DESC = NULL
                    case "listorder":       // sens du tri ASC = 1 et DESC = NULL
                        // On met chaine vide pour le 0 pour avoir NULL en base
                        string newOrder = (keyValue.Value == "0" ? "0" : keyValue.Value);
                        prefColsPref.Add(new SetParam<eLibConst.PREF_COLSPREF>(eLibConst.PREF_COLSPREF.SortOrder, newOrder));
                        break;
                    case "listselcol":
                    case "listcol":
                        #region Colonne  
                        ISet<int> newlistSelCol = new HashSet<int>(formValue.ConvertToListInt(";"));

                        // Ajout des colonnes a ne jamais netoyer (liaisons, rubriques 01, etc.)
                        HashSet<int> listSelColAndColFixed = UpdateFinderPref_AddColFixed(dal, _tab, newlistSelCol);

                        #region colwidth
                        // On recupère les valeurs existantes en base
                        if (defaultPref)
                            dbContent = daoColsPref.GetColsDefaultPref(eLibConst.PREF_COLSPREF.ColWidth);
                        else
                            dbContent = daoColsPref.GetColsPref(eLibConst.PREF_COLSPREF.ColWidth);
                        colWidOpts = SerializerTools.JsonDeserialize<ColWidthOptions>(dbContent);
                        // Sinon on par d'une collection vide
                        if (colWidOpts == null)
                            colWidOpts = new ColWidthOptions();

                        // On supprime les valeurs non utile
                        colWidOpts.ClearNotOptionInList(listSelColAndColFixed);

                        // Tailles de colonnes
                        dbContent = string.Empty;
                        if (colWidOpts.Options.Count > 0)
                            dbContent = SerializerTools.JsonSerialize(colWidOpts);
                        eLibTools.CheckFieldSize(dal, "COLSPREF", eLibConst.PREF_COLSPREF.ColWidth.ToString(), dbContent.Length, true);
                        prefColsPref.Add(new SetParam<eLibConst.PREF_COLSPREF>(eLibConst.PREF_COLSPREF.ColWidth, dbContent));
                        #endregion

                        #region expressfilter
                        // On recupère les valeurs existantes en base

                        if (defaultPref)
                            dbContent = daoColsPref.GetColsDefaultPref(eLibConst.PREF_COLSPREF.FilterOptions);
                        else
                            dbContent = daoColsPref.GetColsPref(eLibConst.PREF_COLSPREF.FilterOptions);
                        colFiltOpts = SerializerTools.JsonDeserialize<ColFilterOptions>(dbContent);
                        // Sinon on par d'une collection vide
                        if (colFiltOpts == null)
                            colFiltOpts = new ColFilterOptions();

                        // On supprime les valeurs non utile
                        colFiltOpts.ClearNotOptionInList(listSelColAndColFixed);

                        // Filtre express
                        dbContent = string.Empty;
                        if (colFiltOpts.Options.Count > 0)
                            dbContent = SerializerTools.JsonSerialize(colFiltOpts);
                        eLibTools.CheckFieldSize(dal, "COLSPREF", eLibConst.PREF_COLSPREF.FilterOptions.ToString(), dbContent.Length, true);
                        prefColsPref.Add(new SetParam<eLibConst.PREF_COLSPREF>(eLibConst.PREF_COLSPREF.FilterOptions, dbContent));
                        #endregion

                        // Liste de colonnes
                        eLibTools.CheckFieldSize(dal, "COLSPREF", eLibConst.PREF_COLSPREF.Col.ToString(), formValue.Length, true);
                        prefColsPref.Add(new SetParam<eLibConst.PREF_COLSPREF>(eLibConst.PREF_COLSPREF.Col, formValue));
                        break;

                    #endregion

                    case "listselwidth":
                    case "listwidth":
                        #region Taille des colonnes
                        try
                        {
                            // Charge les param passés
                            ListColSizeParam newParams = ListColSizeParam.LoadNewParams(formValue);

                            // On recupère les valeurs existantes en base
                            if (defaultPref)
                                dbContent = daoColsPref.GetColsDefaultPref(eLibConst.PREF_COLSPREF.ColWidth);
                            else
                                dbContent = daoColsPref.GetColsPref(eLibConst.PREF_COLSPREF.ColWidth);
                            colWidOpts = SerializerTools.JsonDeserialize<ColWidthOptions>(dbContent);
                            // Sinon on par d'une collection vide
                            if (colWidOpts == null)
                                colWidOpts = new ColWidthOptions();

                            // Fusion des nouveaux avec les anciens
                            newParams.ApplyNewParam(colWidOpts);

                            // Tailles de colonnes
                            dbContent = string.Empty;
                            if (colWidOpts.Options.Count > 0)
                                dbContent = SerializerTools.JsonSerialize(colWidOpts);
                            eLibTools.CheckFieldSize(dal, "COLSPREF", eLibConst.PREF_COLSPREF.ColWidth.ToString(), dbContent.Length, true);
                            prefColsPref.Add(new SetParam<eLibConst.PREF_COLSPREF>(eLibConst.PREF_COLSPREF.ColWidth, dbContent));
                        }
                        catch (ArgumentException)
                        {
                            // Dans le cas d'un argument invalide, on n'effectue aucune mise à jour en db
                            // Jusqu'ici on ne remonte pas se genre d'anomalie, du coup je ne remonte pas l'exception
                        }
                        break;
                    #endregion
                    case "wid":
                        prefColsPref.Add(new SetParam<eLibConst.PREF_COLSPREF>(eLibConst.PREF_COLSPREF.XrmWidgetId, keyValue.Value));
                        break;
                    case "filterexpress":
                        try
                        {
                            // Chargement des nouveaux param
                            ExpressFilterParam newParams = ExpressFilterParam.LoadNewParams(formValue);

                            if (newParams.CancelAll)
                            {
                                // On vide tous les paramètres
                                colFiltOpts = new ColFilterOptions();
                            }
                            else
                            {
                                // On recupère les valeurs existantes en base
                                if (defaultPref)
                                    dbContent = daoColsPref.GetColsDefaultPref(eLibConst.PREF_COLSPREF.FilterOptions);
                                else
                                    dbContent = daoColsPref.GetColsPref(eLibConst.PREF_COLSPREF.FilterOptions);
                                colFiltOpts = SerializerTools.JsonDeserialize<ColFilterOptions>(dbContent);
                                // Sinon on par d'une collection vide
                                if (colFiltOpts == null)
                                    colFiltOpts = new ColFilterOptions();

                                // Fusion des nouveaux avec les anciens
                                newParams.ApplyNewParam(colFiltOpts);
                            }

                            dbContent = string.Empty;
                            if (colFiltOpts.Options.Count > 0)
                                dbContent = SerializerTools.JsonSerialize(colFiltOpts);
                            eLibTools.CheckFieldSize(dal, "COLSPREF", eLibConst.PREF_COLSPREF.FilterOptions.ToString(), dbContent.Length, true);
                            prefColsPref.Add(new SetParam<eLibConst.PREF_COLSPREF>(eLibConst.PREF_COLSPREF.FilterOptions, dbContent));
                        }
                        catch (ArgumentException)
                        {
                            // Dans le cas d'un argument invalide, on n'effectue aucune mise à jour en db
                            // Jusqu'ici on ne remonte pas se genre d'anomalie, du coup je ne remonte pas l'exception
                        }
                        break;
                }
            }

            if (prefColsPref.Count > 0)
            {
                if (defaultPref)
                    daoColsPref.SetColsDefaultPref(prefColsPref);
                else
                    daoColsPref.SetColsPref(prefColsPref);
            }

            return true;
        }

        private HashSet<int> UpdateFinderPref_AddColFixed(eudoDAL dal, int tab, IEnumerable<int> listCol)
        {
            string error;

            // Liste des colonnes et des colonnes fixe qui sont parfois affichées ou parfois non pour eviter de supprimer les valeurs de taille
            // Voir méthode Init bloc "Spécificité d'affichage de colonnes par défaut pour un champ de liaison mais pas pour la recherche AVANCéE" de la classe eFinderList
            HashSet<int> listColAndColFixed = new HashSet<int>(listCol);

            // Colonne 01
            listColAndColFixed.Add(tab + 1);

            if (_tab == TableType.PP.GetHashCode())
            {
                listColAndColFixed.Add(401);
                listColAndColFixed.Add(301);
            }
            else if (_tab == TableType.ADR.GetHashCode())
            {
                listColAndColFixed.Add(201);
            }
            else if (_tab == TableType.PM.GetHashCode())
            {
                listColAndColFixed.Add(201);
            }
            else
            {
                TableLite table = new TableLite(_tab);
                table.ExternalLoadInfo(dal, out error);
                if (error.Length != 0)
                    throw new Exception(string.Concat("Chargement de la table ", _tab, " impossible"));

                if (table.InterPP)
                    listColAndColFixed.Add(201);
                if (table.InterPM)
                    listColAndColFixed.Add(301);
            }

            return listColAndColFixed;
        }
    }
}