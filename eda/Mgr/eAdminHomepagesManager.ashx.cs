using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Web;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminHomepagesManager
    /// </summary>
    public class eAdminHomepagesManager : eAdminManager
    {

        /// <summary>
        /// Type de module Administration / Options utilisateur concerné par le menu
        /// </summary>
        eUserOptionsModules.USROPT_MODULE _targetModule = eUserOptionsModules.USROPT_MODULE.UNDEFINED;
        /// <summary>
        /// Résultat des opérations 
        /// </summary>
        JSONReturnHomepages res = new JSONReturnHomepages();
        // A mettre à jour côté JS dans eAdminHomepages.js
        enum HomepageAction
        {
            UNDEFINED = 0,
            LIST = 1,
            UPDATEUSERS = 2,
            UPDATETOOLTIP = 3,
            DELETE = 4,
            CLONE = 5,
            ADD = 6,
            EDIT = 7
        }

        /// <summary>
        /// Gestion de la demande de rendu de la liste des homepages
        /// </summary>
        /// <param name="context"></param>
        protected override void ProcessManager()
        {
            int nWidth = 800;
            int nHeight = 600;
            int nAction, nId = 0;
            HomepageAction action = HomepageAction.LIST;
            String sSortCol = "";
            Int32 iSort = 0;
            Int32 iDentity = 0;
            String prop, value;
            String
                expressMessageName = string.Empty,
                expressMessageContent = string.Empty,
                expressMessageUsers = string.Empty,
                allUser = string.Empty;

            String sError;

            #region Paramètres
            if (_requestTools.AllKeys.Contains("action") && !String.IsNullOrEmpty(_context.Request.Form["action"]))
                if (Int32.TryParse(_context.Request.Form["action"].ToString(), out nAction))
                {
                    action = (HomepageAction)nAction;
                }

            if (_requestTools.AllKeys.Contains("module") && !String.IsNullOrEmpty(_context.Request.Form["module"]))
            {
                int nTargetModule = 0;
                Int32.TryParse(_context.Request.Form["module"].ToString(), out nTargetModule);
                _targetModule = (eUserOptionsModules.USROPT_MODULE)nTargetModule;
            }


            switch (action)
            {
                case HomepageAction.UNDEFINED:
                    ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", "Action non définie.");
                    LaunchError();
                    break;
                case HomepageAction.LIST:
                    #region LISTE

                    if (_requestTools.AllKeys.Contains("h") && !String.IsNullOrEmpty(_context.Request.Form["h"]))
                        if (!Int32.TryParse(_context.Request.Form["h"].ToString(), out nHeight))
                            nHeight = 600;

                    if (_requestTools.AllKeys.Contains("w") && !String.IsNullOrEmpty(_context.Request.Form["w"]))
                        if (!Int32.TryParse(_context.Request.Form["w"].ToString(), out nWidth))
                            nWidth = 800;

                    if (_requestTools.AllKeys.Contains("sortcol") && !String.IsNullOrEmpty(_context.Request.Form["sortcol"]))
                        sSortCol = _context.Request.Form["sortcol"].ToString();
                    if (_requestTools.AllKeys.Contains("sort") && !String.IsNullOrEmpty(_context.Request.Form["sort"]))
                        Int32.TryParse(_context.Request.Form["sort"], out iSort);



                    eAdminRenderer rdr;


                    switch (_targetModule)
                    {
                        case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME_EXPRESS_MESSAGE:
                            rdr = eAdminRendererFactory.CreateAdminHomeExpressMessageRenderer(_pref, nWidth, nHeight, sSortCol, iSort);
                            break;
                        case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME_V7_HOMEPAGES:
                            rdr = eAdminRendererFactory.CreateAdminHomepagesRenderer(_pref, nWidth, nHeight, sSortCol, iSort);
                            break;
                        default:
                            // On affiche les grilles XRM
                            //rdr = eAdminRendererFactory.CreateXrmHomePageListRenderer(_pref, mPage, nRows, nWidth, nHeight);
                            rdr = eAdminRendererFactory.CreateAdminHomepagesRenderer(_pref, nWidth, nHeight, sSortCol, iSort);
                            break;
                    }

                    if (rdr.ErrorMsg.Length > 0)
                    {
                        ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), rdr.ErrorMsg);
                        LaunchError();
                    }
                    else
                    {

                        res.Success = true;
                        res.Html = GetResultHTML(rdr.GetContents());

                        RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
                    }
                    #endregion
                    break;
                case HomepageAction.DELETE:
                case HomepageAction.CLONE:
                case HomepageAction.UPDATEUSERS:
                    #region Gestion d'erreur
                    if (!_requestTools.AllKeys.Contains("id"))
                    {
                        ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", "Paramètres non renseignés.");
                        LaunchError();
                    }
                    else
                    {
                        nId = eLibTools.GetNum(_context.Request.Form["id"].ToString());
                        if (nId <= 0)
                        {
                            ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", "ID incorrect");
                            LaunchError();
                        }
                    }

                    #endregion

                    #region DELETE/UPDATE

                    switch (action)
                    {
                        case HomepageAction.UPDATEUSERS:
                            #region UPDATE
                            if (!_requestTools.AllKeys.Contains("value"))
                            {
                                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", "Paramètres non renseignés.");
                                LaunchError();
                            }
                            else
                            {
                                //prop = _context.Request.Form["prop"];
                                value = _context.Request.Form["value"];
                                eAdminHomepage.SaveUsers(_pref, nId, value, out sError, _targetModule);
                            }
                            #endregion
                            break;

                        case HomepageAction.DELETE:
                            #region DELETE
                            eAdminHomepage.Delete(_pref, nId, _targetModule, out sError);
                            if (!String.IsNullOrEmpty(sError))
                            {
                                ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, "", eResApp.GetRes(_pref, 1805), "", sError);
                                LaunchError();
                            }
                            #endregion
                            break;
                        case HomepageAction.CLONE:
                            #region CLONE
                            eAdminHomepage.Clone(_pref, nId, out sError);
                            if (!String.IsNullOrEmpty(sError))
                            {
                                ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, "", "Erreur de duplication", "", sError);
                                LaunchError();
                            }
                            #endregion
                            break;

                        default:
                            break;
                    }
                    #endregion
                    break;
                case HomepageAction.UPDATETOOLTIP:
                    break;

                case HomepageAction.ADD:
                case HomepageAction.EDIT:
                    #region ADD \EDIT
                    switch (_targetModule)
                    {
                        case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME_EXPRESS_MESSAGE:
                            if (!_requestTools.AllKeys.Contains("newExpressMessageName") || !_requestTools.AllKeys.Contains("newExpressMessageContent"))
                            {
                                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", "Paramètres non renseignés.");
                                LaunchError();
                            }
                            sError = string.Empty;


                            if (_requestTools.AllKeys.Contains("newExpressMessageName") && !String.IsNullOrEmpty(_context.Request.Form["newExpressMessageName"]))
                                expressMessageName = _context.Request.Form["newExpressMessageName"].ToString();

                            if (_requestTools.AllKeys.Contains("newExpressMessageContent") && !String.IsNullOrEmpty(_context.Request.Form["newExpressMessageContent"]))
                                expressMessageContent = _context.Request.Form["newExpressMessageContent"].ToString();

                            if (_requestTools.AllKeys.Contains("newExpressMessageUsers") && !String.IsNullOrEmpty(_context.Request.Form["newExpressMessageUsers"]))
                                expressMessageUsers = _context.Request.Form["newExpressMessageUsers"].ToString();

                            if (_requestTools.AllKeys.Contains("ident") && !String.IsNullOrEmpty(_context.Request.Form["ident"]))
                                Int32.TryParse(_context.Request.Form["ident"].ToString(), out iDentity);

                            if (_requestTools.AllKeys.Contains("allUser") && !String.IsNullOrEmpty(_context.Request.Form["allUser"]))
                                allUser = _context.Request.Form["allUser"].ToString();


                            if (String.IsNullOrEmpty(expressMessageName) || String.IsNullOrEmpty(expressMessageContent))
                            {
                                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", eResApp.GetRes(_pref, 7875));
                                LaunchError();
                            }

                            if (!String.IsNullOrEmpty(allUser) && allUser == "1" && String.IsNullOrEmpty(expressMessageUsers))
                            {
                                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", eResApp.GetRes(_pref, 7874));
                                LaunchError();
                            }

                            eAdminHomepage hpExpressMessage = new eAdminHomepage(_pref, iDentity, expressMessageName, expressMessageUsers, "", content: HttpUtility.UrlDecode(expressMessageContent));

                            if (action == HomepageAction.ADD)
                                hpExpressMessage.Add(_pref, _targetModule, out sError);
                            else
                                hpExpressMessage.Update(_pref, _targetModule, out sError);

                            if (!String.IsNullOrEmpty(sError) || hpExpressMessage.Id <= 0)
                            {
                                ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, "", eResApp.GetRes(_pref, 1806), "", sError);
                                LaunchError();
                            }
                            else
                            {
                                hpExpressMessage.SaveUsersExpresssMessage(_pref, out sError, _targetModule);
                                if (!String.IsNullOrEmpty(sError))
                                {
                                    ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, "", eResApp.GetRes(_pref, 1806), "", sError);
                                    LaunchError();
                                }
                                else
                                {
                                    res.Success = true;
                                    res.ErrorMsg = sError;
                                    RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
                                }
                            }
                            break;

                        default:
                            break;
                    }

                    #endregion
                    break;

                default:
                    ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", "Action non définie.");
                    LaunchError();
                    break;
            }
            #endregion
        }

        public class JSONReturnHomepages : JSONReturnHTMLContent
        {


        }
    }
}