using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Xml;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm.mgr
{
    /// <className>eCatalogManager</className>
    /// <summary>TODO</summary>
    /// <purpose></purpose>
    /// <authors>KHA</authors>
    /// <date>2011-MM-JJ</date>
    public class eCatalogManager : eEudoManager
    {
        /// <summary>Appelé à l'appel de la page</summary>
        protected override void ProcessManager()
        {
            eCatalog.Operation _operation;
            int _iOp = 0;
            int _iDescId = 0;
            int _iCatType = 0;
            int _iId = 0;
            int _iParentId = -1;
            int _iCatBoundDescid = 0;
            PopupType _catBoundPopup = PopupType.NONE;
            String _sParentValue = String.Empty;
            String _sLabel = String.Empty;
            String _sNewLabel = String.Empty;
            Boolean _bTreeView = false; ;
            String _sData = String.Empty;
            String _sErr = String.Empty;
            eudoDAL _eDAL = eLibTools.GetEudoDAL(_pref);
            XmlDocument _xmlDocReturn = new XmlDocument();
            StringBuilder _sbError = new StringBuilder();
            // valeurs à modifier contenant toutes les langues concernées
            Dictionary<String, String> _diLang = null;
            Boolean _bHidden = false;
            if (!int.TryParse(_context.Request.Form["operation"].ToString(), out _iOp))
            {
                string strBaseError = "Opération de gestion de catalogue mal interprétée";
                string strDevError = strBaseError;
                if (_context.Request.Form["operation"] != null)
                    strDevError = String.Concat(strDevError, " : ", _context.Request.Form["operation"].ToString());

                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 6237),
                    strBaseError,
                    eResApp.GetRes(_pref, 72),
                    strDevError
                );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }

            _operation = (eCatalog.Operation)_iOp;
            if (_context.Request.Form["label"] != null)
                _sLabel = _context.Request.Form["label"];
            if (_context.Request.Form["newlabel"] != null)
                _sNewLabel = _context.Request.Form["newlabel"];

            if (!int.TryParse(_context.Request.Form["parentid"], out _iParentId))
                _iParentId = -1;
            if (_context.Request.Form["parentvalue"] != null)
                _sParentValue = _context.Request.Form["parentvalue"];
            if (!int.TryParse(_context.Request.Form["catbounddescid"], out _iCatBoundDescid))
                _iCatBoundDescid = 0;
            if (_context.Request.Form["catboundpopup"] != null)
                _catBoundPopup = (PopupType)eLibTools.GetNum(_context.Request.Form["catboundpopup"]);

            if (_context.Request.Form["treeview"] != null)
                _bTreeView = bool.Parse(_context.Request.Form["treeview"].ToString());

            //Si les informations descid id ou popuptype sont incorrectes, on met fin au script
            if (!(int.TryParse(_context.Request.Form["descid"], out _iDescId)
                && (int.TryParse(_context.Request.Form["id"], out _iId) || _operation == eCatalog.Operation.Synchro)
                && int.TryParse(_context.Request.Form["pop"], out _iCatType)))
            {
                StringBuilder sbAdditionalDevMsg = new StringBuilder(String.Concat("opération = ", _operation));
                if (_context.Request.Form["descid"] != null)
                    sbAdditionalDevMsg.Append("descid = ").Append(_context.Request.Form["descid"].ToString());
                if (_context.Request.Form["id"] != null)
                    sbAdditionalDevMsg.Append("id = ").Append(_context.Request.Form["id"].ToString());
                if (_context.Request.Form["pop"] != null)
                    sbAdditionalDevMsg.Append("pop = ").Append(_context.Request.Form["pop"].ToString());

                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 6237),
                    eResApp.GetRes(_pref, 2024).Replace(" <PARAM> ", " "),
                    eResApp.GetRes(_pref, 72),
                    String.Concat(eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "descid, id, pop"), " (", sbAdditionalDevMsg.ToString(), ")")
                );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }

            try
            {
                _eDAL.OpenDatabase();
                PopupType catType = (PopupType)_iCatType;

                // On va remplir le dictionnaire avec toutes les valeures qui vont etre modifiées
                if (catType == PopupType.DATA && _context.Request.Form["langUsed"] != null)
                {
                    _diLang = new Dictionary<String, String>();
                    String[] langUsed = _context.Request.Form["langUsed"].ToString().Split(';');
                    String _sToolTip = String.Empty;

                    foreach (String lang in langUsed)
                    {
                        //Bug #60012 - CNA: On teste si la valeur de la langue en cours n'est pas vide, sinon la valeur de catalogue ne s'enregistre pas
                        if ((String.IsNullOrEmpty(_sLabel) && String.Concat("LANG_", lang.PadLeft(2, '0')).ToLower() == _pref.Lang.ToLower() && !String.IsNullOrEmpty(_context.Request.Form[String.Concat("lbl_", lang.PadLeft(2, '0'))]))
                            || String.IsNullOrEmpty(_sNewLabel))
                        {
                            _sNewLabel = _context.Request.Form[String.Concat("lbl_", lang.PadLeft(2, '0'))];
                        }
                        //Libellé
                        _diLang.Add(String.Concat("LANG_", lang.PadLeft(2, '0')), _context.Request.Form[String.Concat("lbl_", lang.PadLeft(2, '0'))]);
                        // Décodage du tooltip
                        _sToolTip = HttpUtility.UrlDecode(_context.Request.Form[String.Concat("tip_", lang.PadLeft(2, '0'))]);
                        _diLang.Add(String.Concat("TIP_LANG_", lang.PadLeft(2, '0')), _sToolTip);
                    }

                    // mode administrateur case désactivée
                    if (_context.Request.Form["hidden"] != null)
                    {
                        _bHidden = (_context.Request.Form["hidden"].ToString().Equals("1") ? true : false);
                    }
                    _diLang.Add("hidden", _bHidden.ToString());

                    if (_context.Request.Form["data"] != null)
                        _sData = _context.Request.Form["data"].ToString();
                }

                //Récupère le parentId
                if (_iParentId <= 0 && !String.IsNullOrEmpty(_sParentValue))
                {
                    if (_iCatBoundDescid > 0)
                    {
                        //Recupère le popupdescid du catBoundDescid
                        int _catBoundPopupDescid = 0;
                        FieldLite fieldInfo = eLibTools.GetFieldInfo(_eDAL, _iCatBoundDescid, FieldLite.Factory());
                        if (fieldInfo != null)
                            _catBoundPopupDescid = fieldInfo.PopupDescId;

                        //Recupère la valeur parente
                        _iParentId = eCatalog.GetCatalogValueID(_pref, _catBoundPopup, _catBoundPopupDescid, _sParentValue, _eDAL, _pref.User);
                    }
                }

                eCatalog.CatalogValue newValue = new eCatalog.CatalogValue();
                eCatalog.CatalogValue originalValue = new eCatalog.CatalogValue();

                originalValue.Id = _iId;

                newValue.Label = _sNewLabel;
                newValue.ParentId = _iParentId;
                newValue.Data = _sData;
                //newValue.IsDisabled = _bHidden;

                eCatalog catalog = null;

                catalog = new eCatalog(_eDAL, _pref, catType, _pref.User, _iDescId, _bTreeView, originalValue, isSnapshot: _pref.IsSnapshot, showHiddenValues: true, bypassNoAutoload: true);

                switch (_operation)
                {
                    case eCatalog.Operation.Insert:
                        if (catalog.AddAllowed)
                        {
                            if (_bTreeView)
                                newValue.ParentId = originalValue.Id;

                            catalog.Insert(newValue, out _sErr, _diLang);
                            _xmlDocReturn = catalog.GetXmlReportDocument(_operation, _sErr, newValue);
                            if (!String.IsNullOrEmpty(_sErr))
                                _sbError.AppendLine(_sErr);
                        }
                        else
                            //_sbError.AppendLine("Ajout non autorisé");
                            _sbError.AppendLine(eResApp.GetRes(_pref, 6433));
                        break;
                    case eCatalog.Operation.Change:
                        if (catalog.UpdateAllowed)
                        {
                            _eDAL.StartTransaction(out _sErr);
                            _xmlDocReturn = catalog.Rename(newValue, originalValue, out _sErr, _diLang);
                        }
                        else
                            //_sErr = "Modification non autorisée";
                            _sErr = eResApp.GetRes(_pref, 6434);

                        if (!String.IsNullOrEmpty(_sErr))
                        {
                            _sbError.AppendLine(_sErr);
                            throw new CatalogException("Une erreur est survenue durant la modification de la valeur de catalogue", _iDescId, new EudoException(_sErr));
                        }
                        break;
                    case eCatalog.Operation.Delete:
                        if (catalog.DeleteAllowed)
                        {
                            _eDAL.StartTransaction(out _sErr);
                            _xmlDocReturn = catalog.Delete(originalValue, out _sErr);
                            if (!String.IsNullOrEmpty(_sErr)) { _sbError.AppendLine(_sErr); }
                        }
                        else
                            //_sErr = "Suppression non autorisée";
                            _sErr = eResApp.GetRes(_pref, 6435);


                        if (!String.IsNullOrEmpty(_sErr))
                            throw new CatalogException("Une erreur est survenue durant la Suppression", _iDescId, new EudoException(_sErr));
                        break;
                    case eCatalog.Operation.Synchro:
                        if (catalog.SynchroAllowed)
                            _xmlDocReturn = catalog.Synchro(false, out _sErr);
                        else
                            //_sErr = "Synchronisation non autorisée";
                            _sErr = eResApp.GetRes(_pref, 6436);


                        if (!String.IsNullOrEmpty(_sErr))
                            _sbError.AppendLine(_sErr);
                        break;
                    case eCatalog.Operation.CountOccurencesOperation:
                        _xmlDocReturn = catalog.SearchNbOccurences(_pref, originalValue, out _sErr);
                        break;
                    default:
                        break;
                }

                if (_sbError.Length > 0)
                    throw new CatalogException(String.Format("Une ou plusieurs erreurs se sont produites pendant l'opération {0}", _operation), _iDescId, new EudoException(_sbError.ToString()));

                if (_eDAL.HasActiveTransaction)
                    _eDAL.CommitTransaction(out _sErr);

            }
            catch (CatalogException XRME)
            {
                _eDAL.RollBackTransaction(out _sErr);
                _sErr = XRME.Message;
                if (!String.IsNullOrEmpty(_sErr))
                    _sbError.AppendLine(_sErr);

                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 6237),
                    XRME.UserMessage ?? _sErr,
                    eResApp.GetRes(_pref, 72),
                    devMsg: String.Concat(
                        XRME.FullDebugMessage
                        , XRME.InnerException != null ? string.Concat(XRME.InnerException.Message, Environment.NewLine, XRME.InnerException.StackTrace) : ""
                    )
                );
            }
            finally
            {
                _eDAL.CloseDatabase();

                LaunchError();

                RenderResult(RequestContentType.XML, delegate () { return _xmlDocReturn.OuterXml; });
            }
        }
    }
}